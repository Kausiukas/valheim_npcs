using HarmonyLib;
using ValheimWorkers.Prefabs;

namespace ValheimWorkers.Patches
{
    [HarmonyPatch(typeof(ZNetScene), "Awake")]
    public static class ZNetSceneAwakePatch
    {
        public static void Postfix(ZNetScene __instance)
        {
            if (__instance == null) return;
            WorkerPrefabManager.RegisterWorkerPrefabs(__instance);
            RelicPrefabManager.RegisterRelics(__instance);
        }
    }

    [HarmonyPatch(typeof(ObjectDB), "Awake")]
    public static class ObjectDbAwakePatch
    {
        public static void Postfix(ObjectDB __instance)
        {
            if (__instance == null) return;
            RelicPrefabManager.RegisterObjectDbItems(__instance);
        }
    }

    [HarmonyPatch(typeof(ObjectDB), "CopyOtherDB")]
    public static class ObjectDbCopyPatch
    {
        public static void Postfix(ObjectDB __instance)
        {
            if (__instance == null) return;
            RelicPrefabManager.RegisterObjectDbItems(__instance);
        }
    }
}
