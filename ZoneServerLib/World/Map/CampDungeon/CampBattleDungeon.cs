using CommonUtility;
using Google.Protobuf.Collections;
using Logger;
using Message.Gate.Protocol.GateC;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    public class CampBattleDungeon : DungeonMap
    {
        private Dictionary<int, Hero> attackerHeroList = new Dictionary<int, Hero>();

        private Dictionary<NatureType, float> playerAddNatures = new Dictionary<NatureType, float>();
        private Dictionary<NatureType, float> monsterAddNatures = new Dictionary<NatureType, float>();
        /// <summary>
        /// 这个类型必需是单个怪
        /// </summary>
        /// <param name="server"></param>
        /// <param name="mapId"></param>
        /// <param name="channel"></param>
        public CampBattleDungeon(ZoneServerApi server, int mapId, int channel) : base(server, mapId, channel)
        {
        }

        public void SetMonsterNature(MapField<int, float> addNature)
        {
            addNature.ForEach(x => monsterAddNatures.Add((NatureType)x.Key, x.Value));
        }
        public void SetPlayerNature(MapField<int, float> addNature)
        {
            addNature.ForEach(x => playerAddNatures.Add((NatureType)x.Key, x.Value));
        }
        //public override Monster CreateMonster(int id, Vec2 position, BaseMonsterGen monGenerator, long hp)
        //{
        //    //因为血量传入hp是一个怪的逻辑,需要将血量传入，aoi的时候会广播血量
        //    Monster monster = base.CreateMonster(id, position, monGenerator, 1000);

        //    if (monsterAddNatures.Count > 0)
        //    {
        //        foreach (var item in monsterAddNatures)
        //        {
        //            long value = (long)(monster.GetNatureValue(item.Key) * (1 + item.Value));
        //            monster.SetNatureBaseValue(item.Key, value);
        //        }
        //        monster.SetNatureBaseValue(NatureType.PRO_HP, monster.GetMaxHp());
        //    }

        //    return monster;
        //}

        public override void CreateHero(Hero hero, bool add2Aoi = true)
        {
            if (hero == null) return;

            if (playerAddNatures.Count > 0)
            {
                foreach (var item in playerAddNatures)
                {
                    long value = (long)(hero.GetNatureValue(item.Key) * (1 + item.Value));
                    hero.SetNatureBaseValue(item.Key, value);
                }
                hero.SetNatureBaseValue(NatureType.PRO_HP, hero.GetMaxHp());
            }

            base.CreateHero(hero, add2Aoi);
        }

        public void RecordHeros()
        {
            attackerHeroList.Clear();
            PlayerChar player = PcList.Values.FirstOrDefault();
            if (player == null) return;
            foreach (var equip in player.HeroMng.GetHeroPos())
            {
                int heroId = equip.Key;
                HeroInfo info = player.HeroMng.GetHeroInfo(heroId);
                if (info == null)
                {
                    return;
                }
                Hero hero = new Hero(server, player, info);
                hero.InitNatureExt(player.NatureValues, player.NatureRatios);
                hero.InitNatures(info);

                attackerHeroList.Add(hero.HeroId, hero);
            }
        }

        protected override void Start()
        {
            RecordHeros();
            base.Start();
        }

        public override void OnStopBattle(PlayerChar player)
        {
            base.OnStopBattle(player);
            player.LeaveDungeon();
        }

        public override void Stop(DungeonResult result)
        {
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

            player.CampBattleAddScore(2);

            //日志
            int pointState = 2;
            if (isQuitDungeon)
            {
                pointState = 3;
            }
            player.BIRecordCheckPointLog((MapType)DungeonModel.Type, DungeonModel.Id.ToString(), pointState, GetFinishTime());
        }

        protected override void Success()
        {
            DoReward();
            PlayerChar player = PcList.Values.FirstOrDefault();
            if (player == null) return;

            NotifyBattleEnd(player);

            try
            {
                player.CampBattleAddScore(1);

                RewardManager manager = GetFinalReward(player.Uid);
                manager.BreakupRewards();
                player.AddRewards(manager, ObtainWay.CampBattle);

                MSG_ZGC_DUNGEON_REWARD rewardMsg = new MSG_ZGC_DUNGEON_REWARD();
                manager.GenerateRewardMsg(rewardMsg.Rewards);

                rewardMsg.DungeonId = DungeonModel.Id;
                rewardMsg.Result = (int)DungeonResult;
                player.Write(rewardMsg);

                //副本类型任务计数
                PlayerAddTaskNum(player);

                //增加伙伴经验
                player.AddHeroExp(DungeonModel.HeroExp);

                //日志
                player.BIRecordCheckPointLog((MapType)DungeonModel.Type, DungeonModel.Id.ToString(), 1, GetFinishTime());
            }
            catch (Exception ex)
            {
                Log.Alert(ex);
            }

            ResetReward();
        }

        private void NotifyBattleEnd(PlayerChar player)
        {
            Monster monster = MonsterList.Values.FirstOrDefault();

            long damage = (monster == null ? 0 : monster.GetNatureValue(NatureType.PRO_HP));

            player.NotifyReleationBattleResult(FortId,DugeonIndex, (MapType)DungeonModel.Type, DungeonResult);
        }

        int FortId;
        int DugeonIndex;
        internal void SetFortId(int fortId,int dugeonIndex)
        {
            FortId = fortId;
            DugeonIndex = dugeonIndex;
        }
    }
}
