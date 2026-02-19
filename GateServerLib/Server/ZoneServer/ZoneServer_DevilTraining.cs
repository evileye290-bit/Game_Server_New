using Logger;
using Message.Gate.Protocol.GateC;
using Message.IdGenerator;
using System.IO;

namespace GateServerLib
{
    public partial class ZoneServer
    {
        private void OnResponse_DevilTrainingInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_DEVIL_TRAINING_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get deviltraining info not find client", pcUid);
            }
        }
        private void OnResponse_DevilTrainingReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_DEVIL_TRAINING_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get deviltraining get reward not find client", pcUid);
            }
        }
        private void OnResponse_DevilTrainingBuyItem(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_BUY_DEVIL_TRAINING_ITEM>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get deviltraining buy item not find client", pcUid);
            }
        }
        private void OnResponse_DevilTrainingPointReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_DEVIL_TRAINING_POINT_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get deviltraining get point reward not find client", pcUid);
            }
        }
        private void OnResponse_DevilTrainingChangeBuff(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CHANGE_DEVIL_TRAINING_BUFF>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get deviltraining change buff not find client", pcUid);
            }
        }
    }
}
