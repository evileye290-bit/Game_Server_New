using System.IO;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.IdGenerator;

namespace GateServerLib
{
    public partial class ZoneServer
    {
        private void OnResponse_SchoolInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SCHOOL_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} SchoolInfo not find client", pcUid);
            }
        }

        private void OnResponse_EnterSchool(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ENTER_SCHOOL>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} enter school not find client", pcUid);
            }
        }

        private void OnResponse_LeaveSchool(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_LEAVE_SCHOOL>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} leave school not find client", pcUid);
            }
        }

        private void OnResponse_SchoolPoolUseItem(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SCHOOL_POOL_USE_ITEM>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} SchoolPoolUseItem not find client", pcUid);
            }
        }

        private void OnResponse_SchoolPoolLevelUp(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SCHOOL_POOL_LEVEL_UP>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} SchoolPoolLevelUp not find client", pcUid);
            }
        }

        private void OnResponse_SchoolTaskFinishInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_SCHOOLTASK_FINISH_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} SchoolTaskFinishInfo not find client", pcUid);
            }
        }

        private void OnResponse_InitSchoolTasksInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_INIT_SCHOOLTASKS_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} InitSchoolTasksInfo not find client", pcUid);
            }
        }

        private void OnResponse_UpdateSchoolTasksInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_UPDATE_SCHOOLTASKS_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} UpdateSchoolTasksInfo not find client", pcUid);
            }
        }

        private void OnResponse_GetSchoolTaskFinishReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_SCHOOLTASK_FINISH_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GetSchoolTaskFinishReward not find client", pcUid);
            }
        }

        private void OnResponse_GetSchoolTaskBoxReward(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_GET_SCHOOLTASK_BOX_REWARD>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} GetSchoolTaskBoxReward not find client", pcUid);
            }
        }

        private void OnResponse_AnswerQuestionInfo(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ANSWER_QUESTION_INFO>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} AnswerQuestionInfo not find client", pcUid);
            }
        }

        private void OnResponse_AnswerQuestionSubmit(MemoryStream stream, int pcUid)
        {
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ANSWER_QUESTION_SUBMIT>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} AnswerQuestionSubmit not find client", pcUid);
            }
        }
    }
}
