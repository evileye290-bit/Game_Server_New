using CommonUtility;

namespace ZoneServerLib
{
    partial class FieldMap
    {
        public DomainManager DomainManager { get; private set; }

        private void InitDomainManager()
        {
            DomainManager = new DomainManager(this);
        }

        public void DomainEffect(FieldObject caster, int id, int skillLevel, int skillGrowth)
        {
            DomainManager.Effect(caster, id, skillLevel, skillGrowth);
        }

        public void RemoveDomain(FieldObject field)
        {
            DomainManager.OnFieldObjectDead(field);
        }
    }
}
