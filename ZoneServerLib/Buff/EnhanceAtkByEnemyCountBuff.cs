using CommonUtility;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class EnhanceAtkByEnemyCountBuff : BaseBuff
    {
        private int enemyCount = 0;
        public EnhanceAtkByEnemyCountBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
            AddListener(TriggerMessageType.EnemyDead, EnemyDead);
            AddListener(TriggerMessageType.AnySkillDoDamage, DoDamage);          
            List<FieldObject> fieldList = new List<FieldObject>();
            SkillSplashChecker.GetEnemyInMap(owner, owner.CurrentMap, fieldList);
            enemyCount = fieldList.Count;
        }

        protected override void Update(float dt)
        {
        }

        protected override void Start()
        {
            owner.AddNatureAddedValue(NatureType.PRO_ATK, enemyCount * (int)c, Model.Notify);
        }

        protected override void End()
        {
            RemoveListener(TriggerMessageType.EnemyDead, EnemyDead);
            owner.AddNatureAddedValue(NatureType.PRO_ATK, enemyCount * (int)c * -1);
        }    

        private void EnemyDead(object param)
        {
            FieldObject fieldObject = param as FieldObject;
            if (fieldObject == null)
            {
                return;
            }
            owner.AddNatureAddedValue(NatureType.PRO_ATK, (int)c * -1);
            if (enemyCount > 0)
            {
                enemyCount--;
            }
        }
        
        private void DoDamage(object param)
        {
            if (enemyCount != 0)
            {
                return;
            }

            List<FieldObject> fieldList = new List<FieldObject>();
            SkillSplashChecker.GetEnemyInMap(owner, owner.CurrentMap, fieldList);
            enemyCount = fieldList.Count;
            owner.AddNatureAddedValue(NatureType.PRO_ATK, enemyCount * (int)c, Model.Notify);
        }
    }
}
