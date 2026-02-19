using CommonUtility;
using ServerModels;
using System;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public class EnhanceDefenceRatioByAllyCountBuff : BaseBuff
    {
        private int allyCount = 0;
        private DateTime nextTime = ZoneServerApi.now;

        public EnhanceDefenceRatioByAllyCountBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
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

            int addV = fieldList.Count - allyCount;
            owner.AddNatureRatio(NatureType.PRO_DEF, addV * (int)c);

            allyCount = fieldList.Count;
        }

        protected override void End()
        {
            owner.AddNatureRatio(NatureType.PRO_DEF, allyCount * (int)c * -1);
            allyCount = 0;
        }
    }
}

