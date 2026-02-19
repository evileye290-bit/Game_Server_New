using ServerShared;
using Logger;
using Message.Battle.Protocol.BattleBM;
using ServerFrame;

namespace BattleServerLib
{
    public partial class BattleServerApi : BaseApi
    {
        // args [mainId]
        public override void Init(string[] args)
        {
            base.Init(args);

            InitFieldMapManager();
            // 录像上传线程启动
            InitVedioUploaer();

            // init阶段结束，起服完成
            InitDone();
        }

        public override void SpecUpdate(double dt)
        {
            recordFrameInfo();
        }

        // 统计最近10秒内的每秒内帧数和CPU睡眠时间，反映当前进程状态
        public void recordFrameInfo()
        {
            FpsAndCpuInfo info = Fps.GetFPSAndCpuInfo();
            if (info == null)
            {
                return;
            }

            MSG_BattleBM_CPU_FPS_INFO msg = new MSG_BattleBM_CPU_FPS_INFO();
            msg.FrameCount = (int)info.fps;
            msg.SleepTime = (int)info.sleepTime;
            msg.Memory = info.memorySize;

            if (BattleManagerServer != null)
            {
                BattleManagerServer.Write(msg);
            }
        }

    }
}
