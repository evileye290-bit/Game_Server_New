using CommonUtility;
using Logger;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    partial class MapManager
    {
        private int dungeonUid = 0;
        private int createrInstanceId = 0;

        private int GenerateDungeonChannel()
        {
            dungeonUid++;
            int channel = (server.SubId << 24) + dungeonUid;
            return channel;
        }

        public DungeonMap CreateDungeon(int mapId, int godHeroCount = 0,int firstPlayer = 0)
        {
            MapModel model = MapLibrary.GetMap(mapId);
            if(model == null)
            {
                Log.Warn($"create dungeon {mapId} failed: no such model");
                return null;
            }
            if (!model.IsDungeon())
            {
                Log.Warn($"create dungeon {mapId} failed: map type is {model.MapType}");
                return null;
            }
            DungeonMap dungeon = null;
            int channel = GenerateDungeonChannel();
            switch(model.MapType)
            {
                case MapType.CommonSingleDungeon:
                    dungeon = new DungeonMap(server, mapId, channel);
                    break;
                case MapType.TeamDungeon:
                    dungeon = new TeamDungeonMap(server, mapId, channel);
                    break;
                case MapType.Gold:
                case MapType.Exp:
                case MapType.SoulPower:
                case MapType.SoulBreath:
                    dungeon = new BenefitsDungeonMap(server, mapId, channel);
                    break;
                case MapType.Hunting:
                    dungeon = new HuntingDungeonMap(server, mapId, channel);
                    break;
                case MapType.Arena:
                    dungeon = new ArenaDungeonMap(server, mapId, channel);
                    break;
                case MapType.Versus:
                    dungeon = new VersusDungeonMap(server, mapId, channel);
                    break;
                case MapType.CrossBattle:
                    dungeon = new CrossBattleDungeonMap(server, mapId, channel);
                    break;
                case MapType.CrossFinals:
                    dungeon = new CrossBattleFinalsDungeonMap(server, mapId, channel);
                    break;
                case MapType.HuntingDeficute:
                case MapType.HuntingTeamDevil:
                    dungeon = new HuntingTeamDungeonMap(server, mapId, channel);
                    break;
                case MapType.IntegralBoss:
                    dungeon = new IntegralBossDungeonMap(server, mapId, channel);
                    break;
                case MapType.SecretArea:
                    dungeon = new SecretAreaDungeon(server, mapId, channel);
                    break;
                case MapType.Chapter:
                    dungeon = new ChapterDungeon(server, mapId, channel);
                    break;
                case MapType.NoCheckSingleDungeon:
                    dungeon = new NoCheckDungeon(server, mapId, channel);
                    break;
                case MapType.GodPath:
                    dungeon = new GodPathDungeon(server, mapId, channel, godHeroCount);
                    break;
                case MapType.GodPathAcrossOcean:
                    dungeon = new AcrossOceanDungeon(server, mapId, channel, godHeroCount);
                    break;
                case MapType.CampBattle:
                    dungeon = new CampBattleDungeon(server, mapId, channel);
                    break;
                case MapType.CampGatherEncounterEnemy:
                    dungeon = new GatherEnemyDungeonMap(server, mapId, channel);
                    break;
                case MapType.CampGatherEncounterMonster:
                    dungeon = new GatherMonsterDungeonMap(server, mapId, channel);
                    break;
                case MapType.CampDefense:
                    dungeon = new CampBattleDefenseDungeon(server, mapId, channel);
                    break;
                case MapType.Tower:
                    dungeon = new TowerDungeon(server, mapId, channel);
                    break;
                case MapType.CampBattleNeutral:
                    dungeon = new CampBattleNeutraDungeon(server, mapId, channel);
                    break;
                case MapType.PushFigure:
                    dungeon = new PushFigureDungeon(server, mapId, channel);
                    break;
                case MapType.VideoPlay:
                case MapType.CrossChallengeVideoPlay:
                    dungeon = new VideoPlayDungeon(server, mapId, channel);
                    break;
                case MapType.HuntingActivitySingle:
                    dungeon = new HuntingActivityDungeonMap(server, mapId, channel);
                    break;
                case MapType.HuntingActivityTeam:
                    dungeon = new HuntingActivityTeamDungeonMap(server, mapId, channel);
                    break;
                case MapType.ThemeBoss:
                    dungeon = new ThemeBossDungeon(server, mapId, channel);
                    break;
                case MapType.CrossBoss:
                    dungeon = new CrossBossDungeon(server, mapId, channel);
                    break;
                case MapType.CrossBossSite:
                    dungeon = new CrossBossDefenseMap(server, mapId, channel);
                    break;
                case MapType.CarnivalBoss:
                    dungeon = new CarnivalBossDungeon(server, mapId, channel);
                    break;
                case MapType.IslandChallenge:
                    dungeon = new IslandChallengeDungeon(server, mapId, channel);
                    break;
                case MapType.CrossChallenge:
                    dungeon = new CrossChallengeDungeonMap(server, mapId, channel);
                    break;
                case MapType.CrossChallengeFinals:
                    dungeon = new CrossChallengeFinalsDungeonMap(server, mapId, channel);
                    break;
                case MapType.HuntingIntrude:
                    dungeon = new HuntingIntrudeDungeonMap(server, mapId, channel);
                    break;
                case MapType.SpaceTimeTower:
                    dungeon = new SpaceTimeTowerDungeon(server, mapId, channel);
                    break;
                default:
                    Log.Warn($"create dungeon {mapId} failed: map type {model.MapType} not supported yet");
                    break;
            }
            
            dungeon?.Open(firstPlayer);
            
            return dungeon;
        }
    }
}
