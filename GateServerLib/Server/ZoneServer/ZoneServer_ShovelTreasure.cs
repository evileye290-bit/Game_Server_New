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
        public void OnResponse_ShovelGameRewards(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SHOVEL_GAME_REWARDS>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get shovel game rewards can not find client", pcUid);
            }
        }

        public void OnResponse_LightTreasurePuzzle(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_LIGHT_TREASURE_PUZZLE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} light treasure puzzle can not find client", pcUid);
            }
        }

        public void OnResponse_RandomPuzzle(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_RANDOM_PUZZLE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} random puzzle can not find client", pcUid);
            }
        }

        public void OnResponse_TreasurePuzzleReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_TREASURE_PUZZLE_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} get treasure puzzle reward can not find client", pcUid);
            }
        }

        public void OnResponse_ShovelTreasureFly(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SHOVEL_TREASURE_FLY>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} shovel treasure fly can not find client", pcUid);
            }
        }

        public void OnResponse_ShovelGameStart(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SHOVEL_GAME_START>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} shovel game start can not find client", pcUid);
            }
        }

        public void OnResponse_ShovelGameRevive(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SHOVEL_GAME_REVIVE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} shovel game revive can not find client", pcUid);
            }
        }
    }
}
