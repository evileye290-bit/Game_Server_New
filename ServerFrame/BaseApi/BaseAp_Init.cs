using CommonUtility;
using DataProperty;
using DBUtility;
using Logger;
using ServerLogger;
using ServerShared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;

namespace ServerFrame
{
    partial class BaseApi
    {
        // 初始化部分
        // 所有server都需要执行的初始化接口
        public virtual void Init(string[] args)
        {
            Engine.System.Begin();
            InitBaseInfo(args);
            InitLogger();
            InitPath();
            InitData();
            InitConfig();
            InitProtocol();
            InitFps();
            InitTrackingLog();
            InitBILog();
            InitDB();
            InitRedis();
            InitTaskTimer();

            InitNetworkGraph();
            InitServerManagerProxy();

            InitKomoeLog();
        }

        private void InitKomoeLog()
        {
            KomoeLogConfig.Init();
            KomoeLogManager.SetServerId(mainId);
        }

        protected void InitBaseInfo(string[] args)
        {
            machineName = System.Environment.MachineName;
            string productName = Application.ProductName;
            if (args.Length >= 1)
            {
                mainId = int.Parse(args[0]);
                if (args.Length >= 2)
                {
                    subId = int.Parse(args[1]);
                }
            }
            serverType = (ServerType)Enum.Parse(typeof(ServerType), productName);
            if (subId > 0)
            {
                serverName = string.Format("{0}_{1}_{2}", serverType.ToString(), mainId, subId);
            }
            else
            {
                serverName = string.Format("{0}_{1}", serverType.ToString(), mainId);
            }
            now = DateTime.Now;
        }

        public void InitLogger()
        {
            var logger = new Logger.ServerLogger();
            bool infoConsolePrint = false;
#if DEBUG
            infoConsolePrint = true;
#endif
            logger.Init(serverName, infoConsolePrint, true, true, true);
            //if (subId != 0)
            //{
            //    logger.Logo = " " + MainId + "_" + SubId + " ";
            //}
            //else
            //{
            //    logger.Logo = " " + MainId + " ";
            //}
            Logger.Log.SetGlobalLogger(logger);
        }

        public void InitPath()
        {
            //path不为空 则curPath = path
            string rootPath = string.Empty;
            DirectoryInfo path = new DirectoryInfo(Application.StartupPath);
            if (path.Parent.Exists)
            {
                rootPath = path.Parent.FullName;
            }
            else
            {
                Logger.Log.Error("Path is error! Please check the input path!");
            }
            CommonUtility.PathExt.SetPath(rootPath);
        }

        public virtual void InitData()
        {
            DataListManager.InitManager();
            string[] files = Directory.GetFiles(PathExt.FullPathFromServerData("XML"), "*.xml", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                DataListManager.inst.Parse(file);
            }
        }

        public virtual void InitConfig()
        {
            //global info
            globalServerData = DataListManager.inst.GetData("GlobalServer", 1);
            clusterId = globalServerData.GetInt("mainId");

            List<string> lstIPAddress = GetHostIPAddress();
            if (lstIPAddress.Count > 0)
            {
                serverIp = lstIPAddress[0];
                Log.WriteLine("GetHostIPAddress ip: " + serverIp);
            }
            else
            {
                Log.ErrorLine("GetHostIPAddress failed: no such ip");
                return;
            }

            //server info
            serverData = DataListManager.inst.GetData(serverType.ToString(), 1);
            if (serverData != null)
            {
                //watchDog = serverData.GetBoolean("watchDog");
                //serverIp = serverData.GetString("ip");
                clientIp = serverData.GetString("clientIp");
                //port = serverData.GetInt("port");
                //foreach (string name in Enum.GetNames(typeof(ServerType)))
                //{
                //    /// 枚举名字
                //    string serverPortStr = name.Substring(0, 1).ToLower() + name.Substring(1) + "Port";
                //    int namePort = serverData.GetInt(serverPortStr);
                //    if (namePort > 0)
                //    {
                //        serverPort[name] = namePort;
                //    }
                //}
            }

            portData = DataListManager.inst.GetData("ServerPort", serverType.ToString());
            if (portData != null)
            {
                port = portData.GetInt("ClientPort") + BaseApi.GetTempSubId(SubId);

                foreach (string name in Enum.GetNames(typeof(ServerType)))
                {
                    /// 枚举名字
                    int namePort = portData.GetInt(name);
                    if (namePort > 0)
                    {
                        serverPort[name] = namePort + BaseApi.GetTempSubId(SubId);
                    }
                }
            }

            //DataList serverDataList = DataListManager.inst.GetDataList("ServerConfig");
            //globalServerData = serverDataList.Get(ServerType.GlobalServer.ToString());
            //clusterId = globalServerData.GetInt("mainId");

            ////server info
            //List<Data> serverGroupData = serverDataList.GetByGroup(serverType.ToString());
            //foreach (var item in serverGroupData)
            //{
            //    if (item.Get("mainId").GetInt() == MainId && item.GetInt("subId") == SubId)
            //    {
            //        serverData = item;
            //        break;
            //    }
            //}
            //if (serverData != null)
            //{
            //    watchDog = serverData.GetBoolean("watchDog");
            //    serverIp = serverData.GetString("ip");
            //    port = serverData.GetInt("port");

            //    foreach (string name in Enum.GetNames(typeof(ServerType)))
            //    {
            //        /// 枚举名字
            //        string serverPortStr = name.Substring(0, 1).ToLower() + name.Substring(1) + "Port";
            //        int namePort = serverData.GetInt(serverPortStr);
            //        if (namePort > 0)
            //        {
            //            serverPort[name] = namePort;
            //        }
            //    }
            //}
        }

        /// <summary>
        /// 取得本机 IP Address
        /// </summary>
        /// <returns></returns>
        public static List<string> GetHostIPAddress()
        {
            List<string> lstIPAddress = new List<string>();
            IPHostEntry IpEntry = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ipa in IpEntry.AddressList)
            {
                if (ipa.AddressFamily == AddressFamily.InterNetwork)
                    lstIPAddress.Add(ipa.ToString());
            }
            return lstIPAddress; // result: 192.168.1.17 ......
        }

        public static int GetTempSubId(int id)
        {
            if (id > 0)
            {
                return id - 1;
            }
            else
            {
                return id;
            }
        }

        public virtual void InitProtocol()
        {
            Message.Shared.Protocol.Shared.SharedIdGenerator.GenerateId();
        }
        public virtual void InitFps()
        {
            fps = new FrameManager();
            Fps.Init(BaseApi.now);
        }

        public virtual void InitNetworkGraph()
        {
            NetworkGraph.Init();
        }

        public virtual void InitServerManagerProxy()
        {
            serverManagerProxy = new ServerManagerProxy(this);
            serverManagerProxy.Init();
        }

        public virtual void InitDB()
        {
            Data persistentConfigData = DataListManager.inst.GetData("PersistentConfig", serverType.ToString());
            if (persistentConfigData == null)
            {
                Log.Warn("{0} init db failed: no such data", serverName);
                return;
            }
            // 不需要初始化db
            if (!persistentConfigData.GetBoolean("MySql"))
            {
                return;
            }

            GameConfig.DbTransactionCount = 200;
            GameConfig.DbQueueCount = 200;
            Data constData = DataListManager.inst.GetData("ConstConfig", 1);
            if (constData != null)
            {
                GameConfig.DbTransactionCount = constData.GetInt("DbTransactionCount");
                GameConfig.DbQueueCount = constData.GetInt("DbQueueCount");
            }

            DataList dbConfigList = DataListManager.inst.GetDataList("DBConfig");
            if (persistentConfigData.GetBoolean("GameDB"))
            {
                Data gameDBData = dbConfigList.Get("gamedb_" + mainId);
                if (gameDBData == null)
                {
                    Log.Error("init db failed: get game db {0} data in DBConfig.xml failed", mainId);
                }
                string dbIp = gameDBData.GetString("ip");
                string dbName = gameDBData.GetString("db");
                string dbAccount = gameDBData.GetString("account");
                string dbPassword = gameDBData.GetString("password");
                string dbPort = gameDBData.GetString("port");
                string type = gameDBData.GetString("type");
                int poolCount = gameDBData.GetInt("threads");

                gameDBPool = new DBManagerPool(poolCount);
                gameDBPool.Init(dbIp, dbName, dbAccount, dbPassword, dbPort);
            }

            if (persistentConfigData.GetBoolean("AccountDB"))
            {
                Data accountDBData = dbConfigList.Get("account");
                if (accountDBData == null)
                {
                    Log.Error("init db failed: get account db data in DBConfig.xml failed");
                }
                string dbIp = accountDBData.GetString("ip");
                string dbName = accountDBData.GetString("db");
                string dbAccount = accountDBData.GetString("account");
                string dbPassword = accountDBData.GetString("password");
                string dbPort = accountDBData.GetString("port");
                string type = accountDBData.GetString("type");
                int poolCount = accountDBData.GetInt("threads");

                accountDBPool = new DBManagerPool(poolCount);
                accountDBPool.Init(dbIp, dbName, dbAccount, dbPassword, dbPort);
            }
        }

        public virtual void InitTrackingLog()
        {
        }

        public virtual void InitBILog()
        {
        }

        public virtual void InitDone()
        {
            isRunning = true;
            State = ServerState.Started;
            serverManagerProxy.InitDone();
            Log.Write("{0} Init Done", serverName);
        }

        public virtual void InitTaskTimer()
        {
            taskTimerMng = new TaskTimerManager();
        }
    }
}
