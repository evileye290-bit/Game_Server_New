using CommonUtility;
using EnumerateUtility;
using ScriptFighting;
using ServerModels;
using ServerShared;
using System.Collections.Generic;

namespace ZoneServerLib
{
    partial class TowerDungeon : DungeonMap
    {
        private List<int> towerBuffList = new List<int>();

        public void SetTowerBuff(List<int> buffList)
        {
            towerBuffList.AddRange(buffList);
        }

        public void InitTowerHeroStatus()
        {
            HeroAddBuffAndSkillEnergy();
        }

        private void HeroAddBuffAndSkillEnergy()
        {
            foreach (var hero in HeroList)
            {
                AddSkillEnergy(hero.Value);
                HeroAddBuff(hero.Value);
            }
        }

        private void HeroAddBuff(Hero hero)
        {
            foreach (var kv in towerBuffList)
            {
                TowerBuffModel model = TowerLibrary.GetTowerBuffModel(kv);
                if (model == null) continue;

                if (model.JobLimit != TowerBuffLimit.AllEnermy)
                {
                    if (!CheckLimit(hero, model)) continue;
                    EventEffect(hero, model, 1);
                }
            }
        }

        private void MonsterAddBuff(IFightingObject monster)
        {
            foreach (var kv in towerBuffList)
            {
                TowerBuffModel model = TowerLibrary.GetTowerBuffModel(kv);
                if (model == null) continue;

                if (model.JobLimit != TowerBuffLimit.AllEnermy) continue;

                EventEffect(monster, model, 1);
            }
        }

        private bool CheckLimit(Hero hero, TowerBuffModel model)
        {
            JobType job = hero.GetJobType();
            switch (model.JobLimit)
            {
                case TowerBuffLimit.All: return true;
                case TowerBuffLimit.Tank: return job == JobType.Tank;
                case TowerBuffLimit.Control: return job == JobType.Control;
                case TowerBuffLimit.SingleAttack: return job == JobType.SingleAttack;
                case TowerBuffLimit.Support: return job == JobType.Support;
                case TowerBuffLimit.GroupAttack: return job == JobType.GroupAttack;
                case TowerBuffLimit.Hero: return hero.HeroId == model.LimitParam;
            }
            return false;
        }

        private void EventEffect(IFightingObject owner, TowerBuffModel model, int skillLevel)
        {
            if (model.EventType != TowerEventType.None)
            {
                DoEffect(owner, model.EventType, model.EventParam, skillLevel);
            }

            if (model.EventType1 != TowerEventType.None)
            {
                DoEffect(owner, model.EventType1, model.EventParam1, skillLevel);
            }
        }

        private void DoEffect(IFightingObject owner, TowerEventType eventType, int eventParam, int skillLevel)
        {
            FieldObject fieldObject = owner as FieldObject;
            switch (eventType)
            {
                case TowerEventType.AddBuff:
                    fieldObject.AddBuff(fieldObject, eventParam, skillLevel);
                    break;
                case TowerEventType.AddTrigger:
                    TriggerCreatedByTower trigger = new TriggerCreatedByTower(fieldObject, eventParam, skillLevel, fieldObject);
                    fieldObject.AddTrigger(trigger);
                    break;
                case TowerEventType.EnhanceSkillEffect:
                    owner.EnhanceSkillEffect(eventParam, skillLevel);
                    break;
            }
        }

    }
}
