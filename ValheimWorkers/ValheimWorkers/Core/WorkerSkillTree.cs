using System;
using System.Collections.Generic;

namespace ValheimWorkers.Core
{
    public enum WorkerHeritage
    {
        Human,
        Skeleton,
        Dvergr
    }

    public enum TaskDignity
    {
        Menial = 1,     // Picking trash, loose sticks/stones, berries, fueling campfire
        Skilled = 2,    // Felling trees, farming, spit cooking
        Specialist = 3, // Mining ore, ballistic hunting, running smelters/kilns
        HighHonor = 4   // Guard combat, base structural repair, raid defense
    }

    public enum WorkerActivitySkill
    {
        Woodcutting = 0,
        Mining = 1,
        Melee = 2,
        Blocking = 3,
        Bows = 4,
        Farming = 5,
        Crafting = 6,
        Athletics = 7
    }

    public class ActivitySkillInfo
    {
        public WorkerActivitySkill Skill;
        public string DisplayName;
        public string Icon;
        public string Description;
        public string BonusFormat;
    }

    public class SkillNode
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string Branch { get; set; }
        public int Tier { get; set; } = 1;
        public int SkillPointCost { get; set; } = 1;
        public string RequiredParentId { get; set; } = "";
        public List<WorkerRole> GrantedRoles { get; set; } = new List<WorkerRole>();
        public float HealthBonus { get; set; } = 0f;
        public float SpeedBonus { get; set; } = 0f;
        public float CarryBonus { get; set; } = 0f;
    }

    public static class WorkerSkillTree
    {
        // -------------------------------------------------------------
        // Human Viking Skill Tree Nodes (4 Branches)
        // -------------------------------------------------------------
        public static readonly Dictionary<string, SkillNode> HumanSkills = new Dictionary<string, SkillNode>(StringComparer.OrdinalIgnoreCase)
        {
            // Branch 1: Woodsman & Forager
            {
                "human_wood_1", new SkillNode
                {
                    Id = "human_wood_1",
                    DisplayName = "Stick & Berry Picker",
                    Description = "Basic foraging instinct. Collects branches, loose stones, berries, and mushrooms.",
                    Branch = "Woodsman",
                    Tier = 1,
                    SkillPointCost = 0,
                    GrantedRoles = new List<WorkerRole> { WorkerRole.Hauler, WorkerRole.Gatherer }
                }
            },
            {
                "human_wood_2", new SkillNode
                {
                    Id = "human_wood_2",
                    DisplayName = "Axeman's Grip",
                    Description = "Learns to swing a woodcutter's axe. Unlocks Lumberjack to fell Beech, Fir, and Pine trees.",
                    Branch = "Woodsman",
                    Tier = 2,
                    SkillPointCost = 1,
                    RequiredParentId = "human_wood_1",
                    GrantedRoles = new List<WorkerRole> { WorkerRole.Lumberjack }
                }
            },
            {
                "human_wood_3", new SkillNode
                {
                    Id = "human_wood_3",
                    DisplayName = "Hardwood Clearcutter",
                    Description = "Can fell noble Birch and Oak trees (FineWood). Increases wood carry weight by +25%.",
                    Branch = "Woodsman",
                    Tier = 3,
                    SkillPointCost = 2,
                    RequiredParentId = "human_wood_2",
                    CarryBonus = 50f
                }
            },
            {
                "human_wood_4", new SkillNode
                {
                    Id = "human_wood_4",
                    DisplayName = "Yggdrasil's Harvester",
                    Description = "Master lumberjack. 25% chance for bonus wood drops, and equipped axes lose 50% less durability.",
                    Branch = "Woodsman",
                    Tier = 4,
                    SkillPointCost = 3,
                    RequiredParentId = "human_wood_3",
                    SpeedBonus = 0.10f
                }
            },

            // Branch 2: Hearth, Farm & Kitchen
            {
                "human_farm_1", new SkillNode
                {
                    Id = "human_farm_1",
                    DisplayName = "Campfire Stoker",
                    Description = "Knows how to keep campfires and torches lit with wood and resin.",
                    Branch = "Hearth",
                    Tier = 1,
                    SkillPointCost = 0,
                    GrantedRoles = new List<WorkerRole> { WorkerRole.BaseTender }
                }
            },
            {
                "human_farm_2", new SkillNode
                {
                    Id = "human_farm_2",
                    DisplayName = "Homestead Cultivator",
                    Description = "Unlocks Farmer role. Uses cultivator to plant crops, harvests mature plants, and feeds tamed animals.",
                    Branch = "Hearth",
                    Tier = 2,
                    SkillPointCost = 1,
                    RequiredParentId = "human_farm_1",
                    GrantedRoles = new List<WorkerRole> { WorkerRole.Farmer }
                }
            },
            {
                "human_farm_3", new SkillNode
                {
                    Id = "human_farm_3",
                    DisplayName = "Mead-Hall Chef",
                    Description = "Unlocks Cook role. Roasts meat without burning to coal, simmers Cauldron stews, and taps fermenters.",
                    Branch = "Hearth",
                    Tier = 3,
                    SkillPointCost = 2,
                    RequiredParentId = "human_farm_2",
                    GrantedRoles = new List<WorkerRole> { WorkerRole.Cook }
                }
            },
            {
                "human_farm_4", new SkillNode
                {
                    Id = "human_farm_4",
                    DisplayName = "Grand Feastmaster",
                    Description = "Master chef. Cooked foods have 20% chance of double yield; radiates +3 HP/s comfort aura to nearby workers.",
                    Branch = "Hearth",
                    Tier = 4,
                    SkillPointCost = 3,
                    RequiredParentId = "human_farm_3",
                    HealthBonus = 25f
                }
            },

            // Branch 3: Ranger & Delver
            {
                "human_hunt_1", new SkillNode
                {
                    Id = "human_hunt_1",
                    DisplayName = "Rock Scavenger",
                    Description = "Gathers surface flint and loose stones from the ground.",
                    Branch = "Ranger",
                    Tier = 1,
                    SkillPointCost = 0,
                    GrantedRoles = new List<WorkerRole> { WorkerRole.Gatherer }
                }
            },
            {
                "human_hunt_2", new SkillNode
                {
                    Id = "human_hunt_2",
                    DisplayName = "Quarry Striker",
                    Description = "Unlocks Miner role. Can use pickaxes to break down boulders and mine Copper and Tin veins.",
                    Branch = "Ranger",
                    Tier = 2,
                    SkillPointCost = 1,
                    RequiredParentId = "human_hunt_1",
                    GrantedRoles = new List<WorkerRole> { WorkerRole.Miner }
                }
            },
            {
                "human_hunt_3", new SkillNode
                {
                    Id = "human_hunt_3",
                    DisplayName = "Eagle-Eye Stalker",
                    Description = "Unlocks Hunter role. Equipped with bows, uses ballistic long-range archery to hunt Deer, Boar, and Birds.",
                    Branch = "Ranger",
                    Tier = 3,
                    SkillPointCost = 2,
                    RequiredParentId = "human_hunt_2",
                    GrantedRoles = new List<WorkerRole> { WorkerRole.Hunter }
                }
            },
            {
                "human_hunt_4", new SkillNode
                {
                    Id = "human_hunt_4",
                    DisplayName = "Apex Tracker",
                    Description = "Master hunter. +40% bow draw speed, can stalk and hunt Wolves and Lox, +1 extra pelt per kill.",
                    Branch = "Ranger",
                    Tier = 4,
                    SkillPointCost = 3,
                    RequiredParentId = "human_hunt_3",
                    SpeedBonus = 0.15f
                }
            },

            // Branch 4: Shield-Wall & Industry
            {
                "human_guard_1", new SkillNode
                {
                    Id = "human_guard_1",
                    DisplayName = "Torch Bearer",
                    Description = "Will fight small vermin (Greylings, Boars) but flees from dangerous monsters.",
                    Branch = "ShieldWall",
                    Tier = 1,
                    SkillPointCost = 0
                }
            },
            {
                "human_guard_2", new SkillNode
                {
                    Id = "human_guard_2",
                    DisplayName = "Kiln & Smelter Hand",
                    Description = "Can operate Charcoal Kilns and Smelters, and uses a Hammer to repair damaged camp buildings.",
                    Branch = "ShieldWall",
                    Tier = 2,
                    SkillPointCost = 1,
                    RequiredParentId = "human_guard_1",
                    GrantedRoles = new List<WorkerRole> { WorkerRole.BaseTender }
                }
            },
            {
                "human_guard_3", new SkillNode
                {
                    Id = "human_guard_3",
                    DisplayName = "Sworn Huscarl",
                    Description = "Unlocks Guard role. Patrols base borders, uses shields to block attacks, and blows Raid War Horn on attack.",
                    Branch = "ShieldWall",
                    Tier = 3,
                    SkillPointCost = 2,
                    RequiredParentId = "human_guard_2",
                    GrantedRoles = new List<WorkerRole> { WorkerRole.Guard },
                    HealthBonus = 50f
                }
            },
            {
                "human_guard_4", new SkillNode
                {
                    Id = "human_guard_4",
                    DisplayName = "Einherjar Warlord",
                    Description = "Champion of Odin. +100 Max HP, cleaving melee strikes, inspires nearby workers so they never panic.",
                    Branch = "ShieldWall",
                    Tier = 4,
                    SkillPointCost = 3,
                    RequiredParentId = "human_guard_3",
                    HealthBonus = 100f
                }
            }
        };

        // -------------------------------------------------------------
        // Skeleton Evolution Forms (Forms I - IV)
        // -------------------------------------------------------------
        public class SkeletonFormDef
        {
            public int FormIndex { get; set; }
            public string FormName { get; set; }
            public string Title { get; set; }
            public float BaseHp { get; set; }
            public float Scale { get; set; }
            public string VisualTint { get; set; }
            public int RequiredBlood { get; set; }
            public int RequiredBones { get; set; }
            public string ExtraItemPrefab { get; set; }
            public int ExtraItemAmount { get; set; }
            public List<WorkerRole> UnlockedRoles { get; set; } = new List<WorkerRole>();
        }

        public static readonly Dictionary<int, SkeletonFormDef> SkeletonForms = new Dictionary<int, SkeletonFormDef>
        {
            {
                1, new SkeletonFormDef
                {
                    FormIndex = 1,
                    FormName = "Rattling Thrall",
                    Title = "Tier I Skeleton",
                    BaseHp = 80f,
                    Scale = 1.0f,
                    VisualTint = "White",
                    RequiredBlood = 10,
                    RequiredBones = 10,
                    UnlockedRoles = new List<WorkerRole> { WorkerRole.Hauler, WorkerRole.Gatherer, WorkerRole.Guard, WorkerRole.Idle, WorkerRole.Follow }
                }
            },
            {
                2, new SkeletonFormDef
                {
                    FormIndex = 2,
                    FormName = "Sinew-Woven Laborer",
                    Title = "Tier II Skeleton",
                    BaseHp = 140f,
                    Scale = 1.06f,
                    VisualTint = "CrimsonTendons",
                    RequiredBlood = 25,
                    RequiredBones = 10,
                    ExtraItemPrefab = "RawMeat",
                    ExtraItemAmount = 4,
                    UnlockedRoles = new List<WorkerRole> { WorkerRole.Hauler, WorkerRole.Gatherer, WorkerRole.Guard, WorkerRole.Lumberjack, WorkerRole.Miner, WorkerRole.Hunter, WorkerRole.Idle, WorkerRole.Follow }
                }
            },
            {
                3, new SkeletonFormDef
                {
                    FormIndex = 3,
                    FormName = "Draugr-Grafted Hulk",
                    Title = "Tier III Skeleton",
                    BaseHp = 190f,
                    Scale = 1.12f,
                    VisualTint = "MossIronPlated",
                    RequiredBlood = 35,
                    RequiredBones = 15,
                    ExtraItemPrefab = "Entrails",
                    ExtraItemAmount = 4,
                    UnlockedRoles = new List<WorkerRole> { WorkerRole.Hauler, WorkerRole.Gatherer, WorkerRole.Guard, WorkerRole.Lumberjack, WorkerRole.Miner, WorkerRole.Hunter, WorkerRole.BaseTender, WorkerRole.Farmer, WorkerRole.Idle, WorkerRole.Follow }
                }
            },
            {
                4, new SkeletonFormDef
                {
                    FormIndex = 4,
                    FormName = "Crimson Revenant Lord",
                    Title = "Tier IV Apex Undead",
                    BaseHp = 280f,
                    Scale = 1.20f,
                    VisualTint = "ObsidianGlow",
                    RequiredBlood = 50,
                    RequiredBones = 20,
                    ExtraItemPrefab = "SurtlingCore",
                    ExtraItemAmount = 3,
                    UnlockedRoles = new List<WorkerRole> { WorkerRole.Hauler, WorkerRole.Gatherer, WorkerRole.Guard, WorkerRole.Lumberjack, WorkerRole.Miner, WorkerRole.Hunter, WorkerRole.BaseTender, WorkerRole.Farmer, WorkerRole.Idle, WorkerRole.Follow }
                }
            }
        };

        // -------------------------------------------------------------
        // Skeleton Anatomical Blood-Grafts
        // -------------------------------------------------------------
        public class SkeletonGraftDef
        {
            public string Id { get; set; }
            public string DisplayName { get; set; }
            public string Description { get; set; }
            public int RequiredBlood { get; set; }
            public int RequiredBones { get; set; }
            public float HealthBonus { get; set; } = 0f;
            public float CarryBonus { get; set; } = 0f;
            public float SpeedBonus { get; set; } = 0f;
        }

        public static readonly Dictionary<string, SkeletonGraftDef> SkeletonGrafts = new Dictionary<string, SkeletonGraftDef>(StringComparer.OrdinalIgnoreCase)
        {
            {
                "skel_graft_1", new SkeletonGraftDef
                {
                    Id = "skel_graft_1",
                    DisplayName = "Barrow-Mule Ribcage",
                    Description = "Reinforced ribcage and vertebrae. +100kg Carry Weight. Never tires.",
                    RequiredBlood = 15,
                    RequiredBones = 10,
                    CarryBonus = 100f
                }
            },
            {
                "skel_graft_2", new SkeletonGraftDef
                {
                    Id = "skel_graft_2",
                    DisplayName = "Scythe-Finger Digits",
                    Description = "Sharpened bone tips. +30% Harvesting and Woodcutting speed.",
                    RequiredBlood = 20,
                    RequiredBones = 10,
                    SpeedBonus = 0.15f
                }
            },
            {
                "skel_graft_3", new SkeletonGraftDef
                {
                    Id = "skel_graft_3",
                    DisplayName = "Hound-Skull Snout",
                    Description = "Animalistic senses detect hidden silver veins and crypt dungeons.",
                    RequiredBlood = 25,
                    RequiredBones = 15
                }
            },
            {
                "skel_graft_4", new SkeletonGraftDef
                {
                    Id = "skel_graft_4",
                    DisplayName = "Blight-Marrow Infusion",
                    Description = "Corrosive marrow heals +2 HP and inflicts poison on melee strikes.",
                    RequiredBlood = 35,
                    RequiredBones = 15,
                    HealthBonus = 40f
                }
            },
            {
                "skel_graft_5", new SkeletonGraftDef
                {
                    Id = "skel_graft_5",
                    DisplayName = "Bone-Splinter Retaliation",
                    Description = "Reflects 20% of incoming melee damage back to attackers.",
                    RequiredBlood = 40,
                    RequiredBones = 20
                }
            },
            {
                "skel_graft_6", new SkeletonGraftDef
                {
                    Id = "skel_graft_6",
                    DisplayName = "Soul-Tethered Phylactery",
                    Description = "Apex graft: Instantly reforms at home post if destroyed in combat.",
                    RequiredBlood = 60,
                    RequiredBones = 25,
                    HealthBonus = 80f
                }
            }
        };

        // -------------------------------------------------------------
        // Dvergr Luxury Patronage Nodes
        // -------------------------------------------------------------
        public class DvergrPatronageNode
        {
            public string Id { get; set; }
            public string DisplayName { get; set; }
            public string Description { get; set; }
            public int RequiredCoins { get; set; }
            public string RequiredMaterialPrefab { get; set; }
            public int RequiredMaterialAmount { get; set; }
            public string RequiredGemPrefab { get; set; }
            public int RequiredGemAmount { get; set; }
            public List<WorkerRole> GrantedRoles { get; set; } = new List<WorkerRole>();
            public float HealthBonus { get; set; } = 0f;
            public float SpeedBonus { get; set; } = 0f;
        }

        public static readonly Dictionary<string, DvergrPatronageNode> DvergrPatronage = new Dictionary<string, DvergrPatronageNode>(StringComparer.OrdinalIgnoreCase)
        {
            {
                "dvergr_mine", new DvergrPatronageNode
                {
                    Id = "dvergr_mine",
                    DisplayName = "Deep-Vein Certification",
                    Description = "Official dwarven mining rites. Unlocks Miner role with +25% pickaxe mining power.",
                    RequiredCoins = 40,
                    RequiredMaterialPrefab = "CopperOre",
                    RequiredMaterialAmount = 5,
                    GrantedRoles = new List<WorkerRole> { WorkerRole.Miner }
                }
            },
            {
                "dvergr_gem", new DvergrPatronageNode
                {
                    Id = "dvergr_gem",
                    DisplayName = "Jeweler's Eye (Gem-Finder)",
                    Description = "Sharp dwarven eye for precious stones. 15% chance to find bonus Amber, Rubies, or Coins when mining rock.",
                    RequiredCoins = 60,
                    RequiredGemPrefab = "Amber",
                    RequiredGemAmount = 3
                }
            },
            {
                "dvergr_smelt", new DvergrPatronageNode
                {
                    Id = "dvergr_smelt",
                    DisplayName = "Master Metallurgist's Seal",
                    Description = "Unlocks BaseTender for Smelters & Kilns. Loads 2x faster with 20% chance of an extra metal bar per batch.",
                    RequiredCoins = 75,
                    RequiredMaterialPrefab = "Bronze",
                    RequiredMaterialAmount = 5,
                    GrantedRoles = new List<WorkerRole> { WorkerRole.BaseTender }
                }
            },
            {
                "dvergr_anvil", new DvergrPatronageNode
                {
                    Id = "dvergr_anvil",
                    DisplayName = "Runic Anvil-Wright",
                    Description = "Unlocks autonomous camp repair with Hammer. Remotely hones all nearby workers' tools to prevent durability loss.",
                    RequiredCoins = 90,
                    RequiredMaterialPrefab = "Iron",
                    RequiredMaterialAmount = 5,
                    RequiredGemPrefab = "Ruby",
                    RequiredGemAmount = 2
                }
            },
            {
                "dvergr_brew", new DvergrPatronageNode
                {
                    Id = "dvergr_brew",
                    DisplayName = "Brew-Master & Mill-Engineer",
                    Description = "Operates Windmills, Spinning Wheels, and Fermenters at +50% processing speed.",
                    RequiredCoins = 80,
                    RequiredMaterialPrefab = "FineWood",
                    RequiredMaterialAmount = 10,
                    RequiredGemPrefab = "SilverNecklace",
                    RequiredGemAmount = 1
                }
            },
            {
                "dvergr_ward", new DvergrPatronageNode
                {
                    Id = "dvergr_ward",
                    DisplayName = "Svartalfheim Runic Ward",
                    Description = "Apex Guild Honor: +120 Max HP, heavy dwarven armor, and a glowing blue Ward Aura reducing damage taken by nearby workers.",
                    RequiredCoins = 150,
                    RequiredMaterialPrefab = "Silver",
                    RequiredMaterialAmount = 5,
                    RequiredGemPrefab = "Crystal",
                    RequiredGemAmount = 2,
                    GrantedRoles = new List<WorkerRole> { WorkerRole.Guard },
                    HealthBonus = 120f
                }
            }
        };

        // -------------------------------------------------------------
        // XP Calculation Helpers
        // -------------------------------------------------------------
        public static int GetXpForNextLevel(int currentLevel)
        {
            if (currentLevel < 1) return 100;
            // Level 1: 100, Level 2: 250, Level 3: 450, Level 4: 700, Level 5: 1000...
            return 50 * currentLevel * (currentLevel + 1);
        }

        public static readonly ActivitySkillInfo[] ActivitySkills = new ActivitySkillInfo[]
        {
            new ActivitySkillInfo { Skill = WorkerActivitySkill.Woodcutting, DisplayName = "Woodcutting", Icon = "🪓", Description = "Felling trees and chopping logs.", BonusFormat = "+{0:F1}% Chop Damage, +{1:F0}% Wood Drops" },
            new ActivitySkillInfo { Skill = WorkerActivitySkill.Mining, DisplayName = "Mining", Icon = "⛏️", Description = "Quarrying rock and mining metal ore veins.", BonusFormat = "+{0:F1}% Pickaxe Damage, +{1:F0}% Bonus Ore" },
            new ActivitySkillInfo { Skill = WorkerActivitySkill.Melee, DisplayName = "Melee Combat", Icon = "⚔️", Description = "Swords, axes, clubs, and knives in combat.", BonusFormat = "+{0:F1}% Slash/Blunt Damage, +{1:F0}% Stagger" },
            new ActivitySkillInfo { Skill = WorkerActivitySkill.Blocking, DisplayName = "Blocking", Icon = "🛡️", Description = "Shield block power and parrying incoming attacks.", BonusFormat = "+{0:F1}% Block Power, +{1:F1}x Parry Factor" },
            new ActivitySkillInfo { Skill = WorkerActivitySkill.Bows, DisplayName = "Bows & Archery", Icon = "🏹", Description = "Ranged marksmanship against wild beasts and raids.", BonusFormat = "+{0:F1}% Draw Speed, +{1:F1}% Pierce Damage" },
            new ActivitySkillInfo { Skill = WorkerActivitySkill.Farming, DisplayName = "Farming & Forage", Icon = "🌾", Description = "Cultivating, planting, and harvesting crops.", BonusFormat = "+{0:F1}% Harvest Speed, +{1:F0}% Seed Retention" },
            new ActivitySkillInfo { Skill = WorkerActivitySkill.Crafting, DisplayName = "Cooking & Crafting", Icon = "🔥", Description = "Spit cooking, cauldrons, fermenting, and smelting.", BonusFormat = "+{0:F1}% Smelt/Cook Speed, +{1:F0}% Double Portion" },
            new ActivitySkillInfo { Skill = WorkerActivitySkill.Athletics, DisplayName = "Athletics & Haul", Icon = "🥾", Description = "Running, load bearing, and hauling resources.", BonusFormat = "+{0:F1}% Movement Speed, +{1:F0}kg Carry Cap" }
        };

        public static float GetActivitySkillNextLevelXp(int level)
        {
            level = Math.Max(1, level);
            return (float)Math.Round(Math.Pow(level + 1, 1.5) * 0.5 + 1.0);
        }
    }
}
