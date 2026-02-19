using Logger;
using ServerShared;
using System;
using System.Net;

namespace ServerFrame
{
    public class BaseGlobalServer : BackendServer
    {
        public BaseGlobalServer(BaseApi api)
            : base(api)
        {
        }

        public override void InitDone()
        {
            Log.Write("{0} init done, connect to {1}", api.ServerName, ServerName);
#if DEBUG
            if (IsIP(serverTcp.IP))
            {
                serverTcp.Connect();
            }
            else
            {
                serverTcp.Connect(Dns.GetHostAddresses(serverTcp.IP)[0].ToString(), serverTcp.Port);
            }

#else
            if (IsIP(serverTcp.IP))
            {
                Logger.Log.Warn("global ip {0} is not domainName ,check the config", serverTcp.IP);
                serverTcp.Connect();
            }
            else
            {
                serverTcp.Connect(Dns.GetHostAddresses(serverTcp.IP)[0].ToString(), serverTcp.Port);
            }
#endif
        }

        // 与Global重连涉及到域名解析，故不能复用base.OnDisconnect
        protected override void OnDisconnet()
        {
            string log = string.Format("disconnect from {0}, connect ip {1} port {2} again",
                ServerName, serverTcp.IP, serverTcp.Port);
            lock (LogList[LogType.ERROR])
            {
                LogList[LogType.ERROR].Enqueue(log);
            }
            state = ServerState.DisConnect;
            SetTcpAlive(false);
#if DEBUG
            if (IsIP(serverTcp.IP))
            {
                serverTcp.Connect();
            }
            else
            {
                Dns.BeginGetHostAddresses(serverTcp.IP, ConnectAsync, null);
            }
#else
            Dns.BeginGetHostAddresses(serverTcp.IP,ConnectAsync, null);
#endif
        }

        public void ConnectAsync(IAsyncResult ar)
        {
            try
            {
                IPAddress[] IPs = Dns.EndGetHostAddresses(ar);
                string globalIp = IPs[0].ToString();
                Logger.Log.Warn("ConnectAsync : get ip from  DNS : {0}", globalIp);
                serverTcp.Connect(globalIp, serverTcp.Port);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ConnectAsync error:" + ex);
            }
        }
    }
}
