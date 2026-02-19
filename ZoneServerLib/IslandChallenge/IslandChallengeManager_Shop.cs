using System.Collections.Generic;
using EnumerateUtility;
using Logger;
using ServerModels;
using ServerShared;
using System.Linq;

namespace ZoneServerLib
{
    public partial class IslandChallengeManager
    {
        #region  Shop

        public bool CurrNodeHaveShop()
        {
            int node = NodeId + 1;
            if (nodeTaskList.ContainsKey(node))
            {
                return nodeTaskList[node].Model.Type == TowerTaskType.Shop;
            }

            return false;
        }

        public bool RandomShopItem(IslandChallengeTaskInfo info)
        {
            if (info == null) return false;

            if (info.NeedFreshShopItem())
            {
                Owner.Cache5MaxBattlePowerHeroInfo();
                info.ShopRefreshed = true;
                BuildTaskParam(info.Model, info);

                SyncIslandChallengeToDB();
                return true;
            }
            return false;
        }

        private void BuildTaskParam(IslandChallengeTaskModel model, IslandChallengeTaskInfo info)
        {
            info.param = new List<int>();
            switch (model.Type)
            {
                case TowerTaskType.Dungeon:
                    {
                        info.param.Add(RandomDungeonId(model));
                    }
                    break;
                case TowerTaskType.Shop:
                    {
                        info.param.AddRange(RandomShopItem(model, Owner.MainTaskId));
                    }
                    break;
            }
        }

        private List<int> RandomShopItem(IslandChallengeTaskModel model, int mainTaskId)
        {
            List<int> itemIds = new List<int>();
            bool hadRandomSoulBone = false;

            for (int i = 0; i < IslandChallengeLibrary.ShopItemCount; i++)
            {
                TowerShopItemType itemType = IslandChallengeLibrary.RandomShopItemType(mainTaskId, hadRandomSoulBone);
                int quality = Owner.GetShopItemQuality(itemType);

                TowerShopItemModel itemModel = IslandChallengeLibrary.RandomShopItem(itemType, quality);

                if (itemModel == null)
                {
                    Log.Warn($"随机商品出错 itemtype {itemType} quality {quality}");
                }

                hadRandomSoulBone |= itemType == TowerShopItemType.SoulBone;
                itemIds.Add(itemModel?.Id ?? 1);
            }

            return itemIds;
        }

        public void AddBuyedShop(int id)
        {
            if (!ShopList.Contains(id))
            {
                ShopList.Add(id);
            }

            SyncShopItemToDB();
        }

        #endregion
    }
}
