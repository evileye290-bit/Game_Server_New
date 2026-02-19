using Logger;
using Message.Gate.Protocol.GateC;
using Message.IdGenerator;
using Message.Zone.Protocol.ZGate;
using System.IO;

namespace GateServerLib
{
    public partial class ZoneServer
    {
        private void OnResponse_DailyQuestionCounter(MemoryStream stream, int uid)
        {
            Client client = Api.ClientMng.FindClientByUid(uid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_DAILY_QUESTION_COUNTER>.Value, stream);
            }
        }

        private void OnResponse_DailyQuestionReward(MemoryStream stream, int uid)
        {
            Client client = Api.ClientMng.FindClientByUid(uid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_DAILY_QUESTION_REWARD>.Value, stream);
            }
        }
    }
}
