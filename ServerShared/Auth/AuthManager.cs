using DataProperty;
using Logger;
using System;
using System.Collections.Generic;

namespace ServerShared
{
    public class AuthManager
    {
        private bool whiteOnly;
        private bool checkVersion;
        public string Version
        { get { return string.Join("|", versionList); } }

        private bool checkSdkToken;
        public bool CheckSdkToken
        { get { return checkSdkToken; } }

        private bool antiAddiction;
        public bool AntiAddiction
        { get { return antiAddiction; } }

        private bool checkBarrackToken;
        public bool CheckBarrackToken
        { get { return checkBarrackToken; } }

        // key mainId
        Dictionary<int, HashSet<string>> whiteList;
        //private HashSet<string> whiteList;
        private HashSet<string> versionList;
        public AuthManager()
        {
            whiteOnly = false;
            checkVersion = false;
            checkSdkToken = false;
            whiteList = new Dictionary<int, HashSet<string>>();
            versionList = new HashSet<string>();
        }

        public void Init()
        {
            whiteList.Clear();
            versionList.Clear();
            Data authConfig = DataListManager.inst.GetData("AuthConfig", 1);
            whiteOnly = authConfig.GetBoolean("WhiteOnly");
            checkVersion = authConfig.GetBoolean("CheckVersion");
            checkSdkToken = authConfig.GetBoolean("CheckSdkToken");
            checkBarrackToken = authConfig.GetBoolean("CheckBarrackToken");
            antiAddiction = authConfig.GetBoolean("AntiAddiction");

            DataList whiteDataList = DataListManager.inst.GetDataList("WhiteList");
            foreach (var data in whiteDataList.AllData)
            {
                try
                {
                    // 防止手抖添加重复ip
                    string ip = data.Value.GetString("IP");
                    string[] mainArr = data.Value.GetString("MainId").Split('|');
                    foreach(var mainStr in mainArr)
                    {
                        if(string.IsNullOrEmpty(mainStr))
                        {
                            continue;
                        }
                        int mainId = 0;
                        if (!int.TryParse(mainStr, out mainId))
                        {
                            continue;
                        }
                        HashSet<string> ipList = null;
                        if (!whiteList.TryGetValue(mainId, out ipList))
                        {
                            ipList = new HashSet<string>();
                            whiteList.Add(mainId, ipList);
                        }
                        if (!ipList.Contains(ip))
                        {
                            ipList.Add(ip);
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Alert(e.ToString());
                }
            }

            DataList versionDataList = DataListManager.inst.GetDataList("VersionConfig");
            foreach (var data in versionDataList.AllData)
            {
                try
                {
                    if (data.Value.GetBoolean("Valid") == true)
                    { 
                        versionList.Add(data.Value.Name);
                    }
                }
                catch (Exception e)
                {
                    Log.Alert(e.ToString());
                }
            }
        }

        public bool CheckWhite(int mainId, string ip, bool isTestAccount)
        {
            if (whiteOnly)
            {
                if (isTestAccount) return true;

                HashSet<string> ipList = null;
                if (whiteList.TryGetValue(0, out ipList))
                {
                    if (ipList.Contains(ip))
                    {
                        return true;
                    }
                }
                if(whiteList.TryGetValue(mainId, out ipList))
                {
                    if (ipList.Contains(ip))
                    {
                        return true;
                    }
                }
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool IsWhite(int mainId)
        {
            if (whiteOnly)
            {
                return whiteList.ContainsKey(mainId);
            }
            else
            {
                return false;
            }
        }

        public bool CheckVersion(string version)
        {
            if (checkVersion)
            {
                return versionList.Contains(version);
            }
            return true;
        }

        public void SetCheckWhite(bool open)
        {
            whiteOnly = open;
        }
    }
}
