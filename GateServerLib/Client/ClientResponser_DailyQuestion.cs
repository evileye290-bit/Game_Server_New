using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateZ;
using System.IO;
using Message.Gate.Protocol.GateC;

namespace GateServerLib
{
    public partial class Client
    {
        private void OnResponse_DailyQuestionCounter(MemoryStream stream)
        {
            if (Uid == 0) return;
            MSG_GateZ_DAILY_QUESTION_COUNTER request = new MSG_GateZ_DAILY_QUESTION_COUNTER();
            request.Uid = Uid;
            WriteToZone(request);
        }
        private void OnResponse_DailyQuestionReward(MemoryStream stream)
        {
            if (Uid == 0) return;
            MSG_CG_DAILY_QUESTION_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_DAILY_QUESTION_REWARD>(stream);
            MSG_GateZ_DAILY_QUESTION_REWARD request = new MSG_GateZ_DAILY_QUESTION_REWARD();
            request.Uid = Uid;
            request.CorrectAnswers = msg.CorrectAnswers;
            request.CostDiamond = msg.CostDiamond;
            WriteToZone(request);
        }
    }
}
