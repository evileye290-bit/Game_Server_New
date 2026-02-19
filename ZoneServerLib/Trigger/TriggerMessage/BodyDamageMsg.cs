namespace ZoneServerLib
{
    public class BodyDamageMsg
    {
        public readonly FieldObject Caster;
        public readonly long Damage;

        public BodyDamageMsg(FieldObject caster, long damage)
        {
            this.Caster = caster;
            this.Damage = damage;
        }

    }
}
