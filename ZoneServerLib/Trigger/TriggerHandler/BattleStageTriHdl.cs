using CommonUtility;

namespace ZoneServerLib
{
    public class BattleStageTriHdl : BaseTriHdl
    {
        public BattleStageTriHdl(BaseTrigger trigger, TriggerHandlerType handlerType, string handlerParam)
            : base(trigger, handlerType, handlerParam)
        {
        }

        public override void Handle()
        {
            DungeonMap dungeon = CurMap as DungeonMap;
            dungeon?.AddBattleStage();
        }
    }
}
