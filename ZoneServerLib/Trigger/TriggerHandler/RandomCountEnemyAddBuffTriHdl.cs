using CommonUtility;
using Logger;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    public class RandomCountEnemyAddBuffTriHdl : BaseTriHdl
    {
        private readonly int count, buffId;

        public RandomCountEnemyAddBuffTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            List<int> param = handlerParam.ToList(':');
            if (param.Count != 2)
            {
                Log.Warn("init RandomCountEnemyAddBuffTriHdl failed, invalid handler param {0}", handlerParam);
                return;
            }

            count = param[0];
            buffId = param[1];
        }

        public override void Handle()
        {
            if (ThisFpsHadHandled()) return;

            List<FieldObject> targetList = new List<FieldObject>();
            targetList.AddRange(CurMap.HeroList.Values.Where(x => x.IsEnemy(Owner)));
            targetList.AddRange(CurMap.MonsterList.Values.Where(x => x.IsEnemy(Owner)));

            if (targetList.Count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    if(targetList.Count<=0) break;

                    int index = RAND.Range(0, targetList.Count - 1);
                    targetList[index].AddBuff(Owner, buffId, trigger.GetFixedParam_SkillLevel());
                    targetList.RemoveAt(index);
                }
            }

            SetThisFspHandled();
        }
    }
}
