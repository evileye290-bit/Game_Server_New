using CommonUtility;
using EnumerateUtility;
using EpPathFinding;
using ServerModels;

namespace ZoneServerLib
{
    public class DizzyBuff : BaseBuff
    {
        public DizzyBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
#if DEBUG
            if (buffModel.Id == 21141)
            {
                Logger.Log.Warn($"buff type {buffModel.BuffType} buff id {buffModel.Id} effect on hero {owner.GetHeroId()} casted by  hero {caster.GetHeroId()}");
            }
#endif
        }

        protected override void Start()
        {
            if(owner.IsMoving)
            {
                owner.OnMoveStop();
                owner.BroadCastStop();
            }
            owner.FsmManager.SetNextFsmStateType(FsmStateType.IDLE, true, s);
        }

        protected override void End()
        {
            //可能有多个眩晕buff，当其中一个结束的时候，其他可能还没有结束，不能直接切状态机
            if (!owner.BuffManager.InBuffState(BuffType.Dizzy))
            {
                if (owner?.FieldObjectType == TYPE.HERO)
                {
                    owner.FsmManager.SetNextFsmStateType(FsmStateType.HERO_IDLE, true, 0.1f);
                }
                else
                { 
                    owner.FsmManager.SetNextFsmStateType(FsmStateType.IDLE, true, 0.1f);
                }
            }
        }
    }
}
