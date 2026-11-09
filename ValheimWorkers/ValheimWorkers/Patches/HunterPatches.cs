using HarmonyLib;
using UnityEngine;
using ValheimWorkers.AI;

namespace ValheimWorkers.Patches
{
    [HarmonyPatch]
    public static class HunterPatches
    {
        private static readonly AccessTools.FieldRef<Attack, Humanoid> s_attackCharacterRef =
            AccessTools.FieldRefAccess<Attack, Humanoid>("m_character");

        [HarmonyPatch(typeof(Attack), "GetProjectileSpawnPoint")]
        [HarmonyPostfix]
        public static void Attack_GetProjectileSpawnPoint_Postfix(Attack __instance, ref Vector3 spawnPoint, ref Vector3 aimDir)
        {
            if (__instance == null) return;
            Humanoid character = s_attackCharacterRef(__instance);
            if (character == null) return;

            var brain = character.GetComponent<WorkerBrain>();
            if (brain != null && brain.CustomAimDir.HasValue)
            {
                aimDir = brain.CustomAimDir.Value;
            }
        }

        [HarmonyPatch(typeof(Character), nameof(Character.GetStealthFactor))]
        [HarmonyPostfix]
        public static void Character_GetStealthFactor_Postfix(Character __instance, ref float __result)
        {
            if (__instance == null) return;
            var brain = __instance.GetComponent<WorkerBrain>();
            if (brain != null && brain.IsCrouchingOrStalking)
            {
                __result = 0.25f; // Cuts prey vision detection range down to 25% while crawling
            }
        }

        [HarmonyPatch(typeof(Character), nameof(Character.IsCrouching))]
        [HarmonyPostfix]
        public static void Character_IsCrouching_Postfix(Character __instance, ref bool __result)
        {
            if (__instance == null) return;
            var brain = __instance.GetComponent<WorkerBrain>();
            if (brain != null && brain.IsCrouchingOrStalking)
            {
                __result = true;
            }
        }

        [HarmonyPatch(typeof(Character), nameof(Character.GetNoiseRange))]
        [HarmonyPostfix]
        public static void Character_GetNoiseRange_Postfix(Character __instance, ref float __result)
        {
            if (__instance == null) return;
            var brain = __instance.GetComponent<WorkerBrain>();
            if (brain != null && brain.IsCrouchingOrStalking)
            {
                __result = 0f; // Silent crawling - prey cannot hear hunter's footsteps
            }
        }

        [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.GetAttackDrawPercentage))]
        [HarmonyPostfix]
        public static void Humanoid_GetAttackDrawPercentage_Postfix(Humanoid __instance, ref float __result)
        {
            if (__instance == null) return;
            var brain = __instance.GetComponent<WorkerBrain>();
            if (brain != null && brain.CurrentCombatMode == WorkerBrain.HunterCombatMode.Ranged)
            {
                if (brain.LastReleasedBowDrawPercentage > 0f)
                {
                    __result = Mathf.Max(__result, brain.LastReleasedBowDrawPercentage);
                }
                else if (brain.ActiveBowDrawPercentage > 0f)
                {
                    __result = Mathf.Max(__result, brain.ActiveBowDrawPercentage);
                }
            }
        }
    }
}
