using System;
using System.Collections.Generic;

namespace ValheimWorkers.Core
{
    public static class WorkerCatalog
    {
        private static readonly Dictionary<string, WorkerArchetype> _archetypes = new Dictionary<string, WorkerArchetype>(StringComparer.OrdinalIgnoreCase);

        static WorkerCatalog()
        {
            RegisterDefaultArchetypes();
        }

        public static void RegisterDefaultArchetypes()
        {
            _archetypes.Clear();

            // 1. Viking Worker (Humanoid Viking - Male or Female)
            Register(new WorkerArchetype
            {
                Id = "Worker_Viking",
                DisplayName = "Viking Worker",
                BasePrefab = "Player",
                FallbackPrefab = "Dverger",
                HireCostItem = "Coins",
                HireCostAmount = 50,
                DefaultRole = WorkerRole.Hauler,
                AllowedRoles = new List<WorkerRole>
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
                },
                MaxHealth = 150f,
                SpeedModifier = 1.0f,
                ToolTier = 1,
                DefaultWeapon = "Club",
                DefaultHelmet = "HelmetLeather",
                DefaultChest = "ArmorLeatherChest",
                DefaultLegs = "ArmorLeatherLegs"
            });

            // 2. Dvergr Worker
            Register(new WorkerArchetype
            {
                Id = "Worker_Dverger",
                DisplayName = "Dvergr Laborer",
                BasePrefab = "Dverger",
                FallbackPrefab = "Skeleton_NoArcher",
                HireCostItem = "Coins",
                HireCostAmount = 75,
                DefaultRole = WorkerRole.Hauler,
                AllowedRoles = new List<WorkerRole>
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
                },
                MaxHealth = 200f,
                SpeedModifier = 0.95f,
                ToolTier = 2,
                DefaultWeapon = "AxeBronze",
                DefaultHelmet = "HelmetBronze",
                DefaultChest = "ArmorBronzeChest",
                DefaultLegs = "ArmorBronzeLegs"
            });

            // 3. Skeleton Worker
            Register(new WorkerArchetype
            {
                Id = "Worker_Skeleton",
                DisplayName = "Skeleton Servant",
                BasePrefab = "Skeleton_NoArcher",
                FallbackPrefab = "Dverger",
                HireCostItem = "BoneFragments",
                HireCostAmount = 20,
                DefaultRole = WorkerRole.Hauler,
                AllowedRoles = new List<WorkerRole>
                {
                    WorkerRole.Hauler,
                    WorkerRole.Guard,
                    WorkerRole.Idle
                },
                MaxHealth = 80f,
                SpeedModifier = 1.0f,
                ToolTier = 1,
                DefaultWeapon = "Club"
            });

            // Aliases for backwards compatibility with console commands
            Register(new WorkerArchetype
            {
                Id = "Worker_Peasant",
                DisplayName = "Settlement Laborer",
                BasePrefab = "Player",
                FallbackPrefab = "Dverger",
                DefaultRole = WorkerRole.Hauler,
                MaxHealth = 120f,
                DefaultWeapon = "Club",
                DefaultChest = "ArmorRagsChest",
                DefaultLegs = "ArmorRagsLegs"
            });

            Register(new WorkerArchetype
            {
                Id = "Worker_Guard",
                DisplayName = "Settlement Guard",
                BasePrefab = "Player",
                FallbackPrefab = "Dverger",
                DefaultRole = WorkerRole.Guard,
                MaxHealth = 250f,
                DefaultWeapon = "SwordBronze",
                DefaultShield = "ShieldWood",
                DefaultHelmet = "HelmetBronze",
                DefaultChest = "ArmorBronzeChest",
                DefaultLegs = "ArmorBronzeLegs"
            });
        }

        public static void Register(WorkerArchetype archetype)
        {
            if (archetype == null || string.IsNullOrEmpty(archetype.Id)) return;
            _archetypes[archetype.Id] = archetype;
        }

        public static WorkerArchetype Get(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (_archetypes.TryGetValue(id, out var arch)) return arch;
            if (_archetypes.TryGetValue("Worker_" + id, out arch)) return arch;
            return null;
        }

        public static IEnumerable<WorkerArchetype> AllArchetypes => _archetypes.Values;
    }
}
