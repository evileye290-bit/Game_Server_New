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
        private void OnResponse_GetRechargeHistory(MemoryStream stream, int uid)
        {
            Client client = Api.ClientMng.FindClientByUid(uid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_RECHARGE_HISTORY>.Value, stream);
            }
        }

        private void OnResponse_RechargeManager(MemoryStream stream, int uid)
        {
            Client client = Api.ClientMng.FindClientByUid(uid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_RECHARGE_MANAGER>.Value, stream);
            }
        }

        private void OnResponse_RechargeHistoryId(MemoryStream stream, int uid)
        {
            Client client = Api.ClientMng.FindClientByUid(uid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_ORDER_ID>.Value, stream);
            }
        }

        private void OnResponse_GetAccumulateRechargeReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_ACCUMULATE_RECHARGE_REWARD>.Value, stream);
            }
            else
            {
                Logger.Log.WarnLine("player {0} get accumulate recharge reward not find client", pcUid);
            }
        }

        private void OnResponse_GetNewRechargeGiftAccumulateReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_NEW_RECHARGE_GIFT_REWARD>.Value, stream);
            }
            else
            {
                Logger.Log.WarnLine("player {0} get new recharge gift accumulate reward not find client", pcUid);
            }
        }
    }
}
