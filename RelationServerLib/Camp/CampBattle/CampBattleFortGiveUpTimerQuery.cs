using CommonUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RelationServerLib
{
    public class CampBattleFortGiveUpTimerQuery : TaskTimerQuery
    {
        public Dictionary<int, CampFort> Forts;
        public int Uid;
        public CampBattleFortGiveUpTimerQuery(double interval,int uid,Dictionary<int,CampFort> forts)
        {
            SetInterval(interval*1000);
            this.Uid = uid;
            Forts = forts;
        }
    }
}
