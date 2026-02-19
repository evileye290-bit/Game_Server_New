using Logger;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
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
        private void OnResponse_PasscardRecharged(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_PASSCARD_RECHARGE_RESULT>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} passcard recharge result not find client", pcUid);
            }
        }

        private void OnResponse_PasscardPanelInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_PASSCARD_PANEL_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} passcard panel not find client", pcUid);
            }
        }

        private void OnResponse_PasscardLevelReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_PASSCARD_LEVEL_REWARD_RESULT>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} passcard level reward not find client", pcUid);
            }
        }

        private void OnResponse_PasscardDailyReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_PASSCARD_DAILY_REWARD_RESULT>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} passcard dailyReward not find client", pcUid);
            }
        }

        private void OnResponse_PasscardRechargeLevel(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_PASSCARD_RECHARGE_LEVEL_RESULT>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} passcard recharge level not find client", pcUid);
            }
        }

        private void OnResponse_PasscardUpdateTask(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_UPDATE_PASSCARD_TASK>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} passcard update task not find client", pcUid);
            }
        }
    }
}
