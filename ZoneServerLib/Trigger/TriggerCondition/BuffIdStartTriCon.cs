using CommonUtility;
using Logger;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public class BuffIdStartTriCon : BaseTriCon
    {
        private int buffId;
        private readonly List<int> buffIds=new List<int>();
        public BuffIdStartTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            string[] ids = conditionParam.Split(':');
            foreach (var id in ids)
            {
                if (!int.TryParse(id, out buffId))
                {
                    Log.Warn($"init BuffIdStartTriCon trigger condition failed: invalid buff id {conditionParam}");
                }
                else
                {
                    buffIds.Add(buffId);
                }
            }
        }

        public override bool Check()
        {
            object param = new Object();
            bool ret = false;
            foreach(var id in buffIds)
            {
                if(trigger.TryGetParam(TriggerParamKey.BuildStartBuffIdKey(id), out param))
                {
                    ret = true;
                    break;
                }

            }
            if (!ret)
            {
                return false;
            }
            //if (!trigger.TryGetParam(TriggerParamKey.BuildStartBuffIdKey(buffId), out param))
            //{
            //    return false;
            //}
            BuffStartTriMsg msg = param as BuffStartTriMsg;
            if (msg == null)
            {
                return false;
            }
            bool ans = false;
            foreach(var buffid in buffIds)
            {
                if (msg.BuffId == buffid)
                {
                    ans = true;
                }
            }

            return ans;
        }
    }
}