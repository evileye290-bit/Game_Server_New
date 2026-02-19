using CommonUtility;
using EnumerateUtility;
using Google.Protobuf.Collections;
using Logger;
using Message.Gate.Protocol.GateC;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{

    public class CampBattleDefenseDungeon : DungeonMap
    {
        private Dictionary<int, Int64> heroBeforeHP = new Dictionary<int, Int64>();

        private Dictionary<int, Hero> attackerHeroList = new Dictionary<int, Hero>();

        private Dictionary<NatureType, float> playerAddNatures = new Dictionary<NatureType, float>();
        private Dictionary<NatureType, float> defenseAddNatures = new Dictionary<NatureType, float>();

        public CampBattleDefenseDungeon(ZoneServerApi server, int mapId, int channel) : base(server, mapId, channel)
        {
        }

        public void SetDefenseNature(MapField<int, float> addNature)
        {
            addNature.ForEach(x => defenseAddNatures.Add((NatureType)x.Key, x.Value));
        }

        public void SetPlayerNature(MapField<int, float> addNature)
        {
            addNature.ForEach(x => playerAddNatures.Add((NatureType)x.Key, x.Value));
        }

        public override void CreateHero(Hero hero, bool add2Aoi = true)
        {
            if (hero == null) return;
            PlayerChar player = hero.Owner as PlayerChar;
            if (player == null)
            {
                if (defenseAddNatures.Count > 0)
                {
                    foreach (var item in defenseAddNatures)
                    {
                        long value = (long)(hero.GetNatureValue(item.Key) * (1 + item.Value));
                        hero.SetNatureBaseValue(item.Key, value);
                    }
                    hero.SetNatureBaseValue(NatureType.PRO_HP, hero.GetMaxHp());
                }
            }
            else
            {
                if (playerAddNatures.Count > 0)
                {
                    foreach (var item in playerAddNatures)
                    {
                        long value = (long)(hero.GetNatureValue(item.Key) * (1 + item.Value));
                        hero.SetNatureBaseValue(item.Key, value);
                    }
                    hero.SetNatureBaseValue(NatureType.PRO_HP, hero.GetMaxHp());
                }
            }
            base.CreateHero(hero, add2Aoi);
        }

        public void AddDefenderRobotHero(List<RobotHeroInfo> heroInfos, Dictionary<int, Int64> heroHp, int ownerUid,
            Dictionary<int, int> natureValues, Dictionary<int, int> natureRatios)
        {
            heroBeforeHP = heroHp;
            List<HeroInfo> infos = RobotManager.GetHeroList(heroInfos);

            Dictionary<int, int> heroPoses = new Dictionary<int, int>();
            for (int i = 0; i < 9; i++)
            {
                foreach (var item in heroInfos)
                {
                    if (item.HeroPos == i)
                    {
                        heroPoses.Add(item.HeroId, item.HeroPos);
                        break;
                    }
                }
            }

            if (heroPoses.Count < heroInfos.Count)
            {
                int i = 0;
                heroPoses.Clear();
                foreach (var item in heroInfos)
                {
                    heroPoses.Add(item.HeroId, i);
                    i++;
                }
            }

            HeroInfo temp = infos.First();
            temp.RobotInfo.Name = "";
            temp.RobotInfo.Sex = 0;
            AddRobotAndHeros(false, infos, ownerUid, natureValues, natureRatios, heroPoses);
        }

        int FortId;
        int DugeonIndex;
        internal void SetFortId(int fortId, int dugeonIndex)
        {
            FortId = fortId;
            DugeonIndex = dugeonIndex;
        }

        public override void OnStopBattle(PlayerChar player)
        {
            isQuitDungeon = true;
            Stop(DungeonResult.Failed);
            player.LeaveDungeon();
        }

        public override void Stop(DungeonResult result)
        {
            SetSpeedUp(false);

            //已经有胜负结果，不再更新（防止临界状态下下，有可能又赢又输）
            if (DungeonResult != DungeonResult.None)
            {
                return;
            }

            // 副本结束取消所有trigger
            DungeonResult = result;
            State = DungeonState.Stopped;

            switch (result)
            {
                case DungeonResult.Success:
                    Success();
                    break;
                case DungeonResult.Failed:
                    Failed();
                    break;
                case DungeonResult.Tie:
                    Tie();
                    break;
                default:
                    break;
            }

            OnStopFighting();
        }

        protected override void Failed()
        {
            //手动退出不给前端推送结算面板
            if (!isQuitDungeon)
            {
                base.Failed();
            }

            PlayerChar player = PcList.Values.FirstOrDefault();
            if (player == null) return;

            NotifyBattleEnd(player);
            NotifySpeedUpEnd(player);

            player.CampBattleAddScore(2);

            //日志
            int pointState = 2;
            if (isQuitDungeon)
            {
                pointState = 3;
            }
            player.BIRecordCheckPointLog((MapType)DungeonModel.Type, DungeonModel.Id.ToString(), pointState, GetFinishTime());

            List<Dictionary<string, object>> consume = player.ParseConsumeInfoToList(null, (int)CounterType.ActionCount, 1);
            player.KomoeEventLogCampBattle(((int)player.Camp).ToString(), player.Camp.ToString(), 0, 3, player.HeroMng.CalcBattlePower(), GetFinishTime(), pointState, "", "", "", 0, consume);
        }

        protected override void Success()
        {
            DoReward();
            PlayerChar player = PcList.Values.FirstOrDefault();
            if (player == null) return;

            NotifyBattleEnd(player);

            try
            {
                //RewardManager mng = GetFinalReward(player.Uid);
                //mng.BreakupRewards();
                player.CampBattleAddScore(1);

                NotifySpeedUpEnd(player);

                //通知前端奖励
                MSG_ZGC_DUNGEON_REWARD rewardMsg = new MSG_ZGC_DUNGEON_REWARD();
                rewardMsg.DungeonId = DungeonModel.Id;
                rewardMsg.Result = (int)DungeonResult;
                //player.Write(rewardMsg);

                //加速战斗需要缓存奖励信息
                player.CheckCacheRewardMsg(rewardMsg);

                //副本类型任务计数
                PlayerAddTaskNum(player);

                //增加伙伴经验
                player.AddHeroExp(DungeonModel.HeroExp);

                //日志
                player.BIRecordCheckPointLog((MapType)DungeonModel.Type, DungeonModel.Id.ToString(), 1, GetFinishTime());

                List<Dictionary<string, object>> consume = player.ParseConsumeInfoToList(null, (int)CounterType.ActionCount, 1);
                player.KomoeEventLogCampBattle(((int)player.Camp).ToString(), player.Camp.ToString(), 0, 3, player.HeroMng.CalcBattlePower(), GetFinishTime(), 1, "", "", "", 0, consume);
            }
            catch (Exception ex)
            {
                Log.Alert(ex);
            }

            ResetReward();
        }

        protected override void OnSkipBattle(PlayerChar player)
        {
            base.OnSkipBattle(player);
            if (HadSpeedUp)
            {
                player.NotifyReleationBattleResult(FortId, DugeonIndex, (MapType)DungeonModel.Type, DungeonResult);
            }
        }

        private void NotifyBattleEnd(PlayerChar player)
        {
            //当前启动了加速，后端比前端早出战斗结果
            if (HadSpeedUp)
            {
                return;
            }

            //Dictionary<int, Int64> defenderHeroDamage = new Dictionary<int, Int64>();

            //foreach (var kv in HeroList)
            //{
            //    if (kv.Value.Owner is PlayerChar) continue;

            //    int heroId = kv.Value.HeroId;
            //    if (heroBeforeHP.ContainsKey(heroId))
            //    {
            //        defenderHeroDamage.Add(heroId, Math.Max(0, heroBeforeHP[heroId] - kv.Value.GetNatureValue(NatureType.PRO_HP)));
            //    }
            //}

            player.NotifyReleationBattleResult(FortId, DugeonIndex, (MapType)DungeonModel.Type, DungeonResult);
        }
    }
}
