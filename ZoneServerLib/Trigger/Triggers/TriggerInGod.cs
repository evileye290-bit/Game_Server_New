using Logger;
using ServerModels;
using ServerShared;

namespace ZoneServerLib
{
    public class TriggerInGod : BaseTrigger
    {
        public override FieldMap CurMap
        { get { return Owner.CurrentMap; } }

        public TriggerInGod(FieldObject owner, int triggerId) : base(owner, owner)
        {
            TriggerModel model = TriggerCreatedByGodLibrary.GetModel(triggerId);
            if (model == null)
            {
                Log.Warn("create trigger in god {0} failed: no such trigger", triggerId);
                return;
            }

            Init(model);
        }

        public override MessageDispatcher GetMessageDispatcher()
        {
            return Owner.GetDispatcher();
        }
    }
}
