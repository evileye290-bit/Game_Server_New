using CommonUtility;
using EnumerateUtility;
using ServerModels;
using ServerShared;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    class DomainInfo
    {
        public int Id { get; private set; }
        public int CasterInstenceId { get; private set; }
        public DomainModel Model { get; private set; }
        public List<FieldObject> EffectFields { get; private set; }

        public DomainInfo(int id, int instanceId, DomainModel model, List<FieldObject> fields)
        {
            Id = id;
            CasterInstenceId = instanceId;
            Model = model;
            EffectFields = fields;
        }
    }

    public class DomainManager
    {
        private List<FieldObject> targetList = new List<FieldObject>();

        //<id, info>
        private ListMap<int, DomainInfo> domainList = new ListMap<int, DomainInfo>();

        private FieldMap currMap;
        public FieldMap CurrMap => currMap;

        public DomainManager(FieldMap map)
        {
            currMap = map;
        }

        public bool HaveDomain(int id) => domainList.ContainsKey(id);

        public void Effect(FieldObject caster, int domainId, int skillLevel, int skillLevelGrowth)
        {
            //if (HaveDomain(domainId)) return;

            DomainModel model = DomainLibrary.GetDomain(domainId);
            if (model == null) return;

            FindTargetes(caster, model.TargetType);

            targetList.ForEach(x => Effect(caster, x, model, skillLevel, skillLevelGrowth));

            domainList.Add(domainId, new DomainInfo(domainId, caster.InstanceId, model, targetList));
        }

        private void FindTargetes(FieldObject caster, DomainTargetType type)
        {
            targetList.Clear();

            switch (type)
            {
                case DomainTargetType.Self:
                    targetList.Add(caster);
                    break;
                case DomainTargetType.AllAlly:
                    SkillSplashChecker.GetAllyInMap(caster, currMap, targetList);
                    break;
                case DomainTargetType.AllEnemy:
                    SkillSplashChecker.GetEnemyInMap(caster, currMap, targetList);
                    break;
            }
        }

        public void Effect(FieldObject caster, FieldObject target, DomainModel model, int skillLevel, int skillLevelGrowth)
        {
            //所有加buff，增加伤害，治疗相关都需要将技能等级成长参数传入
            //trigger，技能增强，增加技能效果的地方都需原始技能等级

            if (model.EventType > DomainEventType.None)
            {
                Effect(caster, target, model.EventType, model.EventParam, skillLevel, skillLevelGrowth);
            }

            if (model.EventType1 > DomainEventType.None)
            {
                Effect(caster, target, model.EventType1, model.EventParam1, skillLevel, skillLevelGrowth);
            }

            if (model.EventType2 > DomainEventType.None)
            {
                Effect(caster, target, model.EventType2, model.EventParam2, skillLevel, skillLevelGrowth);
            }
        }

        private void Effect(FieldObject caster, FieldObject target, DomainEventType type, int param, int skillLevel, int skillLevelGrowth)
        {
            switch (type)
            {
                case DomainEventType.None:
                    break;
                case DomainEventType.TargetAddBuff:
                    target.AddBuff(caster, param, skillLevelGrowth);
                    break;
                case DomainEventType.TargetAddTrigger:
                    target.AddTriggerCreatedBySkill(param, skillLevel, caster);
                    break;
                case DomainEventType.CasterAddBuff:
                    caster.AddBuff(caster, param, skillLevelGrowth);
                    break;
                case DomainEventType.CasterAddTrigger:
                    caster.AddTriggerCreatedBySkill(param, skillLevel, caster);
                    break;
                case DomainEventType.EnhanceSkillEffect:
                    caster.EnhanceSkillEffect(param, skillLevel);
                    break;
                default:
                    break;
            }
        }

        public void OnFieldObjectDead(FieldObject field)
        {
            List<DomainInfo> removeInfos = new List<DomainInfo>();
            foreach (KeyValuePair<int, List<DomainInfo>> kv in domainList)
            {
                removeInfos.AddRange(kv.Value.Where(x => x.CasterInstenceId == field.InstanceId));
            }

            removeInfos.ForEach(RemoveDomain);
        }

        private void RemoveDomain(DomainInfo info)
        {
            if (info?.Model == null || info.EffectFields == null) return;

            domainList.Remove(info.Id, info);

            info.EffectFields.ForEach(x =>
                {
                    switch (info.Model.EventType)
                    {
                        case DomainEventType.CasterAddBuff:
                        case DomainEventType.TargetAddBuff:
                            x.BuffManager?.RemoveBuffsByIdAndInstanceId(info.Model.EventParam, info.CasterInstenceId);
                            break;
                    }
                });
        }
    }
}
