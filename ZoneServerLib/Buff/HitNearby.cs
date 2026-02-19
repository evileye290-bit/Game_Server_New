using CommonUtility;
using ServerModels;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public class HitNearby : BaseBuff
    {
        public HitNearby(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void Update(float dt)
        {
            elapsedTime += dt;
            if (elapsedTime < deltaTime)
            {
                return;
            }

            elapsedTime = 0;
            List<FieldObject> targetList = new List<FieldObject>();
            owner.GetEnemyInSplash(owner, SplashType.Circle, owner.Position, new Vec2(), r, 0, 0, targetList, 999);
            foreach (var target in targetList)
            {
                // 与策划确认 只有BurnBuff才算 DamageType.Burn类型伤害，此处伤害算为Skill伤害
                target.DoSpecDamage(caster, DamageType.Skill, m);
            }
        }
    }
}
