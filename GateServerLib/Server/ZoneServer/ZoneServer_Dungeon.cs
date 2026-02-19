using Message.Gate.Protocol.GateC;
using Message.IdGenerator;
using System.IO;

namespace GateServerLib
{
    partial class ZoneServer
    {
        private void OnResponse_CreateDungeon(MemoryStream stream, int uid)
        {
            Client client = Api.ClientMng.FindClientByUid(uid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CREATE_DUGEON>.Value, stream);
            }
        }

        private void OnResponse_DungeonReward(MemoryStream stream, int uid)
        {
            Client client = Api.ClientMng.FindClientByUid(uid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_DUNGEON_REWARD>.Value, stream);
            }
        }

        private void OnResponse_DungeonStopTime(MemoryStream stream, int uid)
        {
            Client client = Api.ClientMng.FindClientByUid(uid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_DUNGEON_STOPTIME>.Value, stream);
            }
        }

        private void OnResponse_EnergyInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_TEAM_DUNGEON_ENERGY_INFO>.Value, stream);
            }
        }

        private void OnResponse_NPCAppear(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_NPC_APPEAR>.Value, stream);
            }
        }

        private void OnResponse_NPCDisappear(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_NPC_DISAPPEAR>.Value, stream);
            }
        }

        private void OnResponse_ReEnterDungeon(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_REENTER_DUNGEON>.Value, stream);
            }
        }

        private void OnResponse_Revive(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_REVIVE>.Value, stream);
            }
        }

        private void OnResponse_BattleEndTime(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_BATTLE_END_TIME>.Value, stream);
            }
        }

        private void OnResponse_DungeonStartTime(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_DUNGEON_START>.Value, stream);
            }
        }

        private void OnResponse_DungeonLoadingDone(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_DUNGEON_LOADINGDONE>.Value, stream);
            }
        }


        private void OnResponse_TeamMemberLoadingDone(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_TEAMMEMBER_LOADINGDONE>.Value, stream);
            }
        }

        private void OnResponse_BattleStageChange(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_DUNGEON_BATTLE_STAGE>.Value, stream);
            }
        }

        private void OnResponse_DungeonHeroInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_DUNGEON_EQUIPED_HERO>.Value, stream);
            }
        }

        private void OnResponse_MonsterGeneratedWalk(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_MONSTER_GENERATED_WALK>.Value, stream);
            }
        }

        private void OnResponse_DungeonBattleData(MemoryStream stream, int pcUid = 0)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_DUNGEON_BATTLE_DATA>.Value, stream);
            }
        }

        public void OnResponse_RequestQuitDungeon(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_REQUEST_QUIT_DUNGEON>.Value, stream);
            }
        }

        public void OnResponse_ResponseVerifyQuitDungeon(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_RSPONSE_VERIFY_QUIT_DUNGEON>.Value, stream);
            }
        }

        private void OnResponse_DungeonSpeedUp(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_DUNGEON_SPEED_UP>.Value, stream);
            }
        }

        private void OnResponse_DungeonSpeedUpEnd(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_DUNGEON_SPEEDUP_END>.Value, stream);
            }
        }

        private void OnResponse_DungeonSkipBattle(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_DUNGEON_SKIP_BATTLE>.Value, stream);
            }
        }
    }
}
