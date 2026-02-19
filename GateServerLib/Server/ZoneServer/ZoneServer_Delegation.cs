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
        private void OnResponse_DelegationList(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_DELEGATION_LIST>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get delegation list not find client", pcUid);
            }
        }

        private void OnResponse_DelegateHeros(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_DELEGATE_HEROS>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} delegate heros not find client", pcUid);
            }
        }

        private void OnResponse_CompleteDelegation(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_COMPLETE_DELEGATION>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} complete delegation not find client", pcUid);
            }
        }

        private void OnResponse_GetDelegationRewards(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_DELEGATION_REWARDS>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get delegation rewards not find client", pcUid);
            }
        }

        private void OnResponse_DelegationDailyRefresh(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_DELEGATION_DAILY_REFRESH>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} delegation daily refresh not find client", pcUid);
            }
        }

        private void OnResponse_RefreshDelegation(MemoryStream stream, int pcUid)
        {          
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_REFRESH_DELEGATION>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} refresh delegation not find client", pcUid);
            }
        }

        private void OnResponse_BuyDelegationCount(MemoryStream stream, int pcUid)
        {       
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_BUY_DELEGATION_COUNT>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} buy delegation count not find client", pcUid);
            }
        }
    }
}
