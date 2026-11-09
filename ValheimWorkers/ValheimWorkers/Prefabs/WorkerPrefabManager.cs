using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using ValheimWorkers.AI;
using ValheimWorkers.Core;

namespace ValheimWorkers.Prefabs
{
    public static class WorkerPrefabManager
    {
        private static readonly Dictionary<string, GameObject> _cachedWorkerPrefabs = new Dictionary<string, GameObject>(StringComparer.OrdinalIgnoreCase);
        private static GameObject _prefabContainer;

        public static GameObject GetPrefabContainer()
        {
            if (_prefabContainer == null)
            {
                _prefabContainer = new GameObject("ValheimWorkers_PrefabContainer");
                _prefabContainer.SetActive(false);
                UnityEngine.Object.DontDestroyOnLoad(_prefabContainer);
            }
            return _prefabContainer;
        }

        private static void CopyDeclaredFields(Type type, object source, object destination)
        {
            if (type == null || source == null || destination == null) return;
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            foreach (var field in fields)
            {
                if (field.IsInitOnly) continue;
                try
                {
                    field.SetValue(destination, field.GetValue(source));
                }
                catch
                {
                    // Ignore non-copyable internal fields
                }
            }
        }

        private static void ConvertPlayerCloneToNpcHumanoid(GameObject clone, ZNetScene scene)
        {
            Player oldPlayer = clone.GetComponent<Player>();
            if (oldPlayer == null) return;

            // Add Humanoid component and transfer all serialized Character & Humanoid fields from Player
            Humanoid humanoid = clone.AddComponent<Humanoid>();
            CopyDeclaredFields(typeof(Character), oldPlayer, humanoid);
            CopyDeclaredFields(typeof(Humanoid), oldPlayer, humanoid);

            // Remove Player-specific components before Awake ever runs
            UnityEngine.Object.DestroyImmediate(oldPlayer);

            var playerController = clone.GetComponent<PlayerController>();
            if (playerController != null) UnityEngine.Object.DestroyImmediate(playerController);

            var talker = clone.GetComponent<Talker>();
            if (talker != null) UnityEngine.Object.DestroyImmediate(talker);

            var skills = clone.GetComponent<Skills>();
            if (skills != null) UnityEngine.Object.DestroyImmediate(skills);

            // Keep VisEquipment.m_isPlayer = true so Male/Female body models, hair, beard, skin color & armor work,
            // while clearing the destroyed Player reference
            var visEq = clone.GetComponent<VisEquipment>();
            if (visEq != null)
            {
                visEq.m_isPlayer = true;
                Traverse.Create(visEq).Field("m_playerComponent").SetValue(null);
            }

            // Add Container component for 8x4 native player-style inventory storage & UI
            var container = clone.GetComponent<Container>();
            if (container == null)
            {
                container = clone.AddComponent<Container>();
            }
            container.m_width = 8;
            container.m_height = 4;
            container.m_name = "Worker Inventory";
            container.m_privacy = Container.PrivacySetting.Public;
            container.m_checkGuardStone = false;

            // Configure slope traversal physics & obstacle jumping
            humanoid.m_jumpForce = 9.5f;
            humanoid.m_jumpForceForward = 2.5f;

            // Add MonsterAI and copy pathfinding / perception settings from Dverger
            var monsterAI = clone.GetComponent<MonsterAI>();
            if (monsterAI == null)
            {
                monsterAI = clone.AddComponent<MonsterAI>();
                GameObject aiSourceObj = scene.GetPrefab("Dverger") ?? scene.GetPrefab("Skeleton_NoArcher");
                var templateAI = aiSourceObj != null ? aiSourceObj.GetComponent<MonsterAI>() : null;
                if (templateAI != null)
                {
                    CopyDeclaredFields(typeof(BaseAI), templateAI, monsterAI);
                    CopyDeclaredFields(typeof(MonsterAI), templateAI, monsterAI);
                    monsterAI.m_consumeItems = new List<ItemDrop>();
                    monsterAI.m_idleSound = new EffectList();
                    monsterAI.m_alertedEffects = new EffectList();
                }
            }
            monsterAI.m_jumpInterval = 1.2f;

            // Ensure cloned player prefab is persistent so workers are saved to the world file
            var nview = clone.GetComponent<ZNetView>();
            if (nview != null)
            {
                nview.m_persistent = true;
            }
        }

        public static void RegisterWorkerPrefabs(ZNetScene scene)
        {
            if (scene == null) return;

            var namedPrefabs = Traverse.Create(scene).Field<Dictionary<int, GameObject>>("m_namedPrefabs").Value;
            var container = GetPrefabContainer();

            foreach (var archetype in WorkerCatalog.AllArchetypes)
            {
                // If prefab was already cloned in an earlier world session, re-register into the new scene
                if (_cachedWorkerPrefabs.TryGetValue(archetype.Id, out var existingClone) && existingClone != null)
                {
                    if (scene.GetPrefab(archetype.Id) == null)
                    {
                        scene.m_prefabs.Add(existingClone);
                        if (namedPrefabs != null)
                        {
                            namedPrefabs[archetype.Id.GetStableHashCode()] = existingClone;
                        }
                    }
                    continue;
                }

                if (scene.GetPrefab(archetype.Id) != null)
                {
                    _cachedWorkerPrefabs[archetype.Id] = scene.GetPrefab(archetype.Id);
                    continue;
                }

                GameObject baseObj = scene.GetPrefab(archetype.BasePrefab);
                if (baseObj == null)
                {
                    baseObj = scene.GetPrefab(archetype.FallbackPrefab);
                }

                // If still null, try common fallback humanoid prefabs in Valheim
                if (baseObj == null)
                {
                    baseObj = scene.GetPrefab("Player") ?? scene.GetPrefab("Dverger") ?? scene.GetPrefab("Skeleton_NoArcher");
                }

                if (baseObj == null)
                {
                    ValheimWorkersPlugin.Log.LogWarning($"[ValheimWorkers] Could not find base prefab for archetype '{archetype.Id}'.");
                    continue;
                }

                try
                {
                    // Instantiate into the inactive container so unspawned templates never execute visual loops
                    GameObject clone = UnityEngine.Object.Instantiate(baseObj, container.transform);
                    clone.name = archetype.Id;

                    // If cloned from Player, convert Player component to Humanoid + MonsterAI
                    if (clone.GetComponent<Player>() != null)
                    {
                        ConvertPlayerCloneToNpcHumanoid(clone, scene);
                    }

                    // Configure Character & Humanoid and clear default monster equipment (e.g. Dvergr Crossbow)
                    var humanoid = clone.GetComponent<Humanoid>();
                    if (humanoid != null)
                    {
                        humanoid.m_name = archetype.DisplayName;
                        humanoid.m_faction = Character.Faction.Players;
                        humanoid.m_group = "players";
                        humanoid.m_boss = false;
                        humanoid.m_health = archetype.MaxHealth;
                        humanoid.m_speed = humanoid.m_speed * archetype.SpeedModifier;
                        humanoid.m_jumpForce = 9.5f;
                        humanoid.m_jumpForceForward = 2.5f;

                        humanoid.m_defaultItems = Array.Empty<GameObject>();
                        humanoid.m_randomWeapon = Array.Empty<GameObject>();
                        humanoid.m_randomArmor = Array.Empty<GameObject>();
                        humanoid.m_randomShield = Array.Empty<GameObject>();
                        humanoid.m_randomSets = Array.Empty<Humanoid.ItemSet>();
                        humanoid.m_randomItems = Array.Empty<Humanoid.RandomItem>();
                    }
                    else
                    {
                        var character = clone.GetComponent<Character>();
                        if (character != null)
                        {
                            character.m_name = archetype.DisplayName;
                            character.m_faction = Character.Faction.Players;
                            character.m_group = "players";
                            character.m_health = archetype.MaxHealth;
                            character.m_speed = character.m_speed * archetype.SpeedModifier;
                        }
                    }

                    // Remove vanilla Tameable if present to allow custom worker interaction & role cycling
                    var tameable = clone.GetComponent<Tameable>();
                    if (tameable != null)
                    {
                        UnityEngine.Object.DestroyImmediate(tameable);
                    }

                    // Ensure Container component is present on all archetypes for 8x4 inventory
                    var containerComp = clone.GetComponent<Container>();
                    if (containerComp == null)
                    {
                        containerComp = clone.AddComponent<Container>();
                    }
                    containerComp.m_width = 8;
                    containerComp.m_height = 4;
                    containerComp.m_name = archetype.DisplayName;
                    containerComp.m_privacy = Container.PrivacySetting.Public;
                    containerComp.m_checkGuardStone = false;

                    // Attach custom WorkerBrain
                    var brain = clone.GetComponent<WorkerBrain>();
                    if (brain == null)
                    {
                        brain = clone.AddComponent<WorkerBrain>();
                    }

                    // Ensure ZNetView is persistent so workers save to the world database
                    var nview = clone.GetComponent<ZNetView>();
                    if (nview != null)
                    {
                        nview.m_persistent = true;
                    }

                    // Register to ZNetScene collections
                    int hash = archetype.Id.GetStableHashCode();
                    scene.m_prefabs.Add(clone);
                    if (namedPrefabs != null)
                    {
                        namedPrefabs[hash] = clone;
                    }

                    _cachedWorkerPrefabs[archetype.Id] = clone;
                    ValheimWorkersPlugin.Log.LogInfo($"[ValheimWorkers] Successfully registered worker prefab: '{archetype.Id}' (Cloned from '{baseObj.name}')");
                }
                catch (Exception ex)
                {
                    ValheimWorkersPlugin.Log.LogError($"[ValheimWorkers] Failed to clone worker prefab '{archetype.Id}': {ex}");
                }
            }
        }

        public static GameObject SpawnWorker(string archetypeId, Vector3 position, Quaternion rotation, WorkerRole initialRole = WorkerRole.Hauler)
        {
            var arch = WorkerCatalog.Get(archetypeId);
            var config = new WorkerCustomization
            {
                Name = arch != null ? arch.DisplayName : "Viking Worker",
                BasePrefab = arch != null ? arch.Id : archetypeId,
                InitialRole = initialRole,
                MaxHealth = arch != null ? arch.MaxHealth : 150f,
                SpeedModifier = arch != null ? arch.SpeedModifier : 1.0f,
                Weapon = arch != null && !string.IsNullOrEmpty(arch.DefaultWeapon) ? arch.DefaultWeapon : "Club",
                Shield = arch != null ? (arch.DefaultShield ?? "") : "",
                Helmet = arch != null ? (arch.DefaultHelmet ?? "HelmetLeather") : "HelmetLeather",
                Chest = arch != null ? (arch.DefaultChest ?? "ArmorLeatherChest") : "ArmorLeatherChest",
                Legs = arch != null ? (arch.DefaultLegs ?? "ArmorLeatherLegs") : "ArmorLeatherLegs"
            };
            return SpawnCustomWorker(config, position, rotation);
        }

        public static GameObject SpawnCustomWorker(WorkerCustomization config, Vector3 position, Quaternion rotation)
        {
            if (ZNetScene.instance == null)
            {
                ValheimWorkersPlugin.Log.LogWarning("[ValheimWorkers] Cannot spawn worker: ZNetScene.instance is null.");
                return null;
            }

            if (config == null) config = new WorkerCustomization();

            // Find prefab
            string prefabName = config.BasePrefab;
            if (!prefabName.StartsWith("Worker_")) prefabName = "Worker_" + prefabName;

            GameObject prefab = ZNetScene.instance.GetPrefab(prefabName) 
                             ?? ZNetScene.instance.GetPrefab("Worker_Viking")
                             ?? ZNetScene.instance.GetPrefab("Worker_Peasant")
                             ?? ZNetScene.instance.GetPrefab("Worker_Dverger");

            if (prefab == null)
            {
                ValheimWorkersPlugin.Log.LogWarning($"[ValheimWorkers] Worker prefab '{prefabName}' not found in ZNetScene. Attempting on-demand registration fallback...");
                RegisterWorkerPrefabs(ZNetScene.instance);
                prefab = ZNetScene.instance.GetPrefab(prefabName) 
                      ?? ZNetScene.instance.GetPrefab("Worker_Viking")
                      ?? ZNetScene.instance.GetPrefab("Worker_Peasant")
                      ?? ZNetScene.instance.GetPrefab("Worker_Dverger");
            }

            if (prefab == null)
            {
                ValheimWorkersPlugin.Log.LogWarning($"[ValheimWorkers] Cannot spawn worker: no suitable worker prefab found in ZNetScene.");
                return null;
            }

            GameObject instance = UnityEngine.Object.Instantiate(prefab, position, rotation);
            if (!instance.activeSelf)
            {
                instance.SetActive(true);
            }
            var brain = instance.GetComponent<WorkerBrain>();
            if (brain != null)
            {
                brain.Archetype = WorkerCatalog.Get(config.BasePrefab) ?? WorkerCatalog.Get("Worker_Viking");
                brain.SetHome(position, false);
                brain.GiveInitialLoadout(config);
                brain.ApplyCustomization(config.Clone());
                if (!WorkerBrain.ActiveWorkers.Contains(brain))
                {
                    WorkerBrain.ActiveWorkers.Add(brain);
                }
                VerificationTelemetry.UpdateWorkerSnapshot(brain.CreateLiveSnapshot());
            }

            ValheimWorkersPlugin.Log.LogInfo($"[ValheimWorkers] Spawned custom worker '{config.Name}' (Prefab: {prefab.name}) with weapon '{config.Weapon}' and role {config.InitialRole} at {position}.");
            return instance;
        }
    }
}
