using Logger;
using Message.Gate.Protocol.GateC;
using Message.IdGenerator;
using Message.Zone.Protocol.ZGate;
using System.IO;

namespace GateServerLib
{
    public partial class ZoneServer
    {
        public void OnResponse_FieldObjectMove(MemoryStream stream, int uid)
        {
            //MSG_GC_FieldObject_MOVE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GC_FieldObject_MOVE>(stream);
            int pcUid = uid;
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_GC_FieldObject_MOVE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} not find client(FieldObject_MOVE)", pcUid);
            }
        }

        public void OnResponse_CharacterEnterList(MemoryStream stream, int uid)
        {
            //MSG_GC_CHARACTER_ENTER_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GC_CHARACTER_ENTER_LIST>(stream);
            int pcUid = uid;
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_GC_CHARACTER_ENTER_LIST>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} not find client(OnResponse_CharacterEnterList)", pcUid);
            }
        }

        public void OnResponse_HeroEnterList(MemoryStream stream,int uid)
        {
            int pcUid = uid;
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_GC_HERO_ENTER_LIST>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} not find client(OnResponse_HeroEnterList)", pcUid);
            }
        }

        public void OnResponse_MonsterList(MemoryStream stream, int uid)
        {
            int pcUid = uid;
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_GC_MONSTER_ENTER_LIST>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} not find client(OnResponse_MonsterList)", pcUid);
            }
        }

        public void OnResponse_BroadcastList(MemoryStream stream, int uid)
        {
            //MSG_GC_BROADCAST_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GC_BROADCAST_LIST>(stream);
            int pcUid = uid;
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_GC_BROADCAST_LIST>.Value, stream);
            }
            //else
            //{
            //    Log.WarnLine(" player {0} not find client(OnResponse_BroadcastList)", pcUid);
            //}
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="uid"></param>
        public void OnResponse_InstancesRemove(MemoryStream stream, int uid)
        {
            //MSG_GC_INSTANCES_REMOVE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GC_INSTANCES_REMOVE>(stream);
            int pcUid = uid;
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_GC_INSTANCES_REMOVE>.Value, stream);
            }
            //else
            //{
            //    Log.WarnLine(" player {0} not find client(OnResponse_InstancesRemove)", pcUid);
            //}
        }

        public void OnResponse_Interaction(MemoryStream stream, int uid)
        {
            //MSG_ZGC_INTERACTION msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZGC_INTERACTION>(stream);
            int pcUid = uid;
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_INTERACTION>.Value, stream);
            }
            else
            {
                Log.WarnLine(" player {0} not find client(OnResponse_Interaction)", pcUid);
            }
        }

        public void OnResponse_CharacterStop(MemoryStream stream, int uid)
        {
            //MSG_GC_CHARACTER_STOP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GC_CHARACTER_STOP>(stream);
            int pcUid = uid;
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CHARACTER_STOP>.Value, stream);
            }
            else
            {
                Log.WarnLine(" player {0} mot find client(OnResponse_CharacterStop)", pcUid);
            }
        }

        public void OnResponse_NpcMove(MemoryStream stream, int uid)
        {
            //MSG_ZGC_NPC_MOVE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZGC_NPC_MOVE>(stream);
            int pcUid = uid;
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_NPC_MOVE>.Value, stream);
            }
            else
            {
                Log.WarnLine(" player {0} mot find client(OnResponse_NpcMove)", pcUid);
            }
        }

        private void OnResponse_NPCEnterList(MemoryStream stream, int uid)
        {
            Client client = Api.ClientMng.FindClientByUid(uid);
            if (client != null)
            {
                client.Write(Id<MSG_GC_NPC_ENTER_LIST>.Value, stream);
            }
        }

        private void OnResponse_WeaponInfo(MemoryStream stream, int uid)
        {
            Client client = Api.ClientMng.FindClientByUid(uid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_HIDDEN_WEAPON_INFO>.Value, stream);
            }
        }

        private void OnResponse_HeroWeaponInfo(MemoryStream stream, int uid)
        {
            Client client = Api.ClientMng.FindClientByUid(uid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_HERO_HIDDEN_WEAPON_INFO>.Value, stream);
            }
        }

        public void OnResponse_PetEnterList(MemoryStream stream, int uid)
        {
            int pcUid = uid;
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_GC_PET_ENTER_LIST>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} not find client(OnResponse_PetEnterList)", pcUid);
            }
        }
    }
}
