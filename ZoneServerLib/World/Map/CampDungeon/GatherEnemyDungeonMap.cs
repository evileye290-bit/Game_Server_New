using CommonUtility;
using EnumerateUtility;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    public class GatherEnemyDungeonMap : DungeonMap
    {
        public GatherEnemyDungeonMap(ZoneServerApi server, int mapId, int channel) : base(server, mapId, channel)
        {
            PlayerChar player = PcList.Values.FirstOrDefault();
            if (player == null) return;

            int phaseNum = player.server.RelationServer.CampBattlePhaseInfo.PhaseNum;
            phaseNum = Math.Max(1, phaseNum);

            float growth = CampBattleLibrary.GetDifficultyRatio(phaseNum);
            SetMonsterGrowth(growth);
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

            PlayerChar player = PcList.Values.FirstOrDefault();
            if (player != null)
            {
                player.BIRecordCheckPointLog((MapType)DungeonModel.Type, DungeonModel.Id.ToString(), pointState, GetFinishTime());

                List<Dictionary<string, object>> consume = player.ParseConsumeInfoToList(null, (int)CounterType.ActionCount, 1);
                player.KomoeEventLogCampBattle(((int)player.Camp).ToString(), player.Camp.ToString(), 0, 2, player.HeroMng.CalcBattlePower(), GetFinishTime(), pointState, "", "", "", 1, consume);
            }
        }

        protected override void Success()
        {
            DoReward();

            PlayerChar player = PcList.Values.FirstOrDefault();
            if (player != null)
            {
                RewardManager mng = GetFinalReward(player.Uid);
                player.CampGatherSuccessReward(DungeonModel, mng);

                //副本类型任务计数
                PlayerAddTaskNum(player);

                //增加伙伴经验
                player.AddHeroExp(DungeonModel.HeroExp);

                //日志
                player.BIRecordCheckPointLog((MapType)DungeonModel.Type, DungeonModel.Id.ToString(), 1, GetFinishTime());

                List<Dictionary<string, object>> consume = player.ParseConsumeInfoToList(null, (int)CounterType.ActionCount, 1);
                player.KomoeEventLogCampBattle(((int)player.Camp).ToString(), player.Camp.ToString(), 0, 2, player.HeroMng.CalcBattlePower(), GetFinishTime(), 1, "", "", "", 1, consume);
            }

            ResetReward();
        }
    }
}
