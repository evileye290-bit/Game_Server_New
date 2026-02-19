using ServerModels;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    /// <summary>
    /// 某个Hero所有装备达到某品质
    /// model param1 quality
    /// </summary>
    public class GotQualityEquipmentAction : BaseAction
    {
        public GotQualityEquipmentAction(ActionManager manager, ActionModel model, ActionInfo actionInfo)
            : base(manager, model, actionInfo)
        {
        }

        public override bool Check(object obj)
        {
            EquipmentItem equipment = obj as EquipmentItem;
            if (equipment == null) return false;

            if (equipment.Model.Grade >= model.Param1)
            {
                if (!CheckActionBySdk(model.Param1))
                {
                    //所有装备都满足
                    AddNum(model.Param1, true);
                    return true;
                }
            }

            return false;
        }

        public override void SetFinishedBySdk(int current)
        {
            AddNum(model.Param1, true);
        }
    }
}
