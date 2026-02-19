using CommonUtility;
using Logger;
using ScriptFighting;
using ServerModels;
using ServerShared;

namespace ZoneServerLib
{
    //所有加buff，增加伤害，治疗相关都需要将技能等级成长参数传入
    //trigger，技能增强，增加技能效果的地方都需原始技能等级

    public class TriggerCreatedBySkill : BaseTrigger
    {
        public override FieldMap CurMap
        { get { return Owner.CurrentMap; } }

        private int skillLevel;
        public int SkillLevel { get { return skillLevel; } }

        public TriggerCreatedBySkill(FieldObject owner, int triggerId, int skillLevel, FieldObject caster) : base(owner, caster)
        {
            this.skillLevel = skillLevel;
            RecordFixedParam(TriggerParamKey.CreatedBySkillLevel, skillLevel);
            int skillGrowth = SkillLibrary.GetSkillGrowth(skillLevel);
            if (caster as Pet != null)
            {
                skillGrowth = PetLibrary.GetPetInbornSkillGrowth(skillLevel);
            }
            RecordFixedParam(TriggerParamKey.CreatedBySkillLevelGrowth, skillGrowth);

            TriggerModel model = TriggerCreatedBySkillLibrary.GetModel(triggerId);
            if (model == null)
            {
                Log.Warn("create trigger created by skill {0} failed: no such trigger", triggerId);
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

        public override int CalcParam(TriggerHandlerType handlerType, int param)
        {
            return TriggerCreatedBySkillCalculator.CalcParam(Model.Name, handlerType, skillLevel, param);
        }

        public override int CalcParam(TriggerCondition condition, int param)
        {
            return TriggerCreatedBySkillCalculator.CalcParam(Model.Name, condition, skillLevel, param);
        }
    }
}
