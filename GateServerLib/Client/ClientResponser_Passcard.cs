using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateZ;
using MessagePacker;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GateServerLib
{
    public partial class Client
    {
        public void OnResponse_GetPasscardPanelInfo(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_PASSCARD_PANEL_INFO msg = ProtobufHelper.Deserialize<MSG_CG_GET_PASSCARD_PANEL_INFO>(stream);
            MSG_GateZ_GET_PASSCARD_PANEL_INFO request = new MSG_GateZ_GET_PASSCARD_PANEL_INFO();
            WriteToZone(request);
        }

        public void OnResponse_GetPasscardLevelReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_PASSCARD_LEVEL_REWARD msg = ProtobufHelper.Deserialize<MSG_CG_GET_PASSCARD_LEVEL_REWARD>(stream);
            MSG_GateZ_GET_PASSCARD_LEVEL_REWARD request = new MSG_GateZ_GET_PASSCARD_LEVEL_REWARD();
            request.GetAll = msg.GetAll;
            foreach(var item in msg.RewardLevels)
            {
                request.RewardLevels.Add(item);
            }
            
            request.IsSuper = msg.IsSuper;
            WriteToZone(request);
        }

        public void OnResponse_GetPasscardDailyReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_PASSCARD_DAILY_REWARD msg = ProtobufHelper.Deserialize<MSG_CG_GET_PASSCARD_DAILY_REWARD>(stream);
            MSG_GateZ_GET_PASSCARD_DAILY_REWARD request = new MSG_GateZ_GET_PASSCARD_DAILY_REWARD();
            WriteToZone(request);
        }

        public void OnResponse_GetPasscardRechargeLevel(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_PASSCARD_RECHARGED_LEVEL msg = ProtobufHelper.Deserialize<MSG_CG_GET_PASSCARD_RECHARGED_LEVEL>(stream);
            MSG_GateZ_GET_PASSCARD_RECHARGED_LEVEL request = new MSG_GateZ_GET_PASSCARD_RECHARGED_LEVEL();
            request.RechargeId = msg.RechargeId;
            request.Level = msg.Level;
            WriteToZone(request);
        }

        public void OnResponse_GetPasscardRecharge(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_PASSCARD_RECHARGED msg = ProtobufHelper.Deserialize<MSG_CG_GET_PASSCARD_RECHARGED>(stream);
            MSG_GateZ_GET_PASSCARD_RECHARGED request = new MSG_GateZ_GET_PASSCARD_RECHARGED();
            request.RechargeId = msg.RechargeId;
            WriteToZone(request);
        }

        public void OnResponse_GetPasscardTaskExp(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_PASSCARD_TASK_EXP msg = ProtobufHelper.Deserialize<MSG_CG_GET_PASSCARD_TASK_EXP>(stream);
            MSG_GateZ_GET_PASSCARD_TASK_EXP request = new MSG_GateZ_GET_PASSCARD_TASK_EXP();
            request.GetAll = msg.GetAll;
            request.TaskId = msg.TaskId;
            WriteToZone(request);
            
        }
    }
}
