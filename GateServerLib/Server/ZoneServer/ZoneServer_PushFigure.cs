using Logger;
using Message.Gate.Protocol.GateC;
using Message.IdGenerator;
using System.IO;

namespace GateServerLib
{
    public partial class ZoneServer
    {
        private void OnResponse_PushFigureInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_PUSHFIGURE_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} PushFigureInfo not find client", pcUid);
            }
        }

        private void OnResponse_PushFigureFinishTask(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_PUSHFIGURE_FINISHTASK>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} PushFigureFinishTask not find client", pcUid);
            }
        }
    }
}

