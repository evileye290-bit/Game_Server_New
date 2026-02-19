using CommonUtility;
using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class TriggerTriggeredMsgTriHdl : BaseTriHdl
    {
        //private readonly float range;
        private readonly int toTrigger = 0;
        public TriggerTriggeredMsgTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            //string[] paramArr = handlerParam.Split(':');
            //if (paramArr.Length != 2)
            //{
            //    Log.Warn("init range enemy add buff tri hdl failed, invalid handler param {0}", handlerParam);
            //    return;
            //}

            //if (!float.TryParse(paramArr[0], out range) || !int.TryParse(paramArr[1], out buffId))
            //{
            //    Log.Warn("init range enemy add buff tri hdl failed, invalid handler param {0}", handlerParam);
            //    return;
            //}
            if (!int.TryParse(handlerParam, out toTrigger))
            {
                Log.Warn($"init triggerTriggeredMsg with {handlerParam} try parse to int error");
                return;
            }
        }

        public override void Handle()
        {
            //同一帧不连续触发
            if (ThisFpsHadHandled()) return;
            SetThisFspHandled();

            //int skillLevel = trigger.GetFixedParam_SkillLevel();
            //Vec2 center = Owner.Position;
            //List<FieldObject> targetList = new List<FieldObject>();
            //Owner.GetEnemyInSplash(Owner, SplashType.Circle, center, new Vec2(0, 0), range, 0, 0, targetList, 999);
            //foreach (var target in targetList)
            //{
            //    target.AddBuff(Owner, buffId, skillLevel);
            //}
            Owner.DispatchMessage(TriggerMessageType.TriggerTriggerd, Tuple.Create(toTrigger, Owner));
        }
    }
}
