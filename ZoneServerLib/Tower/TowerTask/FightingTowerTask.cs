using EnumerateUtility;
using Message.Gate.Protocol.GateC;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public class FightingTowerTask : TowerTask
    {
        public FightingTowerTask(TowerManager manager, int id) : base(manager, id, TowerTaskType.Fighting)
        {
        }

        public override ErrorCode Execute(int param, MSG_ZGC_TOWER_EXECUTE_TASK msg)
        {
            return ErrorCode.Success;
        }
    }
}
