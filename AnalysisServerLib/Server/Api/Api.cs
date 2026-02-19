using DataProperty;
using ServerFrame;
using ServerShared;
using System;

namespace AnalysisServerLib
{
    public partial class AnalysisServerApi : BaseApi
    {
        public DateTime OpenServerDate { get; set; }
        public DateTime OpenServerTime { get; set; }

        public override void Init(string[] args)
        {
            base.Init(args);

            // init完毕，完成起服
            InitDone();
        }

        public override void InitDone()
        {
            base.InitDone();
            InitOpenServerTime();
        }

        private static void InitLibrarys()
        {
            ActionLibrary.Init(now);
            RechargeLibrary.Init();
        }

        public override void StopServer(int min = 0)
        {
            if (State != ServerState.Stopped && State != ServerState.Stopping)
            {
                // 关闭所有客户端连接 并且禁止新客户端连接
                base.StopServer(min);
            }
        }

        private void InitOpenServerTime()
        {
            Data serverListData = DataListManager.inst.GetData("ServerList", MainId);
            if (serverListData == null)
            {
                OpenServerTime = DateTime.MaxValue;
            }
            else
            {
                string time = serverListData.GetString("openTime");
                if (time == string.Empty)
                {
                    OpenServerTime = DateTime.MaxValue;
                }
                else
                {
                    OpenServerTime = DateTime.Parse(time);
                }
            }
            OpenServerDate = OpenServerTime.Date;
        }
    }
}
