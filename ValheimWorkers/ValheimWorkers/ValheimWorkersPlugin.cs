using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using ValheimWorkers.AI;
using ValheimWorkers.Core;
using ValheimWorkers.Prefabs;
using ValheimWorkers.UI;

namespace ValheimWorkers
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class ValheimWorkersPlugin : BaseUnityPlugin
    {
        public const string PluginGUID = "com.valheimworkers.mod";
        public const string PluginName = "ValheimWorkers";
        public const string PluginVersion = "0.3.0";

        public static ManualLogSource Log { get; private set; }
        public static ValheimWorkersPlugin Instance { get; private set; }
        private static Harmony _harmony;

        // Configuration
        public static ConfigEntry<KeyCode> ToggleUiHotkey { get; private set; }
        public static ConfigEntry<KeyCode> SpawnHotkey { get; private set; }
        public static ConfigEntry<float> DefaultWorkRadius { get; private set; }
        public static ConfigEntry<bool> AutoEnableConsole { get; private set; }
        public static ConfigEntry<bool> DevMode { get; private set; }
        public static ConfigEntry<float> UiFontScale { get; private set; }

        public static bool IsAdminOrHost()
        {
            if (ZNet.instance == null) return true;
            return ZNet.instance.LocalPlayerIsAdminOrHost();
        }

        private void Awake()
        {
            Instance = this;
            Log = Logger;
            Log.LogInfo($"[{PluginName}] Initializing v{PluginVersion} (with NPC Lab & Roster UI)...");

            // Setup Config
            ToggleUiHotkey = Config.Bind("General", "ToggleUiHotkey", KeyCode.F8, "Hotkey to toggle the NPC Lab & Settlement Manager UI.");
            SpawnHotkey = Config.Bind("General", "SpawnHotkey", KeyCode.F7, "Quick hotkey to spawn a default worker in front of player (Admin/Host/DevMode only).");
            DefaultWorkRadius = Config.Bind("General", "DefaultWorkRadius", 25f, "Default radius around home post in which workers operate.");
            AutoEnableConsole = Config.Bind("Development", "AutoEnableConsole", false, "Automatically enable Valheim developer console (F5) and devcommands in singleplayer or for host/admin.");
            DevMode = Config.Bind("Development", "DevMode", true, "Enable developer telemetry, local verification HTTP bridge, and detailed diagnostics.");
            UiFontScale = Config.Bind("UI", "UiFontScale", 1.15f, "UI and font scale multiplier for the F8 Settlement Hall window (0.85 to 1.65).");

            // Attach GUI Components
            gameObject.AddComponent<NpcLabGui>();
            gameObject.AddComponent<NpcCharacterMenuGui>();

            // Initialize Live Verification Telemetry Bridge (active only if DevMode is true)
            VerificationTelemetry.Initialize(DevMode.Value);

            // Initialize Harmony with resilient class-by-class error handling
            try
            {
                _harmony = new Harmony(PluginGUID);
                int patchedClasses = 0;
                int totalClasses = 0;
                foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
                {
                    if (type.GetCustomAttributes(typeof(HarmonyPatch), true).Length > 0)
                    {
                        totalClasses++;
                        try
                        {
                            _harmony.CreateClassProcessor(type).Patch();
                            patchedClasses++;
                        }
                        catch (Exception patchEx)
                        {
                            Log.LogError($"[{PluginName}] Failed to apply Harmony patch '{type.Name}': {patchEx}");
                        }
                    }
                }
                Log.LogInfo($"[{PluginName}] Successfully applied {patchedClasses}/{totalClasses} Harmony patch classes.");
            }
            catch (Exception ex)
            {
                Log.LogError($"[{PluginName}] Failed during Harmony initialization: {ex}");
            }
        }

        private void Update()
        {
            if (DevMode.Value)
            {
                VerificationTelemetry.Tick(Time.deltaTime);
            }

            // Hotkey: F7 Spawns a worker in front of player (requires admin, host, or DevMode)
            if (Input.GetKeyDown(SpawnHotkey.Value))
            {
                if (IsAdminOrHost() || DevMode.Value)
                {
                    if (Player.m_localPlayer != null && ZNetScene.instance != null)
                    {
                        Vector3 spawnPos = Player.m_localPlayer.transform.position + Player.m_localPlayer.transform.forward * 2f;
                        WorkerPrefabManager.SpawnWorker("Worker_Viking", spawnPos, Player.m_localPlayer.transform.rotation, WorkerRole.Hauler);
                        Log.LogInfo($"[{PluginName}] F7 hotkey pressed: Spawned worker at {spawnPos}.");
                    }
                }
            }
        }

        private void OnDestroy()
        {
            VerificationTelemetry.Shutdown();
            try
            {
                _harmony?.UnpatchSelf();
                Log.LogInfo($"[{PluginName}] Unpatched Harmony hooks cleanly.");
            }
            catch (Exception ex)
            {
                Log.LogWarning($"[{PluginName}] Error during unpatching: {ex}");
            }
        }
    }
}
