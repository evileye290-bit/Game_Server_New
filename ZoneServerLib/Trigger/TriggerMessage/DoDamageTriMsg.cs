using CommonUtility;

namespace ZoneServerLib
{
    //造成伤害
    public class DoDamageTriMsg
    {
        public int SkillId { get; private set; }
        public long Damage { get; private set; }

        public FieldObject FieldObject { get; private set; }
        public DoDamageTriMsg(long damage, int skillId, FieldObject fieldObject)
        {
            SkillId = skillId;
            Damage = damage;
            FieldObject = fieldObject;
        }
    }
}
