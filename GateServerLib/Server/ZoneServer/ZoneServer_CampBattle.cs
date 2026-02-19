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
        private void OnResponse_CampBattleFortInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_FORT_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get camp battle fort info not find client", pcUid);
            }
        }

        private void OnResponse_SyncCampBattleInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SYNC_CAMPBATTLE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} sync camp battle info not find client", pcUid);
            }
        }

        private void OnResponse_CampCreateDungeon(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CAMP_CREATE_DUNGEON>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} camp create dungeon not find client", pcUid);
            }
        }

        private void OnResponse_CampRankListByType(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CAMP_RANK_LIST_BY_TYPE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} camp rank by Type not find client", pcUid);
            }
        }


        public void OnResponse_OpenCampBox(MemoryStream stream, int pcUid )
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_OPEN_CAMP_BOX>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} camp rank by Type not find client", pcUid);
            }
        }

        public void OnResponse_CheckInBattleRank(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CHECK_IN_BATTLE_RANK>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} check in battle rank not find client", pcUid);
            }
        }

        public void OnResponse_UseNatureItem(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_USE_NATURE_ITEM>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} use nature item on fort not find client");
            }
        }

        public void OnResponse_BattleBoxCount(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CAMP_BOX_COUNT>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} camp box count not find client");
            }
        }

        public void OnResponse_GiveUpFort(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GIVEUP_FORT>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} camp battle give up fort, count not find client");
            }
        }


        public void OnResponse_HoldFort(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_HOLD_FORT>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} camp battle hold fort, count not find client");
            }
        }

        public void OnResponse_CampbattleAnnouncementList(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CAMPBATTLE_ANNOUNCE_LIST>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} camp battle annunce list, count not find client");
            }
        }

        
    }
}
