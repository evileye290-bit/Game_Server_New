using Logger;
using Message.Gate.Protocol.GateC;
using Message.IdGenerator;
using Message.Zone.Protocol.ZGate;
using System.IO;

namespace GateServerLib
{
    public partial class ZoneServer
    {

        private void OnResponse_RadioAllAnchorRank(MemoryStream stream, int uid)
        {
            Client client = Api.ClientMng.FindClientByUid(uid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_RADIO_ALL_ANCHOR_RANK>.Value, stream);
            }
        }
        private void OnResponse_RadioAnchorContributionRank(MemoryStream stream, int uid)
        {
            Client client = Api.ClientMng.FindClientByUid(uid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_RADIO_ANCHOR_CONTRIBUTION_RANK>.Value, stream);
            }
        }
        private void OnResponse_RadioAllContributionRank(MemoryStream stream, int uid)
        {
            Client client = Api.ClientMng.FindClientByUid(uid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_RADIO_ALL_CONTRIBUTION_RANK>.Value, stream);
            }
        }
        private void OnResponse_RadioContributionReward(MemoryStream stream, int uid)
        {
            Client client = Api.ClientMng.FindClientByUid(uid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_RADIO_CONTRIBUTION_REWARD>.Value, stream);
            }
        }

        public void OnResponse_RadioGift(MemoryStream stream, int uid)
        {
            MSG_ZGC_RADIO_GIFT pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZGC_RADIO_GIFT>(stream);

            foreach (var client in Api.ClientMng.ClientList)
            {
                if (client != null && client.RadioOpen)
                {
                    client.Write(pks);
                }
            }
        }
    }
}
