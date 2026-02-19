using CommonUtility;
using ServerShared;
using System.Linq;

namespace ZoneServerLib
{
    public class GodPathDungeon : DungeonMap
    {

        public GodPathDungeon(ZoneServerApi server, int mapId, int channel, int godHeroCount) : base(server, mapId, channel)
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

            int pointState = 2;
            if (isQuitDungeon)
            {
                pointState = 3;
            }
            //日志
            PcList.Values.FirstOrDefault()?.BIRecordCheckPointLog((MapType)DungeonModel.Type, DungeonModel.Id.ToString(), pointState, GetFinishTime());
            //komoelog
            PcList.Values.FirstOrDefault()?.KomoeLogRecordPveFight(8, 1, DungeonModel.Id.ToString(), null, pointState, GetFinishTime());
        }

        protected override void Success()
        {
            DoReward();
            PlayerChar player = PcList.Values.FirstOrDefault();
            if (player == null) return;

            RewardManager mng = GetFinalReward(player.Uid);
            mng.BreakupRewards();
            player.GodPathReward(mng, DungeonModel.Id);

            //副本类型任务计数
            PlayerAddTaskNum(player);

            //增加伙伴经验
            player.AddHeroExp(DungeonModel.HeroExp);

            //日志
            player.BIRecordCheckPointLog((MapType)DungeonModel.Type, DungeonModel.Id.ToString(), 1, GetFinishTime());

            //komoelog
            player.KomoeLogRecordPveFight(8, 1, DungeonModel.Id.ToString(), mng.RewardList, 1, GetFinishTime());

            ResetReward();
        }
    }
}
