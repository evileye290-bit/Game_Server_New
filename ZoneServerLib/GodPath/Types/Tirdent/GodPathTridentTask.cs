using DBUtility;
using EnumerateUtility;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using ServerModels;
using ServerShared;
using System;

namespace ZoneServerLib
{
    public class GodPathTridentTask : BaseGodPathTask
    {
        /// <summary>
        /// 购买次数
        /// </summary>
        public int BuyCount { get; set; }
        /// <summary>
        /// 可用次数
        /// </summary>
        public int UseCount { get; set; }
        /// <summary>
        /// 当前值
        /// </summary>
        public int CurrentValue { get; set; }
        /// <summary>
        /// 当前值
        /// </summary>
        public int State { get; set; }
        


        public bool UseStart { get; set; }
        private DateTime time { get; set; }

        public GodPathTridentTask(GodPathHero goldPathHero, GodPathTaskModel model, GodPathDBInfo info) : base(goldPathHero, model)
        {
            BuyCount = info.TridentBuyCount;
            UseCount = info.TridentUseCount;
            CurrentValue = info.TridentCurrentValue;
            State = info.TridentState;
        }

        public void CheckUse()
        {
            if (UseStart)
            {
                //之前开启过，没有结算，直接算失败 增加一次失败的概率
                SetResult(GodPathLibrary.TridentFailAdd);
            }
        }

        public void Use(bool isStrategy)
        {
            if (UseStart)
            {
                //之前开启过，没有结算，直接算失败 增加一次失败的概率
                SetCurrentValue(GodPathLibrary.TridentFailAdd);
            }

            UseStart = true;
            time = ZoneServerApi.now;
            UseCount--;

            SyncDBTridentInfo();
        }

        public void AddCount(int count)
        {
            BuyCount += count;
            UseCount += count;
            SyncDBTridentInfo();
        }

        public void SetResult(int value)
        {
            SetCurrentValue(value);

            UseStart = false;
            time = ZoneServerApi.now;

            SyncDBTridentInfo();
        }

        public bool CheckUstTime()
        {
            double passTime = (ZoneServerApi.now - time).TotalSeconds;
            if (passTime < GodPathLibrary.TridentCheckTime)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void SetCurrentValue(int value)
        {
            CurrentValue = Math.Min(CurrentValue + value, GodPathLibrary.TridentMaxValue);
        }

        public void ChangeState()
        {
            State = (int)GodPathTridentType.Push;
            SyncDBTridentInfo();
        }

        public bool CanAddValue()
        {
            if (CurrentValue >= GodPathLibrary.TridentMaxValue)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public override bool Check(HeroInfo hero)
        {
            return State == (int)GodPathTridentType.Push;
        }

        public override void GenerateDBInfo(GodPathDBInfo info)
        {
            info.TridentBuyCount = BuyCount;
            info.TridentUseCount = UseCount;
            info.TridentCurrentValue = CurrentValue;
            info.TridentState = State;
        }

        public override void GenerateMsg(MSG_GOD_HERO_INFO msg)
        {
            msg.TridentBuyCount = BuyCount;
            msg.TridentUseCount = UseCount;
            msg.TridentCurrentValue = CurrentValue;
            msg.TridentState = State;
        }

        public override void GenerateTransformInfo(ZMZ_GOD_HERO_INFO msg)
        {
            msg.TridentBuyCount = BuyCount;
            msg.TridentUseCount = UseCount;
            msg.TridentCurrentValue = CurrentValue;
            msg.TridentState = State;
        }

        public override void Init()
        {
            BuyCount = 0;
            UseCount = GodPathLibrary.TridentUseCount;
            CurrentValue = 0;
            State = (int)GodPathTridentType.Open;
        }

        public override void Reset()
        {
            BuyCount = 0;
            UseCount = 0;
            CurrentValue = 0;
        }

        public override void DailyReset()
        {
            BuyCount = 0;
            UseCount = GodPathLibrary.TridentUseCount;
        }

        private void SyncDBTridentInfo()
        {
            QueryUpdateGodHeroTrident query = new QueryUpdateGodHeroTrident(GodPathHero.Manager.Owner.Uid,
                GodPathHero.HeroId, BuyCount, UseCount, CurrentValue, State);
            GodPathHero.Manager.Owner.server.GameDBPool.Call(query);
        }
    }
}
