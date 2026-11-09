using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using ValheimWorkers.AI;
using ValheimWorkers.Core;
using ValheimWorkers.Prefabs;

namespace ValheimWorkers.Patches
{
    public static class InventoryGuiPatches
    {
        /// <summary>
        /// Ensure the local player is always considered the owner when viewing a worker's container inventory,
        /// preventing InventoryGui from auto-closing the grid in multiplayer or host mode.
        /// </summary>
        [HarmonyPatch(typeof(Container), nameof(Container.IsOwner))]
        public static class ContainerIsOwnerPatch
        {
            public static bool Prefix(Container __instance, ref bool __result)
            {
                if (__instance != null)
                {
                    var worker = __instance.GetComponent<WorkerBrain>();
                    if (worker != null)
                    {
                        var nv = worker.m_nview ?? __instance.GetComponent<ZNetView>();
                        if (nv != null && nv.IsValid() && !nv.IsOwner())
                        {
                            nv.ClaimOwnership();
                        }
                        __result = true;
                        return false;
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// Allow player to view worker inventory even if opened from across the base via F8 Settlement Hall.
        /// </summary>
        [HarmonyPatch(typeof(InventoryGui), "UpdateContainer")]
        public static class InventoryGuiUpdateContainerPatch
        {
            public static void Prefix(ref float ___m_autoCloseDistance, Container ___m_currentContainer)
            {
                if (___m_currentContainer != null && ___m_currentContainer.GetComponent<WorkerBrain>() != null)
                {
                    ___m_autoCloseDistance = 120f;
                }
            }
        }

        /// <summary>
        /// Right-clicking an item in the Worker's container grid equips or unequips the gear on the worker,
        /// exactly mirroring the player's native inventory equip behavior.
        /// </summary>
        [HarmonyPatch(typeof(InventoryGui), "OnRightClickItem")]
        public static class InventoryGuiRightClickPatch
        {
            public static bool Prefix(Container ___m_currentContainer, InventoryGrid ___m_containerGrid, InventoryGrid grid, ItemDrop.ItemData item, Vector2i pos)
            {
                if (grid == null || item == null) return true;
                if (___m_currentContainer == null) return true;

                var worker = ___m_currentContainer.GetComponent<WorkerBrain>();
                if (worker == null) return true;

                if (grid == ___m_containerGrid)
                {
                    worker.OnNpcGridRightClick(item);
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Enable the native cyan equipped highlight box on items in the worker's container grid
        /// when item.m_equipped is true, overcoming vanilla's hardcoded disable for containers.
        /// </summary>
        [HarmonyPatch(typeof(InventoryGrid), "UpdateGui", new[] { typeof(Player), typeof(ItemDrop.ItemData) })]
        public static class InventoryGridUpdateGuiPatch
        {
            public static void Postfix(InventoryGrid __instance, List<InventoryElement> ___m_elements)
            {
                if (__instance == null || ___m_elements == null) return;
                var inv = __instance.GetInventory();
                if (inv == null) return;
                if (InventoryGui.instance == null) return;

                var currentContainer = Traverse.Create(InventoryGui.instance).Field<Container>("m_currentContainer").Value;
                if (currentContainer == null) return;

                var worker = currentContainer.GetComponent<WorkerBrain>();
                if (worker == null) return;

                var containerGrid = Traverse.Create(InventoryGui.instance).Field<InventoryGrid>("m_containerGrid").Value;
                if (containerGrid != __instance) return;

                for (int i = 0; i < ___m_elements.Count; i++)
                {
                    var element = ___m_elements[i];
                    if (element == null || !element.m_used) continue;

                    var item = inv.GetItemAt(element.Position.x, element.Position.y);
                    if (item != null && item.m_equipped && element.m_equiped != null)
                    {
                        element.m_equiped.enabled = true;
                    }
                }
            }
        }

        /// <summary>
        /// Close the NPC Character Menu if InventoryGui closes
        /// </summary>
        [HarmonyPatch(typeof(InventoryGui), "Show")]
        public static class InventoryGuiShowPatch
        {
            public static void Postfix()
            {
                if (Player.m_localPlayer != null && Player.m_localPlayer.GetInventory() != null)
                {
                    var pInv = Player.m_localPlayer.GetInventory();
                    if (pInv.HaveItem(RelicPrefabManager.PrefabSacrificialDagger))
                        VerificationTelemetry.RecordPass("expedition_dagger_discovery", "Discovered Skarde's Sacrificial Dagger.");
                    if (pInv.HaveItem(RelicPrefabManager.PrefabBloodChalice))
                        VerificationTelemetry.RecordPass("expedition_chalice_discovery", "Discovered Blood Chalice of Valka.");
                    if (pInv.HaveItem(RelicPrefabManager.PrefabAwakeningOil) || pInv.HaveItem(RelicPrefabManager.PrefabGjallarhorn))
                        VerificationTelemetry.RecordPass("expedition_human_relics_discovery", "Discovered Draupnir Awakening Oil / Gjallarhorn of the Hearth.");
                    if (pInv.HaveItem(RelicPrefabManager.PrefabDvergrSeal))
                        VerificationTelemetry.RecordPass("expedition_dvergr_seal_discovery", "Discovered Grand Dvergr Guild Seal.");
                }
            }
        }

        [HarmonyPatch(typeof(InventoryGui), "Hide")]
        public static class InventoryGuiHidePatch
        {
            public static void Postfix()
            {
                if (ValheimWorkers.UI.NpcCharacterMenuGui.IsOpen)
                {
                    ValheimWorkers.UI.NpcCharacterMenuGui.Close();
                }
            }
        }
    }
}
