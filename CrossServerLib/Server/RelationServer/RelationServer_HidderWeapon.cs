using EnumerateUtility;
using Logger;
using Message.Corss.Protocol.CorssR;
using Message.Relation.Protocol.RC;
using ServerModels;
using ServerShared;
using System.Collections.Generic;
using System.IO;

namespace CrossServerLib
{
    public partial class RelationServer
    {
        //获取决赛玩家信息
        public void OnResponse_GetHidderWeaponInfo(MemoryStream stream, int uid = 0)
        {
            //MSG_RC_GET_HIDDER_WEAPON_VALUE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RC_GET_HIDDER_WEAPON_VALUE>(stream);
            Log.Write($"player {uid} GetHidderWeaponInfo from main {MainId} ");
            int groupId = CrossBattleLibrary.GetGroupId(MainId);
            if (groupId == 0)
            {
                Log.Warn($"player {uid} GetHidderWeaponInfo from main {MainId} not find group id ");
                return;
            }
            SendHidderWeaponInfo(uid, groupId);
        }

        public void OnResponse_UpdateHidderWeaponInfo(MemoryStream stream, int uid = 0)
        {
            MSG_RC_UPDATE_HIDDER_WEAPON_VALUE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RC_UPDATE_HIDDER_WEAPON_VALUE>(stream);
            Log.Write($"player {uid} UpdateHidderWeaponInfo from main {MainId} ");
            int groupId = CrossBattleLibrary.GetGroupId(MainId);
            if (groupId == 0)
            {
                Log.Warn($"player {uid} UpdateHidderWeaponInfo from main {MainId} not find group id ");
                return;
            }

            if (pks.RingNum > 0)
            {
                JsonPlayerInfo playerInfo = Api.PlayerInfoMng.GetJsonPlayerInfo(groupId, uid);
                if (playerInfo == null)
                {
                    Dictionary<HFPlayerInfo, object> dataList = new Dictionary<HFPlayerInfo, object>();
                    foreach (var item in pks.BaseInfo)
                    {
                        HFPlayerInfo key = (HFPlayerInfo)item.Key;
                        switch (key)
                        {
                            case HFPlayerInfo.MainId:
                                dataList[key] = MainId;
                                break;
                            default:
                                dataList[key] = item.Value;
                                break;
                        }
                    }
                    //添加信息
                    playerInfo = new JsonPlayerInfo(dataList);
                    Api.PlayerInfoMng.AddPlayerInfo(groupId, uid, playerInfo);
                    //Api.CrossRedis.Call(new OperateAddCrossPlayerBaseInfo(uid, playerInfo));
                }
                else
                {
                    foreach (var item in pks.BaseInfo)
                    {
                        if (item.Key == (int)HFPlayerInfo.HeroId)
                        {
                            int heroId = int.Parse(item.Value);
                            if (heroId > 0 && playerInfo.HeroId != heroId)
                            {
                                playerInfo.HeroId = heroId;
                                playerInfo.Icon = heroId;
                                Api.PlayerInfoMng.AddPlayerInfo(groupId, uid, playerInfo);
                            }
                            break;
                        }
                    }
                }

                //检查结算时间
                RechargeGiftModel model;
                if (RechargeLibrary.CheckInRechargeGiftTime(RechargeGiftType.HiddenWeapon, Api.Now(), out model))
                {
                    //需要更新数据
                    Api.HidderWeaponMng.UpdatePlayerValue(groupId, uid, pks.RingNum);
                }
                else
                {
                    Log.Warn($"player {uid} UpdateHidderWeaponInfo failed: time is error, type {RechargeGiftType.HiddenWeapon} value {pks.RingNum} ");
                }
            }

            SendHidderWeaponInfo(uid, groupId);
        }

        private void SendHidderWeaponInfo(int uid, int groupId)
        {
            //获取当前总值值
            int totalValue = Api.HidderWeaponMng.GetCurrentValue(groupId);

            int firstValue = 0;
            //获取第一名暗器值
            RankBaseModel firstModel = Api.HidderWeaponMng.GetFirstValue(groupId);
            if (firstModel != null)
            {
                firstValue = firstModel.Score;
            }
            int playerValue = 0;
            //获取自己暗器值
            RankBaseModel playerModel = Api.HidderWeaponMng.GetPlayerValue(groupId, uid);
            if (playerModel != null)
            {
                playerValue = playerModel.Score;
            }
            MSG_CorssR_GET_HIDDER_WEAPON_VALUE msg = new MSG_CorssR_GET_HIDDER_WEAPON_VALUE();

            msg.Uid = uid;
            msg.Value = playerValue;
            msg.TotalValue = totalValue;
            msg.FirstValue = firstValue;
            if (firstModel != null)
            {
                msg.BaseInfo.AddRange(GetPlayerBaseInfoItemMsg(groupId, firstModel.Uid));
            }

            //JsonPlayerInfo fitstPlayerInfo = Api.PlayerInfoMng.GetJsonPlayerInfo(groupId, firstModel.Uid);
            //if (fitstPlayerInfo != null)
            //{
            //    msg.BaseInfo.Add(GetPlayerInfoMsg(HFPlayerInfo.Uid, fitstPlayerInfo.Uid.ToString()));
            //    msg.BaseInfo.Add(GetPlayerInfoMsg(HFPlayerInfo.MainId, fitstPlayerInfo.MainId.ToString()));
            //    msg.BaseInfo.Add(GetPlayerInfoMsg(HFPlayerInfo.Name, fitstPlayerInfo.Name));
            //    msg.BaseInfo.Add(GetPlayerInfoMsg(HFPlayerInfo.HeroId, fitstPlayerInfo.HeroId.ToString()));
            //    msg.BaseInfo.Add(GetPlayerInfoMsg(HFPlayerInfo.GodType, fitstPlayerInfo.GodType.ToString()));
            //    msg.BaseInfo.Add(GetPlayerInfoMsg(HFPlayerInfo.Icon, fitstPlayerInfo.Icon.ToString()));
            //    msg.BaseInfo.Add(GetPlayerInfoMsg(HFPlayerInfo.BattlePower, fitstPlayerInfo.BattlePower.ToString()));
            //}

            Write(msg, uid);
        }

        private List<CorssR_HFPlayerBaseInfoItem> GetPlayerBaseInfoItemMsg(int groupId, int uid)
        {
            List<CorssR_HFPlayerBaseInfoItem> list = new List<CorssR_HFPlayerBaseInfoItem>();
            JsonPlayerInfo baseInfo = Api.PlayerInfoMng.GetJsonPlayerInfo(groupId, uid);
            if (baseInfo != null)
            {
                list.Add(GetPlayerInfoMsg(HFPlayerInfo.Uid, baseInfo.Uid.ToString()));
                list.Add(GetPlayerInfoMsg(HFPlayerInfo.MainId, baseInfo.MainId.ToString()));
                list.Add(GetPlayerInfoMsg(HFPlayerInfo.Name, baseInfo.Name));
                list.Add(GetPlayerInfoMsg(HFPlayerInfo.HeroId, baseInfo.HeroId.ToString()));
                list.Add(GetPlayerInfoMsg(HFPlayerInfo.GodType, baseInfo.GodType.ToString()));
                list.Add(GetPlayerInfoMsg(HFPlayerInfo.Icon, baseInfo.Icon.ToString()));
                list.Add(GetPlayerInfoMsg(HFPlayerInfo.BattlePower, baseInfo.BattlePower.ToString()));
            }
            else
            {
                Log.Warn("player {0} GetPlayerBaseInfoItemMsg not find base info .", uid);
            }
            return list;
        }



        public void OnResponse_GetSeaTreasureInfo(MemoryStream stream, int uid = 0)
        {
            //MSG_RC_GET_SEA_TREASURE_VALUE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RC_GET_SEA_TREASURE_VALUE>(stream);
            Log.Write($"player {uid} GetSeaTreasureInfo from main {MainId} ");
            int groupId = CrossBattleLibrary.GetGroupId(MainId);
            if (groupId == 0)
            {
                Log.Warn($"player {uid} GetSeaTreasureInfo from main {MainId} not find group id ");
                return;
            }
            SendSeaTreasureInfo(uid, groupId);
        }

        public void OnResponse_UpdateSeaTreasureInfo(MemoryStream stream, int uid = 0)
        {
            MSG_RC_UPDATE_SEA_TREASURE_VALUE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RC_UPDATE_SEA_TREASURE_VALUE>(stream);
            Log.Write($"player {uid} UpdateSeaTreasureInfo from main {MainId} ");
            int groupId = CrossBattleLibrary.GetGroupId(MainId);
            if (groupId == 0)
            {
                Log.Warn($"player {uid} UpdateSeaTreasureInfo from main {MainId} not find group id ");
                return;
            }

            if (pks.RingNum > 0)
            {
                JsonPlayerInfo playerInfo = Api.PlayerInfoMng.GetJsonPlayerInfo(groupId, uid);
                if (playerInfo == null)
                {
                    Dictionary<HFPlayerInfo, object> dataList = new Dictionary<HFPlayerInfo, object>();
                    foreach (var item in pks.BaseInfo)
                    {
                        HFPlayerInfo key = (HFPlayerInfo)item.Key;
                        switch (key)
                        {
                            case HFPlayerInfo.MainId:
                                dataList[key] = MainId;
                                break;
                            default:
                                dataList[key] = item.Value;
                                break;
                        }
                    }
                    //添加信息
                    playerInfo = new JsonPlayerInfo(dataList);
                    Api.PlayerInfoMng.AddPlayerInfo(groupId, uid, playerInfo);
                    //Api.CrossRedis.Call(new OperateAddCrossPlayerBaseInfo(uid, playerInfo));
                }
                else
                {
                    foreach (var item in pks.BaseInfo)
                    {
                        if (item.Key == (int)HFPlayerInfo.HeroId)
                        {
                            int heroId = int.Parse(item.Value);
                            if (heroId > 0 && playerInfo.HeroId != heroId)
                            {
                                playerInfo.HeroId = heroId;
                                playerInfo.Icon = heroId;
                                Api.PlayerInfoMng.AddPlayerInfo(groupId, uid, playerInfo);
                            }
                            break;
                        }
                    }
                }

                //检查结算时间
                RechargeGiftModel model;
                if (RechargeLibrary.CheckInRechargeGiftTime(RechargeGiftType.SeaTreasure, Api.Now(), out model))
                {
                    //需要更新数据
                    Api.SeaTreasureMng.UpdatePlayerValue(groupId, uid, pks.RingNum);
                }
                else
                {
                    Log.Warn($"player {uid} UpdateSeaTreasureInfo failed: time is error, type {RechargeGiftType.SeaTreasure} value {pks.RingNum} ");
                }
     
            }

            SendSeaTreasureInfo(uid, groupId);
        }

        private void SendSeaTreasureInfo(int uid, int groupId)
        {
            //获取当前总值值
            int totalValue = Api.SeaTreasureMng.GetCurrentValue(groupId);

            //JsonPlayerInfo fitstPlayerInfo = null;
            int firstValue = 0;
            //获取第一名暗器值
            RankBaseModel firstModel = Api.SeaTreasureMng.GetFirstValue(groupId);
            if (firstModel != null)
            {
                firstValue = firstModel.Score;

                //fitstPlayerInfo = Api.PlayerInfoMng.GetJsonPlayerInfo(groupId, firstModel.Uid);
            }
            int playerValue = 0;
            //获取自己暗器值
            RankBaseModel playerModel = Api.SeaTreasureMng.GetPlayerValue(groupId, uid);
            if (playerModel != null)
            {
                playerValue = playerModel.Score;
            }
            MSG_CorssR_GET_SEA_TREASURE_VALUE msg = new MSG_CorssR_GET_SEA_TREASURE_VALUE();

            msg.Uid = uid;
            msg.Value = playerValue;
            msg.TotalValue = totalValue;
            msg.FirstValue = firstValue;

            if (firstModel != null)
            {
                msg.BaseInfo.AddRange(GetPlayerBaseInfoItemMsg(groupId, firstModel.Uid));
            }

            //if (fitstPlayerInfo != null)
            //{
            //    msg.BaseInfo.Add(GetPlayerInfoMsg(HFPlayerInfo.Uid, fitstPlayerInfo.Uid.ToString()));
            //    msg.BaseInfo.Add(GetPlayerInfoMsg(HFPlayerInfo.Name, fitstPlayerInfo.Name));
            //    msg.BaseInfo.Add(GetPlayerInfoMsg(HFPlayerInfo.MainId, fitstPlayerInfo.MainId.ToString()));
            //    msg.BaseInfo.Add(GetPlayerInfoMsg(HFPlayerInfo.HeroId, fitstPlayerInfo.HeroId.ToString()));
            //    msg.BaseInfo.Add(GetPlayerInfoMsg(HFPlayerInfo.BattlePower, fitstPlayerInfo.BattlePower.ToString()));
            //    msg.BaseInfo.Add(GetPlayerInfoMsg(HFPlayerInfo.GodType, fitstPlayerInfo.GodType.ToString()));
            //    msg.BaseInfo.Add(GetPlayerInfoMsg(HFPlayerInfo.Icon, fitstPlayerInfo.Icon.ToString()));
            //}

            Write(msg, uid);
        }


        public void OnResponse_GetDivineLoveInfo(MemoryStream stream, int uid = 0)
        {
            //MSG_RC_GET_DIVINE_LOVE_VALUE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RC_GET_DIVINE_LOVE_VALUE>(stream);
            Log.Write($"player {uid} GetDivineLoveInfo from main {MainId} ");
            int groupId = CrossBattleLibrary.GetGroupId(MainId);
            if (groupId == 0)
            {
                Log.Warn($"player {uid} GetDivineLoveInfo from main {MainId} not find group id ");
                return;
            }
            SendDivineLoveInfo(uid, groupId);
        }

        public void OnResponse_UpdateDivineLoveInfo(MemoryStream stream, int uid = 0)
        {
            MSG_RC_UPDATE_DIVINE_LOVE_VALUE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RC_UPDATE_DIVINE_LOVE_VALUE>(stream);
            Log.Write($"player {uid} UpdateDivineLoveInfo from main {MainId} ");
            int groupId = CrossBattleLibrary.GetGroupId(MainId);
            if (groupId == 0)
            {
                Log.Warn($"player {uid} UpdateDivineLoveInfo from main {MainId} not find group id ");
                return;
            }

            if (pks.HeartNum > 0)
            {
                JsonPlayerInfo playerInfo = Api.PlayerInfoMng.GetJsonPlayerInfo(groupId, uid);
                if (playerInfo == null)
                {
                    Dictionary<HFPlayerInfo, object> dataList = new Dictionary<HFPlayerInfo, object>();
                    foreach (var item in pks.BaseInfo)
                    {
                        HFPlayerInfo key = (HFPlayerInfo)item.Key;
                        switch (key)
                        {
                            case HFPlayerInfo.MainId:
                                dataList[key] = MainId;
                                break;
                            default:
                                dataList[key] = item.Value;
                                break;
                        }
                    }
                    //添加信息
                    playerInfo = new JsonPlayerInfo(dataList);
                    Api.PlayerInfoMng.AddPlayerInfo(groupId, uid, playerInfo);
                    //Api.CrossRedis.Call(new OperateAddCrossPlayerBaseInfo(uid, playerInfo));
                }
                else
                {
                    foreach (var item in pks.BaseInfo)
                    {
                        if (item.Key == (int)HFPlayerInfo.HeroId)
                        {
                            int heroId = int.Parse(item.Value);
                            if (heroId > 0 && playerInfo.HeroId != heroId)
                            {
                                playerInfo.HeroId = heroId;
                                playerInfo.Icon = heroId;
                                Api.PlayerInfoMng.AddPlayerInfo(groupId, uid, playerInfo);
                            }
                            break;
                        }
                    }
                }

                //检查结算时间
                RechargeGiftModel model;
                if (RechargeLibrary.CheckInRechargeGiftTime(RechargeGiftType.DivineLove, Api.Now(), out model))
                {
                    //需要更新数据
                    Api.DivineLoveMng.UpdatePlayerValue(groupId, uid, pks.HeartNum);
                }
                else
                {
                    Log.Warn($"player {uid} UpdateDivineLoveInfo failed: time is error, type {RechargeGiftType.DivineLove} value {pks.HeartNum} ");
                }
            }

            SendDivineLoveInfo(uid, groupId);
        }

        private void SendDivineLoveInfo(int uid, int groupId)
        {
            //获取当前总值
            int totalValue = Api.DivineLoveMng.GetCurrentValue(groupId);

            int firstValue = 0;
            //获取第一名爱心值
            RankBaseModel firstModel = Api.DivineLoveMng.GetFirstValue(groupId);
            if (firstModel != null)
            {
                firstValue = firstModel.Score;
            }
            int playerValue = 0;
            //获取自己爱心值
            RankBaseModel playerModel = Api.DivineLoveMng.GetPlayerValue(groupId, uid);
            if (playerModel != null)
            {
                playerValue = playerModel.Score;
            }
            MSG_CorssR_GET_DIVINE_LOVE_VALUE msg = new MSG_CorssR_GET_DIVINE_LOVE_VALUE();

            msg.Uid = uid;
            msg.Value = playerValue;
            msg.TotalValue = totalValue;
            msg.FirstValue = firstValue;
            if (firstModel != null)
            {
                msg.BaseInfo.AddRange(GetPlayerBaseInfoItemMsg(groupId, firstModel.Uid));
            }         

            Write(msg, uid);
        }

        public void OnResponse_GetStoneWallInfo(MemoryStream stream, int uid = 0)
        {
            //MSG_RC_GET_STONE_WALL_VALUE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RC_GET_STONE_WALL_VALUE>(stream);
            Log.Write($"player {uid} GetStoneWallInfo from main {MainId} ");
            int groupId = CrossBattleLibrary.GetGroupId(MainId);
            if (groupId == 0)
            {
                Log.Warn($"player {uid} GetStoneWallInfo from main {MainId} not find group id ");
                return;
            }
            SendStoneWallInfo(uid, groupId);
        }

        private void SendStoneWallInfo(int uid, int groupId)
        {
            //获取当前总值
            int totalValue = Api.StoneWallMng.GetCurrentValue(groupId);

            int firstValue = 0;
            //获取第一名爱心值
            RankBaseModel firstModel = Api.StoneWallMng.GetFirstValue(groupId);
            if (firstModel != null)
            {
                firstValue = firstModel.Score;
            }
            int playerValue = 0;
            //获取自己爱心值
            RankBaseModel playerModel = Api.StoneWallMng.GetPlayerValue(groupId, uid);
            if (playerModel != null)
            {
                playerValue = playerModel.Score;
            }
            MSG_CorssR_GET_STONE_WALL_VALUE msg = new MSG_CorssR_GET_STONE_WALL_VALUE();

            msg.Uid = uid;
            msg.Value = playerValue;
            msg.TotalValue = totalValue;
            msg.FirstValue = firstValue;
            if (firstModel != null)
            {
                msg.BaseInfo.AddRange(GetPlayerBaseInfoItemMsg(groupId, firstModel.Uid));
            }

            Write(msg, uid);
        }

        public void OnResponse_UpdateStoneWallInfo(MemoryStream stream, int uid = 0)
        {
            MSG_RC_UPDATE_STONE_WALL_VALUE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RC_UPDATE_STONE_WALL_VALUE>(stream);
            Log.Write($"player {uid} UpdateStoneWallInfo from main {MainId} ");
            int groupId = CrossBattleLibrary.GetGroupId(MainId);
            if (groupId == 0)
            {
                Log.Warn($"player {uid} UpdateStoneWallInfo from main {MainId} not find group id ");
                return;
            }

            if (pks.HammerNum > 0)
            {
                JsonPlayerInfo playerInfo = Api.PlayerInfoMng.GetJsonPlayerInfo(groupId, uid);
                if (playerInfo == null)
                {
                    Dictionary<HFPlayerInfo, object> dataList = new Dictionary<HFPlayerInfo, object>();
                    foreach (var item in pks.BaseInfo)
                    {
                        HFPlayerInfo key = (HFPlayerInfo)item.Key;
                        switch (key)
                        {
                            case HFPlayerInfo.MainId:
                                dataList[key] = MainId;
                                break;
                            default:
                                dataList[key] = item.Value;
                                break;
                        }
                    }
                    //添加信息
                    playerInfo = new JsonPlayerInfo(dataList);
                    Api.PlayerInfoMng.AddPlayerInfo(groupId, uid, playerInfo);
                    //Api.CrossRedis.Call(new OperateAddCrossPlayerBaseInfo(uid, playerInfo));
                }
                else
                {
                    foreach (var item in pks.BaseInfo)
                    {
                        if (item.Key == (int)HFPlayerInfo.HeroId)
                        {
                            int heroId = int.Parse(item.Value);
                            if (heroId > 0 && playerInfo.HeroId != heroId)
                            {
                                playerInfo.HeroId = heroId;
                                playerInfo.Icon = heroId;
                                Api.PlayerInfoMng.AddPlayerInfo(groupId, uid, playerInfo);
                            }
                            break;
                        }
                    }
                }

                //检查结算时间
                RechargeGiftModel model;
                if (RechargeLibrary.CheckInRechargeGiftTime(RechargeGiftType.StoneWall, Api.Now(), out model))
                {
                    //需要更新数据
                    Api.StoneWallMng.UpdatePlayerValue(groupId, uid, pks.HammerNum);
                }
                else
                {
                    Log.Warn($"player {uid} UpdateStoneWallInfo failed: time is error, type {RechargeGiftType.StoneWall} value {pks.HammerNum} ");
                }
            }

            SendStoneWallInfo(uid, groupId);
        }
    }

}
