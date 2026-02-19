using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class PettyGiftItem : BaseGiftItem
    {
        public int CurFlag { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime RefreshTime { get; set; }
    }
}
