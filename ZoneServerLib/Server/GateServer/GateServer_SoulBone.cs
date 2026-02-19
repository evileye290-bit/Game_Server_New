using Logger;
using Message.Gate.Protocol.GateZ;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public partial class GateServer
    {
        private void OnResponse_SmeltSoulBones(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_SMELT_SOULBONE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_SMELT_SOULBONE>(stream);
            Log.Write("player {0} smelt soulBone", msg.PcUid);
            PlayerChar player = Api.PCManager.FindPc(msg.PcUid);
            if (player != null)
            {
                player.SmeltSoulBone(msg);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(msg.PcUid);
                if (player != null)
                {
                    Log.WarnLine("smelt soulbone fail, player {0} is offline.", msg.PcUid);
                }
                else
                {
                    Log.WarnLine("smelt soulbone fail, can not find player {0} .", msg.PcUid);
                }
            }
        }

        private void OnResponse_EquipSoulBone(MemoryStream stream,int uid = 0)
        {
            MSG_GateZ_EQUIP_SOULBONE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_EQUIP_SOULBONE>(stream);
            Log.Write("player {0} equip soulBone hero {1} uid {2}", msg.PcUid, msg.Hero, msg.Uid);
            PlayerChar player = Api.PCManager.FindPc(msg.PcUid);
            if (player != null)
            {
                player.EquipSoulBone(msg);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(msg.PcUid);
                if (player != null)
                {
                    Log.WarnLine("equip soulbone to hero id {0} fail, player {1} is offline.",msg.Hero, msg.PcUid);
                }
                else
                {
                    Log.WarnLine("equip soulbone fail, can not find player {0} .", msg.PcUid);
                }
            }
        }

        private void OnResponse_SoulBoneQuenching(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_SOULBONE_QUENCHING msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_SOULBONE_QUENCHING>(stream);
            Log.Write("player {0} SoulBoneQuenching  id {1} costId {2}", uid, msg.MainBone, msg.SubBone);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.SoulBoneQuenching(msg);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("equip SoulBoneQuenching fail, player {1} is offline.",uid);
                }
                else
                {
                    Log.WarnLine("equip SoulBoneQuenching fail, can not find player {0} .", uid);
                }
            }
        }
    }
}
