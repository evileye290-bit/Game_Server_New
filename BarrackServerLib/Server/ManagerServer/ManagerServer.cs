using DBUtility;
using EnumerateUtility;
using Logger;
using Message.IdGenerator;
using Message.Manager.Protocol.MB;
using ServerFrame;
using ServerModels;
using ServerShared;
using System.Collections.Generic;
using System.IO;
using Message.Barrack.Protocol.BM;
using Timestamp = CommonUtility.Timestamp;
using System.Threading.Tasks;

namespace BarrackServerLib
{
    public class ServerOnlineState
    {
        public OnlineState State = OnlineState.CLOSE;
        public int MainId;
        public int Page;
        public ServerOnlineState(int main_id)
        {
            MainId = main_id;
            Page = CalcServerPage(main_id);
            State = OnlineState.VERY_HOT;
        }

        public static int CalcServerPage(int main_id)
        {
            return (main_id - 1) / CONST.SERVER_PER_PAGE + 1;
        }
    }


    public partial class ManagerServer : FrontendServer
    {
        BarrackServerApi server;

        public int OnlineCharacterCount { get; private set; }
        public int RegistCharacterCount { get; private set; }

        public ManagerServer(BaseApi api)
            : base(api)
        {
            server = (BarrackServerApi)api;
        }

        protected override void BindResponser()
        {
            base.BindResponser();
            AddResponser(Id<MSG_MB_BLACK_IP>.Value, OnResponse_BlackIp);
            AddResponser(Id<MSG_MB_NOTIFY_LOGOUT>.Value, OnResponse_Logout);
            AddResponser(Id<MSG_MB_CHARACTER_COUNT>.Value, OnResponse_CharacterInfo);
            AddResponser(Id<MSG_MB_UPDATE_CHARACTER_INFO>.Value, OnResponse_UpdateCharacterInfo);

            //流失干预
            //AddResponser(Id<MSG_MB_GET_RUNAWA_TYPE>.Value, OnResponse_GetRunAwayType);
            //AddResponser(Id<MSG_MB_GET_SDK_GIFT>.Value, OnResponse_GetSdkGIft);

            //ResponserEnd
        }

        private void OnResponse_BlackIp(MemoryStream stream, int uid = 0)
        {
            MSG_MB_BLACK_IP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MB_BLACK_IP>(stream);
            Log.Warn("got new black ip {0}", msg.Ip);
        }

        private void OnResponse_Logout(MemoryStream stream, int uid = 0)
        {
            MSG_MB_NOTIFY_LOGOUT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MB_NOTIFY_LOGOUT>(stream);
            if (server.AuthMng.AntiAddiction && server.AntiAddictionServ != null)
            {
                server.AntiAddictionServ.NotifyAddictionInfoFromManager(msg);
            }
        }

        private void OnResponse_CharacterInfo(MemoryStream stream, int uid = 0)
        {
            MSG_MB_CHARACTER_COUNT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MB_CHARACTER_COUNT>(stream);

            RegistCharacterCount = msg.RegistCount;
            OnlineCharacterCount = msg.OnlineCount;

            CheckRegisterCount();
        }

        private void OnResponse_UpdateCharacterInfo(MemoryStream stream, int uid = 0)
        {
            MSG_MB_UPDATE_CHARACTER_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MB_UPDATE_CHARACTER_INFO>(stream);

            QueryLoadAccount queryLoad = new QueryLoadAccount(msg.AccountId, msg.Channel);

            server.AccountDBPool.Call(queryLoad, (ret) =>
            {
                int serverId = msg.SourceMain;
                SortedDictionary<int, SimpleCharacterInfo> infos = SimpleCharacterInfo.GetSimpleCharacterInfos(queryLoad.LoginServers);

                SimpleCharacterInfo info = null;

                if (infos.ContainsKey(serverId))
                {
                    info = infos[serverId];
                }
                else
                {
                    info = new SimpleCharacterInfo() { ServerId = serverId };
                }
                info.Uid = msg.Uid;
                info.Name = msg.Name;
                if (info != null)
                {
                    info.Level = msg.Level;
                    info.HeroId = msg.HeroId;
                    info.GodType = msg.GodType;
                    info.Time = Timestamp.GetUnixTimeStampSeconds(server.Now());
                    infos[serverId] = info;
                }

                server.AccountDBPool.Call(new QueryUpdateAccountInfo(msg.AccountId, msg.Channel, SimpleCharacterInfo.LoginServerCharacterInfosToString(infos)));
            });
        }


        /// <summary>
        /// 根据当前服务器注册人数调整推荐服务器状态
        /// </summary>
        private void CheckRegisterCount()
        {
            //Data data = DataListManager.inst.GetData("ServerList", MainId);
            ServerItemModel data = server.ServersConfig.Get(MainId);
            if (data == null) return;

            if (RegistCharacterCount >= data.RecommendLimit)
            {
                if (server.IsRecommendServer(MainId))
                {
                    server.RemoveRecommendServer(MainId);
                    //server.AccountDBPool.Call(new QueryUpdateServersRecommend(MainId, 0));
                }
            }
        }


        private void OnResponse_GetRunAwayType(MemoryStream stream, int uid = 0)
        {
            MSG_MB_GET_RUNAWA_TYPE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MB_GET_RUNAWA_TYPE>(stream);

            GetRunAwayInfo(msg);
        }

        private async Task GetRunAwayInfo(MSG_MB_GET_RUNAWA_TYPE msg)
        {
            var recommendGift = await GiftRecommendHelper.GetRunAwayInfo(msg.Account, msg.ServerId, msg.Uid.ToString(), msg.GameId);

            MSG_BM_GET_RUNAWA_TYPE response = new MSG_BM_GET_RUNAWA_TYPE() { Uid = msg.Uid };
            if (recommendGift != null)
            {
                Log.Info($"account {msg.Account} uid {msg.Uid} server id {msg.ServerId} runaway");
                response.RunAwayType = int.Parse(recommendGift.intervene_id);
                response.InterveneId = recommendGift.intervene_id;
                response.DataBox = recommendGift.data_box;
            }

            Write(response);
        }

        private void OnResponse_GetSdkGIft(MemoryStream stream, int uid = 0)
        {
            MSG_MB_GET_SDK_GIFT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MB_GET_SDK_GIFT>(stream);

            GetRecommendGift(msg);
        }


        private async Task GetRecommendGift(MSG_MB_GET_SDK_GIFT msg)
        { 
            RecommendGiftInfo recommendGift = await GiftRecommendHelper.GetRecommendGift(msg.Account, msg.ServerId.ToString(), msg.Uid.ToString(), msg.SdkActionType.ToString());
            MSG_BM_GET_SDK_GIFT response = new MSG_BM_GET_SDK_GIFT()
            {
                Uid = msg.Uid,
                ActionId = msg.ActionId,
                GiftId = recommendGift.GiftId,
                Param = msg.Param,
                SdkActionType = msg.SdkActionType,
                DataBox = recommendGift.Data_Box
            };

            Log.Info($"account {msg.Account} uid {msg.Uid} server id {msg.ServerId} recommend gift id {recommendGift.GiftId} score {recommendGift.Score}");

            Write(response);
        }
    }
}
