using System;
using System.Collections.Generic;
using UnityEngine;
using ValheimWorkers.AI;
using ValheimWorkers.Core;

namespace ValheimWorkers.UI
{
    public class NpcCharacterMenuGui : MonoBehaviour
    {
        public static NpcCharacterMenuGui Instance { get; private set; }
        public static bool IsOpen { get; private set; } = false;
        public static WorkerBrain CurrentWorker { get; private set; } = null;

        // Active View State
        public static int ActiveTab { get; set; } = 0; // 0 = Character & Paperdoll, 1 = Valheim Skills (0-100), 2 = Heritage Skill Tree
        public static bool ShowContainerSideDrawer { get; set; } = true;

        private Rect _fullWindowRect = new Rect(120, 60, 980, 680);
        private Rect _drawerRect = new Rect(0, 0, 390, 480);
        private Vector2 _skillsScroll = Vector2.zero;
        private Vector2 _treeScroll = Vector2.zero;
        private Vector2 _backpackScroll = Vector2.zero;

        // Selected Skill Tree Node
        private SkillNode _selectedHumanNode = null;
        private WorkerSkillTree.SkeletonFormDef _selectedSkelForm = null;
        private WorkerSkillTree.SkeletonGraftDef _selectedSkelGraft = null;
        private WorkerSkillTree.DvergrPatronageNode _selectedDvergrNode = null;

        private void Awake()
        {
            Instance = this;
        }

        public static void Open(WorkerBrain worker, int defaultTab = 0)
        {
            if (worker == null) return;
            CurrentWorker = worker;
            ActiveTab = Mathf.Clamp(defaultTab, 0, 2);
            IsOpen = true;
            PlayerController.SetTakeInputDelay(0.3f);
        }

        public static void Close()
        {
            IsOpen = false;
            PlayerController.SetTakeInputDelay(0.3f);
        }

        public static void Toggle(WorkerBrain worker, int defaultTab = 0)
        {
            if (IsOpen && CurrentWorker == worker && ActiveTab == defaultTab)
            {
                Close();
            }
            else
            {
                Open(worker, defaultTab);
            }
        }

        private void OnGUI()
        {
            ValheimGuiTheme.EnsureInitialized();

            // 1. Check if Container InventoryGui is open on a WorkerBrain (Hold [E] mode)
            WorkerBrain containerWorker = GetActiveContainerWorker();
            if (containerWorker != null && ShowContainerSideDrawer && !IsOpen)
            {
                DrawContainerSideDrawer(containerWorker);
            }

            // 2. Full NPC Character Menu Window
            if (IsOpen && CurrentWorker != null)
            {
                // Ensure window stays within screen bounds
                _fullWindowRect.width = Mathf.Min(Screen.width - 40, ValheimGuiTheme.ScaledF(980f));
                _fullWindowRect.height = Mathf.Min(Screen.height - 40, ValheimGuiTheme.ScaledF(680f));
                _fullWindowRect.x = Mathf.Clamp(_fullWindowRect.x, 20, Screen.width - _fullWindowRect.width - 20);
                _fullWindowRect.y = Mathf.Clamp(_fullWindowRect.y, 20, Screen.height - _fullWindowRect.height - 20);

                _fullWindowRect = GUI.Window(
                    942851,
                    _fullWindowRect,
                    DrawFullCharacterWindow,
                    "",
                    ValheimGuiTheme.WindowStyle
                );
            }
        }

        private WorkerBrain GetActiveContainerWorker()
        {
            if (InventoryGui.instance == null || !InventoryGui.IsVisible()) return null;
            var curContainer = HarmonyLib.Traverse.Create(InventoryGui.instance).Field<Container>("m_currentContainer").Value;
            if (curContainer != null)
            {
                return curContainer.GetComponent<WorkerBrain>();
            }
            return null;
        }

        // =========================================================================
        // MODE A: In-World Container Overlay & Side-Drawer (Hold [E])
        // =========================================================================
        private void DrawContainerSideDrawer(WorkerBrain worker)
        {
            float drawerW = ValheimGuiTheme.ScaledF(360f);
            float drawerH = ValheimGuiTheme.ScaledF(460f);
            float drawerX = Screen.width - drawerW - ValheimGuiTheme.ScaledF(24f);
            float drawerY = Mathf.Max(ValheimGuiTheme.ScaledF(80f), (Screen.height - drawerH) * 0.45f);

            VerificationTelemetry.RecordPass("charmenu_side_drawer", $"Rendered NPC container side-drawer for worker '{worker.GetHoverName()}'.");

            _drawerRect = new Rect(drawerX, drawerY, drawerW, drawerH);

            GUILayout.BeginArea(_drawerRect, ValheimGuiTheme.WindowStyle);

            // Header Row: Name & Top-Right Action Buttons
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            GUILayout.Label($"<color=#FFD700><b>{worker.GetHoverName()}</b></color>", ValheimGuiTheme.CardHeaderStyle);
            GUILayout.Label($"{worker.Heritage} • Tier {worker.WorkerTier} • {worker.CurrentRole}", ValheimGuiTheme.LabelMuted);
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            // Top-right action icons matching player InventoryGui
            if (GUILayout.Button(new GUIContent("⚔️", "Worker Activity Skills (0-100)"), ValheimGuiTheme.ButtonNormal, GUILayout.Width(34), GUILayout.Height(34)))
            {
                Open(worker, 1);
            }
            if (GUILayout.Button(new GUIContent("🌳", "Heritage Skill Tree & Evolution"), ValheimGuiTheme.ButtonNormal, GUILayout.Width(34), GUILayout.Height(34)))
            {
                Open(worker, 2);
            }
            if (GUILayout.Button(new GUIContent("🛡️", "Full Character Sheet & Paperdoll"), ValheimGuiTheme.ButtonSelected, GUILayout.Width(34), GUILayout.Height(34)))
            {
                Open(worker, 0);
            }
            GUILayout.EndHorizontal();

            ValheimGuiTheme.DrawDivider();

            // Level & Overall XP Bar
            int nextXp = WorkerSkillTree.GetXpForNextLevel(worker.Level);
            float xpFrac = Mathf.Clamp01((float)worker.CurrentXp / Mathf.Max(1, nextXp));
            GUILayout.BeginHorizontal();
            GUILayout.Label($"<b>Level:</b> {worker.Level}", ValheimGuiTheme.LabelGold);
            GUILayout.FlexibleSpace();
            GUILayout.Label($"{worker.CurrentXp} / {nextXp} XP", ValheimGuiTheme.LabelParchment);
            GUILayout.EndHorizontal();
            ValheimGuiTheme.DrawProgressBar(xpFrac, ValheimGuiTheme.ScaledF(330f), ValheimGuiTheme.ScaledF(12f));

            GUILayout.Space(6);

            // Vitality (HP & Stamina / Blood)
            if (worker.m_character != null)
            {
                float hp = worker.m_character.GetHealth();
                float maxHp = worker.m_character.GetMaxHealth();
                GUILayout.BeginHorizontal();
                GUILayout.Label($"❤️ HP: {hp:F0}/{maxHp:F0}", ValheimGuiTheme.LabelParchment);
                GUILayout.FlexibleSpace();
                ValheimGuiTheme.DrawHealthBar(hp, maxHp, ValheimGuiTheme.ScaledF(170f), ValheimGuiTheme.ScaledF(12f));
                GUILayout.EndHorizontal();
            }

            if (worker.Heritage == WorkerHeritage.Skeleton)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"🩸 Marrow Blood: {worker.MarrowBlood:F0}/100", ValheimGuiTheme.LabelGold);
                GUILayout.FlexibleSpace();
                ValheimGuiTheme.DrawProgressBar(worker.MarrowBlood / 100f, ValheimGuiTheme.ScaledF(170f), ValheimGuiTheme.ScaledF(12f));
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(4);

            // Quick Stats Badges
            GUILayout.BeginHorizontal();
            GUILayout.Label($"🛡️ Armor: <b>{worker.GetTotalArmor():F0}</b>", ValheimGuiTheme.LabelParchment);
            GUILayout.Label($"⚖️ {worker.GetTotalWeight():F0}/{worker.GetMaxWeight():F0}kg", ValheimGuiTheme.LabelParchment);
            GUILayout.Label($"⚔️ {worker.GetWeaponDamage(out string dmgType):F0} {dmgType}", ValheimGuiTheme.LabelGold);
            GUILayout.EndHorizontal();

            ValheimGuiTheme.DrawDivider();

            // 3 Sustenance / Food Slots
            GUILayout.Label($"<b>Active Sustenance ({worker.ActiveFoods.Count}/3 Slots):</b>", ValheimGuiTheme.SubHeaderStyle);
            if (worker.ActiveFoods.Count == 0)
            {
                GUILayout.Label(worker.Heritage == WorkerHeritage.Skeleton ? "Undead (Immune to hunger & cold)" : "No food buffs active (auto-eats from backpack)", ValheimGuiTheme.LabelMuted);
            }
            else
            {
                for (int i = 0; i < worker.ActiveFoods.Count; i++)
                {
                    var f = worker.ActiveFoods[i];
                    int mins = Mathf.FloorToInt(f.RemainingTime / 60f);
                    int secs = Mathf.FloorToInt(f.RemainingTime % 60f);
                    GUILayout.BeginHorizontal(ValheimGuiTheme.SlotBoxStyle);
                    GUILayout.Label($"🍲 <b>{f.DisplayName}</b>", ValheimGuiTheme.LabelParchment);
                    GUILayout.FlexibleSpace();
                    GUILayout.Label($"<color=#FFD700>+{f.HealthBonus:F0} HP</color> | ⏱️ {mins}:{secs:D2}", ValheimGuiTheme.LabelParchment);
                    GUILayout.EndHorizontal();
                }
            }

            // Pride meter (Humans / Dvergr)
            if (worker.Heritage != WorkerHeritage.Skeleton)
            {
                GUILayout.Space(4);
                GUILayout.BeginHorizontal();
                GUILayout.Label($"👑 Dignity & Pride: {worker.PrideMeter * 100f:F0}%", ValheimGuiTheme.LabelParchment);
                GUILayout.FlexibleSpace();
                ValheimGuiTheme.DrawProgressBar(worker.PrideMeter, ValheimGuiTheme.ScaledF(140f), ValheimGuiTheme.ScaledF(10f));
                GUILayout.EndHorizontal();
            }

            GUILayout.FlexibleSpace();

            // Action prompt
            if (GUILayout.Button("Open Full Character Menu (F8)", ValheimGuiTheme.ButtonPrimary, GUILayout.Height(ValheimGuiTheme.ScaledF(28f))))
            {
                Open(worker, 0);
            }

            GUILayout.EndArea();
        }

        // =========================================================================
        // MODE B: Full NPC Character Menu Window
        // =========================================================================
        private void DrawFullCharacterWindow(int windowId)
        {
            if (CurrentWorker == null)
            {
                Close();
                return;
            }

            // Top Banner: Header, Avatar, Level, XP, SP, Top-Right Icons
            DrawHeaderBanner(CurrentWorker);

            // Tab Navigation Bar
            GUILayout.BeginHorizontal();
            if (DrawNavTab("🛡️ Character Sheet & Paperdoll", ActiveTab == 0)) ActiveTab = 0;
            if (DrawNavTab("⚔️ Valheim Skills (0–100)", ActiveTab == 1)) ActiveTab = 1;
            if (DrawNavTab("🌳 Skill Tree & Evolution", ActiveTab == 2)) ActiveTab = 2;
            GUILayout.EndHorizontal();

            ValheimGuiTheme.DrawDivider();

            // Tab Content
            switch (ActiveTab)
            {
                case 0:
                    DrawCharacterSheetTab(CurrentWorker);
                    break;
                case 1:
                    DrawSkillsDialogTab(CurrentWorker);
                    break;
                case 2:
                    DrawSkillTreeTab(CurrentWorker);
                    break;
            }

            GUI.DragWindow(new Rect(0, 0, _fullWindowRect.width, 36));
        }

        private void DrawHeaderBanner(WorkerBrain worker)
        {
            GUILayout.BeginHorizontal();

            // Avatar Icon Box
            string avatar = worker.Heritage switch
            {
                WorkerHeritage.Skeleton => "💀",
                WorkerHeritage.Dvergr => "⛏️",
                _ => "🧔"
            };
            GUILayout.Box(avatar, ValheimGuiTheme.SlotBoxEquippedStyle, GUILayout.Width(ValheimGuiTheme.ScaledF(48f)), GUILayout.Height(ValheimGuiTheme.ScaledF(48f)));

            GUILayout.Space(8);

            // Title & Badges
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Label($"<b>{worker.GetHoverName()}</b>", ValheimGuiTheme.CardHeaderStyle);
            GUILayout.Space(10);
            GUILayout.Label($"[Tier {worker.WorkerTier} {worker.Heritage}]", ValheimGuiTheme.LabelGold);
            GUILayout.EndHorizontal();
            GUILayout.Label($"Assigned Role: <b>{worker.CurrentRole}</b> | Status: {worker.GetCurrentStatusDescription()}", ValheimGuiTheme.LabelMuted);
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            // Level & XP Bar
            int nextXp = WorkerSkillTree.GetXpForNextLevel(worker.Level);
            float xpFrac = Mathf.Clamp01((float)worker.CurrentXp / Mathf.Max(1, nextXp));
            GUILayout.BeginVertical(GUILayout.Width(ValheimGuiTheme.ScaledF(220f)));
            GUILayout.BeginHorizontal();
            GUILayout.Label($"<b>Level {worker.Level}</b>", ValheimGuiTheme.LabelGold);
            GUILayout.FlexibleSpace();
            GUILayout.Label($"{worker.CurrentXp} / {nextXp} XP", ValheimGuiTheme.LabelParchment);
            GUILayout.EndHorizontal();
            ValheimGuiTheme.DrawProgressBar(xpFrac, ValheimGuiTheme.ScaledF(220f), ValheimGuiTheme.ScaledF(12f));

            string spText = worker.SkillPoints > 0 ? $"<color=#FFD700>★ <b>{worker.SkillPoints} SP Available</b></color>" : "<color=#B8B0A4>0 SP Available</color>";
            GUILayout.Label(spText, ValheimGuiTheme.LabelParchment);
            GUILayout.EndVertical();

            GUILayout.Space(12);

            // Close button
            if (GUILayout.Button("✕", ValheimGuiTheme.ButtonDanger, GUILayout.Width(ValheimGuiTheme.ScaledF(28f)), GUILayout.Height(ValheimGuiTheme.ScaledF(28f))))
            {
                Close();
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(4);
        }

        private bool DrawNavTab(string title, bool active)
        {
            var style = active ? ValheimGuiTheme.TabActiveStyle : ValheimGuiTheme.TabInactiveStyle;
            return GUILayout.Button(title, style, GUILayout.Height(ValheimGuiTheme.ScaledF(32f)));
        }

        // =========================================================================
        // TAB 0: Character Sheet & Equipped Paperdoll
        // =========================================================================
        private void DrawCharacterSheetTab(WorkerBrain worker)
        {
            VerificationTelemetry.RecordPass("charmenu_tab1_paperdoll", $"Rendered Paperdoll & Attributes sheet for '{worker.GetHoverName()}'.");
            GUILayout.BeginHorizontal();

            // Column 1: Equipped Paperdoll Slots (Helmet, Chest, Legs, Weapon, Shield, Bow, Ammo, Cape)
            GUILayout.BeginVertical(ValheimGuiTheme.CardStyle, GUILayout.Width(ValheimGuiTheme.ScaledF(280f)));
            GUILayout.Label("EQUIPPED LOADOUT", ValheimGuiTheme.SubHeaderStyle);
            ValheimGuiTheme.DrawDivider();

            DrawPaperdollSlot("🪖 Helmet", worker.Customization?.Helmet, "Iron Helmet");
            GUILayout.Space(2);
            GUILayout.BeginHorizontal();
            DrawPaperdollSlot("🪓 Main Hand", worker.Customization?.Weapon, $"{worker.GetWeaponDamage(out string wType):F0} {wType}");
            DrawPaperdollSlot("🧥 Chest", worker.Customization?.Chest, "Chest Armor");
            DrawPaperdollSlot("🛡️ Off-Hand", worker.Customization?.Shield, $"{worker.GetBlockPower(out float pf):F0} Block");
            GUILayout.EndHorizontal();
            GUILayout.Space(2);
            DrawPaperdollSlot("👖 Legs", worker.Customization?.Legs, "Leg Armor");
            GUILayout.Space(2);
            GUILayout.BeginHorizontal();
            DrawPaperdollSlot("🧣 Cape", worker.GetEquippedShoulderItem()?.m_shared?.m_name ?? "Wolf Cape", "Warmth & Armor");
            DrawPaperdollSlot("🏹 Bow", worker.IsHoldingBow() ? worker.Customization?.Weapon : "Huntsman", $"{worker.GetBowDamage(out float ds):F0} Pierce");
            DrawPaperdollSlot("🎯 Ammo", $"Arrows ({worker.GetTotalArrowCount()})", "Ironhead");
            GUILayout.EndHorizontal();

            GUILayout.Space(8);
            GUILayout.Label("<i>Right-click items in container grid below to equip or unequip instantly.</i>", ValheimGuiTheme.LabelMuted);
            GUILayout.EndVertical();

            GUILayout.Space(10);

            // Column 2: Vitality, Sustenance & Mood
            GUILayout.BeginVertical(ValheimGuiTheme.CardStyle, GUILayout.Width(ValheimGuiTheme.ScaledF(340f)));
            GUILayout.Label("VITALITY & SUSTENANCE", ValheimGuiTheme.SubHeaderStyle);
            ValheimGuiTheme.DrawDivider();

            // Health
            if (worker.m_character != null)
            {
                float hp = worker.m_character.GetHealth();
                float maxHp = worker.m_character.GetMaxHealth();
                GUILayout.BeginHorizontal();
                GUILayout.Label($"❤️ Health: <color=#FFA726>{hp:F0}/{maxHp:F0}</color>", ValheimGuiTheme.LabelParchment);
                GUILayout.FlexibleSpace();
                ValheimGuiTheme.DrawHealthBar(hp, maxHp, ValheimGuiTheme.ScaledF(180f), ValheimGuiTheme.ScaledF(14f));
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(4);

            // Stamina / Marrow Blood / Eitr
            if (worker.Heritage == WorkerHeritage.Skeleton)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"🩸 Marrow Blood: <color=#E53935>{worker.MarrowBlood:F0}/100</color>", ValheimGuiTheme.LabelParchment);
                GUILayout.FlexibleSpace();
                ValheimGuiTheme.DrawProgressBar(worker.MarrowBlood / 100f, ValheimGuiTheme.ScaledF(180f), ValheimGuiTheme.ScaledF(14f));
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("⚡ Stamina: <color=#FFD54F>120/120</color>", ValheimGuiTheme.LabelParchment);
                GUILayout.FlexibleSpace();
                ValheimGuiTheme.DrawProgressBar(1.0f, ValheimGuiTheme.ScaledF(180f), ValheimGuiTheme.ScaledF(14f));
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(10);

            // 3 Sustenance Slots
            GUILayout.Label("<b>3-Slot Food Rations:</b>", ValheimGuiTheme.LabelGold);
            if (worker.ActiveFoods.Count == 0)
            {
                GUILayout.Label(worker.Heritage == WorkerHeritage.Skeleton ? "Undead: Immune to starvation." : "Hungry: No food buffs active.", ValheimGuiTheme.LabelMuted);
            }
            else
            {
                for (int i = 0; i < worker.ActiveFoods.Count; i++)
                {
                    var f = worker.ActiveFoods[i];
                    int m = Mathf.FloorToInt(f.RemainingTime / 60f);
                    int s = Mathf.FloorToInt(f.RemainingTime % 60f);
                    GUILayout.BeginHorizontal(ValheimGuiTheme.SlotBoxStyle);
                    GUILayout.Label($"🍲 <b>{f.DisplayName}</b>", ValheimGuiTheme.LabelParchment);
                    GUILayout.FlexibleSpace();
                    GUILayout.Label($"<color=#FFD700>+{f.HealthBonus:F0} HP</color> | ⏱️ {m}:{s:D2}", ValheimGuiTheme.LabelParchment);
                    GUILayout.EndHorizontal();
                }
            }

            // Pride Section
            if (worker.Heritage != WorkerHeritage.Skeleton)
            {
                GUILayout.Space(8);
                ValheimGuiTheme.DrawDivider();
                GUILayout.BeginHorizontal();
                GUILayout.Label($"👑 Pride: {worker.PrideMeter * 100f:F0}%", ValheimGuiTheme.LabelParchment);
                GUILayout.FlexibleSpace();
                ValheimGuiTheme.DrawProgressBar(worker.PrideMeter, ValheimGuiTheme.ScaledF(180f), ValheimGuiTheme.ScaledF(12f));
                GUILayout.EndHorizontal();

                string moodDesc = worker.PrideMeter >= 0.8f ? "<color=#88FF88>Proud & Dedicated (+15% work speed)</color>"
                    : (worker.PrideMeter >= 0.5f ? "<color=#FFFF88>Content</color>"
                    : (worker.PrideMeter >= 0.1f ? "<color=#FFAA44>Disgruntled (-30% speed)</color>"
                    : "<color=#FF4444>On Strike (Grumbling)</color>"));
                GUILayout.Label(moodDesc, ValheimGuiTheme.LabelParchment);

                if (GUILayout.Button("Appease Pride (Offer Food/Mead)", ValheimGuiTheme.ButtonNormal, GUILayout.Height(ValheimGuiTheme.ScaledF(24f))))
                {
                    AppeaseWorker(worker);
                }
            }

            GUILayout.EndVertical();

            GUILayout.Space(10);

            // Column 3: Combat & Work Attributes
            GUILayout.BeginVertical(ValheimGuiTheme.CardStyle);
            GUILayout.Label("COMBAT & WORK ATTRIBUTES", ValheimGuiTheme.SubHeaderStyle);
            ValheimGuiTheme.DrawDivider();

            DrawStatRow("🛡️ Total Armor:", $"{worker.GetTotalArmor():F0}");
            DrawStatRow("⚖️ Carried Weight:", $"{worker.GetTotalWeight():F0} / {worker.GetMaxWeight():F0} kg");
            DrawStatRow("⚔️ Weapon Damage:", $"{worker.GetWeaponDamage(out string dType):F0} {dType}");
            DrawStatRow("🪓 Tool Tier:", $"Tier {worker.GetToolTier()}");
            DrawStatRow("🏹 Bow Pierce:", $"{worker.GetBowDamage(out float drawSpd):F0} Pierce ({drawSpd:F1}s)");
            DrawStatRow("🛡️ Block / Parry:", $"{worker.GetBlockPower(out float pFactor):F0} Block (x{pFactor:F1})");
            DrawStatRow("🥾 Movement Speed:", $"{worker.GetMoveSpeedMultiplier() * 100f:F1}%");
            DrawStatRow("🔨 Work Efficiency:", $"{worker.GetWorkEfficiencyMultiplier() * 100f:F1}%");
            DrawStatRow("🔥 Cold Resist:", worker.Heritage == WorkerHeritage.Skeleton ? "Undead Immune" : "Protected");

            GUILayout.Space(10);
            GUILayout.Label("<b>Tactical Profile:</b>", ValheimGuiTheme.LabelGold);
            GUILayout.Label(GetTacticalDescription(worker), ValheimGuiTheme.LabelMuted);

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            // Bottom: Worker Backpack Grid Preview
            GUILayout.Space(8);
            DrawBackpackGridPreview(worker);
        }

        private void DrawPaperdollSlot(string slotName, string itemName, string fallback)
        {
            bool hasItem = !string.IsNullOrEmpty(itemName);
            string label = hasItem ? itemName : fallback;
            var style = hasItem ? ValheimGuiTheme.SlotBoxEquippedStyle : ValheimGuiTheme.SlotBoxStyle;

            GUILayout.BeginVertical(style, GUILayout.Height(ValheimGuiTheme.ScaledF(42f)));
            GUILayout.Label(slotName, ValheimGuiTheme.LabelMuted);
            GUILayout.Label($"<b>{label}</b>", hasItem ? ValheimGuiTheme.LabelGold : ValheimGuiTheme.LabelParchment);
            GUILayout.EndVertical();
        }

        private void DrawStatRow(string label, string val)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, ValheimGuiTheme.LabelParchment);
            GUILayout.FlexibleSpace();
            GUILayout.Label($"<b>{val}</b>", ValheimGuiTheme.LabelGold);
            GUILayout.EndHorizontal();
        }

        private string GetTacticalDescription(WorkerBrain worker)
        {
            if (worker.Heritage == WorkerHeritage.Skeleton)
            {
                return "Immune to exhaustion, blizzards, and frost. Heals on hit in melee combat.";
            }
            if (worker.Heritage == WorkerHeritage.Dvergr)
            {
                return "Mistlands artisan. Deepdelving mining specialist with high blunt demolisher damage.";
            }
            return "Viking laborer. Thrives when well-fed. Automatically equips optimal tools and defends camp against raids.";
        }

        private void AppeaseWorker(WorkerBrain worker)
        {
            if (Player.m_localPlayer == null) return;
            var pInv = Player.m_localPlayer.GetInventory();
            if (worker.Heritage == WorkerHeritage.Human)
            {
                var food = pInv.GetAllItems().Find(i => i.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Consumable);
                if (food != null)
                {
                    pInv.RemoveItem(food, 1);
                    worker.PrideMeter = 1.0f;
                    worker.Say("A feast worthy of a warrior! My strength is renewed!");
                    worker.SaveProgressionToZDO();
                }
                else
                {
                    Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Need consumable food or mead in inventory to appease!");
                }
            }
            else if (worker.Heritage == WorkerHeritage.Dvergr)
            {
                if (pInv.CountItems("Coins") >= 10)
                {
                    pInv.RemoveItem("Coins", 10);
                    worker.PrideMeter = 1.0f;
                    worker.Say("Gold makes everything right. Back to work!");
                    worker.SaveProgressionToZDO();
                }
                else
                {
                    Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Need 10x Coins in inventory to appease Dvergr pride!");
                }
            }
        }

        private void DrawBackpackGridPreview(WorkerBrain worker)
        {
            var inv = worker.m_container?.GetInventory();
            if (inv == null) return;

            GUILayout.BeginVertical(ValheimGuiTheme.CardStyle);
            GUILayout.BeginHorizontal();
            GUILayout.Label("<b>WORKER BACKPACK (8x4 CONTAINER)</b>", ValheimGuiTheme.SubHeaderStyle);
            GUILayout.FlexibleSpace();
            GUILayout.Label($"Cyan = Equipped | Slots: {inv.NrOfItems()}/32", ValheimGuiTheme.LabelMuted);
            GUILayout.EndHorizontal();

            ValheimGuiTheme.DrawDivider();

            _backpackScroll = GUILayout.BeginScrollView(_backpackScroll, GUILayout.Height(ValheimGuiTheme.ScaledF(110f)));

            var allItems = inv.GetAllItems();
            int cols = 8;
            int rows = 4;

            for (int r = 0; r < rows; r++)
            {
                GUILayout.BeginHorizontal();
                for (int c = 0; c < cols; c++)
                {
                    var item = inv.GetItemAt(c, r);
                    if (item != null)
                    {
                        var style = item.m_equipped ? ValheimGuiTheme.SlotBoxEquippedStyle : ValheimGuiTheme.SlotBoxStyle;
                        string stackStr = item.m_stack > 1 ? $" x{item.m_stack}" : "";
                        if (GUILayout.Button($"{item.m_shared.m_name}{stackStr}", style, GUILayout.Width(ValheimGuiTheme.ScaledF(108f)), GUILayout.Height(ValheimGuiTheme.ScaledF(24f))))
                        {
                            worker.OnNpcGridRightClick(item);
                        }
                    }
                    else
                    {
                        GUILayout.Box("", ValheimGuiTheme.SlotBoxStyle, GUILayout.Width(ValheimGuiTheme.ScaledF(108f)), GUILayout.Height(ValheimGuiTheme.ScaledF(24f)));
                    }
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        // =========================================================================
        // TAB 1: Valheim SkillsDialog View (0-100)
        // =========================================================================
        private void DrawSkillsDialogTab(WorkerBrain worker)
        {
            VerificationTelemetry.RecordPass("charmenu_tab2_skills_dialog", $"Rendered Valheim 0-100 Skills dialog for '{worker.GetHoverName()}'.");
            GUILayout.BeginVertical(ValheimGuiTheme.CardStyle);
            GUILayout.BeginHorizontal();
            GUILayout.Label("VALHEIM ACTIVITY SKILLS (0–100)", ValheimGuiTheme.SubHeaderStyle);
            GUILayout.FlexibleSpace();
            GUILayout.Label($"Overall Level: <b>{worker.Level}</b> ({worker.CurrentXp} XP)", ValheimGuiTheme.LabelGold);
            GUILayout.EndHorizontal();
            GUILayout.Label("Skills advance organically as the worker performs actions. Skill level ups feed overall character XP.", ValheimGuiTheme.LabelMuted);

            ValheimGuiTheme.DrawDivider();

            _skillsScroll = GUILayout.BeginScrollView(_skillsScroll, GUILayout.Height(ValheimGuiTheme.ScaledF(450f)));

            for (int i = 0; i < WorkerSkillTree.ActivitySkills.Length; i++)
            {
                var skillDef = WorkerSkillTree.ActivitySkills[i];
                int lvl = worker.GetActivitySkillLevel(skillDef.Skill);
                float curXp = worker.GetActivitySkillXp(skillDef.Skill);
                float reqXp = WorkerSkillTree.GetActivitySkillNextLevelXp(lvl);
                float frac = Mathf.Clamp01(curXp / Mathf.Max(1f, reqXp));

                GUILayout.BeginHorizontal(ValheimGuiTheme.CardStyle);

                // Icon Box
                GUILayout.Box(skillDef.Icon, ValheimGuiTheme.SlotBoxStyle, GUILayout.Width(ValheimGuiTheme.ScaledF(38f)), GUILayout.Height(ValheimGuiTheme.ScaledF(38f)));

                GUILayout.Space(8);

                // Skill Name & Level
                GUILayout.BeginVertical(GUILayout.Width(ValheimGuiTheme.ScaledF(160f)));
                GUILayout.Label($"<b>{skillDef.DisplayName}</b>", ValheimGuiTheme.LabelGold);
                GUILayout.Label($"Level <b>{lvl}</b> / 100", ValheimGuiTheme.LabelParchment);
                GUILayout.EndVertical();

                // Progress Bars
                GUILayout.BeginVertical(GUILayout.Width(ValheimGuiTheme.ScaledF(320f)));
                GUILayout.BeginHorizontal();
                GUILayout.Label($"XP: {curXp:F0} / {reqXp:F0} ({frac * 100f:F0}%)", ValheimGuiTheme.LabelMuted);
                GUILayout.FlexibleSpace();
                GUILayout.Label($"-> Lv. {lvl + 1}", ValheimGuiTheme.LabelMuted);
                GUILayout.EndHorizontal();
                ValheimGuiTheme.DrawProgressBar(frac, ValheimGuiTheme.ScaledF(320f), ValheimGuiTheme.ScaledF(12f));
                GUILayout.EndVertical();

                GUILayout.Space(12);

                // Active Passive Bonus
                GUILayout.BeginVertical();
                GUILayout.Label("<b>Passive Bonus:</b>", ValheimGuiTheme.LabelGold);
                string bonusDesc = string.Format(skillDef.BonusFormat, lvl * 0.5f, lvl * 0.25f);
                GUILayout.Label(bonusDesc, ValheimGuiTheme.LabelParchment);
                GUILayout.EndVertical();

                GUILayout.EndHorizontal();
                GUILayout.Space(4);
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        // =========================================================================
        // TAB 2: Heritage Skill Tree & Evolution Node Graph
        // =========================================================================
        private void DrawSkillTreeTab(WorkerBrain worker)
        {
            VerificationTelemetry.RecordPass("charmenu_tab3_skill_tree", $"Rendered Heritage Skill Tree tab for '{worker.GetHoverName()}'.");
            GUILayout.BeginHorizontal();

            // Left: Node Graph Canvas Panel
            GUILayout.BeginVertical(ValheimGuiTheme.CardStyle, GUILayout.Width(ValheimGuiTheme.ScaledF(620f)));
            GUILayout.BeginHorizontal();
            GUILayout.Label($"HERITAGE TREE — {worker.Heritage.ToString().ToUpperInvariant()}", ValheimGuiTheme.SubHeaderStyle);
            GUILayout.FlexibleSpace();
            GUILayout.Label($"Available: <color=#FFD700><b>★ {worker.SkillPoints} SP</b></color>", ValheimGuiTheme.LabelGold);
            GUILayout.EndHorizontal();

            ValheimGuiTheme.DrawDivider();

            _treeScroll = GUILayout.BeginScrollView(_treeScroll, GUILayout.Height(ValheimGuiTheme.ScaledF(450f)));

            if (worker.Heritage == WorkerHeritage.Human)
            {
                DrawHumanTreeCanvas(worker);
            }
            else if (worker.Heritage == WorkerHeritage.Skeleton)
            {
                DrawSkeletonTreeCanvas(worker);
            }
            else if (worker.Heritage == WorkerHeritage.Dvergr)
            {
                DrawDvergrTreeCanvas(worker);
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUILayout.Space(10);

            // Right: Selected Node Inspector Card
            GUILayout.BeginVertical(ValheimGuiTheme.CardStyle);
            DrawNodeInspectorCard(worker);
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }

        private void DrawHumanTreeCanvas(WorkerBrain worker)
        {
            string[] branches = new string[] { "Woodsman", "Hearth", "Ranger", "ShieldWall" };
            string[] branchHeaders = new string[] { "🌲 Woodsman", "🌾 Hearth & Soil", "🏹 Ranger", "⚔️ Shield-Wall" };

            GUILayout.BeginHorizontal();
            for (int b = 0; b < branches.Length; b++)
            {
                GUILayout.BeginVertical(GUILayout.Width(ValheimGuiTheme.ScaledF(145f)));
                GUILayout.Label(branchHeaders[b], ValheimGuiTheme.LabelGold);

                for (int tier = 1; tier <= 4; tier++)
                {
                    string skillId = $"human_{branches[b].ToLowerInvariant()}_{tier}";
                    if (WorkerSkillTree.HumanSkills.TryGetValue(skillId, out var node))
                    {
                        bool learned = worker.HasSkill(node.Id);
                        bool isSelected = _selectedHumanNode != null && _selectedHumanNode.Id == node.Id;
                        var style = learned ? ValheimGuiTheme.ButtonSelected : (isSelected ? ValheimGuiTheme.ButtonPrimary : ValheimGuiTheme.ButtonNormal);

                        string prefix = learned ? "✔ " : (worker.SkillPoints >= node.SkillPointCost ? "★ " : "🔒 ");
                        if (GUILayout.Button($"{prefix}{node.DisplayName}\n<size=10>(Tier {tier} • {node.SkillPointCost} SP)</size>", style, GUILayout.Height(ValheimGuiTheme.ScaledF(48f))))
                        {
                            _selectedHumanNode = node;
                        }

                        if (tier < 4)
                        {
                            GUILayout.Label(learned ? "  ▼  " : "  │  ", ValheimGuiTheme.LabelMuted);
                        }
                    }
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
        }

        private void DrawSkeletonTreeCanvas(WorkerBrain worker)
        {
            GUILayout.Label("<b>4-STAGE EVOLUTION SPINE:</b>", ValheimGuiTheme.LabelGold);
            GUILayout.BeginHorizontal();
            for (int f = 1; f <= 4; f++)
            {
                if (WorkerSkillTree.SkeletonForms.TryGetValue(f, out var formDef))
                {
                    bool isCurrent = worker.EvolutionForm == f;
                    bool unlocked = worker.EvolutionForm >= f;
                    var style = isCurrent ? ValheimGuiTheme.ButtonSelected : (unlocked ? ValheimGuiTheme.ButtonPrimary : ValheimGuiTheme.ButtonNormal);

                    if (GUILayout.Button($"<b>{formDef.FormName}</b>\n(Cost: {formDef.RequiredBlood} Blood)", style, GUILayout.Width(ValheimGuiTheme.ScaledF(140f)), GUILayout.Height(ValheimGuiTheme.ScaledF(46f))))
                    {
                        _selectedSkelForm = formDef;
                        _selectedSkelGraft = null;
                    }
                    if (f < 4) GUILayout.Label(" ➔ ", ValheimGuiTheme.LabelGold);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(12);
            ValheimGuiTheme.DrawDivider();

            GUILayout.Label("<b>6 ANATOMICAL BLOOD-GRAFTS:</b>", ValheimGuiTheme.LabelGold);
            GUILayout.BeginVertical();
            foreach (var kvp in WorkerSkillTree.SkeletonGrafts)
            {
                var graft = kvp.Value;
                bool learned = worker.HasSkill(graft.Id);
                var style = learned ? ValheimGuiTheme.ButtonSelected : ValheimGuiTheme.ButtonNormal;

                GUILayout.BeginHorizontal();
                if (GUILayout.Button($"🦴 <b>{graft.DisplayName}</b> (Blood: {graft.RequiredBlood}, Bones: {graft.RequiredBones})", style, GUILayout.Height(ValheimGuiTheme.ScaledF(28f))))
                {
                    _selectedSkelGraft = graft;
                    _selectedSkelForm = null;
                }
                GUILayout.Label(learned ? "<color=#88FF88>✔ GRAFTED</color>" : "<color=#B8B0A4>Available</color>", ValheimGuiTheme.LabelParchment);
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        private void DrawDvergrTreeCanvas(WorkerBrain worker)
        {
            GUILayout.Label("<b>DVERGR GUILD PATRONAGE & LUXURY MATRIX:</b>", ValheimGuiTheme.LabelGold);
            foreach (var kvp in WorkerSkillTree.DvergrPatronage)
            {
                var node = kvp.Value;
                bool learned = worker.HasSkill(node.Id);
                var style = learned ? ValheimGuiTheme.ButtonSelected : ValheimGuiTheme.ButtonNormal;

                GUILayout.BeginHorizontal(ValheimGuiTheme.SlotBoxStyle);
                if (GUILayout.Button($"💎 <b>{node.DisplayName}</b>", style, GUILayout.Width(ValheimGuiTheme.ScaledF(220f)), GUILayout.Height(ValheimGuiTheme.ScaledF(30f))))
                {
                    _selectedDvergrNode = node;
                }
                GUILayout.Label($"Coins: {node.RequiredCoins} | Mat: {node.RequiredMaterialAmount}x {node.RequiredMaterialPrefab}", ValheimGuiTheme.LabelMuted);
                GUILayout.FlexibleSpace();
                GUILayout.Label(learned ? "<color=#88FF88>✔ CERTIFIED</color>" : "<color=#FFD700>Honorable</color>", ValheimGuiTheme.LabelParchment);
                GUILayout.EndHorizontal();
                GUILayout.Space(2);
            }
        }

        private void DrawNodeInspectorCard(WorkerBrain worker)
        {
            GUILayout.Label("SKILL INSPECTOR", ValheimGuiTheme.SubHeaderStyle);
            ValheimGuiTheme.DrawDivider();

            if (worker.Heritage == WorkerHeritage.Human && _selectedHumanNode != null)
            {
                GUILayout.Label($"<b>{_selectedHumanNode.DisplayName}</b>", ValheimGuiTheme.CardHeaderStyle);
                GUILayout.Label($"Branch: {_selectedHumanNode.Branch} • Tier {_selectedHumanNode.Tier}", ValheimGuiTheme.LabelGold);
                GUILayout.Label(_selectedHumanNode.Description, ValheimGuiTheme.LabelParchment);
                GUILayout.Space(8);

                bool learned = worker.HasSkill(_selectedHumanNode.Id);
                bool parentLearned = string.IsNullOrEmpty(_selectedHumanNode.RequiredParentId) || worker.HasSkill(_selectedHumanNode.RequiredParentId);
                bool canAfford = worker.SkillPoints >= _selectedHumanNode.SkillPointCost;

                if (learned)
                {
                    GUILayout.Label("<color=#88FF88>✔ ALREADY LEARNED</color>", ValheimGuiTheme.LabelGold);
                }
                else if (parentLearned && canAfford)
                {
                    if (GUILayout.Button($"LEARN SKILL ({_selectedHumanNode.SkillPointCost} SP)", ValheimGuiTheme.ButtonPrimary, GUILayout.Height(ValheimGuiTheme.ScaledF(34f))))
                    {
                        worker.SkillPoints -= _selectedHumanNode.SkillPointCost;
                        worker.UnlockSkill(_selectedHumanNode.Id);
                    }
                }
                else
                {
                    GUILayout.Label(!parentLearned ? "<color=#FF6B6B>Requires previous tier skill!</color>" : "<color=#FF6B6B>Need more Skill Points!</color>", ValheimGuiTheme.LabelParchment);
                }
            }
            else if (worker.Heritage == WorkerHeritage.Skeleton)
            {
                if (_selectedSkelForm != null)
                {
                    GUILayout.Label($"<b>{_selectedSkelForm.FormName}</b>", ValheimGuiTheme.CardHeaderStyle);
                    GUILayout.Label(_selectedSkelForm.Title, ValheimGuiTheme.LabelParchment);
                    GUILayout.Space(6);
                    GUILayout.Label($"Required Blood: {_selectedSkelForm.RequiredBlood} | Bones: {_selectedSkelForm.RequiredBones}", ValheimGuiTheme.LabelGold);

                    if (worker.EvolutionForm >= _selectedSkelForm.FormIndex)
                    {
                        GUILayout.Label("<color=#88FF88>✔ CURRENT FORM OR SURPASSED</color>", ValheimGuiTheme.LabelGold);
                    }
                    else if (worker.MarrowBlood >= _selectedSkelForm.RequiredBlood)
                    {
                        if (GUILayout.Button("EVOLVE SKELETON FORM", ValheimGuiTheme.ButtonPrimary, GUILayout.Height(ValheimGuiTheme.ScaledF(34f))))
                        {
                            worker.MarrowBlood -= _selectedSkelForm.RequiredBlood;
                            worker.EvolveSkeletonForm(_selectedSkelForm.FormIndex);
                        }
                    }
                    else
                    {
                        GUILayout.Label("<color=#FF6B6B>Need more Marrow Blood! (Spill blood on skeleton)</color>", ValheimGuiTheme.LabelParchment);
                    }
                }
                else if (_selectedSkelGraft != null)
                {
                    GUILayout.Label($"<b>{_selectedSkelGraft.DisplayName}</b>", ValheimGuiTheme.CardHeaderStyle);
                    GUILayout.Label(_selectedSkelGraft.Description, ValheimGuiTheme.LabelParchment);
                    GUILayout.Space(6);
                    GUILayout.Label($"Required Blood: {_selectedSkelGraft.RequiredBlood} | Bones: {_selectedSkelGraft.RequiredBones}", ValheimGuiTheme.LabelGold);

                    if (worker.HasSkill(_selectedSkelGraft.Id))
                    {
                        GUILayout.Label("<color=#88FF88>✔ GRAFTED</color>", ValheimGuiTheme.LabelGold);
                    }
                    else if (worker.MarrowBlood >= _selectedSkelGraft.RequiredBlood)
                    {
                        if (GUILayout.Button("APPLY BLOOD GRAFT", ValheimGuiTheme.ButtonPrimary, GUILayout.Height(ValheimGuiTheme.ScaledF(34f))))
                        {
                            worker.MarrowBlood -= _selectedSkelGraft.RequiredBlood;
                            worker.UnlockSkill(_selectedSkelGraft.Id);
                        }
                    }
                    else
                    {
                        GUILayout.Label("<color=#FF6B6B>Need more Marrow Blood!</color>", ValheimGuiTheme.LabelParchment);
                    }
                }
                else
                {
                    GUILayout.Label("Select an Evolution Form or Anatomical Blood-Graft on the left to inspect.", ValheimGuiTheme.LabelMuted);
                }
            }
            else if (worker.Heritage == WorkerHeritage.Dvergr && _selectedDvergrNode != null)
            {
                GUILayout.Label($"<b>{_selectedDvergrNode.DisplayName}</b>", ValheimGuiTheme.CardHeaderStyle);
                GUILayout.Label(_selectedDvergrNode.Description, ValheimGuiTheme.LabelParchment);
                GUILayout.Space(6);
                GUILayout.Label($"Cost: {_selectedDvergrNode.RequiredCoins} Coins, {_selectedDvergrNode.RequiredMaterialAmount}x {_selectedDvergrNode.RequiredMaterialPrefab}", ValheimGuiTheme.LabelGold);

                if (worker.HasSkill(_selectedDvergrNode.Id))
                {
                    GUILayout.Label("<color=#88FF88>✔ GUILD CERTIFIED</color>", ValheimGuiTheme.LabelGold);
                }
                else
                {
                    if (GUILayout.Button("GRANT GUILD PATRONAGE", ValheimGuiTheme.ButtonPrimary, GUILayout.Height(ValheimGuiTheme.ScaledF(34f))))
                    {
                        worker.UnlockSkill(_selectedDvergrNode.Id);
                    }
                }
            }
            else
            {
                GUILayout.Label("Select a skill node from the tree on the left to inspect requirements and active bonuses.", ValheimGuiTheme.LabelMuted);
            }
        }
    }
}
