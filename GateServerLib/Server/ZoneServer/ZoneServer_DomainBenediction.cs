using System.IO;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.IdGenerator;

namespace GateServerLib
{
    public partial class ZoneServer
    {
        private void OnResponse_HandleGetStageAward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_DOMAIN_BENEDICTION_GET_STAGE_AWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} HandleGetStageAward not find client", pcUid);
            }
        }
        
        private void OnResponse_HandleGetBaseCurrencyAward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_DOMAIN_BENEDICTION_GET_BASE_CURRENCY_AWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} HandleGetBaseCurrencyAward not find client", pcUid);
            }
        }
        
        private void OnResponse_HandlePrayOperation(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_DOMAIN_BENEDICTION_PRAY_OPERATION>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} HandlePrayOperation not find client", pcUid);
            }
        }
        
        private void OnResponse_HandleDrawOperation(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_DOMAIN_BENEDICTION_DRAW_OPERATION>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} HandleDrawOperation not find client", pcUid);
            }
        }
        
        private void OnResponse_LoadOrUpdate(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_DOMAIN_BENEDICTION_LOAD_AND_UPDATE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} LoadOrUpdate not find client", pcUid);
            }
        }
    }
}