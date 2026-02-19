using DBUtility;
using Engine;
using Message.IdGenerator;
using Logger;
using Message.Battle.Protocol.BattleBM;
using ServerFrame;
using ServerShared;
using SocketShared;
using System;
using System.Collections.Generic;
using System.IO;

namespace BattleManagerServerLib
{
    public partial class BattleServer : FrontendServer
    {
        private BattleManagerServerApi Api
        { get { return (BattleManagerServerApi)api; } }
        private int sleepTime;
        public int SleepTime { get { return sleepTime; } }

        private int frameCount;
        public int FrameCount { get { return frameCount; } }

        private long memory;
        public long Memory { get { return memory; } } 

        public int Battlegroundcount { get; set; }

        public BattleServer(BaseApi api)
            : base(api)
        {
        }

        protected override void BindResponser()
        {
            base.BindResponser();
            AddResponser(Id<MSG_BattleBM_CPU_FPS_INFO>.Value, OnResponse_Cpu_FPS_Info);
            //ResponserEnd
        }

        public void OnResponse_Cpu_FPS_Info(MemoryStream stream, int uid = 0)
        {
            MSG_BattleBM_CPU_FPS_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_BattleBM_CPU_FPS_INFO>(stream);
            sleepTime = msg.SleepTime;
            frameCount = msg.FrameCount;
            memory = msg.Memory;
            Battlegroundcount = msg.MapCount;
            Api.BattleServerManager.CalcBattleServer();
            //Log.WriteLine("battle mainId {0} subId {1} frame count {2} sleep time {3} memory {4}MB", mainId, subId, frameCount, sleepTime, memory);
        }

    }
}
