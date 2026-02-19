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
        private void OnResponse_CampBuildInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CAMPBUILD_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get camp build info not find client", pcUid);
            }
        }

        private void OnResponse_SyncCampBuildInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SYNC_CAMPBUILD_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} sync camp build info not find client", pcUid);
            }
        }


        private void OnResponse_CampBuildGo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CAMPBUILD_GO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} camp build go not find client", pcUid);
            }
        }

        private void OnResponse_BuyCampBuildGoCount(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_BUY_CAMPBUILD_GO_COUNT>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} buy camp build go count not find client", pcUid);
            }
        }


        private void OnResponse_OpenCampBuildBox(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_OPEN_CAMPBUILD_BOX>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} open camp build box not find client", pcUid);
            }
        }


        private void OnResponse_CampBuildRankList(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CAMPBUILD_RANK_LIST>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} camp build rank list not find client", pcUid);
            }
        }


            

    }
}
