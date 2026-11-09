using System.Collections.Generic;

namespace ValheimWorkers.Core
{
    public class WorkerArchetype
    {
        public string Id { get; set; } = "Worker_Peasant";
        public string DisplayName { get; set; } = "Settlement Worker";
        public string BasePrefab { get; set; } = "Dvergr";
        public string FallbackPrefab { get; set; } = "Skeleton_NoArcher";
        public string HireCostItem { get; set; } = "Coins";
        public int HireCostAmount { get; set; } = 50;
        public WorkerRole DefaultRole { get; set; } = WorkerRole.Hauler;
        public List<WorkerRole> AllowedRoles { get; set; } = new List<WorkerRole>
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
        public float MaxHealth { get; set; } = 150f;
        public float SpeedModifier { get; set; } = 1.0f;
        public int ToolTier { get; set; } = 1;
        public string DefaultWeapon { get; set; } = "Club";
        public string DefaultShield { get; set; } = "";
        public string DefaultHelmet { get; set; } = "HelmetLeather";
        public string DefaultChest { get; set; } = "ArmorLeatherChest";
        public string DefaultLegs { get; set; } = "ArmorLeatherLegs";
    }
}
