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
        private void OnResponse_GetHidderWeaponInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_HIDDER_WEAPON_VALUE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GetHidderWeaponInfo not find client", pcUid);
            }
        }

        private void OnResponse_GetHidderWeaponReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_HIDDER_WEAPON_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GetHidderWeaponReward not find client", pcUid);
            }
        }

        private void OnResponse_GetHidderWeaponNumReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_HIDDER_WEAPON_NUM_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GetHidderWeaponNumReward not find client", pcUid);
            }
        }

        private void OnResponse_BuyHidderWeaponItem(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_BUY_HIDDER_WEAPON_ITEM>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} BuyHidderWeaponItem not find client", pcUid);
            }
        }

        private void OnResponse_GetSeaTreasureInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SEA_TREASURE_VALUE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GetSeaTreasurenInfo not find client", pcUid);
            }
        }

        private void OnResponse_GetSeaTreasureReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_SEA_TREASURE_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GetSeaTreasureReward not find client", pcUid);
            }
        }

        private void OnResponse_BuySeaTreasureItem(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_BUY_SEA_TREASURE_ITEM>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} BuySeaTreasureItem not find client", pcUid);
            }
        }

        private void OnResponse_GetSeaTreasureBlessing(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_SEA_TREASURE_BLESSING>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GetSeaTreasureBlessing not find client", pcUid);
            }
        }

        private void OnResponse_GetSeaTreasureNumReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_SEA_TREASURE_NUM_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GetSeaTreasureNumReward not find client", pcUid);
            }
        }

        private void OnResponse_CloseSeaTreasureBlessing(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CLOSE_SEA_TREASURE_BLESSING>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} CloseSeaTreasureBlessing not find client", pcUid);
            }
        }
    }
}
