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
        private void OnResponse_ArenaManager(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ARENA_MANAGER>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} arena manager not find client", pcUid);
            }
        }

        private void OnResponse_SaveArenaDefensive(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SAVE_DEFEMSIVE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} save arena defensive not find client", pcUid);
            }
        }

        private void OnResponse_ResetArenaFightTime(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_RESET_ARENA_FIGHT_TIME>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} reset arena fight time not find client", pcUid);
            }
        }

        private void OnResponse_GetRankLevelReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_RANK_LEVEL_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get rank reward not find client", pcUid);
            }
        }

        private void OnResponse_GetArenaChallenger(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_ARENA_CHALLENGERS>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get arena challenger not find client", pcUid);
            }
        }

        private void OnResponse_ArenaRankInfoList(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ARENA_RANK_INFO_LIST>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} arena rank List not find client", pcUid);
            }
        }

        private void OnResponse_ShowArenaChallengerInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ARENA_CHALLENGER_HERO_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} show arena challenger info not find client", pcUid);
            }
        }

        private void OnResponse_ChallengerRankChange(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CHALLENGER_RANK_CHANGE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} arena challenger rank change info not find client", pcUid);
            }
        }

    }
}
