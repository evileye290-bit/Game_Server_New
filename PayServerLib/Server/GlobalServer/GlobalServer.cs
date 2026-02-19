using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DataProperty;
using Google.Protobuf.WellKnownTypes;
using Logger;
using Message.Barrack.Protocol.BG;
using Message.Global.Protocol.GB;
using Message.Global.Protocol.GP;
using Message.IdGenerator;
using Message.Pay.Protocol.PG;
using MessagePacker;
using ServerFrame;
using ServerModels;
using ServerShared;

namespace PayServerLib
{
    public class GlobalServer : BaseGlobalServer
    {

        private PayServerApi Api => (PayServerApi)api;

        public GlobalServer(BaseApi api) : base(api)
        {
        }

        protected override void BindResponser()
        {
            base.BindResponser();
            AddResponser(Id<MSG_GP_SHUTDOWN>.Value, OnResponse_Shutdown);
            AddResponser(Id<MSG_GP_SET_FPS>.Value, OnResponse_SetFPS);
            AddResponser(Id<MSG_GP_FPS_INFO>.Value, OnResponse_Fps_Info);
            AddResponser(Id<MSG_GP_UPDATE_XML>.Value, OnResponse_UpdateXml);
            AddResponser(Id<MSG_GP_UPDATE_GLOBAL>.Value, OnResponse_UpdateGlobal);
            //ResponserEnd
        }

        public void OnResponse_Shutdown(MemoryStream stream, int uid = 0)
        {
            Log.Warn("global request shutdown");
            Api.State = ServerState.Stopping;
            Api.StoppingTime = PayServerApi.now.AddMinutes(1);
        }

        private void OnResponse_SetFPS(MemoryStream stream, int uid = 0)
        {
            MSG_GP_SET_FPS msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GP_SET_FPS>(stream);
            Api.Fps.SetFPS(msg.FPS);

            MSG_BG_COMMAND_RESULT response = new MSG_BG_COMMAND_RESULT();
            response.Success = true;
            response.Info.Add(String.Format("setFps  barrck main {0} subId {1} successful", Api.MainId, Api.SubId));
            Write(response);
        }

        private void OnResponse_Fps_Info(MemoryStream stream, int uid = 0)
        {
            MSG_GP_FPS_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GP_FPS_INFO>(stream);
            FpsAndCpuInfo info = Api.Fps.GetFPSAndCpuInfo();
            MSG_PG_COMMAND_RESULT msg2global = new MSG_PG_COMMAND_RESULT();
            if (info == null)
            {
                msg2global.Success = false;
                msg2global.Info.Add(String.Format("pay main {0} subId {1} getfps fail", Api.MainId, Api.SubId));
            }
            else
            {
                msg2global.Success = true;
                msg2global.Info.Add(String.Format("pay main {0} subId {1} frame {2} sleep time {3} memory {4}", Api.MainId, Api.SubId, info.fps, info.sleepTime, info.memorySize));
            }
            Write(msg2global);
        }

        private void OnResponse_UpdateXml(MemoryStream stream, int uid = 0)
        {
            MSG_GP_UPDATE_XML msg = ProtobufHelper.Deserialize<MSG_GP_UPDATE_XML>(stream);
            if (msg.Type == 1)
            {
                Api.UpdateServerXml();
            }
            else
            {
                Api.UpdateXml();
            }
            Log.Write("GM update xml");
        }

        private void OnResponse_UpdateGlobal(MemoryStream stream, int uid = 0)
        {
            MSG_GP_UPDATE_GLOBAL msg = ProtobufHelper.Deserialize<MSG_GP_UPDATE_GLOBAL>(stream);

            DataList dataList = DataListManager.inst.GetDataList("AllGlobalInfo");
            if (dataList == null)
            {
                Log.Warn("had not find AllGlobalInfo");
                return;
            }

            foreach (var data in dataList)
            {
                int mainId = data.Value.GetInt("mainId");
                int subId = data.Value.GetInt("subId");
                BackendServer server = Api.BackendServerManager.GetServer(mainId, subId);
                if (server == null)
                {
                    BackendServer globalServer = Api.BackendServerManager.BuildBackendServer(ServerType.GlobalServer, data.Value);
                    globalServer.ConnectBackendServer();

                    Log.Warn($"create now global backend server mainId {mainId} subId {subId}");
                }
            }

            Log.Write("GM update global success !");
        }
    }

}
