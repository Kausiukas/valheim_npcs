using System;
using System.Collections.Generic;
using UnityEngine;
using ValheimWorkers.AI;

namespace ValheimWorkers.Core
{
    public static class LogisticsHelper
    {
        // Maps resource prefabs to their pile piece prefab name and stack requirement
        private static readonly Dictionary<string, (string pilePrefab, int stackSize)> s_overflowPileDefs = 
            new Dictionary<string, (string pilePrefab, int stackSize)>(StringComparer.OrdinalIgnoreCase)
        {
            { "Wood", ("wood_stack", 50) },
            { "FineWood", ("wood_fine_stack", 50) },
            { "CoreWood", ("wood_core_stack", 50) },
            { "YggdrasilWood", ("wood_yggdrasil_stack", 50) },
            { "Stone", ("stone_pile", 50) },
            { "Coal", ("coal_pile", 50) },
            { "BoneFragments", ("bone_stack", 50) },
            { "Coins", ("treasure_stack", 99) }
        };

        /// <summary>
        /// [F05] Tries to construct physical resource piles when chests are full or unavailable.
        /// </summary>
        public static bool TryConstructOverflowResourcePiles(WorkerBrain brain, Vector3 campCenter)
        {
            if (brain == null || brain.m_humanoid == null) return false;
            var inv = brain.m_humanoid.GetInventory();
            if (inv == null) return false;

            var items = new List<ItemDrop.ItemData>(inv.GetAllItems());
            bool constructedAny = false;

            foreach (var item in items)
            {
                if (brain.IsProtectedWorkGear(item)) continue;
                string pName = WorkerBrain.GetItemPrefabName(item);
                if (string.IsNullOrEmpty(pName)) continue;

                if (!s_overflowPileDefs.TryGetValue(pName, out var pileDef)) continue;

                // Build a pile if we have at least the required stack size (or a substantial surplus if chests are completely full)
                int requiredCount = pileDef.stackSize;
                if (item.m_stack < requiredCount)
                {
                    // Also allow partial threshold if inventory is critically packed and cannot store anywhere
                    if (brain.IsInventoryFull() && item.m_stack >= 25 && (pName == "Wood" || pName == "Stone"))
                    {
                        requiredCount = item.m_stack; // Build with what we have
                    }
                    else
                    {
                        continue;
                    }
                }

                GameObject pilePrefab = ZNetScene.instance != null ? ZNetScene.instance.GetPrefab(pileDef.pilePrefab) : null;
                if (pilePrefab == null) continue;

                if (FindSuitablePileLocation(campCenter, out Vector3 placePos))
                {
                    // Place the physical pile piece
                    GameObject newPile = UnityEngine.Object.Instantiate(pilePrefab, placePos, Quaternion.identity);
                    var piece = newPile.GetComponent<Piece>();
                    if (piece != null)
                    {
                        long creatorId = Player.m_localPlayer != null ? Player.m_localPlayer.GetPlayerID() : 0L;
                        piece.SetCreator(creatorId, new Splatform.PlatformUserID());
                    }

                    inv.RemoveItem(item, requiredCount);

                    var zanim = brain.m_humanoid.GetComponent<ZSyncAnimation>();
                    zanim?.SetTrigger("swing_hammer");

                    brain.Say($"Constructed {pileDef.pilePrefab.Replace('_', ' ')}.");
                    brain.LogAction($"Chest overflow: Built '{pileDef.pilePrefab}' using {requiredCount}x {pName} at {placePos:F1}.");
                    brain.RecordTraceStep("BUILD", $"Constructed overflow pile '{pileDef.pilePrefab}' ({requiredCount}x {pName}).");
                    VerificationTelemetry.RecordPass("logistics_overflow_piles", $"Worker '{brain.GetHoverName()}' built '{pileDef.pilePrefab}' when storage was full.");

                    constructedAny = true;
                    break; // Process one pile per cycle to allow smooth animation & positioning
                }
            }

            return constructedAny;
        }

        private static bool FindSuitablePileLocation(Vector3 center, out Vector3 bestPos)
        {
            bestPos = Vector3.zero;
            // Spiral search around camp center (3m to 12m)
            for (float r = 3.5f; r <= 12f; r += 1.8f)
            {
                for (int angle = 0; angle < 360; angle += 40)
                {
                    float rad = angle * Mathf.Deg2Rad;
                    Vector3 testPos = center + new Vector3(Mathf.Cos(rad) * r, 0f, Mathf.Sin(rad) * r);

                    if (Physics.Raycast(new Vector3(testPos.x, testPos.y + 2f, testPos.z), Vector3.down, out RaycastHit hit, 6f, LayerMask.GetMask("Default", "static_solid", "terrain")))
                    {
                        Vector3 groundPt = hit.point;
                        if (hit.normal.y < 0.82f) continue; // Too steep
                        if (ZoneSystem.instance != null && groundPt.y < ZoneSystem.instance.m_waterLevel + 0.3f) continue; // In water

                        // Check 1.2m clearance from existing colliders
                        var overlaps = Physics.OverlapSphere(groundPt + Vector3.up * 0.5f, 1.2f);
                        bool blocked = false;
                        for (int i = 0; i < overlaps.Length; i++)
                        {
                            if (!overlaps[i].isTrigger && overlaps[i].GetComponentInParent<Character>() == null)
                            {
                                blocked = true;
                                break;
                            }
                        }

                        if (!blocked)
                        {
                            bestPos = groundPt;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// [F32] Scans for signs near chests to match designated tags like [Wood], [Stone], [Ore], [Food], [Seeds].
        /// </summary>
        public static Container FindChestMatchingSign(Vector3 center, float radius, ItemDrop.ItemData item, WorkerBrain brain)
        {
            if (item == null) return null;
            string pName = WorkerBrain.GetItemPrefabName(item);
            string sharedName = item.m_shared?.m_name ?? "";

            Collider[] hits = Physics.OverlapSphere(center, radius);
            Container bestChest = null;
            float bestDist = float.MaxValue;

            for (int i = 0; i < hits.Length; i++)
            {
                var chest = hits[i].GetComponentInParent<Container>();
                if (chest == null || chest.IsInUse() || chest.GetComponent<WorkerBrain>() != null) continue;
                var cInv = chest.GetInventory();
                if (cInv == null || cInv.GetEmptySlots() == 0 && !InventoryHelper.HasMatchingItem(cInv, item)) continue;

                // Check for nearby Sign components within 2.2m
                var signs = chest.GetComponentsInChildren<Sign>();
                if (signs == null || signs.Length == 0)
                {
                    var nearbySigns = Physics.OverlapSphere(chest.transform.position, 2.2f);
                    foreach (var h in nearbySigns)
                    {
                        var s = h.GetComponentInParent<Sign>();
                        if (s != null && MatchesSignTag(s.GetText(), pName, sharedName))
                        {
                            float d = Vector3.Distance(brain.transform.position, chest.transform.position);
                            if (d < bestDist)
                            {
                                bestDist = d;
                                bestChest = chest;
                            }
                        }
                    }
                }
                else
                {
                    foreach (var s in signs)
                    {
                        if (MatchesSignTag(s.GetText(), pName, sharedName))
                        {
                            float d = Vector3.Distance(brain.transform.position, chest.transform.position);
                            if (d < bestDist)
                            {
                                bestDist = d;
                                bestChest = chest;
                            }
                        }
                    }
                }
            }

            return bestChest;
        }

        private static bool MatchesSignTag(string signText, string pName, string sharedName)
        {
            if (string.IsNullOrEmpty(signText)) return false;

            string s = signText.ToLowerInvariant();
            string p = pName.ToLowerInvariant();
            string sh = sharedName.ToLowerInvariant();

            if (s.Contains("[wood]") && (p.Contains("wood") || sh.Contains("wood"))) return true;
            if (s.Contains("[stone]") && (p.Contains("stone") || sh.Contains("stone") || p.Contains("flint"))) return true;
            if (s.Contains("[ore]") && (p.Contains("ore") || p.Contains("scrap") || p.Contains("copper") || p.Contains("tin") || p.Contains("iron") || p.Contains("silver"))) return true;
            if (s.Contains("[food]") && (p.Contains("meat") || p.Contains("cooked") || p.Contains("jam") || p.Contains("soup") || p.Contains("stew") || p.Contains("sausage") || p.Contains("bread") || p.Contains("pie"))) return true;
            if (s.Contains("[berries]") && (p.Contains("berry") || p.Contains("mushroom") || p.Contains("honey"))) return true;
            if (s.Contains("[seeds]") && (p.Contains("seed") || p.Contains("cone") || p.Contains("acorn"))) return true;
            if (s.Contains("[trophy]") && (p.Contains("trophy") || sh.Contains("trophy"))) return true;

            return false;
        }

        /// <summary>
        /// [F33] Finds nearby player Carts (Vagon) for mobile hauling.
        /// </summary>
        public static Container FindNearbyCartContainer(Vector3 center, float radius)
        {
            Collider[] hits = Physics.OverlapSphere(center, radius);
            for (int i = 0; i < hits.Length; i++)
            {
                var vagon = hits[i].GetComponentInParent<Vagon>();
                if (vagon != null)
                {
                    var container = vagon.GetComponentInChildren<Container>();
                    if (container != null && !container.IsInUse())
                    {
                        return container;
                    }
                }
            }
            return null;
        }
    }
}
