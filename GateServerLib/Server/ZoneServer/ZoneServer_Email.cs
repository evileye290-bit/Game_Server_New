using Logger;
using Message.Gate.Protocol.GateC;
using Message.IdGenerator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GateServerLib
{
    public partial class ZoneServer
    {
        private void OnResponse_SyncTaskChangeMessage(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_EMAIL_OPENE_BOX>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} sync task List not find client", pcUid);
            }
        }

        private void OnResponse_EmailRemaind(MemoryStream stream, int uid)
        {
            Client client = Api.ClientMng.FindClientByUid(uid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_EMAIL_REMIND>.Value, stream);
            }
        }

        private void OnResponse_EmailRead(MemoryStream stream, int uid)
        {
            Client client = Api.ClientMng.FindClientByUid(uid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_EMAIL_READ>.Value, stream);
            }
        }

        private void OnResponse_GetAttachment(MemoryStream stream, int uid)
        {
            Client client = Api.ClientMng.FindClientByUid(uid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_PICKUP_ATTACHMENT>.Value, stream);
            }
        }

        private void OnResponse_GetAttachmentBatch(MemoryStream stream, int uid)
        {
            Client client = Api.ClientMng.FindClientByUid(uid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_PICKUP_ATTACHMENT_BATCH>.Value, stream);
            }
        }

        
    }
}
