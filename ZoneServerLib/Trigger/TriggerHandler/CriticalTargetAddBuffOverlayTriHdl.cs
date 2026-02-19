using System;
using CommonUtility;
using Logger;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public class CriticalTargetAddBuffOverlayTriHdl : BaseTriHdl
    {
        private List<int> buffList = new List<int>();
        public CriticalTargetAddBuffOverlayTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            string[] paramArr = handlerParam.Split(':');
            if (paramArr.Length == 0)
            {
                Log.Warn("init CriticalTargetAddBuffOverlayTriHdl tri hdl failed, invalid handler param {0}", handlerParam);
                return;
            }

            for(int i = 0; i < paramArr.Length; i++)
            {
                int id = 0;
                if (!int.TryParse(paramArr[i], out id))
                {
                    Log.Warn("init CriticalTargetAddBuffOverlayTriHdl tri hdl failed, invalid handler param {0}", handlerParam);
                    continue;
                }
                buffList.Add(id);
            }
        }

        public override void Handle()
        {
            if (ThisFpsHadHandled()) return;

            object param;
            if (!trigger.TryGetParam(TriggerParamKey.Critical, out param))
            {
                return;
            }
            CriticalTriMsg msg = param as CriticalTriMsg;
            if (msg == null || msg.Target == null || msg.Model == null)
            {
                return;
            }
            Skill skill = Owner.SkillManager.GetSkill(msg.Model.Id);
            if (skill == null)
            {
                return;
            }

            //魂环技能等级
            int skillLevel = trigger.GetFixedParam_SkillLevel();

            int curBuffId = buffList[0];
            int nextBuffId = buffList[0];
            for (int i = 0; i < buffList.Count; i++)
            {
                if (msg.Target.BuffManager.HaveBuff(buffList[i]))
                {
                    curBuffId = buffList[i];
                    nextBuffId = i < buffList.Count - 1 ? buffList[i + 1] : buffList[i];
                    break;
                }
            }

            int skillLevelGrowth = trigger.GetFixedParam_SkillLevelGrowth();

            // 叠加到了最高层
            if (curBuffId == nextBuffId)
            {
                var buff = msg.Target.BuffManager.GetBuff(curBuffId);
                if(buff == null)
                {
                    msg.Target.AddBuff(Owner, curBuffId, skillLevelGrowth);
                }
                else
                {
                    buff.SetBuffDuringTime(buff.S);
                }
            }
            else
            {
                msg.Target.BuffManager.RemoveBuffsById(curBuffId);
                msg.Target.AddBuff(Owner, nextBuffId, skillLevelGrowth);
            }
            SetThisFspHandled();
        }
    }
}

