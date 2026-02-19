using Logger;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
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
        private void OnResponse_ChangeTitleAnswer(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CHANGE_TITLE_ANSWER>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} Change Title answer not find client", pcUid);
            }
        }

        private void OnResponse_TitleChanged(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_TITLE_INFO>.Value, stream);
                Log.Write("client {0} Sync TitleChange", client.Uid);
            }
            else
            {
                Log.WarnLine("player {0} Change Title answer not find client", pcUid);
            }
        }

        private void OnResponse_GetNewTitle(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_NEW_TITLE>.Value, stream);             
            }
            else
            {
                Log.WarnLine("player {0} get new title not find client", pcUid);
            }
        }

        private void OnResponse_GetTitleConditionCount(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_TITLE_CONDITION_COUNT>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get title condition count not find client", pcUid);
            }
        }

        private void OnResponse_LookTitle(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_LOOK_TITLE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} look title not find client", pcUid);
            }
        }
    }
}
