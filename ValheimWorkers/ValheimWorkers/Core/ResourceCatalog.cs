using System;
using System.Collections.Generic;
using UnityEngine;

namespace ValheimWorkers.Core
{
    public enum ResourceCategory
    {
        Forestry,
        Mining,
        Hunting,
        Foraging
    }

    public class GatherableResourceDef
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public ResourceCategory Category { get; set; }
        public string TargetPrefab { get; set; }
        public string RequiredToolType { get; set; } // "Axe", "Pickaxe", "Bow", "None"
        public int RequiredToolTier { get; set; } = 0; // 0 = Stone/Antler/Any, 1 = Bronze, 2 = Iron, 3 = Silver
        public string FallbackDescription { get; set; } // e.g. "Collecting branches and saplings"
        public string[] CompositeSubResources { get; set; } // e.g. Bronze -> CopperOre + TinOre
        public string MissingToolWarning { get; set; }
    }

    public static class ResourceCatalog
    {
        private static readonly Dictionary<string, GatherableResourceDef> _definitions = new Dictionary<string, GatherableResourceDef>(StringComparer.OrdinalIgnoreCase);

        static ResourceCatalog()
        {
            RegisterAll();
        }

        private static void Register(GatherableResourceDef def)
        {
            if (def == null || string.IsNullOrEmpty(def.Id)) return;
            _definitions[def.Id] = def;
        }

        private static void RegisterAll()
        {
            _definitions.Clear();

            // ==================== FORESTRY ====================
            Register(new GatherableResourceDef
            {
                Id = "Wood",
                DisplayName = "Standard Wood",
                Category = ResourceCategory.Forestry,
                TargetPrefab = "Wood",
                RequiredToolType = "Axe",
                RequiredToolTier = 0,
                FallbackDescription = "Collecting fallen branches & bashing saplings",
                MissingToolWarning = "No Axe present! Worker can only bash small saplings and collect branches."
            });

            Register(new GatherableResourceDef
            {
                Id = "CoreWood",
                DisplayName = "Core Wood (Pines)",
                Category = ResourceCategory.Forestry,
                TargetPrefab = "CoreWood",
                RequiredToolType = "Axe",
                RequiredToolTier = 0,
                MissingToolWarning = "Axe required to chop Pine trees for Core Wood."
            });

            Register(new GatherableResourceDef
            {
                Id = "FineWood",
                DisplayName = "Fine Wood (Birch/Oak)",
                Category = ResourceCategory.Forestry,
                TargetPrefab = "FineWood",
                RequiredToolType = "Axe",
                RequiredToolTier = 1, // Bronze Axe or better
                MissingToolWarning = "Bronze Axe (or better) required to fell Birch and Oak for Fine Wood."
            });

            // ==================== MINING & STONE ====================
            Register(new GatherableResourceDef
            {
                Id = "Stone",
                DisplayName = "Stone",
                Category = ResourceCategory.Mining,
                TargetPrefab = "Stone",
                RequiredToolType = "Pickaxe",
                RequiredToolTier = 0,
                FallbackDescription = "Collecting loose ground stones",
                MissingToolWarning = "No Pickaxe present! Worker will collect loose ground stones only."
            });

            Register(new GatherableResourceDef
            {
                Id = "Flint",
                DisplayName = "Flint",
                Category = ResourceCategory.Mining,
                TargetPrefab = "Flint",
                RequiredToolType = "None",
                RequiredToolTier = 0,
                FallbackDescription = "Scouring coastlines for loose flint"
            });

            Register(new GatherableResourceDef
            {
                Id = "CopperOre",
                DisplayName = "Copper Ore",
                Category = ResourceCategory.Mining,
                TargetPrefab = "CopperOre",
                RequiredToolType = "Pickaxe",
                RequiredToolTier = 0, // Antler Pickaxe or better
                MissingToolWarning = "Antler Pickaxe (or better) required to mine Copper deposits."
            });

            Register(new GatherableResourceDef
            {
                Id = "TinOre",
                DisplayName = "Tin Ore",
                Category = ResourceCategory.Mining,
                TargetPrefab = "TinOre",
                RequiredToolType = "Pickaxe",
                RequiredToolTier = 0, // Antler Pickaxe or better
                MissingToolWarning = "Antler Pickaxe (or better) required to mine Tin deposits."
            });

            Register(new GatherableResourceDef
            {
                Id = "Bronze",
                DisplayName = "Bronze (Copper + Tin)",
                Category = ResourceCategory.Mining,
                TargetPrefab = "Bronze",
                RequiredToolType = "Pickaxe",
                RequiredToolTier = 0,
                CompositeSubResources = new[] { "CopperOre", "TinOre" },
                MissingToolWarning = "Antler Pickaxe (or better) required to mine Copper & Tin for Bronze."
            });

            Register(new GatherableResourceDef
            {
                Id = "IronScrap",
                DisplayName = "Iron Scrap",
                Category = ResourceCategory.Mining,
                TargetPrefab = "IronScrap",
                RequiredToolType = "Pickaxe",
                RequiredToolTier = 1, // Antler or Bronze Pickaxe
                MissingToolWarning = "Bronze Pickaxe (or better) required to mine muddy scrap piles."
            });

            Register(new GatherableResourceDef
            {
                Id = "SilverOre",
                DisplayName = "Silver Ore",
                Category = ResourceCategory.Mining,
                TargetPrefab = "SilverOre",
                RequiredToolType = "Pickaxe",
                RequiredToolTier = 2, // Iron Pickaxe
                MissingToolWarning = "Iron Pickaxe (or better) required to mine Silver veins."
            });

            Register(new GatherableResourceDef
            {
                Id = "Obsidian",
                DisplayName = "Obsidian",
                Category = ResourceCategory.Mining,
                TargetPrefab = "Obsidian",
                RequiredToolType = "Pickaxe",
                RequiredToolTier = 2, // Iron Pickaxe
                MissingToolWarning = "Iron Pickaxe (or better) required to mine Obsidian."
            });

            // ==================== HUNTING ====================
            Register(new GatherableResourceDef
            {
                Id = "RawMeat",
                DisplayName = "Game Meat & Hides",
                Category = ResourceCategory.Hunting,
                TargetPrefab = "RawMeat",
                RequiredToolType = "Bow",
                RequiredToolTier = 0,
                FallbackDescription = "Hunting boars/necks with melee weapon",
                MissingToolWarning = "Bow & Arrows recommended to hunt skittish Deer."
            });

            Register(new GatherableResourceDef
            {
                Id = "DeerHide",
                DisplayName = "Deer Hide",
                Category = ResourceCategory.Hunting,
                TargetPrefab = "DeerHide",
                RequiredToolType = "Bow",
                RequiredToolTier = 0,
                MissingToolWarning = "Bow & Arrows required to hunt Deer."
            });

            Register(new GatherableResourceDef
            {
                Id = "LeatherScraps",
                DisplayName = "Leather Scraps (Boars)",
                Category = ResourceCategory.Hunting,
                TargetPrefab = "LeatherScraps",
                RequiredToolType = "None", // Melee or bow
                RequiredToolTier = 0
            });

            Register(new GatherableResourceDef
            {
                Id = "NeckTail",
                DisplayName = "Neck Tails",
                Category = ResourceCategory.Hunting,
                TargetPrefab = "NeckTail",
                RequiredToolType = "None",
                RequiredToolTier = 0
            });

            Register(new GatherableResourceDef
            {
                Id = "Feathers",
                DisplayName = "Feathers (Birds)",
                Category = ResourceCategory.Hunting,
                TargetPrefab = "Feathers",
                RequiredToolType = "Bow",
                RequiredToolTier = 0,
                MissingToolWarning = "Bow & Arrows required to hunt birds for Feathers."
            });

            // ==================== FORAGING ====================
            Register(new GatherableResourceDef
            {
                Id = "Raspberry",
                DisplayName = "Raspberries",
                Category = ResourceCategory.Foraging,
                TargetPrefab = "Raspberry",
                RequiredToolType = "None"
            });

            Register(new GatherableResourceDef
            {
                Id = "Blueberries",
                DisplayName = "Blueberries",
                Category = ResourceCategory.Foraging,
                TargetPrefab = "Blueberries",
                RequiredToolType = "None"
            });

            Register(new GatherableResourceDef
            {
                Id = "Mushroom",
                DisplayName = "Red Mushrooms",
                Category = ResourceCategory.Foraging,
                TargetPrefab = "Mushroom",
                RequiredToolType = "None"
            });

            Register(new GatherableResourceDef
            {
                Id = "MushroomYellow",
                DisplayName = "Yellow Mushrooms",
                Category = ResourceCategory.Foraging,
                TargetPrefab = "MushroomYellow",
                RequiredToolType = "None"
            });

            Register(new GatherableResourceDef
            {
                Id = "Thistle",
                DisplayName = "Thistle",
                Category = ResourceCategory.Foraging,
                TargetPrefab = "Thistle",
                RequiredToolType = "None"
            });

            Register(new GatherableResourceDef
            {
                Id = "Dandelion",
                DisplayName = "Dandelions",
                Category = ResourceCategory.Foraging,
                TargetPrefab = "Dandelion",
                RequiredToolType = "None"
            });

            Register(new GatherableResourceDef
            {
                Id = "Cloudberry",
                DisplayName = "Cloudberries",
                Category = ResourceCategory.Foraging,
                TargetPrefab = "Cloudberry",
                RequiredToolType = "None"
            });
        }

        public static GatherableResourceDef Get(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            _definitions.TryGetValue(id, out var def);
            return def;
        }

        public static IEnumerable<GatherableResourceDef> GetAll() => _definitions.Values;

        public static string GetItemSharedName(string prefabName)
        {
            if (string.IsNullOrEmpty(prefabName)) return "";
            if (ObjectDB.instance != null)
            {
                var prefab = ObjectDB.instance.GetItemPrefab(prefabName);
                if (prefab != null)
                {
                    var drop = prefab.GetComponent<ItemDrop>();
                    if (drop != null && drop.m_itemData?.m_shared != null)
                    {
                        return drop.m_itemData.m_shared.m_name;
                    }
                }
            }
            return "$" + prefabName.ToLowerInvariant();
        }

        private static readonly HashSet<string> StarterResources = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Wood", "Stone", "Flint", "RawMeat", "BoarMeat", "Raspberry", "Mushroom", "LeatherScraps", "DeerHide", "NeckTail", "Resin", "Feathers"
        };

        public static bool IsKnownByPlayer(string resourceId, bool sandboxBypass = false)
        {
            if (sandboxBypass) return true;
            if (Player.m_localPlayer == null) return true;

            var def = Get(resourceId);
            if (def == null) return false;

            string target = def.TargetPrefab ?? resourceId;

            // Starter resources are known by every Viking from the beginning
            if (StarterResources.Contains(resourceId) || StarterResources.Contains(target))
            {
                return true;
            }

            // In Valheim, materials known are stored by shared name (e.g. "$item_copper_ore")
            string sharedName = GetItemSharedName(target);
            if (!string.IsNullOrEmpty(sharedName) && Player.m_localPlayer.IsMaterialKnown(sharedName))
            {
                return true;
            }

            if (Player.m_localPlayer.IsMaterialKnown(target) || Player.m_localPlayer.IsMaterialKnown(resourceId))
            {
                return true;
            }

            var inv = Player.m_localPlayer.GetInventory();
            if (inv != null)
            {
                foreach (var item in inv.GetAllItems())
                {
                    if (item == null) continue;
                    string pName = item.m_dropPrefab != null ? item.m_dropPrefab.name : "";
                    if (string.Equals(pName, target, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(pName, resourceId, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(item.m_shared?.m_name, sharedName, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(item.m_shared?.m_name, target, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
