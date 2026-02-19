using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Relation.Protocol.RZ;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public partial class RelationServer
    {
        private void OnResponse_ContributionInfo(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_CONTRIBUTION_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_CONTRIBUTION_INFO>(stream);

            Api.ContributionMng.UpdateContributionInfo(msg.PhaseNum, msg.CurrentValue);
            Log.Write($"SendContributionInfo {msg.PhaseNum} {msg.CurrentValue}");
            if (msg.GetReward)
            {
                MSG_ZGC_CONTRIBUTION_INFO response = new MSG_ZGC_CONTRIBUTION_INFO();
                response.PhaseNum = msg.PhaseNum;
                response.CurrentValue = msg.CurrentValue;
                Api.PCManager.Broadcast(response);
            }
        }
    }
}
