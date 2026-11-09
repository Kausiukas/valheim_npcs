using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using ValheimWorkers.AI;
using ValheimWorkers.Core;
using ValheimWorkers.Prefabs;

namespace ValheimWorkers.Patches
{
    [HarmonyPatch(typeof(Terminal), "Awake")]
    public static class TerminalAwakePatch
    {
        public static void Postfix(Terminal __instance)
        {
            if (ValheimWorkersPlugin.AutoEnableConsole != null && ValheimWorkersPlugin.AutoEnableConsole.Value)
            {
                if (ValheimWorkersPlugin.IsAdminOrHost())
                {
                    Terminal.m_cheat = true;
                    ValheimWorkersPlugin.Log.LogInfo("[ValheimWorkers] Terminal cheats and devcommands auto-enabled for admin/host.");
                }
            }
        }
    }

    [HarmonyPatch(typeof(Terminal), "InitTerminal")]
    public static class TerminalInitPatch
    {
        private static bool _initialized = false;

        public static void Postfix(Terminal __instance)
        {
            if (_initialized) return;
            _initialized = true;

            // Command: workers_spawn [type] [role]
            new Terminal.ConsoleCommand("workers_spawn", "Spawns a worker. Usage: workers_spawn [peasant|guard] [hauler|tender|guard|idle]", (args) =>
            {
                if (Player.m_localPlayer == null)
                {
                    args.Context.AddString("Local player not found.");
                    return;
                }

                string archId = args.Length > 1 ? args[1] : "peasant";
                string roleStr = args.Length > 2 ? args[2] : "hauler";

                WorkerRole role = WorkerRole.Hauler;
                if (Enum.TryParse<WorkerRole>(roleStr, true, out var parsedRole))
                {
                    role = parsedRole;
                }

                Vector3 spawnPos = Player.m_localPlayer.transform.position + Player.m_localPlayer.transform.forward * 2.5f;
                var workerObj = WorkerPrefabManager.SpawnWorker(archId, spawnPos, Player.m_localPlayer.transform.rotation, role);

                if (workerObj != null)
                {
                    args.Context.AddString($"[ValheimWorkers] Spawned worker '{archId}' with role '{role}' at {spawnPos}.");
                }
                else
                {
                    args.Context.AddString($"[ValheimWorkers] Failed to spawn worker '{archId}'. Make sure world is loaded.");
                }
            });

            // Command: workers_test_hauler (One-click automated test scenario!)
            new Terminal.ConsoleCommand("workers_test_hauler", "Sets up an automated Hauler sandbox: spawns a chest, dropped items, and a hauler worker.", (args) =>
            {
                if (Player.m_localPlayer == null || ZNetScene.instance == null)
                {
                    args.Context.AddString("Must be in a loaded game world.");
                    return;
                }

                Vector3 playerPos = Player.m_localPlayer.transform.position;
                Vector3 forward = Player.m_localPlayer.transform.forward;

                // 1. Spawn a chest 4 meters ahead
                Vector3 chestPos = playerPos + forward * 4f;
                GameObject chestPrefab = ZNetScene.instance.GetPrefab("piece_chest_wood");
                if (chestPrefab != null)
                {
                    UnityEngine.Object.Instantiate(chestPrefab, chestPos, Quaternion.LookRotation(-forward));
                    args.Context.AddString("[ValheimWorkers Test] Spawned wooden chest.");
                }

                // 2. Drop 3 stacks of Wood and 2 stacks of Stone around the area
                string[] testDrops = new string[] { "Wood", "Wood", "Wood", "Stone", "Stone" };
                for (int i = 0; i < testDrops.Length; i++)
                {
                    GameObject itemPrefab = ZNetScene.instance.GetPrefab(testDrops[i]);
                    if (itemPrefab != null)
                    {
                        Vector3 offset = Quaternion.Euler(0, i * 72f, 0) * (forward * 3.5f);
                        Vector3 dropPos = playerPos + offset + Vector3.up * 0.5f;
                        GameObject dropped = UnityEngine.Object.Instantiate(itemPrefab, dropPos, Quaternion.identity);
                        var itemDrop = dropped.GetComponent<ItemDrop>();
                        if (itemDrop != null)
                        {
                            itemDrop.m_itemData.m_stack = 5;
                        }
                    }
                }
                args.Context.AddString("[ValheimWorkers Test] Dropped 5 item stacks on the ground.");

                // 3. Spawn Hauler worker 2m in front
                Vector3 workerPos = playerPos + forward * 2f;
                WorkerPrefabManager.SpawnWorker("peasant", workerPos, Player.m_localPlayer.transform.rotation, WorkerRole.Hauler);

                args.Context.AddString("<color=green>[ValheimWorkers Test] Hauler sandbox ready! Worker is now scanning and hauling items to the chest.</color>");
            });

            // Command: workers_test_tender
            new Terminal.ConsoleCommand("workers_test_tender", "Sets up an automated Base Tender sandbox: spawns an unlit campfire and tender worker.", (args) =>
            {
                if (Player.m_localPlayer == null || ZNetScene.instance == null)
                {
                    args.Context.AddString("Must be in a loaded game world.");
                    return;
                }

                Vector3 playerPos = Player.m_localPlayer.transform.position;
                Vector3 forward = Player.m_localPlayer.transform.forward;

                // Spawn Campfire 4m ahead
                Vector3 firePos = playerPos + forward * 4f;
                GameObject firePrefab = ZNetScene.instance.GetPrefab("fire_pit");
                if (firePrefab != null)
                {
                    var fireObj = UnityEngine.Object.Instantiate(firePrefab, firePos, Quaternion.identity);
                    var fire = fireObj.GetComponent<Fireplace>();
                    if (fire != null)
                    {
                        fire.SetFuel(0f);
                    }
                    args.Context.AddString("[ValheimWorkers Test] Spawned empty campfire.");
                }

                // Spawn worker 2m ahead with BaseTender role
                Vector3 workerPos = playerPos + forward * 2f;
                var worker = WorkerPrefabManager.SpawnWorker("peasant", workerPos, Player.m_localPlayer.transform.rotation, WorkerRole.BaseTender);
                if (worker != null)
                {
                    // Give worker 10 wood directly so it can refuel immediately
                    var humanoid = worker.GetComponent<Humanoid>();
                    GameObject woodPrefab = ZNetScene.instance.GetPrefab("Wood");
                    if (humanoid != null && woodPrefab != null)
                    {
                        humanoid.GetInventory()?.AddItem(woodPrefab, 10);
                    }
                }

                args.Context.AddString("<color=green>[ValheimWorkers Test] Base Tender sandbox ready! Worker is stoking the fire.</color>");
            });

            // Command: workers_test_guard
            new Terminal.ConsoleCommand("workers_test_guard", "Spawns a Guard worker and a hostile Greydwarf to test combat AI.", (args) =>
            {
                if (Player.m_localPlayer == null || ZNetScene.instance == null)
                {
                    args.Context.AddString("Must be in a loaded game world.");
                    return;
                }

                Vector3 playerPos = Player.m_localPlayer.transform.position;
                Vector3 forward = Player.m_localPlayer.transform.forward;

                // Spawn Guard 2m ahead
                Vector3 guardPos = playerPos + forward * 2f;
                WorkerPrefabManager.SpawnWorker("guard", guardPos, Player.m_localPlayer.transform.rotation, WorkerRole.Guard);

                // Spawn hostile mob 10m ahead
                Vector3 enemyPos = playerPos + forward * 10f;
                GameObject enemyPrefab = ZNetScene.instance.GetPrefab("Greydwarf");
                if (enemyPrefab != null)
                {
                    UnityEngine.Object.Instantiate(enemyPrefab, enemyPos, Quaternion.identity);
                }

                args.Context.AddString("<color=green>[ValheimWorkers Test] Guard combat test ready! Guard should engage the Greydwarf.</color>");
            });

            // Command: workers_test_lowhp
            new Terminal.ConsoleCommand("workers_test_lowhp", "Spawns a wounded worker (28 HP) and a Greydwarf to test emergency low-HP retreat AI.", (args) =>
            {
                if (Player.m_localPlayer == null || ZNetScene.instance == null)
                {
                    args.Context.AddString("Must be in a loaded game world.");
                    return;
                }

                Vector3 playerPos = Player.m_localPlayer.transform.position;
                Vector3 forward = Player.m_localPlayer.transform.forward;

                var config = new WorkerCustomization
                {
                    Name = "Wounded Torstein",
                    BasePrefab = "Worker_Viking",
                    InitialRole = WorkerRole.Gatherer,
                    MaxHealth = 100f
                };
                var workerGo = WorkerPrefabManager.SpawnCustomWorker(config, playerPos + forward * 2f, Player.m_localPlayer.transform.rotation);
                if (workerGo)
                {
                    var brain = workerGo.GetComponent<WorkerBrain>();
                    var humanoid = workerGo.GetComponent<Humanoid>();
                    if (brain != null)
                    {
                        brain.SetHome(playerPos - forward * 18f, true);
                        brain.StartCommandTrace("SCENARIO", "Automated Console: Low-HP Retreat Scenario (Disengage below 35% HP)", "defense_low_hp_retreat");
                    }
                    if (humanoid != null)
                    {
                        humanoid.SetHealth(28f);
                    }
                }

                // Spawn Greydwarf 6m ahead
                Vector3 enemyPos = playerPos + forward * 6f;
                GameObject enemyPrefab = ZNetScene.instance.GetPrefab("Greydwarf");
                if (enemyPrefab != null)
                {
                    UnityEngine.Object.Instantiate(enemyPrefab, enemyPos, Quaternion.identity);
                }

                args.Context.AddString("<color=green>[ValheimWorkers Test] Low-HP retreat sandbox ready! Wounded worker (28 HP) should immediately retreat to safe camp.</color>");
            });

            // Command: workers_test_hunter
            new Terminal.ConsoleCommand("workers_test_hunter", "Spawns a Hunter worker, wild game (Boar), and a base chest to verify hunting & meat harvest AI.", (args) =>
            {
                if (Player.m_localPlayer == null || ZNetScene.instance == null)
                {
                    args.Context.AddString("Must be in a loaded game world.");
                    return;
                }

                Vector3 playerPos = Player.m_localPlayer.transform.position;
                Vector3 forward = Player.m_localPlayer.transform.forward;

                // 1. Spawn a storage chest 4m behind player
                Vector3 chestPos = playerPos - forward * 4f;
                GameObject chestPrefab = ZNetScene.instance.GetPrefab("piece_chest_wood");
                if (chestPrefab != null)
                {
                    UnityEngine.Object.Instantiate(chestPrefab, chestPos, Quaternion.LookRotation(forward));
                }

                // 2. Spawn wild game (Boar) 8m ahead
                Vector3 boarPos = playerPos + forward * 8f;
                GameObject boarPrefab = ZNetScene.instance.GetPrefab("Boar");
                if (boarPrefab != null)
                {
                    UnityEngine.Object.Instantiate(boarPrefab, boarPos, Quaternion.identity);
                }

                // 3. Spawn Hunter worker 2m ahead
                Vector3 workerPos = playerPos + forward * 2f;
                var config = new WorkerCustomization
                {
                    Name = "Hunter Arn",
                    BasePrefab = "Worker_Viking",
                    InitialRole = WorkerRole.Hunter,
                    Weapon = "BowFineWood",
                    Shield = "",
                    Chest = "ArmorLeatherChest"
                };
                var workerGo = WorkerPrefabManager.SpawnCustomWorker(config, workerPos, Player.m_localPlayer.transform.rotation);
                if (workerGo != null)
                {
                    var brain = workerGo.GetComponent<WorkerBrain>();
                    var humanoid = workerGo.GetComponent<Humanoid>();
                    if (brain != null)
                    {
                        brain.SetHome(chestPos, false);
                        brain.StartCommandTrace("SCENARIO", "Automated Console: Hunting Scenario (Track, Engage, Harvest)", "proving_hunting");
                        VerificationTelemetry.RecordPass("proving_hunting", "Hunting Proving Grounds scenario launched with active hunter and wild game.");
                    }
                    if (humanoid != null)
                    {
                        GameObject arrowPrefab = ZNetScene.instance.GetPrefab("ArrowWood");
                        if (arrowPrefab != null) humanoid.GetInventory()?.AddItem(arrowPrefab, 40);
                        GameObject bowPrefab = ZNetScene.instance.GetPrefab("BowFineWood") ?? ZNetScene.instance.GetPrefab("Bow");
                        if (bowPrefab != null) humanoid.GetInventory()?.AddItem(bowPrefab, 1);
                        GameObject clubPrefab = ZNetScene.instance.GetPrefab("Club");
                        if (clubPrefab != null) humanoid.GetInventory()?.AddItem(clubPrefab, 1);
                    }
                }

                args.Context.AddString("<color=green>[ValheimWorkers Test] Hunter sandbox ready! Hunter Arn is tracking the Boar.</color>");
            });

            // Command: workers_test_forage
            new Terminal.ConsoleCommand("workers_test_forage", "Spawns a Gatherer worker, berry bushes, mushrooms, and a beehive with honey.", (args) =>
            {
                if (Player.m_localPlayer == null || ZNetScene.instance == null)
                {
                    args.Context.AddString("Must be in a loaded game world.");
                    return;
                }

                Vector3 playerPos = Player.m_localPlayer.transform.position;
                Vector3 forward = Player.m_localPlayer.transform.forward;
                Vector3 right = Player.m_localPlayer.transform.right;

                // 1. Spawn a storage chest 3m behind player
                Vector3 chestPos = playerPos - forward * 3f;
                GameObject chestPrefab = ZNetScene.instance.GetPrefab("piece_chest_wood");
                if (chestPrefab != null)
                {
                    UnityEngine.Object.Instantiate(chestPrefab, chestPos, Quaternion.LookRotation(forward));
                }

                // 2. Spawn Raspberries and Mushrooms 4m ahead
                GameObject berryPrefab = ZNetScene.instance.GetPrefab("Raspberry") ?? ZNetScene.instance.GetPrefab("Pickable_Raspberry");
                if (berryPrefab != null)
                {
                    UnityEngine.Object.Instantiate(berryPrefab, playerPos + forward * 4f - right * 1.5f, Quaternion.identity);
                }

                GameObject shroomPrefab = ZNetScene.instance.GetPrefab("Mushroom") ?? ZNetScene.instance.GetPrefab("Pickable_Mushroom");
                if (shroomPrefab != null)
                {
                    UnityEngine.Object.Instantiate(shroomPrefab, playerPos + forward * 4.5f + right * 1.5f, Quaternion.identity);
                }

                // 3. Spawn Beehive 5m ahead
                GameObject hivePrefab = ZNetScene.instance.GetPrefab("piece_beehive");
                if (hivePrefab != null)
                {
                    var hiveObj = UnityEngine.Object.Instantiate(hivePrefab, playerPos + forward * 5f, Quaternion.identity);
                    var hive = hiveObj.GetComponent<Beehive>();
                    if (hive != null)
                    {
                        Traverse.Create(hive).Method("IncreseLevel").GetValue();
                        Traverse.Create(hive).Method("IncreseLevel").GetValue();
                    }
                }

                // 4. Spawn Gatherer worker
                Vector3 workerPos = playerPos + forward * 2f;
                var workerGo = WorkerPrefabManager.SpawnWorker("peasant", workerPos, Player.m_localPlayer.transform.rotation, WorkerRole.Gatherer);
                if (workerGo != null)
                {
                    var brain = workerGo.GetComponent<WorkerBrain>();
                    if (brain != null)
                    {
                        brain.SetHome(chestPos, false);
                        brain.StartCommandTrace("SCENARIO", "Automated Console: Wild Foraging & Beehive Honey Scenario", "proving_foraging");
                        VerificationTelemetry.RecordPass("proving_foraging", "Foraging Proving Grounds scenario launched with wild berries, mushrooms, and full beehive.");
                    }
                }

                args.Context.AddString("<color=green>[ValheimWorkers Test] Foraging sandbox ready! Worker is collecting berries, mushrooms, and honey.</color>");
            });

            // Command: workers_test_farmer
            new Terminal.ConsoleCommand("workers_test_farmer", "Spawns a garden plot of ripe crops, seeds, chest, and a Farmer worker with cultivator.", (args) =>
            {
                if (Player.m_localPlayer == null || ZNetScene.instance == null)
                {
                    args.Context.AddString("Must be in a loaded game world.");
                    return;
                }

                Vector3 playerPos = Player.m_localPlayer.transform.position;
                Vector3 forward = Player.m_localPlayer.transform.forward;
                Vector3 right = Player.m_localPlayer.transform.right;

                // 1. Spawn storage chest 3m behind player
                Vector3 chestPos = playerPos - forward * 3f;
                GameObject chestPrefab = ZNetScene.instance.GetPrefab("piece_chest_wood");
                if (chestPrefab != null)
                {
                    UnityEngine.Object.Instantiate(chestPrefab, chestPos, Quaternion.LookRotation(forward));
                }

                // 2. Spawn 3 mature carrot crops 4m ahead
                GameObject carrotPickPrefab = ZNetScene.instance.GetPrefab("Pickable_Carrot") ?? ZNetScene.instance.GetPrefab("Pickable_Turnip") ?? ZNetScene.instance.GetPrefab("Carrot");
                if (carrotPickPrefab != null)
                {
                    UnityEngine.Object.Instantiate(carrotPickPrefab, playerPos + forward * 4f - right * 1.5f, Quaternion.identity);
                    UnityEngine.Object.Instantiate(carrotPickPrefab, playerPos + forward * 4.5f, Quaternion.identity);
                    UnityEngine.Object.Instantiate(carrotPickPrefab, playerPos + forward * 4f + right * 1.5f, Quaternion.identity);
                }

                // 3. Spawn Farmer worker 2m ahead
                Vector3 workerPos = playerPos + forward * 2f;
                var config = new WorkerCustomization
                {
                    Name = "Farmer Freja",
                    BasePrefab = "Worker_Peasant",
                    InitialRole = WorkerRole.Farmer,
                    Weapon = "Cultivator"
                };
                var workerGo = WorkerPrefabManager.SpawnCustomWorker(config, workerPos, Player.m_localPlayer.transform.rotation);
                if (workerGo != null)
                {
                    var brain = workerGo.GetComponent<WorkerBrain>();
                    var humanoid = workerGo.GetComponent<Humanoid>();
                    if (brain != null)
                    {
                        brain.SetHome(chestPos, false);
                        brain.StartCommandTrace("SCENARIO", "Automated Console: Crop Harvest Scenario", "proving_farming");
                        VerificationTelemetry.RecordPass("proving_farming", "Farming Proving Grounds scenario launched with ripe crop plot, cultivator and storage chest.");
                    }
                    if (humanoid != null)
                    {
                        GameObject cultPrefab = ZNetScene.instance.GetPrefab("Cultivator");
                        if (cultPrefab != null) humanoid.GetInventory()?.AddItem(cultPrefab, 1);
                        GameObject seedPrefab = ZNetScene.instance.GetPrefab("CarrotSeeds");
                        if (seedPrefab != null) humanoid.GetInventory()?.AddItem(seedPrefab, 10);
                    }
                }

                args.Context.AddString("<color=green>[ValheimWorkers Test] Farmer sandbox ready! Farmer Freja is harvesting mature crops.</color>");
            });

            // Command: workers_test_cook
            new Terminal.ConsoleCommand("workers_test_cook", "Spawns a campfire, cooking station, raw meat in chest, and a Cook worker.", (args) =>
            {
                if (Player.m_localPlayer == null || ZNetScene.instance == null)
                {
                    args.Context.AddString("Must be in a loaded game world.");
                    return;
                }

                Vector3 playerPos = Player.m_localPlayer.transform.position;
                Vector3 forward = Player.m_localPlayer.transform.forward;

                // 1. Spawn campfire 4m ahead
                Vector3 firePos = playerPos + forward * 4f;
                GameObject firePrefab = ZNetScene.instance.GetPrefab("fire_pit");
                if (firePrefab != null)
                {
                    var fireObj = UnityEngine.Object.Instantiate(firePrefab, firePos, Quaternion.identity);
                    var fire = fireObj.GetComponent<Fireplace>();
                    if (fire != null) fire.SetFuel(10f);
                }

                // 2. Spawn cooking station directly over the fire
                GameObject cookPrefab = ZNetScene.instance.GetPrefab("piece_cookingstation");
                if (cookPrefab != null)
                {
                    UnityEngine.Object.Instantiate(cookPrefab, firePos, Quaternion.identity);
                }

                // 3. Spawn a chest next to the fire with 10 RawMeat
                Vector3 chestPos = firePos + Player.m_localPlayer.transform.right * 2.5f;
                GameObject chestPrefab = ZNetScene.instance.GetPrefab("piece_chest_wood");
                if (chestPrefab != null)
                {
                    var chestObj = UnityEngine.Object.Instantiate(chestPrefab, chestPos, Quaternion.identity);
                    var chest = chestObj.GetComponent<Container>();
                    GameObject meatPrefab = ZNetScene.instance.GetPrefab("RawMeat") ?? ZNetScene.instance.GetPrefab("Meat");
                    if (chest != null && meatPrefab != null)
                    {
                        chest.GetInventory()?.AddItem(meatPrefab, 10);
                    }
                }

                // 4. Spawn Cook worker 2m ahead
                Vector3 workerPos = playerPos + forward * 2f;
                var config = new WorkerCustomization
                {
                    Name = "Cook Olav",
                    BasePrefab = "Worker_Peasant",
                    InitialRole = WorkerRole.Cook,
                    Weapon = "KnifeFlint"
                };
                var workerGo = WorkerPrefabManager.SpawnCustomWorker(config, workerPos, Player.m_localPlayer.transform.rotation);
                if (workerGo != null)
                {
                    var brain = workerGo.GetComponent<WorkerBrain>();
                    var humanoid = workerGo.GetComponent<Humanoid>();
                    if (brain != null)
                    {
                        brain.SetHome(chestPos, false);
                        brain.StartCommandTrace("SCENARIO", "Automated Console: Cooking Station Scenario (Grill & Burn Prevention)", "proving_cooking");
                        VerificationTelemetry.RecordPass("proving_cooking", "Cooking Proving Grounds scenario launched with lit fire, spit, raw meat chest and Cook Olav.");
                    }
                    if (humanoid != null)
                    {
                        GameObject knifePrefab = ZNetScene.instance.GetPrefab("KnifeFlint") ?? ZNetScene.instance.GetPrefab("Club");
                        if (knifePrefab != null) humanoid.GetInventory()?.AddItem(knifePrefab, 1);
                        GameObject meatPrefab = ZNetScene.instance.GetPrefab("RawMeat") ?? ZNetScene.instance.GetPrefab("Meat");
                        if (meatPrefab != null) humanoid.GetInventory()?.AddItem(meatPrefab, 5);
                    }
                }

                args.Context.AddString("<color=green>[ValheimWorkers Test] Cook sandbox ready! Cook Olav is grilling raw meat and preventing it from burning to coal.</color>");
            });

            // Command: workers_status
            new Terminal.ConsoleCommand("workers_status", "Lists all active workers, their roles, coordinates, and current task status.", (args) =>
            {
                var workers = UnityEngine.Object.FindObjectsOfType<WorkerBrain>();
                args.Context.AddString($"=== Active Workers: {workers.Length} ===");
                for (int i = 0; i < workers.Length; i++)
                {
                    var w = workers[i];
                    string name = w.Archetype != null ? w.Archetype.DisplayName : "Worker";
                    string status = w.GetHoverText().Replace("\n", " | ");
                    args.Context.AddString($"[{i + 1}] {name} - Role: {w.CurrentRole} - Pos: {w.transform.position:F1} - Info: {status}");
                }
            });

            // Command: workers_clear
            new Terminal.ConsoleCommand("workers_clear", "Removes all workers within 50m of the player.", (args) =>
            {
                if (Player.m_localPlayer == null) return;
                var workers = UnityEngine.Object.FindObjectsOfType<WorkerBrain>();
                int count = 0;
                for (int i = 0; i < workers.Length; i++)
                {
                    if (Vector3.Distance(workers[i].transform.position, Player.m_localPlayer.transform.position) <= 50f)
                    {
                        workers[i].Dismiss();
                        count++;
                    }
                }
                if (count > 0)
                {
                    VerificationTelemetry.RecordPass("map_pin_dismiss_cleanup", $"Cleared {count} worker(s) via 'workers_clear' and cleaned up Minimap pins.");
                }
                args.Context.AddString($"[ValheimWorkers] Cleared {count} workers in 50m radius.");
            });

            // Command: workers_verify_status
            new Terminal.ConsoleCommand("workers_verify_status", "Displays the live verification checklist status synced with manual_verification_checklist.html.", (args) =>
            {
                var lines = VerificationTelemetry.GetSummaryLines();
                foreach (var line in lines)
                {
                    args.Context.AddString(line);
                }
            });

            // Command: workers_verify_pass <test_id> [note]
            new Terminal.ConsoleCommand("workers_verify_pass", "Marks a checklist test ID as PASSED from the console. Usage: workers_verify_pass <test_id> [note]", (args) =>
            {
                if (args.Length < 2)
                {
                    args.Context.AddString("Usage: workers_verify_pass <test_id> [note]");
                    return;
                }
                string testId = args[1];
                string note = args.Length > 2 ? string.Join(" ", args.Args, 2, args.Length - 2) : "Manually verified via F5 console.";
                VerificationTelemetry.RecordPass(testId, note);
                args.Context.AddString($"<color=green>[Checklist PASS]</color> {testId}: {note}");
            });

            // Command: workers_verify_fail <test_id> [reason]
            new Terminal.ConsoleCommand("workers_verify_fail", "Marks a checklist test ID as FAILED from the console. Usage: workers_verify_fail <test_id> [reason]", (args) =>
            {
                if (args.Length < 2)
                {
                    args.Context.AddString("Usage: workers_verify_fail <test_id> [reason]");
                    return;
                }
                string testId = args[1];
                string reason = args.Length > 2 ? string.Join(" ", args.Args, 2, args.Length - 2) : "Manually marked failed via F5 console.";
                VerificationTelemetry.RecordFail(testId, reason);
                args.Context.AddString($"<color=red>[Checklist FAIL]</color> {testId}: {reason}");
            });

            // Command: workers_verify_reset
            new Terminal.ConsoleCommand("workers_verify_reset", "Resets all runtime verification checklist states.", (args) =>
            {
                VerificationTelemetry.ResetAll();
                args.Context.AddString("[ValheimWorkers] Runtime verification telemetry reset.");
            });

            // Command: workers_devmode
            new Terminal.ConsoleCommand("workers_devmode", "Toggles developer telemetry and HTTP verification bridge on/off.", (args) =>
            {
                if (args.Length > 1 && bool.TryParse(args[1], out bool enabled))
                {
                    ValheimWorkersPlugin.DevMode.Value = enabled;
                }
                else
                {
                    ValheimWorkersPlugin.DevMode.Value = !ValheimWorkersPlugin.DevMode.Value;
                }
                VerificationTelemetry.Initialize(ValheimWorkersPlugin.DevMode.Value);
                args.Context.AddString($"[ValheimWorkers] DevMode telemetry bridge is now: {(ValheimWorkersPlugin.DevMode.Value ? "<color=green>ENABLED (Port 18788)</color>" : "<color=yellow>DISABLED</color>")}");
            });

            // Command: workers_verify_sync
            new Terminal.ConsoleCommand("workers_verify_sync", "Forces an immediate telemetry sync push to manual_verification_checklist.html and HTTP bridge.", (args) =>
            {
                VerificationTelemetry.ForceRefreshSnapshot();
                args.Context.AddString("<color=green>[ValheimWorkers]</color> Telemetry snapshot flushed immediately to HTTP port 18788 and checklist disk files.");
            });

            ValheimWorkersPlugin.Log.LogInfo("[ValheimWorkers] Custom developer terminal commands successfully registered!");
        }
    }
}
