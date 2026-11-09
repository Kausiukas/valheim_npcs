using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using ValheimWorkers.Core;

namespace ValheimWorkers.AI
{
    public class BaseTenderTask : IWorkerTask
    {
        public WorkerRole Role => WorkerRole.BaseTender;

        private enum State
        {
            Scanning,
            FetchingFromChest,
            MovingToChest,
            MovingToStructure,
            Refueling,
            MovingToRepair
        }

        private enum StructureAction
        {
            FuelFireplace,
            FuelSmelter,
            LoadKilnWood,
            LoadSmelterOre
        }

        private static readonly int s_fuelHash = "fuel".GetStableHashCode();

        private State _state = State.Scanning;
        private StructureAction _action = StructureAction.FuelFireplace;
        private Fireplace _targetFireplace = null;
        private Smelter _targetSmelter = null;
        private WearNTear _targetWearNTear = null;
        private Container _supplyChest = null;
        private string _neededPrefab = "";
        private int _neededAmount = 1;
        private float _scanTimer = 0f;
        private string _statusDesc = "Base Tender: Scanning base structures...";

        public bool CanExecute(WorkerBrain brain) => brain != null && brain.m_humanoid != null;
        public bool IsPerformingAction => _state == State.Refueling || _state == State.MovingToRepair;

        public void StartTask(WorkerBrain brain)
        {
            _state = State.Scanning;
            ResetTargets();
            _scanTimer = 0f;
            _statusDesc = "Base Tender: Standing by...";
            brain.LogAction($"Started Base Tender task at {brain.HomePosition:F1} (Radius: {brain.WorkRadius:F0}m)");
        }

        public void StopTask(WorkerBrain brain)
        {
            ResetTargets();
            brain.StopMoving();
        }

        public string GetStatusDescription() => _statusDesc;

        private void ResetTargets()
        {
            _targetFireplace = null;
            _targetSmelter = null;
            _targetWearNTear = null;
            _supplyChest = null;
            _neededPrefab = "";
            _neededAmount = 1;
        }

        public void UpdateTask(WorkerBrain brain, float dt)
        {
            _scanTimer += dt;

            switch (_state)
            {
                case State.Scanning:
                    ScanForStructures(brain);
                    break;

                case State.FetchingFromChest:
                    FindChestWithSupply(brain);
                    break;

                case State.MovingToChest:
                    HandleMovingToChest(brain);
                    break;

                case State.MovingToStructure:
                    HandleMovingToStructure(brain);
                    break;

                case State.Refueling:
                    HandleRefueling(brain);
                    break;

                case State.MovingToRepair:
                    HandleMovingToRepair(brain);
                    break;
            }
        }

        private static float GetFireplaceFuel(Fireplace fire)
        {
            var nview = fire.GetComponent<ZNetView>();
            if (nview == null || nview.GetZDO() == null) return 0f;
            return nview.GetZDO().GetFloat(s_fuelHash, 0f);
        }

        private static float GetSmelterFuel(Smelter smelter)
        {
            var nview = smelter.GetComponent<ZNetView>();
            if (nview == null || nview.GetZDO() == null) return 0f;
            return nview.GetZDO().GetFloat(s_fuelHash, 0f);
        }

        private static int GetSmelterQueueSize(Smelter smelter)
        {
            if (smelter == null) return 0;
            try
            {
                return Traverse.Create(smelter).Method("GetQueueSize").GetValue<int>();
            }
            catch
            {
                var nview = smelter.GetComponent<ZNetView>();
                return nview != null && nview.GetZDO() != null ? nview.GetZDO().GetInt("queued", 0) : 0;
            }
        }

        private static int CountItemInCampChests(Collider[] hits, string prefabName)
        {
            int total = 0;
            for (int i = 0; i < hits.Length; i++)
            {
                var chest = hits[i].GetComponentInParent<Container>();
                if (chest == null || chest.IsInUse() || chest.GetComponent<WorkerBrain>() != null) continue;
                var inv = chest.GetInventory();
                if (inv == null) continue;
                var item = InventoryHelper.GetItemByPrefab(inv, prefabName);
                if (item != null) total += item.m_stack;
            }
            return total;
        }

        private static string FindAvailableOreInCampChests(Collider[] hits, Smelter smelter)
        {
            if (smelter.m_conversion == null) return null;
            for (int c = 0; c < smelter.m_conversion.Count; c++)
            {
                var conv = smelter.m_conversion[c];
                if (conv == null || conv.m_from == null) continue;
                string oreName = conv.m_from.name;
                if (CountItemInCampChests(hits, oreName) > 0)
                {
                    return oreName;
                }
            }
            return null;
        }

        private void ScanForStructures(WorkerBrain brain)
        {
            Vector3 center = brain.HasHome ? brain.HomePosition : brain.transform.position;

            if (_scanTimer >= 0.6f)
            {
                _scanTimer = 0f;
                Collider[] hits = Physics.OverlapSphere(center, brain.WorkRadius);

                // Priority 1: Fireplaces needing fuel
                for (int i = 0; i < hits.Length; i++)
                {
                    var fire = hits[i].GetComponentInParent<Fireplace>();
                    if (fire == null || fire.m_infiniteFuel) continue;

                    if (GetFireplaceFuel(fire) < fire.m_maxFuel - 1f)
                    {
                        _targetFireplace = fire;
                        _action = StructureAction.FuelFireplace;
                        _neededPrefab = fire.m_fuelItem != null ? fire.m_fuelItem.name : "Wood";
                        _neededAmount = Mathf.CeilToInt(fire.m_maxFuel - GetFireplaceFuel(fire));
                        brain.LogAction($"Detected fireplace needing '{_neededPrefab}' at {fire.transform.position:F1}.");
                        DecideMaterialSource(brain);
                        return;
                    }
                }

                // Priority 2: Smelters needing Fuel (Coal)
                for (int i = 0; i < hits.Length; i++)
                {
                    var smelter = hits[i].GetComponentInParent<Smelter>();
                    if (smelter == null || smelter.m_fuelItem == null) continue;

                    if (GetSmelterFuel(smelter) < smelter.m_maxFuel - 1)
                    {
                        _targetSmelter = smelter;
                        _action = StructureAction.FuelSmelter;
                        _neededPrefab = smelter.m_fuelItem.name;
                        _neededAmount = Mathf.Min(smelter.m_maxFuel - Mathf.FloorToInt(GetSmelterFuel(smelter)), 10);
                        brain.LogAction($"Detected smelter needing fuel '{_neededPrefab}' at {smelter.transform.position:F1}.");
                        DecideMaterialSource(brain);
                        return;
                    }
                }

                // Priority 3: Charcoal Kilns needing Wood (Smelter with m_fuelItem == null and m_maxOre > 0, converting Wood)
                for (int i = 0; i < hits.Length; i++)
                {
                    var kiln = hits[i].GetComponentInParent<Smelter>();
                    if (kiln == null || kiln.m_fuelItem != null || kiln.m_maxOre <= 0) continue;

                    // Ensure this smelter actually converts Wood to Coal (Charcoal Kiln), not Barley to Flour (Windmill) or Flax to Thread (SpinningWheel)
                    bool convertsWood = false;
                    if (kiln.m_conversion != null)
                    {
                        foreach (var conv in kiln.m_conversion)
                        {
                            if (conv != null && conv.m_from != null && conv.m_from.name == "Wood")
                            {
                                convertsWood = true;
                                break;
                            }
                        }
                    }
                    if (!convertsWood) continue;

                    int queue = GetSmelterQueueSize(kiln);
                    if (queue < kiln.m_maxOre - 2)
                    {
                        int availableWood = CountItemInCampChests(hits, "Wood");
                        if (availableWood > 20)
                        {
                            _targetSmelter = kiln;
                            _action = StructureAction.LoadKilnWood;
                            _neededPrefab = "Wood";
                            _neededAmount = Mathf.Min(kiln.m_maxOre - queue, 15);
                            brain.LogAction($"Detected charcoal kiln needing '{_neededPrefab}' at {kiln.transform.position:F1} (Camp reserve: {availableWood}).");
                            DecideMaterialSource(brain);
                            return;
                        }
                    }
                }

                // Priority 4: Smelters needing Raw Ore (Smelter with m_fuelItem != null and has coal)
                for (int i = 0; i < hits.Length; i++)
                {
                    var smelter = hits[i].GetComponentInParent<Smelter>();
                    if (smelter == null || smelter.m_fuelItem == null || smelter.m_conversion == null) continue;

                    int queue = GetSmelterQueueSize(smelter);
                    if (queue < smelter.m_maxOre - 2 && GetSmelterFuel(smelter) > 0f)
                    {
                        string matchingOre = FindAvailableOreInCampChests(hits, smelter);
                        if (!string.IsNullOrEmpty(matchingOre))
                        {
                            _targetSmelter = smelter;
                            _action = StructureAction.LoadSmelterOre;
                            _neededPrefab = matchingOre;
                            _neededAmount = Mathf.Min(smelter.m_maxOre - queue, 10);
                            brain.LogAction($"Detected smelter needing ore '{matchingOre}' at {smelter.transform.position:F1}.");
                            DecideMaterialSource(brain);
                            return;
                        }
                    }
                }

                // Priority 4B [F28]: Windmills (Barley) & Spinning Wheels (Flax)
                for (int i = 0; i < hits.Length; i++)
                {
                    var processor = hits[i].GetComponentInParent<Smelter>();
                    if (processor == null || processor.m_fuelItem != null || processor.m_conversion == null) continue;

                    foreach (var conv in processor.m_conversion)
                    {
                        if (conv?.m_from == null) continue;
                        string raw = conv.m_from.name;
                        if (raw == "Barley" || raw == "Flax")
                        {
                            int queue = GetSmelterQueueSize(processor);
                            if (queue < processor.m_maxOre - 2)
                            {
                                int available = CountItemInCampChests(hits, raw);
                                if (available > 0)
                                {
                                    _targetSmelter = processor;
                                    _action = StructureAction.LoadKilnWood;
                                    _neededPrefab = raw;
                                    _neededAmount = Mathf.Min(processor.m_maxOre - queue, 15);
                                    brain.LogAction($"Detected mill/wheel needing '{raw}' at {processor.transform.position:F1}.");
                                    DecideMaterialSource(brain);
                                    return;
                                }
                            }
                        }
                    }
                }

                // Priority 5: Damaged Structures needing Repair (when carrying or equipped with Hammer)
                if (brain.HasToolType(ItemDrop.ItemData.ItemType.Tool, "Hammer") || brain.HasToolType(ItemDrop.ItemData.ItemType.Tool, ""))
                {
                    WearNTear damagedPiece = null;
                    float minPieceDist = float.MaxValue;

                    for (int i = 0; i < hits.Length; i++)
                    {
                        var wnt = hits[i].GetComponentInParent<WearNTear>();
                        if (wnt == null) continue;
                        var nview = wnt.GetComponent<ZNetView>();
                        if (nview == null || !nview.IsValid()) continue;

                        if (wnt.GetHealthPercentage() < 0.95f)
                        {
                            float d = Vector3.Distance(brain.transform.position, wnt.transform.position);
                            if (d < minPieceDist)
                            {
                                minPieceDist = d;
                                damagedPiece = wnt;
                            }
                        }
                    }

                    if (damagedPiece != null)
                    {
                        _targetWearNTear = damagedPiece;
                        _state = State.MovingToRepair;
                        _statusDesc = $"Tender: Repairing {damagedPiece.gameObject.name}";
                        brain.LogAction($"Detected damaged structure '{damagedPiece.gameObject.name}' ({damagedPiece.GetHealthPercentage() * 100f:F0}% HP). Moving to repair.");
                        return;
                    }
                }
            }

            // Return near home
            float distToPost = Vector3.Distance(brain.transform.position, center);
            if (distToPost > 3.5f)
            {
                _statusDesc = $"Returning to post ({distToPost:F1}m)";
                brain.LogAction($"All fires/kilns maintained. Returning to home post {center:F1} (dist: {distToPost:F1}m)");
                brain.MoveTo(center, 2f, distToPost > 12f);
            }
            else
            {
                // [F05] If carried inventory has surplus wood or coal and chests are full, build overflow piles
                LogisticsHelper.TryConstructOverflowResourcePiles(brain, center);
                brain.StopMoving();
                _statusDesc = "Tender: Base fires, kilns, and structures in order";
                brain.LogAction($"Standing by at home post {center:F1} (all systems maintained)");
            }
        }

        private void DecideMaterialSource(WorkerBrain brain)
        {
            var inv = brain.m_humanoid?.GetInventory();
            if (inv != null && InventoryHelper.HasItemPrefab(inv, _neededPrefab))
            {
                _state = State.MovingToStructure;
                _statusDesc = $"Tender: Delivering {_neededPrefab}...";
                brain.LogAction($"Using carried '{_neededPrefab}' to fulfill structure order.");
            }
            else
            {
                _state = State.FetchingFromChest;
            }
        }

        private void FindChestWithSupply(WorkerBrain brain)
        {
            _statusDesc = $"Tender: Searching chest for {_neededPrefab}...";
            Vector3 center = brain.HasHome ? brain.HomePosition : brain.transform.position;
            Collider[] hits = Physics.OverlapSphere(center, brain.WorkRadius);

            Container bestChest = null;
            float minDist = float.MaxValue;

            for (int i = 0; i < hits.Length; i++)
            {
                var chest = hits[i].GetComponentInParent<Container>();
                if (chest == null || chest.IsInUse() || chest.GetComponent<WorkerBrain>() != null) continue;
                if (chest.GetInventory() == null) continue;

                if (InventoryHelper.HasItemPrefab(chest.GetInventory(), _neededPrefab))
                {
                    float dist = Vector3.Distance(brain.transform.position, chest.transform.position);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        bestChest = chest;
                    }
                }
            }

            if (bestChest != null)
            {
                _supplyChest = bestChest;
                _state = State.MovingToChest;
                _statusDesc = $"Tender: Fetching {_neededPrefab} from chest...";
                brain.LogAction($"Found '{_neededPrefab}' in chest at {_supplyChest.transform.position:F1}. Moving to withdraw.");
            }
            else
            {
                _statusDesc = $"Tender: Missing {_neededPrefab} in chests";
                brain.LogAction($"No chest within {brain.WorkRadius:F0}m has '{_neededPrefab}'. Returning to scan.");
                ResetTargets();
                _state = State.Scanning;
            }
        }

        private void HandleMovingToChest(WorkerBrain brain)
        {
            if (_supplyChest == null)
            {
                _state = State.Scanning;
                return;
            }

            float dist = Vector3.Distance(brain.transform.position, _supplyChest.transform.position);
            if (dist <= 2.8f)
            {
                brain.StopMoving();
                brain.FaceTarget(_supplyChest.transform.position);

                var chestInv = _supplyChest.GetInventory();
                var workerInv = brain.m_humanoid?.GetInventory();

                if (chestInv != null && workerInv != null)
                {
                    var item = InventoryHelper.GetItemByPrefab(chestInv, _neededPrefab);
                    if (item != null)
                    {
                        int takeAmount = Mathf.Min(item.m_stack, Mathf.Max(_neededAmount, 1));
                        if (workerInv.CanAddItem(item, takeAmount))
                        {
                            chestInv.RemoveItem(item, takeAmount);
                            workerInv.AddItem(item.m_dropPrefab, takeAmount);
                            brain.Say($"Got {takeAmount}x {_neededPrefab}.");
                            brain.LogAction($"Withdrew {takeAmount}x '{_neededPrefab}' from chest.");
                        }
                    }
                }

                _supplyChest = null;
                _state = State.MovingToStructure;
            }
            else
            {
                brain.MoveTo(_supplyChest.transform.position, 2.2f, dist > 14f);
            }
        }

        private void HandleMovingToStructure(WorkerBrain brain)
        {
            Vector3 targetPos = _targetFireplace != null ? _targetFireplace.transform.position :
                               (_targetSmelter != null ? _targetSmelter.transform.position : Vector3.zero);

            if (targetPos == Vector3.zero)
            {
                _state = State.Scanning;
                return;
            }

            float dist = Vector3.Distance(brain.transform.position, targetPos);
            if (dist <= 2.8f)
            {
                brain.StopMoving();
                brain.FaceTarget(targetPos);
                _state = State.Refueling;
            }
            else
            {
                brain.MoveTo(targetPos, 2.2f, dist > 14f);
            }
        }

        private void HandleRefueling(WorkerBrain brain)
        {
            var workerInv = brain.m_humanoid?.GetInventory();

            if (_targetFireplace != null)
            {
                if (workerInv != null && InventoryHelper.HasItemPrefab(workerInv, _neededPrefab))
                {
                    var item = InventoryHelper.GetItemByPrefab(workerInv, _neededPrefab);
                    if (item != null)
                    {
                        workerInv.RemoveItem(item, 1);
                        _targetFireplace.GetComponent<ZNetView>()?.InvokeRPC("AddFuel");
                        brain.Say("Stoked the fire.");
                        brain.LogAction($"Added 1x '{_neededPrefab}' to fireplace at {_targetFireplace.transform.position:F1}.");
                        brain.AddTaskXp(5, TaskDignity.Menial);
                        brain.RecordTraceStep("MAINTENANCE", $"Refueled fireplace with '{_neededPrefab}'.");
                        VerificationTelemetry.RecordPass("tender_refuel_fires", $"Base Tender '{brain.GetHoverName()}' refueled fireplace/torch with '{_neededPrefab}'.");
                    }
                }
                _targetFireplace = null;
            }
            else if (_targetSmelter != null)
            {
                if (workerInv != null && InventoryHelper.HasItemPrefab(workerInv, _neededPrefab))
                {
                    var item = InventoryHelper.GetItemByPrefab(workerInv, _neededPrefab);
                    if (item != null)
                    {
                        if (_action == StructureAction.FuelSmelter)
                        {
                            workerInv.RemoveItem(item, 1);
                            _targetSmelter.GetComponent<ZNetView>()?.InvokeRPC("AddFuel");
                            brain.Say("Added fuel to smelter.");
                            brain.LogAction($"Added 1x '{_neededPrefab}' fuel to smelter at {_targetSmelter.transform.position:F1}.");
                            brain.AddTaskXp(15, TaskDignity.Specialist);
                            brain.RaiseActivitySkill(WorkerActivitySkill.Crafting, 2.0f);
                            brain.RecordTraceStep("MAINTENANCE", "Added fuel to smelter.");
                            VerificationTelemetry.RecordPass("processing_kiln_smelter", $"Base Tender '{brain.GetHoverName()}' fueled smelter with '{_neededPrefab}'.");
                        }
                        else if (_action == StructureAction.LoadKilnWood)
                        {
                            workerInv.RemoveItem(item, 1);
                            _targetSmelter.GetComponent<ZNetView>()?.InvokeRPC("AddOre", _neededPrefab, true);
                            brain.Say("Loaded wood into kiln.");
                            brain.LogAction($"Loaded 1x '{_neededPrefab}' into charcoal kiln at {_targetSmelter.transform.position:F1}.");
                            brain.AddTaskXp(15, TaskDignity.Specialist);
                            brain.RaiseActivitySkill(WorkerActivitySkill.Crafting, 2.0f);
                            brain.RecordTraceStep("MAINTENANCE", "Loaded wood into charcoal kiln.");
                            VerificationTelemetry.RecordPass("processing_kiln_smelter", $"Base Tender '{brain.GetHoverName()}' loaded wood into charcoal kiln.");
                            VerificationTelemetry.RecordPass("tender_smelter_add_ore", $"Dispatched AddOre RPC to Charcoal Kiln with '{_neededPrefab}' (logIt=true).");
                        }
                        else if (_action == StructureAction.LoadSmelterOre)
                        {
                            workerInv.RemoveItem(item, 1);
                            _targetSmelter.GetComponent<ZNetView>()?.InvokeRPC("AddOre", _neededPrefab, true);
                            brain.Say("Loaded ore into smelter.");
                            brain.LogAction($"Loaded 1x '{_neededPrefab}' into smelter at {_targetSmelter.transform.position:F1}.");
                            brain.AddTaskXp(15, TaskDignity.Specialist);
                            brain.RaiseActivitySkill(WorkerActivitySkill.Crafting, 2.0f);
                            brain.RecordTraceStep("MAINTENANCE", $"Loaded '{_neededPrefab}' into smelter.");
                            VerificationTelemetry.RecordPass("processing_kiln_smelter", $"Base Tender '{brain.GetHoverName()}' loaded '{_neededPrefab}' into smelter.");
                            VerificationTelemetry.RecordPass("tender_smelter_add_ore", $"Dispatched AddOre RPC to Smelter with '{_neededPrefab}' (logIt=true).");
                            if (_targetSmelter.gameObject.name.IndexOf("windmill", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                _neededPrefab.IndexOf("Barley", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                VerificationTelemetry.RecordPass("tender_windmill_spinning", $"Base Tender '{brain.GetHoverName()}' fed barley into Windmill.");
                            }
                        }
                    }
                }
                _targetSmelter = null;
            }

            _state = State.Scanning;
        }

        private void HandleMovingToRepair(WorkerBrain brain)
        {
            var nview = _targetWearNTear != null ? _targetWearNTear.GetComponent<ZNetView>() : null;
            if (_targetWearNTear == null || nview == null || !nview.IsValid() || _targetWearNTear.GetHealthPercentage() >= 0.99f)
            {
                _targetWearNTear = null;
                _state = State.Scanning;
                return;
            }

            float dist = Vector3.Distance(brain.transform.position, _targetWearNTear.transform.position);
            if (dist <= 2.8f)
            {
                brain.StopMoving();
                brain.FaceTarget(_targetWearNTear.transform.position);
                brain.AutoEquipTool("Hammer");

                if (!brain.m_humanoid.InAttack())
                {
                    brain.m_humanoid.StartAttack(null, false);
                }

                _targetWearNTear.Repair();
                brain.Say("Repaired structure.");
                brain.LogAction($"Repaired structure '{_targetWearNTear.gameObject.name}' at {_targetWearNTear.transform.position:F1}.");
                brain.AddTaskXp(20, TaskDignity.HighHonor);
                brain.RecordTraceStep("MAINTENANCE", $"Repaired damaged structure '{_targetWearNTear.gameObject.name}'.");
                VerificationTelemetry.RecordPass("base_repair_structures", $"Base Tender '{brain.GetHoverName()}' repaired damaged structure '{_targetWearNTear.gameObject.name}'.");

                _targetWearNTear = null;
                _state = State.Scanning;
            }
            else
            {
                brain.MoveTo(_targetWearNTear.transform.position, 2.2f, dist > 14f);
            }
        }
    }
}