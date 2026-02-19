using Logger;
using Message.Gate.Protocol.GateC;
using Message.IdGenerator;
using System.IO;

namespace GateServerLib
{
    public partial class ZoneServer
    {
        //同步背包
        private void OnResponse_SyncBag(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_BAG_SYNC>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} sync knapsack items not find client", pcUid);
            }
        }

        //更新背包
        private void OnResponse_UpdateBag(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_BAG_UPDATE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} sync knapsack items not find client", pcUid);
            }
        }

        private void OnResponse_SmeltResult(MemoryStream stream,int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SOULBONE_SMELT_RESULT>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} sync smeltResult not find client", pcUid);
            }
        }

        private void OnResponse_EquipSoulBoneResult(MemoryStream stream,int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_EQUIP_SOULBONE_RESULT>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} sync equip soulbone not find client", pcUid);
            }
        }

        private void OnResponse_ItemUse(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ITEM_USE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} use item not find client", pcUid);
            }
        }

        private void OnResponse_ItemUseBatch(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ITEM_USE_BATCH>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} use item batch not find client", pcUid);
            }
        }

        private void OnResponse_ItemSell(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ITEM_SELL>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} sell item not find client", pcUid);
            }
        }


        private void OnResponse_ItemBuy(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ITEM_BUY>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} buy item not find client", pcUid);
            }
        }

        private void OnResponse_ItemCompose(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ITEM_COMPOSE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} buy item not find client", pcUid);
            }
        }
        private void OnResponse_ItemForge(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ITEM_FORGE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} buy item not find client", pcUid);
            }
        }

        private void OnResponse_ItemResolve(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ITEM_RESOLVE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} buy item not find client", pcUid);
            }
        }

        private void OnResponse_BagSpaceInc(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_BAGSPACE_INC>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} buy item not find client", pcUid);
            }
        }

        private void OnResponse_EquipFaceFrame(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_EQUIP_FACEFRAME>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} equip face frame not find client", pcUid);
            }
        }

        //private void OnResponse_EquipFashion(MemoryStream stream, int pcUid)
        //{
        //    Client client = server.ClientMng.FindClientByUid(pcUid);
        //    if (client != null)
        //    {
        //        client.Write(Id<MSG_ZGC_EQUIP_FASHION>.Value, stream);
        //    }
        //    else
        //    {
        //        Log.WarnLine("player {0} equip fashion not find client", pcUid);
        //    }
        //}

        private void OnResponse_UseFireworks(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_USE_FIREWORKS>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} use firework not find client", pcUid);
            }
        }
     
        private void OnResponse_GetFireworkReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_FIREWORK_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get firework reward not find client", pcUid);
            }
        }

        private void OnResponse_ItemBatchResolve(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ITEM_BATCH_RESOLVE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} item batch resolve not find client", pcUid);
            }
        }

        private void OnResponse_ReceiveItem(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_RECEIVE_ITEM>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} receive item can not find client", pcUid);
            }
        }

        private void OnResponse_SoulBoneQuenching(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SOULBONE_QUENCHING>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} SoulBoneQuenching can not find client", pcUid);
            }
        }

        private void OnResponse_ItemExchangeReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ITEM_EXCHANGE_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} item exchange reward can not find client", pcUid);
            }
        }

        private void OnResponse_OpenChooseBox(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_OPEN_CHOOSE_BOX>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} open choose box not find client", pcUid);
            }
        }
    }
}
