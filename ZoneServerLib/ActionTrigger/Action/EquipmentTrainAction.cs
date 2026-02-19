using ServerModels;
using ServerShared;

namespace ZoneServerLib
{
    /// <summary>
    /// 装备强化达到某阶段
    /// model param1 阶段
    /// </summary>
    public class EquipmentTrainAction : BaseAction
    {
        public EquipmentTrainAction(ActionManager manager, ActionModel model, ActionInfo actionInfo)
            : base(manager, model, actionInfo)
        {
        }

        public override bool Check(object obj)
        {
            int level;
            int.TryParse(obj.ToString(), out level);

            if (level >= model.Param1)
            {
                //走sdk推荐礼包
                if (CheckActionBySdk(model.Param1))
                {
                    return false;
                }
                SetNum(level, true);
                return true;
            }
            return false;
        }

        public override void SetFinishedBySdk(int current)
        {
            SetNum(model.Param1, true);
        }
    }
}
