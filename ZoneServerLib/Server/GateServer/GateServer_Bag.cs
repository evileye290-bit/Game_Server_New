using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateZ;
using System.IO;
using System.Linq;

namespace ZoneServerLib
{
    public partial class GateServer
    {

        private void OnResponse_ItemUse(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_ITEM_USE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_ITEM_USE>(stream);
            Log.Write("player {0} use item {1} num {2}", msg.PcUid, msg.Uid, msg.Num);
            PlayerChar player = Api.PCManager.FindPc(msg.PcUid);
            if (player != null)
            {
                player.ItemUse(msg.Uid, msg.Num);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(msg.PcUid);
                if (player != null)
                {
                    Log.WarnLine("useitem fail, player {0} is offline.", msg.PcUid);
                }
                else
                {
                    Log.WarnLine("useitem fail,can not find player {0}.", msg.PcUid);
                }
            }
        }

        private void OnResponse_ItemUseBatch(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_ITEM_USE_BATCH msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_ITEM_USE_BATCH>(stream);
            Log.Write("player {0} use item batch", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.ItemUseBatch(msg);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine($"useitem batch fail, player {uid} is offline.");
                }
                else
                {
                    Log.WarnLine($"useitem batch fail,can not find player {uid}.");
                }
            }
        }

        private void OnResponse_ItemSell(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_ITEM_SELL msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_ITEM_SELL>(stream);
            Log.Write("player {0} sell item mainType {1}", msg.PcUid, msg.MainType);
            PlayerChar player = Api.PCManager.FindPc(msg.PcUid);
            if (player != null)
            {
                player.ItemSell(msg.MainType, msg.Items);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(msg.PcUid);
                if (player != null)
                {
                    Log.WarnLine("sellitem fail, player {0} is offline.", msg.PcUid);
                }
                else
                {
                    Log.WarnLine("sellitem fail, can not find player {0} .", msg.PcUid);
                }
            }

        }

        private void OnResponse_ItemBuy(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_ITEM_BUY msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_ITEM_BUY>(stream);
            Log.Write("player {0} buy item {1} num {2}", msg.PcUid, msg.Id, msg.Num);
            PlayerChar player = Api.PCManager.FindPc(msg.PcUid);
            if (player != null)
            {
                player.ItemBuy(msg.Id, msg.Num);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(msg.PcUid);
                if (player != null)
                {
                    Log.WarnLine("buy item fail, player {0} is offline.", msg.PcUid);
                }
                else
                {
                    Log.WarnLine("buy item fail, can not find player {0} .", msg.PcUid);
                }
            }
        }


        private void OnResponse_UseFireworks(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_USE_FIREWORKS msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_USE_FIREWORKS>(stream);
            Log.Write("player {0} use firework {1}", uid, msg.Id);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.UseFireworks(msg.Id);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("use firework fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("use firework fail, can not find player {0} .", uid);
                }
            }
        }

        private void OnResponse_ItemForge(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_ITEM_FORGE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_ITEM_FORGE>(stream);
            Log.Write("player {0} item forge mainType {1} id {2} num {3}", msg.PcUid, msg.MainType, msg.Id, msg.Num);
            PlayerChar player = Api.PCManager.FindPc(msg.PcUid);
            if (player != null)
            {
                player.ItemForge((MainType)msg.MainType, msg.Id, msg.Num);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(msg.PcUid);
                if (player != null)
                {
                    Log.WarnLine("ItemForge fail, player {0} is offline.", msg.PcUid);
                }
                else
                {
                    Log.WarnLine("ItemForge fail, can not find player {0} .", msg.PcUid);
                }
            }
        }

        private void OnResponse_ItemCompose(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_ITEM_COMPOSE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_ITEM_COMPOSE>(stream);
            Log.Write("player {0} item compose id {1} num {2}", msg.PcUid, msg.Id, msg.Num);
            PlayerChar player = Api.PCManager.FindPc(msg.PcUid);
            if (player != null)
            {
                player.ItemCompose(msg.Id, msg.Num);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(msg.PcUid);
                if (player != null)
                {
                    Log.WarnLine("ItemCompose fail, player {0} is offline.", msg.PcUid);
                }
                else
                {
                    Log.WarnLine("ItemCompose fail, can not find player {0} .", msg.PcUid);
                }
            }
        }

        private void OnResponse_ItemResolve(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_ITEM_RESOLVE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_ITEM_RESOLVE>(stream);
            Log.Write("player {0} item resolve mainType {1} uid {2} num {3}", msg.PcUid, msg.MainType, msg.Uid, msg.Num);
            PlayerChar player = Api.PCManager.FindPc(msg.PcUid);
            if (player != null)
            {
                player.ItemResolve(msg.MainType, msg.Uid, msg.Num);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(msg.PcUid);
                if (player != null)
                {
                    Log.WarnLine("ItemResolve fail, player {0} is offline.", msg.PcUid);
                }
                else
                {
                    Log.WarnLine("ItemResolve fail, can not find player {0} .", msg.PcUid);
                }
            }
        }

        private void OnResponse_BagSpaceInc(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_BAGSPACEINC msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_BAGSPACEINC>(stream);
            Log.Write("player {0} bag space increase {1} num", msg.PcUid, msg.Num);
            PlayerChar player = Api.PCManager.FindPc(msg.PcUid);
            if (player != null)
            {
                player.BagSpaceInc(msg.Num);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(msg.PcUid);
                if (player != null)
                {
                    Log.WarnLine("BagSpaceInc fail, player {0} is offline.", msg.PcUid);
                }
                else
                {
                    Log.WarnLine("BagSpaceInc fail, can not find player {0} .", msg.PcUid);
                }
            }
        }

        private void OnResponse_ItemBatchResolve(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_ITEM_BATCH_RESOLVE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_ITEM_BATCH_RESOLVE>(stream);
            Log.Write("player {0} item batch resolve mainType {1}", uid, msg.MainType);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.ItemBatchResolve(msg.MainType, msg.Items);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("ItemBatchResolve fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("ItemBatchResolve fail, can not find player {0} .", uid);
                }
            }
        }

        private void OnResponse_ReceiveItem(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_RECEIVE_ITEM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_RECEIVE_ITEM>(stream);
            Log.Write($"player {uid} reuqest receive item {msg.ItemId}");

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.ReceiveItem(msg.ItemId);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("reuqest receive item fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("reuqest receive item fail, can not find player {0} .", uid);
                }
            }
        }

        private void OnResponse_ItemExchangeReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_ITEM_EXCHANGE_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_ITEM_EXCHANGE_REWARD>(stream);
            Log.Write($"player {uid} reuqest item exchange reward {msg.Id}");

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.ItemExchangeReward(msg.Id, 1);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("reuqest item exchange reward  fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("reuqest item exchange reward  fail, can not find player {0} .", uid);
                }
            }
        }


        private void OnResponse_OpenChooseBox(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_OPEN_CHOOSE_BOX msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_OPEN_CHOOSE_BOX>(stream);
            Log.Write("player {0} open choose box {1} ", uid, msg.ItemUid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.OpenChooseBox(msg.ItemUid, msg.Items);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine(" player {0} open choose box fail, is offline.", uid);
                }
                else
                {
                    Log.WarnLine(" player {0} open choose box fail, not find.", uid);
                }
            }
        }
    }
}
