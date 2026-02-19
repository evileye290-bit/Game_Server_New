using CommonUtility;
using EnumerateUtility;
using Google.Protobuf.Collections;
using Message.Zone.Protocol.ZM;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib.Email
{
    public class EmailManager
    {
        //private string systemEmailIds = string.Empty;

        //public string SystemEmailUids
        //{
        //    get { return systemEmailUids; }
        //    set { systemEmailUids = value; }
        //}

        private List<EmailItem> emailItemList = new List<EmailItem>();

        //public void BindEmailItemList(List<EmailItem> emailItemList)
        //{
        //    this.emailItemList = emailItemList;
        //}

        public List<EmailItem> GetEmailList()
        {
            return emailItemList;
        }


        public void AddCustomEmailItem(EmailItem emailItem)
        {
            emailItem.DataBase = EmailDbTable.Item;
            emailItem.Type = (int)EmailType.Custom;
            emailItemList.Add(emailItem);
        }
        public void AddPersonEmailItem(EmailInfo info, EmailItem emailItem)
        {
            emailItem.DataBase = EmailDbTable.Item;
            emailItem.Type = info.Type;
            if (string.IsNullOrEmpty(emailItem.Title))
            {
                emailItem.Title = info.Title;
            }
            if (string.IsNullOrEmpty(emailItem.Body))
            {
                emailItem.Body = info.Body;
            }
            if (string.IsNullOrEmpty(emailItem.FromName))
            {
                emailItem.FromName = info.FromName;
            }
            emailItemList.Add(emailItem);

        }
        public void AddSystemEmailItem(EmailInfo info, int isRead, int sendTime, int deleteTime, int isGet)
        {
            EmailItem emailItem = new EmailItem();
            emailItem.DataBase = EmailDbTable.System;
            emailItem.Id = info.Id;
            emailItem.Type = info.Type;
            emailItem.Title = info.Title;
            emailItem.Body = info.Body;
            emailItem.FromName = info.FromName;
            emailItem.Rewards = info.Rewards;

            emailItem.IsRead = (EmailReadType)isRead;
            emailItem.IsGet = isGet;
            emailItem.SendTime = sendTime;
            emailItem.DeleteTime = deleteTime;
            emailItemList.Add(emailItem);

        }

        public EmailItem GetEmailItem(ulong uid, int id, int time)
        {
            foreach (var item in emailItemList)
            {
                if (uid > 0)
                {
                    if (item.Uid == uid)
                    {
                        return item;
                    }
                }
                else
                {
                    if (item.Id == id && item.SendTime == time)
                    {
                        return item;
                    }
                }
            }
            return null;
        }

        public EmailItem GetEmailItemForId(int id, int time)
        {
            if (id == 0)
            {
                return null;
            }
            foreach (var item in emailItemList)
            {
                if (item.Id == id && item.SendTime == time)
                {
                    return item;
                }
            }
            return null;
        }

        public List<EmailItem> GetEmailItemForType(EmailType type)
        {
            int checkType = (int)type;
            List<EmailItem> list = new List<EmailItem>();
            foreach (var item in emailItemList)
            {
                if (item.Type == checkType)
                {
                    list.Add(item);
                }
            }
            return list;
        }

        public bool CheckHasEmailItemByType(EmailType type)
        {
            List<EmailItem> list = GetEmailItemForType(type);
            if (list.Count >0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void DeleteEmail(ulong uid, int id, int time)
        {
            int removeIndex = -1;
            for (int i = 0; i < emailItemList.Count; i++)
            {
                EmailItem item = emailItemList[i];
                if (uid > 0)
                {
                    if (item.Uid == uid)
                    {
                        removeIndex = i;
                        break;
                    }
                }
                else
                {
                    if (item.Id == id && item.SendTime == time)
                    {
                        removeIndex = i;
                        break;
                    }
                }
            }
            if (removeIndex != -1)
            {
                emailItemList.RemoveAt(removeIndex);
            }
        }
    }
}
