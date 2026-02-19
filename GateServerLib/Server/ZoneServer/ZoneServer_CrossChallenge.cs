using System.IO;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.IdGenerator;

namespace GateServerLib
{
    public partial class ZoneServer
    {
        private void OnResponse_CrossChallengeManager(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CROSS_CHALLENGE_MANAGER>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} cross Challenge manager not find client", pcUid);
            }
        }

        private void OnResponse_SaveCrossChallengeDefensive(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SAVE_CROSS_CHALLENGE_DEFEMSIVE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} save cross Challenge defensive not find client", pcUid);
            }
        }

        private void OnResponse_GetCrossChallengeActiveReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_CROSS_CHALLENGE_ACTIVE_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get cross Challenge active reward not find client", pcUid);
            }
        }

        private void OnResponse_GetCrossChallengePreliminaryReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_CROSS_CHALLENGE_PRELIMINARY_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get cross Challenge preliminary reward not find client", pcUid);
            }
        }

        private void OnResponse_GetCrossChallengeRankInfoList(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CROSS_CHALLENGE_RANK_INFO_LIST>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get cross Challenge rank info not find client", pcUid);
            }
        }

        private void OnResponse_GetCrossChallengeChallengerHeroInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CROSS_CHALLENGE_CHALLENGER_HERO_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get cross Challenge challenger info not find client", pcUid);
            }
        }

        private void OnResponse_ShowCrossChallengeLeaderInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SHOW_CROSS_CHALLENGE_LEADER_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} show cross Challenge leader info not find client", pcUid);
            }
        }

        private void OnResponse_ShowCrossChallengeFinals(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SHOW_CROSS_CHALLENGE_FINALS_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} show cross Challenge finals info not find client", pcUid);
            }
        }

        public void OnResponse_ShowCrossChallengeChallenger(MemoryStream stream, int pcUid = 0)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CROSS_CHALLENGE_CHALLENGER_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} show cross Challenge hero info not find client", pcUid);
            }
        }

        
        private void OnResponse_UpdateCrossChallengeDefensiveQueue(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_UPDATE_DEFENSIVE_QUEUE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} update Challenge defensive queue not find client", pcUid);
            }
        }

        private void OnResponse_UpdateCrossChallengeQueue(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_UPDATE_CROSS_CHALLENGE_QUEUE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} update cross Challenge queue not find client", pcUid);
            }
        }

        private void OnResponse_GetCrossChallengeVedio(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            { 
                client.Write(Id<MSG_ZGC_GET_CROSS_CHALLENGE_VIDEO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get cross Challenge video not find client", pcUid);
            }
        }

        private void OnResponse_GetCrossChallengeServerReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_CROSS_CHALLENGE_SERVER_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get cross Challenge server reward not find client", pcUid);
            }
        }

        private void OnResponse_CrossChallengeServerReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_NEW_CROSS_CHALLENGE_SERVER_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} cross Challenge server reward not find client", pcUid);
            }
        }


        private void OnResponse_GetCrossChallengeGuessingPlayersInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_CROSS_CHALLENGE_GUESSING_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} cross Challenge get guessing info not find client", pcUid);
            }
        }
        private void OnResponse_CrossChallengeGuessingChoose(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CROSS_CHALLENGE_GUESSING_CHOOSE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} cross Challenge guessing choose info not find client", pcUid);
            }
        }

        private void OnResponse_CrossChallengeSwapQueue(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CROSS_CHALLENGE_SWAP_QUEUE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} CrossChallengeSwapQueue not find client", pcUid);
            }
        }

        private void OnResponse_CrossChallengeSwapHero(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CROSS_CHALLENGE_SWAP_HERO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} CrossChallengeSwapHero not find client", pcUid);
            }
        }
    }
}
