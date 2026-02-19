using CommonUtility;
using ServerShared;
using System.Linq;

namespace ZoneServerLib
{
    public class PushFigureDungeon : DungeonMap
    {
        public PushFigureDungeon(ZoneServerApi server, int mapId, int channel) : base(server, mapId, channel)
        {
        }

        protected override void Success()
        {
            DoReward();
            PlayerChar player = PcList.Values.FirstOrDefault();
            if (player != null)
            {
                RewardManager mng = GetFinalReward(player.Uid);

                //爬塔
                player?.PushFigureSuccess(mng, DungeonModel.Id);

                //副本类型任务计数
                PlayerAddTaskNum(player);

                //增加伙伴经验
                player.AddHeroExp(DungeonModel.HeroExp);

                //日志
                player.BIRecordCheckPointLog((MapType)DungeonModel.Type, DungeonModel.Id.ToString(), 1, GetFinishTime());

                if (DungeonModel.Contribution > 0)
                {
                    //增加贡献值
                    player.SerndUpdateRankValue(EnumerateUtility.RankType.Contribution, DungeonModel.Contribution);

                    //日志
                    player.BIRecordContributionLog(DungeonModel.Contribution, player.server.ContributionMng.PhaseNum, player.server.ContributionMng.CurrentValue);
                }
            }

            ResetReward();
        }

        protected override void Failed()
        {
            base.Failed();
            //日志
            int pointState = 2;
            if (isQuitDungeon)
            {
                pointState = 3;
            }
            PcList.Values.FirstOrDefault()?.BIRecordCheckPointLog((MapType)DungeonModel.Type, DungeonModel.Id.ToString(), pointState, GetFinishTime());
        }

        public override void Stop(DungeonResult result)
        {
            SetSpeedUp(false);
           
            base.Stop(result);

            PlayerChar player = PcList.Values.FirstOrDefault();
            if (player != null)
            {
                NotifySpeedUpEnd(player);
            }
        }
    }
}
