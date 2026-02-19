using System.Collections.Generic;
using EnumerateUtility;
using Google.Protobuf.Collections;
using Message.Gate.Protocol.GateC;
using ServerModels;
using ServerShared;

namespace ZoneServerLib
{
    public class TowerTaskInfo
    {
        private TowerManager manager;
        public int Id => Model.Id;
        public TowerTaskType TowerTaskType => Model.Type;
        public TowerTaskModel Model { get; private set; }
        public List<int> param { get; set; }
        public bool ShopFreshed { get; set; }

        public TowerTaskInfo(TowerManager manager, TowerTaskModel model)
        {
            this.Model = model;
            this.manager = manager;
        }

        public TowerTaskInfo(TowerManager manager, TowerTaskModel model, List<int> paramList)
        {
            this.Model = model;
            this.manager = manager;

            param = new List<int>();

            if (paramList.Count < 1) return;

            paramList.RemoveAt(0);

            //对于商店来说参数为 是否刷新-itemid1-itemid2-itemid3
            if (model.Type == TowerTaskType.Shop)
            {
                if ((paramList.Count == TowerLibrary.ShopItemCount + 1) || paramList.Count == 1)
                { 
                    ShopFreshed = paramList[0] == 1;
                    paramList.RemoveAt(0);
                }
            }

            param.AddRange(paramList);
        }

        public bool NeedFreshShopItem()
        {
            return Model.Type == TowerTaskType.Shop && !ShopFreshed;
        }

        public override string ToString()
        {
            List<int> paramList = new List<int>() { Id };
            if (Model.Type == TowerTaskType.Shop)
            {
                paramList.Add(ShopFreshed ? 1 : 0);
            }
            paramList.AddRange(param);
            return string.Join("-", paramList);
        }

        public MSG_TOWER_TASK_INFO GenerateMsg()
        {
            MSG_TOWER_TASK_INFO msg = new MSG_TOWER_TASK_INFO();
            msg.TaskId = Id;

            switch (TowerTaskType)
            {
                case TowerTaskType.Dungeon:
                    if (param.Count > 0)
                    {
                        msg.DungeonId = param[0];
                    }
                    break;
                case TowerTaskType.Shop:
                    BuildMsg(msg.ShopItems);
                    break;
            }

            return msg;
        }

        public void BuildMsg(RepeatedField<MSG_TOWER_TASK_ITEM_INFO> msg)
        {
            if (manager.CurrNodeHaveShop() && Model.NodeId == manager.NodeId +1)
            {
                for (int i = 0; i < param.Count; i++)
                {
                    BuildMsg(msg, param[i]);
                }
            }
        }

        private void BuildMsg(RepeatedField<MSG_TOWER_TASK_ITEM_INFO> msg, int itemId)
        {
            MSG_TOWER_TASK_ITEM_INFO itemInfo = new MSG_TOWER_TASK_ITEM_INFO() { ItemId = itemId };
            CommonShopItemModel shopItem = CommonShopLibrary.GetShopItemModel(itemId);

            if (shopItem == null)
            {
                Logger.Log.Warn($"配置表信息有误 TowerTaskType {TowerTaskType} taskId {Id} 商品id {itemId}");
                return;
            }

            ItemBasicInfo basicItem = ItemBasicInfo.Parse(shopItem.Reward);
            switch ((RewardType)basicItem.RewardType)
            {
                case RewardType.SoulBone:
                    ItemBasicInfo cacheBasicInfo;
                    if (manager.SoulBoneList.TryGetValue(basicItem.Id, out cacheBasicInfo))
                    {
                        basicItem = cacheBasicInfo;
                    }
                    else
                    {
                        //随机的魂骨需要保存
                        manager.DBChanged = true;
                        manager.SoulBoneList[basicItem.Id] = basicItem;
                    }
                    SoulBone soulBone = SoulBoneManager.GenerateSoulBoneInfo(basicItem, true);
                    if (soulBone != null)
                    {
                        itemInfo.SoulBone = SoulBoneManager.GenerateSoulBoneMsg(soulBone);
                    }
                    break;
                case RewardType.Equip:
                    MSG_ZGC_ITEM_EQUIPMENT euqipmentMsg = CommonShopItem.GenerateEquipmentMsg(basicItem);
                    itemInfo.Equip = CommonShopItem.EquipmentAndScoreMsg(euqipmentMsg);
                    break;
                default:
                    break;
            }

            msg.Add(itemInfo);
        }
    }
}
