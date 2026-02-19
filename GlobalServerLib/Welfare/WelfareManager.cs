using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Message.Global.Protocol.GR;
using ServerFrame;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GlobalServerLib
{
    public class WelfareManager
    {
        private GlobalServerApi server { get; set; }
        private int StallId { get; set; }
        private Dictionary<int, WelfareStallItem> StallList = new Dictionary<int, WelfareStallItem>();
        private Dictionary<int, Dictionary<int, List<WelfarePlayerItem>>> PlayerList = new Dictionary<int, Dictionary<int, List<WelfarePlayerItem>>>();
        public WelfareManager(GlobalServerApi server)
        {
            this.server = server;

            InitStallId();

            LoadStallFromDB();
        }

        public void UpdateInfo()
        {
            server.TaskTimerMng.Stop();

            LoadStallFromDB();
        }

        private void InitStallId()
        {
            QueryGetMaxWelfareStallId query = new QueryGetMaxWelfareStallId();
            server.AccountDBPool.Call(query, ret =>
            {
                if (query.Id > StallId)
                {
                    StallId = query.Id;
                }
            });
        }

        public int GetNewStallId()
        {
            return ++StallId;
        }

        private void LoadStallFromDB()
        {
            QueryLoadWelfareStalls loadQuery = new QueryLoadWelfareStalls();
            server.AccountDBPool.Call(loadQuery, ret =>
            {
                StallList = loadQuery.List.OrderBy(p => p.Key).ToDictionary(p => p.Key, o => o.Value);

                LoadPlayerFromDB();
            });
        }

        private void LoadPlayerFromDB()
        {
            QueryLoadWelfarePlayer loadQuery = new QueryLoadWelfarePlayer();
            server.AccountDBPool.Call(loadQuery, ret =>
            {
                PlayerList = loadQuery.PlayerList;

                InitTimerManager(server.Now());
            });
        }

        private Dictionary<DateTime, List<int>> GetTimingLists(DateTime now)
        {
            Dictionary<DateTime, List<int>> dic = new Dictionary<DateTime, List<int>>();

            List<int> list;
            foreach (var stall in StallList)
            {
                switch (stall.Value.Type)
                {
                    case WelfareSendType.Daily:
                        {
                            if (now.TimeOfDay <= stall.Value.Time)
                            {
                                DateTime time = now.Date.Add(stall.Value.Time);
                                if (dic.TryGetValue(time, out list))
                                {
                                    list.Add(stall.Key);
                                }
                                else
                                {
                                    list = new List<int>();
                                    list.Add(stall.Key);
                                    dic.Add(time, list);
                                }
                            }
                        }
                        break;
                    case WelfareSendType.Week:
                        {
                            int day = (int)now.DayOfWeek;
                            if (day == 0)
                            {
                                day = 7;
                            }
                            if (stall.Value.Day == day && now.TimeOfDay <= stall.Value.Time)
                            {
                                DateTime time = now.Date.Add(stall.Value.Time);
                                if (dic.TryGetValue(time, out list))
                                {
                                    list.Add(stall.Key);
                                }
                                else
                                {
                                    list = new List<int>();
                                    list.Add(stall.Key);
                                    dic.Add(time, list);
                                }
                            }
                        }
                        break;
                    case WelfareSendType.Month:
                        {
                            if (now.Day == stall.Value.Day && now.TimeOfDay <= stall.Value.Time)
                            {
                                DateTime time = now.Date.Add(stall.Value.Time);
                                if (dic.TryGetValue(time, out list))
                                {
                                    list.Add(stall.Key);
                                }
                                else
                                {
                                    list = new List<int>();
                                    list.Add(stall.Key);
                                    dic.Add(time, list);
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            if (dic.Count > 0)
            {
                dic = dic.OrderBy(p => p.Key).ToDictionary(p => p.Key, o => o.Value);
            }
            else
            {
                if (now.Date != DateTime.Now.Date)
                {
                    dic.Add(now.Date, new List<int>());
                }
            }
            return dic;
        }

        private void InitTimerManager(DateTime time)
        {
            //获取刷新任务
            Dictionary<DateTime, List<int>> taskDic = GetTimingLists(time);
            if (taskDic.Count > 0)
            {
                var kv = taskDic.First();
                double interval = (kv.Key - DateTime.Now).TotalMilliseconds;
                WelfareTimerQuery counterTimer = new WelfareTimerQuery(interval, taskDic);
                server.TaskTimerMng.Call(counterTimer, (ret) =>
                {
                    TimingRefreshByPlayers(counterTimer.TaskDic);
                });
            }
            else
            {
                //当天没有了，下一天
                InitTimerManager(time.Date.AddDays(1));
            }
        }

        private void AddTimer(WelfareSendType type, int day, TimeSpan stallTime, int id)
        {
            DateTime now = DateTime.Now;
            DateTime time = now.Date.Add(stallTime);

            switch (type)
            {
                case WelfareSendType.Daily:
                    {
                        if (now.TimeOfDay <= stallTime)
                        {
                            AddSingleTaskTiming(id, time);
                        }
                    }
                    break;
                case WelfareSendType.Week:
                    {
                        int nowDay = (int)now.DayOfWeek;
                        if (nowDay == 0)
                        {
                            nowDay = 7;
                        }
                        if (day == nowDay && now.TimeOfDay <= stallTime)
                        {
                            AddSingleTaskTiming(id, time);
                        }
                    }
                    break;
                case WelfareSendType.Month:
                    {
                        if (now.Day == day && now.TimeOfDay <= stallTime)
                        {
                            AddSingleTaskTiming(id, time);
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private void AddSingleTaskTiming(int id, DateTime time)
        {
            double interval = (time - DateTime.Now).TotalMilliseconds;
            WelfareTimerQuery counterTimer = new WelfareTimerQuery(interval, new Dictionary<DateTime, List<int>>());
            server.TaskTimerMng.Call(counterTimer, (ret) =>
            {
                DoTask(id); ;
            });
        }

        private void CallBackNextTask(DateTime time, Dictionary<DateTime, List<int>> taskDic)
        {
            if (taskDic.Count > 0)
            {
                var firstTask = taskDic.First();
                double interval = (firstTask.Key - DateTime.Now).TotalMilliseconds;
                WelfareTimerQuery counterTimer = new WelfareTimerQuery(interval, taskDic);
                server.TaskTimerMng.Call(counterTimer, (ret) =>
                {
                    TimingRefreshByPlayers(counterTimer.TaskDic);
                });
            }
            else
            {
                InitTimerManager(time.Date.AddDays(1));
            }
        }

        private void TimingRefreshByPlayers(Dictionary<DateTime, List<int>> taskDic)
        {
            var firstTask = taskDic.First();
            taskDic.Remove(firstTask.Key);
            CallBackNextTask(firstTask.Key, taskDic);

            //有刷新任务
            DoTaskList(firstTask.Value);
        }

        private void DoTaskList(List<int> firstTask)
        {
            foreach (var timingType in firstTask)
            {
                DoTask(timingType);
            }
        }

        private void DoTask(int timingType)
        {
            //server.TrackingLoggerMng.TrackTimerLog(server.MainId, "global", " Welfare" + timingType, server.Now());

            WelfareStallItem stall = GetWelfareStallItem(timingType);
            if (stall != null)
            {
                Dictionary<int, List<WelfarePlayerItem>> lsit = GetWelfarePlayerItems(timingType);
                if (lsit != null)
                {
                    foreach (var kv in lsit)
                    {
                        int mainId = kv.Key;
                        FrontendServer rServer = server.RelationServerManager.GetSinglePointServer(mainId);
                        if (rServer != null)
                        {
                            foreach (var item in kv.Value)
                            {
                                int uid = item.Uid;
                                int emailId = 0;
                                if (item.LastTime == 0)
                                {
                                    emailId = stall.FirstEmail;
                                }
                                else
                                {
                                    emailId = stall.FixedEmail;
                                }

                                MSG_GR_WELFARE_SEND_MAIL msg = new MSG_GR_WELFARE_SEND_MAIL();
                                msg.Uid = uid;
                                msg.MailId = emailId;
                                msg.ServerId = mainId;
                                rServer.Write(msg);

                                item.LastTime = Timestamp.GetUnixTimeStampSeconds(server.Now());
                                server.AccountDBPool.Call(new QueryUpdateWelfarePlayer(mainId, uid, timingType, item.LastTime));
                            }
                        }
                    }
                }
            }
        }

        private WelfareStallItem GetWelfareStallItem(int id)
        {
            WelfareStallItem item;
            StallList.TryGetValue(id, out item);
            return item;
        }

        public Dictionary<int, List<WelfarePlayerItem>> GetWelfarePlayerItems(int id)
        {
            Dictionary<int, List<WelfarePlayerItem>> item;
            PlayerList.TryGetValue(id, out item);
            return item;
        }

        public bool AddStall(WelfareStallItem model)
        {
            //model.Id = GetNewStallId();

            if (!StallList.ContainsKey(model.Id))
            {
                StallList.Add(model.Id, model);

                AddTimer(model.Type, model.Day, model.Time, model.Id);

                server.AccountDBPool.Call(new QueryInsertWelfareStallsItem(model));

                return true;
            }
            else
            {
                return false;
            }
        }

        public bool AddPlayer(WelfarePlayerItem model)
        {
            WelfareStallItem info = GetWelfareStallItem(model.Id);
            if (info != null)
            {
                model.Name = info.Name;
            }
            else
            {
                return false;
            }
            Dictionary<int, List<WelfarePlayerItem>> dic;
            List<WelfarePlayerItem> list;
            if (PlayerList.TryGetValue(model.Id, out dic))
            {
                if (dic.TryGetValue(model.ServerId, out list))
                {
                    foreach (var item in list)
                    {
                        if (item.Uid == model.Uid)
                        {
                            return false;
                        }
                    }
                    list.Add(model);
                }
                else
                {
                    list = new List<WelfarePlayerItem>();
                    list.Add(model);
                    dic.Add(model.ServerId, list);
                }
            }
            else
            {
                dic = new Dictionary<int, List<WelfarePlayerItem>>();
                list = new List<WelfarePlayerItem>();
                list.Add(model);
                dic.Add(model.ServerId, list);
                PlayerList.Add(model.Id, dic);
            }

            server.AccountDBPool.Call(new QueryInsertWelfarePlayerItem(model));
            return true;
        }

        public bool DeleteStall(int id)
        {
            server.AccountDBPool.Call(new QueryDeleteWelfareStalls(id));
            return StallList.Remove(id);
        }

        public bool DeletePlayer(int id, int serverId, int uid)
        {
            Dictionary<int, List<WelfarePlayerItem>> dic;
            List<WelfarePlayerItem> list;
            if (PlayerList.TryGetValue(id, out dic))
            {
                if (dic.TryGetValue(serverId, out list))
                {
                    foreach (var item in list)
                    {
                        if (item.Uid == uid)
                        {
                            server.AccountDBPool.Call(new QueryDeleteWelfarePlayer(serverId, uid, id));
                            return list.Remove(item);
                        }
                    }
                }
            }
            return false;
        }

        public WelfareStallInfoList FindStall(int id)
        {
            if (id == 0)
            {
                return GetWelfareStallInfoList();
            }
            else
            {
                WelfareStallInfoList list = new WelfareStallInfoList();
                WelfareStallItem item = GetWelfareStallItem(id);
                if (item != null)
                {
                    list.list.Add(GetWelfareStallInfo(item));
                }
                return list;
            }
        }

        public WelfarePlayerInfoList FindPlayer(int serverId, int uid)
        {
            if (serverId == 0 && uid == 0)
            {
                return GetWelfarePlayerInfoList();
            }
            WelfarePlayerInfoList list = new WelfarePlayerInfoList();
            foreach (var dic in PlayerList)
            {
                foreach (var kv in dic.Value)
                {
                    if (serverId > 0 && uid > 0)
                    {
                        if (kv.Key == serverId)
                        {
                            foreach (var item in kv.Value)
                            {
                                if (item.Uid == uid)
                                {
                                    list.list.Add(GetWelfarePlayerInfo(item));
                                }
                            }
                        }
                    }
                    else if (serverId > 0)
                    {
                        if (kv.Key == serverId)
                        {
                            foreach (var item in kv.Value)
                            {
                                list.list.Add(GetWelfarePlayerInfo(item));
                            }
                        }
                    }
                    else if (uid > 0)
                    {
                        foreach (var item in kv.Value)
                        {
                            if (item.Uid == uid)
                            {
                                list.list.Add(GetWelfarePlayerInfo(item));
                            }
                        }
                    }
                }
            }
            return list;
        }

        public WelfareStallInfoList GetWelfareStallInfoList()
        {
            WelfareStallInfoList list = new WelfareStallInfoList();
            foreach (var item in StallList)
            {
                WelfareStallInfo info = GetWelfareStallInfo(item.Value);
                list.list.Add(info);
            }
            return list;
        }

        private WelfareStallInfo GetWelfareStallInfo(WelfareStallItem item)
        {
            WelfareStallInfo info = new WelfareStallInfo();
            info.id = item.Id;
            info.name = item.Name;
            info.firstEmail = item.FirstEmail;
            info.fixedEmail = item.FixedEmail;
            info.type = (int)item.Type;
            info.day = item.Day;
            info.time = item.Time.ToString();
            return info;
        }

        public WelfarePlayerInfoList GetWelfarePlayerInfoList()
        {
            WelfarePlayerInfoList list = new WelfarePlayerInfoList();
            foreach (var dic in PlayerList)
            {
                foreach (var kv in dic.Value)
                {
                    foreach (var item in kv.Value)
                    {
                        WelfarePlayerInfo info = GetWelfarePlayerInfo(item);
                        list.list.Add(info);
                    }
                }
            }
            return list;
        }

        private WelfarePlayerInfo GetWelfarePlayerInfo(WelfarePlayerItem item)
        {
            WelfarePlayerInfo info = new WelfarePlayerInfo();
            info.id = item.Id;
            info.name = item.Name;
            info.serverId = item.ServerId;
            info.uid = item.Uid;
            info.LastTime = Timestamp.TimeStampToDateTime(item.LastTime).ToString();
            return info;
        }

        public bool ModifyStall(int id, string name, int firstEmail, int fixedEmainl)
        {
            WelfareStallItem item = GetWelfareStallItem(id);
            if (item != null)
            {
                item.Name = name;
                item.FirstEmail = firstEmail;
                item.FixedEmail = fixedEmainl;
                return true;
            }
            return false;
        }

        //public void ModifyPlayer(WelfarePlayerItem model)
        //{
        //    //不能修改只能删除
        //}
    }
}

