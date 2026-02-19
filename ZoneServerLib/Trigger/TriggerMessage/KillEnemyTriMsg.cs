using CommonUtility;

namespace ZoneServerLib
{
    public class KillEnemyTriMsg
    {
        public readonly int KillerInstanceId;
        public readonly int SkillId;
        public readonly int DeadInstanceId;
        public bool Critical;
        public readonly DamageType DamageType;
        public readonly object Param;

        public KillEnemyTriMsg(int killerInstanceId, int skillId, int deadInstanceId, bool critical, DamageType type, object param)
        {
            KillerInstanceId = killerInstanceId;
            SkillId = skillId;
            DeadInstanceId = deadInstanceId;
            Critical = critical;
            DamageType = type;
            Param = param;
        }
    }
}
