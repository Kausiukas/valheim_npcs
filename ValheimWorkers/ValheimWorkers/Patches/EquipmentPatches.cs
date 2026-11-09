using HarmonyLib;
using UnityEngine;
using ValheimWorkers.AI;

namespace ValheimWorkers.Patches
{
    /// <summary>
    /// Patches Humanoid equipment logic to enforce permanent worker armor locking
    /// and action-commenced item locking.
    /// </summary>
    public static class EquipmentPatches
    {
        [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.UnequipItem))]
        public static class HumanoidUnequipItemPatch
        {
            public static bool Prefix(Humanoid __instance, ItemDrop.ItemData item, bool triggerEquipEffects)
            {
                if (item == null) return true;
                var brain = __instance.GetComponent<WorkerBrain>();
                if (brain == null) return true;

                // 1. Armor remains on ALWAYS unless manually removed by player in GUI
                if (!brain.AllowManualPlayerUnequip && item.m_shared != null)
                {
                    var itemType = item.m_shared.m_itemType;
                    if (itemType == ItemDrop.ItemData.ItemType.Chest ||
                        itemType == ItemDrop.ItemData.ItemType.Legs ||
                        itemType == ItemDrop.ItemData.ItemType.Helmet ||
                        itemType == ItemDrop.ItemData.ItemType.Shoulder)
                    {
                        return false;
                    }
                }

                // 2. Hand items locked during commenced action, unless AI is explicitly swapping weapon modes (e.g. Ranged <-> Melee)
                if (!brain.AllowManualPlayerUnequip && !brain.AllowAiWeaponSwitch && brain.IsActionLocked)
                {
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.UnequipAllItems))]
        public static class HumanoidUnequipAllItemsPatch
        {
            public static bool Prefix(Humanoid __instance)
            {
                var brain = __instance.GetComponent<WorkerBrain>();
                if (brain != null && !brain.AllowManualPlayerUnequip)
                {
                    // Block wholesale unequip routines so worker armor and gear persist
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Humanoid), "SetupVisEquipment")]
        public static class HumanoidSetupVisEquipmentPatch
        {
            public static void Postfix(Humanoid __instance, VisEquipment visEq, bool isRagdoll)
            {
                if (visEq == null || isRagdoll) return;
                var brain = __instance.GetComponent<WorkerBrain>();
                if (brain == null || brain.Customization == null) return;

                var cust = brain.Customization;

                // Enforce persistent armor visuals so workers never undress
                if (!string.IsNullOrEmpty(cust.Chest))
                {
                    visEq.SetChestItem(cust.Chest.GetStableHashCode());
                }
                if (!string.IsNullOrEmpty(cust.Legs))
                {
                    visEq.SetLegItem(cust.Legs.GetStableHashCode());
                }
                if (!string.IsNullOrEmpty(cust.Helmet))
                {
                    visEq.SetHelmetItem(cust.Helmet.GetStableHashCode());
                }

                // If action is commenced/locked, preserve actual equipped weapon/shield/tool in hand
                if (brain.IsActionLocked && !brain.AllowAiWeaponSwitch)
                {
                    var right = brain.GetEquippedRightItem();
                    var left = brain.GetEquippedLeftItem();

                    if (right != null)
                    {
                        string rPrefab = WorkerBrain.GetItemPrefabName(right);
                        if (!string.IsNullOrEmpty(rPrefab))
                        {
                            visEq.SetRightItem(rPrefab.GetStableHashCode(), right.m_variant);
                        }
                    }
                    else if (brain.CurrentCombatMode != WorkerBrain.HunterCombatMode.Ranged && !string.IsNullOrEmpty(cust.Weapon) && !WorkerBrain.IsBowName(cust.Weapon))
                    {
                        visEq.SetRightItem(cust.Weapon.GetStableHashCode(), 0);
                    }

                    if (left != null)
                    {
                        string lPrefab = WorkerBrain.GetItemPrefabName(left);
                        if (!string.IsNullOrEmpty(lPrefab))
                        {
                            bool isBow = WorkerBrain.IsBowName(lPrefab);
                            visEq.SetLeftItem(lPrefab.GetStableHashCode(), left.m_variant, isBow ? 1 : 0);
                        }
                    }
                    else if (brain.CurrentCombatMode == WorkerBrain.HunterCombatMode.Ranged && !string.IsNullOrEmpty(cust.Weapon) && WorkerBrain.IsBowName(cust.Weapon))
                    {
                        visEq.SetLeftItem(cust.Weapon.GetStableHashCode(), 0, 1);
                    }
                    else if (!string.IsNullOrEmpty(cust.Shield) && brain.CurrentCombatMode != WorkerBrain.HunterCombatMode.Ranged)
                    {
                        visEq.SetLeftItem(cust.Shield.GetStableHashCode(), 0, 0);
                    }
                }
            }
        }
    }
}
