namespace ZoneServerLib
{
    partial class FieldObject
    {
        public bool IsNeedCacheMessage()
        {
            return CurDungeon != null && CurDungeon.IsNeedCacheMessage() && CurDungeon.SpeedUpFsmIndex > 0;
        }
    }
}
