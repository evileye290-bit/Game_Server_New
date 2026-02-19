using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateZ;
using System.IO;

namespace GateServerLib
{
    public partial class Client
    {
        public void OnResponse_ActivityComplete(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_ACTIVITY_COMPLETE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_ACTIVITY_COMPLETE>(stream);
            MSG_GateZ_ACTIVITY_COMPLETE request = new MSG_GateZ_ACTIVITY_COMPLETE();
            request.ActivityId = msg.ActivityId;
            WriteToZone(request);
        }

        public void OnResponse_ActivityTypeComplete(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_ACTIVITY_TYPE_COMPLETE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_ACTIVITY_TYPE_COMPLETE>(stream);
            MSG_GateZ_ACTIVITY_TYPE_COMPLETE request = new MSG_GateZ_ACTIVITY_TYPE_COMPLETE();
            request.ActivityType = msg.ActivityType;
            WriteToZone(request);
        }

        public void OnResponse_ActivityRelatedComplete(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_ACTIVITY_RELATED_COMPLETE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_ACTIVITY_RELATED_COMPLETE>(stream);
            MSG_GateZ_ACTIVITY_RELATED_COMPLETE request = new MSG_GateZ_ACTIVITY_RELATED_COMPLETE();
            request.ActivityId = msg.ActivityId;
            WriteToZone(request);
        }

        public void OnResponse_RechargeRebateReward(MemoryStream stream)
        {
            if (curZone == null) return;
            //MSG_CG_RECHARGE_REBATE_GET_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_RECHARGE_REBATE_GET_REWARD>(stream);
            WriteToZone(new MSG_GateZ_RECHARGE_REBATE_GET_REWARD());
        }

        public void OnResponse_GetActivityFinishReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_TASK_FINISH_STATE_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_TASK_FINISH_STATE_REWARD>(stream);
            WriteToZone(new MSG_GateZ_TASK_FINISH_STATE_REWARD() { RewardType = msg.RewardType, Index = msg.Index });
        }

        public void OnResponse_SpceilActivityComplete(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_SPECIAL_ACTIVITY_COMPLETE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_SPECIAL_ACTIVITY_COMPLETE>(stream);
            MSG_GateZ_SPECIAL_ACTIVITY_COMPLETE request = new MSG_GateZ_SPECIAL_ACTIVITY_COMPLETE();
            request.ActivityId = msg.ActivityId;
            WriteToZone(request);
        }

        public void OnResponse_RunawayActivityComplete(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_RUNAWAY_ACTIVITY_COMPLETE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_RUNAWAY_ACTIVITY_COMPLETE>(stream);
            MSG_GateZ_RUNAWAY_ACTIVITY_COMPLETE request = new MSG_GateZ_RUNAWAY_ACTIVITY_COMPLETE();
            request.ActivityId = msg.ActivityId;
            WriteToZone(request);
        }

        public void OnResponse_GetWebPayRebateReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_WEBPAY_REBATE_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_WEBPAY_REBATE_REWARD>(stream);
            MSG_GateZ_GET_WEBPAY_REBATE_REWARD request = new MSG_GateZ_GET_WEBPAY_REBATE_REWARD();
            request.Id = msg.Id;
            WriteToZone(request);
        }
    }
}
