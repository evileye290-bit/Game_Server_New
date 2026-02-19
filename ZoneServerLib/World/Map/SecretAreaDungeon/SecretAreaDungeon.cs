using CommonUtility;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using ServerFrame;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    class SecretAreaDungeon : DungeonMap
    {
        private DateTime timeout = BaseApi.now;
        public DateTime TimeOut => timeout;
        private SecretAreaModel SecretAreaModel { get; }

        public SecretAreaDungeon(ZoneServerApi server, int mapId, int channel) : base(server, mapId, channel)
        {
            SecretAreaModel model = SecretAreaLibrary.GetModelByDungeonId(mapId);
            if (model == null)
            {
                Log.Warn($"create secretareadungeon error, have not find model id {mapId}");
                return;
            }

            this.SecretAreaModel = model;
        }

        public override void OnPlayerMapLoadingDone(PlayerChar player)
        {
            base.OnPlayerMapLoadingDone(player);
            NotifyBattleStage(player);
        }

        protected override void Start()
        {
            timeout.AddMinutes(SecretAreaModel.TimeOut);
            base.Start();
        }

        public override void Stop(DungeonResult result)
        {
            SetSpeedUp(false);

            base.Stop(result);
        }

        protected override void Failed()
        {
            base.Failed();
            PlayerChar player = PcList.Values.FirstOrDefault();
            if (player != null)
            {
                NotifySpeedUpEnd(player);
                int pointState = 2;
                if (isQuitDungeon)
                {
                    pointState = 3;
                }
                player.BIRecordCheckPointLog((MapType)DungeonModel.Type, DungeonModel.Id.ToString(), pointState, GetFinishTime());

                //komoelog
                player.KomoeLogRecordPveFight(0, 1, DungeonModel.Id.ToString(), null, pointState, GetFinishTime());
            }
        }

        protected override void Success()
        {
            int finishTime = (int)(CurrTime - StartTime).TotalSeconds;

            PlayerChar player = PcList.Values.FirstOrDefault();

            if (player != null) 
            {
                NotifySpeedUpEnd(player);

                RewardManager mng = GetFinalReward(player.Uid);
                player.SecretAreaReward(mng, DungeonModel, SecretAreaModel, finishTime);

                //副本类型任务计数
                PlayerAddTaskNum(player);

                //增加伙伴经验
                player.AddHeroExp(DungeonModel.HeroExp);

                //日志
                player.BIRecordCheckPointLog((MapType)DungeonModel.Type, DungeonModel.Id.ToString(), 1, GetFinishTime());              
            }

            ResetReward();
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
            msg.AllStage = SecretAreaModel.Statge;
            player.Write(msg);
        }
    }
}
