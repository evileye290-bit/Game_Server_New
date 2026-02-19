using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateCM;
using Message.Gate.Protocol.GateM;
using Message.Gate.Protocol.GateZ;
using Message.IdGenerator;
using Message.Zone.Protocol.ZGate;
using ServerFrame;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GateServerLib
{
    public partial class ZoneServer
    {
        private void OnResponse_EnterOtherZone(MemoryStream stream, int uid)
        {
            MSG_ZGate_ENTER_OTHER_ZONE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZGate_ENTER_OTHER_ZONE>(stream);
            Client client = Api.ClientMng.FindClientByUid(uid);
            if (client == null) return;
            Log.Write("player {0} enter new zone main {1} sub {2}",uid, msg.MainId, msg.SubId);
            BackendServer newZone = Api.ZoneServerManager.GetServer (msg.MainId, msg.SubId);
            if (newZone == null)
            {
                Log.Warn("player {0} enter new zone main {1} sub {2} failed: zone not exists", uid, msg.MainId, msg.SubId);
                return;
            }
            client.ChangeZone(newZone, msg.MapId, msg.Channel, false);
        }

        private void OnResponse_LeaveWorld(MemoryStream stream, int uid)
        { 
            // 特殊情况，从zone发起player下线操作，如GM踢人情况
            MSG_ZGate_LeaveWorld msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZGate_LeaveWorld>(stream);
            Log.Write("player {0} has to leave world", uid);
            Client client = Api.ClientMng.FindClientByUid(uid);
            if (client == null) return;
            client.EnableCatch(false);
            Api.ClientMng.RemoveClient(client);
        }
         
        private void OnResponse_EnterZone(MemoryStream stream, int uid)
        {
            //MSG_ZGate_ENTER_ZONE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZGate_ENTER_ZONE>(stream);
            //if (msg.Result == (int)ErrorCode.Success)
            //{
            //    Log.Write("player {0} enter map {1} channel {2}", uid, msg.MapId, msg.Channel);
            //}
            //else if (msg.Result == (int)ErrorCode.NotOpen)
            //{
            //    Log.Write("player {0} move to a closed map", uid);
            //}
            //else if (msg.Result == (int)ErrorCode.FullPC)
            //{
            //    Log.Write("player {0} move to a fullpc map ", uid);
            //}
            //Client client = server.ClientMng.FindClientByUid(uid);
            //if (client == null) return;
          
            //MSG_GC_ENTER_ZONE notify = new MSG_GC_ENTER_ZONE();
            //notify.Result = msg.Result;
            //notify.MapId = msg.MapId;
            //notify.MainId = msg.MainId;
            //notify.Channel = msg.Channel;
            //notify.instanceId = msg.instanceId;
            //notify.posX = msg.posX;
            //notify.posY = msg.posY;
            //client.Write(notify);
            int pcUid = uid;
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_GC_ENTER_ZONE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} not find client(OnResponse_EnterZone)", pcUid);
            }
        }

        private void OnResponse_EnterWorld(MemoryStream stream, int uid)
        {
            //MSG_GC_ENTER_WORLD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GC_ENTER_WORLD>(stream);
            int pcUid = uid;
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null )
            {
                client.Write(Id<MSG_GC_ENTER_WORLD>.Value, stream);

                //获取聊天室信息
                //MSG_GateCM_GET_WORLD_ROOM roomRequest = new MSG_GateCM_GET_WORLD_ROOM();
                //roomRequest.PcUid = client.Uid;
                //Api.ChatManagerServer.Write(roomRequest);
            }
            else
            {
                Log.WarnLine("player {0} not find client(OnResponse_EnterWorld)", pcUid);
            }
        }

        private void OnResponse_TimeSync(MemoryStream stream, int uid)
        {
            //MSG_GC_TIME_SYNC msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GC_TIME_SYNC>(stream);
            int pcUid = uid;
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_GC_TIME_SYNC>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} not find client(OnResponse_TimeSync)", pcUid);
            }
        }

        private void OnResponse_ChangeChannel(MemoryStream stream, int uid)
        {
            Client client = Api.ClientMng.FindClientByUid(uid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CHANGE_CHANNEL>.Value, stream);
            }
        }

        private void OnResponse_LoginOtherManager(MemoryStream stream, int uid)
        {
            Client client = Api.ClientMng.FindClientByUid(uid);
            if (client == null)
            {
                return;
            }
            MSG_ZGate_LOGIN_OTHER_MANAGER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZGate_LOGIN_OTHER_MANAGER>(stream);
            Log.Write("player {0} login to other manager {1} map {2} channel {3}", uid, msg.DestMainId, msg.MapId, msg.Channel);
            BackendServer manager = Api.ManagerServerManager.GetSinglePointServer(msg.DestMainId);
            if (manager == null)
            {
                Log.Warn("player {0} login to other manager {1} map {2} channel {3} failed: can not find manager", uid, msg.DestMainId, msg.MapId, msg.Channel);
                Api.ClientMng.RemoveClient(client);
                return;
            }
            // 通过 开始跨manager登录
            MSG_GateM_FORCE_LOGIN request = new MSG_GateM_FORCE_LOGIN();
            request.Uid = uid;
            request.MainId = msg.DestMainId;
            request.MapId = msg.MapId;
            request.Channel = msg.Channel;
            request.SyncData = false;
            manager.Write(request);
        }

        private void OnResponse_ReconnectLogin(MemoryStream stream, int uid)
        {
            Client client = Api.ClientMng.FindClientByUid(uid);
            if (client != null)
            {
                client.Write(Id<MSG_GC_RECONNECT_LOGIN>.Value, stream);

                //获取聊天室信息
                //MSG_GateCM_GET_WORLD_ROOM roomRequest = new MSG_GateCM_GET_WORLD_ROOM();
                //roomRequest.PcUid = client.Uid;
                //Api.ChatManagerServer.Write(roomRequest);
            }
        }

        private void OnResponse_Kick(MemoryStream stream, int uid)
        {
            Client client = Api.ClientMng.FindClientByUid(uid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_KICK>.Value, stream);
            }
        }
    }
}
