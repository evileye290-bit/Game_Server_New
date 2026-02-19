using ServerModels;

namespace ZoneServerLib
{
    /// <summary>
    /// 武魂共鸣每n级触发一次
    /// model param1 阶段步进进度
    /// </summary>
    public class ResonanceAction : BaseAction
    {
        public ResonanceAction(ActionManager manager, ActionModel model, ActionInfo actionInfo)
            : base(manager, model, actionInfo)
        {
        }

        public override bool Check(object obj)
        {
            int level;
            if (!int.TryParse(obj.ToString(), out level))
            {
                return false;
            }

            //基础等级限制
            if (level < model.Param1) return false;

            int stage = level / model.Param2;

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
