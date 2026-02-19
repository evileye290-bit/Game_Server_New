using CommonUtility;
using ServerModels;
using ServerShared;
using System;

namespace ZoneServerLib
{
    /// <summary>
    /// 超过param1天没有登陆
    /// model param1 days
    /// </summary>
    public class BackMoreThanDayesAction : BaseAction
    {
        public BackMoreThanDayesAction(ActionManager manager, ActionModel model, ActionInfo actionInfo)
            : base(manager, model, actionInfo)
        {
        }

        public override bool Check(object obj)
        {
            DateTime lastLoginTime = (DateTime)obj;

            if (lastLoginTime == null) return false;

            DateTime now = Owner.server.Now();

            DateTime time = Timestamp.TimeStampToDateTime(this.actionInfo.Time);
            if ((now - time).TotalDays < 2) return false;

            if ((now - lastLoginTime).TotalDays >= model.Param1)
            {
                AddNum(1);

                //走sdk推荐礼包
                if (CheckActionBySdk(model.Param1))
                {
                    return false;
                }

                return true;
            }
            return false;
        }
    }
}
