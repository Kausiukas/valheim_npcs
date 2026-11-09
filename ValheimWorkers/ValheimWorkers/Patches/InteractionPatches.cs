using HarmonyLib;
using ValheimWorkers.AI;

namespace ValheimWorkers.Patches
{
    [HarmonyPatch(typeof(Character), nameof(Character.GetHoverText))]
    public static class CharacterHoverPatch
    {
        public static bool Prefix(Character __instance, ref string __result)
        {
            var brain = __instance.GetComponent<WorkerBrain>();
            if (brain != null)
            {
                __result = brain.GetHoverText();
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Character), nameof(Character.GetHoverName))]
    public static class CharacterHoverNamePatch
    {
        public static bool Prefix(Character __instance, ref string __result)
        {
            var brain = __instance.GetComponent<WorkerBrain>();
            if (brain != null)
            {
                __result = brain.GetHoverName();
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Container), nameof(Container.Interact))]
    public static class ContainerInteractPatch
    {
        public static bool Prefix(Container __instance, Humanoid character, bool hold, bool alt, ref bool __result)
        {
            var brain = __instance.GetComponent<WorkerBrain>() ?? __instance.GetComponentInParent<WorkerBrain>();
            if (brain != null)
            {
                // Route directly to WorkerBrain.Interact so [E] toggles Follow/Station,
                // and [Hold E] opens the worker inventory.
                __result = brain.Interact(character, hold, alt);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Container), nameof(Container.GetHoverText))]
    public static class ContainerHoverTextPatch
    {
        public static bool Prefix(Container __instance, ref string __result)
        {
            var brain = __instance.GetComponent<WorkerBrain>() ?? __instance.GetComponentInParent<WorkerBrain>();
            if (brain != null)
            {
                __result = brain.GetHoverText();
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Container), nameof(Container.GetHoverName))]
    public static class ContainerHoverNamePatch
    {
        public static bool Prefix(Container __instance, ref string __result)
        {
            var brain = __instance.GetComponent<WorkerBrain>() ?? __instance.GetComponentInParent<WorkerBrain>();
            if (brain != null)
            {
                __result = brain.GetHoverName();
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Container), nameof(Container.UseItem))]
    public static class ContainerUseItemPatch
    {
        public static bool Prefix(Container __instance, Humanoid user, ItemDrop.ItemData item, ref bool __result)
        {
            var brain = __instance.GetComponent<WorkerBrain>() ?? __instance.GetComponentInParent<WorkerBrain>();
            if (brain != null)
            {
                __result = brain.UseItem(user, item);
                return false;
            }
            return true;
        }
    }
}
