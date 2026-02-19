using CommonUtility;
using ServerModels;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    public class VersusDungeonMap : DungeonMap
    {
        public VersusDungeonMap(ZoneServerApi server, int mapId, int channel) : base(server, mapId, channel)
        {
            canRevive = false;
            //mixSkillManager = null;
        }

        private PlayerRankBaseInfo arenaRankInfo = null;

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

            if (arenaRankInfo != null)
            {
                foreach (var player in PcList)
                {
                    player.Value.SendChallengeResult(result);

                    //副本类型任务计数
                    PlayerAddTaskNum(player.Value);

                    int pointState = GetPointState(result);
                    //日志
                    player.Value.BIRecordCheckPointLog((MapType)DungeonModel.Type, DungeonModel.Id.ToString(), pointState, GetFinishTime());

                    player.Value.KomoeLogRecordPvpFight(3, 1, null, pointState, 0, 0, "0", "0", arenaRankInfo.Uid, arenaRankInfo.BattlePower, GetFinishTime());
                }
            }
            else
            {
                Failed();
            }
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

        public void AddDefenderRobot(PlayerRankBaseInfo info, List<HeroInfo> infos, PetInfo petInfo = null)
        {
            arenaRankInfo = info;

            Dictionary<int, int> heroPoses = new Dictionary<int, int>();
            foreach (var item in arenaRankInfo.Defensive)
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
            temp.RobotInfo.Sex = info.Sex;

            Dictionary<int, PetInfo> queuePet = null;
            if (petInfo != null)
            {
                queuePet = new Dictionary<int, PetInfo>();
                queuePet.Add(1, petInfo);
            }
            AddRobotAndHeros(false, infos, info.Uid, info.NatureValues, info.NatureRatios, heroPoses, queuePet);
        }

        internal void AddAttackerMirror(PlayerChar playerChar)
        {
            AddMirrorRobot(true, playerChar);
        }
    }
}
