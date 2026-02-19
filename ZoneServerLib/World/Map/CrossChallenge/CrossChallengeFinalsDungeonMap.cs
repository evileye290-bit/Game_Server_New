using System.Collections.Generic;
using System.Linq;
using CommonUtility;
using EnumerateUtility;
using Message.Zone.Protocol.ZR;
using ServerModels;
using ServerShared;

namespace ZoneServerLib
{
    public class CrossChallengeFinalsDungeonMap : CrossChallengeDungeonMap
    {
        public CrossChallengeFinalsDungeonMap(ZoneServerApi server, int mapId, int channel) : base(server, mapId, channel)
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

            //副本结束取消所有trigger
            //DungeonResult = result;
            State = DungeonState.Stopped;
            OnStopFighting();

            if (FightInfo.Type != ChallengeIntoType.CrossChallengeFinals) return;


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

            string filePath = BattleFpsManager.Close(DungeonResult, winUid, attacker);

            //SendCrossChallengeResult(winUid, filePath);

            player.CrossChallengeInfoMng.CacheFinalInfo(winUid, filePath);

            CheckAndGotoNextRound();
        }

        private void CheckAndGotoNextRound()
        {
            //没有当前场次的队伍信息，直接判负
            if (!FightInfo.HeroQueue.ContainsKey(BattleRound))
            {
                player.CrossChallengeInfoMng.CacheFinalInfo(playerUid, "");
            }

            if (BattleRound >= CrossChallengeLibrary.BattleRound)
            {
                int winUid = player.CrossChallengeInfoMng.BattleResult.Count(x => x == playerUid) >= 2 ? playerUid : FightInfo.Uid;

                //三场战斗完成
                SendCrossChallengeResult(winUid, player.CrossChallengeInfoMng.BattleResult, string.Join("|",player.CrossChallengeInfoMng.VideoPathList));
            }
            else
            {
                player?.CrossChallengeGotoNextRound(FightInfo, DungeonModel.Id, BattleRound + 1);
            }
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

        private static List<int> winIndex = new List<int>() {3, 4,7, 8};
        private static bool NeedReverse(int winPlayerIndex, CrossBattleTiming timing)
        {
            switch (timing)
            {
                case CrossBattleTiming.BattleTime1:
                case CrossBattleTiming.BattleTime4:
                    return winPlayerIndex % 2 == 0;
                case CrossBattleTiming.BattleTime2:
                case CrossBattleTiming.BattleTime5:
                    return winIndex.Contains(winPlayerIndex);
                case CrossBattleTiming.BattleTime3:
                case CrossBattleTiming.BattleTime6:
                    return winPlayerIndex > 4;
            }

            return false;
        }

        public void SendCrossChallengeResult(int winUid, List<int> winInfo, string fileName)
        {
            List<int> winList = winInfo.ConvertAll(x => x == winUid ? 1 : 0);

            int index = 0;
            FightInfo.HeroIndex.TryGetValue(winUid, out index);
            if (NeedReverse(index, (CrossBattleTiming)FightInfo.TimingId))
            {
                for (int i = 0; i < winList.Count; i++)
                {
                    if (winList[i] == 0)
                    {
                        winList[i] = 1;
                    }
                    else if (winList[i] == 1)
                    {
                        winList[i] = 0;
                    }
                }
            }

            string winInfoStr = string.Join("_", winList);

            //获取到玩家2 信息，开始战斗
            MSG_ZR_SET_CROSS_CHALLENGE_BATTLE_RESULT addMsg = new MSG_ZR_SET_CROSS_CHALLENGE_BATTLE_RESULT
            {
                TimingId = FightInfo.TimingId,
                GroupId = FightInfo.GroupId,
                TeamId = FightInfo.TeamId,
                FightId = FightInfo.FightId,
                FileName = fileName,
                BattleInfo = winInfoStr
            };


            addMsg.WinUid = index;

            server.RelationServer.Write(addMsg, FightInfo.Uid);
        }
    }
}