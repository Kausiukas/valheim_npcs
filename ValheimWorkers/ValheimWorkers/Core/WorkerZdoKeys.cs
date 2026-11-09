namespace ValheimWorkers.Core
{
    public static class WorkerZdoKeys
    {
        public static readonly int RoleHash = "ValheimWorkers_Role".GetStableHashCode();
        public static readonly int HomePosHash = "ValheimWorkers_HomePos".GetStableHashCode();
        public static readonly int HasHomeHash = "ValheimWorkers_HasHome".GetStableHashCode();
        public static readonly int WorkPostPosHash = "ValheimWorkers_WorkPostPos".GetStableHashCode();
        public static readonly int HasWorkPostHash = "ValheimWorkers_HasWorkPost".GetStableHashCode();
        public static readonly int OwnerPlayerHash = "ValheimWorkers_OwnerPlayer".GetStableHashCode();
        public static readonly int ArchetypeHash = "ValheimWorkers_Archetype".GetStableHashCode();
        public static readonly int WorkRadiusHash = "ValheimWorkers_WorkRadius".GetStableHashCode();
        public static readonly int ReturnThresholdHash = "ValheimWorkers_ReturnThreshold".GetStableHashCode();
        public static readonly int DesignatedResourcesHash = "ValheimWorkers_DesignatedResources".GetStableHashCode();

        // Progression, Skill Tree, Pride & Evolution
        public static readonly int WorkerTierHash = "ValheimWorkers_Tier".GetStableHashCode();
        public static readonly int LevelHash = "ValheimWorkers_Level".GetStableHashCode();
        public static readonly int XpHash = "ValheimWorkers_Xp".GetStableHashCode();
        public static readonly int SkillPointsHash = "ValheimWorkers_SkillPoints".GetStableHashCode();
        public static readonly int SkillsBitmaskHash = "ValheimWorkers_SkillsBitmask".GetStableHashCode();
        public static readonly int MarrowBloodHash = "ValheimWorkers_MarrowBlood".GetStableHashCode();
        public static readonly int PrideMeterHash = "ValheimWorkers_PrideMeter".GetStableHashCode();
        public static readonly int EvolutionFormHash = "ValheimWorkers_EvolutionForm".GetStableHashCode();
        public static readonly int DvergrLuxuryCountHash = "ValheimWorkers_DvergrLuxury".GetStableHashCode();

        // Valheim 0-100 Activity Skills Hashes
        public static readonly int[] ActivitySkillLevelHashes = new int[]
        {
            "VW_Skill_Wood".GetStableHashCode(),
            "VW_Skill_Mine".GetStableHashCode(),
            "VW_Skill_Melee".GetStableHashCode(),
            "VW_Skill_Block".GetStableHashCode(),
            "VW_Skill_Bow".GetStableHashCode(),
            "VW_Skill_Farm".GetStableHashCode(),
            "VW_Skill_Craft".GetStableHashCode(),
            "VW_Skill_Athl".GetStableHashCode()
        };

        public static readonly int[] ActivitySkillXpHashes = new int[]
        {
            "VW_Skill_Wood_Xp".GetStableHashCode(),
            "VW_Skill_Mine_Xp".GetStableHashCode(),
            "VW_Skill_Melee_Xp".GetStableHashCode(),
            "VW_Skill_Block_Xp".GetStableHashCode(),
            "VW_Skill_Bow_Xp".GetStableHashCode(),
            "VW_Skill_Farm_Xp".GetStableHashCode(),
            "VW_Skill_Craft_Xp".GetStableHashCode(),
            "VW_Skill_Athl_Xp".GetStableHashCode()
        };
    }
}
