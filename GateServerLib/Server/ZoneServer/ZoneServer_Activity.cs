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
        private void OnResponse_SyncActivityListMessage(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ACTIVITY_LIST>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} sync activity List not find client", pcUid);
            }
        }

        private void OnResponse_SyncActivityChange(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ACTIVITY_CHANGE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} sync activity change not find client", pcUid);
            }
        }

        private void OnResponse_ActivityCompleteResult(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ACTIVITY_COMPLETE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} activity complete not find client", pcUid);
            }
        }

        private void OnResponse_QuestionnaireCompleteResult(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_QUESTIONNAIRE_COMPLETE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} questionnaire complete not find client", pcUid);
            }
        }

        private void OnResponse_ActivityTypeCompleteResult(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ACTIVITY_TYPE_COMPLETE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} activity type complete not find client", pcUid);
            }
        }

        private void OnResponse_ActivityRelatedCompleteResult(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ACTIVITY_RELATED_COMPLETE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} activity related complete not find client", pcUid);
            }
        }

        private void OnResponse_RechargeRebateInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_RECHARGE_REBATE_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} RechargeRebateInfo not find client", pcUid);
            }
        }

        private void OnResponse_RechargeRebateReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_RECHARGE_REBATE_GET_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} RechargeRebateReward not find client", pcUid);
            }
        }

        private void OnResponse_SyncSpceilActivityListMessage(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SPECIAL_ACTIVITY_MANAGER>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} sync special activity List not find client", pcUid);
            }
        }

        private void OnResponse_SyncSpceilActivityChange(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SPECIAL_ACTIVITY_CHANGE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} sync special activity change not find client", pcUid);
            }
        }

        private void OnResponse_SpecialActivityComplete(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SPECIAL_ACTIVITY_COMPLETE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} sync special activity Complete not find client", pcUid);
            }
        }


        private void OnResponse_SyncRunawayActivityListMessage(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_RUNAWAY_ACTIVITY_MANAGER>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} sync Runaway activity List not find client", pcUid);
            }
        }

        private void OnResponse_SyncRunawayActivityChange(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_RUNAWAY_ACTIVITY_CHANGE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} sync Runaway activity change not find client", pcUid);
            }
        }

        private void OnResponse_RunawayActivityComplete(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_RUNAWAY_ACTIVITY_COMPLETE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} sync Runaway activity Complete not find client", pcUid);
            }
        }

        private void OnResponse_WebPayRecbargeRebateInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_WEBPAY_RECHARGE_REBATE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} sync WebPay Recbarge Rebate Info not find client", pcUid);
            }
        }

        private void OnResponse_GetWebPayRebateReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_WEBPAY_REBATE_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} sync Get WebPay Rebate Reward not find client", pcUid);
            }
        }
    }
}
