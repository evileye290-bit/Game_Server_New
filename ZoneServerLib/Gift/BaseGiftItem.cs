using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class BaseGiftItem
    {
        public ulong Uid { get; set; }
        public int Id { get; set; }
        public int Type { get; set; }
        public int BuyState { get; set; }
    }
}
