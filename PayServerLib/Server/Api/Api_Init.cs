using System;
using CommonUtility;
using DataProperty;
using DBUtility;
using Logger;
using MySql.Data.MySqlClient;
using ServerFrame;
using ServerModels;
using ServerShared;

namespace PayServerLib
{
    partial class PayServerApi
    {
        public string PayUrl { get; private set;}
        private ServerListConfig serversConfig;
        public ServerListConfig ServersConfig
        { get { return serversConfig; } }

        public BackendServer GlobalServer
        { get { return serverManagerProxy.GetSinglePointBackendServer(ServerType.GlobalServer, ClusterId); } }

        public FrontendServerManager ManagerServerManager
        { get { return serverManagerProxy.GetFrontendServerManager(ServerType.ManagerServer); } }

        public BackendServerManager BackendServerManager
        { get { return serverManagerProxy.GetBackendServerManager(ServerType.GlobalServer); } }

        //public override void InitDB()
        //{
        //}

        //public override void InitRedis()
        //{
        //}

        public override void InitProtocol()
        {
            base.InitProtocol();
            Message.Global.Protocol.GB.GBIdGenerator.GenerateId();
            Message.Global.Protocol.GM.GMIdGenerator.GenerateId();
            Message.Barrack.Protocol.BG.BGIdGenerator.GenerateId();
            Message.Manager.Protocol.MB.MBIdGenerator.GenerateId();

            Message.Global.Protocol.GP.GPIdGenerator.GenerateId();
            Message.Manager.Protocol.MP.MPIdGenerator.GenerateId();
            Message.Pay.Protocol.PM.PMIdGenerator.GenerateId();
            Message.Pay.Protocol.PG.PGIdGenerator.GenerateId();
        }

        public override void InitTrackingLog()
        {
            string statLogPrefix = $"_{MainId}_{SubId}";
            string logServerKey = string.Empty;
            DataList gameConfig = DataListManager.inst.GetDataList("ConstConfig");
            foreach (var item in gameConfig)
            {
                Data data = item.Value;
                logServerKey = data.GetString("LogServerKey");
            }
            trackingLoggerManager = new TrackingLoggerManager(logServerKey, statLogPrefix);
            TrackingLoggerMng.CreateLogger(TrackingLogType.CREATEACCOUNT);// 创建账号
            //TrackingLoggerMng.CreateLogger(TrackingLogType.CREATECHAR);//
        }

        public void InitPayInfoManager()
        {
            payInfoMng = new PayInfoManager(this);
        }

        public void InitSDK()
        {
            Data payData = DataListManager.inst.GetData("PayServer", 1);

            int payPort = payData.GetInt("payPort");
            if (payPort > 0)
            {
                payServer = SDKFactory.BuildBasePayServer(SDKType.SEA, payPort);
                payServer.Init(this);

                Data data = DataListManager.inst.GetData("UrlConfig", 1);
                PayUrl = data.GetString("payUrl");

                Log.Write($"PayServer init with port {payPort} url {PayUrl}");
            }

            payPort = payData.GetInt("huaweiPayPort");
            if (payPort > 0)
            {
                payServer_Huawei = SDKFactory.BuildBasePayServer(SDKType.Huawei, payPort);
                payServer_Huawei.Init(this);

                Log.Write($"Huawei PayServer init with port {payPort}");
            }

            int mallPort = payData.GetInt("mallPort");
            if (mallPort > 0)
            {
                vMallServer = new VMallServer(mallPort.ToString());
                vMallServer.Init(this);
                Log.Write("VMallServer init with {0}", mallPort);
            }
        }

        public override void UpdateXml()
        {
            base.UpdateXml();
            GameConfig.InitGameCongfig();

            VMallHelper.InitConfigData();
            InitServerList();
        }

        private void InitServerList()
        {
            if (GameConfig.UseDbServerList == 1)
            {
                InitServerListFromDb();
            }
            else
            {
                InitServerListFromXml();
            }
        }

        public void UpdateServerXml()
        {
            string[] files = System.IO.Directory.GetFiles(PathExt.FullPathFromServerData("XML"), "Server\\ServerList.xml", System.IO.SearchOption.AllDirectories);
            foreach (string file in files)
            {
                DataListManager.inst.Chnage(file);
            }

            //ResetCacheState();
            InitServerList();
        }

        private void InitServerListFromXml()
        {
            ServerListConfig serversConfig = new ServerListConfig();
            ServerItemModel serverItem;
            DataList dataList = DataListManager.inst.GetDataList("ServerList");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                serverItem = new ServerItemModel
                {
                    Id = data.ID,
                    //Name = data.Name,
                    IsOpen = data.GetBoolean("isOpen"),
                    IsNew = data.GetBoolean("IsNew"),
                    IsLineUp = data.GetBoolean("IsLineUp"),
                    IsRecommend = data.GetBoolean("IsRecommend"),
                    IsMaintaining = data.GetBoolean("IsMaintaining"),
                    RegistLimit = data.GetInt("RegistLimit"),
                    RecommendLimit = data.GetInt("RecommendLimit"),
                    OpenTime = DateTime.Parse(data.GetString("openTime")),
                    CrossGroup = data.GetInt("crossGroup")
                };
                serverItem.SetName(data.Name);

                serversConfig.Add(serverItem);
            }

            this.serversConfig = serversConfig;
            ServersConfig.InitList();

            InitServerRedirect();
        }

        public void InitServerListFromDb()
        {
            ServerListConfig serversConfig = new ServerListConfig();

            DataList dbConfigList = DataListManager.inst.GetDataList("DBConfig");
            if (dbConfigList == null)
            {
                Log.Error("init db failed: get db config data in DBConfig.xml failed");
                return;
            }
            Data accountDBData = dbConfigList.Get("account");
            if (accountDBData == null)
            {
                Log.Error("init db failed: get account db data in DBConfig.xml failed");
                return;
            }
            string dbIp = accountDBData.GetString("ip");
            string dbName = accountDBData.GetString("db");
            string dbAccount = accountDBData.GetString("account");
            string dbPassword = accountDBData.GetString("password");
            string dbPort = accountDBData.GetString("port");

            DBManager db = new DBManager();
            db.Init(dbIp, dbName, dbAccount, dbPassword, dbPort);
            MySqlDataReader reader = null;
            try
            {
                ServerItemModel serverItem;
                MySqlCommand cmd = db.Conn.CreateCommand();
                db.Conn.Open();
                cmd.CommandText = @"SELECT `id`,`isOpen`,`isNew`,`isLineUp`,`isRecommend`,`isMaintaining`,`registLimit`,`name`,
                                    `openTime`,`crossGroup`,`recommendLimit`,`registCharacterCount` FROM `server_list`;";
                cmd.CommandType = System.Data.CommandType.Text;

                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    serverItem = new ServerItemModel
                    {
                        Id = reader.GetInt32(0),
                        IsOpen = reader.GetBoolean(1),
                        IsNew = reader.GetBoolean(2),
                        IsLineUp = reader.GetBoolean(3),
                        IsRecommend = reader.GetBoolean(4),
                        IsMaintaining = reader.GetBoolean(5),
                        RegistLimit = reader.GetInt32(6),
                        //Name = reader.GetString(7),
                        OpenTime = reader.GetDateTime(8),
                        CrossGroup = reader.GetInt32(9),
                        RecommendLimit = reader.GetInt32(10),
                        RegistCharacterCount = reader.GetInt32(11)
                    };
                    serverItem.SetName(reader.GetString(7));
                    serversConfig.Add(serverItem);
                }
            }
            catch (Exception e)
            {
                Log.ErrorLine(e.ToString());
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
                db.Conn.Close();
            }

            this.serversConfig = serversConfig;
            ServersConfig.InitList();
            InitServerRedirect();
        }

        private void InitServerRedirect()
        {
            var dataList = DataListManager.inst.GetDataList("ServerListRedirect");
            foreach (var item in dataList)
            {
                serversConfig.ServerRedirect.Add(item.Value.ID, item.Value.GetInt("Redirect"));
            }
        }
    }
}
