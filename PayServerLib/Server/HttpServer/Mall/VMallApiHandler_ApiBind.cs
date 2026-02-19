using System;
using System.Collections.Generic;

namespace PayServerLib
{
    public partial class VMallApiHandler
    {
        private Dictionary<string, Action<VMallSession>> vmallCallBack = new Dictionary<string, Action<VMallSession>>();

        private static List<string> keysList = new List<string>();

        public static bool CheckApiName(string apiName)
        {
            if (keysList.Contains(apiName))
            {
                return true;
            }

            return false;
        }


        public VMallApiHandler()
        {
        }

        private void BindListener()
        {
            AddActionHandler("queryServerList", QueryServerList);
            //AddActionHandler("queryRoleInfo", QueryRoleInfo);
            AddActionHandler("recharge", Recharge);
        }

        private void AddActionHandler(string key,Action<VMallSession> action)
        {
            if (!keysList.Contains(key))
            {
                keysList.Add(key);
                vmallCallBack.Add(key, action);
            }
        }

    }
}