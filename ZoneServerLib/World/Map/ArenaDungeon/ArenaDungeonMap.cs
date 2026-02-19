using CommonUtility;
using EnumerateUtility;
using ServerModels;
using ServerShared;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    public class ArenaDungeonMap : DungeonMap
    {
        private PlayerRankBaseInfo arenaRankInfo = null;

        public ArenaDungeonMap(ZoneServerApi server, int mapId, int channel) : base(server, mapId, channel)
        {
            IsSpeedUpDungeon = true;
        }

        public override void OnStopBattle(PlayerChar player)
        {
            Stop(DungeonResult.Failed);
        }

        public override void Stop(DungeonResult result)
        {
            // 已经有胜负结果，不再更新（防止临界状态下下，有可能又赢又输）
            if (DungeonResult != DungeonResult.None)
            {
                return;
            }

            //副本结束取消所有trigger
            DungeonResult = result;
            State = DungeonState.Stopped;
            OnStopFighting();

            SetSpeedUp(false);

            if (arenaRankInfo != null)
            {
                foreach (var player in PcList)
                {
                    NotifySpeedUpEnd(player.Value);                  

                    player.Value.SendChallengeResult(result, arenaRankInfo);

                    //副本类型任务计数
                    PlayerAddTaskNum(player.Value);

                    //战斗录像
                    //BattleFpsManager.Close(DungeonResult, result == DungeonResult.Success ? player.Value.Uid: arenaRankInfo.Uid, player.Value.Uid);

                    int pointState = GetPointState(result);
                    //日志
                    player.Value.BIRecordCheckPointLog((MapType)DungeonModel.Type, DungeonModel.Id.ToString(), pointState, GetFinishTime());                 
                }
            }
            else
            {
                Failed();
            }

            Logger.Log.DebugLine($"speed up ---------------end----------------- dungeon result {result}");
        }

        public override void OnPlayerMapLoadingDone(PlayerChar player)
        {
            NotifyIsFirstStart(player);
            NotifyDungeonStopTime(player);
            NotifyEnrgyCanInfo(player);
            switch (State)
            {
                // 副本还在准备阶段，那么不需要再做额外等待，直接开始
                case DungeonState.Open:
                    Start();
                    break;
                // 副本已经开始 则开始战斗
                case DungeonState.Started:
                    //player.StartFighting();
                    //StartHeros(player);
                    //Start();
                    break;
                default:
                    break;
            }
            if (player.GetReEnterDungeon())
            {
                SyncReEnterDungeonInfo(player);
                player.SetReEnterDungeon(false);
            }
        }

        public void AddAttackerMirror(PlayerChar player)
        {
            AddMirrorRobot(true,player);
        }

        public void AddArenaDefender(PlayerRankBaseInfo info)
        {
            arenaRankInfo = info;
            List<HeroInfo> infos = RobotManager.GetHeroList(arenaRankInfo.HeroInfos);

            Dictionary<int, int> heroPoses = new Dictionary<int, int>();
            foreach(var item in arenaRankInfo.Defensive)
            {
                heroPoses.Add(item, arenaRankInfo.DefPoses[arenaRankInfo.Defensive.IndexOf(item)]);
            }
            if (infos.Count == 0)
            {
                string def = "";
                arenaRankInfo.Defensive.ForEach(kv => def += "|" + kv);
                arenaRankInfo.DefPoses.ForEach(kv => def += "|" + kv);
                Logger.Log.Warn($"arena add {def} to got no infos");
            }
            HeroInfo temp = infos.First();
            temp.RobotInfo.Name = info.Name;
            temp.RobotInfo.Sex= info.Sex;

            Dictionary<int, PetInfo> queuePet = null;
            if (arenaRankInfo.PetInfo != null)
            {
                PetInfo petInfo = RobotManager.GetPetInfoFromRobot(arenaRankInfo.PetInfo);
                queuePet = new Dictionary<int, PetInfo>();
                queuePet.Add(1, petInfo);
            }

            AddRobotAndHeros(false,infos, info.Uid, info.NatureValues, info.NatureRatios, heroPoses, queuePet);
        }

        public override void CreateHero(Hero hero, bool add2Aoi = true)
        {
            base.CreateHero(hero, add2Aoi);

            if (hero.IsAttacker)
            {
                hero.SetHeroOneSideWalkVec(ArenaLibrary.ChallengerWalkVec);
            }
            else
            {
                hero.SetHeroOneSideWalkVec(ArenaLibrary.DefenderWalkVec);
            }

            //hero.SetHeroOneSideWalkVec(walkVec);
        }
    }
}
