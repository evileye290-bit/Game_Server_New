using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateZ;
using System.IO;

namespace GateServerLib
{
    public partial class Client
    {
        public void OnResponse_GetShopInfo(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_SHOP_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_SHOP_INFO>(stream);
            MSG_GateZ_SHOP_INFO request = new MSG_GateZ_SHOP_INFO()
            {
                ShopType = msg.ShopType,
            };
            WriteToZone(request);
        }

        public void OnResponse_ShopBuyItem(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_SHOP_BUY_ITEM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_SHOP_BUY_ITEM>(stream);
            MSG_GateZ_SHOP_BUY_ITEM request = new MSG_GateZ_SHOP_BUY_ITEM()
            {
                ShopType = msg.ShopType,
                ItemId = msg.ItemId,
                BuyNum = msg.BuyNum,
            };
            WriteToZone(request);
        }

        public void OnResponse_ShopFresh(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_SHOP_REFRESH msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_SHOP_REFRESH>(stream);
            MSG_GateZ_SHOP_REFRESH request = new MSG_GateZ_SHOP_REFRESH()
            {
                ShopType = msg.ShopType,
            };
            WriteToZone(request);
        }

        public void OnResponse_ShopSoulBoneBonus(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_SHOP_SOULBONE_BONUS msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_SHOP_SOULBONE_BONUS>(stream);
            MSG_GateZ_SHOP_SOULBONE_BONUS request = new MSG_GateZ_SHOP_SOULBONE_BONUS();
            WriteToZone(request);
        }

        public void OnResponse_ShopSoulBoneReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_SHOP_SOULBONE_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_SHOP_SOULBONE_REWARD>(stream);
            MSG_GateZ_SHOP_SOULBONE_REWARD request = new MSG_GateZ_SHOP_SOULBONE_REWARD();
            WriteToZone(request);
        }

        public void OnResponse_BuyShopItem(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_BUY_SHOP_ITEM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_BUY_SHOP_ITEM>(stream);
            MSG_GateZ_BUY_SHOP_ITEM request = new MSG_GateZ_BUY_SHOP_ITEM()
            {
                ShopItemId = msg.ShopItemId,
                BuyCount = msg.BuyCount,
                CouponId = msg.CouponId
            };
            WriteToZone(request);
        }
    }
}
