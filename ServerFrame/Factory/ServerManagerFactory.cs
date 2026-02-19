using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ServerFrame
{
    public class ServerManagerFactory
    {
        public static BackendServerManager CreateBackendServerManager(BaseApi api, ServerType serverType)
        {
            // 先找有没有继承的ServerManager，如果没有则创建通用的ServerManager
            BackendServerManager serverManager = (BackendServerManager)(CreateBaseServerManager(api, serverType));
            if (serverManager == null)
            {
                serverManager = new BackendServerManager(api, serverType);
            }
            return serverManager;
        }

        public static FrontendServerManager CreateFrontendServerManager(BaseApi api, ServerType serverType)
        {
            // 先找有没有继承的ServerManager，如果没有则创建通用的ServerManager
            FrontendServerManager serverManager = (FrontendServerManager)(CreateBaseServerManager(api, serverType));
            if (serverManager == null)
            {
                serverManager = new FrontendServerManager(api, serverType);
            }
            return serverManager;
        }
        
        private static BaseServerManager CreateBaseServerManager(BaseApi api, ServerType serverType)
        {
            Assembly assembly = null;
            string assemblyName = Application.ProductName + "Lib";
            string className = assemblyName + "." + serverType.ToString() + "Manager";
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
                object[] parameters = new object[2];
                parameters[0] = (object)api;
                parameters[1] = (object)serverType;
                return (BaseServerManager)assembly.CreateInstance(className, false, BindingFlags.Default, null, parameters, null, null);
            }
            return null;
        }
    }
}
