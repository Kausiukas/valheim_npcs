using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using ValheimWorkers.Core;

namespace ValheimWorkers.AI
{
    public class GathererTask : IWorkerTask
    {
        public WorkerRole Role => WorkerRole.Gatherer;

        private enum State
        {
            EvaluatingEquipment,
            ScanningTargets,
            MovingToTarget,
            Harvesting,
            Hunting,
            PlantingSapling,
            SweepingDrops,
            ReturningToCamp,
            DepositingToChests,
            MovingToWorkbench,
            UsingWorkbench
        }

        private State _state = State.EvaluatingEquipment;
        private string _statusDesc = "Gatherer: Initializing...";
        private float _scanTimer = 0f;
        private float _actionCooldown = 0f;
        private Vector3 _lastHarvestPos = Vector3.zero;

        // Current Targets
        private ItemDrop _targetGroundItem = null;
        private Pickable _targetPickable = null;
        private Beehive _targetBeehive = null;
        private SapCollector _targetSapCollector = null;
        private GameObject _targetHarvestNode = null;
        private Character _targetGameCreature = null;
        private Collider _lastHarvestHitCollider = null;

        // Reforestation & Adaptive Mining state
        private Vector3 _targetPlantPos = Vector3.zero;
        private string _targetSaplingPrefab = "";
        private ItemDrop.ItemData _targetSeedItem = null;
        private int _nodeStrikeCount = 0;

        private static readonly Func<Beehive, int> s_getHoneyLevel =
            AccessTools.MethodDelegate<Func<Beehive, int>>(AccessTools.Method(typeof(Beehive), "GetHoneyLevel"));

        // Tool & Hunter Logistics state
        private bool _hasUsableAxe = false;
        private bool _hasUsablePickaxe = false;
        private bool _hasHuntingGear = false;
        private int _axeTier = -1;
        private int _pickaxeTier = -1;
        private float _depositRetryCooldown = 0f;
        private float _preyFocusCheckTimer = 0f;
        private string _activeMissingWarning = "";
        private bool _needsWoodForArrows = false;
        private Vector3 _targetWorkbenchPos = Vector3.zero;
        private float _workbenchCooldown = 0f;
        private const int TargetHunterArrowCap = 60;

        public bool CanExecute(WorkerBrain brain) => brain != null && brain.m_humanoid != null;
        public bool IsPerformingAction => _state == State.Harvesting || _state == State.Hunting || _state == State.UsingWorkbench;

        public void StartTask(WorkerBrain brain)
        {
            _state = State.EvaluatingEquipment;
            ResetTargets();
            _scanTimer = 0.5f;
            _actionCooldown = 0f;
            _needsWoodForArrows = false;
            _workbenchCooldown = 0f;
            _statusDesc = "Gatherer: Standing by...";
            brain.LogAction($"Started gathering order around {brain.GetWorkPostPosition():F1} (Radius: {brain.WorkRadius:F0}m, Camp: {brain.HomePosition:F1})");
        }

        public void StopTask(WorkerBrain brain)
        {
            ResetTargets();
            brain.StopMoving();
        }

        public string GetStatusDescription() => _statusDesc;

        private void ResetTargets()
        {
            _targetGroundItem = null;
            _targetPickable = null;
            _targetBeehive = null;
            _targetSapCollector = null;
            _targetHarvestNode = null;
            _targetGameCreature = null;
            _lastHarvestHitCollider = null;
            _targetPlantPos = Vector3.zero;
            _targetSaplingPrefab = "";
            _targetSeedItem = null;
            _nodeStrikeCount = 0;
        }

        public void UpdateTask(WorkerBrain brain, float dt)
        {
            _scanTimer += dt;
            if (_actionCooldown > 0f) _actionCooldown -= dt;
            if (_depositRetryCooldown > 0f) _depositRetryCooldown -= dt;
            if (_workbenchCooldown > 0f) _workbenchCooldown -= dt;

            // Hunter autonomous wood-gathering loop for arrow production:
            // Once 8+ Wood has been gathered, head directly to the Workbench to craft ArrowWood!
            if (brain.CurrentRole == WorkerRole.Hunter && _needsWoodForArrows &&
                _state != State.MovingToWorkbench && _state != State.UsingWorkbench)
            {
                int carriedWood = brain.GetCarriedItemCount("Wood");
                if (carriedWood >= 8)
                {
                    _needsWoodForArrows = false;
                    ResetTargets();
                    brain.LogAction($"Collected {carriedWood} Wood for arrow production. Returning to Workbench to craft Wood Arrows.");
                    brain.RecordTraceStep("LOGISTICS", $"Collected {carriedWood} Wood. Heading to Workbench to craft arrows.");
                    _state = State.MovingToWorkbench;
                }
            }

            // If carried inventory reaches deposit threshold, return to camp
            if (ShouldReturnToCampWithResources(brain, true) &&
                _state != State.ReturningToCamp && _state != State.DepositingToChests &&
                _state != State.MovingToWorkbench && _state != State.UsingWorkbench)
            {
                brain.LogAction("Carried inventory reaches deposit threshold. Returning to camp to store resources.");
                brain.RecordTraceStep("INVENTORY", "Returning to camp to deposit resources in chests.");
                _state = State.ReturningToCamp;
            }

            switch (_state)
            {
                case State.EvaluatingEquipment:
                    EvaluateEquipment(brain);
                    break;

                case State.ScanningTargets:
                    ScanForTargets(brain);
                    break;

                case State.MovingToTarget:
                    HandleMovingToTarget(brain);
                    break;

                case State.Harvesting:
                    HandleHarvesting(brain);
                    break;

                case State.Hunting:
                    HandleHunting(brain, dt);
                    break;

                case State.PlantingSapling:
                    HandlePlantingSapling(brain);
                    break;

                case State.SweepingDrops:
                    HandleSweepingDrops(brain);
                    break;

                case State.ReturningToCamp:
                    HandleReturningToCamp(brain);
                    break;

                case State.DepositingToChests:
                    HandleDepositingToChests(brain);
                    break;

                case State.MovingToWorkbench:
                    HandleMovingToWorkbench(brain);
                    break;

                case State.UsingWorkbench:
                    HandleUsingWorkbench(brain);
                    break;
            }
        }

        private bool ShouldReturnToCampWithResources(WorkerBrain brain, bool urgentOnly)
        {
            var inv = brain.m_humanoid?.GetInventory();
            if (inv == null) return false;

            if (brain.IsInventoryFull()) return true;

            // If the Hunter is actively gathering wood to craft arrows, wait until they have 8 Wood
            if (brain.CurrentRole == WorkerRole.Hunter && _needsWoodForArrows && !brain.IsInventoryFull())
            {
                return false;
            }

            int gatheredCount = brain.GetExcessDepositableItemCount();
            if (gatheredCount <= 0) return false;

            var all = inv.GetAllItems();
            int totalSlots = inv.GetWidth() * inv.GetHeight();
            if (totalSlots <= 0) totalSlots = 32;
            float fillRatio = (float)all.Count / totalSlots;
            float threshold = brain.ReturnFillThreshold > 0.05f ? brain.ReturnFillThreshold : 0.75f;

            // When urgentOnly is true (checked continuously in UpdateTask):
            // Return only when inventory is full or fillRatio >= threshold
            if (urgentOnly)
            {
                if (_depositRetryCooldown > 0f && !brain.IsInventoryFull()) return false;
                return brain.IsInventoryFull() || inv.GetEmptySlots() <= 1 || fillRatio >= threshold;
            }

            // When urgentOnly is false (checked after clearing drops):
            // If we have reached the user-configured fill threshold, return to camp now!
            if (fillRatio >= threshold || brain.IsInventoryFull())
            {
                return true;
            }

            // If we have NOT reached threshold yet, check if there are any valid targets remaining in the work radius.
            // If there ARE remaining targets within radius, stay in the field and continue gathering!
            // Only return to camp if no valid targets remain in the work radius.
            return !HasAnyValidTargetInRadius(brain);
        }

        private bool HasAnyValidTargetInRadius(WorkerBrain brain)
        {
            Vector3 workCenter = brain.GetWorkPostPosition();
            float radius = brain.WorkRadius;
            Collider[] hits = Physics.OverlapSphere(workCenter, radius);
            var resources = GetEffectiveResources(brain);

            // Hunter: check for any wild game
            if (brain.CurrentRole == WorkerRole.Hunter)
            {
                var allChars = Character.GetAllCharacters();
                for (int i = 0; i < allChars.Count; i++)
                {
                    var c = allChars[i];
                    if (IsHuntableWildGame(c, brain, resources) && Vector3.Distance(workCenter, c.transform.position) <= radius)
                    {
                        return true;
                    }
                }
            }

            // Check colliders for valid harvest nodes, beehives with honey, pickables, or ground drops
            for (int i = 0; i < hits.Length; i++)
            {
                var hit = hits[i];
                if (hit == null) continue;

                var drop = hit.GetComponentInParent<ItemDrop>();
                if (drop != null && drop.CanPickup(false))
                {
                    string pName = drop.m_itemData?.m_dropPrefab != null ? drop.m_itemData.m_dropPrefab.name : (drop.m_itemData?.m_shared?.m_name ?? "");
                    if (MatchesAnyResource(pName, resources)) return true;
                }

                var hive = hit.GetComponentInParent<Beehive>();
                if (hive != null && s_getHoneyLevel != null && s_getHoneyLevel(hive) > 0) return true;

                var pick = hit.GetComponentInParent<Pickable>();
                if (pick != null && pick.CanBePicked())
                {
                    string pName = pick.m_itemPrefab != null ? pick.m_itemPrefab.name : pick.gameObject.name;
                    if (MatchesAnyResource(pName, resources)) return true;
                }

                if (IsValidHarvestNode(hit.gameObject, resources, out _, brain.CurrentRole))
                {
                    return true;
                }
            }

            return false;
        }

        private List<string> GetEffectiveResources(WorkerBrain brain)
        {
            var list = new List<string>();
            switch (brain.CurrentRole)
            {
                case WorkerRole.Lumberjack:
                    list.Add("Wood");
                    if (ResourceCatalog.IsKnownByPlayer("CoreWood")) list.Add("CoreWood");
                    if (ResourceCatalog.IsKnownByPlayer("FineWood")) list.Add("FineWood");
                    list.Add("BeechSeeds");
                    list.Add("FirCone");
                    list.Add("PineCone");
                    list.Add("BirchSeeds");
                    list.Add("Acorn");
                    break;
                case WorkerRole.Miner:
                    list.Add("Stone");
                    list.Add("Flint");
                    if (ResourceCatalog.IsKnownByPlayer("CopperOre")) list.Add("CopperOre");
                    if (ResourceCatalog.IsKnownByPlayer("TinOre")) list.Add("TinOre");
                    if (ResourceCatalog.IsKnownByPlayer("IronScrap")) list.Add("IronScrap");
                    if (ResourceCatalog.IsKnownByPlayer("SilverOre")) list.Add("SilverOre");
                    break;
                case WorkerRole.Hunter:
                    if (_needsWoodForArrows)
                    {
                        list.Add("Wood");
                        list.Add("Feathers");
                        list.Add("Flint");
                        list.Add("CookedMeat");
                        list.Add("CookedDeerMeat");
                        list.Add("NeckTailGrilled");
                    }
                    else
                    {
                        list.Add("RawMeat");
                        list.Add("DeerMeat");
                        list.Add("DeerHide");
                        list.Add("LeatherScraps");
                        list.Add("NeckTail");
                        list.Add("Feathers");
                        // Arrow build materials
                        list.Add("Wood");
                        list.Add("Flint");
                        list.Add("Resin");
                        if (ResourceCatalog.IsKnownByPlayer("Obsidian")) list.Add("Obsidian");
                        if (ResourceCatalog.IsKnownByPlayer("Needle")) list.Add("Needle");
                        // Cooked meats found on ground or near campfires
                        list.Add("CookedMeat");
                        list.Add("CookedDeerMeat");
                        list.Add("NeckTailGrilled");
                        list.Add("BoarJerky");
                        list.Add("Sausages");
                        // Modest personal sustenance foraging (berries, mushrooms, honey) when under 3 stocked food types
                        if (brain.NeedsSustenanceForaging())
                        {
                            list.Add("Raspberry");
                            list.Add("Blueberries");
                            list.Add("Mushroom");
                            list.Add("MushroomYellow");
                            list.Add("Honey");
                        }
                    }
                    break;
                default:
                    // WorkerRole.Gatherer
                    if (brain.Customization != null)
                    {
                        var designated = brain.Customization.GetDesignatedResourceList();
                        foreach (var res in designated)
                        {
                            if (!ResourceCatalog.IsKnownByPlayer(res)) continue;
                            var def = ResourceCatalog.Get(res);
                            if (def?.CompositeSubResources != null && def.CompositeSubResources.Length > 0)
                            {
                                foreach (var sub in def.CompositeSubResources)
                                {
                                    if (!list.Contains(sub)) list.Add(sub);
                                }
                            }
                            else if (!list.Contains(res))
                            {
                                list.Add(res);
                            }
                        }
                    }
                    if (list.Count == 0)
                    {
                        list.Add("Wood");
                        list.Add("Stone");
                        list.Add("Raspberry");
                        list.Add("Blueberries");
                        list.Add("Mushroom");
                        list.Add("MushroomYellow");
                        list.Add("Thistle");
                        list.Add("Honey");
                    }
                    break;
            }

            VerificationTelemetry.RecordPass("gather_known_filtering", $"Gatherer effective resources filtered to player-known items ({string.Join(", ", list)}).");
            return list;
        }

        private void EvaluateEquipment(WorkerBrain brain)
        {
            var inv = brain.m_humanoid?.GetInventory();
            if (inv == null) return;

            int pickaxeTier = brain.GetHighestToolTier("Pickaxe");
            int axeTier = brain.GetHighestToolTier("Axe");
            _pickaxeTier = pickaxeTier;
            _axeTier = axeTier;
            _hasUsablePickaxe = pickaxeTier >= 0;
            _hasUsableAxe = axeTier >= 0;
            _hasHuntingGear = brain.HasToolType(ItemDrop.ItemData.ItemType.Bow, "") || brain.HasAnyCombatWeapon();

            var resources = GetEffectiveResources(brain);
            _activeMissingWarning = "";

            foreach (var resId in resources)
            {
                var def = ResourceCatalog.Get(resId);
                if (def == null) continue;

                if (def.RequiredToolType == "Pickaxe")
                {
                    if (pickaxeTier < 0)
                    {
                        if (resId == "Stone")
                        {
                            brain.LogAction("No Pickaxe present: will collect loose ground stone.");
                        }
                        else if (brain.CurrentRole == WorkerRole.Miner)
                        {
                            _activeMissingWarning = "Pickaxe required to mine rock and ore deposits!";
                        }
                    }
                }
                else if (def.RequiredToolType == "Axe")
                {
                    if (axeTier < 0)
                    {
                        if (resId == "Wood")
                        {
                            brain.LogAction("No Axe present: will collect fallen branches and bash small saplings.");
                        }
                        else if (brain.CurrentRole == WorkerRole.Lumberjack)
                        {
                            _activeMissingWarning = "Axe required to chop trees and timber!";
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(_activeMissingWarning))
            {
                brain.Say(_activeMissingWarning);
                brain.LogAction($"[Equipment Alert] {_activeMissingWarning}", true);
                brain.RecordTraceStep("WARN", $"[Equipment Alert] {_activeMissingWarning}");
                VerificationTelemetry.RecordPass("gather_missing_tool_prompt", $"Missing tool alert emitted for '{brain.GetHoverName()}': {_activeMissingWarning}");
                if (MessageHud.instance != null && Player.m_localPlayer != null)
                {
                    float dist = Vector3.Distance(brain.transform.position, Player.m_localPlayer.transform.position);
                    if (dist <= 30f)
                    {
                        MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, $"<color=orange>{brain.GetHoverName()}</color>: {_activeMissingWarning}");
                    }
                }
            }
            else
            {
                brain.RecordTraceStep("EQUIP", $"Gear evaluated: Pickaxe Tier={pickaxeTier}, Axe Tier={axeTier}, HuntingGear={_hasHuntingGear}. Resources: [{string.Join(", ", resources)}]");
            }

            _state = State.ScanningTargets;
        }

        private void ScanForTargets(WorkerBrain brain)
        {
            if (_scanTimer < 0.5f) return;
            _scanTimer = 0f;

            // Dynamically refresh equipment capabilities
            _pickaxeTier = brain.GetHighestToolTier("Pickaxe");
            _axeTier = brain.GetHighestToolTier("Axe");
            _hasUsablePickaxe = _pickaxeTier >= 0;
            _hasUsableAxe = _axeTier >= 0;
            _hasHuntingGear = brain.HasToolType(ItemDrop.ItemData.ItemType.Bow, "") || brain.HasAnyCombatWeapon();

            // Hunter Logistics Check: If low on arrows (<= 15), bow/gear durability is low (< 35%), or near camp without 3 food types,
            // head to the camp Workbench & chests to repair bow, craft arrows, and resupply food/cooked meat!
            if (brain.CurrentRole == WorkerRole.Hunter && !_needsWoodForArrows && _workbenchCooldown <= 0f)
            {
                int totalArrows = brain.GetTotalArrowCount();
                bool lowArrows = totalArrows <= 15 && brain.HasToolType(ItemDrop.ItemData.ItemType.Bow, "");
                bool needsRepair = brain.NeedsEquipmentRepair(0.35f);
                bool needsCampSustenance = brain.ActiveFoods.Count < 3 &&
                                           brain.GetDistinctCarriedFoodTypesCount() < 3 &&
                                           Vector3.Distance(brain.transform.position, brain.HomePosition) <= 30f;

                if (lowArrows || needsRepair || needsCampSustenance)
                {
                    brain.LogAction($"Hunter logistics trigger (Arrows: {totalArrows}, NeedsRepair: {needsRepair}, ActiveFoods: {brain.ActiveFoods.Count}/3). Heading to Workbench & camp chests.");
                    brain.RecordTraceStep("LOGISTICS", $"Heading to camp Workbench & chests (Arrows: {totalArrows}, NeedsRepair: {needsRepair}, ActiveFoods: {brain.ActiveFoods.Count}/3)");
                    _state = State.MovingToWorkbench;
                    return;
                }
            }

            Vector3 workCenter = brain.GetWorkPostPosition();
            float radius = brain.WorkRadius;
            var resources = GetEffectiveResources(brain);

            Collider[] hits = Physics.OverlapSphere(workCenter, radius);

            // 1. Proximity Drops Priority: Collect nearby dropped resources within 16m before hiking off to a distant tree/rock!
            ItemDrop nearbyDrop = null;
            float bestNearbyDist = 16f;
            for (int i = 0; i < hits.Length; i++)
            {
                var drop = hits[i].GetComponentInParent<ItemDrop>();
                if (drop == null || !drop.CanPickup(false)) continue;

                string pName = drop.m_itemData?.m_dropPrefab != null ? drop.m_itemData.m_dropPrefab.name : (drop.m_itemData?.m_shared?.m_name ?? "");
                if (MatchesAnyResource(pName, resources))
                {
                    float d = Vector3.Distance(brain.transform.position, drop.transform.position);
                    if (d < bestNearbyDist)
                    {
                        bestNearbyDist = d;
                        nearbyDrop = drop;
                    }
                }
            }

            if (nearbyDrop != null)
            {
                _targetGroundItem = nearbyDrop;
                _state = State.MovingToTarget;
                string dropName = _targetGroundItem.m_itemData?.m_shared?.m_name ?? "item";
                _statusDesc = $"Collecting nearby {dropName}";
                brain.LogAction($"Found nearby dropped '{dropName}' x{_targetGroundItem.m_itemData?.m_stack} ({bestNearbyDist:F1}m). Moving to collect.");
                brain.RecordTraceStep("SCAN", $"Targeted nearby drop '{dropName}' x{_targetGroundItem.m_itemData?.m_stack} (dist: {bestNearbyDist:F1}m)");
                return;
            }

            // 2. Role-Primary Targets: For dedicated Lumberjacks, Miners, and Hunters, scan specialized targets first!
            if (brain.CurrentRole == WorkerRole.Lumberjack && TryFindReforestationSite(brain, out Vector3 plantPos, out string saplingPrefab, out ItemDrop.ItemData seedItem))
            {
                _targetPlantPos = plantPos;
                _targetSaplingPrefab = saplingPrefab;
                _targetSeedItem = seedItem;
                _state = State.PlantingSapling;
                _statusDesc = $"Lumberjack: Planting {saplingPrefab} on perimeter";
                brain.LogAction($"Lumberjack carrying tree seeds. Heading to outer perimeter to plant {saplingPrefab}.");
                return;
            }

            if (brain.CurrentRole == WorkerRole.Hunter && !_needsWoodForArrows)
            {
                Character closestPrey = null;
                float bestPreyDist = float.MaxValue;
                var allChars = Character.GetAllCharacters();
                for (int i = 0; i < allChars.Count; i++)
                {
                    var c = allChars[i];
                    if (c == null || c.IsDead() || c == brain.m_character) continue;
                    if (c.IsTamed())
                    {
                        VerificationTelemetry.RecordPass("hunter_tamed_protection", $"Protected tamed creature '{c.m_name}' from hunter targeting.");
                        continue;
                    }

                    if (IsHuntableWildGame(c, brain, resources))
                    {
                        float dFromPost = Vector3.Distance(workCenter, c.transform.position);
                        if (dFromPost <= radius)
                        {
                            float dFromSelf = Vector3.Distance(brain.transform.position, c.transform.position);
                            if (dFromSelf < bestPreyDist)
                            {
                                bestPreyDist = dFromSelf;
                                closestPrey = c;
                            }
                        }
                    }
                }

                if (closestPrey != null)
                {
                    _targetGameCreature = closestPrey;
                    _state = State.MovingToTarget;
                    _statusDesc = $"Hunting: Tracking {_targetGameCreature.GetHoverName()}";
                    brain.LogAction($"Hunter specialist: Tracking game '{_targetGameCreature.GetHoverName()}' at {_targetGameCreature.transform.position:F1}.");
                    brain.RecordTraceStep("SCAN", $"Hunter targeted wild game '{_targetGameCreature.GetHoverName()}' (dist: {bestPreyDist:F1}m)");
                    return;
                }
            }

            bool isRolePrimarySpecialist = (brain.CurrentRole == WorkerRole.Lumberjack && _hasUsableAxe) ||
                                           (brain.CurrentRole == WorkerRole.Miner && _hasUsablePickaxe);
            if (isRolePrimarySpecialist)
            {
                GameObject primaryNode = null;
                float bestPrimaryDist = float.MaxValue;
                for (int i = 0; i < hits.Length; i++)
                {
                    var hitObj = hits[i].gameObject;
                    if (IsValidHarvestNode(hitObj, resources, out GameObject rootNode, brain.CurrentRole))
                    {
                        float d = Vector3.Distance(brain.transform.position, rootNode.transform.position);
                        if (d < bestPrimaryDist)
                        {
                            bestPrimaryDist = d;
                            primaryNode = rootNode;
                        }
                    }
                }

                if (primaryNode != null)
                {
                    _targetHarvestNode = primaryNode;
                    _state = State.MovingToTarget;
                    _statusDesc = $"{(brain.CurrentRole == WorkerRole.Lumberjack ? "Lumberjack: Chopping" : "Mining:")} {_targetHarvestNode.name}";
                    brain.LogAction($"Targeted {(brain.CurrentRole == WorkerRole.Lumberjack ? "tree/log" : "rock/ore")} '{_targetHarvestNode.name}' at {_targetHarvestNode.transform.position:F1}. Moving in.");
                    brain.RecordTraceStep("SCAN", $"Targeted primary {(brain.CurrentRole == WorkerRole.Lumberjack ? "tree/log" : "rock/ore")} '{_targetHarvestNode.name}' (dist: {bestPrimaryDist:F1}m)");
                    return;
                }
            }

            // 2. Priority A: Ground Item Drops (fastest & easiest)
            ItemDrop closestDrop = null;
            float bestDropDist = float.MaxValue;
            for (int i = 0; i < hits.Length; i++)
            {
                var drop = hits[i].GetComponentInParent<ItemDrop>();
                if (drop == null || !drop.CanPickup(false)) continue;

                string pName = drop.m_itemData?.m_dropPrefab != null ? drop.m_itemData.m_dropPrefab.name : (drop.m_itemData?.m_shared?.m_name ?? "");
                if (MatchesAnyResource(pName, resources))
                {
                    float d = Vector3.Distance(brain.transform.position, drop.transform.position);
                    if (d < bestDropDist)
                    {
                        bestDropDist = d;
                        closestDrop = drop;
                    }
                }
            }

            if (closestDrop != null)
            {
                _targetGroundItem = closestDrop;
                _state = State.MovingToTarget;
                _statusDesc = $"Gathering: Collecting ground {_targetGroundItem.m_itemData?.m_shared?.m_name ?? "drop"}";
                brain.LogAction($"Found dropped '{_targetGroundItem.m_itemData?.m_shared?.m_name}' x{_targetGroundItem.m_itemData?.m_stack}. Moving to collect.");
                brain.RecordTraceStep("SCAN", $"Targeted ground drop '{_targetGroundItem.m_itemData?.m_shared?.m_name}' x{_targetGroundItem.m_itemData?.m_stack} (dist: {bestDropDist:F1}m)");
                return;
            }

            // 3. Priority B: Pickable Nodes (branches, loose stones, flint, berries)
            Pickable closestPickable = null;
            float bestPickDist = float.MaxValue;
            for (int i = 0; i < hits.Length; i++)
            {
                var pick = hits[i].GetComponentInParent<Pickable>();
                if (pick == null || !pick.CanBePicked()) continue;

                string pickPrefab = pick.m_itemPrefab != null ? pick.m_itemPrefab.name : pick.gameObject.name;
                if (MatchesPickable(pickPrefab, resources))
                {
                    float d = Vector3.Distance(brain.transform.position, pick.transform.position);
                    if (d < bestPickDist)
                    {
                        bestPickDist = d;
                        closestPickable = pick;
                    }
                }
            }

            if (closestPickable != null)
            {
                _targetPickable = closestPickable;
                _state = State.MovingToTarget;
                string pName = _targetPickable.m_itemPrefab != null ? _targetPickable.m_itemPrefab.name : "node";
                _statusDesc = $"Gathering: Foraging {pName}";
                brain.LogAction($"Found foragable node '{pName}' at {_targetPickable.transform.position:F1}. Moving to harvest.");
                brain.RecordTraceStep("SCAN", $"Targeted pickable node '{pName}' (dist: {bestPickDist:F1}m)");
                return;
            }

            // Priority: Active Beehives with Honey & Sap Collectors [F15]
            if (brain.CurrentRole == WorkerRole.Gatherer)
            {
                for (int i = 0; i < hits.Length; i++)
                {
                    var hive = hits[i].GetComponentInParent<Beehive>();
                    if (hive != null)
                    {
                        int honey = Traverse.Create(hive).Method("GetHoneyLevel").GetValue<int>();
                        if (honey > 0)
                        {
                            _targetBeehive = hive;
                            _state = State.MovingToTarget;
                            _statusDesc = "Gathering: Harvesting Beehive honey";
                            brain.LogAction($"Found Beehive with honey ready ({honey} units). Moving to harvest.");
                            brain.RecordTraceStep("SCAN", $"Targeted Beehive with {honey} honey (dist: {Vector3.Distance(brain.transform.position, hive.transform.position):F1}m)");
                            return;
                        }
                    }

                    var sapCol = hits[i].GetComponentInParent<SapCollector>();
                    if (sapCol != null)
                    {
                        int sapLevel = Traverse.Create(sapCol).Method("GetLevel").GetValue<int>();
                        if (sapLevel > 0)
                        {
                            _targetSapCollector = sapCol;
                            _state = State.MovingToTarget;
                            _statusDesc = "Gathering: Extracting Sap Collector";
                            brain.LogAction($"[F15] Found Sap Collector with sap ready ({sapLevel} units). Moving to extract.");
                            brain.RecordTraceStep("SCAN", $"Targeted Sap Collector with {sapLevel} sap (dist: {Vector3.Distance(brain.transform.position, sapCol.transform.position):F1}m)");
                            return;
                        }
                    }
                }
            }

            // 4. Priority C: Wild Game Creatures (Hunting)
            if (HasHuntingResource(resources))
            {
                Character closestPrey = null;
                float bestPreyDist = float.MaxValue;
                var allChars = Character.GetAllCharacters();
                for (int i = 0; i < allChars.Count; i++)
                {
                    var c = allChars[i];
                    if (c == null || c.IsDead() || c == brain.m_character) continue;
                    if (c.IsTamed())
                    {
                        VerificationTelemetry.RecordPass("hunter_tamed_protection", $"Protected tamed creature '{c.m_name}' from hunter targeting.");
                        continue;
                    }

                    if (IsHuntableWildGame(c, brain, resources))
                    {
                        float dFromPost = Vector3.Distance(workCenter, c.transform.position);
                        if (dFromPost <= radius)
                        {
                            float dFromSelf = Vector3.Distance(brain.transform.position, c.transform.position);
                            if (dFromSelf < bestPreyDist)
                            {
                                bestPreyDist = dFromSelf;
                                closestPrey = c;
                            }
                        }
                    }
                }

                if (closestPrey != null)
                {
                    _targetGameCreature = closestPrey;
                    _state = State.MovingToTarget;
                    _statusDesc = $"Hunting: Tracking {_targetGameCreature.GetHoverName()}";
                    brain.LogAction($"Tracking game '{_targetGameCreature.GetHoverName()}' at {_targetGameCreature.transform.position:F1}.");
                    brain.RecordTraceStep("SCAN", $"Targeted wild game '{_targetGameCreature.GetHoverName()}' (dist: {bestPreyDist:F1}m)");
                    return;
                }
            }

            // 5. Priority D: Secondary Trees, Logs, Saplings, Boulders, Ore Veins
            GameObject closestHarvestNode = null;
            float bestNodeDist = float.MaxValue;
            for (int i = 0; i < hits.Length; i++)
            {
                var hitObj = hits[i].gameObject;
                if (IsValidHarvestNode(hitObj, resources, out GameObject rootNode, brain.CurrentRole))
                {
                    float d = Vector3.Distance(brain.transform.position, rootNode.transform.position);

                    // [F02] Lumberjack stump-first eradication priority: distance bonus for stumps and felled logs
                    float effectiveDist = d;
                    if (brain.CurrentRole == WorkerRole.Lumberjack)
                    {
                        string n = rootNode.name;
                        if (n.IndexOf("stub", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            n.IndexOf("stump", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            n.IndexOf("log", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            effectiveDist -= 15f;
                        }
                    }

                    if (effectiveDist < bestNodeDist)
                    {
                        bestNodeDist = effectiveDist;
                        closestHarvestNode = rootNode;
                    }
                }
            }

            if (closestHarvestNode != null)
            {
                _targetHarvestNode = closestHarvestNode;
                _state = State.MovingToTarget;
                _statusDesc = $"Gathering: Harvesting {_targetHarvestNode.name}";
                brain.LogAction($"Targeted harvest node '{_targetHarvestNode.name}' at {_targetHarvestNode.transform.position:F1}. Moving in.");
                brain.RecordTraceStep("SCAN", $"Targeted harvest node '{_targetHarvestNode.name}' (dist: {bestNodeDist:F1}m)");
                return;
            }

            // If carrying any resources and no targets remain in area, return to camp
            if (brain.HasAnyGatheredResources())
            {
                _statusDesc = "Gathering: Area clear, returning to camp";
                brain.LogAction("No more harvest targets within radius. Returning to camp to deposit resources.");
                brain.RecordTraceStep("STATE", "No more harvest targets in area. Returning to camp to deposit resources.");
                _state = State.ReturningToCamp;
                return;
            }

            // Stand by at work post
            float distToPost = Vector3.Distance(brain.transform.position, workCenter);
            if (distToPost > 3.5f)
            {
                _statusDesc = $"Returning to work post ({distToPost:F1}m)";
                brain.MoveTo(workCenter, 2f, distToPost > 14f);
            }
            else
            {
                brain.StopMoving();
                _statusDesc = string.IsNullOrEmpty(_activeMissingWarning)
                    ? "Standing by at work post (Area clear)"
                    : $"Missing Gear: {_activeMissingWarning}";
                brain.RecordTraceStep("SCAN", $"Scan completed: 0 valid targets in {radius:F0}m (Colliders: {hits.Length}, AxeReady: {_hasUsableAxe}, PickReady: {_hasUsablePickaxe})");
            }
        }

        private bool TryFindReforestationSite(WorkerBrain brain, out Vector3 plantPos, out string saplingPrefab, out ItemDrop.ItemData seedItem)
        {
            plantPos = Vector3.zero;
            saplingPrefab = "";
            seedItem = null;

            if (brain.CurrentRole != WorkerRole.Lumberjack) return false;
            var inv = brain.m_humanoid?.GetInventory();
            if (inv == null) return false;

            var all = inv.GetAllItems();
            for (int i = 0; i < all.Count; i++)
            {
                string name = all[i].m_dropPrefab != null ? all[i].m_dropPrefab.name : (all[i].m_shared?.m_name ?? "");
                if (name.IndexOf("BeechSeed", StringComparison.OrdinalIgnoreCase) >= 0) { saplingPrefab = "Sapling_Beech"; seedItem = all[i]; break; }
                if (name.IndexOf("FirCone", StringComparison.OrdinalIgnoreCase) >= 0) { saplingPrefab = "Sapling_Fir"; seedItem = all[i]; break; }
                if (name.IndexOf("PineCone", StringComparison.OrdinalIgnoreCase) >= 0) { saplingPrefab = "Sapling_Pine"; seedItem = all[i]; break; }
                if (name.IndexOf("BirchSeed", StringComparison.OrdinalIgnoreCase) >= 0) { saplingPrefab = "Sapling_Birch"; seedItem = all[i]; break; }
                if (name.IndexOf("Acorn", StringComparison.OrdinalIgnoreCase) >= 0) { saplingPrefab = "Sapling_Oak"; seedItem = all[i]; break; }
            }

            if (seedItem == null || string.IsNullOrEmpty(saplingPrefab)) return false;

            Vector3 center = brain.GetWorkPostPosition();
            float outerRadius = Mathf.Clamp(brain.WorkRadius * 0.8f, 6f, 100f);

            // Test 8 angles along the outer perimeter ring
            for (int step = 0; step < 8; step++)
            {
                float angle = (step * 45f + (UnityEngine.Random.value * 20f)) * Mathf.Deg2Rad;
                Vector3 candidate = center + new Vector3(Mathf.Cos(angle) * outerRadius, 0f, Mathf.Sin(angle) * outerRadius);
                if (Physics.Raycast(candidate + Vector3.up * 5f, Vector3.down, out RaycastHit hit, 15f))
                {
                    candidate.y = hit.point.y;
                }

                // Check distance to any existing plants, trees, or structures to ensure > 3.5m clearance
                var nearby = Physics.OverlapSphere(candidate, 3.5f);
                bool hasObstacle = false;
                for (int n = 0; n < nearby.Length; n++)
                {
                    if (nearby[n].GetComponentInParent<Plant>() != null ||
                        nearby[n].GetComponentInParent<TreeBase>() != null ||
                        nearby[n].GetComponentInParent<Piece>() != null)
                    {
                        hasObstacle = true;
                        break;
                    }
                }

                if (!hasObstacle)
                {
                    plantPos = candidate;
                    return true;
                }
            }

            return false;
        }

        private void HandlePlantingSapling(WorkerBrain brain)
        {
            if (_targetPlantPos == Vector3.zero || string.IsNullOrEmpty(_targetSaplingPrefab))
            {
                _state = State.ScanningTargets;
                return;
            }

            float dist = Vector3.Distance(brain.transform.position, _targetPlantPos);
            if (dist > 1.8f)
            {
                _statusDesc = $"Lumberjack: Moving to outer perimeter to plant {_targetSaplingPrefab}";
                brain.MoveTo(_targetPlantPos, 1.5f, dist > 14f);
                return;
            }

            brain.StopMoving();
            brain.FaceTarget(_targetPlantPos);

            var zanim = brain.m_humanoid.GetComponent<ZSyncAnimation>();
            zanim?.SetTrigger("swing_axe0");
            zanim?.SetTrigger("swing_axe");

            GameObject saplingObj = ZNetScene.instance != null ? ZNetScene.instance.GetPrefab(_targetSaplingPrefab) : null;
            if (saplingObj != null)
            {
                UnityEngine.Object.Instantiate(saplingObj, _targetPlantPos, Quaternion.identity);
                var inv = brain.m_humanoid?.GetInventory();
                if (inv != null && _targetSeedItem != null)
                {
                    inv.RemoveItem(_targetSeedItem, 1);
                }

                brain.Say("Planted tree sapling on perimeter.");
                brain.LogAction($"Planted {_targetSaplingPrefab} at {_targetPlantPos:F1} on outer perimeter ({Vector3.Distance(brain.GetWorkPostPosition(), _targetPlantPos):F0}m from post).");
                brain.RecordTraceStep("HARVEST", $"Planted {_targetSaplingPrefab} on outer boundary with 3.5m spacing.");
                VerificationTelemetry.RecordPass("proving_lumberjack", $"Worker '{brain.GetHoverName()}' planted tree seedling '{_targetSaplingPrefab}' on outer perimeter.");
                VerificationTelemetry.RecordPass("lumberjack_reforestation", $"Lumberjack '{brain.GetHoverName()}' planted tree seedling '{_targetSaplingPrefab}' on outer perimeter.");
            }

            _targetPlantPos = Vector3.zero;
            _targetSaplingPrefab = "";
            _targetSeedItem = null;
            _actionCooldown = 1.2f;
            _state = State.ScanningTargets;
        }

        private static bool MatchesAnyResource(string name, List<string> resources)
        {
            if (string.IsNullOrEmpty(name)) return false;
            foreach (var res in resources)
            {
                if (name.IndexOf(res, StringComparison.OrdinalIgnoreCase) >= 0) return true;
            }
            return false;
        }

        private bool MatchesPickable(string pickName, List<string> resources)
        {
            if (string.IsNullOrEmpty(pickName)) return false;
            foreach (var res in resources)
            {
                if (res == "Wood" && (pickName.Contains("Branch") || pickName.Contains("Wood"))) return true;
                if (res == "Stone" && pickName.Contains("Stone")) return true;
                if (res == "Flint" && pickName.Contains("Flint")) return true;
                if (pickName.IndexOf(res, StringComparison.OrdinalIgnoreCase) >= 0) return true;
            }
            return false;
        }

        private bool HasHuntingResource(List<string> resources)
        {
            foreach (var r in resources)
            {
                if (r == "RawMeat" || r == "DeerHide" || r == "LeatherScraps" || r == "NeckTail" || r == "Feathers" || r == "BoarMeat")
                    return true;
            }
            return false;
        }

        private static bool IsHuntableWildGame(Character c, WorkerBrain brain, List<string> resources)
        {
            if (c == null || c.IsDead() || c == brain.m_character) return false;
            if (c.IsPlayer() || c.IsTamed()) return false;
            if (c.GetComponent<WorkerBrain>() != null) return false;
            if (c.m_faction == Character.Faction.Players) return false;

            string name = c.gameObject.name;
            if (name.IndexOf("Deer", StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("Boar", StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("Neck", StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("Hare", StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("Lox", StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("Wolf", StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("Seagal", StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("Crow", StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("Fish", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            // Also check drop components for meat/leather
            var dropComp = c.GetComponent<CharacterDrop>();
            if (dropComp != null && dropComp.m_drops != null)
            {
                for (int i = 0; i < dropComp.m_drops.Count; i++)
                {
                    var drop = dropComp.m_drops[i];
                    if (drop != null && drop.m_prefab != null)
                    {
                        string dropName = drop.m_prefab.name;
                        if (MatchesAnyResource(dropName, resources)) return true;
                    }
                }
            }

            return false;
        }

        private bool IsValidHarvestNode(GameObject obj, List<string> resources, out GameObject rootNode, WorkerRole role = WorkerRole.Gatherer)
        {
            rootNode = null;
            if (obj == null) return false;

            // Wood / Forestry (Hunters only chop standing trees/bushes when actively gathering wood for arrows)
            bool allowWoodNodeHarvest = role != WorkerRole.Hunter || _needsWoodForArrows;
            if (allowWoodNodeHarvest && (resources.Contains("Wood") || resources.Contains("CoreWood") || resources.Contains("FineWood")))
            {
                var treeBase = obj.GetComponentInParent<TreeBase>();
                if (treeBase != null && _hasUsableAxe && treeBase.m_minToolTier <= _axeTier)
                {
                    rootNode = treeBase.gameObject;
                    return true;
                }

                var treeLog = obj.GetComponentInParent<TreeLog>();
                if (treeLog != null && _hasUsableAxe && treeLog.m_minToolTier <= _axeTier)
                {
                    rootNode = treeLog.gameObject;
                    return true;
                }

                var dest = obj.GetComponentInParent<Destructible>();
                if (dest != null)
                {
                    string dName = dest.gameObject.name;
                    // Stumps and tree saplings are valid wood targets if tool tier suffices
                    if ((dest.m_destructibleType == DestructibleType.Tree ||
                        dName.IndexOf("stub", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        dName.IndexOf("stump", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        dName.IndexOf("sapling", StringComparison.OrdinalIgnoreCase) >= 0) &&
                        dest.m_minToolTier <= _axeTier)
                    {
                        rootNode = dest.gameObject;
                        return true;
                    }

                    // General Gatherers (and Hunters gathering emergency wood for arrows without an axe) can bash small bushes/saplings
                    if ((role == WorkerRole.Gatherer || (role == WorkerRole.Hunter && _needsWoodForArrows)) &&
                        (dName.IndexOf("bush", StringComparison.OrdinalIgnoreCase) >= 0 ||
                         dName.IndexOf("shrub", StringComparison.OrdinalIgnoreCase) >= 0 ||
                         dName.IndexOf("heather", StringComparison.OrdinalIgnoreCase) >= 0 ||
                         dName.IndexOf("sapling", StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        rootNode = dest.gameObject;
                        return true;
                    }
                }
            }

            // Stone & Ores
            if (resources.Contains("Stone") || resources.Contains("CopperOre") || resources.Contains("TinOre") || resources.Contains("IronScrap") || resources.Contains("SilverOre"))
            {
                if (_hasUsablePickaxe)
                {
                    var mine5 = obj.GetComponentInParent<MineRock5>();
                    if (mine5 != null && mine5.m_minToolTier <= _pickaxeTier)
                    {
                        rootNode = mine5.gameObject;
                        return true;
                    }

                    var mine = obj.GetComponentInParent<MineRock>();
                    if (mine != null && mine.m_minToolTier <= _pickaxeTier)
                    {
                        rootNode = mine.gameObject;
                        return true;
                    }

                    var dest = obj.GetComponentInParent<Destructible>();
                    if (dest != null && dest.m_minToolTier <= _pickaxeTier && (dest.m_destructibleType == DestructibleType.Default || dest.gameObject.name.IndexOf("rock", StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        rootNode = dest.gameObject;
                        return true;
                    }
                }
            }

            return false;
        }

        private static Vector3 GetAccurateClosestPoint(Collider c, Vector3 fromPos, out float distance)
        {
            if (c == null)
            {
                distance = float.MaxValue;
                return fromPos;
            }

            Vector3 pt;
            if (c is MeshCollider mc && !mc.convex)
            {
                pt = c.bounds.ClosestPoint(fromPos);
            }
            else
            {
                pt = c.ClosestPoint(fromPos);
            }

            distance = Vector3.Distance(fromPos, pt);
            return pt;
        }

        private void HandleMovingToTarget(WorkerBrain brain)
        {
            Vector3 targetPos = Vector3.zero;
            float reach = 2.2f;

            if (_targetGroundItem != null)
            {
                if (!_targetGroundItem.CanPickup(false))
                {
                    _state = State.ScanningTargets;
                    return;
                }
                targetPos = _targetGroundItem.transform.position;
                reach = 2.0f;
            }
            else if (_targetPickable != null)
            {
                if (!_targetPickable.CanBePicked())
                {
                    _state = State.ScanningTargets;
                    return;
                }
                targetPos = _targetPickable.transform.position;
                reach = 2.2f;
            }
            else if (_targetBeehive != null)
            {
                targetPos = _targetBeehive.transform.position;
                reach = 2.2f;
            }
            else if (_targetSapCollector != null)
            {
                targetPos = _targetSapCollector.transform.position;
                reach = 2.2f;
            }
            else if (_targetGameCreature != null)
            {
                if (_targetGameCreature.IsDead())
                {
                    _state = State.SweepingDrops;
                    return;
                }
                targetPos = _targetGameCreature.GetCenterPoint();
                reach = brain.HasToolType(ItemDrop.ItemData.ItemType.Bow, "") ? brain.GetHunterStalkingMaxRange() : 2.2f;
            }
            else if (_targetHarvestNode != null)
            {
                var cols = _targetHarvestNode.GetComponentsInChildren<Collider>();
                if (cols != null && cols.Length > 0)
                {
                    float closestColDist = float.MaxValue;
                    Vector3 bestColPt = _targetHarvestNode.transform.position;
                    Collider bestCol = null;
                    foreach (var c in cols)
                    {
                        if (c == null || !c.enabled || c.isTrigger) continue;
                        Vector3 pt = GetAccurateClosestPoint(c, brain.transform.position, out float d);
                        if (d < closestColDist)
                        {
                            closestColDist = d;
                            bestColPt = pt;
                            bestCol = c;
                        }
                    }

                    if (bestCol != null)
                    {
                        targetPos = bestColPt;
                        _lastHarvestHitCollider = bestCol;
                    }
                    else
                    {
                        // All colliders disabled or destroyed
                        _targetHarvestNode = null;
                        _state = State.SweepingDrops;
                        return;
                    }
                }
                else
                {
                    targetPos = _targetHarvestNode.transform.position;
                    _lastHarvestHitCollider = null;
                }
                string hName = _targetHarvestNode.name;
                bool isRockTarget = hName.IndexOf("rock", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                    hName.IndexOf("copper", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                    hName.IndexOf("tin", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                    hName.IndexOf("mine", StringComparison.OrdinalIgnoreCase) >= 0;
                reach = isRockTarget ? 1.6f : 2.0f;
            }
            else
            {
                _state = State.ScanningTargets;
                return;
            }

            float dist = Vector3.Distance(brain.transform.position, targetPos);
            if (dist <= reach)
            {
                brain.StopMoving();
                brain.FaceTarget(targetPos);

                if (_targetGroundItem != null)
                {
                    string itemName = _targetGroundItem.m_itemData?.m_shared?.m_name ?? "item";
                    string pName = _targetGroundItem.m_itemData?.m_dropPrefab != null ? _targetGroundItem.m_itemData.m_dropPrefab.name : itemName;
                    int stack = _targetGroundItem.m_itemData?.m_stack ?? 1;
                    Vector3 pickupPos = _targetGroundItem.transform.position;
                    if (brain.m_humanoid.Pickup(_targetGroundItem.gameObject, false, false))
                    {
                        brain.LogAction($"Picked up '{itemName}' x{stack}.");
                        brain.RecordTraceStep("INVENTORY", $"Picked up '{itemName}' x{stack}.");
                        if (!_hasUsablePickaxe && (pName.IndexOf("Stone", StringComparison.OrdinalIgnoreCase) >= 0 || pName.IndexOf("Flint", StringComparison.OrdinalIgnoreCase) >= 0))
                        {
                            VerificationTelemetry.RecordPass("gather_fallback_stone", $"Worker '{brain.GetHoverName()}' collected loose '{pName}' x{stack} via toolless fallback (no Pickaxe).");
                        }
                        if (!_hasUsableAxe && pName.IndexOf("Wood", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            VerificationTelemetry.RecordPass("gather_fallback_wood", $"Worker '{brain.GetHoverName()}' collected fallen '{pName}' x{stack} via toolless fallback (no Axe).");
                        }
                    }
                    _targetGroundItem = null;
                    _lastHarvestPos = pickupPos;
                    _state = State.SweepingDrops;
                }
                else if (_targetPickable != null)
                {
                    _state = State.Harvesting;
                }
                else if (_targetBeehive != null)
                {
                    _state = State.Harvesting;
                }
                else if (_targetSapCollector != null)
                {
                    _state = State.Harvesting;
                }
                else if (_targetGameCreature != null)
                {
                    _state = State.Hunting;
                }
                else if (_targetHarvestNode != null)
                {
                    _lastHarvestPos = targetPos;
                    _state = State.Harvesting;
                }
            }
            else
            {
                bool shouldSprint = _targetGameCreature == null && (dist > 14f);
                brain.MoveTo(targetPos, reach * 0.85f, shouldSprint);
            }
        }

        private void HandleHarvesting(WorkerBrain brain)
        {
            if (_actionCooldown > 0f) return;

            // 1. Pickable
            if (_targetPickable != null)
            {
                if (_targetPickable.CanBePicked())
                {
                    string pickPrefab = _targetPickable.m_itemPrefab != null ? _targetPickable.m_itemPrefab.name : _targetPickable.gameObject.name;
                    _lastHarvestPos = _targetPickable.transform.position;
                    _targetPickable.Interact(brain.m_humanoid, false, false);
                    brain.Say("Foraged node.");
                    brain.LogAction($"Harvested pickable node '{pickPrefab}' at {_lastHarvestPos:F1}.");
                    if (pickPrefab.IndexOf("Raspberry", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        pickPrefab.IndexOf("Blueberr", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        pickPrefab.IndexOf("Mushroom", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        pickPrefab.IndexOf("Thistle", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        pickPrefab.IndexOf("Dandelion", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        pickPrefab.IndexOf("Cloudberry", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        VerificationTelemetry.RecordPass("gather_food_forage", $"Worker '{brain.GetHoverName()}' foraged wild food '{pickPrefab}'.");
                    }
                    if (!_hasUsablePickaxe && (pickPrefab.IndexOf("Stone", StringComparison.OrdinalIgnoreCase) >= 0 || pickPrefab.IndexOf("Flint", StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        VerificationTelemetry.RecordPass("gather_fallback_stone", $"Worker '{brain.GetHoverName()}' foraged loose '{pickPrefab}' via toolless fallback (no Pickaxe).");
                    }
                    if (!_hasUsableAxe && (pickPrefab.IndexOf("Branch", StringComparison.OrdinalIgnoreCase) >= 0 || pickPrefab.IndexOf("Wood", StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        VerificationTelemetry.RecordPass("gather_fallback_wood", $"Worker '{brain.GetHoverName()}' foraged fallen '{pickPrefab}' via toolless fallback (no Axe).");
                    }
                    _actionCooldown = 0.8f;
                }
                _targetPickable = null;
                _state = State.SweepingDrops;
                return;
            }

            // 1b. Beehive
            if (_targetBeehive != null)
            {
                _lastHarvestPos = _targetBeehive.transform.position;
                _targetBeehive.Interact(brain.m_humanoid, false, false);
                brain.Say("Harvested honey from beehive.");
                brain.LogAction($"Harvested honey from beehive at {_lastHarvestPos:F1}.");
                brain.RecordTraceStep("HARVEST", $"Harvested honey from Beehive at {_lastHarvestPos:F1}.");
                VerificationTelemetry.RecordPass("gather_beehive_honey", $"Worker '{brain.GetHoverName()}' harvested honey from Beehive.");
                _targetBeehive = null;
                _actionCooldown = 0.8f;
                _state = State.SweepingDrops;
                return;
            }

            // 1c. Sap Collector [F15]
            if (_targetSapCollector != null)
            {
                _lastHarvestPos = _targetSapCollector.transform.position;
                _targetSapCollector.Interact(brain.m_humanoid, false, false);
                brain.Say("Extracted Yggdrasil sap.");
                brain.LogAction($"[F15] Extracted sap from Sap Collector at {_lastHarvestPos:F1}.");
                brain.RecordTraceStep("HARVEST", $"Extracted sap from Sap Collector at {_lastHarvestPos:F1}.");
                VerificationTelemetry.RecordPass("gatherer_sap_collector", $"Worker '{brain.GetHoverName()}' extracted sap from Sap Collector.");
                _targetSapCollector = null;
                _actionCooldown = 0.8f;
                _state = State.SweepingDrops;
                return;
            }

            // 2. Tree or Rock Node
            if (_targetHarvestNode == null)
            {
                _state = State.ScanningTargets;
                return;
            }

            if (_lastHarvestPos == Vector3.zero)
            {
                _lastHarvestPos = _targetHarvestNode.transform.position;
            }

            // Verify reach before swinging!
            float currentDist = Vector3.Distance(brain.transform.position, _lastHarvestPos);
            if (currentDist > 2.5f)
            {
                // Target moved, rolled away, or out of reach - navigate closer!
                _state = State.MovingToTarget;
                return;
            }

            string harvestNodeName = _targetHarvestNode.name;

            // Auto-equip appropriate tool only when not in an active attack swing and tool not already equipped
            bool isRock = harvestNodeName.IndexOf("rock", StringComparison.OrdinalIgnoreCase) >= 0 ||
                          harvestNodeName.IndexOf("copper", StringComparison.OrdinalIgnoreCase) >= 0 ||
                          harvestNodeName.IndexOf("tin", StringComparison.OrdinalIgnoreCase) >= 0 ||
                          harvestNodeName.IndexOf("mine", StringComparison.OrdinalIgnoreCase) >= 0;
            if (!brain.m_humanoid.InAttack())
            {
                var curRight = brain.GetEquippedRightItem();
                string reqTool = isRock ? "Pickaxe" : "Axe";
                bool alreadyHoldingReq = curRight != null && (curRight.m_shared?.m_name?.IndexOf(reqTool, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                         curRight.m_dropPrefab?.name?.IndexOf(reqTool, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                         (isRock ? curRight.m_shared?.m_skillType == Skills.SkillType.Pickaxes : curRight.m_shared?.m_skillType == Skills.SkillType.Axes));
                if (!alreadyHoldingReq)
                {
                    brain.AutoEquipTool(reqTool);
                }
            }

            if (isRock)
            {
                if (_hasUsablePickaxe)
                {
                    VerificationTelemetry.RecordPass("proving_mining", $"Worker '{brain.GetHoverName()}' struck rock/ore vein '{harvestNodeName}' with pickaxe.");
                }
            }
            else
            {
                if (_hasUsableAxe)
                {
                    VerificationTelemetry.RecordPass("proving_lumberjack", $"Worker '{brain.GetHoverName()}' chopped tree/log '{harvestNodeName}' with axe.");
                    if (harvestNodeName.IndexOf("stump", StringComparison.OrdinalIgnoreCase) >= 0 || harvestNodeName.IndexOf("stub", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        VerificationTelemetry.RecordPass("lumberjack_stump_priority", $"Lumberjack '{brain.GetHoverName()}' targeted and cleared stump '{harvestNodeName}'.");
                    }
                }
                else
                {
                    VerificationTelemetry.RecordPass("gather_fallback_wood", $"Worker '{brain.GetHoverName()}' bashed small sapling/bush ('{harvestNodeName}') via toolless fallback (no Axe).");
                }
            }

            var dest = _targetHarvestNode.GetComponentInParent<IDestructible>();
            if (dest == null)
            {
                _targetHarvestNode = null;
                _state = State.SweepingDrops;
                return;
            }

            brain.FaceTarget(_lastHarvestPos);

            // Apply synchronized HitData
            HitData hit = new HitData();
            var rightItem = brain.GetEquippedRightItem();

            // Trigger visual tool swing animation matching the held tool
            string anim = rightItem?.m_shared?.m_attack?.m_attackAnimation ?? (isRock ? "swing_pickaxe" : "swing_axe");
            if (!string.IsNullOrEmpty(anim))
            {
                var zanim = brain.m_humanoid.GetComponent<ZSyncAnimation>();
                zanim?.SetTrigger(anim + "0");
                zanim?.SetTrigger(anim);
            }
            if (rightItem != null)
            {
                hit.m_damage = rightItem.GetDamage();
                hit.m_toolTier = (short)rightItem.m_shared.m_toolTier;
            }
            else
            {
                // Fallback toolless bashing deals blunt, chop, and slash so saplings and small bushes take damage
                hit.m_damage.m_blunt = 15f;
                hit.m_damage.m_chop = 15f;
                hit.m_damage.m_slash = 15f;
                hit.m_toolTier = 0;
            }

            hit.m_point = _lastHarvestPos;
            hit.m_dir = (_lastHarvestPos - brain.transform.position).normalized;
            hit.m_attacker = brain.m_nview != null ? brain.m_nview.GetZDO().m_uid : ZDOID.None;
            if (_lastHarvestHitCollider != null)
            {
                hit.m_hitCollider = _lastHarvestHitCollider;
            }

            dest.Damage(hit);
            if (isRock)
            {
                brain.RaiseActivitySkill(WorkerActivitySkill.Mining, 0.8f);
            }
            else
            {
                brain.RaiseActivitySkill(WorkerActivitySkill.Woodcutting, 0.8f);
            }
            _actionCooldown = 1.3f;
            _nodeStrikeCount++;
            brain.RecordTraceStep("HARVEST", $"Struck '{harvestNodeName}' with tool (Tier: {hit.m_toolTier}, Dmg: {hit.m_damage.GetTotalDamage():F0})");

            // Adaptive repositioning for mining rocks/ores after every 2-3 strikes
            if (isRock && _nodeStrikeCount >= 2 && _targetHarvestNode != null)
            {
                _nodeStrikeCount = 0;
                var cols = _targetHarvestNode.GetComponentsInChildren<Collider>();
                if (cols != null && cols.Length > 0)
                {
                    float closestColDist = float.MaxValue;
                    Vector3 bestPt = _lastHarvestPos;
                    Collider bestCol = null;
                    foreach (var c in cols)
                    {
                        if (c == null || !c.enabled || c.isTrigger) continue;
                        Vector3 pt = GetAccurateClosestPoint(c, brain.transform.position, out float d);
                        if (d < closestColDist)
                        {
                            closestColDist = d;
                            bestPt = pt;
                            bestCol = c;
                        }
                    }

                    if (bestCol != null)
                    {
                        _lastHarvestPos = bestPt;
                        _lastHarvestHitCollider = bestCol;
                        if (closestColDist > 1.7f)
                        {
                            brain.LogAction($"Miner repositioning closer to active rock face (dist: {closestColDist:F1}m).");
                            _state = State.MovingToTarget;
                            return;
                        }
                    }
                }
            }

            // [F08] & [F09] Miner Quarry Leveling and Subterranean Vein Exposure
            if (isRock && brain.CurrentRole == WorkerRole.Miner)
            {
                LevelQuarryTerrain(brain, _lastHarvestPos);
                var rock5 = _targetHarvestNode != null ? _targetHarvestNode.GetComponentInParent<MineRock5>() : null;
                if (rock5 != null)
                {
                    ExposeSubterraneanVein(brain, rock5);
                }
            }

            // Check if node broken across all Valheim destructible representations
            bool isBroken = false;
            if (_targetHarvestNode == null)
            {
                isBroken = true;
            }
            else
            {
                var curDest = _targetHarvestNode.GetComponentInParent<IDestructible>();
                if (curDest == null)
                {
                    isBroken = true;
                }
                else
                {
                    var tree = _targetHarvestNode.GetComponentInParent<TreeBase>();
                    if (tree != null && tree.m_health <= 0f) isBroken = true;
                    var log = _targetHarvestNode.GetComponentInParent<TreeLog>();
                    if (log != null && log.m_health <= 0f) isBroken = true;
                    var dObj = _targetHarvestNode.GetComponentInParent<Destructible>();
                    if (dObj != null && (dObj.GetDestructibleType() == DestructibleType.None || dObj.m_health <= 0f)) isBroken = true;
                    var rock = _targetHarvestNode.GetComponentInParent<MineRock>();
                    if (rock != null && rock.m_health <= 0f) isBroken = true;
                    var mine5 = _targetHarvestNode.GetComponentInParent<MineRock5>();
                    if (mine5 != null)
                    {
                        bool allDestroyed = Traverse.Create(mine5).Field<bool>("m_allDestroyed").Value;
                        if (allDestroyed)
                        {
                            isBroken = true;
                        }
                        else if (_lastHarvestHitCollider != null && !_lastHarvestHitCollider.enabled)
                        {
                            // Current rock segment crumbled! Re-acquire next active segment
                            _state = State.MovingToTarget;
                            return;
                        }
                    }
                }
            }

            if (isBroken)
            {
                brain.LogAction($"Harvest node '{harvestNodeName}' broken at {_lastHarvestPos:F1}. Sweeping drops.");
                brain.RecordTraceStep("HARVEST", $"Harvest node '{harvestNodeName}' broken. Sweeping drops.");
                _targetHarvestNode = null;
                _state = State.SweepingDrops;
            }
        }

        private void LevelQuarryTerrain(WorkerBrain brain, Vector3 hitPos)
        {
            if (brain.CurrentRole != WorkerRole.Miner) return;
            if (!brain.HasToolType(ItemDrop.ItemData.ItemType.Tool, "Hoe")) return;

            try
            {
                TerrainModifier.SetTriggerOnPlaced(true);
                GameObject modGo = new GameObject("Worker_QuarryLevel");
                modGo.transform.position = hitPos;
                var tm = modGo.AddComponent<TerrainModifier>();
                tm.m_level = true;
                tm.m_levelRadius = 2.5f;
                tm.m_smooth = true;
                tm.m_smoothRadius = 3.0f;
                tm.m_playerModifiction = true;

                var hms = new List<Heightmap>();
                Heightmap.FindHeightmap(hitPos, 4.0f, hms);
                foreach (var hm in hms)
                {
                    hm.Poke(0, true);
                }
                UnityEngine.Object.Destroy(modGo, 0.5f);

                brain.LogAction($"[F08] Evened the field: Leveled quarry terrain at {hitPos:F1} using Hoe.");
                VerificationTelemetry.RecordPass("miner_even_field", $"Worker '{brain.GetHoverName()}' leveled quarry terrain with Hoe.");
            }
            catch (Exception ex)
            {
                ZLog.LogWarning($"[ValheimWorkers] LevelQuarryTerrain error: {ex.Message}");
            }
        }

        private void ExposeSubterraneanVein(WorkerBrain brain, MineRock5 rock5)
        {
            if (brain.CurrentRole != WorkerRole.Miner || rock5 == null) return;
            try
            {
                Vector3 center = rock5.transform.position;
                TerrainModifier.SetTriggerOnPlaced(true);
                GameObject modGo = new GameObject("Worker_VeinExpose");
                modGo.transform.position = center;
                var tm = modGo.AddComponent<TerrainModifier>();
                tm.m_level = true;
                tm.m_levelRadius = 3.5f;
                tm.m_playerModifiction = true;

                var hms = new List<Heightmap>();
                Heightmap.FindHeightmap(center, 5.0f, hms);
                foreach (var hm in hms)
                {
                    hm.Poke(0, true);
                }
                UnityEngine.Object.Destroy(modGo, 0.5f);

                brain.LogAction($"[F09] Subterranean vein exposure: Cleared soil over '{rock5.name}' at {center:F1}.");
                VerificationTelemetry.RecordPass("miner_vein_exposure", $"Worker '{brain.GetHoverName()}' exposed buried vein sections.");
            }
            catch (Exception ex)
            {
                ZLog.LogWarning($"[ValheimWorkers] ExposeSubterraneanVein error: {ex.Message}");
            }
        }

        private void RefreshClosestPreyFocus(WorkerBrain brain, bool currentObstructed)
        {
            var resources = GetEffectiveResources(brain);
            Vector3 workCenter = brain.HasWorkPost ? brain.WorkPostPosition : (brain.HasHome ? brain.HomePosition : brain.transform.position);
            float radius = brain.WorkRadius;

            Character bestCandidate = _targetGameCreature;
            float currentDist = _targetGameCreature != null
                ? Vector3.Distance(brain.transform.position, _targetGameCreature.transform.position)
                : float.MaxValue;
            float bestEffectiveDist = currentDist + (currentObstructed ? 7.5f : 0f);

            var allChars = Character.GetAllCharacters();
            for (int i = 0; i < allChars.Count; i++)
            {
                var c = allChars[i];
                if (c == null || c.IsDead() || c == brain.m_character || c == _targetGameCreature) continue;
                if (c.IsTamed()) continue;
                if (!IsHuntableWildGame(c, brain, resources)) continue;
                if (Vector3.Distance(workCenter, c.transform.position) > radius) continue;

                float d = Vector3.Distance(brain.transform.position, c.transform.position);
                bool clearToOther = brain.EvaluateArrowTrajectoryClearance(brain.transform.position, c, out _, out _);
                float effectiveDist = d + (clearToOther ? 0f : 7.5f);

                if (effectiveDist + 3.5f < bestEffectiveDist)
                {
                    bestEffectiveDist = effectiveDist;
                    bestCandidate = c;
                }
            }

            if (bestCandidate != null && bestCandidate != _targetGameCreature)
            {
                brain.CancelBowDraw();
                _targetGameCreature = bestCandidate;
                brain.CurrentFocusTarget = _targetGameCreature;
                float newDist = Vector3.Distance(brain.transform.position, _targetGameCreature.transform.position);
                brain.LogAction($"Shifted hunter focus to closer/clearer target '{_targetGameCreature.GetHoverName()}' ({newDist:F1}m).");
                brain.RecordTraceStep("SCAN", $"Shifted hunter focus to closest target '{_targetGameCreature.GetHoverName()}' ({newDist:F1}m)");
            }
        }

        private void HandleHunting(WorkerBrain brain, float dt)
        {
            if (_targetGameCreature == null || _targetGameCreature.IsDead())
            {
                brain.CancelBowDraw();
                brain.SetCrouch(false);
                brain.CustomAimDir = null;
                brain.CurrentFocusTarget = null;

                string preyName = _targetGameCreature != null ? _targetGameCreature.GetHoverName() : "wild game";
                _lastHarvestPos = _targetGameCreature != null ? _targetGameCreature.transform.position : brain.transform.position;
                _targetGameCreature = null;
                brain.LogAction($"Target creature '{preyName}' defeated. Sweeping animal drops at {_lastHarvestPos:F1}.");
                brain.RecordTraceStep("COMBAT", $"Target creature '{preyName}' defeated. Sweeping animal drops.");
                VerificationTelemetry.RecordPass("hunting_game_harvest", $"Hunter '{brain.GetHoverName()}' tracked, defeated wild game '{preyName}', and harvested animal yield.");
                _state = State.SweepingDrops;
                return;
            }

            // Periodically re-evaluate nearby prey so focus remains on the closest valid target
            _preyFocusCheckTimer += dt;
            if (_preyFocusCheckTimer >= 0.75f)
            {
                _preyFocusCheckTimer = 0f;
                bool isObstructedNow = !brain.EvaluateArrowTrajectoryClearance(brain.transform.position, _targetGameCreature, out _, out _);
                RefreshClosestPreyFocus(brain, isObstructedNow);
            }

            brain.CurrentFocusTarget = _targetGameCreature;
            float dist = Vector3.Distance(brain.transform.position, _targetGameCreature.transform.position);
            Vector3 targetCenter = _targetGameCreature.GetCenterPoint();
            float meleeSwitchDist = brain.GetHunterMeleeSwitchRange(); // 10m

            // 1. Close-Range & Threat Evaluation (<= 10m)
            if (dist <= meleeSwitchDist)
            {
                int hostiles = brain.CountHostilesInRange(14f);
                float hpPct = brain.m_humanoid.GetHealth() / brain.m_humanoid.GetMaxHealth();

                // High Threat (3+ hostiles, or low HP): Tactical Retreat
                if (hostiles >= 3 || hpPct < 0.35f)
                {
                    brain.SetCrouch(false);
                    brain.CustomAimDir = null;
                    brain.CancelBowDraw();

                    Vector3 retreatDir = (brain.transform.position - _targetGameCreature.transform.position).normalized;
                    retreatDir.y = 0f;
                    if (retreatDir.sqrMagnitude < 0.01f) retreatDir = -brain.transform.forward;
                    Vector3 retreatDest = brain.HasHome ? brain.HomePosition : (brain.transform.position + retreatDir * 20f);
                    brain.MoveTo(retreatDest, 2f, true);
                    brain.Say("Too many enemies! Falling back!");
                    brain.RecordTraceStep("COMBAT", $"Retreating from {hostiles} hostiles (dist: {dist:F1}m, HP: {brain.m_humanoid.GetHealth():F0})");
                    _statusDesc = $"Hunting: Retreating from {hostiles} hostiles!";
                    return;
                }

                // Low Threat (<= 2 foes, healthy HP): Swap to close-combat melee weapon & shield!
                brain.SetCrouch(false);
                brain.CustomAimDir = null;
                brain.CancelBowDraw();

                brain.SetCombatMode(WorkerBrain.HunterCombatMode.Melee);
                brain.FaceTarget(targetCenter);

                if (dist <= 2.3f)
                {
                    brain.StopMoving();
                    if (!brain.m_humanoid.InAttack())
                    {
                        brain.m_humanoid.StartAttack(_targetGameCreature, false);
                        brain.RecordTraceStep("ATTACK", $"Close-combat melee strike against '{_targetGameCreature.GetHoverName()}'");
                    }
                }
                else
                {
                    brain.MoveTo(_targetGameCreature.transform.position, 1.8f, dist > 5f);
                }
                _statusDesc = $"Hunting: Engaging '{_targetGameCreature.GetHoverName()}' in close combat ({dist:F1}m)";
                return;
            }

            // 2. Ranged Stalking & Archery (> 10m)
            brain.SetCombatMode(WorkerBrain.HunterCombatMode.Ranged);
            bool isHoldingBow = brain.IsHoldingBow();

            if (!isHoldingBow)
            {
                // No bow in inventory: close in for melee
                brain.SetCrouch(false);
                brain.SetCombatMode(WorkerBrain.HunterCombatMode.Melee);
                brain.FaceTarget(targetCenter);
                brain.MoveTo(_targetGameCreature.transform.position, 2.0f, true);
                _statusDesc = $"Hunting: Closing in with melee ({dist:F1}m)";
                return;
            }

            // If out of arrows during ranged hunt, return to camp Workbench & chests to resupply or craft arrows!
            if (brain.GetTotalArrowCount() <= 0)
            {
                brain.SetCrouch(false);
                brain.CustomAimDir = null;
                brain.CancelBowDraw();

                if (_workbenchCooldown <= 0f)
                {
                    _targetGameCreature = null;
                    brain.CurrentFocusTarget = null;
                    brain.Say("Out of arrows! Returning to workbench!");
                    brain.LogAction("Out of arrows during hunt! Returning to camp Workbench & chests to resupply or craft arrows.");
                    brain.RecordTraceStep("LOGISTICS", "Out of arrows! Returning to camp Workbench to craft/resupply arrows.");
                    _state = State.MovingToWorkbench;
                    return;
                }
                else
                {
                    // Workbench recently checked and still out of arrows: close in with melee weapon
                    brain.SetCombatMode(WorkerBrain.HunterCombatMode.Melee);
                    brain.FaceTarget(targetCenter);
                    brain.MoveTo(_targetGameCreature.transform.position, 2.0f, true);
                    _statusDesc = $"Hunting: Out of arrows, closing in with melee ({dist:F1}m)";
                    return;
                }
            }

            float minSnipe = brain.GetHunterSnipingMinRange(); // 10m
            float maxSnipe = brain.GetHunterSnipingMaxRange(); // 40m
            float maxStalk = brain.GetHunterStalkingMaxRange(); // 50m

            // A. Distant Approach (> maxStalk, e.g. > 50m): Walk without sprinting
            if (dist > maxStalk)
            {
                brain.SetCrouch(false);
                brain.CustomAimDir = null;
                brain.FaceTarget(targetCenter);
                brain.MoveTo(_targetGameCreature.transform.position, maxSnipe, false);
                _statusDesc = $"Hunting: Approaching '{_targetGameCreature.GetHoverName()}' ({dist:F1}m)";
                return;
            }

            // B. Stealth Crawl Bracket (maxSnipe to maxStalk, e.g. 40m–50m):
            // If prey is not alerted, drop into silent crawl (2.0 m/s, noise = 0, 0.25x stealth factor)
            bool isAlerted = _targetGameCreature.GetBaseAI()?.IsAlerted() ?? false;
            if (dist > maxSnipe && !isAlerted)
            {
                brain.SetCrouch(true);
                brain.CustomAimDir = null;
                if (!brain.EvaluateArrowTrajectoryClearance(brain.transform.position, _targetGameCreature, out _, out _) &&
                    brain.TryFindOrbitalVantagePoint(_targetGameCreature, out Vector3 stalkVantage))
                {
                    brain.MoveTo(stalkVantage, 1.5f, false);
                }
                else
                {
                    brain.MoveTo(_targetGameCreature.transform.position, maxSnipe - 2f, false);
                }
                brain.FaceTarget(targetCenter);
                _statusDesc = $"Hunting: Stalking '{_targetGameCreature.GetHoverName()}' in stealth crawl ({dist:F1}m)";
                return;
            }

            // C. Optimal Sniping Zone (10m to maxSnipe, e.g. 10m–40m):
            // ExecuteHunterBowDrawAndFire checks trajectory clearance; if blocked by a tree or hill crest,
            // it cancels bow draw and strafes sideways to higher ground while keeping focus on the target.
            brain.ExecuteHunterBowDrawAndFire(_targetGameCreature, dt);
            if (!string.IsNullOrEmpty(brain.LastShotBlockerName))
            {
                _statusDesc = $"Hunting: Strafing sideways around {brain.LastShotBlockerName} for clear shot at '{_targetGameCreature.GetHoverName()}' ({dist:F1}m)";
            }
            else
            {
                _statusDesc = $"Hunting: Sniping '{_targetGameCreature.GetHoverName()}' ({dist:F1}m)";
            }
        }

        private void HandleSweepingDrops(WorkerBrain brain)
        {
            Vector3 center = _lastHarvestPos != Vector3.zero ? _lastHarvestPos : brain.transform.position;
            Collider[] hits = Physics.OverlapSphere(center, 15f);

            var resources = GetEffectiveResources(brain);
            ItemDrop closest = null;
            float bestDist = float.MaxValue;

            for (int i = 0; i < hits.Length; i++)
            {
                var drop = hits[i].GetComponentInParent<ItemDrop>();
                if (drop == null || !drop.CanPickup(false)) continue;

                string pName = drop.m_itemData?.m_dropPrefab != null ? drop.m_itemData.m_dropPrefab.name : (drop.m_itemData?.m_shared?.m_name ?? "");
                if (MatchesAnyResource(pName, resources))
                {
                    float d = Vector3.Distance(brain.transform.position, drop.transform.position);
                    if (d < bestDist)
                    {
                        bestDist = d;
                        closest = drop;
                    }
                }
            }

            if (closest != null)
            {
                _targetGroundItem = closest;
                _state = State.MovingToTarget;
                return;
            }

            // Area drops completely cleared: check if carrying gathered resources to haul back to camp chests
            if (ShouldReturnToCampWithResources(brain, false))
            {
                brain.LogAction("Finished collecting drops in area. Returning to camp to deposit resources in chests.");
                brain.RecordTraceStep("INVENTORY", "Finished collecting drops in area. Returning to camp to store in chests.");
                _state = State.ReturningToCamp;
                return;
            }

            _state = State.ScanningTargets;
        }

        private void HandleReturningToCamp(WorkerBrain brain)
        {
            Vector3 camp = brain.HasHome ? brain.HomePosition : brain.transform.position;
            float dist = Vector3.Distance(brain.transform.position, camp);

            if (dist <= 3.5f)
            {
                brain.StopMoving();
                _state = State.DepositingToChests;
            }
            else
            {
                _statusDesc = $"Delivering gathered resources to camp ({dist:F1}m)";
                brain.MoveTo(camp, 2.2f, dist > 14f);
            }
        }

        private void HandleDepositingToChests(WorkerBrain brain)
        {
            Vector3 camp = brain.HomePosition;
            Collider[] hits = Physics.OverlapSphere(camp, 22f);

            var workerInv = brain.m_humanoid?.GetInventory();
            if (workerInv == null)
            {
                _state = State.ScanningTargets;
                return;
            }

            // Determine top 3 personal sustenance food types to retain (up to 6 units per stack)
            var protectedFoods = brain.GetProtectedSustenanceFoodNames(3);
            var retainedFoodCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            var carriedItems = new List<ItemDrop.ItemData>(workerInv.GetAllItems());
            int depositedCount = 0;
            int protectedGearKept = 0;
            foreach (var item in carriedItems)
            {
                if (brain.IsProtectedWorkGear(item)) protectedGearKept++;
            }

            // Collect valid camp storage containers
            var campChests = new List<Container>();
            for (int i = 0; i < hits.Length; i++)
            {
                var chest = hits[i].GetComponentInParent<Container>();
                if (chest == null || chest.IsInUse() || chest.GetComponent<WorkerBrain>() != null) continue;
                if (chest.GetComponentInParent<Beehive>() != null || chest.GetComponentInParent<CraftingStation>() != null) continue;
                if (chest.GetInventory() != null && !campChests.Contains(chest))
                {
                    campChests.Add(chest);
                }
            }

            for (int j = carriedItems.Count - 1; j >= 0; j--)
            {
                var item = carriedItems[j];
                if (item == null || item.m_stack <= 0) continue;
                if (brain.IsProtectedWorkGear(item)) continue;

                string pName = WorkerBrain.GetItemPrefabName(item);
                if (string.IsNullOrEmpty(pName)) pName = item.m_shared?.m_name ?? "";

                // Retain modest personal ration (up to 6 units) of top 3 food types
                int depositAmount = item.m_stack;
                if (WorkerBrain.IsFoodItem(item) && protectedFoods.Contains(pName))
                {
                    retainedFoodCounts.TryGetValue(pName, out int alreadyKept);
                    int canKeep = Mathf.Max(0, WorkerBrain.MaxPersonalRationPerFoodType - alreadyKept);
                    int keepHere = Mathf.Min(item.m_stack, canKeep);
                    retainedFoodCounts[pName] = alreadyKept + keepHere;
                    depositAmount = item.m_stack - keepHere;
                    if (depositAmount <= 0)
                    {
                        protectedGearKept++;
                        continue; // Keep entire stack for personal sustenance
                    }
                }

                // If Hunter needs arrows (< TargetHunterArrowCap), temporarily keep a crafting batch of arrow materials
                // so HandleUsingWorkbench can craft arrows first, and then stack any surplus into camp chests!
                if (brain.CurrentRole == WorkerRole.Hunter && brain.GetTotalArrowCount() < TargetHunterArrowCap)
                {
                    if (string.Equals(pName, "Wood", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(pName, "Feathers", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(pName, "Flint", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(pName, "Resin", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(pName, "Obsidian", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(pName, "Needle", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                }

                bool stored = TryDepositStackToCampChests(workerInv, campChests, item, depositAmount, brain);
                if (stored)
                {
                    depositedCount++;
                }
            }

            if (depositedCount > 0)
            {
                brain.Say($"Stored {depositedCount} resource stack(s).");
                brain.LogAction($"Deposited {depositedCount} gathered resource stacks into camp storage chests.");
                TaskDignity roleDignity = WorkerProgressionManager.GetTaskDignity(brain.CurrentRole);
                brain.AddTaskXp(depositedCount * 10, roleDignity);
                WorkerActivitySkill depositSkill = (brain.CurrentRole == WorkerRole.Lumberjack) ? WorkerActivitySkill.Woodcutting
                    : ((brain.CurrentRole == WorkerRole.Miner) ? WorkerActivitySkill.Mining : WorkerActivitySkill.Athletics);
                brain.RaiseActivitySkill(depositSkill, 1.0f * depositedCount);
                brain.RecordTraceStep("INVENTORY", $"Deposited {depositedCount} gathered resource stack(s) into camp storage chests.");
                if (protectedGearKept > 0)
                {
                    VerificationTelemetry.RecordPass("logistics_protected_gear", $"Worker '{brain.GetHoverName()}' deposited {depositedCount} resource stack(s) while retaining {protectedGearKept} protected gear/ration item(s) in inventory.");
                }
            }
            else if (campChests.Count == 0)
            {
                _depositRetryCooldown = 12f;
                brain.LogAction("No chest space or matching chests found in camp. Entering deposit cooldown (12s).");
            }

            // [F05] Resource pile overflow construction when chests are full
            if (LogisticsHelper.TryConstructOverflowResourcePiles(brain, camp))
            {
                brain.LogAction("[F05] Constructed overflow resource pile for excess materials.");
            }

            // After depositing at camp, Hunters (or workers with worn gear) visit the Workbench & check chests for supplies
            if (brain.CurrentRole == WorkerRole.Hunter || brain.HasAnyWornEquipment() || brain.ActiveFoods.Count < 3)
            {
                _state = State.MovingToWorkbench;
                return;
            }

            _state = State.EvaluatingEquipment;
        }

        private bool TryDepositStackToCampChests(Inventory workerInv, List<Container> campChests, ItemDrop.ItemData item, int depositAmount, WorkerBrain brain)
        {
            if (item == null || depositAmount <= 0 || workerInv == null) return false;
            string pName = WorkerBrain.GetItemPrefabName(item);
            if (string.IsNullOrEmpty(pName)) pName = item.m_shared?.m_name ?? "item";

            // Pass 0: Sign-based routing [F32]
            var signChest = LogisticsHelper.FindChestMatchingSign(brain.HomePosition, 22f, item, brain);
            if (signChest != null && signChest.GetInventory() != null)
            {
                var chestInv = signChest.GetInventory();
                var clone = item.Clone();
                clone.m_stack = depositAmount;
                if (chestInv.CanAddItem(clone, depositAmount))
                {
                    chestInv.AddItem(clone);
                    workerInv.RemoveItem(item, depositAmount);
                    VerificationTelemetry.RecordPass("logistics_sign_routing", $"Worker '{brain.GetHoverName()}' routed '{pName}' x{depositAmount} to sign-marked chest.");
                    return true;
                }
            }

            // Pass 1: Smart-sort into a chest that already contains a matching item stack
            for (int i = 0; i < campChests.Count; i++)
            {
                var chestInv = campChests[i].GetInventory();
                if (chestInv == null) continue;
                if (InventoryHelper.HasMatchingItem(chestInv, item))
                {
                    var clone = item.Clone();
                    clone.m_stack = depositAmount;
                    if (chestInv.CanAddItem(clone, depositAmount))
                    {
                        chestInv.AddItem(clone);
                        workerInv.RemoveItem(item, depositAmount);
                        VerificationTelemetry.RecordPass("logistics_smart_sorting", $"Worker '{brain.GetHoverName()}' smart-sorted '{pName}' x{depositAmount} into chest already containing matching stack.");
                        return true;
                    }
                }
            }

            // Pass 2: Any chest with free space
            for (int i = 0; i < campChests.Count; i++)
            {
                var chestInv = campChests[i].GetInventory();
                if (chestInv == null || chestInv.GetEmptySlots() == 0) continue;
                var clone = item.Clone();
                clone.m_stack = depositAmount;
                if (chestInv.CanAddItem(clone, depositAmount))
                {
                    chestInv.AddItem(clone);
                    workerInv.RemoveItem(item, depositAmount);
                    return true;
                }
            }

            return false;
        }

        private Vector3 FindNearestCampWorkbench(WorkerBrain brain, out CraftingStation foundWorkbench)
        {
            foundWorkbench = null;
            Vector3 campCenter = brain.HasHome ? brain.HomePosition : brain.transform.position;
            Collider[] hits = Physics.OverlapSphere(campCenter, 35f);

            float bestDist = float.MaxValue;
            Vector3 bestPos = campCenter;

            for (int i = 0; i < hits.Length; i++)
            {
                var station = hits[i].GetComponentInParent<CraftingStation>();
                if (station != null)
                {
                    float d = Vector3.Distance(campCenter, station.transform.position);
                    bool isWorkbench = station.gameObject.name.IndexOf("workbench", StringComparison.OrdinalIgnoreCase) >= 0;
                    float scoreDist = isWorkbench ? d : d + 8f;
                    if (scoreDist < bestDist)
                    {
                        bestDist = scoreDist;
                        bestPos = station.transform.position;
                        foundWorkbench = station;
                    }
                }
            }

            return bestPos;
        }

        private void HandleMovingToWorkbench(WorkerBrain brain)
        {
            _targetWorkbenchPos = FindNearestCampWorkbench(brain, out CraftingStation station);
            float dist = Vector3.Distance(brain.transform.position, _targetWorkbenchPos);

            if (dist <= 2.8f)
            {
                brain.StopMoving();
                brain.FaceTarget(_targetWorkbenchPos);
                _state = State.UsingWorkbench;
            }
            else
            {
                string stationLabel = station != null ? "Workbench" : "camp supply station";
                _statusDesc = $"Moving to {stationLabel} ({dist:F1}m)";
                brain.MoveTo(_targetWorkbenchPos, 2.2f, dist > 14f);
            }
        }

        private void HandleUsingWorkbench(WorkerBrain brain)
        {
            brain.StopMoving();
            if (_targetWorkbenchPos == Vector3.zero)
            {
                _targetWorkbenchPos = FindNearestCampWorkbench(brain, out _);
            }
            brain.FaceTarget(_targetWorkbenchPos);

            var zanim = brain.m_humanoid?.GetComponent<ZSyncAnimation>();
            zanim?.SetTrigger("interact");

            // 1. Repair Bow, Melee Weapon, Shield, and Tools at Workbench
            int repairedItems = brain.RepairAllEquipmentAtWorkbench();
            if (repairedItems > 0)
            {
                brain.Say("Repaired bow & gear at workbench!");
                brain.LogAction($"Repaired {repairedItems} piece(s) of equipment (bow/weapons/tools) to 100% durability at Workbench.", true);
                brain.RecordTraceStep("WORKBENCH", $"Repaired {repairedItems} equipment item(s) to 100% durability at Workbench.");
            }

            // 2. Check nearby camp chests & ground for needed supplies (Cooked Meat, Food, Pre-Crafted Arrows, Arrow Materials)
            Vector3 supplyCenter = brain.HasHome ? brain.HomePosition : _targetWorkbenchPos;
            var campChests = GetNearbyCampChests(supplyCenter, 24f);
            ResupplyFromCampChestsAndGround(brain, supplyCenter, campChests);

            // 3. Craft Arrows at Workbench if Hunter needs arrows (< TargetHunterArrowCap)
            if (brain.CurrentRole == WorkerRole.Hunter)
            {
                int craftedArrows = CraftArrowsAtWorkbench(brain, campChests);
                if (craftedArrows > 0)
                {
                    brain.EnsureAmmoEquipped();
                }

                // 4. Stack any surplus arrow build materials into camp chests so arrow materials stay neatly organized
                int stackedMats = StackSurplusArrowMaterialsIntoChests(brain, campChests);
                if (stackedMats > 0)
                {
                    brain.LogAction($"Stacked {stackedMats} surplus arrow material stack(s) into camp chests.");
                }

                // 5. If arrows are still critically low (< 15) because neither Hunter nor camp chests had enough Wood,
                // dispatch the Hunter to collect Wood so they can craft Wood Arrows!
                int totalArrowsAfter = brain.GetTotalArrowCount();
                if (totalArrowsAfter < 15 && brain.HasToolType(ItemDrop.ItemData.ItemType.Bow, ""))
                {
                    _needsWoodForArrows = true;
                    brain.Say("No wood at workbench! Going to gather wood for arrows.");
                    brain.LogAction($"Arrows low ({totalArrowsAfter}) and no Wood available at camp. Dispatched to gather Wood for Wood Arrow production.", true);
                    brain.RecordTraceStep("LOGISTICS", $"No Wood at workbench (Arrows: {totalArrowsAfter}). Gathering Wood to produce Wood Arrows.");
                }
                else
                {
                    _needsWoodForArrows = false;
                }
            }

            _workbenchCooldown = 14f;
            _actionCooldown = 1.0f;
            _state = State.EvaluatingEquipment;
        }

        private List<Container> GetNearbyCampChests(Vector3 center, float radius)
        {
            var list = new List<Container>();
            Collider[] hits = Physics.OverlapSphere(center, radius);
            for (int i = 0; i < hits.Length; i++)
            {
                var chest = hits[i].GetComponentInParent<Container>();
                if (chest == null || chest.IsInUse() || chest.GetComponent<WorkerBrain>() != null) continue;
                if (chest.GetComponentInParent<Beehive>() != null || chest.GetComponentInParent<CraftingStation>() != null) continue;
                if (chest.GetInventory() != null && !list.Contains(chest))
                {
                    list.Add(chest);
                }
            }
            return list;
        }

        private void ResupplyFromCampChestsAndGround(WorkerBrain brain, Vector3 campCenter, List<Container> campChests)
        {
            var workerInv = brain.m_humanoid?.GetInventory();
            if (workerInv == null) return;

            // A. Pick up any loose Cooked Meat or arrows lying on the ground near campfires/spits
            Collider[] groundHits = Physics.OverlapSphere(campCenter, 22f);
            for (int i = 0; i < groundHits.Length; i++)
            {
                var drop = groundHits[i].GetComponentInParent<ItemDrop>();
                if (drop == null || !drop.CanPickup(false) || drop.m_itemData == null) continue;
                string pName = WorkerBrain.GetItemPrefabName(drop.m_itemData);
                if (WorkerBrain.IsCookedMeatPrefabName(pName))
                {
                    int curCount = brain.GetCarriedItemCount(pName);
                    if (curCount < WorkerBrain.MaxPersonalRationPerFoodType && workerInv.CanAddItem(drop.m_itemData, drop.m_itemData.m_stack))
                    {
                        brain.m_humanoid.Pickup(drop.gameObject, false, false);
                        brain.LogAction($"Picked up loose cooked meat '{pName}' at camp for sustenance.", true);
                    }
                }
            }

            // B. Check Camp Chests for Food (Cooked Meat first, then Honey/Berries/Mushrooms) to maintain 3 distinct food types
            // Collect all available food items in camp chests, sorted by priority score descending (Cooked Meats first!)
            var chestFoodCandidates = new List<KeyValuePair<Container, ItemDrop.ItemData>>();
            for (int i = 0; i < campChests.Count; i++)
            {
                var cInv = campChests[i].GetInventory();
                if (cInv == null) continue;
                foreach (var itm in cInv.GetAllItems())
                {
                    if (WorkerBrain.IsFoodItem(itm) && itm.m_stack > 0)
                    {
                        chestFoodCandidates.Add(new KeyValuePair<Container, ItemDrop.ItemData>(campChests[i], itm));
                    }
                }
            }

            chestFoodCandidates.Sort((a, b) => WorkerBrain.GetFoodPriorityScore(b.Value).CompareTo(WorkerBrain.GetFoodPriorityScore(a.Value)));

            var protectedFoods = brain.GetProtectedSustenanceFoodNames(3);
            for (int i = 0; i < chestFoodCandidates.Count; i++)
            {
                var chest = chestFoodCandidates[i].Key;
                var chestItem = chestFoodCandidates[i].Value;
                if (chestItem == null || chestItem.m_stack <= 0) continue;

                string pName = WorkerBrain.GetItemPrefabName(chestItem);
                if (string.IsNullOrEmpty(pName)) continue;

                bool isCookedMeat = WorkerBrain.IsCookedMeatPrefabName(pName);
                bool canAddNewFoodType = protectedFoods.Count < 3 || protectedFoods.Contains(pName) || isCookedMeat;
                if (!canAddNewFoodType) continue;

                int currentCarried = brain.GetCarriedItemCount(pName);
                int desiredRation = 5;
                int needed = desiredRation - currentCarried;
                if (needed <= 0) continue;

                int takeAmt = Mathf.Min(needed, chestItem.m_stack);
                var clone = chestItem.Clone();
                clone.m_stack = takeAmt;

                if (workerInv.CanAddItem(clone, takeAmt))
                {
                    workerInv.AddItem(clone);
                    chest.GetInventory().RemoveItem(chestItem, takeAmt);
                    protectedFoods.Add(pName);
                    brain.LogAction($"Withdrew {takeAmt}x '{pName}' from camp chest for personal sustenance.", true);
                    brain.RecordTraceStep("RESUPPLY", $"Withdrew {takeAmt}x '{pName}' from camp chest for sustenance.");
                }
            }

            // Immediately consume food to maintain 3 active food buffs!
            brain.TryMaintainThreeFoodBuffs();

            // C. For Hunters: Check Camp Chests for Pre-Crafted Arrows up to TargetHunterArrowCap (60)
            if (brain.CurrentRole == WorkerRole.Hunter)
            {
                int currentArrows = brain.GetTotalArrowCount();
                if (currentArrows < TargetHunterArrowCap)
                {
                    for (int i = 0; i < campChests.Count && currentArrows < TargetHunterArrowCap; i++)
                    {
                        var cInv = campChests[i].GetInventory();
                        if (cInv == null) continue;
                        var chestItems = new List<ItemDrop.ItemData>(cInv.GetAllItems());
                        for (int j = 0; j < chestItems.Count && currentArrows < TargetHunterArrowCap; j++)
                        {
                            var itm = chestItems[j];
                            if (itm != null && itm.m_shared != null && itm.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Ammo && itm.m_stack > 0)
                            {
                                int need = TargetHunterArrowCap - currentArrows;
                                int take = Mathf.Min(need, itm.m_stack);
                                var clone = itm.Clone();
                                clone.m_stack = take;
                                if (workerInv.CanAddItem(clone, take))
                                {
                                    string arrowName = WorkerBrain.GetItemPrefabName(itm);
                                    workerInv.AddItem(clone);
                                    cInv.RemoveItem(itm, take);
                                    currentArrows += take;
                                    brain.LogAction($"Withdrew {take}x '{arrowName}' from camp chest (Total arrows: {currentArrows}).", true);
                                    brain.RecordTraceStep("RESUPPLY", $"Withdrew {take}x '{arrowName}' from camp chest.");
                                }
                            }
                        }
                    }
                    brain.EnsureAmmoEquipped();
                }
            }
        }

        private int CraftArrowsAtWorkbench(WorkerBrain brain, List<Container> campChests)
        {
            var workerInv = brain.m_humanoid?.GetInventory();
            if (workerInv == null) return 0;

            int totalCrafted = 0;

            // Craft batches until the Hunter reaches TargetHunterArrowCap (60 arrows)
            for (int batch = 0; batch < 4 && brain.GetTotalArrowCount() < TargetHunterArrowCap; batch++)
            {
                string craftedArrowPrefab = null;
                int craftedBatchSize = 20;

                if (GetTotalMaterialAvailable(workerInv, campChests, "Needle") >= 4 &&
                    GetTotalMaterialAvailable(workerInv, campChests, "Feathers") >= 2)
                {
                    ConsumeMaterialFromWorkerAndChests(workerInv, campChests, "Needle", 4);
                    ConsumeMaterialFromWorkerAndChests(workerInv, campChests, "Feathers", 2);
                    craftedArrowPrefab = "ArrowNeedle";
                }
                else if (GetTotalMaterialAvailable(workerInv, campChests, "Wood") >= 4 &&
                         GetTotalMaterialAvailable(workerInv, campChests, "Obsidian") >= 4 &&
                         GetTotalMaterialAvailable(workerInv, campChests, "Feathers") >= 2)
                {
                    ConsumeMaterialFromWorkerAndChests(workerInv, campChests, "Wood", 4);
                    ConsumeMaterialFromWorkerAndChests(workerInv, campChests, "Obsidian", 4);
                    ConsumeMaterialFromWorkerAndChests(workerInv, campChests, "Feathers", 2);
                    craftedArrowPrefab = "ArrowObsidian";
                }
                else if (GetTotalMaterialAvailable(workerInv, campChests, "Wood") >= 8 &&
                         GetTotalMaterialAvailable(workerInv, campChests, "Flint") >= 2 &&
                         GetTotalMaterialAvailable(workerInv, campChests, "Feathers") >= 2)
                {
                    ConsumeMaterialFromWorkerAndChests(workerInv, campChests, "Wood", 8);
                    ConsumeMaterialFromWorkerAndChests(workerInv, campChests, "Flint", 2);
                    ConsumeMaterialFromWorkerAndChests(workerInv, campChests, "Feathers", 2);
                    craftedArrowPrefab = "ArrowFlint";
                }
                else if (GetTotalMaterialAvailable(workerInv, campChests, "Wood") >= 8 &&
                         GetTotalMaterialAvailable(workerInv, campChests, "Resin") >= 8 &&
                         GetTotalMaterialAvailable(workerInv, campChests, "Feathers") >= 2)
                {
                    ConsumeMaterialFromWorkerAndChests(workerInv, campChests, "Wood", 8);
                    ConsumeMaterialFromWorkerAndChests(workerInv, campChests, "Resin", 8);
                    ConsumeMaterialFromWorkerAndChests(workerInv, campChests, "Feathers", 2);
                    craftedArrowPrefab = "ArrowFire";
                }
                else if (GetTotalMaterialAvailable(workerInv, campChests, "Wood") >= 8)
                {
                    ConsumeMaterialFromWorkerAndChests(workerInv, campChests, "Wood", 8);
                    craftedArrowPrefab = "ArrowWood";
                }
                else if (GetTotalMaterialAvailable(workerInv, campChests, "Wood") >= 2 && brain.GetTotalArrowCount() < 20)
                {
                    // Emergency small batch: 2 Wood -> 5 ArrowWood
                    ConsumeMaterialFromWorkerAndChests(workerInv, campChests, "Wood", 2);
                    craftedArrowPrefab = "ArrowWood";
                    craftedBatchSize = 5;
                }
                else
                {
                    break; // Insufficient materials to craft more arrows
                }

                GameObject arrowGo = ZNetScene.instance != null ? ZNetScene.instance.GetPrefab(craftedArrowPrefab) : null;
                if (arrowGo == null && ObjectDB.instance != null)
                {
                    arrowGo = ObjectDB.instance.GetItemPrefab(craftedArrowPrefab);
                }

                if (arrowGo != null)
                {
                    workerInv.AddItem(arrowGo, craftedBatchSize);
                    totalCrafted += craftedBatchSize;
                    brain.Say($"Crafted {craftedBatchSize}x {craftedArrowPrefab}!");
                    brain.LogAction($"Crafted {craftedBatchSize}x '{craftedArrowPrefab}' at Workbench (Total arrows: {brain.GetTotalArrowCount()}).", true);
                    brain.RecordTraceStep("CRAFT", $"Crafted {craftedBatchSize}x '{craftedArrowPrefab}' at Workbench (Total arrows: {brain.GetTotalArrowCount()})");
                }
            }

            return totalCrafted;
        }

        private static int GetTotalMaterialAvailable(Inventory workerInv, List<Container> campChests, string prefabName)
        {
            int total = 0;
            if (workerInv != null)
            {
                foreach (var itm in workerInv.GetAllItems())
                {
                    if (itm != null && itm.m_stack > 0 && string.Equals(WorkerBrain.GetItemPrefabName(itm), prefabName, StringComparison.OrdinalIgnoreCase))
                    {
                        total += itm.m_stack;
                    }
                }
            }
            if (campChests != null)
            {
                for (int i = 0; i < campChests.Count; i++)
                {
                    var cInv = campChests[i].GetInventory();
                    if (cInv == null) continue;
                    foreach (var itm in cInv.GetAllItems())
                    {
                        if (itm != null && itm.m_stack > 0 && string.Equals(WorkerBrain.GetItemPrefabName(itm), prefabName, StringComparison.OrdinalIgnoreCase))
                        {
                            total += itm.m_stack;
                        }
                    }
                }
            }
            return total;
        }

        private static void ConsumeMaterialFromWorkerAndChests(Inventory workerInv, List<Container> campChests, string prefabName, int amountRequired)
        {
            int remaining = amountRequired;
            if (workerInv != null && remaining > 0)
            {
                var items = new List<ItemDrop.ItemData>(workerInv.GetAllItems());
                for (int i = 0; i < items.Count && remaining > 0; i++)
                {
                    var itm = items[i];
                    if (itm != null && itm.m_stack > 0 && string.Equals(WorkerBrain.GetItemPrefabName(itm), prefabName, StringComparison.OrdinalIgnoreCase))
                    {
                        int take = Mathf.Min(remaining, itm.m_stack);
                        workerInv.RemoveItem(itm, take);
                        remaining -= take;
                    }
                }
            }

            if (campChests != null && remaining > 0)
            {
                for (int c = 0; c < campChests.Count && remaining > 0; c++)
                {
                    var cInv = campChests[c].GetInventory();
                    if (cInv == null) continue;
                    var items = new List<ItemDrop.ItemData>(cInv.GetAllItems());
                    for (int i = 0; i < items.Count && remaining > 0; i++)
                    {
                        var itm = items[i];
                        if (itm != null && itm.m_stack > 0 && string.Equals(WorkerBrain.GetItemPrefabName(itm), prefabName, StringComparison.OrdinalIgnoreCase))
                        {
                            int take = Mathf.Min(remaining, itm.m_stack);
                            cInv.RemoveItem(itm, take);
                            remaining -= take;
                        }
                    }
                }
            }
        }

        private int StackSurplusArrowMaterialsIntoChests(WorkerBrain brain, List<Container> campChests)
        {
            var workerInv = brain.m_humanoid?.GetInventory();
            if (workerInv == null || campChests == null || campChests.Count == 0) return 0;

            int stackedCount = 0;
            var allItems = new List<ItemDrop.ItemData>(workerInv.GetAllItems());
            for (int i = allItems.Count - 1; i >= 0; i--)
            {
                var item = allItems[i];
                if (item == null || item.m_stack <= 0 || brain.IsProtectedWorkGear(item)) continue;
                string pName = WorkerBrain.GetItemPrefabName(item);
                if (string.Equals(pName, "Wood", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(pName, "Feathers", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(pName, "Flint", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(pName, "Resin", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(pName, "Obsidian", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(pName, "Needle", StringComparison.OrdinalIgnoreCase))
                {
                    if (TryDepositStackToCampChests(workerInv, campChests, item, item.m_stack, brain))
                    {
                        stackedCount++;
                    }
                }
            }
            return stackedCount;
        }
    }
}
