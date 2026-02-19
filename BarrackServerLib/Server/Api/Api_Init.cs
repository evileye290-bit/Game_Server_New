using CommonUtility;
using DataProperty;
using Logger;
using ServerFrame;
using ServerShared;
using System;
using System.Collections.Generic;
using DBUtility;
using MySql.Data.MySqlClient;
using ServerModels;
using System.Linq;

namespace BarrackServerLib
{
    partial class BarrackServerApi
    {
        public BackendServer GlobalServer
        { get { return serverManagerProxy.GetSinglePointBackendServer(ServerType.GlobalServer, ClusterId); } }

        public GateServerManager GateServerManager
        { get { return (GateServerManager)(serverManagerProxy.GetFrontendServerManager(ServerType.GateServer)); } }

        public FrontendServerManager ManagerServerManager
        { get { return serverManagerProxy.GetFrontendServerManager(ServerType.ManagerServer); } }

        private ClientManager clientManager;
        public ClientManager ClientMng
        { get { return clientManager; } }

        private AuthManager authMng;
        public AuthManager AuthMng
        { get { return authMng; } }

        private ServerListConfig serversConfig;
        public ServerListConfig ServersConfig
        { get { return serversConfig; } }

        protected DBManagerPool lossInterveneDBPool;
        public DBManagerPool LossInterveneDBPool
        { get { return lossInterveneDBPool; } }

        public string PayUrl = string.Empty;
        public override void InitProtocol()
        {
            base.InitProtocol();
            Message.Global.Protocol.GB.GBIdGenerator.GenerateId();
            Message.Barrack.Protocol.BG.BGIdGenerator.GenerateId();
            Message.Barrack.Protocol.BarrackC.BarrackCIdGenerator.GenerateId();
            Message.Client.Protocol.CBarrack.CBarrackIdGenerator.GenerateId();
            Message.Manager.Protocol.MB.MBIdGenerator.GenerateId();
            Message.Barrack.Protocol.BM.BMIdGenerator.GenerateId();
            Message.Gate.Protocol.GateB.GateBIdGenerator.GenerateId();
            Message.Barrack.Protocol.BGate.BGateIdGenerator.GenerateId();
        }

        public override void InitDB()
        {
            base.InitDB();

            Data persistentConfigData = DataListManager.inst.GetData("PersistentConfig", ServerType.ToString());
            if (persistentConfigData == null)
            {
                Log.Warn("{0} init db failed: no such data", ServerName);
                return;
            }

            DataList dbConfigList = DataListManager.inst.GetDataList("DBConfig");
            if (persistentConfigData.GetBoolean("LossIntervene"))
            {
                Data gameDBData = dbConfigList.Get("loss_intervene");
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

                lossInterveneDBPool = new DBManagerPool(poolCount);
                lossInterveneDBPool.Init(dbIp, dbName, dbAccount, dbPassword, dbPort);
            }
        }

        public void InitClient()
        {
            clientManager = new ClientManager();
            clientManager.Init(this);
            //DataList constconfigDataList = DataListManager.inst.GetDataList("ConstConfig");
            Engine.System.Listen((ushort)base.Port, (ushort listen_port) =>
            {
                Client client = new Client(this);
                client.Listen(listen_port);
            });
            InitAccountEnterManager();
            InitAntiAddictionServ();
            InitPayInfoManager();
        }

        public override void InitTrackingLog()
        {
            string statLogPrefix = string.Format("_{0}_{1}", MainId, SubId);
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

        public override void InitBILog()
        {
            string statLogPrefix = string.Format("_{0}_{1}", mainId, subId);
            string logServerKey = string.Empty;
            DataList gameConfig = DataListManager.inst.GetDataList("ConstConfig");
            foreach (var item in gameConfig)
            {
                Data data = item.Value;
                logServerKey = data.GetString("LogServerKey");
            }

            biLoggerManager = new BILoggerManager(logServerKey,ServerType.ToString(), statLogPrefix, this);
            //BILoggerMng.CreateLogger(BILoggerType.ACTIVITE);//

            BIXmlUpdate();
        }


        public void InitAuthManager()
        {
            authMng = new AuthManager();
            authMng.Init();
        }

        public void InitAccountEnterManager()
        {
            accountEnterMng = new AccountEnterManager(this);
        }

        public void InitAntiAddictionServ()
        {
            antiAddictionServ = new AntiAddictionService(this);
        }

        public void InitPayInfoManager()
        {
        }

        public bool CheckCanEnterOldServer(int destMainId, string loginServers, bool testAccount, string account)
        {
            if (testAccount) return true;

            //充值返利允许进入老服务器
            if (RechargeRebateLibrary.IsNeedRebate(account) && RechargeRebateLibrary.IsCurrServerRebateAvailable(destMainId)) return true;

            int serverId = serversConfig.GetRedirectServerId(destMainId);
            ManagerServer manager = ManagerServerManager.GetSinglePointServer(serverId) as ManagerServer;
            if (manager == null) return true;

            bool isNewAccount = string.IsNullOrEmpty(loginServers);

            var infos = SimpleCharacterInfo.GetSimpleCharacterInfos(loginServers);
            if (!infos.ContainsKey(destMainId))
            {
                isNewAccount = true;
            }

            //新号，当前服mainId注册达到上限
            if (isNewAccount && !CheckRegistLimit(destMainId)) return false;

            return true;
        }

        private bool CheckRegistLimit(int serverId)
        {
            serverId = serversConfig.GetRedirectServerId(serverId);
            ServerItemModel data = ServersConfig.Get(serverId);
            if (data == null) return false;

            ManagerServer manager = ManagerServerManager.GetSinglePointServer(serverId) as ManagerServer;
            if (manager?.RegistCharacterCount >= data.RegistLimit) return false;

            return true;
        }

        public override void UpdateXml()
        {
            base.UpdateXml();
            GameConfig.InitGameCongfig();
            BIXmlUpdate();
            authMng.Init();
            InitServerList();
            GiftRecommendHelper.SyncSdkInterveneInfo();
        }

        private void BIXmlUpdate()
        {
            var biConfigData = DataListManager.inst.GetData("BILogConfig", 1);
            int fileSize = biConfigData.GetInt("FileSize"); 
            int refreshTime = biConfigData.GetInt("RefreshTime");
            BILoggerMng.UpdateXml(refreshTime,fileSize);
        }

        private static void DoTaskStart(Action action)
        {
            var task = new System.Threading.Tasks.Task(() => action());
            task.Start();
        }

        private void InitRechargeRebate()
        {
            RechargeRebateLibrary.Init(mainId);
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

        //private void ResetCacheState()
        //{
        //    openingServerList.Clear();
        //    newServerList.Clear();
        //    lineUpServer.Clear();
        //    recommendServer.Clear();
        //    maintainingServer.Clear();
        //}
        
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

        private void InitServerListFromXml()
        {
            ServerListConfig serversConfig = new ServerListConfig();
            ServerItemModel serverItem;
            DataList dataList = DataListManager.inst.GetDataList("ServerList");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                serverItem = new ServerItemModel();
                serverItem.Id = data.ID;
                //serverItem.Name = data.Name;
                serverItem.SetName(data.Name);
                serverItem.IsOpen = data.GetBoolean("isOpen");
                serverItem.IsNew = data.GetBoolean("IsNew");
                serverItem.IsLineUp = data.GetBoolean("IsLineUp");
                serverItem.IsRecommend = data.GetBoolean("IsRecommend");
                serverItem.IsMaintaining = data.GetBoolean("IsMaintaining");
                serverItem.RegistLimit = data.GetInt("RegistLimit");
                serverItem.RecommendLimit = data.GetInt("RecommendLimit");
                serverItem.OpenTime = DateTime.Parse(data.GetString("openTime"));
                serverItem.CrossGroup = data.GetInt("crossGroup");

                serversConfig.Add(serverItem);
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
                    serverItem = new ServerItemModel();
                    serverItem.Id = reader.GetInt32(0);
                    serverItem.IsOpen = reader.GetBoolean(1);
                    serverItem.IsNew = reader.GetBoolean(2);
                    serverItem.IsLineUp = reader.GetBoolean(3);
                    serverItem.IsRecommend = reader.GetBoolean(4);
                    serverItem.IsMaintaining = reader.GetBoolean(5);
                    serverItem.RegistLimit = reader.GetInt32(6);
                    serverItem.SetName(reader.GetString(7));
                    //serverItem.Name = reader.GetString(7);
                    serverItem.OpenTime = reader.GetDateTime(8);
                    serverItem.CrossGroup = reader.GetInt32(9);
                    serverItem.RecommendLimit = reader.GetInt32(10);
                    serverItem.RegistCharacterCount = reader.GetInt32(11);
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

            InitServerListToDb(serversConfig);

            this.serversConfig = serversConfig;
            ServersConfig.InitList();
            InitServerRedirect();
        }

        private void InitServerListToDb(ServerListConfig serversConfig)
        {
            ServerItemModel serverItem;
            DataList dataList = DataListManager.inst.GetDataList("ServerList");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                if (!serversConfig.List.ContainsKey(data.ID))
                {
                    serverItem = new ServerItemModel();
                    serverItem.Id = data.ID;
                    serverItem.IsOpen = data.GetBoolean("isOpen");
                    serverItem.IsNew = data.GetBoolean("IsNew");
                    serverItem.IsLineUp = data.GetBoolean("IsLineUp");
                    serverItem.IsRecommend = data.GetBoolean("IsRecommend");
                    serverItem.IsMaintaining = data.GetBoolean("IsMaintaining");
                    serverItem.RegistLimit = data.GetInt("RegistLimit");
                    serverItem.SetName(data.Name);
                    //serverItem.Name = data.Name;
                    serverItem.OpenTime = DateTime.Parse(data.GetString("openTime"));
                    serverItem.CrossGroup = data.GetInt("crossGroup");
                    serverItem.RecommendLimit = data.GetInt("RecommendLimit");

                    serversConfig.Add(serverItem);
                    AccountDBPool.Call(new QueryInsertServerItem(serverItem));
                }
            }
        }

        public bool IsLineUpServer(int serverId)
        {
            return ServersConfig.lineUpServer.Contains(serverId);
        }

        public void SetLineUpServer(IEnumerable<int> servers)
        {
            ServersConfig.lineUpServer.Clear();
            ServersConfig.lineUpServer.AddRange(servers);
            AccountDBPool.Call(new QueryUpdateServersLineUpServer(servers.ToList()));
        }
        
        public bool IsRecommendServer(int serverId)
        {
            return ServersConfig.recommendServer.Contains(serverId);
        }

        public void SetRecommendServer(int serverId)
        {
            if (ServersConfig.recommendServer.Contains(serverId))
            {
                ServersConfig.recommendServer.Remove(serverId);
                AccountDBPool.Call(new QueryUpdateServersRecommend(serverId, 0));
            }
            else
            {
                //当前服务器注册人数满了
                //if (CheckRegistLimit(serverId)) return;

                ServersConfig.recommendServer.Add(serverId);
                AccountDBPool.Call(new QueryUpdateServersRecommend(serverId, 1));
            }
        }

        public void RemoveRecommendServer(int serverId)
        {
            ServersConfig.newServerList.Remove(serverId);
            AccountDBPool.Call(new QueryUpdateServersNew(serverId, 0)); 
            ServersConfig.recommendServer.Remove(serverId);
            AccountDBPool.Call(new QueryUpdateServersRecommend(serverId, 0));
        }

        public bool IsMaintainingServer(int serverId)
        {
            return ServersConfig.maintainingServer.Contains(serverId);
        }

        public void SetMaintainingState(bool state)
        {
            ServersConfig.ChangeMaintainingServer(state);
            if (state)
            {
                //DataList dataList = DataListManager.inst.GetDataList("ServerList");
                //if (dataList == null) return; ;

                //dataList.ForEach(x => ServersConfig.maintainingServer.Add(x.Key));
                //foreach (var item in ServersConfig.List.Keys.ToList())
                //{
                //    ServersConfig.maintainingServer.Add(item);
                //}
                AccountDBPool.Call(new QueryUpdateAllServersMaintaining(1));
            }
            else
            {
                //serversConfig.maintainingServer.Clear();
                AccountDBPool.Call(new QueryUpdateAllServersMaintaining(0));
            }
        }

        public void SetMaintainingServer(int serverId)
        {
            if (serversConfig.maintainingServer.Contains(serverId))
            {
                //ServersConfig.maintainingServer.Remove(serverId);
                AccountDBPool.Call(new QueryUpdateServersMaintaining(serverId, 0));
            }
            else
            {
                //ServersConfig.maintainingServer.Add(serverId);
                AccountDBPool.Call(new QueryUpdateServersMaintaining(serverId, 1));
            }
            ServersConfig.ChangeMaintainingServer(serverId);
        }

        public void SetOpeningServer(int serverId)
        {
            if (ServersConfig.openingServerList.Contains(serverId))
            {
                //ServersConfig.openingServerList.Remove(serverId);
                AccountDBPool.Call(new QueryUpdateServersOpen(serverId, 0));
            }
            else
            {
                //ServersConfig.openingServerList.Add(serverId);
                AccountDBPool.Call(new QueryUpdateServersOpen(serverId, 1));
                SetNewServer(serverId, true);
            }
            ServersConfig.ChangeOpeningServer(serverId);
        }

        public bool IsOpening(int serverId)
        {
            return ServersConfig.openingServerList.Contains(serverId);
        }
        
        public bool IsNewServer(int serverId)
        {
            return ServersConfig.newServerList.Contains(serverId);
        }

        public void SetNewServer(int serverId, bool add = false)
        {
            if (ServersConfig.newServerList.Contains(serverId))
            {
                ServersConfig.newServerList.Remove(serverId);
                AccountDBPool.Call(new QueryUpdateServersNew(serverId, 0));
            }
            else
            {
                add = true;
            }
            if (add)
            {
                ServersConfig.newServerList.Add(serverId);
                AccountDBPool.Call(new QueryUpdateServersNew(serverId, 1));
            }
        }


    }
}
