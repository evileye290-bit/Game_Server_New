using System.Collections.Generic;
using System.Linq;
using CommonUtility;
using EnumerateUtility;
using Logger;
using ServerModels;

namespace ZoneServerLib
{
    public class CrossChallengeDungeonMap : DungeonMap
    {
        protected int playerUid;
        protected PlayerChar player;
        protected PlayerCrossFightInfo FightInfo;

        //战斗轮次
        protected int BattleRound;

        public CrossChallengeDungeonMap(ZoneServerApi server, int mapId, int channel) : base(server, mapId, channel)
        {
            IsSpeedUpDungeon = true;
        }

        protected override void InitBattleFpsManager()
        {
            BattleFpsManager = new CrossChallengeFpsManager(this);
        }

        public override void OnStopBattle(PlayerChar player)
        {
            Stop(DungeonResult.Failed);
            OnSkipBattle(player);
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

            if (FightInfo.Type == ChallengeIntoType.CrossChallengePreliminary)
            {
                foreach (var player in PcList)
                {
                    NotifySpeedUpEnd(player.Value);

                    player.Value.SendCrossChallengeResult(result);

                    //战斗录像
                    BattleFpsManager.Close(DungeonResult, result == DungeonResult.Success ? player.Value.Uid : FightInfo.Uid, player.Value.Uid);

                    int pointState = GetPointState(result);
                    //日志
                    player.Value.BIRecordCheckPointLog((MapType)DungeonModel.Type, DungeonModel.Id.ToString(), pointState, GetFinishTime());
                }
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

        public void SetBattleRound(int round)
        {
            BattleRound = round;
        }

        public void SetChallengeIntoType(PlayerCrossFightInfo fightInfo, int playerUid)
        {
            this.FightInfo = fightInfo;
            this.playerUid = playerUid;
        }

        public virtual void AddAttackerMirror(PlayerChar player)
        {
            this.player = player;
            player.IsAttacker = true;

            Dictionary<int, HeroInfo> heroInfos;
            if (!player.HeroMng.CrossChallengeQueue.TryGetValue(BattleRound, out heroInfos))
            {
                Log.Error($"cross challenge had not find attacker {player.Uid} round {BattleRound} hero queue");
                return;
            }

            List<HeroInfo> heros = heroInfos.Values.ToList();
            Dictionary<int, int> poses = new Dictionary<int, int>();

            foreach (var pos in heroInfos)
            {
                poses.Add(pos.Value.Id, pos.Key);
            }

            if (player.Uid > 0)
            {
                Robot robot = Robot.CopyFromPlayer(server, player);
                robot.IsAttacker = true;
                robot.EnterMap(this);
                robot.SetOwnerUid(player.Uid);
                base.AddRobot(robot);

                robot.SetHeroPoses(poses);
                robot.SetHeroInfos(heros);
                robot.CopyHeros2CrossMap(player);
            }
            else
            {
                AddRobotAndHeros(true, heros, player.Uid, player.NatureValues, player.NatureRatios, poses);
            }
        }

        public void AddCrossDefender(PlayerCrossFightInfo info, int ownerUid = -1)
        {
            Dictionary<int, RobotHeroInfo> heroInfos;
            if (!info.HeroQueue.TryGetValue(BattleRound, out heroInfos))
            {
                Log.Error($"cross challenge had not find defender round {BattleRound} hero queue");
                return;
            }

            List<HeroInfo> infos = new List<HeroInfo>();
            Dictionary<int, int> heroPoses = new Dictionary<int, int>();
            foreach (var kv in heroInfos)
            {
                heroPoses.Add(kv.Value.HeroId, kv.Key);

                HeroInfo heroInfo = RobotManager.InitFromRobotInfo(kv.Value);
                heroInfo.CrossChallengeQueueNum = BattleRound;
                heroInfo.CrossChallengePositionNum = kv.Key;
                infos.Add(heroInfo);
            }

            HeroInfo temp = infos.First();
            temp.RobotInfo.Name = info.Name;
            temp.RobotInfo.Sex = info.Sex;
            AddRobotAndHeros(false, infos, ownerUid, info.NatureValues, info.NatureRatios, heroPoses);
        }
    }
}