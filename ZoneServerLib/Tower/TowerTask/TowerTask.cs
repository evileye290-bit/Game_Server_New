using EnumerateUtility;
using Message.Gate.Protocol.GateC;

namespace ZoneServerLib
{
    public abstract class TowerTask 
    {
        public TowerTaskInfo TaskInfo { get; private set; }

        public int TaskId { get; private set; }
        public int NodeId { get; private set; }//当前完成的节点
        public TowerTaskType Type { get; private set; }
        public TowerManager Manager { get; private set; }

        public TowerTask(TowerManager manager, int id, TowerTaskType type)
        {
            TaskId = id;
            Type = type;
            Manager = manager;
        }

        public void SetNodeId(int nodeId)
        {
            NodeId = nodeId;
        }

        public void SetTowerTaskInfo(TowerTaskInfo taskInfo)
        {
            this.TaskInfo = taskInfo;
        }

        public abstract ErrorCode Execute(int param, MSG_ZGC_TOWER_EXECUTE_TASK msg);

        public virtual bool CheckFinished()
        {
            return true;
        }
    }
}
