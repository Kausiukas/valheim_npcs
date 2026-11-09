using System;
using System.Collections;
using System.Reflection;
using HarmonyLib;

namespace ValheimWorkers.Patches
{
    /// <summary>
    /// Fixes vanilla Valheim / Unity MaterialMan NullReferenceException loop.
    /// In vanilla, if a PropertyContainer has a missing material property block,
    /// UpdateBlock throws NullReferenceException before RemoveAt() is called,
    /// causing an infinite spam of NREs every frame.
    /// </summary>
    [HarmonyPatch(typeof(MaterialMan), "Update")]
    public static class MaterialManUpdatePatch
    {
        private static readonly AccessTools.FieldRef<MaterialMan, IList> s_containersRef =
            AccessTools.FieldRefAccess<MaterialMan, IList>("m_containersToUpdate");

        private static MethodInfo s_cachedUpdateBlockMethod;

        public static bool Prefix(MaterialMan __instance)
        {
            if (__instance == null) return true;

            var list = s_containersRef(__instance);
            if (list == null || list.Count == 0) return false;

            for (int i = list.Count - 1; i >= 0; i--)
            {
                object container = list[i];
                list.RemoveAt(i); // Always pop the container first to prevent infinite error loops

                if (container == null) continue;

                try
                {
                    if (s_cachedUpdateBlockMethod == null)
                    {
                        s_cachedUpdateBlockMethod = container.GetType().GetMethod("UpdateBlock", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    }
                    s_cachedUpdateBlockMethod?.Invoke(container, null);
                }
                catch
                {
                    // Suppress harmless material update error on destroyed/missing visual elements
                }
            }

            return false; // Skip the vanilla loop
        }
    }
}
