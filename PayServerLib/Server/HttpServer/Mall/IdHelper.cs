using System;
using System.Threading;

namespace PayServerLib
{
    public class IdHelper
    {
        public static int Id;

        public static int GenerateId()
        {
            Interlocked.CompareExchange(ref Id, 0, Int32.MaxValue);
            return Interlocked.Increment(ref Id);
        }
    }
}