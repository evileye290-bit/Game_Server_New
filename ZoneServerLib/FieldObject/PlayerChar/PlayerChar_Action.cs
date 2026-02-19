using EnumerateUtility;
using ServerModels;
using System;
using System.Collections.Generic;
using Message.Manager.Protocol.MZ;

namespace ZoneServerLib
{
    partial class PlayerChar
    {
        public ActionManager ActionManager { get; private set; }

        public void InitActionAanager()
        {
            ActionManager = new ActionManager(this);
        }

        public void InitActionInfo(List<ActionInfo> actionInfos, List<TimingGiftInfo> timingGiftInfos)
        {
            ActionManager.BindActionInfo(actionInfos, timingGiftInfos);
        }

        public void RecordAction(ActionType type, Object param)
        {
            ActionManager.RecordActionAndCheck(type, param);
        }

        public void RefreshAction()
        {
            ActionManager.Refresh();
        }


        public void ReturnSdkGift(MSG_MZ_GET_SDK_GIFT msg)
        {
            ActionManager.InvokeBySdk(msg.ActionId, msg.GiftId, msg.Param, msg.SdkActionType, msg.DataBox);
        }
    }
}
