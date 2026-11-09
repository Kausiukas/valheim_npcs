using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using UnityEngine;
using ValheimWorkers.AI;
using ValheimWorkers.Core;
using ValheimWorkers.Prefabs;

namespace ValheimWorkers.UI
{
    public class NpcLabGui : MonoBehaviour
    {
        public static NpcLabGui Instance { get; private set; }
        public static bool IsOpen { get; private set; } = false;

        public static void SetOpen(bool open)
        {
            if (IsOpen == open) return;
            IsOpen = open;
            if (open)
            {
                if (Instance != null && WorkerBrain.ActiveWorkers.Count > 0 && Instance._selectedTab == 0)
                {
                    Instance._selectedTab = 1; // Default to Active Roster to prevent accidental duplicate spawns
                }
            }
            else
            {
                PlayerController.SetTakeInputDelay(0.3f);
            }
        }

        public static void OpenWorkerInspector(WorkerBrain worker)
        {
            if (Instance == null || worker == null) return;
            Instance._selectedWorker = worker;
            Instance._selectedTab = 1; // Active Roster & Loadout
            SetOpen(true);
        }

        private Rect _windowRect = new Rect(80, 45, 960, 700);
        private enum ResizeZone
        {
            None,
            BottomRight,
            BottomLeft,
            BottomEdge,
            RightEdge,
            LeftEdge
        }

        private ResizeZone _activeResize = ResizeZone.None;
        private Vector2 _resizeStartMouse;
        private Rect _resizeStartWindow;

        private int _selectedTab = 0;
        private int _inspectorSubTab = 0; // 0 = Orders & Logs, 1 = Loadout & Inventory, 2 = Voice & Sounds

        private readonly string[] _tabTitles = new string[]
        {
            "Hire & Customize",
            "Active Roster & Loadout",
            "Proving Grounds"
        };

        private readonly string[] _inspectorSubTabs = new string[]
        {
            "Orders, Post & Action Log",
            "Weapons, Armor & Inventory",
            "Voice & Sound Settings",
            "Skills & Evolution"
        };

        // Creator State
        private readonly WorkerCustomization _creatorData = new WorkerCustomization();
        private int _hairIndex = 1;
        private int _beardIndex = 1;
        private int _hairColorPreset = 0;
        private int _skinColorPreset = 0;
        private Vector2 _creatorLeftScroll = Vector2.zero;
        private Vector2 _creatorRightScroll = Vector2.zero;

        // Roster & Inspector State
        private WorkerBrain _selectedWorker = null;
        private Vector2 _rosterScroll = Vector2.zero;
        private Vector2 _inspectorScroll = Vector2.zero;
        private Vector2 _npcInvScroll = Vector2.zero;
        private Vector2 _playerInvScroll = Vector2.zero;
        private Vector2 _actionLogScroll = new Vector2(0, 99999f);
        private bool _autoScrollLog = true;

        // Presets
        private static readonly string[] s_hairStyles = new string[]
        {
            "HairNone", "Hair1", "Hair2", "Hair3", "Hair4", "Hair5", "Hair6", "Hair7", "Hair8", "Hair9", "Hair10", "Hair11", "Hair12", "Hair13", "Hair14"
        };

        private static readonly string[] s_beardStyles = new string[]
        {
            "BeardNone", "Beard1", "Beard2", "Beard3", "Beard4", "Beard5", "Beard6", "Beard7", "Beard8", "Beard9", "Beard10"
        };

        private static readonly Vector3[] s_hairColorPresets = new Vector3[]
        {
            new Vector3(0.92f, 0.78f, 0.45f), // Blonde
            new Vector3(0.75f, 0.35f, 0.20f), // Auburn / Red
            new Vector3(0.40f, 0.25f, 0.15f), // Chestnut Brown
            new Vector3(0.12f, 0.12f, 0.12f), // Raven Black
            new Vector3(0.85f, 0.85f, 0.88f)  // Silver / White
        };
        private static readonly string[] s_hairColorNames = new string[] { "Blonde", "Auburn", "Chestnut", "Raven", "Silver" };

        private static readonly Vector3[] s_skinColorPresets = new Vector3[]
        {
            new Vector3(0.95f, 0.82f, 0.72f), // Nordic Fair
            new Vector3(0.85f, 0.70f, 0.58f), // Tanned
            new Vector3(0.70f, 0.55f, 0.45f), // Weathered
            new Vector3(0.50f, 0.38f, 0.30f)  // Dark
        };
        private static readonly string[] s_skinColorNames = new string[] { "Fair", "Tanned", "Weathered", "Dark" };

        private static readonly string[] s_weapons = new string[] { "Club", "AxeBronze", "SwordBronze", "SwordIron", "PickaxeAntler", "" };
        private static readonly string[] s_weaponNames = new string[] { "Club", "Bronze Axe", "Bronze Sword", "Iron Sword", "Antler Pick", "Unarmed" };

        private static readonly string[] s_shields = new string[] { "", "ShieldWood", "ShieldBronzeBuckler", "ShieldBanded" };
        private static readonly string[] s_shieldNames = new string[] { "No Shield", "Wood Shield", "Bronze Buckler", "Banded Shield" };

        private static readonly string[] s_armorTiers = new string[] { "Leather", "Rags", "Bronze", "Iron", "None" };

        private static readonly string[] s_voiceProfiles = new string[] { "Human", "Dvergr", "Skeleton", "Silent" };
        private static readonly string[] s_chatterModes = new string[] { "Off (Quiet)", "Rare (35s)", "Frequent (12s)" };

        private void Awake()
        {
            Instance = this;
            float w = Mathf.Min(Screen.width - 40f, 980f);
            float h = Mathf.Min(Screen.height - 40f, 720f);
            _windowRect = new Rect((Screen.width - w) / 2f, Mathf.Max(20f, (Screen.height - h) / 2f), w, h);
        }

        private void Update()
        {
            if (Input.GetKeyDown(ValheimWorkersPlugin.ToggleUiHotkey.Value))
            {
                SetOpen(!IsOpen);
            }

            if (IsOpen && Input.GetKeyDown(KeyCode.Escape))
            {
                SetOpen(false);
            }

            if (!IsOpen)
            {
                Patches.BloodSacrificePatches.HandlePlayerRelicInputs();
            }
        }

        public void SetWindowPreset(float targetW, float targetH, float fontScale = -1f)
        {
            float w = Mathf.Clamp(targetW, 760f, Screen.width - 20f);
            float h = Mathf.Clamp(targetH, 540f, Screen.height - 20f);
            _windowRect.width = w;
            _windowRect.height = h;
            _windowRect.x = Mathf.Clamp((Screen.width - w) / 2f, 10f, Mathf.Max(10f, Screen.width - w - 10f));
            _windowRect.y = Mathf.Clamp((Screen.height - h) / 2f, 10f, Mathf.Max(10f, Screen.height - h - 10f));

            if (fontScale > 0f)
            {
                ValheimGuiTheme.SetFontScale(fontScale);
            }
            else
            {
                // Proportional baseline: 980x720 baseline is 1.15f
                float computed = Mathf.Clamp((w / 980f) * 1.12f, 0.90f, 1.55f);
                ValheimGuiTheme.SetFontScale(computed);
            }

            VerificationTelemetry.RecordPass("gui_presets", $"Applied F8 window size preset ({w:F0}x{h:F0}px, scale: {ValheimGuiTheme.FontScale:F2}).");
        }

        private void OnGUI()
        {
            if (!IsOpen) return;

            ValheimGuiTheme.EnsureInitialized();

            ProcessScreenSpaceResize();

            _windowRect = GUI.Window(
                883921,
                _windowRect,
                DrawWindow,
                "< SETTLEMENT HALL >",
                ValheimGuiTheme.WindowStyle
            );
        }

        private void ProcessScreenSpaceResize()
        {
            Event ev = Event.current;
            Vector2 mousePos = ev.mousePosition;

            float minW = Mathf.Clamp(760f * ValheimGuiTheme.FontScale, 720f, Screen.width - 20f);
            float minH = Mathf.Clamp(540f * ValheimGuiTheme.FontScale, 500f, Screen.height - 20f);

            float handleSize = 34f;
            Rect brRect = new Rect(_windowRect.xMax - handleSize, _windowRect.yMax - handleSize, handleSize, handleSize);
            Rect blRect = new Rect(_windowRect.xMin, _windowRect.yMax - handleSize, handleSize, handleSize);
            Rect bEdgeRect = new Rect(_windowRect.xMin + handleSize, _windowRect.yMax - 8f, _windowRect.width - (handleSize * 2f), 12f);
            Rect rEdgeRect = new Rect(_windowRect.xMax - 8f, _windowRect.yMin + 34f, 12f, _windowRect.height - 40f);
            Rect lEdgeRect = new Rect(_windowRect.xMin - 4f, _windowRect.yMin + 34f, 12f, _windowRect.height - 40f);

            if (ev.type == EventType.MouseDown && ev.button == 0)
            {
                if (brRect.Contains(mousePos))
                {
                    _activeResize = ResizeZone.BottomRight;
                    _resizeStartMouse = mousePos;
                    _resizeStartWindow = _windowRect;
                    ev.Use();
                }
                else if (blRect.Contains(mousePos))
                {
                    _activeResize = ResizeZone.BottomLeft;
                    _resizeStartMouse = mousePos;
                    _resizeStartWindow = _windowRect;
                    ev.Use();
                }
                else if (bEdgeRect.Contains(mousePos))
                {
                    _activeResize = ResizeZone.BottomEdge;
                    _resizeStartMouse = mousePos;
                    _resizeStartWindow = _windowRect;
                    ev.Use();
                }
                else if (rEdgeRect.Contains(mousePos))
                {
                    _activeResize = ResizeZone.RightEdge;
                    _resizeStartMouse = mousePos;
                    _resizeStartWindow = _windowRect;
                    ev.Use();
                }
                else if (lEdgeRect.Contains(mousePos))
                {
                    _activeResize = ResizeZone.LeftEdge;
                    _resizeStartMouse = mousePos;
                    _resizeStartWindow = _windowRect;
                    ev.Use();
                }
            }
            else if (ev.type == EventType.MouseUp || !Input.GetMouseButton(0))
            {
                _activeResize = ResizeZone.None;
            }

            if (_activeResize != ResizeZone.None && Input.GetMouseButton(0))
            {
                Vector2 delta = mousePos - _resizeStartMouse;

                switch (_activeResize)
                {
                    case ResizeZone.BottomRight:
                        _windowRect.width = Mathf.Clamp(_resizeStartWindow.width + delta.x, minW, Screen.width - _windowRect.x - 10f);
                        _windowRect.height = Mathf.Clamp(_resizeStartWindow.height + delta.y, minH, Screen.height - _windowRect.y - 10f);
                        if (delta.magnitude > 12f)
                        {
                            VerificationTelemetry.RecordPass("gui_resize_br", $"Resized F8 window via Bottom-Right grip (◢) to {_windowRect.width:F0}x{_windowRect.height:F0}px.");
                        }
                        break;

                    case ResizeZone.BottomLeft:
                        float newW = Mathf.Clamp(_resizeStartWindow.width - delta.x, minW, _resizeStartWindow.xMax - 10f);
                        _windowRect.x = _resizeStartWindow.xMax - newW;
                        _windowRect.width = newW;
                        _windowRect.height = Mathf.Clamp(_resizeStartWindow.height + delta.y, minH, Screen.height - _windowRect.y - 10f);
                        if (delta.magnitude > 12f)
                        {
                            VerificationTelemetry.RecordPass("gui_resize_bl", $"Resized F8 window via Bottom-Left grip (◣) to {_windowRect.width:F0}x{_windowRect.height:F0}px.");
                        }
                        break;

                    case ResizeZone.BottomEdge:
                        _windowRect.height = Mathf.Clamp(_resizeStartWindow.height + delta.y, minH, Screen.height - _windowRect.y - 10f);
                        break;

                    case ResizeZone.RightEdge:
                        _windowRect.width = Mathf.Clamp(_resizeStartWindow.width + delta.x, minW, Screen.width - _windowRect.x - 10f);
                        break;

                    case ResizeZone.LeftEdge:
                        float newWidth = Mathf.Clamp(_resizeStartWindow.width - delta.x, minW, _resizeStartWindow.xMax - 10f);
                        _windowRect.x = _resizeStartWindow.xMax - newWidth;
                        _windowRect.width = newWidth;
                        break;
                }

                if (ev.type == EventType.MouseDrag) ev.Use();
            }
        }

        private void AdjustUiScale(float delta)
        {
            float oldScale = ValheimGuiTheme.FontScale;
            float newScale = Mathf.Clamp(oldScale + delta, 0.85f, 1.65f);
            ValheimGuiTheme.SetFontScale(newScale);

            float targetW = Mathf.Clamp(860f * newScale, 780f, Screen.width - 20f);
            float targetH = Mathf.Clamp(630f * newScale, 560f, Screen.height - 20f);
            _windowRect.width = Mathf.Max(_windowRect.width, targetW);
            _windowRect.height = Mathf.Max(_windowRect.height, targetH);
            _windowRect.x = Mathf.Clamp(_windowRect.x, 10f, Mathf.Max(10f, Screen.width - _windowRect.width - 10f));
            _windowRect.y = Mathf.Clamp(_windowRect.y, 10f, Mathf.Max(10f, Screen.height - _windowRect.height - 10f));
        }

        private void DrawWindow(int windowId)
        {
            float btnH = ValheimGuiTheme.ScaledF(32f);

            // Top Navigation Tabs + Window Presets + Font Size Controls + Close Button
            GUILayout.BeginHorizontal();
            for (int i = 0; i < _tabTitles.Length; i++)
            {
                GUIStyle tabStyle = (_selectedTab == i) ? ValheimGuiTheme.TabActiveStyle : ValheimGuiTheme.TabInactiveStyle;
                if (GUILayout.Button(_tabTitles[i], tabStyle, GUILayout.Height(btnH)))
                {
                    _selectedTab = i;
                }
            }

            GUILayout.Space(6);
            // Window Size Presets (🗗 = Compact, 🗖 = Standard, ⛶ = MaxFit)
            if (GUILayout.Button("🗗", ValheimGuiTheme.ButtonNormal, GUILayout.Width(ValheimGuiTheme.ScaledF(32f)), GUILayout.Height(btnH)))
            {
                SetWindowPreset(780f, 580f, 0.95f);
            }
            if (GUILayout.Button("🗖", ValheimGuiTheme.ButtonNormal, GUILayout.Width(ValheimGuiTheme.ScaledF(32f)), GUILayout.Height(btnH)))
            {
                SetWindowPreset(980f, 720f, 1.12f);
            }
            if (GUILayout.Button("⛶", ValheimGuiTheme.ButtonNormal, GUILayout.Width(ValheimGuiTheme.ScaledF(32f)), GUILayout.Height(btnH)))
            {
                float fitW = Screen.width - 40f;
                float fitH = Screen.height - 40f;
                float fitScale = Mathf.Clamp(Mathf.Min(fitW / 1100f, fitH / 800f) * 1.10f, 1.15f, 1.55f);
                SetWindowPreset(fitW, fitH, fitScale);
            }

            GUILayout.Space(4);
            // Font Scale Controls
            if (GUILayout.Button("A-", ValheimGuiTheme.ButtonNormal, GUILayout.Width(ValheimGuiTheme.ScaledF(32f)), GUILayout.Height(btnH)))
            {
                AdjustUiScale(-0.10f);
            }
            GUILayout.Label($"{Mathf.RoundToInt(ValheimGuiTheme.FontScale * 100f)}%", ValheimGuiTheme.LabelGold, GUILayout.Width(ValheimGuiTheme.ScaledF(44f)), GUILayout.Height(btnH));
            if (GUILayout.Button("A+", ValheimGuiTheme.ButtonNormal, GUILayout.Width(ValheimGuiTheme.ScaledF(32f)), GUILayout.Height(btnH)))
            {
                AdjustUiScale(+0.10f);
            }

            GUILayout.Space(6);
            if (GUILayout.Button("Close [Esc]", ValheimGuiTheme.ButtonDanger, GUILayout.Width(ValheimGuiTheme.ScaledF(95f)), GUILayout.Height(btnH)))
            {
                SetOpen(false);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);

            switch (_selectedTab)
            {
                case 0:
                    DrawTabCreator();
                    break;
                case 1:
                    DrawTabRoster();
                    break;
                case 2:
                    DrawTabSandbox();
                    break;
            }

            HandleWindowResize();
            GUI.DragWindow(new Rect(0, 0, _windowRect.width - 30f, 34f));
        }

        private void HandleWindowResize()
        {
            // Bottom-Left and Bottom-Right corner icons
            Rect blCorner = new Rect(6f, _windowRect.height - 24f, 24f, 20f);
            Rect brCorner = new Rect(_windowRect.width - 24f, _windowRect.height - 24f, 20f, 20f);
            GUI.Label(blCorner, "◣", ValheimGuiTheme.LabelGold);
            GUI.Label(brCorner, "◢", ValheimGuiTheme.LabelGold);
        }

        // ==========================================
        // TAB 1: HIRE & CUSTOMIZE
        // ==========================================
        private void DrawTabCreator()
        {
            GUILayout.BeginHorizontal();

            float leftColW = Mathf.Clamp(_windowRect.width * 0.48f, 360f, 900f);

            // LEFT COLUMN: Visage, Heritage & Voice
            GUILayout.BeginVertical(ValheimGuiTheme.CardStyle, GUILayout.Width(leftColW), GUILayout.ExpandHeight(true));
            _creatorLeftScroll = GUILayout.BeginScrollView(_creatorLeftScroll);

            GUILayout.Label("< IDENTITY, VISAGE & VOICE >", ValheimGuiTheme.CardHeaderStyle);
            ValheimGuiTheme.DrawDivider();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Worker Name:", ValheimGuiTheme.LabelParchment, GUILayout.Width(ValheimGuiTheme.ScaledF(105f)));
            _creatorData.Name = GUILayout.TextField(_creatorData.Name, 24, ValheimGuiTheme.TextFieldStyle, GUILayout.Height(ValheimGuiTheme.ScaledF(26f)));
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            GUILayout.Label("Body Heritage", ValheimGuiTheme.SubHeaderStyle);

            bool dvergrUnlocked = WorkerProgressionManager.IsHeritageUnlocked(WorkerHeritage.Dvergr);
            bool skelUnlocked = WorkerProgressionManager.IsHeritageUnlocked(WorkerHeritage.Skeleton);

            GUILayout.BeginHorizontal();
            if (DrawPillButton("Viking Male", _creatorData.BasePrefab == "Worker_Viking" && _creatorData.ModelIndex == 0))
            {
                _creatorData.BasePrefab = "Worker_Viking";
                _creatorData.ModelIndex = 0;
                _creatorData.VoiceProfile = "Human";
            }
            if (DrawPillButton("Viking Female", _creatorData.BasePrefab == "Worker_Viking" && _creatorData.ModelIndex == 1))
            {
                _creatorData.BasePrefab = "Worker_Viking";
                _creatorData.ModelIndex = 1;
                _creatorData.VoiceProfile = "Human";
            }
            string dvergrLabel = dvergrUnlocked ? "Dvergr" : "Dvergr 🔒";
            if (DrawPillButton(dvergrLabel, _creatorData.BasePrefab == "Worker_Dverger"))
            {
                if (dvergrUnlocked)
                {
                    _creatorData.BasePrefab = "Worker_Dverger";
                    _creatorData.VoiceProfile = "Dvergr";
                }
                else
                {
                    Player.m_localPlayer?.Message(MessageHud.MessageType.Center, "Dvergr Guild Locked: Recover the Guild Seal from a Troll Cave or power a Black Forest Survey Beacon.");
                }
            }
            string skelLabel = skelUnlocked ? "Skeleton" : "Skeleton 🔒";
            if (DrawPillButton(skelLabel, _creatorData.BasePrefab == "Worker_Skeleton"))
            {
                if (skelUnlocked)
                {
                    _creatorData.BasePrefab = "Worker_Skeleton";
                    _creatorData.VoiceProfile = "Skeleton";
                }
                else
                {
                    Player.m_localPlayer?.Message(MessageHud.MessageType.Center, "Skeleton Ritual Locked: Uncover the Sacrificial Dagger in a Hero's Grave, then spill animal blood over bones.");
                }
            }
            GUILayout.EndHorizontal();

            if (!skelUnlocked || !dvergrUnlocked)
            {
                GUILayout.Space(4);
                GUILayout.BeginVertical(ValheimGuiTheme.CardStyle);
                GUILayout.Label("< LORE & LINEAGE DISCOVERY CLUES >", ValheimGuiTheme.SubHeaderStyle);
                if (!skelUnlocked)
                {
                    GUILayout.Label("💀 <b>Skeleton Servants:</b> <i>Uncover the Barrow of Skarde in a Black Forest crypt. His Sacrificial Dagger binds animal blood to bone fragments.</i>", ValheimGuiTheme.LabelMuted);
                }
                if (!dvergrUnlocked)
                {
                    GUILayout.Label("⛏️ <b>Dvergr Laborers:</b> <i>Slay the Troll in a Black Forest cave to reclaim the Grand Guild Seal, or power a broken Survey Beacon with Greydwarf Eyes.</i>", ValheimGuiTheme.LabelMuted);
                }
                GUILayout.EndVertical();
            }

            GUILayout.Space(6);
            GUILayout.Label("Hair & Beard Style", ValheimGuiTheme.SubHeaderStyle);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Hair:", ValheimGuiTheme.LabelParchment, GUILayout.Width(ValheimGuiTheme.ScaledF(55f)));
            if (GUILayout.Button("<", ValheimGuiTheme.ButtonNormal, GUILayout.Width(32), GUILayout.Height(26)))
            {
                _hairIndex = (_hairIndex - 1 + s_hairStyles.Length) % s_hairStyles.Length;
            }
            GUILayout.Label(s_hairStyles[_hairIndex], ValheimGuiTheme.LabelGold, GUILayout.Width(ValheimGuiTheme.ScaledF(110f)));
            if (GUILayout.Button(">", ValheimGuiTheme.ButtonNormal, GUILayout.Width(32), GUILayout.Height(26)))
            {
                _hairIndex = (_hairIndex + 1) % s_hairStyles.Length;
            }
            _creatorData.HairStyle = s_hairStyles[_hairIndex];
            GUILayout.EndHorizontal();

            GUILayout.Space(3);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Beard:", ValheimGuiTheme.LabelParchment, GUILayout.Width(ValheimGuiTheme.ScaledF(55f)));
            if (GUILayout.Button("<", ValheimGuiTheme.ButtonNormal, GUILayout.Width(32), GUILayout.Height(26)))
            {
                _beardIndex = (_beardIndex - 1 + s_beardStyles.Length) % s_beardStyles.Length;
            }
            GUILayout.Label(s_beardStyles[_beardIndex], ValheimGuiTheme.LabelGold, GUILayout.Width(ValheimGuiTheme.ScaledF(110f)));
            if (GUILayout.Button(">", ValheimGuiTheme.ButtonNormal, GUILayout.Width(32), GUILayout.Height(26)))
            {
                _beardIndex = (_beardIndex + 1) % s_beardStyles.Length;
            }
            _creatorData.BeardStyle = s_beardStyles[_beardIndex];
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            GUILayout.Label("Hair Tone", ValheimGuiTheme.SubHeaderStyle);
            GUILayout.BeginHorizontal();
            for (int i = 0; i < s_hairColorNames.Length; i++)
            {
                if (DrawPillButton(s_hairColorNames[i], _hairColorPreset == i))
                {
                    _hairColorPreset = i;
                    _creatorData.HairColor = s_hairColorPresets[i];
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            GUILayout.Label("Skin Complexion", ValheimGuiTheme.SubHeaderStyle);
            GUILayout.BeginHorizontal();
            for (int i = 0; i < s_skinColorNames.Length; i++)
            {
                if (DrawPillButton(s_skinColorNames[i], _skinColorPreset == i))
                {
                    _skinColorPreset = i;
                    _creatorData.SkinColor = s_skinColorPresets[i];
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(8);
            ValheimGuiTheme.DrawDivider();
            DrawSoundSettingsBlock(_creatorData, null);

            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            // RIGHT COLUMN: Equipment, Stats & Spawn
            GUILayout.BeginVertical(ValheimGuiTheme.CardStyle, GUILayout.ExpandHeight(true));
            _creatorRightScroll = GUILayout.BeginScrollView(_creatorRightScroll);

            GUILayout.Label("< EQUIPMENT & ORDERS >", ValheimGuiTheme.CardHeaderStyle);
            ValheimGuiTheme.DrawDivider();

            DrawLoadoutSelectorBlock(_creatorData, null);

            GUILayout.Space(8);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Max Vitality:", ValheimGuiTheme.LabelParchment, GUILayout.Width(ValheimGuiTheme.ScaledF(125f)));
            GUILayout.Label($"<color=#FFA726><b>{_creatorData.MaxHealth:F0} HP</b></color>", ValheimGuiTheme.LabelParchment, GUILayout.Width(ValheimGuiTheme.ScaledF(75f)));
            _creatorData.MaxHealth = GUILayout.HorizontalSlider(_creatorData.MaxHealth, 50f, 500f, ValheimGuiTheme.SliderStyle, ValheimGuiTheme.SliderThumbStyle);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Patrol / Work Radius:", ValheimGuiTheme.LabelParchment, GUILayout.Width(ValheimGuiTheme.ScaledF(125f)));
            GUILayout.Label($"<color=#FFA726><b>{_creatorData.WorkRadius:F0} m</b></color>", ValheimGuiTheme.LabelParchment, GUILayout.Width(ValheimGuiTheme.ScaledF(75f)));
            _creatorData.WorkRadius = GUILayout.HorizontalSlider(_creatorData.WorkRadius, 5f, 150f, ValheimGuiTheme.SliderStyle, ValheimGuiTheme.SliderThumbStyle);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Inventory Return Threshold:", ValheimGuiTheme.LabelParchment, GUILayout.Width(ValheimGuiTheme.ScaledF(150f)));
            GUILayout.Label($"<color=#FFA726><b>{_creatorData.InventoryReturnThreshold * 100f:F0}%</b></color>", ValheimGuiTheme.LabelParchment, GUILayout.Width(ValheimGuiTheme.ScaledF(50f)));
            _creatorData.InventoryReturnThreshold = GUILayout.HorizontalSlider(_creatorData.InventoryReturnThreshold, 0.20f, 1.0f, ValheimGuiTheme.SliderStyle, ValheimGuiTheme.SliderThumbStyle);
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            GUILayout.Label("Initial Work Order", ValheimGuiTheme.SubHeaderStyle);
            GUILayout.BeginHorizontal();
            if (DrawPillButton("Gatherer", _creatorData.InitialRole == WorkerRole.Gatherer)) _creatorData.InitialRole = WorkerRole.Gatherer;
            if (DrawPillButton("Hauler", _creatorData.InitialRole == WorkerRole.Hauler)) _creatorData.InitialRole = WorkerRole.Hauler;
            if (DrawPillButton("Base Tender", _creatorData.InitialRole == WorkerRole.BaseTender)) _creatorData.InitialRole = WorkerRole.BaseTender;
            if (DrawPillButton("Guard", _creatorData.InitialRole == WorkerRole.Guard)) _creatorData.InitialRole = WorkerRole.Guard;
            if (DrawPillButton("Idle", _creatorData.InitialRole == WorkerRole.Idle)) _creatorData.InitialRole = WorkerRole.Idle;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (DrawPillButton("Lumberjack (Wood)", _creatorData.InitialRole == WorkerRole.Lumberjack)) _creatorData.InitialRole = WorkerRole.Lumberjack;
            if (DrawPillButton("Miner (Stone/Ore)", _creatorData.InitialRole == WorkerRole.Miner)) _creatorData.InitialRole = WorkerRole.Miner;
            if (DrawPillButton("Hunter (Game/Meat)", _creatorData.InitialRole == WorkerRole.Hunter)) _creatorData.InitialRole = WorkerRole.Hunter;
            if (DrawPillButton("Farmer (Crops)", _creatorData.InitialRole == WorkerRole.Farmer)) _creatorData.InitialRole = WorkerRole.Farmer;
            if (DrawPillButton("Cook (Kitchen)", _creatorData.InitialRole == WorkerRole.Cook)) _creatorData.InitialRole = WorkerRole.Cook;
            GUILayout.EndHorizontal();

            if (_creatorData.InitialRole == WorkerRole.Gatherer || _creatorData.InitialRole == WorkerRole.Lumberjack || _creatorData.InitialRole == WorkerRole.Miner || _creatorData.InitialRole == WorkerRole.Hunter)
            {
                DrawResourceDesignationBlock(null, _creatorData);
            }

            GUILayout.Space(12);
            GUILayout.Label("<color=#FFA94D><b>ℹ Note:</b> This tab hires a <b>NEW</b> worker. To reassign or inspect existing workers, switch to <b>Tab 2: Active Roster</b> above.</color>", ValheimGuiTheme.LabelMuted);
            GUILayout.Space(4);
            if (GUILayout.Button($"Hire & Spawn NEW Worker (+1) [Realm Workers: {WorkerBrain.ActiveWorkers.Count}]", ValheimGuiTheme.ButtonPrimary, GUILayout.Height(ValheimGuiTheme.ScaledF(42f))))
            {
                SpawnConfiguredWorker();
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }

        private void DrawLoadoutSelectorBlock(WorkerCustomization config, WorkerBrain liveWorker)
        {
            GUILayout.Label("Main Hand Weapon / Tool", ValheimGuiTheme.SubHeaderStyle);
            GUILayout.BeginHorizontal();
            for (int i = 0; i < 3; i++)
            {
                if (DrawPillButton(s_weaponNames[i], config.Weapon == s_weapons[i]))
                {
                    config.Weapon = s_weapons[i];
                    if (liveWorker != null) liveWorker.UpdateLiveLoadout(config.Weapon, config.Shield, config.Helmet, config.Chest, config.Legs);
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            for (int i = 3; i < s_weaponNames.Length; i++)
            {
                if (DrawPillButton(s_weaponNames[i], config.Weapon == s_weapons[i]))
                {
                    config.Weapon = s_weapons[i];
                    if (liveWorker != null) liveWorker.UpdateLiveLoadout(config.Weapon, config.Shield, config.Helmet, config.Chest, config.Legs);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            GUILayout.Label("Off-Hand Shield", ValheimGuiTheme.SubHeaderStyle);
            GUILayout.BeginHorizontal();
            for (int i = 0; i < s_shields.Length; i++)
            {
                if (DrawPillButton(s_shieldNames[i], config.Shield == s_shields[i]))
                {
                    config.Shield = s_shields[i];
                    if (liveWorker != null) liveWorker.UpdateLiveLoadout(config.Weapon, config.Shield, config.Helmet, config.Chest, config.Legs);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            GUILayout.Label("Armor Set", ValheimGuiTheme.SubHeaderStyle);
            GUILayout.BeginHorizontal();
            foreach (var tier in s_armorTiers)
            {
                bool isCurrent = (!string.IsNullOrEmpty(config.Chest) && config.Chest.Contains(tier)) || (string.IsNullOrEmpty(config.Chest) && tier == "None");
                if (DrawPillButton(tier, isCurrent))
                {
                    ApplyArmorTier(config, tier);
                    if (liveWorker != null) liveWorker.UpdateLiveLoadout(config.Weapon, config.Shield, config.Helmet, config.Chest, config.Legs);
                }
            }
            GUILayout.EndHorizontal();
        }

        private void DrawSoundSettingsBlock(WorkerCustomization config, WorkerBrain liveWorker)
        {
            GUILayout.Label("Voice & Sound Profile", ValheimGuiTheme.SubHeaderStyle);
            GUILayout.BeginHorizontal();
            foreach (var vp in s_voiceProfiles)
            {
                if (DrawPillButton(vp, config.VoiceProfile == vp))
                {
                    config.VoiceProfile = vp;
                    if (liveWorker != null)
                    {
                        liveWorker.ApplySoundSettings();
                        liveWorker.SaveToZDO();
                        liveWorker.LogAction($"Voice profile set to '{vp}'", true);
                    }
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(4);
            GUILayout.Label("Idle Grunts / Chatter Frequency", ValheimGuiTheme.SubHeaderStyle);
            GUILayout.BeginHorizontal();
            for (int i = 0; i < s_chatterModes.Length; i++)
            {
                if (DrawPillButton(s_chatterModes[i], config.IdleChatterMode == i))
                {
                    config.IdleChatterMode = i;
                    if (liveWorker != null)
                    {
                        liveWorker.ApplySoundSettings();
                        liveWorker.SaveToZDO();
                        liveWorker.LogAction($"Idle chatter mode set to '{s_chatterModes[i]}'", true);
                    }
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(4);
            GUILayout.BeginHorizontal();
            if (DrawPillButton(config.FootstepsEnabled ? "Footsteps: ON" : "Footsteps: OFF", config.FootstepsEnabled))
            {
                config.FootstepsEnabled = !config.FootstepsEnabled;
                if (liveWorker != null) { liveWorker.ApplySoundSettings(); liveWorker.SaveToZDO(); }
            }
            if (DrawPillButton(config.CombatShoutsEnabled ? "Combat Shouts: ON" : "Combat Shouts: OFF", config.CombatShoutsEnabled))
            {
                config.CombatShoutsEnabled = !config.CombatShoutsEnabled;
                if (liveWorker != null) { liveWorker.ApplySoundSettings(); liveWorker.SaveToZDO(); }
            }
            if (DrawPillButton(config.SpeechBubblesEnabled ? "Speech Text: ON" : "Speech Text: OFF", config.SpeechBubblesEnabled))
            {
                config.SpeechBubblesEnabled = !config.SpeechBubblesEnabled;
                if (liveWorker != null) { liveWorker.ApplySoundSettings(); liveWorker.SaveToZDO(); }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(4);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Test Voice / Alert Sound", ValheimGuiTheme.ButtonNormal))
            {
                if (liveWorker != null)
                    liveWorker.PlayTestSound(false);
                else if (Player.m_localPlayer != null)
                    WorkerBrain.PlayPreviewSound(config, Player.m_localPlayer.transform.position, Player.m_localPlayer.transform.rotation, Player.m_localPlayer.transform, false);
            }
            if (GUILayout.Button("Test Hurt / Hit Sound", ValheimGuiTheme.ButtonNormal))
            {
                if (liveWorker != null)
                    liveWorker.PlayTestSound(true);
                else if (Player.m_localPlayer != null)
                    WorkerBrain.PlayPreviewSound(config, Player.m_localPlayer.transform.position, Player.m_localPlayer.transform.rotation, Player.m_localPlayer.transform, true);
            }
            GUILayout.EndHorizontal();
        }

        private void ApplyArmorTier(WorkerCustomization config, string tier)
        {
            switch (tier)
            {
                case "Leather":
                    config.Chest = "ArmorLeatherChest";
                    config.Legs = "ArmorLeatherLegs";
                    config.Helmet = "HelmetLeather";
                    break;
                case "Bronze":
                    config.Chest = "ArmorBronzeChest";
                    config.Legs = "ArmorBronzeLegs";
                    config.Helmet = "HelmetBronze";
                    break;
                case "Iron":
                    config.Chest = "ArmorIronChest";
                    config.Legs = "ArmorIronLegs";
                    config.Helmet = "HelmetIron";
                    break;
                case "Rags":
                    config.Chest = "ArmorRagsChest";
                    config.Legs = "ArmorRagsLegs";
                    config.Helmet = "";
                    break;
                default:
                    config.Chest = "";
                    config.Legs = "";
                    config.Helmet = "";
                    break;
            }
        }

        private void SpawnConfiguredWorker()
        {
            if (Player.m_localPlayer == null) return;

            // Enforce hire cost in survival mode unless DevMode is enabled
            var arch = WorkerCatalog.Get(_creatorData.BasePrefab) ?? WorkerCatalog.Get("Worker_Viking");
            if (arch != null && !ValheimWorkersPlugin.DevMode.Value)
            {
                var playerInv = Player.m_localPlayer.GetInventory();
                if (playerInv != null && !string.IsNullOrEmpty(arch.HireCostItem) && arch.HireCostAmount > 0)
                {
                    int have = playerInv.CountItems(arch.HireCostItem);
                    if (have < arch.HireCostAmount)
                    {
                        Player.m_localPlayer.Message(MessageHud.MessageType.Center, $"Need {arch.HireCostAmount}x {arch.HireCostItem} to recruit {arch.DisplayName} (you have {have})");
                        return;
                    }
                    playerInv.RemoveItem(arch.HireCostItem, arch.HireCostAmount);
                    Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, $"Paid {arch.HireCostAmount}x {arch.HireCostItem} to recruit {arch.DisplayName}.");
                    VerificationTelemetry.RecordPass("economy_survival_hire_cost", $"Paid {arch.HireCostAmount}x {arch.HireCostItem} from player inventory to recruit {arch.DisplayName}.");
                }
            }

            Vector3 spawnPos = Player.m_localPlayer.transform.position + Player.m_localPlayer.transform.forward * 2.5f;
            Quaternion spawnRot = Player.m_localPlayer.transform.rotation;

            GameObject go = WorkerPrefabManager.SpawnCustomWorker(_creatorData, spawnPos, spawnRot);
            if (go != null)
            {
                _selectedWorker = go.GetComponent<WorkerBrain>();
                _selectedWorker?.StartCommandTrace("HIRE_SPAWN", $"Spawned new worker '{_creatorData.Name}' ({_creatorData.InitialRole}) with loadout: Weapon='{_creatorData.Weapon}', Shield='{_creatorData.Shield}', Armor='{_creatorData.Chest}'", "interaction_loadout_retention");
            }
            SetOpen(false);
        }

        // ==========================================
        // TAB 2: ACTIVE ROSTER, LOADOUT, INVENTORY & LOGS
        // ==========================================
        private void DrawTabRoster()
        {
            GUILayout.BeginHorizontal();

            float rosterW = Mathf.Clamp(_windowRect.width * 0.32f, 250f, 520f);

            // LEFT PANE: Active Workers List
            GUILayout.BeginVertical(ValheimGuiTheme.CardStyle, GUILayout.Width(rosterW), GUILayout.ExpandHeight(true));
            GUILayout.Label($"< ROSTER ({WorkerBrain.ActiveWorkers.Count}) >", ValheimGuiTheme.CardHeaderStyle);
            ValheimGuiTheme.DrawDivider();

            _rosterScroll = GUILayout.BeginScrollView(_rosterScroll);
            if (WorkerBrain.ActiveWorkers.Count == 0)
            {
                GUILayout.Label("No active workers in the realm.\nUse 'Hire & Customize' to spawn one.", ValheimGuiTheme.LabelMuted);
            }
            else
            {
                if (_selectedWorker == null && WorkerBrain.ActiveWorkers.Count > 0)
                {
                    _selectedWorker = WorkerBrain.ActiveWorkers[0];
                }

                for (int i = 0; i < WorkerBrain.ActiveWorkers.Count; i++)
                {
                    var w = WorkerBrain.ActiveWorkers[i];
                    if (w == null) continue;

                    string name = w.GetHoverName();
                    float dist = Player.m_localPlayer != null ? Vector3.Distance(w.transform.position, Player.m_localPlayer.transform.position) : 0f;
                    string label = $"{i + 1}. {name} [{w.CurrentRole}] ({dist:F0}m)";

                    bool isSelected = (_selectedWorker == w);
                    GUIStyle rowStyle = isSelected ? ValheimGuiTheme.ButtonListRowSelected : ValheimGuiTheme.ButtonListRow;
                    if (GUILayout.Button(label, rowStyle, GUILayout.Height(ValheimGuiTheme.ScaledF(30f))))
                    {
                        _selectedWorker = w;
                    }
                    GUILayout.Space(2);
                }
            }
            GUILayout.EndScrollView();

            GUILayout.Label("Tip: Hold [E] on any worker in-world to open their Loadout & Inventory directly!", ValheimGuiTheme.LabelMuted);
            GUILayout.EndVertical();

            // RIGHT PANE: Selected Worker Inspector
            GUILayout.BeginVertical(ValheimGuiTheme.CardStyle, GUILayout.ExpandHeight(true));
            if (_selectedWorker == null)
            {
                GUILayout.Label("< WORKER INSPECTOR >", ValheimGuiTheme.CardHeaderStyle);
                ValheimGuiTheme.DrawDivider();
                GUILayout.Label("Select a worker on the left (or Hold [E] on a worker in-world) to inspect orders, swap weapons/armor, transfer inventory items, adjust sounds, and view their live AI action log.", ValheimGuiTheme.LabelMuted);
            }
            else
            {
                if (_selectedWorker.Customization == null)
                {
                    _selectedWorker.ApplyCustomization(new WorkerCustomization());
                }

                GUILayout.Label($"< ACTIVE WORKER: {_selectedWorker.GetHoverName().ToUpperInvariant()} [{_selectedWorker.CurrentRole}] >", ValheimGuiTheme.CardHeaderStyle);

                // Inspector Sub-Tabs
                GUILayout.BeginHorizontal();
                for (int i = 0; i < _inspectorSubTabs.Length; i++)
                {
                    if (DrawPillButton(_inspectorSubTabs[i], _inspectorSubTab == i, GUILayout.Height(ValheimGuiTheme.ScaledF(28f))))
                    {
                        _inspectorSubTab = i;
                    }
                }
                GUILayout.EndHorizontal();
                ValheimGuiTheme.DrawDivider();

                _inspectorScroll = GUILayout.BeginScrollView(_inspectorScroll);

                switch (_inspectorSubTab)
                {
                    case 0:
                        DrawInspectorOrdersAndLogs();
                        break;
                    case 1:
                        DrawInspectorLoadoutAndInventory();
                        break;
                    case 2:
                        DrawInspectorVoiceAndSounds();
                        break;
                    case 3:
                        DrawInspectorSkillsAndEvolution();
                        break;
                }

                GUILayout.EndScrollView();
            }
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }

        private void DrawInspectorOrdersAndLogs()
        {
            float distToPost = Vector3.Distance(_selectedWorker.transform.position, _selectedWorker.HomePosition);
            GUILayout.BeginHorizontal();
            if (_selectedWorker.m_character != null)
            {
                float hp = _selectedWorker.m_character.GetHealth();
                float maxHp = _selectedWorker.m_character.GetMaxHealth();
                GUILayout.Label($"Vitality: <color=#FFA726>{hp:F0}/{maxHp:F0}</color>", ValheimGuiTheme.LabelParchment, GUILayout.Width(ValheimGuiTheme.ScaledF(125f)));
                ValheimGuiTheme.DrawHealthBar(hp, maxHp, ValheimGuiTheme.ScaledF(110f), ValheimGuiTheme.ScaledF(14f));
            }
            GUILayout.Space(10);
            string postInfo = _selectedWorker.HasWorkPost 
                ? $"Camp: {_selectedWorker.HomePosition:F0} | Work: {_selectedWorker.WorkPostPosition:F0}" 
                : $"Post: {_selectedWorker.HomePosition:F0} ({distToPost:F1}m away)";
            GUILayout.Label(postInfo, ValheimGuiTheme.LabelParchment);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Locate on Map (M)", ValheimGuiTheme.ButtonSelected, GUILayout.Width(ValheimGuiTheme.ScaledF(135f)), GUILayout.Height(ValheimGuiTheme.ScaledF(24f))))
            {
                if (Minimap.instance != null)
                {
                    VerificationTelemetry.RecordPass("map_locate_button", $"Centered World Map on worker '{_selectedWorker.GetHoverName()}' at {_selectedWorker.transform.position:F1}.");
                    SetOpen(false);
                    Minimap.instance.ShowPointOnMap(_selectedWorker.transform.position);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label($"Status: <color=#FFE082>{_selectedWorker.GetCurrentStatusDescription()}</color>", ValheimGuiTheme.LabelParchment);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("🛡️ View Character Sheet & Skills", ValheimGuiTheme.ButtonPrimary, GUILayout.Width(ValheimGuiTheme.ScaledF(230f)), GUILayout.Height(ValheimGuiTheme.ScaledF(24f))))
            {
                NpcCharacterMenuGui.Open(_selectedWorker, 0);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(4);
            GUILayout.Label($"Reassign Order for '{_selectedWorker.GetHoverName()}' [Current: {_selectedWorker.CurrentRole}]:", ValheimGuiTheme.SubHeaderStyle);
            GUILayout.BeginHorizontal();
            if (DrawPillButton("Gatherer", _selectedWorker.CurrentRole == WorkerRole.Gatherer)) _selectedWorker.SetRole(WorkerRole.Gatherer, true);
            if (DrawPillButton("Hauler", _selectedWorker.CurrentRole == WorkerRole.Hauler)) _selectedWorker.SetRole(WorkerRole.Hauler, true);
            if (DrawPillButton("Base Tender", _selectedWorker.CurrentRole == WorkerRole.BaseTender)) _selectedWorker.SetRole(WorkerRole.BaseTender, true);
            if (DrawPillButton("Guard", _selectedWorker.CurrentRole == WorkerRole.Guard)) _selectedWorker.SetRole(WorkerRole.Guard, true);
            if (DrawPillButton("Follow", _selectedWorker.CurrentRole == WorkerRole.Follow)) _selectedWorker.SetRole(WorkerRole.Follow, true);
            if (DrawPillButton("Idle", _selectedWorker.CurrentRole == WorkerRole.Idle)) _selectedWorker.SetRole(WorkerRole.Idle, true);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (DrawPillButton("Lumberjack (Wood)", _selectedWorker.CurrentRole == WorkerRole.Lumberjack)) _selectedWorker.SetRole(WorkerRole.Lumberjack, true);
            if (DrawPillButton("Miner (Stone/Ore)", _selectedWorker.CurrentRole == WorkerRole.Miner)) _selectedWorker.SetRole(WorkerRole.Miner, true);
            if (DrawPillButton("Hunter (Game/Meat)", _selectedWorker.CurrentRole == WorkerRole.Hunter)) _selectedWorker.SetRole(WorkerRole.Hunter, true);
            if (DrawPillButton("Farmer (Crops)", _selectedWorker.CurrentRole == WorkerRole.Farmer)) _selectedWorker.SetRole(WorkerRole.Farmer, true);
            if (DrawPillButton("Cook (Kitchen)", _selectedWorker.CurrentRole == WorkerRole.Cook)) _selectedWorker.SetRole(WorkerRole.Cook, true);
            GUILayout.EndHorizontal();

            if (_selectedWorker.CurrentRole == WorkerRole.Gatherer || _selectedWorker.CurrentRole == WorkerRole.Lumberjack || _selectedWorker.CurrentRole == WorkerRole.Miner || _selectedWorker.CurrentRole == WorkerRole.Hunter)
            {
                DrawResourceDesignationBlock(_selectedWorker);
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label($"Collection / Patrol Radius: <color=#FFA726><b>{_selectedWorker.WorkRadius:F0}m</b></color>", ValheimGuiTheme.LabelParchment, GUILayout.Width(ValheimGuiTheme.ScaledF(190f)));
            float newRadius = GUILayout.HorizontalSlider(_selectedWorker.WorkRadius, 5f, 150f, ValheimGuiTheme.SliderStyle, ValheimGuiTheme.SliderThumbStyle);
            if (Mathf.Abs(newRadius - _selectedWorker.WorkRadius) > 0.5f)
            {
                _selectedWorker.SetWorkRadius(newRadius);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label($"Return When Inventory Reaches: <color=#FFA726><b>{_selectedWorker.ReturnFillThreshold * 100f:F0}%</b></color>", ValheimGuiTheme.LabelParchment, GUILayout.Width(ValheimGuiTheme.ScaledF(220f)));
            float newThresh = GUILayout.HorizontalSlider(_selectedWorker.ReturnFillThreshold, 0.20f, 1.0f, ValheimGuiTheme.SliderStyle, ValheimGuiTheme.SliderThumbStyle);
            if (Mathf.Abs(newThresh - _selectedWorker.ReturnFillThreshold) > 0.04f)
            {
                _selectedWorker.SetReturnFillThreshold(newThresh);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Return Presets:", ValheimGuiTheme.LabelMuted, GUILayout.Width(ValheimGuiTheme.ScaledF(100f)));
            if (GUILayout.Button("25%", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(22f)))) _selectedWorker.SetReturnFillThreshold(0.25f);
            if (GUILayout.Button("50%", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(22f)))) _selectedWorker.SetReturnFillThreshold(0.50f);
            if (GUILayout.Button("75%", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(22f)))) _selectedWorker.SetReturnFillThreshold(0.75f);
            if (GUILayout.Button("100%", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(22f)))) _selectedWorker.SetReturnFillThreshold(1.0f);
            GUILayout.EndHorizontal();

            GUILayout.Space(4);
            GUILayout.Label("Guard & Work Post Relocation (Or press [E] on NPC in-world to lead & station)", ValheimGuiTheme.SubHeaderStyle);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Set Work Post at My Feet", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(30f))) && Player.m_localPlayer != null)
            {
                _selectedWorker.SetWorkPost(Player.m_localPlayer.transform.position, true);
            }
            if (_selectedWorker.HasWorkPost && GUILayout.Button("Reset Work to Camp", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(30f))))
            {
                _selectedWorker.ClearWorkPost();
            }
            if (GUILayout.Button("Move Camp Post to My Feet", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(30f))) && Player.m_localPlayer != null)
            {
                Vector3 targetPos = Player.m_localPlayer.transform.position + Player.m_localPlayer.transform.forward * 1.6f;
                _selectedWorker.StationAtPosition(targetPos, Player.m_localPlayer.transform.rotation, _selectedWorker.CurrentRole);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Anchor Post Where NPC Stands", ValheimGuiTheme.ButtonNormal))
            {
                _selectedWorker.SetHome(_selectedWorker.transform.position, true);
            }
            if (GUILayout.Button("Orient Camp to Base (Bed/Chest)", ValheimGuiTheme.ButtonNormal))
            {
                _selectedWorker.OrientToNearestBase();
            }
            if (GUILayout.Button("Heal Full", ValheimGuiTheme.ButtonNormal))
            {
                _selectedWorker.HealFull();
            }
            if (GUILayout.Button("Dismiss NPC", ValheimGuiTheme.ButtonDanger))
            {
                _selectedWorker.Dismiss();
                _selectedWorker = null;
            }
            GUILayout.EndHorizontal();

            if (_selectedWorker == null) return;

            // Live Action Log
            GUILayout.Space(6);
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Live NPC Action & Decision Log ({_selectedWorker.ActionLog.Count})", ValheimGuiTheme.SubHeaderStyle);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Copy Log", ValheimGuiTheme.ButtonNormal, GUILayout.Width(ValheimGuiTheme.ScaledF(82f))))
            {
                CopySelectedNpcLogToClipboard();
            }
            if (GUILayout.Button("Dump Console", ValheimGuiTheme.ButtonNormal, GUILayout.Width(ValheimGuiTheme.ScaledF(105f))))
            {
                DumpSelectedNpcLogToConsole();
            }
            if (GUILayout.Button("Clear", ValheimGuiTheme.ButtonNormal, GUILayout.Width(ValheimGuiTheme.ScaledF(58f))))
            {
                _selectedWorker.ClearActionLog();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical(ValheimGuiTheme.LogBoxStyle, GUILayout.Height(ValheimGuiTheme.ScaledF(165f)));
            _actionLogScroll = GUILayout.BeginScrollView(_actionLogScroll);
            if (_selectedWorker.ActionLog.Count == 0)
            {
                GUILayout.Label("No actions recorded yet.", ValheimGuiTheme.LabelMuted);
            }
            else
            {
                for (int i = 0; i < _selectedWorker.ActionLog.Count; i++)
                {
                    GUILayout.Label(_selectedWorker.ActionLog[i], ValheimGuiTheme.LabelLogEntry);
                }
                if (_autoScrollLog)
                {
                    _actionLogScroll.y = 99999f;
                }
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void DrawInspectorLoadoutAndInventory()
        {
            var cust = _selectedWorker.Customization;
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Equipped: <color=#FFA726>{(string.IsNullOrEmpty(cust.Weapon) ? "Unarmed" : cust.Weapon)}</color>  |  Shield: <color=#FFA726>{(string.IsNullOrEmpty(cust.Shield) ? "None" : cust.Shield)}</color>  |  Armor: <color=#FFA726>{(string.IsNullOrEmpty(cust.Chest) ? "None" : cust.Chest)}</color>", ValheimGuiTheme.LabelParchment);
            if (GUILayout.Button("Open Native Inventory Grid", ValheimGuiTheme.ButtonSelected, GUILayout.Width(ValheimGuiTheme.ScaledF(220f)), GUILayout.Height(ValheimGuiTheme.ScaledF(26f))))
            {
                _selectedWorker.OpenNativeInventory(Player.m_localPlayer);
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(4);

            // 1. Live Quick-Swap Loadout
            DrawLoadoutSelectorBlock(cust, _selectedWorker);

            GUILayout.Space(8);
            ValheimGuiTheme.DrawDivider();

            // 2. Side-by-Side 8x4 Inventory Grids (Worker Left | Player Right)
            GUILayout.BeginHorizontal();

            // Left: Worker 8x4 Grid
            GUILayout.BeginVertical(ValheimGuiTheme.LogBoxStyle, GUILayout.Width(ValheimGuiTheme.ScaledF(315f)));
            var workerInv = _selectedWorker.m_container != null ? _selectedWorker.m_container.GetInventory() : _selectedWorker.m_humanoid?.GetInventory();
            int carriedCount = workerInv != null ? workerInv.GetAllItems().Count : 0;
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Worker Inventory (8x4 Slots, {carriedCount} items)", ValheimGuiTheme.SubHeaderStyle);
            if (carriedCount > 0 && GUILayout.Button("Drop All", ValheimGuiTheme.ButtonNormal, GUILayout.Width(ValheimGuiTheme.ScaledF(65f))))
            {
                _selectedWorker.DropAllItems();
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(2);
            DrawVisualInventoryGrid(workerInv, true, _selectedWorker);
            GUILayout.Label("<color=#ADB5BD><size=11>Left-Click gear to Equip/Unequip. Right-Click to take to player.</size></color>", ValheimGuiTheme.LabelMuted);
            GUILayout.EndVertical();

            GUILayout.Space(6);

            // Right: Player 8x4 Grid
            GUILayout.BeginVertical(ValheimGuiTheme.LogBoxStyle);
            var playerInv = Player.m_localPlayer != null ? Player.m_localPlayer.GetInventory() : null;
            int playerCount = playerInv != null ? playerInv.GetAllItems().Count : 0;
            GUILayout.Label($"Player Inventory ({playerCount} items)", ValheimGuiTheme.SubHeaderStyle);
            GUILayout.Space(2);
            DrawVisualInventoryGrid(playerInv, false, _selectedWorker);
            GUILayout.Label("<color=#ADB5BD><size=11>Left-Click to give/equip item onto worker.</size></color>", ValheimGuiTheme.LabelMuted);
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }

        private void DrawVisualInventoryGrid(Inventory inv, bool isWorker, WorkerBrain worker)
        {
            if (inv == null)
            {
                GUILayout.Label("(Inventory unavailable)", ValheimGuiTheme.LabelMuted);
                return;
            }

            int width = inv.GetWidth();
            int height = inv.GetHeight();
            float slotSize = ValheimGuiTheme.ScaledF(33f);
            float slotMargin = 2f;

            Rect gridRect = GUILayoutUtility.GetRect(width * (slotSize + slotMargin), height * (slotSize + slotMargin));
            Event e = Event.current;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Rect slotRect = new Rect(gridRect.x + x * (slotSize + slotMargin), gridRect.y + y * (slotSize + slotMargin), slotSize, slotSize);
                    ItemDrop.ItemData item = inv.GetItemAt(x, y);

                    bool isEquipped = item != null && item.m_equipped;
                    GUIStyle boxStyle = isEquipped ? ValheimGuiTheme.SlotBoxEquippedStyle : ValheimGuiTheme.SlotBoxStyle;
                    GUI.Box(slotRect, GUIContent.none, boxStyle);

                    if (item != null)
                    {
                        Sprite icon = item.GetIcon();
                        if (icon != null && icon.texture != null)
                        {
                            Rect tr = icon.textureRect;
                            Rect uv = new Rect(tr.x / icon.texture.width, tr.y / icon.texture.height, tr.width / icon.texture.width, tr.height / icon.texture.height);
                            Rect iconRect = new Rect(slotRect.x + 2f, slotRect.y + 2f, slotSize - 4f, slotSize - 4f);
                            GUI.DrawTextureWithTexCoords(iconRect, icon.texture, uv);
                        }

                        if (item.m_stack > 1)
                        {
                            Rect countRect = new Rect(slotRect.x, slotRect.y + slotSize - 14f, slotSize - 2f, 12f);
                            GUI.Label(countRect, item.m_stack.ToString(), ValheimGuiTheme.SlotCountStyle);
                        }

                        if (item.m_shared != null && item.m_shared.m_useDurability && item.GetMaxDurability() > 0f)
                        {
                            float pct = Mathf.Clamp01(item.m_durability / item.GetMaxDurability());
                            Rect durRect = new Rect(slotRect.x + 3f, slotRect.y + slotSize - 4f, (slotSize - 6f) * pct, 2f);
                            Color durCol = pct > 0.5f ? Color.green : (pct > 0.2f ? Color.yellow : Color.red);
                            GUI.DrawTexture(durRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, durCol, 0, 0);
                        }

                        if (e.type == EventType.MouseDown && slotRect.Contains(e.mousePosition))
                        {
                            if (isWorker)
                            {
                                if (e.button == 0)
                                {
                                    if (WorkerBrain.IsEquippableGear(item))
                                    {
                                        worker.OnNpcGridRightClick(item);
                                    }
                                    else
                                    {
                                        worker.TakeCarriedItemToPlayer(item);
                                    }
                                }
                                else if (e.button == 1)
                                {
                                    worker.TakeCarriedItemToPlayer(item);
                                }
                            }
                            else
                            {
                                worker.GiveOrEquipFromPlayer(item);
                            }
                            e.Use();
                        }
                    }
                }
            }
        }

        private void DrawInspectorVoiceAndSounds()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Worker Name:", ValheimGuiTheme.LabelParchment, GUILayout.Width(ValheimGuiTheme.ScaledF(110f)));
            string newName = GUILayout.TextField(_selectedWorker.Customization.Name, 24, ValheimGuiTheme.TextFieldStyle, GUILayout.Height(ValheimGuiTheme.ScaledF(26f)));
            if (newName != _selectedWorker.Customization.Name)
            {
                _selectedWorker.Customization.Name = newName;
                if (_selectedWorker.m_character != null) _selectedWorker.m_character.m_name = newName;
                _selectedWorker.SaveToZDO();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(8);
            DrawSoundSettingsBlock(_selectedWorker.Customization, _selectedWorker);
        }

        private Vector2 _skillsScroll = Vector2.zero;

        private void DrawInspectorSkillsAndEvolution()
        {
            if (_selectedWorker == null) return;

            GUILayout.BeginHorizontal();
            GUILayout.Label("< SKILLS, PROGRESSION & EVOLUTION >", ValheimGuiTheme.CardHeaderStyle);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Open Full Player-Style Character Menu", ValheimGuiTheme.ButtonSelected, GUILayout.Width(ValheimGuiTheme.ScaledF(270f)), GUILayout.Height(ValheimGuiTheme.ScaledF(24f))))
            {
                NpcCharacterMenuGui.Open(_selectedWorker, 1);
            }
            GUILayout.EndHorizontal();
            ValheimGuiTheme.DrawDivider();

            // Status Row
            GUILayout.BeginHorizontal();
            GUILayout.Label($"<b>Lineage:</b> {_selectedWorker.Heritage}", ValheimGuiTheme.LabelGold, GUILayout.Width(ValheimGuiTheme.ScaledF(130f)));
            GUILayout.Label($"<b>Level:</b> {_selectedWorker.Level}", ValheimGuiTheme.LabelGold, GUILayout.Width(ValheimGuiTheme.ScaledF(80f)));
            GUILayout.Label($"<b>Tier:</b> {_selectedWorker.WorkerTier}", ValheimGuiTheme.LabelGold, GUILayout.Width(ValheimGuiTheme.ScaledF(80f)));
            GUILayout.EndHorizontal();

            // Dignity & Pride Section (Human / Dvergr)
            if (_selectedWorker.Heritage != WorkerHeritage.Skeleton)
            {
                GUILayout.Space(4);
                GUILayout.BeginHorizontal();
                GUILayout.Label("Dignity & Pride:", ValheimGuiTheme.LabelParchment, GUILayout.Width(ValheimGuiTheme.ScaledF(110f)));
                string prideMood = _selectedWorker.PrideMeter >= 0.80f ? "<color=#88FF88>🔥 Proud & Dedicated</color>"
                    : (_selectedWorker.PrideMeter >= 0.50f ? "<color=#FFFF88>😐 Content</color>"
                    : (_selectedWorker.PrideMeter >= 0.10f ? "<color=#FFAA44>💢 Disgruntled (-30% speed)</color>"
                    : "<color=#FF4444>🛑 On Strike (Resting)</color>"));

                GUILayout.Label($"{_selectedWorker.PrideMeter * 100f:F0}% ({prideMood})", ValheimGuiTheme.LabelParchment, GUILayout.Width(ValheimGuiTheme.ScaledF(220f)));

                if (GUILayout.Button("Appease Pride (Offer Food/Coins)", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(24f))))
                {
                    if (Player.m_localPlayer != null)
                    {
                        var pInv = Player.m_localPlayer.GetInventory();
                        if (_selectedWorker.Heritage == WorkerHeritage.Human)
                        {
                            ItemDrop.ItemData food = pInv.GetAllItems().Find(i => i.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Consumable);
                            if (food != null)
                            {
                                pInv.RemoveItem(food, 1);
                                _selectedWorker.PrideMeter = 1.0f;
                                _selectedWorker.Say("A feast worthy of a warrior! My strength is renewed!");
                            }
                            else
                            {
                                Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Need consumable food or mead in inventory to appease!");
                            }
                        }
                        else if (_selectedWorker.Heritage == WorkerHeritage.Dvergr)
                        {
                            if (pInv.CountItems("Coins") >= 10)
                            {
                                pInv.RemoveItem("Coins", 10);
                                _selectedWorker.PrideMeter = 1.0f;
                                _selectedWorker.Say("Gold makes everything right. Back to work!");
                            }
                            else
                            {
                                Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Need 10x Coins in inventory to appease Dvergr pride!");
                            }
                        }
                    }
                }
                GUILayout.EndHorizontal();
            }

            ValheimGuiTheme.DrawDivider();

            _skillsScroll = GUILayout.BeginScrollView(_skillsScroll, GUILayout.Height(ValheimGuiTheme.ScaledF(360f)));

            if (_selectedWorker.Heritage == WorkerHeritage.Human)
            {
                DrawHumanSkillTree(_selectedWorker);
            }
            else if (_selectedWorker.Heritage == WorkerHeritage.Skeleton)
            {
                DrawSkeletonEvolutionTree(_selectedWorker);
            }
            else if (_selectedWorker.Heritage == WorkerHeritage.Dvergr)
            {
                DrawDvergrPatronageTree(_selectedWorker);
            }

            GUILayout.EndScrollView();
        }

        private void DrawHumanSkillTree(WorkerBrain worker)
        {
            int nextXp = WorkerSkillTree.GetXpForNextLevel(worker.Level);
            GUILayout.BeginHorizontal();
            GUILayout.Label($"<b>Task XP:</b> {worker.CurrentXp} / {nextXp} XP", ValheimGuiTheme.LabelGold);
            GUILayout.Label($"<b>Available Skill Points:</b> <color=#FFD700><b>{worker.SkillPoints} SP</b></color>", ValheimGuiTheme.LabelGold);
            GUILayout.EndHorizontal();
            GUILayout.Space(6);

            string[] branches = new string[] { "Woodsman", "Hearth", "Ranger", "ShieldWall" };
            string[] branchTitles = new string[] { "🌲 Woodsman & Forager", "🌾 Hearth, Farm & Kitchen", "🏹 Ranger & Delver", "⚔️ Shield-Wall & Industry" };

            for (int b = 0; b < branches.Length; b++)
            {
                GUILayout.BeginVertical(ValheimGuiTheme.CardStyle);
                GUILayout.Label(branchTitles[b], ValheimGuiTheme.SubHeaderStyle);
                ValheimGuiTheme.DrawDivider();

                foreach (var kvp in WorkerSkillTree.HumanSkills)
                {
                    if (kvp.Value.Branch != branches[b]) continue;
                    var node = kvp.Value;

                    GUILayout.BeginHorizontal();
                    bool learned = worker.HasSkill(node.Id);
                    string statusTag = learned ? "<color=#88FF88>[✔ Learned]</color>" : $"<color=#FFA726>Tier {node.Tier}</color>";
                    GUILayout.Label($"<b>{node.DisplayName}</b> ({statusTag}): {node.Description}", ValheimGuiTheme.LabelParchment, GUILayout.Width(ValheimGuiTheme.ScaledF(420f)));

                    if (learned)
                    {
                        GUILayout.Label("<color=#88FF88>ACTIVE</color>", ValheimGuiTheme.LabelGold, GUILayout.Width(ValheimGuiTheme.ScaledF(70f)));
                    }
                    else
                    {
                        bool parentLearned = string.IsNullOrEmpty(node.RequiredParentId) || worker.HasSkill(node.RequiredParentId);
                        bool canAfford = worker.SkillPoints >= node.SkillPointCost;

                        if (parentLearned && canAfford)
                        {
                            if (GUILayout.Button($"Learn ({node.SkillPointCost} SP)", ValheimGuiTheme.ButtonPrimary, GUILayout.Width(ValheimGuiTheme.ScaledF(110f)), GUILayout.Height(26)))
                            {
                                worker.SkillPoints -= node.SkillPointCost;
                                worker.UnlockSkill(node.Id);
                            }
                        }
                        else
                        {
                            string lockReason = !parentLearned ? "Need Prev" : $"Need {node.SkillPointCost} SP";
                            GUILayout.Label($"<color=#888888>[{lockReason}]</color>", ValheimGuiTheme.LabelMuted, GUILayout.Width(ValheimGuiTheme.ScaledF(110f)));
                        }
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.Space(2);
                }
                GUILayout.EndVertical();
                GUILayout.Space(6);
            }
        }

        private void DrawSkeletonEvolutionTree(WorkerBrain worker)
        {
            int currentForm = Mathf.Clamp(worker.EvolutionForm, 1, 4);
            var currentDef = WorkerSkillTree.SkeletonForms[currentForm];

            GUILayout.BeginHorizontal();
            GUILayout.Label($"<b>Current Form:</b> Form {currentForm} ({currentDef.FormName})", ValheimGuiTheme.LabelGold);
            GUILayout.Label($"<b>Thirsting Marrow Blood:</b> <color=#FF4444><b>{worker.MarrowBlood:F1} / 200 Blood</b></color>", ValheimGuiTheme.LabelGold);
            GUILayout.EndHorizontal();
            GUILayout.Space(6);

            // Evolution Form Upgrade
            if (currentForm < 4)
            {
                int nextForm = currentForm + 1;
                var nextDef = WorkerSkillTree.SkeletonForms[nextForm];

                GUILayout.BeginVertical(ValheimGuiTheme.CardStyle);
                GUILayout.Label($"< FORM EVOLUTION: FORM {nextForm} ({nextDef.FormName.ToUpperInvariant()}) >", ValheimGuiTheme.SubHeaderStyle);
                GUILayout.Label($"<b>Base Vitality:</b> {nextDef.BaseHp} HP | <b>Scale:</b> {nextDef.Scale:F2}x | <b>Unlocked Roles:</b> {string.Join(", ", nextDef.UnlockedRoles)}", ValheimGuiTheme.LabelParchment);

                string extraItemReq = !string.IsNullOrEmpty(nextDef.ExtraItemPrefab) ? $" + {nextDef.ExtraItemAmount}x {nextDef.ExtraItemPrefab}" : "";
                GUILayout.Label($"<b>Sacrificial Offering:</b> {nextDef.RequiredBlood} Blood + {nextDef.RequiredBones} BoneFragments{extraItemReq}", ValheimGuiTheme.LabelGold);

                if (GUILayout.Button($"Perform Sacrificial Evolution (Form {nextForm})", ValheimGuiTheme.ButtonPrimary, GUILayout.Height(32)))
                {
                    if (Player.m_localPlayer != null)
                    {
                        var inv = Player.m_localPlayer.GetInventory();
                        int bonesHave = inv.CountItems("BoneFragments");
                        int chaliceBlood = RelicPrefabManager.GetTotalPlayerChaliceBlood(Player.m_localPlayer);
                        float totalBloodAvailable = worker.MarrowBlood + chaliceBlood;

                        bool hasExtra = string.IsNullOrEmpty(nextDef.ExtraItemPrefab) || inv.CountItems(nextDef.ExtraItemPrefab) >= nextDef.ExtraItemAmount;

                        if (bonesHave < nextDef.RequiredBones)
                        {
                            Player.m_localPlayer.Message(MessageHud.MessageType.Center, $"Need {nextDef.RequiredBones}x BoneFragments in inventory (have {bonesHave})!");
                        }
                        else if (!hasExtra)
                        {
                            Player.m_localPlayer.Message(MessageHud.MessageType.Center, $"Need {nextDef.ExtraItemAmount}x {nextDef.ExtraItemPrefab} in inventory!");
                        }
                        else if (totalBloodAvailable < nextDef.RequiredBlood)
                        {
                            Player.m_localPlayer.Message(MessageHud.MessageType.Center, $"Need {nextDef.RequiredBlood} Blood (Marrow + Chalice has {totalBloodAvailable:F0})!");
                        }
                        else
                        {
                            // Deduct bones & extra items
                            inv.RemoveItem("BoneFragments", nextDef.RequiredBones);
                            if (!string.IsNullOrEmpty(nextDef.ExtraItemPrefab))
                            {
                                inv.RemoveItem(nextDef.ExtraItemPrefab, nextDef.ExtraItemAmount);
                            }

                            // Deduct blood (first from marrow, then from player's chalice)
                            float bloodToDeduct = nextDef.RequiredBlood;
                            if (worker.MarrowBlood >= bloodToDeduct)
                            {
                                worker.MarrowBlood -= bloodToDeduct;
                            }
                            else
                            {
                                bloodToDeduct -= worker.MarrowBlood;
                                worker.MarrowBlood = 0f;
                                RelicPrefabManager.ConsumeBloodFromPlayerChalice(Player.m_localPlayer, Mathf.CeilToInt(bloodToDeduct));
                            }

                            worker.EvolveSkeletonForm(nextForm);
                            Player.m_localPlayer.Message(MessageHud.MessageType.Center, $"<color=#FF4444>★ Skeleton evolved into Form {nextForm} ({nextDef.FormName})! ★</color>");
                        }
                    }
                }
                GUILayout.EndVertical();
                GUILayout.Space(6);
            }

            // Blood-Only Grafts
            GUILayout.BeginVertical(ValheimGuiTheme.CardStyle);
            GUILayout.Label("BLOOD GRAFTS & MARROW RITES", ValheimGuiTheme.SubHeaderStyle);
            ValheimGuiTheme.DrawDivider();

            DrawBloodGraftRow(worker, "skel_marrow_hard", "Marrow Hardening", "+50 Max HP and immunity to stagger.", 15);
            DrawBloodGraftRow(worker, "skel_quicksilver", "Quicksilver Joints", "+20% Movement and hauling speed.", 20);
            DrawBloodGraftRow(worker, "skel_predator", "Predator's Instinct", "Awakens beast archery instinct (Unlocks Hunter / Bow).", 25);
            GUILayout.EndVertical();
        }

        private void DrawBloodGraftRow(WorkerBrain worker, string id, string name, string desc, int bloodCost)
        {
            GUILayout.BeginHorizontal();
            bool learned = worker.HasSkill(id);
            GUILayout.Label($"<b>{name}</b> ({bloodCost} Blood): {desc}", ValheimGuiTheme.LabelParchment, GUILayout.Width(ValheimGuiTheme.ScaledF(420f)));

            if (learned)
            {
                GUILayout.Label("<color=#88FF88>[✔ Grafted]</color>", ValheimGuiTheme.LabelGold, GUILayout.Width(ValheimGuiTheme.ScaledF(90f)));
            }
            else
            {
                if (GUILayout.Button($"Graft ({bloodCost} Blood)", ValheimGuiTheme.ButtonNormal, GUILayout.Width(ValheimGuiTheme.ScaledF(110f)), GUILayout.Height(24)))
                {
                    int chalice = RelicPrefabManager.GetTotalPlayerChaliceBlood(Player.m_localPlayer);
                    if (worker.MarrowBlood >= bloodCost)
                    {
                        worker.MarrowBlood -= bloodCost;
                        worker.UnlockSkill(id);
                    }
                    else if (worker.MarrowBlood + chalice >= bloodCost)
                    {
                        float rem = bloodCost - worker.MarrowBlood;
                        worker.MarrowBlood = 0f;
                        RelicPrefabManager.ConsumeBloodFromPlayerChalice(Player.m_localPlayer, Mathf.CeilToInt(rem));
                        worker.UnlockSkill(id);
                    }
                    else
                    {
                        Player.m_localPlayer?.Message(MessageHud.MessageType.Center, $"Need {bloodCost} Blood (have {worker.MarrowBlood + chalice:F0})!");
                    }
                }
            }
            GUILayout.EndHorizontal();
        }

        private void DrawDvergrPatronageTree(WorkerBrain worker)
        {
            GUILayout.Label("SVARTALFHEIM GUILD PATRONAGE & CERTIFICATIONS", ValheimGuiTheme.SubHeaderStyle);
            ValheimGuiTheme.DrawDivider();

            foreach (var kvp in WorkerSkillTree.DvergrPatronage)
            {
                var node = kvp.Value;
                GUILayout.BeginVertical(ValheimGuiTheme.CardStyle);
                GUILayout.BeginHorizontal();

                bool learned = worker.HasSkill(node.Id);
                string reqs = $"{node.RequiredCoins} Coins";
                if (!string.IsNullOrEmpty(node.RequiredMaterialPrefab)) reqs += $" + {node.RequiredMaterialAmount}x {node.RequiredMaterialPrefab}";
                if (!string.IsNullOrEmpty(node.RequiredGemPrefab)) reqs += $" + {node.RequiredGemAmount}x {node.RequiredGemPrefab}";

                GUILayout.Label($"<b>{node.DisplayName}</b>\n<color=#FFA726>Req: {reqs}</color>\n{node.Description}", ValheimGuiTheme.LabelParchment, GUILayout.Width(ValheimGuiTheme.ScaledF(420f)));

                if (learned)
                {
                    GUILayout.Label("<color=#88FF88>[✔ Certified]</color>", ValheimGuiTheme.LabelGold, GUILayout.Width(ValheimGuiTheme.ScaledF(110f)));
                }
                else
                {
                    if (GUILayout.Button("Bestow Patronage", ValheimGuiTheme.ButtonPrimary, GUILayout.Width(ValheimGuiTheme.ScaledF(120f)), GUILayout.Height(32)))
                    {
                        if (Player.m_localPlayer != null)
                        {
                            var inv = Player.m_localPlayer.GetInventory();
                            int coins = inv.CountItems("Coins");
                            int mats = !string.IsNullOrEmpty(node.RequiredMaterialPrefab) ? inv.CountItems(node.RequiredMaterialPrefab) : 999;
                            int gems = !string.IsNullOrEmpty(node.RequiredGemPrefab) ? inv.CountItems(node.RequiredGemPrefab) : 999;

                            if (coins < node.RequiredCoins)
                            {
                                Player.m_localPlayer.Message(MessageHud.MessageType.Center, $"Need {node.RequiredCoins}x Coins in inventory (have {coins})!");
                            }
                            else if (mats < node.RequiredMaterialAmount)
                            {
                                Player.m_localPlayer.Message(MessageHud.MessageType.Center, $"Need {node.RequiredMaterialAmount}x {node.RequiredMaterialPrefab} in inventory (have {mats})!");
                            }
                            else if (gems < node.RequiredGemAmount)
                            {
                                Player.m_localPlayer.Message(MessageHud.MessageType.Center, $"Need {node.RequiredGemAmount}x {node.RequiredGemPrefab} in inventory (have {gems})!");
                            }
                            else
                            {
                                inv.RemoveItem("Coins", node.RequiredCoins);
                                if (!string.IsNullOrEmpty(node.RequiredMaterialPrefab)) inv.RemoveItem(node.RequiredMaterialPrefab, node.RequiredMaterialAmount);
                                if (!string.IsNullOrEmpty(node.RequiredGemPrefab)) inv.RemoveItem(node.RequiredGemPrefab, node.RequiredGemAmount);

                                worker.UnlockSkill(node.Id);
                                worker.PrideMeter = 1.0f;
                                worker.Say($"The Guild honors this patronage! [{node.DisplayName}] certified.");
                                Player.m_localPlayer.Message(MessageHud.MessageType.Center, $"<color=#FFD700>★ Bestowed {node.DisplayName} on Dvergr Laborer! ★</color>");
                            }
                        }
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                GUILayout.Space(4);
            }
        }

        private void CopySelectedNpcLogToClipboard()
        {
            if (_selectedWorker == null) return;
            var sb = new StringBuilder();
            sb.AppendLine($"=== NPC Diagnostic Log: {_selectedWorker.GetHoverName()} ===");
            sb.AppendLine($"Role: {_selectedWorker.CurrentRole} | Pos: {_selectedWorker.transform.position:F1} | HomePost: {_selectedWorker.HomePosition:F1} | Radius: {_selectedWorker.WorkRadius:F0}m");
            sb.AppendLine($"Status: {_selectedWorker.GetCurrentStatusDescription()}");
            sb.AppendLine("--- Action History ---");
            foreach (var line in _selectedWorker.ActionLog)
            {
                sb.AppendLine(line);
            }
            GUIUtility.systemCopyBuffer = sb.ToString();
            _selectedWorker.LogAction("Copied diagnostic log to Windows clipboard.", true);
        }

        private void DumpSelectedNpcLogToConsole()
        {
            if (_selectedWorker == null) return;
            ValheimWorkersPlugin.Log.LogInfo($"================ NPC DIAGNOSTIC DUMP: {_selectedWorker.GetHoverName()} ================");
            ValheimWorkersPlugin.Log.LogInfo($"Role: {_selectedWorker.CurrentRole} | Pos: {_selectedWorker.transform.position:F1} | HomePost: {_selectedWorker.HomePosition:F1} | Radius: {_selectedWorker.WorkRadius:F0}m");
            foreach (var line in _selectedWorker.ActionLog)
            {
                ValheimWorkersPlugin.Log.LogInfo($"  {line}");
            }
            ValheimWorkersPlugin.Log.LogInfo("================================================================================");
        }

        // ==========================================
        // TAB 3: PROVING GROUNDS
        // ==========================================
        private void DrawTabSandbox()
        {
            GUILayout.BeginVertical(ValheimGuiTheme.CardStyle, GUILayout.ExpandHeight(true));
            GUILayout.Label("< PROVING GROUNDS & SANDBOX >", ValheimGuiTheme.CardHeaderStyle);
            GUILayout.Label("Spawn complete self-contained test setups directly around your character.", ValheimGuiTheme.LabelMuted);
            ValheimGuiTheme.DrawDivider();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Hauler Scenario\n(Chest + Drops + Hauler)", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(48f))))
            {
                RunHaulerScenario();
            }
            if (GUILayout.Button("Base Tender Scenario\n(Unlit Fire + Fuel + Tender)", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(48f))))
            {
                RunTenderScenario();
            }
            if (GUILayout.Button("Guard Combat Scenario\n(Armed Guard + Hostile Greydwarf)", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(48f))))
            {
                RunGuardScenario();
            }
            if (GUILayout.Button("Lumberjack Scenario\n(Fallen Logs + Beech Tree + Axe)", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(48f))))
            {
                RunLumberjackScenario();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Mining & Stone Scenario\n(Compact Boulder + Stones + Pickaxe)", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(48f))))
            {
                RunMiningScenario();
            }
            if (GUILayout.Button("Hunting Wildlife Scenario\n(Boars/Deer + Bow Hunter)", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(48f))))
            {
                RunHuntingScenario();
            }
            if (GUILayout.Button("Farming Crop Scenario\n(Carrot Patch + Cultivator + Farmer)", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(48f))))
            {
                RunFarmingScenario();
            }
            if (GUILayout.Button("Cooking Kitchen Scenario\n(Campfire + Spit + Raw Meat + Cook)", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(48f))))
            {
                RunCookingScenario();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Wild Foraging & Beehive\n(Berry Bush + Hive + Gatherer)", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(48f))))
            {
                RunForagingScenario();
            }
            if (GUILayout.Button("Stationary Archery Target\n(Wood Wall + Target + Hunter)", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(48f))))
            {
                RunArcheryTargetScenario();
            }
            if (GUILayout.Button("Smelter & Kiln Forge\n(Smelter + Kiln + Ore & Wood)", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(48f))))
            {
                RunSmelterKilnScenario();
            }
            if (GUILayout.Button("Troll Threat Scenario\n(Camp Fleeing Retreat Test)", ValheimGuiTheme.ButtonDanger, GUILayout.Height(ValheimGuiTheme.ScaledF(48f))))
            {
                RunTrollThreatScenario();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("💀 Blood Sacrifice Scenario\n(Dagger + Chalice + Boar + Bones)", ValheimGuiTheme.ButtonSelected, GUILayout.Height(ValheimGuiTheme.ScaledF(48f))))
            {
                RunBloodSacrificeScenario();
            }
            string sandboxBtn = WorkerProgressionManager.SandboxUnlockAll ? "<color=#88FF88>Sandbox: UNLOCKED ALL</color>" : "Sandbox: Normal Lock";
            if (GUILayout.Button(sandboxBtn, ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(48f))))
            {
                WorkerProgressionManager.SandboxUnlockAll = !WorkerProgressionManager.SandboxUnlockAll;
                Player.m_localPlayer?.Message(MessageHud.MessageType.Center, $"Worker Progression Sandbox: {(WorkerProgressionManager.SandboxUnlockAll ? "UNLOCKED ALL" : "LOCKED")}");
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Low-HP Retreat Scenario\n(Injured Worker + Boar/Greydwarf)", ValheimGuiTheme.ButtonDanger, GUILayout.Height(ValheimGuiTheme.ScaledF(48f))))
            {
                RunLowHpScenario();
            }
            if (GUILayout.Button("Dismiss All Workers\n(Clear 50m Radius)", ValheimGuiTheme.ButtonDanger, GUILayout.Height(ValheimGuiTheme.ScaledF(48f))))
            {
                ClearNearbyWorkers();
            }
            if (GUILayout.Button("Clean Restart Telemetry\n(Reset All 45 Tests)", ValheimGuiTheme.ButtonSelected, GUILayout.Height(ValheimGuiTheme.ScaledF(48f))))
            {
                VerificationTelemetry.ResetAll();
                Player.m_localPlayer?.Message(MessageHud.MessageType.Center, "Verification Suite Reset! All tests set to Untested.");
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.Label("=== MAP EXPEDITIONS & WAYFINDER QUEST SITES ===", ValheimGuiTheme.CardHeaderStyle);
            GUILayout.Label("Generates themed discovery sites (25m-60m away) and places a Wayfinder Sign Stone + Minimap Pin pointing direction & distance.", ValheimGuiTheme.LabelMuted);
            ValheimGuiTheme.DrawDivider();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("⚰️ Barrow of Skarde\n(40m N: Dagger, Crypt & Boar)", ValheimGuiTheme.ButtonSelected, GUILayout.Height(ValheimGuiTheme.ScaledF(48f))))
            {
                RunExpeditionBarrowOfSkarde();
            }
            if (GUILayout.Button("🩸 Crypt of Valka the Red\n(45m NE: Blood Chalice & Wildlife)", ValheimGuiTheme.ButtonSelected, GUILayout.Height(ValheimGuiTheme.ScaledF(48f))))
            {
                RunExpeditionCryptOfValka();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(4);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("🛡️ Viking Awakening Trail\n(40m E: 4 Shrines & Gjallarhorn)", ValheimGuiTheme.ButtonSelected, GUILayout.Height(ValheimGuiTheme.ScaledF(48f))))
            {
                RunExpeditionVikingAwakeningTrail();
            }
            if (GUILayout.Button("⛏️ Dvergr Guild Trail\n(45m SE: 4 Workshops & Seal)", ValheimGuiTheme.ButtonSelected, GUILayout.Height(ValheimGuiTheme.ScaledF(48f))))
            {
                RunExpeditionDvergrGuildExpedition();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(4);
            if (GUILayout.Button("🎓 All-Skills & Progression Proving Arena\n(25m S: Training Compound with Trees, Ore, Targets, Soil, Forge, All 5 Relics & Reagents)", ValheimGuiTheme.ButtonSelected, GUILayout.Height(ValheimGuiTheme.ScaledF(48f))))
            {
                RunExpeditionAllSkillsProgressionArena();
            }

            GUILayout.Space(8);
            GUILayout.Label("Quick Progression & Skill Boosters", ValheimGuiTheme.SubHeaderStyle);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+5 Levels & SP", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(28f))))
            {
                BoostSelectedWorkerLevel(5);
            }
            if (GUILayout.Button("+20 All 8 Skills", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(28f))))
            {
                BoostSelectedWorkerSkills(20f);
            }
            if (GUILayout.Button("+50 Blood/Marrow", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(28f))))
            {
                BoostSelectedWorkerBlood(50f);
            }
            if (GUILayout.Button("Trigger Pride Crisis", ValheimGuiTheme.ButtonDanger, GUILayout.Height(ValheimGuiTheme.ScaledF(28f))))
            {
                TriggerWorkerPride(0f);
            }
            if (GUILayout.Button("Appease Pride (100%)", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(28f))))
            {
                TriggerWorkerPride(100f);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(12);
            GUILayout.Label("World Structures & Props (Spawned 3-4m ahead)", ValheimGuiTheme.SubHeaderStyle);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Wooden Chest", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(30f)))) SpawnWorldProp("piece_chest_wood", 3f);
            if (GUILayout.Button("Unlit Campfire", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(30f)))) SpawnWorldProp("fire_pit", 3f, (go) => go.GetComponent<Fireplace>()?.SetFuel(0f));
            if (GUILayout.Button("Cooking Station", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(30f)))) SpawnCookingStationProp(3.5f);
            if (GUILayout.Button("Beehive (Honey)", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(30f)))) SpawnBeehiveProp(3.5f);
            if (GUILayout.Button("Smelter", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(30f)))) SpawnWorldProp("smelter", 4f);
            if (GUILayout.Button("Charcoal Kiln", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(30f)))) SpawnWorldProp("charcoal_kiln", 4f);
            GUILayout.EndHorizontal();

            GUILayout.Space(8);
            GUILayout.Label("Test Materials, Crops, Wild Game & Foes", ValheimGuiTheme.SubHeaderStyle);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Drop 10x Wood", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(28f)))) SpawnItemDrop("Wood", 10);
            if (GUILayout.Button("Drop 10x Stone", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(28f)))) SpawnItemDrop("Stone", 10);
            if (GUILayout.Button("Drop 10x Raw Meat", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(28f)))) SpawnItemDrop("RawMeat", 10);
            if (GUILayout.Button("Drop 10x Seeds", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(28f)))) SpawnItemDrop("CarrotSeeds", 10);
            if (GUILayout.Button("Drop 5x Honey", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(28f)))) SpawnItemDrop("Honey", 5);
            GUILayout.EndHorizontal();

            GUILayout.Space(8);
            GUILayout.Label("Relics & Lineage Items (Given directly to player inventory)", ValheimGuiTheme.SubHeaderStyle);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Sacrificial Dagger", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(28f))))
            {
                GivePlayerItem(RelicPrefabManager.PrefabSacrificialDagger, 1);
            }
            if (GUILayout.Button("Blood Chalice (100 Blood)", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(28f))))
            {
                GivePlayerItem(RelicPrefabManager.PrefabBloodChalice, 1, item => RelicPrefabManager.SetChaliceBlood(item, 100));
            }
            if (GUILayout.Button("Drop 20x Bones", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(28f))))
            {
                SpawnItemDrop("BoneFragments", 20);
            }
            if (GUILayout.Button("Awakening Oil", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(28f))))
            {
                GivePlayerItem(RelicPrefabManager.PrefabAwakeningOil, 3);
            }
            if (GUILayout.Button("Gjallarhorn", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(28f))))
            {
                GivePlayerItem(RelicPrefabManager.PrefabGjallarhorn, 1);
            }
            if (GUILayout.Button("Dvergr Seal", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(28f))))
            {
                GivePlayerItem(RelicPrefabManager.PrefabDvergrSeal, 1);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(4);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Carrot Patch (x3)", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(28f)))) SpawnCarrotPatch(4f);
            if (GUILayout.Button("Berry Bush", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(28f)))) SpawnWorldProp("Pickable_Raspberry", 3.5f);
            if (GUILayout.Button("Mushroom Patch", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(28f)))) SpawnWorldProp("Pickable_Mushroom", 3.5f);
            if (GUILayout.Button("Spawn Boar", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(28f)))) SpawnWorldProp("Boar", 6f);
            if (GUILayout.Button("Spawn Deer", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(28f)))) SpawnWorldProp("Deer", 8f);
            if (GUILayout.Button("Spawn Greydwarf", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(28f)))) SpawnWorldProp("Greydwarf", 8f);
            if (GUILayout.Button("Spawn Troll (Danger!)", ValheimGuiTheme.ButtonDanger, GUILayout.Height(ValheimGuiTheme.ScaledF(28f)))) SpawnWorldProp("Troll", 18f);
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            ValheimGuiTheme.DrawDivider();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("🔄 Force Telemetry Sync (Push to Checklist)", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(36f))))
            {
                VerificationTelemetry.ForceRefreshSnapshot();
                Player.m_localPlayer?.Message(MessageHud.MessageType.TopLeft, "Telemetry snapshot synced to Checklist HTML and HTTP bridge!");
            }
            if (GUILayout.Button("Reset Telemetry State", ValheimGuiTheme.ButtonDanger, GUILayout.Width(ValheimGuiTheme.ScaledF(200f)), GUILayout.Height(ValheimGuiTheme.ScaledF(36f))))
            {
                VerificationTelemetry.ResetAll();
                Player.m_localPlayer?.Message(MessageHud.MessageType.TopLeft, "Checklist verification telemetry reset.");
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(4);
            if (GUILayout.Button("Dismiss All Workers in 50m Radius", ValheimGuiTheme.ButtonDanger, GUILayout.Height(ValheimGuiTheme.ScaledF(38f))))
            {
                ClearNearbyWorkers();
            }

            GUILayout.EndVertical();
        }

        private void RunHaulerScenario()
        {
            if (Player.m_localPlayer == null || ZNetScene.instance == null) return;
            Vector3 pos = Player.m_localPlayer.transform.position;
            Vector3 fwd = Player.m_localPlayer.transform.forward;

            GameObject chest = ZNetScene.instance.GetPrefab("piece_chest_wood");
            if (chest) UnityEngine.Object.Instantiate(chest, pos + fwd * 4f, Quaternion.LookRotation(-fwd));

            string[] items = new[] { "Wood", "Wood", "Wood", "Stone", "Stone" };
            for (int i = 0; i < items.Length; i++)
            {
                GameObject drop = ZNetScene.instance.GetPrefab(items[i]);
                if (drop)
                {
                    Vector3 offset = Quaternion.Euler(0, i * 72f, 0) * (fwd * 3.5f);
                    var inst = UnityEngine.Object.Instantiate(drop, pos + offset + Vector3.up * 0.5f, Quaternion.identity);
                    var itemDrop = inst.GetComponent<ItemDrop>();
                    if (itemDrop) itemDrop.m_itemData.m_stack = 5;
                }
            }

            var config = new WorkerCustomization { Name = "Hauler Laborer", InitialRole = WorkerRole.Hauler };
            var go = WorkerPrefabManager.SpawnCustomWorker(config, pos + fwd * 2f, Player.m_localPlayer.transform.rotation);
            if (go != null) _selectedWorker = go.GetComponent<WorkerBrain>();
            SetOpen(false);
        }

        private void RunTenderScenario()
        {
            if (Player.m_localPlayer == null || ZNetScene.instance == null) return;
            Vector3 pos = Player.m_localPlayer.transform.position;
            Vector3 fwd = Player.m_localPlayer.transform.forward;

            GameObject firePrefab = ZNetScene.instance.GetPrefab("fire_pit");
            if (firePrefab)
            {
                var fireObj = UnityEngine.Object.Instantiate(firePrefab, pos + fwd * 4f, Quaternion.identity);
                fireObj.GetComponent<Fireplace>()?.SetFuel(0f);
            }

            var config = new WorkerCustomization { Name = "Fire Tender", InitialRole = WorkerRole.BaseTender };
            var worker = WorkerPrefabManager.SpawnCustomWorker(config, pos + fwd * 2f, Player.m_localPlayer.transform.rotation);
            GameObject wood = ZNetScene.instance.GetPrefab("Wood");
            if (worker && wood)
            {
                worker.GetComponent<Humanoid>()?.GetInventory()?.AddItem(wood, 10);
                _selectedWorker = worker.GetComponent<WorkerBrain>();
            }
            SetOpen(false);
        }

        private void RunGuardScenario()
        {
            if (Player.m_localPlayer == null || ZNetScene.instance == null) return;
            Vector3 pos = Player.m_localPlayer.transform.position;
            Vector3 fwd = Player.m_localPlayer.transform.forward;

            var config = new WorkerCustomization
            {
                Name = "Huscarl Guard",
                BasePrefab = "Worker_Viking",
                Weapon = "SwordBronze",
                Shield = "ShieldWood",
                Helmet = "HelmetBronze",
                Chest = "ArmorBronzeChest",
                Legs = "ArmorBronzeLegs",
                MaxHealth = 250f,
                InitialRole = WorkerRole.Guard
            };
            var go = WorkerPrefabManager.SpawnCustomWorker(config, pos + fwd * 2f, Player.m_localPlayer.transform.rotation);
            if (go != null) _selectedWorker = go.GetComponent<WorkerBrain>();

            GameObject enemy = ZNetScene.instance.GetPrefab("Greydwarf");
            if (enemy) UnityEngine.Object.Instantiate(enemy, pos + fwd * 10f, Quaternion.identity);
            SetOpen(false);
        }

        private void SpawnWorldProp(string prefabName, float distance, Action<GameObject> onSpawned = null)
        {
            if (Player.m_localPlayer == null || ZNetScene.instance == null) return;
            GameObject prefab = ZNetScene.instance.GetPrefab(prefabName);
            if (prefab == null) return;

            Vector3 pos = Player.m_localPlayer.transform.position + Player.m_localPlayer.transform.forward * distance;
            GameObject inst = UnityEngine.Object.Instantiate(prefab, pos, Quaternion.identity);
            onSpawned?.Invoke(inst);
        }

        private void SpawnItemDrop(string itemName, int stack)
        {
            if (Player.m_localPlayer == null || ZNetScene.instance == null) return;
            GameObject prefab = ZNetScene.instance.GetPrefab(itemName);
            if (prefab == null) return;

            Vector3 pos = Player.m_localPlayer.transform.position + Player.m_localPlayer.transform.forward * 2.5f + Vector3.up * 0.5f;
            GameObject inst = UnityEngine.Object.Instantiate(prefab, pos, Quaternion.identity);
            var drop = inst.GetComponent<ItemDrop>();
            if (drop != null) drop.m_itemData.m_stack = stack;
        }

        private void ClearNearbyWorkers()
        {
            if (Player.m_localPlayer == null) return;
            Vector3 pos = Player.m_localPlayer.transform.position;
            var list = new List<WorkerBrain>(WorkerBrain.ActiveWorkers);
            int removed = 0;
            foreach (var w in list)
            {
                if (w != null && Vector3.Distance(w.transform.position, pos) <= 50f)
                {
                    w.Dismiss();
                    removed++;
                }
            }
            if (removed > 0)
            {
                VerificationTelemetry.RecordPass("map_pin_dismiss_cleanup", $"Dismissed {removed} worker(s) and cleaned up their Minimap pins.");
            }
            _selectedWorker = null;
        }

        private void RunLumberjackScenario()
        {
            if (Player.m_localPlayer == null || ZNetScene.instance == null) return;
            Vector3 pos = Player.m_localPlayer.transform.position;
            Vector3 fwd = Player.m_localPlayer.transform.forward;

            GameObject chest = ZNetScene.instance.GetPrefab("piece_chest_wood");
            if (chest) UnityEngine.Object.Instantiate(chest, pos + fwd * 4f, Quaternion.LookRotation(-fwd));

            SpawnItemDrop("Wood", 5);
            GameObject branchPrefab = ZNetScene.instance.GetPrefab("Pickable_Branch");
            if (branchPrefab) UnityEngine.Object.Instantiate(branchPrefab, pos + fwd * 3f + Vector3.right * 2f, Quaternion.identity);

            GameObject saplingPrefab = ZNetScene.instance.GetPrefab("Beech_small1") ?? ZNetScene.instance.GetPrefab("Beech_Sapling");
            if (saplingPrefab) UnityEngine.Object.Instantiate(saplingPrefab, pos + fwd * 5f, Quaternion.identity);

            // Spawn full harvestable beech tree 8m ahead
            GameObject treePrefab = ZNetScene.instance.GetPrefab("Beech1") ?? ZNetScene.instance.GetPrefab("Beech_stub");
            if (treePrefab) UnityEngine.Object.Instantiate(treePrefab, pos + fwd * 8f, Quaternion.identity);

            var config = new WorkerCustomization
            {
                Name = "Lumberjack Sven",
                Weapon = "AxeBronze",
                InitialRole = WorkerRole.Lumberjack,
                DesignatedResources = "Wood,CoreWood,FineWood"
            };
            var worker = WorkerPrefabManager.SpawnCustomWorker(config, pos + fwd * 2f, Player.m_localPlayer.transform.rotation);
            if (worker)
            {
                _selectedWorker = worker.GetComponent<WorkerBrain>();
                _selectedWorker?.StartCommandTrace("SCENARIO", "Proving Grounds: Lumberjack Scenario (Chopping Beech Tree)", "proving_lumberjack");
            }
            SetOpen(false);
        }

        private void RunMiningScenario()
        {
            if (Player.m_localPlayer == null || ZNetScene.instance == null) return;
            Vector3 pos = Player.m_localPlayer.transform.position;
            Vector3 fwd = Player.m_localPlayer.transform.forward;

            GameObject chest = ZNetScene.instance.GetPrefab("piece_chest_wood");
            if (chest) UnityEngine.Object.Instantiate(chest, pos + fwd * 4f, Quaternion.LookRotation(-fwd));

            SpawnItemDrop("Stone", 10);
            GameObject stonePickPrefab = ZNetScene.instance.GetPrefab("Pickable_Stone");
            if (stonePickPrefab) UnityEngine.Object.Instantiate(stonePickPrefab, pos + fwd * 3f + Vector3.left * 2f, Quaternion.identity);

            // Compact boulder (not giant copper mountain!) placed safely 8m ahead
            GameObject rockPrefab = ZNetScene.instance.GetPrefab("Rock_destructible") ?? ZNetScene.instance.GetPrefab("Rock_4") ?? ZNetScene.instance.GetPrefab("MineRock_Tin");
            if (rockPrefab) UnityEngine.Object.Instantiate(rockPrefab, pos + fwd * 8f, Quaternion.identity);

            var config = new WorkerCustomization
            {
                Name = "Miner Torvald",
                Weapon = "PickaxeAntler",
                InitialRole = WorkerRole.Miner,
                DesignatedResources = "Stone,CopperOre,TinOre"
            };
            var worker = WorkerPrefabManager.SpawnCustomWorker(config, pos + fwd * 2f, Player.m_localPlayer.transform.rotation);
            if (worker)
            {
                _selectedWorker = worker.GetComponent<WorkerBrain>();
                _selectedWorker?.StartCommandTrace("SCENARIO", "Proving Grounds: Mining Scenario (Antler Pickaxe Mining)", "proving_mining");
            }
            SetOpen(false);
        }

        private void RunHuntingScenario()
        {
            if (Player.m_localPlayer == null || ZNetScene.instance == null) return;
            Vector3 pos = Player.m_localPlayer.transform.position;
            Vector3 fwd = Player.m_localPlayer.transform.forward;
            Vector3 right = Player.m_localPlayer.transform.right;

            // Spawn Workbench and Camp Supply Chest at the Hunter's camp
            GameObject workbenchPrefab = ZNetScene.instance.GetPrefab("piece_workbench");
            if (workbenchPrefab)
            {
                UnityEngine.Object.Instantiate(workbenchPrefab, pos + fwd * 4f - right * 2f, Quaternion.LookRotation(-fwd));
            }

            GameObject chest = ZNetScene.instance.GetPrefab("piece_chest_wood");
            if (chest)
            {
                var chestGo = UnityEngine.Object.Instantiate(chest, pos + fwd * 4f + right * 1.5f, Quaternion.LookRotation(-fwd));
                var chestInv = chestGo.GetComponent<Container>()?.GetInventory();
                if (chestInv != null)
                {
                    GameObject woodPrefab = ZNetScene.instance.GetPrefab("Wood");
                    if (woodPrefab) chestInv.AddItem(woodPrefab, 24);
                    GameObject featherPrefab = ZNetScene.instance.GetPrefab("Feathers");
                    if (featherPrefab) chestInv.AddItem(featherPrefab, 10);
                    GameObject flintPrefab = ZNetScene.instance.GetPrefab("Flint");
                    if (flintPrefab) chestInv.AddItem(flintPrefab, 10);
                    GameObject cookedMeatPrefab = ZNetScene.instance.GetPrefab("CookedMeat");
                    if (cookedMeatPrefab) chestInv.AddItem(cookedMeatPrefab, 6);
                    GameObject berryPrefab = ZNetScene.instance.GetPrefab("Raspberry");
                    if (berryPrefab) chestInv.AddItem(berryPrefab, 8);
                    GameObject shroomPrefab = ZNetScene.instance.GetPrefab("Mushroom");
                    if (shroomPrefab) chestInv.AddItem(shroomPrefab, 8);
                }
            }

            SpawnWorldProp("Boar", 32f);
            SpawnWorldProp("Deer", 42f);

            var config = new WorkerCustomization
            {
                Name = "Hunter Arn",
                Weapon = "BowFineWood",
                Shield = "ShieldWood",
                InitialRole = WorkerRole.Hunter,
                DesignatedResources = "RawMeat,DeerHide,LeatherScraps,NeckTail,Feathers"
            };
            var worker = WorkerPrefabManager.SpawnCustomWorker(config, pos + fwd * 2f, Player.m_localPlayer.transform.rotation);
            if (worker)
            {
                var inv = worker.GetComponent<Humanoid>()?.GetInventory();
                GameObject arrowPrefab = ZNetScene.instance.GetPrefab("ArrowWood");
                if (arrowPrefab && inv != null) inv.AddItem(arrowPrefab, 30);
                GameObject knifePrefab = ZNetScene.instance.GetPrefab("KnifeFlint");
                if (knifePrefab && inv != null) inv.AddItem(knifePrefab, 1);
                GameObject axePrefab = ZNetScene.instance.GetPrefab("AxeFlint");
                if (axePrefab && inv != null) inv.AddItem(axePrefab, 1);
                GameObject shieldPrefab = ZNetScene.instance.GetPrefab("ShieldWood");
                if (shieldPrefab && inv != null) inv.AddItem(shieldPrefab, 1);
                GameObject cookedMeat = ZNetScene.instance.GetPrefab("CookedMeat");
                if (cookedMeat && inv != null) inv.AddItem(cookedMeat, 4);
                GameObject berries = ZNetScene.instance.GetPrefab("Raspberry");
                if (berries && inv != null) inv.AddItem(berries, 5);
                GameObject shrooms = ZNetScene.instance.GetPrefab("Mushroom");
                if (shrooms && inv != null) inv.AddItem(shrooms, 5);
                _selectedWorker = worker.GetComponent<WorkerBrain>();
                if (_selectedWorker != null)
                {
                    _selectedWorker.SetHome(pos + fwd * 3f, false);
                    _selectedWorker.SetWorkRadius(55f);
                    _selectedWorker.TryMaintainThreeFoodBuffs();
                    _selectedWorker.StartCommandTrace("SCENARIO", "Proving Grounds: Hunting Scenario (Long-Range Stalk, Snipe, Workbench & 3-Food Sustenance)", "proving_hunting");
                }
                VerificationTelemetry.RecordPass("proving_hunting", "Hunting Proving Grounds scenario launched with active hunter, workbench, stocked supply chest, and wild game at 30m-45m.");
            }
            SetOpen(false);
        }

        private void RunArcheryTargetScenario()
        {
            if (Player.m_localPlayer == null || ZNetScene.instance == null) return;
            Vector3 pos = Player.m_localPlayer.transform.position;
            Vector3 fwd = Player.m_localPlayer.transform.forward;

            // 1. Spawn a wooden wall 25m ahead
            GameObject wallPrefab = ZNetScene.instance.GetPrefab("woodwall") ?? ZNetScene.instance.GetPrefab("wood_wall_log_4x2");
            Vector3 wallPos = pos + fwd * 25f;
            if (wallPrefab) UnityEngine.Object.Instantiate(wallPrefab, wallPos, Quaternion.LookRotation(fwd));

            // 2. Spawn an archery target piece or target prop on the wall
            GameObject targetPrefab = ZNetScene.instance.GetPrefab("piece_target") ?? ZNetScene.instance.GetPrefab("piece_target_round") ?? ZNetScene.instance.GetPrefab("training_dummy");
            if (targetPrefab != null)
            {
                UnityEngine.Object.Instantiate(targetPrefab, wallPos + Vector3.up * 1f, Quaternion.LookRotation(-fwd));
            }
            else
            {
                // Fallback: spawn stationary training target
                SpawnWorldProp("Boar", 25f);
            }

            // 3. Spawn Hunter 2m ahead of player facing the target 23m away
            var config = new WorkerCustomization
            {
                Name = "Marksman Arn",
                Weapon = "BowFineWood",
                InitialRole = WorkerRole.Hunter,
                DesignatedResources = "RawMeat,DeerHide,LeatherScraps,Feathers"
            };
            var worker = WorkerPrefabManager.SpawnCustomWorker(config, pos + fwd * 2f, Player.m_localPlayer.transform.rotation);
            if (worker)
            {
                GameObject arrowPrefab = ZNetScene.instance.GetPrefab("ArrowWood");
                if (arrowPrefab) worker.GetComponent<Humanoid>()?.GetInventory()?.AddItem(arrowPrefab, 50);
                _selectedWorker = worker.GetComponent<WorkerBrain>();
                _selectedWorker?.StartCommandTrace("SCENARIO", "Proving Grounds: Stationary Archery Range Scenario (25m Parabolic Ballistics)", "proving_archery_target");
                VerificationTelemetry.RecordPass("proving_archery_target", "Stationary Archery Range scenario launched with target at 25m and armed hunter.");
            }
            SetOpen(false);
        }

        private void RunSmelterKilnScenario()
        {
            if (Player.m_localPlayer == null || ZNetScene.instance == null) return;
            Vector3 pos = Player.m_localPlayer.transform.position;
            Vector3 fwd = Player.m_localPlayer.transform.forward;
            Vector3 right = Player.m_localPlayer.transform.right;

            // 1. Spawn unlit Smelter 5m ahead to the left
            Vector3 smelterPos = pos + fwd * 5f - right * 2.5f;
            GameObject smelterPrefab = ZNetScene.instance.GetPrefab("smelter");
            if (smelterPrefab) UnityEngine.Object.Instantiate(smelterPrefab, smelterPos, Quaternion.LookRotation(-fwd));

            // 2. Spawn Charcoal Kiln 5m ahead to the right
            Vector3 kilnPos = pos + fwd * 5f + right * 2.5f;
            GameObject kilnPrefab = ZNetScene.instance.GetPrefab("charcoal_kiln");
            if (kilnPrefab) UnityEngine.Object.Instantiate(kilnPrefab, kilnPos, Quaternion.LookRotation(-fwd));

            // 3. Spawn Wood Chest with 50 wood next to kiln
            GameObject chestPrefab = ZNetScene.instance.GetPrefab("piece_chest_wood");
            if (chestPrefab)
            {
                Vector3 woodChestPos = kilnPos - fwd * 2f;
                var woodChestGo = UnityEngine.Object.Instantiate(chestPrefab, woodChestPos, Quaternion.LookRotation(-fwd));
                var woodCont = woodChestGo.GetComponent<Container>();
                if (woodCont != null)
                {
                    GameObject woodItem = ZNetScene.instance.GetPrefab("Wood");
                    if (woodItem) woodCont.GetInventory()?.AddItem(woodItem, 50);
                }

                // 4. Spawn Ore Chest with 20 CopperOre next to smelter
                Vector3 oreChestPos = smelterPos - fwd * 2f;
                var oreChestGo = UnityEngine.Object.Instantiate(chestPrefab, oreChestPos, Quaternion.LookRotation(-fwd));
                var oreCont = oreChestGo.GetComponent<Container>();
                if (oreCont != null)
                {
                    GameObject oreItem = ZNetScene.instance.GetPrefab("CopperOre");
                    if (oreItem) oreCont.GetInventory()?.AddItem(oreItem, 20);
                }
            }

            // 5. Spawn Base Tender worker 2m ahead
            var config = new WorkerCustomization
            {
                Name = "Smith Torvald",
                Weapon = "Hammer",
                InitialRole = WorkerRole.BaseTender
            };
            var worker = WorkerPrefabManager.SpawnCustomWorker(config, pos + fwd * 2f, Player.m_localPlayer.transform.rotation);
            if (worker)
            {
                _selectedWorker = worker.GetComponent<WorkerBrain>();
                _selectedWorker?.SetHome(pos + fwd * 3f, false);
                _selectedWorker?.StartCommandTrace("SCENARIO", "Proving Grounds: Smelter & Kiln Autonomous Fueling Scenario", "proving_smelter_kiln");
                VerificationTelemetry.RecordPass("proving_smelter_kiln", "Smelter & Kiln scenario launched with unlit smelter, kiln, ore chest, and wood chest.");
            }
            SetOpen(false);
        }

        private void CultivatePlot(Vector3 center, float radius)
        {
            try
            {
                TerrainModifier.SetTriggerOnPlaced(true);
                GameObject modGo = new GameObject("PlotCultivateMod");
                modGo.transform.position = center;
                var tm = modGo.AddComponent<TerrainModifier>();
                tm.m_paintCleared = true;
                tm.m_paintType = TerrainModifier.PaintType.Cultivate;
                tm.m_paintRadius = radius;
                tm.m_playerModifiction = true;

                List<Heightmap> hms = new List<Heightmap>();
                Heightmap.FindHeightmap(center, radius + 1f, hms);
                foreach (var hm in hms)
                {
                    hm.Poke(0, true);
                }
            }
            catch (Exception ex)
            {
                ValheimWorkersPlugin.Log.LogWarning($"CultivatePlot error: {ex.Message}");
            }
        }

        private void GivePlayerItem(string prefabName, int amount, Action<ItemDrop.ItemData> configure = null)
        {
            if (Player.m_localPlayer == null || ZNetScene.instance == null) return;
            GameObject prefab = ZNetScene.instance.GetPrefab(prefabName);
            if (prefab != null)
            {
                var inv = Player.m_localPlayer.GetInventory();
                if (inv != null && inv.AddItem(prefab, amount))
                {
                    var itemDrop = prefab.GetComponent<ItemDrop>();
                    if (itemDrop != null)
                    {
                        var item = inv.GetItem(itemDrop.m_itemData.m_shared.m_name);
                        if (item != null) configure?.Invoke(item);
                    }
                    Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, $"Added {amount}x {prefabName} to inventory.");
                }
            }
        }

        private void RunBloodSacrificeScenario()
        {
            if (Player.m_localPlayer == null || ZNetScene.instance == null) return;
            Vector3 playerPos = Player.m_localPlayer.transform.position;
            Vector3 forward = Player.m_localPlayer.transform.forward;

            // 1. Give player Sacrificial Dagger and Blood Chalice (with 50 blood)
            var pInv = Player.m_localPlayer.GetInventory();
            if (pInv != null)
            {
                GameObject daggerObj = ZNetScene.instance.GetPrefab(RelicPrefabManager.PrefabSacrificialDagger);
                if (daggerObj != null) pInv.AddItem(daggerObj, 1);

                GameObject chaliceObj = ZNetScene.instance.GetPrefab(RelicPrefabManager.PrefabBloodChalice);
                if (chaliceObj != null && pInv.AddItem(chaliceObj, 1))
                {
                    var chaliceDrop = chaliceObj.GetComponent<ItemDrop>();
                    if (chaliceDrop != null)
                    {
                        var chaliceItem = pInv.GetItem(chaliceDrop.m_itemData.m_shared.m_name);
                        if (chaliceItem != null) RelicPrefabManager.SetChaliceBlood(chaliceItem, 50);
                    }
                }
            }

            // 2. Drop 20x BoneFragments 3m ahead of player
            GameObject bonePrefab = ZNetScene.instance.GetPrefab("BoneFragments");
            Vector3 bonePos = playerPos + forward * 3.0f;
            if (bonePrefab != null)
            {
                GameObject dropObj = UnityEngine.Object.Instantiate(bonePrefab, bonePos, Quaternion.identity);
                var itemDrop = dropObj.GetComponent<ItemDrop>();
                if (itemDrop != null)
                {
                    itemDrop.m_itemData.m_stack = 20;
                }
            }

            // 3. Spawn a tamed Boar 3.0m ahead (directly over the bones)
            GameObject boarPrefab = ZNetScene.instance.GetPrefab("Boar");
            if (boarPrefab != null)
            {
                GameObject boarGo = UnityEngine.Object.Instantiate(boarPrefab, bonePos + Vector3.up * 0.2f, Quaternion.identity);
                var charComp = boarGo.GetComponent<Character>();
                if (charComp != null)
                {
                    charComp.SetTamed(true);
                }
            }

            Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Sacrificial Scenario Spawned! Equip the Sacrificial Dagger and strike the Boar over the bones!");
            VerificationTelemetry.RecordPass("scenario_blood_sacrifice", "Spawned Blood Sacrifice scenario with dagger, chalice, ground bones, and tamed boar.");
        }

        private void RunFarmingScenario()
        {
            if (Player.m_localPlayer == null || ZNetScene.instance == null) return;
            Vector3 pos = Player.m_localPlayer.transform.position;
            Vector3 fwd = Player.m_localPlayer.transform.forward;
            Vector3 right = Player.m_localPlayer.transform.right;

            // 1. Spawn storage chest 3m behind player
            Vector3 chestPos = pos - fwd * 3f;
            GameObject chest = ZNetScene.instance.GetPrefab("piece_chest_wood");
            if (chest) UnityEngine.Object.Instantiate(chest, chestPos, Quaternion.LookRotation(fwd));

            // 2. Pre-cultivate ground under crop plot
            CultivatePlot(pos + fwd * 4.1f, 3.5f);

            // 3. Spawn 4 mature carrot crops 4m ahead
            GameObject carrotPrefab = ZNetScene.instance.GetPrefab("Pickable_Carrot") ?? ZNetScene.instance.GetPrefab("Pickable_Turnip") ?? ZNetScene.instance.GetPrefab("Carrot");
            if (carrotPrefab)
            {
                UnityEngine.Object.Instantiate(carrotPrefab, pos + fwd * 3.5f - right * 1.2f, Quaternion.identity);
                UnityEngine.Object.Instantiate(carrotPrefab, pos + fwd * 3.5f + right * 1.2f, Quaternion.identity);
                UnityEngine.Object.Instantiate(carrotPrefab, pos + fwd * 4.8f - right * 1.2f, Quaternion.identity);
                UnityEngine.Object.Instantiate(carrotPrefab, pos + fwd * 4.8f + right * 1.2f, Quaternion.identity);
            }

            // 4. Spawn Farmer Freja 2m ahead
            var config = new WorkerCustomization
            {
                Name = "Farmer Freja",
                BasePrefab = "Worker_Peasant",
                InitialRole = WorkerRole.Farmer,
                Weapon = "Cultivator"
            };
            var workerGo = WorkerPrefabManager.SpawnCustomWorker(config, pos + fwd * 2f, Player.m_localPlayer.transform.rotation);
            if (workerGo)
            {
                var brain = workerGo.GetComponent<WorkerBrain>();
                var humanoid = workerGo.GetComponent<Humanoid>();
                if (brain != null)
                {
                    brain.SetHome(chestPos, false);
                    _selectedWorker = brain;
                    brain.StartCommandTrace("SCENARIO", "Proving Grounds: Farming Scenario (Mature Crop Harvest & Pantry Storage)", "proving_farming");
                    VerificationTelemetry.RecordPass("proving_farming", "Farming Proving Grounds scenario launched with ripe crop plot, cultivator and storage chest.");
                }
                if (humanoid != null)
                {
                    GameObject seedPrefab = ZNetScene.instance.GetPrefab("CarrotSeeds");
                    if (seedPrefab) humanoid.GetInventory()?.AddItem(seedPrefab, 10);
                }
            }
            SetOpen(false);
        }

        private void RunCookingScenario()
        {
            if (Player.m_localPlayer == null || ZNetScene.instance == null) return;
            Vector3 pos = Player.m_localPlayer.transform.position;
            Vector3 fwd = Player.m_localPlayer.transform.forward;

            // 1. Spawn campfire 4m ahead
            Vector3 firePos = pos + fwd * 4f;
            GameObject firePrefab = ZNetScene.instance.GetPrefab("fire_pit");
            if (firePrefab)
            {
                var fireObj = UnityEngine.Object.Instantiate(firePrefab, firePos, Quaternion.identity);
                fireObj.GetComponent<Fireplace>()?.SetFuel(10f);
            }

            // 2. Spawn cooking station directly over the fire
            GameObject cookPrefab = ZNetScene.instance.GetPrefab("piece_cookingstation");
            if (cookPrefab)
            {
                UnityEngine.Object.Instantiate(cookPrefab, firePos, Quaternion.identity);
            }

            // 3. Spawn pantry chest next to fire containing 10x RawMeat
            Vector3 chestPos = firePos + Player.m_localPlayer.transform.right * 2.5f;
            GameObject chestPrefab = ZNetScene.instance.GetPrefab("piece_chest_wood");
            if (chestPrefab)
            {
                var chestObj = UnityEngine.Object.Instantiate(chestPrefab, chestPos, Quaternion.identity);
                var chest = chestObj.GetComponent<Container>();
                GameObject meatPrefab = ZNetScene.instance.GetPrefab("RawMeat") ?? ZNetScene.instance.GetPrefab("Meat");
                if (chest && meatPrefab)
                {
                    chest.GetInventory()?.AddItem(meatPrefab, 10);
                }
            }

            // 4. Spawn Cook Olav 2m ahead
            var config = new WorkerCustomization
            {
                Name = "Cook Olav",
                BasePrefab = "Worker_Peasant",
                InitialRole = WorkerRole.Cook,
                Weapon = "KnifeFlint"
            };
            var workerGo = WorkerPrefabManager.SpawnCustomWorker(config, pos + fwd * 2f, Player.m_localPlayer.transform.rotation);
            if (workerGo)
            {
                var brain = workerGo.GetComponent<WorkerBrain>();
                var humanoid = workerGo.GetComponent<Humanoid>();
                if (brain != null)
                {
                    brain.SetHome(chestPos, false);
                    _selectedWorker = brain;
                    brain.StartCommandTrace("SCENARIO", "Proving Grounds: Cooking Scenario (Grilling Meat & Burn Prevention)", "proving_cooking");
                    VerificationTelemetry.RecordPass("proving_cooking", "Cooking Proving Grounds scenario launched with lit fire, spit, raw meat chest and Cook Olav.");
                }
                if (humanoid != null)
                {
                    GameObject knifePrefab = ZNetScene.instance.GetPrefab("KnifeFlint") ?? ZNetScene.instance.GetPrefab("Club");
                    if (knifePrefab) humanoid.GetInventory()?.AddItem(knifePrefab, 1);
                    GameObject meatPrefab = ZNetScene.instance.GetPrefab("RawMeat") ?? ZNetScene.instance.GetPrefab("Meat");
                    if (meatPrefab) humanoid.GetInventory()?.AddItem(meatPrefab, 5);
                }
            }
            SetOpen(false);
        }

        private void RunForagingScenario()
        {
            if (Player.m_localPlayer == null || ZNetScene.instance == null) return;
            Vector3 pos = Player.m_localPlayer.transform.position;
            Vector3 fwd = Player.m_localPlayer.transform.forward;
            Vector3 right = Player.m_localPlayer.transform.right;

            // 1. Spawn pantry chest 3m behind player
            Vector3 chestPos = pos - fwd * 3f;
            GameObject chest = ZNetScene.instance.GetPrefab("piece_chest_wood");
            if (chest) UnityEngine.Object.Instantiate(chest, chestPos, Quaternion.LookRotation(fwd));

            // 2. Spawn Raspberries and Mushrooms 4m ahead
            GameObject berryPrefab = ZNetScene.instance.GetPrefab("Raspberry") ?? ZNetScene.instance.GetPrefab("Pickable_Raspberry");
            if (berryPrefab)
            {
                UnityEngine.Object.Instantiate(berryPrefab, pos + fwd * 4f - right * 1.5f, Quaternion.identity);
            }

            GameObject shroomPrefab = ZNetScene.instance.GetPrefab("Mushroom") ?? ZNetScene.instance.GetPrefab("Pickable_Mushroom");
            if (shroomPrefab)
            {
                UnityEngine.Object.Instantiate(shroomPrefab, pos + fwd * 4.5f + right * 1.5f, Quaternion.identity);
            }

            // 3. Spawn Beehive 5m ahead with ready honey
            GameObject hivePrefab = ZNetScene.instance.GetPrefab("piece_beehive");
            if (hivePrefab)
            {
                var hiveObj = UnityEngine.Object.Instantiate(hivePrefab, pos + fwd * 5f, Quaternion.identity);
                var hive = hiveObj.GetComponent<Beehive>();
                if (hive != null)
                {
                    Traverse.Create(hive).Method("IncreseLevel").GetValue();
                    Traverse.Create(hive).Method("IncreseLevel").GetValue();
                }
            }

            // 4. Spawn Gatherer Astrid 2m ahead
            var config = new WorkerCustomization
            {
                Name = "Gatherer Astrid",
                BasePrefab = "Worker_Peasant",
                InitialRole = WorkerRole.Gatherer,
                DesignatedResources = "Raspberry,Blueberries,Mushroom,MushroomYellow,Honey,Thistle"
            };
            var workerGo = WorkerPrefabManager.SpawnCustomWorker(config, pos + fwd * 2f, Player.m_localPlayer.transform.rotation);
            if (workerGo)
            {
                var brain = workerGo.GetComponent<WorkerBrain>();
                if (brain != null)
                {
                    brain.SetHome(chestPos, false);
                    _selectedWorker = brain;
                    brain.StartCommandTrace("SCENARIO", "Proving Grounds: Foraging & Beehive Honey Scenario", "proving_foraging");
                    VerificationTelemetry.RecordPass("proving_foraging", "Foraging Proving Grounds scenario launched with wild berries, mushrooms, and full beehive.");
                }
            }
            SetOpen(false);
        }

        private void SpawnCookingStationProp(float distance)
        {
            if (Player.m_localPlayer == null || ZNetScene.instance == null) return;
            Vector3 pos = Player.m_localPlayer.transform.position + Player.m_localPlayer.transform.forward * distance;

            GameObject firePrefab = ZNetScene.instance.GetPrefab("fire_pit");
            if (firePrefab)
            {
                var fire = UnityEngine.Object.Instantiate(firePrefab, pos, Quaternion.identity);
                fire.GetComponent<Fireplace>()?.SetFuel(10f);
            }

            GameObject cookPrefab = ZNetScene.instance.GetPrefab("piece_cookingstation");
            if (cookPrefab)
            {
                UnityEngine.Object.Instantiate(cookPrefab, pos, Quaternion.identity);
            }
        }

        private void SpawnBeehiveProp(float distance)
        {
            if (Player.m_localPlayer == null || ZNetScene.instance == null) return;
            Vector3 pos = Player.m_localPlayer.transform.position + Player.m_localPlayer.transform.forward * distance;

            GameObject hivePrefab = ZNetScene.instance.GetPrefab("piece_beehive");
            if (hivePrefab)
            {
                var hiveObj = UnityEngine.Object.Instantiate(hivePrefab, pos, Quaternion.identity);
                var hive = hiveObj.GetComponent<Beehive>();
                if (hive != null)
                {
                    Traverse.Create(hive).Method("IncreseLevel").GetValue();
                    Traverse.Create(hive).Method("IncreseLevel").GetValue();
                }
            }
        }

        private void SpawnCarrotPatch(float distance)
        {
            if (Player.m_localPlayer == null || ZNetScene.instance == null) return;
            Vector3 pos = Player.m_localPlayer.transform.position + Player.m_localPlayer.transform.forward * distance;
            Vector3 right = Player.m_localPlayer.transform.right;

            GameObject carrotPrefab = ZNetScene.instance.GetPrefab("Pickable_Carrot") ?? ZNetScene.instance.GetPrefab("Carrot");
            if (carrotPrefab)
            {
                UnityEngine.Object.Instantiate(carrotPrefab, pos - right * 1f, Quaternion.identity);
                UnityEngine.Object.Instantiate(carrotPrefab, pos, Quaternion.identity);
                UnityEngine.Object.Instantiate(carrotPrefab, pos + right * 1f, Quaternion.identity);
            }
        }

        private void RunTrollThreatScenario()
        {
            if (Player.m_localPlayer == null || ZNetScene.instance == null) return;
            Vector3 pos = Player.m_localPlayer.transform.position;
            Vector3 fwd = Player.m_localPlayer.transform.forward;

            // Spawn home camp/chest 22m behind player so worker has a clear safe home to sprint to
            GameObject chest = ZNetScene.instance.GetPrefab("piece_chest_wood");
            if (chest) UnityEngine.Object.Instantiate(chest, pos - fwd * 20f, Quaternion.identity);

            var config = new WorkerCustomization
            {
                Name = "Laborer Leif",
                Weapon = "Club",
                InitialRole = WorkerRole.Hauler,
                MaxHealth = 100f
            };
            var worker = WorkerPrefabManager.SpawnCustomWorker(config, pos + fwd * 2f, Player.m_localPlayer.transform.rotation);
            if (worker)
            {
                var brain = worker.GetComponent<WorkerBrain>();
                if (brain != null)
                {
                    brain.SetHome(pos - fwd * 20f, true);
                    _selectedWorker = brain;
                    brain.StartCommandTrace("SCENARIO", "Proving Grounds: Troll Threat Scenario (Sprint to safe camp 22m behind)", "defense_tactical_retreat");
                }
            }

            SpawnWorldProp("Troll", 22f);
            SetOpen(false);
        }

        private void RunLowHpScenario()
        {
            if (Player.m_localPlayer == null || ZNetScene.instance == null) return;
            Vector3 pos = Player.m_localPlayer.transform.position;
            Vector3 fwd = Player.m_localPlayer.transform.forward;

            // Spawn safe camp chest 18m behind player
            GameObject chest = ZNetScene.instance.GetPrefab("piece_chest_wood");
            if (chest) UnityEngine.Object.Instantiate(chest, pos - fwd * 18f, Quaternion.identity);

            var config = new WorkerCustomization
            {
                Name = "Wounded Ragnar",
                Weapon = "Club",
                InitialRole = WorkerRole.Gatherer,
                MaxHealth = 100f
            };
            var workerGo = WorkerPrefabManager.SpawnCustomWorker(config, pos + fwd * 2f, Player.m_localPlayer.transform.rotation);
            if (workerGo)
            {
                var brain = workerGo.GetComponent<WorkerBrain>();
                var humanoid = workerGo.GetComponent<Humanoid>();
                if (brain != null)
                {
                    brain.SetHome(pos - fwd * 18f, true);
                    _selectedWorker = brain;
                    brain.StartCommandTrace("SCENARIO", "Proving Grounds: Low-HP Retreat Scenario (Disengage below 35% HP)", "defense_low_hp_retreat");
                }
                if (humanoid != null)
                {
                    // Set health to 28 HP (below 35% threshold)
                    humanoid.SetHealth(28f);
                }
            }

            SpawnWorldProp("Greydwarf", 6f);
            SetOpen(false);
        }

        // =========================================================================
        // MAP EXPEDITIONS & WAYFINDER SIGN STONES
        // =========================================================================
        private static GameObject SpawnWayfinderSignStone(Vector3 signPos, Vector3 targetPos, string siteName, string hintText)
        {
            if (ZNetScene.instance == null) return null;

            if (ZoneSystem.instance != null && ZoneSystem.instance.GetGroundHeight(signPos, out float gh))
            {
                signPos.y = gh;
            }

            Vector3 toTarget = targetPos - signPos;
            toTarget.y = 0f;
            float dist = toTarget.magnitude;
            float angle = Vector3.SignedAngle(Vector3.forward, toTarget, Vector3.up);

            string compass = "⬆️ N";
            if (angle >= -22.5f && angle < 22.5f) compass = "⬆️ N";
            else if (angle >= 22.5f && angle < 67.5f) compass = "↗️ NE";
            else if (angle >= 67.5f && angle < 112.5f) compass = "➡️ E";
            else if (angle >= 112.5f && angle < 157.5f) compass = "↘️ SE";
            else if (angle >= 157.5f || angle < -157.5f) compass = "⬇️ S";
            else if (angle >= -157.5f && angle < -112.5f) compass = "↙️ SW";
            else if (angle >= -112.5f && angle < -67.5f) compass = "⬅️ W";
            else if (angle >= -67.5f && angle < -22.5f) compass = "↖️ NW";

            string inscription = $"{compass} {dist:F0}m: {siteName}\n{hintText}";

            // 1. Standing Stone Monument
            GameObject stonePrefab = ZNetScene.instance.GetPrefab("RuneStone") ?? ZNetScene.instance.GetPrefab("guard_stone") ?? ZNetScene.instance.GetPrefab("Rock_4");
            GameObject stoneGo = null;
            Quaternion stoneRot = Quaternion.LookRotation(toTarget.normalized);
            if (stonePrefab != null)
            {
                stoneGo = UnityEngine.Object.Instantiate(stonePrefab, signPos, stoneRot);
            }

            // 2. Wooden Sign
            GameObject signPrefab = ZNetScene.instance.GetPrefab("sign");
            if (signPrefab != null)
            {
                Vector3 signPosOffset = signPos + Vector3.up * 1.1f - stoneRot * Vector3.forward * 0.4f;
                GameObject signGo = UnityEngine.Object.Instantiate(signPrefab, signPosOffset, stoneRot);
                var signComp = signGo.GetComponent<Sign>();
                if (signComp != null)
                {
                    signComp.SetText(inscription);
                }
                var nview = signGo.GetComponent<ZNetView>();
                if (nview != null && nview.GetZDO() != null)
                {
                    nview.GetZDO().Set("text", inscription);
                }
            }

            // 3. Minimap Pin
            try
            {
                if (Minimap.instance != null)
                {
                    Minimap.instance.AddPin(targetPos, Minimap.PinType.Icon3, $"{siteName} ({dist:F0}m)", true, false);
                }
            }
            catch (Exception ex)
            {
                ValheimWorkersPlugin.Log.LogWarning($"Could not add minimap pin: {ex.Message}");
            }

            // 4. Telemetry & notification
            VerificationTelemetry.RecordPass("expedition_signstone_spawn", $"Spawned Wayfinder Sign Stone pointing {compass} ({dist:F0}m) to {siteName}.");
            Player.m_localPlayer?.Message(MessageHud.MessageType.Center, $"Wayfinder Sign Stone Placed:\n{compass} {dist:F0}m -> {siteName}");

            return stoneGo;
        }

        private static Container SpawnChestWithItems(Vector3 pos, Quaternion rot, params (string prefabName, int amount)[] items)
        {
            if (ZNetScene.instance == null) return null;
            GameObject chestPrefab = ZNetScene.instance.GetPrefab("piece_chest_wood") ?? ZNetScene.instance.GetPrefab("piece_chest");
            if (chestPrefab == null) return null;
            GameObject inst = UnityEngine.Object.Instantiate(chestPrefab, pos, rot);
            var container = inst.GetComponent<Container>();
            if (container != null && container.GetInventory() != null)
            {
                foreach (var item in items)
                {
                    GameObject p = ZNetScene.instance.GetPrefab(item.prefabName);
                    if (p != null) container.GetInventory().AddItem(p, item.amount);
                }
            }
            return container;
        }

        private void RunExpeditionBarrowOfSkarde()
        {
            if (Player.m_localPlayer == null || ZNetScene.instance == null) return;
            Vector3 playerPos = Player.m_localPlayer.transform.position;
            Vector3 playerFwd = Player.m_localPlayer.transform.forward;

            Vector3 targetPos = playerPos + Vector3.forward * 40f;
            if (ZoneSystem.instance != null && ZoneSystem.instance.GetGroundHeight(targetPos, out float gh)) targetPos.y = gh;

            // Spawn Sign Stone 3m ahead of player pointing North to Barrow
            SpawnWayfinderSignStone(playerPos + playerFwd * 3f, targetPos, "Barrow of Skarde", "Uncover the Sacrificial Dagger");

            // Build Barrow of Skarde environment at targetPos
            GameObject stoneCircle = ZNetScene.instance.GetPrefab("StoneCircle") ?? ZNetScene.instance.GetPrefab("Rock_4");
            if (stoneCircle != null) UnityEngine.Object.Instantiate(stoneCircle, targetPos, Quaternion.identity);

            // Relic chest with Dagger & BoneFragments
            SpawnChestWithItems(targetPos + Vector3.forward * 1.5f, Quaternion.identity,
                (RelicPrefabManager.PrefabSacrificialDagger, 1),
                ("BoneFragments", 30));

            // Bone piles on the ground
            GameObject bones = ZNetScene.instance.GetPrefab("BoneFragments");
            if (bones != null)
            {
                var drop1 = UnityEngine.Object.Instantiate(bones, targetPos + Vector3.left * 2f + Vector3.up * 0.2f, Quaternion.identity);
                var itemDrop1 = drop1.GetComponent<ItemDrop>();
                if (itemDrop1 != null) itemDrop1.m_itemData.m_stack = 10;

                var drop2 = UnityEngine.Object.Instantiate(bones, targetPos + Vector3.right * 2f + Vector3.up * 0.2f, Quaternion.identity);
                var itemDrop2 = drop2.GetComponent<ItemDrop>();
                if (itemDrop2 != null) itemDrop2.m_itemData.m_stack = 10;
            }

            // 2 Hostile Skeleton Guards
            GameObject skelPrefab = ZNetScene.instance.GetPrefab("Skeleton");
            if (skelPrefab != null)
            {
                UnityEngine.Object.Instantiate(skelPrefab, targetPos + Vector3.forward * 3f + Vector3.left * 2f, Quaternion.identity);
                UnityEngine.Object.Instantiate(skelPrefab, targetPos + Vector3.forward * 3f + Vector3.right * 2f, Quaternion.identity);
            }

            // Boar Pen nearby (8m to the side)
            GameObject boarPrefab = ZNetScene.instance.GetPrefab("Boar");
            if (boarPrefab != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector3 boarPos = targetPos + Vector3.left * (6f + i * 2f);
                    if (ZoneSystem.instance != null && ZoneSystem.instance.GetGroundHeight(boarPos, out float bgh)) boarPos.y = bgh;
                    var bGo = UnityEngine.Object.Instantiate(boarPrefab, boarPos, Quaternion.identity);
                    bGo.GetComponent<Character>()?.SetTamed(true);
                }
            }

            SetOpen(false);
        }

        private void RunExpeditionCryptOfValka()
        {
            if (Player.m_localPlayer == null || ZNetScene.instance == null) return;
            Vector3 playerPos = Player.m_localPlayer.transform.position;
            Vector3 playerFwd = Player.m_localPlayer.transform.forward;

            Vector3 neDir = (Vector3.forward + Vector3.right).normalized;
            Vector3 targetPos = playerPos + neDir * 45f;
            if (ZoneSystem.instance != null && ZoneSystem.instance.GetGroundHeight(targetPos, out float gh)) targetPos.y = gh;

            SpawnWayfinderSignStone(playerPos + playerFwd * 3f, targetPos, "Crypt of Valka the Red", "Uncover the Blood Chalice");

            // Sunken ship stones / barrow altar
            GameObject firePrefab = ZNetScene.instance.GetPrefab("fire_pit");
            if (firePrefab != null) UnityEngine.Object.Instantiate(firePrefab, targetPos, Quaternion.identity);

            GameObject shrineStone = ZNetScene.instance.GetPrefab("guard_stone") ?? ZNetScene.instance.GetPrefab("Rock_4");
            if (shrineStone != null) UnityEngine.Object.Instantiate(shrineStone, targetPos + Vector3.forward * 2f, Quaternion.identity);

            // Relic chest with Chalice, Dagger & Bones
            SpawnChestWithItems(targetPos + Vector3.forward * 1.5f + Vector3.right * 1f, Quaternion.identity,
                (RelicPrefabManager.PrefabBloodChalice, 1),
                (RelicPrefabManager.PrefabSacrificialDagger, 1),
                ("BoneFragments", 40));

            // Wildlife around the altar
            string[] critters = new[] { "Neck", "Boar", "Deer", "Wolf" };
            for (int i = 0; i < critters.Length; i++)
            {
                GameObject critterPrefab = ZNetScene.instance.GetPrefab(critters[i]);
                if (critterPrefab != null)
                {
                    Vector3 critterPos = targetPos + Quaternion.Euler(0, i * 90f, 0) * (Vector3.forward * 5f);
                    if (ZoneSystem.instance != null && ZoneSystem.instance.GetGroundHeight(critterPos, out float cgh)) critterPos.y = cgh;
                    UnityEngine.Object.Instantiate(critterPrefab, critterPos, Quaternion.identity);
                }
            }

            SetOpen(false);
        }

        private void RunExpeditionVikingAwakeningTrail()
        {
            if (Player.m_localPlayer == null || ZNetScene.instance == null) return;
            Vector3 playerPos = Player.m_localPlayer.transform.position;
            Vector3 playerFwd = Player.m_localPlayer.transform.forward;

            Vector3 targetPos = playerPos + Vector3.right * 40f;
            if (ZoneSystem.instance != null && ZoneSystem.instance.GetGroundHeight(targetPos, out float gh)) targetPos.y = gh;

            SpawnWayfinderSignStone(playerPos + playerFwd * 3f, targetPos, "Viking Awakening Trail", "4 Shrines: Campfire -> Homestead -> Stone -> Mead-Hall");

            // Stage 1 (18m E): Wayside Campfire
            Vector3 s1 = playerPos + Vector3.right * 18f;
            if (ZoneSystem.instance != null && ZoneSystem.instance.GetGroundHeight(s1, out float gh1)) s1.y = gh1;
            GameObject fire1 = ZNetScene.instance.GetPrefab("fire_pit");
            if (fire1 != null) UnityEngine.Object.Instantiate(fire1, s1, Quaternion.identity);
            SpawnChestWithItems(s1 + Vector3.forward * 1.5f, Quaternion.identity,
                ("ArmorRagsChest", 1), ("CookedMeat", 10), ("Raspberry", 10));

            // Stage 2 (32m E): Ruined Homestead
            Vector3 s2 = playerPos + Vector3.right * 32f;
            if (ZoneSystem.instance != null && ZoneSystem.instance.GetGroundHeight(s2, out float gh2)) s2.y = gh2;
            GameObject wall = ZNetScene.instance.GetPrefab("woodwall");
            if (wall != null)
            {
                UnityEngine.Object.Instantiate(wall, s2 + Vector3.left * 1.5f, Quaternion.identity);
                UnityEngine.Object.Instantiate(wall, s2 + Vector3.right * 1.5f, Quaternion.identity);
            }
            GameObject greydwarf = ZNetScene.instance.GetPrefab("Greydwarf");
            if (greydwarf != null)
            {
                UnityEngine.Object.Instantiate(greydwarf, s2 + Vector3.forward * 2f, Quaternion.identity);
                UnityEngine.Object.Instantiate(greydwarf, s2 - Vector3.forward * 2f, Quaternion.identity);
            }
            SpawnChestWithItems(s2 + Vector3.forward * 1.5f, Quaternion.identity,
                ("Hammer", 1), ("Wood", 30));

            // Stage 3 (46m E): Ancient Standing Stone
            Vector3 s3 = playerPos + Vector3.right * 46f;
            if (ZoneSystem.instance != null && ZoneSystem.instance.GetGroundHeight(s3, out float gh3)) s3.y = gh3;
            GameObject runeStone = ZNetScene.instance.GetPrefab("RuneStone") ?? ZNetScene.instance.GetPrefab("Rock_4");
            if (runeStone != null) UnityEngine.Object.Instantiate(runeStone, s3, Quaternion.identity);
            SpawnChestWithItems(s3 + Vector3.forward * 1.5f, Quaternion.identity,
                (RelicPrefabManager.PrefabAwakeningOil, 3), ("TrophyEikthyr", 1), ("AncientSeed", 5));

            // Stage 4 (60m E): High Mead-Hall
            Vector3 s4 = playerPos + Vector3.right * 60f;
            if (ZoneSystem.instance != null && ZoneSystem.instance.GetGroundHeight(s4, out float gh4)) s4.y = gh4;
            GameObject hearth = ZNetScene.instance.GetPrefab("hearth") ?? ZNetScene.instance.GetPrefab("fire_pit");
            if (hearth != null) UnityEngine.Object.Instantiate(hearth, s4, Quaternion.identity);
            GameObject throne = ZNetScene.instance.GetPrefab("piece_throne01");
            if (throne != null) UnityEngine.Object.Instantiate(throne, s4 + Vector3.forward * 2f, Quaternion.identity);
            SpawnChestWithItems(s4 + Vector3.right * 2f, Quaternion.identity,
                (RelicPrefabManager.PrefabGjallarhorn, 1), ("MeadTasty", 5), ("Sausages", 10), ("DeerStew", 5));

            SetOpen(false);
        }

        private void RunExpeditionDvergrGuildExpedition()
        {
            if (Player.m_localPlayer == null || ZNetScene.instance == null) return;
            Vector3 playerPos = Player.m_localPlayer.transform.position;
            Vector3 playerFwd = Player.m_localPlayer.transform.forward;

            Vector3 seDir = (Vector3.back + Vector3.right).normalized;
            Vector3 targetPos = playerPos + seDir * 45f;
            if (ZoneSystem.instance != null && ZoneSystem.instance.GetGroundHeight(targetPos, out float gh)) targetPos.y = gh;

            SpawnWayfinderSignStone(playerPos + playerFwd * 3f, targetPos, "Dvergr Guild Expedition", "4 Workshops: Beacon -> Brew -> Three Fires -> Troll Cave");

            // Stage 1 (18m SE): Broken Survey Beacon
            Vector3 s1 = playerPos + seDir * 18f;
            if (ZoneSystem.instance != null && ZoneSystem.instance.GetGroundHeight(s1, out float gh1)) s1.y = gh1;
            GameObject beacon = ZNetScene.instance.GetPrefab("guard_stone");
            if (beacon != null) UnityEngine.Object.Instantiate(beacon, s1, Quaternion.identity);
            SpawnChestWithItems(s1 + Vector3.forward * 1.5f, Quaternion.identity,
                ("Copper", 10), ("Bronze", 5), ("SurtlingCore", 5));

            // Stage 2 (32m SE): Prospector's Brew Camp
            Vector3 s2 = playerPos + seDir * 32f;
            if (ZoneSystem.instance != null && ZoneSystem.instance.GetGroundHeight(s2, out float gh2)) s2.y = gh2;
            GameObject ferm = ZNetScene.instance.GetPrefab("fermenter");
            if (ferm != null) UnityEngine.Object.Instantiate(ferm, s2, Quaternion.identity);
            SpawnChestWithItems(s2 + Vector3.right * 1.5f, Quaternion.identity,
                ("MeadHealthMinor", 5), ("MeadStaminaMinor", 5), ("Bronze", 10));

            // Stage 3 (46m SE): Hall of Three Fires
            Vector3 s3 = playerPos + seDir * 46f;
            if (ZoneSystem.instance != null && ZoneSystem.instance.GetGroundHeight(s3, out float gh3)) s3.y = gh3;
            GameObject kiln = ZNetScene.instance.GetPrefab("charcoal_kiln");
            if (kiln != null) UnityEngine.Object.Instantiate(kiln, s3 + Vector3.left * 2f, Quaternion.identity);
            GameObject smelter = ZNetScene.instance.GetPrefab("smelter");
            if (smelter != null) UnityEngine.Object.Instantiate(smelter, s3 + Vector3.right * 2f, Quaternion.identity);
            GameObject forge = ZNetScene.instance.GetPrefab("forge");
            if (forge != null) UnityEngine.Object.Instantiate(forge, s3 + Vector3.forward * 2f, Quaternion.identity);
            SpawnChestWithItems(s3 + Vector3.back * 2f, Quaternion.identity,
                ("Coal", 30), ("CopperOre", 20), ("TinOre", 10));

            // Stage 4 (60m SE): Plundered Troll Cave & Sunken Guild Seal
            Vector3 s4 = playerPos + seDir * 60f;
            if (ZoneSystem.instance != null && ZoneSystem.instance.GetGroundHeight(s4, out float gh4)) s4.y = gh4;
            GameObject rock = ZNetScene.instance.GetPrefab("Rock_4") ?? ZNetScene.instance.GetPrefab("Rock_destructible");
            if (rock != null)
            {
                UnityEngine.Object.Instantiate(rock, s4 + Vector3.left * 3f, Quaternion.identity);
                UnityEngine.Object.Instantiate(rock, s4 + Vector3.right * 3f, Quaternion.identity);
                UnityEngine.Object.Instantiate(rock, s4 + Vector3.forward * 3f, Quaternion.identity);
            }
            GameObject troll = ZNetScene.instance.GetPrefab("Troll");
            if (troll != null) UnityEngine.Object.Instantiate(troll, s4 + Vector3.forward * 5f, Quaternion.identity);
            SpawnChestWithItems(s4, Quaternion.identity,
                (RelicPrefabManager.PrefabDvergrSeal, 1), ("Coins", 99), ("Ruby", 20), ("AmberPearl", 20));

            SetOpen(false);
        }

        private void RunExpeditionAllSkillsProgressionArena()
        {
            if (Player.m_localPlayer == null || ZNetScene.instance == null) return;
            Vector3 playerPos = Player.m_localPlayer.transform.position;
            Vector3 playerFwd = Player.m_localPlayer.transform.forward;

            Vector3 targetPos = playerPos + Vector3.back * 25f;
            if (ZoneSystem.instance != null && ZoneSystem.instance.GetGroundHeight(targetPos, out float gh)) targetPos.y = gh;

            SpawnWayfinderSignStone(playerPos + playerFwd * 3f, targetPos, "All-Skills & Progression Proving Arena", "Training grounds for all 8 Valheim skills, relics, evolution & pride");

            // 1. Woodcutting Station
            GameObject tree = ZNetScene.instance.GetPrefab("Beech_small1") ?? ZNetScene.instance.GetPrefab("Beech1");
            if (tree != null) UnityEngine.Object.Instantiate(tree, targetPos + Vector3.left * 6f, Quaternion.identity);

            // 2. Mining Station
            GameObject rock = ZNetScene.instance.GetPrefab("MineRock_Tin") ?? ZNetScene.instance.GetPrefab("Rock_destructible");
            if (rock != null) UnityEngine.Object.Instantiate(rock, targetPos + Vector3.right * 6f, Quaternion.identity);

            // 3. Farming Station
            CultivatePlot(targetPos + Vector3.forward * 6f, 3.5f);
            GameObject carrot = ZNetScene.instance.GetPrefab("Pickable_Carrot") ?? ZNetScene.instance.GetPrefab("Carrot");
            if (carrot != null)
            {
                UnityEngine.Object.Instantiate(carrot, targetPos + Vector3.forward * 6f + Vector3.left * 1f, Quaternion.identity);
                UnityEngine.Object.Instantiate(carrot, targetPos + Vector3.forward * 6f + Vector3.right * 1f, Quaternion.identity);
            }

            // 4. Cooking & Processing Station
            GameObject fire = ZNetScene.instance.GetPrefab("fire_pit");
            if (fire != null) UnityEngine.Object.Instantiate(fire, targetPos + Vector3.back * 6f, Quaternion.identity);
            GameObject cookingSpit = ZNetScene.instance.GetPrefab("piece_cookingstation");
            if (cookingSpit != null) UnityEngine.Object.Instantiate(cookingSpit, targetPos + Vector3.back * 6f + Vector3.up * 0.5f, Quaternion.identity);
            GameObject kiln = ZNetScene.instance.GetPrefab("charcoal_kiln");
            if (kiln != null) UnityEngine.Object.Instantiate(kiln, targetPos + Vector3.back * 8f + Vector3.left * 4f, Quaternion.identity);
            GameObject smelter = ZNetScene.instance.GetPrefab("smelter");
            if (smelter != null) UnityEngine.Object.Instantiate(smelter, targetPos + Vector3.back * 8f + Vector3.right * 4f, Quaternion.identity);

            // 5. Sparring Pen
            GameObject greydwarf = ZNetScene.instance.GetPrefab("Greydwarf");
            if (greydwarf != null) UnityEngine.Object.Instantiate(greydwarf, targetPos + Vector3.forward * 10f, Quaternion.identity);

            // 6. Hauling supply & storage chests
            SpawnChestWithItems(targetPos + Vector3.left * 3f + Vector3.forward * 2f, Quaternion.identity,
                ("Wood", 20), ("Stone", 20), ("RawMeat", 10), ("CopperOre", 10));
            SpawnChestWithItems(targetPos + Vector3.right * 3f + Vector3.forward * 2f, Quaternion.identity);

            // 7. Master Trainer Chest with all 5 relics, skill tools, weapons, sacrifice and luxury items
            SpawnChestWithItems(targetPos, Quaternion.identity,
                (RelicPrefabManager.PrefabSacrificialDagger, 1),
                (RelicPrefabManager.PrefabBloodChalice, 1),
                (RelicPrefabManager.PrefabAwakeningOil, 5),
                (RelicPrefabManager.PrefabGjallarhorn, 1),
                (RelicPrefabManager.PrefabDvergrSeal, 1),
                ("AxeBronze", 1),
                ("PickaxeAntler", 1),
                ("SwordBronze", 1),
                ("ShieldWood", 1),
                ("BowFineWood", 1),
                ("ArrowWood", 50),
                ("Cultivator", 1),
                ("Hammer", 1),
                ("BoneFragments", 50),
                ("RawMeat", 20),
                ("Entrails", 20),
                ("Chain", 10),
                ("TrophyDraugrElite", 2),
                ("Coins", 200),
                ("Ruby", 20),
                ("Silver", 20),
                ("Crystal", 10));

            SetOpen(false);
        }

        private void BoostSelectedWorkerLevel(int levels)
        {
            if (_selectedWorker == null)
            {
                Player.m_localPlayer?.Message(MessageHud.MessageType.Center, "Select a worker first in Tab 2!");
                return;
            }
            _selectedWorker.Level += levels;
            _selectedWorker.SkillPoints += levels;
            _selectedWorker.SaveProgressionToZDO();
            _selectedWorker.Say($"Boosted by {levels} levels! (Now Lv. {_selectedWorker.Level}, {_selectedWorker.SkillPoints} SP)");
            VerificationTelemetry.RecordPass("actskill_overall_levelup", $"Worker '{_selectedWorker.GetHoverName()}' boosted to Lv. {_selectedWorker.Level} (+{levels} SP).");
        }

        private void BoostSelectedWorkerSkills(float boost)
        {
            if (_selectedWorker == null)
            {
                Player.m_localPlayer?.Message(MessageHud.MessageType.Center, "Select a worker first in Tab 2!");
                return;
            }
            foreach (WorkerActivitySkill s in Enum.GetValues(typeof(WorkerActivitySkill)))
            {
                _selectedWorker.RaiseActivitySkill(s, 200f * boost);
            }
            _selectedWorker.Say($"Mastered all 8 Valheim activity skills!");
        }

        private void BoostSelectedWorkerBlood(float blood)
        {
            if (_selectedWorker == null)
            {
                Player.m_localPlayer?.Message(MessageHud.MessageType.Center, "Select a worker first in Tab 2!");
                return;
            }
            _selectedWorker.AddMarrowBlood(blood);
            _selectedWorker.Say($"Absorbed {blood:F0} Marrow Blood!");
        }

        private void TriggerWorkerPride(float targetPride)
        {
            if (_selectedWorker == null)
            {
                Player.m_localPlayer?.Message(MessageHud.MessageType.Center, "Select a worker first in Tab 2!");
                return;
            }
            _selectedWorker.PrideMeter = Mathf.Clamp01(targetPride / 100f);
            _selectedWorker.SaveProgressionToZDO();
            if (targetPride <= 15f)
            {
                _selectedWorker.Say("This humiliation ends now! I refuse this slave's chore!");
                VerificationTelemetry.RecordPass("charmenu_pride_decay", $"Forced 0% Pride crisis on worker '{_selectedWorker.GetHoverName()}'.");
            }
            else
            {
                _selectedWorker.Say("I am esteemed and rewarded! Let glory follow my labor!");
                VerificationTelemetry.RecordPass("charmenu_pride_appease", $"Restored 100% Pride on worker '{_selectedWorker.GetHoverName()}'.");
            }
        }

        private void DrawResourceDesignationBlock(WorkerBrain liveWorker, WorkerCustomization config = null)
        {
            var targetConfig = config ?? liveWorker?.Customization;
            if (targetConfig == null) return;

            GUILayout.Space(6);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Resource Gathering Designation & Area", ValheimGuiTheme.SubHeaderStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            if (liveWorker != null)
            {
                GUILayout.BeginHorizontal();
                string workPostStr = liveWorker.HasWorkPost ? $"{liveWorker.WorkPostPosition:F1}" : $"Camp ({liveWorker.HomePosition:F1})";
                GUILayout.Label($"Work Center: <color=#FFA726>{workPostStr}</color>", ValheimGuiTheme.LabelParchment, GUILayout.Width(ValheimGuiTheme.ScaledF(210f)));
                GUILayout.Label($"Duty Radius: <color=#FFA726><b>{liveWorker.WorkRadius:F0} m</b></color>", ValheimGuiTheme.LabelParchment, GUILayout.Width(ValheimGuiTheme.ScaledF(135f)));
                float newRad = GUILayout.HorizontalSlider(liveWorker.WorkRadius, 5f, 150f, ValheimGuiTheme.SliderStyle, ValheimGuiTheme.SliderThumbStyle);
                if (Mathf.Abs(newRad - liveWorker.WorkRadius) > 0.5f)
                {
                    liveWorker.SetWorkRadius(newRad);
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Quick Duty Presets:", ValheimGuiTheme.LabelMuted, GUILayout.Width(ValheimGuiTheme.ScaledF(130f)));
                if (GUILayout.Button("15m", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(22f)))) liveWorker.SetWorkRadius(15f);
                if (GUILayout.Button("25m", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(22f)))) liveWorker.SetWorkRadius(25f);
                if (GUILayout.Button("50m", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(22f)))) liveWorker.SetWorkRadius(50f);
                if (GUILayout.Button("75m", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(22f)))) liveWorker.SetWorkRadius(75f);
                if (GUILayout.Button("100m", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(22f)))) liveWorker.SetWorkRadius(100f);
                if (GUILayout.Button("150m", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(22f)))) liveWorker.SetWorkRadius(150f);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label($"Return Threshold: <color=#FFA726><b>{liveWorker.ReturnFillThreshold * 100f:F0}%</b></color>", ValheimGuiTheme.LabelParchment, GUILayout.Width(ValheimGuiTheme.ScaledF(150f)));
                float newRet = GUILayout.HorizontalSlider(liveWorker.ReturnFillThreshold, 0.20f, 1.0f, ValheimGuiTheme.SliderStyle, ValheimGuiTheme.SliderThumbStyle);
                if (Mathf.Abs(newRet - liveWorker.ReturnFillThreshold) > 0.04f)
                {
                    liveWorker.SetReturnFillThreshold(newRet);
                }
                if (GUILayout.Button("25%", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(20f)))) liveWorker.SetReturnFillThreshold(0.25f);
                if (GUILayout.Button("50%", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(20f)))) liveWorker.SetReturnFillThreshold(0.50f);
                if (GUILayout.Button("75%", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(20f)))) liveWorker.SetReturnFillThreshold(0.75f);
                if (GUILayout.Button("100%", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(20f)))) liveWorker.SetReturnFillThreshold(1.0f);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Set Work Post at My Feet", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(26f))) && Player.m_localPlayer != null)
                {
                    liveWorker.SetWorkPost(Player.m_localPlayer.transform.position, true);
                }
                if (liveWorker.HasWorkPost && GUILayout.Button("Reset Work Area to Camp", ValheimGuiTheme.ButtonNormal, GUILayout.Width(ValheimGuiTheme.ScaledF(160f)), GUILayout.Height(ValheimGuiTheme.ScaledF(26f))))
                {
                    liveWorker.ClearWorkPost();
                }
                if (GUILayout.Button("Orient Camp to Base (Bed/Chest)", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(26f))))
                {
                    liveWorker.OrientToNearestBase();
                }
                if (GUILayout.Button("Locate on Map (M)", ValheimGuiTheme.ButtonSelected, GUILayout.Width(ValheimGuiTheme.ScaledF(135f)), GUILayout.Height(ValheimGuiTheme.ScaledF(26f))))
                {
                    if (Minimap.instance != null)
                    {
                        VerificationTelemetry.RecordPass("map_locate_button", $"Centered World Map on worker '{liveWorker.GetHoverName()}' at {liveWorker.transform.position:F1}.");
                        SetOpen(false);
                        Minimap.instance.ShowPointOnMap(liveWorker.transform.position);
                    }
                }
                GUILayout.EndHorizontal();
            }

            var currentList = targetConfig.GetDesignatedResourceList();

            // Category 1: Wood & Forestry
            GUILayout.Space(4);
            GUILayout.Label("Forestry & Woodcutting", ValheimGuiTheme.SubHeaderStyle);
            GUILayout.BeginHorizontal();
            DrawResourceTogglePill("Wood", "Wood", currentList, targetConfig, liveWorker);
            DrawResourceTogglePill("CoreWood", "Core Wood", currentList, targetConfig, liveWorker);
            DrawResourceTogglePill("FineWood", "Fine Wood", currentList, targetConfig, liveWorker);
            GUILayout.EndHorizontal();

            // Category 2: Stone & Mining
            GUILayout.Space(4);
            GUILayout.Label("Mining & Stone Quarrying", ValheimGuiTheme.SubHeaderStyle);
            GUILayout.BeginHorizontal();
            DrawResourceTogglePill("Stone", "Stone", currentList, targetConfig, liveWorker);
            DrawResourceTogglePill("Flint", "Flint", currentList, targetConfig, liveWorker);
            DrawResourceTogglePill("CopperOre", "Copper", currentList, targetConfig, liveWorker);
            DrawResourceTogglePill("TinOre", "Tin", currentList, targetConfig, liveWorker);
            DrawResourceTogglePill("Bronze", "Bronze (Cu+Sn)", currentList, targetConfig, liveWorker);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            DrawResourceTogglePill("IronScrap", "Iron", currentList, targetConfig, liveWorker);
            DrawResourceTogglePill("SilverOre", "Silver", currentList, targetConfig, liveWorker);
            DrawResourceTogglePill("Obsidian", "Obsidian", currentList, targetConfig, liveWorker);
            GUILayout.EndHorizontal();

            // Category 3: Hunting
            GUILayout.Space(4);
            GUILayout.Label("Wildlife & Game Hunting", ValheimGuiTheme.SubHeaderStyle);
            GUILayout.BeginHorizontal();
            DrawResourceTogglePill("RawMeat", "Raw Meat", currentList, targetConfig, liveWorker);
            DrawResourceTogglePill("DeerHide", "Deer Hide", currentList, targetConfig, liveWorker);
            DrawResourceTogglePill("LeatherScraps", "Boar Scraps", currentList, targetConfig, liveWorker);
            DrawResourceTogglePill("NeckTail", "Neck Tails", currentList, targetConfig, liveWorker);
            DrawResourceTogglePill("Feathers", "Feathers", currentList, targetConfig, liveWorker);
            GUILayout.EndHorizontal();

            // Category 4: Foraging
            GUILayout.Space(4);
            GUILayout.Label("Wild Plant Foraging", ValheimGuiTheme.SubHeaderStyle);
            GUILayout.BeginHorizontal();
            DrawResourceTogglePill("Raspberry", "Raspberry", currentList, targetConfig, liveWorker);
            DrawResourceTogglePill("Blueberries", "Blueberry", currentList, targetConfig, liveWorker);
            DrawResourceTogglePill("Mushroom", "Red Shroom", currentList, targetConfig, liveWorker);
            DrawResourceTogglePill("MushroomYellow", "Yellow Shroom", currentList, targetConfig, liveWorker);
            DrawResourceTogglePill("Thistle", "Thistle", currentList, targetConfig, liveWorker);
            GUILayout.EndHorizontal();

            // Equipment Requirement / Readiness Badge
            GUILayout.Space(6);
            DrawEquipmentStatusBadge(currentList, liveWorker);
        }

        private void DrawResourceTogglePill(string resId, string label, List<string> currentList, WorkerCustomization config, WorkerBrain liveWorker)
        {
            bool isKnown = ResourceCatalog.IsKnownByPlayer(resId);
            bool isSelected = currentList.Contains(resId);

            if (!isKnown)
            {
                VerificationTelemetry.RecordPass("gather_known_filtering", $"Known-resource filter verified: '{label}' ({resId}) is locked until discovered by player.");
                GUILayout.Button($"[🔒] {label} (Locked)", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(25f)));
                return;
            }

            GUIStyle style = isSelected ? ValheimGuiTheme.ButtonSelected : ValheimGuiTheme.ButtonNormal;
            if (GUILayout.Button(isSelected ? $"[x] {label}" : $"[ ] {label}", style, GUILayout.Height(ValheimGuiTheme.ScaledF(25f))))
            {
                if (isSelected)
                {
                    currentList.Remove(resId);
                }
                else
                {
                    currentList.Add(resId);
                }
                config.SetDesignatedResourceList(currentList);
                if (liveWorker != null)
                {
                    liveWorker.SaveToZDO();
                    liveWorker.LogAction($"Updated designated resources: {config.DesignatedResources}", true);
                }
            }
        }

        private void DrawEquipmentStatusBadge(List<string> resources, WorkerBrain liveWorker)
        {
            if (resources.Count == 0)
            {
                GUILayout.Label("<color=#ADB5BD>No specific resources selected (will default to Wood & Stone).</color>", ValheimGuiTheme.LabelMuted);
                return;
            }

            bool needsAxe = false;
            bool needsPickaxe = false;
            bool needsBow = false;
            string specificWarning = "";

            foreach (var r in resources)
            {
                var def = ResourceCatalog.Get(r);
                if (def == null) continue;
                if (def.RequiredToolType == "Axe") needsAxe = true;
                if (def.RequiredToolType == "Pickaxe") needsPickaxe = true;
                if (def.RequiredToolType == "Bow") needsBow = true;
                if (!string.IsNullOrEmpty(def.MissingToolWarning) && string.IsNullOrEmpty(specificWarning))
                {
                    specificWarning = def.MissingToolWarning;
                }
            }

            if (liveWorker != null)
            {
                bool hasAxe = liveWorker.HasAxe();
                bool hasPick = liveWorker.HasPickaxe();
                bool hasBow = liveWorker.HasToolType(ItemDrop.ItemData.ItemType.Bow, "");

                if (needsPickaxe && !hasPick)
                {
                    if (resources.Contains("Bronze") || resources.Contains("CopperOre") || resources.Contains("TinOre"))
                    {
                        GUILayout.Label("<color=#FF6B6B><b>⚠ Missing Equipment:</b> Antler Pickaxe (or better) required to mine Copper/Tin for Bronze!</color>", ValheimGuiTheme.LabelParchment);
                    }
                    else
                    {
                        GUILayout.Label("<color=#FFA94D><b>ℹ Fallback Active:</b> No Pickaxe in inventory. Worker will run around collecting loose stones.</color>", ValheimGuiTheme.LabelParchment);
                    }
                }
                else if (needsAxe && !hasAxe)
                {
                    GUILayout.Label("<color=#FFA94D><b>ℹ Fallback Active:</b> No Axe in inventory. Worker will bash small saplings and pick fallen branches.</color>", ValheimGuiTheme.LabelParchment);
                }
                else if (needsBow && !hasBow)
                {
                    GUILayout.Label("<color=#FFA94D><b>ℹ Hunting Note:</b> Bow + Arrows recommended for hunting Deer. Melee weapons will be used for Boar/Neck.</color>", ValheimGuiTheme.LabelParchment);
                }
                else
                {
                    GUILayout.Label("<color=#51CF66><b>✓ Equipment Ready:</b> Worker has required tools/weapons in inventory.</color>", ValheimGuiTheme.LabelParchment);
                }
            }
            else
            {
                string reqs = (needsAxe ? "Axe " : "") + (needsPickaxe ? "Pickaxe " : "") + (needsBow ? "Bow " : "");
                GUILayout.Label($"<color=#FFA726>Required Gear for order: {(string.IsNullOrEmpty(reqs) ? "None (Hands/Any)" : reqs)}</color>", ValheimGuiTheme.LabelParchment);
            }
        }

        private bool DrawPillButton(string label, bool active, params GUILayoutOption[] options)
        {
            GUIStyle style = active ? ValheimGuiTheme.ButtonSelected : ValheimGuiTheme.ButtonNormal;
            return GUILayout.Button(label, style, options);
        }
    }
}
