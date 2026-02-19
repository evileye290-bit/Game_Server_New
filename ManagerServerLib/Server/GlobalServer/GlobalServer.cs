using CommonUtility;
using DataProperty;
using DBUtility;
using DBUtility.Sql;
using EnumerateUtility;
using Logger;
using Message.Global.Protocol.GM;
using Message.IdGenerator;
using Message.Manager.Protocol.MG;
using Message.Manager.Protocol.MR;
using Message.Manager.Protocol.MZ;
using ServerFrame;
using ServerModels;
using ServerShared;
using ServerShared.Map;
using System;
using System.Collections.Generic;
using System.IO;

namespace ManagerServerLib
{
    public partial class GlobalServer : BaseGlobalServer
    {
        private ManagerServerApi Api
        { get { return (ManagerServerApi)api; } }
        public GlobalServer(BaseApi api)
            : base(api)
        {
        }

        protected override void BindResponser()
        {
            base.BindResponser();
            AddResponser(Id<MSG_GM_MAP_INFO>.Value, OnResponse_MapInfo);
            AddResponser(Id<MSG_GM_ZONE_INFO>.Value, OnResponse_ZoneInfo);
            AddResponser(Id<MSG_GM_ALL_ZONE_INFO>.Value, OnResponse_AllZoneInfo);
            AddResponser(Id<MSG_GM_KICK_PLAYER>.Value, OnResponse_KickPlayer);
            AddResponser(Id<MSG_GM_FREEZE_PLAYER>.Value, OnResponse_FreezePlayer);
            AddResponser(Id<MSG_GM_SHUTDOWN_ZONE>.Value, OnResponse_ShutdownZone);
            AddResponser(Id<MSG_GM_SEND_EMAIL>.Value, OnResponse_SendEmail);
            AddResponser(Id<MSG_GM_REISSUE_ARENA_EMAIL>.Value, OnResponse_ReissueArenaEmail);
            AddResponser(Id<MSG_GM_UPDATE_XML>.Value, OnResponse_UpdateXml);
            AddResponser(Id<MSG_GM_UPDATE_RANKLIST>.Value, OnResponse_UpdateRankList);
            AddResponser(Id<MSG_GM_SHUTDOWN_MANAGER>.Value, OnResponse_ShutdownManager);
            AddResponser(Id<MSG_GM_SHUTDOWN_RELATION>.Value, OnResponse_ShutdownRelation);
            AddResponser(Id<MSG_GM_SHUTDOWN_MAIN>.Value, OnResponse_ShutdownMain);
            AddResponser(Id<MSG_GM_SET_ZONE_FPS>.Value, OnResponse_SetZoneFps);
            AddResponser(Id<MSG_GM_SET_Relation_FPS>.Value, OnResponse_SetRelationFps);
            AddResponser(Id<MSG_GM_SET_FPS>.Value, OnResponse_SetFps);
            AddResponser(Id<MSG_GM_RELATION_FPS_INFO>.Value, OnResponse_RelationFpsInfo);
            AddResponser(Id<MSG_GM_FPS_INFO>.Value, OnResponse_FpsInfo);

            //客服相关
            AddResponser(Id<MSG_GM_CHARACTER_LIST>.Value, OnResponse_CharacterList);
            AddResponser(Id<MSG_GM_CHARACTER_INFO>.Value, OnResponse_CharacterInfo);
            AddResponser(Id<MSG_GM_ECHARGE_TEST>.Value, OnResponse_TestRecharge);
            AddResponser(Id<MSG_GM_UNVOICE>.Value, OnResponse_UnVoice);
            AddResponser(Id<MSG_GM_VOICE>.Value, OnResponse_Voice);
            AddResponser(Id<MSG_GM_CUSTOM_ANNOUNCEMENT>.Value, OnResponse_CustomAnnouncement);
            AddResponser(Id<MSG_GM_MOVE_PLAYER_CITY>.Value, OnResponse_MovePlayerCity);
            AddResponser(Id<MSG_GM_FREEZE>.Value, OnResponse_Freeze);
            AddResponser(Id<MSG_GM_UNFREEZE>.Value, OnResponse_UnFreeze);
            AddResponser(Id<MSG_GM_BAG>.Value, OnResponse_Bag);
            AddResponser(Id<MSG_GM_ORDER_STATE>.Value, OnResponse_OrderState);
            AddResponser(Id<MSG_GM_DELETE_BAG_ITEM>.Value, OnResponse_DeleteBagItem);
            //AddResponser(Id<MSG_GM_ACCOUNT_ID>.Value, OnResponse_AccountId);
            AddResponser(Id<MSG_GM_REPAIR_ORDER>.Value, OnResponse_RepairOrder);
            AddResponser(Id<MSG_GM_VIRTUAL_RECHARGE>.Value, OnResponse_VirtualRecharge);
            //AddResponser(Id<MSG_GM_SEND_ITEM>.Value, OnResponse_SendItem);
            //AddResponser(Id<MSG_GM_SEND_MAIL>.Value, OnResponse_SendMail);
            //AddResponser(Id<MSG_GM_CAN_RECEIVE_CHANNEL_TASK>.Value, OnResponse_CanReceiveChannelTask);
            //AddResponser(Id<MSG_GM_RECEIVE_CHANNEL_TASK>.Value, OnResponse_ReceiveChannelTask);
            AddResponser(Id<MSG_GM_BAD_WORDS>.Value, OnResponse_BadWords);

            AddResponser(Id<MSG_GM_HK_USER_INFO>.Value, OnResponse_HKUserInfo);
            AddResponser(Id<MSG_GM_RELOAD_FAMILY>.Value, OnResponseReloadFamily);
            AddResponser(Id<MSG_GM_WAIT_COUNT>.Value, OnResponse_WaitCount);
            AddResponser(Id<MSG_GM_FULL_COUNT>.Value, OnResponse_FullCount);

            AddResponser(Id<MSG_GM_ARENA_INFO>.Value, OnResponse_ArenaInfo);
            AddResponser(Id<MSG_GM_FAMILY_INFO>.Value, OnResponse_FamilyInfo);
            AddResponser(Id<MSG_GM_SERVER_INFO>.Value, OnResponse_ServerInfo);
            AddResponser(Id<MSG_GM_GIFT_CODE>.Value, OnResponse_GiftCode);
            AddResponser(Id<MSG_GM_GAME_COUNTER>.Value, OnResponse_GameCounter);
            AddResponser(Id<MSG_GM_CHANGE_FAMLIY_NAME>.Value, OnResponse_ChangeFamilyName);

            //AddResponser(Id<MSG_GM_CHAR_ALL_INFO>.Value, OnResponse_CharAllInfo);
            AddResponser(Id<MSG_GM_ITEM_TYPE_LIST>.Value, OnResponse_ItemTypeList);
            AddResponser(Id<MSG_GM_PET_TYPE_LIST>.Value, OnResponse_PetTypeList);
            AddResponser(Id<MSG_GM_PET_MOUNT_LIST>.Value, OnResponse_PetMountList);
            AddResponser(Id<MSG_GM_DELETE_PET>.Value, OnResponse_DeletePet);
            AddResponser(Id<MSG_GM_DELETE_PET_MOUNT>.Value, OnResponse_DeletePetMount);
            AddResponser(Id<MSG_GM_EQUIP_LIST>.Value, OnResponse_EquipList);
            AddResponser(Id<MSG_GM_PET_LIST>.Value, OnResponse_PetList);
            AddResponser(Id<MSG_GM_PET_MOUNT_STRENGTH>.Value, OnResponse_PetMountStrength);
            AddResponser(Id<MSG_GM_DELETE_ITEM>.Value, OnResponse_DeleteItem);
            AddResponser(Id<MSG_GM_DELETE_CHAR>.Value, OnResponse_DeleteChar);
            //AddResponser(Id<MSG_GM_CHARACTER_LIST_BY_ACCOUNT_NAME>.Value, OnResponse_CharacterListByAccountName);
            AddResponser(Id<MSG_GM_ORDER_LIST>.Value, OnResponse_OrderList);
            AddResponser(Id<MSG_GM_SPEC_ITEM>.Value, OnResponse_SpecItem);
            AddResponser(Id<MSG_GM_SPEC_PET>.Value, OnResponse_SpecPet);
            AddResponser(Id<MSG_GM_UPDATE_ITEM_COUNT>.Value, OnResponse_UpdateItemCount);
            //AddResponser(Id<MSG_GM_SPEC_EMAIL>.Value, OnResponse_SpecEmail);
            AddResponser(Id<MSG_GM_UPDATE_CHAR_DATA>.Value, OnResponse_UpdateCharData);
            AddResponser(Id<MSG_GM_ZONE_TRANSFORM>.Value, OnResponse_ZoneTransform);
        

            AddResponser(Id<MSG_GM_PLAYER_LEVEL>.Value, OnResponse_PlayerLevel);
            AddResponser(Id<MSG_GM_PLAYER_EXP>.Value, OnResponse_PlayerExp);
            AddResponser(Id<MSG_GM_BUZY_ONLINE_COUNT>.Value, OnResponse_BuzyOnlineCount);
            AddResponser(Id<MSG_GM_TIP_OFF_INFO>.Value, OnResponse_GetTipOffInfo);
            AddResponser(Id<MSG_GM_IGNORE_TIP_OFF>.Value, OnResponse_IgnoreTipOff);


            AddResponser(Id<MSG_GM_ADD_REWARD>.Value, OnResponse_AddReward);
            AddResponser(Id<MSG_GM_EQUIP_HERO>.Value, OnResponse_EquipHero);
            AddResponser(Id<MSG_GM_ALL_CHAT>.Value, OnResponse_AllChat);
            AddResponser(Id<MSG_GM_ABSORB_SOULRING>.Value, OnResponse_AbsorbSoulRing);
            AddResponser(Id<MSG_GM_ABSORB_FINISH>.Value, OnResponse_AbsorbFinish);
            AddResponser(Id<MSG_GM_ADD_HEROEXP>.Value, OnResponse_AddHeroExp);
            AddResponser(Id<MSG_GM_HERO_AWAKEN>.Value, OnResponse_HeroAwaken);
            AddResponser(Id<MSG_GM_HERO_LEVELUP>.Value, OnResponse_HeroLevelUp);
            AddResponser(Id<MSG_GM_UPDATE_HERO_POS>.Value, OnResponse_UpdateHeroPos);
            AddResponser(Id<MSG_GM_GIFT_OPEN>.Value, OnResponse_GiftOpen);


            //http select commond
            AddResponser(Id<MSG_GM_queryRoleInfo>.Value, OnResponse_queryRoleInfo);

            AddResponser(Id<MSG_GM_GET_ITEM_INFO>.Value, OnResponse_GetItemInfo);
            AddResponser(Id<MSG_GM_DEL_ITEM_NUM>.Value, OnResponse_ChangeItemNum);
            AddResponser(Id<MSG_GM_DEL_ACTIVITY_PROGRESS>.Value, OnResponse_DeleteActiveProgress);

            //ResponserEnd
        }

        private void OnResponse_MapInfo(MemoryStream stream, int uid = 0)
        {
            MSG_GM_MAP_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_MAP_INFO>(stream);
            Log.Write("global request main {0} map {1} info", msg.MainId, msg.MapId);
            ZoneServerManager zoneManager = Api.ZoneServerManager;
            MSG_MG_COMMAND_RESULT response = new MSG_MG_COMMAND_RESULT();
            if (msg.MainId != Api.MainId)
            {
                response.Success = false;
                response.Info.Add(String.Format("MapInfo main {0} map {1} failed: find zone manager failed", msg.MainId, msg.MapId));
                Write(response);
                return;
            }
            response.Success = true;
            foreach (var item in zoneManager.ServerList)
            {
                ZoneServer zone = item.Value as ZoneServer;
                foreach (var mapItem in zone.AllMap)
                {
                    if (mapItem.Value.MapId == msg.MapId)
                    {
                        int channel = int.Parse(item.Key.Split('_')[1]);
                        Map map = mapItem.Value;
                        // 显示该指定main id下 该map有多少线，每个线挂在哪个zone，当前地图有多少人，有多少人在切往该图中
                        response.Info.Add(String.Format("map {0} channel {1} player count {2} will enter count {3} main {4} sub {5}",
                            map.MapId, map.Channel, map.ClientListMap.Count, map.ClientEnterList.Count, map.MainId, map.SubId));
                    }
                }
            }
            Write(response);
        }

        private void OnResponse_RelationFpsInfo(MemoryStream stream, int uid = 0)
        {
            MSG_GM_RELATION_FPS_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_RELATION_FPS_INFO>(stream);
            Log.Write("global request relation main {0}  info", msg.MainId);
            MSG_MG_COMMAND_RESULT response = new MSG_MG_COMMAND_RESULT();
            response.Success = true;
            if (Api.RelationServer != null)
            {
                response.Info.Add(String.Format("relation main {0} sleep time {1} frame {2} memory {3}",
                    msg.MainId, Api.RelationServer.SleepTime, Api.RelationServer.FrameCount, Api.RelationServer.Memory));
            }
            Write(response);
        }
        private void OnResponse_FpsInfo(MemoryStream stream, int uid = 0)
        {
            MSG_GM_FPS_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_FPS_INFO>(stream);
            Log.Write("global request manager main {0} info", msg.MainId);
            FpsAndCpuInfo info = Api.Fps.GetFPSAndCpuInfo();
            MSG_MG_COMMAND_RESULT response = new MSG_MG_COMMAND_RESULT();
            if (info == null)
            {
                response.Success = false;
                response.Info.Add(String.Format("manager main {0} getfps fail", Api.MainId));
            }
            else
            {
                response.Success = true;
                response.Info.Add(String.Format("manager main {0} frame {1} sleep time {2} memory {3} player count {4}",
                Api.MainId, info.fps, info.sleepTime, info.memorySize, Api.ZoneServerManager.OnlineCount));
            }
            Write(response);
        }

        private void OnResponse_ZoneInfo(MemoryStream stream, int uid = 0)
        {
            MSG_GM_ZONE_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_ZONE_INFO>(stream);
            Log.Write("global request main {0} zone {1} info", msg.MainId, msg.SubId);
            ZoneServerManager zoneManager = Api.ZoneServerManager;
            MSG_MG_COMMAND_RESULT response = new MSG_MG_COMMAND_RESULT();
            if (msg.MainId != Api.MainId)
            {
                response.Success = false;
                response.Info.Add(String.Format("ZoneInfo main {0} sub {1} failed: find zone manager failed", msg.MainId, msg.SubId));
                Write(response);
                return;
            }

            // 显示该zone有多少图 每个图当前多少人，多少人在切往该图中，该zone CPU信息 帧率 人数统计信息
            ZoneServer zone = (ZoneServer)zoneManager.GetServer(MainId, msg.SubId);
            if (zone == null)
            {
                response.Success = false;
                response.Info.Add(String.Format("ZoneInfo main {0} sub {1} failed: find zone failed", msg.MainId, msg.SubId));
                Write(response);
                return;
            }

            response.Success = true;
            response.Info.Add(String.Format("zone main {0} sub {1} sleep time {2} frame {3} player count {4}",
                zone.MainId, zone.SubId, zone.SleepTime, zone.FrameCount, zone.ClientListZone.Count));
            int count = 0;
            foreach (var item in zone.AllMap)
            {
                int mapId = int.Parse(item.Key.Split('_')[0]);
                int channel = int.Parse(item.Key.Split('_')[1]);
                Map map = item.Value;
                response.Info.Add(String.Format("map {0} channel {1} player count {2} will enter count {3}",
                    map.MapId, map.Channel, map.ClientListMap.Count, map.ClientEnterList.Count));
                count++;
                if (count >= 30)
                {
                    Write(response);
                    response = new MSG_MG_COMMAND_RESULT();
                    response.Success = true;
                    count = 0;
                }
            }
            Write(response);
        }

        private void OnResponse_AllZoneInfo(MemoryStream stream, int uid = 0)
        {
            MSG_GM_ALL_ZONE_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_ALL_ZONE_INFO>(stream);
            Log.Write("global request main {0} all zone info", msg.MainId);
            ZoneServerManager zoneManager = Api.ZoneServerManager;
            MSG_MG_COMMAND_RESULT response = new MSG_MG_COMMAND_RESULT();
            if (msg.MainId != Api.MainId)
            {
                response.Success = false;
                response.Info.Add(String.Format("AllZoneInfo main {0} failed: find zone manager failed", msg.MainId));
                Write(response);
                return;
            }

            // 显示该main id下所有zone的人数 CPU 帧率 挂载map数 挂载副本数
            response.Success = true;
            int totalCount = 0;
            foreach (var item in zoneManager.ServerList)
            {
                ZoneServer zone = ((ZoneServer)item.Value);
                response.Info.Add(String.Format("zone sub {0} map count {1} sleep time {2} frame {3} memory {4} player count {5}",
                    zone.SubId, zone.AllMap.Count, zone.SleepTime, zone.FrameCount, zone.Memory, zone.ClientListZone.Count));
                totalCount += zone.ClientListZone.Count;
            }
            response.Info.Add(String.Format("total player count {0}", totalCount));
            Write(response);
        }

        private void OnResponse_KickPlayer(MemoryStream stream, int uid = 0)
        {
            MSG_GM_KICK_PLAYER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_KICK_PLAYER>(stream);
            Log.Write("global request main {0} kick player {1}", msg.MainId, msg.Uid);
            MSG_MG_COMMAND_RESULT response = new MSG_MG_COMMAND_RESULT();
            ZoneServerManager zoneManager = Api.ZoneServerManager;
            if (msg.MainId != Api.MainId)
            {
                response.Success = false;
                response.Info.Add(String.Format("kick player main {0} uid {1} failed: find zone manager failed", msg.MainId, msg.Uid));
                Write(response);
                return;
            }

            MSG_MZ_KICK_PLAYER request = new MSG_MZ_KICK_PLAYER();
            request.Uid = msg.Uid;
            foreach (var zone in zoneManager.ServerList)
            {
                zone.Value.Write(request);
            }
            zoneManager.RemoveOfflineClient(msg.Uid);
        }

        private void OnResponse_FreezePlayer(MemoryStream stream, int uid = 0)
        {
            MSG_GM_FREEZE_PLAYER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_FREEZE_PLAYER>(stream);
            Log.Write("global request main {0} freeze uid {1} type {2} hour {3}", msg.MainId, msg.Uid, msg.FreezeType, msg.Hour);
            MSG_MG_COMMAND_RESULT response = new MSG_MG_COMMAND_RESULT();
            ZoneServerManager zoneManager = Api.ZoneServerManager;
            if (msg.MainId != Api.MainId)
            {
                response.Success = false;
                response.Info.Add(String.Format("freeze player main {0} uid {1} freezeType {2} hour {3} failed: find zone manager failed", msg.MainId, msg.Uid, msg.FreezeType, msg.Hour));
                Write(response);
                return;
            }
            // 先踢下线
            MSG_MZ_KICK_PLAYER request = new MSG_MZ_KICK_PLAYER();
            request.Uid = msg.Uid;
            foreach (var zone in zoneManager.ServerList)
            {
                zone.Value.Write(request);
            }
            DateTime freezeTime = DateTime.MinValue;
            if (msg.FreezeType == (int)FreezeState.Freeze)
            {
                freezeTime = ManagerServerApi.now.AddHours(msg.Hour);
            }
            //server.DB.Call(new QueryFreezePlayer(msg.Uid, msg.freezeType, freezeTime, ""));
            response.Success = true;
            response.Info.Add(String.Format("freeze player main {0} uid {1} freezeType {2} succeed: freezeTime {3}", msg.MainId, msg.Uid, msg.FreezeType, freezeTime.ToString()));
            Write(response);
        }

        private void OnResponse_ShutdownZone(MemoryStream stream, int uid = 0)
        {
            MSG_GM_SHUTDOWN_ZONE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_SHUTDOWN_ZONE>(stream);
            Log.Warn("Global require shutdown server main {0} sub {1}", msg.MainId, msg.SubId);
            ZoneServerManager zoneManager = Api.ZoneServerManager;
            MSG_MG_COMMAND_RESULT response = new MSG_MG_COMMAND_RESULT();
            if (msg.MainId != Api.MainId)
            {
                response.Success = false;
                response.Info.Add(String.Format("shutdown main {0} sub {1} failed: find zone manager failed", msg.MainId, msg.SubId));
                Write(response);
                return;
            }

            MSG_MZ_SHUTDOWN notify = new MSG_MZ_SHUTDOWN();
            CONST.ALARM_OPEN = false;
            if (msg.SubId == 0)
            {
                // 关闭所有zone
                foreach (var zone in zoneManager.ServerList)
                {
                    zone.Value.Write(notify);
                }
            }
            else
            {
                // 关闭指定zone
                ZoneServer zone = (ZoneServer)zoneManager.GetServer(MainId, msg.SubId);
                if (zone == null)
                {
                    response.Success = false;
                    response.Info.Add(String.Format("shutdown main {0} sub {1} failed: find zone failed", msg.MainId, msg.SubId));
                    Write(response);
                    return;
                }
                zone.Write(notify);
            }
        }

        private void OnResponse_ShutdownManager(MemoryStream stream, int uid = 0)
        {
            Log.Warn("global request shutdown manager");
            CONST.ALARM_OPEN = false;
            Api.State = ServerState.Stopping;
            Api.StoppingTime = ManagerServerApi.now.AddMinutes(1);
            MSG_MR_SHUTDOWN msgRelation = new MSG_MR_SHUTDOWN();
            if (Api.RelationServer != null)
            {
                Api.RelationServer.Write(msgRelation);
            }
            MSG_MZ_SHUTDOWN msgZone = new MSG_MZ_SHUTDOWN();
            // 关闭所有zone
            foreach (var zone in Api.ZoneServerManager.ServerList)
            {
                zone.Value.Write(msgZone);
            }
        }

        private void OnResponse_ShutdownRelation(MemoryStream stream, int uid = 0)
        {
            Log.Warn("global request shutdown relation {0}", Api.MainId);
            MSG_MR_SHUTDOWN msg = new MSG_MR_SHUTDOWN();
            if (Api.RelationServer != null)
            {
                Api.RelationServer.Write(msg);
            }
            CONST.ALARM_OPEN = false;
        }

        private void OnResponse_ShutdownMain(MemoryStream stream, int uid = 0)
        {
            Log.Warn("global request shutdown main {0}", Api.MainId);
            CONST.ALARM_OPEN = false;
            Api.State = ServerState.Stopping;
            Api.StoppingTime = ManagerServerApi.now.AddMinutes(1);
            MSG_MR_SHUTDOWN msgRelation = new MSG_MR_SHUTDOWN();
            if (Api.RelationServer != null)
            {
                Api.RelationServer.Write(msgRelation);
            }
            MSG_MZ_SHUTDOWN msgZone = new MSG_MZ_SHUTDOWN();
            // 关闭所有zone
            foreach (var zone in Api.ZoneServerManager.ServerList)
            {
                zone.Value.Write(msgZone);
            }
        }

        private void OnResponse_SendEmail(MemoryStream stream, int uid = 0)
        {
            MSG_GM_SEND_EMAIL pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_SEND_EMAIL>(stream);
            Log.Write("GM semd email {0} save {1} days", pks.EmailId, pks.SaveTime);
            MSG_MR_GM_SEND_EMAIL msg = new MSG_MR_GM_SEND_EMAIL();
            msg.EmailId = pks.EmailId;
            msg.SaveTime = pks.SaveTime;
            msg.MainId = pks.MainId;
            msg.SqlConditions = pks.SqlConditions;
            if (Api.RelationServer != null)
            {
                Api.RelationServer.Write(msg);
            }
        }

        private void OnResponse_ReissueArenaEmail(MemoryStream stream, int uid = 0)
        {
            MSG_GM_REISSUE_ARENA_EMAIL pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_REISSUE_ARENA_EMAIL>(stream);
            Log.Write("GM reissue arena email date {0}", pks.ReissueTime);
            MSG_MR_GM_REISSUE_ARENA_EMAIL msg = new MSG_MR_GM_REISSUE_ARENA_EMAIL();
            msg.ReissueTime = pks.ReissueTime;
            msg.MainId = pks.MainId;
            if (Api.RelationServer != null)
            {
                Api.RelationServer.Write(msg);
            }
        }

        private void OnResponse_UpdateXml(MemoryStream stream, int uid = 0)
        {
            MSG_GM_UPDATE_XML pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_UPDATE_XML>(stream);
            Log.Write("GM update xml");

            Api.UpdateXml();

            if (Api.ZoneServerManager != null)
            {
                MSG_MZ_UPDATE_XML msg2Allzone = new MSG_MZ_UPDATE_XML();
                msg2Allzone.Type = pks.Type;
                Api.ZoneServerManager.Broadcast(msg2Allzone);

                Api.ZoneServerManager.InitServerOpenTime();
            }
 
            if (Api.RelationServer != null)
            {
                MSG_MR_GM_UPDATE_XML msg = new MSG_MR_GM_UPDATE_XML();
                msg.Type = pks.Type;
                Api.RelationServer.Write(msg);
            }
        }

        private void OnResponse_UpdateRankList(MemoryStream stream, int uid = 0)
        {
            MSG_GM_UPDATE_RANKLIST pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_UPDATE_RANKLIST>(stream);
            Log.Write("GM update rank list");

            MSG_MR_UPDATE_RANKLIST msg = new MSG_MR_UPDATE_RANKLIST();
            if (Api.RelationServer != null)
            {
                Api.RelationServer.Write(msg);
            }
        }

        private void OnResponse_TestRecharge(MemoryStream stream, int uid = 0)
        {
            var pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_ECHARGE_TEST>(stream);
            Log.Write("GM test recharge");

            RechargeItemModel rechargeItem = RechargeLibrary.GetRechargeItem(pks.GiftId);//rechargeitemid
            if (rechargeItem != null)
            {
                RechargePriceModel price = RechargeLibrary.GetRechargePrice(rechargeItem.RechargeId);
                if (price != null)
                {
                    int historyId = Api.RechargeMng.GetNewHistoryId();
                    Api.RechargeMng.SaveHistoryId(pks.Uid, pks.GiftId, historyId, 0);
                    string orderInfo = $"test_{pks.Uid}_{pks.GiftId}";
                    Api.RechargeMng.UpdateRechargeManager(historyId, orderInfo, ManagerServerApi.now, price.Money, "Global", RechargeWay.Global, "1", "0");
                }
            }
        }


        private void OnResponse_CharacterList(MemoryStream stream, int uid = 0)
        {
            MSG_GM_CHARACTER_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_CHARACTER_LIST>(stream);
            Api.GameDBPool.Call(new QueryCharacterList(msg.AccountId), ret =>
            {
                try
                {
                    MSG_MG_CHARACTER_LIST notify = new MSG_MG_CHARACTER_LIST();
                    notify.CustomUid = msg.CustomUid;
                    List<MSG_MG_CHARACTER_LIST.Types.CharSimpleInfo> list = (List<MSG_MG_CHARACTER_LIST.Types.CharSimpleInfo>)ret;
                    foreach (var item in list)
                    {
                        notify.List.Add(item);
                    }
                    Write(notify);
                }
                catch (Exception e)
                {
                    Log.Alert(e.ToString());
                }
            });
        }

        private void OnResponse_CharacterInfo(MemoryStream stream, int uid = 0)
        {
            MSG_GM_CHARACTER_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_CHARACTER_INFO>(stream);
            Log.Write("Globla server request uid {0} name {1} info", msg.Uid, msg.Name);
            MSG_MG_CHARACTER_INFO response = new MSG_MG_CHARACTER_INFO();
            response.CustomUid = msg.CustomUid;
            if (msg.Uid == 0 && msg.Name == string.Empty)
            {
                Write(response);
                return;
            }
            Api.GameDBPool.Call(new QueryCharacterInfo(msg.Uid, msg.Name), ret =>
            {
                Write(response);
            });
        }

        private void OnResponse_UnVoice(MemoryStream stream, int uid = 0)
        {
            MSG_GM_UNVOICE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_UNVOICE>(stream);
            MSG_MG_CUSTOM_RESULT response = new MSG_MG_CUSTOM_RESULT();
            response.CustomUid = msg.CustomUid;
            if (msg.ServerId != api.MainId)
            {
                return;
            }
            UpdateTipOffState(msg.Uid, GMOperate.UnVoice);
           
            api.GameDBPool.Call(new QueryUpdateSilenceTime(msg.Uid, BaseApi.now.AddMinutes(msg.Minutes).ToString(), msg.Reason));
            ZoneServer zServer = Api.ZoneServerManager.GetClientZone(msg.Uid);
            if (zServer != null)
            {
                // 被禁言角色在zone内存中 通知 同步禁言状态
                MSG_MZ_UNVOICE notify = new MSG_MZ_UNVOICE();
                notify.Uid = msg.Uid;
                notify.Reason = msg.Reason;
                notify.Time = BaseApi.now.AddMinutes(msg.Minutes).ToString();
                zServer.Write(notify);
            }
        }

        private void OnResponse_Voice(MemoryStream stream, int uid = 0)
        {
            MSG_GM_VOICE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_VOICE>(stream);
            if (msg.ServerId != Api.MainId)
            {
                return;
            }
            UpdateTipOffState(msg.Uid, GMOperate.Voice);

            Api.GameDBPool.Call(new QueryUpdateSilenceTime(msg.Uid, DateTime.MinValue.ToString(), ""));
            ZoneServer zServer = Api.ZoneServerManager.GetClientZone(msg.Uid);
            if (zServer != null)
            {
                // 被禁言角色在zone内存中 通知 同步禁言状态
                MSG_MZ_VOICE notify = new MSG_MZ_VOICE();
                notify.Uid = msg.Uid;
                zServer.Write(notify);
            }
        }

        private void OnResponse_CustomAnnouncement(MemoryStream stream, int uid = 0)
        {
            MSG_GM_CUSTOM_ANNOUNCEMENT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_CUSTOM_ANNOUNCEMENT>(stream);

            ZoneServerManager zoneManager = Api.ZoneServerManager;
            if (msg.ServerId != Api.MainId) return;
            MSG_MZ_BROADCAST_ANNOUNCEMENT announcement = new MSG_MZ_BROADCAST_ANNOUNCEMENT();
            announcement.Type = (int)ANNOUNCEMENT_TYPE.CUSTOM_SYSTEM;
            announcement.List.Add(msg.Content);
            zoneManager.Broadcast(announcement);
        }

        private void OnResponse_MovePlayerCity(MemoryStream stream, int uid = 0)
        {
            MSG_GM_MOVE_PLAYER_CITY msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_MOVE_PLAYER_CITY>(stream);
            ZoneServerManager zoneManager = Api.ZoneServerManager;
            if (msg.ServerId != Api.MainId) return;
            Data cityData = DataListManager.inst.GetData("Zone", CONST.MAIN_MAP_ID);
            if (cityData == null)
            {
                return;
            }
            float beginX = cityData.GetFloat("BeginPosX");
            float beginY = cityData.GetFloat("BeginPosY");
            Api.GameDBPool.Call(new QueryMovePlayer(msg.Uid, CONST.MAIN_MAP_ID, beginX, beginY));
            ZoneServer zServer = zoneManager.GetClientZone(msg.Uid);
            if (zServer != null)
            {
                // 通知
                MSG_MZ_MOVE_PLAYER notify = new MSG_MZ_MOVE_PLAYER();
                notify.Uid = msg.Uid;
                notify.MapId = 1;
                notify.PosX = beginX;
                notify.PosY = beginY;
                zServer.Write(notify);
            }
        }

        private void OnResponse_Freeze(MemoryStream stream, int uid = 0)
        {
            MSG_GM_FREEZE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_FREEZE>(stream);
            //if (msg.ServerId != Api.MainId)
            //{
            //    return;
            //}
            // 先踢下线
            MSG_MZ_KICK_PLAYER request = new MSG_MZ_KICK_PLAYER();
            request.Uid = msg.Uid;
            ZoneServerManager zoneManager = Api.ZoneServerManager;
            foreach (var zone in zoneManager.ServerList)
            {
                zone.Value.Write(request);
            }
            DateTime freezeTime = BaseApi.now.AddHours(msg.Hours);
            Api.GameDBPool.Call(new QueryUpdateFreezePlayer(msg.Uid, (int)FreezeState.Freeze, freezeTime.ToString(), msg.Reason));
            UpdateTipOffState(msg.Uid, GMOperate.Freeze);
        }

        private void OnResponse_UnFreeze(MemoryStream stream, int uid = 0)
        {
            MSG_GM_UNFREEZE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_UNFREEZE>(stream);
            //if (msg.ServerId != Api.MainId) return;
            Api.GameDBPool.Call(new QueryUpdateFreezePlayer(msg.Uid, 0, DateTime.MinValue.ToString(), ""));
            UpdateTipOffState(msg.Uid, GMOperate.UnFreeze);
        }

        private void OnResponse_Bag(MemoryStream stream, int uid = 0)
        {
            MSG_GM_BAG msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_BAG>(stream);
            ZoneServerManager zoneManager = Api.ZoneServerManager;
            if (msg.ServerId != Api.MainId) return;
            MSG_MG_BAG response = new MSG_MG_BAG();
            response.CustomUid = msg.CustomUid;
            Api.GameDBPool.Call(new QueryBag(msg.Uid), ret =>
            {
                if (ret != null)
                {
                    List<ItemInfo> list = (List<ItemInfo>)ret;
                    foreach (var item in list)
                    {
                        MSG_MG_BAG.Types.Item info = new MSG_MG_BAG.Types.Item();
                        info.ItemId = item.Uid;
                        info.ItemType = item.TypeId;
                        info.Num = item.PileNum;
                        response.List.Add(info);
                    }
                    Write(response);
                }
            });
        }

        private void OnResponse_OrderState(MemoryStream stream, int uid = 0)
        {
            MSG_GM_ORDER_STATE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_ORDER_STATE>(stream);
            MSG_MG_ORDER_STATE response = new MSG_MG_ORDER_STATE();
            response.CustomUid = msg.CustomUid;
            response.Uid = msg.Uid;
            response.OrderId = msg.OrderId;
            QueryOrderState query = new QueryOrderState(msg.Uid, msg.OrderId);
            Api.GameDBPool.Call(query, ret =>
            {
                response.Money = query.money;
                response.Time = query.time;
                response.State = query.state;
                response.ProductId = query.productId;
                Write(response);
            });
        }

        private void OnResponse_DeleteBagItem(MemoryStream stream, int uid = 0)
        {
            MSG_GM_DELETE_BAG_ITEM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_DELETE_BAG_ITEM>(stream);
            MSG_MG_CUSTOM_RESULT response = new MSG_MG_CUSTOM_RESULT();
            ZoneServerManager zoneManager = Api.ZoneServerManager;
            if (msg.ServerId != Api.MainId) return;
            ZoneServer zServer = zoneManager.GetClientZone(msg.Uid);
            if (zServer != null)
            {
                // 必须先冻结才能删除物品
                response.CustomUid = msg.CustomUid;
                response.Result = 0;
                Write(response);
                return;
            }
            else
            {
                response.CustomUid = msg.CustomUid;
                response.Result = 1;
                Write(response);
                Api.GameDBPool.Call(new QueryDeleteBagItem(msg.Uid, msg.ItemId));
            }
        }

        //private void OnResponse_AccountId(MemoryStream stream, int uid = 0)
        //{
        //    MSG_GM_ACCOUNT_ID msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_ACCOUNT_ID>(stream);
        //    MSG_MG_ACCOUNT_ID response = new MSG_MG_ACCOUNT_ID();
        //    response.CustomUid = msg.CustomUid;
        //    server.DB.Call(new QueryAccountId(msg.Uid, msg.Name), DBProxyDefault.DefaultTableName, DBProxyDefault.DefaultOperateType, ret =>
        //    {
        //        response.accountId = (int)ret;
        //        Write(response);
        //    });
        //}

        private void OnResponse_RepairOrder(MemoryStream stream, int uid = 0)
        {
            MSG_GM_REPAIR_ORDER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_REPAIR_ORDER>(stream);
            MSG_MG_CUSTOM_RESULT response = new MSG_MG_CUSTOM_RESULT();
            ZoneServerManager zoneManager = Api.ZoneServerManager;
            if (msg.ServerId != Api.MainId) return;
            Log.Warn("custom client request repair order main {0} uid {1} orderId {2} orderInfo {3}", msg.ServerId, msg.Uid, msg.OrderId, msg.OrderInfo);
            response.CustomUid = msg.CustomUid;

            Api.RechargeMng.UpdateRechargeManager(msg.OrderId, msg.OrderInfo, ManagerServerApi.now, msg.Amount, "Repair", RechargeWay.Repair, "1", "0");
            response.Result = 1;
            Write(response);
        }

        private void OnResponse_VirtualRecharge(MemoryStream stream, int uid = 0)
        {
            MSG_GM_VIRTUAL_RECHARGE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_VIRTUAL_RECHARGE>(stream);
            Log.Warn("custom client request add virtual recharge main {0} uid {1} money {2}", msg.ServerId, msg.Uid, msg.Cash);
            ZoneServerManager zoneManager = Api.ZoneServerManager;
            if (msg.ServerId != Api.MainId) return;
            Api.GameDBPool.Call(new QueryVirtualRecharge(msg.Uid, msg.Cash));
            ZoneServer zServer = zoneManager.GetClientZone(msg.Uid);
            if (zServer != null)
            {
                // 在线更新内存
                MSG_MZ_VIRTUAL_RECHARGE notify = new MSG_MZ_VIRTUAL_RECHARGE();
                notify.Uid = msg.Uid;
                notify.Money = msg.Cash;
                zServer.Write(notify);
                return;
            }
        }

        //private void OnResponse_SendItem(MemoryStream stream, int uid = 0)
        //{
        //    MSG_GM_SEND_ITEM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_SEND_ITEM>(stream);
        //    ZoneManager zoneManager = server.ZoneMng;
        //    if (msg.ServerId != server.MainId) return;
        //    Log.Warn("custom client request send item to main {0} uid {1} item {2}", msg.ServerId, msg.Uid, msg.item);
        //    MSG_MR_SEND_ITEM notify = new MSG_MR_SEND_ITEM();
        //    notify.serverId = msg.ServerId;
        //    notify.item = msg.item.Replace('@','|');
        //    notify.Uid = msg.Uid;
        //    server.Relation.Write(notify);
        //}

        //private void OnResponse_SendMail(MemoryStream stream, int uid = 0)
        //{
        //    MSG_GM_SEND_MAIL msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_SEND_MAIL>(stream);
        //    if (msg.ServerId != server.MainId) return;
        //    Log.Warn("custom client request send email main {0} uid {1} mail {2}", msg.ServerId, msg.Uid, msg.mailId);
        //    MSG_MR_SEND_MAIL notify = new MSG_MR_SEND_MAIL();
        //    notify.serverId = msg.ServerId;
        //    notify.MailId = msg.mailId;
        //    notify.Uid = msg.Uid;
        //    notify.reward = msg.reward;
        //    server.Relation.Write(notify);
        //}

        //private void OnResponse_ReceiveChannelTask(MemoryStream stream, int uid = 0)
        //{ 
        //    MSG_GM_RECEIVE_CHANNEL_TASK msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_RECEIVE_CHANNEL_TASK>(stream);
        //    MSG_MG_RECEIVE_CHANNEL_TASK response = new MSG_MG_RECEIVE_CHANNEL_TASK();
        //    response.ResIndex = msg.ResIndex;
        //    ReceiveChannelTaskReward(response, msg.AccountName, msg.CharName, msg.TaskId);
        //}

        private void OnResponse_BadWords(MemoryStream stream, int uid = 0)
        {
            MSG_GM_BAD_WORDS msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_BAD_WORDS>(stream);
            MSG_MZ_BAD_WORDS notify = new MSG_MZ_BAD_WORDS();
            notify.Content = msg.Content;
            Api.ZoneServerManager.Broadcast(notify);
        }

        private void OnResponse_HKUserInfo(MemoryStream stream, int uid = 0)
        {
            MSG_GM_HK_USER_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_HK_USER_INFO>(stream);
            Log.Write("global request channel uid {0} resIndex {1} hk main {2} user info", msg.ChannelUid, msg.ResIndex, msg.MainId);
            MSG_MG_HK_USER_INFO response = new MSG_MG_HK_USER_INFO();
            response.ResIndex = msg.ResIndex;
            ZoneServerManager zoneManager = Api.ZoneServerManager;
            if (msg.MainId != Api.MainId)
            {
                response.ErrCode = 65535;
                Write(response);
                return;
            }
            // 查找角色信息
            Api.GameDBPool.Call(new QueryHKUserInfo(msg.ChannelUid), ret =>
            {
                try
                {
                    List<MSG_MG_HK_USER_INFO.Types.UserInfo> list = (List<MSG_MG_HK_USER_INFO.Types.UserInfo>)ret;
                    foreach (var item in list)
                    {
                        response.UserList.Add(item);
                    }
                    response.ErrCode = 0;
                    Write(response);
                }
                catch (Exception e)
                {
                    Log.Alert(e.ToString());
                }
            });
        }

        public void OnResponseReloadFamily(MemoryStream stream, int uid = 0)
        {
            MSG_GM_RELOAD_FAMILY msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_RELOAD_FAMILY>(stream);
            Log.Write("global request reload family main {0} familyId {1}", msg.MainId, msg.FamilyId);
            MSG_MR_RELOAD_FAMILY notify = new MSG_MR_RELOAD_FAMILY();
            notify.MainId = msg.MainId;
            notify.FamillyId = msg.FamilyId;
            if (Api.RelationServer != null)
            {
                Api.RelationServer.Write(notify);
            }
        }

        private void OnResponse_WaitCount(MemoryStream stream, int uid = 0)
        {
            MSG_GM_WAIT_COUNT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_WAIT_COUNT>(stream);
            CONST.ONLINE_COUNT_WAIT_COUNT = msg.Count;
        }

        private void OnResponse_FullCount(MemoryStream stream, int uid = 0)
        {
            MSG_GM_FULL_COUNT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_FULL_COUNT>(stream);
            CONST.ONLINE_COUNT_FULL_COUNT = msg.Count;
        }


        private void OnResponse_SetRelationFps(MemoryStream stream, int uid = 0)
        {
            MSG_GM_SET_Relation_FPS msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_SET_Relation_FPS>(stream);
            MSG_MG_COMMAND_RESULT response = new MSG_MG_COMMAND_RESULT();

            MSG_MR_SET_FPS notify = new MSG_MR_SET_FPS();
            notify.FPS = msg.Fps;
            if (Api.RelationServer != null)
            {
                Api.RelationServer.Write(notify);
            }
        }
        private void OnResponse_SetZoneFps(MemoryStream stream, int uid = 0)
        {
            MSG_GM_SET_ZONE_FPS msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_SET_ZONE_FPS>(stream);
            MSG_MG_COMMAND_RESULT response = new MSG_MG_COMMAND_RESULT();
            ZoneServerManager zoneManager = Api.ZoneServerManager;
            if (msg.MainId != Api.MainId)
            {
                response.Success = false;
                response.Info.Add(String.Format("setZoneFps main {0} sub {1} failed: find zone manager failed", msg.MainId, msg.SubId));
                Write(response);
                return;
            }

            MSG_MZ_SET_FPS notify = new MSG_MZ_SET_FPS();
            notify.FPS = msg.Fps;

            if (msg.SubId == 0)
            {
                //foreach (var zone in zoneManager.ZoneList)
                //{
                //    zone.Value.State = ServerState.Stopping;
                //    zone.Value.Write(notify);
                //}
                zoneManager.Broadcast(notify);
            }
            else
            {
                // 关闭指定zone
                ZoneServer zone = (ZoneServer)zoneManager.GetServer(MainId, msg.SubId);
                if (zone == null)
                {
                    response.Success = false;
                    response.Info.Add(String.Format("setZoneFps main {0} sub {1} failed: find zone failed", msg.MainId, msg.SubId));
                    Write(response);
                    return;
                }
                zone.Write(notify);
            }
        }

        private void OnResponse_SetFps(MemoryStream stream, int uid = 0)
        {
            MSG_GM_SET_FPS msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_SET_FPS>(stream);
            Api.Fps.SetFPS(msg.FPS);

            MSG_MG_COMMAND_RESULT response = new MSG_MG_COMMAND_RESULT();
            response.Success = true;
            response.Info.Add(String.Format("setmanagerFps main {0} successful", Api.MainId));
            Write(response);
        }

        private void OnResponse_PlayerLevel(MemoryStream stream, int uid = 0)
        {
            MSG_GM_PLAYER_LEVEL msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_PLAYER_LEVEL>(stream);
            Log.Write("gm request set player {0} level {1}", msg.Level, msg.Level);
            ZoneServerManager zoneManager = Api.ZoneServerManager;
            if (msg.MainId != Api.MainId)
            {
                Log.Warn("gm request set player {0} level {1} failed: main id {2} not exist", msg.Uid, msg.Level, msg.MainId);
                return;
            }
            ZoneServer zone = zoneManager.GetClientZone(msg.Uid);
            if (zone == null)
            {
                // 不在线且无离线缓存 直接更新数据库
                Api.GameDBPool.Call(new QueryUpdatePlayerLevel(msg.Uid, msg.Level, ManagerServerApi.now));
            }
            else
            {
                // 通知对应zone 更新内存和数据库
                MSG_MZ_PLAYER_LEVEL notify = new MSG_MZ_PLAYER_LEVEL();
                notify.Uid = msg.Uid;
                notify.Level = msg.Level;
                zone.Write(notify);
            }
        }

        private void OnResponse_PlayerExp(MemoryStream stream, int uid = 0)
        {
        }
        private void OnResponse_BuzyOnlineCount(MemoryStream stream, int uid = 0)
        {
            MSG_GM_BUZY_ONLINE_COUNT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_BUZY_ONLINE_COUNT>(stream);
            Log.Write("gm request set buzy online count {0}", msg.Count);
            MapBallenceProxy.BuzyOnlineCount = msg.Count;
        }


        private void OnResponse_GetTipOffInfo (MemoryStream stream, int uid = 0)
        {
            MSG_GM_TIP_OFF_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_TIP_OFF_INFO>(stream);
            Log.Write("gm request get tip off info startServerId {0} endServerId {1}", msg.StartServerId, msg.EndServerId);

            string endTime = ManagerServerApi.now.ToString(CONST.DATETIME_TO_STRING_2 + " 23:59:59");
            string beginTime = ManagerServerApi.now.AddDays(-1).ToString(CONST.DATETIME_TO_STRING_2 + " 00:00:00");//

            if ((TipOffType)msg.Type == TipOffType.All)
            {
                QueryLoadAllTypeTipOffInfo query = new QueryLoadAllTypeTipOffInfo(msg.StartServerId, msg.EndServerId, beginTime, endTime, msg.CurPage, msg.PageSize);
                Api.AccountDBPool.Call(query, ret =>
                {
                    SendTipOffMsgToGlobal(query.Infos, msg.CustomUid, msg.CurPage, query.TotalCount);
                });
            }
            else
            {
                QueryLoadOneTypeTipOffInfo query = new QueryLoadOneTypeTipOffInfo(msg.Type, msg.StartServerId, msg.EndServerId, beginTime, endTime, msg.CurPage, msg.PageSize);
                Api.AccountDBPool.Call(query, ret =>
                {
                    SendTipOffMsgToGlobal(query.Infos, msg.CustomUid, msg.CurPage, query.TotalCount);
                });
            }
        }

        private void SendTipOffMsgToGlobal(List<TipOffInfo> infoList, int customUid, int curPage, int totalCount)
        {
            MSG_MG_TIP_OFF_INFO response = new MSG_MG_TIP_OFF_INFO();
            response.CustomUid = customUid;
            foreach (var item in infoList)
            {
                TIP_OFF_INFO info = new TIP_OFF_INFO()
                {
                    Id = item.Id,
                    ServerId = item.ServerId,
                    Name = item.Name,
                    DestUid = item.DestUid,
                    DestName = item.DestName,
                    Type = item.Type,
                    Content = item.Content,
                    Description = item.Description,
                    Time = item.Time
                };             
                response.List.Add(info);
            }
            response.CurPage = curPage;
            response.TotalCount = totalCount;
            Write(response);
        }

        private void OnResponse_IgnoreTipOff(MemoryStream stream, int uid = 0)
        {
            MSG_GM_IGNORE_TIP_OFF msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_IGNORE_TIP_OFF>(stream);
            Log.Write("gm request ignore tip off id {0}", msg.Id);
            QueryUpdateTipOffIgnoreState query = new QueryUpdateTipOffIgnoreState(msg.Id, 1);
            Api.AccountDBPool.Call(query, ret =>
            {
                MSG_MG_IGNORE_TIP_OFF response = new MSG_MG_IGNORE_TIP_OFF();
                response.CustomUid = msg.CustomUid;
                Write(response);
            });
        }

        private void OnResponse_AddReward(MemoryStream stream, int uid = 0)
        {
            MSG_GM_ADD_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_ADD_REWARD>(stream);
            Log.Write("gm request player add reward {0}", msg.Reward);
            MSG_MZ_ADD_REWARD response = new MSG_MZ_ADD_REWARD();
            response.Reward = msg.Reward;
            Api.ZoneServerManager.Broadcast(response);
        }

        private void OnResponse_EquipHero(MemoryStream stream, int uid = 0)
        {
            MSG_GM_EQUIP_HERO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_EQUIP_HERO>(stream);
            Log.Write("gm request player equip hero {0}", msg.HeroId);
            MSG_MZ_EQUIP_HERO response = new MSG_MZ_EQUIP_HERO();
            response.HeroId = msg.HeroId;
            response.Equip = msg.Equip;
            Api.ZoneServerManager.Broadcast(response);
        }

        private void OnResponse_AllChat(MemoryStream stream, int uid = 0)
        {
            MSG_GM_ALL_CHAT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_ALL_CHAT>(stream);
            Log.Write("gm request player all chat, channel {0} words {1} param {2}", msg.Channel, msg.Words, msg.Param);
            MSG_MZ_ALL_CHAT response = new MSG_MZ_ALL_CHAT();
            response.Channel = msg.Channel;
            response.Words = msg.Words;
            response.Param = msg.Param;
            Api.ZoneServerManager.Broadcast(response);
        }

        private void OnResponse_AbsorbSoulRing(MemoryStream stream, int uid = 0)
        {
            MSG_GM_ABSORB_SOULRING msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_ABSORB_SOULRING>(stream);
            Log.Write("gm request player absorb soulirng hero {0} slot {1}", msg.HeroId, msg.Slot);
            MSG_MZ_ABSORB_SOULRING response = new MSG_MZ_ABSORB_SOULRING();
            response.HeroId = msg.HeroId;
            response.Slot = msg.Slot;
            Api.ZoneServerManager.Broadcast(response);
        }

        private void OnResponse_AbsorbFinish(MemoryStream stream, int uid = 0)
        {
            MSG_GM_ABSORB_FINISH msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_ABSORB_FINISH>(stream);
            Log.Write("gm request player absorb finish hero {0}", msg.HeroId);
            MSG_MZ_ABSORB_FINISH response = new MSG_MZ_ABSORB_FINISH();
            response.HeroId = msg.HeroId;
            Api.ZoneServerManager.Broadcast(response);
        }

        private void OnResponse_AddHeroExp(MemoryStream stream, int uid = 0)
        {
            MSG_GM_ADD_HEROEXP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_ADD_HEROEXP>(stream);
            Log.Write("gm request player add hero exp {0}", msg.Exp);
            MSG_MZ_ADD_HEROEXP response = new MSG_MZ_ADD_HEROEXP();
            response.Exp = msg.Exp;
            Api.ZoneServerManager.Broadcast(response);
        }

        private void OnResponse_HeroAwaken(MemoryStream stream, int uid = 0)
        {
            MSG_GM_HERO_AWAKEN msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_HERO_AWAKEN>(stream);
            Log.Write("gm request player hero {0} awaken", msg.HeroId);
            MSG_MZ_HERO_AWAKEN response = new MSG_MZ_HERO_AWAKEN();
            response.HeroId = msg.HeroId;
            Api.ZoneServerManager.Broadcast(response);
        }

        private void OnResponse_HeroLevelUp(MemoryStream stream, int uid = 0)
        {
            MSG_GM_HERO_LEVELUP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_HERO_LEVELUP>(stream);
            Log.Write("gm request player hero {0} level up", msg.HeroId);
            MSG_MZ_HERO_LEVELUP response = new MSG_MZ_HERO_LEVELUP();
            response.HeroId = msg.HeroId;
            Api.ZoneServerManager.Broadcast(response);
        }

        private void OnResponse_UpdateHeroPos(MemoryStream stream, int uid = 0)
        {
            MSG_GM_UPDATE_HERO_POS msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_UPDATE_HERO_POS>(stream);
            Log.Write("gm request player update hero pos {0}", msg.HeroPos);
            MSG_MZ_UPDATE_HERO_POS response = new MSG_MZ_UPDATE_HERO_POS();
            response.HeroPos = msg.HeroPos;
            Api.ZoneServerManager.Broadcast(response);
        }

        private void OnResponse_GiftOpen(MemoryStream stream, int uid = 0)
        {
            MSG_GM_GIFT_OPEN msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_GIFT_OPEN>(stream);
            Log.Write("gm request player gift {0} open", msg.GiftItemId);
            MSG_MZ_GIFT_OPEN response = new MSG_MZ_GIFT_OPEN();
            response.GiftItemId = msg.GiftItemId;
            Api.ZoneServerManager.Broadcast(response);
        }

        private void UpdateTipOffState(int destUid, GMOperate operate)
        {
            QueryLoadDestUidTipOffStateInfo queryTipState = new QueryLoadDestUidTipOffStateInfo(destUid);
            api.AccountDBPool.Call(queryTipState, ret =>
            {             
                if (queryTipState.Infos.Count > 0)
                {
                    switch (operate)
                    {
                        case GMOperate.UnVoice:
                            api.AccountDBPool.Call(new QueryUpdateTipOffUnVoice(destUid, 1));
                            break;
                        case GMOperate.Voice:
                            api.AccountDBPool.Call(new QueryUpdateTipOffUnVoice(destUid, 0));
                            break;
                        case GMOperate.Freeze:
                            api.AccountDBPool.Call(new QueryUpdateTipOffFreeze(destUid, 1));
                            break;
                        case GMOperate.UnFreeze:
                            api.AccountDBPool.Call(new QueryUpdateTipOffFreeze(destUid, 0));
                            break;                     
                        default:
                            break;
                    }                
                }
            });
            
        }
    }
}