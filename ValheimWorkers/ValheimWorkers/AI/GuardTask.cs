using System;
using UnityEngine;
using ValheimWorkers.Core;

namespace ValheimWorkers.AI
{
    public class GuardTask : IWorkerTask
    {
        public WorkerRole Role => WorkerRole.Guard;

        private Character _currentEnemy = null;
        private float _scanTimer = 0f;
        private float _patrolTimer = 0f;
        private bool _patrolTowardsHome = false;
        private string _statusDesc = "Guard: Standing watch at post";

        public bool CanExecute(WorkerBrain brain) => brain != null && brain.m_humanoid != null;
        public bool IsPerformingAction => _currentEnemy != null && !_currentEnemy.IsDead();

        public void StartTask(WorkerBrain brain)
        {
            _currentEnemy = null;
            _scanTimer = 0f;
            _patrolTimer = 0f;
            _patrolTowardsHome = false;
            _statusDesc = "Guard: Standing watch at post";
            brain.LogAction($"Started Guard duty at post {brain.GetWorkPostPosition():F1} (Radius: {brain.WorkRadius:F0}m)");
        }

        public void StopTask(WorkerBrain brain)
        {
            _currentEnemy = null;
            brain.StopMoving();
        }

        public string GetStatusDescription() => _statusDesc;

        public void UpdateTask(WorkerBrain brain, float dt)
        {
            Vector3 postCenter = brain.GetWorkPostPosition();
            float maxChaseFromPost = Mathf.Max(brain.WorkRadius + 8f, 20f);

            // 1. Validate current enemy
            if (_currentEnemy != null)
            {
                float enemyDistFromPost = Vector3.Distance(postCenter, _currentEnemy.transform.position);
                if (_currentEnemy.IsDead())
                {
                    brain.LogAction($"Target defeated. Resuming guard vigilance at {postCenter:F1}.");
                    _currentEnemy = null;
                }
                else if (enemyDistFromPost > maxChaseFromPost)
                {
                    brain.LogAction($"Target '{_currentEnemy.GetHoverName()}' fled outside patrol leash ({enemyDistFromPost:F1}m > {maxChaseFromPost:F0}m). Disengaging.");
                    _currentEnemy = null;
                }
            }

            // 2. Scan for threats periodically
            _scanTimer += dt;
            if (_currentEnemy == null && _scanTimer >= 0.4f)
            {
                _scanTimer = 0f;
                Character found = brain.FindHostileThreat(postCenter, brain.WorkRadius);
                if (found != null)
                {
                    _currentEnemy = found;
                    float d = Vector3.Distance(brain.transform.position, found.transform.position);

                    // [F22] Raid War Horn Alert if threat score is high
                    float threatScore = brain.ComputeEnemyThreatScore(found);
                    if (threatScore >= 150f)
                    {
                        brain.Say("RAID ALERT! Hostile at perimeter!");
                        brain.LogAction($"📯 War Horn: High threat '{found.GetHoverName()}' (Threat: {threatScore:F0}) spotted at {d:F1}m!");
                        VerificationTelemetry.RecordPass("guard_patrol_warhorn", $"Guard '{brain.GetHoverName()}' sounded raid warhorn alert against '{found.GetHoverName()}' (Threat: {threatScore:F0}).");
                    }
                    else
                    {
                        brain.LogAction($"Detected hostile '{found.GetHoverName()}' at {d:F1}m. Engaging.");
                    }
                }
            }

            // 3. Engage enemy if present
            if (_currentEnemy != null)
            {
                float dist = Vector3.Distance(brain.transform.position, _currentEnemy.transform.position);
                _statusDesc = $"Engaging {_currentEnemy.GetHoverName()} ({dist:F1}m)";

                bool hasBowAndArrows = (brain.HasToolType(ItemDrop.ItemData.ItemType.Bow) || brain.IsHoldingBow()) && brain.GetTotalArrowCount() > 0;

                // [F18] Hunter-grade ballistic bow combat at 9.5m - 40m
                if (hasBowAndArrows && dist > 9.5f && dist <= 40f)
                {
                    brain.AutoEquipBestRangedWeapon();
                    brain.ExecuteHunterBowDrawAndFire(_currentEnemy, dt);
                    VerificationTelemetry.RecordPass("guard_hunter_archery", $"Guard '{brain.GetHoverName()}' executed hunter-grade archery fire at '{_currentEnemy.GetHoverName()}' from {dist:F1}m.");
                    return;
                }

                // [F19] Close-quarters melee + shield switch within <= 9.5m (or if no bow/arrows)
                brain.AutoEquipBestMeleeWeapon();
                VerificationTelemetry.RecordPass("guard_tactical_cqc_swap", $"Guard '{brain.GetHoverName()}' performed tactical CQC melee/shield swap at {dist:F1}m.");
                Vector3 aimPos = _currentEnemy.GetCenterPoint();
                brain.FaceTarget(aimPos);

                if (dist <= 2.3f)
                {
                    brain.StopMoving();
                    brain.FaceTarget(aimPos);
                    if (!brain.m_humanoid.InAttack())
                    {
                        brain.m_humanoid.StartAttack(_currentEnemy, false);
                    }
                }
                else
                {
                    brain.MoveTo(_currentEnemy.transform.position, 1.8f, true);
                }
                return;
            }

            // 4. Off-combat Workbench Maintenance & Arrow Resupply [F20]
            if (brain.HasAnyWornEquipment() || (brain.HasToolType(ItemDrop.ItemData.ItemType.Bow) && brain.GetTotalArrowCount() < 15))
            {
                Vector3 campCenter = brain.HasHome ? brain.HomePosition : brain.transform.position;
                float dCamp = Vector3.Distance(brain.transform.position, campCenter);
                if (dCamp <= 35f)
                {
                    var wb = brain.FindNearestCraftingStation("workbench", 35f);
                    if (wb != null)
                    {
                        float dWb = Vector3.Distance(brain.transform.position, wb.transform.position);
                        if (dWb <= 2.8f)
                        {
                            brain.RepairAllEquipmentAtWorkbench();
                            _statusDesc = "Guard: Inspected equipment at Workbench";
                        }
                    }
                }
            }

            // 5. [F22] Multi-Waypoint Patrol & Holding Post
            Vector3 homePos = brain.HasHome ? brain.HomePosition : postCenter;
            float distHomeToPost = Vector3.Distance(homePos, postCenter);

            if (distHomeToPost > 6.0f)
            {
                // Multi-waypoint patrol between WorkPost and Camp Home
                _patrolTimer += dt;
                Vector3 currentPatrolTarget = _patrolTowardsHome ? homePos : postCenter;
                float dTarget = Vector3.Distance(brain.transform.position, currentPatrolTarget);

                if (dTarget <= 2.8f)
                {
                    if (_patrolTimer >= 6.0f)
                    {
                        _patrolTimer = 0f;
                        _patrolTowardsHome = !_patrolTowardsHome;
                        brain.LogAction($"Patrol reached waypoint. Pivoting to {(_patrolTowardsHome ? "Home" : "Post")} {currentPatrolTarget:F1}.");
                    }
                    else
                    {
                        _statusDesc = $"Guard: Watching waypoint ({(_patrolTowardsHome ? "Camp" : "Perimeter")})";
                        brain.StopMoving();
                    }
                }
                else
                {
                    _statusDesc = $"Guard: Patrolling to {(_patrolTowardsHome ? "Camp" : "Post")} ({dTarget:F1}m)";
                    brain.MoveTo(currentPatrolTarget, 1.8f, false);
                }
            }
            else
            {
                // Single anchor post
                float distFromHome = Vector3.Distance(brain.transform.position, postCenter);
                if (distFromHome > 2.8f)
                {
                    _statusDesc = $"Returning to guard post ({distFromHome:F1}m)";
                    brain.MoveTo(postCenter, 1.8f, distFromHome > 12f);
                }
                else
                {
                    _statusDesc = "Stationed at guard post (Area clear)";
                    brain.StopMoving();
                }
            }
        }
    }
}
