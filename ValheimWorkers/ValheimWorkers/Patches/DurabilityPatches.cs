using HarmonyLib;
using UnityEngine;
using ValheimWorkers.AI;

namespace ValheimWorkers.Patches
{
    /// <summary>
    /// Patches combat, mining, and harvesting durability routines to apply controlled
    /// durability wear with a 1.0f minimum floor so worker gear never breaks mid-fight
    /// while still wearing down realistically for workbench repairs.
    /// </summary>
    public static class DurabilityPatches
    {
        [HarmonyPatch(typeof(Humanoid), "DrainEquipedItemDurability")]
        public static class DrainEquipedItemDurabilityPatch
        {
            public static bool Prefix(Humanoid __instance)
            {
                if (__instance.GetComponent<WorkerBrain>() != null)
                {
                    // Prevent native drain from reaching 0 and unequipping gear; controlled wear is applied below
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Attack), "DoMeleeAttack")]
        public static class AttackMeleeDurabilityPatch
        {
            public static void Postfix(Attack __instance, Humanoid ___m_character, ItemDrop.ItemData ___m_weapon)
            {
                if (___m_character != null && ___m_character.GetComponent<WorkerBrain>() != null && ___m_weapon != null)
                {
                    if (___m_weapon.m_shared != null && ___m_weapon.m_shared.m_useDurability)
                    {
                        ___m_weapon.m_durability = Mathf.Max(1.0f, ___m_weapon.m_durability - 1.0f);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Attack), "ProjectileAttackTriggered")]
        public static class AttackProjectileDurabilityPatch
        {
            public static void Postfix(Attack __instance, Humanoid ___m_character, ItemDrop.ItemData ___m_weapon)
            {
                if (___m_character != null && ___m_character.GetComponent<WorkerBrain>() != null && ___m_weapon != null)
                {
                    if (___m_weapon.m_shared != null && ___m_weapon.m_shared.m_useDurability)
                    {
                        ___m_weapon.m_durability = Mathf.Max(1.0f, ___m_weapon.m_durability - 1.5f);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Humanoid), "BlockAttack")]
        public static class HumanoidBlockDurabilityPatch
        {
            public static void Postfix(Humanoid __instance)
            {
                if (__instance.GetComponent<WorkerBrain>() != null)
                {
                    var left = Traverse.Create(__instance).Field<ItemDrop.ItemData>("m_leftItem").Value;
                    if (left != null && left.m_shared != null && left.m_shared.m_useDurability)
                    {
                        left.m_durability = Mathf.Max(1.0f, left.m_durability - 0.5f);
                    }
                }
            }
        }
    }
}
