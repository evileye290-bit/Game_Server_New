using CommonUtility;

namespace ZoneServerLib
{
    public class AddBuffPileCountTriHdl : BaseTriHdl
    {
        readonly int buffId, pileCount;
        public AddBuffPileCountTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            string[] paramList = handlerParam.Split(':');
            if (paramList.Length == 1)
            {
                buffId = int.Parse(paramList[0]);
                pileCount = 1;
            }
            else
            {
                buffId = int.Parse(paramList[0]);
                pileCount = int.Parse(paramList[1]);
            }
        }

        public override void Handle()
        {
            //同一帧不连续触发
            if (ThisFpsHadHandled()) return;
            SetThisFspHandled();

            int pileNum = pileCount;
            BaseBuff buff = Owner.BuffManager.GetBuff(buffId);
            if (buff == null)
            {
                if (pileNum > 0)
                {
                    Owner.AddBuff(trigger.Owner, buffId, trigger.GetFixedParam_SkillLevelGrowth(), pileCount);
                }
            }
            else
            {
                if (buff.Model.OverlayType == BuffOverlayType.PileById)
                {
                    buff.AddPileNum(pileNum);
                }
            }
        }
    }
}
