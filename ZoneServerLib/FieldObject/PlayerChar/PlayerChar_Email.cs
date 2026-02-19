using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Google.Protobuf.Collections;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using Message.Zone.Protocol.ZR;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using ZoneServerLib.Email;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        //邮件
        public EmailManager EmailMng = new EmailManager();

        private string systemEmailIds = string.Empty;

        public string SystemEmailIds
        {
            get { return systemEmailIds; }
        }

        public void BindEmailItems(List<EmailItem> list)
        {
            foreach (var emailItem in list)
            {
                if (CheckEmailTimeout(emailItem))
                {
                    //邮件已经超时
                    continue;
                }

               
                if (emailItem.Id != 0)
                {
                    //说明邮件在xml中，但是奖励是自定义
                    EmailInfo info = EmailLibrary.GetEmailInfo(emailItem.Id);
                    if (info != null)
                    {
                        switch (emailItem.DataBase)
                        {
                            case EmailDbTable.System:
                                EmailMng.AddSystemEmailItem(info, (int)emailItem.IsRead, emailItem.SendTime, emailItem.DeleteTime, emailItem.IsGet);
                                break;
                            case EmailDbTable.Item:
                                EmailMng.AddPersonEmailItem(info, emailItem);
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        //XML中没有找到邮件， 删掉没用邮件
                        Log.Warn("player {0} bind email items not find {1} email {2} info uid {3}", Uid, emailItem.Type, emailItem.Id, emailItem.Uid);
                        UpdateEmail(emailItem.Uid, EmailDbTable.Item, (int)EmailUpdateType.Delete);
                    }
                }
                else
                {
                    //id 是 0 说明邮件不在xml中，是自定义邮件
                    EmailMng.AddCustomEmailItem(emailItem);
                }
            }
        }

        private bool CheckEmailTimeout(EmailItem emailItem)
        {
            if (emailItem.DeleteTime > 0)
            {
                //邮件有删除时间
                DateTime time = Timestamp.TimeStampToDateTime(emailItem.DeleteTime);
                if (ZoneServerApi.now > time)
                {
                    //邮件超时删除邮件
                    UpdateEmail(emailItem.Uid, EmailDbTable.Item, (int)EmailUpdateType.Delete);
                    return true;
                }
            }
            else
            {
                EmailInfo info = EmailLibrary.GetEmailInfo(emailItem.Id);
                if (info != null && !info.IsDelete)
                {
                    DateTime time = Timestamp.TimeStampToDateTime(emailItem.SendTime);
                    time = time.AddDays(info.ValidityDate);
                    if (ZoneServerApi.now > time)
                    {
                        //邮件超时删除邮件
                        UpdateEmail(emailItem.Uid, EmailDbTable.Item, (int)EmailUpdateType.Delete);
                        return true;
                    }
                }
                else
                {
                    DateTime time = Timestamp.TimeStampToDateTime(emailItem.SendTime);
                    time = time.AddDays(EmailLibrary.DeleteOverTime);
                    if (ZoneServerApi.now > time)
                    {
                        //邮件超时删除邮件
                        UpdateEmail(emailItem.Uid, EmailDbTable.Item, (int)EmailUpdateType.Delete);
                        return true;
                    }
                }
            }
            return false;
        }

        public void BindSystemEmails(string systemEmailIds)
        {
            this.systemEmailIds = systemEmailIds;
            //格式： 读取状态（0|1）：邮件ID：邮件时间戳：附件领取状态（0|1）| 读取状态（0|1）：邮件ID：邮件时间戳：附件领取状态（0|1）
            string[] tempItems = StringSplit.GetArray("|", systemEmailIds);
            foreach (var itemString in tempItems)
            {
                if (string.IsNullOrEmpty(itemString))
                {
                    continue;
                }
                string[] emailItem = StringSplit.GetArray(":", itemString);
                if (emailItem.Length == 5)
                {
                    int isRead = int.Parse(emailItem[0]);
                    int id = int.Parse(emailItem[1]);
                    int sendTime = int.Parse(emailItem[2]);
                    int deleteTime = int.Parse(emailItem[3]);
                    int isGet = int.Parse(emailItem[4]);

                    if (deleteTime > 0)
                    {
                        //邮件有删除时间
                        DateTime time = Timestamp.TimeStampToDateTime(deleteTime);
                        if (ZoneServerApi.now > time)
                        {
                            //删除邮件修改字符串
                            DeleteSystemEmail(itemString);
                            //邮件超时删除邮件
                            SyncRelationDeleteAllEmail(id, sendTime, deleteTime);
                            continue;
                        }
                    }
                    else
                    {
                        DateTime time = Timestamp.TimeStampToDateTime(sendTime);
                        time = time.AddDays(EmailLibrary.DeleteOverTime);
                        if (ZoneServerApi.now > time)
                        {
                            //删除邮件修改字符串
                            DeleteSystemEmail(itemString);
                            //邮件超时删除邮件
                            SyncRelationDeleteAllEmail(id, sendTime, deleteTime);
                            continue;
                        }
                    }
                    EmailInfo info = EmailLibrary.GetEmailInfo(id);
                    if (info != null)
                    {
                        EmailMng.AddSystemEmailItem(info, isRead, sendTime, deleteTime, isGet);
                    }
                    else
                    {
                        Log.Warn("player {0} bind system email items not find email {1} info", Uid, id);
                        //删除邮件修改字符串
                        DeleteSystemEmail(itemString);
                    }
                }
                else
                {
                    Log.Warn("player {0} bind system email items Length error {1}", Uid, itemString);
                    //删除邮件修改字符串
                    DeleteSystemEmail(itemString);
                }
            }
        }

        private void SyncRelationDeleteAllEmail(int id, int sendTime, int deleteTime)
        {
            MSG_ZR_DELETE_ALL_EMAI msg = new MSG_ZR_DELETE_ALL_EMAI();
            msg.EmailId = id;
            msg.SendTime = sendTime;
            msg.DeleteTime = deleteTime;
            server.SendToRelation(msg);
        }

        public void AddNewSystemEmail(EmailInfo info, int sendTime, int deleteTime, int isGet)
        {
            //给所有人发
            string newInfo = string.Format("0:{0}:{1}:{2}:{3}|", info.Id, sendTime, deleteTime, isGet);
            systemEmailIds += newInfo;
            EmailMng.AddSystemEmailItem(info, 0, sendTime, deleteTime, isGet);
        }

        public void AddNewPersonEmail(EmailInfo info, ulong emailUid, string body, string rewards, int sendTime, int deleteTime, int isGet, string param = "")
        {
            EmailItem emailItem = new EmailItem();
            emailItem.Uid = emailUid;
            emailItem.Id = info.Id;

            emailItem.Rewards = rewards;
            emailItem.IsRead = (int)EmailReadType.NotRead;
            emailItem.IsGet = isGet;
            emailItem.SendTime = sendTime;
            emailItem.DeleteTime = deleteTime;

            emailItem.Body = body;
            emailItem.Param = param;

            //emailItem.Type = info.Type;
            //emailItem.DataBase = EmailDbTable.Item;
            EmailMng.AddPersonEmailItem(info, emailItem);
        }

        public void AddNewCustomEmail(ulong emailUid, string title, string body, string fromName, string rewards, int sendTime, int deleteTime, int isGet, string param)
        {
            EmailItem emailItem = new EmailItem();
            emailItem.Uid = emailUid;
            emailItem.Title = title;
            emailItem.Body = body;
            emailItem.FromName = fromName;

            emailItem.Rewards = rewards;
            emailItem.IsRead = (int)EmailReadType.NotRead;
            emailItem.IsGet = isGet;
            emailItem.SendTime = sendTime;
            emailItem.DeleteTime = deleteTime;
            emailItem.Param = param;
            //emailItem.Type = (int)EmailType.Custom;
            //emailItem.DataBase = EmailDbTable.Item;
            EmailMng.AddCustomEmailItem(emailItem);
        }

        public void ReadEmail(ulong uid, int id, int time, int language)
        {
            EmailItem email = EmailMng.GetEmailItem(uid, id, time);
            if (email != null)
            {
                EmailUpdateType updateType = EmailUpdateType.None;

                if (email.IsRead == (int)EmailReadType.NotRead)
                {
                    email.IsRead = EmailReadType.Read;
                    //修改数据库
                    switch (email.DataBase)
                    {
                        case EmailDbTable.System:
                            //修改字符串
                            ReadSystemEmail(email.Id, email.SendTime, email.DeleteTime);
                            updateType = EmailUpdateType.Read;
                            break;
                        default:
                            //修改值
                            updateType = EmailUpdateType.Read;
                            break;
                    }
                }

                if (CheckDeleteEmail(email))
                {
                    updateType = EmailUpdateType.Delete;
                }

                if (updateType != EmailUpdateType.None)
                {
                    UpdateEmail(email.Uid, email.DataBase, (int)updateType);
                }

                SyncEmailBody(email.Uid, email.Id, email.SendTime, email.Body, language);
                //BI 邮件
                KomoeEventLogMailFlow(3, email.Type, email.Id, email.Title, email.Body, email.SendTime, email.DeleteTime, 0, RewardManager.GetRewardDic(email.Rewards));
            }
            else
            {
                //没有找到邮件
                Log.Warn("player {0} read email items not find email {1} send time {2}", Uid, id, time);
            }
        }

        private bool CheckDeleteEmail(EmailItem email)
        {
            EmailInfo info = EmailLibrary.GetEmailInfo(email.Id);
            if (email.DeleteTime != 0)
            {
                return false;
            }
            if (info != null && !info.IsDelete)
            {
                return false;
            }
            switch ((EmailType)email.Type)
            {
                case EmailType.Daily:
                    if (email.IsRead != EmailReadType.Run)
                    {
                        return false;
                    }
                    break;
                default:
                    if (!string.IsNullOrEmpty(email.Rewards))
                    {
                        if (email.IsGet != 1)
                        {
                            return false;
                        }
                    }
                    if (info != null && info.TaskId > 0 && email.IsRead != EmailReadType.Run)
                    {
                        return false;
                    }
                    break;
            }
            //没有附件，没有删除日期，沒有隐藏任务，有阅后即删标识，看完删掉
            switch (email.DataBase)
            {
                case EmailDbTable.System:
                    //修改字符串
                    DeleteSystemEmail(email.Id, email.SendTime, email.DeleteTime);
                    //updateType = EmailUpdateType.Delete;
                    EmailMng.DeleteEmail(email.Uid, email.Id, email.SendTime);
                    return true;
                default:
                    //修改值
                    //updateType = EmailUpdateType.Delete;
                    EmailMng.DeleteEmail(email.Uid, email.Id, email.SendTime);
                    return true;
            }
        }

        private void RunSystemEmail(int emailId, int sendTime, int deleteTime)
        {
            string oldState = string.Format("1:{0}:{1}:{2}:", emailId, sendTime, deleteTime);
            if (SystemEmailIds.Contains(oldState))
            {
                string newState = string.Format("2:{0}:{1}:{2}:", emailId, sendTime, deleteTime);
                systemEmailIds = systemEmailIds.Replace(oldState, newState);
            }
            else
            {
                Log.Warn("player {0} run email items not find string {1} in {2}", uid, oldState, SystemEmailIds);
            }
        }

        private void ReadSystemEmail(int emailId, int sendTime, int deleteTime)
        {
            string oldState = string.Format("0:{0}:{1}:{2}:", emailId, sendTime, deleteTime);
            if (SystemEmailIds.Contains(oldState))
            {
                string newState = string.Format("1:{0}:{1}:{2}:", emailId, sendTime, deleteTime);
                systemEmailIds = systemEmailIds.Replace(oldState, newState);
            }
            else
            {
                Log.Warn("player {0} read email items not find string {1} in {2}", uid, oldState, SystemEmailIds);
            }
        }

        private void GetSystemEmailAttchment(int emailId, int sendTime, int deleteTime)
        {
            string oldState = string.Format(":{0}:{1}:{2}:0", emailId, sendTime, deleteTime);
            if (SystemEmailIds.Contains(oldState))
            {
                string newState = string.Format(":{0}:{1}:{2}:1", emailId, sendTime, deleteTime);
                systemEmailIds = systemEmailIds.Replace(oldState, newState);
            }
            else
            {
                Log.Warn("player {0} get email attchment items not find string {1} in {2}", uid, oldState, SystemEmailIds);
            }
        }

        public void DeleteSystemEmail(int emailId, int sendTime, int deleteTime)
        {
            List<string> stateList = GetDeleteSystemEmailStateList(emailId, sendTime, deleteTime);
            DeleteSystemEmail(stateList);
            //string state1 = string.Format("0:{0}:{1}:{2}:1|", emailId, sendTime, deleteTime);
            //string state2 = string.Format("0:{0}:{1}:{2}:0|", emailId, sendTime, deleteTime);
            //string state3 = string.Format("1:{0}:{1}:{2}:1|", emailId, sendTime, deleteTime);
            //string state4 = string.Format("1:{0}:{1}:{2}:0|", emailId, sendTime, deleteTime);
            //string state5 = string.Format("2:{0}:{1}:{2}:1|", emailId, sendTime, deleteTime);
            //string state6 = string.Format("2:{0}:{1}:{2}:0|", emailId, sendTime, deleteTime);
            //systemEmailIds = systemEmailIds.Replace(state1, "");
            //systemEmailIds = systemEmailIds.Replace(state2, "");
            //systemEmailIds = systemEmailIds.Replace(state3, "");
            //systemEmailIds = systemEmailIds.Replace(state4, "");
            //systemEmailIds = systemEmailIds.Replace(state5, "");
            //systemEmailIds = systemEmailIds.Replace(state6, "");
        }

        public static List<string> GetDeleteSystemEmailStateList(int emailId, int sendTime, int deleteTime)
        {
            List<string> stateList = new List<string>();
            for (int i = 0; i <= 2; i++)
            {
                for (int j = 0; j <= 1; j++)
                {
                    string state = string.Format("{0}:{1}:{2}:{3}:{4}|", i, emailId, sendTime, deleteTime, j);
                    stateList.Add(state);
                }
            }

            return stateList;
        }

        public void DeleteSystemEmail(List<string> stateList)
        {
            foreach (var state in stateList)
            {
                DeleteSystemEmail(state);
            }
        }

        private void DeleteSystemEmail(string state)
        {
            systemEmailIds = systemEmailIds.Replace(state, "");
        }

        public void GetAllEmailAttachment()
        {
            RewardManager totalRewardMng = new RewardManager();
            int needTotalSpace = 0;
            int petEggNeedSpace = 0;
            bool spaceCheck = true;
            List<EmailItem> removeList = new List<EmailItem>();
            List<EmailItem> emailList = EmailMng.GetEmailList();
            int getCount = 0;
            foreach (var email in emailList)
            {
                if (getCount >= EmailLibrary.OnekeyCount)
                {
                    break;
                }
                if (email.IsGet == 0 && !string.IsNullOrEmpty(email.Rewards))
                {

                    RewardManager rewardMng = new RewardManager();
                    rewardMng.InitSimpleReward(email.Rewards);

                    //当奖励只有货币的时候不需要检查背包空间
                    needTotalSpace += GetNeedBagCount(rewardMng);
                    //检查宠物蛋背包空间
                    petEggNeedSpace += GetNeedPetEggBagCount(rewardMng);
                    spaceCheck = needTotalSpace <= BagManager.GetBagRestSpace() && petEggNeedSpace <= PetManager.GetPetEggBagRestSpace();
                    if (spaceCheck)
                    {
                        email.IsGet = 1;
                        email.IsRead = EmailReadType.Read;
                        foreach (var item in rewardMng.AllRewards)
                        {
                            totalRewardMng.AddReward(item);
                        }
                        removeList.Add(email);
                        getCount++;
                        server.TrackingLoggerMng.RecordGetEmailRewardLog(Uid, email.Id, email.Rewards, email.Param, server.MainId, server.Now());

                        //BI 邮件
                        KomoeEventLogMailFlow(6, email.Type, email.Id, email.Title, email.Body, email.SendTime, email.DeleteTime, 1, RewardManager.GetRewardDic(email.Rewards));
                    }
                    else
                    {
                        //背包已满
                        break;
                    }
                }
                //else
                //{
                //    //没有找到邮件
                //    Log.Warn("player {0} get attachment items not find {1} email {2} send time {3}", uid, emailUid, emailId, time);
                //}
            }

            MSG_ZGC_PICKUP_ATTACHMENT_BATCH msg = new MSG_ZGC_PICKUP_ATTACHMENT_BATCH();

            foreach (var email in removeList)
            {
                //修改数据库
                switch (email.DataBase)
                {
                    case EmailDbTable.System:
                        //修改字符串
                        GetSystemEmailAttchment(email.Id, email.SendTime, email.DeleteTime);
                        break;
                    default:
                        //修改值
                        break;
                }

                //检查是否删除邮件
                UpdateEmail(email.Uid, email.DataBase, (int)EmailUpdateType.Get);

                MSG_ZGC_EMAIL_READ_BATCH itemMsg = new MSG_ZGC_EMAIL_READ_BATCH();
                itemMsg.Id = email.Id;
                itemMsg.UidHigh = email.Uid.GetHigh();
                itemMsg.UidLow = email.Uid.GetLow();
                itemMsg.SendTime = email.SendTime;
                msg.Emails.Add(itemMsg);
            }

            if (totalRewardMng.AllRewards.Count > 0)
            {
                totalRewardMng.BreakupRewards();
                AddRewards(totalRewardMng, ObtainWay.Eamil);
                totalRewardMng.GenerateRewardItemInfo(msg.Rewards);
            }
            if (!spaceCheck)
            {
                //int bagSpace = BagManager.GetBagRestSpace();
                Log.Warn("player {0} get email attachment failed, restBagSpace not enough", Uid);
                msg.Result = (int)ErrorCode.BagSpaceNotEnough;
                Write(msg);
            }
            else
            {
                if (totalRewardMng.AllRewards.Count > 0)
                {
                    msg.Result = (int)ErrorCode.Success;
                    Write(msg);
                }
            }
        }

        public void GetEmailAttachment(ulong emailUid, int emailId, int time)
        {
            EmailItem email = EmailMng.GetEmailItem(emailUid, emailId, time);
            if (email != null)
            {
                EmailUpdateType updateType = EmailUpdateType.None;

                RewardManager rewardMng = new RewardManager();
                rewardMng.InitSimpleReward(email.Rewards);

                //当奖励只有货币的时候不需要检查背包空间
                int needTotalSpace = GetNeedBagCount(rewardMng);
                int petEggNeedSpace = GetNeedPetEggBagCount(rewardMng);
                bool spaceCheck = needTotalSpace <= BagManager.GetBagRestSpace() && petEggNeedSpace <= PetManager.GetPetEggBagRestSpace();

                if (email.IsGet == 0 && spaceCheck && !CheckEmailTimeout(email))
                {
                    email.IsGet = 1;

                    AddRewards(rewardMng, ObtainWay.Eamil, email.Id.ToString());

                    server.TrackingLoggerMng.RecordGetEmailRewardLog(Uid, email.Id, email.Rewards, email.Param, server.MainId, server.Now());

                    //BI 邮件
                    KomoeEventLogMailFlow(6, email.Type, email.Id, email.Title, email.Body, email.SendTime, email.DeleteTime, 1, RewardManager.GetRewardDic(email.Rewards));

                    //修改数据库
                    switch (email.DataBase)
                    {
                        case EmailDbTable.System:
                            //修改字符串
                            GetSystemEmailAttchment(email.Id, email.SendTime, email.DeleteTime);
                            updateType = EmailUpdateType.Get;
                            break;
                        default:
                            //修改值
                            updateType = EmailUpdateType.Get;
                            break;
                    }
                }

                //检查是否删除邮件
                if (CheckDeleteEmail(email))
                {
                    updateType = EmailUpdateType.Delete;
                }

                if (updateType != EmailUpdateType.None)
                {
                    UpdateEmail(email.Uid, email.DataBase, (int)updateType);
                }

                SyncEmailAttachment(email, rewardMng, spaceCheck);
            }
            else
            {
                //没有找到邮件
                Log.Warn("player {0} get attachment items not find {1} email {2} send time {3}", uid, emailUid, emailId, time);
            }
        }

        public void GetEmailTask(ulong emailUid, int emailId, int time)
        {
            EmailItem email = EmailMng.GetEmailItem(emailUid, emailId, time);
            if (email != null)
            {
                EmailInfo info = EmailLibrary.GetEmailInfo(emailId);
                if (info == null)
                {
                    Log.Warn("player {0} get email task not find email info {1}", uid, emailId);
                    return;
                }
                string[] rewards = null;
                if (!string.IsNullOrEmpty(email.Rewards))
                {
                    rewards = email.Rewards.Split(':');
                }
                int rewardType = 0;
                if (rewards != null)
                {
                    rewardType = int.Parse(rewards[1]);
                }
                if (rewardType != (int)RewardType.Task && info.TaskId <= 0)
                {
                    Log.Warn("player {0} get email task fail, email {1} rewardType {2} or task id {3}", uid, emailId, rewardType, info.TaskId);
                    return;
                }

                //过期邮件处理
                if (IsGetTaskEmailTimeOut(email, info.TaskId))
                {
                    Log.Warn("player {0} get email task email {1} fail, email out of date, task id {2}", uid, emailId, info.TaskId);
                    return;
                }

                if (RunEmail(email))
                {
                    AcceptEmailTask(email, info.TaskId);
                }
            }
            else
            {
                //没有找到邮件
                Log.Warn("player {0} get email task not find {1} email {2} send time {3}", uid, emailUid, emailId, time);
            }
        }

        private bool RunEmail(EmailItem email)
        {
            bool isRun = false;
            EmailUpdateType updateType = EmailUpdateType.None;

            if (email.IsRead == EmailReadType.Read)
            {
                email.IsRead = EmailReadType.Run;
                isRun = true;
                //修改数据库
                switch (email.DataBase)
                {
                    case EmailDbTable.System:
                        //修改字符串
                        RunSystemEmail(email.Id, email.SendTime, email.DeleteTime);
                        updateType = EmailUpdateType.Run;
                        break;
                    default:
                        //修改值
                        updateType = EmailUpdateType.Run;
                        break;
                }
            }

            if (CheckDeleteEmail(email))
            {
                updateType = EmailUpdateType.Delete;
            }

            if (updateType != EmailUpdateType.None)
            {
                UpdateEmail(email.Uid, email.DataBase, (int)updateType);
            }

            return isRun;
        }

        public void UpdateEmail(ulong emailUid, EmailDbTable emailType, int updateType)
        {
            switch (emailType)
            {
                case EmailDbTable.System:
                    MSG_ZR_UPDATE_EMAI msg = new MSG_ZR_UPDATE_EMAI();
                    msg.PcUid = Uid;
                    //读{s|i} 领取{s|i} 删除{s|i}
                    msg.UpdateType = updateType;
                    msg.EmailIdS = SystemEmailIds;
                    server.SendToRelation(msg);
                    break;
                default:
                    //string emailItemTableName = "email_items";
                    EmailUpdateType type = (EmailUpdateType)updateType;
                    switch (type)
                    {
                        case EmailUpdateType.Read:
                            server.GameDBPool.Call(new QueryReadEmailItem(Uid, emailUid));
                            break;
                        case EmailUpdateType.Run:
                            server.GameDBPool.Call(new QueryRunEmailItem(Uid, emailUid));
                            break;
                        case EmailUpdateType.Get:
                            server.GameDBPool.Call(new QueryGetEmailItem(Uid, emailUid));
                            break;
                        case EmailUpdateType.Delete:
                            server.GameDBPool.Call(new QueryDeleteEmailItem(Uid, emailUid));
                            break;
                        default:
                            break;
                    }
                    break;
            }
        }

        public void SendPersonEmail(int emailId, string boday = "", string reward = "", int saveTime = 0, string param = "")
        {
            EmailInfo info = EmailLibrary.GetEmailInfo(emailId);
            if (info != null)
            {
                SendPersonEmail(info, boday, reward, saveTime, param);
            }
            else
            {
                Log.Warn("player {0} send email not find email id:{1}", Uid, emailId);
            }
        }

        private void SendPersonEmail(EmailInfo info, string boday = "", string reward = "", int saveTime = 0, string param = "")
        {
            string newReward = string.Empty;
            if (string.IsNullOrEmpty(newReward))
            {
                newReward = info.Rewards;
            }

            MSG_ZR_SEND_EMAIL msg = new MSG_ZR_SEND_EMAIL();
            msg.EmailId = info.Id;
            msg.Uid = Uid;
            msg.Reward = reward;
            msg.SaveTime = saveTime;
            msg.Param = param;
            server.SendToRelation(msg);

            //ulong emailUid = server.UID.NewIuid(server.MainId, server.SubId);

            //int sendTime = Timestamp.GetUnixTimeStampSeconds(ZoneServerApi.now);

            ////int deleteTime = GetEmailDeleteTime(saveTime);
            //int deleteTime = GetEmailDeleteTime(info.ValidityDate);



            //if (string.IsNullOrEmpty(boday))
            //{
            //    boday = info.Body;
            //}

            //int isGet = GetEmailIsGet(newReward, info.TaskId);

            ////增加邮件信息
            ////string tableName = "email_items";
            //server.GameDBPool.Call(new QueryInsertEmailItem(Uid, emailUid, info.Id, isGet, sendTime, deleteTime, "", "", boday, newReward, param));

            //AddNewPersonEmail(info, emailUid, boday, newReward, sendTime, deleteTime, isGet, param);

            //SyncNewEmail();
        }

        //public void SendCustomEmail(string title, string fromName, string text, string rewards = "", string param = "")
        //{
        //    //邮件ID自增
        //    ulong emailUid = server.UID.NewEuid(server.MainId, server.SubId);

        //    int sendTime = Timestamp.GetUnixTimeStampSeconds(ZoneServerApi.now);

        //    int deleteTime = GetEmailDeleteTime(EmailLibrary.DeleteOverTime);

        //    int isGet = GetEmailIsGet(rewards, 0);

        //    //增加邮件信息
        //    //string tableName = "email_items";
        //    server.GameDBPool.Call(new QueryInsertEmailItem(Uid, emailUid, 0, isGet, sendTime, deleteTime, title, fromName, text, rewards, param));

        //    AddNewCustomEmail(emailUid, title, text, fromName, rewards, sendTime, deleteTime, isGet, param);

        //    SyncNewEmail();
        //}

        //private int GetEmailDeleteTime(int saveTime)
        //{
        //    int deleteTime = 0;
        //    if (saveTime != 0)
        //    {
        //        deleteTime = Timestamp.GetUnixTimeStampSeconds(ZoneServerApi.now.AddDays(saveTime));
        //    }
        //    return deleteTime;
        //}

        private int GetEmailDeleteTime(int sendTime, int defaultSaveTime)
        {
            int deleteTime = 0;
            DateTime sendDateTime = Timestamp.TimeStampToDateTime(sendTime);
            deleteTime = Timestamp.GetUnixTimeStampSeconds(sendDateTime.AddDays(defaultSaveTime));
            return deleteTime;
        }

        private int GetEmailIsGet(string rewards, int taskId)
        {
            int isGet = 1;
            if (!string.IsNullOrEmpty(rewards) || taskId > 0)
            {
                isGet = 0;
            }
            return isGet;
        }

        public void CheckNewEmail()
        {
            List<EmailItem> emailList = EmailMng.GetEmailList();
            foreach (var email in emailList)
            {
                if (email.IsGet == 0)
                {
                    SyncNewEmail();
                    return;
                }
            }
        }

        public void SyncEmailList(int language)
        {
            MSG_ZGC_EMAIL_OPENE_BOX msg = new MSG_ZGC_EMAIL_OPENE_BOX();
            List<EmailItem> emailList = EmailMng.GetEmailList();
            List<EmailItem> removeEmailList = new List<EmailItem>();
            //检查过期邮件
            foreach (var email in emailList)
            {
                if (CheckEmailTimeout(email))
                {
                    removeEmailList.Add(email);
                }
                //检查是否删除邮件
                if (!string.IsNullOrEmpty(email.Rewards))
                {
                    if (email.IsGet == 1)
                    {
                        EmailInfo info = EmailLibrary.GetEmailInfo(email.Id);
                        if (info == null)
                        {
                            removeEmailList.Add(email);
                        }
                        else
                        {
                            if (info.IsDelete)
                            {
                                removeEmailList.Add(email);
                            }
                        }
                    }
                }
            }

            if (removeEmailList.Count > 0)
            {
                foreach (var email in removeEmailList)
                {
                    //检查是否删除邮件
                    if (CheckDeleteEmail(email))
                    {
                        UpdateEmail(email.Uid, email.DataBase, (int)EmailUpdateType.Delete);
                    }
                    else
                    {
                        emailList.Remove(email);
                    }
                }
            }

            int page = 1;
            if (emailList.Count > CONST.EMAIL_MAX_COUNT)
            {
                int tempNum = 0;
                for (int i = 0; i < emailList.Count; i++)
                {
                    if (tempNum == 0)
                    {
                        msg = new MSG_ZGC_EMAIL_OPENE_BOX();
                    }
                    if (i == emailList.Count - 1)
                    {
                        msg.IsEnd = true;
                    }
                    EmailItem email = emailList[i];
                    tempNum++;
                    msg.List.Add(GetEmailInfo(email, language));
                    if (tempNum == CONST.EMAIL_MAX_COUNT)
                    {
                        msg.Page = page;
                        if (tempNum >= CONST.EMAIL_MAX_COUNT)
                        {

                        }
                        Write(msg);
                        tempNum = 0;
                        page++;
                    }
                }
                if (tempNum > 0)
                {
                    msg.Page = page;
                    msg.IsEnd = true;
                    Write(msg);
                }
            }
            else
            {
                msg.Page = page;
                msg.IsEnd = true;
                foreach (var email in emailList)
                {
                    msg.List.Add(GetEmailInfo(email, language));
                }
                Write(msg);
            }

        }

        private MSG_ZGC_EMAIL_ITEM GetEmailInfo(EmailItem email, int language)
        {
            MSG_ZGC_EMAIL_ITEM info = new MSG_ZGC_EMAIL_ITEM();
            List<string> titleArry = GetEmailLanguageMsg(email.Title);
            List<string> fromNameArry = GetEmailLanguageMsg(email.FromName);
            switch ((LocalizationType)language)
            {
                case LocalizationType.CN:
                    if (titleArry.Count > 0)
                    {
                        info.Title = titleArry[0];
                        info.From = fromNameArry[0];
                    }
                    else
                    {
                        info.Title = email.Title;
                        info.From = email.FromName;
                    }
                    break;
                case LocalizationType.HK:
                    if (titleArry.Count > 1)
                    {
                        info.Title = titleArry[1];
                        info.From = fromNameArry[1];
                    }
                    else
                    {
                        info.Title = email.Title;
                        info.From = email.FromName;
                    }
                    break;
                case LocalizationType.TH:
                    if (titleArry.Count > 3)
                    {
                        info.Title = titleArry[3];
                        info.From = fromNameArry[03];
                    }
                    else
                    {
                        info.Title = email.Title;
                        info.From = email.FromName;
                    }
                    break;
                case LocalizationType.EN:
                default:
                    if (titleArry.Count > 2)
                    {
                        info.Title = titleArry[2];
                        info.From = fromNameArry[2];
                    }
                    else
                    {
                        info.Title = email.Title;
                        info.From = email.FromName;
                    }
                    break;
            }
            
            info.UidHigh = email.Uid.GetHigh();
            info.UidLow = email.Uid.GetLow();
            info.Id = email.Id;
            info.SendTime = email.SendTime;
            info.Type = email.Type;
            info.IsRead = (int)email.IsRead;
            info.IsGet = email.IsGet;
            //info.From = email.FromName;
            //info.Title = email.Title;
            if (email.DeleteTime > 0)
            {
                info.DeleteTime = email.DeleteTime;
            }
            else
            {
                info.DeleteTime = GetEmailDeleteTime(email.SendTime, EmailLibrary.DeleteOverTime);
            }

            if (!string.IsNullOrEmpty(email.Rewards))
            {
                List<ItemBasicInfo> rewards = RewardDropLibrary.GetSimpleRewards(email.Rewards);
                foreach (var item in rewards)
                {
                    REWARD_ITEM_INFO rewardItem = GetRewardItemInfo(item);
                    info.Rewards.Add(rewardItem);
                }
            }

            info.Param = email.Param;
            return info;
        }

        private List<string> GetEmailLanguageMsg(string info)
        {
            List<string> getTitle = new List<string>();
            if (string.IsNullOrEmpty(info))
            {
                getTitle.Add("");
            }
            else
            {
                string[] list = StringSplit.GetArray("||", info);
                getTitle = list.ToList();
            }
            return getTitle;
        }

        public void SyncNewEmail()
        {
            MSG_ZGC_EMAIL_REMIND msg = new MSG_ZGC_EMAIL_REMIND();
            Write(msg);
        }

        private void SyncEmailBody(ulong uid, int id, int time, string body, int language)
        {
            MSG_ZGC_EMAIL_READ msg = new MSG_ZGC_EMAIL_READ();
            List<string> bodyArry = GetEmailLanguageMsg(body);
            switch ((LocalizationType)language)
            {
                case LocalizationType.CN:
                    if (bodyArry.Count > 0)
                    {
                        msg.Body = bodyArry[0];
                    }
                    else
                    {
                        msg.Body = body;
                    }
                    break;
                case LocalizationType.HK:
                    if (bodyArry.Count > 1)
                    {
                        msg.Body = bodyArry[1];
                    }
                    else
                    {
                        msg.Body = body;
                    }
                    break;
                case LocalizationType.TH:
                    if (bodyArry.Count > 3)
                    {
                        msg.Body = bodyArry[3];
                    }
                    else
                    {
                        msg.Body = body;
                    }
                    break;
                case LocalizationType.EN:
                default:
                    if (bodyArry.Count > 2)
                    {
                        msg.Body = bodyArry[2];
                    }
                    else
                    {
                        msg.Body = body;
                    }
                    break;
            }
           
            msg.UidHigh = uid.GetHigh();
            msg.UidLow = uid.GetLow();
            msg.Id = id;
            msg.SendTime = time;
            Write(msg);
        }

        private void SyncEmailAttachment(EmailItem email, RewardManager rewardMng, bool spaceCheck)
        {
            MSG_ZGC_PICKUP_ATTACHMENT msg = new MSG_ZGC_PICKUP_ATTACHMENT();
            //if (CheckEmailTimeout(email))
            //{
            //    Log.Warn("player {0} get email attachment failed, email is out of date", Uid);
            //    msg.Result = (int)ErrorCode.ItemExpiration;
            //    Write(msg);
            //    return;
            //}

            if (string.IsNullOrEmpty(email.Rewards))
            {
                Log.Warn("player {0} get email attachment failed, rewards is empty", Uid);
                msg.Result = (int)ErrorCode.NotExist;
                Write(msg);
                return;
            }

            if (!spaceCheck)
            {
                int bagSpace = BagManager.GetBagRestSpace();
                int petEggBagSpace = PetManager.GetPetEggBagRestSpace();
                Log.Warn("player {0} get email {1} attachment failed, restBagSpace {2} petEggBagSpace {3} is not enough", Uid, email.Id, bagSpace, petEggBagSpace);
                msg.Result = (int)ErrorCode.BagSpaceNotEnough;
                Write(msg);
                return;
            }

            List<ItemBasicInfo> allRewards = RewardDropLibrary.GetSimpleRewards(email.Rewards);
            if (rewardMng != null)
            {
                rewardMng.GenerateRewardItemInfo(msg.Rewards);
            }

            msg.Result = (int)ErrorCode.Success;
            Write(msg);
        }

        public void SendEmailTransfrom()
        {
            List<EmailItem> emailItemList = EmailMng.GetEmailList();
            if (emailItemList.Count > CONST.EMAIL_MAX_COUNT)
            {
                int tempNum = 0;
                int totalNum = 0;
                MSG_ZMZ_EMAIL_INFO emailMsg = new MSG_ZMZ_EMAIL_INFO();
                foreach (var item in emailItemList)
                {
                    if (tempNum == 0)
                    {
                        emailMsg = new MSG_ZMZ_EMAIL_INFO();
                    }
                    emailMsg.EmailList.Add(GetEmailTransform(item));
                    tempNum++;
                    totalNum++;
                    if (totalNum == emailItemList.Count)
                    {
                        emailMsg.IsEnd = true;
                    }
                    if (tempNum == CONST.EMAIL_MAX_COUNT)
                    {
                        server.ManagerServer.Write(emailMsg, Uid);
                        tempNum = 0;
                    }
                }
                if (tempNum > 0)
                {
                    server.ManagerServer.Write(emailMsg, Uid);
                }
            }
            else
            {
                MSG_ZMZ_EMAIL_INFO emailMsg = new MSG_ZMZ_EMAIL_INFO();
                emailMsg.IsEnd = true;
                foreach (var task in emailItemList)
                {
                    emailMsg.EmailList.Add(GetEmailTransform(task));
                }
                server.ManagerServer.Write(emailMsg, Uid);
            }
        }
        private ZMZ_EMAIL_ITEM GetEmailTransform(EmailItem item)
        {
            ZMZ_EMAIL_ITEM info = new ZMZ_EMAIL_ITEM();
            info.Uid = item.Uid;
            info.Id = item.Id;
            info.Type = item.Type;
            info.IsRead = (int)item.IsRead;
            info.IsGet = item.IsGet;
            info.FromName = item.FromName;
            info.Title = item.Title;
            info.Body = item.Body;
            info.Rewards = item.Rewards;
            info.DeleteTime = item.DeleteTime;
            info.SendTime = item.SendTime;
            info.Param = item.Param;
            info.Database = (int)item.DataBase;
            return info;
        }

        public void LoadEmailTransform(RepeatedField<ZMZ_EMAIL_ITEM> infoList)
        {
            List<EmailItem> emailItemList = new List<EmailItem>();
            foreach (var item in infoList)
            {
                EmailItem info = new EmailItem();
                info.Uid = item.Uid;
                info.Id = item.Id;
                info.Type = item.Type;
                info.IsRead = (EmailReadType)item.IsRead;
                info.IsGet = item.IsGet;
                info.FromName = item.FromName;
                info.Title = item.Title;
                info.Body = item.Body;
                info.Rewards = item.Rewards;
                info.DeleteTime = item.DeleteTime;
                info.SendTime = item.SendTime;
                info.Param = item.Param;
                info.DataBase = (EmailDbTable)item.Database;
                emailItemList.Add(info);
            }
            BindEmailItems(emailItemList);
        }

        private bool CheckBagRestSpace(string rewards)
        {
            if (string.IsNullOrEmpty(rewards))
            {
                return false;
            }

            int bagSpace = BagManager.GetBagRestSpace();

            if (bagSpace <= 0)
            {
                return false;
            }

            if (rewards.Contains("|"))
            {
                string[] rewardsArray = rewards.Split('|');
                if (bagSpace < rewardsArray.Count())
                {
                    return false;
                }
                return true;
            }

            return true;
        }

        //获取奖励信息
        private REWARD_ITEM_INFO GetRewardItemInfo(ItemBasicInfo info)
        {
            REWARD_ITEM_INFO item = new REWARD_ITEM_INFO();
            item.MainType = info.RewardType;
            item.TypeId = info.Id;
            item.Num = info.Num;
            if (info.Attrs != null && info.Attrs.Count > 0)
            {
                foreach (var attr in info.Attrs)
                {
                    item.Param.Add(attr);
                }
            }
            return item;
        }


        //public void ClearTempEmailId(int type)
        //{
        //    switch ((EmailSaveType)type)
        //    {
        //        case EmailSaveType.CampBattleScore:
        //            //记录已经发送奖励
        //            //server.GameDBPool.Call(new QueryClearCampBattleScoreReward(Uid));
        //            break;
        //        case EmailSaveType.CampBattleCollection:
        //            //记录已经发送奖励
        //            //server.GameDBPool.Call(new QueryClearCampBattleCollectionReward(Uid));
        //            break;
        //        case EmailSaveType.CampBattleFight:
        //            //记录已经发送奖励
        //            //server.GameDBPool.Call(new QueryClearCampBattleFightReward(Uid));
        //            break;
        //        default:
        //            break;
        //    }
        //}
    }
}
