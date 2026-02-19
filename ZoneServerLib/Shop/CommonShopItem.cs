using CommonUtility;
using EnumerateUtility;
using Message.Gate.Protocol.GateC;
using ScriptFunctions;
using ServerModels;
using ServerShared;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public class CommonShopItem : ShopItem//通用商品类
    {
        //商城商品表id，非道具id
        public int ShopItemId { get; private set; }

        public CommonShopItem(DBShopItemInfo dbShopItemInfo) : base(dbShopItemInfo.Id, dbShopItemInfo.BuyCount, dbShopItemInfo.ItemInfo)
        {
            this.ShopItemId = dbShopItemInfo.ShopItemId;
        }

        public override MSG_ZGC_SHOP_ITEM GenerateMsg()
        {        
            MSG_ZGC_SHOP_ITEM msg = new MSG_ZGC_SHOP_ITEM();
            msg.Id = ShopItemId;
            msg.BuyNum = BuyCount;
            msg.ItemInfo = string.Empty;

            CommonShopItemModel shopItem = CommonShopLibrary.GetShopItemModel(ShopItemId);
            ItemBasicInfo basicItem = ItemBasicInfo.Parse(shopItem.Reward);
            switch ((RewardType)basicItem.RewardType)
            {
                case RewardType.Equip:
                    MSG_ZGC_ITEM_EQUIPMENT euqipmentMsg = GenerateEquipmentMsg(basicItem);
                    msg.EquipmentInfo = EquipmentAndScoreMsg(euqipmentMsg);
                    break;
                case RewardType.HiddenWeapon:
                    MSG_ZGC_ITEM_EQUIPMENT euqipmentMsg1 = new MSG_ZGC_ITEM_EQUIPMENT()
                    {
                        Id = basicItem.Id,
                        PileNum = 1, 
                        EquipedHeroId = -1, 
                        PartType = 5,
                        HiddenWeapon =  new ZGC_HIDDEN_WEAPON_INFO()
                    };

                    msg.EquipmentInfo = HiddenWeaponItem.HiddenWeaponScoreMsg(euqipmentMsg1, null);
                    break;
                default:
                    break;
            }
            return msg;
        }      

        public static MSG_ZGC_ITEM_EQUIPMENT GenerateEquipmentMsg(ItemBasicInfo basicItem)
        {
            EquipmentInfo info = new EquipmentInfo()
            {
                TypeId = basicItem.Id,
                PileNum = 1,
                EquipHeroId = -1,
            };
            EquipmentItem item = new EquipmentItem(info);

            MSG_ZGC_ITEM_EQUIPMENT syncMsg = new MSG_ZGC_ITEM_EQUIPMENT()
            {          
                Id = item.Id,
                PileNum = item.PileNum,
                EquipedHeroId = item.EquipInfo.EquipHeroId,
                PartType = item.Model.Part,
            };
            return syncMsg;
        }

        public static MSG_ZGC_ITEM_EQUIPMENT EquipmentAndScoreMsg(MSG_ZGC_ITEM_EQUIPMENT msg)
        {
            //评分
            Dictionary<NatureType, long> dic = new Dictionary<NatureType, long>();
            EquipmentModel equipModel = EquipLibrary.GetEquipModel(msg.Id);
            if (equipModel != null)
            {
                foreach (var item in equipModel.BaseNatureDic)
                {
                    dic.Add(item.Key, item.Value);
                }
            }
            msg.Score = ScriptManager.BattlePower.CaculateItemScore2(dic);
            return msg;
        }
    }
}
