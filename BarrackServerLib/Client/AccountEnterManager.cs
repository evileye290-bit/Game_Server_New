using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarrackServerLib
{
    public class AccountEnterManager
    {
        // key accountName, value AccountEnter
        //public Dictionary<string, AccountEnter> AccountEnterList = new Dictionary<string, AccountEnter>();
        public List<AccountReEnter> AccountEnterRemoveList = new List<AccountReEnter>();

        public Queue<AccountEnter> EnterList = new Queue<AccountEnter>();
        //public Queue<AccountEnter> AfterEnterList = new Queue<AccountEnter>();

        //public SortedDictionary<string, string> reEnter = new SortedDictionary<string, string>();
        public Dictionary<string, AccountReEnter> AccountReEnterList = new Dictionary<string, AccountReEnter>();

        private BarrackServerApi server;

        public AccountEnterManager(BarrackServerApi server)
        {
            this.server = server;
        }

        public void Add(AccountEnter enter)
        {
            EnterList.Enqueue(enter);
        }

        public void UpdateAccountEnter()
        {
            //Queue<AccountEnter> temp = new Queue<AccountEnter>();
            int count = EnterList.Count;
            while (count > 0)
            {
                count--;
                AccountEnter enter = EnterList.Dequeue();
                enter.DoLogin();
                //AfterEnterList.Enqueue(enter);
            }

            lock (AccountReEnterList)
            {
                foreach (var item in AccountReEnterList)
                {
                    try
                    {
                        if (item.Value.Timestamp < BarrackServerApi.now)
                        {
                            Logger.Log.Warn("account {0} auth check expired", item.Key);
                            AccountEnterRemoveList.Add(item.Value);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Log.Warn(e.ToString());
                    }
                }
                foreach (var item in AccountEnterRemoveList)
                {
                    AccountReEnterList.Remove(item.AccountName);
                }
            }
            AccountEnterRemoveList.Clear();
        }

        //public void AddAccountReEnter(string accountName, int token)
        //{
        //    reEnter.Add(accountName, token.ToString());
        //}

        //public bool CheckToken(string accountName, string token)
        //{
        //    string tempToken="";
        //    if (reEnter.TryGetValue(accountName, out tempToken))
        //    {
        //        if (!string.IsNullOrEmpty(tempToken) && tempToken.Equals(token))
        //        {
        //            return true;
        //        }
        //    }
        //    return false;
        //}

        public void AddAccountReEnter(string account_name, string token)
        {
            AccountReEnter account;
            lock (AccountReEnterList)
            {
                if (AccountReEnterList.TryGetValue(account_name, out account) == true)
                {
                    account.Timestamp = BarrackServerApi.now.AddMinutes(5);
                    account.Token = token;
                }
                else
                {
                    account = new AccountReEnter(account_name, token);
                    AccountReEnterList.Add(account_name, account);
                }
            }
        }


        public bool CheckAccountReEnter(string account_name, string token)
        {
            AccountReEnter account = null;
            lock (AccountReEnterList)
            {
                if (AccountReEnterList.TryGetValue(account_name, out account) == false)
                {
                    return false;
                }
                if (account.Timestamp < BarrackServerApi.now)
                {
                    return false;
                }
                if (account.Token != token)
                {
                    return false;
                }
                AccountReEnterList.Remove(account_name);
            }
            return true;
        }
    }

}

