using System;
using System.Collections.Generic;
using UnityEngine;
using ValheimWorkers.Core;

namespace ValheimWorkers.AI
{
    public class HaulerTask : IWorkerTask
    {
        public WorkerRole Role => WorkerRole.Hauler;

        private enum State
        {
            SearchingItem,
            MovingToItem,
            FindingChest,
            MovingToChest,
            Depositing
        }

        private State _state = State.SearchingItem;
        private ItemDrop _targetItem = null;
        private Container _targetChest = null;
        private float _scanTimer = 0f;
        private string _statusDesc = "Searching for dropped items...";

        public bool CanExecute(WorkerBrain brain) => brain != null && brain.m_humanoid != null;
        public bool IsPerformingAction => false;

        public void StartTask(WorkerBrain brain)
        {
            _state = State.SearchingItem;
            _targetItem = null;
            _targetChest = null;
            _scanTimer = 0f;
            _statusDesc = "Starting Hauler task...";
            brain.LogAction($"Started Hauler task around {brain.HomePosition:F1} (Radius: {brain.WorkRadius:F0}m)");
        }

        public void StopTask(WorkerBrain brain)
        {
            _targetItem = null;
            _targetChest = null;
            brain.StopMoving();
        }

        public string GetStatusDescription() => _statusDesc;

        public void UpdateTask(WorkerBrain brain, float dt)
        {
            _scanTimer += dt;

            // If worker already has items in inventory, check if we should deposit or batch-collect nearby drops
            if (ShouldDepositNow(brain))
            {
                if (_state != State.FindingChest && _state != State.MovingToChest && _state != State.Depositing)
                {
                    _state = State.FindingChest;
                }
            }

            switch (_state)
            {
                case State.SearchingItem:
                    FindNearestItem(brain);
                    break;

                case State.MovingToItem:
                    HandleMovingToItem(brain);
                    break;

                case State.FindingChest:
                    FindNearestChest(brain);
                    break;

                case State.MovingToChest:
                    HandleMovingToChest(brain);
                    break;

                case State.Depositing:
                    HandleDepositing(brain);
                    break;
            }
        }

        private bool ShouldDepositNow(WorkerBrain brain)
        {
            if (!HasItemsToDeposit(brain)) return false;
            if (brain.IsInventoryFull()) return true;

            int carriedCount = GetCarriedResourceCount(brain);
            if (carriedCount >= 25) return true;

            // If there are other ground items within 16m of worker, continue batching!
            Collider[] nearbyHits = Physics.OverlapSphere(brain.transform.position, 16f);
            for (int i = 0; i < nearbyHits.Length; i++)
            {
                var drop = nearbyHits[i].GetComponentInParent<ItemDrop>();
                if (drop != null && drop.CanPickup(false))
                {
                    return false; // Still nearby items to collect in this batch
                }
            }

            // No nearby items left: return to chest and store
            return true;
        }

        private int GetCarriedResourceCount(WorkerBrain brain)
        {
            var inv = brain.m_humanoid?.GetInventory();
            if (inv == null) return 0;
            int total = 0;
            var all = inv.GetAllItems();
            for (int i = 0; i < all.Count; i++)
            {
                if (!brain.IsProtectedWorkGear(all[i])) total += all[i].m_stack;
            }
            return total;
        }

        private bool HasItemsToDeposit(WorkerBrain brain)
        {
            var inv = brain.m_humanoid?.GetInventory();
            if (inv == null) return false;

            var allItems = inv.GetAllItems();
            for (int i = 0; i < allItems.Count; i++)
            {
                if (!brain.IsProtectedWorkGear(allItems[i]))
                {
                    return true;
                }
            }
            return false;
        }

        private void FindNearestItem(WorkerBrain brain)
        {
            Vector3 center = brain.GetWorkPostPosition();

            if (_scanTimer >= 0.5f)
            {
                _scanTimer = 0f;
                float radius = brain.WorkRadius;

                Collider[] hits = Physics.OverlapSphere(center, radius);
                ItemDrop closest = null;
                float minDist = float.MaxValue;

                for (int i = 0; i < hits.Length; i++)
                {
                    var drop = hits[i].GetComponentInParent<ItemDrop>();
                    if (drop == null || !drop.CanPickup(false)) continue;
                    // Do not pick up weapons, armor, or tools so haulers never steal dropped player equipment
                    if (drop.m_itemData != null && WorkerBrain.IsEquippableGear(drop.m_itemData)) continue;

                    float dist = Vector3.Distance(brain.transform.position, drop.transform.position);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closest = drop;
                    }
                }

                if (closest != null)
                {
                    _targetItem = closest;
                    _state = State.MovingToItem;
                    string itemName = _targetItem.m_itemData?.m_shared?.m_name ?? "item";
                    string locName = Localization.instance != null ? Localization.instance.Localize(itemName) : itemName;
                    _statusDesc = $"Hauling: Moving to {locName}";
                    brain.LogAction($"Found dropped '{locName}' x{_targetItem.m_itemData?.m_stack ?? 1} at {_targetItem.transform.position:F1}. Moving to collect.");
                    return;
                }
            }

            // Return near work post while waiting for items
            float distToPost = Vector3.Distance(brain.transform.position, center);
            if (distToPost > 3.5f)
            {
                _statusDesc = $"Returning to work post ({distToPost:F1}m)";
                brain.LogAction($"No ground items found. Returning to work post {center:F1} (dist: {distToPost:F1}m)");
                brain.MoveTo(center, 2f, distToPost > 12f);
            }
            else
            {
                brain.StopMoving();
                _statusDesc = "Standing by at post (No items on ground)";
                brain.LogAction($"Standing by at work post {center:F1} (no dropped items in {brain.WorkRadius:F0}m)");
            }
        }

        private void HandleMovingToItem(WorkerBrain brain)
        {
            if (_targetItem == null || !_targetItem.CanPickup(false))
            {
                _state = State.SearchingItem;
                return;
            }

            float dist = Vector3.Distance(brain.transform.position, _targetItem.transform.position);
            if (dist <= 2.2f)
            {
                brain.StopMoving();
                brain.FaceTarget(_targetItem.transform.position);

                string itemName = _targetItem.m_itemData?.m_shared?.m_name ?? "item";
                string locName = Localization.instance != null ? Localization.instance.Localize(itemName) : itemName;
                int stack = _targetItem.m_itemData?.m_stack ?? 1;

                bool picked = brain.m_humanoid.Pickup(_targetItem.gameObject, false, false);
                if (picked)
                {
                    brain.Say("Collected items.");
                    brain.LogAction($"Picked up '{locName}' x{stack}. Searching for storage chest.");
                }
                _targetItem = null;
                _state = State.FindingChest;
            }
            else
            {
                brain.MoveTo(_targetItem.transform.position, 1.8f, dist > 14f);
            }
        }

        private void FindNearestChest(WorkerBrain brain)
        {
            Vector3 center = brain.HasHome ? brain.HomePosition : brain.transform.position;
            float searchRadius = Mathf.Max(brain.WorkRadius, 25f);
            Collider[] hits = Physics.OverlapSphere(center, searchRadius);

            var workerInv = brain.m_humanoid?.GetInventory();
            var carriedItems = workerInv != null ? workerInv.GetAllItems() : new List<ItemDrop.ItemData>();

            Container bestMatchingChest = null;
            float minMatchingDist = float.MaxValue;

            Container bestEmptyChest = null;
            float minEmptyDist = float.MaxValue;

            for (int i = 0; i < hits.Length; i++)
            {
                var chest = hits[i].GetComponentInParent<Container>();
                if (chest == null || chest.IsInUse() || chest.GetComponent<WorkerBrain>() != null) continue;
                var chestInv = chest.GetInventory();
                if (chestInv == null) continue;

                float dist = Vector3.Distance(brain.transform.position, chest.transform.position);

                // Priority 1: Check if chest already has matching items and space
                bool hasMatch = false;
                foreach (var item in carriedItems)
                {
                    if (brain.IsProtectedWorkGear(item)) continue;
                    string pName = item.m_dropPrefab != null ? item.m_dropPrefab.name : item.m_shared.m_name;
                    if (InventoryHelper.HasMatchingItem(chestInv, item) && chestInv.CanAddItem(item, item.m_stack))
                    {
                        hasMatch = true;
                        break;
                    }
                }

                if (hasMatch && dist < minMatchingDist)
                {
                    minMatchingDist = dist;
                    bestMatchingChest = chest;
                }

                // Priority 2: Chest with empty slots
                if (chestInv.GetEmptySlots() > 0 && dist < minEmptyDist)
                {
                    minEmptyDist = dist;
                    bestEmptyChest = chest;
                }
            }

            Container chosenChest = bestMatchingChest ?? bestEmptyChest;
            if (chosenChest == null)
            {
                chosenChest = LogisticsHelper.FindNearbyCartContainer(center, searchRadius);
            }

            if (chosenChest != null)
            {
                _targetChest = chosenChest;
                _state = State.MovingToChest;
                _statusDesc = "Delivering items to chest...";
                brain.LogAction($"Located storage chest at {_targetChest.transform.position:F1} (Match: {bestMatchingChest != null}). Moving to deposit.");
                if (bestMatchingChest != null)
                {
                    VerificationTelemetry.RecordPass("logistics_smart_sorting", $"Worker '{brain.GetHoverName()}' prioritized chest at {_targetChest.transform.position:F1} that already contained matching item stacks.");
                }
            }
            else
            {
                // [F05] Construct physical overflow resource piles!
                if (LogisticsHelper.TryConstructOverflowResourcePiles(brain, center))
                {
                    _statusDesc = "Hauler: Built overflow resource pile";
                    _state = State.SearchingItem;
                    return;
                }

                _statusDesc = "Waiting: No chest with free space in radius";
                brain.LogAction($"Cannot deposit carried items: no chest with free space within {brain.WorkRadius:F0}m of {center:F1}.");
                float distToPost = Vector3.Distance(brain.transform.position, center);
                if (distToPost > 3.5f)
                {
                    brain.MoveTo(center, 2f, false);
                }
                else
                {
                    brain.StopMoving();
                }
            }
        }

        private void HandleMovingToChest(WorkerBrain brain)
        {
            if (_targetChest == null)
            {
                _state = State.FindingChest;
                return;
            }

            float dist = Vector3.Distance(brain.transform.position, _targetChest.transform.position);
            if (dist <= 2.8f)
            {
                brain.StopMoving();
                brain.FaceTarget(_targetChest.transform.position);
                _state = State.Depositing;
            }
            else
            {
                brain.MoveTo(_targetChest.transform.position, 2.2f, dist > 14f);
            }
        }

        private void HandleDepositing(WorkerBrain brain)
        {
            if (_targetChest == null || _targetChest.IsInUse())
            {
                _state = State.FindingChest;
                return;
            }

            var workerInv = brain.m_humanoid?.GetInventory();
            var chestInv = _targetChest.GetInventory();
            if (workerInv == null || chestInv == null)
            {
                _state = State.SearchingItem;
                return;
            }

            var items = new List<ItemDrop.ItemData>(workerInv.GetAllItems());
            int transferredCount = 0;
            int protectedCount = 0;

            foreach (var item in items)
            {
                if (brain.IsProtectedWorkGear(item))
                {
                    protectedCount++;
                    continue;
                }

                if (chestInv.CanAddItem(item, item.m_stack))
                {
                    chestInv.AddItem(item);
                    workerInv.RemoveItem(item);
                    transferredCount++;
                }
            }

            if (transferredCount > 0)
            {
                brain.Say($"Stored {transferredCount} stack(s).");
                brain.LogAction($"Deposited {transferredCount} item stack(s) into chest at {_targetChest.transform.position:F1}.");
                brain.AddTaskXp(5 * transferredCount, TaskDignity.Menial);
                brain.RaiseActivitySkill(WorkerActivitySkill.Athletics, 1.0f * transferredCount);
                if (protectedCount > 0)
                {
                    VerificationTelemetry.RecordPass("logistics_protected_gear", $"Worker '{brain.GetHoverName()}' deposited {transferredCount} item stack(s) while retaining {protectedCount} protected gear item(s).");
                }
                if (_targetChest.GetComponentInParent<Vagon>() != null ||
                    _targetChest.gameObject.name.IndexOf("cart", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    _targetChest.gameObject.name.IndexOf("vagon", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    VerificationTelemetry.RecordPass("logistics_cart_loading", $"Hauler '{brain.GetHoverName()}' deposited {transferredCount} item stack(s) into mobile cart storage.");
                }
            }

            _targetChest = null;
            _state = State.SearchingItem;
        }
    }
}
