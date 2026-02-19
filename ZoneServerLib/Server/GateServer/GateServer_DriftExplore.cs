using System.IO;
using Logger;
using Message.Gate.Protocol.GateZ;

namespace ZoneServerLib
{
    public partial class GateServer
    {
        /// <summary>
        /// 领取任务奖励
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="uid"></param>
        public void OnResponse_DriftExploreTaskReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_DRIFT_EXPLORE_TASK_REWARD pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_DRIFT_EXPLORE_TASK_REWARD>(stream);
            Log.Write($"player [{uid}] DriftExploreTaskReward");
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn($"player [{uid}] DriftExploreTaskReward not in gateid [{SubId}] pc list");
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} DriftExploreTaskReward not in map ", uid);
                return;
            }

            player.DriftExploreTaskReward(pks.TaskId);
        }
        
        /// <summary>
        /// 领取豪华奖励
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="uid"></param>
        public void OnResponse_DriftExploreReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_DRIFT_EXPLORE_REWARD pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_DRIFT_EXPLORE_REWARD>(stream);
            Log.Write($"player [{uid}] DriftExploreReward");
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn($"player [{uid}] DriftExploreReward not in gateid [{SubId}] pc list");
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} DriftExploreReward not in map ", uid);
                return;
            }

            player.DriftExploreReward();
        }

    }
}