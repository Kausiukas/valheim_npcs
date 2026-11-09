using System;
using HarmonyLib;
using UnityEngine;
using ValheimWorkers.AI;
using ValheimWorkers.Core;
using ValheimWorkers.Prefabs;

namespace ValheimWorkers.Patches
{
    [HarmonyPatch]
    public static class BloodSacrificePatches
    {
        public static int GetBloodYieldForCreature(Character c)
        {
            if (c == null) return 10;
            string name = c.gameObject.name.ToLowerInvariant();
            if (name.Contains("neck") || name.Contains("seagull") || name.Contains("crow")) return 3;
            if (name.Contains("boar") || name.Contains("piggy")) return 10;
            if (name.Contains("deer")) return 15;
            if (name.Contains("wolf") || name.Contains("fenring")) return 25;
            if (name.Contains("troll") || name.Contains("bear") || name.Contains("bjorn")) return 45;
            if (name.Contains("lox")) return 75;
            return 10;
        }

        [HarmonyPatch(typeof(Character), nameof(Character.Damage))]
        [HarmonyPrefix]
        public static void Character_Damage_Prefix(Character __instance, HitData hit)
        {
            if (__instance == null || hit == null) return;

            // 1. Check if attacker is Player wielding the Sacrificial Dagger
            if (hit.GetAttacker() is Player player)
            {
                ItemDrop.ItemData weapon = player.GetCurrentWeapon();
                if (weapon != null && weapon.m_dropPrefab != null && weapon.m_dropPrefab.name == RelicPrefabManager.PrefabSacrificialDagger)
                {
                    // Like butcher's knife, guarantee lethal one-shot against tamed animals
                    if (__instance.IsTamed())
                    {
                        hit.m_damage.m_slash = 99999f;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Character), nameof(Character.Damage))]
        [HarmonyPostfix]
        public static void Character_Damage_Postfix(Character __instance, HitData hit)
        {
            if (__instance == null || hit == null) return;

            Character attacker = hit.GetAttacker();

            // A. Player attacking with Sacrificial Dagger
            if (attacker is Player player)
            {
                ItemDrop.ItemData weapon = player.GetCurrentWeapon();
                if (weapon != null && weapon.m_dropPrefab != null && weapon.m_dropPrefab.name == RelicPrefabManager.PrefabSacrificialDagger)
                {
                    if (__instance.IsDead())
                    {
                        ProcessSacrificialKill(player, __instance);
                    }
                }
            }

            // B. Skeleton Worker combat blood-feeding
            if (attacker != null)
            {
                var workerBrain = attacker.GetComponent<WorkerBrain>();
                if (workerBrain != null && workerBrain.Heritage == WorkerHeritage.Skeleton)
                {
                    // Only living bleeding creatures feed marrow (not undead or bosses)
                    if (!__instance.IsBoss() && __instance.m_faction != Character.Faction.Undead)
                    {
                        workerBrain.AddMarrowBlood(0.5f);

                        if (__instance.IsDead())
                        {
                            int yield = GetBloodYieldForCreature(__instance);
                            float combatBlood = Mathf.Clamp(yield * 0.25f, 2.0f, 6.0f);
                            workerBrain.AddMarrowBlood(combatBlood);
                            workerBrain.Say("*drinks blood*");
                            VerificationTelemetry.RecordPass("skeleton_combat_feed", $"Skeleton worker '{workerBrain.GetHoverName()}' absorbed {combatBlood:F1} Marrow Blood from slain {__instance.gameObject.name}.");
                            VerificationTelemetry.RecordPass("skilltree_skel_combat_bloodfeed", $"Skeleton worker '{workerBrain.GetHoverName()}' fed on blood during combat (+{combatBlood:F1} Marrow Blood).");
                        }
                    }
                }
            }

            // C. Viking's own spilled blood into Blood Chalice
            if (__instance is Player damagedPlayer && damagedPlayer == Player.m_localPlayer)
            {
                float dmg = hit.GetTotalDamage();
                if (dmg > 5f)
                {
                    int bloodCaught = Mathf.Clamp(Mathf.RoundToInt(dmg * 0.40f / 2.5f), 1, 20);
                    RelicPrefabManager.AddBloodToPlayerChalice(damagedPlayer, bloodCaught);
                }
            }
        }

        private static void ProcessSacrificialKill(Player player, Character victim)
        {
            Vector3 deathPos = victim.transform.position;
            int bloodYield = GetBloodYieldForCreature(victim);

            // Scan 3.5m radius for dropped BoneFragments
            Collider[] cols = Physics.OverlapSphere(deathPos, 3.5f);
            ItemDrop boneStack = null;

            foreach (var col in cols)
            {
                if (col == null) continue;
                var drop = col.GetComponentInParent<ItemDrop>();
                if (drop != null && drop.m_itemData != null && drop.m_itemData.m_dropPrefab != null)
                {
                    if (drop.m_itemData.m_dropPrefab.name == "BoneFragments" && drop.m_itemData.m_stack >= 10)
                    {
                        boneStack = drop;
                        break;
                    }
                }
            }

            if (boneStack != null)
            {
                // Consume 10 bone fragments
                var nview = boneStack.GetComponent<ZNetView>();
                if (boneStack.m_itemData.m_stack <= 10)
                {
                    if (nview != null && nview.IsValid()) nview.ClaimOwnership();
                    UnityEngine.Object.Destroy(boneStack.gameObject);
                }
                else
                {
                    boneStack.m_itemData.m_stack -= 10;
                }

                // Spawn effects
                GameObject bloodFx = ZNetScene.instance?.GetPrefab("vfx_blood_hit");
                if (bloodFx != null) UnityEngine.Object.Instantiate(bloodFx, deathPos, Quaternion.identity);

                GameObject lightFx = ZNetScene.instance?.GetPrefab("fx_Lightning");
                if (lightFx != null) UnityEngine.Object.Instantiate(lightFx, deathPos, Quaternion.identity);

                // Spawn Level 1 Skeleton worker
                WorkerPrefabManager.SpawnWorker("Worker_Skeleton", deathPos, Quaternion.identity, WorkerRole.Hauler);

                player.Message(MessageHud.MessageType.Center, "<color=#FF4444>★ The spilled blood animates the bones! A Skeleton Servant rises! ★</color>");
                WorkerProgressionManager.UnlockHeritage(WorkerHeritage.Skeleton, 1);
                VerificationTelemetry.RecordPass("sacrifice_raise_skeleton", $"Raised Skeleton Worker at {deathPos:F1} by sacrificing {victim.gameObject.name} over bones.");
                VerificationTelemetry.RecordPass("acquire_skel_dagger_boar", $"Sacrificed {victim.gameObject.name} over bones with sacrificial dagger, raising a Skeleton Worker!");
            }
            else
            {
                // No bones nearby - collect blood into player's Blood Chalice if carried
                int stored = RelicPrefabManager.AddBloodToPlayerChalice(player, bloodYield);
                if (stored > 0)
                {
                    VerificationTelemetry.RecordPass("acquire_skel_chalice_harvest", $"Collected {stored} blood into Blood Chalice from {victim.gameObject.name}.");
                }
                else
                {
                    player.Message(MessageHud.MessageType.TopLeft, $"Spilled {bloodYield} warm blood, but no bones were nearby to absorb it!");
                }
            }
        }

        // --- Active Chalice & Blood-Letting Rite Handling ---
        public static void HandlePlayerRelicInputs()
        {
            Player player = Player.m_localPlayer;
            if (player == null || player.IsDead()) return;
            if (InventoryGui.IsVisible() || TextInput.IsVisible()) return;
            if (Chat.instance != null && Chat.instance.HasFocus()) return;
            if (Console.instance != null && Console.IsVisible()) return;

            ItemDrop.ItemData weapon = player.GetCurrentWeapon();
            if (weapon == null || weapon.m_dropPrefab == null) return;

            string weaponPrefab = weapon.m_dropPrefab.name;

            // 1. Blood Chalice interactions
            if (weaponPrefab == RelicPrefabManager.PrefabBloodChalice)
            {
                // Left-Click: Throw blood on ground bones or existing skeleton
                if (Input.GetMouseButtonDown(0))
                {
                    ThrowChaliceBlood(player);
                }

                // Middle-Click (or G key): Blood-letting self-sacrifice rite
                if (Input.GetMouseButtonDown(2) || Input.GetKeyDown(KeyCode.G))
                {
                    PerformBloodLettingRite(player);
                }
            }
            // 2. Sacrificial Dagger Blood-Letting
            else if (weaponPrefab == RelicPrefabManager.PrefabSacrificialDagger)
            {
                if (Input.GetMouseButtonDown(2) || Input.GetKeyDown(KeyCode.G))
                {
                    PerformBloodLettingRite(player);
                }
            }
        }

        private static void PerformBloodLettingRite(Player player)
        {
            if (player.GetHealth() <= 35f)
            {
                player.Message(MessageHud.MessageType.Center, "<color=#FF4444>Too weak for the Blood-Letting Rite!</color>");
                return;
            }

            // Deal 30 physical damage to player
            HitData selfHit = new HitData();
            selfHit.m_damage.m_slash = 30f;
            selfHit.m_point = player.transform.position;
            player.Damage(selfHit);

            // Add 15 blood to Chalice
            int added = RelicPrefabManager.AddBloodToPlayerChalice(player, 15);
            player.Message(MessageHud.MessageType.Center, $"<color=#FF4444>You slice your palm in an ancient blood-rite (+{added} Blood into Chalice).</color>");
            VerificationTelemetry.RecordPass("blood_letting_rite", $"Player performed Blood-Letting Rite for {added} Blood.");
            VerificationTelemetry.RecordPass("acquire_skel_chalice_selfbleed", $"Player performed Viking self-bleed ritual to charge Blood Chalice (+{added} Blood).");
        }

        private static void ThrowChaliceBlood(Player player)
        {
            int currentBlood = RelicPrefabManager.GetTotalPlayerChaliceBlood(player);
            if (currentBlood < 10)
            {
                player.Message(MessageHud.MessageType.Center, "<color=#FF8888>Not enough Blood stored in Chalice (Need 10+ Blood)!</color>");
                return;
            }

            Vector3 aimPos = player.transform.position + player.transform.forward * 2.5f;

            // Check if aiming at an existing Skeleton worker
            Ray ray = Camera.main != null ? Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f)) : new Ray(player.GetEyePoint(), player.GetLookDir());
            if (Physics.Raycast(ray, out RaycastHit hit, 5.0f))
            {
                var brain = hit.collider.GetComponentInParent<WorkerBrain>();
                if (brain != null && brain.Heritage == WorkerHeritage.Skeleton)
                {
                    if (RelicPrefabManager.ConsumeBloodFromPlayerChalice(player, 15))
                    {
                        brain.AddMarrowBlood(15f);
                        brain.Say("*bones absorb warm blood*");
                        player.Message(MessageHud.MessageType.Center, $"<color=#FF4444>Poured 15 Blood into {brain.GetHoverName()}'s marrow!</color>");
                        return;
                    }
                }
            }

            // Check for dropped bones on the ground
            Collider[] cols = Physics.OverlapSphere(aimPos, 3.5f);
            ItemDrop targetBones = null;

            foreach (var col in cols)
            {
                if (col == null) continue;
                var drop = col.GetComponentInParent<ItemDrop>();
                if (drop != null && drop.m_itemData != null && drop.m_itemData.m_dropPrefab != null)
                {
                    if (drop.m_itemData.m_dropPrefab.name == "BoneFragments" && drop.m_itemData.m_stack >= 10)
                    {
                        targetBones = drop;
                        break;
                    }
                }
            }

            if (targetBones != null)
            {
                if (RelicPrefabManager.ConsumeBloodFromPlayerChalice(player, 10))
                {
                    Vector3 spawnPos = targetBones.transform.position;
                    var nview = targetBones.GetComponent<ZNetView>();
                    if (targetBones.m_itemData.m_stack <= 10)
                    {
                        if (nview != null && nview.IsValid()) nview.ClaimOwnership();
                        UnityEngine.Object.Destroy(targetBones.gameObject);
                    }
                    else
                    {
                        targetBones.m_itemData.m_stack -= 10;
                    }

                    // Effects & spawn
                    GameObject bloodFx = ZNetScene.instance?.GetPrefab("vfx_blood_hit");
                    if (bloodFx != null) UnityEngine.Object.Instantiate(bloodFx, spawnPos, Quaternion.identity);

                    WorkerPrefabManager.SpawnWorker("Worker_Skeleton", spawnPos, Quaternion.identity, WorkerRole.Hauler);
                    player.Message(MessageHud.MessageType.Center, "<color=#FF4444>★ Blood poured on bones! A Skeleton Servant rises! ★</color>");
                    WorkerProgressionManager.UnlockHeritage(WorkerHeritage.Skeleton, 1);
                    VerificationTelemetry.RecordPass("acquire_skel_chalice_pour", $"Poured Blood Chalice onto bone fragments at {spawnPos:F1}, raising a Skeleton Worker!");
                }
            }
            else
            {
                player.Message(MessageHud.MessageType.Center, "No BoneFragments (10+) found in front of you to raise!");
            }
        }
    }
}
