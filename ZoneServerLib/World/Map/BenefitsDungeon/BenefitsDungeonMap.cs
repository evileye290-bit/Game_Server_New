using CommonUtility;
using EnumerateUtility;
using Message.Gate.Protocol.GateC;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    public partial class BenefitsDungeonMap : DungeonMap
    {
        private bool passedDungeon = true;
        public BenefitsDungeonMap(ZoneServerApi server, int mapId, int channel) : base(server, mapId, channel)
        {
        }

        public override void OnPlayerMapLoadingDone(PlayerChar player)
        {
            base.OnPlayerMapLoadingDone(player);
            NotifyBattleStage(player);
        }

        public override void OnStopBattle(PlayerChar player)
        {
            passedDungeon = false;
            Stop(DungeonResult.Success);
        }

        protected override void Success()
        {
            //1. 通用奖励
            DoReward();

            PlayerChar player = PcList.Values.FirstOrDefault();
            if (player != null)
            {
                RewardManager mng = GetFinalReward(player.Uid);
                player.BenefitReward(mng, DungeonModel, passedDungeon, battleStage);

                //副本类型任务计数
                PlayerAddTaskNum(player);

                //增加伙伴经验
                player.AddHeroExp(DungeonModel.HeroExp);

                //日志
                player.BIRecordCheckPointLog((MapType)DungeonModel.Type, DungeonModel.Id.ToString(), 1, GetFinishTime());

                //komoelog
                int passFlag = 1;
                if (!passedDungeon)
                {
                    passFlag = 2;
                }
                switch ((MapType)DungeonModel.Type)
                {                  
                    case MapType.SoulPower:
                        player.KomoeLogRecordPveFight(4, 1, DungeonModel.Id.ToString(), mng.RewardList, passFlag, GetFinishTime());
                        break;
                    case MapType.SoulBreath:
                        player.KomoeLogRecordPveFight(3, 1, DungeonModel.Id.ToString(), mng.RewardList, passFlag, GetFinishTime());
                        break;                
                    default:
                        break;
                }
            }

            ResetReward();
        }

        protected override void Failed()
        {
            passedDungeon = false;
            Success();
        }

        protected override void OnBattleStageChange()
        {
            foreach (var kv in PcList)
            {
                NotifyBattleStage(kv.Value);
            }
        }

        private void NotifyBattleStage(PlayerChar player)
        {
            MSG_ZGC_DUNGEON_BATTLE_STAGE msg = new MSG_ZGC_DUNGEON_BATTLE_STAGE();
            msg.Stage = battleStage;
            msg.AllStage = 10;
            player.Write(msg);
        }
    }
}
