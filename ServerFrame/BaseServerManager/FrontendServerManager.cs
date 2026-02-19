using DataProperty;
using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerFrame
{
    public class FrontendServerManager: BaseServerManager
    {
        private int serverUid = 0;
        // allServers 涉及到网络库多线程竞争，应用层访问应确认先上锁
        protected Dictionary<int, FrontendServer> allServers = new Dictionary<int, FrontendServer>();

        protected Dictionary<string, FrontendServer> serverList = new Dictionary<string, FrontendServer>();
        public Dictionary<string, FrontendServer> ServerList
        { get { return serverList; } }

        private List<FrontendServer> removeServers = new List<FrontendServer>();
        protected object allServersLock = new object();

        // 如 Global 对 Gate 的管理
        public FrontendServerManager(BaseApi api, ServerType serverType)
            : base(api, serverType)
        {
            //string listenPort = serverType.ToString().Substring(0, 1).ToLower() + serverType.ToString().Substring(1) + "Port";
            //ushort port = (ushort)api.ServerData.GetInt(listenPort);
            int port = api.PortData.GetInt(serverType.ToString()) + BaseApi.GetTempSubId(api.SubId);
            Engine.System.Listen((ushort)port, 10, (mPort) =>
            {
                FrontendServer frontendServer = ServerFactory.CreateFrontendServer(api, serverType, this, (ushort)port);
            });
        }

        public override void UpdateServers(double dt)
        {
            lock (allServersLock)
            {
                foreach (var server in allServers)
                {
                    try
                    {
                        server.Value.Update(dt);

                        if (server.Value.ServerTcp.SocketIsNull())
                        {
                            removeServers.Add(server.Value);
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Alert(e.ToString());
                    }
                }
                if (removeServers.Count != 0)
                {
                    foreach (var item in removeServers)
                    {
                        allServers.Remove(item.ServerUid);
                        Log.Warn($" server {api.ServerName} disconnect frontend server {item.ServerType}");
                        //item.ServerTcp.Disconnect();
                    }
                    removeServers.Clear();
                }
            }
        }

        public void BindServer(FrontendServer server)
        {
            if (server == null)
            {
                Log.Error("bind frontend failed: server is null");
                return;
            }
            //Log.Info("{0} frontend server manager bind {1}", api.ServerName, server.ServerType);
            lock (allServersLock)
            {
                server.InitUid(GetNewServerUid());
                allServers.Add(server.ServerUid, server);
            }
        }
        public int GetNewServerUid()
        {
            return serverUid++;
        }
        public virtual void DestroyServer(FrontendServer server)
        {
            if (server == null)
            {
                Log.Error("destroy frontend server failed: server is null");
                return;
            }
            Log.Warn("{0} frontend server manager destroy {1} ip {2}", api.ServerName, server.ServerName, server.ServerIp);
            lock (allServersLock)
            {
                serverList.Remove(server.Key);

                if (allServers.ContainsKey(server.ServerUid))
                {
                    removeServers.Add(server);
                }
            }
        }

        public virtual void AddServer(FrontendServer server)
        {
            if (server == null)
            {
                Log.Error("{0} frontend server manager add server failed: frontend is null", api.ServerName);
                return;
            }
            FrontendServer oldServer = null;
            if (serverList.TryGetValue(server.Key, out oldServer))
            {
                Log.Warn("{0} frontend server manager already add {1} ip {2}, and replace by ip {3}", 
                    api.ServerName, server.ServerName, oldServer.ServerIp, server.ServerIp);
                // 删除Connect时AllFrontends已经保存的该server
                DestroyServer(oldServer);
            }
            Log.Info("{0} frontend server manager add {1} ip {2}", api.ServerName, server.ServerName, server.ServerIp);
            serverList.Add(server.Key, server);
        }

        public void RemoveServer(int mainId, int subId)
        {
            mainId = GetRedirectId(mainId);
            string key = BaseServer.MakeKey(mainId, subId);
            if (!serverList.ContainsKey(key))
            {
                Log.Warn("{0} frontend server manager remove {1} failed: not in list", api.ServerName, key);
                return;
            }
            serverList.Remove(key);
        }

        public FrontendServer GetServer(int mainId, int subId)
        {
            FrontendServer server = null;
            mainId = GetRedirectId(mainId);
            serverList.TryGetValue(BaseServer.MakeKey(mainId, subId), out server);
            return server;
        }

        // 对于单点类服务器，并不配置sub id，即subId为0
        public FrontendServer GetSinglePointServer(int mainId)
        {
            mainId = GetRedirectId(mainId);
            return GetServer(mainId, 0);
        }

        // 随便找一个Server
        public FrontendServer GetOneServer()
        {
            foreach (var item in serverList)
            {
                return item.Value;
            }
            return null;
        }

        public FrontendServer GetOneServer(int mainId, bool isRedirect = true)
        {
            if (isRedirect)
            {
                mainId = GetRedirectId(mainId);
            }

            foreach (var item in serverList)
            {
                if(item.Value.MainId == mainId)
                {
                    return item.Value;
                }
            }
            return null;

        }

        public List<FrontendServer> GetAllServer(int mainId)
        {
            mainId = GetRedirectId(mainId);
            List<FrontendServer> list = new List<FrontendServer>();
            foreach (var item in serverList)
            {
                if (item.Value.MainId == mainId)
                {
                    list.Add(item.Value);
                }
            }
            return list;

        }

        public FrontendServer GetWatchDogServer()
        {
            foreach (var item in serverList)
            {
                //if (item.Value.WatchDog)
                //{
                    return item.Value;
                //}
            }
            return null;
        }

        public void Broadcast<T>(T msg, int mainId = 0) where T : Google.Protobuf.IMessage
        {
            ArraySegment<byte> head;
            ArraySegment<byte> body;
            BaseServer.BroadCastMsgMemoryMaker(msg, out head, out body);
            if (mainId == 0)
            {
                foreach (var server in serverList)
                {
                    server.Value.Write(head, body);
                }
            }
            else
            {
                foreach (var server in serverList)
                {
                    if (server.Value.MainId == mainId)
                    {
                        server.Value.Write(head, body);
                    }
                }
            }
        }

    }
}
