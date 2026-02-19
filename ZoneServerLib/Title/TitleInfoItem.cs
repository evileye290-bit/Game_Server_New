using EnumerateUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class TitleInfoItem
    {
        public int Id { get; set; }
        public TitleState State { get; set; }
        public int FinishCount { get; set; }
        public int Used { get; set; }
        public bool NeedSyncDb { get; set; }
    }
}
