using Logger;
using Message.Gate.Protocol.GateC;
using Message.IdGenerator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GateServerLib
{
    public partial class ZoneServer
    {
        private void OnResponse_GetCarnivalBossInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CARNIVAL_BOSS_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GetCarnivalBossInfo not find client", pcUid);
            }
        }

        private void OnResponse_EnterCarnivalBossDungeon(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ENTER_CARNIVAL_BOSS_DUNGEON>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} EnterCarnivalBossDungeon not find client", pcUid);
            }
        }

        private void OnResponse_GetCarnivalBossReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_CARNIVAL_BOSS_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GetCarnivalBossReward not find client", pcUid);
            }
        }

        private void OnResponse_UpdateCarnivalBossQueue(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_UPDATE_CARNIVAL_BOSS_QUEUE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} UpdateCarnivalBossQueue not find client", pcUid);
            }
        }

        private void OnResponse_GetCarnivalRechargeInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CARNIVAL_RECHARGE_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GetCarnivalRechargeInfo not find client", pcUid);
            }
        }

        private void OnResponse_GetCarnivalRechargeReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_CARNIVAL_RECHARGE_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GetCarnivalRechargeReward not find client", pcUid);
            }
        }

        private void OnResponse_GetCarnivalMallInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CARNIVAL_MALL_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GetCarnivalMallInfo not find client", pcUid);
            }
        }

        private void OnResponse_BuyCarnivalMallGiftItem(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_BUY_CARNIVAL_MALL_GIFT_ITEM>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} BuyCarnivalMallGiftItem not find client", pcUid);
            }
        }
    }
}
