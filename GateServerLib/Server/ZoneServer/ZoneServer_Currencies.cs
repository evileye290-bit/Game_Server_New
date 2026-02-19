using Logger;
using Message.Gate.Protocol.GateC;
using Message.IdGenerator;
using System.IO;

namespace GateServerLib
{
    public partial class ZoneServer
    {
        private void OnResponse_SyncCurrencies(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SYNC_CURRENCIES>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} sync currencies state not find client", pcUid);
            }
        }

        private void OnResponse_FirstAddExp(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_FIRST_ADD_EXP>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} sync add exp state not find client", pcUid);
            }
        }

        private void OnResponse_SyncCounter(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_COUNTER_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} sync counter not find client", pcUid);
            }
        }

        private void OnResponse_BuyCounter(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_COUNTER_BUY_COUNT>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} buy counter not find client", pcUid);
            }
        }

        private void OnResponse_GetSpecialCount(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_SPECIAL_COUNT>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get special count not find client", pcUid);
            }
        }      
    }
}
