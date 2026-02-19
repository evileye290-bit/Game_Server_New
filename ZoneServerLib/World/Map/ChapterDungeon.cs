using CommonUtility;
using ServerShared;
using System.Linq;

namespace ZoneServerLib
{
    class ChapterDungeon : DungeonMap
    {
        public ChapterDungeon(ZoneServerApi server, int mapId, int channel) : base(server, mapId, channel)
        {
        }

        protected override void Failed()
        {
            base.Failed();
            PlayerChar player = PcList.Values.FirstOrDefault();
            if (player != null)
            {
                player.DeleteSpaceTimePower(DungeonModel.Power);
                int pointState = 2;
                if (isQuitDungeon)
                {
                    pointState = 3;
                }
                player.BIRecordCheckPointLog((MapType)DungeonModel.Type, DungeonModel.Id.ToString(), pointState, GetFinishTime());
            }
        }

        protected override void Success()
        {
            PlayerChar player = PcList.Values.FirstOrDefault();
            if (player != null)
            {
                RewardManager mng = GetFinalReward(player.Uid);
                player.ChapterReward(mng, DungeonModel);

                //副本类型任务计数
                PlayerAddTaskNum(player);

                //增加伙伴经验
                player.AddHeroExp(DungeonModel.HeroExp);

                player.BIRecordCheckPointLog((MapType)DungeonModel.Type, DungeonModel.Id.ToString(), 1, GetFinishTime());
            }

            ResetReward();
        }

    }
}
