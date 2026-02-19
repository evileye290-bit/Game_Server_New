using System;
using System.Collections.Generic;
using System.IO;
using Engine;
using Message.IdGenerator;
using ServerShared;
using SocketShared;
using Logger;
using Message.Manager.Protocol.MM;
using ServerShared.Map;
using Message.Manager.Protocol.MZ;
using ServerFrame;

namespace ManagerServerLib
{
    public class BackendManagerServer : BackendServer
    {
        ManagerServerResponser serverResponser;

        public BackendManagerServer(BaseApi api)
            : base(api)
        {
            serverResponser = new ManagerServerResponser(api, this);
        }


        protected override void BindResponser()
        {
            base.BindResponser();
            //ResponserEnd
        }

        public override void OnResponse(uint id, MemoryStream stream, int uid)
        {
            serverResponser.OnResponse(id, stream, uid);
        }
    }
}
