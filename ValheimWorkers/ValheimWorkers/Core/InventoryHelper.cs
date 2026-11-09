using System;
using UnityEngine;

namespace ValheimWorkers.Core
{
    public static class InventoryHelper
    {
        public static bool HasMatchingItem(Inventory inv, ItemDrop.ItemData itemToMatch)
        {
            if (inv == null || itemToMatch == null) return false;
            string matchPrefab = itemToMatch.m_dropPrefab != null ? itemToMatch.m_dropPrefab.name : "";
            string matchShared = itemToMatch.m_shared != null ? itemToMatch.m_shared.m_name : "";

            foreach (var invItem in inv.GetAllItems())
            {
                if (invItem == null) continue;
                string pName = invItem.m_dropPrefab != null ? invItem.m_dropPrefab.name : "";
                string sName = invItem.m_shared != null ? invItem.m_shared.m_name : "";

                if (!string.IsNullOrEmpty(matchPrefab) && string.Equals(pName, matchPrefab, StringComparison.OrdinalIgnoreCase))
                    return true;
                if (!string.IsNullOrEmpty(matchShared) && string.Equals(sName, matchShared, StringComparison.OrdinalIgnoreCase))
                    return true;
                if (!string.IsNullOrEmpty(matchPrefab) && string.Equals(sName, matchPrefab, StringComparison.OrdinalIgnoreCase))
                    return true;
                if (!string.IsNullOrEmpty(matchShared) && string.Equals(pName, matchShared, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        public static bool HasItemPrefab(Inventory inv, string prefabName)
        {
            return GetItemByPrefab(inv, prefabName) != null;
        }

        public static ItemDrop.ItemData GetItemByPrefab(Inventory inv, string prefabName)
        {
            if (inv == null || string.IsNullOrEmpty(prefabName)) return null;
            foreach (var item in inv.GetAllItems())
            {
                if (item == null) continue;
                string pName = item.m_dropPrefab != null ? item.m_dropPrefab.name : "";
                if (string.Equals(pName, prefabName, StringComparison.OrdinalIgnoreCase))
                    return item;

                if (item.m_shared != null && string.Equals(item.m_shared.m_name, prefabName, StringComparison.OrdinalIgnoreCase))
                    return item;

                if (item.m_shared != null && item.m_shared.m_name.StartsWith("$item_"))
                {
                    string candidate = item.m_shared.m_name.Substring(6);
                    if (string.Equals(candidate.Replace("_", ""), prefabName.Replace("_", ""), StringComparison.OrdinalIgnoreCase))
                        return item;
                }
            }
            return null;
        }
    }
}
