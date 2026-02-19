using CommonUtility;
using System.Collections.Generic;

namespace ZoneServerLib
{

    /// <summary>
    /// 对多个目标造成伤害
    /// </summary>
    public class DoDamageTargetListTriMsg
    {
        public Skill Skill { get; private set; }
        public List<FieldObject> TargetList { get; private set; }
        public DoDamageTargetListTriMsg(Skill skill, List<FieldObject> fieldObject)
        {
            Skill = skill;
            TargetList = new List<FieldObject>();
            TargetList.AddRange(fieldObject);
        }
    }
}
