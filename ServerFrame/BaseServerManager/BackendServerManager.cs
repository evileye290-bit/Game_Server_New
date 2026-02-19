using System;
using System.Collections.Generic;
using DataProperty;
using Logger;

namespace ServerFrame
{
    public class BackendServerManager : BaseServerManager
    {
        protected Dictionary<string, BackendServer> serverList = new Dictionary<string, BackendServer>();
        public Dictionary<string, BackendServer> ServerList
        { get { return serverList; } }

        public BackendServerManager(BaseApi api, ServerType serverType)
            : base(api, serverType)
        {
            if (serverType == ServerType.GlobalServer)
            {
                //pay需要链接到所有的global
                if (api.ServerType == ServerType.PayServer)
                {
                    DataList dataList = DataListManager.inst.GetDataList("AllGlobalInfo");
                    if(dataList == null) return;

                    foreach (var data in dataList)
                    {
                        BuildBackendServer(serverType, data.Value);
                    }
                }
                else
                {
                    // 创建Global的ServerManager同时创建Global Server
                    //DataList serverDataList = DataListManager.inst.GetDataList("ServerConfig");
                    //Data globalData = serverDataList.Get("GlobalServer");
                    //string serverPort1 = Application.ProductName.ToString().Substring(0, 1).ToLower() + Application.ProductName.ToString().Substring(1) + "Port";
                    Data globalData = DataListManager.inst.GetData("GlobalServer", 1);
                    BuildBackendServer(serverType, globalData);


                    //int port = globalData.GetInt("port");
                    //Dictionary<string, int> serverPort = new Dictionary<string, int>();
                    //foreach (string name in Enum.GetNames(typeof(ServerType)))
                    //{
                    //    /// 枚举名字
                    //    string serverPortStr = name.Substring(0, 1).ToLower() + name.Substring(1) + "Port";
                    //    int namePort = globalData.GetInt(serverPortStr);
                    //    if (namePort > 0)
                    //    {
                    //        serverPort[name] = namePort;
                    //    }
                    //}

                    //BaseServerInfo baseInfo = new BaseServerInfo(serverType, mainId, subId, false, serverIp, port, clientIp, serverPort);
                    //BackendServer globalServer = ServerFactory.CreateBackendServer(api, baseInfo, this);
                    //AddServer(globalServer);
                }
            }
        }

        public BackendServer BuildBackendServer(ServerType serverType, Data globalData)
        {
            int mainId = globalData.GetInt("mainId");
            int subId = globalData.GetInt("subId");
            string globalIp = globalData.GetString("globalIp");

            Data portData = DataListManager.inst.GetData("ServerPort", "GlobalServer");
            if (portData != null)
            {
                int port = portData.GetInt("ClientPort") + BaseApi.GetTempSubId(subId);
                Dictionary<string, int> serverPort = new Dictionary<string, int>();
                foreach (string name in Enum.GetNames(typeof(ServerType)))
                {
                    // 枚举名字
                    int namePort = portData.GetInt(name);
                    if (namePort > 0)
                    {
                        serverPort[name] = namePort + BaseApi.GetTempSubId(subId);
                    }
                }

                BaseServerInfo baseInfo = new BaseServerInfo(serverType, mainId, subId, false, globalIp, 0, "", serverPort);
                BackendServer globalServer = ServerFactory.CreateBackendServer(api, baseInfo, this);
                AddServer(globalServer);
                return globalServer;
            }

            return null;
        }

        public override void UpdateServers(double dt)
        {
            foreach (var server in serverList)
            {
                try
                {
                    server.Value.Update(dt);
                }
                catch (Exception e)
                {
                    Log.Alert(e.ToString());
                }
            }
        }

        public void AddServer(BackendServer server)
        {
            if (server == null)
            {
                Log.ErrorLine("{0} backend server manager add failed: server is null", api.ServerName);
                return;
            }
            string key = server.Key;
            if (serverList.ContainsKey(key))
            {
                Log.Warn("{0} backend server manager already add {1}, and will replace", api.ServerName, server.ServerName);
                ServerList.Remove(key);
            }
            Log.Info("{0} backend server manager add {1} ip {2}", api.ServerName, server.ServerName, server.ServerIp);
            serverList.Add(key, server);
        }

        public BackendServer GetServer(int main_id, int sub_id)
        {
            main_id = GetRedirectId(main_id);
            BackendServer server = null;
            string key = BaseServer.MakeKey(main_id, sub_id);
            serverList.TryGetValue(key, out server);
            return server;
        }

        public BackendServer GetSinglePointServer(int mainId)
        {
            return GetServer(mainId, 0);
        }


        // 随便找一个Server
        public BackendServer GetOneServer()
        {
            foreach (var item in serverList)
            {
                return item.Value;
            }
            return null;
        }

        public BackendServer GetWatchDogServer()
        {
            foreach (var item in serverList)
            {
                if (item.Value.WatchDog)
                {
                    return item.Value;
                }
            }
            return null;
        } 

        public void DestroyServer(BackendServer server)
        {
            if (server == null)
            {
                Log.Error("{0} destory backend server failed: server is null", api.ServerName);
                return;
            }
            Log.Warn("{0} backend server manager destory {1} ip {2}", api.ServerName, server.ServerName, server.ServerIp);
            string key = BaseServer.MakeKey(server.MainId, server.SubId);
            serverList.Remove(key);
        }

        public void Broadcast<T>(T msg) where T : Google.Protobuf.IMessage
        {
            ArraySegment<byte> head;
            ArraySegment<byte> body;
            BaseServer.BroadCastMsgMemoryMaker(msg, out head, out body);
            foreach (var server in serverList)
            {
                server.Value.Write(head, body);
            }
        }
    }
}
