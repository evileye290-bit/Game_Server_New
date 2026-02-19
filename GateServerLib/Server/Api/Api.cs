using ServerShared;
using Logger;
using ServerFrame;

namespace GateServerLib
{
    public partial class GateServerApi : BaseApi
    {
        // args [mainId subId]
        public override void Init(string[] args)
        {
            base.Init(args);

            InitWordChecker();
            InitAuthManager();

            InitClient();
            //InitChatMng();
#if DEBUG
            InitMsgCatchLog();
#endif
            // init阶段结束，起服完成
            InitDone();
        }


        public override void SpecUpdate(double dt)
        {
            TrackingLoggerMng.CheckNewLogFile(GateServerApi.now);
            BILoggerMng.CheckNewLogFile(now);
            clientManager.Update(lastTime);
            UID.ConvertTimestamp();
        }
        
        public override void StopServer(int min = 0)
        {
            if (State != ServerState.Stopped && State != ServerState.Stopping)
            {
                // 关闭所有客户端连接 并且禁止新客户端连接
                base.StopServer(min);
                clientManager.DestroyAllClients();
            }
        }

    }
}
