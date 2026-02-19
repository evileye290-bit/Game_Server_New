using EnumerateUtility;
using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using System.IO;

namespace GateServerLib
{
    public partial class Client
    {
        public void OnResponse_Suggest(MemoryStream stream)
        {
            MSG_CG_SUGGEST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_SUGGEST>(stream);

            if (IsGm == 0)
            {
                int len = server.WordChecker.GetWordLen(msg.Suggest);
                if (len < WordLengthLimit.BadGameJudgeInputLimitLow || len > WordLengthLimit.BadGameJudgeInputLimitHigh)
                {
                    MSG_ZGC_SUGGEST response = new MSG_ZGC_SUGGEST();
                    response.Result = (int)ErrorCode.LengthLimit;
                    Write(response);
                    return;
                }

                if (server.WordChecker.HasBadWord(msg.Suggest))
                {
                    MSG_ZGC_SUGGEST response = new MSG_ZGC_SUGGEST();
                    response.Result = (int)ErrorCode.BadWord;
                    Write(response);
                    return;
                }
            }

            MSG_GateZ_SUGGEST msg2z = new MSG_GateZ_SUGGEST();
            msg2z.Uid = Uid;
            msg2z.Suggest = msg.Suggest;
            WriteToZone(msg2z);
        }
    }
}
