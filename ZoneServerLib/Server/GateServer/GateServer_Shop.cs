using DataProperty;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateZ;
using ServerShared;
using System.Collections.Generic;
using System.IO;

namespace ZoneServerLib
{
    public partial class GateServer
    {
        public void OnResponse_GetShopInfo(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_SHOP_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_SHOP_INFO>(stream);
            Log.Write("player {0} request get shop {1} info", uid, msg.ShopType);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn($"player {uid} shop {msg.ShopType} info not find pc");
                return;
            }
            if ((ShopType)msg.ShopType == ShopType.SoulBone)
            {
                player.GetShopInfo((ShopType)msg.ShopType);
            }
            else
            {
                player.GetCommonShopInfo(msg.ShopType);
            }
        }

        private void OnResponse_ShopBuyItem(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_SHOP_BUY_ITEM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_SHOP_BUY_ITEM>(stream);
            Log.Write("player {0} request shop {1} buy item {2} num {3}", uid, msg.ShopType, msg.ItemId, msg.BuyNum);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} shop {1} buy item {2} not find pc", uid, msg.ShopType, msg.ItemId);
                return;
            }
            player.ShopBuyItem((ShopType)msg.ShopType, msg.ItemId, msg.BuyNum);
        }

        private void OnResponse_ShopRefresh(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_SHOP_REFRESH msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_SHOP_REFRESH>(stream);
            Log.Write("player {0} request refresh shop {1}", uid, msg.ShopType);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn($"player {uid} shop {msg.ShopType} fresh not find pc");
                return;
            }
            if ((ShopType)msg.ShopType == ShopType.SoulBone)
            {
                player.ShopRefresh((ShopType)msg.ShopType);
            }
            else
            {
                player.CommonShopRefresh(msg.ShopType);
            }
        }

        private void OnResponse_ShopSoulBoneBonus(MemoryStream stream, int uid = 0)
        {
            PlayerChar player = Api.PCManager.FindPc(uid);
            Log.Write("player {0} request shop soulbone bonus", uid);
            if (player == null)
            {
                Log.Warn($"player {uid} shop ShopSoulBoneBonus not find pc");
                return;
            }
            player.ShopSoulBoneBonus();
        }

        private void OnResponse_ShopSoulBoneReward(MemoryStream stream, int uid = 0)
        {
            Log.Write("player {0} request shop soulbone reward", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn($"player {uid} shop ShopSoulBoneReward not find pc");
                return;
            }
            player.ShopSoulBoneReward();
        }

        private void OnResponse_BuyShopItem(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_BUY_SHOP_ITEM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_BUY_SHOP_ITEM>(stream);
            Log.Write("player {0} request buy shop item {1} count {2}", uid, msg.ShopItemId, msg.BuyCount);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} buy shop item {1} not find pc", uid, msg.ShopItemId);
                return;
            }
            player.BuyShopItem(msg.ShopItemId, msg.BuyCount, msg.CouponId);
        }
    }
}
