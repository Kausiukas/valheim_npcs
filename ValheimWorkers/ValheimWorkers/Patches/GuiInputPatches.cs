using HarmonyLib;
using UnityEngine;
using ValheimWorkers.UI;

namespace ValheimWorkers.Patches
{
    [HarmonyPatch(typeof(PlayerController), "TakeInput")]
    public static class PlayerControllerTakeInputPatch
    {
        public static bool Prefix(ref bool __result)
        {
            if (NpcLabGui.IsOpen)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PlayerController), "InInventoryEtc")]
    public static class PlayerControllerInInventoryEtcPatch
    {
        public static void Postfix(ref bool __result)
        {
            if (NpcLabGui.IsOpen)
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(Player), "PlayerAttackInput")]
    public static class PlayerAttackInputPatch
    {
        public static bool Prefix()
        {
            if (NpcLabGui.IsOpen)
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Player), "TakeInput")]
    public static class PlayerTakeInputPatch
    {
        public static bool Prefix(ref bool __result)
        {
            if (NpcLabGui.IsOpen)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(StoreGui), "IsVisible")]
    public static class StoreGuiIsVisiblePatch
    {
        public static void Postfix(ref bool __result)
        {
            if (NpcLabGui.IsOpen)
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(GameCamera), "UpdateMouseCapture")]
    public static class GameCameraMouseCapturePatch
    {
        public static bool Prefix()
        {
            if (NpcLabGui.IsOpen)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                return false;
            }
            return true;
        }
    }
}
