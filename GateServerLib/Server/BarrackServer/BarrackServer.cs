using System;
using System.Collections.Generic;
using System.IO;
using Engine;
using SocketShared;
using ServerShared;
using Logger;
using Message.Gate.Protocol.GateB;
using Message.IdGenerator;
using Message.Barrack.Protocol.BGate;
using DBUtility;
using ServerFrame;

namespace GateServerLib
{
    public class BarrackServer : BackendServer
    {
        private GateServerApi Api
        { get { return (GateServerApi)api; } }

        DateTime lastSendFpsTime = DateTime.MinValue;

        public int GateCount = 1;
        public int InGameTotalCount = 0;

        public BarrackServer(BaseApi api)
            : base(api)
        {
        }

        protected override void BindResponser()
        {
            base.BindResponser();
            AddResponser(Id<MSG_BGate_LOGIN>.Value, OnResponse_Login);
            AddResponser(Id<MSG_BGate_GatesInfo>.Value, OnResponse_GatesInfo);
            AddResponser(Id<MSG_BGate_Shutdown>.Value, OnResponse_ShutDown);
            //ResponserEnd
        }

        public override void Update(double dt)
        {
            base.Update(dt);
            try
            {
                if ((GateServerApi.now - lastSendFpsTime).TotalSeconds >= 5)
                {
                    MSG_GateB_FPS_INFO fpsInfo = new MSG_GateB_FPS_INFO();
                    fpsInfo.MainId = Api.MainId;
                    fpsInfo.GateId = Api.SubId;
                    fpsInfo.Fps = Api.Fps.GetFrame();
                    fpsInfo.SleepTime = Api.Fps.GetSleep();
                    fpsInfo.ClientCount = Api.ClientMng.CurCount;
                    fpsInfo.InGameCount = Api.ClientMng.InGameCount;
                    Write(fpsInfo);
                    lastSendFpsTime = GateServerApi.now;
                }
            }
            catch (Exception e)
            {
                Log.Alert(e.ToString());
            }
        }

        private void OnResponse_Login(MemoryStream stream, int uid = 0)
        {
            MSG_BGate_LOGIN msg = MessagePacker.ProtobufHelper.Deserialize<MSG_BGate_LOGIN>(stream);
            Log.Write("account {0} device {1} channel {2} token {3} login", msg.AccountId, msg.DeviceId,msg.ChannelName, msg.Token);
            Api.ClientMng.AddLoginClient(msg.AccountId, msg.DeviceId, msg.ChannelName, msg.Token, msg.SdkUuid, msg.IsRebate,
                msg.ChannelId, msg.Idfa, msg.Idfv, msg.Imei, msg.Imsi, msg.Anid, msg.Oaid, msg.PackageName, msg.ExtendId, msg.Caid, msg.MainId,
                msg.Tour, msg.Platform, msg.ClientVersion, msg.DeviceModel, msg.OsVersion, msg.Network, msg.Mac, msg.GameId);
        }

        private void OnResponse_GatesInfo(MemoryStream stream, int uid = 0)
        {
            MSG_BGate_GatesInfo msg = MessagePacker.ProtobufHelper.Deserialize<MSG_BGate_GatesInfo>(stream);
            Log.Write("gates count {0} in game total count {1}", msg.GatesCount, msg.InGameTotalCount);
            GateCount = msg.GatesCount;
            InGameTotalCount = msg.InGameTotalCount;
            Api.ClientMng.CalcLoginDeltaTime(InGameTotalCount);
        }

        private void OnResponse_ShutDown(MemoryStream stream, int uid = 0)
        {
            Log.Warn("barrack request shut down gate server");
            Api.StopServer(1);
        }

        // barrack 需要通知IP 和端口号
        protected override void SendRegistSpecInfo()
        {
            MSG_GateB_GATE_IP_INFO msg = new MSG_GateB_GATE_IP_INFO();
            msg.IP = api.ClientIp;
            msg.Port = api.Port.ToString();
            //msg.IP = api.ServerData.GetString("clientIp");
            //msg.Port = api.ServerData.GetString("port");
            Write(msg);
        }
    }
}