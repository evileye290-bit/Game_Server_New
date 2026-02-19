using System.Collections.Generic;

namespace ServerFrame
{
    public class BaseServerInfo
    {
        private ServerType serverType = ServerType.Invalid;
        public ServerType ServerType
        { get { return serverType; } }

        private int mainId = 0;
        public int MainId
        { get { return mainId; } }

        private int subId = 0;
        public int SubId
        { get { return subId; } }

        private bool watchDog = false;
        public bool WatchDog
        { get { return watchDog; } }

        private string serverName = string.Empty;
        public string ServerName
        { get { return serverName; } }

        private string key = string.Empty;
        public string Key
        { get { return key; } }

        private string serverIp = string.Empty;
        public string ServerIp
        { get { return serverIp; } }

        private int port;
        public int Port { get { return port; } }

        private string clientIp = string.Empty;
        public string ClientIp
        { get { return clientIp; } }

        private Dictionary<string, int> serverPort = new Dictionary<string, int>();
        public Dictionary<string, int> ServerPort
        { get { return serverPort; } }

        public BaseServerInfo()
        {
        }

        public BaseServerInfo(ServerType serverType, int mainId, int subId, bool watchDog, string serverIp,
            int port, string clientIp, Dictionary<string, int> serverPort)
        {
            this.serverType = serverType;
            this.mainId = mainId;
            this.subId = subId;
            this.watchDog = watchDog;
            this.serverIp = serverIp;
            this.port = port;
            this.clientIp = clientIp;
            this.serverPort = serverPort;
            if (subId > 0)
            {
                serverName = string.Format("{0}_{1}_{2}", serverType.ToString(), mainId, subId);
            }
            else
            {
                serverName = string.Format("{0}_{1}", serverType.ToString(), mainId);
            }
            key = string.Format("{0}_{1}", mainId, subId);
        }

        public int GetServerPornt(ServerType type)
        {
            int port;
            serverPort.TryGetValue(type.ToString(), out port);
            return port;
        }
    }
}
