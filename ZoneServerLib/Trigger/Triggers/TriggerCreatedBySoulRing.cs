using CommonUtility;
using Logger;
using ScriptFighting;
using ServerModels;
using ServerShared;

namespace ZoneServerLib
{
    public class TriggerCreatedBySoulRing : BaseTrigger
    {
        private readonly int skillGrowth;
        
        public override FieldMap CurMap=>Owner.CurrentMap;

        public TriggerCreatedBySoulRing(FieldObject owner, int triggerId, int skillGrowth) : base(owner, owner)
        {
            TriggerModel model = TriggerCreatedBySoulRingLibrary.GetModel(triggerId);
            if (model == null)
            {
                Log.Warn("create trigger created by soul ring {0} failed: no such trigger", triggerId);
                return;
            }

            this.skillGrowth = skillGrowth;

            RecordFixedParam(TriggerParamKey.CreatedBySkillLevel, skillGrowth);
            RecordFixedParam(TriggerParamKey.CreatedBySkillLevelGrowth, skillGrowth);

            Init(model);
        }

        public override MessageDispatcher GetMessageDispatcher()
        {
            return Owner.GetDispatcher();
        }

        protected override bool ProbabilityHappened()
        {
            int probability = TriggerCreatedBySkillCalculator.CalcProbability(Model.Name, skillGrowth, Model.Probability);
            if(probability >= 10000)
            {
                return true;
            }
            if(probability <= 0)
            {
                return false;
            }
            return RAND.Range(1, 10000) <= probability;
        }
        
    }
}
