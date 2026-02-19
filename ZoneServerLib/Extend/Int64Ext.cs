using Message.Gate.Protocol.GateC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public static class Int64Ext
    {
        public static Int64Type ToInt64TypeMsg(this long value)
        {
            Int64Type value64 = new Int64Type();
            value64.High = value.GetHigh();
            value64.Low = value.GetLow();
            return value64;
        }

        public static int GetHigh(this long num)
        {
            return (int)(num >> 31);
        }

        public static int GetLow(this long num)
        {
            return (int)(num & 0x7FFFFFFF);
        }

        public static long GetInt64(int high, int low)
        {
            return ((long)high << 31) | low;
        }

        public static long GetInt64(this Int64Type value)
        {
            return GetInt64(value.High, value.Low);
        }

        public static int ToIntValue(this long value)
        {
            return value <= int.MaxValue ? (int) value : 0;
        }
    }
}
