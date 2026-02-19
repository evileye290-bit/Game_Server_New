using System;
using System.Collections.Generic;
using ServerShared;
using Logger;
using System.IO;
using Message.IdGenerator;
using Message.Barrack.Protocol.BG;
using DBUtility;
using ServerFrame;

namespace GlobalServerLib
{
    /// <summary>
    /// 服务器封装，保存了进程引用
    /// </summary>
    public partial class CrossServer : FrontendServer
    {
        GlobalServerApi Api
        { get { return (GlobalServerApi)api; } }

        public CrossServer(BaseApi api)
            : base(api)
        {
        }

        protected override void BindResponser()
        {
            base.BindResponser();
           
            //ResponseEnd
        }
    }
}