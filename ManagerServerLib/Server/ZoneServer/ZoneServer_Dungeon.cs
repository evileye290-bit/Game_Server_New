using System;
using EnumerateUtility;
using Logger;
using Message.Manager.Protocol.MZ;
using Message.Zone.Protocol.ZM;
using ServerFrame;
using System.IO;
using DataProperty;
using DBUtility;
using Message.Manager.Protocol.MR;
using Message.Relation.Protocol.RZ;
using Message.Zone.Protocol.ZR;
using ServerShared;
using ServerModels;

namespace ManagerServerLib
{
    public partial class ZoneServer : FrontendServer
    {
        private void OnResponse_NeedDungeon(MemoryStream stream, int uid = 0)
        {
            MSG_ZM_NEED_DUNGEON msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZM_NEED_DUNGEON>(stream);
            Log.Write($"player {msg.Uid} from sub {SubId} need dungeon {msg.DestDungeonId}");
            MSG_MZ_NEED_DUNGEON_FAILED response = new MSG_MZ_NEED_DUNGEON_FAILED();
            response.Uid = msg.Uid;
            response.DestDungeonId = msg.DestDungeonId;
            Client client = null;
            if (clientListZone.TryGetValue(msg.Uid, out client) == false)
            {
                Log.Warn("player {0} need dungeon {1} failed: client not exists", msg.Uid, msg.DestDungeonId);
                response.Result = (int)ErrorCode.NotExist;
                Write(response);
                return;
            }
            ZoneServer destZone = Api.ZoneServerManager.FindOneDungeonServer();
            if (destZone == null)
            {
                Log.Warn("player {0} need dungeon {1} failed: find no dest zone", msg.Uid, msg.DestDungeonId);
                response.Result = (int)ErrorCode.NotExist;
                Write(response);
                return;
            }

            // 成功 通知destZone 创建副本
            MSG_MZ_CREATE_DUNGEON notify = new MSG_MZ_CREATE_DUNGEON();
            notify.Uid = msg.Uid;
            notify.OriginZoneSubId = SubId;
            notify.DestDunegonId = msg.DestDungeonId;
            notify.OriginMapId = msg.OriginMapId;
            notify.OriginChannel = msg.OriginChannel;
            notify.OriginPosX = msg.OriginPosX;
            notify.OriginPosY = msg.OriginPosY;
            notify.TheoryMemberCount = msg.TheoryMemberCount;
            notify.TeamId = msg.TeamId;
            notify.HuntingHelp = msg.HuntingHelp;
            destZone.Write(notify);

            client.DestZone = destZone;
        }

        private void OnResponse_CreateDungeonFailed(MemoryStream stream, int uid = 0)
        {
            MSG_ZM_CREATE_DUNGEON_FAILED msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZM_CREATE_DUNGEON_FAILED>(stream);
            Log.Warn($"player {msg.Uid} create dungeon {msg.DestDunegonId} failed: {msg.Result}");

            ZoneServer originZone = Api.ZoneServerManager.GetServer(MainId, msg.OriginZoneSubId) as ZoneServer;
            if(originZone == null)
            {
                return;
            }

            Client client = originZone.GetClient(msg.Uid);
            if(client != null)
            {
                client.DestZone = null;
            }

            MSG_MZ_NEED_DUNGEON_FAILED notify = new MSG_MZ_NEED_DUNGEON_FAILED();
            notify.Uid = msg.Uid;
            notify.DestDungeonId = msg.DestDunegonId;
            notify.Result = msg.Result;
            originZone.Write(notify);
        }

        private void OnResponse_HuntingChange(MemoryStream stream, int uid = 0)
        {
            MSG_ZMZ_HUNTING_CHANGE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZMZ_HUNTING_CHANGE>(stream);

            ZoneServerManager zoneManager = Api.ZoneServerManager;

            MSG_MZ_HUNTING_CHANGE response = new MSG_MZ_HUNTING_CHANGE()
            {
                Uid = msg.Uid,
                ResearchChange = msg.ResearchChange, 
                PassedId = msg.PassedId, 
                IsActivity = msg.IsActivity,
                HuntingIntrude = msg.HuntingIntrude,
            };

            Client client = zoneManager.GetClient(msg.Uid);
            if (client != null)
            {
                client.Zone.Write(response);
                return;
            }

            OfflineClient offlineClient = zoneManager.GetOfflineClient(msg.Uid);
            ZoneServer zone = null;
            if (offlineClient != null)
            {
                zone = zoneManager.GetZone(offlineClient.MapId, offlineClient.Channel);
            }

            if (zone != null)
            {
                zone.Write(response);
                return;
            }

            QueryLoadHuntingResearch query = new QueryLoadHuntingResearch(msg.Uid);
            Api.GameDBPool.Call(query, callback =>
            {
                if ((int) callback == 1)
                {
                    int research = Math.Min(query.Research + msg.ResearchChange, DataListManager.inst.GetData("Hunting", 1).GetInt("Research"));
                    if (research > query.Research)
                    {
                        Api.GameDBPool.Call(new QueryUpdateHuntingResearch(msg.Uid, research));
                    }
                    //刷新排行榜
                    Api.RelationServer.Write(new MSG_MR_UPDATE_RANK_VALUE() {RankType = (int) RankType.Hunting, Value = research, MainId = msg.MainId}, msg.Uid);
                }
            });

             //魂兽入侵
            if (msg.HuntingIntrude)
            {
                HuntingIntrudeModel model;
                HuntingIntrudeBuffSuitModel buffSuitModel;
                HuntingLibrary.RandomHuntingIntrude(out model, out buffSuitModel);
                if (model == null)
                {
                    Log.Error($"OnResponse_HuntingChange had not random a valid HuntingIntrudeModel ");
                    return;
                }

                HuntingIntrudeInfo info = new HuntingIntrudeInfo()
                {
                    Uid = msg.Uid,
                    Id = msg.HuntingIntrudeId,
                    IntrudeId = model.Id,
                    BuffSuitId = buffSuitModel.Id,
                    JobLimit = model.RandomJobLimit(),
                    EndTime = BaseApi.now.AddHours(HuntingLibrary.HuntingIntrudeExistHour)
                };
                Api.GameDBPool.Call(new QueryInsertHuntingIntrude(info));
            }
        }
    }
}
