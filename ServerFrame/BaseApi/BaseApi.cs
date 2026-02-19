using CommonUtility;
using DataProperty;
using DBUtility;
using Logger;
using RedisUtility;
using ServerShared;
using System;
using System.Collections.Generic;

namespace ServerFrame
{
    public partial class BaseApi : ILoggerTimestamp
    {
        protected int mainId = 0;
        public int MainId
        { get { return mainId; } }
        protected int subId = 0;
        public int SubId
        { get { return subId; } }

        private ServerType serverType = ServerType.Invalid;
        public ServerType ServerType
        { get { return serverType; } }

        private string serverName = string.Empty;
        public string ServerName
        { get { return serverName; } }

        private bool watchDog = false;
        public bool WatchDog
        { get { return watchDog; } }

        private string machineName = string.Empty;
        public string MachineName
        { get { return machineName; } }

        private int clusterId = 0;
        public int ClusterId
        { get { return clusterId; } }

        private string serverIp = string.Empty;
        public string ServerIp
        { get { return serverIp; } }

        private string clientIp = string.Empty;
        public string ClientIp
        { get { return clientIp; } }

        private int port = 0;
        public int Port
        { get { return port; } }

        private Dictionary<string, int> serverPort  = new Dictionary<string, int>();
        public Dictionary<string, int> ServerPort
        { get { return serverPort; } }

        protected double lastTime = 0;

        public static DateTime now = DateTime.Now;
        public static string nowString = now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        protected bool isRunning = false;
        public ServerState State = ServerState.Stopped;
        public DateTime StoppingTime = DateTime.MaxValue;

        protected FrameManager fps;
        public FrameManager Fps
        {
            get { return fps; }
        }

        public const string DATETIME_DATE_STRING = "yyyy-MM-dd HH:mm";
        public const string DATETIME_TIME_STRING = "HH:mm";

        protected Data portData;
        public Data PortData
        { get { return portData; } }

        protected Data serverData;
        public Data ServerData
        { get { return serverData; } }

        protected Data globalServerData;

        protected ServerManagerProxy serverManagerProxy;
        public ServerManagerProxy ServerManagerProxy
        { get { return serverManagerProxy; } }

        protected TrackingLoggerManager trackingLoggerManager;
        public TrackingLoggerManager TrackingLoggerMng
        { get { return trackingLoggerManager; } }


        protected BILoggerManager biLoggerManager;
        public BILoggerManager BILoggerMng
        { get { return biLoggerManager; } }


        protected DBManagerPool gameDBPool;
        public DBManagerPool GameDBPool
        { get { return gameDBPool; } }

        protected DBManagerPool accountDBPool;
        public DBManagerPool AccountDBPool
        { get { return accountDBPool; } }

        protected RedisOperatePool gameRedis;
        public RedisOperatePool GameRedis
        { get { return gameRedis; } }

        protected RedisOperatePool crossRedis;
        public RedisOperatePool CrossRedis
        { get { return crossRedis; } }

        protected TaskTimerManager taskTimerMng;
        public TaskTimerManager TaskTimerMng
        { get { return taskTimerMng; } }

        public static Random Random = new Random();

        public virtual void UpdateXml()
        {
            InitData();
        }

        // 执行 模版模式，子类实现CommonUpdate 及 SpecUpdate方法实现轮询
        public virtual void Run()
        {
            bool isPassDay = false;
            var time = new CommonUtility.Time();
            time.Init();
            while (IsRunning())
            {
                try
                {
                    Fps.SetFrameBegin();
                    now = Fps.Now;
                    nowString = now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    var dt = time.Update(now,out isPassDay);
                    lastTime = dt.TotalMilliseconds;
                    if (isPassDay)
                    {
                        PassDay();
                    }
                    CommonUpdate(lastTime);
                    SpecUpdate(lastTime);
                    Fps.SetFrameEnd();
                }
                catch (OutOfMemoryException)
                {
                    Log.Error("got out of memory exception, will stop");
                    StopServer(1);
                }
                catch (Exception e)
                {
                    Log.Alert(e.ToString());
                }
            }

        }

        public virtual void PassDay()
        { 
        }

        public virtual void ProcessInput()
        { 
        }

        // 关闭服务
        public virtual void StopServer(int min)
        {
            if (State != ServerState.Stopped && State != ServerState.Stopping)
            {
                State = ServerState.Stopping;
                StoppingTime = now.AddMinutes(min);
            }
        }

        public virtual void Exit()
        {
            try
            {
                if (gameDBPool != null)
                {
                    gameDBPool.Abort();
                    gameDBPool.Exit();
                }
                if (accountDBPool != null)
                {
                    accountDBPool.Abort();
                    accountDBPool.Exit();
                }
            }
            catch (Exception e)
            {
                Log.Alert("exit db failed: {0}", e.ToString());
            }
            if (BILoggerMng!=null)
            {
                BILoggerMng.Close();
            }
            if (TrackingLoggerMng !=null)
            {
                TrackingLoggerMng.Close();
            }
            Engine.System.End();
            Log.Close();
        }

        public bool IsRunning()
        {
            return isRunning;
        }

        public DateTime Now()
        {
            return now;
        }
        public string NowString()
        {
            return nowString;
        }

        public int GetInitialOffsetId()
        {
            // 前4位serverId 后6位逻辑id
            return mainId * 100000 + 1;
        }

        public static int GetMainIdByUid(int uid)
        {
            // 前4位serverId 后6位逻辑id
            return uid / 100000;
        }
    }
}
