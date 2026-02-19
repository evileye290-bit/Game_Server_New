using DBUtility;
using Message.IdGenerator;
using Logger;
using Message.Battle.Protocol.BattleG;
using ServerFrame;
using ServerShared;
using System;
using System.Collections.Generic;
using System.IO;

namespace GlobalServerLib
{
    public partial class BattleServer : FrontendServer
    {
        GlobalServerApi Api
        { get { return (GlobalServerApi)api; } }

        public BattleServer(BaseApi api)
            : base(api)
        {
        }

        protected override void BindResponser()
        {
            base.BindResponser();
        }

    }
}
