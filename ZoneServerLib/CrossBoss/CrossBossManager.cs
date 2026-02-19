using EnumerateUtility;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class CrossBossManager
    {
        public CrossBossCounterInfo CounterInfo = new CrossBossCounterInfo();
        private PlayerChar owner { get; set; }

        public CrossBossManager(PlayerChar owner)
        {
            this.owner = owner;
        }

       
        public void Init(CrossBossCounterInfo counterInfo)
        {
            CounterInfo = counterInfo;
        }

        public void SetPassRewardState(int state)
        {
            CounterInfo.PassReward = state;
        }

        public void SetScoreState(int state)
        {
            CounterInfo.Score = state;
        }
    }
}
