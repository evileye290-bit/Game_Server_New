using ServerModels;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public class SkillHitMsg
    {
        public readonly SkillModel Model;
        public readonly List<FieldObject> TargetList;

        public SkillHitMsg(SkillModel model, List<FieldObject> targetList)
        {
            Model = model;
            TargetList = new List<FieldObject>(targetList);
        }
    }
}
