using Logger;
using System.IO;
using Message.Relation.Protocol.RZ;
using Message.Relation.Protocol.RR;
using Message.Zone.Protocol.ZR;
using EnumerateUtility;
using ServerFrame;
using Message.Gate.Protocol.GateC;
using Message.Relation.Protocol.RC;
using System.Collections.Generic;
using CommonUtility;
using ServerModels;

namespace RelationServerLib
{
    public partial class ZoneServer
    {
        public void OnResponse_GetHidderWeaponInfo(MemoryStream stream, int uid = 0)
        {
            //MSG_ZR_GET_HIDDER_WEAPON_VALUE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_GET_HIDDER_WEAPON_VALUE>(stream);
            Log.Write("player {0} GetHidderWeaponInfo.", uid);
            MSG_RC_GET_HIDDER_WEAPON_VALUE msg = new MSG_RC_GET_HIDDER_WEAPON_VALUE();
            Api.WriteToCross(msg, uid);
        }
        public void OnResponse_UpdateHidderWeaponInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_UPDATE_HIDDER_WEAPON_VALUE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_UPDATE_HIDDER_WEAPON_VALUE>(stream);
            Log.Write("player {0} UpdateHidderWeaponInfo.", uid);
            MSG_RC_UPDATE_HIDDER_WEAPON_VALUE msg = new MSG_RC_UPDATE_HIDDER_WEAPON_VALUE();
            msg.RingNum = pks.RingNum;

            msg.BaseInfo.AddRange(GetPlayerBaseInfoItemMsg(uid));

            Api.WriteToCross(msg, uid);
        }

        public void OnResponse_GetSeaTreasureInfo(MemoryStream stream, int uid = 0)
        {
            //MSG_ZR_GET_SEA_TREASURE_VALUE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_GET_SEA_TREASURE_VALUE>(stream);
            Log.Write("player {0} GetSeaTreasureInfo.", uid);
            MSG_RC_GET_SEA_TREASURE_VALUE msg = new MSG_RC_GET_SEA_TREASURE_VALUE();
            Api.WriteToCross(msg, uid);
        }
        public void OnResponse_UpdateSeaTreasureInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_UPDATE_SEA_TREASURE_VALUE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_UPDATE_SEA_TREASURE_VALUE>(stream);
            Log.Write("player {0} UpdateSeaTreasureInfo.", uid);
            MSG_RC_UPDATE_SEA_TREASURE_VALUE msg = new MSG_RC_UPDATE_SEA_TREASURE_VALUE();
            msg.RingNum = pks.RingNum;

            msg.BaseInfo.AddRange(GetPlayerBaseInfoItemMsg(uid)) ;

            Api.WriteToCross(msg, uid);
        }

        public void OnResponse_GetDivineLoveInfo(MemoryStream stream, int uid = 0)
        {
            //MSG_ZR_GET_DIVINE_LOVE_VALUE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_GET_DIVINE_LOVE_VALUE>(stream);
            Log.Write("player {0} GetDivineLoveInfo.", uid);
            MSG_RC_GET_DIVINE_LOVE_VALUE msg = new MSG_RC_GET_DIVINE_LOVE_VALUE();
            Api.WriteToCross(msg, uid);
        }

        public void OnResponse_UpdateDivineLoveInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_UPDATE_DIVINE_LOVE_VALUE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_UPDATE_DIVINE_LOVE_VALUE>(stream);
            Log.Write("player {0} UpdateDivineLoveInfo.", uid);
            MSG_RC_UPDATE_DIVINE_LOVE_VALUE msg = new MSG_RC_UPDATE_DIVINE_LOVE_VALUE();
            msg.HeartNum = pks.HeartNum;

            msg.BaseInfo.AddRange(GetPlayerBaseInfoItemMsg(uid));

            Api.WriteToCross(msg, uid);
        }

        public void OnResponse_GetStoneWallInfo(MemoryStream stream, int uid = 0)
        {
            //MSG_ZR_GET_STONE_WALL_VALUE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_GET_STONE_WALL_VALUE>(stream);
            Log.Write("player {0} GetStoneWallInfo.", uid);
            MSG_RC_GET_STONE_WALL_VALUE msg = new MSG_RC_GET_STONE_WALL_VALUE();
            Api.WriteToCross(msg, uid);
        }

        public void OnResponse_UpdateStoneWallInfo(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_UPDATE_STONE_WALL_VALUE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_UPDATE_STONE_WALL_VALUE>(stream);
            Log.Write("player {0} UpdateStoneWallInfo.", uid);
            MSG_RC_UPDATE_STONE_WALL_VALUE msg = new MSG_RC_UPDATE_STONE_WALL_VALUE();
            msg.HammerNum = pks.HammerNum;

            msg.BaseInfo.AddRange(GetPlayerBaseInfoItemMsg(uid));

            Api.WriteToCross(msg, uid);
        }

        private List<RC_HFPlayerBaseInfoItem> GetPlayerBaseInfoItemMsg(int uid)
        {
            List<RC_HFPlayerBaseInfoItem> list = new List<RC_HFPlayerBaseInfoItem>();
            RedisPlayerInfo baseInfo = Api.RPlayerInfoMng.GetPlayerInfo(uid);
            if (baseInfo != null)
            {
                list.Add(GetHWPlayerBaseInfoMsg(HFPlayerInfo.Uid, uid));
                list.Add(GetHWPlayerBaseInfoMsg(HFPlayerInfo.MainId, baseInfo.GetIntValue(HFPlayerInfo.MainId)));
                list.Add(GetHWPlayerBaseInfoMsg(HFPlayerInfo.Name, baseInfo.GetStringValue(HFPlayerInfo.Name)));
                list.Add(GetHWPlayerBaseInfoMsg(HFPlayerInfo.HeroId, baseInfo.GetIntValue(HFPlayerInfo.HeroId)));
                list.Add(GetHWPlayerBaseInfoMsg(HFPlayerInfo.GodType, baseInfo.GetIntValue(HFPlayerInfo.GodType)));
                list.Add(GetHWPlayerBaseInfoMsg(HFPlayerInfo.Icon, baseInfo.GetIntValue(HFPlayerInfo.Icon)));
                list.Add(GetHWPlayerBaseInfoMsg(HFPlayerInfo.BattlePower, baseInfo.GetIntValue(HFPlayerInfo.BattlePower)));
                //foreach (var kv in baseInfo.DataList)
                //{
                //    RC_HFPlayerBaseInfoItem item = GetHWPlayerBaseInfoMsg(kv.Key, kv.Value);
                //    msg.BaseInfo.Add(GetHWPlayerBaseInfoMsg(kv.Key, kv.Value));
                //}
            }
            else
            {
                Log.Warn("player {0} GetPlayerBaseInfoItemMsg not find base info .", uid);
            }
            return list;
        }

        private static RC_HFPlayerBaseInfoItem GetHWPlayerBaseInfoMsg(HFPlayerInfo key, object value)
        {
            RC_HFPlayerBaseInfoItem item = new RC_HFPlayerBaseInfoItem();
            item.Key = (int)key;
            item.Value = value.ToString();
            return item;
        }

    }
}
