using System;
using System.Collections.Generic;
using UnityEngine;

namespace ValheimWorkers.Core
{
    [Serializable]
    public class WorkerCustomization
    {
        public string Name = "Viking Worker";
        public string BasePrefab = "Worker_Viking"; // Worker_Viking, Worker_Dverger, Worker_Skeleton
        public int ModelIndex = 0; // 0 = Male, 1 = Female
        public string HairStyle = "Hair1";
        public string BeardStyle = "Beard1";
        public Vector3 HairColor = new Vector3(0.9f, 0.75f, 0.45f); // Blonde default
        public Vector3 SkinColor = new Vector3(0.95f, 0.8f, 0.7f); // Nordic fair default

        public string Weapon = "Club";
        public string Shield = "";
        public string Chest = "ArmorLeatherChest";
        public string Legs = "ArmorLeatherLegs";
        public string Helmet = "HelmetLeather";

        public float MaxHealth = 150f;
        public float SpeedModifier = 1.0f;
        public float WorkRadius = 25f;
        public float InventoryReturnThreshold = 0.75f;
        public int ToolTier = 1;
        public WorkerRole InitialRole = WorkerRole.Hauler;
        public string DesignatedResources = "Wood,Stone";

        // Sound & Voice Settings
        public string VoiceProfile = "Human"; // Human, Dvergr, Skeleton, Silent
        public int IdleChatterMode = 0;       // 0 = Off, 1 = Rare (35s), 2 = Frequent (12s)
        public bool CombatShoutsEnabled = true;
        public bool FootstepsEnabled = true;
        public bool SpeechBubblesEnabled = true;

        // Stable ZDO hash keys
        public static readonly int NameHash = "VW_CustomName".GetStableHashCode();
        public static readonly int BasePrefabHash = "VW_BasePrefab".GetStableHashCode();
        public static readonly int ModelIndexHash = "VW_ModelIndex".GetStableHashCode();
        public static readonly int HairStyleHash = "VW_HairStyle".GetStableHashCode();
        public static readonly int BeardStyleHash = "VW_BeardStyle".GetStableHashCode();
        public static readonly int HairColorHash = "VW_HairColor".GetStableHashCode();
        public static readonly int SkinColorHash = "VW_SkinColor".GetStableHashCode();
        public static readonly int WeaponHash = "VW_Weapon".GetStableHashCode();
        public static readonly int ShieldHash = "VW_Shield".GetStableHashCode();
        public static readonly int ChestHash = "VW_Chest".GetStableHashCode();
        public static readonly int LegsHash = "VW_Legs".GetStableHashCode();
        public static readonly int HelmetHash = "VW_Helmet".GetStableHashCode();
        public static readonly int MaxHealthHash = "VW_MaxHealth".GetStableHashCode();
        public static readonly int SpeedModHash = "VW_SpeedMod".GetStableHashCode();
        public static readonly int VoiceProfileHash = "VW_VoiceProfile".GetStableHashCode();
        public static readonly int IdleChatterHash = "VW_IdleChatter".GetStableHashCode();
        public static readonly int CombatShoutsHash = "VW_CombatShouts".GetStableHashCode();
        public static readonly int FootstepsHash = "VW_Footsteps".GetStableHashCode();
        public static readonly int SpeechBubblesHash = "VW_SpeechBubbles".GetStableHashCode();

        public List<string> GetDesignatedResourceList()
        {
            var list = new List<string>();
            if (string.IsNullOrEmpty(DesignatedResources)) return list;
            string[] parts = DesignatedResources.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var p in parts)
            {
                string trimmed = p.Trim();
                if (!string.IsNullOrEmpty(trimmed) && !list.Contains(trimmed))
                {
                    list.Add(trimmed);
                }
            }
            return list;
        }

        public void SetDesignatedResourceList(IEnumerable<string> resources)
        {
            if (resources == null)
            {
                DesignatedResources = "";
                return;
            }
            DesignatedResources = string.Join(",", resources);
        }

        public WorkerCustomization Clone()
        {
            return (WorkerCustomization)MemberwiseClone();
        }

        public void SaveToZDO(ZDO zdo)
        {
            if (zdo == null) return;
            zdo.Set(NameHash, Name ?? "Viking Worker");
            zdo.Set(BasePrefabHash, BasePrefab ?? "Worker_Viking");
            zdo.Set(ModelIndexHash, ModelIndex);
            zdo.Set(HairStyleHash, HairStyle ?? "Hair1");
            zdo.Set(BeardStyleHash, BeardStyle ?? "Beard1");
            zdo.Set(HairColorHash, HairColor);
            zdo.Set(SkinColorHash, SkinColor);
            zdo.Set(WeaponHash, Weapon ?? "");
            zdo.Set(ShieldHash, Shield ?? "");
            zdo.Set(ChestHash, Chest ?? "");
            zdo.Set(LegsHash, Legs ?? "");
            zdo.Set(HelmetHash, Helmet ?? "");
            zdo.Set(MaxHealthHash, MaxHealth);
            zdo.Set(SpeedModHash, SpeedModifier);
            zdo.Set(WorkerZdoKeys.WorkRadiusHash, WorkRadius);
            zdo.Set(WorkerZdoKeys.ReturnThresholdHash, InventoryReturnThreshold);
            zdo.Set(WorkerZdoKeys.RoleHash, (int)InitialRole);
            zdo.Set(WorkerZdoKeys.DesignatedResourcesHash, DesignatedResources ?? "Wood,Stone");
            zdo.Set(VoiceProfileHash, VoiceProfile ?? "Human");
            zdo.Set(IdleChatterHash, IdleChatterMode);
            zdo.Set(CombatShoutsHash, CombatShoutsEnabled);
            zdo.Set(FootstepsHash, FootstepsEnabled);
            zdo.Set(SpeechBubblesHash, SpeechBubblesEnabled);
        }

        public static WorkerCustomization LoadFromZDO(ZDO zdo)
        {
            if (zdo == null) return new WorkerCustomization();

            var data = new WorkerCustomization
            {
                Name = zdo.GetString(NameHash, "Viking Worker"),
                BasePrefab = zdo.GetString(BasePrefabHash, "Worker_Viking"),
                ModelIndex = zdo.GetInt(ModelIndexHash, 0),
                HairStyle = zdo.GetString(HairStyleHash, "Hair1"),
                BeardStyle = zdo.GetString(BeardStyleHash, "Beard1"),
                HairColor = zdo.GetVec3(HairColorHash, new Vector3(0.9f, 0.75f, 0.45f)),
                SkinColor = zdo.GetVec3(SkinColorHash, new Vector3(0.95f, 0.8f, 0.7f)),
                Weapon = zdo.GetString(WeaponHash, "Club"),
                Shield = zdo.GetString(ShieldHash, ""),
                Chest = zdo.GetString(ChestHash, "ArmorLeatherChest"),
                Legs = zdo.GetString(LegsHash, "ArmorLeatherLegs"),
                Helmet = zdo.GetString(HelmetHash, "HelmetLeather"),
                MaxHealth = zdo.GetFloat(MaxHealthHash, 150f),
                SpeedModifier = zdo.GetFloat(SpeedModHash, 1.0f),
                WorkRadius = zdo.GetFloat(WorkerZdoKeys.WorkRadiusHash, 25f),
                InventoryReturnThreshold = zdo.GetFloat(WorkerZdoKeys.ReturnThresholdHash, 0.75f),
                InitialRole = (WorkerRole)zdo.GetInt(WorkerZdoKeys.RoleHash, (int)WorkerRole.Hauler),
                DesignatedResources = zdo.GetString(WorkerZdoKeys.DesignatedResourcesHash, "Wood,Stone"),
                VoiceProfile = zdo.GetString(VoiceProfileHash, "Human"),
                IdleChatterMode = zdo.GetInt(IdleChatterHash, 0),
                CombatShoutsEnabled = zdo.GetBool(CombatShoutsHash, true),
                FootstepsEnabled = zdo.GetBool(FootstepsHash, true),
                SpeechBubblesEnabled = zdo.GetBool(SpeechBubblesHash, true)
            };
            return data;
        }
    }
}
