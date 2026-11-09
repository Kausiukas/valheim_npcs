using HarmonyLib;
using ValheimWorkers.AI;

namespace ValheimWorkers.Patches
{
    [HarmonyPatch(typeof(MonsterAI), "UpdateAI")]
    public static class MonsterAIUpdateAIPatch
    {
        public static bool Prefix(MonsterAI __instance, float dt, ref bool __result)
        {
            var brain = __instance.GetComponent<WorkerBrain>();
            if (brain != null)
            {
                // Intercept vanilla MonsterAI so it never randomly wanders, flees, or runs back to old m_spawnPoint
                brain.UpdateWorkerAI(dt);
                __result = true;
                return false;
            }
            return true;
        }
    }
}
