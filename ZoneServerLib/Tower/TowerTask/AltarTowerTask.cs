using EnumerateUtility;
using Message.Gate.Protocol.GateC;

namespace ZoneServerLib
{
    public class AltarTowerTask : TowerTask
    {
        public AltarTowerTask(TowerManager manager, int id) : base(manager, id, TowerTaskType.Altar)
        {
        }

        public override ErrorCode Execute(int param, MSG_ZGC_TOWER_EXECUTE_TASK msg)
        {
            msg.ReviveHeroId = Manager.ReviveRandomHero();
            Manager.GotoNextNode();

            return ErrorCode.Success;
        }

        public override bool CheckFinished()
        {
            return TaskId != 0;
        }
    }
}
