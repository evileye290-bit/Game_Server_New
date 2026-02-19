using ServerModels;

namespace ZoneServerLib
{
    /// <summary>
    /// 冰冻buff 与眩晕buff一起使用，冰冻buff不做任何处理，需要同步前端表现
    /// </summary>
    public class FreezeBuff : BaseBuff
    {
        public FreezeBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void Start()
        {
        }
    }
}

