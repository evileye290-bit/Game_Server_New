using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    //免疫致命伤害，锁顶血量值1
    public class IgnoreAttackLockHPSomeTimeBuff : BaseBuff
    {
        //当次免疫剩余时间
        private float leftTime = 0f;
        //免疫开启倒计时
        private bool onceStart = false;
        
        public IgnoreAttackLockHPSomeTimeBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void Update(float dt)
        {
            //还没有触发死亡免疫
            if(!onceStart) return;

            leftTime -= dt;

            if (leftTime <= 0)
            {
                //死亡次数减少一次
                AddPileNum(-1);
                onceStart = false;

                if (pileNum <= 0)
                {
                    OnEnd();
                }
            }
        }

        public bool IgnoreDamageStart()
        {
            //在当次免疫死亡时间内
            if (onceStart && leftTime > 0) return true;
            
            if(isEnd) return false;
            
            //没有免死次数了
            if(pileNum<=0) return false;
            
            onceStart = true;
            leftTime = c;

            return true;
        }

        protected override void SendBuffEndMsg()
        {
            if (owner.SubcribedMessage(TriggerMessageType.BuffEnd))
            {
                BuffEndTriMsg msg = new BuffEndTriMsg(Id, LeftTime <= 0 ? BuffEndReason.Time : BuffEndReason.Damage);
                owner.DispatchMessage(TriggerMessageType.BuffEnd, msg);
            }
        }
    }
}
