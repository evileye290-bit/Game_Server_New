using System.IO;
using CommonUtility;
using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateZ;

namespace GateServerLib
{
    public partial class Client
    {
        private void OnResponse_HiddenWeaponEquip(MemoryStream stream)
        {
            MSG_CG_HIDENWEAPON_EQUIP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_HIDENWEAPON_EQUIP>(stream);
            MSG_GateZ_HIDENWEAPON_EQUIP request = new MSG_GateZ_HIDENWEAPON_EQUIP();
            request.Uid = ExtendClass.GetUInt64(msg.UidHigh, msg.UidLow);
            request.HeroId = msg.HeroId;
            WriteToZone(request);
        }

        private void OnResponse_HiddenWeaponUpgrade(MemoryStream stream)
        {
            MSG_CG_HIDENWEAPON_UPGRADE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_HIDENWEAPON_UPGRADE>(stream);
            MSG_GateZ_HIDENWEAPON_UPGRADE request = new MSG_GateZ_HIDENWEAPON_UPGRADE();
            request.HeroId = msg.HeroId;
            WriteToZone(request);
        }

        private void OnResponse_HiddenWeaponStar(MemoryStream stream)
        {
            MSG_CG_HIDENWEAPON_STAR msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_HIDENWEAPON_STAR>(stream);
            MSG_GateZ_HIDENWEAPON_STAR request = new MSG_GateZ_HIDENWEAPON_STAR();
            request.HeroId = msg.HeroId;
            WriteToZone(request);
        }

        private void OnResponse_HiddenWeaponWash(MemoryStream stream)
        {
            MSG_CG_HIDENWEAPON_WASH msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_HIDENWEAPON_WASH>(stream);
            MSG_GateZ_HIDENWEAPON_WASH request = new MSG_GateZ_HIDENWEAPON_WASH()
            {
                HeroId = msg.HeroId,
                WashCount = msg.WashCount
            };
            request.LockIndex.Add(msg.LockIndex);
            WriteToZone(request);
        }

        private void OnResponse_HiddenWeaponWashConfirm(MemoryStream stream)
        {
            MSG_CG_HIDENWEAPON_WASH_CONFIRM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_HIDENWEAPON_WASH_CONFIRM>(stream);
            MSG_GateZ_HIDENWEAPON_WASH_CONFIRM request = new MSG_GateZ_HIDENWEAPON_WASH_CONFIRM()
            {
                HeroId = msg.HeroId,
                Index = msg.Index
            };
            WriteToZone(request);
        }

        private void OnResponse_HiddenWeaponSmash(MemoryStream stream)
        {
            MSG_CG_HIDENWEAPON_SMASH msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_HIDENWEAPON_SMASH>(stream);
            MSG_GateZ_HIDENWEAPON_SMASH request = new MSG_GateZ_HIDENWEAPON_SMASH();
            msg.SmashList.ForEach(x=>request.SmashList.Add(ExtendClass.GetUInt64(x.UidHigh, x.UidLow)));
            WriteToZone(request);
        }
    }
}
