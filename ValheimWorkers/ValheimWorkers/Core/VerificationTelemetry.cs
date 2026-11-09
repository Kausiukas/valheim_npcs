using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using ValheimWorkers.AI;

namespace ValheimWorkers.Core
{
    /// <summary>
    /// Synchronizes in-game test conditions, worker actions, and console errors/exceptions
    /// with the interactive manual_verification_checklist.html dashboard via both a local
    /// HTTP bridge (http://127.0.0.1:18788) and zero-CORS disk snapshots (verification_telemetry.js).
    /// </summary>
    public static class VerificationTelemetry
    {
        public const int HttpPort = 18788;

        public class TestRecord
        {
            public string Id;
            public string Status = "untested"; // "untested", "passed", "failed"
            public string Timestamp = "";
            public string Evidence = "";
            public int PassCount = 0;
            public int ErrorCount = 0;
            public string LastError = "";
        }

        public class LogRecord
        {
            public string Time;
            public string Level;
            public string Source;
            public string Message;
        }

        public class ErrorRecord
        {
            public string Time;
            public string Type;
            public string Message;
            public string StackTrace;
            public string RelatedTestId;
        }

        public class WorkerLiveSnapshot
        {
            public string WorkerId;
            public string Name;
            public string Role;
            public string TaskState;
            public string StatusText;
            public string Position;
            public string HomePost;
            public string WorkPost;
            public float WorkRadius;
            public float DistFromPlayer;
            public float DistFromPost;
            public float Health;
            public float MaxHealth;
            public string EquippedRight;
            public string EquippedLeft;
            public string ArmorSummary;
            public string CarriedItemsSummary;
            public string ActiveCommandId;
            public string ActiveCommandSummary;
        }

        public class CommandTraceStep
        {
            public string TimeOffset; // e.g. "+0.0s", "+1.2s"
            public string Category;   // "CMD", "STATE", "EQUIP", "SCAN", "MOVE", "HARVEST", "COMBAT", "INVENTORY", "UNSTUCK", "WARN"
            public string Message;
            public string Position;
        }

        public class CommandTraceSession
        {
            public string TraceId;
            public string WorkerName;
            public string RoleAtStart;
            public string CommandType;
            public string CommandText;
            public string StartedAt;
            public float StartTime;
            public string InitialState;
            public string OutcomeStatus = "ACTIVE"; // "ACTIVE", "COMPLETED", "WARNING_STALLED", "FAILED"
            public string OutcomeSummary = "";
            public string RelatedTestId = "";
            public List<CommandTraceStep> Steps = new List<CommandTraceStep>();
        }

        public static readonly string[] AllTestIds = new string[]
        {
            "interaction_follow_toggle",
            "interaction_equip_persistence",
            "interaction_loadout_retention",
            "mechanic_auto_unstuck",
            "mechanic_food_healing",
            "locomotion_slope_direct_drive",
            "gui_resize_br",
            "gui_resize_bl",
            "gui_presets",
            "logistics_duty_radius",
            "map_minimap_pin",
            "map_world_map_pin",
            "map_locate_button",
            "map_pin_dismiss_cleanup",
            "defense_manageable_fight",
            "defense_tactical_retreat",
            "defense_low_hp_retreat",
            "defense_orient_main_base",
            "gather_known_filtering",
            "gather_missing_tool_prompt",
            "gather_fallback_stone",
            "gather_fallback_wood",
            "gather_food_forage",
            "gather_beehive_honey",
            "hunting_game_harvest",
            "farming_crop_harvest",
            "cooking_station_grill",
            "logistics_protected_gear",
            "logistics_smart_sorting",
            "proving_lumberjack",
            "proving_mining",
            "proving_hunting",
            "proving_archery_target",
            "proving_farming",
            "proving_cooking",
            "proving_foraging",
            "proving_smelter_kiln",
            "proving_troll_threat",
            "proving_low_hp_scenario",
            "economy_survival_hire_cost",
            "death_drop_inventory",
            "duplication_anti_cheat",
            "hunter_tamed_protection",
            "farmer_cultivated_ground",
            "tender_smelter_add_ore",
            "lumberjack_stump_priority",
            "lumberjack_reforestation",
            "logistics_overflow_piles",
            "miner_even_field",
            "miner_vein_exposure",
            "farming_crop_replant",
            "farmer_auto_cultivate_patch",
            "farmer_proactive_planting",
            "farming_chest_deposit",
            "gatherer_sap_collector",
            "farming_feed_tamed",
            "guard_hunter_archery",
            "guard_tactical_cqc_swap",
            "worker_workbench_repair",
            "guard_patrol_warhorn",
            "cooking_prevent_burn",
            "cooking_cauldron_stew",
            "cooking_fermenter_tap",
            "cooking_pantry_deposit",
            "processing_kiln_smelter",
            "tender_windmill_spinning",
            "tender_refuel_fires",
            "base_repair_structures",
            "logistics_sign_routing",
            "logistics_cart_loading",
            "social_ship_boarding",
            "social_night_torch",
            "social_tavern_rest",

            // Module 9: Map Expeditions, Wayfinder Sign Stones & Hidden Relics
            "expedition_signstone_spawn",
            "expedition_dagger_discovery",
            "expedition_chalice_discovery",
            "expedition_human_relics_discovery",
            "expedition_dvergr_seal_discovery",

            // Module 10: All 10 NPC Acquisition Paths
            "acquire_skel_dagger_boar",
            "acquire_skel_chalice_harvest",
            "acquire_skel_chalice_selfbleed",
            "acquire_skel_chalice_pour",
            "acquire_human_t1_lost_soul",
            "acquire_human_t2_homesteader",
            "acquire_human_t3_drengr",
            "acquire_human_t4_einherjar",
            "acquire_dvergr_t1_drudge",
            "acquire_dvergr_t2_prospector",
            "acquire_dvergr_t3_smith",
            "acquire_dvergr_t4_sentinel",

            // Module 11: Valheim Activity Skills (0-100) & Progression
            "actskill_woodcutting",
            "actskill_mining",
            "actskill_melee",
            "actskill_blocking",
            "actskill_bows",
            "actskill_farming",
            "actskill_crafting",
            "actskill_athletics",
            "actskill_overall_levelup",

            // Module 12: Heritage Skill Trees, Blood Evolution & Luxury Patronage
            "skilltree_human_woodsman",
            "skilltree_human_hearth",
            "skilltree_human_ranger",
            "skilltree_human_shieldwall",
            "skilltree_skel_evolutions",
            "skilltree_skel_graft_1_2",
            "skilltree_skel_graft_3_4",
            "skilltree_skel_graft_5_6",
            "skilltree_skel_combat_bloodfeed",
            "skilltree_dvergr_mine_gem",
            "skilltree_dvergr_smelt_anvil",
            "skilltree_dvergr_brew_ward",

            // Module 13: Pride/Dignity Mechanics & Player-Style Character Menu UI
            "charmenu_pride_decay",
            "charmenu_pride_appease",
            "charmenu_side_drawer",
            "charmenu_tab1_paperdoll",
            "charmenu_tab2_skills_dialog",
            "charmenu_tab3_skill_tree"
        };

        private static readonly object s_lock = new object();
        private static readonly Dictionary<string, TestRecord> s_tests = new Dictionary<string, TestRecord>(StringComparer.OrdinalIgnoreCase);
        private static readonly List<LogRecord> s_logs = new List<LogRecord>();
        private static readonly List<ErrorRecord> s_errors = new List<ErrorRecord>();
        private static readonly Dictionary<string, WorkerLiveSnapshot> s_workerSnapshots = new Dictionary<string, WorkerLiveSnapshot>();
        private static readonly List<CommandTraceSession> s_commandTraces = new List<CommandTraceSession>();

        private const int MaxLogs = 80;
        private const int MaxErrors = 35;
        private const int MaxTraces = 30;
        private const int MaxTraceSteps = 60;
        private static int s_traceCounter = 1;

        private static bool s_initialized = false;
        private static bool s_dirty = true;
        private static bool s_forceImmediateFlush = true;
        private static float s_flushTimer = 0f;
        private static int s_sequence = 1;
        private static int s_cachedWorkerCount = 0;
        private static bool s_cachedWorldLoaded = false;
        private static string s_cachedJson = "{}";
        private static float s_sceneScanTimer = 0f;

        private static TcpListener s_tcpListener;
        private static Thread s_listenerThread;
        private static volatile bool s_running = false;
        private static bool s_enabled = false;
        private static int s_mainThreadId;
        public static bool IsMainThread => s_mainThreadId != 0 && System.Threading.Thread.CurrentThread.ManagedThreadId == s_mainThreadId;

        private static string[] s_targetDirectories = null;

        private static void LoadPersistedTelemetry(string dir)
        {
            try
            {
                if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir)) return;
                string jsonFile = Path.Combine(dir, "verification_telemetry.json");
                if (!File.Exists(jsonFile)) return;

                string json = File.ReadAllText(jsonFile);
                if (string.IsNullOrEmpty(json)) return;

                foreach (var id in AllTestIds)
                {
                    string search = "\"" + id + "\":";
                    int idx = json.IndexOf(search, StringComparison.OrdinalIgnoreCase);
                    if (idx < 0) continue;

                    int blockStart = json.IndexOf('{', idx);
                    if (blockStart < 0) continue;
                    int blockEnd = json.IndexOf('}', blockStart);
                    if (blockEnd < 0) continue;

                    string block = json.Substring(blockStart, blockEnd - blockStart + 1);
                    string status = ExtractJsonStringValue(block, "status");
                    if (status == "passed")
                    {
                        if (s_tests.TryGetValue(id, out var existing))
                        {
                            existing.Status = "passed";
                            existing.Timestamp = ExtractJsonStringValue(block, "timestamp");
                            existing.Evidence = ExtractJsonStringValue(block, "evidence");
                            existing.PassCount = ExtractJsonIntValue(block, "passCount", 1);
                            existing.ErrorCount = ExtractJsonIntValue(block, "errorCount", 0);
                            existing.LastError = ExtractJsonStringValue(block, "lastError");
                        }
                    }
                    else if (status == "failed")
                    {
                        if (s_tests.TryGetValue(id, out var existing) && existing.Status != "passed")
                        {
                            existing.Status = "failed";
                            existing.Timestamp = ExtractJsonStringValue(block, "timestamp");
                            existing.Evidence = ExtractJsonStringValue(block, "evidence");
                            existing.PassCount = ExtractJsonIntValue(block, "passCount", 0);
                            existing.ErrorCount = ExtractJsonIntValue(block, "errorCount", 1);
                            existing.LastError = ExtractJsonStringValue(block, "lastError");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ValheimWorkersPlugin.Log?.LogWarning($"[ValheimWorkers] Telemetry load error from '{dir}': {ex.Message}");
            }
        }

        private static string ExtractJsonStringValue(string json, string key)
        {
            string search = "\"" + key + "\":\"";
            int start = json.IndexOf(search, StringComparison.OrdinalIgnoreCase);
            if (start < 0) return "";
            start += search.Length;
            int end = json.IndexOf('"', start);
            if (end < 0) return "";
            return json.Substring(start, end - start).Replace("\\\"", "\"").Replace("\\\\", "\\");
        }

        private static int ExtractJsonIntValue(string json, string key, int defaultValue)
        {
            string search = "\"" + key + "\":";
            int start = json.IndexOf(search, StringComparison.OrdinalIgnoreCase);
            if (start < 0) return defaultValue;
            start += search.Length;
            int end = start;
            while (end < json.Length && (char.IsDigit(json[end]) || json[end] == '-')) end++;
            if (int.TryParse(json.Substring(start, end - start), out int val)) return val;
            return defaultValue;
        }

        public static void Initialize(bool enable = false)
        {
            s_enabled = enable;
            if (!s_enabled) return;
            if (s_initialized) return;
            s_initialized = true;
            s_mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;

            string configTelemetryDir = Path.Combine(BepInEx.Paths.ConfigPath, "ValheimWorkers", "telemetry");
            try
            {
                if (!Directory.Exists(configTelemetryDir)) Directory.CreateDirectory(configTelemetryDir);
            }
            catch { }

            var targetDirs = new List<string> { configTelemetryDir };
            try
            {
                string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string brainRoot = Path.Combine(userProfile, ".gemini", "antigravity", "brain");
                if (Directory.Exists(brainRoot))
                {
                    foreach (var d in Directory.GetDirectories(brainRoot))
                    {
                        if (File.Exists(Path.Combine(d, "manual_verification_checklist.html")) ||
                            File.Exists(Path.Combine(d, "verification_telemetry.json")) ||
                            File.Exists(Path.Combine(d, "verification_telemetry.js")))
                        {
                            targetDirs.Add(d);
                            string htmlSrc = Path.Combine(d, "manual_verification_checklist.html");
                            string htmlDst = Path.Combine(configTelemetryDir, "manual_verification_checklist.html");
                            if (File.Exists(htmlSrc) && (!File.Exists(htmlDst) || File.GetLastWriteTimeUtc(htmlSrc) > File.GetLastWriteTimeUtc(htmlDst)))
                            {
                                File.Copy(htmlSrc, htmlDst, true);
                            }
                        }
                    }
                }
            }
            catch { }
            s_targetDirectories = targetDirs.ToArray();

            lock (s_lock)
            {
                s_tests.Clear();
                foreach (var id in AllTestIds)
                {
                    s_tests[id] = new TestRecord { Id = id };
                }

                // Restore previously verified test records from disk so restarts do NOT wipe checklist progress
                LoadPersistedTelemetry(configTelemetryDir);
                foreach (var dir in targetDirs)
                {
                    if (dir != configTelemetryDir) LoadPersistedTelemetry(dir);
                }
            }

            Application.logMessageReceived += OnUnityLogMessageReceived;

            RecordLog("System", $"Verification Telemetry initialized (HTTP port {HttpPort} + disk snapshot sync to {s_targetDirectories.Length} targets).", "INFO");
            StartHttpServer();
            RebuildAndFlushSnapshot();
        }

        public static void Shutdown()
        {
            if (!s_enabled || !s_initialized) return;
            s_initialized = false;

            Application.logMessageReceived -= OnUnityLogMessageReceived;
            StopHttpServer();
            RebuildAndFlushSnapshot();
        }

        public static void Tick(float dt)
        {
            if (!s_enabled || !s_initialized) return;

            // Prune dead or destroyed workers
            for (int i = WorkerBrain.ActiveWorkers.Count - 1; i >= 0; i--)
            {
                var wb = WorkerBrain.ActiveWorkers[i];
                if (wb == null || wb.gameObject == null || (wb.m_character != null && wb.m_character.IsDead()))
                {
                    WorkerBrain.ActiveWorkers.RemoveAt(i);
                }
            }

            bool worldLoaded = Player.m_localPlayer != null && ZNetScene.instance != null;
            bool justLoaded = worldLoaded && !s_cachedWorldLoaded;
            s_sceneScanTimer += dt;
            // Debounced scene discovery fallback: run only on initial world load or every 10 seconds
            if (worldLoaded && (justLoaded || s_sceneScanTimer >= 10.0f))
            {
                s_sceneScanTimer = 0f;
                var sceneBrains = UnityEngine.Object.FindObjectsOfType<WorkerBrain>();
                if (sceneBrains != null)
                {
                    for (int i = 0; i < sceneBrains.Length; i++)
                    {
                        var wb = sceneBrains[i];
                        if (wb != null && wb.gameObject != null && (wb.m_character == null || !wb.m_character.IsDead()))
                        {
                            if (!WorkerBrain.ActiveWorkers.Contains(wb))
                            {
                                WorkerBrain.ActiveWorkers.Add(wb);
                            }
                        }
                    }
                }
            }

            int activeWorkers = WorkerBrain.ActiveWorkers.Count;

            // Check if World Map (M) is open while workers are pinned
            if (worldLoaded && activeWorkers > 0 && Minimap.instance != null && Minimap.IsOpen())
            {
                RecordPass("map_world_map_pin", $"World Map (M) opened with {activeWorkers} active synced worker pin(s).");
            }

            if (activeWorkers != s_cachedWorkerCount || worldLoaded != s_cachedWorldLoaded)
            {
                s_cachedWorkerCount = activeWorkers;
                s_cachedWorldLoaded = worldLoaded;
                s_dirty = true;
            }

            s_flushTimer += dt;
            // Continuous Heartbeat: flush immediately on force flag, or when dirty (>= 1.0s), or every 2.0s while running
            if (s_forceImmediateFlush || (s_dirty && s_flushTimer >= 1.0f) || s_flushTimer >= 2.0f)
            {
                s_flushTimer = 0f;
                s_forceImmediateFlush = false;
                s_dirty = false;
                RebuildAndFlushSnapshot();
            }
        }

        public static void RecordPass(string testId, string evidence, bool logToConsole = true)
        {
            if (!s_enabled || string.IsNullOrEmpty(testId)) return;

            bool firstTimePass = false;
            string now = null;

            lock (s_lock)
            {
                if (s_tests.TryGetValue(testId, out var rec) && rec.Status == "passed")
                {
                    rec.PassCount++;
                    return; // Early return: eliminates heavy string format, logging, and disk sync
                }

                if (rec == null)
                {
                    rec = new TestRecord { Id = testId };
                    s_tests[testId] = rec;
                }

                now = DateTime.Now.ToString("HH:mm:ss");
                firstTimePass = true;
                rec.Status = "passed";
                rec.Timestamp = now;
                rec.Evidence = evidence ?? "Condition verified in-game.";
                rec.PassCount++;
                s_sequence++;
                s_dirty = true;
                s_forceImmediateFlush = true;
                AddLogInternal("VERIFY", "Checklist", $"[PASS: {testId}] {rec.Evidence}");
            }

            if (firstTimePass && logToConsole && ValheimWorkersPlugin.Log != null)
            {
                ValheimWorkersPlugin.Log.LogInfo($"[ValheimWorkers][Checklist PASS] ({testId}) {evidence}");
            }
        }

        public static void RecordFail(string testId, string reason, bool logToConsole = true)
        {
            if (!s_enabled || string.IsNullOrEmpty(testId)) return;

            string now = DateTime.Now.ToString("HH:mm:ss");

            lock (s_lock)
            {
                if (!s_tests.TryGetValue(testId, out var rec))
                {
                    rec = new TestRecord { Id = testId };
                    s_tests[testId] = rec;
                }

                rec.Status = "failed";
                rec.Timestamp = now;
                rec.Evidence = reason ?? "Marked failed.";
                rec.ErrorCount++;
                rec.LastError = reason ?? "";
                s_sequence++;
                s_dirty = true;
                s_forceImmediateFlush = true;

                AddLogInternal("FAIL", "Checklist", $"[FAIL: {testId}] {rec.Evidence}");
            }

            if (logToConsole && ValheimWorkersPlugin.Log != null)
            {
                ValheimWorkersPlugin.Log.LogWarning($"[ValheimWorkers][Checklist FAIL] ({testId}) {reason}");
            }
        }

        public static void RecordLog(string source, string message, string level = "INFO")
        {
            if (!s_enabled || string.IsNullOrEmpty(message)) return;

            lock (s_lock)
            {
                AddLogInternal(level, source ?? "Worker", message);
                s_sequence++;
                s_dirty = true;
            }
        }

        public static void ResetAll()
        {
            if (!s_enabled) return;

            lock (s_lock)
            {
                foreach (var kvp in s_tests)
                {
                    kvp.Value.Status = "untested";
                    kvp.Value.Timestamp = "";
                    kvp.Value.Evidence = "";
                    kvp.Value.PassCount = 0;
                    kvp.Value.ErrorCount = 0;
                    kvp.Value.LastError = "";
                }
                s_errors.Clear();
                s_logs.Clear();
                s_commandTraces.Clear();
                s_workerSnapshots.Clear();
                AddLogInternal("INFO", "Checklist", "All runtime verification states and command traces reset.");
                s_sequence++;
                s_dirty = true;
                s_forceImmediateFlush = true;
                s_cachedJson = BuildTelemetryJsonInternal();
            }
            if (IsMainThread)
            {
                RebuildAndFlushSnapshot();
            }
        }

        public static void ForceRefreshSnapshot()
        {
            if (!s_enabled) return;

            if (!IsMainThread)
            {
                lock (s_lock)
                {
                    s_dirty = true;
                    s_forceImmediateFlush = true;
                }
                return;
            }

            s_flushTimer = 0f;
            s_forceImmediateFlush = false;
            s_dirty = false;

            // Prune dead or destroyed workers
            for (int i = WorkerBrain.ActiveWorkers.Count - 1; i >= 0; i--)
            {
                var wb = WorkerBrain.ActiveWorkers[i];
                if (wb == null || wb.gameObject == null || (wb.m_character != null && wb.m_character.IsDead()))
                {
                    WorkerBrain.ActiveWorkers.RemoveAt(i);
                }
            }

            bool worldLoaded = Player.m_localPlayer != null && ZNetScene.instance != null;
            if (worldLoaded)
            {
                var sceneBrains = UnityEngine.Object.FindObjectsOfType<WorkerBrain>();
                if (sceneBrains != null)
                {
                    for (int i = 0; i < sceneBrains.Length; i++)
                    {
                        var wb = sceneBrains[i];
                        if (wb != null && wb.gameObject != null && (wb.m_character == null || !wb.m_character.IsDead()))
                        {
                            if (!WorkerBrain.ActiveWorkers.Contains(wb))
                            {
                                WorkerBrain.ActiveWorkers.Add(wb);
                            }
                        }
                    }
                }
            }

            s_cachedWorkerCount = WorkerBrain.ActiveWorkers.Count;
            s_cachedWorldLoaded = worldLoaded;
            lock (s_lock)
            {
                s_sequence++;
            }
            RebuildAndFlushSnapshot();
        }

        public static string StartCommandTrace(string workerName, string role, string commandType, string commandText, string initialState, string relatedTestId = "")
        {
            if (!s_enabled) return "";

            string traceId;
            lock (s_lock)
            {
                traceId = $"CMD-{s_traceCounter++:D3}";
                var session = new CommandTraceSession
                {
                    TraceId = traceId,
                    WorkerName = workerName ?? "Worker",
                    RoleAtStart = role ?? "Unknown",
                    CommandType = commandType ?? "CUSTOM",
                    CommandText = commandText ?? "",
                    StartedAt = DateTime.Now.ToString("HH:mm:ss"),
                    StartTime = Time.time,
                    InitialState = initialState ?? "",
                    OutcomeStatus = "ACTIVE",
                    OutcomeSummary = "Awaiting follow-up actions...",
                    RelatedTestId = relatedTestId ?? ""
                };

                // Add initial CMD step
                session.Steps.Add(new CommandTraceStep
                {
                    TimeOffset = "+0.0s",
                    Category = "CMD",
                    Message = commandText ?? "Command initiated",
                    Position = ""
                });

                s_commandTraces.Add(session);
                if (s_commandTraces.Count > MaxTraces)
                {
                    s_commandTraces.RemoveAt(0);
                }

                AddLogInternal("CMD", workerName ?? "Worker", $"[{traceId}] {commandText}");
                s_sequence++;
                s_dirty = true;
                s_forceImmediateFlush = true;
            }
            return traceId;
        }

        public static void RecordTraceStep(string traceId, string category, string message, Vector3? position = null)
        {
            if (!s_enabled || string.IsNullOrEmpty(traceId) || string.IsNullOrEmpty(message)) return;

            lock (s_lock)
            {
                CommandTraceSession session = null;
                for (int i = s_commandTraces.Count - 1; i >= 0; i--)
                {
                    if (string.Equals(s_commandTraces[i].TraceId, traceId, StringComparison.OrdinalIgnoreCase))
                    {
                        session = s_commandTraces[i];
                        break;
                    }
                }

                if (session == null) return;

                float elapsed = Mathf.Max(0f, Time.time - session.StartTime);
                string posStr = position.HasValue ? $"{position.Value.x:F1}, {position.Value.y:F1}, {position.Value.z:F1}" : "";

                session.Steps.Add(new CommandTraceStep
                {
                    TimeOffset = $"+{elapsed:F1}s",
                    Category = category ?? "STATE",
                    Message = message,
                    Position = posStr
                });

                if (session.Steps.Count > MaxTraceSteps)
                {
                    session.Steps.RemoveAt(0);
                }

                // If currently ACTIVE, update running outcome summary
                if (session.OutcomeStatus == "ACTIVE")
                {
                    session.OutcomeSummary = $"In progress: {message}";
                }

                s_sequence++;
                s_dirty = true;
            }
        }

        public static void CompleteCommandTrace(string traceId, string outcomeStatus, string outcomeSummary)
        {
            if (!s_enabled || string.IsNullOrEmpty(traceId)) return;

            lock (s_lock)
            {
                CommandTraceSession session = null;
                for (int i = s_commandTraces.Count - 1; i >= 0; i--)
                {
                    if (string.Equals(s_commandTraces[i].TraceId, traceId, StringComparison.OrdinalIgnoreCase))
                    {
                        session = s_commandTraces[i];
                        break;
                    }
                }

                if (session == null) return;

                session.OutcomeStatus = outcomeStatus ?? "COMPLETED";
                session.OutcomeSummary = outcomeSummary ?? "Completed successfully.";

                float elapsed = Mathf.Max(0f, Time.time - session.StartTime);
                session.Steps.Add(new CommandTraceStep
                {
                    TimeOffset = $"+{elapsed:F1}s",
                    Category = string.Equals(outcomeStatus, "FAILED", StringComparison.OrdinalIgnoreCase) ? "WARN" : "STATE",
                    Message = $"Outcome [{session.OutcomeStatus}]: {session.OutcomeSummary}",
                    Position = ""
                });

                s_sequence++;
                s_dirty = true;
                s_forceImmediateFlush = true;
            }
        }

        public static string GetTraceSummary(string traceId)
        {
            if (string.IsNullOrEmpty(traceId)) return "";
            lock (s_lock)
            {
                for (int i = s_commandTraces.Count - 1; i >= 0; i--)
                {
                    if (string.Equals(s_commandTraces[i].TraceId, traceId, StringComparison.OrdinalIgnoreCase))
                    {
                        return $"[{s_commandTraces[i].OutcomeStatus}] {s_commandTraces[i].OutcomeSummary}";
                    }
                }
            }
            return "";
        }

        public static void UpdateWorkerSnapshot(WorkerLiveSnapshot snapshot)
        {
            if (!s_enabled || snapshot == null || string.IsNullOrEmpty(snapshot.WorkerId)) return;
            lock (s_lock)
            {
                s_workerSnapshots[snapshot.WorkerId] = snapshot;
                s_sequence++;
                s_dirty = true;
            }
        }

        public static void RemoveWorkerSnapshot(string workerId)
        {
            if (!s_enabled || string.IsNullOrEmpty(workerId)) return;
            lock (s_lock)
            {
                if (s_workerSnapshots.Remove(workerId))
                {
                    s_sequence++;
                    s_dirty = true;
                }
            }
        }

        public static List<string> GetSummaryLines()
        {
            var lines = new List<string>();
            lock (s_lock)
            {
                int passed = 0;
                int failed = 0;
                foreach (var kv in s_tests.Values)
                {
                    if (kv.Status == "passed") passed++;
                    else if (kv.Status == "failed") failed++;
                }

                lines.Add($"=== ValheimWorkers Verification Checklist ({passed}/{s_tests.Count} Passed, {failed} Failed, {s_errors.Count} Errors) ===");
                foreach (var id in AllTestIds)
                {
                    if (!s_tests.TryGetValue(id, out var r)) continue;
                    string badge = r.Status == "passed" ? "[PASS]" : (r.Status == "failed" ? "[FAIL]" : "[----]");
                    string detail = !string.IsNullOrEmpty(r.Evidence) ? $" - {r.Evidence} ({r.Timestamp})" : "";
                    lines.Add($"  {badge} {r.Id}{detail}");
                }
            }
            return lines;
        }

        private static void AddLogInternal(string level, string source, string message)
        {
            s_logs.Add(new LogRecord
            {
                Time = DateTime.Now.ToString("HH:mm:ss"),
                Level = level,
                Source = source,
                Message = message
            });

            if (s_logs.Count > MaxLogs)
            {
                s_logs.RemoveAt(0);
            }
        }

        private static void OnUnityLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            if (type != LogType.Error && type != LogType.Exception) return;

            string combined = (condition ?? "") + "\n" + (stackTrace ?? "");
            // Filter out noisy unrelated vanilla shader/audio warnings unless it's an exception or mentions ValheimWorkers
            bool isModRelated = combined.IndexOf("ValheimWorkers", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                combined.IndexOf("WorkerBrain", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                combined.IndexOf("GathererTask", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                combined.IndexOf("HaulerTask", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                combined.IndexOf("BaseTenderTask", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                combined.IndexOf("NpcLabGui", StringComparison.OrdinalIgnoreCase) >= 0;

            if (!isModRelated && type != LogType.Exception) return;

            string relatedTestId = InferRelatedTestId(combined);
            string now = DateTime.Now.ToString("HH:mm:ss");

            lock (s_lock)
            {
                s_errors.Add(new ErrorRecord
                {
                    Time = now,
                    Type = type.ToString(),
                    Message = condition ?? "Unknown Error",
                    StackTrace = stackTrace ?? "",
                    RelatedTestId = relatedTestId
                });

                if (s_errors.Count > MaxErrors)
                {
                    s_errors.RemoveAt(0);
                }

                AddLogInternal("ERROR", isModRelated ? "ValheimWorkers" : "Unity", condition ?? "Exception occurred");

                if (!string.IsNullOrEmpty(relatedTestId) && s_tests.TryGetValue(relatedTestId, out var rec))
                {
                    rec.ErrorCount++;
                    rec.LastError = $"[{now}] {condition}";
                }

                s_sequence++;
                s_dirty = true;
                s_forceImmediateFlush = true;
            }
        }

        private static string InferRelatedTestId(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            if (text.IndexOf("ProcessScreenSpaceResize", StringComparison.OrdinalIgnoreCase) >= 0) return "gui_resize_br";
            if (text.IndexOf("SetWindowPreset", StringComparison.OrdinalIgnoreCase) >= 0) return "gui_presets";
            if (text.IndexOf("UpdateMapPin", StringComparison.OrdinalIgnoreCase) >= 0) return "map_minimap_pin";
            if (text.IndexOf("RemoveMapPin", StringComparison.OrdinalIgnoreCase) >= 0) return "map_pin_dismiss_cleanup";
            if (text.IndexOf("UpdateSelfDefenseAndThreats", StringComparison.OrdinalIgnoreCase) >= 0 ||
                text.IndexOf("ComputeEnemyThreatScore", StringComparison.OrdinalIgnoreCase) >= 0) return "defense_tactical_retreat";
            if (text.IndexOf("EvaluateEquipment", StringComparison.OrdinalIgnoreCase) >= 0) return "gather_missing_tool_prompt";
            if (text.IndexOf("LogisticsHelper", StringComparison.OrdinalIgnoreCase) >= 0) return "logistics_overflow_piles";
            if (text.IndexOf("CookTask", StringComparison.OrdinalIgnoreCase) >= 0) return "cooking_station_grill";
            if (text.IndexOf("FarmerTask", StringComparison.OrdinalIgnoreCase) >= 0) return "farming_crop_harvest";
            if (text.IndexOf("GuardTask", StringComparison.OrdinalIgnoreCase) >= 0) return "defense_manageable_fight";
            if (text.IndexOf("BaseTenderTask", StringComparison.OrdinalIgnoreCase) >= 0) return "processing_kiln_smelter";
            if (text.IndexOf("GathererTask", StringComparison.OrdinalIgnoreCase) >= 0) return "gather_fallback_wood";
            if (text.IndexOf("HaulerTask", StringComparison.OrdinalIgnoreCase) >= 0 ||
                text.IndexOf("HandleDepositing", StringComparison.OrdinalIgnoreCase) >= 0) return "logistics_smart_sorting";
            if (text.IndexOf("NpcLabGui", StringComparison.OrdinalIgnoreCase) >= 0) return "gui_resize_br";
            return "";
        }

        private static void RebuildAndFlushSnapshot()
        {
            if (!s_enabled || s_targetDirectories == null) return;

            if (!IsMainThread)
            {
                lock (s_lock)
                {
                    s_dirty = true;
                    s_forceImmediateFlush = true;
                }
                return;
            }

            string json;
            try
            {
                lock (s_lock)
                {
                    json = BuildTelemetryJsonInternal();
                    s_cachedJson = json;
                }
            }
            catch (Exception ex)
            {
                ValheimWorkersPlugin.Log?.LogWarning($"[ValheimWorkers] Error generating telemetry JSON: {ex.Message}");
                json = s_cachedJson ?? "{}";
            }

            string jsContent = "window.__VALHEIM_TELEMETRY__ = " + json + ";\n";

            foreach (var dir in s_targetDirectories)
            {
                try
                {
                    if (Directory.Exists(dir))
                    {
                        string jsonPath = Path.Combine(dir, "verification_telemetry.json");
                        using (var fs = new FileStream(jsonPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                        using (var writer = new StreamWriter(fs, Encoding.UTF8))
                        {
                            writer.Write(json);
                        }

                        string jsPath = Path.Combine(dir, "verification_telemetry.js");
                        using (var fs = new FileStream(jsPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                        using (var writer = new StreamWriter(fs, Encoding.UTF8))
                        {
                            writer.Write(jsContent);
                        }
                    }
                }
                catch
                {
                    // Ignore transient file lock conflicts
                }
            }
        }

        private static string BuildTelemetryJsonInternal()
        {
            var sb = new StringBuilder(4096);
            long unixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            // Collect active workers live snapshots directly from active instances
            var liveWorkersList = new List<WorkerLiveSnapshot>();

            if (IsMainThread && Player.m_localPlayer != null)
            {
                var sceneBrains = UnityEngine.Object.FindObjectsOfType<WorkerBrain>();
                if (sceneBrains != null)
                {
                    for (int i = 0; i < sceneBrains.Length; i++)
                    {
                        var wb = sceneBrains[i];
                        if (wb != null && wb.gameObject != null && (wb.m_character == null || !wb.m_character.IsDead()))
                        {
                            if (!WorkerBrain.ActiveWorkers.Contains(wb))
                            {
                                WorkerBrain.ActiveWorkers.Add(wb);
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < WorkerBrain.ActiveWorkers.Count; i++)
            {
                var wb = WorkerBrain.ActiveWorkers[i];
                if (wb == null || wb.gameObject == null || (wb.m_character != null && wb.m_character.IsDead())) continue;
                try
                {
                    var snap = wb.CreateLiveSnapshot();
                    if (snap != null)
                    {
                        liveWorkersList.Add(snap);
                        s_workerSnapshots[snap.WorkerId] = snap;
                    }
                }
                catch
                {
                    // Ignore single worker snapshot error
                }
            }

            if (liveWorkersList.Count == 0 && s_workerSnapshots.Count > 0)
            {
                liveWorkersList.AddRange(s_workerSnapshots.Values);
            }

            int finalWorkerCount = liveWorkersList.Count;
            s_cachedWorkerCount = finalWorkerCount;

            sb.Append("{");
            sb.AppendFormat(CultureInfo.InvariantCulture, "\"seq\":{0},", s_sequence);
            sb.AppendFormat("\"updatedAt\":\"{0}\",", EscapeJson(DateTime.Now.ToString("HH:mm:ss")));
            sb.AppendFormat(CultureInfo.InvariantCulture, "\"updatedUnixMs\":{0},", unixMs);
            sb.AppendFormat("\"pluginVersion\":\"{0}\",", EscapeJson(ValheimWorkersPlugin.PluginVersion));
            sb.AppendFormat("\"worldLoaded\":{0},", s_cachedWorldLoaded ? "true" : "false");
            sb.AppendFormat(CultureInfo.InvariantCulture, "\"activeWorkers\":{0},", finalWorkerCount);
            sb.AppendFormat(CultureInfo.InvariantCulture, "\"errorCount\":{0},", s_errors.Count);

            // Tests dictionary
            sb.Append("\"tests\":{");
            bool firstTest = true;
            foreach (var id in AllTestIds)
            {
                if (!s_tests.TryGetValue(id, out var r)) continue;
                if (!firstTest) sb.Append(",");
                firstTest = false;

                sb.AppendFormat("\"{0}\":{{", EscapeJson(r.Id));
                sb.AppendFormat("\"status\":\"{0}\",", EscapeJson(r.Status));
                sb.AppendFormat("\"timestamp\":\"{0}\",", EscapeJson(r.Timestamp));
                sb.AppendFormat("\"evidence\":\"{0}\",", EscapeJson(r.Evidence));
                sb.AppendFormat(CultureInfo.InvariantCulture, "\"passCount\":{0},", r.PassCount);
                sb.AppendFormat(CultureInfo.InvariantCulture, "\"errorCount\":{0},", r.ErrorCount);
                sb.AppendFormat("\"lastError\":\"{0}\"", EscapeJson(r.LastError));
                sb.Append("}");
            }
            sb.Append("},");

            // Recent logs
            sb.Append("\"logs\":[");
            for (int i = 0; i < s_logs.Count; i++)
            {
                if (i > 0) sb.Append(",");
                var l = s_logs[i];
                sb.Append("{");
                sb.AppendFormat("\"time\":\"{0}\",", EscapeJson(l.Time));
                sb.AppendFormat("\"level\":\"{0}\",", EscapeJson(l.Level));
                sb.AppendFormat("\"source\":\"{0}\",", EscapeJson(l.Source));
                sb.AppendFormat("\"message\":\"{0}\"", EscapeJson(l.Message));
                sb.Append("}");
            }
            sb.Append("],");

            // Errors
            sb.Append("\"errors\":[");
            for (int i = 0; i < s_errors.Count; i++)
            {
                if (i > 0) sb.Append(",");
                var e = s_errors[i];
                sb.Append("{");
                sb.AppendFormat("\"time\":\"{0}\",", EscapeJson(e.Time));
                sb.AppendFormat("\"type\":\"{0}\",", EscapeJson(e.Type));
                sb.AppendFormat("\"message\":\"{0}\",", EscapeJson(e.Message));
                sb.AppendFormat("\"stackTrace\":\"{0}\",", EscapeJson(e.StackTrace));
                sb.AppendFormat("\"relatedTestId\":\"{0}\"", EscapeJson(e.RelatedTestId));
                sb.Append("}");
            }
            sb.Append("],");

            // Active Workers Live Snapshot
            sb.Append("\"workers\":[");
            for (int i = 0; i < liveWorkersList.Count; i++)
            {
                if (i > 0) sb.Append(",");
                var w = liveWorkersList[i];
                sb.Append("{");
                sb.AppendFormat("\"id\":\"{0}\",", EscapeJson(w.WorkerId));
                sb.AppendFormat("\"name\":\"{0}\",", EscapeJson(w.Name));
                sb.AppendFormat("\"role\":\"{0}\",", EscapeJson(w.Role));
                sb.AppendFormat("\"taskState\":\"{0}\",", EscapeJson(w.TaskState));
                sb.AppendFormat("\"statusText\":\"{0}\",", EscapeJson(w.StatusText));
                sb.AppendFormat("\"pos\":\"{0}\",", EscapeJson(w.Position));
                sb.AppendFormat("\"homePost\":\"{0}\",", EscapeJson(w.HomePost));
                sb.AppendFormat("\"workPost\":\"{0}\",", EscapeJson(w.WorkPost));
                sb.AppendFormat(CultureInfo.InvariantCulture, "\"radius\":{0:F1},", w.WorkRadius);
                sb.AppendFormat(CultureInfo.InvariantCulture, "\"distPlayer\":{0:F1},", w.DistFromPlayer);
                sb.AppendFormat(CultureInfo.InvariantCulture, "\"distPost\":{0:F1},", w.DistFromPost);
                sb.AppendFormat(CultureInfo.InvariantCulture, "\"health\":{0:F0},", w.Health);
                sb.AppendFormat(CultureInfo.InvariantCulture, "\"maxHealth\":{0:F0},", w.MaxHealth);
                sb.AppendFormat("\"right\":\"{0}\",", EscapeJson(w.EquippedRight));
                sb.AppendFormat("\"left\":\"{0}\",", EscapeJson(w.EquippedLeft));
                sb.AppendFormat("\"armor\":\"{0}\",", EscapeJson(w.ArmorSummary));
                sb.AppendFormat("\"carried\":\"{0}\",", EscapeJson(w.CarriedItemsSummary));
                sb.AppendFormat("\"activeCmdId\":\"{0}\",", EscapeJson(w.ActiveCommandId));
                sb.AppendFormat("\"activeCmdSummary\":\"{0}\"", EscapeJson(w.ActiveCommandSummary));
                sb.Append("}");
            }
            sb.Append("],");

            // Command trace sessions
            sb.Append("\"commandTraces\":[");
            for (int i = 0; i < s_commandTraces.Count; i++)
            {
                if (i > 0) sb.Append(",");
                var t = s_commandTraces[i];
                sb.Append("{");
                sb.AppendFormat("\"id\":\"{0}\",", EscapeJson(t.TraceId));
                sb.AppendFormat("\"worker\":\"{0}\",", EscapeJson(t.WorkerName));
                sb.AppendFormat("\"role\":\"{0}\",", EscapeJson(t.RoleAtStart));
                sb.AppendFormat("\"type\":\"{0}\",", EscapeJson(t.CommandType));
                sb.AppendFormat("\"cmd\":\"{0}\",", EscapeJson(t.CommandText));
                sb.AppendFormat("\"startedAt\":\"{0}\",", EscapeJson(t.StartedAt));
                sb.AppendFormat("\"initialState\":\"{0}\",", EscapeJson(t.InitialState));
                sb.AppendFormat("\"status\":\"{0}\",", EscapeJson(t.OutcomeStatus));
                sb.AppendFormat("\"outcome\":\"{0}\",", EscapeJson(t.OutcomeSummary));
                sb.AppendFormat("\"testId\":\"{0}\",", EscapeJson(t.RelatedTestId));
                sb.Append("\"steps\":[");
                for (int j = 0; j < t.Steps.Count; j++)
                {
                    if (j > 0) sb.Append(",");
                    var s = t.Steps[j];
                    sb.Append("{");
                    sb.AppendFormat("\"offset\":\"{0}\",", EscapeJson(s.TimeOffset));
                    sb.AppendFormat("\"cat\":\"{0}\",", EscapeJson(s.Category));
                    sb.AppendFormat("\"msg\":\"{0}\",", EscapeJson(s.Message));
                    sb.AppendFormat("\"pos\":\"{0}\"", EscapeJson(s.Position));
                    sb.Append("}");
                }
                sb.Append("]");
                sb.Append("}");
            }
            sb.Append("]");

            sb.Append("}");
            return sb.ToString();
        }

        private static string EscapeJson(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            var sb = new StringBuilder(s.Length + 8);
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                switch (c)
                {
                    case '\"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\b': sb.Append("\\b"); break;
                    case '\f': sb.Append("\\f"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < ' ')
                        {
                            sb.AppendFormat("\\u{0:X4}", (int)c);
                        }
                        else
                        {
                            sb.Append(c);
                        }
                        break;
                }
            }
            return sb.ToString();
        }

        // ====================================================================
        // Lightweight Local Loopback HTTP Server (127.0.0.1:18788)
        // ====================================================================
        private static void StartHttpServer()
        {
            try
            {
                s_running = true;
                s_tcpListener = new TcpListener(IPAddress.Loopback, HttpPort);
                s_tcpListener.Start();
                s_listenerThread = new Thread(HttpServerLoop)
                {
                    IsBackground = true,
                    Name = "ValheimWorkers_TelemetryHttp"
                };
                s_listenerThread.Start();
            }
            catch (Exception ex)
            {
                ValheimWorkersPlugin.Log?.LogWarning($"[ValheimWorkers] Telemetry HTTP server could not bind port {HttpPort} (file sync still active): {ex.Message}");
            }
        }

        private static void StopHttpServer()
        {
            s_running = false;
            try
            {
                s_tcpListener?.Stop();
            }
            catch
            {
                // Ignore
            }
        }

        private static void HttpServerLoop()
        {
            while (s_running)
            {
                try
                {
                    TcpClient client = s_tcpListener.AcceptTcpClient();
                    ThreadPool.QueueUserWorkItem(HandleHttpClient, client);
                }
                catch
                {
                    if (!s_running) break;
                }
            }
        }

        private static void HandleHttpClient(object state)
        {
            using (TcpClient client = (TcpClient)state)
            {
                try
                {
                    client.ReceiveTimeout = 2000;
                    client.SendTimeout = 2000;
                    using (NetworkStream stream = client.GetStream())
                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8, false, 1024, true))
                    {
                        string requestLine = reader.ReadLine();
                        if (string.IsNullOrEmpty(requestLine)) return;

                        string[] parts = requestLine.Split(' ');
                        string method = parts.Length > 0 ? parts[0].ToUpperInvariant() : "GET";
                        string rawPath = parts.Length > 1 ? parts[1] : "/";
                        int qIdx = rawPath.IndexOf('?');
                        string path = qIdx >= 0 ? rawPath.Substring(0, qIdx) : rawPath;

                        if (method == "OPTIONS")
                        {
                            WriteHttpResponse(stream, "204 No Content", "text/plain", new byte[0]);
                            return;
                        }

                        if (path.Equals("/api/reset", StringComparison.OrdinalIgnoreCase))
                        {
                            if (method != "POST")
                            {
                                WriteHttpResponse(stream, "405 Method Not Allowed", "text/plain", Encoding.UTF8.GetBytes("POST method required."));
                                return;
                            }
                            ResetAll();
                            string json;
                            lock (s_lock)
                            {
                                json = s_cachedJson ?? "{}";
                            }
                            byte[] resetBody = Encoding.UTF8.GetBytes(json);
                            WriteHttpResponse(stream, "200 OK", "application/json; charset=utf-8", resetBody);
                            return;
                        }

                        if (path.Equals("/api/telemetry.js", StringComparison.OrdinalIgnoreCase))
                        {
                            string json;
                            lock (s_lock)
                            {
                                json = s_cachedJson ?? "{}";
                            }
                            string js = "window.__VALHEIM_TELEMETRY__ = " + json + ";";
                            WriteHttpResponse(stream, "200 OK", "application/javascript; charset=utf-8", Encoding.UTF8.GetBytes(js));
                            return;
                        }

                        if (path.Equals("/api/sync", StringComparison.OrdinalIgnoreCase))
                        {
                            lock (s_lock)
                            {
                                s_forceImmediateFlush = true;
                            }
                            string json;
                            lock (s_lock)
                            {
                                json = s_cachedJson ?? "{}";
                            }
                            byte[] body = Encoding.UTF8.GetBytes(json);
                            WriteHttpResponse(stream, "200 OK", "application/json; charset=utf-8", body);
                            return;
                        }

                        if (path.Equals("/api/telemetry", StringComparison.OrdinalIgnoreCase) ||
                            path.Equals("/telemetry", StringComparison.OrdinalIgnoreCase))
                        {
                            string json;
                            lock (s_lock)
                            {
                                json = s_cachedJson ?? "{}";
                            }
                            byte[] body = Encoding.UTF8.GetBytes(json);
                            WriteHttpResponse(stream, "200 OK", "application/json; charset=utf-8", body);
                            return;
                        }

                        // Serve manual_verification_checklist.html on "/"
                        foreach (var dir in s_targetDirectories)
                        {
                            string htmlFile = Path.Combine(dir, "manual_verification_checklist.html");
                            if (File.Exists(htmlFile))
                            {
                                byte[] htmlBytes = File.ReadAllBytes(htmlFile);
                                WriteHttpResponse(stream, "200 OK", "text/html; charset=utf-8", htmlBytes);
                                return;
                            }
                        }

                        string fallbackJson;
                        lock (s_lock)
                        {
                            fallbackJson = s_cachedJson ?? "{}";
                        }
                        byte[] fallbackBody = Encoding.UTF8.GetBytes(fallbackJson);
                        WriteHttpResponse(stream, "200 OK", "application/json; charset=utf-8", fallbackBody);
                    }
                }
                catch
                {
                    // Ignore client disconnect
                }
            }
        }

        private static void WriteHttpResponse(NetworkStream stream, string status, string contentType, byte[] body)
        {
            string headers =
                $"HTTP/1.1 {status}\r\n" +
                $"Content-Type: {contentType}\r\n" +
                $"Content-Length: {body.Length}\r\n" +
                "Access-Control-Allow-Origin: http://127.0.0.1:18788\r\n" +
                "Access-Control-Allow-Methods: GET, POST, OPTIONS\r\n" +
                "Cache-Control: no-store, no-cache, must-revalidate\r\n" +
                "Connection: close\r\n\r\n";

            byte[] headerBytes = Encoding.ASCII.GetBytes(headers);
            stream.Write(headerBytes, 0, headerBytes.Length);
            if (body.Length > 0)
            {
                stream.Write(body, 0, body.Length);
            }
            stream.Flush();
        }
    }
}
