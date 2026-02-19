using CommonUtility;
using DataProperty;
using EnumerateUtility;
using ServerModels;
using System.Collections.Generic;

namespace ServerShared
{
    public static class EmailLibrary
    {
        private static Dictionary<int, EmailInfo> emailInfoList = new Dictionary<int, EmailInfo>();
        public static Dictionary<int, EmailInfo> EmailInfoList
        {
            get { return emailInfoList; }
        }

        private static Dictionary<int, List<int>> emailTaskGroups = new Dictionary<int, List<int>>();
        public static Dictionary<int, List<int>> EmailTaskGroups
        {
            get { return emailTaskGroups; }
        }

        //private static Dictionary<int, int> emailTaskIds = new Dictionary<int, int>();
        //public static Dictionary<int, int> EmailTaskIds
        //{
        //    get { return emailTaskIds; }
        //}

        private static Dictionary<int, Dictionary<int, int>> emailTaskLevelGroups = new Dictionary<int, Dictionary<int, int>>();
        public static Dictionary<int, Dictionary<int, int>> EmailTaskLevelGroups
        {
            get { return emailTaskLevelGroups; }
        }

        public static int ItemOverTimeEmail { get; set; }
        public static int HeroAppraiseEmail { get; set; }
        public static int FireworkLuckyEmail { get; set; }
        public static int ShopGoldEmail { get; set; }
        public static int ItemGameLevelEmail { get; set; }
        public static int OnePieceBattleEmail { get; set; }
        public static int GiftCodeRewardEmail { get; set; }
        public static int ContributionEmail { get; set; }
        public static int DeleteOverTime { get; set; }

        public static int OnekeyCount { get; set; }
        public static void BindEmailDatas()
        {
            //emailInfoList.Clear();
            //emailTaskGroups.Clear();
            //emailTaskIds.Clear();
            //emailTaskLevelGroups.Clear();

            BindConfig();
            BindEmailInfoData();
            BindEmailTaskLevelDatas();
        }

        private static void BindConfig()
        {
            DataList gameConfig = DataListManager.inst.GetDataList("EmailConfig");
            foreach (var item in gameConfig)
            {
                Data data = item.Value;
                ItemOverTimeEmail = data.GetInt("ItemOverTimeEmail");
                HeroAppraiseEmail = data.GetInt("HeroAppraiseEmail");
                DeleteOverTime = data.GetInt("DeleteOverTime");
                FireworkLuckyEmail = data.GetInt("FireworkLuckyEmail");
                ShopGoldEmail = data.GetInt("ShopGoldEmail");
                ItemGameLevelEmail = data.GetInt("ItemGameLevelEmail");
                OnePieceBattleEmail = data.GetInt("OnePieceBattleEmail");
                GiftCodeRewardEmail = data.GetInt("GiftCodeRewardEmail");
                ContributionEmail = data.GetInt("ContributionEmail");
                OnekeyCount = data.GetInt("OnekeyCount");
            }
        }

        private static void BindEmailInfoData()
        {
            Dictionary<int, EmailInfo> emailInfoList = new Dictionary<int, EmailInfo>();
            Dictionary<int, List<int>> emailTaskGroups = new Dictionary<int, List<int>>();

            List<int> list;
            // 处理邮件内容
            DataList dataLsit = DataListManager.inst.GetDataList("Email");
            foreach (var item in dataLsit)
            {
                Data data = item.Value;
                //判断公告有效日期
                //string deleteTime = data.GetString("Delete");
                //if (!string.IsNullOrEmpty(deleteTime))
                //{
                //    DateTime deleteDateTime = DateTime.Parse(deleteTime);
                //    //DeleteTime = deleteDateTime.ToString(DATETIME_DATE_STRING);
                //    if (deleteDateTime < DateTime.Now)
                //    {
                //        //超过有效期不进行加载
                //        continue;
                //    }
                //}
                int id = data.ID;
                int type = data.GetInt("Type");
                bool isDelete = data.GetBoolean("isDelete");
                int validityDate = data.GetInt("ValidityDate");
                //int taskId = data.GetInt("HideTaskId");
                string title = data.GetString("Title");
                string fromName = data.GetString("EndText");
                string body = data.GetString("Content");
                string rewards = data.GetString("Reward");
                int taskId = 0;
                if (rewards != "" && !rewards.Contains("|"))
                {
                    string[] reward = rewards.Split(':');
                    if (int.Parse(reward[1]) == (int)RewardType.Task)
                    {
                        taskId = int.Parse(reward[0]);
                    }
                }
                switch ((EmailType)type)
                {
                    case EmailType.Daily:
                        int group = data.GetInt("Group");
                        //emailTaskIds.Add(id, taskId);

                        if (emailTaskGroups.TryGetValue(group, out list))
                        {
                            list.Add(id);
                        }
                        else
                        {
                            list = new List<int>();
                            list.Add(id);
                            emailTaskGroups.Add(group, list);
                        }
                        break;
                    default:
                        break;
                }

                EmailInfo info = new EmailInfo();
                info.Id = id;
                info.Type = type;
                info.IsDelete = isDelete;
                info.ValidityDate = validityDate;
                info.TaskId = taskId;
                info.Title = title;
                info.FromName = fromName;
                info.Body = body;
                info.Rewards = rewards;
                emailInfoList.Add(id, info);
                //TODO 定时发送设置
                //string SendTime = data.GetString("Send");
                //if (!string.IsNullOrEmpty(SendTime))
                //{
                //    DateTime sendDateTime = DateTime.Parse(data.GetString("Send"));
                //    //SendTime = sendDateTime.ToString(DATETIME_DATE_STRING);
                //}
            }
            EmailLibrary.emailInfoList = emailInfoList;
            EmailLibrary.emailTaskGroups = emailTaskGroups;
        }

        public static void BindEmailTaskLevelDatas()
        {
            Dictionary<int, Dictionary<int, int>> emailTaskLevelGroups = new Dictionary<int, Dictionary<int, int>>();

            Dictionary<int, int> list;
            // 处理邮件内容
            DataList dataLsit = DataListManager.inst.GetDataList("EmailTaskGroup");
            foreach (var item in dataLsit)
            {
                Data data = item.Value;

                int level = data.ID;

                string groupString = data.GetString("Group");
                string[] groups = StringSplit.GetArray("|", groupString);
                foreach (var str in groups)
                {
                    string[] group = StringSplit.GetArray(":", str);
                    int groupId = int.Parse(group[0]);
                    int pro = int.Parse(group[1]);

                    if (emailTaskLevelGroups.TryGetValue(level, out list))
                    {
                        list[groupId] = pro;
                    }
                    else
                    {
                        list = new Dictionary<int, int>();
                        list[groupId] = pro;
                        emailTaskLevelGroups.Add(level, list);
                    }
                }
            }
            EmailLibrary.emailTaskLevelGroups = emailTaskLevelGroups;
        }

        public static EmailInfo GetEmailInfo(int id)
        {
            EmailInfo info;
            EmailInfoList.TryGetValue(id, out info);
            return info;
        }

        public static List<int> GetEmailGroupTasks(int group)
        {
            List<int> tasks;
            EmailTaskGroups.TryGetValue(group, out tasks);
            return tasks;
        }

        //public static int GetEmailTaskId(int emailId)
        //{
        //    int taskId;
        //    EmailTaskIds.TryGetValue(emailId, out taskId);
        //    return taskId;
        //}

        public static Dictionary<int, int> GetEmailTaskLevelGroups(int level)
        {
            Dictionary<int, int> groups;
            EmailTaskLevelGroups.TryGetValue(level, out groups);
            return groups;
        }
        //public void DeleteEmailDatas(int emailUid)
        //{
        //    EmailInfoModel emailInfo;
        //    if (emailInfoList.TryGetValue(emailUid, out emailInfo))
        //    {
        //        emailInfoList.Remove(emailUid);
        //    }
        //    else
        //    {
        //        Log.WarnLine("Delete Email Datas not find Uid:{0}", emailUid);
        //    }
        //}

        //public List<int> GetEmailUidsById(int id)
        //{
        //    List<int> uids = new List<int>();
        //    foreach (var email in emailInfoList)
        //    {
        //        if (email.Value.Id == id)
        //        {
        //            uids.Add(email.Key);
        //        }
        //    }
        //    return uids;
        //}

    }
}
