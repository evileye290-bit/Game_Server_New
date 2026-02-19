using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Logger;
using Message.Relation.Protocol.RZ;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelationServerLib
{
    public class EmailManager
    {
        private RelationServerApi server { get; set; }
        public EmailManager(RelationServerApi server)
        {
            this.server = server;
        }

        public void GmSendEmail(int emailId, int saveTime, int mainID, string sqlConditions)
        {
            EmailInfo email = EmailLibrary.GetEmailInfo(emailId);
            if (email != null)
            {
                int sendTime = Timestamp.GetUnixTimeStampSeconds(RelationServerApi.now);

                //int deleteTime = GetEmailDeleteTime(saveTime);
                int deleteTime = GetEmailDeleteTime(email.ValidityDate);

                int isGet = GetEmailIsGet(email.Rewards, email.TaskId);

                //选出任务UID
                //string characterTableName = "character";

                QueryGetSendEmailUids query = new QueryGetSendEmailUids(sqlConditions);
                server.GameDBPool.Call(query, (ret) =>
                {
                    if (query.PcUids.Count > 0)
                    {
                        Log.Write("GmSendEmail sen email count:{0}", query.PcUids.Count);

                        List<int> uidList = new List<int>();
                        string uids = "";
                        int i = 0;
                        int querySize = 500;
                        foreach (var uid in query.PcUids)
                        {
                            // 每次搜索500条  
                            if (i % querySize == 0)
                            {
                                uidList.Add(uid);
                                uids = uid.ToString();
                            }
                            else
                            {
                                uidList.Add(uid);
                                uids += "," + uid.ToString();
                            }
                            i++;
                            if (i % querySize == 0 || i == query.PcUids.Count)
                            {

                                //string tableName = "email";
                                server.GameDBPool.Call(new QueryGmSqlEmailSend(uids, emailId, sendTime, deleteTime, isGet));

                                if (uidList.Count > 0)
                                {
                                    MSG_RZ_SEND_EMAILS msg = GetSendEmailMsg(0, email.Id, (int)EmailType.System, sendTime, deleteTime, isGet, "", "", "", "", uidList);
                                    server.ZoneManager.Broadcast(msg);
                                }

                                //MSG_RZ_SEND_EMAILS msg = new MSG_RZ_SEND_EMAILS();
                                //MSG_RZ_EMAIL_ITEM emailItem = new MSG_RZ_EMAIL_ITEM();
                                //emailItem.Uid = 0;
                                //emailItem.Id = email.Id;
                                //emailItem.Type = email.Type;
                                //emailItem.SendTime = sendTime;
                                //emailItem.DeleteTime = deleteTime;
                                //emailItem.IsGet = isGet;
                                //emailItem.Title = email.Title;
                                //emailItem.Body = email.Body;
                                //emailItem.FromName = email.FromName;
                                //emailItem.Rewards = email.Rewards;
                                //emailItem.PcUids.AddRange(uidList);
                                ////emailItem.PcUids.AddRange(query.PcUids);
                                //msg.Emails.Add(emailItem);
                                //AddSendEmailMsg(mainID, msg);
                            }
                        }
                    }
                    else
                    {
                        Log.Warn("GmSendEmail not find email sqlConditions:{0}", sqlConditions);
                    }
                });
                Log.Write("GM send email {0} save {1} days main id {2} sqlConditions {3}", emailId, email.ValidityDate, mainID, sqlConditions);
            }
            else
            {
                Log.Warn("GmSendEmail not find email id:{0}", emailId);
            }
        }

        public void SendCustomEmail(int pcUid, string title, string fromName, string text, string rewards = "", int saveTime = 0, string param = "")
        {
            //邮件ID自增
            ulong emailUid = server.UID.NewEuid(server.MainId, server.MainId);

            int sendTime = Timestamp.GetUnixTimeStampSeconds(RelationServerApi.now);

            int deleteTime = GetEmailDeleteTime(saveTime);

            int isGet = GetEmailIsGet(rewards, 0);
            //增加邮件信息
            //string tableName = "email_items";
            server.GameDBPool.Call(new QueryInsertEmailItem(pcUid, emailUid, 0, isGet, sendTime, deleteTime, title, fromName, text, rewards, param));

            int emailType = (int)EmailType.Custom;

            if (pcUid > 0)
            {
                MSG_RZ_SEND_EMAILS msg = GetSendEmailMsg(emailUid, 0, emailType, sendTime, deleteTime, isGet, title, fromName, text, rewards, new List<int>() { pcUid }, param);

                Client client = server.ZoneManager.GetClient(pcUid);
                if (client != null)
                {
                    client.CurZone.Write(msg);
                }
                else
                {
                    //Log.Warn("player {0} try get client failed on send custom email", pcUid);
                    server.ZoneManager.Broadcast(msg);
                }
            }
            server.TrackingLoggerMng.RecordSendEmailRewardLog(pcUid, 0, rewards, param, server.MainId, server.Now());
        }

        public void SendSystemEmailAll(int emailId, int saveTime = 0)
        {
            EmailInfo email = EmailLibrary.GetEmailInfo(emailId);
            if (email != null)
            {
                int sendTime = Timestamp.GetUnixTimeStampSeconds(RelationServerApi.now);

                //int deleteTime = GetEmailDeleteTime(saveTime);
                int deleteTime = GetEmailDeleteTime(email.ValidityDate);

                int isGet = GetEmailIsGet(email.Rewards, email.TaskId);

                //for (int i = 0; i < 20; i++)
                //{
                    //string tableName = "email";
                    server.GameDBPool.Call(new QuerySendSystemEmail(emailId, sendTime, deleteTime, isGet));
                //}

                MSG_RZ_SEND_EMAILS msg = GetSendEmailMsg(0, email.Id, email.Type, sendTime, deleteTime, isGet, email.Title, email.FromName, email.Body, email.Rewards, null);
                server.ZoneManager.Broadcast(msg);
            }
            else
            {
                Log.Warn("gm send system email not find email id:{0}", emailId);
            }

            server.TrackingLoggerMng.RecordSendEmailRewardLog(0, emailId, "", "", server.MainId, server.Now());
        }

        public void SendPersonEmail(int pcUid, int emailId, string reward = "", int saveTime = 0, string param = "")
        {
            EmailInfo email = EmailLibrary.GetEmailInfo(emailId);
            if (email != null)
            {
                SendPersonEmail(pcUid, email, email.Body, reward, saveTime, param);
            }
            else
            {
                Log.Warn("gm send email not find email id:{0}", emailId);
            }
        }

        public void SendPersonEmail(int pcUid, EmailInfo email, string body, string reward = "", int saveTime = 0, string param = "")
        {

            ulong emailUid = server.UID.NewEuid(server.MainId, server.MainId);

            int sendTime = Timestamp.GetUnixTimeStampSeconds(RelationServerApi.now);

            //int deleteTime = GetEmailDeleteTime(saveTime);
            int deleteTime = GetEmailDeleteTime(email.ValidityDate);

            string newReward = reward;
            if (string.IsNullOrEmpty(newReward))
            {
                newReward = email.Rewards;
            }

            int isGet = GetEmailIsGet(newReward, email.TaskId);

            //增加邮件信息
            //string tableName = "email_items";
            server.GameDBPool.Call(new QueryInsertEmailItem(pcUid, emailUid, email.Id, isGet, sendTime, deleteTime, "", "", body, newReward, param));

            int emailType = (int)EmailType.Person;

            if (pcUid > 0)
            {
                MSG_RZ_SEND_EMAILS msg = GetSendEmailMsg(emailUid, email.Id, emailType, sendTime, deleteTime, isGet,
                email.Title, email.FromName, body, newReward, new List<int>() { pcUid }, param);

                Client client = server.ZoneManager.GetClient(pcUid);
                if (client != null)
                {
                    client.CurZone.Write(msg);
                }
                else
                {
                    //Log.Warn("player {0} try get client failed on send person email", pcUid);
                    server.ZoneManager.Broadcast(msg);
                }
            }

            server.TrackingLoggerMng.RecordSendEmailRewardLog(pcUid, email.Id, newReward, param, server.MainId, server.Now());
        }

        //private void BroadCastEmailList(ulong uid, int id, int type, int sendTime, int deleteTime, int isGet,
        //    string title, string fromName, string text, string rewards, List<int> pcUids)
        //{
        //    MSG_RZ_SEND_EMAILS msg = GetSendEmailMsg(uid, id, type, sendTime, deleteTime, isGet, title, fromName, text, rewards, pcUids);
        //    ZoneManagerBroadCast(msg);
        //}

        private static MSG_RZ_SEND_EMAILS GetSendEmailMsg(ulong uid, int id, int type, int sendTime, int deleteTime,
            int isGet, string title, string fromName, string text, string rewards, List<int> pcUids, string param = "")
        {
            MSG_RZ_SEND_EMAILS msg = new MSG_RZ_SEND_EMAILS();
            MSG_RZ_EMAIL_ITEM emailItem = new MSG_RZ_EMAIL_ITEM();
            emailItem.Uid = uid;
            emailItem.Id = id;
            emailItem.Type = type;
            emailItem.SendTime = sendTime;
            emailItem.DeleteTime = deleteTime;
            emailItem.IsGet = isGet;
            emailItem.Title = title;
            emailItem.Body = text;
            emailItem.FromName = fromName;
            emailItem.Rewards = rewards;
            if (pcUids != null)
            {
                emailItem.PcUids.AddRange(pcUids);
            }
            emailItem.Param = param;
            msg.Emails.Add(emailItem);
            return msg;
        }

        private int GetEmailDeleteTime(int saveTime)
        {
            int deleteTime = 0;
            if (saveTime != 0)
            {
                deleteTime = Timestamp.GetUnixTimeStampSeconds(RelationServerApi.now.AddDays(saveTime));
            }
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
    }
}
