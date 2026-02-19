using Engine;
using Logger;
using ServerShared;
using System;
using System.IO;
using Message.Shared.Protocol.Shared;
using System.Collections.Generic;

namespace ServerFrame
{
    /* gate->global, 
     * 故在GateApi中，global server为backend server
     * 在GlobalApi中，gate server为frontend server
     */
    public class FrontendServer : BaseServer
    {
        protected FrontendServerManager serverManager;
        public FrontendServerManager ServerManager
        { get { return serverManager; } }
        public FrontendServer(BaseApi api)
            : base(api)
        {
        }

        public override void InitNetwork(string ip, ushort port)
        {
            serverTcp = new Tcp(ip, port);
            serverTcp.OnRead = OnRead;
            serverTcp.OnDisconnect = OnDisconnect;
            serverTcp.OnAccept = OnAccept;
            StartListen();
        }

        public void InitServerManager(FrontendServerManager serverManager)
        {
            this.serverManager = serverManager;
        }

        protected void StartListen()
        {
            serverTcp.Accept();
        }

        protected virtual void OnAccept(bool ret)
        {
            state = ServerState.Started;
            serverManager.BindServer(this);
            SetTcpAlive(true);
        }

        protected virtual void OnDisconnect()
        {
            lock (LogList)
            {
                string log = string.Format("{0} disconnected from {1}", ServerName, api.ServerName);
                LogList[LogType.ERROR].Enqueue(log);
                while (LogList[LogType.INFO].Count > 0)
                {
                    try
                    {
                        log = LogList[LogType.INFO].Dequeue();
                        Log.Info(log);
                    }
                    catch (Exception e)
                    {
                        Log.Alert(e.ToString());
                    }
                }
                while (LogList[LogType.WARN].Count > 0)
                {
                    try
                    {
                        log = LogList[LogType.WARN].Dequeue();
                        Log.Alert(log);
                    }
                    catch (Exception e)
                    {
                        Log.Alert(e.ToString());
                    }
                }
                while (LogList[LogType.ERROR].Count > 0)
                {
                    try
                    {
                        log = LogList[LogType.ERROR].Dequeue();
                        Log.Error(log);
                    }
                    catch (Exception e)
                    {
                        Log.Alert(e.ToString());
                    }
                }
            }

            state = ServerState.Stopped;
            SetTcpAlive(false);
            serverManager.DestroyServer(this);
            //string content = string.Format("gate main {0} disconnected from zone since {1}", MainId, Api.now.ToString());
            //server.db.Call(new QueryAlarm((int)AlarmType.NETWORK, MainId, 0, Api.now.ToString(), content), "alarm", DBUtility.DBOperateType.Write);
        }

        public override void OnResponse_RegistServer(MemoryStream stream, int uid = 0)
        {
            //MSG_REGIST_SERVER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_REGIST_SERVER>(stream);
            MSG_REGIST_SERVER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_REGIST_SERVER>(stream);
            MSG_SERVER_BASE_INFO info = msg.ServerInfo;
            ServerType frontendServerType = (ServerFrame.ServerType)(info.ServerType);
            Log.Info("{0} main {1} sub {2} regist to {3}", frontendServerType, info.MainId, info.SubId, api.ServerName);
            Dictionary<string, int> serverPort = new Dictionary<string, int>();
            foreach (var item in info.ServerPort)
            {
                serverPort[item.Key] = item.Value;
            }
            BaseServerInfo baseInfo = new BaseServerInfo(frontendServerType, info.MainId, info.SubId, info.WatchDog, info.ServerIp, info.Port, info.ClientIp, serverPort);
            InitBaseInfo(baseInfo);
            api.ServerManagerProxy.RegistFrontendServer(this);

            // 完成对frontend server注册后，通知frontend发送注册的具体业务数据
            MSG_REGIST_SUCCESS notify = new MSG_REGIST_SUCCESS();
            Write(notify);
        }

    }
}
