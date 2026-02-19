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
        private void OnResponse_CrossManager(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CROSS_BATTLE_MANAGER>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} cross manager not find client", pcUid);
            }
        }

        private void OnResponse_SaveCrossDefensive(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SAVE_CROSS_BATTLE_DEFEMSIVE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} save cross defensive not find client", pcUid);
            }
        }

        private void OnResponse_GetCrossBattleActiveReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_CROSS_BATTLE_ACTIVE_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get cross active reward not find client", pcUid);
            }
        }

        private void OnResponse_GetCrossBattlePreliminaryReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_CROSS_BATTLE_PRELIMINARY_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get cross preliminary reward not find client", pcUid);
            }
        }

        private void OnResponse_GetCrossRankInfoList(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CROSS_BATTLE_RANK_INFO_LIST>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get cross rank info not find client", pcUid);
            }
        }

        private void OnResponse_GetCrossChallengerHeroInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CROSS_BATTLE_CHALLENGER_HERO_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get cross challenger info not find client", pcUid);
            }
        }

        private void OnResponse_ShowCrossLeaderInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SHOW_CROSS_LEADER_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} show cross leader info not find client", pcUid);
            }
        }

        private void OnResponse_ShowCrossBattleFinals(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SHOW_CROSS_BATTLE_FINALS_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} show cross finals info not find client", pcUid);
            }
        }

        public void OnResponse_ShowCrossBattleChallenger(MemoryStream stream, int pcUid = 0)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CROSS_BATTLE_CHALLENGER_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} show cross hero info not find client", pcUid);
            }
        }

        
        private void OnResponse_UpdateDefensiveQueue(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_UPDATE_DEFENSIVE_QUEUE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} update defensive queue not find client", pcUid);
            }
        }

        private void OnResponse_UpdateCrossQueue(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_UPDATE_CROSS_QUEUE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} update cross queue not find client", pcUid);
            }
        }

        private void OnResponse_GetCrossBattleVedio(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_CROSS_VIDEO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get cross video not find client", pcUid);
            }
        }

        private void OnResponse_GetCrossBattleServerReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_CROSS_BATTLE_SERVER_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get cross server reward not find client", pcUid);
            }
        }

        private void OnResponse_CrossBattleServerReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_NEW_CROSS_BATTLE_SERVER_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} cross battle server reward not find client", pcUid);
            }
        }


        private void OnResponse_GetGuessingPlayersInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_GUESSING_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} cross get guessing info not find client", pcUid);
            }
        }
        private void OnResponse_CrossGuessingChoose(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CROSS_GUESSING_CHOOSE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} cross guessing choose info not find client", pcUid);
            }
        }
    }
}
