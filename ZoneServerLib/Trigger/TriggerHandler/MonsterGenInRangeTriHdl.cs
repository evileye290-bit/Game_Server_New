using System.Collections.Generic;
using CommonUtility;
using EnumerateUtility;
using Logger;
using ServerModels.Monster;
using ServerShared;

namespace ZoneServerLib
{
    public class MonsterGenInRangeTriHdl : BaseTriHdl
    {
        private readonly int r;
        private readonly int genId;
        public MonsterGenInRangeTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            string[] paramList = handlerParam.Split(':');
            if (paramList.Length != 2)
            {
                Log.Warn($"MonsterGenInRangeTriHdl param error {handlerParam}");
                return;
            }

            int.TryParse(paramList[0], out r);
            int.TryParse(paramList[1], out genId);
        }

        public override void Handle()
        {
            FieldObject target = trigger.Owner;

            MonsterGenModel model = MonsterGenLibrary.GetModelById(genId);
            if (model == null) return;

            BaseMonsterGen gen = MonsterGenFactory.CreateMonsterGen(CurMap, model);
            if (gen != null && gen.GenType == MonsterGenType.Mannual)
            {
                //同一帧不连续触发
                if (ThisFpsHadHandled()) return;
                SetThisFspHandled();

                List<Vec2> posList = new List<Vec2>();
                for (int i = 0; i < gen.Model.GenCount; i++)
                {
                    gen.GenerateMonstersWithPosition(1, target.RandomPos(r));
                }
            }
        }
    }
}