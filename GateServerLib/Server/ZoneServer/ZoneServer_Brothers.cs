using Logger;
using Message.Gate.Protocol.GateC;
using Message.IdGenerator;
using System.IO;

namespace GateServerLib
{
    public partial class ZoneServer
    {
        private void OnResponse_BrotherInvite(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_BROTHERS_INVITE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} invite brother can not find client", pcUid);
            }
        }

        private void OnResponse_BrotherResponse(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_BROTHERS_RESPONSE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} response invite brother can not find client", pcUid);
            }
        }


        public void OnResponse_BrothersRemove(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_BROTHERS_REMOVE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} brothers remove can not find client", pcUid);
            }
        }

        public void OnResponse_SyncBrothersList(MemoryStream stream,int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SYNC_BROTHERS_LIST>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} sync_brothers_list can not find client", pcUid);
            }
        }

        public void OnResponse_SyncBrothersInviterList(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SYNC_BROTHERS_INVITER_LIST>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} sync_brothers_inviter_list can not find client", pcUid);
            }
        }
    }
}
