using CommonUtility;
using EnumerateUtility;
using EnumerateUtility.Activity;
using Logger;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using ServerModels;

namespace ZoneServerLib
{
    public class HuntingDungeonMap : DungeonMap
    {
        public HuntingDungeonMap(ZoneServerApi server, int mapId, int channel) : base(server, mapId, channel)
        {
        }

        protected override bool HadPassedHuntingDungeon()
        {
            return PcList.Values.First()?.HuntingManager?.CheckPassedByDungeonId(DungeonModel.Id) == true;
        }


        protected override bool CheckHuntingPeriodBuffEffectCondition()
        {
            return PcList.Values.First()?.CheckHuntingPeriodBuffEffect() == true;
        }

        protected override void Start()
        {
            List<int> researchList = new List<int>();
            PcList.ForEach(x => researchList.Add(x.Value.HuntingManager.Research));

            if (researchList.Count > 0)
            {
                //首次打改副本，副本难度降低
                float growth = HuntingLibrary.GetGrowth(researchList.Max());
                float discount = HadPassedHuntingDungeon() ? 1.0f : HuntingLibrary.Discount;
                SetMonsterGrowth(growth * discount);
            }

            base.Start();
        }


        protected override void Failed()
        {
            base.Failed();

            PlayerChar player;
            foreach (var kv in PcList)
            {
                player = kv.Value;

                //日志
                int pointState = 2;
                if (isQuitDungeon)
                {
                    pointState = 3;
                }
                player.BIRecordCheckPointLog((MapType)DungeonModel.Type, DungeonModel.Id.ToString(), pointState, GetFinishTime());

                //komoelog
                kv.Value.KomoeLogRecordPveFight(2, 1, DungeonModel.Id.ToString(), null, pointState, GetFinishTime());
            }
        }

        protected override void Success()
        {
            DoReward();
            bool huntingIntrude = CheckHuntingIntrude();

            foreach (var kv in PcList)
            {
                try
                {
                    PlayerChar player = kv.Value;
                    RewardManager mng = GetFinalReward(player.Uid);
                    mng.BreakupRewards();
                    player.HuntingReward(mng, DungeonModel, this);

                    if (huntingIntrude)
                    {
                        player.AddHuntingIntrude();
                    }

                    //副本类型任务计数
                    PlayerAddTaskNum(player);

                    //增加伙伴经验
                    player.AddHeroExp(DungeonModel.HeroExp);

                    //日志
                    player.BIRecordCheckPointLog((MapType)DungeonModel.Type, DungeonModel.Id.ToString(), 1, GetFinishTime());

                    //komoelog
                    player.KomoeLogRecordPveFight(2, 1, DungeonModel.Id.ToString(), mng.RewardList, 1, GetFinishTime());

                    player.AddRunawayActivityNumForType(RunawayAction.Hunting);
                }
                catch (Exception ex)
                {
                    Log.Alert(ex);
                }
            }

            ResetReward();
        }

        private bool CheckHuntingIntrude()
        {
            if (PcList.Count == 0) return false;

            if (PcList.Values.FirstOrDefault(x => x.HuntingManager.Research < HuntingLibrary.HuntingIntrudeResearchLimit) != null)
            {
                return false;
            }

            return HuntingLibrary.HuntingIntrudeProbability >= RAND.Range(0, 10000);
        }
    }
}
