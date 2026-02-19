using Logger;
using Message.Shared.Protocol.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerFrame
{
    public class KeepaliveManager
    {
        private DateTime lastKeepaliveTime;
        private int keepaliveCount;
        private const int baseDeltaTime = 64;
        private BaseServer server;
        public KeepaliveManager(BaseServer server)
        {
            this.server = server;
            Alive();
        }

        public void Alive()
        {
            lastKeepaliveTime = BaseApi.now;
            keepaliveCount = 0;
        }

        public bool CheckAlive()
        {
            int deltaTime = baseDeltaTime >> (keepaliveCount);
            if(deltaTime <= 0)
            {
                return false;
            }
            if((BaseApi.now - lastKeepaliveTime).TotalSeconds > deltaTime)
            {
                keepaliveCount++;
                lastKeepaliveTime = BaseApi.now;
                Log.Write($"keep alive to {server.ToString()} count {keepaliveCount}");
                KeepalivePing();
            }
            return true;
        }

        protected void KeepalivePing()
        {
            MSG_KEEPALIVE_PING msg = new MSG_KEEPALIVE_PING();
            server.Write(msg);
        }
    }
}
