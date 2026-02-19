using Engine;
using Logger;
using Message.Shared.Protocol.Shared;
using ServerShared;
using System.Collections.Generic;
using System.IO;

namespace ServerFrame
{
    /* gate->global, 
     * 故在GateApi中，global server为backend server
     * 在GlobalApi中，gate server为frontend server
    */
    public class BackendServer : BaseServer
    {
        protected BackendServerManager serverManager;
        public BackendServerManager ServerManager
        { get { return serverManager; } }

        public BackendServer(BaseApi api)
            : base(api)
        {
        }

        public override void InitNetwork(string ip, ushort port)
        {
            serverTcp = new Tcp(ip, port);
            serverTcp.OnRead = OnRead;
            serverTcp.OnDisconnect = OnDisconnet;
            serverTcp.OnConnect = OnConnect;
        }

        public void InitServerManager(BackendServerManager serverManager)
        {
            this.serverManager = serverManager;
        }

        public override void InitDone()
        {
            Log.Write("{0} init done, connect to {1} ip {2} port {3}",
               api.ServerName,
               ServerType.ToString(), serverTcp.IP, serverTcp.Port, MainId, SubId);
        }

        public void ConnectBackendServer()
        {
            serverTcp.Connect();
        }

        protected virtual void OnConnect(bool ret)
        {
            if (state == ServerState.Stopped)
            {
                return;
            }

            if (ret)
            {
                string log = string.Format("{0} connected to {1}", api.ServerName, ServerName);
                lock (LogList[LogType.INFO])
                {
                    LogList[LogType.INFO].Enqueue(log);
                }
                state = ServerState.Started;

                MSG_SERVER_BASE_INFO info = new MSG_SERVER_BASE_INFO();
                info.ServerType = (int)api.ServerType;
                info.MainId = api.MainId;
                info.SubId = api.SubId;
                info.WatchDog = api.WatchDog;
                info.ServerIp = api.ServerIp;
                info.ClientIp = api.ClientIp;
                info.Port = api.Port;
                foreach (var item in api.ServerPort)
                {
                    info.ServerPort[item.Key] = item.Value;
                }

                MSG_REGIST_SERVER msg = new MSG_REGIST_SERVER();
                msg.ServerInfo = info;
                Write(msg);
                SetTcpAlive(true);
            }
            else
            {
                string log = string.Format("{0} conncet to {1} ip {2} port {3} failed: try again", api.ServerName, ServerName, serverTcp.IP, serverTcp.Port);

                lock (LogList[LogType.ERROR])
                {
                    LogList[LogType.ERROR].Enqueue(log);
                }
                serverTcp.Connect();
                SetTcpAlive(false);
            }
        }

        protected virtual void OnDisconnet()
        {
            string log = string.Format("disconnect from {0}, connect ip {1} port {2} again", ServerName, serverTcp.IP, serverTcp.Port);
            lock (LogList[LogType.ERROR])
            {
                LogList[LogType.ERROR].Enqueue(log);
            }

            SetTcpAlive(false);

            if (NeedConnect())
            {
                if (state != ServerState.Stopped)
                {
                    state = ServerState.DisConnect;
                    serverTcp.Connect();
                }
            }
            else
            {
                serverManager.DestroyServer(this);
            }
            //string content = string.Format("zone main {0} disconnected from gate since {1}", MainId, Api.now.ToString());
            //api.DB.Call(new QueryAlarm((int)AlarmType.NETWORK, MainId, SubId, Api.now.ToString(), content), "alarm", DBUtility.DBOperateType.Write);
        }

        protected virtual bool NeedConnect()
        {
            return true;
        }

        public override void OnResponse_NotifyServer(MemoryStream stream, int uid = 0)
        {
            // 只用通过注册中心Global才会收到新server通知，创建该server对应的BackendServer
            if (ServerType != ServerFrame.ServerType.GlobalServer)
            {
                Log.Warn("{0} got notify server msg from {1}, check it!", api.ServerName, ServerName);
                return;
            }

            MSG_NOTIFY_SERVER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_NOTIFY_SERVER>(stream);
            MSG_SERVER_BASE_INFO info = msg.ServerInfo;
            ServerType backendServerType = (ServerType)info.ServerType;
            Log.Info("{0} got backend type {1} main {2} sub {3} ip {4}", api.ServerName, backendServerType, info.MainId, info.SubId, info.ServerIp);
            if (!NetworkGraph.NeedConnect(api.ServerType, api.MainId, backendServerType, info.MainId))
            {
                Log.Warn("{0} got backend type {1} main {2} sub {3} ip {4} failed: invalid in network graph", api.ServerName, ((ServerType)info.ServerType).ToString(), info.MainId, info.SubId, info.ServerIp);
                return;
            }

            //Data backendServerData = null;
            //DataList serverDataList = DataListManager.inst.GetDataList("ServerConfig");
            //List<Data> serverGroupData = serverDataList.GetByGroup(backendServerType.ToString());
            //foreach (var item in serverGroupData)
            //{
            //    if (item.Get("mainId").GetInt() == info.MainId && item.GetInt("subId") == info.SubId)
            //    {
            //        backendServerData = item;
            //        break;
            //    }
            //}
            //if (backendServerData == null)
            //{
            //    Log.Warn("{0} got backend type {1} main {2} sub {3} ip {4} failed: no such data",
            //    api.ServerName, ((ServerType)info.ServerType).ToString(), info.MainId, info.SubId, info.ServerIp);
            //    return;
            //}

            BackendServerManager manager = api.ServerManagerProxy.GetBackendServerManager(backendServerType);
            if (manager == null)
            {
                Log.Warn("{0} got backend type {1} main {2} sub {3} ip {4} failed: no such server manager",
                api.ServerName, ((ServerType)info.ServerType).ToString(), info.MainId, info.SubId, info.ServerIp);
                return;
            }

            Dictionary<string, int> serverPort = new Dictionary<string, int>();
            foreach (var item in info.ServerPort)
            {
                serverPort[item.Key] = item.Value;
            }
            BaseServerInfo baseInfo = new BaseServerInfo((ServerType)info.ServerType, info.MainId, info.SubId, info.WatchDog, info.ServerIp, info.Port, info.ClientIp, serverPort);
            BackendServer backendServer = ServerFactory.CreateBackendServer(api, baseInfo, manager);
            api.ServerManagerProxy.RegistBackendServer(backendServer);
        }

        public override void OnResponse_RegistSuccess(MemoryStream stream, int uid = 0)
        {
            Log.Info("{0} regist to {1} success", api.ServerName, ServerName);
            SendRegistSpecInfo();
        }

        // 根据不同服务器业务具体实现，
        // e.g. zone与manager断线重连后应通知manager当前所有map上的player等信息
        protected virtual void SendRegistSpecInfo() { }
    }
}
