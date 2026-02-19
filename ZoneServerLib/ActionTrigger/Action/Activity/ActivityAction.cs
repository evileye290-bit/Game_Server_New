using ServerModels;

namespace ZoneServerLib
{
    public abstract class UserActivityAction : BaseAction
    {
        public UserActivityAction(ActionManager manager, ActionModel model, ActionInfo actionInfo)
            : base(manager, model, actionInfo)
        {
        }

        public override bool Check(object obj)
        {
            return false;
        }

        protected override void InitActionInfo()
        {
        }

        public override string BuildActionInfo()
        {
            return string.Empty;
        }
    }
}
