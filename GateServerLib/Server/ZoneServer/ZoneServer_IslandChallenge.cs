using Logger;
using Message.Gate.Protocol.GateC;
using Message.IdGenerator;
using System.IO;

namespace GateServerLib
{
    public partial class ZoneServer
    {
        private void OnResponse_IslandChallengeInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ISLAND_CHALLENGE_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} IslandChallengeInfo not find client", pcUid);
            }
        }

        private void OnResponse_IslandChallengeReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ISLAND_CHALLENGE_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} owerReward not find client", pcUid);
            }
        }

        private void OnResponse_IslandChallengeShopItemList(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ISLAND_CHALLENGE_SHOP_ITEM>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} IslandChallengeShopItemList not find client", pcUid);
            }
        }

        private void OnResponse_IslandChallengeTime(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ISLAND_CHALLENGE_TIME>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} IslandChallengeTime not find client", pcUid);
            }
        }

        private void OnResponse_IslandChallengeExecuteTask(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ISLAND_CHALLENGE_EXECUTE_TASK>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} IslandChallengeExecuteTask not find client", pcUid);
            }
        }

        private void OnResponse_IslandChallengeHeroPos(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ISLAND_CHALLENGE_HERO_POS>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} IslandChallengeHeroPos not find client", pcUid);
            }
        }

        private void OnResponse_IslandChallengeHeroInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ISLAND_CHALLENGE_HERO_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} IslandChallengeHeroInfo not find client", pcUid);
            }
        }

        private void OnResponse_IslandChallengeReviveHero(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ISLAND_CHALLENGE_HERO_REVIVE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} IslandChallengeReviveHero not find client", pcUid);
            }
        }

        private void OnResponse_IslandChallengeDungeonGrowth(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ISLAND_CHALLENGE_DUNGOEN_GROWTH>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} IslandChallengeDungeonGrowth not find client", pcUid);
            }
        }

        private void OnResponse_IslandChallengeReset(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ISLAND_CHALLENGE_RESET>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} IslandChallengeReset not find client", pcUid);
            }
        }

        private void OnResponse_IslandChallengeUpdateWinInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ISLAND_CHALLENGE_UPDATE_WININFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} IslandChallengeUpdateWinInfo not find client", pcUid);
            }
        }

        private void OnResponse_IslandChallengeSwapQueue(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ISLAND_CHALLENGE_SWAP_QUEUE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} IslandChallengeSwapQueue not find client", pcUid);
            }
        }
    }
}
