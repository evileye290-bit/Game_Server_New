using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateZ;
using System.IO;
using Message.Gate.Protocol.GateC;
using EnumerateUtility.Questionnaire;
using ServerModels;
using EnumerateUtility;
using MessagePacker;

namespace GateServerLib
{
    public partial class Client
    {

        public void OnResponse_ContributionInfo(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_CONTRIBUTION_INFO msg = ProtobufHelper.Deserialize<MSG_CG_CONTRIBUTION_INFO>(stream);
            MSG_GateZ_CONTRIBUTION_INFO request = new MSG_GateZ_CONTRIBUTION_INFO();

            WriteToZone(request);
        }

        public void OnResponse_GetContributionReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_CONTRIBUTION_REWARD msg = ProtobufHelper.Deserialize<MSG_CG_GET_CONTRIBUTION_REWARD>(stream);
            MSG_GateZ_GET_CONTRIBUTION_REWARD request = new MSG_GateZ_GET_CONTRIBUTION_REWARD();

            WriteToZone(request);
        }
    }
}
