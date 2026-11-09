using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using ValheimWorkers.Core;

namespace ValheimWorkers.Prefabs
{
    public static class RelicPrefabManager
    {
        public const string PrefabSacrificialDagger = "VW_SacrificialDagger";
        public const string PrefabBloodChalice = "VW_BloodChalice";
        public const string PrefabAwakeningOil = "VW_AwakeningOil";
        public const string PrefabGjallarhorn = "VW_Gjallarhorn";
        public const string PrefabDvergrSeal = "VW_DvergrSeal";

        public const string CustomDataBloodKey = "VW_Blood";
        public const int MaxChaliceBlood = 100;

        private static readonly Dictionary<string, GameObject> _cachedRelics = new Dictionary<string, GameObject>(StringComparer.OrdinalIgnoreCase);

        public static void RegisterRelics(ZNetScene scene)
        {
            if (scene == null) return;
            var namedPrefabs = Traverse.Create(scene).Field<Dictionary<int, GameObject>>("m_namedPrefabs").Value;
            var container = WorkerPrefabManager.GetPrefabContainer();

            // 1. Sacrificial Dagger (Cloned from KnifeFlint / KnifeChitin)
            CreateOrRegisterItem(scene, namedPrefabs, container, PrefabSacrificialDagger, "KnifeFlint", item =>
            {
                item.m_itemData.m_shared.m_name = "Skarde's Sacrificial Dagger";
                item.m_itemData.m_shared.m_description = "Ancient sacrificial blade recovered from a hero's barrow. Strikes down tamed beasts in a single blow, spilling warm blood to animate dry bones.";
                item.m_itemData.m_shared.m_damages.m_slash = 22f;
                item.m_itemData.m_shared.m_damages.m_pierce = 18f;
                item.m_itemData.m_shared.m_weight = 1.0f;
                item.m_itemData.m_shared.m_maxDurability = 250f;
            });

            // 2. Blood Chalice (Cloned from Tankard / TankardAnniversary)
            CreateOrRegisterItem(scene, namedPrefabs, container, PrefabBloodChalice, "TankardAnniversary", item =>
            {
                item.m_itemData.m_shared.m_name = "Blood Chalice of Valka";
                item.m_itemData.m_shared.m_description = "Runic chalice that drinks the spilled blood of slain beasts and the Viking's own wounds. Stores warm blood to animate bones or nourish undead thralls.\n<color=#FF5555>Stored Blood: 0 / 100</color>";
                item.m_itemData.m_shared.m_maxStackSize = 1;
                item.m_itemData.m_shared.m_weight = 2.0f;
            }, "Tankard");

            // 3. Awakening Oil (Cloned from MeadTasty / MeadHealthMedium)
            CreateOrRegisterItem(scene, namedPrefabs, container, PrefabAwakeningOil, "MeadTasty", item =>
            {
                item.m_itemData.m_shared.m_name = "Draupnir Awakening Oil";
                item.m_itemData.m_shared.m_description = "Anointed honey-salve brewed from forest herbs and antler essence. When poured over an ancient standing stone, it awakens the petrified drengr sleeping within.";
                item.m_itemData.m_shared.m_maxStackSize = 10;
                item.m_itemData.m_shared.m_weight = 1.0f;
            }, "MeadHealthMedium");

            // 4. Gjallarhorn of the Hearth (Cloned from Horn / Tankard)
            CreateOrRegisterItem(scene, namedPrefabs, container, PrefabGjallarhorn, "TankardOdin", item =>
            {
                item.m_itemData.m_shared.m_name = "Gjallarhorn of the Hearth";
                item.m_itemData.m_shared.m_description = "Ancient carved war horn. When sounded inside a comfortable Jarl's mead-hall, it summons wandering Einherjar champions to pledge fealty.";
                item.m_itemData.m_shared.m_maxStackSize = 1;
                item.m_itemData.m_shared.m_weight = 2.0f;
            }, "Tankard");

            // 5. Grand Dvergr Guild Seal (Cloned from DvergrKey / Ruby)
            CreateOrRegisterItem(scene, namedPrefabs, container, PrefabDvergrSeal, "DvergrKey", item =>
            {
                item.m_itemData.m_shared.m_name = "Grand Dvergr Guild Seal";
                item.m_itemData.m_shared.m_description = "Heavy engraved bronze and ruby seal recovered from a troll cave. Binds the guild masters of Svartalfheim to honor contracts of stone and steel.";
                item.m_itemData.m_shared.m_maxStackSize = 5;
                item.m_itemData.m_shared.m_weight = 1.0f;
                item.m_itemData.m_shared.m_value = 150;
            }, "Ruby");
        }

        private static void CreateOrRegisterItem(ZNetScene scene, Dictionary<int, GameObject> namedPrefabs, GameObject container,
            string targetName, string baseName, Action<ItemDrop> configure, string fallbackBase = null)
        {
            if (_cachedRelics.TryGetValue(targetName, out var existing) && existing != null)
            {
                if (scene.GetPrefab(targetName) == null)
                {
                    scene.m_prefabs.Add(existing);
                    if (namedPrefabs != null) namedPrefabs[targetName.GetStableHashCode()] = existing;
                }
                return;
            }

            GameObject baseObj = scene.GetPrefab(baseName);
            if (baseObj == null && !string.IsNullOrEmpty(fallbackBase))
            {
                baseObj = scene.GetPrefab(fallbackBase);
            }
            if (baseObj == null)
            {
                ValheimWorkersPlugin.Log.LogWarning($"[RelicPrefabManager] Could not find base prefab '{baseName}' for '{targetName}'.");
                return;
            }

            try
            {
                GameObject clone = UnityEngine.Object.Instantiate(baseObj, container.transform);
                clone.name = targetName;

                var itemDrop = clone.GetComponent<ItemDrop>();
                if (itemDrop != null)
                {
                    configure?.Invoke(itemDrop);
                }

                int hash = targetName.GetStableHashCode();
                scene.m_prefabs.Add(clone);
                if (namedPrefabs != null) namedPrefabs[hash] = clone;
                _cachedRelics[targetName] = clone;

                ValheimWorkersPlugin.Log.LogInfo($"[RelicPrefabManager] Registered relic prefab '{targetName}' (cloned from '{baseObj.name}')");
            }
            catch (Exception ex)
            {
                ValheimWorkersPlugin.Log.LogError($"[RelicPrefabManager] Failed to create relic '{targetName}': {ex}");
            }
        }

        public static void RegisterObjectDbItems(ObjectDB odb)
        {
            if (odb == null || odb.m_items == null || odb.m_items.Count == 0) return;
            var itemByHash = Traverse.Create(odb).Field<Dictionary<int, GameObject>>("m_itemByHash").Value;

            foreach (var kvp in _cachedRelics)
            {
                if (kvp.Value == null) continue;
                if (!odb.m_items.Contains(kvp.Value))
                {
                    odb.m_items.Add(kvp.Value);
                }
                if (itemByHash != null)
                {
                    itemByHash[kvp.Key.GetStableHashCode()] = kvp.Value;
                }
            }
        }

        // --- Chalice Blood Helper Functions ---
        public static int GetChaliceBlood(ItemDrop.ItemData item)
        {
            if (item == null || item.m_customData == null) return 0;
            if (item.m_customData.TryGetValue(CustomDataBloodKey, out string str) && int.TryParse(str, out int blood))
            {
                return Mathf.Clamp(blood, 0, MaxChaliceBlood);
            }
            return 0;
        }

        public static void SetChaliceBlood(ItemDrop.ItemData item, int amount)
        {
            if (item == null) return;
            if (item.m_customData == null) item.m_customData = new Dictionary<string, string>();
            int blood = Mathf.Clamp(amount, 0, MaxChaliceBlood);
            item.m_customData[CustomDataBloodKey] = blood.ToString();

            // Update description tooltip
            string baseDesc = "Runic chalice that drinks the spilled blood of slain beasts and the Viking's own wounds. Stores warm blood to animate bones or nourish undead thralls.";
            item.m_shared.m_description = $"{baseDesc}\n<color=#FF5555><b>Stored Blood: {blood} / {MaxChaliceBlood}</b></color>";
        }

        public static int AddBloodToPlayerChalice(Player player, int bloodToAdd)
        {
            if (player == null || bloodToAdd <= 0) return 0;
            var inv = player.GetInventory();
            if (inv == null) return 0;

            foreach (var item in inv.GetAllItems())
            {
                if (item != null && item.m_dropPrefab != null && item.m_dropPrefab.name == PrefabBloodChalice)
                {
                    int current = GetChaliceBlood(item);
                    int space = MaxChaliceBlood - current;
                    if (space <= 0) continue;

                    int added = Mathf.Min(space, bloodToAdd);
                    SetChaliceBlood(item, current + added);
                    player.Message(MessageHud.MessageType.TopLeft, $"<color=#FF4444>The Blood Chalice drinks warm blood (+{added} Blood). [{current + added}/{MaxChaliceBlood}]</color>");
                    return added;
                }
            }
            return 0;
        }

        public static bool ConsumeBloodFromPlayerChalice(Player player, int bloodNeeded)
        {
            if (player == null || bloodNeeded <= 0) return false;
            var inv = player.GetInventory();
            if (inv == null) return false;

            foreach (var item in inv.GetAllItems())
            {
                if (item != null && item.m_dropPrefab != null && item.m_dropPrefab.name == PrefabBloodChalice)
                {
                    int current = GetChaliceBlood(item);
                    if (current >= bloodNeeded)
                    {
                        SetChaliceBlood(item, current - bloodNeeded);
                        player.Message(MessageHud.MessageType.TopLeft, $"Poured {bloodNeeded} Blood from Chalice. [{current - bloodNeeded}/{MaxChaliceBlood} remaining]");
                        return true;
                    }
                }
            }
            return false;
        }

        public static int GetTotalPlayerChaliceBlood(Player player)
        {
            if (player == null) return 0;
            var inv = player.GetInventory();
            if (inv == null) return 0;

            int total = 0;
            foreach (var item in inv.GetAllItems())
            {
                if (item != null && item.m_dropPrefab != null && item.m_dropPrefab.name == PrefabBloodChalice)
                {
                    total += GetChaliceBlood(item);
                }
            }
            return total;
        }
    }
}
