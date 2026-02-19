using DataProperty;
using Message.Client.Protocol.CBarrack;
using Message.Manager.Protocol.MB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ServerShared;

namespace BarrackServerLib
{
    //责任链模式变形，内存作为主要的维护空间，时长记录在barrack和manager责任流转，
    //logout时持久化到数据库，第一次登录时加载到内存         =============    不再在数据库维护
    //触发沉迷时 通知cmge  触发条件（1 manager到时间了或 2 在不被允许的时间内）
    public class AntiAddictionService
    {
        private BarrackServerApi server = null;

        private static string IOS_notifyAddictionTodayUrl = "https://lo.cmge.com/realName/addiction";

        private static string notifyAddictionTodayUrl = "https://u8.cmge.com/realName/addiction";

        //private static string appKey = "aaaaaaaaaaaaaaaaaaa";
        private static HttpClient client = new HttpClient();

        public AntiAddictionService(BarrackServerApi server)
        {
            Logger.Log.Write("Anti Addiction Service started");
            this.server = server;

            //更新防沉迷信息
            now = BarrackServerApi.now;
            GetConfig();
            //无需锁定，因为防沉迷是不允许12点登录的
            Addictions = new HashSet<string>();
            underAges = new Dictionary<string, int>(100000);
            //underAgesOnline = new Dictionary<string, bool>(100000);
        }

        HashSet<string> Addictions = new HashSet<string>();//今天已经被限制的未成年(已经沉迷)

        //所有需要的未成年玩家，当日时长累计 manager->barrack 登出时记录时长，或者到点需要被kick,  barrack->manager 登入时剩余时间 秒
        Dictionary<string, int> underAges = new Dictionary<string, int>(100000);

        //所有在线的未成年
        //Dictionary<string, bool> underAgesOnline = new Dictionary<string, bool>(100000);

        //只存未成年人的，登出就清理
        //Dictionary<string, string> accountUserId = new Dictionary<string, string>(100000);
        //Dictionary<string, string> accountPackageName = new Dictionary<string, string>(100000);
        Dictionary<string, AddictionInfo> accountInfos = new Dictionary<string, AddictionInfo>(100000);

        DateTime now = DateTime.Now;

        #region config

        private TimeSpan beginTime = new TimeSpan(1, 0, 0);
        private TimeSpan endTime = new TimeSpan(1, 0, 0);
        private HashSet<DateTime> holidays = new HashSet<DateTime>();
        private double holidayLimit = 3;
        private double noneHolidayLimit = 1.5;

        #endregion

        public void Refresh()
        {
            if (BarrackServerApi.now.Date != this.now.Date)
            {
                //更新防沉迷信息
                now = BarrackServerApi.now;
                GetConfig();
                //无需锁定，因为防沉迷是不允许12点登录的
                Addictions = new HashSet<string>();
                underAges = new Dictionary<string, int>(100000);
                //underAgesOnline = new Dictionary<string, bool>(100000);
            }
        }

        public void Update()
        {
            Refresh();
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

        public bool CheckTime()
        {
#if DEBUG
            Console.Write("checktime " + BarrackServerApi.now.TimeOfDay + " " + endTime + " " + beginTime);
#endif
            if (ServerFrame.BaseApi.now.TimeOfDay > endTime && BarrackServerApi.now.TimeOfDay < beginTime)
            {
                return true;
            }
            return false;
        }

        public bool TodayIsHoliday()
        {
            foreach (var day in holidays)
            {
                if (BarrackServerApi.now.DayOfYear == day.DayOfYear)
                {
                    return true;
                }
            }
            return false;
        }

        private async Task<string> NotifyAddition(Dictionary<string, string> info, bool IsIOS)
        {
            HttpContent contents = new FormUrlEncodedContent(info);

            contents.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");
            string url = "";
            if (IsIOS)
            {
                url = IOS_notifyAddictionTodayUrl;
            }
            else
            {
                url = notifyAddictionTodayUrl;
            }
            var response = await client.PostAsync(url, contents);
            var responseString = await response.Content.ReadAsStringAsync();
#if DEBUG
            Console.Write("NotifyAddition " + responseString);
#endif
            return responseString;
        }

        private async void NotifyAddictionToCMGE(AddictionInfo info)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();

            if (info.IsIOS)
            {
                dic.Add("p", info.packageName);
                dic.Add("t", info.todayTime.ToString());
                dic.Add("uid", info.userId);
                dic.Add("ad", info.addcitionType.ToString());
                string token = Guid.NewGuid().ToString("N").Substring(0, 6);
                dic.Add("z", token);
            }
            else
            {
                dic.Add("gameTime", info.todayTime.ToString());
                dic.Add("userId", info.userId);
                dic.Add("adType", info.addcitionType.ToString());
            }


            string md5 = GetSign(dic, info);
            dic.Add("sign", md5);

            await NotifyAddition(dic, info.IsIOS);
        }

        private string GetSign(Dictionary<string, string> dic, AddictionInfo info)
        {
            List<string> keys = new List<string>(dic.Keys);
            //CMGESDKApi.sortParamNames(keys);
            string paramValues = "";
            foreach (string param in keys)
            {//拼接参数值
                if (param == "sign")
                {
                    continue;
                }
                string paramValue = dic[param];
                if (paramValue != null)
                {
                    paramValues = paramValues + param + "=" + paramValue + "&";
                }
            }

            paramValues += "key=" + info.appKey;

            string md5 = EncryptHelper.MD5Encode(paramValues);
            return md5;
        }

        public void NotifyAddictionToCMGE(MSG_CB_USER_LOGIN pks, AddictionType type)
        {
            //AddictionInfo info = new AddictionInfo();
            //info.accountId = pks.AccountName+"$"+pks.ChannelName;
            ////info.packageName = pks.packageName;
            //info.addcitionType = (int)type;
            //info.userId = pks.SdkId;

            //underAges.TryGetValue(pks.AccountName, out info.todayTime);

            //bool isIos = string.IsNullOrEmpty(pks.Token) || string.IsNullOrWhiteSpace(pks.Token);
            //if (isIos)
            //{
            //    //info.appKey = CmgeSDK_IOS.ProductKey;
            //    //info.IsIOS = true;
            //}
            //else
            //{
            //    info.appKey = CMGESDKApi.appSecret;
            //    info.IsIOS = false;
            //}

            //NotifyAddictionToCMGE(info);
        }

        public bool BeforeSDKLoginCheck(MSG_CB_USER_LOGIN pks)
        {
            string realAccount = pks.AccountName + "$" + pks.ChannelName;

            //先对时间检查
#if DEBUG
            Logger.Log.Write("before sdk login check anti addiction");
#endif
            bool underAge = pks.UnderAge;
            if (underAge)
            {
#if DEBUG
                Logger.Log.Write("before sdk login check anti addiction 1");
#endif
                if (!CheckTime())
                {
#if DEBUG
                    Logger.Log.Write("before sdk login check anti addiction 2");
#endif
                    NotifyAddictionToCMGE(pks, AddictionType.NotInTime);

                    return false;
                }

                if (!underAges.ContainsKey(realAccount))
                {
#if DEBUG
                    Logger.Log.Write("before sdk login check anti addiction 3");
#endif
                    underAges.Add(realAccount, 0);
                    return true;
                }

                //拿出信息
                if (Addictions.Contains(realAccount))
                {
                    if (TodayIsHoliday())
                    {
                        NotifyAddictionToCMGE(pks, AddictionType.HolidayTimeLimit);
                    }
                    else
                    {
                        NotifyAddictionToCMGE(pks, AddictionType.NoneHolidayTimeLimit);
                    }
#if DEBUG
                    Logger.Log.Write("before sdk login check anti addiction 4");
#endif
                    return false;
                }
            }
            else
            {
                return true;
            }

            return true;
        }

        public void AfterSDKLogin(MSG_CB_USER_LOGIN pks)
        {
            if (pks.UnderAge)
            {
                //缓存accountId 用于数据同步
                AddictionInfo info = new AddictionInfo();
                info.userId = pks.SdkId;
                info.accountId = pks.AccountName + "$" + pks.ChannelName;
                underAges.TryGetValue(info.accountId, out info.serverTime);
                //info.packageName = pks.packageName;

                accountInfos[info.accountId] = info;
            }
        }

        public AddictionInfo GetLoginAddictionInfo(string accountId)
        {
            AddictionInfo info = null;
            if (accountInfos.ContainsKey(accountId))
            {
                info = accountInfos[accountId];
            }
            return info;
        }

        public void NotifyAddictionInfoFromManager(MSG_MB_NOTIFY_LOGOUT logout)
        {
            string accountId = logout.AccountId;
            int tempTime = logout.ServerTime;
            if (logout.AddictionType > 0)
            {
                AddictionInfo info = null;
                if (accountInfos.TryGetValue(logout.AccountId, out info))
                {
                    if (info != null)
                    {
                        info.todayTime = tempTime;
                        info.addcitionType = logout.AddictionType;
                        NotifyAddictionToCMGE(info);
                    }
                }

                //todo 更新本地内存
                Addictions.Add(logout.AccountId);
                underAges[accountId] = tempTime;
            }
            else
            {
                //todo 更新本地内存
                //Addictions.Add(logout.accountId);
                underAges[accountId] = tempTime;
            }
        }

    }

    public class AddictionInfo
    {
        public string packageName = "";
        public int todayTime = 0;
        public string userId = "";
        public int addcitionType = 0;

        public string appKey = "";

        public bool IsIOS;

        //only game server 
        public string accountId = "";//此处为拼接的内容
        public int serverTime = 0;
    }

    public enum AddictionType
    {
        Visitor = 1,
        NotInTime = 2,
        HolidayTimeLimit = 3,
        NoneHolidayTimeLimit = 4,
    }
}

