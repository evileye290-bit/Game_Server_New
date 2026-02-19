using CommonUtility;
using ServerModels;

namespace ZoneServerLib
{
    public class AddHitOnSkillMissBuff : BaseBuff
    {
        private MessageDispatcher dispatcher;
        public AddHitOnSkillMissBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
            pileNum = 0;
            dispatcher = caster.GetDispatcher();
        }

        protected override void Start()
        {
            dispatcher?.AddListener(TriggerMessageType.SkillMissed, OnSkillMiss);
            dispatcher?.AddListener(TriggerMessageType.AnySkillDoDamageTargetList, OnHit);
        }

        protected override void Pile(int addNum)
        {
            owner.AddNatureAddedValue(NatureType.PRO_HIT, (int)(c * addNum), buffModel.Notify);
        }

        protected override void End()
        {
            owner.AddNatureAddedValue(NatureType.PRO_HIT, pileNum * (int)c * -1);
        }

        private void OnSkillMiss(object obj)
        {
            AddPileNum(1);
        }

        private void OnHit(object obj)
        {
            OnEnd();
        }
    }
}