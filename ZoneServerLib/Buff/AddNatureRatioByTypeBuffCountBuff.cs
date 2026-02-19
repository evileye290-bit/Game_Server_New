using CommonUtility;
using ServerModels;
using System.Linq;

namespace ZoneServerLib
{
    //根据自身某类buff数量，增加自身攻击力百分比
    //c:单个buff增加的百分比
    //m:buff类型
    //n:NatureType属性类型
    public class AddNatureRatioByTypeBuffCountBuff : BaseBuff
    {
        private int ratio, addRatio;
        private int lastCount;
        BuffType buffType;
        NatureType nature;

        public AddNatureRatioByTypeBuffCountBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
            ratio = (int)c;
            buffType = (BuffType)m;
            nature = (NatureType)n;
        }

        protected override void Start()
        {
            lastCount = owner.BuffManager.GetBuffList().Where(x => x.BuffType == buffType).Sum(x => x.PileNum);
            addRatio = lastCount * ratio;
            if (addRatio > 0)
            {
                owner.AddNatureRatio(nature, addRatio);
            }
        }

        protected override void Update(float dt)
        {
            elapsedTime += dt;
            if (elapsedTime < deltaTime)
            {
                return;
            }

            int count = owner.BuffManager.GetBuffList().Where(x => x.BuffType == buffType).Sum(x => x.PileNum);
            int det = count - lastCount;
            addRatio = det * ratio;
            lastCount = count;
            if (addRatio > 0)
            {
                owner.AddNatureRatio(nature, addRatio);
            }
        }

        protected override void End()
        {
            if (lastCount > 0)
            {
                owner.AddNatureRatio(NatureType.PRO_ATK, lastCount * ratio * -1);
            }
        }
    }
}

