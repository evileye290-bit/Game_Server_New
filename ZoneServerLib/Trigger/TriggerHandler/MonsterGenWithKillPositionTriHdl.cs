using CommonUtility;
using EnumerateUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class MonsterGenWithKillPositionTriHdl : BaseTriHdl
    {
        private readonly int genId;
        public MonsterGenWithKillPositionTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            :base(trigger, handlerType, handlerParam)
        {
            int.TryParse(handlerParam, out genId);
        }

        public override void Handle()
        {
            object param;
            if (!trigger.TryGetParam(TriggerParamKey.KillEnemy, out param))
            {
                return;
            }

            KillEnemyTriMsg msg = param as KillEnemyTriMsg;
            if (msg == null)
            {
                return;
            }
            if (trigger.CurMap == null)
            {
                return;
            }
            FieldObject target = CurMap.GetFieldObject(msg.DeadInstanceId);
            BaseMonsterGen gen = trigger.CurMap.GetMonsterGen(genId);
            if (gen != null && gen.GenType == MonsterGenType.Mannual)
            {
                //同一帧不连续触发
                if (ThisFpsHadHandled()) return;
                SetThisFspHandled();
                gen.GenerateMonstersWithPosition(gen.Model.GenCount, target.Position);
            }
        }
    }
}
