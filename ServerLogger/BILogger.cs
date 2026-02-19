using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using ThinkingData.Analytics;

namespace Logger
{
    public class BILogger
    {
        private StreamWriter tw = null;
        private int refreshTime = 5; // 每5分钟更新一次
        public DateTime NextNewLogFileTime;
        public DateTime LastCheckTime = DateTime.Now;
        private string fileName;
        //private int writeLength = 0;
        private string prefix;
        private string serverKey;
        private BILoggerType type;
        private int writeLength = 0;
        private int flushLength = 0;
        private int fileSize = 1048576; // 每个log文件大小为1M
        private readonly int flushLengthThreshold = 2048; // 超过2048字节则写入磁盘
        private readonly int flushTimeThreshold = 30;     // 超过30秒则写入磁盘
        public string Dir = "c:/Log/BILog/";
        ILoggerTimestamp loggerTimestamp;
        public BILogger(ILoggerTimestamp loggerTimestamp, string server_key, string prefix, int refresh_time, BILoggerType type)
        {
            this.serverKey = server_key;
            this.refreshTime = refresh_time;
            // prefix = serverName_param
            // e.g. ZoneServer_1_1 ManagerServcer_1 BarrackServer_2
            this.prefix = prefix;
            this.type = type;
            this.loggerTimestamp = loggerTimestamp;

            //We create a new log file every time we run the app.
            fileName = GetSaveFileName(prefix);
            NextNewLogFileTime = DateTime.Now.AddMinutes(refreshTime);
            // create a writer and open the file
            tw = new StreamWriter(fileName);
            tw.AutoFlush = false;

        }
        public BILogger()
        {
            
        }

        /// <summary>
        /// 实例化nLog，即为获取配置文件相关信息(获取以当前正在初始化的类命名的记录器)
        /// </summary>
        private static NLog.Logger _logger = LogManager.GetCurrentClassLogger();

        private static BILogger _obj;

        public static BILogger _
        {
            get { return _obj ?? (new BILogger()); }
            set { _obj = value; }
        }

        public static void Trace(string msg)
        {
            _logger.Trace(msg);
        }


        internal void UpdateXml(int refreshTime, int fileSize)
        {
            SetRefreshTime(refreshTime);
            SetFileSize(fileSize);
        }

        public void SetRefreshTime(int refreshTime)
        {
            this.refreshTime = refreshTime;
        }

        public void SetFileSize(int fileSize)
        {
            this.fileSize = fileSize;
        }

        public void SetLogDir(string path)
        {
            Dir = path;
        }

        public void CheckNewLogFile(DateTime now)
        {
            if (writeLength > 0)
            {
                //隔一段时间生成一个文件
                if ((now > NextNewLogFileTime && now.Minute % refreshTime == 0) || (now.Date != LastCheckTime.Date))
                {
                    CreateLogFile();
                    LastCheckTime = loggerTimestamp.Now();
                }
            }
        }

        //private void InitNewLogFile()
        //{
        //    if (Directory.Exists(Dir) == false)
        //    {
        //        Directory.CreateDirectory(Dir);
        //    }
        //    LastCheckTime = DateTime.Now;
        //    NextNewLogFileTime = DateTime.Now.AddMinutes(refreshTime);
        //    fileName = Dir + serverKey + prefix + "_" + type.ToString() + "_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm") + ".log.now";
        //}
        private string GetSaveFileName(string prefix)
        {
            DateTime now = loggerTimestamp.Now();
            string path = Dir + now.ToString("yyyy_MM_dd") + "/";
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch
            {
                Log.Warn("Could not create save directory for log. See Logger.cs.");
            }

            string dt = "" + now.ToString("yyyy-MM-dd-HH-mm-ss");
            NextNewLogFileTime = now.AddMinutes(refreshTime);
            //Save directory is created in ConfigFileHandler
            return path + serverKey + prefix + "-" + type.ToString() + "_" + dt + ".txt.now"; ;
        }


        private void CheckFileSize(int length, DateTime now)
        {
            bool needCreate = false;
            writeLength += length;
            flushLength += length;
            if (flushLength >= flushLengthThreshold || (now.Date - LastCheckTime).TotalSeconds > flushTimeThreshold)
            {
                tw.Flush();
                flushLength = 0;
            }
            if (writeLength > fileSize)
            {
                needCreate = true;
            }
            if (LastCheckTime != DateTime.MaxValue && LastCheckTime.Date != now.Date)
            {
                needCreate = true;
            }

            if ((LastCheckTime - now).TotalMinutes > refreshTime)
            {
                needCreate = true;
            }

            LastCheckTime = now;
            if (needCreate)
            {
                CreateLogFile();
            }
        }

        private void CreateLogFile()
        {
            tw.Close();
            // 日志输出完毕，删除 .now 后缀
            string closeFileName = fileName.Replace(".now", "");
            try
            {
                System.IO.File.Move(fileName, closeFileName);
            }
            catch (Exception e)
            {
                Log.Alert(e.ToString());
            }
            fileName = GetSaveFileName(prefix);
            // 创建新的log file
            tw = new StreamWriter(fileName);
            tw.AutoFlush = false;
            writeLength = 0;
            flushLength = 0;
        }

        //public void Write(string log, BILoggerType type)
        //{
        //    if (tw == null)
        //    {
        //        // 第一次写入 则先创建文件
        //        tw = new StreamWriter(fileName);
        //        tw.AutoFlush = true;
        //    }
        //    try
        //    {
        //        log = string.Format("{0}|{1}", type.ToString(), log);
        //        tw.WriteLine(log);
        //    }
        //    catch (Exception e)
        //    {
        //        Log.Alert(e.ToString());
        //    }
        //}

        public void Write(string log, BILoggerType type)
        {
            try
            {
                string nowStr = loggerTimestamp.NowString();
                log = string.Format("{0}|{1}", type.ToString(), log);
                tw.WriteLine(log);
                CheckFileSize(log.Length, loggerTimestamp.Now());
            }
            catch (Exception e)
            {
                Log.Alert(e.ToString());
            }
        }


        public void Close()
        {
            if (tw == null)
            {
                return;
            }
            // 日志输出完毕，删除 .now 后缀
            tw.Close();
            string closeFileName = fileName.Replace(".now", "");
            try
            {
                System.IO.File.Move(fileName, closeFileName);
            }
            catch (Exception e)
            {
                Log.Alert(e.ToString());
            }
            finally
            {
                tw = null;
            }
        }

    }

    public partial class BILoggerManager
    {
        public const string DATETIME_TO_STRING = "yyyy-MM-dd HH:mm:ss";

        public Dictionary<BILoggerType, BILogger> LoggerList = new Dictionary<BILoggerType, BILogger>();
        private string serverKey;
        private string prefix;

        ILoggerTimestamp loggerTimestamp;
        public BILoggerManager(string server_key, string serverType, string prefix, ILoggerTimestamp loggerTimestamp)
        {
            this.serverKey = server_key;
            this.prefix = prefix;
            this.loggerTimestamp = loggerTimestamp;

            //string dir = string.Format($"{LogBusDir}/{serverKey}_{serverType}_{prefix}/");
            //if (!Directory.Exists(dir))
            //{
            //    Directory.CreateDirectory(dir);
            //}

            //ta = new ThinkingdataAnalytics(new LoggerConsumer(dir));
        }

        public void UpdateXml(int refreshTime, int fileSize)
        {
            foreach (var item in LoggerList)
            {
                item.Value.UpdateXml(refreshTime, fileSize);
            }

        }

        public void CreateLogger(BILoggerType type)
        {
            int periodTime = 5;
            BILogger logger = new BILogger(loggerTimestamp, serverKey, prefix, periodTime, type);
            if (LoggerList.ContainsKey(type))
            {
                LoggerList[type] = logger;
            }
            else
            {
                LoggerList.Add(type, logger);
            }
        }

        public void CheckNewLogFile(DateTime now)
        {
            foreach (var item in LoggerList)
            {
                try
                {
                    item.Value.CheckNewLogFile(now);
                }
                catch (Exception e)
                {
                    Log.Alert(e.ToString());
                }
            }
        }

        public void Write(string log, BILoggerType type)
        {
            BILogger logger = null;
            if (LoggerList.TryGetValue(type, out logger))
            {
                logger.Write(log, type);
            }
        }

        public void Close()
        {
            foreach (var item in LoggerList)
            {
                item.Value.Close();
            }
        }

        #region api

        //Login
        public void RecordLoginLog(int userId, string accountId, string name, string deviceId, string channel, int serverId, string loginIp, int level, string sdkUuid, int vipLevel, int diamond, int god, int exp, int battlePower, string idfa, string caid, string idfv, string imei, string oa_id, string imsi, string AN_id, string pachageName, string extendId)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
            {
                deviceId = "0000000000";
            }

            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "default";
            }
            string log_new = $"{userId}|{accountId}|{name}|{deviceId}|{channel}|{serverId}|{loginIp}|{vipLevel}|{level}|{sdkUuid}|{diamond}|{god}|{exp}|{loggerTimestamp.Now().ToString(DATETIME_TO_STRING)}";
            Write(log_new, BILoggerType.LOGIN);

            LoginTaLog(userId, accountId, name, deviceId, channel, serverId, loginIp, level, battlePower, idfa, caid, idfv, imei, oa_id, imsi, AN_id, pachageName, extendId);
        }

        //Logout
        public void RecordLogoutLog(int userId, string accountId, string name, string deviceId, string channel, int serverId, string logoutIp, int gold, int diamond, int exp, int friendlyHeart, int sotoCoin, int resonanceCrystal, int shellCoin, int battlePower, int onlineTime = 0, int level = 1, string sdkUuid = "default", int huntingLevel = 0, int vipLevel = 1)
        {
            //
            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "default";
            }
            string log_new = $"{userId}|{accountId}|{name}|{deviceId}|{channel}|{serverId}|{logoutIp}|{onlineTime}|{vipLevel}|{level}|{sdkUuid}|Natural|{diamond}|{gold}|{exp}|{friendlyHeart}|{sotoCoin}|{resonanceCrystal}|{shellCoin}|{loggerTimestamp.Now().ToString(DATETIME_TO_STRING)}";
            Write(log_new, BILoggerType.LOGOUT);
            LogoutTaLog(userId, accountId, name, deviceId, channel, serverId, logoutIp, gold, diamond, exp, friendlyHeart, sotoCoin, resonanceCrystal, shellCoin, battlePower, onlineTime, level, sdkUuid, vipLevel, huntingLevel);
        }

        //shop
        public void RecordShopByItem(int userId, string accountId, string deviceId, string channel, int serverId, string shopType, string currencyType, int currencyCount, string obtainType, string moduleId, int itemType, int itemCount, int level = 1, string sdkUuid = "default", string timingGiftType = "", int buyCount = 1)
        {
            //CURRENCY_CONSUME
            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "default";
            }
            string log_new = $"{userId}|{level}|{accountId}|{deviceId}|{channel}|{serverId}|{shopType}|{currencyType}|{currencyCount}|{obtainType}|{moduleId}|{itemType}|{itemCount}|{sdkUuid}|{timingGiftType}|{buyCount}|{loggerTimestamp.Now().ToString(DATETIME_TO_STRING)}";
            Write(log_new, BILoggerType.SHOP);
            ShopTaLog(userId, accountId, deviceId, channel, serverId, shopType, currencyType, currencyCount, obtainType, moduleId, itemType, itemCount, level, sdkUuid, timingGiftType, buyCount);
        }

        //Obtain item
        public void RecordObtainItem(int userId, string accountId, string deviceId, string channel, int serverId, int count, int currCount, string itemType, string obtainType, string moduleId, int level = 1, int year = 0, string sdkUuid = "default")
        {
            //CURRENCY_CONSUME
            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "default";
            }
            string log_new = $"{userId}|{level}|{accountId}|{deviceId}|{channel}|{serverId}|{itemType}|{obtainType}|{moduleId}|{year}|{count}|{currCount}|{sdkUuid}|{loggerTimestamp.Now().ToString(DATETIME_TO_STRING)}";
            Write(log_new, BILoggerType.ITEMPRODUCE);
            //ObtainItemTaLog(userId, accountId, deviceId, channel, serverId, count, currCount, itemType, obtainType, moduleId, level, year, sdkUuid);
        }

        //Consume item
        public void RecordConsumeItem(int userId, string accountId, string deviceId, string channel, int serverId, int count, int currCount, string itemType, string consumeType, string moduleId, int level = 1, string sdkUuid = "default", int ringLevel = 0)
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "default";
            }
            string log_new = $"{userId}|{level}|{accountId}|{deviceId}|{channel}|{serverId}|{itemType}|{consumeType}|{moduleId}|{count}|{currCount}|{sdkUuid}|{loggerTimestamp.Now().ToString(DATETIME_TO_STRING)}";
            Write(log_new, BILoggerType.ITEMCONSUME);

            //ConsumeItemTaLog(userId, accountId, deviceId, channel, serverId, count, currCount, itemType, consumeType, moduleId, level, sdkUuid);
        }

        //Obtain Currency
        public void RecordObtainCurrency(int userId, string accountId, string deviceId, string channel, int serverId, int count, int currCount, string currencyType, string obtainType, string moduleId, int level = 1, string sdkUuid = "default")
        {
            //CURRENCY_CONSUME
            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "default";
            }
            string log_new = $"{userId}|{level}|{accountId}|{deviceId}|{channel}|{serverId}|{currencyType}|{obtainType}|{moduleId}|{count}|{currCount}|{sdkUuid}|{loggerTimestamp.Now().ToString(DATETIME_TO_STRING)}";
            Write(log_new, BILoggerType.OBTAINCURRENCY);
            //ObtainCurrencyTaLog(userId, accountId, deviceId, channel, serverId, count, currCount, currencyType, obtainType, moduleId, level, sdkUuid);
        }
       
        //Obtain Warehouse Currency
        public void RecordObtainWarehouseCurrency(int userId, string accountId, string deviceId, string channel, int serverId, int count, long currCount, string currencyType, string obtainType, string moduleId, int level = 1, string sdkUuid = "default")
        {
            //CURRENCY_CONSUME
            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "default";
            }
            string log_new = $"{userId}|{level}|{accountId}|{deviceId}|{channel}|{serverId}|{currencyType}|{obtainType}|{moduleId}|{count}|{currCount}|{sdkUuid}|{loggerTimestamp.Now().ToString(DATETIME_TO_STRING)}";
            Write(log_new, BILoggerType.OBTAINCURRENCY);
            //ObtainCurrencyTaLog(userId, accountId, deviceId, channel, serverId, count, currCount, currencyType, obtainType, moduleId, level, sdkUuid);
        }

        //Consume Currency
        public void RecordConsumeCurrency(int userId, string accountId, string deviceId, string channel, int serverId, int count, int currCount, string currencyType, string consumeType, string moduleId, int level = 1, string sdkUuid = "default")
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "default";
            }
            string log_new = $"{userId}|{level}|{accountId}|{deviceId}|{channel}|{serverId}|{currencyType}|{consumeType}|{moduleId}|{count}|{currCount}|{sdkUuid}|{loggerTimestamp.Now().ToString(DATETIME_TO_STRING)}";
            Write(log_new, BILoggerType.CONSUMECURRENCY);

            //ConsumeCurrencyTaLog(userId, accountId, deviceId, channel, serverId, count, currCount, currencyType, consumeType, moduleId, level, sdkUuid);
        }
      
        //Consume Warehouse Currency
        public void RecordConsumeWarehouseCurrency(int userId, string accountId, string deviceId, string channel, int serverId, int count, long currCount, string currencyType, string consumeType, string moduleId, int level = 1, string sdkUuid = "default")
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "default";
            }
            string log_new = $"{userId}|{level}|{accountId}|{deviceId}|{channel}|{serverId}|{currencyType}|{consumeType}|{moduleId}|{count}|{currCount}|{sdkUuid}|{loggerTimestamp.Now().ToString(DATETIME_TO_STRING)}";
            Write(log_new, BILoggerType.CONSUMECURRENCY);

            //ConsumeCurrencyTaLog(userId, accountId, deviceId, channel, serverId, count, currCount, currencyType, consumeType, moduleId, level, sdkUuid);
        }

        //Recharge
        public void RecordRechargeLog(int userId, string accountId, string deviceId, string channel, int serverId, float money, string gameOrderId, string sdkOrderId, string payOrderId, string moneyType, string payWay, string productId, string rechargeGiftType, string rechargeSubType, int level = 1, string sdkUuid = "default", int vipLevel = 1)
        {
            //
            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "default";
            }
            int isSuccess = 1;
            string log_new = $"{userId}|{accountId}|{deviceId}|{channel}|{serverId}|{vipLevel}|{level}|{money}|{gameOrderId}|{sdkOrderId}|{payOrderId}|{moneyType}|{isSuccess}|{payWay}|{productId}|{rechargeGiftType}|{rechargeSubType}|{sdkUuid}|{loggerTimestamp.Now().ToString(DATETIME_TO_STRING)}";
            Write(log_new, BILoggerType.RECHARGE);

        }

        ////注册新账号时候的埋点
        //public void RecordActivateLog(string accountId, string deviceId, string channel, string serverId, string sdkUuid = "default", string pachageName = "cmge_pachage_name_9e25")
        //{
        //    //
        //    if (string.IsNullOrEmpty(channel))
        //    {
        //        channel = "default";
        //    }
        //    string log_new = $"{accountId}|{deviceId}|{channel}|{serverId}|{pachageName}|{sdkUuid}|{loggerTimestamp.Now().ToString(DATETIME_TO_STRING)}";
        //    Write(log_new, BILoggerType.ACTIVITE);
        //}

        ////Create
        //public void RecordCreateCharLog(string userId, string accountId, string deviceId, string channel, string serverId, string registerIp, string sdkUuid = "default", string idfa_imei = "cmge75959d43514e9e75", int phoneOS = 4, string countryCode = "CN", string cmgeSDKId = "0000000000", string pachageName = "cmge_pachage_name_9e25")
        //{
        //    //对应中手游 register
        //    if (string.IsNullOrWhiteSpace(channel))
        //    {
        //        channel = "default";
        //    }
        //    string log_new = $"{userId}|{accountId}|{deviceId}|{channel}|{serverId}|{registerIp}|{idfa_imei}|{phoneOS}|{countryCode}|{cmgeSDKId}|{pachageName}|{sdkUuid}|{loggerTimestamp.Now().ToString(DATETIME_TO_STRING)}";
        //    Write(log_new, BILoggerType.CREATECHAR);//要和激活事件一起上传
        //}



        ////Task
        //public void RecordTaskLog(int userId, string accountId, string deviceId, string channel, int serverId, string taskType, int taskId, int taskState, int powerValue, int level = 1, string sdkUuid = "default", int vipLevel = 1)
        //{
        //    if (string.IsNullOrWhiteSpace(channel))
        //    {
        //        channel = "default";
        //    }
        //    string log_new = $"{userId}|{accountId}|{deviceId}|{channel}|{serverId}|{taskType}|{taskId}|{taskState}|{powerValue}|{vipLevel}|{level}|{sdkUuid}|{loggerTimestamp.Now().ToString(DATETIME_TO_STRING)}";
        //    Write(log_new, BILoggerType.TASK);
        //}

        ////关卡
        //public void RecordCheckPointLog(int userId, string accountId, string deviceId, string channel, int serverId, string pointType, string pointId, bool pointState, int useTime, int powerValue, int level = 1, string sdkUuid = "default", int vipLevel = 1)
        //{
        //    //
        //    if (string.IsNullOrWhiteSpace(channel))
        //    {
        //        channel = "default";
        //    }
        //    int success = pointState ? 1 : 0;
        //    string log_new = $"{userId}|{accountId}|{deviceId}|{channel}|{serverId}|{pointType}|{pointId}|{success}|{useTime}|{powerValue}|{vipLevel}|{level}|{sdkUuid}|{loggerTimestamp.Now().ToString(DATETIME_TO_STRING)}";
        //    Write(log_new, BILoggerType.CHECKPOINT);
        //}

        ////Activity 活动
        //public void RecordActivityLog(int userId, string accountId, string deviceId, string channel, int serverId, string activityType, int activityId, string sdkUuid = "default")
        //{
        //    //
        //    if (string.IsNullOrWhiteSpace(channel))
        //    {
        //        channel = "default";
        //    }
        //    string log_new = $"{userId}|{accountId}|{deviceId}|{channel}|{serverId}|{activityType}|{activityId}|{sdkUuid}|{loggerTimestamp.Now().ToString(DATETIME_TO_STRING)}";
        //    Write(log_new, BILoggerType.ACTIVITY);
        //}

        ////养成
        //public void RecordDevelopLog(int userId, string accountId, string deviceId, string channel, int serverId, string developType, string targetId, int beforeLevel, int afterLevel, int heroId = 0, int heroLevel = 0, int level = 1, string sdkUuid = "default")
        //{
        //    //
        //    if (string.IsNullOrWhiteSpace(channel))
        //    {
        //        channel = "default";
        //    }
        //    string log_new = $"{userId}|{level}|{accountId}|{deviceId}|{channel}|{serverId}|{developType}|{heroId}|{heroLevel}|{targetId}|{beforeLevel}|{afterLevel}|0|0|{sdkUuid}|0|{loggerTimestamp.Now().ToString(DATETIME_TO_STRING)}";
        //    Write(log_new, BILoggerType.DEVELOP);
        //}

        ////抽卡
        //public void RecordRecruitHero(int userId, string accountId, string deviceId, string channel, int serverId, string drawType, string currencyType, int currencyCount, List<int> heroIds, int level = 1, string sdkUuid = "default")
        //{
        //    //CURRENCY_CONSUME
        //    if (string.IsNullOrWhiteSpace(channel))
        //    {
        //        channel = "default";
        //    }
        //    string heroId = string.Empty;
        //    if (heroIds.Count > 0)
        //    {
        //        foreach (var item in heroIds)
        //        {
        //            heroId += ":" + item;
        //        }
        //        //去掉第一个逗号
        //        heroId = heroId.Substring(1);
        //    }

        //    string log_new = $"{userId}|{level}|{accountId}|{deviceId}|{channel}|{serverId}|{drawType}|{currencyType}|{currencyCount}|{heroId}|{sdkUuid}|{loggerTimestamp.Now().ToString(DATETIME_TO_STRING)}";
        //    Write(log_new, BILoggerType.RECRUIT);
        //}

        ////魂环替换
        //public void RecordRingReplace(int userId, string accountId, string deviceId, string channel, int serverId, int heroId, int heroLevel, int ringIndex, int oldId, int oldYyear, int newId, int newYear, int level = 1, string sdkUuid = "default")
        //{
        //    //CURRENCY_CONSUME
        //    if (string.IsNullOrWhiteSpace(channel))
        //    {
        //        channel = "default";
        //    }
        //    string log_new = $"{userId}|{level}|{accountId}|{deviceId}|{channel}|{serverId}|{heroId}|{heroLevel}|{ringIndex}|{oldId}|{oldYyear}|{newId}|{newYear}|{sdkUuid}|{loggerTimestamp.Now().ToString(DATETIME_TO_STRING)}";
        //    Write(log_new, BILoggerType.RINGREPLACE);
        //}
        ////魂骨替换
        //public void RecordBoneReplace(int userId, string accountId, string deviceId, string channel, int serverId, int heroId, int heroLevel, int boneIndex, int oldId, int oldPower, int newId, int newPower, int level = 1, string sdkUuid = "default")
        //{
        //    //CURRENCY_CONSUME
        //    if (string.IsNullOrWhiteSpace(channel))
        //    {
        //        channel = "default";
        //    }
        //    string log_new = $"{userId}|{level}|{accountId}|{deviceId}|{channel}|{serverId}|{heroId}|{heroLevel}|{boneIndex}|{oldId}|{oldPower}|{newId}|{newPower}|{sdkUuid}|{loggerTimestamp.Now().ToString(DATETIME_TO_STRING)}";
        //    Write(log_new, BILoggerType.BONEREPLACE);
        //}
        ////装备替换
        //public void RecordEquipmentReplace(int userId, string accountId, string deviceId, string channel, int serverId, int heroId, int heroLevel, int equipmentIndex, int oldId, int oldPower, int newId, int newPower, int level = 1, string sdkUuid = "default")
        //{
        //    //CURRENCY_CONSUME
        //    if (string.IsNullOrWhiteSpace(channel))
        //    {
        //        channel = "default";
        //    }
        //    string log_new = $"{userId}|{level}|{accountId}|{deviceId}|{channel}|{serverId}|{heroId}|{heroLevel}|{equipmentIndex}|{oldId}|{oldPower}|{newId}|{newPower}|{sdkUuid}|{loggerTimestamp.Now().ToString(DATETIME_TO_STRING)}";
        //    Write(log_new, BILoggerType.EQUIPMENTREPLACE);
        //}
        ////装备升级
        //public void RecordEquipmentUpgrade(int userId, string accountId, string deviceId, string channel, int serverId, int heroId, int heroLevel, int equipmentIndex, int id, int state, int oldPower, int newPower, int level = 1, string sdkUuid = "default")
        //{
        //    //CURRENCY_CONSUME
        //    if (string.IsNullOrWhiteSpace(channel))
        //    {
        //        channel = "default";
        //    }
        //    string log_new = $"{userId}|{level}|{accountId}|{deviceId}|{channel}|{serverId}|{heroId}|{heroLevel}|{equipmentIndex}|{id}|{state}|{oldPower}|{newPower}|{sdkUuid}|{loggerTimestamp.Now().ToString(DATETIME_TO_STRING)}";
        //    Write(log_new, BILoggerType.EQUIPMENT);
        //}

        ////20201110
        ////限时礼包
        //public void RecordLimitPack(int userId, string accountId, string deviceId, string channel, int serverId, int level, float money, string money_type, int launch_id, int pack_id, int operation_type, int operation_time = 0, string sdkUuid = "default")
        //{
        //    if (string.IsNullOrWhiteSpace(channel))
        //    {
        //        channel = "default";
        //    }
        //    string log_new = $"{userId}|{accountId}|{deviceId}|{channel}|{serverId}|{level}|{money}|{money_type}|{launch_id}|{pack_id}|{operation_type}|{operation_time}|{sdkUuid}|{loggerTimestamp.Now().ToString(DATETIME_TO_STRING)}";
        //    Write(log_new, BILoggerType.LIMITPACK);
        //}

        ////神位替换
        //public void RecordGodReplace(int userId, int level, string accountId, string deviceId, string channel, int serverId, int card_id, int card_level, int god_id_old, int card_number_old, int god_id_new, int card_number_new, string sdkUuid = "default")
        //{
        //    if (string.IsNullOrWhiteSpace(channel))
        //    {
        //        channel = "default";
        //    }
        //    string log_new = $"{userId}|{level}|{accountId}|{deviceId}|{channel}|{serverId}|{card_id}|{card_level}|{god_id_old}|{card_number_old}|{god_id_new}|{card_number_new}|{sdkUuid}|{loggerTimestamp.Now().ToString(DATETIME_TO_STRING)}";
        //    Write(log_new, BILoggerType.GODREPLACE);
        //}

        //public void RecordGod(int userId, int level, string accountId, string deviceId, string channel, int serverId, int card_id, int card_level, int god_id, string sdkUuid = "default")
        //{
        //    if (string.IsNullOrWhiteSpace(channel))
        //    {
        //        channel = "default";
        //    }
        //    string log_new = $"{userId}|{level}|{accountId}|{deviceId}|{channel}|{serverId}|{card_id}|{card_level}|{god_id}|{sdkUuid}|{loggerTimestamp.Now().ToString(DATETIME_TO_STRING)}";
        //    Write(log_new, BILoggerType.GOD);
        //}

        //public void RecordWishingWell(int userId, string accountId, string deviceId, string channel, int serverId, int level, int well_state, string currency_in, string currency_out, int quantity_in, int quantity_out, string sdkUuid = "default")
        //{
        //    if (string.IsNullOrWhiteSpace(channel))
        //    {
        //        channel = "default";
        //    }
        //    string log_new = $"{userId}|{level}|{accountId}|{deviceId}|{channel}|{serverId}|{level}|{well_state}|{currency_in}|{currency_out}|{quantity_in}|{quantity_out}|{sdkUuid}|{loggerTimestamp.Now().ToString(DATETIME_TO_STRING)}";
        //    Write(log_new, BILoggerType.WISHINGWELL);
        //}

        //public void RecordTreasureMap(int userId, string accountId, string deviceId, string channel, int serverId, int level, int map_state, int blood, int quantity, string sdkUuid = "default")
        //{
        //    if (string.IsNullOrWhiteSpace(channel))
        //    {
        //        channel = "default";
        //    }
        //    string log_new = $"{userId}|{level}|{accountId}|{deviceId}|{channel}|{serverId}|{level}|{map_state}|{blood}|{quantity}|{sdkUuid}|{loggerTimestamp.Now().ToString(DATETIME_TO_STRING)}";
        //    Write(log_new, BILoggerType.TREASUREMAP);
        //}

        //public void UpdateXml(int refreshTime, int fileSize)
        //{
        //    foreach (var item in LoggerList)
        //    {
        //        item.Value.UpdateXml(refreshTime, fileSize);
        //    }

        //}
        #endregion

    }
}
