using CommonUtility;

namespace ZoneServerLib
{
    public class BuffEndTriMsg
    {
        public readonly int BuffId;
        public readonly BuffEndReason Reason;
        public BuffEndTriMsg(int buffId, BuffEndReason reason)
        {
            BuffId = buffId;
            Reason = reason;
        }
    }
}
