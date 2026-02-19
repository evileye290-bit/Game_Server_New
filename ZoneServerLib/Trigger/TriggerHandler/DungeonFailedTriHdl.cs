using CommonUtility;
using Logger;

namespace ZoneServerLib
{
    public class DungeonFailedTriHdl : BaseTriHdl
    {
        DungeonMap dungeon;
        public DungeonFailedTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
            dungeon = CurMap as DungeonMap;
            if (dungeon == null)
            {
                Log.Warn("trigger handler type {0} in init failed: cur map is not dungeon", handlerType);
                return;
            }
        }

        public override void Handle()
        {
            if (dungeon == null || dungeon.State != DungeonState.Started)
            {
                return;
            }

            dungeon.SetSpeedUp(false);

            dungeon.Stop(DungeonResult.Failed);
            //Log.Warn("DungeonFailedTriHdl DungeonResult.Failed");
        }
    }
}
