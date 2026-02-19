using Logger;
using Message.Gate.Protocol.GateC;
using Message.IdGenerator;
using System.IO;

namespace GateServerLib
{
    public partial class ZoneServer
    {
        private void OnResponse_TowerInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_TOWER_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} TowerInfo not find client", pcUid);
            }
        }

        private void OnResponse_TowerReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_TOWER_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} owerReward not find client", pcUid);
            }
        }

        private void OnResponse_TowerShopItemList(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_TOWER_SHOP_ITEM>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} TowerShopItemList not find client", pcUid);
            }
        }

        private void OnResponse_TowerTime(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_TOWER_TIME>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} TowerTime not find client", pcUid);
            }
        }

        private void OnResponse_TowerExecuteTask(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_TOWER_EXECUTE_TASK>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} TowerExecuteTask not find client", pcUid);
            }
        }

        private void OnResponse_TowerSelectBuff(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_TOWER_SELECT_BUFF>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} TowerSelectBuff not find client", pcUid);
            }
        }

        private void OnResponse_TowerBuff(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_TOWER_BUFF>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} TowerBuff not find client", pcUid);
            }
        }

        private void OnResponse_TowerRandomBuff(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_TOWER_RANDOM_BUFF>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} TowerRandomBuff not find client", pcUid);
            }
        }
        private void OnResponse_TowerHeroPos(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_UPDATE_TOWER_HERO_POS>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} TowerHeroPos not find client", pcUid);
            }
        }

        private void OnResponse_TowerHeroInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_INIT_TOWER_HERO_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} TowerHeroInfo not find client", pcUid);
            }
        }

        private void OnResponse_TowerReviveHero(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_TOWER_HERO_REVIVE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} TowerReviveHero not find client", pcUid);
            }
        }

        private void OnResponse_TowerDungeonGrowth(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_TOWER_DUNGOEN_GROWTH>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} TowerDungeonGrowth not find client", pcUid);
            }
        }
    }
}
