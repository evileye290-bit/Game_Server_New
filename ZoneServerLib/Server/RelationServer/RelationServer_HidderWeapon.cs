using Message.Relation.Protocol.RZ;
using EnumerateUtility;
using System.IO;
using DBUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using System;
using Message.Gate.Protocol.GateZ;
using System.Collections.Generic;
using CommonUtility;
using Google.Protobuf.Collections;
using ServerModels;
using ServerShared;

namespace ZoneServerLib
{
    public partial class RelationServer
    {
        public void OnResponse_GetHidderWeaponInfo(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_GET_HIDDER_WEAPON_VALUE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_GET_HIDDER_WEAPON_VALUE>(stream);
            Log.Write($"player {uid} GetHidderWeaponInfo from main {MainId} ");

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn($"player {uid} GetHidderWeaponInfo from main {MainId} not find player ");
                return;
            }
   

            MSG_ZGC_HIDDER_WEAPON_VALUE msg = new MSG_ZGC_HIDDER_WEAPON_VALUE();
            msg.TotalValue = pks.TotalValue;
            msg.Value = pks.Value;
            msg.HiddenWeaponNum = player.CrossBossInfoMng.CounterInfo.HiddenWeaponRingNum;
            msg.HiddenWeaponNumRewards.AddRange(player.CrossBossInfoMng.CounterInfo.HiddenWeaponNumRewards);

            if (pks.BaseInfo.Count > 0)
            {
                Dictionary<HFPlayerInfo, object> dataList = new Dictionary<HFPlayerInfo, object>();
                foreach (var item in pks.BaseInfo)
                {
                    dataList[(HFPlayerInfo)item.Key] = item.Value;
                }
                RedisPlayerInfo rInfo = new RedisPlayerInfo(dataList);

                HIDDER_WEAPON_FIRST_INFO info = new HIDDER_WEAPON_FIRST_INFO();
                info.Uid = rInfo.GetIntValue(HFPlayerInfo.Uid);
                info.Name = rInfo.GetStringValue(HFPlayerInfo.Name);
                info.MainId = rInfo.GetIntValue(HFPlayerInfo.MainId);
                info.HeroId = rInfo.GetIntValue(HFPlayerInfo.HeroId);
                info.GodType = rInfo.GetIntValue(HFPlayerInfo.GodType);
                info.Value = pks.FirstValue;
                msg.FirstInfo = info;
            }
            else
            {
                HIDDER_WEAPON_FIRST_INFO info = new HIDDER_WEAPON_FIRST_INFO();
                info.Uid = player.Uid;
                info.Name = player.Name;
                info.MainId = MainId;
                info.HeroId = player.HeroId;

                info.Value = pks.Value;
                msg.FirstInfo = info;
            }
            player.Write(msg);
        }


        public void OnResponse_GetSeaTreasureInfo(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_GET_SEA_TREASURE_VALUE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_GET_SEA_TREASURE_VALUE>(stream);
            Log.Write($"player {uid} GetHidderWeaponInfo from main {MainId} ");

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn($"player {uid} GetHidderWeaponInfo from main {MainId} not find player ");
                return;
            }


            MSG_ZGC_SEA_TREASURE_VALUE msg = new MSG_ZGC_SEA_TREASURE_VALUE();
            msg.TotalValue = pks.TotalValue;
            msg.Value = pks.Value;
            msg.BlessingNum = player.CrossBossInfoMng.CounterInfo.BlessingNum;
            msg.BlessingMultiple = player.CrossBossInfoMng.CounterInfo.BlessingMultiple;
            msg.ItemList.AddRange(player.CrossBossInfoMng.CounterInfo.ItemList);
            msg.SeaTreasureNum = player.CrossBossInfoMng.CounterInfo.SeaTreasureNum;
            msg.SeaTreasureNumRewards.AddRange(player.CrossBossInfoMng.CounterInfo.SeaTreasureNumRewards);

            if (pks.BaseInfo.Count > 0)
            {
                Dictionary<HFPlayerInfo, object> dataList = new Dictionary<HFPlayerInfo, object>();
                foreach (var item in pks.BaseInfo)
                {
                    dataList[(HFPlayerInfo)item.Key] = item.Value;
                }
                RedisPlayerInfo rInfo = new RedisPlayerInfo(dataList);

                SEA_TREASURE_FIRST_INFO info = new SEA_TREASURE_FIRST_INFO();
                info.Uid = rInfo.GetIntValue(HFPlayerInfo.Uid);
                info.Name = rInfo.GetStringValue(HFPlayerInfo.Name);
                info.MainId = rInfo.GetIntValue(HFPlayerInfo.MainId);
                info.HeroId = rInfo.GetIntValue(HFPlayerInfo.HeroId);
                info.GodType = rInfo.GetIntValue(HFPlayerInfo.GodType);
                info.Value = pks.FirstValue;
                msg.FirstInfo = info;
            }
            else
            {
                SEA_TREASURE_FIRST_INFO info = new SEA_TREASURE_FIRST_INFO();
                info.Uid = player.Uid;
                info.Name = player.Name;
                info.MainId = MainId;
                info.HeroId = player.HeroId;

                info.Value = pks.Value;
                msg.FirstInfo = info;
            }



            player.Write(msg);
        }


        public void OnResponse_GetDivineLoveInfo(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_GET_DIVINE_LOVE_VALUE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_GET_DIVINE_LOVE_VALUE>(stream);
            Log.Write($"player {uid} GetDivineLoveInfo from main {MainId} ");

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn($"player {uid} GetDivineLoveInfo from main {MainId} not find player ");
                return;
            }

            MSG_ZGC_DIVINE_LOVE_VALUE msg = new MSG_ZGC_DIVINE_LOVE_VALUE();
            msg.TotalValue = pks.TotalValue;
            msg.Value = pks.Value;
            DivineLoveInfo divineLoveInfo = player.DivineLoveMng.GetDivineLoveInfo((int)ActivityPlayType.High);
            if (divineLoveInfo != null)
            {
                msg.HeartNum = divineLoveInfo.HeartNum;
                msg.CumulateRewards.AddRange(divineLoveInfo.CumulateRewardList);
            }                           

            if (pks.BaseInfo.Count > 0)
            {
                Dictionary<HFPlayerInfo, object> dataList = new Dictionary<HFPlayerInfo, object>();
                foreach (var item in pks.BaseInfo)
                {
                    dataList[(HFPlayerInfo)item.Key] = item.Value;
                }
                RedisPlayerInfo rInfo = new RedisPlayerInfo(dataList);

                DIVINE_LOVE_FIRST_INFO info = new DIVINE_LOVE_FIRST_INFO();
                info.Uid = rInfo.GetIntValue(HFPlayerInfo.Uid);
                info.Name = rInfo.GetStringValue(HFPlayerInfo.Name);
                info.MainId = rInfo.GetIntValue(HFPlayerInfo.MainId);
                info.HeroId = rInfo.GetIntValue(HFPlayerInfo.HeroId);
                info.GodType = rInfo.GetIntValue(HFPlayerInfo.GodType);
                info.Value = pks.FirstValue;
                msg.FirstInfo = info;
            }
            else
            {
                DIVINE_LOVE_FIRST_INFO info = new DIVINE_LOVE_FIRST_INFO();
                info.Uid = player.Uid;
                info.Name = player.Name;
                info.MainId = MainId;
                info.HeroId = player.HeroId;

                info.Value = pks.Value;
                msg.FirstInfo = info;
            }
            player.Write(msg);
        }

        public void OnResponse_GetStoneWallInfo(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_GET_STONE_WALL_VALUE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_GET_STONE_WALL_VALUE>(stream);
            Log.Write($"player {uid} GetStoneWallInfo from main {MainId} ");

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn($"player {uid} GetStoneWallInfo from main {MainId} not find player ");
                return;
            }

            MSG_ZGC_STONE_WALL_VALUE msg = new MSG_ZGC_STONE_WALL_VALUE();
            msg.TotalValue = pks.TotalValue;
            msg.Value = pks.Value;
            StoneWallInfo stoneWallInfo = player.StoneWallMng.GetStoneWallInfo((int)ActivityPlayType.High);
            if (stoneWallInfo != null)
            {
                msg.HammerNum = stoneWallInfo.HammerNum;         
            }

            if (pks.BaseInfo.Count > 0)
            {
                Dictionary<HFPlayerInfo, object> dataList = new Dictionary<HFPlayerInfo, object>();
                foreach (var item in pks.BaseInfo)
                {
                    dataList[(HFPlayerInfo)item.Key] = item.Value;
                }
                RedisPlayerInfo rInfo = new RedisPlayerInfo(dataList);

                STONE_WALL_FIRST_INFO info = new STONE_WALL_FIRST_INFO();
                info.Uid = rInfo.GetIntValue(HFPlayerInfo.Uid);
                info.Name = rInfo.GetStringValue(HFPlayerInfo.Name);
                info.MainId = rInfo.GetIntValue(HFPlayerInfo.MainId);
                info.HeroId = rInfo.GetIntValue(HFPlayerInfo.HeroId);
                info.GodType = rInfo.GetIntValue(HFPlayerInfo.GodType);
                info.Value = pks.FirstValue;
                msg.FirstInfo = info;
            }
            else
            {
                STONE_WALL_FIRST_INFO info = new STONE_WALL_FIRST_INFO();
                info.Uid = player.Uid;
                info.Name = player.Name;
                info.MainId = MainId;
                info.HeroId = player.HeroId;

                info.Value = pks.Value;
                msg.FirstInfo = info;
            }
            player.Write(msg);
        }

        public void OnResponse_ClearValue(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_CLEAR_VALUE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_CLEAR_VALUE>(stream);
            Log.Write($"cross ClearValue from main {MainId} ");

            RechargeGiftType type = (RechargeGiftType)pks.GiftType;

            foreach (var pc in Api.PCManager.PcList)
            {
                pc.Value.ClearCrossActivityValue(type);
            }
            foreach (var pc in Api.PCManager.PcOfflineList)
            {
                pc.Value.ClearCrossActivityValue(type);
            }
        }
    }
}
