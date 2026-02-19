using CommonUtility;
using Logger;
using ScriptFighting;
using ServerModels;

namespace ZoneServerLib
{
    public class DamageReboundBuff : BaseBuff
    {
        // 反伤百分比c 在OnHit中处理反伤逻辑 此处无需处理
        public DamageReboundBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

     
    }
}
