using Logger;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateG;
using Message.Global.Protocol.GGate;
using Message.IdGenerator;
using ServerFrame;
using ServerShared;
using System;
using System.IO;

namespace GateServerLib
{
    public class GlobalServer : BaseGlobalServer
    {
        private GateServerApi Api
        { get { return (GateServerApi)api; } }

        public GlobalServer(BaseApi api)
            : base(api)
        {
        }


        protected override void BindResponser()
        {
            base.BindResponser();
            AddResponser(Id<MSG_GGATE_SET_FPS>.Value, OnResponse_SetFps);
            AddResponser(Id<MSG_GGATE_SHUTDOWN_GATE>.Value, OnResponse_ShutDown_Gate);
            AddResponser(Id<MSG_GGate_GATE_FPS_INFO>.Value, OnResponse_Gate_Fps_Info);
            AddResponser(Id<MSG_GGate_UPDATE_XML>.Value, OnResponse_UpdateXml);
            AddResponser(Id<MSG_GGate_ANNOUNCEMENT>.Value, OnResponse_CustomAnnouncement);
            AddResponser(Id<MSG_GGate_WHITE_LIST>.Value, OnResponse_WhiteList);
            AddResponser(Id<MSG_GGate_WAIT_COUNT>.Value, OnResponse_WaitCount);
            AddResponser(Id<MSG_GGate_FULL_COUNT>.Value, OnResponse_FullCount);
            AddResponser(Id<MSG_GGate_WAIT_SEC>.Value, OnResponse_WaitSec);
            //ResponserEnd
        }

        private void OnResponse_SetFps(MemoryStream stream, int uid = 0)
        {
            MSG_GGATE_SET_FPS msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GGATE_SET_FPS>(stream);
            Api.Fps.SetFPS(msg.FPS);
            MSG_GateG_COMMAND_RESULT response = new MSG_GateG_COMMAND_RESULT();
            response.Success = true;
            response.Info.Add(String.Format("setgateFps main {0} gateId {1} successful", Api.MainId, Api.SubId));
            Write(response);
        }

        private void OnResponse_ShutDown_Gate(MemoryStream stream, int uid = 0)
        {
            Log.Warn("global request shutdown gate");
            CONST.ALARM_OPEN = false;
            Api.StopServer(1);

            MSG_GateG_COMMAND_RESULT msg2global = new MSG_GateG_COMMAND_RESULT();
            msg2global.Success = false;
            msg2global.Info.Add(String.Format("battle main {0} sub {1} frame {2} sleep time {3} memory{4}",
         Api.MainId, Api.SubId, 0, 0, 0));
            Write(msg2global);
        }


        private void OnResponse_Gate_Fps_Info(MemoryStream stream, int uid = 0)
        {
            MSG_GGate_GATE_FPS_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GGate_GATE_FPS_INFO>(stream);
            FpsAndCpuInfo info = Api.Fps.GetFPSAndCpuInfo();
            MSG_GateG_COMMAND_RESULT msg2global = new MSG_GateG_COMMAND_RESULT();
            if (info == null)
            {
                msg2global.Success = false;
                msg2global.Info.Add(String.Format("gate main {0} sub {1} getfps fail",
                Api.MainId, Api.SubId));
            }
            else
            {
                msg2global.Success = true;
                msg2global.Info.Add(String.Format("gate main {0} sub {1} frame {2} sleep time {3} memory {4} player count {5}",
                Api.MainId, Api.SubId, info.fps, info.sleepTime, info.memorySize, Api.ClientMng.ClientList.Count));
            }
            Write(msg2global);
        }


        private void OnResponse_UpdateXml(MemoryStream stream, int uid = 0)
        {
            MSG_GGate_UPDATE_XML msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GGate_UPDATE_XML>(stream);
            if (msg.Type == 1)
            {
                //Api.UpdateServerXml();
            }
            else
            {
                Api.UpdateXml();
            }
            Log.Write("GM update xml");
        }


        private void OnResponse_CustomAnnouncement(MemoryStream stream, int uid = 0)
        {
            MSG_GGate_ANNOUNCEMENT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GGate_ANNOUNCEMENT>(stream);
            MSG_GC_ANNOUNCEMENT notify = new MSG_GC_ANNOUNCEMENT();
            notify.Type = msg.Type;
            //notify.Bottom = msg.Bottom;
            foreach (var item in msg.List)
            {
                notify.List.Add(item);
            }
            ArraySegment<byte> head;
            ArraySegment<byte> body;
            Client.BroadCastMsgMemoryMaker(notify, out head, out body);
            Api.ClientMng.Broadcast(head, body);
        }

        private void OnResponse_WhiteList(MemoryStream stream, int uid = 0)
        {
            MSG_GGate_WHITE_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GGate_WHITE_LIST>(stream);
            Log.Write("global request white list {0}", msg.Open.ToString());
            Api.AuthMng.SetCheckWhite(msg.Open);
        }

        private void OnResponse_WaitCount(MemoryStream stream, int uid = 0)
        {
            MSG_GGate_WAIT_COUNT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GGate_WAIT_COUNT>(stream);
            Log.Info("global request set wait count {0}", msg.Count);
            CONST.ONLINE_COUNT_WAIT_COUNT = msg.Count;
            if (Api.BarrackServerWatchDog != null)
            {
                Api.ClientMng.CalcLoginDeltaTime(Api.BarrackServerWatchDog.InGameTotalCount);
            }
        }

        private void OnResponse_FullCount(MemoryStream stream, int uid = 0)
        {
            MSG_GGate_FULL_COUNT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GGate_FULL_COUNT>(stream);
            Log.Info("global request set full count {0}", msg.Count);
            CONST.ONLINE_COUNT_FULL_COUNT = msg.Count;
            if (Api.BarrackServerWatchDog != null)
            {
                Api.ClientMng.CalcLoginDeltaTime(Api.BarrackServerWatchDog.InGameTotalCount);
            }
        }

        private void OnResponse_WaitSec(MemoryStream stream, int uid = 0)
        {
            MSG_GGate_WAIT_SEC msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GGate_WAIT_SEC>(stream);
            Log.Info("global request set wait sec {0}", msg.Second);
            CONST.LOGIN_QUEUE_PERIOD = msg.Second * 1000;
            if (Api.BarrackServerWatchDog != null)
            {
                Api.ClientMng.CalcLoginDeltaTime(Api.BarrackServerWatchDog.InGameTotalCount);
            }
        }
    }
}