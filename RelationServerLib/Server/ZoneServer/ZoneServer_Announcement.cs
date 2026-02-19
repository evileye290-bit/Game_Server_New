using Logger;
using Message.Relation.Protocol.RC;
using Message.Relation.Protocol.RZ;
using Message.Zone.Protocol.ZR;
using ServerFrame;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelationServerLib
{
    public partial class ZoneServer
    {      

        public void OnResponse_IntegralBossStart(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_INTEGRALBOSS_START msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_INTEGRALBOSS_START>(stream);

            MSG_RZ_INTEGRALBOSS_START response = new MSG_RZ_INTEGRALBOSS_START();
                   
            ZoneManager.BroadCastCount++;
            if (ZoneManager.BroadCastCount > 1)
            {
                return;
            }
            FrontendServer server = Api.ZoneManager.GetOneServer();
            server.Write(response);
        }

        public void OnResponse_IntegralBossEnd(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_INTEGRALBOSS_END msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_INTEGRALBOSS_END>(stream);
            ZoneManager.BroadCastCount = 0;
        }


        public void OnResponse_CrossBroadcastAnnouncement(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_BROADCAST_ANNOUNCEMENT pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_BROADCAST_ANNOUNCEMENT>(stream);

            MSG_RC_BROADCAST_ANNOUNCEMENT msg = new MSG_RC_BROADCAST_ANNOUNCEMENT();
            msg.Type = pks.Type;
            msg.List.AddRange(pks.List);
            Api.WriteToCross(msg);
        }

        public void OnResponse_CrossNotesList(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_CROSS_NOTES_LIST pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_CROSS_NOTES_LIST>(stream);

            MSG_RC_CROSS_NOTES_LIST msg = new MSG_RC_CROSS_NOTES_LIST();
            msg.Type = pks.Type;
            foreach (var item in pks.List)
            {
                msg.List.Add(GetCrossNotes(item));
            }
            Api.WriteToCross(msg);
        }

        public RC_CROSS_NOTES GetCrossNotes(ZR_CROSS_NOTES info)
        {
            RC_CROSS_NOTES msg = new RC_CROSS_NOTES();
            msg.Time = info.Time;
            msg.List.AddRange(info.List);
            return msg;
        }

        public void OnResponse_GetNotesListByType(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_NOTES_LIST_BY_TYPE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_NOTES_LIST_BY_TYPE>(stream);

            MSG_RC_NOTES_LIST_BY_TYPE msg = new MSG_RC_NOTES_LIST_BY_TYPE();
            msg.Type = pks.Type;
            Api.WriteToCross(msg, uid);
        }

    }
}
