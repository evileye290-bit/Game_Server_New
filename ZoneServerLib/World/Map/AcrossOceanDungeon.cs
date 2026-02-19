using CommonUtility;
using Logger;
using ServerShared;
using System;
using System.Linq;

namespace ZoneServerLib
{
    public class AcrossOceanDungeon : DungeonMap
    {

        public AcrossOceanDungeon(ZoneServerApi server, int mapId, int channel, int godHeroCount) : base(server, mapId, channel)
        {
            IniMonsterGensByGodHeroCount(godHeroCount);
        }

        protected override void Failed()
        {
            //手动退出不给前端推送结算面板
            if (!isQuitDungeon)
            {
                base.Failed();
            }
            //日志
            int pointState = 2;
            if (isQuitDungeon)
            {
                pointState = 3;
            }
            PcList.Values.FirstOrDefault()?.BIRecordCheckPointLog((MapType)DungeonModel.Type, DungeonModel.Id.ToString(), pointState, GetFinishTime());
        }

        protected override void Success()
        {
            DoReward();
            PlayerChar player = PcList.Values.FirstOrDefault();
            if (player == null) return;

            RewardManager mng = GetFinalReward(player.Uid);
            mng.BreakupRewards();
            player.GodPathAcrossOceanReward(mng, DungeonModel);

            //副本类型任务计数
            PlayerAddTaskNum(player);

            //增加伙伴经验
            player.AddHeroExp(DungeonModel.HeroExp);

            //日志
            player.BIRecordCheckPointLog((MapType)DungeonModel.Type, DungeonModel.Id.ToString(), 1, GetFinishTime());

            ResetReward();
        }
    }
}
