using System.IO;
using Logger;
using Message.Gate.Protocol.GateZ;

namespace ZoneServerLib
{
    public partial class GateServer
    {
        private void OnResponse_HiddenWeaponEquip(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_HIDENWEAPON_EQUIP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_HIDENWEAPON_EQUIP>(stream);
            Log.Write("player {0} equip HiddenWeaponEquip uid {1} hero {2}", uid, msg.Uid, msg.HeroId);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.EquipHiddenWeapon(msg.Uid, msg.HeroId);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("HiddenWeaponEquip fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("HiddenWeaponEquip, can not find player {0} .", uid);
                }
            }
        }

        private void OnResponse_HiddenWeaponUpgrade(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_HIDENWEAPON_UPGRADE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_HIDENWEAPON_UPGRADE>(stream);
            Log.Write("player {0} equip HiddenWeaponUpgrade hero {1}", uid, msg.HeroId);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.UpgradeHiddenWeapon(msg.HeroId);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("HiddenWeaponUpgrade fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("HiddenWeaponUpgrade, can not find player {0} .", uid);
                }
            }
        }

        private void OnResponse_HiddenWeaponStar(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_HIDENWEAPON_STAR msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_HIDENWEAPON_STAR>(stream);
            Log.Write("player {0} equip HiddenWeaponEquip hero {1}", uid, msg.HeroId);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.HiddenWeaponStar(msg.HeroId);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("HiddenWeaponStar fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("HiddenWeaponStar, can not find player {0} .", uid);
                }
            }
        }

        private void OnResponse_HiddenWeaponWash(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_HIDENWEAPON_WASH msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_HIDENWEAPON_WASH>(stream);
            Log.Write("player {0} HiddenWeaponWash  hero {1}", uid, msg.HeroId);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.HiddenWeaponWash(msg);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("HiddenWeaponWash fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("HiddenWeaponWash, can not find player {0} .", uid);
                }
            }
        }

        private void OnResponse_HiddenWeaponWashConfirm(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_HIDENWEAPON_WASH_CONFIRM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_HIDENWEAPON_WASH_CONFIRM>(stream);
            Log.Write("player {0} HiddenWeaponWashConfirm hero {1}", uid, msg.HeroId);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.HiddenWeaponWashConfirm(msg.HeroId, msg.Index);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("HiddenWeaponWashConfirm fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("HiddenWeaponWashConfirm, can not find player {0} .", uid);
                }
            }
        }

        private void OnResponse_HiddenWeaponSmash(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_HIDENWEAPON_SMASH msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_HIDENWEAPON_SMASH>(stream);
            Log.Write("player {0} HiddenWeaponSmash ", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.HiddenWeaponSmash(msg.SmashList);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("HiddenWeaponSmash fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("HiddenWeaponSmash, can not find player {0} .", uid);
                }
            }
        }

    }
}
