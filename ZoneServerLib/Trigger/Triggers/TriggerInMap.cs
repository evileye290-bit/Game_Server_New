using CommonUtility;
using Logger;
using ServerModels;
using ServerShared;

namespace ZoneServerLib
{
    public class TriggerInMap : BaseTrigger
    {
        private FieldMap curMap;
        public override FieldMap CurMap
        { get { return curMap; } }

        public TriggerInMap(FieldMap map, int triggerId) : base()
        {
            curMap = map;
            TriggerModel model = TriggerInMapLibrary.GetModel(triggerId);
            if (model == null)
            {
                Log.Warn("create trigger in map {0} failed: no such trigger", triggerId);
                return;
            }

            Init(model);
        }

        public override MessageDispatcher GetMessageDispatcher()
        {
            return curMap.GetMessageDispatcher();
        }
    }
}
