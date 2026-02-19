using ServerModels;

namespace ZoneServerLib
{
    public class CureSelfOnceBuff : BaseBuff
    {
        public CureSelfOnceBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel) :
            base(caster, owner, skillLevel, buffModel)
        {
        }

        protected override void Start()
        {
            if (happened) return;
            owner.DoCure(caster, n, Model.DispatchCureSKillMsg);
            happened = true;
            isEnd = true;
        }
    }
}
