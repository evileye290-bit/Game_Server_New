using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Message.IdGenerator;
using ServerShared;
using Message.Global.Protocol.GB;
using Logger;
using DataProperty;
using Message.Barrack.Protocol.BG;
using ServerFrame;
using MessagePacker;
using EnumerateUtility;
using ServerModels;

namespace BarrackServerLib
{
    public class GlobalServer : BaseGlobalServer
    {

        private BarrackServerApi Api
        { get { return (BarrackServerApi)api; } }

        public GlobalServer(BaseApi api) : base(api)
        {
        }

        protected override void BindResponser()
        {
            base.BindResponser();
            AddResponser(Id<MSG_GB_REFRESH_ANNOUNCEMENT>.Value, OnResponse_Announcement);
            AddResponser(Id<MSG_GB_CHECK_AUTH>.Value, OnResponse_CheckAuth);
            AddResponser(Id<MSG_GB_SHUTDOWN>.Value, OnResponse_Shutdown);
            AddResponser(Id<MSG_GB_BLACKLIST_RELOAD>.Value, OnResponse_BlackListReload);
            AddResponser(Id<MSG_GB_VERSION_RELOAD>.Value, OnResponse_VersionReload);
            AddResponser(Id<MSG_GB_RECOMMEND_SERVERS>.Value, OnResponse_RecommendServers);
            AddResponser(Id<MSG_GB_SET_FPS>.Value, OnResponse_SetFPS);
            AddResponser(Id<MSG_GB_FPS_INFO>.Value, OnResponse_Fps_Info);
            AddResponser(Id<MSG_GB_ALL_GATE_INFO>.Value, OnResponse_AllGateInfo);
            AddResponser(Id<MSG_GB_SHUTDOWN_GATE>.Value, OnResponse_ShutDownGate);
            AddResponser(Id<MSG_GB_WHITE_LIST>.Value, OnResponse_WhiteList);
            AddResponser(Id<MSG_GB_UPDATE_XML>.Value, OnResponse_UpdateXml);
            AddResponser(Id<MSG_GB_UPDATE_NEW_SERVER>.Value, OnResponse_NewServer);
            AddResponser(Id<MSG_GB_UPDATE_LINEUP_SERVER>.Value, OnResponse_LineupServers);
            AddResponser(Id<MSG_GB_UPDATE_RECOMMEND_SERVER>.Value, OnResponse_RecommendServer);

            AddResponser(Id<MSG_GB_GET_SERVER_STATE>.Value, OnResponse_GetServerState);
            AddResponser(Id<MSG_GB_SET_SERVER_STATE>.Value, OnResponse_SetServerState);
            //ResponserEnd
        }

        public void OnResponse_Announcement(MemoryStream stream, int uid = 0)
        {
            Log.Write("global request reload annoucement");
            //Api.HasAnnouncement = false;
            //server.DB.Call(new QueryAnnouncement(), ret =>
            //{
            //    server.AnnouncementList = (List<Announcement>)ret;
            //    server.InitAnnouncement();
            //});
        }

        public void OnResponse_CheckAuth(MemoryStream stream, int uid = 0)
        {
            MSG_GB_CHECK_AUTH msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GB_CHECK_AUTH>(stream);
            Log.Write("global request check auth {0}", msg.Check);
            //server.AuthCheck = msg.check;
        }

        public void OnResponse_Shutdown(MemoryStream stream, int uid = 0)
        {
            Log.Warn("global request shutdown");
            Api.State = ServerState.Stopping;
            Api.StoppingTime = BarrackServerApi.now.AddMinutes(1);
        }

        public void OnResponse_BlackListReload(MemoryStream stream, int uid = 0)
        {
            Log.Write("global server request reload blacklist");
        }

        public void OnResponse_VersionReload(MemoryStream stream, int uid = 0)
        {
            Log.Write("global server request reload version");
            string[] files = Directory.GetFiles(CommonUtility.PathExt.FullPathFromServerData("XML/Config"), "VersionConfig.xml", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                DataListManager.inst.DataLists.Remove("VersionConfig");
                DataListManager.inst.Parse(file);
            }
        }

        public void OnResponse_RecommendServers(MemoryStream stream, int uid = 0)
        {
            MSG_GB_RECOMMEND_SERVERS msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GB_RECOMMEND_SERVERS>(stream);
            if (msg.Servers.Count == 0)
            {
                return;
            }
            string recommendServers = "";
            for (int i = 0; i < msg.Servers.Count; i++)
            {
                if (i < msg.Servers.Count - 1)
                {
                    recommendServers += i + "|";
                }
                else
                {
                    recommendServers += i;
                }
            }
            //server.DB.Call(new QueryUpdateRecommendServer(recommendServers), "announcement",DBOperateType.Write, ret1 =>
            //{
            //    server.DB.Call(new QueryAnnouncement(), "announcement", DBOperateType.Read, ret2 =>
            //    {
            //        server.AnnouncementList = (List<Announcement>)ret2;
            //        server.InitAnnouncement();
            //    });
            //});
        }

        private void OnResponse_SetFPS(MemoryStream stream, int uid = 0)
        {
            MSG_GB_SET_FPS msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GB_SET_FPS>(stream);
            Api.Fps.SetFPS(msg.FPS);

            MSG_BG_COMMAND_RESULT response = new MSG_BG_COMMAND_RESULT();
            response.Success = true;
            response.Info.Add(String.Format("setFps  barrck main {0} subId {1} successful", Api.MainId, Api.SubId));
            Write(response);
        }

        private void OnResponse_Fps_Info(MemoryStream stream, int uid = 0)
        {
            MSG_GB_FPS_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GB_FPS_INFO>(stream);
            FpsAndCpuInfo info = Api.Fps.GetFPSAndCpuInfo();
            MSG_BG_COMMAND_RESULT msg2global = new MSG_BG_COMMAND_RESULT();
            if (info == null)
            {
                msg2global.Success = false;
                msg2global.Info.Add(String.Format("barrack main {0} subId {1} getfps fail", Api.MainId, Api.SubId));
            }
            else
            {
                msg2global.Success = true;
                msg2global.Info.Add(String.Format("barrack main {0} subId {1} frame {2} sleep time {3} memory {4}",
                Api.MainId, Api.SubId, info.fps, info.sleepTime, info.memorySize));
            }
            Write(msg2global);
        }

        private void OnResponse_AllGateInfo(MemoryStream stream, int uid = 0)
        {
            MSG_GB_ALL_GATE_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GB_ALL_GATE_INFO>(stream);
            MSG_BG_ALL_GATE_INFO response = new MSG_BG_ALL_GATE_INFO();
            int totalClientCount = 0;
            int totalInGameCount = 0;
            foreach (var item in Api.GateServerManager.ServerList.Values)
            {
                GateServer gate = (GateServer)item;
                string info = String.Format("gate {0} frame {1} sleep time {2} client count {3} in game count {4}",
                gate.SubId, gate.Fps, gate.SleepTime, gate.ClientCount, gate.InGameCount);
                totalClientCount += gate.ClientCount;
                totalInGameCount += gate.InGameCount;
                response.Infos.Add(info);
            }
            response.Infos.Add(String.Format("total client count {0}, total in game count {1}", totalClientCount, totalInGameCount));
            Write(response);
        }

        private void OnResponse_ShutDownGate(MemoryStream stream, int uid = 0)
        {
            MSG_GB_SHUTDOWN_GATE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GB_SHUTDOWN_GATE>(stream);
            Log.Warn("global request shut down gate {0}", msg.GateId);
            if (msg.GateId != 0)
            {
                GateServer gate = (GateServer)Api.GateServerManager.GetServer(api.ClusterId, msg.GateId);
                if (gate != null)
                {
                    gate.Shutdown();
                }
            }
            else
            {
                foreach (var item in Api.GateServerManager.ServerList.Values)
                {
                    GateServer gate = (GateServer)item;
                    gate.Shutdown();
                }
            }
        }

        private void OnResponse_WhiteList(MemoryStream stream, int uid = 0)
        {
            MSG_GB_WHITE_LIST msg = ProtobufHelper.Deserialize<MSG_GB_WHITE_LIST>(stream);
            Log.Write("global request white list {0}", msg.Open.ToString());
            Api.AuthMng.SetCheckWhite(msg.Open);
        }

        private void OnResponse_UpdateXml(MemoryStream stream, int uid = 0)
        {
            MSG_GB_UPDATE_XML msg = ProtobufHelper.Deserialize<MSG_GB_UPDATE_XML>(stream);
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

        private void OnResponse_NewServer(MemoryStream stream, int uid = 0)
        {
            MSG_GB_UPDATE_NEW_SERVER msg = ProtobufHelper.Deserialize<MSG_GB_UPDATE_NEW_SERVER>(stream);
            msg.Servers.ForEach(x => Api.SetNewServer(x));
            Log.Write($"GM set new servers {string.Join("|", msg.Servers)}");
        }

        private void OnResponse_LineupServers(MemoryStream stream, int uid = 0)
        {
            MSG_GB_UPDATE_LINEUP_SERVER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GB_UPDATE_LINEUP_SERVER>(stream);
            Api.SetLineUpServer(msg.Servers);
            Log.Write($"GM set line up servers {string.Join("|", msg.Servers)}");
        }

        private void OnResponse_RecommendServer(MemoryStream stream, int uid = 0)
        {
            MSG_GB_UPDATE_RECOMMEND_SERVER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GB_UPDATE_RECOMMEND_SERVER>(stream);
            msg.Servers.ForEach(x => Api.SetRecommendServer(x));
            Log.Write($"GM set recommend servers {string.Join("|", msg.Servers)}");
        }

        private void OnResponse_GetServerState(MemoryStream stream, int uid = 0)
        {
            MSG_GB_GET_SERVER_STATE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GB_GET_SERVER_STATE>(stream);

            List<MSG_SERVER_INFO> serverInfos = new List<MSG_SERVER_INFO>();
            MSG_BG_GET_SERVER_STATE response = new MSG_BG_GET_SERVER_STATE() {Uid = msg.Uid, Page = msg.Page};

            if (Api.ServersConfig.List.Count == 0)
            {
                Write(response, msg.Uid);
                return;;
            }


            int page = Math.Max(msg.Page, 1);
            int start = Math.Max((page - 1) * msg.PageCount, 0);

            var orderList = Api.ServersConfig.List
                .Where(x => x.Key >= msg.ServerBegin && x.Key<=msg.ServerEnd)
                .OrderByDescending(x => x.Key)
                .ToList();

            response.ServerCount = orderList.Count;
            for (int i = 0; i < msg.PageCount; i++)
            {
                int index = start + i;
                if (orderList.Count <= index) break;

                ServerItemModel model = orderList[index].Value;
                MSG_SERVER_INFO serverInfo = new MSG_SERVER_INFO()
                {
                    ServerId = model.Id,
                    IsOpening = Api.IsOpening(model.Id),
                    IsRecommend = Api.IsRecommendServer(model.Id),
                    IsMainTaining = Api.IsMaintainingServer(model.Id)
                };

                ManagerServer manager = Api.ManagerServerManager.GetSinglePointServer(model.Id) as ManagerServer;
                if (manager != null)
                {
                    serverInfo.RegistCount = manager.RegistCharacterCount;
                }

                serverInfos.Add(serverInfo);
            }

            serverInfos.OrderByDescending(x => x.ServerId).ForEach(x=>response.ServerInfos.Add(x));

            Write(response, msg.Uid);
        }

        private void OnResponse_SetServerState(MemoryStream stream, int uid = 0)
        {
            MSG_GB_SET_SERVER_STATE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GB_SET_SERVER_STATE>(stream);
            MSG_BG_SET_SERVER_STATE response = new MSG_BG_SET_SERVER_STATE() { Uid = msg.Uid, Result = (int)ErrorCode.Success };

            if (msg.MaintainAll)
            {
                Api.SetMaintainingState(true);
                Write(response, msg.Uid);
                return;
            }

            if (msg.CancelMaintainAll)
            {
                Api.SetMaintainingState(false);
            }

            if (msg.Recommend > 0)
            {
                Api.SetRecommendServer(msg.Recommend);
            }

            if (msg.Mintain > 0)
            {
                Api.SetMaintainingServer(msg.Mintain);
            }

            if (msg.Open > 0)
            {
                Api.SetOpeningServer(msg.Open);
            }

            if (msg.New > 0)
            {
                Api.SetNewServer(msg.New);
            }

            Log.Write("GM set SetServerState {0}", msg);

            Write(response, msg.Uid);
        }

    }
}
