using CommonUtility;
using Logger;
using ServerModels;
using ServerShared;

namespace ZoneServerLib
{
    public class TriggerInMonster : BaseTrigger
    {
        public override FieldMap CurMap
        { get { return Owner.CurrentMap; } }

        public TriggerInMonster(FieldObject owner, int triggerId) : base(owner, owner)
        {
            TriggerModel model = TriggerInMonsterLibrary.GetModel(triggerId);
            if (model == null)
            {
                Log.Warn("create trigger in monster {0} failed: no such trigger", triggerId);
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
