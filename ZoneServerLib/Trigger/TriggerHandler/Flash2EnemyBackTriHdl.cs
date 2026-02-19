using CommonUtility;
using EnumerateUtility;
using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class Flash2EnemyBackTriHdl : BaseTriHdl
    {
        float range =4 ;
        int type = 1;
        public Flash2EnemyBackTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            string[] temps = handlerParam.Split(':');
            range = float.Parse(temps[0]);
            type = int.Parse(temps[1]);
        }

        public override void Handle()
        {
            Log.Debug("hero instanceId {0} Flash2EnemyBackTriHdl done!", Owner.InstanceId);
            if (Owner.IsHero)
            {
                //同一帧不连续触发
                if (ThisFpsHadHandled()) return;
                List<FieldObject> objects = new List<FieldObject>();
                Owner.GetEnemyInSplash(Owner, SplashType.Circle, Owner.Position, new Vec2(0, 0), 50, 0f, 0f, objects, 20, -1, true);
                FieldObject target = null;
                //lowhp hpPercentLow 攻击最高 防御最低
                Owner.FilterTargets(objects, (TargetFilterType)type);


                target = objects.FirstOrDefault();

                if (target != null)
                {
                    
                    Vec2 targetPos = target.Position;
                    float targetRange = target.Radius;
                    float ownerRadius = Owner.Radius;
                    Vec2 temp = Owner.Position;

                    Vec2 pos = (targetPos - temp);
                    
                    pos = pos * (targetRange + ownerRadius+ range) /(float)pos.GetLength() + targetPos;
                    //Owner.Transmit(pos);
                    Owner.FsmManager.SetNextFsmStateType(FsmStateType.HERO_TRANSMIT, false, pos);
                    SetThisFspHandled();
                }
            }
        }
    }
}
