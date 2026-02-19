using EnumerateUtility;
using Google.Protobuf.Collections;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Relation.Protocol.RZ;
using Message.Zone.Protocol.ZGate;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public partial class RelationServer
    {
        private void OnResponse_IntegralBossStart(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_INTEGRALBOSS_START msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_INTEGRALBOSS_START>(stream);         
            BroadcastAnnouncement(ANNOUNCEMENT_TYPE.HOUR_BOSS_BATTLE);
        }

        private void OnResponse_IntegralBossEnd(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_INTEGRALBOSS_END msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_INTEGRALBOSS_END>(stream);        
        }

        private void OnResponse_BroadcastAnnouncement(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_BROADCAST_ANNOUNCEMENT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_BROADCAST_ANNOUNCEMENT>(stream);
            BroadcastAnnouncement(msg.Type, msg.List);
        }

        public void BroadcastAnnouncement(int type, RepeatedField<string> list)
        {
            MSG_ZGate_BROADCAST_ANNOUNCEMENT msg = new MSG_ZGate_BROADCAST_ANNOUNCEMENT();
            msg.Type = type;
            foreach (var item in list)
            {
                msg.List.Add(item);
            }
            Api.GateManager.Broadcast(msg);

            Log.Write("relation BroadcastAnnouncement type {0} list count {1}", type, list.Count);
        }

        private void BroadcastAnnouncement(ANNOUNCEMENT_TYPE type)
        {
            MSG_ZGate_BROADCAST_ANNOUNCEMENT msg = new MSG_ZGate_BROADCAST_ANNOUNCEMENT();
            List<string> list = new List<string>();
            msg.Type = (int)type;
            foreach (var item in list)
            {
                msg.List.Add(item);
            }
            Api.GateManager.Broadcast(msg);

            Log.Write("BroadcastAnnouncement type {0} list count {1}", type, list.Count);
        }

        public void OnResponse_ReturnNotesList(MemoryStream stream, int uid = 0)
        {
            MSG_ZGC_CROSS_NOTES_LIST pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZGC_CROSS_NOTES_LIST>(stream);
            Log.Write("cross server ReturnNotesList");

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} get arena challenger from relation failed: not find player ", uid);
                return;
            }
            player.Write(pks);
        }
    } 
}
