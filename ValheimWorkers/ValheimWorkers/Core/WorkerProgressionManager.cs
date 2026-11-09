using System;
using System.Collections.Generic;
using UnityEngine;
using ValheimWorkers.AI;

namespace ValheimWorkers.Core
{
    public static class WorkerProgressionManager
    {
        public static bool SandboxUnlockAll { get; set; } = false;

        // --- Lineage Unlock Keys (stored in Player custom data & ZoneSystem) ---
        public const string KeySkeletonUnlocked = "VW_SkeletonUnlocked";
        public const string KeyHumanUnlocked = "VW_HumanUnlocked";
        public const string KeyDvergrUnlocked = "VW_DvergrUnlocked";

        public const string KeySkeletonTier = "VW_SkeletonTier";
        public const string KeyHumanTier = "VW_HumanTier";
        public const string KeyDvergrTier = "VW_DvergrTier";

        public static int GetLineageTier(WorkerHeritage heritage)
        {
            if (SandboxUnlockAll || (ValheimWorkersPlugin.DevMode != null && ValheimWorkersPlugin.DevMode.Value))
            {
                return 4;
            }

            if (Player.m_localPlayer == null) return 1;

            string key = heritage switch
            {
                WorkerHeritage.Skeleton => KeySkeletonTier,
                WorkerHeritage.Human => KeyHumanTier,
                WorkerHeritage.Dvergr => KeyDvergrTier,
                _ => KeyHumanTier
            };

            if (Player.m_localPlayer.m_customData.TryGetValue(key, out string valStr) && int.TryParse(valStr, out int tier))
            {
                return Mathf.Clamp(tier, 1, 4);
            }

            // Defaults: Humans start at Tier 1 (Lost Souls); Skeletons and Dvergr start locked (Tier 0) until unlocked
            return heritage == WorkerHeritage.Human ? 1 : 0;
        }

        public static void SetLineageTier(WorkerHeritage heritage, int tier)
        {
            if (Player.m_localPlayer == null) return;
            string key = heritage switch
            {
                WorkerHeritage.Skeleton => KeySkeletonTier,
                WorkerHeritage.Human => KeyHumanTier,
                WorkerHeritage.Dvergr => KeyDvergrTier,
                _ => KeyHumanTier
            };

            tier = Mathf.Clamp(tier, 0, 4);
            Player.m_localPlayer.m_customData[key] = tier.ToString();
            VerificationTelemetry.RecordPass("progression_tier_update", $"Set lineage {heritage} unlock tier to {tier}.");
        }

        public static bool IsHeritageUnlocked(WorkerHeritage heritage)
        {
            return GetLineageTier(heritage) > 0;
        }

        public static void UnlockHeritage(WorkerHeritage heritage, int initialTier = 1)
        {
            SetLineageTier(heritage, Mathf.Max(1, initialTier));
            if (Player.m_localPlayer != null)
            {
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, $"<color=#FFD700>★ Lineage Unlocked: {heritage} (Tier {initialTier}) ★</color>");
            }

            if (heritage == WorkerHeritage.Human)
            {
                if (initialTier == 1) VerificationTelemetry.RecordPass("acquire_human_t1_lost_soul", "Unlocked Human Tier 1: Lost Soul worker lineage.");
                else if (initialTier == 2) VerificationTelemetry.RecordPass("acquire_human_t2_homesteader", "Unlocked Human Tier 2: Homesteader worker lineage.");
                else if (initialTier == 3) VerificationTelemetry.RecordPass("acquire_human_t3_drengr", "Unlocked Human Tier 3: Veteran Drengr worker lineage.");
                else if (initialTier >= 4) VerificationTelemetry.RecordPass("acquire_human_t4_einherjar", "Unlocked Human Tier 4: Einherjar Champion worker lineage.");
            }
            else if (heritage == WorkerHeritage.Dvergr)
            {
                if (initialTier == 1) VerificationTelemetry.RecordPass("acquire_dvergr_t1_drudge", "Unlocked Dvergr Tier 1: Stranded Drudge worker lineage.");
                else if (initialTier == 2) VerificationTelemetry.RecordPass("acquire_dvergr_t2_prospector", "Unlocked Dvergr Tier 2: Deep-Vein Prospector worker lineage.");
                else if (initialTier == 3) VerificationTelemetry.RecordPass("acquire_dvergr_t3_smith", "Unlocked Dvergr Tier 3: Master Forge-Smith worker lineage.");
                else if (initialTier >= 4) VerificationTelemetry.RecordPass("acquire_dvergr_t4_sentinel", "Unlocked Dvergr Tier 4: Arcane Ward-Sentinel worker lineage.");
            }
        }

        // --- Role Permission & Capability Check ---
        public static bool CanPerformRole(WorkerBrain worker, WorkerRole role, out string reason)
        {
            reason = string.Empty;
            if (worker == null) return false;
            if (SandboxUnlockAll || (ValheimWorkersPlugin.DevMode != null && ValheimWorkersPlugin.DevMode.Value)) return true;

            // Idle and Follow are always permitted
            if (role == WorkerRole.Idle || role == WorkerRole.Follow) return true;

            WorkerHeritage heritage = worker.Heritage;

            // 1. Skeleton Lineage Check
            if (heritage == WorkerHeritage.Skeleton)
            {
                int formIndex = worker.EvolutionForm;
                if (!WorkerSkillTree.SkeletonForms.TryGetValue(formIndex, out var formDef))
                {
                    formDef = WorkerSkillTree.SkeletonForms[1];
                }

                if (formDef.UnlockedRoles.Contains(role)) return true;

                // Also check if granted by blood graft
                if (role == WorkerRole.Hunter && worker.HasSkill("skel_predator")) return true;

                reason = $"Requires Skeleton Evolution Form {GetRequiredFormForRole(role)} or Sacrificial Graft";
                return false;
            }

            // 2. Dvergr Lineage Check
            if (heritage == WorkerHeritage.Dvergr)
            {
                // Dvergr inherently can haul and gather loose items
                if (role == WorkerRole.Hauler || role == WorkerRole.Gatherer) return true;

                // Check patronage nodes
                foreach (var kvp in WorkerSkillTree.DvergrPatronage)
                {
                    if (kvp.Value.GrantedRoles.Contains(role) && worker.HasSkill(kvp.Key))
                    {
                        return true;
                    }
                }

                // Dvergr tier 2+ unlocks miner, tier 3+ unlocks basetender, tier 4 unlocks guard
                if (role == WorkerRole.Miner && (worker.WorkerTier >= 2 || worker.HasSkill("dvergr_mine"))) return true;
                if (role == WorkerRole.BaseTender && (worker.WorkerTier >= 3 || worker.HasSkill("dvergr_smelt"))) return true;
                if (role == WorkerRole.Guard && (worker.WorkerTier >= 4 || worker.HasSkill("dvergr_ward"))) return true;

                reason = $"Dvergr requires Guild Patronage or higher Tier for {role}";
                return false;
            }

            // 3. Human Lineage Check
            // Tier 1 Lost Souls can only Haul and Forage
            if (role == WorkerRole.Hauler) return true;
            if (role == WorkerRole.Gatherer) return true;

            // Check if granted by any unlocked skill
            foreach (var kvp in WorkerSkillTree.HumanSkills)
            {
                if (kvp.Value.GrantedRoles.Contains(role) && worker.HasSkill(kvp.Key))
                {
                    return true;
                }
            }

            // Tier-based fallbacks for pre-trained human tiers
            if (worker.WorkerTier >= 2 && (role == WorkerRole.Lumberjack || role == WorkerRole.Farmer || role == WorkerRole.Cook)) return true;
            if (worker.WorkerTier >= 3 && (role == WorkerRole.Miner || role == WorkerRole.Hunter)) return true;
            if (worker.WorkerTier >= 4 && (role == WorkerRole.Guard || role == WorkerRole.BaseTender)) return true;

            reason = $"Requires corresponding Skill Tree node or higher Tier (Current Tier: {worker.WorkerTier})";
            return false;
        }

        private static int GetRequiredFormForRole(WorkerRole role)
        {
            switch (role)
            {
                case WorkerRole.Lumberjack:
                case WorkerRole.Miner:
                case WorkerRole.Hunter:
                    return 2;
                case WorkerRole.BaseTender:
                case WorkerRole.Farmer:
                case WorkerRole.Cook:
                    return 3;
                default:
                    return 1;
            }
        }

        // --- Task Dignity & Pride Logic ---
        public static TaskDignity GetTaskDignity(WorkerRole role)
        {
            switch (role)
            {
                case WorkerRole.Hauler:
                case WorkerRole.Gatherer:
                case WorkerRole.Idle:
                case WorkerRole.Follow:
                    return TaskDignity.Menial;

                case WorkerRole.Lumberjack:
                case WorkerRole.Farmer:
                case WorkerRole.Cook:
                    return TaskDignity.Skilled;

                case WorkerRole.Miner:
                case WorkerRole.Hunter:
                case WorkerRole.BaseTender:
                    return TaskDignity.Specialist;

                case WorkerRole.Guard:
                    return TaskDignity.HighHonor;

                default:
                    return TaskDignity.Menial;
            }
        }

        public static void EvaluatePrideDecay(WorkerBrain worker, TaskDignity taskDignity, float dt)
        {
            if (worker == null) return;
            // Skeletons have zero ego
            if (worker.Heritage == WorkerHeritage.Skeleton)
            {
                worker.PrideMeter = 1.0f;
                return;
            }

            // Low-tier workers have no pride issues
            if (worker.WorkerTier <= 2)
            {
                worker.PrideMeter = 1.0f;
                return;
            }

            // Dvergr with luxury items in inventory have zero pride decay
            if (worker.Heritage == WorkerHeritage.Dvergr && worker.HasLuxuryItem())
            {
                worker.PrideMeter = 1.0f;
                return;
            }

            // If task is beneath their station (e.g., Tier 4 Einherjar hauling trash)
            int expectedDignity = worker.WorkerTier - 1;
            if ((int)taskDignity < expectedDignity)
            {
                // Decays over 300 seconds (5 minutes)
                float decayRate = 1.0f / 300f;
                worker.PrideMeter = Mathf.Clamp01(worker.PrideMeter - decayRate * dt);
                if (worker.PrideMeter < 0.25f)
                {
                    VerificationTelemetry.RecordPass("charmenu_pride_decay", $"Worker '{worker.GetHoverName()}' (Tier {worker.WorkerTier}) suffered Pride decay (Pride: {worker.PrideMeter * 100:F0}%) while performing menial work.");
                }
            }
            else
            {
                // Recovers pride when doing worthy tasks
                worker.PrideMeter = Mathf.Clamp01(worker.PrideMeter + 0.05f * dt);
            }
        }

        // --- Skeleton Visual Evolution Application ---
        public static void ApplySkeletonEvolutionVisuals(WorkerBrain worker)
        {
            if (worker == null || worker.Heritage != WorkerHeritage.Skeleton) return;

            int form = Mathf.Clamp(worker.EvolutionForm, 1, 4);
            if (!WorkerSkillTree.SkeletonForms.TryGetValue(form, out var formDef)) return;

            VerificationTelemetry.RecordPass("skilltree_skel_evolutions", $"Skeleton '{worker.GetHoverName()}' evolved into Form {form}: {formDef.FormName}.");

            // 1. Adjust scale
            worker.transform.localScale = Vector3.one * formDef.Scale;

            // 2. Adjust max health and character stats
            if (worker.m_character != null)
            {
                float baseHp = formDef.BaseHp;
                if (worker.HasSkill("skel_marrow_hard")) baseHp += 50f;
                worker.m_character.SetMaxHealth(baseHp);
                worker.m_character.SetHealth(baseHp);
            }

            // 3. Tint renderers
            Renderer[] renderers = worker.GetComponentsInChildren<Renderer>(true);
            Color tintColor = form switch
            {
                1 => Color.white,
                2 => new Color(1.0f, 0.65f, 0.65f), // Crimson red tint
                3 => new Color(0.65f, 0.85f, 0.65f), // Moss / iron tint
                4 => new Color(0.95f, 0.25f, 0.25f), // Obsidian glowing red
                _ => Color.white
            };

            foreach (var r in renderers)
            {
                if (r == null || r.material == null) continue;
                if (r.material.HasProperty("_Color"))
                {
                    r.material.color = tintColor;
                }
            }
        }
    }
}
