using EnumerateUtility;
using Message.Gate.Protocol.GateC;
using ServerShared;

namespace ZoneServerLib
{
    public class CompsiteTowerTask : TowerTask
    {
        public CompsiteTowerTask(TowerManager manager, int id) : base(manager, id, TowerTaskType.Compsite)
        { 
        }

        public override ErrorCode Execute(int param, MSG_ZGC_TOWER_EXECUTE_TASK msg)
        {
            Manager.AddHeroHpRatio(TowerLibrary.HpRatio);
            Manager.GotoNextNode();

            return ErrorCode.Success;
        }

        public override bool CheckFinished()
        {
            return TaskId != 0;
        }
    }
}
