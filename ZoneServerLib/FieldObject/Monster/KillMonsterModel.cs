using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class KillMonsterModel
    {
        private bool isFinish = false;

        public bool IsFinish
        {
            get { return isFinish; }
            set { isFinish = value; }
        }

        private int needCount = 0;

        public int NeedCount
        {
            get { return needCount; }
            set { needCount = value; }
        }
    }
}
