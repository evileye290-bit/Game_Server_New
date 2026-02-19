using System.Collections.Generic;
using DBUtility;
using Logger;
using Message.Barrack.Protocol.BG;
using Message.IdGenerator;
using ServerFrame;
using ServerModels;
using ServerShared;
using System.IO;
using System.Web.Script.Serialization;

namespace GlobalServerLib
{
    /// <summary>
    /// 服务器封装，保存了进程引用
    /// </summary>
    public partial class BarrackServer : FrontendServer
    {
        private GlobalServerApi Api => (GlobalServerApi)api;

        public BarrackServer(BaseApi api)
            : base(api)
        {
        }

        protected override void BindResponser()
        {
            base.BindResponser();
            AddResponser(Id<MSG_BG_REGISTER>.Value, OnResponse_Regist);
            AddResponser(Id<MSG_BG_ALARM_NOTIFY>.Value, OnResponse_AlarmNotify);
            AddResponser(Id<MSG_BG_HEARTBEAT>.Value, OnResponse_Heartbeat);
            AddResponser(Id<MSG_BG_COMMAND_RESULT>.Value, OnResponse_CommandResult);
            AddResponser(Id<MSG_BG_ALL_GATE_INFO>.Value, OnResponse_AllGateInfo);
            AddResponser(Id<MSG_BG_KTPLAY_INFOS>.Value, OnResponse_KTPlayInfo);
            AddResponser(Id<MSG_BG_GET_SERVER_STATE>.Value, OnResponse_GetServerState);
            AddResponser(Id<MSG_BG_SET_SERVER_STATE>.Value, OnResponse_SetServerState);
            //ResponseEnd
        }

        private void OnResponse_Regist(MemoryStream stream, int uid = 0)
        {
            //MSG_BG_REGISTER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_BG_REGISTER>(stream);
            //Log.Write("barrack {0} regist to global", msg.MainId);
            //InitBaseInfo(msg.MainId, 0);
            //serverManager.AddServer(this);
            //registInfo.Regist(msg.MainId, msg.Ip, msg.managerPort, msg.gatePort);
            //Api.OnBarrackRegist(this);
        }

        private void OnResponse_AlarmNotify(MemoryStream stream, int uid = 0)
        {
            MSG_BG_ALARM_NOTIFY msg = MessagePacker.ProtobufHelper.Deserialize<MSG_BG_ALARM_NOTIFY>(stream);
            //Log.Warn(msg.Content);
            Api.AccountDBPool.Call(new QueryAlarm(msg.Type, msg.Main, msg.Sub, msg.Time, msg.Content));
            switch ((AlarmType)msg.Type)
            {
                case AlarmType.DB:
                    //DBExceptionList.Add(DateTime.Now);
                    break;
                case AlarmType.NETWORK:
                    // SendEmail
                    //globalserverApi.SendAlarmMail("网络异常报警", msg.Content);
                    break;
            }
        }

        private void OnResponse_Heartbeat(MemoryStream stream, int uid = 0)
        {
            MSG_BG_HEARTBEAT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_BG_HEARTBEAT>(stream);
            Log.Info("barrack heart beat");
        }

        private void OnResponse_CommandResult(MemoryStream stream, int uid = 0)
        {
            MSG_BG_COMMAND_RESULT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_BG_COMMAND_RESULT>(stream);
            if (msg.Success == true)
            {
                Log.Warn("==============================================================================");
                foreach (var info in msg.Info)
                {
                    Log.Warn(info);
                }
                Log.Warn("==============================================================================");
            }
            else
            {
                Log.Warn("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
                foreach (var info in msg.Info)
                {
                    Log.Warn(info);
                }
                Log.Warn("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
            }
        }

        private void OnResponse_AllGateInfo(MemoryStream stream, int uid = 0)
        {
            MSG_BG_ALL_GATE_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_BG_ALL_GATE_INFO>(stream);
            Log.Warn("==============================================================================");
            foreach (var info in msg.Infos)
            {
                Log.Warn(info);
            }
            Log.Warn("==============================================================================");
        }

        private void OnResponse_GetServerState(MemoryStream stream, int uid = 0)
        {
            MSG_BG_GET_SERVER_STATE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_BG_GET_SERVER_STATE>(stream);
            Client client = Api.ClientMng.FindClient(uid);
            //Log.Info("gm request GetServerState globalServer");

            if (client == null)
            {
                Log.Info($"nof find client {uid}");
                return;
            }

            ServerInfoList response = new ServerInfoList()
            {
                Page = msg.Page, 
                serverInfos = new List<ServerDetailInfo>(), 
                ServerCount = msg.ServerCount,
                result = 1
            };

            msg.ServerInfos.ForEach(x =>
            {
                response.serverInfos.Add(new ServerDetailInfo()
                {
                    serverId = x.ServerId,
                    isOpening = x.IsOpening,
                    registCount = x.RegistCount,
                    isMainTaining = x.IsMainTaining,
                    isRecommend = x.IsRecommend,
                });
            });

            var jser = new JavaScriptSerializer();
            string json = jser.Serialize(response);
            client.WriteString(json);

            //Log.Info("gm request GetServerState globalServer return");
        }

        private void OnResponse_SetServerState(MemoryStream stream, int uid = 0)
        {
            MSG_BG_SET_SERVER_STATE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_BG_SET_SERVER_STATE>(stream);
            Client client = Api.ClientMng.FindClient(uid);
            Log.Info("gm request SetServerState globalServer");

            if (client == null)
            {
                Log.Info($"nof find client {uid}");
                return;
            }

            client.SendSuccess();
        }
    }
}