using System.IO;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.IdGenerator;

namespace GateServerLib
{
    public partial class ZoneServer
    {
        
        private void OnResponse_DriftExploreTaskReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_DRIFT_EXPLORE_TASK_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} DriftExploreTaskReward not find client", pcUid);
            }
        }

        private void OnResponse_DriftExploreReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_DRIFT_EXPLORE_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} DriftExploreReward not find client", pcUid);
            }
        }

        private void OnResponse_InitDriftExploreInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_INIT_DRIFT_EXPLORE_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} InitDriftExploreInfo not find client", pcUid);
            }
        }

        private void OnResponse_UpdateDriftExploreTaskInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_UPDATE_DRIFT_EXPLORE_TASK_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} UpdateDriftExploreTaskInfo not find client", pcUid);
            }
        }

        private void OnResponse_UpdateDriftExploreInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_UPDATE_DRIFT_EXPLORE_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} UpdateDriftExploreInfo not find client", pcUid);
            }
        }

       
    }
}
