using System;
using System.Reflection;
using System.Windows.Forms;

namespace ServerFrame
{
    public class ServerFactory
    {
        // serverType 与 serverData是主动connect目标server的数据
        public static BackendServer CreateBackendServer(BaseApi api, BaseServerInfo info, BackendServerManager serverManager)
        {
            BackendServer server = (BackendServer)(CreateBaseServer(api, (ServerType)info.ServerType, string.Empty));
            if (server == null)
            {
                server = (BackendServer)(CreateBaseServer(api, (ServerType)info.ServerType, "Backend"));
            }
            server.InitBaseInfo(info);
            server.InitServerManager(serverManager);
            server.InitNetwork(info.ServerIp, (ushort)info.GetServerPornt(api.ServerType));
            return server;
        }

        // serverType 是等待其主动connect的froentend side server的数据
        public static FrontendServer CreateFrontendServer(BaseApi api, ServerType serverType, FrontendServerManager serverManager, ushort port)
        {
            FrontendServer server = (FrontendServer)(CreateBaseServer(api, serverType, string.Empty));
            if (server == null)
            {
                server = (FrontendServer)(CreateBaseServer(api, serverType, "Frontend"));
            }
            server.InitServerManager(serverManager);
            server.InitNetwork(api.ClientIp, port);
            return server;
        }

        private static BaseServer CreateBaseServer(BaseApi api, ServerType serverType, string prefix)
        {
            Assembly assembly = null;
            string assemblyName = Application.ProductName + "Lib";
            string className = assemblyName + "." + prefix +  serverType.ToString();
            Assembly[] asses = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var ass in asses)
            {
                if (ass.ToString().Contains(assemblyName))
                {
                    assembly = ass;
                    break;
                }
            }
            if (assembly == null)
            {
                assembly = Assembly.Load(assemblyName);
            }
            if (assembly != null)
            {
                object[] parameters = new object[1];
                parameters[0] = (object)api;
                return (BaseServer)assembly.CreateInstance(className, false, BindingFlags.Default, null, parameters, null, null );
            }
            return null;
        }
    }
}
