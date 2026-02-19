using DataProperty;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerFrame
{
    public class BaseServerManager
    {
        protected BaseApi api;
        public BaseApi Api
        { get { return api; } }

        protected ServerType serverType = ServerType.Invalid;
        public ServerType ServerType
        { get { return serverType; } }
        public BaseServerManager(BaseApi api, ServerType serverType)
        {
            this.api = api;
            this.serverType = serverType;
        }

        public virtual void UpdateServers(double dt)
        { 
        }

        public int GetRedirectId(int mainId)
        {
            var data = DataListManager.inst.GetData("ServerListRedirect", mainId);
            if (data != null)
            {
                mainId = data.GetInt("Redirect");
            }

            return mainId;
        }
    }
}
