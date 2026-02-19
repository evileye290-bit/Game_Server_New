using DataProperty;
using DBUtility;
using Logger;
using ServerFrame;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;

namespace AnalysisServerLib
{
    partial class AnalysisServerApi
    {
        protected DBManagerPool logDBPool;
        public DBManagerPool LogDBPool => logDBPool;

        public BackendServer GlobalServer
        { get { return serverManagerProxy.GetSinglePointBackendServer(ServerType.GlobalServer, ClusterId); } }

        public override void InitConfig()
        {
            base.InitConfig();
        }

        public override void InitDB()
        {
            Data persistentConfigData = DataListManager.inst.GetData("PersistentConfig", ServerType.ToString());
            if (persistentConfigData == null)
            {
                Log.Warn("{0} init db failed: no such data", ServerType);
                return;
            }

            // 不需要初始化db
            if (!persistentConfigData.GetBoolean("MySql"))
            {
                return;
            }

            DataList dbConfigList = DataListManager.inst.GetDataList("DBConfig");
            if (persistentConfigData.GetBoolean("LogDB"))
            {
                Data gameDBData = dbConfigList.Get("logdb");
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

                logDBPool = new DBManagerPool(poolCount);
                logDBPool.Init(dbIp, dbName, dbAccount, dbPassword, dbPort);
            }
        }

        public override void ProcessDBPostUpdate()
        {
            base.ProcessDBPostUpdate();

            logDBPool?.Update();
        }

        public override void ProcessDBExceptionLog()
        {
            if (logDBPool != null)
            {
                foreach (var item in logDBPool.DBManagerList)
                {
                    try
                    {
                        Queue<string> queue = item.GetExceptionLogQueue();
                        if (queue != null && queue.Count != 0)
                        {
                            Log.Error(queue.Dequeue());
                            lock (item.ReconnectInfo)
                            {
                                if (State != ServerState.Stopping && item.ReconnectInfo.TryConnectTime >= item.ReconnectInfo.MaxConnectTime)
                                {
                                    // DB断开连接 则立即终止服务 方式回档
                                    Log.Error("{0} stop  because game db disconnect!", ServerName);
                                    StopServer(1);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Alert(e.ToString());
                    }
                }
            }
        }


        public override void InitData()
        {
            base.InitData();

            InitLibrarys();
        }

        public override void InitProtocol()
        {
            base.InitProtocol();
            Message.Analysis.Protocol.AG.AGIdGenerator.GenerateId();
            Message.Global.Protocol.GA.GAIdGenerator.GenerateId();
            Message.Global.Protocol.GB.GBIdGenerator.GenerateId();
            Message.Barrack.Protocol.BG.BGIdGenerator.GenerateId();
            Message.Barrack.Protocol.BarrackC.BarrackCIdGenerator.GenerateId();
            Message.Client.Protocol.CBarrack.CBarrackIdGenerator.GenerateId();
            Message.Manager.Protocol.MB.MBIdGenerator.GenerateId();
            Message.Barrack.Protocol.BM.BMIdGenerator.GenerateId();
            Message.Gate.Protocol.GateB.GateBIdGenerator.GenerateId();
            Message.Barrack.Protocol.BGate.BGateIdGenerator.GenerateId();
            Message.Corss.Protocol.CorssG.CorssGIdGenerator.GenerateId();
            Message.Corss.Protocol.CorssR.CorssRIdGenerator.GenerateId();
            Message.Relation.Protocol.RC.RCIdGenerator.GenerateId();
            Message.Zone.Protocol.ZR.ZRIdGenerator.GenerateId();
            Message.Gate.Protocol.GateC.GateCIdGenerator.GenerateId();
        }

        public override void UpdateXml()
        {
            base.UpdateXml();
        }
    }
}
