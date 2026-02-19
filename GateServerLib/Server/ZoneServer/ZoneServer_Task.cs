using Logger;
using Message.Gate.Protocol.GateC;
using Message.IdGenerator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GateServerLib
{
    public partial class ZoneServer
    {
        //private void OnResponse_SyncTaskListMessage(MemoryStream stream, int pcUid)
        //{
        //    Client client = Api.ClientMng.FindClientByUid(pcUid);
        //    if (client != null)
        //    {
        //        client.Write(Id<MSG_ZGC_TASK_LIST>.Value, stream);
        //    }
        //    else
        //    {
        //        Log.WarnLine("player {0} sync task List not find client", pcUid);
        //    }
        //}

        private void OnResponse_SyncTaskChange(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_TASK_CHANGE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} sync task change not find client", pcUid);
            }
        }

        private void OnResponse_SyncGetTaskResult(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_TASK_RESULT>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} sync get task result not find client", pcUid);
            }
        }

        private void OnResponse_TaskCollectResult(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_TASK_COLLECT>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} task colect not find client", pcUid);
            }
        }
        private void OnResponse_TaskCompleteResult(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_TASK_COMPLETE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} task complete not find client", pcUid);
            }
        }

        private void OnResponse_TaskFlyAnswer(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_USE_TASKFLY_ANSWER>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} task fly answer not find client", pcUid);
            }
        }

        private void OnResponse_TaskFlySetDone(MemoryStream stream, int pcUid)
        {
            
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_TASKFLY_POSITION_SETDONE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} task fly setdone not find client", pcUid);
            }
        }

        private void OnResponse_TaskFinishState(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_TASK_FINISH_STATE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} taskFinishState not find client", pcUid);
            }
        }

        private void OnResponse_TaskFinishStateReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_TASK_FINISH_STATE_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} TaskFinishStateReward not find client", pcUid);
            }
        }
    }
}
