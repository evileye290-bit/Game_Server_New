using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using Logger;
using EnumerateUtility;

namespace ZoneServerLib
{
    public partial class GateServer
    {
        public void OnResponse_GetHidderWeaponInfo(MemoryStream stream, int uid = 0)
        {
            //MSG_GateZ_GET_HIDDER_WEAPON_VALUE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_HIDDER_WEAPON_VALUE>(stream);
            Log.Write("player {0} GetHidderWeaponInfo", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GetHidderWeaponInfo not in gateid {1} pc list", uid, SubId);
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} GetHidderWeaponInfo not in map ", uid);
                return;
            }

            player.GetHidderWeaponInfo();           
        }

        public void OnResponse_GetHidderWeaponReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_HIDDER_WEAPON_REWARD pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_HIDDER_WEAPON_REWARD>(stream);
            Log.Write("player {0} GetHidderWeaponReward id {1} {2} {3} num {4}", uid, pks.Id, pks.IsConsecutive, pks.UseDiamond, pks.RingNum);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GetHidderWeaponReward not in gateid {1} pc list", uid, SubId);
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} GetHidderWeaponReward not in map ", uid);
                return;
            }

            player.GetHidderWeaponReward(pks.Id, pks.IsConsecutive, pks.UseDiamond, pks.RingNum);
        }

        public void OnResponse_GetHidderWeaponNumReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_HIDDER_WEAPON_NUM_REWARD pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_HIDDER_WEAPON_NUM_REWARD>(stream);
            Log.Write("player {0} GetHidderWeaponNumReward id {1} RewardNum {2}", uid, pks.Id, pks.RewardNum);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GetHidderWeaponNumReward not in gateid {1} pc list", uid, SubId);
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} GetHidderWeaponNumReward not in map ", uid);
                return;
            }

            player.GetHidderWeaponNumReward(pks.Id, pks.RewardNum);
        }

        public void OnResponse_BuyHidderWeaponItem(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_BUY_HIDDER_WEAPON_ITEM pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_BUY_HIDDER_WEAPON_ITEM>(stream);
            Log.Write("player {0} BuyHidderWeaponItem id {1} num {2}", uid, pks.Id, pks.Num);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} BuyHidderWeaponItem not in gateid {1} pc list", uid, SubId);
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} BuyHidderWeaponItem not in map ", uid);
                return;
            }

            player.BuyHidderWeaponItem(pks.Id, pks.Num);
        }


        public void OnResponse_GetSeaTreasureInfo(MemoryStream stream, int uid = 0)
        {
            //MSG_GateZ_GET_SEA_TREASURE_VALUE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_SEA_TREASURE_VALUE>(stream);
            Log.Write("player {0} GetSeaTreasureInfo", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GetSeaTreasureInfo not in gateid {1} pc list", uid, SubId);
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} GetSeaTreasureInfo not in map ", uid);
                return;
            }

            player.GetSeaTreasureInfo();
        }

        public void OnResponse_GetSeaTreasureReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_SEA_TREASURE_REWARD pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_SEA_TREASURE_REWARD>(stream);
            Log.Write("player {0} GetHidderWeaponReward id {1} {2}", uid, pks.Id, pks.UseDiamond);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GetSeaTreasureReward not in gateid {1} pc list", uid, SubId);
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} GetSeaTreasureReward not in map ", uid);
                return;
            }

            player.GetSeaTreasureReward(pks.Id, pks.IsConsecutive, pks.UseDiamond);
        }

        public void OnResponse_BuySeaTreasureItem(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_BUY_SEA_TREASURE_ITEM pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_BUY_SEA_TREASURE_ITEM>(stream);
            Log.Write("player {0} BuyHidderWeaponItem id {1} num {2}", uid, pks.Id, pks.Num);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} BuySeaTreasureItem not in gateid {1} pc list", uid, SubId);
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} BuySeaTreasureItem not in map ", uid);
                return;
            }

            player.BuySeaTreasureItem(pks.Id, pks.Num);
        }

        public void OnResponse_GetNotesListByType(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_NOTES_LIST_BY_TYPE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_NOTES_LIST_BY_TYPE>(stream);
            Log.Write("player {0} GetNotesListByType type {1}", uid, pks.Type);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GetNotesListByType not in gateid {1} pc list", uid, SubId);
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} GetNotesListByType not in map ", uid);
                return;
            }

            player.GetNotesListByType(pks.Type);
        }

        public void OnResponse_GetSeaTreasureBlessing(MemoryStream stream, int uid = 0)
        {
            //MSG_GateZ_GET_SEA_TREASURE_BLESSING pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_SEA_TREASURE_BLESSING>(stream);
            Log.Write("player {0} GetSeaTreasureBlessing", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GetSeaTreasureBlessing not in gateid {1} pc list", uid, SubId);
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} GetSeaTreasureBlessing not in map ", uid);
                return;
            }

            player.GetSeaTreasureInfo();
        }

        public void OnResponse_CloseSeaTreasureBlessing(MemoryStream stream, int uid = 0)
        {
            //MSG_GateZ_CLOSE_SEA_TREASURE_BLESSING pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_CLOSE_SEA_TREASURE_BLESSING>(stream);
            Log.Write("player {0} CloseSeaTreasureBlessing", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} CloseSeaTreasureBlessing not in gateid {1} pc list", uid, SubId);
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} CloseSeaTreasureBlessing not in map ", uid);
                return;
            }

            player.CloseSeaTreasureBlessing();
        }

        public void OnResponse_GetSeaTreasureNumReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_SEA_TREASURE_NUM_REWARD pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_SEA_TREASURE_NUM_REWARD>(stream);
            Log.Write("player {0} GetSeaTreasureNumReward id {1} num {2}", uid, pks.Id, pks.RewardNum);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GetSeaTreasureNumReward not in gateid {1} pc list", uid, SubId);
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} GetSeaTreasureNumReward not in map ", uid);
                return;
            }

            player.GetSeaTreasureNumReward(pks.Id, pks.RewardNum);
        }
    }
}
