using CommonUtility;
using EnumerateUtility;
using ServerModels;
using System;
using Message.Zone.Protocol.ZM;

namespace ZoneServerLib
{
    public class BaseAction
    {
        private PlayerChar owner;
        protected ActionManager actionManager;

        protected ActionModel model;
        protected ActionInfo actionInfo;

        public PlayerChar Owner => owner;
        public ActionInfo ActionInfo => actionInfo;
        public ActionModel Model => model;
        public int Id => model.Id;
        public int State => actionInfo.State;
        public int CurNum => actionInfo.Num;
        public ActionFrequence ActionFrequence => model.ActionFrequence;

        public BaseAction(ActionManager manager, ActionModel model, ActionInfo actionInfo)
        {
            this.model = model;
            this.actionInfo = actionInfo;
            this.actionManager = manager;
            this.owner = manager.Owner;

            InitActionInfo();
        }
        protected virtual ActionInfo GetActionInfo()
        {
            actionInfo.Infos = BuildActionInfo();
            return actionInfo;
        }

        protected virtual void InitActionInfo()
        {
        }

        public virtual string BuildActionInfo()
        {
            return String.Empty;
        }

        public virtual bool Check(Object obj)
        {
            return false;
        }

        public virtual void Refresh()
        {
            CheckAndReset();
        }

        protected virtual void Reset()
        {
            actionInfo.State = 0;
            actionInfo.Num = 0;
            actionInfo.Infos = string.Empty;
            actionInfo.Time = Timestamp.GetUnixTimeStampSeconds(owner.server.Now());
            actionManager.SyncUpdateAction(GetActionInfo());
        }

        protected bool CheckAndReset()
        {
            if (CheckOutOfTime())
            {
                Reset();
                return true;
            }
            return false;
        }

        protected bool CheckOutOfTime()
        {
            DateTime now = owner.server.Now();
            DateTime lastTime = Timestamp.TimeStampToDateTime(actionInfo.Time);
            int days = DateTime.DaysInMonth(lastTime.Year, lastTime.Month);

            bool outofDate = false;
            switch (model.ActionFrequence)
            {
                case ActionFrequence.Daily: 
                    outofDate = now > lastTime.AddDays(1).Date; 
                    break;
                case ActionFrequence.Weekly: 
                    outofDate = now > lastTime.AddDays(7).Date;
                    break;
                case ActionFrequence.Monthly: 
                    outofDate = now > lastTime.AddDays(days).Date;
                    break;
                case ActionFrequence.Time: 
                    outofDate = now > model.EndTime;
                    break;
            }

            return outofDate;
        }

        protected void AddNum(int num, bool finished = false)
        {
            actionInfo.Num += num;
            actionInfo.Time = Timestamp.GetUnixTimeStampSeconds(owner.server.Now());

            SetFinished(finished);

            actionManager.SyncUpdateAction(GetActionInfo());
        }

        protected void SetNum(int num, bool finished = false)
        {
            actionInfo.Num = num;
            actionInfo.Time = Timestamp.GetUnixTimeStampSeconds(owner.server.Now());

            SetFinished(finished);

            actionManager.SyncUpdateAction(GetActionInfo());
        }

        protected void SetFinished(bool handled)
        {
            if (!handled) return;

            actionInfo.State = 1;

            //只触发一次的完成后，下次就无须再加载了
            if (model.ActionFrequence == ActionFrequence.Once || model.ActionFrequence == ActionFrequence.Time)
            {
                actionManager.ActionFinished(this);
            }
        }

        protected bool CheckActionBySdk(int current)
        {
            if (model.SdkAction > 0)
            {
                if (model.SdkActionParam == null || model.SdkActionParam.Contains(current))
                {
                    //向barrack请求礼包
                    MSG_ZM_GET_SDK_GIFT msg = new MSG_ZM_GET_SDK_GIFT()
                    {
                        Uid = owner.Uid,
                        ActionId = model.Id,
                        SdkActionType = model.SdkAction,
                        Param = current,
                        Account = owner.AccountName
                    };
                    owner.server.ManagerServer.Write(msg);
                    return true;
                }
            }

            return false;
        }

        public virtual void SetFinishedBySdk(int current)
        {
        }
    }
}
