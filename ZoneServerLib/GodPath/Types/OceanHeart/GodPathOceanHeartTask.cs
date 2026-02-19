using DBUtility;
using EnumerateUtility;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class GodPathOceanHeartTask : BaseGodPathTask
    {
        /// <summary>
        /// 每日点击次数
        /// </summary>
        public int HeartDailyCount { get; set; }
        /// <summary>
        /// 购买次数
        /// </summary>
        public int HeartBuyCount { get; set; }
        /// <summary>
        /// 可用次数
        /// </summary>
        public int HeartUseCount { get; set; }
        /// <summary>
        /// 当前值
        /// </summary>
        public int HeartCurrentValue { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public int HeartState { get; set; }
        /// <summary>
        /// 翻牌奖励
        /// </summary>
        public string HeartRewards = string.Empty;

        public GodPathOceanHeartTask(GodPathHero goldPathHero, GodPathTaskModel model, GodPathDBInfo info) : base(goldPathHero, model)
        {
            HeartDailyCount = info.HeartDailyCount;
            HeartBuyCount = info.HeartBuyCount;
            HeartUseCount = info.HeartUseCount;
            HeartCurrentValue = info.HeartCurrentValue;
            HeartState = info.HeartState;
            HeartRewards = info.HeartRewards;
        }

        public void ChangeState()
        {
            HeartRewards = string.Empty;
            HeartState = (int)GodPathOceanHeartType.Repaint;
            SyncDBOceanHeartInfo();
        }

        public void AddCount(int count)
        {
            HeartBuyCount += count;
            HeartUseCount += count;
            SyncDBOceanHeartInfo();
        }

        public void SetHeartCurrentValue(int value)
        {
            HeartDailyCount++;
            HeartUseCount--;
            HeartCurrentValue = Math.Min(HeartCurrentValue + value, GodPathLibrary.HeartMaxValue);
        }

        public void SetDrawReward()
        {
            HeartRewards = GetNewDrawReward();
            SyncDBOceanHeartInfo();
        }

        private string GetNewDrawReward()
        {
            string newReward = string.Empty;
            GodPathHeartMode newModel = GodPathLibrary.GetPathHeartModel(HeartCurrentValue);
            if (newModel != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    newReward += newModel.GeDrawReward() + "|";
                }
            }
            return newReward;
        }

        public override bool Check(HeroInfo hero)
        {
            return HeartState == (int)GodPathOceanHeartType.Repaint;
        }

        public override void GenerateDBInfo(GodPathDBInfo info)
        {
            info.HeartDailyCount = HeartDailyCount;
            info.HeartBuyCount = HeartBuyCount;
            info.HeartUseCount = HeartUseCount;
            info.HeartCurrentValue = HeartCurrentValue;
            info.HeartState = HeartState;
            info.HeartRewards = HeartRewards;
        }

        public override void GenerateMsg(MSG_GOD_HERO_INFO msg)
        {
            msg.HeartDailyCount = HeartDailyCount;
            msg.HeartBuyCount = HeartBuyCount;
            msg.HeartUseCount = HeartUseCount;
            msg.HeartCurrentValue = HeartCurrentValue;
            msg.HeartState = HeartState;
            msg.HeartRewards = HeartRewards;
        }

        public override void GenerateTransformInfo(ZMZ_GOD_HERO_INFO msg)
        {
            msg.HeartDailyCount = HeartDailyCount;
            msg.HeartBuyCount = HeartBuyCount;
            msg.HeartUseCount = HeartUseCount;
            msg.HeartCurrentValue = HeartCurrentValue;
            msg.HeartState = HeartState;
            msg.HeartRewards = HeartRewards;
        }

        public override void Init()
        {
            HeartDailyCount = 0;
            HeartBuyCount = 0;
            HeartUseCount = GodPathLibrary.HeartUseCount;
            HeartCurrentValue = 0;
            HeartState = (int)GodPathOceanHeartType.Open;
            HeartRewards = GetNewDrawReward();
        }

        public override void Reset()
        {
            HeartDailyCount = 0;
            HeartBuyCount = 0;
            HeartUseCount = 0;
            HeartCurrentValue = 0;
            HeartState = (int)GodPathOceanHeartType.Open;
            HeartRewards = string.Empty;
        }

        public override void DailyReset()
        {
            HeartDailyCount = 0;
            HeartBuyCount = 0;
            HeartUseCount = GodPathLibrary.HeartUseCount;
        }

        private void SyncDBOceanHeartInfo()
        {
            QueryUpdateGodHeroOceanHeart query = new QueryUpdateGodHeroOceanHeart(GodPathHero.Manager.Owner.Uid,
                GodPathHero.HeroId, HeartDailyCount, HeartBuyCount, HeartUseCount, HeartCurrentValue, HeartState, HeartRewards);
            GodPathHero.Manager.Owner.server.GameDBPool.Call(query);
        }
    }
}
