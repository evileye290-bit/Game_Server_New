using System.IO;
using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateZ;
using MessagePacker;

namespace GateServerLib
{
    public partial class Client
    {
        private void OnResponse_EnterSchool(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_ENTER_SCHOOL msg = ProtobufHelper.Deserialize<MSG_CG_ENTER_SCHOOL>(stream);
            WriteToZone(new MSG_GateZ_ENTER_SCHOOL() { SchoolId = msg.SchoolId });
        }

        private void OnResponse_LeaveSchool(MemoryStream stream)
        {
            if (curZone == null) return;
            WriteToZone(new MSG_GateZ_LEAVE_SCHOOL());
        }

        private void OnResponse_SchoolPoolUseItem(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_SCHOOL_POOL_USE_ITEM msg = ProtobufHelper.Deserialize<MSG_CG_SCHOOL_POOL_USE_ITEM>(stream);
            WriteToZone(new MSG_GateZ_SCHOOL_POOL_USE_ITEM() { ItemId = msg.ItemId });
        }

        private void OnResponse_SchoolPoolLevelUp(MemoryStream stream)
        {
            if (curZone == null) return;
            WriteToZone(new MSG_GateZ_SCHOOL_POOL_LEVEL_UP());
        }

        private void OnResponse_GetSchoolTaskFinishReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_SCHOOLTASK_FINISH_REWARD msg = ProtobufHelper.Deserialize<MSG_CG_GET_SCHOOLTASK_FINISH_REWARD>(stream);
            WriteToZone(new MSG_GateZ_GET_SCHOOLTASK_FINISH_REWARD() { GetAll = msg.GetAll, TaskId = msg.TaskId });
        }

        private void OnResponse_GetSchoolTaskBoxReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_SCHOOLTASK_BOX_REWARD msg = ProtobufHelper.Deserialize<MSG_CG_GET_SCHOOLTASK_BOX_REWARD>(stream);
            WriteToZone(new MSG_GateZ_GET_SCHOOLTASK_BOX_REWARD() { RewardType = msg.RewardType, Index = msg.Index});
        }

        private void OnResponse_AnswerQuestionStart(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_ANSWER_QUESTION_START msg = ProtobufHelper.Deserialize<MSG_CG_ANSWER_QUESTION_START>(stream);
            WriteToZone(new MSG_GateZ_ANSWER_QUESTION_START() {Type = msg.Type});
        }

        private void OnResponse_AnswerQuestionSubmit(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_ANSWER_QUESTION_SUBMIT msg = ProtobufHelper.Deserialize<MSG_CG_ANSWER_QUESTION_SUBMIT>(stream);
            WriteToZone(new MSG_GateZ_ANSWER_QUESTION_SUBMIT() { Type = msg.Type, QuestionNum = msg.QuestionNum, AnswerNum = msg.AnswerNum, IsEnd = msg.IsEnd });
        }
    }
}
