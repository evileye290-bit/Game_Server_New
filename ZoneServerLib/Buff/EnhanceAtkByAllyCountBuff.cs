using CommonUtility;
using ServerModels;
using System;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public class EnhanceAtkByAllyCountBuff : BaseBuff
    {
        private int addedAtk = 0;
        private DateTime nextTime = ZoneServerApi.now;

        public EnhanceAtkByAllyCountBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void Update(float dt)
        {
            if (ZoneServerApi.now > nextTime)
            {
                CheckAllyCount();
                nextTime = ZoneServerApi.now.AddMilliseconds(200d);
            }
        }

        protected override void Start()
        {
            CheckAllyCount();
        }

        private void CheckAllyCount()
        {
            List<FieldObject> fieldList = new List<FieldObject>();
            SkillSplashChecker.GetAllyInMap(owner, owner.CurrentMap, fieldList);

            //需要排除自己
            if (fieldList.Count > 1)
            {
                //当前应该增加的值
                int addV = (int)(m * (1 + (fieldList.Count - 1) * c * 0.0001f));

                owner.AddNatureAddedValue(NatureType.PRO_ATK, (addV - addedAtk));

                addedAtk = addV;
            }
        }

        protected override void End()
        {
            owner.AddNatureAddedValue(NatureType.PRO_ATK, addedAtk * -1);
        }
    }
}

