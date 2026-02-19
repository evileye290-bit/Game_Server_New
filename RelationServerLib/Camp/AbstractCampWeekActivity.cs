using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnumerateUtility;
using Logger;
using ServerShared;

namespace RelationServerLib
{
    public abstract class AbstractCampWeekActivity
    {
        public DateTime NowShowBeginTime { get { return nowShowBegin; } }
        public DateTime NowShowEndTime { get { return nowShowEnd; } }

        private int phaseNum;
        private DateTime begin;
        private DateTime end;

        protected int nowShowPhaseNum;
        protected DateTime nowShowBegin;
        protected DateTime nowShowEnd;

        private int nextPhaseNum = 1;

        public DateTime NextBegin
        {
            get { return nextBegin; }
        }

        private DateTime nextBegin;
        private DateTime nextEnd;

        protected CampActivityType type;
        protected RelationServerApi server;
        protected CampActivityTimerData timerData;

        public AbstractCampWeekActivity(CampActivityType type, RelationServerApi server)
        {
            this.type = type;
            this.server = server;
        }

        public virtual void Init(int phase)
        {
            this.nowShowPhaseNum = phase;
            Tuple<DateTime, DateTime> periodNow;
            if (phase>0)
            {
                periodNow = CampActivityLibrary.CalcWeekPhaseByPhaseNum(type, timerData, RelationServerApi.now, server.OpenServerDate, this.nowShowPhaseNum);
            }
            else
            {
                periodNow = CampActivityLibrary.CalcWeekPhase(type, timerData, RelationServerApi.now, server.OpenServerDate, ref this.nowShowPhaseNum);
            }

            nowShowBegin = periodNow.Item1;
            nowShowEnd = periodNow.Item2;

            phaseNum = nowShowPhaseNum;
            begin = nowShowBegin;
            end = nowShowEnd;

            CalcNextPhaseInfo();

            if (phaseNum > 0)
            {
                if (nowShowBegin < RelationServerApi.now)
                {
                    begin = nextBegin;
                }
                else
                {
                    begin = nowShowBegin;
                }

                if (nowShowEnd < RelationServerApi.now)
                {
                    end = nextEnd;
                }
                else
                {
                    end = nowShowEnd;
                }
            }


            //Log.Info("camp activity phase {0} type {1} init begin {2} end {3}", nowShowPhaseNum, type.ToString(),nowShowBegin.ToString(),nowShowEnd.ToString());
        }


        public void LoadXMLData(CampActivityTimerData timerData)
        {
            this.timerData = timerData;
        }

        public virtual void Update(double dt)
        {
            if (CheckEnd())
            {
                PhaseEnd();
            }
            if (CheckBegin())
            {
                PhaseBegin();
            }
        }

        /// <summary>
        /// 周期开始
        /// </summary>
        private void PhaseBegin()
        {
            nowShowBegin = begin;
            nowShowEnd = end;
            nowShowPhaseNum = phaseNum;
            if (nowShowPhaseNum > 0)
            {
                DoBeginBusiness();
            }
            CalcNextPhaseInfo();
            begin = nextBegin;
            phaseNum = nextPhaseNum;
            if (nowShowPhaseNum > 0)
            {
                SyncCampActivityPhaseInfo2Zone();
            }
            Log.Info("Camp activity phase {0} type {1} begin from {2} to {3} ", nowShowPhaseNum, type.ToString(), nowShowBegin.ToString(), nowShowEnd.ToString());
        }

        /// <summary>
        /// 周期结束
        /// </summary>
        private void PhaseEnd()
        {
            if (nowShowPhaseNum > 0)
            {
                DoEndBusiness();
                Log.Info("Camp activity phase {0} type {1} end!", nowShowPhaseNum, type.ToString(), nowShowEnd.ToString());
            }
            end = nextEnd;
        }

        protected abstract void DoBeginBusiness();
        protected abstract void DoEndBusiness();

        /// <summary>
        /// 计算新周期信息
        /// </summary>
        private void CalcNextPhaseInfo()
        {
            if (nowShowPhaseNum == 0)
            {
                Tuple<DateTime, DateTime> nextPeriod = CampActivityLibrary.CalcWeekPhase(type, timerData, RelationServerApi.now, server.OpenServerDate, ref nextPhaseNum);
                nextBegin = nextPeriod.Item1;
                nextEnd = nextPeriod.Item2;
            }
            else
            {
                nextPhaseNum = nowShowPhaseNum + 1;
                Tuple<DateTime, DateTime> nextPeriod = CampActivityLibrary.CalcWeekPhaseByPhaseNum(type, timerData, RelationServerApi.now, server.OpenServerDate, nextPhaseNum);
                nextBegin = nextPeriod.Item1;
                nextEnd = nextPeriod.Item2;
            }
        }

        /// <summary>
        /// 当前不在周期内
        /// </summary>
        /// <param name="now"></param>
        /// <returns></returns>
        public bool CheckWrongTime(DateTime now)
        {
            if ((now < nowShowBegin || now > nowShowEnd))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 周期开始
        /// </summary>
        /// <returns></returns>
        private bool CheckBegin()
        {
            if (begin < RelationServerApi.now)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 周期结束
        /// </summary>
        /// <returns></returns>
        private bool CheckEnd()
        {
            if (end < RelationServerApi.now )
            {
                return true;
            }
            return false;
        }


        public virtual void SyncCampActivityPhaseInfo2Zone()
        {
            foreach (var item in server.ZoneManager.ServerList)
            {
                ((ZoneServer)item.Value).SyncCampActivityPhaseInfo(nowShowPhaseNum, type, nowShowBegin, nowShowEnd);
            }
            Log.Info($"sync camp activity phase info to zone : phase {nowShowPhaseNum} type {type} begin {nowShowBegin} end {nowShowEnd}");
        }

    }
}
