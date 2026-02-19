using System;
using ServerFrame;
using ServerShared;

namespace PayServerLib
{
    public partial class PayServerApi : BaseApi
    {
        private VMallServer vMallServer;
        private PayServerBase payServer = null;
        private PayServerBase payServer_Huawei = null;
        private DateTime serverListUpDateTime = DateTime.Now;

        private PayInfoManager payInfoMng = null;
        public PayInfoManager PayInfoMng => payInfoMng;

        // args [path]
        public override void Init(string[] args)
        {
            base.Init(args);
            GameConfig.InitGameCongfig();

            InitSDK();
            InitPayInfoManager();
            InitServerList();
            VMallHelper.InitConfigData();

            // init完毕，完成起服
            InitDone();
        }

        public override void InitDone()
        {
            base.InitDone();

            payServer?.NotifyInitDone();
            vMallServer?.NotifyInitDone();

            payServer_Huawei?.NotifyInitDone();
        }

        public override void SpecUpdate(double dt)
        {
            UpdatePayServer();
            UpdateVMallServer();

            UpdateServerList();
        }

        public override void StopServer(int min = 0)
        {
            if (State != ServerState.Stopped && State != ServerState.Stopping)
            {
                // 关闭所有客户端连接 并且禁止新客户端连接
                base.StopServer(min);
            }
        }


        void UpdatePayServer()
        {
            PayInfoMng.UpdatePayInfo();

            payServer?.Update();

            payServer_Huawei?.Update();
        }

        void UpdateVMallServer()
        {
           vMallServer?.Update();
        }

        private void UpdateServerList()
        {
            if ((BaseApi.now - serverListUpDateTime).TotalSeconds > 10)
            {
                serverListUpDateTime = BaseApi.now;
                InitServerList();
            }
        }

        public int GetDestServerMainId(int id)
        {
            if (id > 1000)
            {
                return id;
            }
            else
            {
                int temp = MainId / 1000;
                return temp * 1000 + id;
            }
        }


        public VMallSession GetCacheHttpSession(int sessionUid)
        {
            return vMallServer.GetCacheHttpSession(sessionUid);
        }
    }

}
