using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class AddTrigger4EnemyTriHdl : BaseTriHdl
    {
        private readonly int triggerId = 0;
        public AddTrigger4EnemyTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            if (!int.TryParse(handlerParam, out triggerId))
            {
                Log.Warn("init add trigger tri hdl failed, invalid handler param {0}", handlerParam);
                return;
            }
        }

        public override void Handle()
        {
            if (ThisFpsHadHandled()) return;
            SetThisFspHandled();

            foreach (var kv in Owner.CurDungeon.MonsterList)
            {
                if (kv.Value.IsEnemy(Owner))
                {
                    kv.Value.AddTriggerCreatedBySkill(triggerId, 1, Owner);
                }
            }

            foreach (var kv in Owner.CurDungeon.HeroList)
            {
                if (kv.Value.IsEnemy(Owner))
                {
                    kv.Value.AddTriggerCreatedBySkill(triggerId, 1, Owner);
                }
            }
        }
    }
}
