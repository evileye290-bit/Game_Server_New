using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateZ;
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
        public void OnResponse_DriftExploreTaskReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_DRIFT_EXPLORE_TASK_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_DRIFT_EXPLORE_TASK_REWARD>(stream);
            MSG_GateZ_DRIFT_EXPLORE_TASK_REWARD request = new MSG_GateZ_DRIFT_EXPLORE_TASK_REWARD();
            request.TaskId = msg.TaskId;
            WriteToZone(request);
        }

        public void OnResponse_DriftExploreReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_DRIFT_EXPLORE_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_DRIFT_EXPLORE_REWARD>(stream);
            MSG_GateZ_DRIFT_EXPLORE_REWARD request = new MSG_GateZ_DRIFT_EXPLORE_REWARD();
            WriteToZone(request);
        }

    }
}
