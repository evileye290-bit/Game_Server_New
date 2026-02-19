using ServerModels;

namespace ZoneServerLib
{
    /// <summary>
    /// 武魂共鸣角色数每n级触发一次
    /// model param1 阶段步进进度
    /// </summary>
    public class ResonancePosCountAction : BaseAction
    {
        public ResonancePosCountAction(ActionManager manager, ActionModel model, ActionInfo actionInfo)
            : base(manager, model, actionInfo)
        {
        }

        public override bool Check(object obj)
        {
            int count;
            if (!int.TryParse(obj.ToString(), out count))
            {
                return false;
            }

            int stage = count / model.Param1;

            if (stage > CurNum)
            {
                SetNum(stage);
                //走sdk推荐礼包
                if (CheckActionBySdk(stage))
                {
                    return false;
                }
                return true;
            }
            return false;
        }
    }
}
