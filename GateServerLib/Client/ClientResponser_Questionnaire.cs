using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateZ;
using System.IO;
using Message.Gate.Protocol.GateC;
using EnumerateUtility.Questionnaire;
using ServerModels;
using EnumerateUtility;
using MessagePacker;

namespace GateServerLib
{
    public partial class Client
    {
        public void OnResponse_QuestionnaireComplete(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_QUESTIONNAIRE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_QUESTIONNAIRE>(stream);
            MSG_GateZ_QUESTIONNAIRE_COMPLETE request = new MSG_GateZ_QUESTIONNAIRE_COMPLETE();
            request.PcUid = Uid;
            request.QuestionnaireId = msg.QustionnaireId;
            foreach (var question in msg.Questions)
            {
                MSG_GateZ_QUESTION_COMPLETE completeQuestion = new MSG_GateZ_QUESTION_COMPLETE();
                completeQuestion.Answers.AddRange(question.Answers);

                completeQuestion.QuestionId = question.QuestionId;
                completeQuestion.QuestionType = question.QuestionType;
                completeQuestion.Input = question.Input;
                int inputMaxLimitLength = 200;
                switch ((QuestionType)completeQuestion.QuestionType)
                {
                    case QuestionType.None:
                        break;
                    case QuestionType.Jump:
                        break;
                    case QuestionType.Pictrue:
                        break;
                    case QuestionType.Matrix:
                        break;
                    case QuestionType.Multi:
                        break;
                    case QuestionType.Normal:
                        inputMaxLimitLength = WordLengthLimit.QuestionOptionInput;
                        break;
                    case QuestionType.Input:
                        inputMaxLimitLength = WordLengthLimit.QuestionnaireInput;
                        break;
                    case QuestionType.Telephone:
                        inputMaxLimitLength = WordLengthLimit.QuestionnaireTelephoneInput;
                        break;
                    default:
                        break;
                }
                if (completeQuestion.Input.Length > inputMaxLimitLength)
                {
                    MSG_ZGC_QUESTIONNAIRE_COMPLETE msgToClient = new MSG_ZGC_QUESTIONNAIRE_COMPLETE();
                    msgToClient.Result = MSG_ZGC_QUESTIONNAIRE_COMPLETE.Types.RESULT.Error;
                    Logger.Log.Warn("client {0} Input too long ", this.AccountName);
                    Write(msgToClient);
                    return;
                }
                request.Questions.Add(completeQuestion);
            }
            WriteToZone(request);
        }
    }
}
