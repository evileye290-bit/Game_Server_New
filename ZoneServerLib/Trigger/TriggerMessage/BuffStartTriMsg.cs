using CommonUtility;

namespace ZoneServerLib
{
    public class BuffStartTriMsg
    {
        public readonly int BuffId, BuffType;
        public readonly BuffEndReason Reason;

        public BuffStartTriMsg(int buffId, int buffType)
        {
            BuffId = buffId;
            BuffType = buffType;
        }
    }
}

