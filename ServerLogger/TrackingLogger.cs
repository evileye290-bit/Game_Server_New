using System;
using System.Collections.Generic;
using System.IO;

namespace Logger
{
    public enum TrackingLogType
    {
        CREATEACCOUNT = 1, // 创建账号
        CREATECHAR = 2,     //创建角色
        LOGIN = 3,          //登录
        LOGOUT = 4,         //退出
        ONLINE = 5,       //在线
        CONSUME = 6,        //消耗
        OBTAIN = 7,         //获得
        CONSUMECURRENCY = 8,
        OBTAINCURRENCY = 9,
        SHOP = 10, //商店
        RECHARGE = 11,       //充值
        TASK = 12,           //任务

        LISTENCHAT = 13,    //言论
        TIPOFF = 14,       // 举报
        GAMECOMMENT = 15,   //对游戏的评价
        SUGGEST = 16,       // 吐槽
        QUESTION = 17,      //问卷

        RANK = 18,  //排行
        CROSSRANK = 19,          //跨服BOSS
        REFRESH = 20,
        RELATIONRANK = 21,
        TIMER = 22,
        RANKEMAIL = 23,
        RECHARGETIMER = 24,

        SendEmail = 25,
        GetEmail = 26,
        SoulBoneQuenching = 27,
        DungeonQueue = 28,
        HiddenWeapon = 29,
        Warehouse = 30,
        //ENTERMAP = 8,       //进入地图
        //QUITMAP = 9,       //退出地图
        //BATTLE = 12,        //战斗
        //COMMENT = 13,      // 英雄评论
        //BATTLESTAT1V1 = 17, //统计1v1阵容
        //BATTLESTAT2V2 = 18, //统计2v2阵容

        //CHECKPOINT = 23,
        //DEVELOP = 24,
        //ACTIVITY = 25,
        //ACTIVITE = 26,//注册账号时候
    }

    public class TrackingLogger
    {
        private StreamWriter tw = null;
        private int refreshTime = 5; // 每5分钟更新一次
        public DateTime NextNewLogFileTime;
        public DateTime LastCheckTime;
        private string fileName;
        //private int writeLength = 0;
        private string prefix;
        private string serverKey;
        private TrackingLogType type;
        public string Dir = "c:/Log/TrackingLog/";

        public TrackingLogger(string server_key, string prefix, int refresh_time, TrackingLogType type)
        {
            this.serverKey = server_key;
            this.refreshTime = refresh_time;
            // prefix = serverName_param
            // e.g. ZoneServer_1_1 ManagerServcer_1 BarrackServer_2
            this.prefix = prefix;
            this.type = type;
            InitNewLogFile();
        }

        public void SetLogDir(string path)
        {
            Dir = path;
        }

        public void CheckNewLogFile(DateTime now)
        {
            // 保证多个进程都是同一时间创建该类型文件
            if ((now > NextNewLogFileTime && now.Minute % refreshTime == 0) || (now.Date != LastCheckTime.Date))
            {
                Close();
                InitNewLogFile();
            }
            LastCheckTime = now;
        }

        private void InitNewLogFile()
        {
            if (Directory.Exists(Dir) == false)
            {
                Directory.CreateDirectory(Dir);
            }
            LastCheckTime = DateTime.Now;
            NextNewLogFileTime = DateTime.Now.AddMinutes(refreshTime);
            fileName = Dir + serverKey + prefix + "_" + type.ToString() + "_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".log.now";
        }

        public void Write(string log, TrackingLogType type)
        {
            if (tw == null)
            {
                // 第一次写入 则先创建文件
                tw = new StreamWriter(fileName);
                tw.AutoFlush = true;
            }
            try
            {
                log = string.Format("{0}|{1}", type.ToString(), log);
                tw.WriteLine(log);
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

    public class TrackingLoggerManager
    {
        public Dictionary<TrackingLogType, TrackingLogger> LoggerList = new Dictionary<TrackingLogType, TrackingLogger>();
        private string serverKey;
        private string prefix;
        public TrackingLoggerManager(string server_key, string prefix)
        {
            this.serverKey = server_key;
            this.prefix = prefix;
        }

        public void CreateLogger(TrackingLogType type)
        {
            int periodTime = 10;

            TrackingLogger logger = new TrackingLogger(serverKey, prefix, periodTime, type);
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

        public void Write(string log, TrackingLogType type)
        {
            TrackingLogger logger = null;
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

        #region API

        public void RecordRefreshLog(int userId, string accountId, string deviceId, string channel, int serverId,
         string lastTime, string timingType, int timingId, string way, DateTime now)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
            {
                deviceId = "0000000000";
            }

            if (string.IsNullOrWhiteSpace(channel))
            {
                channel = "default";
            }
            string log_new = $"{userId}|{accountId}|{deviceId}|{channel}|{serverId}|{lastTime}|{timingType}|{timingId}|{way}|{now.ToString("yyyy-MM-dd HH:mm:ss")}";
            Write(log_new, TrackingLogType.REFRESH);
        }

        public void TrackRankLog(int mainId, string rankType, int rank, int score, int uid, DateTime now)
        {
            string log_new = string.Format("{0}|{1}|{2}|{3}|{4}|{5}", mainId, rankType, rank, score, uid, now.ToString("yyyy-MM-dd HH:mm:ss"));
            Write(log_new, TrackingLogType.RANK);
        }

        public void RecordCrossRankLog(int userId, int addScore, int tatalValue, int rank, string rankType, int groupId, int paramId, DateTime now)
        {
            string log_new = $"{userId}|{addScore}|{tatalValue}|{rank}|{rankType}|{groupId}|{paramId}|{now.ToString("yyyy-MM-dd HH:mm:ss")}";
            Write(log_new, TrackingLogType.CROSSRANK);
        }

        public void RecordRealtionRankLog(int userId, int addScore, int tatalValue, int rank, string rankType, int groupId, int paramId, DateTime now)
        {
            string log_new = $"{userId}|{addScore}|{tatalValue}|{rank}|{rankType}|{groupId}|{paramId}|{now.ToString("yyyy-MM-dd HH:mm:ss")}";
            Write(log_new, TrackingLogType.RELATIONRANK);
        }

        public void TrackTimerLog(int mainId, string serverType, string keyType, DateTime now)
        {
            string log_new = string.Format("{0}|{1}|{2}|{3}", mainId, serverType, keyType, now.ToString("yyyy-MM-dd HH:mm:ss"));
            Write(log_new, TrackingLogType.TIMER);
        }

        public void TrackRankEmailLog(int groupId, int paramId, string rankType, int uid, int score, int email, int rank, DateTime now)
        {
            string log_new = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}", groupId, paramId, rankType, uid, score, email, rank, now.ToString("yyyy-MM-dd HH:mm:ss"));
            Write(log_new, TrackingLogType.RANKEMAIL);
        }

        public void TrackRechargeTimerLog(int mainId, string serverType, string keyType, DateTime now)
        {
            string log_new = string.Format("{0}|{1}|{2}|{3}", mainId, serverType, keyType, now.ToString("yyyy-MM-dd HH:mm:ss"));
            Write(log_new, TrackingLogType.RECHARGETIMER);
        }

        public void RecordGetEmailRewardLog(int userId, int emailId, string reward, string param, int mainId, DateTime now)
        {
            string newReward = reward.Replace("|", "-");
            string newParam = param.Replace("|", "-");
            string log_new = $"{userId}|{emailId}|{reward}|{newReward}|{newParam}|{mainId}|{now.ToString("yyyy-MM-dd HH:mm:ss")}";
            Write(log_new, TrackingLogType.GetEmail);
        }

        public void RecordSendEmailRewardLog(int userId, int emailId, string reward, string param, int mainId, DateTime now)
        {
            string newReward = reward.Replace("|", "-");
            string newParam = param.Replace("|", "-");
            string log_new = $"{userId}|{emailId}|{reward}|{newReward}|{newParam}|{mainId}|{now.ToString("yyyy-MM-dd HH:mm:ss")}";
            Write(log_new, TrackingLogType.SendEmail);
        }

        public void RecordSoulBoneQuenching(int userId, string msg, ulong old, int oldId, List<int> oldSpec, ulong newUid, int newId, List<int> newSpec, DateTime now)
        {
            string log_new = $"{userId}|{msg}|{old}|{oldId}|{string.Join("_", oldSpec)}|{newUid}|{newId}|{string.Join("_", newSpec)}|{now.ToString("yyyy -MM-dd HH:mm:ss")}";
            Write(log_new, TrackingLogType.SendEmail);
        }

        public void TrackDungeonQueueLog(int userId, int queueType, string queueStr, DateTime now)
        {
            string log_new = string.Format("{0}|{1}|{2}|{3}", userId, queueType, queueStr, now.ToString("yyyy-MM-dd HH:mm:ss"));
            Write(log_new, TrackingLogType.DungeonQueue);
        }

        public void RecordWarehouseItemLog(int userId, ulong warehouseUid, string reward, string param, int mainId, DateTime now)
        {
            string rewardStr = reward.Replace("|", "-");
            string paramStr = param.Replace("|", "-");
            string log_new = $"{userId}|{warehouseUid}|{rewardStr}|{paramStr}|{mainId}|{now.ToString("yyyy-MM-dd HH:mm:ss")}";
            Write(log_new, TrackingLogType.Warehouse);
        }
        #endregion
    }
}
