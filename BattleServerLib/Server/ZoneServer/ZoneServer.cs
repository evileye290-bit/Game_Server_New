using System;
using System.Collections.Generic;
using System.IO;
using Engine;
using SocketShared;
using ServerShared;
using Logger;
using Message.IdGenerator;
using Message.Battle.Protocol.BattleZ;
using Message.Zone.Protocol.ZBattle;
using DBUtility;
using ServerFrame;


namespace BattleServerLib
{
    public partial class ZoneServer:BackendServer
    {
        private BattleServerApi Api
        {get {return (BattleServerApi)api;}}


        public ZoneServer(BaseApi api):base(api)
        {       
            
        }

        protected override void BindResponser()
        {
            base.BindResponser();
         
            //ResponserEnd
        }

    }
}