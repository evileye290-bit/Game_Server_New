using Message.Gate.Protocol.GateC;

namespace ZoneServerLib
{
    partial class PlayerChar
    {
        public void CheckCacheRewardMsg(MSG_ZGC_DUNGEON_REWARD msg)
        {
            if (CurDungeon?.HadSpeedUp == true)
            {
                CurDungeon.CachePlayerMessage(msg, this);
            }
            else
            {
                Write(msg);
            }
        }
    }
}
