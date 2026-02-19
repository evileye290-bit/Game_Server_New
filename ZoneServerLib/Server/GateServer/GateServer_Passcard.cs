using Logger;
using Message.Gate.Protocol.GateZ;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public partial class GateServer
    {
        public void OnResponse_GetPasscardPanelInfo(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_PASSCARD_PANEL_INFO pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_PASSCARD_PANEL_INFO>(stream);
            Log.Write("player {0} request get passcard panel info", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} get passcard panel info not in gateid {1} pc list", uid, SubId);
                return;
            }
            player.GetPasscardInfos(pks);
        }

        public void OnResponse_GetPasscardLevelReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_PASSCARD_LEVEL_REWARD pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_PASSCARD_LEVEL_REWARD>(stream);
            Log.Write("player {0} request get passcard level reward", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} get passcard level reward not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.GetPasscardReward(pks);
        }

        public void OnResponse_GetPasscardDailyReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_PASSCARD_DAILY_REWARD pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_PASSCARD_DAILY_REWARD>(stream);
            Log.Write("player {0} request get passcard daily reward", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} get passcard daily reward not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.GetPasscardDailyReward(pks);
        }

        public void OnResponse_GetPasscardRechargeLevel(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_PASSCARD_RECHARGED_LEVEL pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_PASSCARD_RECHARGED_LEVEL>(stream);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} get passcard recharge level not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.UpdatePasscardRechargeLevel(pks);
        }

        public void OnResponse_GetPasscardRecharged(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_PASSCARD_RECHARGED pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_PASSCARD_RECHARGED>(stream);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} get passcard recharge not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.GetPasscardBought(pks);
        }

        public void OnResponse_GetPasscardTask(MemoryStream stream,int uid = 0)
        {
            MSG_GateZ_GET_PASSCARD_TASK_EXP pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_PASSCARD_TASK_EXP>(stream);
            Log.Write("player {0} request get passcard task", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} get passcard task exp not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.GetPasscardTaskComplete(pks);
        }
    }
}
