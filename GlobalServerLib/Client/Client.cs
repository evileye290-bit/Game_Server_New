using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalServerLib
{
    partial class Client
    {
        public Client(GlobalServerApi api)
        {
            this.api = api;
            InitTcp();
            BindResponser();
        }
        public int Uid = 0;
        private GlobalServerApi api;
        public DateTime LastConnectTime = DateTime.Now;
    }
}
