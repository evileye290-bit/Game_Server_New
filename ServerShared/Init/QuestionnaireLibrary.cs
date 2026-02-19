using DataProperty;
using EnumerateUtility.Questionnaire;
using ServerModels;
using System;
using System.Collections.Generic;

namespace ServerShared
{
    public static class QuestionnaireLibrary
    {
        private static Dictionary<int, QuestionnaireInfo> questionnaireInfoList = new Dictionary<int, QuestionnaireInfo>();
        private static Dictionary<int, QuestionInfo> questionInfoList = new Dictionary<int, QuestionInfo>();

        public static void BindQuestionnaireInfo()
        {
            //先初始化问题再初始化问卷

            //questionInfoList.Clear();
            //questionnaireInfoList.Clear();

            Dictionary<int, QuestionnaireInfo> questionnaireInfoList = new Dictionary<int, QuestionnaireInfo>();
            Dictionary<int, QuestionInfo> questionInfoList = new Dictionary<int, QuestionInfo>();

            DataList dataList = DataListManager.inst.GetDataList("Question");
            foreach (var item in dataList)
            {

                QuestionInfo question = InitQuestionInfo(item.Value);
                //Console.WriteLine("question {0}", question.QuestionId);
                questionInfoList.Add(question.QuestionId, question);
            }

            DataList questionnaireDataList = DataListManager.inst.GetDataList("Questionnaire");
            foreach (var item in questionnaireDataList)
            {
                QuestionnaireInfo questionnaire = InitQuestionnaireInfo(item.Value);
                questionnaireInfoList.Add(questionnaire.Id, questionnaire);
            }
            QuestionnaireLibrary.questionnaireInfoList = questionnaireInfoList;
            QuestionnaireLibrary.questionInfoList = questionInfoList;

            BindQuestionnaireWithQuestions();
        }
        //初始化问卷
        public static QuestionnaireInfo InitQuestionnaireInfo(Data data)
        {
            QuestionnaireInfo info = new QuestionnaireInfo();
            info.Id = data.ID;
            info.LimitDate = data.GetInt("date");
            info.LimitLevel = data.GetInt("lv");
            info.Reward = data.GetString("award");
            //Console.WriteLine(info.Reward);
            return info;
        }

        //初始化问题
        public static QuestionInfo InitQuestionInfo(Data data)
        {
            QuestionInfo info = new QuestionInfo();
            info.QuestionId = data.ID;
            info.Type = (QuestionType)data.GetInt("type");
            info.QuestionnaireId = info.QuestionId / 100;
            string temp = data.GetString("input");
            if (!string.IsNullOrWhiteSpace(temp))
            {
                info.HasInput = true;
            }
            info.LoadOptions(data.GetString("option"));
            return info;
        }

        public static QuestionnaireInfo GetQuestionnaireInfo(int id)
        {
            QuestionnaireInfo info = null;
            questionnaireInfoList.TryGetValue(id, out info);
            return info;
        }

        public static QuestionInfo GetQuestionInfo(int id)
        {
            QuestionInfo info = null;
            questionInfoList.TryGetValue(id, out info);
            return info;
        }

        public static void BindQuestionnaireWithQuestions()
        {
            foreach (var info in questionInfoList)
            {
                int questionnaireId = info.Value.QuestionnaireId;
                //Console.WriteLine("qn {0}",info.Value.QuestionnaireId);
                //Console.WriteLine("q {0}", info.Value.QuestionId);
                QuestionnaireInfo thisQuestionnaire = null;
                if (questionnaireInfoList.TryGetValue(questionnaireId, out thisQuestionnaire))
                {
                    thisQuestionnaire.Add(info.Value);
                    //Console.WriteLine(thisQuestionnaire.Questions.Count);

                }
            }
        }

        public static int GetNextQuestionnaire(List<int> questionnaireIds, int level, DateTime timeCreated)
        {
            QuestionnaireInfo info = null;
            if (questionnaireIds == null)
            {
                return 0;
            }

            //int lifeDayCount = (DateTime.Now - timeCreated).Days;
            List<int> minus = new List<int>();
            foreach (int QNId in questionnaireInfoList.Keys)
            {
                if (!questionnaireIds.Contains(QNId))
                {
                    minus.Add(QNId);
                }
            }
            minus.Sort();
            List<QuestionnaireInfo> qns = new List<QuestionnaireInfo>();
            foreach (int QnId in minus)
            {
                QuestionnaireInfo tempinfo = null;
                tempinfo = GetQuestionnaireInfo(QnId);
                if (tempinfo != null)
                {
                    qns.Add(tempinfo);
                }
            }
            if (qns.Count == 0)
            {
                //全部完成
                return -1;
            }
            foreach (QuestionnaireInfo questionInfo in qns)
            {
                if (CheckInfo(questionInfo, level, timeCreated))
                {
                    info = questionInfo;
                    return info.Id;
                }
            }

            //没到触发条件
            return -2;

        }

        //如果传入的<0，对所有的判断
        public static int GetNextQuestionnaire(int questionnaireId, int level, DateTime timeCreated)
        {
            //if (questionnaireId < 0)
            //{
            //    //可能是初始化或者跨zone失败
            //    return -3;
            //}

            QuestionnaireInfo info = null;

            List<int> minus = new List<int>();
            foreach (int QNId in questionnaireInfoList.Keys)
            {
                if (QNId > questionnaireId)
                {
                    minus.Add(QNId);
                }
            }
            minus.Sort();
            List<QuestionnaireInfo> qns = new List<QuestionnaireInfo>();
            foreach (int QnId in minus)
            {
                QuestionnaireInfo tempinfo = null;
                tempinfo = GetQuestionnaireInfo(QnId);
                if (tempinfo != null)
                {
                    qns.Add(tempinfo);
                }
            }
            if (qns.Count == 0)
            {
                //全部完成
                return -1;
            }
            foreach (QuestionnaireInfo questionInfo in qns)
            {
                if (CheckInfo(questionInfo, level, timeCreated))
                {
                    info = questionInfo;
                    return info.Id;
                }
            }

            //没到触发条件
            return -2;
        }

        public static bool CheckInfo(QuestionnaireInfo info, int level, DateTime timeCreated)
        {
            int lifeDayCount = (DateTime.Now - timeCreated).Days;
            if (info.LimitLevel <= level && info.LimitDate <= lifeDayCount)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static List<int> GetQuestionnaireIds()
        {
            List<int> list = new List<int>();
            foreach (int key in questionnaireInfoList.Keys)
            {
                list.Add(key);
            }
            list.Sort();
            return list;
        }


        public static List<int> GetAvailableIds(int questionnaireId, int level, DateTime timeCreated)
        {
            List<int> availableIds = new List<int>();

            List<int> minus = new List<int>();
            foreach (int QNId in questionnaireInfoList.Keys)
            {
                if (QNId > questionnaireId)
                {
                    minus.Add(QNId);
                }
            }
            List<QuestionnaireInfo> qns = new List<QuestionnaireInfo>();
            foreach (int QnId in minus)
            {
                QuestionnaireInfo tempinfo = null;
                tempinfo = GetQuestionnaireInfo(QnId);
                if (tempinfo != null)
                {
                    qns.Add(tempinfo);
                }
            }
            if (qns.Count == 0)
            {
                //全部完成
                return null;
            }
            foreach (QuestionnaireInfo questionnaireInfo in qns)
            {
                if (CheckInfo(questionnaireInfo, level, timeCreated))
                {
                    int id = questionnaireInfo.Id;
                    availableIds.Add(id);
                }
            }
            return availableIds;
        }
    }
}
