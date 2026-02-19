using System;
using CommonUtility;
using Logger;
using ServerShared;
using System.Collections.Generic;
using EnumerateUtility;

namespace ZoneServerLib
{
    public partial class IntegralBossDungeonMap : TeamDungeonMap
    {
        public IntegralBossDungeonMap(ZoneServerApi server, int mapId, int channel) : base(server, mapId, channel)
        {
        }

        protected override void Failed()
        {
            base.Failed();
            foreach (var kv in PcList)
            {
                //日志
                int pointState = 2;
                if (isQuitDungeon)
                {
                    pointState = 3;
                }
                kv.Value.BIRecordCheckPointLog((MapType)DungeonModel.Type, DungeonModel.Id.ToString(), pointState, GetFinishTime());

                //komoelog
                kv.Value.KomoeLogRecordPveFight(1, 1, DungeonModel.Id.ToString(), null, pointState, GetFinishTime());
            }
        }

        protected override void Success()
        {
            DoReward();
            OnTeamDungeonFinished();
            PlayerChar player = null;
            foreach (var kv in PcList)
            {
                try
                {
                    player = kv.Value;
                    RewardManager mng = GetFinalReward(player.Uid);
                    mng.BreakupRewards();
                    player.IntegralBossReward(mng, DungeonModel.Id);

                    //副本类型任务计数
                    PlayerAddTaskNum(player);

                    //增加伙伴经验
                    player.AddHeroExp(DungeonModel.HeroExp);

                    //日志
                    player.BIRecordCheckPointLog((MapType)DungeonModel.Type, DungeonModel.Id.ToString(), 1, GetFinishTime());

                    //komoelog
                    player.KomoeLogRecordPveFight(1, 1, DungeonModel.Id.ToString(), mng.RewardList, 1, GetFinishTime());                 
                }
                catch (Exception ex)
                {
                    Log.Alert(ex);
                }
            }

            ResetReward();
        }
    }
}
