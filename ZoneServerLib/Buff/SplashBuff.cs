using CommonUtility;
using ServerModels;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public class SplashBuff : BaseBuff
    {
        public SplashBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        public override void SpecLogic(object param)
        {
            DamageTriMsg msg = param as DamageTriMsg;
            if(msg == null)
            {
                return;
            }
            
            int damage = (int)(msg.Damage * c * 0.0001f);
            // 周围队友跟着躺枪
            List<FieldObject> targetList = new List<FieldObject>();
            owner.GetEnemyInSplash(owner, SplashType.Circle, msg.Target.Position, new Vec2(), r, 0, 0, targetList, 999, owner.InstanceId);
            foreach (var target in targetList)
            {
                target.DoSpecDamage(msg.Caster, DamageType.Splash, damage);
            }
        }
    }
}
