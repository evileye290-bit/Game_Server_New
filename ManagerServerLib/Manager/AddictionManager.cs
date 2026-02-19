using DataProperty;
using Logger;
using Message.Barrack.Protocol.BM;
using Message.Manager.Protocol.MB;
using Message.Manager.Protocol.MZ;
using Message.Zone.Protocol.ZM;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ManagerServerLib
{
    public class AddictionManager
    {
        public static Dictionary<string, int> AccountIdTodayOnlineTime = new Dictionary<string, int>();//只记录今天登录过的未成年人
        public static Dictionary<string, int> AccountIdUnderAgeOnline = new Dictionary<string, int>();//当前在线的未成年人
        //public static Dictionary<int, string> AccountUidName = new Dictionary<int, string>();// 登录时更新 用于查找accountId
        public static Dictionary<string, int> AccountIdCharUid = new Dictionary<string, int>();//登录时更新，用于查找uid

        DateTime last = DateTime.Now;
        TimeSpan span = new TimeSpan(0, 1, 0);

        #region config

        private TimeSpan beginTime = new TimeSpan(1, 0, 0);
        private TimeSpan endTime = new TimeSpan(1, 0, 0);
        private HashSet<DateTime> holidays = new HashSet<DateTime>();
        private double holidayLimit = 3 * 3600;
        private double noneHolidayLimit = 1.5 * 3600;


        #endregion

        private ManagerServerApi server = null;
        public AddictionManager(ManagerServerApi server)
        {
            this.server = server;
            Log.Info("AddictionManager Started");
        }

        DateTime refreshTime = DateTime.Now - new TimeSpan(24, 0, 0);

        public void Refresh()
        {
            if (ManagerServerApi.now.Date != refreshTime.Date)
            {
                //更新防沉迷信息
                refreshTime = ManagerServerApi.now;
                GetConfig();
                AccountIdTodayOnlineTime = new Dictionary<string, int>();
                AccountIdUnderAgeOnline = new Dictionary<string, int>();
                //AccountUidName = new Dictionary<int, string>();
            }
        }


        public void GetConfig()
        {
            DataList holidayConfig = DataListManager.inst.GetDataList("Holiday");
            DataList additionConfig = DataListManager.inst.GetDataList("AddictionType");

            holidays = new HashSet<DateTime>();
            foreach (var data in holidayConfig)
            {
                holidays.Add(DateTime.Parse(data.Value.GetString("date")));
            }
            foreach (var data in additionConfig)
            {
                if (data.Value.Name.Equals("everyday"))
                {
                    string param = data.Value.GetString("param");
                    int beginTime = int.Parse(param.Split('-')[0]);
                    int endTime = int.Parse(param.Split('-')[1]);

                    this.beginTime = new TimeSpan(beginTime, 0, 0);
                    this.endTime = new TimeSpan(endTime, 0, 0);
                }
                else if (data.Value.Name.Equals("holiday"))
                {
                    holidayLimit = double.Parse(data.Value.GetString("param")) * 3600;
                }
                else if (data.Value.Name.Equals("none_holiday"))
                {
                    noneHolidayLimit = double.Parse(data.Value.GetString("param")) * 3600;
                }
            }
        }


        public void Update()
        {
            if(!DataListManager.inst.GetData("AuthConfig", 1).GetBoolean("AntiAddiction"))
            {
                return;
            }
            if (ManagerServerApi.now > last.Add(span))
            {
                //只在此处更新在线时间
                int tempSpan = (int)(ManagerServerApi.now - last).TotalSeconds;
                last = last.Add(span);
                foreach (var kv in AccountIdUnderAgeOnline)
                {
                    AccountIdTodayOnlineTime[kv.Key] += tempSpan;
                    int timeCount = AccountIdTodayOnlineTime[kv.Key];
                    int kickAns = CheckKickOut(timeCount);
                    if (kickAns > 0)
                    {
                        //Kick
                        Kick(AccountIdCharUid[kv.Key], timeCount);
                        NotifyBarrackKickOrLogOut(kv.Key, kickAns, timeCount);
                    }
                }
            }

            Refresh();
        }

        public int CheckKickOut(int timeCount)
        {
            if (!CheckTime())
            {
                return 2;
            }
            if (TodayIsHoliday())
            {
                if (timeCount >= holidayLimit)
                {
                    return 3;
                }
            }
            else
            {
                if (timeCount >= noneHolidayLimit)
                {
                    return 4;
                }
            }
            return -1;
        }

        public void Kick(int uid, int timecount)
        {
            ZoneServerManager zoneManager = server.ZoneServerManager;

            MSG_MZ_ADDICTION_KICK_PLAYER request = new MSG_MZ_ADDICTION_KICK_PLAYER();
            request.Uid = uid;
            foreach (var zone in zoneManager.ServerList)
            {
                zone.Value.Write(request);
            }
            zoneManager.RemoveOfflineClient(uid);

            int mainId = server.ZoneServerManager.ServerList.First().Key.Split('_')[0].ToInt() ;
            Log.Write("addictionManager main {0} kick player {1} with {2} seconds", mainId, uid, timecount);
        }

        public void NotifyBarrackKickOrLogOut(string accountId, int addictionType, int serverTime)
        {
            MSG_MB_NOTIFY_LOGOUT msg = new MSG_MB_NOTIFY_LOGOUT();
            msg.AccountId = accountId;
            msg.AddictionType = addictionType;
            msg.ServerTime = serverTime;

            foreach (var bserver in server.BarrackServerManager.ServerList)
            {
                bserver.Value.Write(msg);
                break;
            }
        }

        public bool CheckTime()
        {
            if (ManagerServerApi.now.TimeOfDay > endTime && ManagerServerApi.now.TimeOfDay < beginTime)
            {
                return true;
            }
            return false;
        }

        public bool TodayIsHoliday()
        {
            foreach (var day in holidays)
            {
                if (ManagerServerApi.now.DayOfYear == day.DayOfYear)
                {
                    return true;
                }
            }
            return false;
        }

        public void SetUnderAgeOnline(Client client, bool online)
        {
            //enterWorld leaveWorld
            
            AccountIdCharUid[client.AccountId] = client.CharacterUid;
            if (!AccountIdUnderAgeOnline.ContainsKey(client.AccountId))
            {
                //AccountIdUnderAgeOnline[client.AccountId] = 0;
                //AccountIdTodayOnlineTime[client.AccountId] = 0;
                Log.Write($"set {client.AccountId} online {online} find no info");
                return;
            }

            if (online)
            {
                AccountIdUnderAgeOnline[client.AccountId] += 1;
            }
            else
            {
                AccountIdUnderAgeOnline[client.AccountId] -= 1;
            }
        }

        public void AddUnderAgeAccountId(MSG_BM_NOTIFY_ADDICTION_INFO info)
        {
            //登录时barrack 发过来
            string accountId = info.AccountId;
            int accountUid = info.AccountUid;
            int serverTime = info.ServerTime;
            //AccountUidName[accountUid] = accountId;
            Log.Write($"MSG_BM_NOTIFY_ADDICTION_INFO with accountId {info.AccountId} removeUnderAge {info.RemoveUnderAge}");
            if (info.RemoveUnderAge)
            {
                AccountIdUnderAgeOnline.Remove(accountId);
                return;
            }
            if (!AccountIdUnderAgeOnline.ContainsKey(accountId))
            {
                AccountIdUnderAgeOnline[accountId] = 0;
                AccountIdTodayOnlineTime[accountId] = serverTime;
            }
            else
            {
                int temp = AccountIdTodayOnlineTime[accountId];
                AccountIdTodayOnlineTime[accountId] = temp > serverTime ? temp : serverTime;//考虑信息先后可能出错，以最大为准
            }

        }

        internal void Logout(MSG_ZM_LOGOUT msg)
        {
            //仅仅是通知barrack 在线维护仍然在别处

            int timeCount = 0;
            //string name = "";
            if (AccountIdTodayOnlineTime.TryGetValue(msg.AccountId, out timeCount))
            {
                NotifyBarrackKickOrLogOut(msg.AccountId, -1, timeCount);
            }
        }
    }
}

