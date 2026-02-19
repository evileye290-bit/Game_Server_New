using System;
using DBUtility;
using System.Collections.Generic;
using EnumerateUtility.Questionnaire;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using Message.Zone.Protocol.ZM;
using ServerModels;
using ServerShared;
using EnumerateUtility.Activity;


namespace ZoneServerLib
{
    public partial class PlayerChar
    {

        //用做统计的完成与否，同时只有全部完成该活动才完成
        //public List<int> CompletedQuestionnaireIds = new List<int>();

        //缓存上一个，要进行跨zone
        //public int cachedNextQuestionnaireId = 0;

        //public int GetNextQuestionnaire()
        //{
        //    int next = QuestionnaireLibrary.GetNextQuestionnaire(cachedNextQuestionnaireId, this.Level, this.TimeCreated);

        //    if (next > 0)
        //    {
        //        cachedNextQuestionnaireId = next;
        //    }

        //    return next;
        //}

        //要发活动和问卷两个流，这是问卷所需流的处理
        //统计并记录信息
        //不要管是否成功
        //发放奖励
        public void QuestionnaireComplete(MSG_GateZ_QUESTIONNAIRE_COMPLETE msg)
        {
            //如果有，判断是否符合条件
            QuestionnaireInfo info = QuestionnaireLibrary.GetQuestionnaireInfo(msg.QuestionnaireId);
            if (info == null)
            {
                return;
            }
            if (!QuestionnaireLibrary.CheckInfo(info, this.Level, this.TimeCreated))
            {
                return;
            }
            //暂时先不做问题深度校验
            Dictionary<int,QuestionInfo> questionInfos = info.Questions;
            List<Question> questions = new List<Question>();
            //依次对问题判断，出现错误就直接返回
            foreach (MSG_GateZ_QUESTION_COMPLETE questionMsg in msg.Questions)
            {
                Question question = new Question();
                question.Id = questionMsg.QuestionId;
                question.QuestionnaireId = info.Id ;
                question.Answers.AddRange(questionMsg.Answers); 
                question.Input = questionMsg.Input;
                QuestionInfo questionInfo = questionInfos[question.Id];

                if (questionInfo.Type != (QuestionType)questionMsg.QuestionType)
                {
                    return;
                }
                else
                {
                    question.Type = questionInfo.Type;
                }
                questions.Add(question);
            }

            //GetQuestionnaireReward(info);
            //拿到所有的问题数据，写入埋点
            RecordQuestions(questions);
        }

        public void RecordQuestions(List<Question> questions)
        {
            foreach (Question question in questions)
            {
                RecordQuestionnaireLog(question);
            }
        }

        //public int GetFirstQuestionnaire()
        //{
        //    return QuestionnaireLibrary.GetNextQuestionnaire(cachedNextQuestionnaireId, this.Level, this.TimeCreated);
        //}
    }
}
