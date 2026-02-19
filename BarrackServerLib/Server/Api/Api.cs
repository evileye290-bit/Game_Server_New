using System;
using System.Collections.Generic;
using System.Linq;
using BarrackServerLib.Server.HttpServer;
using CommonUtility;
using Logger;
using Message.Barrack.Protocol.BM;
using ServerFrame;
using ServerShared;

namespace BarrackServerLib
{
    public partial class BarrackServerApi:BaseApi
    {
        private AccountEnterManager accountEnterMng = null;
        public AccountEnterManager AccountEnterMng
        { get { return accountEnterMng; } }

        private AntiAddictionService antiAddictionServ = null;
        public AntiAddictionService AntiAddictionServ
        { get { return antiAddictionServ; } }

        // args [path]
        public override void Init(string[] args)
        {
            base.Init(args);
            GameConfig.InitGameCongfig();
            InitClient();
            InitAuthManager();
            InitServerList();
            // init完毕，完成起服
            InitDone();
        }

        public override void InitDone()
        {
            base.InitDone();

            DoTaskStart(InitRechargeRebate);

            GiftRecommendHelper.SyncSdkInterveneInfo();
        }

        public override void SpecUpdate(double dt)
        {
            //UpdateU8Server();
            BILoggerMng.CheckNewLogFile(now);
            AccountEnterMng.UpdateAccountEnter();
            AntiAddictionServ.Update();
            //UpdateHttpServer();
            //UpdateKTPlayServer();
            clientManager.Update(lastTime);

            //NotifyInGameCount(dt);
        }

        public override void ProcessDBPostUpdate()
        {
            base.ProcessDBPostUpdate();

            lossInterveneDBPool?.Update();
        }

        public override void StopServer(int min = 0)
        {
            if (State != ServerState.Stopped && State != ServerState.Stopping)
            {
                // 关闭所有客户端连接 并且禁止新客户端连接
                base.StopServer(min);
                clientManager.DestroyAllClients();
            }
        }

        void UpdateU8Server()
        {
            U8LoginServer.ProcessInfoQueue();

            lock (U8LoginServer.LogList)
            {
                while (U8LoginServer.LogList[LogType.INFO].Count > 0)
                {
                    try
                    {
                        string log = U8LoginServer.LogList[LogType.INFO].Dequeue();
                        Log.Info(log);
                    }
                    catch (Exception e)
                    {
                        Log.Alert(e.ToString());
                    }
                }
                while (U8LoginServer.LogList[LogType.WARN].Count > 0)
                {
                    try
                    {
                        string log = U8LoginServer.LogList[LogType.WARN].Dequeue();
                        Log.Alert(log);
                    }
                    catch (Exception e)
                    {
                        Log.Alert(e.ToString());
                    }
                }
                while (U8LoginServer.LogList[LogType.ERROR].Count > 0)
                {
                    try
                    {
                        string log = U8LoginServer.LogList[LogType.ERROR].Dequeue();
                        Log.Error(log);
                    }
                    catch (Exception e)
                    {
                        Log.Alert(e.ToString());
                    }
                }
            }
        }

        public int GetDestServerMainId(int id)
        {
            if (id > 1000)
            {
                return id;
            }
            else
            {
                int temp = MainId / 1000;
                return temp * 1000 + id;
            }
        }

        private double notifyNextTime = 5000;
        public void NotifyInGameCount(double dt)
        {
            notifyNextTime -= dt;

            if (notifyNextTime <= 0)
            {
                List<FrontendServer> servers = ManagerServerManager.ServerList.Values.ToList();

                int ingameCount = GateServerManager.GetOnlineCount(), serverCount = servers.Count;

                MSG_BM_NOTIFY_SERVER_STATE_INFO msg = new MSG_BM_NOTIFY_SERVER_STATE_INFO() { InGameCount = ingameCount, ServerCount = serverCount };
                ManagerServerManager.Broadcast(msg);

                notifyNextTime = 5000;
            }
        }
    }

}
