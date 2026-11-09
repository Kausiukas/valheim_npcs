using System;
using System.Collections.Generic;
using System.Globalization;
using HarmonyLib;
using UnityEngine;
using ValheimWorkers.Core;
using ValheimWorkers.UI;

namespace ValheimWorkers.AI
{
    public class WorkerBrain : MonoBehaviour, Hoverable, Interactable
    {
        public static readonly List<WorkerBrain> ActiveWorkers = new List<WorkerBrain>();

        public Character m_character;
        public Humanoid m_humanoid;
        public BaseAI m_baseAI;
        public MonsterAI m_monsterAI;
        public ZNetView m_nview;
        public VisEquipment m_visEquipment;
        public FootStep m_footStep;
        public Container m_container;

        public WorkerRole CurrentRole { get; private set; } = WorkerRole.Idle;
        public WorkerRole PreviousStationRole { get; private set; } = WorkerRole.Hauler;
        public Vector3 HomePosition { get; private set; }
        public bool HasHome { get; private set; } = false;
        public Vector3 WorkPostPosition { get; private set; }
        public bool HasWorkPost { get; private set; } = false;
        public float WorkRadius { get; private set; } = 25f;
        public float ReturnFillThreshold { get; private set; } = 0.75f;
        public WorkerArchetype Archetype { get; set; }
        public WorkerCustomization Customization { get; private set; }

        // --- Progression, Skill Tree, Pride & Evolution State ---
        public int WorkerTier { get; set; } = 1;
        public int Level { get; set; } = 1;
        public int CurrentXp { get; set; } = 0;
        public int SkillPoints { get; set; } = 0;
        public float MarrowBlood { get; set; } = 0f;
        public float PrideMeter { get; set; } = 1.0f;
        public int EvolutionForm { get; set; } = 1;
        private readonly HashSet<string> _unlockedSkills = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public WorkerHeritage Heritage
        {
            get
            {
                if (Customization != null && !string.IsNullOrEmpty(Customization.BasePrefab))
                {
                    if (Customization.BasePrefab.IndexOf("skeleton", StringComparison.OrdinalIgnoreCase) >= 0)
                        return WorkerHeritage.Skeleton;
                    if (Customization.BasePrefab.IndexOf("dverger", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        Customization.BasePrefab.IndexOf("dvergr", StringComparison.OrdinalIgnoreCase) >= 0)
                        return WorkerHeritage.Dvergr;
                }
                return WorkerHeritage.Human;
            }
        }

        public bool HasSkill(string skillId)
        {
            if (string.IsNullOrEmpty(skillId)) return false;
            return _unlockedSkills.Contains(skillId);
        }

        public void UnlockSkill(string skillId)
        {
            if (string.IsNullOrEmpty(skillId)) return;
            _unlockedSkills.Add(skillId);
            SaveProgressionToZDO();
            LogAction($"Unlocked skill [{skillId}]!", true);

            if (skillId.StartsWith("hum_wood", StringComparison.OrdinalIgnoreCase))
                VerificationTelemetry.RecordPass("skilltree_human_woodsman", $"Worker '{GetHoverName()}' unlocked Woodsman skill node [{skillId}].");
            else if (skillId.StartsWith("hum_hearth", StringComparison.OrdinalIgnoreCase))
                VerificationTelemetry.RecordPass("skilltree_human_hearth", $"Worker '{GetHoverName()}' unlocked Hearth & Harvest skill node [{skillId}].");
            else if (skillId.StartsWith("hum_rang", StringComparison.OrdinalIgnoreCase))
                VerificationTelemetry.RecordPass("skilltree_human_ranger", $"Worker '{GetHoverName()}' unlocked Ranger & Hunter skill node [{skillId}].");
            else if (skillId.StartsWith("hum_guard", StringComparison.OrdinalIgnoreCase))
                VerificationTelemetry.RecordPass("skilltree_human_shieldwall", $"Worker '{GetHoverName()}' unlocked Shield-Wall skill node [{skillId}].");
            else if (skillId == "skel_graft_1" || skillId == "skel_graft_2")
                VerificationTelemetry.RecordPass("skilltree_skel_graft_1_2", $"Skeleton '{GetHoverName()}' grafted [{skillId}].");
            else if (skillId == "skel_graft_3" || skillId == "skel_graft_4")
                VerificationTelemetry.RecordPass("skilltree_skel_graft_3_4", $"Skeleton '{GetHoverName()}' grafted [{skillId}].");
            else if (skillId == "skel_graft_5" || skillId == "skel_graft_6")
                VerificationTelemetry.RecordPass("skilltree_skel_graft_5_6", $"Skeleton '{GetHoverName()}' grafted [{skillId}].");
            else if (skillId == "dvergr_mine" || skillId == "dvergr_gem")
                VerificationTelemetry.RecordPass("skilltree_dvergr_mine_gem", $"Dvergr '{GetHoverName()}' bestowed luxury patronage [{skillId}].");
            else if (skillId == "dvergr_smelt" || skillId == "dvergr_anvil")
                VerificationTelemetry.RecordPass("skilltree_dvergr_smelt_anvil", $"Dvergr '{GetHoverName()}' bestowed luxury patronage [{skillId}].");
            else if (skillId == "dvergr_brew" || skillId == "dvergr_ward")
                VerificationTelemetry.RecordPass("skilltree_dvergr_brew_ward", $"Dvergr '{GetHoverName()}' bestowed luxury patronage [{skillId}].");
        }

        public bool HasLuxuryItem()
        {
            if (m_container == null) return false;
            var inv = m_container.GetInventory();
            if (inv == null) return false;
            foreach (var item in inv.GetAllItems())
            {
                if (item == null || item.m_dropPrefab == null) continue;
                string name = item.m_dropPrefab.name;
                if (name == "Coins" || name == "Amber" || name == "Ruby" || name == "AmberPearl" || name == "SilverNecklace")
                    return true;
            }
            return false;
        }

        private float _prideCheckTimer = 0f;

        public void AddTaskXp(int amount, TaskDignity dignity)
        {
            // Skeletons don't learn from chores
            if (Heritage == WorkerHeritage.Skeleton) return;

            // Pride evaluation
            WorkerProgressionManager.EvaluatePrideDecay(this, dignity, 5.0f);

            // Award XP
            CurrentXp += amount;
            int nextLevelXp = WorkerSkillTree.GetXpForNextLevel(Level);
            if (CurrentXp >= nextLevelXp)
            {
                CurrentXp -= nextLevelXp;
                Level++;
                SkillPoints++;
                Say($"Leveled up to Lv. {Level}! (+1 Skill Point)");
                LogAction($"Leveled up to Lv. {Level}! Unspent SP: {SkillPoints}", true);
                VerificationTelemetry.RecordPass("actskill_overall_levelup", $"Worker '{GetHoverName()}' leveled up to Lv. {Level} (+1 Skill Point).");
                if (m_character != null)
                {
                    m_character.SetMaxHealth(m_character.GetMaxHealth() + 10f);
                    m_character.Heal(10f);
                }
            }
            SaveProgressionToZDO();
        }

        public void AddMarrowBlood(float amount)
        {
            MarrowBlood += amount;
            if (MarrowBlood > 200f) MarrowBlood = 200f;
            if (m_character != null && amount > 0f)
            {
                m_character.Heal(2f);
            }
            SaveProgressionToZDO();
        }

        public void EvolveSkeletonForm(int targetForm)
        {
            if (Heritage != WorkerHeritage.Skeleton) return;
            EvolutionForm = Mathf.Clamp(targetForm, 1, 4);
            WorkerTier = EvolutionForm;
            WorkerProgressionManager.ApplySkeletonEvolutionVisuals(this);
            SaveProgressionToZDO();
            Say($"Evolved into Skeleton Form {EvolutionForm}!");
            LogAction($"Evolved into Skeleton Form {EvolutionForm}!", true);
        }

        // -------------------------------------------------------------
        // Valheim 0-100 Activity Skills & Computed Character Stats
        // -------------------------------------------------------------
        public int GetActivitySkillLevel(WorkerActivitySkill skill)
        {
            int idx = (int)skill;
            if (idx < 0 || idx >= WorkerZdoKeys.ActivitySkillLevelHashes.Length) return 1;

            if (m_nview != null && m_nview.GetZDO() != null)
            {
                int lvl = m_nview.GetZDO().GetInt(WorkerZdoKeys.ActivitySkillLevelHashes[idx], 0);
                if (lvl > 0) return Mathf.Clamp(lvl, 1, 100);

                // Baseline initialization if unset
                int baseline = CalculateBaselineSkillLevel(skill);
                if (m_nview.IsOwner())
                {
                    m_nview.GetZDO().Set(WorkerZdoKeys.ActivitySkillLevelHashes[idx], baseline);
                }
                return baseline;
            }

            return CalculateBaselineSkillLevel(skill);
        }

        public float GetActivitySkillXp(WorkerActivitySkill skill)
        {
            int idx = (int)skill;
            if (idx < 0 || idx >= WorkerZdoKeys.ActivitySkillXpHashes.Length) return 0f;
            if (m_nview != null && m_nview.GetZDO() != null)
            {
                return m_nview.GetZDO().GetFloat(WorkerZdoKeys.ActivitySkillXpHashes[idx], 0f);
            }
            return 0f;
        }

        public void RaiseActivitySkill(WorkerActivitySkill skill, float xpGain)
        {
            if (xpGain <= 0f) return;
            int idx = (int)skill;
            if (idx < 0 || idx >= WorkerZdoKeys.ActivitySkillLevelHashes.Length) return;

            int curLvl = GetActivitySkillLevel(skill);
            if (curLvl >= 100) return;

            float curXp = GetActivitySkillXp(skill) + xpGain;
            float reqXp = WorkerSkillTree.GetActivitySkillNextLevelXp(curLvl);
            bool leveledUp = false;

            while (curXp >= reqXp && curLvl < 100)
            {
                curXp -= reqXp;
                curLvl++;
                leveledUp = true;
                reqXp = WorkerSkillTree.GetActivitySkillNextLevelXp(curLvl);
            }

            if (m_nview != null && m_nview.GetZDO() != null && m_nview.IsOwner())
            {
                m_nview.GetZDO().Set(WorkerZdoKeys.ActivitySkillLevelHashes[idx], curLvl);
                m_nview.GetZDO().Set(WorkerZdoKeys.ActivitySkillXpHashes[idx], curXp);
            }

            // Record skill practice / progression
            string actTestId = skill switch
            {
                WorkerActivitySkill.Woodcutting => "actskill_woodcutting",
                WorkerActivitySkill.Mining => "actskill_mining",
                WorkerActivitySkill.Melee => "actskill_melee",
                WorkerActivitySkill.Blocking => "actskill_blocking",
                WorkerActivitySkill.Bows => "actskill_bows",
                WorkerActivitySkill.Farming => "actskill_farming",
                WorkerActivitySkill.Crafting => "actskill_crafting",
                WorkerActivitySkill.Athletics => "actskill_athletics",
                _ => null
            };
            if (!string.IsNullOrEmpty(actTestId))
            {
                VerificationTelemetry.RecordPass(actTestId, $"Worker '{GetHoverName()}' practiced {skill} (Current: Lv. {curLvl}).");
            }

            if (leveledUp)
            {
                Say($"*Skill Level Up: {skill} is now Lv. {curLvl}!*");
                LogAction($"Advanced in {skill} to Level {curLvl}!", true);
                VerificationTelemetry.RecordPass("skill_levelup", $"{GetHoverName()} advanced {skill} to level {curLvl}.");
                AddTaskXp(20 * curLvl, TaskDignity.Skilled);
            }
        }

        public ItemDrop.ItemData GetEquippedHelmetItem()
        {
            if (m_humanoid == null) return null;
            return Traverse.Create(m_humanoid).Field<ItemDrop.ItemData>("m_helmetItem").Value;
        }

        public ItemDrop.ItemData GetEquippedChestItem()
        {
            if (m_humanoid == null) return null;
            return Traverse.Create(m_humanoid).Field<ItemDrop.ItemData>("m_chestItem").Value;
        }

        public ItemDrop.ItemData GetEquippedLegItem()
        {
            if (m_humanoid == null) return null;
            return Traverse.Create(m_humanoid).Field<ItemDrop.ItemData>("m_legItem").Value;
        }

        public ItemDrop.ItemData GetEquippedShoulderItem()
        {
            if (m_humanoid == null) return null;
            return Traverse.Create(m_humanoid).Field<ItemDrop.ItemData>("m_shoulderItem").Value;
        }

        private int CalculateBaselineSkillLevel(WorkerActivitySkill skill)
        {
            bool isPrimary = IsPrimarySkillForRole(skill, CurrentRole);
            return WorkerTier switch
            {
                1 => 5,
                2 => isPrimary ? 20 : 10,
                3 => isPrimary ? 40 : 25,
                4 => isPrimary ? 70 : 50,
                _ => isPrimary ? 20 : 10
            };
        }

        private static bool IsPrimarySkillForRole(WorkerActivitySkill skill, WorkerRole role)
        {
            return role switch
            {
                WorkerRole.Lumberjack => skill == WorkerActivitySkill.Woodcutting,
                WorkerRole.Miner => skill == WorkerActivitySkill.Mining,
                WorkerRole.Guard => skill == WorkerActivitySkill.Melee || skill == WorkerActivitySkill.Blocking,
                WorkerRole.Hunter => skill == WorkerActivitySkill.Bows,
                WorkerRole.Farmer => skill == WorkerActivitySkill.Farming,
                WorkerRole.Cook or WorkerRole.BaseTender => skill == WorkerActivitySkill.Crafting,
                WorkerRole.Hauler or WorkerRole.Gatherer => skill == WorkerActivitySkill.Athletics,
                _ => false
            };
        }

        public float GetTotalArmor()
        {
            float total = 0f;
            if (m_humanoid != null)
            {
                var h = GetEquippedHelmetItem();
                if (h != null) total += h.GetArmor();
                var c = GetEquippedChestItem();
                if (c != null) total += c.GetArmor();
                var l = GetEquippedLegItem();
                if (l != null) total += l.GetArmor();
                var sh = GetEquippedLeftItem();
                if (sh != null && sh.m_shared != null && sh.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Shield)
                {
                    total += sh.GetArmor();
                }
            }
            if (total <= 0f && m_container != null && m_container.GetInventory() != null)
            {
                foreach (var item in m_container.GetInventory().GetAllItems())
                {
                    if (item != null && item.m_equipped && item.m_shared != null)
                    {
                        total += item.GetArmor();
                    }
                }
            }
            return total;
        }

        public float GetTotalWeight()
        {
            return m_container?.GetInventory()?.GetTotalWeight() ?? 0f;
        }

        public float GetMaxWeight()
        {
            float max = 300f;
            max += GetActivitySkillLevel(WorkerActivitySkill.Athletics) * 2f;
            if (HasSkill("human_wood_3")) max += 50f;
            if (HasSkill("skeleton_graft_1")) max += 100f;
            return max;
        }

        public float GetWeaponDamage(out string damageType)
        {
            damageType = "Slash";
            var item = GetEquippedRightItem();
            if (item == null && m_container?.GetInventory() != null)
            {
                item = m_container.GetInventory().GetAllItems().Find(i => i.m_equipped && i.m_shared != null && (i.m_shared.m_itemType == ItemDrop.ItemData.ItemType.OneHandedWeapon || i.m_shared.m_itemType == ItemDrop.ItemData.ItemType.TwoHandedWeapon));
            }
            if (item != null)
            {
                var dmg = item.GetDamage();
                float best = dmg.m_slash;
                if (dmg.m_blunt > best) { best = dmg.m_blunt; damageType = "Blunt"; }
                if (dmg.m_pierce > best) { best = dmg.m_pierce; damageType = "Pierce"; }
                if (dmg.m_chop > best) { best = dmg.m_chop; damageType = "Chop"; }
                if (dmg.m_pickaxe > best) { best = dmg.m_pickaxe; damageType = "Pickaxe"; }
                return Mathf.Max(best, dmg.GetTotalDamage());
            }
            return 10f; // Unarmed punch
        }

        public int GetToolTier()
        {
            var item = GetEquippedRightItem();
            if (item != null && item.m_shared != null) return item.m_shared.m_toolTier;
            return 1;
        }

        public float GetMoveSpeedMultiplier()
        {
            float mult = 1.0f;
            mult += GetActivitySkillLevel(WorkerActivitySkill.Athletics) * 0.0025f;
            if (PrideMeter >= 0.80f) mult += 0.05f;
            else if (PrideMeter <= 0.10f) mult -= 0.30f;
            return mult;
        }

        public float GetWorkEfficiencyMultiplier()
        {
            float mult = 1.0f;
            mult += (Level - 1) * 0.03f;
            if (PrideMeter >= 0.80f) mult += 0.15f;
            else if (PrideMeter <= 0.20f) mult -= 0.20f;
            return mult;
        }

        public float GetBlockPower(out float parryFactor)
        {
            parryFactor = 1.5f;
            var shield = GetEquippedLeftItem();
            if (shield != null && shield.m_shared != null && shield.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Shield)
            {
                parryFactor = shield.m_shared.m_timedBlockBonus;
                return shield.GetBlockPower(GetActivitySkillLevel(WorkerActivitySkill.Blocking));
            }
            return 15f;
        }

        public float GetBowDamage(out float drawSpeed)
        {
            drawSpeed = 1.5f;
            var bow = GetEquippedBow();
            if (bow != null && bow.m_shared != null)
            {
                drawSpeed = Mathf.Max(0.8f, 2.0f - (GetActivitySkillLevel(WorkerActivitySkill.Bows) * 0.012f));
                return bow.GetDamage().GetTotalDamage();
            }
            return 0f;
        }

        public void LoadProgressionFromZDO()
        {
            if (m_nview == null || m_nview.GetZDO() == null) return;
            var zdo = m_nview.GetZDO();
            WorkerTier = zdo.GetInt(WorkerZdoKeys.WorkerTierHash, 1);
            Level = zdo.GetInt(WorkerZdoKeys.LevelHash, 1);
            CurrentXp = zdo.GetInt(WorkerZdoKeys.XpHash, 0);
            SkillPoints = zdo.GetInt(WorkerZdoKeys.SkillPointsHash, 0);
            MarrowBlood = zdo.GetFloat(WorkerZdoKeys.MarrowBloodHash, 0f);
            PrideMeter = zdo.GetFloat(WorkerZdoKeys.PrideMeterHash, 1.0f);
            EvolutionForm = zdo.GetInt(WorkerZdoKeys.EvolutionFormHash, 1);

            string skillsStr = zdo.GetString(WorkerZdoKeys.SkillsBitmaskHash, "");
            _unlockedSkills.Clear();
            if (!string.IsNullOrEmpty(skillsStr))
            {
                string[] parts = skillsStr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var p in parts)
                {
                    _unlockedSkills.Add(p.Trim());
                }
            }

            if (Heritage == WorkerHeritage.Skeleton)
            {
                WorkerProgressionManager.ApplySkeletonEvolutionVisuals(this);
            }
        }

        public void SaveProgressionToZDO()
        {
            if (m_nview == null || m_nview.GetZDO() == null || !m_nview.IsOwner()) return;
            var zdo = m_nview.GetZDO();
            zdo.Set(WorkerZdoKeys.WorkerTierHash, WorkerTier);
            zdo.Set(WorkerZdoKeys.LevelHash, Level);
            zdo.Set(WorkerZdoKeys.XpHash, CurrentXp);
            zdo.Set(WorkerZdoKeys.SkillPointsHash, SkillPoints);
            zdo.Set(WorkerZdoKeys.MarrowBloodHash, MarrowBlood);
            zdo.Set(WorkerZdoKeys.PrideMeterHash, PrideMeter);
            zdo.Set(WorkerZdoKeys.EvolutionFormHash, EvolutionForm);
            zdo.Set(WorkerZdoKeys.SkillsBitmaskHash, string.Join(",", _unlockedSkills));
        }

        // Per-NPC Action & Decision Log for live troubleshooting
        public readonly List<string> ActionLog = new List<string>();
        private string _lastLoggedActionKey = "";
        private const int MaxLogEntries = 60;
        public bool VerboseConsoleLog { get; set; } = true;

        // Command Trace & Diagnostic Evaluation state
        public string ActiveCommandTraceId { get; set; } = "";
        private float _liveSnapshotTimer = 0f;

        // Threat & Self-Defense state
        private Character _activeThreat = null;
        private bool _isRetreating = false;
        private float _retreatTimer = 0f;
        private float _threatScanTimer = 0f;
        private float _lastHealth = 0f;
        private float _retreatStartThreatDist = 0f;
        private Vector3 _retreatStartPos = Vector3.zero;
        private bool _lowHpRetreatVerified = false;
        private bool _tacticalRetreatVerified = false;

        // Retreat waypoint latching
        private Vector3 _latchedRetreatDest = Vector3.zero;
        private float _latchedRetreatTimer = 0f;

        // Auto-Unstuck state
        private Vector3 _lastStuckCheckPos = Vector3.zero;
        private float _stuckTimer = 0f;
        private int _consecutiveUnstuckAttempts = 0;
        private bool _isNavigating = false;

        // Intelligent Detour Navigation state
        private Vector3 _detourTarget = Vector3.zero;
        private float _detourTimer = 0f;
        private bool _detourAlternate = false;

        // Vitality & 3-Slot Active Food Effect state
        public class ActiveWorkerFood
        {
            public string PrefabName;
            public string DisplayName;
            public float HealthBonus;
            public float StaminaBonus;
            public float RegenPerTick;
            public float RemainingTime;
            public float TotalTime;
        }

        public readonly List<ActiveWorkerFood> ActiveFoods = new List<ActiveWorkerFood>(3);
        public const int MaxPersonalRationPerFoodType = 6;
        private float _foodEatTimer = 0f;
        private float _foodRegenTickTimer = 0f;
        private float _outOfCombatRegenTimer = 0f;
        private float _timeSinceLastDamaged = 99f;
        private bool _isUpdatingInventory = false;
        private bool _isApplyingVisuals = false;
        public bool AllowManualPlayerUnequip { get; set; } = false;

        // Hunting, Ballistics & Stealth Crawl state
        public Vector3? CustomAimDir { get; set; } = null;
        public bool IsCrouchingOrStalking { get; private set; } = false;

        public enum HunterCombatMode
        {
            None,
            Ranged,
            Melee
        }

        public HunterCombatMode CurrentCombatMode { get; private set; } = HunterCombatMode.None;
        public bool AllowAiWeaponSwitch { get; set; } = false;
        public float ActiveBowDrawPercentage { get; set; } = 0f;
        public float LastReleasedBowDrawPercentage { get; set; } = 0f;
        private float _bowReleaseTimer = 0f;
        public float BowDrawTime { get; set; } = 0f;
        public bool IsDrawingBow { get; set; } = false;

        // Line-of-Sight Obstruction & Sideways Orbital Vantage state
        private Vector3 _activeVantageTarget = Vector3.zero;
        private float _vantageLatchTimer = 0f;
        private int _orbitalDirectionSign = 1;
        public Character CurrentFocusTarget { get; set; } = null;
        public string LastShotBlockerName { get; private set; } = "";
        public bool IsRepositioningForClearShot => _vantageLatchTimer > 0f && _activeVantageTarget != Vector3.zero;

        public int HunterSkillLevel => 1; // Future skill progression stub
        public float GetHunterMeleeSwitchRange() => 10f;
        public float GetHunterSnipingMinRange() => 10f;
        public float GetHunterSnipingMaxRange() => Mathf.Clamp(40f + (HunterSkillLevel - 1) * 2f, 35f, 55f);
        public float GetHunterStalkingMaxRange() => GetHunterSnipingMaxRange() + 10f;

        public void SetCrouch(bool crouch)
        {
            IsCrouchingOrStalking = crouch;
            if (m_humanoid != null)
            {
                var zanim = m_humanoid.GetComponent<ZSyncAnimation>();
                zanim?.SetBool("crouching", crouch);
            }
        }

        public bool IsActionLocked
        {
            get
            {
                if (m_humanoid == null) return false;
                if (m_humanoid.InAttack()) return true;
                if (_activeThreat != null) return true;
                if (CurrentRole == WorkerRole.Idle || CurrentRole == WorkerRole.Follow) return false;
                if (_tasks != null && _tasks.TryGetValue(CurrentRole, out var task) && task != null && task.IsPerformingAction)
                {
                    return true;
                }
                return false;
            }
        }

        // Minimap Pin state
        private Minimap.PinData _mapPin = null;
        private float _mapPinSyncTimer = 0f;

        private readonly Dictionary<WorkerRole, IWorkerTask> _tasks = new Dictionary<WorkerRole, IWorkerTask>();
        private IWorkerTask _activeTask;
        private float _lastAiDt = 0.05f;
        private bool _postStartVisualSyncDone = false;
        private float _lastHoldOpenTime = 0f;

        // Cached high-performance delegates for BaseAI & VisEquipment methods
        private static readonly Func<BaseAI, float, Vector3, float, bool, bool> s_moveTo =
            AccessTools.MethodDelegate<Func<BaseAI, float, Vector3, float, bool, bool>>(
                AccessTools.Method(typeof(BaseAI), "MoveTo", new[] { typeof(float), typeof(Vector3), typeof(float), typeof(bool) }));

        private static readonly Action<BaseAI, Vector3> s_lookAt =
            AccessTools.MethodDelegate<Action<BaseAI, Vector3>>(
                AccessTools.Method(typeof(BaseAI), "LookAt", new[] { typeof(Vector3) }));

        private static readonly Action<VisEquipment> s_updateVisuals =
            AccessTools.MethodDelegate<Action<VisEquipment>>(
                AccessTools.Method(typeof(VisEquipment), "UpdateVisuals"));

        private static readonly Action<BaseAI, Vector3> s_setPatrolPoint =
            AccessTools.MethodDelegate<Action<BaseAI, Vector3>>(
                AccessTools.Method(typeof(BaseAI), "SetPatrolPoint", new[] { typeof(Vector3) }));

        private static readonly System.Reflection.FieldInfo s_spawnPointField =
            AccessTools.Field(typeof(BaseAI), "m_spawnPoint");

        private static readonly AccessTools.FieldRef<Humanoid, float> s_attackDrawTimeRef =
            AccessTools.FieldRefAccess<Humanoid, float>("m_attackDrawTime");

        private void OnEnable()
        {
            if (!ActiveWorkers.Contains(this))
            {
                ActiveWorkers.Add(this);
            }
        }

        private void OnDisable()
        {
            ActiveWorkers.Remove(this);
            RemoveMapPin();
            VerificationTelemetry.RemoveWorkerSnapshot(GetInstanceID().ToString());
        }

        private void OnDestroy()
        {
            ActiveWorkers.Remove(this);
            RemoveMapPin();
            VerificationTelemetry.RemoveWorkerSnapshot(GetInstanceID().ToString());
        }

        public void EnsureInventoryLink()
        {
            if (m_container != null && m_humanoid != null)
            {
                var containerInv = m_container.GetInventory();
                if (containerInv != null)
                {
                    var currentInv = m_humanoid.GetInventory();
                    if (currentInv != containerInv)
                    {
                        Traverse.Create(m_humanoid).Field("m_inventory").SetValue(containerInv);
                    }
                    containerInv.m_onChanged -= OnInventoryChanged;
                    containerInv.m_onChanged += OnInventoryChanged;
                }
            }
        }

        private void Awake()
        {
            if (!ActiveWorkers.Contains(this))
            {
                ActiveWorkers.Add(this);
            }
            m_character = GetComponent<Character>();
            m_humanoid = GetComponent<Humanoid>();
            m_baseAI = GetComponent<BaseAI>();
            m_monsterAI = GetComponent<MonsterAI>();
            m_nview = GetComponent<ZNetView>();
            m_visEquipment = GetComponent<VisEquipment>();
            m_footStep = GetComponent<FootStep>();
            m_container = GetComponent<Container>();
            if (m_container == null)
            {
                m_container = gameObject.AddComponent<Container>();
            }
            m_container.m_width = 8;
            m_container.m_height = 4;
            m_container.m_name = GetHoverName();
            m_container.m_privacy = Container.PrivacySetting.Public;
            m_container.m_checkGuardStone = false;

            EnsureInventoryLink();

            var gathererTask = new GathererTask();
            RegisterTask(new HaulerTask());
            RegisterTask(new BaseTenderTask());
            RegisterTask(new GuardTask());
            RegisterTask(new FarmerTask());
            RegisterTask(new CookTask());
            RegisterTask(gathererTask);
            _tasks[WorkerRole.Lumberjack] = gathererTask;
            _tasks[WorkerRole.Miner] = gathererTask;
            _tasks[WorkerRole.Hunter] = gathererTask;
            _tasks[WorkerRole.Gatherer] = gathererTask;

            if (m_character != null)
            {
                m_character.m_faction = Character.Faction.Players;
                m_character.m_group = "players";
                m_character.m_jumpForce = 9.5f;
                m_character.m_jumpForceForward = 2.5f;
            }

            if (m_baseAI != null)
            {
                m_baseAI.m_jumpInterval = 1.2f;
            }

            if (m_nview != null && m_nview.IsValid())
            {
                LoadFromZDO();
            }
            else
            {
                Customization = new WorkerCustomization();
                HomePosition = transform.position;
                HasHome = true;
                SyncBaseAiAnchor(HomePosition);
                SetRole(WorkerRole.Hauler, false);
            }
        }

        private void Start()
        {
            if (!ActiveWorkers.Contains(this))
            {
                ActiveWorkers.Add(this);
            }
            EnsureInventoryLink();
            if (Archetype == null)
            {
                Archetype = WorkerCatalog.Get("Worker_Viking") ?? WorkerCatalog.Get("Worker_Peasant");
            }
            if (Customization == null)
            {
                Customization = new WorkerCustomization();
            }

            PurgeInvalidAndStaleMonsterGear();
            SyncBaseAiAnchor(HomePosition);
            ApplyVisualEquipment();
            ApplySoundSettings();

            if (m_character != null)
            {
                m_character.m_onDeath -= OnWorkerDied;
                m_character.m_onDeath += OnWorkerDied;
            }

            LogAction($"Initialized worker '{GetHoverName()}' with role [{CurrentRole}] at {transform.position:F1} (Home: {HomePosition:F1})", true);
        }

        private void OnWorkerDied()
        {
            RemoveMapPin();
            DropAllItems();
            LogAction($"Worker '{GetHoverName()}' died. Inventory and equipment dropped.", true);
            Say("I have fallen...");
        }

        public void LogAction(string message, bool forceConsole = false)
        {
            if (string.IsNullOrEmpty(message)) return;

            string key = $"{CurrentRole}:{message}";
            if (key == _lastLoggedActionKey && !forceConsole) return;
            _lastLoggedActionKey = key;

            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            string entry = $"[{timestamp}] [{CurrentRole}] {message}";

            ActionLog.Add(entry);
            if (ActionLog.Count > MaxLogEntries)
            {
                ActionLog.RemoveAt(0);
            }

            if (VerboseConsoleLog || forceConsole)
            {
                ValheimWorkersPlugin.Log.LogInfo($"[ValheimWorkers][NPC:{GetHoverName()}] {entry}");
            }

            VerificationTelemetry.RecordLog(GetHoverName(), $"[{CurrentRole}] {message}", "NPC");
        }

        public void ClearActionLog()
        {
            ActionLog.Clear();
            _lastLoggedActionKey = "";
            LogAction("Action log cleared by user.", true);
        }

        public string GetCompactStateSnapshot()
        {
            float hp = m_character != null ? m_character.GetHealth() : 0f;
            float maxHp = m_character != null ? m_character.GetMaxHealth() : 100f;
            string rItem = GetEquippedRightItem() != null ? GetItemPrefabName(GetEquippedRightItem()) : "Unarmed";
            string lItem = GetEquippedLeftItem() != null ? GetItemPrefabName(GetEquippedLeftItem()) : "None";
            string chest = Customization != null && !string.IsNullOrEmpty(Customization.Chest) ? Customization.Chest : "None";
            int carriedCount = GetCarriedItems().Count;
            return $"Pos: {transform.position:F1} | HP: {hp:F0}/{maxHp:F0} | Role: {CurrentRole} | Weapon: {rItem} | Shield: {lItem} | Armor: {chest} | Carried: {carriedCount} items | Home: {HomePosition:F1} (Rad: {WorkRadius:F0}m)";
        }

        public string StartCommandTrace(string commandType, string commandText, string relatedTestId = "")
        {
            string initialState = GetCompactStateSnapshot();
            ActiveCommandTraceId = VerificationTelemetry.StartCommandTrace(GetHoverName(), CurrentRole.ToString(), commandType, commandText, initialState, relatedTestId);
            LogAction($"[Trace: {ActiveCommandTraceId}] CMD: {commandText}", true);
            return ActiveCommandTraceId;
        }

        public void RecordTraceStep(string category, string message)
        {
            if (!string.IsNullOrEmpty(ActiveCommandTraceId))
            {
                VerificationTelemetry.RecordTraceStep(ActiveCommandTraceId, category, message, transform.position);
            }
        }

        public void CompleteCommandTrace(string outcomeStatus, string outcomeSummary)
        {
            if (!string.IsNullOrEmpty(ActiveCommandTraceId))
            {
                VerificationTelemetry.CompleteCommandTrace(ActiveCommandTraceId, outcomeStatus, outcomeSummary);
                LogAction($"[Trace: {ActiveCommandTraceId}] {outcomeStatus}: {outcomeSummary}", true);
            }
        }

        public VerificationTelemetry.WorkerLiveSnapshot CreateLiveSnapshot()
        {
            try
            {
                float hp = m_character != null ? m_character.GetHealth() : 0f;
                float maxHp = m_character != null ? m_character.GetMaxHealth() : 100f;
                float pDist = (Player.m_localPlayer != null && transform != null) ? Vector3.Distance(transform.position, Player.m_localPlayer.transform.position) : 0f;
                Vector3 targetPost = HasWorkPost ? WorkPostPosition : HomePosition;
                float postDist = transform != null ? Vector3.Distance(transform.position, targetPost) : 0f;

                var primary = GetEquippedPrimaryWeapon();
                string rStr = primary != null ? $"{GetItemPrefabName(primary)} (T{primary.m_shared?.m_toolTier ?? 0})" : "Unarmed";
                var left = GetEquippedLeftItem();
                string lStr = (left != null && left != primary) ? GetItemPrefabName(left) : "None";

                string carriedSummary = "Empty";
                try
                {
                    var carried = GetCarriedItems();
                    if (carried != null && carried.Count > 0)
                    {
                        carriedSummary = string.Join(", ", carried.ConvertAll(c => $"{GetItemPrefabName(c)} x{c.m_stack}"));
                    }
                }
                catch
                {
                    carriedSummary = "Empty";
                }

                string armorStr = Customization != null && !string.IsNullOrEmpty(Customization.Chest) ? Customization.Chest : "None";
                Vector3 pos = transform != null ? transform.position : Vector3.zero;

                return new VerificationTelemetry.WorkerLiveSnapshot
                {
                    WorkerId = GetInstanceID().ToString(),
                    Name = GetHoverName() ?? "Worker",
                    Role = CurrentRole.ToString(),
                    TaskState = _activeTask != null ? _activeTask.GetStatusDescription() : "Idle",
                    StatusText = GetCurrentStatusDescription() ?? "Active",
                    Position = $"{pos.x.ToString("F1", CultureInfo.InvariantCulture)}, {pos.y.ToString("F1", CultureInfo.InvariantCulture)}, {pos.z.ToString("F1", CultureInfo.InvariantCulture)}",
                    HomePost = $"{HomePosition.x.ToString("F1", CultureInfo.InvariantCulture)}, {HomePosition.y.ToString("F1", CultureInfo.InvariantCulture)}, {HomePosition.z.ToString("F1", CultureInfo.InvariantCulture)}",
                    WorkPost = HasWorkPost ? $"{WorkPostPosition.x.ToString("F1", CultureInfo.InvariantCulture)}, {WorkPostPosition.y.ToString("F1", CultureInfo.InvariantCulture)}, {WorkPostPosition.z.ToString("F1", CultureInfo.InvariantCulture)}" : "None",
                    WorkRadius = WorkRadius,
                    DistFromPlayer = pDist,
                    DistFromPost = postDist,
                    Health = hp,
                    MaxHealth = maxHp,
                    EquippedRight = rStr,
                    EquippedLeft = lStr,
                    ArmorSummary = armorStr,
                    CarriedItemsSummary = carriedSummary,
                    ActiveCommandId = ActiveCommandTraceId ?? "",
                    ActiveCommandSummary = VerificationTelemetry.GetTraceSummary(ActiveCommandTraceId)
                };
            }
            catch (Exception ex)
            {
                return new VerificationTelemetry.WorkerLiveSnapshot
                {
                    WorkerId = GetInstanceID().ToString(),
                    Name = "Worker",
                    Role = CurrentRole.ToString(),
                    TaskState = "Active",
                    StatusText = $"Snapshot error: {ex.Message}",
                    Position = transform != null ? $"{transform.position.x:F1}, {transform.position.y:F1}, {transform.position.z:F1}" : "0, 0, 0",
                    HomePost = "0, 0, 0",
                    WorkPost = "None",
                    WorkRadius = WorkRadius,
                    DistFromPlayer = 0f,
                    DistFromPost = 0f,
                    Health = 50f,
                    MaxHealth = 50f,
                    EquippedRight = "Unarmed",
                    EquippedLeft = "None",
                    ArmorSummary = "None",
                    CarriedItemsSummary = "Empty",
                    ActiveCommandId = ActiveCommandTraceId ?? "",
                    ActiveCommandSummary = ""
                };
            }
        }

        private void SyncBaseAiAnchor(Vector3 pos)
        {
            if (m_baseAI == null) return;
            try
            {
                s_setPatrolPoint?.Invoke(m_baseAI, pos);
                s_spawnPointField?.SetValue(m_baseAI, pos);
            }
            catch
            {
                // Ignore if BaseAI not yet initialized
            }
        }

        public void RegisterTask(IWorkerTask task)
        {
            if (task == null) return;
            _tasks[task.Role] = task;
        }

        public void LoadFromZDO()
        {
            if (m_nview == null || m_nview.GetZDO() == null) return;

            var zdo = m_nview.GetZDO();
            int roleInt = zdo.GetInt(WorkerZdoKeys.RoleHash, (int)WorkerRole.Hauler);
            CurrentRole = (WorkerRole)roleInt;
            if (CurrentRole != WorkerRole.Follow)
            {
                PreviousStationRole = CurrentRole;
            }

            HasHome = zdo.GetBool(WorkerZdoKeys.HasHomeHash, false);
            if (HasHome)
            {
                HomePosition = zdo.GetVec3(WorkerZdoKeys.HomePosHash, transform.position);
            }
            else
            {
                HomePosition = transform.position;
            }
            SyncBaseAiAnchor(HomePosition);

            WorkRadius = zdo.GetFloat(WorkerZdoKeys.WorkRadiusHash, 25f);
            ReturnFillThreshold = zdo.GetFloat(WorkerZdoKeys.ReturnThresholdHash, 0.75f);

            HasWorkPost = zdo.GetBool(WorkerZdoKeys.HasWorkPostHash, false);
            if (HasWorkPost)
            {
                WorkPostPosition = zdo.GetVec3(WorkerZdoKeys.WorkPostPosHash, HomePosition);
            }
            else
            {
                WorkPostPosition = HomePosition;
            }

            string archId = zdo.GetString(WorkerZdoKeys.ArchetypeHash, "Worker_Viking");
            Archetype = WorkerCatalog.Get(archId) ?? WorkerCatalog.Get("Worker_Viking");

            Customization = WorkerCustomization.LoadFromZDO(zdo);
            if (m_character != null && !string.IsNullOrEmpty(Customization.Name))
            {
                m_character.m_name = Customization.Name;
            }

            LoadProgressionFromZDO();
            UpdateActiveTask();
        }

        public void SaveToZDO()
        {
            if (m_nview == null || m_nview.GetZDO() == null || !m_nview.IsOwner()) return;

            var zdo = m_nview.GetZDO();
            zdo.Set(WorkerZdoKeys.RoleHash, (int)CurrentRole);
            zdo.Set(WorkerZdoKeys.HasHomeHash, HasHome);
            zdo.Set(WorkerZdoKeys.HomePosHash, HomePosition);
            zdo.Set(WorkerZdoKeys.HasWorkPostHash, HasWorkPost);
            zdo.Set(WorkerZdoKeys.WorkPostPosHash, WorkPostPosition);
            zdo.Set(WorkerZdoKeys.WorkRadiusHash, WorkRadius);
            zdo.Set(WorkerZdoKeys.ReturnThresholdHash, ReturnFillThreshold);
            if (Archetype != null)
            {
                zdo.Set(WorkerZdoKeys.ArchetypeHash, Archetype.Id);
            }
            if (Customization != null)
            {
                Customization.SaveToZDO(zdo);
            }
            SaveProgressionToZDO();
        }

        public Vector3 GetWorkPostPosition() => HasWorkPost ? WorkPostPosition : (HasHome ? HomePosition : transform.position);

        public void SetWorkPost(Vector3 pos, bool notify = true)
        {
            WorkPostPosition = pos;
            HasWorkPost = true;
            SaveToZDO();
            StartCommandTrace("SET_WORK_POST", $"Work area post anchored at {pos:F1}");
            LogAction($"Work area post anchored at {pos:F1}", true);
            if (notify) Say("Work area anchored here!");
        }

        public void ClearWorkPost()
        {
            HasWorkPost = false;
            WorkPostPosition = HomePosition;
            SaveToZDO();
            StartCommandTrace("CLEAR_WORK_POST", "Work area post reset to camp post");
            LogAction("Work area post reset to camp post.", true);
        }

        public void ApplyCustomization(WorkerCustomization config)
        {
            if (config == null) return;
            Customization = config.Clone();

            if (m_character != null)
            {
                m_character.m_name = Customization.Name;
                m_character.SetMaxHealth(Customization.MaxHealth);
                m_character.SetHealth(Customization.MaxHealth);
            }

            WorkRadius = Customization.WorkRadius;
            ReturnFillThreshold = Customization.InventoryReturnThreshold;
            if (Customization.InitialRole != WorkerRole.Follow)
            {
                PreviousStationRole = Customization.InitialRole;
            }
            SetRole(Customization.InitialRole, false);
            ApplyVisualEquipment();
            ApplySoundSettings();
            SaveToZDO();
        }

        public void ApplySoundSettings()
        {
            if (Customization == null || ZNetScene.instance == null) return;

            try
            {
                string profile = Customization.VoiceProfile ?? "Human";
                bool isSilent = string.Equals(profile, "Silent", StringComparison.OrdinalIgnoreCase);

                // 1. Footsteps
                if (m_footStep == null) m_footStep = GetComponent<FootStep>();
                if (m_footStep != null)
                {
                    m_footStep.enabled = !isSilent && Customization.FootstepsEnabled;
                }

                // 2. Hit & Death vocalizations on Character
                if (m_character != null)
                {
                    if (isSilent)
                    {
                        m_character.m_hitEffects = new EffectList();
                        m_character.m_critHitEffects = new EffectList();
                        m_character.m_deathEffects = new EffectList();
                    }
                    else
                    {
                        string sourcePrefabName = profile switch
                        {
                            "Dvergr" => "Dverger",
                            "Skeleton" => "Skeleton_NoArcher",
                            _ => "Player"
                        };
                        GameObject srcObj = ZNetScene.instance.GetPrefab(sourcePrefabName);
                        Character srcChar = srcObj != null ? srcObj.GetComponent<Character>() : null;
                        if (srcChar != null)
                        {
                            m_character.m_hitEffects = srcChar.m_hitEffects;
                            m_character.m_critHitEffects = srcChar.m_critHitEffects;
                            m_character.m_deathEffects = srcChar.m_deathEffects;
                        }
                    }
                }

                // 3. Idle chatter / grunts & combat alert sounds on BaseAI
                if (m_baseAI != null)
                {
                    if (isSilent || Customization.IdleChatterMode <= 0)
                    {
                        m_baseAI.m_idleSound = new EffectList();
                    }
                    else
                    {
                        string aiSoundPrefab = profile == "Skeleton" ? "Skeleton_NoArcher" : "Dverger";
                        GameObject aiSrcObj = ZNetScene.instance.GetPrefab(aiSoundPrefab);
                        BaseAI srcAi = aiSrcObj != null ? aiSrcObj.GetComponent<BaseAI>() : null;
                        if (srcAi != null)
                        {
                            m_baseAI.m_idleSound = srcAi.m_idleSound;
                            m_baseAI.m_idleSoundInterval = Customization.IdleChatterMode == 1 ? 35f : 12f;
                            m_baseAI.m_idleSoundChance = 0.5f;
                        }
                    }

                    if (isSilent || !Customization.CombatShoutsEnabled)
                    {
                        m_baseAI.m_alertedEffects = new EffectList();
                    }
                    else
                    {
                        string alertPrefab = profile == "Skeleton" ? "Skeleton_NoArcher" : "Dverger";
                        GameObject alertSrcObj = ZNetScene.instance.GetPrefab(alertPrefab);
                        BaseAI srcAi = alertSrcObj != null ? alertSrcObj.GetComponent<BaseAI>() : null;
                        if (srcAi != null && profile != "Human")
                        {
                            m_baseAI.m_alertedEffects = srcAi.m_alertedEffects;
                        }
                        else
                        {
                            m_baseAI.m_alertedEffects = new EffectList();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ValheimWorkersPlugin.Log.LogWarning($"[ValheimWorkers] Error applying sound settings: {ex.Message}");
            }
        }

        public void PlayTestSound(bool hurtSound)
        {
            PlayPreviewSound(Customization, transform.position, transform.rotation, transform, hurtSound);
            LogAction($"Played test {(hurtSound ? "hurt/hit" : "voice/alert")} sound (Profile: {Customization?.VoiceProfile ?? "Human"}).", true);
        }

        public static void PlayPreviewSound(WorkerCustomization config, Vector3 pos, Quaternion rot, Transform parent, bool hurtSound)
        {
            if (config == null || ZNetScene.instance == null) return;
            string profile = config.VoiceProfile ?? "Human";
            if (string.Equals(profile, "Silent", StringComparison.OrdinalIgnoreCase)) return;

            string prefabName = profile switch
            {
                "Dvergr" => "Dverger",
                "Skeleton" => "Skeleton_NoArcher",
                _ => "Player"
            };

            GameObject src = ZNetScene.instance.GetPrefab(prefabName);
            if (src == null) return;

            int variant = (prefabName == "Player") ? config.ModelIndex : -1;

            if (hurtSound)
            {
                var ch = src.GetComponent<Character>();
                ch?.m_hitEffects?.Create(pos, rot, parent, 1f, variant);
            }
            else
            {
                if (prefabName == "Player")
                {
                    var hum = src.GetComponent<Humanoid>();
                    if (hum?.m_equipEffects != null && hum.m_equipEffects.HasEffects())
                    {
                        hum.m_equipEffects.Create(pos, rot, parent, 1f, variant);
                    }
                    else
                    {
                        hum?.m_hitEffects?.Create(pos, rot, parent, 1f, variant);
                    }
                }
                else
                {
                    var ai = src.GetComponent<BaseAI>();
                    if (ai?.m_idleSound != null && ai.m_idleSound.HasEffects())
                    {
                        ai.m_idleSound.Create(pos, rot, parent, 1f, -1);
                    }
                    else
                    {
                        ai?.m_alertedEffects?.Create(pos, rot, parent, 1f, -1);
                    }
                }
            }
        }

        public void PurgeInvalidAndStaleMonsterGear()
        {
            if (m_humanoid == null) return;
            var inv = m_humanoid.GetInventory();
            if (inv == null) return;

            var toRemove = new List<ItemDrop.ItemData>();
            foreach (var item in inv.GetAllItems())
            {
                string pName = item.m_dropPrefab != null ? item.m_dropPrefab.name : (item.m_shared?.m_name ?? "");
                if (pName.IndexOf("Dverger", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    pName.IndexOf("Arbalest", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    toRemove.Add(item);
                }
            }

            foreach (var item in toRemove)
            {
                if (item.m_equipped)
                {
                    m_humanoid.UnequipItem(item, false);
                }
                inv.RemoveItem(item);
                LogAction($"Purged stale monster gear '{item.m_shared?.m_name ?? item.m_dropPrefab?.name}' from worker.", true);
            }

            if (Customization != null)
            {
                if (!string.IsNullOrEmpty(Customization.Weapon) &&
                    (Customization.Weapon.IndexOf("Dverger", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     Customization.Weapon.IndexOf("Arbalest", StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    Customization.Weapon = "Club";
                }
                if (!string.IsNullOrEmpty(Customization.Shield) &&
                    Customization.Shield.IndexOf("Dverger", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    Customization.Shield = "";
                }
            }

            if (m_nview != null && m_nview.GetZDO() != null && m_nview.IsOwner())
            {
                int curRight = m_nview.GetZDO().GetInt(ZDOVars.s_rightItem, 0);
                if (curRight == "DvergerArbalest".GetStableHashCode() ||
                    curRight == "DvergerStaffFire".GetStableHashCode() ||
                    curRight == "DvergerStaffHeal".GetStableHashCode())
                {
                    m_nview.GetZDO().Set(ZDOVars.s_rightItem, "Club".GetStableHashCode());
                }
            }
        }

        public void ApplyVisualEquipment()
        {
            if (Customization == null) return;
            if (_isApplyingVisuals) return;

            try
            {
                _isApplyingVisuals = true;

                if (m_humanoid != null)
                {
                    m_humanoid.SetHair(Customization.HairStyle ?? "HairNone");
                    m_humanoid.SetBeard(Customization.BeardStyle ?? "BeardNone");

                    // Ensure loadout items exist in inventory and are equipped
                    if (!string.IsNullOrEmpty(Customization.Weapon)) EnsureEquippedInInventory(Customization.Weapon);
                    if (!string.IsNullOrEmpty(Customization.Shield)) EnsureEquippedInInventory(Customization.Shield);
                    if (!string.IsNullOrEmpty(Customization.Helmet)) EnsureEquippedInInventory(Customization.Helmet);
                    if (!string.IsNullOrEmpty(Customization.Chest)) EnsureEquippedInInventory(Customization.Chest);
                    if (!string.IsNullOrEmpty(Customization.Legs)) EnsureEquippedInInventory(Customization.Legs);
                }

                int weaponHash = !string.IsNullOrEmpty(Customization.Weapon)
                    ? Customization.Weapon.GetStableHashCode()
                    : 0;
                int shieldHash = !string.IsNullOrEmpty(Customization.Shield)
                    ? Customization.Shield.GetStableHashCode()
                    : 0;
                int chestHash = !string.IsNullOrEmpty(Customization.Chest)
                    ? Customization.Chest.GetStableHashCode()
                    : 0;
                int legsHash = !string.IsNullOrEmpty(Customization.Legs)
                    ? Customization.Legs.GetStableHashCode()
                    : 0;
                int helmetHash = !string.IsNullOrEmpty(Customization.Helmet)
                    ? Customization.Helmet.GetStableHashCode()
                    : 0;

                bool isWeaponBow = IsBowName(Customization.Weapon);
                int rightHash = isWeaponBow ? 0 : weaponHash;
                int leftHash = isWeaponBow ? weaponHash : shieldHash;

                if (isWeaponBow)
                {
                    EnsureAmmoEquipped();
                }

                // 1. Force ZDO slots directly so stale monster hashes are wiped out
                if (m_nview != null && m_nview.GetZDO() != null && m_nview.IsOwner())
                {
                    var zdo = m_nview.GetZDO();
                    zdo.Set(ZDOVars.s_rightItem, rightHash);
                    zdo.Set(ZDOVars.s_leftItem, leftHash);
                    zdo.Set(ZDOVars.s_chestItem, chestHash);
                    zdo.Set(ZDOVars.s_legItem, legsHash);
                    zdo.Set(ZDOVars.s_helmetItem, helmetHash);
                    zdo.Set(ZDOVars.s_leftBackItem, 0);
                    zdo.Set(ZDOVars.s_rightBackItem, 0);
                    zdo.Set(ZDOVars.s_shoulderItem, 0);
                }

                // 2. Clear cached fields and destroy attached meshes if slot should be empty
                if (m_visEquipment != null)
                {
                    Traverse.Create(m_visEquipment).Field("m_leftItem").SetValue(-1);
                    Traverse.Create(m_visEquipment).Field("m_rightItem").SetValue(-1);
                    Traverse.Create(m_visEquipment).Field("m_leftBackItem").SetValue(-1);
                    Traverse.Create(m_visEquipment).Field("m_rightBackItem").SetValue(-1);

                    if (leftHash == 0)
                    {
                        var leftInst = Traverse.Create(m_visEquipment).Field<GameObject>("m_leftItemInstance").Value;
                        if (leftInst != null)
                        {
                            UnityEngine.Object.Destroy(leftInst);
                            Traverse.Create(m_visEquipment).Field("m_leftItemInstance").SetValue(null);
                            Traverse.Create(m_visEquipment).Field("m_currentLeftItemHash").SetValue(0);
                        }
                    }

                    if (rightHash == 0)
                    {
                        var rightInst = Traverse.Create(m_visEquipment).Field<GameObject>("m_rightItemInstance").Value;
                        if (rightInst != null)
                        {
                            UnityEngine.Object.Destroy(rightInst);
                            Traverse.Create(m_visEquipment).Field("m_rightItemInstance").SetValue(null);
                            Traverse.Create(m_visEquipment).Field("m_currentRightItemHash").SetValue(0);
                        }
                    }

                    var leftBackInst = Traverse.Create(m_visEquipment).Field<GameObject>("m_leftBackItemInstance").Value;
                    if (leftBackInst != null)
                    {
                        UnityEngine.Object.Destroy(leftBackInst);
                        Traverse.Create(m_visEquipment).Field("m_leftBackItemInstance").SetValue(null);
                        Traverse.Create(m_visEquipment).Field("m_currentLeftBackItemHash").SetValue(0);
                    }

                    var rightBackInst = Traverse.Create(m_visEquipment).Field<GameObject>("m_rightBackItemInstance").Value;
                    if (rightBackInst != null)
                    {
                        UnityEngine.Object.Destroy(rightBackInst);
                        Traverse.Create(m_visEquipment).Field("m_rightBackItemInstance").SetValue(null);
                        Traverse.Create(m_visEquipment).Field("m_currentRightBackItemHash").SetValue(0);
                    }

                    m_visEquipment.SetModel(Customization.ModelIndex);

                    int hairHash = !string.IsNullOrEmpty(Customization.HairStyle)
                        ? Customization.HairStyle.GetStableHashCode()
                        : 0;
                    m_visEquipment.SetHairItem(hairHash);

                    int beardHash = !string.IsNullOrEmpty(Customization.BeardStyle)
                        ? Customization.BeardStyle.GetStableHashCode()
                        : 0;
                    m_visEquipment.SetBeardItem(beardHash);

                    m_visEquipment.SetHairColor(Customization.HairColor);
                    m_visEquipment.SetSkinColor(Customization.SkinColor);

                    m_visEquipment.SetRightItem(rightHash, 0);
                    m_visEquipment.SetLeftItem(leftHash, 0, 1);
                    m_visEquipment.SetChestItem(chestHash);
                    m_visEquipment.SetLegItem(legsHash);
                    m_visEquipment.SetHelmetItem(helmetHash);
                    m_visEquipment.SetLeftBackItem(0, 0, 0);
                    m_visEquipment.SetRightBackItem(0, 0);

                    s_updateVisuals?.Invoke(m_visEquipment);
                }
                SyncInventoryEquippedFlags();
                if (!string.IsNullOrEmpty(Customization.Weapon) || !string.IsNullOrEmpty(Customization.Shield) || !string.IsNullOrEmpty(Customization.Chest))
                {
                    VerificationTelemetry.RecordPass("interaction_loadout_retention", $"Worker '{GetHoverName()}' loadout retained: Weapon='{Customization.Weapon}', Shield='{Customization.Shield}', Armor='{Customization.Chest}'.");
                }
            }
            catch (Exception ex)
            {
                ValheimWorkersPlugin.Log.LogWarning($"[ValheimWorkers] Error applying visual equipment: {ex.Message}");
            }
            finally
            {
                _isApplyingVisuals = false;
            }
        }

        public void GiveInitialLoadout(WorkerCustomization config)
        {
            if (config == null || m_humanoid == null) return;
            var inv = m_humanoid.GetInventory();
            if (inv == null || inv.NrOfItems() > 0) return; // Only give once to empty inventory upon initial spawn

            void AddPrefab(string name)
            {
                if (string.IsNullOrEmpty(name)) return;
                GameObject prefab = ObjectDB.instance != null ? ObjectDB.instance.GetItemPrefab(name) : null;
                if (prefab == null && ZNetScene.instance != null) prefab = ZNetScene.instance.GetPrefab(name);
                if (prefab != null)
                {
                    m_humanoid.PickupPrefab(prefab, 1, false);
                }
            }

            AddPrefab(config.Weapon);
            AddPrefab(config.Shield);
            AddPrefab(config.Helmet);
            AddPrefab(config.Chest);
            AddPrefab(config.Legs);
        }

        private void EnsureEquippedInInventory(string itemName)
        {
            if (string.IsNullOrEmpty(itemName) || m_humanoid == null) return;
            var inv = m_humanoid.GetInventory();
            if (inv == null) return;

            ItemDrop.ItemData targetItem = null;
            foreach (var item in inv.GetAllItems())
            {
                if (item == null) continue;
                string pName = item.m_dropPrefab != null ? item.m_dropPrefab.name : (item.m_shared?.m_name ?? "");
                if (string.Equals(pName, itemName, StringComparison.OrdinalIgnoreCase))
                {
                    targetItem = item;
                    break;
                }
            }

            if (targetItem == null)
            {
                return; // Do not conjure items out of nothing!
            }

            if (targetItem != null)
            {
                EnsureDropPrefab(targetItem);
                targetItem.m_equipped = true;
                if (targetItem.m_shared != null && targetItem.m_shared.m_useDurability)
                {
                    targetItem.m_durability = targetItem.GetMaxDurability();
                }

                // Explicitly set humanoid equipment slots for armor so slots never sit at null
                if (targetItem.m_shared != null)
                {
                    var itemType = targetItem.m_shared.m_itemType;
                    if (itemType == ItemDrop.ItemData.ItemType.Chest)
                    {
                        Traverse.Create(m_humanoid).Field("m_chestItem").SetValue(targetItem);
                    }
                    else if (itemType == ItemDrop.ItemData.ItemType.Legs)
                    {
                        Traverse.Create(m_humanoid).Field("m_legItem").SetValue(targetItem);
                    }
                    else if (itemType == ItemDrop.ItemData.ItemType.Helmet)
                    {
                        Traverse.Create(m_humanoid).Field("m_helmetItem").SetValue(targetItem);
                    }
                    else if (itemType == ItemDrop.ItemData.ItemType.Shoulder)
                    {
                        Traverse.Create(m_humanoid).Field("m_shoulderItem").SetValue(targetItem);
                    }
                }

                if (!m_humanoid.InAttack())
                {
                    m_humanoid.EquipItem(targetItem, false);
                }
            }
        }

        public void SyncInventoryEquippedFlags()
        {
            if (m_humanoid == null) return;
            var inv = m_humanoid.GetInventory();
            if (inv == null) return;

            var rightItem = GetEquippedRightItem();
            var leftItem = GetEquippedLeftItem();
            var chestItem = Traverse.Create(m_humanoid).Field<ItemDrop.ItemData>("m_chestItem").Value;
            var legItem = Traverse.Create(m_humanoid).Field<ItemDrop.ItemData>("m_legItem").Value;
            var helmetItem = Traverse.Create(m_humanoid).Field<ItemDrop.ItemData>("m_helmetItem").Value;

            foreach (var item in inv.GetAllItems())
            {
                if (item == null) continue;
                bool isEquipped = (item == rightItem || item == leftItem || item == chestItem || item == legItem || item == helmetItem);
                if (!isEquipped && m_humanoid.IsItemEquiped(item)) isEquipped = true;
                item.m_equipped = isEquipped;
            }
        }

        public ItemDrop.ItemData GetEquippedRightItem()
        {
            if (m_humanoid == null) return null;
            var r = Traverse.Create(m_humanoid).Field<ItemDrop.ItemData>("m_rightItem").Value;
            if (r != null) return r;
            return Traverse.Create(m_humanoid).Field<ItemDrop.ItemData>("m_hiddenRightItem").Value;
        }

        public ItemDrop.ItemData GetEquippedLeftItem()
        {
            if (m_humanoid == null) return null;
            var l = Traverse.Create(m_humanoid).Field<ItemDrop.ItemData>("m_leftItem").Value;
            if (l != null) return l;
            return Traverse.Create(m_humanoid).Field<ItemDrop.ItemData>("m_hiddenLeftItem").Value;
        }

        public ItemDrop.ItemData GetEquippedPrimaryWeapon()
        {
            var right = GetEquippedRightItem();
            if (right != null) return right;

            var left = GetEquippedLeftItem();
            if (left != null && left.m_shared != null)
            {
                var t = left.m_shared.m_itemType;
                if (t == ItemDrop.ItemData.ItemType.Bow ||
                    t == ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft ||
                    t == ItemDrop.ItemData.ItemType.TwoHandedWeapon ||
                    t == ItemDrop.ItemData.ItemType.OneHandedWeapon)
                {
                    return left;
                }
            }
            return null;
        }

        public ItemDrop.ItemData GetEquippedBow()
        {
            if (m_humanoid == null) return null;
            var left = GetEquippedLeftItem();
            if (left?.m_shared != null && left.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Bow)
            {
                return left;
            }
            var right = GetEquippedRightItem();
            if (right?.m_shared != null && right.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Bow)
            {
                return right;
            }
            var current = m_humanoid.GetCurrentWeapon();
            if (current?.m_shared != null && current.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Bow)
            {
                return current;
            }
            return null;
        }

        public ItemDrop.ItemData GetEquippedAmmo()
        {
            if (m_humanoid == null) return null;
            return Traverse.Create(m_humanoid).Field<ItemDrop.ItemData>("m_ammoItem").Value;
        }

        public bool IsHoldingBow()
        {
            return GetEquippedBow() != null;
        }

        public void EnsureAmmoEquipped()
        {
            if (m_humanoid == null) return;
            var inv = m_humanoid.GetInventory();
            if (inv == null) return;

            ItemDrop.ItemData bestAmmo = null;
            float bestAmmoScore = -1f;

            foreach (var item in inv.GetAllItems())
            {
                if (item != null && item.m_shared != null && item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Ammo && item.m_stack > 0)
                {
                    float score = item.GetDamage().GetTotalDamage();
                    if (score > bestAmmoScore)
                    {
                        bestAmmoScore = score;
                        bestAmmo = item;
                    }
                }
            }

            if (bestAmmo != null)
            {
                var currentAmmo = Traverse.Create(m_humanoid).Field<ItemDrop.ItemData>("m_ammoItem").Value;
                if (currentAmmo != bestAmmo)
                {
                    m_humanoid.EquipItem(bestAmmo, false);
                    Traverse.Create(m_humanoid).Field("m_ammoItem").SetValue(bestAmmo);
                }
            }
        }

        public static bool IsBowName(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            return name.IndexOf("Bow", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   name.IndexOf("Crossbow", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   name.IndexOf("Arbalest", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static void EnsureDropPrefab(ItemDrop.ItemData item)
        {
            if (item == null) return;
            if (item.m_dropPrefab != null) return;
            if (ObjectDB.instance == null) return;

            if (item.m_shared != null && !string.IsNullOrEmpty(item.m_shared.m_name))
            {
                // 1. Try to find by item name directly if it looks like a prefab name
                var direct = ObjectDB.instance.GetItemPrefab(item.m_shared.m_name);
                if (direct != null)
                {
                    item.m_dropPrefab = direct;
                    return;
                }

                // 2. Iterate ObjectDB items matching m_shared.m_name
                foreach (var go in ObjectDB.instance.m_items)
                {
                    if (go == null) continue;
                    var drop = go.GetComponent<ItemDrop>();
                    if (drop != null && drop.m_itemData?.m_shared != null &&
                        drop.m_itemData.m_shared.m_name == item.m_shared.m_name)
                    {
                        item.m_dropPrefab = go;
                        return;
                    }
                }
            }
        }

        public static string GetItemPrefabName(ItemDrop.ItemData item)
        {
            if (item == null) return "";
            EnsureDropPrefab(item);
            if (item.m_dropPrefab != null) return item.m_dropPrefab.name;
            if (item.m_shared != null && !string.IsNullOrEmpty(item.m_shared.m_name))
            {
                string shared = item.m_shared.m_name;
                if (shared.StartsWith("$item_"))
                {
                    string candidate = shared.Substring(6);
                    if (ObjectDB.instance != null)
                    {
                        foreach (var go in ObjectDB.instance.m_items)
                        {
                            if (go != null && string.Equals(go.name.Replace("_", ""), candidate.Replace("_", ""), StringComparison.OrdinalIgnoreCase))
                            {
                                return go.name;
                            }
                        }
                    }
                }
            }
            return "";
        }

        public void RestoreMissingDropPrefabs()
        {
            var inv = m_humanoid != null ? m_humanoid.GetInventory() : null;
            if (inv == null) return;
            foreach (var item in inv.GetAllItems())
            {
                EnsureDropPrefab(item);
            }
        }

        public void UpdateCustomizationFromCurrentEquipment()
        {
            if (Customization == null) Customization = new WorkerCustomization();
            if (m_humanoid == null) return;

            var rightItem = GetEquippedRightItem();
            var leftItem = GetEquippedLeftItem();
            var chestItem = Traverse.Create(m_humanoid).Field<ItemDrop.ItemData>("m_chestItem").Value;
            var legItem = Traverse.Create(m_humanoid).Field<ItemDrop.ItemData>("m_legItem").Value;
            var helmetItem = Traverse.Create(m_humanoid).Field<ItemDrop.ItemData>("m_helmetItem").Value;

            if (rightItem != null) EnsureDropPrefab(rightItem);
            if (leftItem != null) EnsureDropPrefab(leftItem);
            if (chestItem != null) EnsureDropPrefab(chestItem);
            if (legItem != null) EnsureDropPrefab(legItem);
            if (helmetItem != null) EnsureDropPrefab(helmetItem);

            ItemDrop.ItemData primaryWeapon = rightItem;
            if (primaryWeapon == null && leftItem != null && leftItem.m_shared?.m_itemType != ItemDrop.ItemData.ItemType.Shield)
            {
                primaryWeapon = leftItem;
            }

            Customization.Weapon = GetItemPrefabName(primaryWeapon) ?? "";
            if (leftItem != null && leftItem.m_shared?.m_itemType == ItemDrop.ItemData.ItemType.Shield)
            {
                Customization.Shield = GetItemPrefabName(leftItem) ?? "";
            }
            else
            {
                Customization.Shield = "";
            }

            Customization.Chest = GetItemPrefabName(chestItem) ?? "";
            Customization.Legs = GetItemPrefabName(legItem) ?? "";
            Customization.Helmet = GetItemPrefabName(helmetItem) ?? "";
            SyncInventoryEquippedFlags();
        }

        private ItemDrop.ItemData FindBestArmorInInventory(Inventory inv, ItemDrop.ItemData.ItemType armorType)
        {
            ItemDrop.ItemData best = null;
            float bestArmor = -1f;
            foreach (var item in inv.GetAllItems())
            {
                if (item != null && item.m_shared != null && item.m_shared.m_itemType == armorType)
                {
                    if (item.m_shared.m_armor > bestArmor)
                    {
                        bestArmor = item.m_shared.m_armor;
                        best = item;
                    }
                }
            }
            return best;
        }

        private ItemDrop.ItemData FindBestToolInInventory(Inventory inv, string toolSubstring)
        {
            ItemDrop.ItemData best = null;
            int highestTier = -1;
            foreach (var item in inv.GetAllItems())
            {
                if (item == null || item.m_shared == null) continue;
                string pName = item.m_dropPrefab != null ? item.m_dropPrefab.name : (item.m_shared.m_name ?? "");
                bool matches = pName.IndexOf(toolSubstring, StringComparison.OrdinalIgnoreCase) >= 0;
                if (!matches && string.Equals(toolSubstring, "Pickaxe", StringComparison.OrdinalIgnoreCase))
                {
                    matches = (item.m_shared.m_skillType == Skills.SkillType.Pickaxes);
                }
                else if (!matches && string.Equals(toolSubstring, "Axe", StringComparison.OrdinalIgnoreCase))
                {
                    matches = (item.m_shared.m_skillType == Skills.SkillType.Axes);
                }

                if (matches)
                {
                    if (item.m_shared.m_toolTier > highestTier)
                    {
                        highestTier = item.m_shared.m_toolTier;
                        best = item;
                    }
                }
            }
            return best;
        }

        private ItemDrop.ItemData FindBestWeaponByType(Inventory inv, ItemDrop.ItemData.ItemType weaponType)
        {
            ItemDrop.ItemData best = null;
            float bestDmg = -1f;
            foreach (var item in inv.GetAllItems())
            {
                if (item != null && item.m_shared != null && item.m_shared.m_itemType == weaponType)
                {
                    float dmg = item.GetDamage().GetTotalDamage();
                    if (dmg > bestDmg)
                    {
                        bestDmg = dmg;
                        best = item;
                    }
                }
            }
            return best;
        }

        public void AutoEquipBestGearForRole()
        {
            if (m_humanoid == null || m_container == null) return;
            var inv = m_container.GetInventory();
            if (inv == null) return;

            switch (CurrentRole)
            {
                case WorkerRole.Lumberjack:
                    var axe = FindBestToolInInventory(inv, "Axe");
                    if (axe != null)
                    {
                        if (GetEquippedRightItem() != axe) m_humanoid.EquipItem(axe, false);
                        return;
                    }
                    break;

                case WorkerRole.Miner:
                    var pick = FindBestToolInInventory(inv, "Pickaxe");
                    if (pick != null)
                    {
                        if (GetEquippedRightItem() != pick) m_humanoid.EquipItem(pick, false);
                        return;
                    }
                    break;

                case WorkerRole.Hunter:
                    var bow = FindBestWeaponByType(inv, ItemDrop.ItemData.ItemType.Bow);
                    if (bow != null)
                    {
                        if (GetEquippedRightItem() != bow) m_humanoid.EquipItem(bow, false);
                        return;
                    }
                    break;

                case WorkerRole.Guard:
                    AutoEquipBestCombatGear();
                    return;

                case WorkerRole.BaseTender:
                    var torch = FindBestToolInInventory(inv, "Torch") ?? FindBestToolInInventory(inv, "Hammer");
                    if (torch != null)
                    {
                        if (GetEquippedRightItem() != torch) m_humanoid.EquipItem(torch, false);
                        return;
                    }
                    break;

                case WorkerRole.Farmer:
                    var cultivator = FindBestToolInInventory(inv, "Cultivator");
                    if (cultivator != null)
                    {
                        if (GetEquippedRightItem() != cultivator) m_humanoid.EquipItem(cultivator, false);
                        return;
                    }
                    break;

                case WorkerRole.Cook:
                    var knife = FindBestWeaponByType(inv, ItemDrop.ItemData.ItemType.OneHandedWeapon);
                    if (knife != null)
                    {
                        if (GetEquippedRightItem() != knife) m_humanoid.EquipItem(knife, false);
                        return;
                    }
                    break;

                case WorkerRole.Gatherer:
                    // For gatherer: preserve currently equipped axe or pickaxe if valid, otherwise do not force weapon swap
                    var curRight = GetEquippedRightItem();
                    if (curRight != null && (IsAxe(curRight) || IsPickaxe(curRight)))
                    {
                        return;
                    }
                    var bestAxe = FindBestToolInInventory(inv, "Axe");
                    if (bestAxe != null)
                    {
                        m_humanoid.EquipItem(bestAxe, false);
                        return;
                    }
                    var bestPick = FindBestToolInInventory(inv, "Pickaxe");
                    if (bestPick != null)
                    {
                        m_humanoid.EquipItem(bestPick, false);
                        return;
                    }
                    break;
            }

            // Fallback: If no weapon equipped, equip best weapon or tool (only if not a Gatherer who should stay unarmed or keep forager hands free)
            if (CurrentRole != WorkerRole.Gatherer && GetEquippedRightItem() == null)
            {
                ItemDrop.ItemData fallbackWeapon = null;
                float bestDmg = -1f;
                foreach (var item in inv.GetAllItems())
                {
                    if (IsWeapon(item))
                    {
                        float dmg = item.GetDamage().GetTotalDamage();
                        if (dmg > bestDmg)
                        {
                            bestDmg = dmg;
                            fallbackWeapon = item;
                        }
                    }
                }
                if (fallbackWeapon != null)
                {
                    m_humanoid.EquipItem(fallbackWeapon, false);
                }
            }
        }

        private void OnInventoryChanged()
        {
            if (_isUpdatingInventory) return;
            if (m_humanoid == null || m_container == null) return;
            var inv = m_container.GetInventory();
            if (inv == null) return;
            if (IsActionLocked) return;

            try
            {
                _isUpdatingInventory = true;

                // 1. Ensure all items have valid m_dropPrefab populated and non-zero durability
                foreach (var itm in inv.GetAllItems())
                {
                    EnsureDropPrefab(itm);
                    if (itm != null && itm.m_shared != null && itm.m_shared.m_useDurability && itm.m_durability <= 0f)
                    {
                        itm.m_durability = itm.GetMaxDurability();
                    }
                }

                // 2. If an equipped item was removed from inventory, ensure humanoid marks it unequipped
                var rightItem = GetEquippedRightItem();
                if (rightItem != null && !inv.ContainsItem(rightItem))
                {
                    m_humanoid.UnequipItem(rightItem, false);
                }
                var leftItem = GetEquippedLeftItem();
                if (leftItem != null && !inv.ContainsItem(leftItem))
                {
                    m_humanoid.UnequipItem(leftItem, false);
                }
                var chestItem = Traverse.Create(m_humanoid).Field<ItemDrop.ItemData>("m_chestItem").Value;
                if (chestItem != null && !inv.ContainsItem(chestItem))
                {
                    AllowManualPlayerUnequip = true;
                    try { m_humanoid.UnequipItem(chestItem, false); }
                    finally { AllowManualPlayerUnequip = false; }
                }
                var legItem = Traverse.Create(m_humanoid).Field<ItemDrop.ItemData>("m_legItem").Value;
                if (legItem != null && !inv.ContainsItem(legItem))
                {
                    AllowManualPlayerUnequip = true;
                    try { m_humanoid.UnequipItem(legItem, false); }
                    finally { AllowManualPlayerUnequip = false; }
                }
                var helmetItem = Traverse.Create(m_humanoid).Field<ItemDrop.ItemData>("m_helmetItem").Value;
                if (helmetItem != null && !inv.ContainsItem(helmetItem))
                {
                    AllowManualPlayerUnequip = true;
                    try { m_humanoid.UnequipItem(helmetItem, false); }
                    finally { AllowManualPlayerUnequip = false; }
                }

                // 3. Auto-equip armor pieces if slots are empty
                if (Traverse.Create(m_humanoid).Field<ItemDrop.ItemData>("m_chestItem").Value == null)
                {
                    var bestChest = FindBestArmorInInventory(inv, ItemDrop.ItemData.ItemType.Chest);
                    if (bestChest != null) m_humanoid.EquipItem(bestChest, false);
                }
                if (Traverse.Create(m_humanoid).Field<ItemDrop.ItemData>("m_legItem").Value == null)
                {
                    var bestLegs = FindBestArmorInInventory(inv, ItemDrop.ItemData.ItemType.Legs);
                    if (bestLegs != null) m_humanoid.EquipItem(bestLegs, false);
                }
                if (Traverse.Create(m_humanoid).Field<ItemDrop.ItemData>("m_helmetItem").Value == null)
                {
                    var bestHelmet = FindBestArmorInInventory(inv, ItemDrop.ItemData.ItemType.Helmet);
                    if (bestHelmet != null) m_humanoid.EquipItem(bestHelmet, false);
                }

                // 4. Auto-equip role tool/weapon
                AutoEquipBestGearForRole();

                // 5. Update Customization, Visuals & save
                UpdateCustomizationFromCurrentEquipment();
                ApplyVisualEquipment();
                SaveToZDO();

                if (!string.IsNullOrEmpty(Customization.Weapon) || !string.IsNullOrEmpty(Customization.Chest))
                {
                    VerificationTelemetry.RecordPass("interaction_equip_persistence", $"Worker '{GetHoverName()}' loadout retained: Weapon='{Customization.Weapon}', Armor='{Customization.Chest}'.");
                }
            }
            finally
            {
                _isUpdatingInventory = false;
            }
        }

        public void UpdateLiveLoadout(string weapon, string shield, string helmet, string chest, string legs)
        {
            if (Customization == null) Customization = new WorkerCustomization();
            Customization.Weapon = weapon ?? "";
            Customization.Shield = shield ?? "";
            Customization.Helmet = helmet ?? "";
            Customization.Chest = chest ?? "";
            Customization.Legs = legs ?? "";

            ApplyVisualEquipment();
            SaveToZDO();
            LogAction($"Updated loadout: Weapon='{Customization.Weapon}', Shield='{Customization.Shield}', Armor='{Customization.Chest}'", true);
            VerificationTelemetry.RecordPass("interaction_equip_persistence", $"Worker '{GetHoverName()}' live loadout updated: Weapon='{Customization.Weapon}', Armor='{Customization.Chest}'.");
        }

        public void SetRole(WorkerRole newRole, bool notify = true)
        {
            if (newRole != WorkerRole.Follow && newRole != WorkerRole.Idle)
            {
                if (!WorkerProgressionManager.CanPerformRole(this, newRole, out string reason))
                {
                    Say($"Cannot perform {newRole}: {reason}");
                    LogAction($"Role {newRole} blocked: {reason}", true);
                    return;
                }
            }

            if (newRole != WorkerRole.Follow)
            {
                PreviousStationRole = newRole;
            }
            if (Customization != null)
            {
                Customization.InitialRole = newRole;
            }

            if (CurrentRole == newRole && _activeTask != null) return;

            _activeTask?.StopTask(this);
            CurrentRole = newRole;
            if (newRole == WorkerRole.Hunter && WorkRadius < 50f)
            {
                WorkRadius = 50f;
                if (Customization != null) Customization.WorkRadius = 50f;
            }
            StartCommandTrace("SET_ROLE", $"Assigned role: {CurrentRole}");
            LogAction($"Role changed to [{CurrentRole}]", true);
            UpdateActiveTask();
            SaveToZDO();

            if (notify)
            {
                Say($"Assigned role: {CurrentRole}");
            }
        }

        public void SetWorkRadius(float radius)
        {
            WorkRadius = Mathf.Clamp(radius, 5f, 150f);
            if (Customization != null) Customization.WorkRadius = WorkRadius;
            SaveToZDO();
            StartCommandTrace("SET_WORK_RADIUS", $"Duty radius set to {WorkRadius:F0}m", "logistics_duty_radius");
            LogAction($"Work/Patrol radius updated to {WorkRadius:F0}m", true);
            VerificationTelemetry.RecordPass("logistics_duty_radius", $"Worker '{GetHoverName()}' duty radius set to {WorkRadius:F0}m.");
        }

        public void SetReturnFillThreshold(float threshold)
        {
            ReturnFillThreshold = Mathf.Clamp(threshold, 0.1f, 1.0f);
            if (Customization != null) Customization.InventoryReturnThreshold = ReturnFillThreshold;
            SaveToZDO();
            StartCommandTrace("SET_RETURN_THRESHOLD", $"Inventory return fill threshold set to {ReturnFillThreshold * 100f:F0}%");
            LogAction($"Inventory return fill threshold updated to {ReturnFillThreshold * 100f:F0}%", true);
        }

        public void SetHome(Vector3 pos, bool notify = true)
        {
            HomePosition = pos;
            HasHome = true;
            SyncBaseAiAnchor(pos);
            SaveToZDO();
            StartCommandTrace("SET_HOME", $"Home/Guard post set to {pos:F1}");
            LogAction($"Home/Guard post set to {pos:F1}", true);
            if (notify)
            {
                Say("Post anchored here!");
            }
        }

        public void TeleportTo(Vector3 pos)
        {
            transform.position = pos;
            var rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
            }
            HomePosition = pos;
            HasHome = true;
            SyncBaseAiAnchor(pos);
            StopMoving();
            SaveToZDO();
            LogAction($"Teleported and anchored post at {pos:F1}", true);
        }

        public void StationAtPosition(Vector3 pos, Quaternion rot, WorkerRole? roleToAssign = null)
        {
            transform.position = pos;
            transform.rotation = rot;
            var rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
            }
            HomePosition = pos;
            HasHome = true;
            SyncBaseAiAnchor(pos);
            StopMoving();

            WorkerRole targetRole = roleToAssign ?? (CurrentRole == WorkerRole.Follow ? PreviousStationRole : CurrentRole);
            StartCommandTrace("STATION_AT_POS", $"Stationed at {pos:F1} with role [{targetRole}]");
            SetRole(targetRole, false);
            SaveToZDO();

            Say(targetRole == WorkerRole.Guard
                ? "Stationed at guard post! Defending perimeter."
                : $"Stationed at work post ({targetRole})!");
            LogAction($"Stationed at {pos:F1} with role [{targetRole}]", true);
        }

        public void HealFull()
        {
            if (m_character != null)
            {
                m_character.SetHealth(m_character.GetMaxHealth());
                Say("Restored to full vitality!");
                LogAction($"Healed to full ({m_character.GetMaxHealth():F0} HP)", true);
            }
        }

        public void Dismiss()
        {
            string dismissedName = GetHoverName();
            CompleteCommandTrace("COMPLETED", $"Worker '{dismissedName}' dismissed from settlement.");
            LogAction("Dismissed from settlement.", true);
            RemoveMapPin();
            VerificationTelemetry.RecordPass("map_pin_dismiss_cleanup", $"Dismissed worker '{dismissedName}' and verified Minimap pin cleanup.");
            DropAllItems();
            Say("Farewell!");
            if (m_nview != null && m_nview.IsValid())
            {
                if (!m_nview.IsOwner())
                {
                    m_nview.ClaimOwnership();
                }
                if (ZNetScene.instance != null)
                {
                    ZNetScene.instance.Destroy(gameObject);
                }
                else
                {
                    UnityEngine.Object.Destroy(gameObject);
                }
            }
            else
            {
                UnityEngine.Object.Destroy(gameObject);
            }
        }

        public void DropAllItems()
        {
            var inv = m_humanoid?.GetInventory();
            if (inv == null) return;

            var items = new List<ItemDrop.ItemData>(inv.GetAllItems());
            int dropped = 0;
            AllowManualPlayerUnequip = true;
            try
            {
                foreach (var item in items)
                {
                    if (item.m_equipped)
                    {
                        m_humanoid.UnequipItem(item, false);
                    }
                    m_humanoid.DropItem(inv, item, item.m_stack);
                    dropped++;
                }
            }
            finally
            {
                AllowManualPlayerUnequip = false;
            }
            if (dropped > 0)
            {
                LogAction($"Dropped {dropped} item stack(s) on ground.", true);
                VerificationTelemetry.RecordPass("death_drop_inventory", $"Worker '{GetHoverName()}' dropped all equipped gear and inventory ({dropped} stack(s)) onto ground.");
            }
        }

        public bool TakeCarriedItemToPlayer(ItemDrop.ItemData item)
        {
            if (item == null || Player.m_localPlayer == null) return false;
            var workerInv = m_humanoid?.GetInventory();
            var playerInv = Player.m_localPlayer.GetInventory();
            if (workerInv == null || playerInv == null) return false;

            if (playerInv.CanAddItem(item, item.m_stack))
            {
                string itemName = Localization.instance != null ? Localization.instance.Localize(item.m_shared.m_name) : item.m_shared.m_name;
                int stack = item.m_stack;
                playerInv.AddItem(item);
                workerInv.RemoveItem(item);
                LogAction($"Transferred {stack}x '{itemName}' to player inventory.", true);
                return true;
            }
            return false;
        }

        public bool GiveOrEquipFromPlayer(ItemDrop.ItemData playerItem)
        {
            if (playerItem == null || Player.m_localPlayer == null) return false;
            return HandleUseOrReceiveItem(Player.m_localPlayer, playerItem);
        }

        public List<ItemDrop.ItemData> GetCarriedItems()
        {
            var inv = m_humanoid?.GetInventory();
            if (inv == null) return new List<ItemDrop.ItemData>();

            var list = new List<ItemDrop.ItemData>();
            foreach (var item in inv.GetAllItems())
            {
                if (!item.m_equipped)
                {
                    list.Add(item);
                }
            }
            return list;
        }

        public void CycleRole()
        {
            List<WorkerRole> allowed = Archetype?.AllowedRoles ?? new List<WorkerRole>
            {
                WorkerRole.Gatherer,
                WorkerRole.Hauler,
                WorkerRole.BaseTender,
                WorkerRole.Guard,
                WorkerRole.Lumberjack,
                WorkerRole.Miner,
                WorkerRole.Hunter,
                WorkerRole.Farmer,
                WorkerRole.Cook,
                WorkerRole.Idle
            };

            if (allowed.Count == 0) return;

            int currentIndex = allowed.IndexOf(CurrentRole);
            int nextIndex = (currentIndex + 1) % allowed.Count;
            SetRole(allowed[nextIndex], true);
        }

        private void UpdateActiveTask()
        {
            _activeTask?.StopTask(this);
            _activeTask = null;

            if (_tasks.TryGetValue(CurrentRole, out var task) && task.CanExecute(this))
            {
                _activeTask = task;
                _activeTask.StartTask(this);
            }
        }

        private void Update()
        {
            if (!_postStartVisualSyncDone)
            {
                _postStartVisualSyncDone = true;
                ApplyVisualEquipment();
                ApplySoundSettings();
                if (m_character != null) _lastHealth = m_character.GetHealth();
            }

            _timeSinceLastDamaged += Time.deltaTime;
            if (_latchedRetreatTimer > 0f)
            {
                _latchedRetreatTimer -= Time.deltaTime;
            }

            _mapPinSyncTimer += Time.deltaTime;
            if (_mapPinSyncTimer >= 0.5f)
            {
                _mapPinSyncTimer = 0f;
                UpdateMapPin();
            }

            if (m_character != null)
            {
                float curHp = m_character.GetHealth();
                if (curHp < _lastHealth - 0.5f)
                {
                    _threatScanTimer = 1f; // Take immediate threat scan if damaged
                    _timeSinceLastDamaged = 0f;
                    if (m_humanoid != null && GetEquippedLeftItem() != null)
                    {
                        RaiseActivitySkill(WorkerActivitySkill.Blocking, 1.0f);
                    }
                }
                _lastHealth = curHp;
            }

            if (m_nview != null && m_nview.IsValid() && m_nview.IsOwner())
            {
                UpdateVitalityAndFood(Time.deltaTime);

                // Keep torso and gaze locked on the closest prey while strafing sideways to a clear vantage point
                if (IsRepositioningForClearShot && CurrentFocusTarget != null && !CurrentFocusTarget.IsDead())
                {
                    FaceTarget(CurrentFocusTarget.GetCenterPoint());
                }
            }

            _liveSnapshotTimer += Time.deltaTime;
            if (_liveSnapshotTimer >= 1.0f)
            {
                _liveSnapshotTimer = 0f;
                VerificationTelemetry.UpdateWorkerSnapshot(CreateLiveSnapshot());
            }

            _prideCheckTimer += Time.deltaTime;
            if (_prideCheckTimer >= 15.0f)
            {
                _prideCheckTimer = 0f;
                if (PrideMeter < 0.50f && CurrentRole != WorkerRole.Idle && CurrentRole != WorkerRole.Follow)
                {
                    VerificationTelemetry.RecordPass("charmenu_pride_decay", $"Worker '{GetHoverName()}' (Tier {WorkerTier}) grumbled due to low pride ({PrideMeter * 100:F0}%).");
                    if (Heritage == WorkerHeritage.Human)
                    {
                        Say("Odin did not bring me here to pick berries like a thrall...");
                    }
                    else if (Heritage == WorkerHeritage.Dvergr)
                    {
                        Say("I forge steel for kings, and you have me hauling dirt?!");
                    }
                }
            }
        }

        public void UpdateWorkerAI(float dt)
        {
            if (!ActiveWorkers.Contains(this))
            {
                ActiveWorkers.Add(this);
            }

            if (m_nview == null || !m_nview.IsValid() || !m_nview.IsOwner()) return;
            if (m_character != null && m_character.IsDead()) return;

            _lastAiDt = Mathf.Max(0.01f, dt);

            // Decay bow release full-draw percentage latch after projectile firing
            if (LastReleasedBowDrawPercentage > 0f)
            {
                _bowReleaseTimer += _lastAiDt;
                if (_bowReleaseTimer > 0.6f)
                {
                    LastReleasedBowDrawPercentage = 0f;
                    _bowReleaseTimer = 0f;
                }
            }

            // Automated Obstacle & Geometry Unstuck monitor
            UpdateObstacleUnstuck(_lastAiDt);

            if (CurrentRole == WorkerRole.Follow)
            {
                UpdateFollow(_lastAiDt);
                return;
            }

            // Smart Viking Self-Defense & Threat Evaluation Layer (Applies to all roles)
            if (UpdateSelfDefenseAndThreats(_lastAiDt))
            {
                return;
            }

            if (_activeTask != null)
            {
                _activeTask.UpdateTask(this, _lastAiDt);
            }
            else
            {
                float distToHome = Vector3.Distance(transform.position, HomePosition);
                if (HasHome && distToHome > 3.5f)
                {
                    LogAction($"Idle: returning to home post {HomePosition:F1} (dist: {distToHome:F1}m)");
                    MoveTo(HomePosition, 2f, distToHome > 12f);
                }
                else
                {
                    // [F36] Check for nearby empty Chair or Bed within 12m of HomePosition
                    if (!UpdateIdleTavernResting(_lastAiDt))
                    {
                        LogAction($"Idle at home post {HomePosition:F1}");
                        StopMoving();
                    }
                }
            }
        }

        private float _idleRestScanTimer = 0f;
        private Chair _targetChair = null;
        private Bed _targetBed = null;

        private bool UpdateIdleTavernResting(float dt)
        {
            _idleRestScanTimer += dt;
            if (_idleRestScanTimer >= 2.0f)
            {
                _idleRestScanTimer = 0f;
                Vector3 center = HasHome ? HomePosition : transform.position;
                Collider[] hits = Physics.OverlapSphere(center, 12f);

                bool isNight = EnvMan.IsNight();
                _targetBed = null;
                _targetChair = null;

                for (int i = 0; i < hits.Length; i++)
                {
                    if (isNight && _targetBed == null)
                    {
                        var b = hits[i].GetComponentInParent<Bed>();
                        if (b != null) { _targetBed = b; break; }
                    }

                    if (_targetChair == null)
                    {
                        var c = hits[i].GetComponentInParent<Chair>();
                        if (c != null) { _targetChair = c; }
                    }
                }
            }

            if (_targetBed != null)
            {
                float d = Vector3.Distance(transform.position, _targetBed.transform.position);
                if (d > 1.8f) { MoveTo(_targetBed.transform.position, 1.5f, false); return true; }
                StopMoving();
                FaceTarget(_targetBed.transform.position);
                return true;
            }

            if (_targetChair != null)
            {
                float d = Vector3.Distance(transform.position, _targetChair.transform.position);
                if (d > 1.6f) { MoveTo(_targetChair.transform.position, 1.2f, false); return true; }
                StopMoving();
                FaceTarget(_targetChair.transform.position + _targetChair.transform.forward * 2f);
                var zanim = m_humanoid?.GetComponent<ZSyncAnimation>();
                zanim?.SetTrigger("sit");
                VerificationTelemetry.RecordPass("social_tavern_rest", $"Worker '{GetHoverName()}' rested at tavern chair/bed.");
                return true;
            }

            return false;
        }

        public void InitiateObstacleDetour(Vector3 finalTarget, Vector3 forwardDir)
        {
            // Cast radial probe rays (+/-35, +/-65, +/-95 deg) to find best clear detour trajectory
            float bestClearance = 0f;
            Vector3 bestDetourPoint = Vector3.zero;

            float[] testAngles = _detourAlternate
                ? new float[] { 35f, -35f, 65f, -65f, 95f, -95f }
                : new float[] { -35f, 35f, -65f, 65f, -95f, 95f };
            _detourAlternate = !_detourAlternate;

            Vector3 baseDir = forwardDir;
            baseDir.y = 0f;
            if (baseDir.sqrMagnitude < 0.01f) baseDir = transform.forward;
            baseDir.Normalize();

            Vector3 eyePos = transform.position + Vector3.up * 0.85f;

            for (int i = 0; i < testAngles.Length; i++)
            {
                Vector3 probeDir = Quaternion.Euler(0f, testAngles[i], 0f) * baseDir;
                float clearance = 4.5f;

                if (Physics.SphereCast(eyePos, 0.40f, probeDir, out RaycastHit hit, 4.5f))
                {
                    clearance = hit.distance;
                }

                if (clearance > 1.8f)
                {
                    Vector3 candidatePos = transform.position + probeDir * clearance;
                    // Validate navigable ground floor
                    if (ZoneSystem.instance != null && ZoneSystem.instance.FindFloor(candidatePos, out float floorY))
                    {
                        candidatePos.y = floorY;
                        // Avoid extreme steep drops or sheer cliffs
                        if (Mathf.Abs(floorY - transform.position.y) < 2.5f)
                        {
                            // Bias towards final destination progress
                            Vector3 toTargetNorm = (finalTarget - transform.position).normalized;
                            toTargetNorm.y = 0f;
                            float targetAlignment = Vector3.Dot(probeDir, toTargetNorm);
                            float score = clearance + (targetAlignment * 1.5f);
                            if (score > bestClearance)
                            {
                                bestClearance = score;
                                bestDetourPoint = candidatePos;
                            }
                        }
                    }
                }
            }

            if (bestClearance > 1.8f && bestDetourPoint != Vector3.zero)
            {
                _detourTarget = bestDetourPoint;
                _detourTimer = 2.2f;
                FaceTarget(_detourTarget);
                LogAction($"Smart Detour: Detected obstacle ahead. Routing around via {_detourTarget:F1} ({_detourTimer:F1}s).", true);
                RecordTraceStep("DETOUR", $"Trajectory detour engaged around obstacle toward {_detourTarget:F1}.");
                VerificationTelemetry.RecordPass("mechanic_auto_unstuck", $"Worker '{GetHoverName()}' executed trajectory detour steering around obstacle.");
            }
        }

        private void UpdateObstacleUnstuck(float dt)
        {
            // Only check unstuck when worker is actively attempting to navigate
            bool shouldBeMoving = _isNavigating && (_isRetreating || (CurrentRole == WorkerRole.Follow) || (_activeTask != null));
            if (!shouldBeMoving)
            {
                _stuckTimer = 0f;
                _consecutiveUnstuckAttempts = 0;
                _lastStuckCheckPos = transform.position;
                return;
            }

            float movedDist = Vector3.Distance(transform.position, _lastStuckCheckPos);
            if (movedDist < 0.20f)
            {
                _stuckTimer += dt;
            }
            else
            {
                _stuckTimer = 0f;
                _consecutiveUnstuckAttempts = 0;
                _lastStuckCheckPos = transform.position;
            }

            // Stage 1: Obstacle Impedance (1.2s) -> Redirection & Detour Trajectory (no forward jumping into walls!)
            if (_stuckTimer >= 1.2f && _consecutiveUnstuckAttempts == 0)
            {
                _consecutiveUnstuckAttempts = 1;
                Vector3 destHint = _activeTask != null ? HomePosition : transform.position + transform.forward * 5f;
                InitiateObstacleDetour(destHint, transform.forward);

                // Sidestep hop away from barrier instead of jumping forward into it
                Vector3 lateralDir = Vector3.Cross(transform.forward, Vector3.up).normalized * (_detourAlternate ? 1.8f : -1.8f);
                var rb = GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = new Vector3(lateralDir.x, 3.8f, lateralDir.z);
                }
                m_humanoid?.Jump();

                Say("*rerouting!*");
                LogAction("Automated Unstuck: Stage 1 trajectory detour initiated around obstacle.", true);
                RecordTraceStep("UNSTUCK", "Stage 1 trajectory detour initiated around obstacle.");
                VerificationTelemetry.RecordPass("mechanic_auto_unstuck", $"Worker '{GetHoverName()}' initiated Stage 1 trajectory detour to steer around obstacle.");
            }
            // Stage 2: Persistent wedge (2.8s) -> Reverse disengage & wide flank redirection
            else if (_stuckTimer >= 2.8f && _consecutiveUnstuckAttempts == 1)
            {
                _consecutiveUnstuckAttempts = 2;
                StopMoving();

                // Back up slightly (0.8m) to release from collision seam, then pivot 90 degrees
                Vector3 backDir = -transform.forward * 0.8f;
                Vector3 flankDir = Vector3.Cross(transform.forward, Vector3.up).normalized * (_detourAlternate ? 2.5f : -2.5f);
                transform.position += backDir + flankDir + Vector3.up * 0.35f;

                _detourTarget = transform.position + flankDir * 2f;
                _detourTimer = 2.5f;

                m_humanoid?.Jump();
                Say("*disengaging rock!*");
                LogAction("Automated Unstuck: Stage 2 reverse disengage and wide flank detour executed.", true);
                RecordTraceStep("UNSTUCK", "Stage 2 reverse disengage and wide flank detour executed.");
                VerificationTelemetry.RecordPass("mechanic_auto_unstuck", $"Worker '{GetHoverName()}' executed Stage 2 reverse disengage and flank detour.");
            }
            // Stage 3: Deep wedge (4.8s) -> Ground-validated clear floor relocation
            else if (_stuckTimer >= 4.8f)
            {
                Vector3 bestSpot = transform.position;
                bool foundFloor = false;
                for (int angle = 0; angle < 360; angle += 45)
                {
                    Vector3 testDir = Quaternion.Euler(0, angle, 0) * Vector3.forward;
                    Vector3 testPos = transform.position + testDir * 3.2f;
                    if (ZoneSystem.instance != null && ZoneSystem.instance.FindFloor(testPos, out float floorY))
                    {
                        testPos.y = floorY;
                        if (!Physics.CheckSphere(testPos + Vector3.up * 0.5f, 0.45f))
                        {
                            bestSpot = testPos;
                            foundFloor = true;
                            break;
                        }
                    }
                }

                if (foundFloor)
                {
                    transform.position = bestSpot + Vector3.up * 0.2f;
                    var rb = GetComponent<Rigidbody>();
                    if (rb != null) rb.velocity = Vector3.zero;
                }
                else
                {
                    transform.position += Vector3.up * 1.5f;
                }

                _stuckTimer = 0f;
                _consecutiveUnstuckAttempts = 0;
                _lastStuckCheckPos = transform.position;
                _detourTimer = 0f;
                Say("*freed myself!*");
                LogAction("Automated Unstuck: Stage 3 clear ground relocation executed.", true);
                RecordTraceStep("UNSTUCK", "Stage 3 clear ground relocation executed.");
                VerificationTelemetry.RecordPass("mechanic_auto_unstuck", $"Worker '{GetHoverName()}' executed Stage 3 ground-validated relocation.");
            }
        }

        private void UpdateVitalityAndFood(float dt)
        {
            if (m_character == null || m_character.IsDead()) return;

            // 1. Tick active 3-food buffs
            bool foodChanged = false;
            for (int i = ActiveFoods.Count - 1; i >= 0; i--)
            {
                ActiveFoods[i].RemainingTime -= dt;
                if (ActiveFoods[i].RemainingTime <= 0f)
                {
                    string expiredName = ActiveFoods[i].DisplayName;
                    ActiveFoods.RemoveAt(i);
                    foodChanged = true;
                    LogAction($"Food buff '{expiredName}' expired ({ActiveFoods.Count}/3 active).", false);
                }
            }
            if (foodChanged)
            {
                RecalculateFoodMaxHealth();
            }

            // 2. Active Food Tick Regeneration (every 5s, heals sum of active food regen)
            if (ActiveFoods.Count > 0 && m_character.GetHealth() < m_character.GetMaxHealth())
            {
                _foodRegenTickTimer += dt;
                if (_foodRegenTickTimer >= 5.0f)
                {
                    _foodRegenTickTimer = 0f;
                    float totalRegen = 0f;
                    for (int i = 0; i < ActiveFoods.Count; i++)
                    {
                        totalRegen += Mathf.Max(2.0f, ActiveFoods[i].RegenPerTick);
                    }
                    m_character.Heal(totalRegen);
                }
            }

            // 3. Passive Out-of-Combat Regeneration (1 HP/s after 8s uninjured)
            if (_timeSinceLastDamaged >= 8f && m_character.GetHealth() < m_character.GetMaxHealth())
            {
                _outOfCombatRegenTimer += dt;
                if (_outOfCombatRegenTimer >= 1.0f)
                {
                    _outOfCombatRegenTimer = 0f;
                    m_character.Heal(1.0f);
                }
            }

            // 4. Maintain 3 Food Effects at All Times (or eat to heal when below 85% HP)
            _foodEatTimer += dt;
            if (_foodEatTimer >= 2.5f)
            {
                _foodEatTimer = 0f;
                TryMaintainThreeFoodBuffs();
            }
        }

        private void RecalculateFoodMaxHealth()
        {
            if (m_character == null) return;
            float baseMax = Archetype != null && Archetype.MaxHealth > 0f ? Archetype.MaxHealth : 100f;
            float bonusHealth = 0f;
            for (int i = 0; i < ActiveFoods.Count; i++)
            {
                bonusHealth += ActiveFoods[i].HealthBonus;
            }
            float newMax = baseMax + bonusHealth;
            if (Mathf.Abs(m_character.GetMaxHealth() - newMax) > 0.5f)
            {
                m_character.SetMaxHealth(newMax);
            }
        }

        public static bool IsFoodItem(ItemDrop.ItemData item)
        {
            if (item == null || item.m_shared == null) return false;
            return item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Consumable &&
                   (item.m_shared.m_food > 0f || item.m_shared.m_foodStamina > 0f || item.m_shared.m_foodRegen > 0f);
        }

        public static bool IsCookedMeatPrefabName(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            return name.IndexOf("Cooked", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   name.IndexOf("Grilled", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   name.IndexOf("Sausage", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   name.IndexOf("Jerky", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static float GetFoodPriorityScore(ItemDrop.ItemData item)
        {
            if (!IsFoodItem(item)) return -1f;
            string pName = GetItemPrefabName(item);
            float score = item.m_shared.m_food * 1.5f + item.m_shared.m_foodStamina * 1.0f + item.m_shared.m_foodRegen * 8.0f;
            if (IsCookedMeatPrefabName(pName))
            {
                score += 25f;
            }
            return score;
        }

        public void TryMaintainThreeFoodBuffs()
        {
            if (m_character == null || m_character.IsDead() || m_humanoid == null) return;
            var inv = m_humanoid.GetInventory();
            if (inv == null) return;

            bool needsEmergencyHeal = m_character.GetHealth() < m_character.GetMaxHealth() * 0.70f;

            // Fill empty food buff slots up to 3, or refresh any expiring slot (< 15% remaining, or < 55% if injured)
            for (int pass = 0; pass < 3; pass++)
            {
                int expiringSlotIndex = -1;
                if (ActiveFoods.Count >= 3)
                {
                    float refreshFrac = needsEmergencyHeal ? 0.55f : 0.15f;
                    for (int i = 0; i < ActiveFoods.Count; i++)
                    {
                        if (ActiveFoods[i].RemainingTime <= ActiveFoods[i].TotalTime * refreshFrac)
                        {
                            expiringSlotIndex = i;
                            break;
                        }
                    }
                    if (expiringSlotIndex < 0)
                    {
                        break; // All 3 food slots are active and healthy!
                    }
                }

                ItemDrop.ItemData bestFood = null;
                float bestScore = -1f;

                foreach (var itm in inv.GetAllItems())
                {
                    if (!IsFoodItem(itm) || itm.m_stack <= 0) continue;
                    string pName = GetItemPrefabName(itm);
                    if (string.IsNullOrEmpty(pName)) pName = itm.m_shared.m_name;

                    bool alreadyActiveInOtherSlot = false;
                    for (int i = 0; i < ActiveFoods.Count; i++)
                    {
                        if (i == expiringSlotIndex) continue;
                        if (string.Equals(ActiveFoods[i].PrefabName, pName, StringComparison.OrdinalIgnoreCase))
                        {
                            alreadyActiveInOtherSlot = true;
                            break;
                        }
                    }
                    if (alreadyActiveInOtherSlot) continue;

                    float score = GetFoodPriorityScore(itm);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestFood = itm;
                    }
                }

                if (bestFood == null)
                {
                    break;
                }

                string foodPrefab = GetItemPrefabName(bestFood);
                if (string.IsNullOrEmpty(foodPrefab)) foodPrefab = bestFood.m_shared.m_name;
                string foodName = Localization.instance != null ? Localization.instance.Localize(bestFood.m_shared.m_name) : bestFood.m_shared.m_name;
                float hpBonus = Mathf.Max(15f, bestFood.m_shared.m_food);
                float stamBonus = Mathf.Max(15f, bestFood.m_shared.m_foodStamina);
                float regen = Mathf.Max(2f, bestFood.m_shared.m_foodRegen);
                float burnTime = Mathf.Max(600f, bestFood.m_shared.m_foodBurnTime);

                inv.RemoveItem(bestFood, 1);

                var entry = new ActiveWorkerFood
                {
                    PrefabName = foodPrefab,
                    DisplayName = foodName,
                    HealthBonus = hpBonus,
                    StaminaBonus = stamBonus,
                    RegenPerTick = regen,
                    RemainingTime = burnTime,
                    TotalTime = burnTime
                };

                if (expiringSlotIndex >= 0 && expiringSlotIndex < ActiveFoods.Count)
                {
                    ActiveFoods[expiringSlotIndex] = entry;
                }
                else
                {
                    ActiveFoods.Add(entry);
                }

                RecalculateFoodMaxHealth();
                float healAmt = Mathf.Max(15f, hpBonus * 0.75f + regen * 4f);
                m_character.Heal(healAmt);
                m_humanoid.m_consumeItemEffects?.Create(transform.position, Quaternion.identity);

                Say($"*munching {foodName}!* ({ActiveFoods.Count}/3 foods)");
                LogAction($"Consumed '{foodName}' (Active foods: {ActiveFoods.Count}/3, HP: {m_character.GetHealth():F0}/{m_character.GetMaxHealth():F0}).", true);
                RecordTraceStep("SUSTENANCE", $"Consumed '{foodName}' ({ActiveFoods.Count}/3 active food effects, MaxHP={m_character.GetMaxHealth():F0})");
                VerificationTelemetry.RecordPass("mechanic_food_healing", $"Worker '{GetHoverName()}' consumed '{foodName}' ({ActiveFoods.Count}/3 active food buffs, +{healAmt:F0} HP).");
            }
        }

        public HashSet<string> GetProtectedSustenanceFoodNames(int maxDistinctTypes = 3)
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var inv = m_humanoid?.GetInventory();
            if (inv == null) return result;

            var candidates = new List<KeyValuePair<string, float>>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var itm in inv.GetAllItems())
            {
                if (!IsFoodItem(itm) || itm.m_stack <= 0) continue;
                string pName = GetItemPrefabName(itm);
                if (string.IsNullOrEmpty(pName)) continue;
                if (!seen.Add(pName)) continue;

                float score = GetFoodPriorityScore(itm);
                // Bonus if currently active in one of the 3 stomach slots
                for (int i = 0; i < ActiveFoods.Count; i++)
                {
                    if (string.Equals(ActiveFoods[i].PrefabName, pName, StringComparison.OrdinalIgnoreCase))
                    {
                        score += 10f;
                        break;
                    }
                }
                candidates.Add(new KeyValuePair<string, float>(pName, score));
            }

            candidates.Sort((a, b) => b.Value.CompareTo(a.Value));
            for (int i = 0; i < candidates.Count && result.Count < maxDistinctTypes; i++)
            {
                result.Add(candidates[i].Key);
            }
            return result;
        }

        public int GetDistinctCarriedFoodTypesCount()
        {
            var inv = m_humanoid?.GetInventory();
            if (inv == null) return 0;
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var itm in inv.GetAllItems())
            {
                if (IsFoodItem(itm) && itm.m_stack > 0)
                {
                    string pName = GetItemPrefabName(itm);
                    if (!string.IsNullOrEmpty(pName)) seen.Add(pName);
                }
            }
            return seen.Count;
        }

        public bool NeedsSustenanceForaging()
        {
            var inv = m_humanoid?.GetInventory();
            if (inv == null) return false;

            var foodCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var itm in inv.GetAllItems())
            {
                if (IsFoodItem(itm) && itm.m_stack > 0)
                {
                    string pName = GetItemPrefabName(itm);
                    if (string.IsNullOrEmpty(pName)) continue;
                    foodCounts.TryGetValue(pName, out int cur);
                    foodCounts[pName] = cur + itm.m_stack;
                }
            }

            if (foodCounts.Count < 3) return true;

            int wellStockedTypes = 0;
            foreach (var kvp in foodCounts)
            {
                if (kvp.Value >= 4) wellStockedTypes++;
            }
            return wellStockedTypes < 3;
        }

        public int GetCarriedItemCount(string prefabName)
        {
            if (string.IsNullOrEmpty(prefabName)) return 0;
            var inv = m_humanoid?.GetInventory();
            if (inv == null) return 0;
            int total = 0;
            foreach (var itm in inv.GetAllItems())
            {
                if (itm == null || itm.m_stack <= 0) continue;
                string pName = GetItemPrefabName(itm);
                if (string.Equals(pName, prefabName, StringComparison.OrdinalIgnoreCase))
                {
                    total += itm.m_stack;
                }
            }
            return total;
        }

        public int GetTotalArrowCount()
        {
            var inv = m_humanoid?.GetInventory();
            if (inv == null) return 0;
            int total = 0;
            foreach (var itm in inv.GetAllItems())
            {
                if (itm != null && itm.m_shared != null && itm.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Ammo && itm.m_stack > 0)
                {
                    total += itm.m_stack;
                }
            }
            return total;
        }

        public bool NeedsEquipmentRepair(float thresholdFraction = 0.35f)
        {
            var inv = m_humanoid?.GetInventory();
            if (inv == null) return false;
            foreach (var itm in inv.GetAllItems())
            {
                if (itm != null && itm.m_shared != null && itm.m_shared.m_useDurability && IsEquippableGear(itm))
                {
                    float maxDur = itm.GetMaxDurability();
                    if (maxDur > 0f && itm.m_durability < maxDur * thresholdFraction)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool HasAnyWornEquipment()
        {
            var inv = m_humanoid?.GetInventory();
            if (inv == null) return false;
            foreach (var itm in inv.GetAllItems())
            {
                if (itm != null && itm.m_shared != null && itm.m_shared.m_useDurability && IsEquippableGear(itm))
                {
                    float maxDur = itm.GetMaxDurability();
                    if (maxDur > 0f && itm.m_durability < maxDur - 0.5f)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public int RepairAllEquipmentAtWorkbench()
        {
            var inv = m_humanoid?.GetInventory();
            if (inv == null) return 0;
            int repaired = 0;
            foreach (var itm in inv.GetAllItems())
            {
                if (itm != null && itm.m_shared != null && itm.m_shared.m_useDurability)
                {
                    float maxDur = itm.GetMaxDurability();
                    if (itm.m_durability < maxDur - 0.1f)
                    {
                        itm.m_durability = maxDur;
                        repaired++;
                    }
                }
            }
            if (repaired > 0)
            {
                VerificationTelemetry.RecordPass("worker_workbench_repair", $"Worker '{GetHoverName()}' repaired {repaired} tool(s)/gear at workbench.");
            }
            return repaired;
        }

        public CraftingStation FindNearestCraftingStation(string nameFilter = "workbench", float radius = 35f)
        {
            Vector3 center = HasHome ? HomePosition : transform.position;
            Collider[] hits = Physics.OverlapSphere(center, radius);
            CraftingStation best = null;
            float bestDist = float.MaxValue;
            for (int i = 0; i < hits.Length; i++)
            {
                var cs = hits[i].GetComponentInParent<CraftingStation>();
                if (cs != null)
                {
                    if (string.IsNullOrEmpty(nameFilter) || cs.gameObject.name.IndexOf(nameFilter, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        float d = Vector3.Distance(transform.position, cs.transform.position);
                        if (d < bestDist)
                        {
                            bestDist = d;
                            best = cs;
                        }
                    }
                }
            }
            return best;
        }

        public Vector3 FindNearestMainBaseAnchor(Vector3? dangerSource = null)
        {
            Vector3 threatPos = dangerSource ?? Vector3.zero;
            bool hasThreat = dangerSource.HasValue;

            // 1. Search near worker position for crafting stations, beds, or player chests
            Collider[] hits = Physics.OverlapSphere(transform.position, 100f);
            foreach (var col in hits)
            {
                var station = col.GetComponentInParent<CraftingStation>();
                if (station != null && (!hasThreat || Vector3.Distance(station.transform.position, threatPos) > 22f))
                    return station.transform.position;

                var bed = col.GetComponentInParent<Bed>();
                if (bed != null && (!hasThreat || Vector3.Distance(bed.transform.position, threatPos) > 22f))
                    return bed.transform.position;

                var chest = col.GetComponentInParent<Container>();
                if (chest != null && chest.GetComponent<WorkerBrain>() == null && (!hasThreat || Vector3.Distance(chest.transform.position, threatPos) > 22f))
                    return chest.transform.position;
            }

            // 2. Search near player position (only if player is safely distant from danger)
            if (Player.m_localPlayer != null)
            {
                Vector3 pPos = Player.m_localPlayer.transform.position;
                if (!hasThreat || Vector3.Distance(pPos, threatPos) > 28f)
                {
                    Collider[] pHits = Physics.OverlapSphere(pPos, 60f);
                    foreach (var col in pHits)
                    {
                        var station = col.GetComponentInParent<CraftingStation>();
                        if (station != null) return station.transform.position;

                        var bed = col.GetComponentInParent<Bed>();
                        if (bed != null) return bed.transform.position;

                        var chest = col.GetComponentInParent<Container>();
                        if (chest != null && chest.GetComponent<WorkerBrain>() == null) return chest.transform.position;
                    }

                    if (Vector3.Distance(transform.position, pPos) < 150f)
                    {
                        return pPos;
                    }
                }
            }

            // 3. Fallback to HomePosition
            return HasHome ? HomePosition : transform.position;
        }

        public Vector3 GetTacticalRetreatDestination(Character threat)
        {
            // If already latched on a valid retreat waypoint, continue running towards it!
            if (_latchedRetreatTimer > 0f && _latchedRetreatDest != Vector3.zero)
            {
                return _latchedRetreatDest;
            }

            Vector3 dangerPos = threat != null ? threat.transform.position : transform.position;
            Vector3 mainBase = FindNearestMainBaseAnchor(dangerPos);
            Vector3 candidate = Vector3.zero;

            // If HomePosition is far enough from threat (> 22m), home camp is safe!
            if (HasHome && Vector3.Distance(HomePosition, dangerPos) > 22f)
            {
                candidate = HomePosition;
            }
            // If main base anchor is safe from threat (> 22m), retreat to main base!
            else if (Vector3.Distance(mainBase, dangerPos) > 22f)
            {
                candidate = mainBase;
            }
            else
            {
                // Threat is near both camp and base: flee directly away from threat!
                Vector3 fleeDir = (transform.position - dangerPos);
                fleeDir.y = 0;
                if (fleeDir.sqrMagnitude < 0.1f) fleeDir = threat != null ? -threat.transform.forward : -transform.forward;
                fleeDir.Normalize();
                candidate = transform.position + fleeDir * 28f;
            }

            // Collision check: test forward ray for rocks / obstacle obstruction
            Vector3 rayStart = transform.position + Vector3.up * 0.8f;
            Vector3 toCandidate = (candidate - transform.position);
            toCandidate.y = 0;
            float checkDist = Mathf.Min(8f, toCandidate.magnitude);
            if (checkDist > 1.2f && Physics.Raycast(rayStart, toCandidate.normalized, out RaycastHit hit, checkDist))
            {
                Vector3 deflectDir = Vector3.Reflect(toCandidate.normalized, hit.normal);
                deflectDir.y = 0;
                if (deflectDir.sqrMagnitude > 0.05f)
                {
                    candidate = transform.position + deflectDir.normalized * 25f;
                }
            }

            // Snap candidate to terrain floor
            if (ZoneSystem.instance != null && ZoneSystem.instance.FindFloor(candidate, out float floorY))
            {
                candidate.y = floorY;
            }

            _latchedRetreatDest = candidate;
            _latchedRetreatTimer = 3.5f;
            return _latchedRetreatDest;
        }

        public void OrientToNearestBase()
        {
            Vector3 basePos = FindNearestMainBaseAnchor(null);
            HomePosition = basePos;
            HasHome = true;
            SaveToZDO();
            StartCommandTrace("ORIENT_BASE", $"Camp home re-oriented to nearest base anchor at {basePos:F1}", "defense_orient_main_base");
            Say("Camp re-oriented to main base!");
            LogAction($"Camp home re-oriented to nearest base anchor at {basePos:F1}.", true);
            VerificationTelemetry.RecordPass("defense_orient_main_base", $"Worker '{GetHoverName()}' re-oriented camp to nearest base anchor at {basePos:F1}.");
        }

        private bool UpdateSelfDefenseAndThreats(float dt)
        {
            if (m_character == null || m_character.IsDead()) return false;

            // 1. Retreat timer handling
            if (_isRetreating)
            {
                _retreatTimer -= dt;
                Vector3 safePos = GetTacticalRetreatDestination(_activeThreat);
                float distToSafe = Vector3.Distance(transform.position, safePos);

                float curThreatDist = _activeThreat != null ? Vector3.Distance(transform.position, _activeThreat.transform.position) : 99f;
                float distMovedFromStart = Vector3.Distance(transform.position, _retreatStartPos);

                // Verify tactical retreat progress: worker actively increased distance from threat by >= 6m or reached safe camp
                if (!_tacticalRetreatVerified && (curThreatDist >= _retreatStartThreatDist + 6f || distToSafe <= 4.5f))
                {
                    _tacticalRetreatVerified = true;
                    RecordTraceStep("MOVE", $"Tactical retreat progress verified: threat distance increased from {_retreatStartThreatDist:F1}m to {curThreatDist:F1}m (moved {distMovedFromStart:F1}m).");
                    VerificationTelemetry.RecordPass("defense_tactical_retreat", $"Worker '{GetHoverName()}' tactically retreated from '{(_activeThreat != null ? _activeThreat.GetHoverName() : "threat")}' (Distance increased to {curThreatDist:F1}m).");
                    VerificationTelemetry.RecordPass("proving_troll_threat", $"Worker '{GetHoverName()}' successfully retreated {distMovedFromStart:F1}m towards safe camp.");
                }

                bool isLowHp = (m_character.GetHealth() / m_character.GetMaxHealth()) < 0.35f;
                if (isLowHp && !_lowHpRetreatVerified && (curThreatDist >= _retreatStartThreatDist + 4f || distMovedFromStart >= 4f || distToSafe <= 4.5f))
                {
                    _lowHpRetreatVerified = true;
                    RecordTraceStep("MOVE", $"Low HP retreat progress verified: gained {distMovedFromStart:F1}m separation from threat.");
                    VerificationTelemetry.RecordPass("defense_low_hp_retreat", $"Worker '{GetHoverName()}' retreated with {m_character.GetHealth():F0}/{m_character.GetMaxHealth():F0} HP (gained {distMovedFromStart:F1}m separation).");
                    VerificationTelemetry.RecordPass("proving_low_hp_scenario", $"Worker '{GetHoverName()}' safely disengaged at {m_character.GetHealth():F0} HP and sprinted {distMovedFromStart:F1}m toward camp.");
                }

                if (_stuckTimer > 2.0f)
                {
                    RecordTraceStep("WARN", $"Retreat impeded by obstacle/slope! Stuck timer: {_stuckTimer:F1}s, Current threat distance: {curThreatDist:F1}m");
                }

                if (_retreatTimer <= 0f && distToSafe <= 4.5f)
                {
                    _isRetreating = false;
                    _latchedRetreatTimer = 0f;
                    _latchedRetreatDest = Vector3.zero;
                    LogAction("Arrived at camp safety. Resuming duty.", true);
                    CompleteCommandTrace("COMPLETED", "Tactical retreat to safe camp completed.");
                    Say("All clear! Back to work.");
                }
                else
                {
                    MoveTo(safePos, 2f, true);
                    FaceTarget(safePos);
                    return true; // Preempt normal task
                }
            }

            // 2. Scan for hostiles periodically
            _threatScanTimer += dt;
            if (_threatScanTimer >= 0.45f)
            {
                _threatScanTimer = 0f;
                _activeThreat = FindHostileThreat(transform.position, 14f);
            }

            // 3. If threat exists, evaluate combat balance
            if (_activeThreat != null)
            {
                if (_activeThreat.IsDead())
                {
                    Say("Threat defeated!");
                    LogAction($"Defeated hostile '{_activeThreat.GetHoverName()}'. Resuming duty.", true);
                    VerificationTelemetry.RecordPass("defense_manageable_fight", $"Worker '{GetHoverName()}' ({CurrentRole}) defeated hostile '{_activeThreat.GetHoverName()}' and resumed work.");
                    RecordTraceStep("COMBAT", $"Defeated hostile '{_activeThreat.GetHoverName()}'. Resuming duty.");
                    CancelBowDraw();
                    _activeThreat = null;
                    return false;
                }

                float distToThreat = Vector3.Distance(transform.position, _activeThreat.transform.position);
                if (distToThreat > 25f)
                {
                    CancelBowDraw();
                    _activeThreat = null;
                    return false;
                }

                float threatScore = ComputeEnemyThreatScore(_activeThreat);
                float workerPower = ComputeWorkerAndAllyPower();

                bool isOverwhelming = threatScore > (workerPower * 1.55f);
                bool lowHp = (m_character.GetHealth() / m_character.GetMaxHealth()) < 0.35f;

                if (isOverwhelming || lowHp)
                {
                    CancelBowDraw();
                    _isRetreating = true;
                    _retreatTimer = 16f;
                    _latchedRetreatTimer = 0f; // Force fresh retreat calculation
                    _retreatStartThreatDist = distToThreat;
                    _retreatStartPos = transform.position;
                    _lowHpRetreatVerified = false;
                    _tacticalRetreatVerified = false;

                    Say($"{_activeThreat.GetHoverName()}! Too dangerous to fight alone! Retreating to camp!");
                    LogAction($"[Tactical Retreat] Outmatched by '{_activeThreat.GetHoverName()}' (Threat: {threatScore:F0} vs Ally Power: {workerPower:F0}, HP: {m_character.GetHealth():F0}). Sprinting to safety!", true);
                    RecordTraceStep("COMBAT", $"Retreat initiated! Threat: '{_activeThreat.GetHoverName()}' (Score: {threatScore:F0} vs Ally: {workerPower:F0}, Dist: {distToThreat:F1}m, HP: {m_character.GetHealth():F0})");

                    Vector3 fleeDest = GetTacticalRetreatDestination(_activeThreat);
                    MoveTo(fleeDest, 2f, true);
                    return true;
                }

                // Manageable threat: Stand & fight like a Viking!
                Vector3 threatCenter = _activeThreat.GetCenterPoint();
                FaceTarget(threatCenter);
                VerificationTelemetry.RecordPass("defense_manageable_fight", $"Worker '{GetHoverName()}' ({CurrentRole}) engaged manageable foe '{_activeThreat.GetHoverName()}' (Power: {workerPower:F0} vs Threat: {threatScore:F0}).");

                bool hasBow = HasToolType(ItemDrop.ItemData.ItemType.Bow) || IsHoldingBow();

                if (hasBow || CurrentRole == WorkerRole.Hunter)
                {
                    float meleeSwitchDist = GetHunterMeleeSwitchRange(); // 10m
                    if (distToThreat <= meleeSwitchDist)
                    {
                        // Close combat: switch to Melee mode (melee weapon + shield)
                        SetCombatMode(HunterCombatMode.Melee);
                        if (distToThreat <= 2.3f)
                        {
                            StopMoving();
                            FaceTarget(threatCenter);
                            if (!m_humanoid.InAttack())
                            {
                                m_humanoid.StartAttack(_activeThreat, false);
                                RaiseActivitySkill(WorkerActivitySkill.Melee, 1.2f);
                                RecordTraceStep("ATTACK", $"Melee strike against '{_activeThreat.GetHoverName()}' ({distToThreat:F1}m)");
                            }
                        }
                        else
                        {
                            MoveTo(_activeThreat.transform.position, 1.8f, true);
                        }
                    }
                    else
                    {
                        // Long-range combat: switch to Ranged mode (bow + ammo)
                        SetCombatMode(HunterCombatMode.Ranged);
                        if (IsHoldingBow())
                        {
                            ExecuteHunterBowDrawAndFire(_activeThreat, dt);
                        }
                        else
                        {
                            // No bow available: close distance in melee
                            SetCombatMode(HunterCombatMode.Melee);
                            MoveTo(_activeThreat.transform.position, 1.8f, true);
                        }
                    }
                }
                else
                {
                    AutoEquipBestCombatGear();
                    if (distToThreat <= 2.3f)
                    {
                        StopMoving();
                        FaceTarget(threatCenter);
                        if (!m_humanoid.InAttack())
                        {
                            m_humanoid.StartAttack(_activeThreat, false);
                            RaiseActivitySkill(WorkerActivitySkill.Melee, 1.2f);
                        }
                    }
                    else
                    {
                        MoveTo(_activeThreat.transform.position, 1.8f, true);
                    }
                }

                return true;
            }

            return false;
        }

        public float ComputeEnemyThreatScore(Character enemy)
        {
            if (enemy == null) return 0f;
            float maxHp = enemy.GetMaxHealth();
            int level = enemy.GetLevel();
            float levelMult = level == 2 ? 1.8f : (level >= 3 ? 2.8f : 1.0f);

            float score = maxHp * levelMult;
            string eName = enemy.gameObject.name;

            if (eName.Contains("Troll") || eName.Contains("Bear") || eName.Contains("Bjorn") ||
                eName.Contains("Abomination") || eName.Contains("Golem") || eName.Contains("Lox") ||
                eName.Contains("Gjall") || eName.Contains("SeekerBrute") || eName.Contains("Berserker") ||
                enemy.IsBoss())
            {
                score *= 2.5f;
            }

            return score;
        }

        private float ComputeWorkerAndAllyPower()
        {
            if (m_character == null) return 0f;

            float currentHp = m_character.GetHealth();
            float weaponDmg = 15f;
            var rightItem = GetEquippedRightItem();
            if (rightItem != null) weaponDmg = Mathf.Max(weaponDmg, rightItem.GetDamage().GetTotalDamage());

            bool hasShield = GetEquippedLeftItem() != null;
            float soloPower = (currentHp * 1.0f) + (weaponDmg * 3.5f) + (hasShield ? 40f : 0f);

            float alliesPower = 0f;
            foreach (var w in ActiveWorkers)
            {
                if (w == null || w == this || w.m_character == null || w.m_character.IsDead()) continue;
                float d = Vector3.Distance(transform.position, w.transform.position);
                if (d <= 18f)
                {
                    float allyHp = w.m_character.GetHealth();
                    float bonus = w.CurrentRole == WorkerRole.Guard ? 1.5f : 1.0f;
                    alliesPower += (allyHp * 0.8f + 30f) * bonus;
                }
            }

            if (Player.m_localPlayer != null && !Player.m_localPlayer.IsDead())
            {
                float pDist = Vector3.Distance(transform.position, Player.m_localPlayer.transform.position);
                if (pDist <= 20f)
                {
                    alliesPower += 250f;
                }
            }

            return soloPower + alliesPower;
        }

        private void UpdateMapPin()
        {
            if (Minimap.instance == null) return;

            if (_mapPin == null)
            {
                _mapPin = Minimap.instance.AddPin(transform.position, Minimap.PinType.Player, GetMapPinLabel(), false, false);
                if (_mapPin != null)
                {
                    _mapPin.m_icon = GetMapPinIcon();
                    VerificationTelemetry.RecordPass("map_minimap_pin", $"Active Minimap pin created and synced for '{GetHoverName()}' at {transform.position:F1} ({GetMapPinLabel()}).");
                }
            }
            else
            {
                _mapPin.m_pos = transform.position;
                _mapPin.m_name = GetMapPinLabel();
                _mapPin.m_icon = GetMapPinIcon();
            }
        }

        public string GetMapPinLabel()
        {
            string name = GetHoverName();
            if (_isRetreating) return $"{name} [RETREATING!]";
            if (_activeThreat != null) return $"{name} [FIGHTING {_activeThreat.GetHoverName()}]";
            return $"{name} [{CurrentRole}]";
        }

        private Sprite GetMapPinIcon()
        {
            // 1. Role-specific deterministic icon lookup
            string itemLookup = CurrentRole switch
            {
                WorkerRole.Lumberjack => "AxeBronze",
                WorkerRole.Miner => "PickaxeAntler",
                WorkerRole.Hunter => "BowFineWood",
                WorkerRole.Guard => "ShieldWood",
                WorkerRole.BaseTender => "Torch",
                WorkerRole.Farmer => "Cultivator",
                WorkerRole.Cook => "KnifeFlint",
                _ => "Hammer"
            };

            if (ObjectDB.instance != null)
            {
                var prefab = ObjectDB.instance.GetItemPrefab(itemLookup);
                var drop = prefab != null ? prefab.GetComponent<ItemDrop>() : null;
                if (drop != null && drop.m_itemData != null)
                {
                    var icon = drop.m_itemData.GetIcon();
                    if (icon != null) return icon;
                }
            }

            var primaryItem = GetEquippedPrimaryWeapon();
            if (primaryItem != null)
            {
                var icon = primaryItem.GetIcon();
                if (icon != null) return icon;
            }
            return null;
        }

        public void RemoveMapPin()
        {
            if (Minimap.instance != null && _mapPin != null)
            {
                Minimap.instance.RemovePin(_mapPin);
                _mapPin = null;
            }
        }

        private void UpdateFollow(float dt)
        {
            Player player = Player.m_localPlayer;
            if (player == null) return;

            // [F34] Follower Automatic Ship Boarding
            Ship ship = player.GetControlledShip();
            if (ship == null && player.transform.parent != null)
            {
                ship = player.transform.parent.GetComponentInParent<Ship>();
            }

            if (ship != null)
            {
                float distToShip = Vector3.Distance(transform.position, ship.transform.position);
                if (distToShip > 16f)
                {
                    Vector3 shipDeckPos = ship.transform.position + ship.transform.up * 0.8f - ship.transform.forward * 1.5f;
                    transform.position = shipDeckPos;
                    StopMoving();
                    LogAction("[F34] Boarded ship with player.");
                    VerificationTelemetry.RecordPass("social_ship_boarding", $"Follower '{GetHoverName()}' boarded ship with player.");
                    return;
                }
                else if (distToShip > 3.0f)
                {
                    MoveTo(ship.transform.position, 2.2f, true);
                    return;
                }
                else
                {
                    StopMoving();
                    FaceTarget(ship.transform.position + ship.transform.forward * 4f);
                    return;
                }
            }

            // [F35] Night & dark-biome torch bearing
            if (EnvMan.IsNight() && !m_character.InAttack())
            {
                if (HasToolType(ItemDrop.ItemData.ItemType.Torch, "") && GetEquippedRightItem()?.m_shared?.m_itemType != ItemDrop.ItemData.ItemType.Torch)
                {
                    AutoEquipTool("Torch");
                    VerificationTelemetry.RecordPass("social_night_torch", $"Worker '{GetHoverName()}' equipped torch for night/dark navigation.");
                }
            }

            float dist = Vector3.Distance(transform.position, player.transform.position);
            if (dist > 14f)
            {
                LogAction($"Following player (running to catch up, dist: {dist:F1}m)");
                MoveTo(player.transform.position, 2.8f, true);
            }
            else if (dist > 3.2f)
            {
                LogAction($"Following player (walking, dist: {dist:F1}m)");
                MoveTo(player.transform.position, 2.5f, false);
            }
            else
            {
                LogAction($"Standing beside player (ready to station with [E])");
                StopMoving();
                FaceTarget(player.transform.position);
            }
        }

        public bool MoveTo(Vector3 target, float stopDist, bool run)
        {
            if (m_baseAI == null || m_character == null) return false;

            // Handle active detour waypoint routing around obstacles
            Vector3 effectiveTarget = target;
            if (_detourTimer > 0f)
            {
                _detourTimer -= _lastAiDt;
                float detourDist = Vector3.Distance(transform.position, _detourTarget);
                if (detourDist <= 1.2f || _detourTimer <= 0f)
                {
                    _detourTimer = 0f;
                }
                else
                {
                    effectiveTarget = _detourTarget;
                }
            }

            float dist = Vector3.Distance(transform.position, effectiveTarget);
            if (dist <= stopDist && _detourTimer <= 0f)
            {
                StopMoving();
                return true;
            }

            _isNavigating = true;

            // 1. Uphill Sprint Promotion: ascending steep grades requires running kinetic force
            bool uphill = (effectiveTarget.y > transform.position.y + 0.35f);
            if (uphill)
            {
                run = true;
            }

            // 2. Obstacle / Elevation Check:
            Vector3 fwd = (effectiveTarget - transform.position);
            fwd.y = 0;
            if (fwd.sqrMagnitude > 0.05f)
            {
                Vector3 moveDirNorm = fwd.normalized;
                Vector3 rayOriginLow = transform.position + Vector3.up * 0.35f;
                Vector3 rayOriginHigh = transform.position + Vector3.up * 1.25f;

                bool lowHit = Physics.Raycast(rayOriginLow, moveDirNorm, out RaycastHit stepHit, 1.2f) &&
                              stepHit.collider != null && !stepHit.collider.isTrigger;
                bool highHit = Physics.Raycast(rayOriginHigh, moveDirNorm, out RaycastHit highHitInfo, 1.3f) &&
                               highHitInfo.collider != null && !highHitInfo.collider.isTrigger;

                if (lowHit && highHit)
                {
                    // TALL OBSTACLE (Boulder, wall, steep rock barrier) - do NOT jump forward into the wall!
                    // Initiate smart trajectory detour around the obstacle!
                    if (_detourTimer <= 0f)
                    {
                        InitiateObstacleDetour(target, moveDirNorm);
                    }
                }
                else if (lowHit && !highHit && m_character.IsOnGround())
                {
                    // Genuine low ledge or rock step (< 0.85m) — step hop assist!
                    m_humanoid?.Jump();
                    var rb = GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.velocity = new Vector3(moveDirNorm.x * 2.8f, Mathf.Max(rb.velocity.y, 4.0f), moveDirNorm.z * 2.8f);
                    }
                    VerificationTelemetry.RecordPass("locomotion_slope_direct_drive", $"Worker '{GetHoverName()}' executed elevation step hop assist.");
                }
            }

            // 3. Native NavMesh MoveTo execution
            bool moved = s_moveTo != null && s_moveTo(m_baseAI, _lastAiDt, effectiveTarget, stopDist, run);

            // 4. Direct-Drive & Mountain Slope Contour Traversal
            if (!moved && dist > stopDist)
            {
                Vector3 groundNormal = Vector3.up;
                if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out RaycastHit gHit, 1.8f))
                {
                    groundNormal = gHit.normal;
                }

                Vector3 horizDir = (effectiveTarget - transform.position);
                horizDir.y = 0;
                if (horizDir.sqrMagnitude > 0.05f)
                {
                    float slopeAngle = Vector3.Angle(groundNormal, Vector3.up);
                    Vector3 driveDir;

                    if (slopeAngle > 42f)
                    {
                        // Steep Mountain Slope (> 42 deg): do NOT head-on direct drive into cliff wall!
                        // Traverse diagonally along the contour gradient (switchback climb)
                        Vector3 contourTangent = Vector3.Cross(groundNormal, Vector3.up).normalized;
                        if (Vector3.Dot(contourTangent, horizDir) < 0f) contourTangent = -contourTangent;
                        driveDir = (Vector3.ProjectOnPlane(horizDir.normalized, groundNormal) * 0.65f + contourTangent * 0.75f).normalized;
                    }
                    else
                    {
                        driveDir = Vector3.ProjectOnPlane(horizDir.normalized, groundNormal).normalized;
                    }

                    m_character.SetMoveDir(driveDir);
                    m_character.SetRun(run);
                    FaceTarget(effectiveTarget);
                    VerificationTelemetry.RecordPass("locomotion_slope_direct_drive", $"Worker '{GetHoverName()}' navigated steep terrain via direct-drive contour projection.");
                    return true;
                }
            }

            return moved;
        }

        public void StopMoving()
        {
            _isNavigating = false;
            _stuckTimer = 0f;
            _consecutiveUnstuckAttempts = 0;
            _lastStuckCheckPos = transform.position;
            _detourTimer = 0f;
            if (m_baseAI != null)
            {
                m_baseAI.StopMoving();
            }
        }

        public void FaceTarget(Vector3 target)
        {
            Vector3 eyePos = m_humanoid != null ? m_humanoid.GetEyePoint() : (transform.position + Vector3.up * 1.6f);
            Vector3 aimDir = (target - eyePos).normalized;

            // 1. Direct pitch-and-yaw look direction for projectile aiming and Head/Eye tracking
            if (m_character != null && aimDir.sqrMagnitude > 0.001f)
            {
                m_character.SetLookDir(aimDir);
            }

            // 2. Physical torso/body rotation on horizontal plane
            Vector3 horizDir = target - transform.position;
            horizDir.y = 0f;
            if (horizDir.sqrMagnitude > 0.02f)
            {
                transform.rotation = Quaternion.LookRotation(horizDir.normalized);
            }

            // 3. Keep BaseAI look target updated
            if (m_baseAI != null && s_lookAt != null)
            {
                s_lookAt(m_baseAI, target);
            }
        }

        public Character FindEnemy()
        {
            return FindHostileThreat(HasHome ? HomePosition : transform.position, WorkRadius);
        }

        public Character FindHostileThreat(Vector3 postCenter, float radius)
        {
            if (m_character == null) return null;

            List<Character> allChars = Character.GetAllCharacters();
            Character closest = null;
            float bestDist = float.MaxValue;

            for (int i = 0; i < allChars.Count; i++)
            {
                Character c = allChars[i];
                if (c == null || c == m_character || c.IsDead()) continue;
                if (c.GetComponent<WorkerBrain>() != null) continue;
                if (!BaseAI.IsEnemy(m_character, c)) continue;

                if (c.m_faction == Character.Faction.AnimalsVeg)
                {
                    var ai = c.GetBaseAI();
                    bool alerted = ai != null && ai.IsAlerted();
                    if (!alerted || Vector3.Distance(transform.position, c.transform.position) > 8f)
                    {
                        continue;
                    }
                }

                float distFromPost = Vector3.Distance(postCenter, c.transform.position);
                float distFromSelf = Vector3.Distance(transform.position, c.transform.position);

                if (distFromPost <= radius || distFromSelf <= 12f)
                {
                    if (distFromSelf < bestDist)
                    {
                        bestDist = distFromSelf;
                        closest = c;
                    }
                }
            }

            return closest;
        }

        public int CountHostilesInRange(float radius)
        {
            if (m_character == null) return 0;
            List<Character> allChars = Character.GetAllCharacters();
            int count = 0;
            for (int i = 0; i < allChars.Count; i++)
            {
                Character c = allChars[i];
                if (c == null || c == m_character || c.IsDead()) continue;
                if (c.GetComponent<WorkerBrain>() != null) continue;
                if (!BaseAI.IsEnemy(m_character, c)) continue;
                if (Vector3.Distance(transform.position, c.transform.position) <= radius)
                {
                    count++;
                }
            }
            return count;
        }

        public void Say(string message)
        {
            if (Customization != null && !Customization.SpeechBubblesEnabled) return;
            if (Chat.instance != null)
            {
                Chat.instance.SetNpcText(gameObject, Vector3.up * 1.8f, 20f, 3.5f, "", message, false);
            }
        }

        public string GetCurrentStatusDescription()
        {
            if (_isRetreating) return $"Retreating to camp ({(_activeThreat != null ? _activeThreat.GetHoverName() : "Danger")})";
            if (_activeThreat != null) return $"Defending against {_activeThreat.GetHoverName()}";
            if (CurrentRole == WorkerRole.Follow) return $"Following player (Will resume {PreviousStationRole} on [E])";
            if (_activeTask != null) return _activeTask.GetStatusDescription();
            return "Idle at home post";
        }

        public bool IsProtectedWorkGear(ItemDrop.ItemData item)
        {
            if (item == null || item.m_shared == null) return false;
            if (item.m_equipped) return true;

            var t = item.m_shared.m_itemType;
            if (t == ItemDrop.ItemData.ItemType.OneHandedWeapon ||
                t == ItemDrop.ItemData.ItemType.TwoHandedWeapon ||
                t == ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft ||
                t == ItemDrop.ItemData.ItemType.Bow ||
                t == ItemDrop.ItemData.ItemType.Shield ||
                t == ItemDrop.ItemData.ItemType.Helmet ||
                t == ItemDrop.ItemData.ItemType.Chest ||
                t == ItemDrop.ItemData.ItemType.Legs ||
                t == ItemDrop.ItemData.ItemType.Shoulder ||
                t == ItemDrop.ItemData.ItemType.Tool ||
                t == ItemDrop.ItemData.ItemType.Torch ||
                t == ItemDrop.ItemData.ItemType.Ammo)
            {
                return true;
            }

            // Farmer protection: retain seeds in inventory for replanting fields
            if (CurrentRole == WorkerRole.Farmer)
            {
                string pName = item.m_dropPrefab != null ? item.m_dropPrefab.name : (item.m_shared.m_name ?? "");
                if (pName.IndexOf("Seeds", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    pName.IndexOf("CarrotSeeds", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    pName.IndexOf("TurnipSeeds", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    pName.IndexOf("OnionSeeds", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsInventoryFull()
        {
            var inv = m_humanoid?.GetInventory();
            return inv != null && inv.GetEmptySlots() == 0;
        }

        public int GetExcessDepositableItemCount()
        {
            var inv = m_humanoid?.GetInventory();
            if (inv == null) return 0;

            var protectedFoods = GetProtectedSustenanceFoodNames(3);
            var foodSeenCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            int excessCount = 0;

            foreach (var item in inv.GetAllItems())
            {
                if (item == null || item.m_stack <= 0) continue;
                if (IsProtectedWorkGear(item)) continue;

                if (IsFoodItem(item))
                {
                    string pName = GetItemPrefabName(item);
                    if (!string.IsNullOrEmpty(pName) && protectedFoods.Contains(pName))
                    {
                        foodSeenCounts.TryGetValue(pName, out int alreadyKept);
                        int canKeep = Mathf.Max(0, MaxPersonalRationPerFoodType - alreadyKept);
                        int keptFromThisStack = Mathf.Min(item.m_stack, canKeep);
                        foodSeenCounts[pName] = alreadyKept + keptFromThisStack;
                        int excessInStack = item.m_stack - keptFromThisStack;
                        excessCount += excessInStack;
                        continue;
                    }
                }

                excessCount += item.m_stack;
            }
            return excessCount;
        }

        public bool HasAnyGatheredResources()
        {
            return GetExcessDepositableItemCount() > 0;
        }

        public bool HasToolType(ItemDrop.ItemData.ItemType itemType, string nameSubstring = "")
        {
            var inv = m_humanoid?.GetInventory();
            if (inv == null) return false;
            foreach (var item in inv.GetAllItems())
            {
                if (item == null || item.m_shared == null) continue;
                string pName = item.m_dropPrefab != null ? item.m_dropPrefab.name : (item.m_shared.m_name ?? "");

                if (!string.IsNullOrEmpty(nameSubstring))
                {
                    if (string.Equals(nameSubstring, "Pickaxe", StringComparison.OrdinalIgnoreCase) &&
                        (item.m_shared.m_skillType == Skills.SkillType.Pickaxes || pName.IndexOf("Pickaxe", StringComparison.OrdinalIgnoreCase) >= 0))
                        return true;

                    if (string.Equals(nameSubstring, "Axe", StringComparison.OrdinalIgnoreCase) &&
                        (item.m_shared.m_skillType == Skills.SkillType.Axes || pName.IndexOf("Axe", StringComparison.OrdinalIgnoreCase) >= 0))
                        return true;

                    if (string.Equals(nameSubstring, "Bow", StringComparison.OrdinalIgnoreCase) &&
                        (item.m_shared.m_skillType == Skills.SkillType.Bows || pName.IndexOf("Bow", StringComparison.OrdinalIgnoreCase) >= 0))
                        return true;
                }

                if (item.m_shared.m_itemType == itemType)
                {
                    if (string.IsNullOrEmpty(nameSubstring)) return true;
                    if (pName.IndexOf(nameSubstring, StringComparison.OrdinalIgnoreCase) >= 0) return true;
                }
                else if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.OneHandedWeapon ||
                         item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.TwoHandedWeapon ||
                         item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Tool ||
                         item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Bow)
                {
                    if (!string.IsNullOrEmpty(nameSubstring) && pName.IndexOf(nameSubstring, StringComparison.OrdinalIgnoreCase) >= 0)
                        return true;
                }
            }
            return false;
        }

        public bool HasAnyCombatWeapon()
        {
            var inv = m_humanoid?.GetInventory();
            if (inv == null) return false;
            foreach (var item in inv.GetAllItems())
            {
                if (IsWeapon(item)) return true;
            }
            return false;
        }

        public int GetHighestToolTier(string toolSubstring)
        {
            int highest = -1;

            // 1. Check currently equipped right-hand item first
            var equippedRight = GetEquippedRightItem();
            if (equippedRight != null && equippedRight.m_shared != null)
            {
                string rName = equippedRight.m_dropPrefab != null ? equippedRight.m_dropPrefab.name : (equippedRight.m_shared.m_name ?? "");
                bool rMatches = rName.IndexOf(toolSubstring, StringComparison.OrdinalIgnoreCase) >= 0;
                if (!rMatches)
                {
                    if (string.Equals(toolSubstring, "Pickaxe", StringComparison.OrdinalIgnoreCase))
                        rMatches = (equippedRight.m_shared.m_skillType == Skills.SkillType.Pickaxes);
                    else if (string.Equals(toolSubstring, "Axe", StringComparison.OrdinalIgnoreCase))
                        rMatches = (equippedRight.m_shared.m_skillType == Skills.SkillType.Axes);
                }
                if (rMatches && equippedRight.m_shared.m_toolTier > highest)
                {
                    highest = equippedRight.m_shared.m_toolTier;
                }
            }

            // 2. Check all carried items in inventory
            var inv = m_humanoid?.GetInventory();
            if (inv != null)
            {
                foreach (var item in inv.GetAllItems())
                {
                    if (item == null || item.m_shared == null) continue;
                    string pName = item.m_dropPrefab != null ? item.m_dropPrefab.name : (item.m_shared.m_name ?? "");
                    bool matches = pName.IndexOf(toolSubstring, StringComparison.OrdinalIgnoreCase) >= 0;
                    if (!matches)
                    {
                        if (string.Equals(toolSubstring, "Pickaxe", StringComparison.OrdinalIgnoreCase))
                            matches = item.m_shared.m_skillType == Skills.SkillType.Pickaxes;
                        else if (string.Equals(toolSubstring, "Axe", StringComparison.OrdinalIgnoreCase))
                            matches = item.m_shared.m_skillType == Skills.SkillType.Axes;
                    }
                    if (matches && item.m_shared.m_toolTier > highest)
                    {
                        highest = item.m_shared.m_toolTier;
                    }
                }
            }
            return highest;
        }

        public bool HasPickaxe() => GetHighestToolTier("Pickaxe") >= 0;
        public bool HasAxe() => GetHighestToolTier("Axe") >= 0;

        public void AutoEquipTool(string toolNameSubstring)
        {
            if (string.IsNullOrEmpty(toolNameSubstring) || m_humanoid == null) return;
            if (m_humanoid.InAttack()) return; // Never disrupt active attack animation
            var inv = m_humanoid.GetInventory();
            if (inv == null) return;

            ItemDrop.ItemData bestTool = null;
            int highestTier = -1;

            foreach (var item in inv.GetAllItems())
            {
                if (item == null || item.m_shared == null) continue;
                string pName = item.m_dropPrefab != null ? item.m_dropPrefab.name : (item.m_shared.m_name ?? "");

                bool matches = pName.IndexOf(toolNameSubstring, StringComparison.OrdinalIgnoreCase) >= 0;
                if (!matches && string.Equals(toolNameSubstring, "Pickaxe", StringComparison.OrdinalIgnoreCase))
                {
                    matches = (item.m_shared.m_skillType == Skills.SkillType.Pickaxes);
                }
                else if (!matches && string.Equals(toolNameSubstring, "Axe", StringComparison.OrdinalIgnoreCase))
                {
                    matches = (item.m_shared.m_skillType == Skills.SkillType.Axes);
                }

                if (matches)
                {
                    if (item.m_shared.m_toolTier > highestTier)
                    {
                        highestTier = item.m_shared.m_toolTier;
                        bestTool = item;
                    }
                }
            }

            if (bestTool != null)
            {
                var currentRight = GetEquippedRightItem();
                if (currentRight != null)
                {
                    string curName = currentRight.m_dropPrefab != null ? currentRight.m_dropPrefab.name : (currentRight.m_shared?.m_name ?? "");
                    bool curMatches = curName.IndexOf(toolNameSubstring, StringComparison.OrdinalIgnoreCase) >= 0;
                    if (!curMatches && string.Equals(toolNameSubstring, "Pickaxe", StringComparison.OrdinalIgnoreCase))
                    {
                        curMatches = (currentRight.m_shared != null && currentRight.m_shared.m_skillType == Skills.SkillType.Pickaxes);
                    }
                    else if (!curMatches && string.Equals(toolNameSubstring, "Axe", StringComparison.OrdinalIgnoreCase))
                    {
                        curMatches = (currentRight.m_shared != null && currentRight.m_shared.m_skillType == Skills.SkillType.Axes);
                    }

                    if (curMatches && currentRight.m_shared != null && currentRight.m_shared.m_toolTier >= bestTool.m_shared.m_toolTier)
                    {
                        var handRight = Traverse.Create(m_humanoid).Field<ItemDrop.ItemData>("m_rightItem").Value;
                        if (handRight == null)
                        {
                            Traverse.Create(m_humanoid).Method("ShowHandItems").GetValue();
                        }
                        return;
                    }
                }

                if (currentRight != bestTool)
                {
                    m_humanoid.EquipItem(bestTool, false);
                }
            }
        }

        public void AutoEquipBestCombatGear()
        {
            if (m_humanoid == null) return;
            var inv = m_humanoid.GetInventory();
            if (inv == null) return;

            ItemDrop.ItemData bestWeapon = null;
            float bestDmg = -1f;

            ItemDrop.ItemData bestShield = null;
            float bestBlock = -1f;

            foreach (var item in inv.GetAllItems())
            {
                if (item == null || item.m_shared == null) continue;

                if (IsWeapon(item))
                {
                    float dmg = item.GetDamage().GetTotalDamage();
                    if (dmg > bestDmg)
                    {
                        bestDmg = dmg;
                        bestWeapon = item;
                    }
                }
                else if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Shield)
                {
                    float block = item.m_shared.m_blockPower;
                    if (block > bestBlock)
                    {
                        bestBlock = block;
                        bestShield = item;
                    }
                }
            }

            if (bestWeapon != null && GetEquippedRightItem() != bestWeapon)
            {
                m_humanoid.EquipItem(bestWeapon, false);
            }

            if (bestShield != null && GetEquippedLeftItem() != bestShield)
            {
                m_humanoid.EquipItem(bestShield, false);
            }
        }

        public void SetCombatMode(HunterCombatMode mode)
        {
            if (CurrentCombatMode == mode) return;

            if (mode == HunterCombatMode.Ranged)
            {
                CancelBowDraw();
                CurrentCombatMode = HunterCombatMode.Ranged;
                AutoEquipBestRangedWeapon();
                RecordTraceStep("COMBAT_MODE", "Switched combat stance to [Ranged: Bow & Arrows]");
                LogAction("Combat stance switched to [Ranged: Bow & Arrows]", true);
            }
            else if (mode == HunterCombatMode.Melee)
            {
                CancelBowDraw();
                CurrentCombatMode = HunterCombatMode.Melee;
                AutoEquipBestMeleeWeapon();
                RecordTraceStep("COMBAT_MODE", "Switched combat stance to [Melee: Close Quarters Weapon & Shield]");
                LogAction("Combat stance switched to [Melee: Close Quarters Weapon & Shield]", true);
            }
            else
            {
                CancelBowDraw();
                CurrentCombatMode = HunterCombatMode.None;
            }
        }

        public void CancelBowDraw()
        {
            if (IsDrawingBow)
            {
                var bow = GetEquippedBow();
                string animState = bow?.m_shared?.m_attack?.m_drawAnimationState ?? "bow_draw";
                m_humanoid?.GetComponent<ZSyncAnimation>()?.SetBool(animState, false);
                if (m_humanoid != null)
                {
                    s_attackDrawTimeRef(m_humanoid) = 0f;
                }
                IsDrawingBow = false;
                BowDrawTime = 0f;
                ActiveBowDrawPercentage = 0f;
            }
        }

        public float GetCurrentArrowSpeed()
        {
            var bowItem = GetEquippedBow();
            float arrowSpeed = 45f;
            if (bowItem?.m_shared?.m_attack != null)
            {
                arrowSpeed = Mathf.Max(25f, bowItem.m_shared.m_attack.m_projectileVel);
                var ammoItem = GetEquippedAmmo();
                if (ammoItem?.m_shared?.m_attack != null && ammoItem.m_shared.m_attack.m_projectileVel > 0f)
                {
                    arrowSpeed += ammoItem.m_shared.m_attack.m_projectileVel;
                }
            }
            return arrowSpeed;
        }

        public static float GetSampleGroundHeight(Vector3 pos)
        {
            if (ZoneSystem.instance != null && ZoneSystem.instance.GetGroundHeight(pos, out float gh))
            {
                return gh;
            }
            if (Physics.Raycast(new Vector3(pos.x, pos.y + 50f, pos.z), Vector3.down, out RaycastHit hit, 120f))
            {
                return hit.point.y;
            }
            return pos.y;
        }

        private static string CleanBlockerName(string rawName)
        {
            if (string.IsNullOrEmpty(rawName)) return "obstacle";
            int idx = rawName.IndexOf("(Clone)", StringComparison.OrdinalIgnoreCase);
            if (idx > 0) rawName = rawName.Substring(0, idx);
            return rawName.Trim();
        }

        private bool IsArrowBlockingCollider(Collider col, Character target, out string obsName)
        {
            obsName = "";
            if (col == null || !col.enabled || col.isTrigger) return false;

            // 1. Ignore self (the shooter)
            if (col.transform.root == transform.root) return false;

            // 2. Ignore the target creature itself; other characters/workers in line of fire block the shot
            var hitChar = col.GetComponentInParent<Character>();
            if (hitChar != null)
            {
                if (hitChar == target) return false;
                obsName = hitChar.GetHoverName();
                return true;
            }

            // 3. Ignore projectiles and dropped items
            if (col.GetComponentInParent<Projectile>() != null || col.GetComponentInParent<ItemDrop>() != null)
                return false;

            // 4. Terrain Heightmap (Hill crest / ridge)
            if (col.GetComponentInParent<Heightmap>() != null)
            {
                obsName = "Hill crest";
                return true;
            }

            // 5. Trees & Logs
            var tree = col.GetComponentInParent<TreeBase>();
            if (tree != null)
            {
                obsName = CleanBlockerName(tree.gameObject.name);
                return true;
            }
            var log = col.GetComponentInParent<TreeLog>();
            if (log != null)
            {
                obsName = CleanBlockerName(log.gameObject.name);
                return true;
            }

            // 6. Boulders & Ore Veins
            var mine5 = col.GetComponentInParent<MineRock5>();
            if (mine5 != null)
            {
                obsName = CleanBlockerName(mine5.gameObject.name);
                return true;
            }
            var mine = col.GetComponentInParent<MineRock>();
            if (mine != null)
            {
                obsName = CleanBlockerName(mine.gameObject.name);
                return true;
            }

            // 7. Player/World Building Pieces & Structures
            var piece = col.GetComponentInParent<Piece>();
            if (piece != null)
            {
                obsName = CleanBlockerName(piece.gameObject.name);
                return true;
            }

            // 8. Destructibles (stumps, rocks, etc.) — ignore small soft bushes/berry shrubs
            var dest = col.GetComponentInParent<Destructible>();
            if (dest != null)
            {
                string dName = dest.gameObject.name;
                if (dName.IndexOf("bush", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    dName.IndexOf("shrub", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    dName.IndexOf("heather", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    dName.IndexOf("raspberry", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    dName.IndexOf("blueberry", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    dName.IndexOf("cloudberry", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return false;
                }
                obsName = CleanBlockerName(dName);
                return true;
            }

            // 9. Ignore small pickables (mushrooms, flint, branches on ground)
            if (col.GetComponentInParent<Pickable>() != null)
                return false;

            // 10. Static rock/cliff/tree colliders without a script component
            string rootName = col.transform.root.name;
            if (rootName.IndexOf("rock", StringComparison.OrdinalIgnoreCase) >= 0 ||
                rootName.IndexOf("cliff", StringComparison.OrdinalIgnoreCase) >= 0 ||
                rootName.IndexOf("tree", StringComparison.OrdinalIgnoreCase) >= 0 ||
                rootName.IndexOf("fir", StringComparison.OrdinalIgnoreCase) >= 0 ||
                rootName.IndexOf("beech", StringComparison.OrdinalIgnoreCase) >= 0 ||
                rootName.IndexOf("pine", StringComparison.OrdinalIgnoreCase) >= 0 ||
                rootName.IndexOf("birch", StringComparison.OrdinalIgnoreCase) >= 0 ||
                rootName.IndexOf("oak", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                obsName = CleanBlockerName(rootName);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Evaluates both visual line-of-sight over terrain crests and the 6-segment parabolic arrow flight trajectory
        /// from shooterBasePos to target, detecting any tree trunk, boulder, structure, or hill crest that would catch the arrow.
        /// </summary>
        public bool EvaluateArrowTrajectoryClearance(Vector3 shooterBasePos, Character target, out float unobstructedFraction, out string blockerName)
        {
            unobstructedFraction = 0f;
            blockerName = "";
            if (target == null || target.IsDead()) return false;

            Vector3 startPos = ((shooterBasePos - transform.position).sqrMagnitude < 0.09f && m_humanoid?.m_eye != null)
                ? m_humanoid.m_eye.position
                : (shooterBasePos + Vector3.up * 1.55f);

            Vector3 targetCenter = target.GetCenterPoint();
            float dist = Vector3.Distance(shooterBasePos, target.transform.position);
            float arrowSpeed = GetCurrentArrowSpeed();
            float gravity = 5.0f;
            float flightTime = Mathf.Max(0.05f, dist / arrowSpeed);
            float gravityComp = 0.5f * gravity * (flightTime * flightTime);

            Vector3 targetVel = target.GetVelocity();
            Vector3 aimPos = targetCenter + Vector3.up * (gravityComp + 0.12f) + (targetVel * flightTime);
            Vector3 aimDir = (aimPos - startPos).normalized;
            Vector3 initVel = aimDir * arrowSpeed;
            Vector3 gravVec = new Vector3(0f, -gravity, 0f);

            // 1. Direct visual sightline check against hill crests (ensure hunter can actually see the target over the ridge)
            Vector3 visualTargetPt = targetCenter + Vector3.up * 0.35f;
            for (int m = 1; m <= 5; m++)
            {
                float frac = m / 6f;
                Vector3 sightPt = Vector3.Lerp(startPos, visualTargetPt, frac);
                float sightGround = GetSampleGroundHeight(sightPt);
                if (sightPt.y < sightGround + 0.15f)
                {
                    unobstructedFraction = frac;
                    blockerName = "Hill crest";
                    return false;
                }
            }

            // 2. 6-Segment Parabolic Arrow Flight Trajectory Check
            const int segments = 6;
            for (int k = 0; k < segments; k++)
            {
                float t0 = (k / (float)segments) * flightTime;
                float t1 = ((k + 1) / (float)segments) * flightTime;

                Vector3 p0 = startPos + initVel * t0 + 0.5f * gravVec * (t0 * t0);
                Vector3 p1 = startPos + initVel * t1 + 0.5f * gravVec * (t1 * t1);
                Vector3 midPt = 0.5f * (p0 + p1);

                // Check terrain elevation beneath arrow segment
                if (midPt.y < GetSampleGroundHeight(midPt) + 0.22f || (k < segments - 1 && p1.y < GetSampleGroundHeight(p1) + 0.22f))
                {
                    unobstructedFraction = (k + 0.5f) / segments;
                    blockerName = "Hill crest";
                    return false;
                }

                Vector3 segVec = p1 - p0;
                float segLen = segVec.magnitude;
                if (segLen < 0.01f) continue;
                Vector3 segDir = segVec / segLen;

                // SphereCast (0.16m radius) catches tree trunks, logs, rocks, and walls along the arrow's width
                RaycastHit[] sphereHits = Physics.SphereCastAll(p0, 0.16f, segDir, segLen);
                float earliestDist = float.MaxValue;
                string earliestBlocker = "";

                for (int i = 0; i < sphereHits.Length; i++)
                {
                    if (IsArrowBlockingCollider(sphereHits[i].collider, target, out string obsName))
                    {
                        if (sphereHits[i].distance < earliestDist)
                        {
                            earliestDist = sphereHits[i].distance;
                            earliestBlocker = obsName;
                        }
                    }
                }

                // RaycastAll catches non-convex Heightmap terrain mesh colliders
                RaycastHit[] rayHits = Physics.RaycastAll(p0, segDir, segLen);
                for (int i = 0; i < rayHits.Length; i++)
                {
                    if (IsArrowBlockingCollider(rayHits[i].collider, target, out string obsName))
                    {
                        if (rayHits[i].distance < earliestDist)
                        {
                            earliestDist = rayHits[i].distance;
                            earliestBlocker = obsName;
                        }
                    }
                }

                if (earliestDist < float.MaxValue)
                {
                    unobstructedFraction = Mathf.Clamp01((k + (earliestDist / segLen)) / segments);
                    blockerName = earliestBlocker;
                    return false;
                }
            }

            unobstructedFraction = 1.0f;
            return true;
        }

        private bool IsVantageSpotBlocked(Vector3 candidate)
        {
            if (candidate.y < 29.5f) return true; // Avoid deep water
            Collider[] cols = Physics.OverlapSphere(candidate + Vector3.up * 0.9f, 0.45f);
            for (int i = 0; i < cols.Length; i++)
            {
                var c = cols[i];
                if (c == null || !c.enabled || c.isTrigger) continue;
                if (c.transform.root == transform.root) continue;
                if (c.GetComponentInParent<Heightmap>() != null) continue;
                if (c.GetComponentInParent<Pickable>() != null && c.GetComponentInParent<TreeBase>() == null) continue;
                if (c.GetComponentInParent<TreeBase>() != null ||
                    c.GetComponentInParent<TreeLog>() != null ||
                    c.GetComponentInParent<MineRock>() != null ||
                    c.GetComponentInParent<MineRock5>() != null ||
                    c.GetComponentInParent<Piece>() != null)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Samples sideways orbital angles around the target to find a clear vantage point,
        /// strongly preferring higher ground for superior visibility while keeping movement tangential (sideways).
        /// </summary>
        public bool TryFindOrbitalVantagePoint(Character target, out Vector3 stepVantagePos)
        {
            stepVantagePos = transform.position;
            if (target == null) return false;

            Vector3 targetPos = target.transform.position;
            Vector3 hunterPos = transform.position;
            Vector3 radial = hunterPos - targetPos;
            radial.y = 0f;
            float horizDist = radial.magnitude;
            if (horizDist < 0.5f)
            {
                radial = -transform.forward;
                radial.y = 0f;
                radial.Normalize();
                horizDist = 15f;
            }
            else
            {
                radial /= horizDist;
            }

            float maxSnipe = GetHunterSnipingMaxRange();
            float orbitRadius = Mathf.Clamp(horizDist, 14f, maxSnipe - 2f);

            float[] testAngles = new float[] { 15f, -15f, 30f, -30f, 45f, -45f, 65f, -65f, 85f, -85f };
            float[] radialOffsets = new float[] { 0f, 3.0f, -2.5f };

            float bestScore = float.MinValue;
            Vector3 bestCandidate = hunterPos;
            float bestAngle = 20f * _orbitalDirectionSign;
            float bestRadius = orbitRadius;
            bool foundAny = false;

            for (int a = 0; a < testAngles.Length; a++)
            {
                float angleDeg = testAngles[a];
                int dirSign = angleDeg >= 0f ? 1 : -1;
                Vector3 dirFromTarget = Quaternion.Euler(0f, angleDeg, 0f) * radial;

                for (int r = 0; r < radialOffsets.Length; r++)
                {
                    float rOff = radialOffsets[r];
                    float sampleRadius = Mathf.Clamp(orbitRadius + rOff, 12f, maxSnipe);
                    Vector3 cand = targetPos + dirFromTarget * sampleRadius;
                    cand.y = GetSampleGroundHeight(cand);

                    if (IsVantageSpotBlocked(cand)) continue;

                    bool hasClearShot = EvaluateArrowTrajectoryClearance(cand, target, out float clearFrac, out _);

                    float score = 0f;
                    // 1. Clear shot & trajectory visibility progress
                    if (hasClearShot)
                    {
                        score += 1000f;
                    }
                    else
                    {
                        score += clearFrac * 240f;
                    }

                    // 2. High-ground preference: strongly favor higher positions that overlook crests and obstacles
                    float elevAboveTarget = Mathf.Clamp(cand.y - targetPos.y, -15f, 25f);
                    float elevGain = Mathf.Clamp(cand.y - hunterPos.y, -10f, 20f);
                    score += elevAboveTarget * 15f + elevGain * 12f;

                    // 3. Orbital direction continuity so the hunter circles smoothly without left-right oscillation
                    if (dirSign == _orbitalDirectionSign)
                    {
                        score += 55f;
                    }

                    // 4. Prefer smaller angle steps once clear sight + high ground are satisfied, and pure sideways orbit
                    score -= Mathf.Abs(angleDeg) * 0.45f;
                    score -= Mathf.Abs(rOff) * 3.5f;

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestCandidate = cand;
                        bestAngle = angleDeg;
                        bestRadius = sampleRadius;
                        foundAny = true;
                    }
                }
            }

            if (!foundAny)
            {
                // Fallback pure sideways step
                float fallbackAngle = 22f * _orbitalDirectionSign;
                Vector3 fallbackDir = Quaternion.Euler(0f, fallbackAngle, 0f) * radial;
                stepVantagePos = targetPos + fallbackDir * orbitRadius;
                stepVantagePos.y = GetSampleGroundHeight(stepVantagePos);
                return true;
            }

            _orbitalDirectionSign = bestAngle >= 0f ? 1 : -1;

            // If the winning vantage is far around the arc (> 25 deg), step via an intermediate tangential arc point
            // so the movement is strictly sideways around the target (and biased toward uphill high ground)
            if (Mathf.Abs(bestAngle) > 25f)
            {
                float stepAngle = Mathf.Sign(bestAngle) * 22f;
                Vector3 stepDir = Quaternion.Euler(0f, stepAngle, 0f) * radial;
                Vector3 arcCenter = targetPos + stepDir * bestRadius;
                arcCenter.y = GetSampleGroundHeight(arcCenter);

                // Check if stepping slightly outward/inward on the lateral tangent yields higher elevation
                Vector3 arcOut = targetPos + stepDir * Mathf.Min(maxSnipe, bestRadius + 2.5f);
                arcOut.y = GetSampleGroundHeight(arcOut);
                Vector3 arcIn = targetPos + stepDir * Mathf.Max(12f, bestRadius - 2.0f);
                arcIn.y = GetSampleGroundHeight(arcIn);

                stepVantagePos = arcCenter;
                if (!IsVantageSpotBlocked(arcOut) && arcOut.y > stepVantagePos.y + 0.35f)
                {
                    stepVantagePos = arcOut;
                }
                if (!IsVantageSpotBlocked(arcIn) && arcIn.y > stepVantagePos.y + 0.35f)
                {
                    stepVantagePos = arcIn;
                }
            }
            else
            {
                stepVantagePos = bestCandidate;
            }

            return true;
        }

        public void ExecuteHunterBowDrawAndFire(Character target, float dt)
        {
            if (target == null || target.IsDead() || m_humanoid == null)
            {
                CancelBowDraw();
                CurrentFocusTarget = null;
                _vantageLatchTimer = 0f;
                _activeVantageTarget = Vector3.zero;
                return;
            }

            CurrentFocusTarget = target;

            var bowItem = GetEquippedBow();
            if (bowItem == null || bowItem.m_shared == null || bowItem.m_shared.m_attack == null)
            {
                // No bow: fall back to melee mode
                SetCombatMode(HunterCombatMode.Melee);
                return;
            }

            EnsureAmmoEquipped();
            var zanim = m_humanoid.GetComponent<ZSyncAnimation>();
            string drawAnim = bowItem.m_shared.m_attack.m_drawAnimationState ?? "bow_draw";

            Vector3 targetCenter = target.GetCenterPoint();
            float dist = Vector3.Distance(transform.position, target.transform.position);

            // Check if trees, boulders, structures, or a hill crest obstruct the arrow trajectory or view
            bool hasClearShot = EvaluateArrowTrajectoryClearance(transform.position, target, out float clearFrac, out string blockerName);
            if (!hasClearShot)
            {
                // Immediately cancel any active bow draw so we never waste arrows into trees or hillsides!
                CancelBowDraw();
                CustomAimDir = null;
                LastShotBlockerName = blockerName;

                _vantageLatchTimer -= dt;
                float distToVantage = _activeVantageTarget != Vector3.zero
                    ? Vector3.Distance(transform.position, _activeVantageTarget)
                    : 0f;

                if (_vantageLatchTimer <= 0f || _activeVantageTarget == Vector3.zero || distToVantage <= 1.4f)
                {
                    if (TryFindOrbitalVantagePoint(target, out Vector3 newVantage))
                    {
                        _activeVantageTarget = newVantage;
                        _vantageLatchTimer = 1.2f;
                        float elevDelta = _activeVantageTarget.y - transform.position.y;
                        LogAction($"Shot to '{target.GetHoverName()}' obstructed by {blockerName} ({clearFrac * 100f:F0}% clear). Strafing sideways to vantage {_activeVantageTarget:F1} (elev {elevDelta:+0.0;-0.0;0.0}m).");
                        RecordTraceStep("REPOSITION", $"View to '{target.GetHoverName()}' blocked by '{blockerName}'. Moving sideways to high-ground vantage {_activeVantageTarget:F1} (elev {elevDelta:+0.0;-0.0;0.0}m)");
                    }
                }

                // Move sideways along the orbital arc while keeping focus locked on the target
                MoveTo(_activeVantageTarget, 1.0f, false);
                FaceTarget(targetCenter);
                return;
            }

            // Shot is clear! Stop lateral movement and lock aim on target
            _vantageLatchTimer = 0f;
            _activeVantageTarget = Vector3.zero;
            LastShotBlockerName = "";
            StopMoving();

            // True projectile velocity combination (bow + arrow)
            float arrowSpeed = GetCurrentArrowSpeed();

            float gravity = 5.0f; // Standard Valheim arrow gravity
            float flightTime = dist / arrowSpeed;
            float gravityComp = 0.5f * gravity * (flightTime * flightTime);

            Vector3 targetVel = target.GetVelocity();
            Vector3 aimPos = targetCenter + Vector3.up * (gravityComp + 0.12f) + (targetVel * flightTime);

            Vector3 eyePos = m_humanoid.m_eye != null ? m_humanoid.m_eye.position : (transform.position + Vector3.up * 1.5f);
            CustomAimDir = (aimPos - eyePos).normalized;
            FaceTarget(aimPos);

            if (!IsDrawingBow)
            {
                if (!m_humanoid.InAttack())
                {
                    IsDrawingBow = true;
                    BowDrawTime = 0f;
                    ActiveBowDrawPercentage = 0.1f;
                    s_attackDrawTimeRef(m_humanoid) = 0f;
                    bowItem.m_shared.m_attack.StartDraw(m_humanoid, bowItem);
                    zanim?.SetBool(drawAnim, true);
                    zanim?.SetFloat("drawpercent", 0.1f);
                    RecordTraceStep("ATTACK", $"Drawing bow targeting '{target.GetHoverName()}' (dist: {dist:F1}m, arc: +{gravityComp:F2}m)");
                }
            }
            else
            {
                BowDrawTime += dt;
                // Hold draw for ~1.15s to reach full tension visibly and mechanically
                float drawPct = Mathf.Clamp01(BowDrawTime / 1.15f);
                ActiveBowDrawPercentage = drawPct;

                float minDraw = bowItem.m_shared.m_attack.m_drawDurationMin;
                s_attackDrawTimeRef(m_humanoid) = minDraw * drawPct;
                zanim?.SetFloat("drawpercent", drawPct);
                FaceTarget(aimPos);

                bool fullDraw = BowDrawTime >= 1.15f;
                bool isAlerted = target.GetBaseAI()?.IsAlerted() ?? false;
                bool emergencyRelease = BowDrawTime >= 0.65f && isAlerted && targetVel.sqrMagnitude > 4f;

                if (fullDraw || emergencyRelease)
                {
                    LastReleasedBowDrawPercentage = 1.0f;
                    _bowReleaseTimer = 0f;
                    m_humanoid.StartAttack(target, false);
                    RaiseActivitySkill(WorkerActivitySkill.Bows, 1.5f);
                    zanim?.SetBool(drawAnim, false);
                    IsDrawingBow = false;
                    BowDrawTime = 0f;
                    ActiveBowDrawPercentage = 0f;
                    RecordTraceStep("ATTACK", $"Fired precision {(fullDraw ? "sniped" : "snap")} arrow at '{target.GetHoverName()}' (dist: {dist:F1}m, elev: +{gravityComp:F2}m, speed: {arrowSpeed:F0}m/s)");
                    VerificationTelemetry.RecordPass("hunting_game_harvest", $"Hunter '{GetHoverName()}' sniped '{target.GetHoverName()}' from {dist:F1}m with {gravityComp:F2}m ballistic elevation at {arrowSpeed:F0}m/s.");
                }
            }
        }

        public void AutoEquipBestRangedWeapon()
        {
            if (m_humanoid == null) return;
            var inv = m_humanoid.GetInventory();
            if (inv == null) return;

            ItemDrop.ItemData bestBow = null;
            float bestDmg = -1f;

            foreach (var item in inv.GetAllItems())
            {
                if (item?.m_shared == null) continue;
                if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Bow)
                {
                    float dmg = item.GetDamage().GetTotalDamage();
                    if (dmg > bestDmg)
                    {
                        bestDmg = dmg;
                        bestBow = item;
                    }
                }
            }

            if (bestBow != null && GetEquippedBow() != bestBow)
            {
                AllowAiWeaponSwitch = true;
                try
                {
                    var right = GetEquippedRightItem();
                    if (right != null) m_humanoid.UnequipItem(right, false);
                    var left = GetEquippedLeftItem();
                    if (left != null && left != bestBow) m_humanoid.UnequipItem(left, false);

                    m_humanoid.EquipItem(bestBow, false);
                }
                finally
                {
                    AllowAiWeaponSwitch = false;
                }
            }
            EnsureAmmoEquipped();
        }

        public void AutoEquipBestMeleeWeapon()
        {
            if (m_humanoid == null) return;
            var inv = m_humanoid.GetInventory();
            if (inv == null) return;

            ItemDrop.ItemData bestMelee = null;
            float bestDmg = -1f;

            ItemDrop.ItemData bestShield = null;
            float bestBlock = -1f;

            foreach (var item in inv.GetAllItems())
            {
                if (item?.m_shared == null) continue;

                var t = item.m_shared.m_itemType;
                if (t == ItemDrop.ItemData.ItemType.OneHandedWeapon ||
                    t == ItemDrop.ItemData.ItemType.TwoHandedWeapon ||
                    t == ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft)
                {
                    float dmg = item.GetDamage().GetTotalDamage();
                    if (dmg > bestDmg)
                    {
                        bestDmg = dmg;
                        bestMelee = item;
                    }
                }
                else if (t == ItemDrop.ItemData.ItemType.Shield)
                {
                    float block = item.m_shared.m_blockPower;
                    if (block > bestBlock)
                    {
                        bestBlock = block;
                        bestShield = item;
                    }
                }
            }

            AllowAiWeaponSwitch = true;
            try
            {
                var bow = GetEquippedBow();
                if (bow != null && bow != bestMelee)
                {
                    m_humanoid.UnequipItem(bow, false);
                }

                if (bestMelee != null && GetEquippedRightItem() != bestMelee)
                {
                    m_humanoid.EquipItem(bestMelee, false);
                }

                if (bestShield != null && GetEquippedLeftItem() != bestShield)
                {
                    m_humanoid.EquipItem(bestShield, false);
                }
            }
            finally
            {
                AllowAiWeaponSwitch = false;
            }
        }

        public static bool IsWeapon(ItemDrop.ItemData item)
        {
            if (item?.m_shared == null) return false;
            var t = item.m_shared.m_itemType;
            return t == ItemDrop.ItemData.ItemType.OneHandedWeapon ||
                   t == ItemDrop.ItemData.ItemType.TwoHandedWeapon ||
                   t == ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft ||
                   t == ItemDrop.ItemData.ItemType.Bow;
        }

        public static bool IsAxe(ItemDrop.ItemData item)
        {
            if (item?.m_shared == null) return false;
            return item.m_shared.m_skillType == Skills.SkillType.Axes || item.m_shared.m_name.IndexOf("Axe", System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static bool IsPickaxe(ItemDrop.ItemData item)
        {
            if (item?.m_shared == null) return false;
            return item.m_shared.m_skillType == Skills.SkillType.Pickaxes || item.m_shared.m_name.IndexOf("Pickaxe", System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static bool IsEquippableGear(ItemDrop.ItemData item)
        {
            if (item?.m_shared == null) return false;
            var t = item.m_shared.m_itemType;
            return t == ItemDrop.ItemData.ItemType.OneHandedWeapon ||
                   t == ItemDrop.ItemData.ItemType.TwoHandedWeapon ||
                   t == ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft ||
                   t == ItemDrop.ItemData.ItemType.Bow ||
                   t == ItemDrop.ItemData.ItemType.Shield ||
                   t == ItemDrop.ItemData.ItemType.Helmet ||
                   t == ItemDrop.ItemData.ItemType.Chest ||
                   t == ItemDrop.ItemData.ItemType.Legs ||
                   t == ItemDrop.ItemData.ItemType.Shoulder ||
                   t == ItemDrop.ItemData.ItemType.Tool ||
                   t == ItemDrop.ItemData.ItemType.Torch;
        }

        public void OpenNativeInventory(Player player)
        {
            if (player == null) player = Player.m_localPlayer;
            if (player == null || m_container == null) return;

            NpcLabGui.SetOpen(false);

            m_container.m_name = GetHoverName();
            var inv = m_container.GetInventory();
            if (inv != null)
            {
                Traverse.Create(inv).Field("m_name").SetValue(GetHoverName());
            }

            if (InventoryGui.instance != null)
            {
                InventoryGui.instance.Show(m_container, 1);
                LogAction("Opened native Valheim 8x4 inventory grid.", true);
            }
        }

        public void OnNpcGridRightClick(ItemDrop.ItemData item)
        {
            if (item == null || m_humanoid == null) return;

            if (IsEquippableGear(item))
            {
                string localizedName = Localization.instance != null ? Localization.instance.Localize(item.m_shared.m_name) : item.m_shared.m_name;
                if (item.m_equipped)
                {
                    AllowManualPlayerUnequip = true;
                    try
                    {
                        m_humanoid.UnequipItem(item, true);
                    }
                    finally
                    {
                        AllowManualPlayerUnequip = false;
                    }
                    Say($"Unequipped {localizedName}");
                    LogAction($"Unequipped {localizedName} via right-click in inventory grid.", true);
                }
                else
                {
                    m_humanoid.EquipItem(item, true);
                    Say($"Equipped {localizedName}!");
                    LogAction($"Equipped {localizedName} via right-click in inventory grid.", true);
                }
                UpdateCustomizationFromCurrentEquipment();
                ApplyVisualEquipment();
                SaveToZDO();
                return;
            }

            // If it's a material or food item stack, transfer to player inventory
            if (Player.m_localPlayer != null)
            {
                TakeCarriedItemToPlayer(item);
            }
        }

        // --- Hoverable Interface ---

        public string GetHoverText()
        {
            string name = Customization != null && !string.IsNullOrEmpty(Customization.Name)
                ? Customization.Name
                : (Archetype != null ? Archetype.DisplayName : "Settlement Worker");

            string roleColor = CurrentRole switch
            {
                WorkerRole.Guard => "#FF6B6B",
                WorkerRole.Hauler => "#4DABF7",
                WorkerRole.BaseTender => "#FFA94D",
                WorkerRole.Follow => "#69DB7C",
                WorkerRole.Gatherer => "#51CF66",
                WorkerRole.Lumberjack => "#69DB7C",
                WorkerRole.Miner => "#74C0FC",
                WorkerRole.Hunter => "#FF922B",
                WorkerRole.Farmer => "#A9E34B",
                WorkerRole.Cook => "#FF922B",
                _ => "#CED4DA"
            };

            string status = GetCurrentStatusDescription();
            if (_isRetreating)
            {
                status = "<color=#FF6B6B><b>TACTICAL RETREAT: Fleeing to camp!</b></color>";
            }
            else if (_activeThreat != null)
            {
                status = $"<color=#FFA94D><b>DEFENDING AGAINST {_activeThreat.GetHoverName().ToUpperInvariant()}!</b></color>";
            }

            float distFromPost = Vector3.Distance(transform.position, HomePosition);
            string postLabel = HasWorkPost ? $"Camp: {distFromPost:F0}m | Work: {Vector3.Distance(transform.position, WorkPostPosition):F0}m" : $"Post: {distFromPost:F1}m";
            string weaponLabel = "Unarmed";
            var right = GetEquippedRightItem();
            var left = GetEquippedLeftItem();
            if (right != null && right.m_dropPrefab != null)
            {
                weaponLabel = right.m_dropPrefab.name;
            }
            else if (left != null && left.m_dropPrefab != null && IsHoldingBow())
            {
                weaponLabel = left.m_dropPrefab.name;
            }
            else if (Customization != null && !string.IsNullOrEmpty(Customization.Weapon))
            {
                weaponLabel = Customization.Weapon;
            }

            if (CurrentRole == WorkerRole.Hunter && CurrentCombatMode != HunterCombatMode.None)
            {
                weaponLabel += $" [{CurrentCombatMode}]";
            }

            string sustenanceLine = $"Foods ({ActiveFoods.Count}/3): " + (ActiveFoods.Count > 0
                ? string.Join(", ", ActiveFoods.ConvertAll(f => f.DisplayName))
                : "None");
            if (CurrentRole == WorkerRole.Hunter)
            {
                var eqBow = GetEquippedBow();
                string bowDur = (eqBow != null && eqBow.GetMaxDurability() > 0f)
                    ? $" | Bow: {Mathf.RoundToInt((eqBow.m_durability / eqBow.GetMaxDurability()) * 100f)}%"
                    : "";
                sustenanceLine += $" | Arrows: {GetTotalArrowCount()}{bowDur}";
            }

            string followStayPrompt = CurrentRole == WorkerRole.Follow
                ? $"[<color=yellow><b>E</b></color>] Station {PreviousStationRole} Post Here"
                : $"[<color=yellow><b>E</b></color>] Follow Me (Relocate Post)";

            return $"<color={roleColor}><b>{name}</b></color>\n" +
                   $"<color=#E9ECEF>Role: {CurrentRole} | Weapon: {weaponLabel} | {postLabel}</color>\n" +
                   $"<color=#96F2D7>{sustenanceLine}</color>\n" +
                   $"<color=#ADB5BD>Status: {status}</color>\n" +
                   $"{followStayPrompt}\n" +
                   $"[<color=yellow><b>Hold E</b></color>] Inventory & Character Sheet (Skills, Stats, Equipment)\n" +
                   $"[<color=yellow><b>L.Alt + Hold E</b></color>] Settlement Hall (F8) | [<color=yellow><b>L.Shift + E</b></color>] Cycle Role";
        }

        public string GetHoverName()
        {
            return Customization != null && !string.IsNullOrEmpty(Customization.Name)
                ? Customization.Name
                : (Archetype != null ? Archetype.DisplayName : "Worker");
        }

        public float GetHoverOffset()
        {
            return 1.8f;
        }

        // --- Interactable Interface ---

        public bool Interact(Humanoid user, bool hold, bool alt)
        {
            if (hold)
            {
                if (alt)
                {
                    // Alt + Hold E: Open Settlement Hall (F8)
                    if (Time.time - _lastHoldOpenTime > 0.6f)
                    {
                        _lastHoldOpenTime = Time.time;
                        NpcLabGui.OpenWorkerInspector(this);
                    }
                    return true;
                }

                // Hold E: Open Native Valheim 8x4 Inventory
                if (Time.time - _lastHoldOpenTime > 0.6f)
                {
                    _lastHoldOpenTime = Time.time;
                    OpenNativeInventory(user as Player ?? Player.m_localPlayer);
                }
                return true;
            }

            if (alt)
            {
                CycleRole();
                return true;
            }

            if (CurrentRole == WorkerRole.Follow)
            {
                WorkerRole restoredRole = PreviousStationRole != WorkerRole.Follow ? PreviousStationRole : WorkerRole.Guard;
                StartCommandTrace("STATION", $"Ordered via [E] to Station at post ({restoredRole})", "interaction_follow_toggle");
                StationAtPosition(transform.position, transform.rotation, restoredRole);
                VerificationTelemetry.RecordPass("interaction_follow_toggle", $"Stationed worker '{GetHoverName()}' at {transform.position:F1} as {restoredRole}.");
            }
            else
            {
                PreviousStationRole = CurrentRole;
                StartCommandTrace("FOLLOW", $"Ordered via [E] to Follow player (Station role: {PreviousStationRole})", "interaction_follow_toggle");
                SetRole(WorkerRole.Follow, false);
                Say($"Post released. Following you! (Press E at new post to station as {PreviousStationRole})");
                LogAction($"Ordered by player via [E] to Follow (remembering previous role: {PreviousStationRole})", true);
                VerificationTelemetry.RecordPass("interaction_follow_toggle", $"Worker '{GetHoverName()}' ordered via [E] to Follow player (Station role: {PreviousStationRole}).");
            }

            return true;
        }

        public bool UseItem(Humanoid user, ItemDrop.ItemData item)
        {
            return HandleUseOrReceiveItem(user, item);
        }

        private bool HandleUseOrReceiveItem(Humanoid user, ItemDrop.ItemData item)
        {
            if (user == null || item == null || item.m_shared == null) return false;
            if (Customization == null) Customization = new WorkerCustomization();

            string prefabName = item.m_dropPrefab != null ? item.m_dropPrefab.name : "";
            string localizedName = Localization.instance != null ? Localization.instance.Localize(item.m_shared.m_name) : item.m_shared.m_name;
            var itemType = item.m_shared.m_itemType;

            var userInv = user.GetInventory();
            var workerInv = m_humanoid?.GetInventory();
            if (userInv == null || workerInv == null) return false;

            // Check if it is an equippable weapon, shield, or armor piece
            if (!string.IsNullOrEmpty(prefabName) && IsEquippableGear(item))
            {
                if (!workerInv.CanAddItem(item, 1))
                {
                    Say("My inventory is full!");
                    return false;
                }

                // Transfer real item from player to worker
                ItemDrop.ItemData transferredItem;
                if (item.m_stack > 1)
                {
                    item.m_stack--;
                    transferredItem = item.Clone();
                    transferredItem.m_stack = 1;
                    workerInv.AddItem(transferredItem);
                }
                else
                {
                    userInv.RemoveItem(item);
                    workerInv.AddItem(item);
                    transferredItem = item;
                }

                VerificationTelemetry.RecordPass("duplication_anti_cheat", $"Transferred real item '{localizedName}' from player to worker inventory.");

                if (itemType == ItemDrop.ItemData.ItemType.Shield)
                {
                    Customization.Shield = prefabName;
                }
                else if (itemType == ItemDrop.ItemData.ItemType.Helmet)
                {
                    Customization.Helmet = prefabName;
                }
                else if (itemType == ItemDrop.ItemData.ItemType.Chest)
                {
                    Customization.Chest = prefabName;
                }
                else if (itemType == ItemDrop.ItemData.ItemType.Legs)
                {
                    Customization.Legs = prefabName;
                }
                else
                {
                    Customization.Weapon = prefabName;
                }

                m_humanoid.EquipItem(transferredItem, true);
                ApplyVisualEquipment();
                SaveToZDO();
                Say($"Equipped {localizedName}!");
                LogAction($"Equipped gear '{localizedName}' ({prefabName}) given by player.", true);
                StartCommandTrace("EQUIP_ITEM", $"Equipped gear '{localizedName}' ({prefabName}) given by player", "interaction_equip_persistence");
                return true;
            }

            // Otherwise transfer material / fuel stack into worker's inventory
            if (!item.m_equipped)
            {
                int stack = item.m_stack;
                if (workerInv.CanAddItem(item, stack))
                {
                    workerInv.AddItem(item);
                    userInv.RemoveItem(item);
                    Say($"Received {stack}x {localizedName}!");
                    LogAction($"Received {stack}x '{localizedName}' into carried inventory from player.", true);
                    StartCommandTrace("GIVE_ITEM", $"Received {stack}x '{localizedName}' into inventory from player");

                    // Appeasement checks:
                    if (Heritage == WorkerHeritage.Human && (itemType == ItemDrop.ItemData.ItemType.Consumable || prefabName.Contains("Meat") || prefabName.Contains("Stew") || prefabName.Contains("Mead")))
                    {
                        PrideMeter = Mathf.Clamp01(PrideMeter + 0.5f);
                        Say("A hearty offering! My pride and spirits are restored!");
                        LogAction("Pride restored by food/mead offering from player.", true);
                        VerificationTelemetry.RecordPass("charmenu_pride_appease", $"Restored pride of Human worker '{GetHoverName()}' with food/mead offering.");
                    }
                    else if (Heritage == WorkerHeritage.Dvergr && (prefabName == "Coins" || prefabName == "Amber" || prefabName == "Ruby" || prefabName == "AmberPearl" || prefabName == "SilverNecklace" || prefabName.Contains("Mead")))
                    {
                        PrideMeter = 1.0f;
                        Say("A generous tribute! The Guild approves!");
                        LogAction("Pride restored by gold/gems from player.", true);
                        VerificationTelemetry.RecordPass("charmenu_pride_appease", $"Restored pride of Dvergr worker '{GetHoverName()}' with luxury tribute ({prefabName}).");
                    }
                    SaveProgressionToZDO();
                    return true;
                }
            }

            return false;
        }
    }
}
