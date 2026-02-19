using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateM;
using Message.IdGenerator;
using Message.Manager.Protocol.MB;
using Message.Manager.Protocol.MGate;
using Message.Manager.Protocol.MZ;
using ServerFrame;
using ServerShared;
using System.IO;

namespace ManagerServerLib
{
    public class GateServer : FrontendServer
    {
        private ManagerServerApi Api
        { get { return (ManagerServerApi)api; } }

        public GateServer(BaseApi api):base(api)
        {
        }

        protected override void BindResponser()
        {
            base.BindResponser();
            AddResponser(Id<MSG_GateM_MaxUid>.Value, OnResponse_MaxUid);
            AddResponser(Id<MSG_GateM_CharacterGetZone>.Value, OnResponse_CharacterGetZone);
            AddResponser(Id<MSG_GateM_FORCE_LOGIN>.Value, OnResponse_ForceLogin);
            AddResponser(Id<MSG_GateM_REPEAT_LOGIN>.Value, OnResponse_RepeatLogin);
            AddResponser(Id<MSG_GateM_CREATE_NEW_CHARACTER>.Value, OnResponse_CreatedNewCharacter);
            AddResponser(Id<MSG_GateM_LOGOUT>.Value, OnResponse_ClientLogout);
            //ResponserEnd
        }

        private void OnResponse_MaxUid(MemoryStream stream, int uid = 0)
        {
            MSG_GateM_MaxUid msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateM_MaxUid>(stream);
            MSG_MGate_MaxUid response = new MSG_MGate_MaxUid();
            response.AccountName = msg.AccountName;
            response.ChannelName = msg.ChannelName;

            response.MaxUid = ++Api.MaxCharUid;
            response.Result = (int)ErrorCode.Success;

            Write(response);
        }

        private void OnResponse_CharacterGetZone(MemoryStream stream, int uid = 0)
        {
            MSG_GateM_CharacterGetZone msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateM_CharacterGetZone>(stream);
            MSG_MGate_CharacterGetZone response = new MSG_MGate_CharacterGetZone();
            response.AccountName = msg.AccountName;
            response.Uid = msg.Uid;
            response.MainId = msg.MainId;
            response.ChannelName = msg.ChannelName;
            response.Token = msg.Token;
            ZoneServerManager zoneManager = Api.ZoneServerManager;
            
            if (Api.MainId != msg.MainId)
            {
                response.Result = (int)ErrorCode.ServerNotExist;
                Write(response);
                return;
            }
            Map map = null;
            ZoneServer zone = null;
            //判断重复登录
            Client otherClient = zoneManager.GetClient(msg.Uid);
            if (otherClient != null && otherClient.Zone != null && otherClient.Zone.State == ServerState.Started)
            {
                // 1.通知otherClient已登录的zone踢掉该client
                MSG_MZ_REPEAT_LOGIN kickPacket = new MSG_MZ_REPEAT_LOGIN();
                kickPacket.CharacterUid = msg.Uid;
                otherClient.Zone.Write(kickPacket);
                // 2.通知当前连接的client登录失败，回退到选服界面重新登录（不允许在旧连接未断开的情况下进入游戏，防止因网络时序不确定性引起的bug）
                response.Result = (int)ErrorCode.AlreadyLogin;
                Write(response);
                otherClient.Zone.RemoveClient(msg.Uid);
                return;
            }

            // 有离线缓存 直接转发过去 不需要路由
            OfflineClient offlineClient = zoneManager.GetOfflineClient(msg.Uid);
            if (offlineClient != null)
            {
                zone = (ZoneServer)zoneManager.GetServer(MainId, offlineClient.SubId);

                //如果当前zone被禁止进入
                if (zone != null && !ZoneTransformManager.Instance.IsForbided(offlineClient.SubId))
                {
                    // 通知客户端 连上次离线的zone
                    response.MainId = zone.MainId;
                    response.SubId = zone.SubId;
                    response.MapId = offlineClient.MapId;
                    response.Channel = offlineClient.Channel;
                    response.Result = (int)ErrorCode.Success;
                    Write(response);
                    return;
                }
            }

            // 目标地图在当前manager集群中 且当前集群负载健康 则尝试在当前zone分配
            if (Api.ZoneServerManager.GetZone(msg.MapId, msg.Channel, out zone, out map))
            {
                response.MainId = zone.MainId;
                response.SubId = zone.SubId;
                response.MapId = map.MapId;
                response.Channel = map.Channel;
                response.Result = (int)ErrorCode.Success;
                Write(response);
                map.WillEnter(msg.Uid);
                return;
            }
            // 该map不存在或者极限情况所有channel人数均达到max 则尝试拉回主城
            else if (Api.ZoneServerManager.GetZone(CONST.MAIN_MAP_ID, CONST.MAIN_MAP_CHANNEL, out zone, out map))
            {
                response.MainId = zone.MainId;
                response.SubId = zone.SubId;
                response.MapId = map.MapId;
                response.Channel = map.Channel;
                response.Result = (int)ErrorCode.Success;
                Write(response);
                map.WillEnter(msg.Uid);
                return;
            }
            else
            {
                //RouteManagerForLogin(msg);
                //服务器已满
                response.Result = (int)ErrorCode.MaxCount;
                Write(response);
                return;
            }
        }

        //public void RouteManagerForLogin(MSG_GateM_CharacterGetZone msg)
        //{
        //    MSG_MGate_RouteToOtherManager routeMsg = new MSG_MGate_RouteToOtherManager();
        //    routeMsg.accountName = msg.AccountName;
        //    routeMsg.uid = msg.Uid;
        //    routeMsg.channelName = msg.ChannelName;
        //    // 找到对应的manager信息
        //    ManagerMapInfo managerInfo = Api.BallenceProxy.FindTheManager(msg.MapId, msg.Channel);
        //    // 未找到 则根据负载状态 分配一个
        //    if (managerInfo == null || managerInfo.IsHealthy() == false)
        //    {
        //        managerInfo = Api.BallenceProxy.FindOneManager(msg.MapId);
        //    }
        //    if (managerInfo == null)
        //    {
        //        // 乍滴都没找到 则通知他拉回主城
        //        routeMsg.mainId = msg.MainId;
        //        routeMsg.mapId = CONST.MAIN_MAP_ID;
        //        routeMsg.channel = CONST.MAIN_MAP_CHANNEL;
        //        Write(routeMsg);
        //        Log.Write("player {0} get zone main {1} map {2} channel {3} failed: will try to route to main city ",
        //            msg.Uid, msg.MainId, msg.MapId, msg.Channel);
        //        return;
        //    }
        //    else
        //    {
        //        // 找到对应manager  
        //        routeMsg.mainId = managerInfo.MainId;
        //        routeMsg.mapId = msg.MapId;
        //        routeMsg.channel = msg.Channel;
        //        Write(routeMsg);
        //        Log.Write("player {0} get zone main {1} map {2} channel {3} route to manager {4}",
        //            msg.Uid, msg.MainId, msg.MapId, msg.Channel, managerInfo.MainId);
        //        return;
        //    }
        //}

        private void OnResponse_ForceLogin(MemoryStream stream, int uid = 0)
        {
            MSG_GateM_FORCE_LOGIN msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateM_FORCE_LOGIN>(stream);
            Log.Write("player {0} force login main {1} map {2} channel {3}", msg.Uid, msg.MainId, msg.MapId, msg.Channel);
            MSG_MGate_FORCE_LOGIN response = new MSG_MGate_FORCE_LOGIN();
            response.Uid = msg.Uid;
            response.MapId = msg.MapId;
            response.Channel = msg.Channel;
            response.MainId = msg.MainId;
            response.SyncData = msg.SyncData;
            ZoneServer zone = Api.ZoneServerManager.GetZone(msg.MapId, msg.Channel);
            if (zone == null)
            {
                response.Result = (int)ErrorCode.NotOpen;
                Write(response);
                return;
            }
            Map map = zone.GetMap(msg.MapId, msg.Channel);
            if (map == null)
            {
                response.Result = (int)ErrorCode.NotOpen;
                Write(response);
                return;
            }
            response.Result = (int)ErrorCode.Success;
            response.SubId = zone.SubId;
            Write(response);
            map.WillEnter(msg.Uid);
        }

        private void OnResponse_RepeatLogin(MemoryStream stream, int uid = 0)
        {
            MSG_GateM_REPEAT_LOGIN msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateM_REPEAT_LOGIN>(stream);
            Log.Write("player {0} repeate login", msg.Uid);
            Client otherClient = Api.ZoneServerManager.GetClient(msg.Uid);
            if (otherClient != null && otherClient.Zone != null && otherClient.Zone.State == ServerState.Started)
            {
                MSG_MZ_REPEAT_LOGIN kickPacket = new MSG_MZ_REPEAT_LOGIN();
                kickPacket.CharacterUid = msg.Uid;
                otherClient.Zone.Write(kickPacket);
            }
        }

        private void OnResponse_CreatedNewCharacter(MemoryStream stream, int uid = 0)
        {
            Api.OnRegistNewCharacter();
        }

        public void OnResponse_ClientLogout(MemoryStream stream, int uid = 0)
        {
            MSG_GateM_LOGOUT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateM_LOGOUT>(stream);
            //下线时更新账号服务器玩家角色信息
            MSG_MB_UPDATE_CHARACTER_INFO notify = new MSG_MB_UPDATE_CHARACTER_INFO
            {
                AccountId = msg.AccountId,
                Channel = msg.Channel,
                Uid = msg.Uid,
                Level = msg.Level,
                HeroId = msg.HeroId,
                GodType = msg.GodType,
                Name = msg.Name,
                SourceMain = msg.SourceMain
            };
            Api.BarrackServerManager.Broadcast(notify);
        }
    }
}
