using CommonUtility;
using EnumerateUtility;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    public class CrossBattleFinalsDungeonMap : CrossBattleDungeonMap
    {
        public CrossBattleFinalsDungeonMap(ZoneServerApi server, int mapId, int channel) : base(server, mapId, channel)
        {
            canRevive = false;
            HadSpeedUp = false;
            IsSpeedUpDungeon = false;

            SetKickPlayerDelayTime(10);
        }

        public override void Update(float dt)
        {
            base.Update(dt);

            StartBattle();
        }

        public void StartBattle()
        {
            if (State == DungeonState.Open)
            {
                Start();
                AddFps();
            }
        }

        private void AddFps()
        {
            FpsNum++;
            BattleFpsManager.CheckChanged();
        }

        public override void Stop(DungeonResult result)
        {
            // 已经有胜负结果，不再更新（防止临界状态下下，有可能又赢又输）
            if (DungeonResult != DungeonResult.None)
            {
                return;
            }

            //战斗录像
            int winUid = 0;
            int attacker = playerUid;

            ////需要在此计算，否则OnStopFighting会清除血量信息
            //if (result == DungeonResult.TimeOut)
            //{
            //    winUid = GetCrossWinner(playerUid, FightInfo.Uid);
            //}

            //副本结束取消所有trigger
            //DungeonResult = result;
            State = DungeonState.Stopped;
            OnStopFighting();

            if (FightInfo == null || FightInfo.Type != ChallengeIntoType.CrossFinals) return;


            winUid = GetCrossWinner(playerUid, FightInfo.Uid);
            if (winUid == playerUid)
            {
                DungeonResult = DungeonResult.Success;
                Success();
            }
            else
            {
                DungeonResult = DungeonResult.Failed;
                Failed();
            }
            //switch (result)
            //{
            //    case DungeonResult.Success:
            //        Success();
            //        winUid = playerUid;
            //        break;
            //    case DungeonResult.Failed:
            //        winUid = FightInfo.Uid;
            //        Failed();
            //        break;
            //    case DungeonResult.Tie:
            //        Tie();
            //        break;
            //    default:
            //        break;
            //}

            string filePath = BattleFpsManager.Close(DungeonResult, winUid, attacker);
            SendCrossBattleResult(winUid, filePath);
        }

        private int GetCrossWinner(int attackerUid, int defenderUid)
        {
            //平局剩余人数多者胜利，人数相同血量高者胜利
            int uid = 0;
            List<Hero> attacker = new List<Hero>();
            List<Hero> defender = new List<Hero>();

            foreach (var kv in HeroList)
            {
                if (kv.Value.IsAttacker)
                {
                    if (!kv.Value.IsDead)
                    {
                        attacker.Add(kv.Value);
                    }
                }
                else
                {
                    if (!kv.Value.IsDead)
                    {
                        defender.Add(kv.Value);
                    }
                }
            }

            if (attacker.Count == defender.Count)
            {
                uid = attacker.Sum(x => x.GetHp()) > defender.Sum(x => x.GetHp()) ? attackerUid : defenderUid;
            }
            else
            {
                uid = attacker.Count > defender.Count ? attackerUid : defenderUid;
            }

            return uid;
        }

    }
}