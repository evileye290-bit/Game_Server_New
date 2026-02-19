using CommonUtility;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Manager.Protocol.MZ;
using Message.Zone.Protocol.ZM;
using Message.Zone.Protocol.ZR;
using ServerFrame;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZoneServerLib
{
    public partial class ManagerServer : BackendServer
    {
        private void OnResponse_NeedDungeonFailed(MemoryStream stream, int uid = 0)
        {
            MSG_MZ_NEED_DUNGEON_FAILED msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MZ_NEED_DUNGEON_FAILED>(stream);
            Log.Write($"player {msg.Uid} need dungeon failed {msg.Result}");
            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            if(player == null)
            {
                return;
            }

            // 通知客户端 创建副本失败
            MSG_ZGC_CREATE_DUGEON response = new MSG_ZGC_CREATE_DUGEON();
            response.DungeonId = msg.DestDungeonId;
            response.Result = msg.Result;
            player.SetIsTransforming(false);
            player.Write(response);
        }

        private void OnResponse_CreateDungeon2(MemoryStream stream, int uid = 0)
        {
            MSG_MZ_CREATE_DUNGEON msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MZ_CREATE_DUNGEON>(stream);
            Log.Write($"OnResponse_CreateDungeon2 will create dungeon {msg.DestDunegonId} for player {msg.Uid} from sub id {msg.OriginZoneSubId}");

            int uid2Manager = 0;

            MapModel model=MapLibrary.GetMap(msg.DestDunegonId);
            if (model.IsTeamDungeon())
            {
                uid2Manager = msg.Uid;
            }

            DungeonMap dungeon = Api.MapManager.CreateDungeon(msg.DestDunegonId, msg.GodHeroCount, uid2Manager);
            if (dungeon == null)
            {
                MSG_ZM_CREATE_DUNGEON_FAILED response = new MSG_ZM_CREATE_DUNGEON_FAILED();
                response.Uid = msg.Uid;
                response.OriginZoneSubId = msg.OriginZoneSubId;
                response.Result = (int)ErrorCode.CreateDungeonFailed;
                Write(response);
                return;
            }

            if (dungeon.Model.IsTeamDungeon())
            {
                TeamDungeonMap teamDungeon = dungeon as TeamDungeonMap;
                teamDungeon.InitTeamDungeonMap(msg.TheoryMemberCount, msg.TeamId);
                teamDungeon.SetIsHelpState(msg.HuntingHelp, msg.Uid);
            }

            // manager依旧分配player所在的zone来创建副本，直接进入，无需跨zone
            if (Api.SubId == msg.OriginZoneSubId)
            {
                PlayerChar player = Api.PCManager.FindPc(msg.Uid);
                if (player != null)
                {
                    // 成功 进入副本
                    player.IsAttacker = true;
                    player.SetIsTransforming(false);
                    player.RecordEnterMapInfo(dungeon.MapId, dungeon.Channel, dungeon.BeginPosition);
                    player.RecordOriginMapInfo();
                    player.OnMoveMap();
                }
                else
                {
                    // player下线，副本直接关闭
                    dungeon.Close();
                }
                return;
            }

            // 不同zone 需要做跨zone处理
            MSG_ZM_CLIENT_ENTER_TRANSFORM request = new MSG_ZM_CLIENT_ENTER_TRANSFORM();
            request.Uid = msg.Uid;
            request.OriginSubId = msg.OriginZoneSubId;
            request.Result = (int)ErrorCode.Success;

            EnterMapInfo origin = new EnterMapInfo();
            origin.SetInfo(msg.OriginMapId, msg.OriginChannel, new Vec2(msg.OriginPosX, msg.OriginPosY));
            EnterMapInfo dest = new EnterMapInfo();
            dest.SetInfo(dungeon.MapId, dungeon.Channel, new Vec2(dungeon.BeginPosition));

            PlayerEnter playerEnter = new PlayerEnter(Api, msg.Uid, msg.OriginZoneSubId, origin, dest);
            Api.PCManager.AddPlayerEnter(playerEnter);

            Write(request);
        }

        private void OnResponse_CreatedMap2Manager(MemoryStream stream,int uid = 0)
        {
            MSG_ZM_NEW_MAP_RESPONSE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZM_NEW_MAP_RESPONSE>(stream);
            //进行下一步处理 
            //跨zone会找不到player
            //PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            //if (player == null) return;

            //Log.Write($"player {player.Uid} OnResponse_CreatedMap2Manager {msg.DungeonId}");

            //player.AfterManagerCreateDungeon(msg.DungeonId,msg.Channel);

            DungeonModel dungeonModel = DungeonLibrary.GetDungeon(msg.DungeonId);
            DungeonMap dungeon = Api.MapManager.GetFieldMap(msg.DungeonId, msg.Channel) as DungeonMap;

            // 根据状态 决定在当前zone创建副本还是请求manager做均衡负载
            if (dungeon.Model.IsTeamDungeon())
            {
                //向relation通知队员进入副本
                (dungeon as TeamDungeonMap)?.NotifyTeamMembersEnter(msg.Uid);
            }
        }

        private void OnResponse_ZoneTransform(MemoryStream stream, int uid = 0)
        {
            MSG_MZ_ZONE_TRANSFORM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MZ_ZONE_TRANSFORM>(stream);
            Log.WarnLine($"gm request zone transform server {msg.MainId} from zones {string.Join("-", msg.FromZones)} to zones {string.Join("-", msg.ToZones)}");

            ZoneTransformManager.Instance.UpdateZonesInfo(msg.IsForce, msg.FromZones.ToList(), msg.ToZones.ToList());

            int num = 0 ;


            //强制传送
            if (msg.IsForce && msg.FromZones.Contains(api.SubId))
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                Log.WarnLine($"start transform player zoneType {api.ServerType} zoneid {api.MainId} subid {api.SubId}");

                foreach (var kv in Api.PCManager.PcList)
                {
                    PlayerChar player = kv.Value;
                    if (player.InDungeon || !player.IsOnline() || player.IsTransforming) continue;

                    num++;
                    player.AskForEnterMap(player.CurrentMap.MapId, player.CurrentMap.Channel, player.Position);
                }

                stopwatch.Stop();
                Log.WarnLine($"end transform player zoneType {api.ServerType} zoneid {api.MainId} subid {api.SubId} player count {num} cost time {stopwatch.ElapsedMilliseconds}");
            }
        }


        private void OnResponse_HuntingChange(MemoryStream stream, int uid = 0)
        {
            MSG_MZ_HUNTING_CHANGE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MZ_HUNTING_CHANGE>(stream);
            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            if (player == null)
            {
                player = Api.PCManager.FindOfflinePc(msg.Uid);
                if (player == null) return;
            }

            player.HuntingManager.AddResearch(msg.ResearchChange);
            if (msg.IsActivity)
            {
                player.HuntingManager.AddActivityPassed(msg.PassedId);
            }
            else
            {
                player.HuntingManager.AddPassedId(msg.PassedId);
            }

            if (msg.HuntingIntrude)
            {
                player.AddHuntingIntrude();
            }
        }
    }
}
