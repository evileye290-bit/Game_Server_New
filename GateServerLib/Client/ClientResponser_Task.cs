using CommonUtility;
using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateZ;
using System.IO;

namespace GateServerLib
{
    public partial class Client
    {
        public void OnResponse_TaskComplete(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_TASK_COMPLETE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_TASK_COMPLETE>(stream);
            MSG_GateZ_TASK_COMPLETE request = new MSG_GateZ_TASK_COMPLETE();
            request.TaskId = msg.TaskId;
            request.ZoneNpcId = msg.ZoneNpcId;
            WriteToZone(request);
        }

        public void OnResponse_OpenEmailTask(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_OPENE_EMAIL_TASK msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_OPENE_EMAIL_TASK>(stream);
            MSG_GateZ_OPENE_EMAIL_TASK request = new MSG_GateZ_OPENE_EMAIL_TASK();
            request.PcUid = Uid;
            request.Uid = ExtendClass.GetUInt64(msg.UidHigh, msg.UidLow);
            request.Id = msg.Id;
            request.SendTime = msg.SendTime;
            WriteToZone(request);
        }

        public void OnResponse_TaskCollect(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_TASK_COLLECT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_TASK_COLLECT>(stream);
            MSG_GateZ_TASK_COLLECT request = new MSG_GateZ_TASK_COLLECT();
            request.ZoneGoodsId = msg.ZoneGoodsId;
            WriteToZone(request);
        }

        public void OnResponse_AutoPathFinding(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_AUTOPATHFINDING msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_AUTOPATHFINDING>(stream);
            MSG_GateZ_AUTOPATHFINDING request = new MSG_GateZ_AUTOPATHFINDING();
            request.TargetId = msg.TargetId;
            request.TargetType = msg.TargetType;
            WriteToZone(request);
        }

        public void OnResponse_CrossPortal(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_CROSS_PORTAL msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_CROSS_PORTAL>(stream);
            MSG_GateZ_CROSS_PORTAL request = new MSG_GateZ_CROSS_PORTAL();
            request.PortalId = msg.PortalId;
            WriteToZone(request);
        }



        public void OnResponse_TaskSelect(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_TASK_SELECT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_TASK_SELECT>(stream);
            MSG_GateZ_TASK_SELECT request = new MSG_GateZ_TASK_SELECT();
            request.PcUid = Uid;
            request.TaskId = msg.TaskId;
            request.Index = msg.Index;
            WriteToZone(request);
        }

        public void OnResponse_TaskMake(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_TASK_MAKE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_TASK_MAKE>(stream);
            MSG_GateZ_TASK_MAKE request = new MSG_GateZ_TASK_MAKE();
            request.PcUid = Uid;
            request.TaskId = msg.TaskId;
            WriteToZone(request);
        }

        public void OnResponse_TaskFlyDone(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_TASKFLY_FLY_DONE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_TASKFLY_FLY_DONE>(stream);
            MSG_GateZ_TASKFLY_FLY_DONE request = new MSG_GateZ_TASKFLY_FLY_DONE();
            request.PcUid = Uid;
            WriteToZone(request);
             
        }

        public void OnResponse_TaskFly_StartPathFinding(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_TASKFLY_STARTPATHFINDING msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_TASKFLY_STARTPATHFINDING>(stream);
            MSG_GateZ_TASKFLY_STARTPATHFINDING request = new MSG_GateZ_TASKFLY_STARTPATHFINDING();
            request.PcUid = Uid;
            WriteToZone(request);
        }
    }
}
