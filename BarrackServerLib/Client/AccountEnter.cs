using Message.Client.Protocol.CBarrack;
using System;
using System.Collections.Generic;

namespace BarrackServerLib
{
    public class AccountEnter
    {
        private bool IsIOS;
        private Client client;
        private bool success;
        private MSG_CB_USER_LOGIN pks;
        private Dictionary<string, object> ans;

        public AccountEnter(Client client, MSG_CB_USER_LOGIN pks, bool success, bool isIos, Dictionary<string, object> ans = null)
        {
            this.client = client;
            this.pks = pks;
            this.success = success;
            this.ans = ans;
            IsIOS = isIos;
        }

        public void DoLogin()
        {
            if (this.client != null && this.pks != null)
            {
                try
                {
                    if (!IsIOS)
                    {
                        client.DoLogin(pks, success);
                    }
                    else
                    {
                        client.IOSDoLogin(pks, success);
                    }
                }
                catch (Exception e)
                {
                    Logger.Log.Warn("some dologin with e {0}", e.ToString());
                }
            }
        }
    }

    public class AccountReEnter
    {
        public string AccountName; // 账号名
        public DateTime Timestamp = DateTime.MinValue; // 有效期
        public string Token;
        public AccountReEnter(string account_name, string token)
        {
            AccountName = account_name;
            Token = token;
            Timestamp = BarrackServerApi.now.AddMinutes(5);
        }
    }
}
