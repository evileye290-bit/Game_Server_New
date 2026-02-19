using CommonUtility;
using EnumerateUtility;
using Message.Gate.Protocol.GateC;
using ServerModels;
using ServerShared;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public class CrossBossDefenseMap : CrossBattleDungeonMap
    {
        public CrossBossDefenseMap(ZoneServerApi server, int mapId, int channel) : base(server, mapId, channel)
        {
            IsSpeedUpDungeon = true;
        }

        public override Vec2 CalcBeginPos(int i, FieldObject field)
        {
            int index = i > DungeonModel.PlayerPos.Count ? 1 : i;
            if (field is PlayerChar)
            {
                index = 1;
            }

            return DungeonModel.PlayerPos[index];
        }
        public override void AddAttackerMirror(PlayerChar player)
        {
            player.IsAttacker = true;

            foreach (var queue in player.HeroMng.CrossBossQueue)
            {
                Dictionary<int, int> poses = new Dictionary<int, int>();
                List<HeroInfo> heros = new List<HeroInfo>();

                foreach (var pos in queue.Value)
                {
                    int posId = pos.Key;
                    HeroInfo heroInfo = pos.Value;
                    poses.Add(heroInfo.Id, posId);
                    heros.Add(heroInfo);
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

            foreach (var player in PcList)
            {
                NotifySpeedUpEnd(player.Value);

                //通知Relation 获取排名
                RewardManager mng = GetFinalReward(player.Value.Uid);
                mng.BreakupRewards();
                player.Value.AddRewards(mng, ObtainWay.CrossBossDefense, DungeonModel.Id.ToString());

                //通知前端奖励
                MSG_ZGC_DUNGEON_REWARD rewardMsg = player.Value.GetRewardSyncMsg(mng);
                rewardMsg.DungeonId = DungeonModel.Id;
                rewardMsg.Result = (int)DungeonResult;

                player.Value.CheckCacheRewardMsg(rewardMsg);
                player.Value.SendCrossBossDefenseResult(result, FightInfo);

                //副本类型任务计数
                PlayerAddTaskNum(player.Value);

                int pointState = GetPointState(result);
                //日志
                player.Value.BIRecordCheckPointLog((MapType)DungeonModel.Type, DungeonModel.Id.ToString(), pointState, GetFinishTime());

                //komoelog
                player.Value.KomoeLogRecordPveFight(6, 1, DungeonModel.Id.ToString(), mng.RewardList, 1, GetFinishTime());
            }

            Logger.Log.DebugLine($"speed up ---------------end----------------- dungeon result {result}");
        }
    }
}