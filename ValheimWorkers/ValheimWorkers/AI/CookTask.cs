using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using ValheimWorkers.Core;

namespace ValheimWorkers.AI
{
    public class CookTask : IWorkerTask
    {
        public WorkerRole Role => WorkerRole.Cook;

        private enum State
        {
            ScanningKitchen,
            FetchingRawIngredients,
            MovingToStation,
            InteractingStation,
            SweepingCookedFood,
            ReturningToChests,
            DepositingFood
        }

        private State _state = State.ScanningKitchen;
        private CookingStation _targetStation = null;
        private ItemDrop.ItemData _rawMeatToCook = null;
        private ItemDrop _targetCookedDrop = null;

        private float _scanTimer = 0f;
        private float _actionCooldown = 0f;
        private string _statusDesc = "Cook: Initializing...";

        private static readonly string[] s_rawMeatKeywords = new string[]
        {
            "RawMeat", "MeatRaw", "BoarMeat", "NeckTail", "DeerMeat", "FishRaw", "LoxMeat", "ChickenMeat", "HareMeat", "WolfMeat", "BearMeat"
        };

        private static readonly string[] s_cookedFoodKeywords = new string[]
        {
            "CookedMeat", "MeatCooked", "NeckTailGrilled", "CookedDeerMeat", "FishCooked", "CookedFish", "CookedLoxMeat", "CookedChickenMeat", "CookedHareMeat", "WolfMeatCooked", "CookedWolfMeat", "CookedBearMeat", "BoarMeatCooked", "CookedBoarMeat"
        };

        public bool CanExecute(WorkerBrain brain) => brain != null && brain.m_humanoid != null;
        public bool IsPerformingAction => _state == State.InteractingStation || _state == State.SweepingCookedFood;

        public void StartTask(WorkerBrain brain)
        {
            _state = State.ScanningKitchen;
            ResetTargets();
            _scanTimer = 0.5f;
            _actionCooldown = 0f;
            _statusDesc = "Cook: Inspecting kitchen...";
            brain.LogAction($"Started cook order at {brain.HomePosition:F1} (Radius: {brain.WorkRadius:F0}m)");
        }

        public void StopTask(WorkerBrain brain)
        {
            ResetTargets();
            brain.StopMoving();
        }

        public string GetStatusDescription() => _statusDesc;

        private void ResetTargets()
        {
            _targetStation = null;
            _rawMeatToCook = null;
            _targetCookedDrop = null;
        }

        public void UpdateTask(WorkerBrain brain, float dt)
        {
            _scanTimer += dt;
            if (_actionCooldown > 0f) _actionCooldown -= dt;

            switch (_state)
            {
                case State.ScanningKitchen:
                    ScanKitchen(brain);
                    break;

                case State.FetchingRawIngredients:
                    HandleFetchingRawIngredients(brain);
                    break;

                case State.MovingToStation:
                    HandleMovingToStation(brain);
                    break;

                case State.InteractingStation:
                    HandleInteractingStation(brain);
                    break;

                case State.SweepingCookedFood:
                    HandleSweepingCookedFood(brain);
                    break;

                case State.ReturningToChests:
                    HandleReturningToChests(brain);
                    break;

                case State.DepositingFood:
                    HandleDepositingFood(brain);
                    break;
            }
        }

        private static bool IsRawMeat(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            if (IsCookedFood(name)) return false;
            if (name.IndexOf("cooked", StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("grilled", StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("coal", StringComparison.OrdinalIgnoreCase) >= 0)
                return false;

            for (int i = 0; i < s_rawMeatKeywords.Length; i++)
            {
                if (name.IndexOf(s_rawMeatKeywords[i], StringComparison.OrdinalIgnoreCase) >= 0) return true;
            }
            return false;
        }

        private static bool IsCookedFood(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            for (int i = 0; i < s_cookedFoodKeywords.Length; i++)
            {
                if (name.IndexOf(s_cookedFoodKeywords[i], StringComparison.OrdinalIgnoreCase) >= 0) return true;
            }
            return false;
        }

        private void ScanKitchen(WorkerBrain brain)
        {
            if (_scanTimer < 0.6f) return;
            _scanTimer = 0f;

            Vector3 center = brain.HasHome ? brain.HomePosition : brain.transform.position;
            float radius = brain.WorkRadius;
            Collider[] hits = Physics.OverlapSphere(center, radius);

            // 1. Check for nearby dropped cooked food on ground
            ItemDrop closestFoodDrop = null;
            float bestDropDist = float.MaxValue;
            for (int i = 0; i < hits.Length; i++)
            {
                var drop = hits[i].GetComponentInParent<ItemDrop>();
                if (drop == null || !drop.CanPickup(false)) continue;

                string pName = drop.m_itemData?.m_dropPrefab != null ? drop.m_itemData.m_dropPrefab.name : (drop.m_itemData?.m_shared?.m_name ?? "");
                if (IsCookedFood(pName))
                {
                    float d = Vector3.Distance(brain.transform.position, drop.transform.position);
                    if (d < bestDropDist)
                    {
                        bestDropDist = d;
                        closestFoodDrop = drop;
                    }
                }
            }

            if (closestFoodDrop != null)
            {
                _targetCookedDrop = closestFoodDrop;
                _state = State.SweepingCookedFood;
                return;
            }

            // 2. Scan CookingStations & Fermenters [F23 & F25]
            for (int i = 0; i < hits.Length; i++)
            {
                var fermenter = hits[i].GetComponentInParent<Fermenter>();
                if (fermenter != null)
                {
                    string statusStr = HarmonyLib.Traverse.Create(fermenter).Method("GetStatus").GetValue()?.ToString() ?? "";
                    if (statusStr.Equals("Ready", StringComparison.OrdinalIgnoreCase))
                    {
                        float d = Vector3.Distance(brain.transform.position, fermenter.transform.position);
                        if (d <= 2.2f)
                        {
                            fermenter.Interact(brain.m_humanoid, false, false);
                            brain.Say("Tapped aged mead.");
                            brain.LogAction("[F25] Tapped finished mead potions from Fermenter.");
                            brain.AddTaskXp(15, TaskDignity.Skilled);
                            brain.RaiseActivitySkill(WorkerActivitySkill.Crafting, 2.0f);
                            VerificationTelemetry.RecordPass("cooking_fermenter_tap", $"Cook '{brain.GetHoverName()}' tapped finished mead potions from Fermenter.");
                            _state = State.SweepingCookedFood;
                            return;
                        }
                        else
                        {
                            _statusDesc = $"Moving to tap Fermenter ({d:F1}m)";
                            brain.MoveTo(fermenter.transform.position, 2f, d > 12f);
                            return;
                        }
                    }
                }
            }

            CookingStation bestStationWithDone = null;
            CookingStation bestStationWithFreeSlot = null;
            float bestDoneDist = float.MaxValue;
            float bestFreeDist = float.MaxValue;

            for (int i = 0; i < hits.Length; i++)
            {
                var station = hits[i].GetComponentInParent<CookingStation>();
                if (station == null) continue;

                float d = Vector3.Distance(brain.transform.position, station.transform.position);

                // Priority A: Station has done item ready to pull off
                bool haveDone = Traverse.Create(station).Method("HaveDoneItem").GetValue<bool>();
                if (haveDone && d < bestDoneDist)
                {
                    bestDoneDist = d;
                    bestStationWithDone = station;
                }

                // Priority B: Station has free slot and fire is lit
                bool isFull = Traverse.Create(station).Method("IsStationFull").GetValue<bool>();
                bool fireLit = Traverse.Create(station).Method("IsFireLit").GetValue<bool>();
                if (!isFull && fireLit && d < bestFreeDist)
                {
                    bestFreeDist = d;
                    bestStationWithFreeSlot = station;
                }
            }

            // If a station has done meat, immediately move to pull it off before it burns!
            if (bestStationWithDone != null)
            {
                _targetStation = bestStationWithDone;
                _state = State.MovingToStation;
                _statusDesc = "Cooking: Meat cooked! Pulling off spit";
                brain.LogAction($"CookingStation at {_targetStation.transform.position:F1} has cooked meat ready. Moving to remove.");
                brain.RecordTraceStep("COOK", "Cooked food ready on spit. Moving to retrieve.");
                return;
            }

            // If station has free slot, check if worker carries raw meat or needs to fetch from chest
            if (bestStationWithFreeSlot != null)
            {
                _targetStation = bestStationWithFreeSlot;
                var inv = brain.m_humanoid?.GetInventory();
                _rawMeatToCook = FindRawMeatInInventory(inv, _targetStation);

                if (_rawMeatToCook != null)
                {
                    _state = State.MovingToStation;
                    _statusDesc = $"Cooking: Grilling {_rawMeatToCook.m_shared?.m_name}";
                    brain.LogAction($"Worker carrying raw meat '{_rawMeatToCook.m_shared?.m_name}'. Moving to place on spit.");
                    brain.RecordTraceStep("COOK", $"Carrying raw meat '{_rawMeatToCook.m_shared?.m_name}'. Moving to cooking station.");
                    return;
                }
                else
                {
                    // Check nearby chests for raw meat
                    _state = State.FetchingRawIngredients;
                    return;
                }
            }

            // 3. If carrying cooked food, deposit in chests
            if (CarryingCookedFood(brain))
            {
                _statusDesc = "Storing cooked meals in chest";
                _state = State.ReturningToChests;
                return;
            }

            // Stand by near station or post
            float distToPost = Vector3.Distance(brain.transform.position, center);
            if (distToPost > 3.5f)
            {
                _statusDesc = $"Returning to kitchen ({distToPost:F1}m)";
                brain.MoveTo(center, 2f, distToPost > 14f);
            }
            else
            {
                brain.StopMoving();
                _statusDesc = "Cook: Standing by (Kitchen prepared)";
            }
        }

        private static ItemDrop.ItemData FindRawMeatInInventory(Inventory inv, CookingStation station = null)
        {
            if (inv == null) return null;
            var all = inv.GetAllItems();
            for (int i = 0; i < all.Count; i++)
            {
                var item = all[i];
                if (item == null) continue;
                string pName = item.m_dropPrefab != null ? item.m_dropPrefab.name : (item.m_shared?.m_name ?? "");
                if (IsRawMeat(pName))
                {
                    if (station == null || StationAcceptsItem(station, item))
                    {
                        return item;
                    }
                }
            }
            return null;
        }

        private static bool StationAcceptsItem(CookingStation station, ItemDrop.ItemData item)
        {
            if (station == null || item == null) return false;
            if (station.m_conversion == null || station.m_conversion.Count == 0) return true;
            foreach (var conv in station.m_conversion)
            {
                if (conv != null && conv.m_from != null)
                {
                    string fromName = conv.m_from.name;
                    string itemName = item.m_dropPrefab != null ? item.m_dropPrefab.name : (item.m_shared?.m_name ?? "");
                    if (string.Equals(fromName, itemName, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool CarryingCookedFood(WorkerBrain brain)
        {
            var inv = brain.m_humanoid?.GetInventory();
            if (inv == null) return false;
            var all = inv.GetAllItems();
            for (int i = 0; i < all.Count; i++)
            {
                string pName = all[i].m_dropPrefab != null ? all[i].m_dropPrefab.name : (all[i].m_shared?.m_name ?? "");
                if (IsCookedFood(pName)) return true;
            }
            return false;
        }

        private void HandleFetchingRawIngredients(WorkerBrain brain)
        {
            Vector3 center = brain.HasHome ? brain.HomePosition : brain.transform.position;
            Collider[] hits = Physics.OverlapSphere(center, brain.WorkRadius);

            Container bestChest = null;
            ItemDrop.ItemData meatInChest = null;
            float bestDist = float.MaxValue;

            for (int i = 0; i < hits.Length; i++)
            {
                var chest = hits[i].GetComponentInParent<Container>();
                if (chest == null || chest.IsInUse() || chest.GetComponent<WorkerBrain>() != null) continue;

                var cInv = chest.GetInventory();
                if (cInv == null) continue;

                var meat = FindRawMeatInInventory(cInv);
                if (meat != null)
                {
                    float d = Vector3.Distance(brain.transform.position, chest.transform.position);
                    if (d < bestDist)
                    {
                        bestDist = d;
                        bestChest = chest;
                        meatInChest = meat;
                    }
                }
            }

            if (bestChest != null && meatInChest != null)
            {
                float dChest = Vector3.Distance(brain.transform.position, bestChest.transform.position);
                if (dChest <= 2.2f)
                {
                    brain.StopMoving();
                    var inv = brain.m_humanoid?.GetInventory();
                    var cInv = bestChest.GetInventory();
                    if (inv != null && cInv != null)
                    {
                        // Take up to 5 raw meat pieces
                        int takeAmount = Mathf.Min(meatInChest.m_stack, 5);
                        if (inv.AddItem(meatInChest.m_dropPrefab, takeAmount))
                        {
                            cInv.RemoveItem(meatInChest, takeAmount);
                            brain.LogAction($"Took {takeAmount}x raw meat from chest at {bestChest.transform.position:F1}.");
                            brain.RecordTraceStep("INVENTORY", $"Retrieved {takeAmount}x raw meat from pantry.");
                        }
                    }
                    _state = State.ScanningKitchen;
                }
                else
                {
                    _statusDesc = $"Fetching raw meat from chest ({dChest:F1}m)";
                    brain.MoveTo(bestChest.transform.position, 2f, dChest > 14f);
                }
            }
            else
            {
                // No raw meat available
                _state = State.ScanningKitchen;
            }
        }

        private void HandleMovingToStation(WorkerBrain brain)
        {
            if (_targetStation == null)
            {
                _state = State.ScanningKitchen;
                return;
            }

            float dist = Vector3.Distance(brain.transform.position, _targetStation.transform.position);
            if (dist <= 2.2f)
            {
                brain.StopMoving();
                brain.FaceTarget(_targetStation.transform.position);
                _state = State.InteractingStation;
            }
            else
            {
                brain.MoveTo(_targetStation.transform.position, 1.8f, dist > 14f);
            }
        }

        private void HandleInteractingStation(WorkerBrain brain)
        {
            if (_actionCooldown > 0f) return;
            if (_targetStation == null)
            {
                _state = State.ScanningKitchen;
                return;
            }

            bool haveDone = Traverse.Create(_targetStation).Method("HaveDoneItem").GetValue<bool>();
            if (haveDone)
            {
                // Pull cooked meat off the spit
                _targetStation.Interact(brain.m_humanoid, false, false);
                brain.Say("Took cooked meat off spit.");
                brain.LogAction($"Removed cooked food from CookingStation at {_targetStation.transform.position:F1}.");
                brain.RecordTraceStep("COOK", "Removed cooked food from cooking spit.");
                VerificationTelemetry.RecordPass("cooking_station_grill", $"Cook '{brain.GetHoverName()}' removed cooked food from cooking station.");
                VerificationTelemetry.RecordPass("cooking_prevent_burn", $"Cook '{brain.GetHoverName()}' prevented food from overcooking into coal.");
                _actionCooldown = 1.0f;
                _state = State.SweepingCookedFood;
                return;
            }

            // Check if can place raw meat onto spit
            var inv = brain.m_humanoid?.GetInventory();
            var rawMeat = FindRawMeatInInventory(inv);
            bool isFull = Traverse.Create(_targetStation).Method("IsStationFull").GetValue<bool>();

            if (rawMeat != null && !isFull)
            {
                if (_targetStation.UseItem(brain.m_humanoid, rawMeat))
                {
                    brain.Say("Placed raw meat on spit.");
                    brain.LogAction($"Mounted raw meat '{rawMeat.m_shared?.m_name}' onto CookingStation.");
                    brain.RecordTraceStep("COOK", $"Placed raw '{rawMeat.m_shared?.m_name}' on cooking spit.");
                    VerificationTelemetry.RecordPass("cooking_station_grill", $"Cook '{brain.GetHoverName()}' placed raw meat on cooking station spit.");
                    if (_targetStation.gameObject.name.IndexOf("cauldron", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        (rawMeat.m_shared?.m_name != null && (rawMeat.m_shared.m_name.IndexOf("stew", StringComparison.OrdinalIgnoreCase) >= 0 || rawMeat.m_shared.m_name.IndexOf("soup", StringComparison.OrdinalIgnoreCase) >= 0)))
                    {
                        VerificationTelemetry.RecordPass("cooking_cauldron_stew", $"Cook '{brain.GetHoverName()}' prepared stew/soup in Cauldron.");
                    }
                    _actionCooldown = 1.0f;
                }
            }

            _state = State.ScanningKitchen;
        }

        private void HandleSweepingCookedFood(WorkerBrain brain)
        {
            Vector3 center = _targetStation != null ? _targetStation.transform.position : brain.transform.position;
            Collider[] hits = Physics.OverlapSphere(center, 8f);

            ItemDrop closest = null;
            float bestDist = float.MaxValue;
            for (int i = 0; i < hits.Length; i++)
            {
                var drop = hits[i].GetComponentInParent<ItemDrop>();
                if (drop == null || !drop.CanPickup(false)) continue;

                string pName = drop.m_itemData?.m_dropPrefab != null ? drop.m_itemData.m_dropPrefab.name : (drop.m_itemData?.m_shared?.m_name ?? "");
                if (IsCookedFood(pName))
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
                float d = Vector3.Distance(brain.transform.position, closest.transform.position);
                if (d <= 2.0f)
                {
                    brain.StopMoving();
                    string foodName = closest.m_itemData?.m_shared?.m_name ?? "cooked food";
                    int stack = closest.m_itemData?.m_stack ?? 1;
                    if (brain.m_humanoid.Pickup(closest.gameObject, false, false))
                    {
                        brain.LogAction($"Collected cooked '{foodName}' x{stack}.");
                        brain.RecordTraceStep("INVENTORY", $"Picked up cooked food '{foodName}' x{stack}.");
                    }
                }
                else
                {
                    brain.MoveTo(closest.transform.position, 1.8f, false);
                    return;
                }
            }

            _state = State.ScanningKitchen;
        }

        private void HandleReturningToChests(WorkerBrain brain)
        {
            Vector3 camp = brain.HasHome ? brain.HomePosition : brain.transform.position;
            float dist = Vector3.Distance(brain.transform.position, camp);

            if (dist <= 3.5f)
            {
                brain.StopMoving();
                _state = State.DepositingFood;
            }
            else
            {
                _statusDesc = $"Hauling cooked food to pantry ({dist:F1}m)";
                brain.MoveTo(camp, 2.5f, dist > 14f);
            }
        }

        private void HandleDepositingFood(WorkerBrain brain)
        {
            var inv = brain.m_humanoid?.GetInventory();
            if (inv == null || !CarryingCookedFood(brain))
            {
                _state = State.ScanningKitchen;
                return;
            }

            Vector3 camp = brain.HasHome ? brain.HomePosition : brain.transform.position;
            Collider[] hits = Physics.OverlapSphere(camp, 22f);

            Container bestMatchingChest = null;
            float bestMatchingDist = float.MaxValue;
            Container bestEmptyChest = null;
            float bestEmptyDist = float.MaxValue;

            var carriedFood = new List<ItemDrop.ItemData>();
            foreach (var item in inv.GetAllItems())
            {
                if (item == null) continue;
                string pName = item.m_dropPrefab != null ? item.m_dropPrefab.name : (item.m_shared?.m_name ?? "");
                if (IsCookedFood(pName)) carriedFood.Add(item);
            }

            for (int i = 0; i < hits.Length; i++)
            {
                var chest = hits[i].GetComponentInParent<Container>();
                if (chest == null || chest.IsInUse() || chest.GetComponent<WorkerBrain>() != null) continue;

                var cInv = chest.GetInventory();
                if (cInv == null) continue;

                float d = Vector3.Distance(brain.transform.position, chest.transform.position);

                // Priority 1: Has matching cooked food and can add item
                bool hasMatch = false;
                for (int j = 0; j < carriedFood.Count; j++)
                {
                    if (InventoryHelper.HasMatchingItem(cInv, carriedFood[j]) && cInv.CanAddItem(carriedFood[j], carriedFood[j].m_stack))
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
                if (LogisticsHelper.TryConstructOverflowResourcePiles(brain, camp))
                {
                    _statusDesc = "Cook: Built overflow resource pile";
                    _state = State.ScanningKitchen;
                    return;
                }
                _statusDesc = "Cook: No pantry chest available";
                _state = State.ScanningKitchen;
                return;
            }

            float dChest = Vector3.Distance(brain.transform.position, targetChest.transform.position);
            if (dChest <= 2.5f)
            {
                brain.StopMoving();
                brain.FaceTarget(targetChest.transform.position);

                var cInv = targetChest.GetInventory();
                int depositedCount = 0;

                for (int i = carriedFood.Count - 1; i >= 0; i--)
                {
                    var item = carriedFood[i];
                    if (cInv.CanAddItem(item, item.m_stack))
                    {
                        cInv.AddItem(item);
                        inv.RemoveItem(item);
                        depositedCount++;
                    }
                }

                if (depositedCount > 0)
                {
                    brain.Say("Stored cooked meals in pantry chest.");
                    brain.LogAction($"Deposited {depositedCount} cooked meal stack(s) into pantry chest at {targetChest.transform.position:F1}.");
                    brain.AddTaskXp(depositedCount * 10, TaskDignity.Skilled);
                    brain.RaiseActivitySkill(WorkerActivitySkill.Crafting, 1.0f * depositedCount);
                    brain.RecordTraceStep("INVENTORY", $"Stored {depositedCount} cooked meal stack(s) in pantry.");
                    VerificationTelemetry.RecordPass("cooking_pantry_deposit", $"Cook '{brain.GetHoverName()}' stored {depositedCount} cooked meal stack(s) in pantry chest.");
                }

                _state = State.ScanningKitchen;
            }
            else
            {
                _statusDesc = $"Moving to pantry chest ({dChest:F1}m)";
                brain.MoveTo(targetChest.transform.position, 2f, dChest > 14f);
            }
        }
    }
}
