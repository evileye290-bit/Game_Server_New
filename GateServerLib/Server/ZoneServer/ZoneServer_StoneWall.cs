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
        private void OnResponse_GetStoneWallInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_STONE_WALL_VALUE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GetStoneWallInfo not find client", pcUid);
            }
        }

        private void OnResponse_GetStoneWallInfoList(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_STONE_WALL_INFO_LIST>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GetStoneWallInfoList not find client", pcUid);
            }
        }

        private void OnResponse_GetStoneWallReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_STONE_WALL_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GetStoneWallReward not find client", pcUid);
            }
        }

        private void OnResponse_BuyStoneWallItem(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_BUY_STONE_WALL_ITEM>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} BuyStoneWallItem not find client", pcUid);
            }
        }

        private void OnResponse_ResetStoneWall(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_RESET_STONE_WALL>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} ResetStoneWall not find client", pcUid);
            }
        }
    }
}
