using CommonUtility;
using Logger;
using ScriptFighting;
using ServerModels;
using ServerShared;

namespace ZoneServerLib
{
    public class TriggerCreatedByGuideSoulItem : BaseTrigger
    {
        public override FieldMap CurMap
        { get { return Owner.CurrentMap; } }

        private int skillLevel;
        public int SkillLevel { get { return skillLevel; } }

        public TriggerCreatedByGuideSoulItem(FieldObject owner, int triggerId, int skillLevel, FieldObject caster) : base(owner, caster)
        {
            this.skillLevel = skillLevel;
            RecordFixedParam(TriggerParamKey.CreatedBySkillLevel, skillLevel);
            RecordFixedParam(TriggerParamKey.CreatedBySkillLevelGrowth, SkillLibrary.GetSkillGrowth(skillLevel));

            TriggerModel model = TriggerCreatedByGuideSoulItemLibrary.GetModel(triggerId);
            if (model == null)
            {
                Log.Warn("create trigger created by guide soul item {0} failed: no such trigger", triggerId);
                return;
            }

            Init(model);
        }


        public override MessageDispatcher GetMessageDispatcher()
        {
            return Owner.GetDispatcher();
        }

        protected override bool ProbabilityHappened()
        {
            int probability = TriggerCreatedBySkillCalculator.CalcProbability(Model.Name, skillLevel, Model.Probability);
            if (probability >= 10000)
            {
                return true;
            }
            if (probability <= 0)
            {
                return false;
            }
            return RAND.Range(1, 10000) <= probability;
        }
    }
}