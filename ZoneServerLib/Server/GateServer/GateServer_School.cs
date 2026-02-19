using System.IO;
using Logger;
using Message.Gate.Protocol.GateZ;
using MessagePacker;
using EnumerateUtility.Activity;

namespace ZoneServerLib
{
    public partial class GateServer
    {
        private void OnResponse_EnterSchool(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_ENTER_SCHOOL pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_ENTER_SCHOOL>(stream);
            Log.Write("player {0} request enter school", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} EnterSchool not in gate id {1} pc list", uid, SubId);
                return;
            }

            player.EnterSchool(pks.SchoolId);
        }

        private void OnResponse_LeaveSchool(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_LEAVE_SCHOOL pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_LEAVE_SCHOOL>(stream);
            Log.Write("player {0} request leave school", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} LeaveSchool not in gate id {1} pc list", uid, SubId);
                return;
            }

            player.LeaveSchool();
        }

        private void OnResponse_SchoolPoolUseItem(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_SCHOOL_POOL_USE_ITEM msg = ProtobufHelper.Deserialize<MSG_GateZ_SCHOOL_POOL_USE_ITEM>(stream);
            Log.Write("player {0} request SchoolPoolUseItem", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} SchoolPoolUseItem not in gate id {1} pc list", uid, SubId);
                return;
            }

            player.SchoolPoolUseItem(msg.ItemId);
        }

        private void OnResponse_SchoolPoolLevelUp(MemoryStream stream, int uid = 0)
        {
            Log.Write("player {0} request SchoolPoolLevelUp", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} SchoolPoolLevelUp not in gate id {1} pc list", uid, SubId);
                return;
            }

            player.SchoolPoolLevelUp();
        }

        private void OnResponse_GetSchoolTaskFinishReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_SCHOOLTASK_FINISH_REWARD msg = ProtobufHelper.Deserialize<MSG_GateZ_GET_SCHOOLTASK_FINISH_REWARD>(stream);
            Log.Write("player {0} request GetSchoolTaskFinishReward", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GetSchoolTaskFinishReward not in gate id {1} pc list", uid, SubId);
                return;
            }

            player.GetSchoolTasksFinishReward(msg.GetAll, msg.TaskId);
        }

        private void OnResponse_GetSchoolTaskBoxReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_SCHOOLTASK_BOX_REWARD msg = ProtobufHelper.Deserialize<MSG_GateZ_GET_SCHOOLTASK_BOX_REWARD>(stream);
            Log.Write("player {0} request GetSchoolTaskBoxReward", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GetSchoolTaskBoxReward not in gate id {1} pc list", uid, SubId);
                return;
            }

            player.GetSchoolTaskBoxReward((TaskFinishType)msg.RewardType, msg.Index);
        }

        private void OnResponse_AnswerQuestionStart(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_ANSWER_QUESTION_START msg = ProtobufHelper.Deserialize<MSG_GateZ_ANSWER_QUESTION_START>(stream);
            Log.Write("player {0} request AnswerQuestionStart", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} AnswerQuestionStart not in gate id {1} pc list", uid, SubId);
                return;
            }

            player.RecordAnswerQuestionStart(msg.Type);
        }

        private void OnResponse_AnswerQuestionSubmit(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_ANSWER_QUESTION_SUBMIT msg = ProtobufHelper.Deserialize<MSG_GateZ_ANSWER_QUESTION_SUBMIT>(stream);
            Log.Write("player {0} request AnswerQuestionSubmit", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} AnswerQuestionSubmit not in gate id {1} pc list", uid, SubId);
                return;
            }

            player.AnswerQuestionSubmit(msg.Type, msg.QuestionNum, msg.AnswerNum, msg.IsEnd);
        }
    }
}
