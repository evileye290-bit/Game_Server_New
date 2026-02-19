using Logger;
using System;
using System.Collections.Generic;

namespace ServerFrame
{
    public class ServerManagerProxy
    {
        Dictionary<ServerType, FrontendServerManager> frontendMngList = new Dictionary<ServerType, FrontendServerManager>();
        Dictionary<ServerType, BackendServerManager> backendMngList = new Dictionary<ServerType, BackendServerManager>();
        private BaseApi api;
        private JoinStrategyModel joinStrategyModel;

        public ServerManagerProxy(BaseApi api)
        {
            this.api = api;
        }

        public void Init()
        {
            if (!NetworkGraph.Graph.TryGetValue(api.ServerType, out joinStrategyModel))
            {
                return;
            }

            FrontendServerManager frontendMng;
            BackendServerManager backendMng;
            foreach (var item in joinStrategyModel.StrategyList)
            {
                switch (item.Value)
                { 
                    case JoinStrategy.AcceptAll:
                    case JoinStrategy.AcceptById:
                        frontendMng = ServerManagerFactory.CreateFrontendServerManager(api, item.Key);
                        frontendMngList.Add(frontendMng.ServerType, frontendMng);
                        break;

                    case JoinStrategy.ConnectAll:
                    case JoinStrategy.ConnectById:
                        backendMng = ServerManagerFactory.CreateBackendServerManager(api, item.Key);
                        backendMngList.Add(backendMng.ServerType, backendMng);
                        break;

                    case JoinStrategy.BothById:
                        frontendMng = ServerManagerFactory.CreateFrontendServerManager(api, item.Key);
                        //frontendMng = new FrontendServerManager(api, item.Key);
                        frontendMngList.Add(frontendMng.ServerType, frontendMng);
                        backendMng = ServerManagerFactory.CreateBackendServerManager(api, item.Key);
                        backendMngList.Add(backendMng.ServerType, backendMng);
                        break;
                    default:
                        break;
                }
            }
        }

        public void InitDone()
        { 
            foreach (var manager in backendMngList.Values)
            {
                foreach (var server in manager.ServerList.Values)
                {
                    server.InitDone();
                }
            }
            foreach (var manager in frontendMngList.Values)
            {
                foreach (var server in manager.ServerList.Values)
                {
                    server.InitDone();
                }
            }
        }

        public void Update(double dt)
        {
            foreach (var serverManager in frontendMngList)
            {
                try
                {
                    serverManager.Value.UpdateServers(dt);
                }
                catch (Exception e)
                {
                    Log.Alert(e.ToString());
                }
            }
            foreach (var serverManager in backendMngList)
            {
                try
                {
                    serverManager.Value.UpdateServers(dt);
                }
                catch (Exception e)
                {
                    Log.Alert(e.ToString());
                }
            }
        }

        public void RegistFrontendServer(FrontendServer server)
        {
            AddOrReplaceFrontendServer(server);
            // 对于Global Server，作为注册中心需要根据连接拓扑图通知相关服务器
            if (api.ServerType == ServerType.GlobalServer)
            {
                foreach (var manager in frontendMngList.Values)
                {
                    foreach (var frontend in manager.ServerList.Values)
                    {
                        if (NetworkGraph.NeedConnect(server.ServerType, server.MainId, frontend.ServerType, frontend.MainId))
                        {
                            // server 需要 主动连接 frontend，则告诉 server 该 frontend 信息
                            server.NotifyBackendServer(frontend);
                        }
                        if (NetworkGraph.NeedAccept(server.ServerType, server.MainId, frontend.ServerType, frontend.MainId))
                        {
                            // frontend 需要 主动连接 server， 则告诉 frontend 该 server 信息
                            frontend.NotifyBackendServer(server);
                        }
                    }
                }

                foreach (var manager in backendMngList.Values)
                {
                    foreach (var backend in manager.ServerList.Values)
                    {
                        if (NetworkGraph.NeedConnect(server.ServerType, server.MainId, backend.ServerType, backend.MainId))
                        {
                            // server 需要 主动连接 backend，则告诉 server 该 backend 信息
                            server.NotifyBackendServer(backend);

                        }
                        if (NetworkGraph.NeedAccept(server.ServerType, server.MainId, backend.ServerType, backend.MainId))
                        {
                            // backend 需要 主动连接 server， 则告诉 backend 该 server 信息
                            backend.NotifyBackendServer(server);
                        }
                    }
                }
            }
        }

        public void RegistBackendServer(BackendServer server)
        {
            BackendServer oldServer = GetBackendServer(server.ServerType, server.MainId, server.SubId);
            // 该backend server与 global 断线重连，也会收到通知，忽略即可
            if (oldServer != null && oldServer.ServerIp == server.ServerIp)
            {
                Log.Warn("{0} regist backend server {1} failed: same server ip {2} and ignore", api.ServerName, server.ServerName, server.ServerIp);
                return;
            }
            // 同类型server物理机不同，此情况只应在故障迁移时出现,替换oldServer
            if (oldServer != null)
            {
                //oldServer可能在不停地Reconnect，将state置为Stopped停止重连
                Log.Warn("{0} regist backend server {1} ip {2} will replace old ip {3}", 
                    api.ServerName, server.ServerName, server.ServerIp, oldServer.ServerIp);
                oldServer.Stop();
            }
            AddOrReplaceBackendServer(server);
            server.ConnectBackendServer();
        }

        public void AddOrReplaceBackendServer(BackendServer server)
        {
            BackendServerManager manager = GetBackendServerManager(server.ServerType);
            if (manager != null)
            {
                manager.AddServer(server);
            }
        }

        public void AddOrReplaceFrontendServer(FrontendServer server)
        {
            FrontendServerManager manager = GetFrontendServerManager(server.ServerType);
            if (manager != null)
            {
                manager.AddServer(server);
            }
        }

        public BackendServerManager GetBackendServerManager(ServerType serverType)
        {
            BackendServerManager manager = null;
            backendMngList.TryGetValue(serverType, out manager);
            return manager;
        }

        public FrontendServerManager GetFrontendServerManager(ServerType serverType)
        {
            FrontendServerManager manager = null;
            frontendMngList.TryGetValue(serverType, out manager);
            return manager;
        }

        public FrontendServer GetFrontendServer(ServerType serverType, int mainId, int subId)
        {
            FrontendServerManager manager = GetFrontendServerManager(serverType);
            if (manager != null)
            {
                return manager.GetServer(mainId, subId);
            }
            return null;
        }

        public FrontendServer GetSinglePointFrontendServer(ServerType serverType, int mainId)
        {
            return GetFrontendServer(serverType, mainId, 0);
        }

        public BackendServer GetBackendServer(ServerType serverType, int mainId, int subId)
        {
            BackendServerManager manager = GetBackendServerManager(serverType);
            if (manager != null)
            {
                return manager.GetServer(mainId, subId);
            }
            return null;
        }

        public BackendServer GetSinglePointBackendServer(ServerType serverType, int mainId)
        {
            return GetBackendServer(serverType, mainId, 0);
        }

    }
}
