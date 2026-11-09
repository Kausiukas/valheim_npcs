using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using ValheimWorkers.Core;

namespace ValheimWorkers.AI
{
    public class FarmerTask : IWorkerTask
    {
        public WorkerRole Role => WorkerRole.Farmer;

        private enum State
        {
            EvaluatingEquipment,
            ScanningCrops,
            MovingToCrop,
            Harvesting,
            Replanting,
            ProactivePlanting,
            FeedingTamedAnimals,
            SweepingProduce,
            ReturningToCamp,
            DepositingToChests
        }

        private State _state = State.EvaluatingEquipment;
        private string _statusDesc = "Farmer: Initializing...";
        private float _scanTimer = 0f;
        private float _actionCooldown = 0f;
        private Vector3 _lastHarvestPos = Vector3.zero;
        private string _lastHarvestedCropType = "Carrot";

        // Targets
        private Pickable _targetCrop = null;
        private Plant _targetPlant = null;
        private ItemDrop _targetProduceDrop = null;
        private Vector3 _proactivePlantPos = Vector3.zero;
        private bool _proactiveNeedsCultivate = false;
        private string _proactiveSeedPrefab = "";
        private string _proactiveSaplingPrefab = "";
        private Character _targetTamedAnimal = null;

        private static readonly string[] s_cropKeywords = new string[]
        {
            "Carrot", "Turnip", "Onion", "Barley", "Flax"
        };

        private static readonly string[] s_seedKeywords = new string[]
        {
            "CarrotSeeds", "TurnipSeeds", "OnionSeeds", "Barley", "Flax"
        };

        public bool CanExecute(WorkerBrain brain) => brain != null && brain.m_humanoid != null;
        public bool IsPerformingAction => _state == State.Harvesting || _state == State.Replanting || _state == State.ProactivePlanting || _state == State.SweepingProduce;

        public void StartTask(WorkerBrain brain)
        {
            _state = State.EvaluatingEquipment;
            ResetTargets();
            _scanTimer = 0.5f;
            _actionCooldown = 0f;
            _statusDesc = "Farmer: Inspecting fields...";
            brain.LogAction($"Started farming orders around {brain.GetWorkPostPosition():F1} (Radius: {brain.WorkRadius:F0}m)");
        }

        public void StopTask(WorkerBrain brain)
        {
            ResetTargets();
            brain.StopMoving();
        }

        public string GetStatusDescription() => _statusDesc;

        private void ResetTargets()
        {
            _targetCrop = null;
            _targetPlant = null;
            _targetProduceDrop = null;
            _proactivePlantPos = Vector3.zero;
            _targetTamedAnimal = null;
        }

        public void UpdateTask(WorkerBrain brain, float dt)
        {
            _scanTimer += dt;
            if (_actionCooldown > 0f) _actionCooldown -= dt;

            // Urgent inventory threshold: return to camp if inventory is full
            if (brain.IsInventoryFull() && _state != State.ReturningToCamp && _state != State.DepositingToChests)
            {
                brain.LogAction("Carried inventory is full of produce. Returning to store in chests.");
                brain.RecordTraceStep("INVENTORY", "Produce inventory full. Returning to camp to deposit in chests.");
                _state = State.ReturningToCamp;
            }

            switch (_state)
            {
                case State.EvaluatingEquipment:
                    EvaluateEquipment(brain);
                    break;

                case State.ScanningCrops:
                    ScanForCrops(brain);
                    break;

                case State.MovingToCrop:
                    HandleMovingToCrop(brain);
                    break;

                case State.Harvesting:
                    HandleHarvesting(brain);
                    break;

                case State.Replanting:
                    HandleReplanting(brain);
                    break;

                case State.ProactivePlanting:
                    HandleProactivePlanting(brain);
                    break;

                case State.FeedingTamedAnimals:
                    HandleFeedingTamedAnimals(brain);
                    break;

                case State.SweepingProduce:
                    HandleSweepingProduce(brain);
                    break;

                case State.ReturningToCamp:
                    HandleReturningToCamp(brain);
                    break;

                case State.DepositingToChests:
                    HandleDepositingToChests(brain);
                    break;
            }
        }

        private void EvaluateEquipment(WorkerBrain brain)
        {
            brain.AutoEquipTool("Cultivator");
            _state = State.ScanningCrops;
        }

        private void ScanForCrops(WorkerBrain brain)
        {
            if (_scanTimer < 0.6f) return;
            _scanTimer = 0f;

            Vector3 workCenter = brain.GetWorkPostPosition();
            float radius = brain.WorkRadius;
            Collider[] hits = Physics.OverlapSphere(workCenter, radius);

            // 1. Check for nearby dropped produce / seeds
            ItemDrop closestDrop = null;
            float bestDropDist = float.MaxValue;
            for (int i = 0; i < hits.Length; i++)
            {
                var drop = hits[i].GetComponentInParent<ItemDrop>();
                if (drop == null || !drop.CanPickup(false)) continue;

                string pName = drop.m_itemData?.m_dropPrefab != null ? drop.m_itemData.m_dropPrefab.name : (drop.m_itemData?.m_shared?.m_name ?? "");
                if (IsCropOrSeed(pName))
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
                _targetProduceDrop = closestDrop;
                _state = State.MovingToCrop;
                string dropName = _targetProduceDrop.m_itemData?.m_shared?.m_name ?? "crop";
                _statusDesc = $"Collecting dropped {dropName}";
                brain.LogAction($"Found dropped produce '{dropName}' x{_targetProduceDrop.m_itemData?.m_stack}. Moving to collect.");
                brain.RecordTraceStep("SCAN", $"Targeted produce drop '{dropName}' (dist: {bestDropDist:F1}m)");
                return;
            }

            // 2. Check for mature Pickable crops
            Pickable closestCrop = null;
            float bestCropDist = float.MaxValue;
            for (int i = 0; i < hits.Length; i++)
            {
                var pick = hits[i].GetComponentInParent<Pickable>();
                if (pick == null || !pick.CanBePicked()) continue;

                string pickName = pick.m_itemPrefab != null ? pick.m_itemPrefab.name : pick.gameObject.name;
                if (IsCrop(pickName))
                {
                    float d = Vector3.Distance(brain.transform.position, pick.transform.position);
                    if (d < bestCropDist)
                    {
                        bestCropDist = d;
                        closestCrop = pick;
                    }
                }
            }

            if (closestCrop != null)
            {
                _targetCrop = closestCrop;
                _state = State.MovingToCrop;
                string cName = _targetCrop.m_itemPrefab != null ? _targetCrop.m_itemPrefab.name : "crop";
                _statusDesc = $"Harvesting mature {cName}";
                brain.LogAction($"Found mature crop '{cName}' at {_targetCrop.transform.position:F1}. Moving to harvest.");
                brain.RecordTraceStep("SCAN", $"Targeted mature crop '{cName}' (dist: {bestCropDist:F1}m)");
                return;
            }

            // 3. [F12 & F13] Check if Farmer has seeds to proactively plant on cultivated or new patches
            var inv = brain.m_humanoid?.GetInventory();
            TryWithdrawSeedsFromCampChests(brain);

            if (HasAnySeedsInInventory(inv, out string seedPrefab, out string saplingPrefab))
            {
                if (FindNextPlantingSpot(brain, out Vector3 plantPos, out bool needsCultivate))
                {
                    _proactivePlantPos = plantPos;
                    _proactiveNeedsCultivate = needsCultivate;
                    _proactiveSeedPrefab = seedPrefab;
                    _proactiveSaplingPrefab = saplingPrefab;
                    _state = State.ProactivePlanting;
                    _statusDesc = needsCultivate ? "Cultivating new garden patch..." : "Planting seed on cultivated land...";
                    brain.LogAction($"Proactive planting: Targeting spot at {plantPos:F1} (Cultivate needed: {needsCultivate}) with '{seedPrefab}'.");
                    return;
                }
            }

            // 4. [F16] Check for hungry tamed animals to feed
            Character hungryAnimal = FindHungryTamedAnimal(brain);
            if (hungryAnimal != null && HasAnimalFeed(inv, hungryAnimal, out string feedPrefab))
            {
                _targetTamedAnimal = hungryAnimal;
                _state = State.FeedingTamedAnimals;
                _statusDesc = $"Feeding tamed {_targetTamedAnimal.GetHoverName()}";
                return;
            }

            // 5. If carrying harvested crops, return to camp chests
            if (brain.HasAnyGatheredResources())
            {
                _statusDesc = "Field clear, returning to store harvest";
                brain.LogAction("All crops harvested. Returning to camp to deposit produce in chests.");
                _state = State.ReturningToCamp;
                return;
            }

            // Stand by at work post
            float distToPost = Vector3.Distance(brain.transform.position, workCenter);
            if (distToPost > 3.5f)
            {
                _statusDesc = $"Returning to garden post ({distToPost:F1}m)";
                brain.MoveTo(workCenter, 2f, distToPost > 14f);
            }
            else
            {
                brain.StopMoving();
                _statusDesc = "Standing by at garden (Crops growing)";
            }
        }

        private static bool IsCrop(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            for (int i = 0; i < s_cropKeywords.Length; i++)
            {
                if (name.IndexOf(s_cropKeywords[i], StringComparison.OrdinalIgnoreCase) >= 0) return true;
            }
            return false;
        }

        private static bool IsCropOrSeed(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            for (int i = 0; i < s_cropKeywords.Length; i++)
            {
                if (name.IndexOf(s_cropKeywords[i], StringComparison.OrdinalIgnoreCase) >= 0) return true;
            }
            for (int i = 0; i < s_seedKeywords.Length; i++)
            {
                if (name.IndexOf(s_seedKeywords[i], StringComparison.OrdinalIgnoreCase) >= 0) return true;
            }
            return false;
        }

        private static bool HasAnySeedsInInventory(Inventory inv, out string seedPrefab, out string saplingPrefab)
        {
            seedPrefab = "";
            saplingPrefab = "";
            if (inv == null) return false;

            for (int i = 0; i < s_seedKeywords.Length; i++)
            {
                var item = InventoryHelper.GetItemByPrefab(inv, s_seedKeywords[i]);
                if (item != null && item.m_stack > 0)
                {
                    seedPrefab = s_seedKeywords[i];
                    saplingPrefab = seedPrefab switch
                    {
                        "TurnipSeeds" => "sapling_turnip",
                        "OnionSeeds" => "sapling_onion",
                        "Barley" => "sapling_barley",
                        "Flax" => "sapling_flax",
                        _ => "sapling_carrot"
                    };
                    return true;
                }
            }
            return false;
        }

        private static void TryWithdrawSeedsFromCampChests(WorkerBrain brain)
        {
            var inv = brain.m_humanoid?.GetInventory();
            if (inv == null) return;

            int currentSeeds = 0;
            for (int i = 0; i < s_seedKeywords.Length; i++)
            {
                var it = InventoryHelper.GetItemByPrefab(inv, s_seedKeywords[i]);
                if (it != null) currentSeeds += it.m_stack;
            }

            if (currentSeeds >= 10) return;

            Vector3 camp = brain.HasHome ? brain.HomePosition : brain.transform.position;
            Collider[] hits = Physics.OverlapSphere(camp, 25f);

            for (int i = 0; i < hits.Length; i++)
            {
                var chest = hits[i].GetComponentInParent<Container>();
                if (chest == null || chest.IsInUse() || chest.GetComponent<WorkerBrain>() != null) continue;
                var cInv = chest.GetInventory();
                if (cInv == null) continue;

                for (int s = 0; s < s_seedKeywords.Length; s++)
                {
                    var seedInChest = InventoryHelper.GetItemByPrefab(cInv, s_seedKeywords[s]);
                    if (seedInChest != null && seedInChest.m_stack > 0)
                    {
                        int take = Mathf.Min(seedInChest.m_stack, 30);
                        if (inv.AddItem(seedInChest.m_dropPrefab, take))
                        {
                            cInv.RemoveItem(seedInChest, take);
                            brain.LogAction($"Withdrew {take}x {s_seedKeywords[s]} from chest for planting.");
                            return;
                        }
                    }
                }
            }
        }

        private bool FindNextPlantingSpot(WorkerBrain brain, out Vector3 plantPos, out bool needsCultivate)
        {
            plantPos = Vector3.zero;
            needsCultivate = false;
            Vector3 center = brain.GetWorkPostPosition();
            float maxR = brain.WorkRadius;

            // Pass 1 [F13]: Look for available cultivated land that doesn't have a plant within 0.55m
            for (float r = 1.0f; r <= maxR; r += 0.85f)
            {
                int steps = Mathf.Max(6, Mathf.RoundToInt(2f * Mathf.PI * r / 0.85f));
                for (int i = 0; i < steps; i++)
                {
                    float angle = (i * 360f / steps) * Mathf.Deg2Rad;
                    Vector3 testPos = center + new Vector3(Mathf.Cos(angle) * r, 0f, Mathf.Sin(angle) * r);

                    if (Physics.Raycast(new Vector3(testPos.x, testPos.y + 2f, testPos.z), Vector3.down, out RaycastHit hit, 5f, LayerMask.GetMask("Default", "static_solid", "terrain")))
                    {
                        if (hit.normal.y < 0.82f) continue;
                        if (ZoneSystem.instance != null && hit.point.y < ZoneSystem.instance.m_waterLevel + 0.3f) continue;

                        var hmap = Heightmap.FindHeightmap(hit.point);
                        if (hmap != null && hmap.IsCultivated(hit.point))
                        {
                            if (!HasPlantNearby(hit.point, 0.55f))
                            {
                                plantPos = hit.point;
                                needsCultivate = false;
                                return true;
                            }
                        }
                    }
                }
            }

            // Pass 2 [F13]: If all cultivated land is full, find an adjacent uncultivated patch to expand!
            if (brain.HasToolType(ItemDrop.ItemData.ItemType.Tool, "Cultivator"))
            {
                for (float r = 1.2f; r <= maxR; r += 1.0f)
                {
                    int steps = Mathf.Max(6, Mathf.RoundToInt(2f * Mathf.PI * r / 1.0f));
                    for (int i = 0; i < steps; i++)
                    {
                        float angle = (i * 360f / steps) * Mathf.Deg2Rad;
                        Vector3 testPos = center + new Vector3(Mathf.Cos(angle) * r, 0f, Mathf.Sin(angle) * r);

                        if (Physics.Raycast(new Vector3(testPos.x, testPos.y + 2f, testPos.z), Vector3.down, out RaycastHit hit, 5f, LayerMask.GetMask("Default", "static_solid", "terrain")))
                        {
                            if (hit.normal.y < 0.85f) continue;
                            if (ZoneSystem.instance != null && hit.point.y < ZoneSystem.instance.m_waterLevel + 0.4f) continue;

                            // Must be clear of building pieces, rocks, and existing plants within 1.0m
                            if (!HasObstacleNearby(hit.point, 1.0f))
                            {
                                plantPos = hit.point;
                                needsCultivate = true;
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        private static bool HasPlantNearby(Vector3 pos, float radius)
        {
            var colliders = Physics.OverlapSphere(pos, radius);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].GetComponentInParent<Plant>() != null || colliders[i].GetComponentInParent<Pickable>() != null)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool HasObstacleNearby(Vector3 pos, float radius)
        {
            var colliders = Physics.OverlapSphere(pos, radius);
            for (int i = 0; i < colliders.Length; i++)
            {
                var col = colliders[i];
                if (col.isTrigger) continue;
                if (col.GetComponentInParent<Plant>() != null || 
                    col.GetComponentInParent<Pickable>() != null ||
                    col.GetComponentInParent<Piece>() != null ||
                    col.GetComponentInParent<Destructible>() != null)
                {
                    return true;
                }
            }
            return false;
        }

        private void HandleMovingToCrop(WorkerBrain brain)
        {
            Vector3 targetPos = Vector3.zero;
            float reach = 2.2f;

            if (_targetProduceDrop != null)
            {
                if (!_targetProduceDrop.CanPickup(false))
                {
                    _state = State.ScanningCrops;
                    return;
                }
                targetPos = _targetProduceDrop.transform.position;
                reach = 2.0f;
            }
            else if (_targetCrop != null)
            {
                if (!_targetCrop.CanBePicked())
                {
                    _state = State.ScanningCrops;
                    return;
                }
                targetPos = _targetCrop.transform.position;
                reach = 2.2f;
            }
            else
            {
                _state = State.ScanningCrops;
                return;
            }

            float dist = Vector3.Distance(brain.transform.position, targetPos);
            if (dist <= reach)
            {
                brain.StopMoving();
                brain.FaceTarget(targetPos);

                if (_targetProduceDrop != null)
                {
                    string itemName = _targetProduceDrop.m_itemData?.m_shared?.m_name ?? "produce";
                    int stack = _targetProduceDrop.m_itemData?.m_stack ?? 1;
                    Vector3 pickupPos = _targetProduceDrop.transform.position;
                    if (brain.m_humanoid.Pickup(_targetProduceDrop.gameObject, false, false))
                    {
                        brain.LogAction($"Picked up produce '{itemName}' x{stack}.");
                    }
                    _targetProduceDrop = null;
                    _lastHarvestPos = pickupPos;
                    _state = State.SweepingProduce;
                }
                else
                {
                    _state = State.Harvesting;
                }
            }
            else
            {
                brain.MoveTo(targetPos, reach * 0.85f, dist > 14f);
            }
        }

        private static string ExtractCropType(string name)
        {
            if (string.IsNullOrEmpty(name)) return "Carrot";
            if (name.IndexOf("Turnip", StringComparison.OrdinalIgnoreCase) >= 0) return "Turnip";
            if (name.IndexOf("Onion", StringComparison.OrdinalIgnoreCase) >= 0) return "Onion";
            if (name.IndexOf("Barley", StringComparison.OrdinalIgnoreCase) >= 0) return "Barley";
            if (name.IndexOf("Flax", StringComparison.OrdinalIgnoreCase) >= 0) return "Flax";
            return "Carrot";
        }

        private void HandleHarvesting(WorkerBrain brain)
        {
            if (_actionCooldown > 0f) return;

            if (_targetCrop != null)
            {
                if (_targetCrop.CanBePicked())
                {
                    string cropName = _targetCrop.m_itemPrefab != null ? _targetCrop.m_itemPrefab.name : _targetCrop.gameObject.name;
                    _lastHarvestedCropType = ExtractCropType(cropName);
                    _lastHarvestPos = _targetCrop.transform.position;
                    _targetCrop.Interact(brain.m_humanoid, false, false);
                    brain.Say("Harvested ripe crop.");
                    brain.LogAction($"Harvested mature crop '{cropName}' at {_lastHarvestPos:F1}.");
                    brain.AddTaskXp(5, TaskDignity.Skilled);
                    brain.RaiseActivitySkill(WorkerActivitySkill.Farming, 1.5f);
                    VerificationTelemetry.RecordPass("farming_crop_harvest", $"Farmer '{brain.GetHoverName()}' harvested mature crop '{cropName}'.");
                    _actionCooldown = 0.8f;
                    _targetCrop = null;
                    _state = State.Replanting;
                    return;
                }
                _targetCrop = null;
            }

            _state = State.ScanningCrops;
        }

        private void HandleReplanting(WorkerBrain brain)
        {
            if (_actionCooldown > 0f) return;
            if (_lastHarvestPos == Vector3.zero)
            {
                _state = State.SweepingProduce;
                return;
            }

            var inv = brain.m_humanoid?.GetInventory();
            if (inv == null)
            {
                _state = State.SweepingProduce;
                return;
            }

            string seedPrefab = _lastHarvestedCropType switch
            {
                "Turnip" => "TurnipSeeds",
                "Onion" => "OnionSeeds",
                "Barley" => "Barley",
                "Flax" => "Flax",
                _ => "CarrotSeeds"
            };

            string saplingPrefab = _lastHarvestedCropType switch
            {
                "Turnip" => "sapling_turnip",
                "Onion" => "sapling_onion",
                "Barley" => "sapling_barley",
                "Flax" => "sapling_flax",
                _ => "sapling_carrot"
            };

            ItemDrop.ItemData seedItem = InventoryHelper.GetItemByPrefab(inv, seedPrefab);
            if (seedItem == null || seedItem.m_stack <= 0)
            {
                if (!HasAnySeedsInInventory(inv, out seedPrefab, out saplingPrefab))
                {
                    _state = State.SweepingProduce;
                    return;
                }
                seedItem = InventoryHelper.GetItemByPrefab(inv, seedPrefab);
            }

            float dist = Vector3.Distance(brain.transform.position, _lastHarvestPos);
            if (dist > 2.2f)
            {
                brain.MoveTo(_lastHarvestPos, 1.8f, dist > 12f);
                _statusDesc = $"Moving to replant {_lastHarvestedCropType}";
                return;
            }

            brain.StopMoving();
            brain.FaceTarget(_lastHarvestPos);

            GameObject plantPrefabObj = ZNetScene.instance != null ? ZNetScene.instance.GetPrefab(saplingPrefab) : null;
            if (plantPrefabObj != null)
            {
                Vector3 spawnPos = _lastHarvestPos;
                if (Physics.Raycast(new Vector3(_lastHarvestPos.x, _lastHarvestPos.y + 1.5f, _lastHarvestPos.z), Vector3.down, out RaycastHit hit, 4f))
                {
                    spawnPos = hit.point;
                }

                var hmap = Heightmap.FindHeightmap(spawnPos);
                bool isCultivated = hmap != null && hmap.IsCultivated(spawnPos);
                if (!isCultivated)
                {
                    AutoCultivateSoil(spawnPos);
                    isCultivated = true;
                    brain.AutoEquipTool("Cultivator");
                    brain.m_humanoid.GetComponent<ZSyncAnimation>()?.SetTrigger("swing_axe0");
                    brain.LogAction($"Cultivated soil at {spawnPos:F1} before replanting.");
                    VerificationTelemetry.RecordPass("farmer_auto_cultivate_patch", $"Farmer '{brain.GetHoverName()}' auto-cultivated soil at {spawnPos:F1} before replanting.");
                }
                else
                {
                    VerificationTelemetry.RecordPass("farmer_cultivated_ground", $"Farmer '{brain.GetHoverName()}' verified cultivated plot at {spawnPos:F1}.");
                }

                if (isCultivated && !HasPlantNearby(spawnPos, 0.4f))
                {
                    UnityEngine.Object.Instantiate(plantPrefabObj, spawnPos, Quaternion.identity);
                    inv.RemoveItem(seedItem, 1);

                    brain.AutoEquipTool("Cultivator");
                    brain.Say("Replanted field.");
                    brain.LogAction($"Replanted {_lastHarvestedCropType} at {spawnPos:F1}.");
                    VerificationTelemetry.RecordPass("farming_crop_replant", $"Farmer '{brain.GetHoverName()}' replanted {_lastHarvestedCropType} in garden plot.");
                    _actionCooldown = 0.8f;
                }
            }

            _state = State.SweepingProduce;
        }

        private void HandleProactivePlanting(WorkerBrain brain)
        {
            if (_actionCooldown > 0f) return;
            if (_proactivePlantPos == Vector3.zero)
            {
                _state = State.ScanningCrops;
                return;
            }

            float dist = Vector3.Distance(brain.transform.position, _proactivePlantPos);
            if (dist > 2.2f)
            {
                _statusDesc = $"Moving to plant {_proactiveSeedPrefab} ({dist:F1}m)";
                brain.MoveTo(_proactivePlantPos, 1.8f, dist > 12f);
                return;
            }

            brain.StopMoving();
            brain.FaceTarget(_proactivePlantPos);

            var inv = brain.m_humanoid?.GetInventory();
            var seedItem = InventoryHelper.GetItemByPrefab(inv, _proactiveSeedPrefab);
            if (seedItem == null || seedItem.m_stack <= 0)
            {
                _state = State.ScanningCrops;
                return;
            }

            GameObject saplingObj = ZNetScene.instance != null ? ZNetScene.instance.GetPrefab(_proactiveSaplingPrefab) : null;
            if (saplingObj != null)
            {
                if (_proactiveNeedsCultivate)
                {
                    AutoCultivateSoil(_proactivePlantPos);
                    brain.AutoEquipTool("Cultivator");
                    brain.m_humanoid.GetComponent<ZSyncAnimation>()?.SetTrigger("swing_axe0");
                    brain.LogAction($"[F13] Cultivated new garden patch at {_proactivePlantPos:F1}.");
                    VerificationTelemetry.RecordPass("farmer_auto_cultivate_patch", $"Farmer '{brain.GetHoverName()}' auto-cultivated new garden patch at {_proactivePlantPos:F1}.");
                }

                if (!HasPlantNearby(_proactivePlantPos, 0.5f))
                {
                    UnityEngine.Object.Instantiate(saplingObj, _proactivePlantPos, Quaternion.identity);
                    inv.RemoveItem(seedItem, 1);

                    brain.AutoEquipTool("Cultivator");
                    brain.Say($"Planted {_proactiveSeedPrefab}.");
                    brain.LogAction($"[F12] Proactively planted '{_proactiveSeedPrefab}' at {_proactivePlantPos:F1}.");
                    VerificationTelemetry.RecordPass("farmer_proactive_planting", $"Farmer '{brain.GetHoverName()}' proactively planted '{_proactiveSeedPrefab}'.");
                    _actionCooldown = 0.7f;
                }
            }

            _proactivePlantPos = Vector3.zero;
            _state = State.ScanningCrops;
        }

        private Character FindHungryTamedAnimal(WorkerBrain brain)
        {
            Vector3 center = brain.GetWorkPostPosition();
            Collider[] hits = Physics.OverlapSphere(center, brain.WorkRadius);

            for (int i = 0; i < hits.Length; i++)
            {
                var ch = hits[i].GetComponentInParent<Character>();
                if (ch == null || !ch.IsTamed()) continue;

                var monsterAI = ch.GetComponent<MonsterAI>();
                if (monsterAI != null)
                {
                    bool isHungry = Traverse.Create(monsterAI).Method("IsHungry").GetValue<bool>();
                    if (isHungry) return ch;
                }
            }
            return null;
        }

        private static bool HasAnimalFeed(Inventory inv, Character animal, out string feedPrefab)
        {
            feedPrefab = "";
            if (inv == null || animal == null) return false;

            string aName = animal.gameObject.name.ToLowerInvariant();
            string[] candidateFeeds = aName.Contains("boar") ? new[] { "Carrot", "Raspberry", "Mushroom", "Blueberries" } :
                                      aName.Contains("lox") ? new[] { "Cloudberry", "Barley", "Flax" } :
                                      aName.Contains("hen") || aName.Contains("chicken") ? new[] { "Barley", "Dandelion", "CarrotSeeds" } :
                                      aName.Contains("wolf") ? new[] { "RawMeat", "DeerMeat", "NeckTail" } :
                                      new[] { "Carrot", "Raspberry" };

            foreach (var f in candidateFeeds)
            {
                var item = InventoryHelper.GetItemByPrefab(inv, f);
                if (item != null && item.m_stack > 0)
                {
                    feedPrefab = f;
                    return true;
                }
            }
            return false;
        }

        private void HandleFeedingTamedAnimals(WorkerBrain brain)
        {
            if (_targetTamedAnimal == null || _targetTamedAnimal.IsDead())
            {
                _state = State.ScanningCrops;
                return;
            }

            float dist = Vector3.Distance(brain.transform.position, _targetTamedAnimal.transform.position);
            if (dist > 3.0f)
            {
                _statusDesc = $"Moving to feed {_targetTamedAnimal.GetHoverName()} ({dist:F1}m)";
                brain.MoveTo(_targetTamedAnimal.transform.position, 2.5f, dist > 14f);
                return;
            }

            brain.StopMoving();
            brain.FaceTarget(_targetTamedAnimal.transform.position);

            var inv = brain.m_humanoid?.GetInventory();
            if (HasAnimalFeed(inv, _targetTamedAnimal, out string feedPrefab))
            {
                var feedItem = InventoryHelper.GetItemByPrefab(inv, feedPrefab);
                if (feedItem != null && feedItem.m_dropPrefab != null)
                {
                    inv.RemoveItem(feedItem, 1);
                    Vector3 dropPos = brain.transform.position + brain.transform.forward * 1.2f + Vector3.up * 0.5f;
                    GameObject dropped = UnityEngine.Object.Instantiate(feedItem.m_dropPrefab, dropPos, Quaternion.identity);
                    var itemDrop = dropped.GetComponent<ItemDrop>();
                    if (itemDrop != null)
                    {
                        itemDrop.m_itemData.m_stack = 1;
                    }

                    brain.Say($"Fed {_targetTamedAnimal.GetHoverName()}.");
                    brain.LogAction($"[F16] Dropped 1x {feedPrefab} to feed hungry tamed {_targetTamedAnimal.GetHoverName()}.");
                    VerificationTelemetry.RecordPass("farming_feed_tamed", $"Farmer '{brain.GetHoverName()}' fed tamed '{_targetTamedAnimal.GetHoverName()}'.");
                    _actionCooldown = 1.0f;
                }
            }

            _targetTamedAnimal = null;
            _state = State.ScanningCrops;
        }

        private void AutoCultivateSoil(Vector3 pos)
        {
            try
            {
                TerrainModifier.SetTriggerOnPlaced(true);
                GameObject modGo = new GameObject("Worker_CultivateMod");
                modGo.transform.position = pos;
                var tm = modGo.AddComponent<TerrainModifier>();
                tm.m_paintCleared = true;
                tm.m_paintType = TerrainModifier.PaintType.Cultivate;
                tm.m_paintRadius = 1.8f;
                tm.m_playerModifiction = true;

                List<Heightmap> hms = new List<Heightmap>();
                Heightmap.FindHeightmap(pos, 3.0f, hms);
                foreach (var hm in hms)
                {
                    hm.Poke(0, true);
                }
            }
            catch (Exception ex)
            {
                ValheimWorkersPlugin.Log.LogWarning($"AutoCultivateSoil error: {ex.Message}");
            }
        }

        private void HandleSweepingProduce(WorkerBrain brain)
        {
            Vector3 center = _lastHarvestPos != Vector3.zero ? _lastHarvestPos : brain.transform.position;
            Collider[] hits = Physics.OverlapSphere(center, 12f);

            ItemDrop closest = null;
            float bestDist = float.MaxValue;
            for (int i = 0; i < hits.Length; i++)
            {
                var drop = hits[i].GetComponentInParent<ItemDrop>();
                if (drop == null || !drop.CanPickup(false)) continue;

                string pName = drop.m_itemData?.m_dropPrefab != null ? drop.m_itemData.m_dropPrefab.name : (drop.m_itemData?.m_shared?.m_name ?? "");
                if (IsCropOrSeed(pName))
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
                _targetProduceDrop = closest;
                _state = State.MovingToCrop;
                return;
            }

            var inv = brain.m_humanoid?.GetInventory();
            if (inv != null && brain.HasAnyGatheredResources())
            {
                int carriedCount = 0;
                var all = inv.GetAllItems();
                for (int i = 0; i < all.Count; i++)
                {
                    if (!brain.IsProtectedWorkGear(all[i])) carriedCount += all[i].m_stack;
                }

                if (carriedCount >= 10)
                {
                    brain.LogAction("Finished harvesting area with full produce sacks. Returning to camp chests.");
                    _state = State.ReturningToCamp;
                    return;
                }
            }

            _state = State.ScanningCrops;
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
                _statusDesc = $"Returning to camp with harvest ({dist:F1}m)";
                brain.MoveTo(camp, 2.5f, dist > 14f);
            }
        }

        private void HandleDepositingToChests(WorkerBrain brain)
        {
            var inv = brain.m_humanoid?.GetInventory();
            if (inv == null || !brain.HasAnyGatheredResources())
            {
                _state = State.ScanningCrops;
                return;
            }

            Vector3 camp = brain.HasHome ? brain.HomePosition : brain.transform.position;
            Collider[] hits = Physics.OverlapSphere(camp, 22f);

            var carriedProduce = new List<ItemDrop.ItemData>();
            foreach (var item in inv.GetAllItems())
            {
                if (item == null || brain.IsProtectedWorkGear(item)) continue;
                carriedProduce.Add(item);
            }

            if (carriedProduce.Count == 0)
            {
                _state = State.ScanningCrops;
                return;
            }

            Container bestMatchingChest = null;
            float bestMatchingDist = float.MaxValue;
            Container bestEmptyChest = null;
            float bestEmptyDist = float.MaxValue;

            for (int i = 0; i < hits.Length; i++)
            {
                var chest = hits[i].GetComponentInParent<Container>();
                if (chest == null || chest.IsInUse() || chest.GetComponent<WorkerBrain>() != null) continue;

                var cInv = chest.GetInventory();
                if (cInv == null) continue;

                float d = Vector3.Distance(brain.transform.position, chest.transform.position);

                bool hasMatch = false;
                for (int j = 0; j < carriedProduce.Count; j++)
                {
                    if (InventoryHelper.HasMatchingItem(cInv, carriedProduce[j]) && cInv.CanAddItem(carriedProduce[j], carriedProduce[j].m_stack))
                    {
                        hasMatch = true;
                        break;
                    }
                }

                if (hasMatch && d < bestMatchingDist)
                {
                    bestMatchingDist = d;
                    bestMatchingChest = chest;
                }
                else if (cInv.HaveEmptySlot() && d < bestEmptyDist)
                {
                    bestEmptyDist = d;
                    bestEmptyChest = chest;
                }
            }

            Container targetChest = bestMatchingChest ?? bestEmptyChest;
            if (targetChest == null)
            {
                // [F05] If no chest has room, construct overflow resource piles!
                if (LogisticsHelper.TryConstructOverflowResourcePiles(brain, camp))
                {
                    _statusDesc = "Farmer: Built overflow resource pile";
                    _state = State.ScanningCrops;
                    return;
                }

                _statusDesc = "Farmer: No storage chest available";
                _state = State.ScanningCrops;
                return;
            }

            float dChest = Vector3.Distance(brain.transform.position, targetChest.transform.position);
            if (dChest <= 2.5f)
            {
                brain.StopMoving();
                brain.FaceTarget(targetChest.transform.position);

                var cInv = targetChest.GetInventory();
                int depositedCount = 0;

                for (int i = carriedProduce.Count - 1; i >= 0; i--)
                {
                    var item = carriedProduce[i];
                    if (cInv.CanAddItem(item, item.m_stack))
                    {
                        cInv.AddItem(item);
                        inv.RemoveItem(item);
                        depositedCount++;
                    }
                }

                if (depositedCount > 0)
                {
                    brain.Say("Stored harvest in chest.");
                    brain.LogAction($"Deposited {depositedCount} produce stack(s) into chest at {targetChest.transform.position:F1}.");
                    brain.AddTaskXp(depositedCount * 10, TaskDignity.Skilled);
                    brain.RaiseActivitySkill(WorkerActivitySkill.Farming, 1.0f * depositedCount);
                    VerificationTelemetry.RecordPass("farming_chest_deposit", $"Farmer '{brain.GetHoverName()}' stored {depositedCount} produce stack(s) in settlement chests.");
                }

                _state = State.ScanningCrops;
            }
            else
            {
                _statusDesc = $"Hauling harvest to chest ({dChest:F1}m)";
                brain.MoveTo(targetChest.transform.position, 2f, dChest > 14f);
            }
        }
    }
}
