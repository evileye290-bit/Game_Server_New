using System;
using System.Collections.Generic;
using System.IO;
using Engine;
using Message.IdGenerator;
using SocketShared;
using ServerShared;
using Logger;
using CommonUtility;
using Message.Relation.Protocol.RZ;
using Message.Manager.Protocol.MZ;
using DataProperty;
using EnumerateUtility;

namespace ZoneServerLib
{
    public partial class RelationServer
    {
        public void OnResponse_CharFamilyInfo(MemoryStream stream, int uid = 0)
        {
            //MSG_RZ_CHAR_FAMILY_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_CHAR_FAMILY_INFO>(stream);
            //PlayerChar player = server.PCManager.FindPc(msg.Uid);
            //if (player == null) return;
            ////if (player.CurrentMap != null && player.CurrentMap.DungeonData != null && player.CurrentMap.DungeonData.Type == (int)DungeonType.FamilyBoss)
            ////{
            ////    //在家族副本中的任务不需要接受家族副本信息变更
            ////    return;
            ////}
            //player.FamilyLevel = msg.Level;
            //player.FamilyTitle = (FamilyTitle)msg.Title;
            //player.FamilyName = msg.FamilyName;
            //player.Fid = msg.Fid;
            //// 通知客户端
            //PKS_ZC_FAMILY_NOTIFY notify = new PKS_ZC_FAMILY_NOTIFY();
            //notify.fild = msg.Fid;
            //notify.title = msg.Title;
            //notify.level = msg.Level;
            //notify.hasApplication = msg.hasApplication;
            ////notify.dungeons.AddRange(msg.dungeons);
            //player.Write(notify);

            //// 家族徽章
            ////player.UpdateFamilyBadge(player.FamilyLevel);

            //List<int> canPracticeList = new List<int>();
            //foreach (var item in GameConfig.FamilyPracticeUnlock)
            //{
            //    if (item.Value <= player.FamilyLevel)
            //    {
            //        canPracticeList.Add(item.Key);
            //    }
            //}
            //List<int> newPracticeList = new List<int>();
            //foreach (var item in canPracticeList)
            //{
            //    if (player.FamilyPracticeList.ContainsKey(item) == false)
            //    {
            //        newPracticeList.Add(item);
            //    }
            //}
            //if (newPracticeList.Count != 0)
            //{
            //    foreach (var item in newPracticeList)
            //    {
            //        // 新家族有新的修炼
            //        FamilyPractice newPractice = new FamilyPractice(item, 0);
            //        player.FamilyPracticeList.Add(newPractice.Id, newPractice);
            //    }
            //    // 同步db
            //    //player.SyncDBFamilyPractice();
            //    //player.BindStat();
            //}
            //foreach (var title in player.TitleList)
            //{
            //    if (title.Value.ConditionType == TitleType.FamilyWar && title.Value.EquipIndex == 0)
            //    {
            //        if (server.WinFid != player.Fid || player.Fid == 0 || (title.Value.ConditionNum == 1 && player.FamilyTitle != FamilyTitle.Chief))
            //        {
            //            player.Packet_Info.TitleId = 0;
            //            title.Value.EquipIndex = -1;
            //            //保存DB
            //            server.DB.Call(new QueryUpdateTitile(player.Uid, title.Value.Id, title.Value.EquipIndex), player.DBIndex);

            //            PKS_ZC_CHANGE_TITLE changeMsg = new PKS_ZC_CHANGE_TITLE();
            //            changeMsg.InstanceId = player.Instance_id;
            //            changeMsg.TItleId = 0;
            //            player.BroadCastNearby(changeMsg);
            //            player.Write(changeMsg);
            //        }
            //    }
            //}
            //检查家族称呼
            //player.CheckAddFamilyWarTitle();

            //if (player.CurrentMap != null && player.CurrentMap.PVPType == PVPType.Family)
            //{
            //    PKS_ZC_CHAR_SIMPLE_INFO character_info = new PKS_ZC_CHAR_SIMPLE_INFO();
            //    player.GetSimpleInfo(character_info);
            //    player.BroadCastNearby(character_info);
            //}
        }

        public void OnResponse_FamilyList(MemoryStream stream, int uid = 0)
        {
            //PKS_ZC_FAMILY_LIST msg = MessagePacker.ProtobufHelper.Deserialize<PKS_ZC_FAMILY_LIST>(stream);
            //PlayerChar player = server.PCManager.FindPc(msg.Uid);
            //if (player != null)
            //{
            //    player.Write(msg);
            //}
        }

        public void OnResponse_FamilyDetailInfo(MemoryStream stream, int uid = 0)
        {
            //PKS_ZC_FAMILY_DETAIL_INFO msg = MessagePacker.ProtobufHelper.Deserialize<PKS_ZC_FAMILY_DETAIL_INFO>(stream);
            //PlayerChar player = server.PCManager.FindPc(msg.Uid);
            //if (player != null)
            //{
            //    player.Write(msg);
            //}
        }

        public void OnResponse_SearchFamily(MemoryStream stream, int uid = 0)
        {
            //PKS_ZC_SEARCH_FAMILY msg = MessagePacker.ProtobufHelper.Deserialize<PKS_ZC_SEARCH_FAMILY>(stream);
            //PlayerChar player = server.PCManager.FindPc(msg.Uid);
            //if (player != null)
            //{
            //    player.Write(msg);
            //}
        }

        public void OnResponse_JoinFamily(MemoryStream stream, int uid = 0)
        {
            //MSG_RZ_JOIN_FAMILY msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_JOIN_FAMILY>(stream);
            //PlayerChar player = server.PCManager.FindPc(msg.Uid);
            //if (player != null)
            //{
            //    PKS_ZC_JOIN_FAMILY notify = new PKS_ZC_JOIN_FAMILY();
            //    notify.Result = msg.Result;
            //    foreach (var item in msg.list)
            //    {
            //        notify.List.Add(item);
            //    }
            //    player.Write(notify);
            //}
        }

        public void OnResponse_JoinFamilies(MemoryStream stream, int uid = 0)
        {
            //MSG_RZ_JOIN_FAMILIES msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_JOIN_FAMILIES>(stream);
            //PlayerChar player = server.PCManager.FindPc(msg.Uid);
            //if (player != null)
            //{
            //    PKS_ZC_JOIN_FAMILIES notify = new PKS_ZC_JOIN_FAMILIES();
            //    foreach (var item in msg.list)
            //    {
            //        notify.List.Add(item);
            //    }
            //    player.Write(notify);
            //}
        }

        public void OnResponse_NewFamilyApplicant(MemoryStream stream, int uid = 0)
        {
            //MSG_RZ_NEW_FAMILY_APPLICANT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_NEW_FAMILY_APPLICANT>(stream);
            //PlayerChar player = server.PCManager.FindPc(msg.Uid);
            //if (player != null)
            //{
            //    PKS_ZC_NEW_FAMILY_APPLICANT notify = new PKS_ZC_NEW_FAMILY_APPLICANT();
            //    notify.applyUid = msg.applyUid;
            //    player.Write(notify);
            //}
        }

        public void OnResponse_CreateFamily(MemoryStream stream, int uid = 0)
        {
            //MSG_RZ_CREATE_FAMILY msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_CREATE_FAMILY>(stream);
            //PlayerChar player = server.PCManager.FindPc(msg.Uid);
            //if (player != null)
            //{
            //    if (msg.Result == (int)ErrorCode.Success)
            //    {
            //        // 扣钱
            //        Dictionary<int, int> currencies = new Dictionary<int, int>();
            //        currencies.Add((int)CurrenciesType.GOLD, -1 * GameConfig.CreateFamilyCost);
            //        player.SynchronizeToClientCoins(currencies);
            //    }
            //    PKS_ZC_CREATE_FAMILY notify = new PKS_ZC_CREATE_FAMILY();
            //    notify.Result = msg.Result;
            //    player.Write(notify);
            //}
        }

        public void OnResponse_UpdateFamilyInfo(MemoryStream stream, int uid = 0)
        {
            //MSG_RZ_UPDATE_FAMILY_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_UPDATE_FAMILY_INFO>(stream);
            //PlayerChar player = server.PCManager.FindPc(msg.Uid);
            //if (player != null)
            //{
            //    player.Fid = msg.Fid;
            //    player.FamilyTitle = (FamilyTitle)msg.Title;
            //    // 通知客户端 家族信息变更
            //    PKS_ZC_FAMILY_NOTIFY notify = new PKS_ZC_FAMILY_NOTIFY();
            //    notify.fild = msg.Fid;
            //    notify.title = msg.Title;
            //    player.Write(notify);
            //}
        }

        public void OnResponse_FamilyApplyList(MemoryStream stream, int uid = 0)
        {
            //PKS_ZC_FAMILY_APPLY_LIST msg = MessagePacker.ProtobufHelper.Deserialize<PKS_ZC_FAMILY_APPLY_LIST>(stream);
            //PlayerChar player = server.PCManager.FindPc(msg.Uid);
            //if (player != null)
            //{
            //    player.Write(msg);
            //}
        }

        public void OnResponse_FamilyApplicationAgree(MemoryStream stream, int uid = 0)
        {
            //PKS_ZC_FAMILY_APPLICATION_AGREE msg = MessagePacker.ProtobufHelper.Deserialize<PKS_ZC_FAMILY_APPLICATION_AGREE>(stream);
            //PlayerChar player = server.PCManager.FindPc(msg.Uid);
            //if (player != null)
            //{
            //    player.Write(msg);
            //}
        }

        public void OnResponse_AssignFamilyTitle(MemoryStream stream, int uid = 0)
        {
            //MSG_RZ_ASSIGN_FAMILY_TITLE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_ASSIGN_FAMILY_TITLE>(stream);
            //PlayerChar player = server.PCManager.FindPc(msg.Uid);
            //if (player != null)
            //{
            //    PKS_ZC_ASSIGN_FAMILY_TITLE notify = new PKS_ZC_ASSIGN_FAMILY_TITLE();
            //    notify.Result = msg.Result;
            //    notify.memberUid = msg.MemberUid;
            //    notify.memberTitle = msg.memberTitle;
            //    notify.myTitle = msg.myTitle;
            //    player.Write(notify);
            //}
        }

        public void OnResponse_QuitFamily(MemoryStream stream, int uid = 0)
        {
            //MSG_RZ_QUIT_FAMILY msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_QUIT_FAMILY>(stream);
            //Log.Write("player {0} quit family result {1}", msg.Uid, msg.Result);
            //PlayerChar player = server.PCManager.FindPc(msg.Uid);
            //if (player == null) return;
            //if (msg.Result == (int)ErrorCode.Success)
            //{
            //    // 退出家族 相关数据维护
            //    player.Fid = 0;
            //    player.FamilyTitle = FamilyTitle.Nobody;
            //    player.FamilyLevel = 0;
            //    player.FamilyName = "";

            //    // db已在relation同步
            //    player.Currencies[CurrenciesType.FamilyFreezeContribution] += player.Currencies[CurrenciesType.FamilyContribution];
            //    player.Currencies[CurrenciesType.FamilyContribution] = 0;

            //    // 同步客户端
            //    PKS_ZC_Currencies msgCurrencies = new PKS_ZC_Currencies();
            //    PKS_ZC_Coin coinContribution = new PKS_ZC_Coin();
            //    coinContribution.CoinNum = (int)CurrenciesType.FamilyContribution;
            //    coinContribution.Count = player.Currencies[CurrenciesType.FamilyContribution];

            //    PKS_ZC_Coin coinFreezeContribution = new PKS_ZC_Coin();
            //    coinFreezeContribution.CoinNum = (int)CurrenciesType.FamilyFreezeContribution;
            //    coinFreezeContribution.Count = player.Currencies[CurrenciesType.FamilyFreezeContribution];

            //    msgCurrencies.Currencies.Add(coinContribution);
            //    msgCurrencies.Currencies.Add(coinFreezeContribution);
            //    player.Write(msgCurrencies);
            //}
            //PKS_ZC_QUIT_FAMILY notify = new PKS_ZC_QUIT_FAMILY();
            //notify.Result = msg.Result;
            //notify.kicked = msg.kicked;
            //player.Write(notify);
        }

        public void OnResponse_KickFamilyMember(MemoryStream stream, int uid = 0)
        {
            //MSG_RZ_KICK_FAMILY_MEMBER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_KICK_FAMILY_MEMBER>(stream);
            //Log.Write("player {0} kick family member {1} result {2}", msg.Uid, msg.MemberUid, msg.Result);
            //PlayerChar player = server.PCManager.FindPc(msg.Uid);
            //if (player != null)
            //{
            //    PKS_ZC_KICK_FAMILY_MEMBER notify = new PKS_ZC_KICK_FAMILY_MEMBER();
            //    notify.memberUid = msg.MemberUid;
            //    notify.Result = msg.Result;
            //    player.Write(notify);
            //}
        }

        public void OnResponse_FamilyRank(MemoryStream stream, int uid = 0)
        {
            //PKS_ZC_FAMILY_RANK msg = MessagePacker.ProtobufHelper.Deserialize<PKS_ZC_FAMILY_RANK>(stream);
            //PlayerChar player = server.PCManager.FindPc(msg.Uid);
            //if (player != null)
            //{
            //    player.Write(msg);
            //}
        }

        public void OnResponse_FamilyLevelUp(MemoryStream stream, int uid = 0)
        {
            //MSG_RZ_FAMILY_LEVELUP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_FAMILY_LEVELUP>(stream);
            //PlayerChar player = server.PCManager.FindPc(msg.Uid);
            //if (player == null) return;
            //if (player.Fid != msg.Fid) return;
            //player.FamilyLevel = msg.familyLevel;
            //// 检查是有有新的修炼
            //List<int> canPracticeList = new List<int>();
            //foreach (var item in GameConfig.FamilyPracticeUnlock)
            //{
            //    if (item.Value <= player.FamilyLevel)
            //    {
            //        canPracticeList.Add(item.Key);
            //    }
            //}
            //List<int> newPracticeList = new List<int>();
            //foreach (var item in canPracticeList)
            //{
            //    if (player.FamilyPracticeList.ContainsKey(item) == false)
            //    {
            //        newPracticeList.Add(item);
            //    }
            //}
            //if (newPracticeList.Count != 0)
            //{
            //    foreach (var item in newPracticeList)
            //    {
            //        // 新家族有新的修炼
            //        FamilyPractice newPractice = new FamilyPractice(item, 0);
            //        player.FamilyPracticeList.Add(newPractice.Id, newPractice);
            //    }
            //    // 同步db
            //    //player.SyncDBFamilyPractice();
            //    player.BindStat();
            //}
            //// 通知客户端 家族升级
            //PKS_ZC_FAMILY_NOTIFY notify = new PKS_ZC_FAMILY_NOTIFY();
            //notify.fild = msg.Fid;
            //notify.title = (int)player.FamilyTitle;
            //notify.level = player.FamilyLevel;
            //notify.hasApplication = false;
            //player.Write(notify);
        }

        public void OnResponse_FamilyContentEdit(MemoryStream stream, int uid = 0)
        {
            //MSG_RZ_FAMILY_CONTENT_EDIT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_FAMILY_CONTENT_EDIT>(stream);
            //PlayerChar player = server.PCManager.FindPc(msg.Uid);
            //if (player != null)
            //{
            //    PKS_ZC_FAMILY_CONTENT_EDIT notify = new PKS_ZC_FAMILY_CONTENT_EDIT();
            //    notify.Result = msg.Result;
            //    notify.Type = msg.Type;
            //    notify.content = msg.Content;
            //    player.Write(notify);
            //}
        }

        //public void OnResponse_JoinFamilySuccess(MemoryStream stream, int uid = 0)
        //{
        //    // 废弃 与 MSG_RZ_CHAR_FAMILY_INFO 功能重复
        //    MSG_RZ_JOIN_FAMILY_SUCCESS msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_JOIN_FAMILY_SUCCESS>(stream);
        //    PlayerChar player = server.PCManager.FindPc(msg.Uid);
        //    if (player == null) return;
        //    player.FamilyLevel = msg.Level;
        //    player.FamilyTitle = (FamilyTitle)msg.Title;
        //    player.Fid = msg.Fid;
        //    // TODO 是否需要通知客户端
        //    // 校验家族修炼 
        //}

        //public void OnResponse_FamilyFirstWins(MemoryStream stream, int uid = 0)
        //{
        //    PKS_ZC_FAMILY_DUNGEON_FIRST_WIN_LIST msg = MessagePacker.ProtobufHelper.Deserialize<PKS_ZC_FAMILY_DUNGEON_FIRST_WIN_LIST>(stream);
        //    PlayerChar player = server.PCManager.FindPc(msg.Uid);
        //    if (player != null)
        //    {
        //        player.Write(msg);
        //    }
        //}

        //public void OnResponse_FamilySingleDamages(MemoryStream stream, int uid = 0)
        //{
        //    PKS_ZC_FAMILY_DUNGEON_SINGLE_DAMAGE_LIST msg = MessagePacker.ProtobufHelper.Deserialize<PKS_ZC_FAMILY_DUNGEON_SINGLE_DAMAGE_LIST>(stream);
        //    PlayerChar player = server.PCManager.FindPc(msg.Uid);
        //    if (player != null)
        //    {
        //        player.Write(msg);
        //    }
        //}

        //public void OnResponse_FamilyAccumulateDamages(MemoryStream stream, int uid = 0)
        //{
        //    PKS_ZC_FAMILY_DUNGEON_ACCUMULATE_DAMAGE_LIST msg = MessagePacker.ProtobufHelper.Deserialize<PKS_ZC_FAMILY_DUNGEON_ACCUMULATE_DAMAGE_LIST>(stream);
        //    PlayerChar player = server.PCManager.FindPc(msg.Uid);
        //    if (player != null)
        //    {
        //        player.Write(msg);
        //    }
        //}

        //public void OnResponse_GetFamilyReward(MemoryStream stream, int uid = 0)
        //{
        //    PKS_RZ_GET_FAMILY_DUNGEON_REWARD pks = MessagePacker.ProtobufHelper.Deserialize<PKS_RZ_GET_FAMILY_DUNGEON_REWARD>(stream);
        //    PlayerChar player = server.PCManager.FindPc(pks.PcUid);
        //    if (player != null)
        //    {
        //        FamilyDungeonRewardModel dungeonRewards = FamilyLibrary.GetDungeonRewards(pks.Stage);
        //        if (dungeonRewards != null)
        //        {
        //            PKS_ZC_FAMILY_DUNGEON_REWARD msg = new PKS_ZC_FAMILY_DUNGEON_REWARD();

        //            if (pks.Damage > 0)
        //            {
        //                List<int[]> allRewards = new List<int[]>();
        //                FamilyRewardItem pass = dungeonRewards.GetPassReward(pks.DamageIndex + 1);
        //                if (pass != null)
        //                {
        //                    string rewardString = player.ChangeRewardJobItems(pass.Reward);
        //                    List<int[]> rewards = StringSplit.GetRewardList(rewardString);
        //                    foreach (var item in rewards)
        //                    {
        //                        PKS_ZC_REWARD_ITEM rewardItem = new PKS_ZC_REWARD_ITEM();
        //                        rewardItem.RewardId = item[0];
        //                        rewardItem.Number = item[1];
        //                        msg.Pass.Add(rewardItem);
        //                    }

        //                    //ActiviyReward FR20161129
        //                    player.RecordOutputLog(ObtainWay.ActiviyReward, (int)ActivityRewardType.FamliyDungeonPass, pks.DamageIndex+1, rewards);

        //                    allRewards.AddRange(rewards);
        //                }
        //                FamilyRewardItem contribution = dungeonRewards.GetContributionReward(pks.DamageIndex + 1);
        //                if (contribution != null)
        //                {
        //                    string rewardString = player.ChangeRewardJobItems(contribution.Reward);
        //                    List<int[]> rewards = StringSplit.GetRewardList(rewardString);
        //                    foreach (var item in rewards)
        //                    {
        //                        PKS_ZC_REWARD_ITEM rewardItem = new PKS_ZC_REWARD_ITEM();
        //                        rewardItem.RewardId = item[0];
        //                        rewardItem.Number = item[1];
        //                        msg.Damage.Add(rewardItem);
        //                    }

        //                    //ActiviyReward FR20161129
        //                    player.RecordOutputLog(ObtainWay.ActiviyReward, (int)ActivityRewardType.FamliyDungeonContribution, pks.DamageIndex+1, rewards);

        //                    allRewards.AddRange(rewards);
        //                    msg.rank = pks.DamageIndex + 1;
        //                }
        //                if (pks.IsFirst)
        //                {
        //                    FamilyRewardItem firstKill = dungeonRewards.GetFirstKillReward(pks.FamilyJob);
        //                    if (firstKill != null)
        //                    {
        //                        string rewardString = player.ChangeRewardJobItems(firstKill.Reward);
        //                        List<int[]> rewards = StringSplit.GetRewardList(rewardString);
        //                        foreach (var item in rewards)
        //                        {
        //                            PKS_ZC_REWARD_ITEM rewardItem = new PKS_ZC_REWARD_ITEM();
        //                            rewardItem.RewardId = item[0];
        //                            rewardItem.Number = item[1];
        //                            msg.First.Add(rewardItem);
        //                        }

        //                        //ActiviyReward FR20161129
        //                        player.RecordOutputLog(ObtainWay.ActiviyReward, (int)ActivityRewardType.FamliyDungeonFrist, pks.FamilyJob, rewards);

        //                        allRewards.AddRange(rewards);
        //                    }
        //                }
        //                if (allRewards.Count > 0)
        //                {
        //                    player.AddRewards(allRewards, (int)EmailItemType.DUNGEON);
        //                }
        //            }
        //            player.Write(msg);

        //        }
        //        else
        //        {
        //            Log.Warn("player {0} GetFamilyRearad not find reward {1} ", pks.PcUid, pks.Stage);
        //        }
        //    }
        //    else
        //    {
        //        Log.Warn("player {0} GetFamilyRearad not find uid", pks.PcUid);
        //    }
        //}

        //public void OnResponse_LeaveFamilyDungeon(MemoryStream stream, int uid = 0)
        //{
        //    PKS_ZC_FAMILY_DUNGEON_LEAVE_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<PKS_ZC_FAMILY_DUNGEON_LEAVE_REWARD>(stream);
        //    PlayerChar player = server.PCManager.FindPc(msg.PcUid);
        //    if (player != null)
        //    {
        //        player.Write(msg);
        //    }
        //}

        //public void OnResponse_FamilyDungeonView(MemoryStream stream, int uid = 0)
        //{
        //    PKS_ZC_FAMILY_DUNGEON_VIEW msg = MessagePacker.ProtobufHelper.Deserialize<PKS_ZC_FAMILY_DUNGEON_VIEW>(stream);
        //    FieldMap map = server.DungeonManager.GetDungeonItem(msg.Uid);
        //    if (map != null && map.DungeonData != null && map.DungeonData.Type == (int)DungeonType.FamilyBoss)
        //    {
        //        foreach (var player in map.PcList)
        //        {
        //            int damage = map.GetPcDamage(player.Value.Uid);
        //            PKS_ZC_FAMILY_DUNGEON_VIEW_ITEM info = new PKS_ZC_FAMILY_DUNGEON_VIEW_ITEM();
        //            info.uid = player.Value.Uid;
        //            info.name = player.Value.Packet_Info.name;
        //            info.camp = player.Value.Packet_Info.camp;
        //            info.damage = damage;
        //            msg.Current.Add(info);
        //        }
        //        msg.pcNum = map.PcList.Count;

        //        foreach (var player in map.PcList)
        //        {
        //            if (player.Value.IsRobot == false)
        //            {
        //                player.Value.Write(msg);
        //            }
        //        }
        //    }
        //}

        //public void OnResponse_EntetFamilyDungeon(MemoryStream stream, int uid = 0)
        //{
        //    PKS_RZ_ENTER_FAMILY_DUNGEON pks = MessagePacker.ProtobufHelper.Deserialize<PKS_RZ_ENTER_FAMILY_DUNGEON>(stream);
        //    int pcUid = pks.PcUid;
        //    int stage = pks.DungeonId;
        //    PlayerChar player = server.PCManager.FindPc(pcUid);
        //    if (player != null)
        //    {
        //        if (pks.Result == PKS_RZ_ENTER_FAMILY_DUNGEON.RESULT.DUNGEON_ERROR)
        //        {
        //            PKS_ZC_ENTER_FAMILY_DUNGEON response = new PKS_ZC_ENTER_FAMILY_DUNGEON();
        //            response.Result = PKS_ZC_ENTER_FAMILY_DUNGEON.RESULT.COUNT_ERROR;
        //            player.Write(response);
        //            return;
        //        }
        //        else if (pks.Result == PKS_RZ_ENTER_FAMILY_DUNGEON.Types.RESULT.Success)
        //        {
        //            player.AskManagerFamilyBossDungeon(stage);
        //        }
        //    }
        //}

        //public void OnResponse_GetFamilyDungeonBossHp(MemoryStream stream, int uid = 0)
        //{
        //    MSG_RZ_GET_FAMILY_BOSS_HP pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_GET_FAMILY_BOSS_HP>(stream);
        //    int familyUid = pks.Fid;
        //    int stage = pks.DungeonId;
        //    int instanceId = pks.InstanceId;
        //    int hp = pks.Hp;
        //    bool isEnd = pks.IsEnd;

        //    FieldMap map = server.DungeonManager.GetDungeonItem(instanceId);
        //    if (map != null)
        //    {
        //        if (isEnd)
        //        {
        //            map.IsEnd = true;
        //            Log.Warn("DungeonId {0} GetFamilyDungeonBossHp map {1} isEnd, family {2}", stage, instanceId, familyUid);
        //        }
        //        else
        //        {
        //            if (map.DungeonData != null && map.DungeonData.Type == (int)DungeonType.FamilyBoss && hp > 0)
        //            {
        //                map.BossHp = hp;
        //                //BOSS血量
        //                //if (map.MonsterList.Count == 1)
        //                //{
        //                foreach (var monster in map.MonsterList)
        //                {
        //                    monster.Value.Nature.CAL_HP = hp;
        //                    monster.Value.Packet_Info.hp = hp;
        //                }
        //                //}
        //                Log.WriteLine("DungeonId {0} GetFamilyDungeonBossHp BossHp {1} family {2} monster count {3}", stage, hp, familyUid, map.MonsterList.Count);
        //            }
        //            else
        //            {
        //                Log.Warn("DungeonId {0} GetFamilyDungeonBossHp map {1} error, family {2}", stage, instanceId, familyUid);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        Log.Warn("DungeonId {0} GetFamilyDungeonBossHp GetDungeonItem {1} error, family {2}", stage, instanceId, familyUid);
        //    }
        //}

        //public void OnResponse_GetFamilyWarInfo(MemoryStream stream, int uid = 0)
        //{
        //    PKS_ZC_FAMILY_WAR_LIST msg = MessagePacker.ProtobufHelper.Deserialize<PKS_ZC_FAMILY_WAR_LIST>(stream);
        //    PlayerChar player = server.PCManager.FindPc(msg.Uid);
        //    if (player != null)
        //    {
        //        player.Write(msg);
        //    }
        //}

        //public void OnResponse_OpenFamilyWarMember(MemoryStream stream, int uid = 0)
        //{
        //    PKS_ZC_FAMILY_WAR_MEMVER_LIST msg = MessagePacker.ProtobufHelper.Deserialize<PKS_ZC_FAMILY_WAR_MEMVER_LIST>(stream);
        //    PlayerChar player = server.PCManager.FindPc(msg.Uid);
        //    if (player != null)
        //    {
        //        player.Write(msg);
        //    }
        //}

        //public void OnResponse_SaveFamilyWarMember(MemoryStream stream, int uid = 0)
        //{
        //    PKS_ZC_SAVE_FAMILY_WAR_MEMBER msg = MessagePacker.ProtobufHelper.Deserialize<PKS_ZC_SAVE_FAMILY_WAR_MEMBER>(stream);
        //    PlayerChar player = server.PCManager.FindPc(msg.Uid);
        //    if (player != null)
        //    {
        //        player.Write(msg);
        //    }
        //}

        //public void OnResponse_GetFamilyWarReward(MemoryStream stream, int uid = 0)
        //{
        //    PKS_ZC_GET_FAMILY_WAR_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<PKS_ZC_GET_FAMILY_WAR_REWARD>(stream);
        //    PlayerChar player = server.PCManager.FindPc(msg.Uid);
        //    if (player != null)
        //    {
        //        Log.Write("player {0} GetFamilyWarReward state {1} result {2}", msg.Uid, player.Currencies[CurrenciesType.FamilyWarReward], (int)msg.Result);
        //        if (msg.Result == PKS_ZC_GET_FAMILY_WAR_REWARD.Types.RESULT.Success)
        //        {
        //            if (player.Currencies[CurrenciesType.FamilyWarReward] < 1)
        //            {
        //                List<int[]> rewards = new List<int[]>();
        //                if (!string.IsNullOrEmpty(msg.FamilyWar))
        //                {
        //                    string newRewardString = player.ChangeRewardJobItems(msg.FamilyWar);
        //                    rewards.AddRange(StringSplit.GetRewardList(newRewardString));
        //                }
        //                if (!string.IsNullOrEmpty(msg.Guide))
        //                {
        //                    string newRewardString = player.ChangeRewardJobItems(msg.Guide);
        //                    rewards.AddRange(StringSplit.GetRewardList(newRewardString));
        //                }

        //                //领取状态置位
        //                rewards.Add(new int[] { (int)CurrenciesType.FamilyWarReward, 1 });
        //                player.AddRewards(rewards, (int)EmailItemType.NONE);
        //            }
        //            else
        //            {
        //                Log.Warn("player {0} GetFamilyWarReward count {1}", msg.Uid, player.Currencies[CurrenciesType.FamilyWarReward]);
        //            }
        //        }
        //        player.Write(msg);
        //    }
        //}
           
        /// <summary>
        /// 进入家族战副本
        /// </summary>
        /// <param name="stream"></param>
        //public void OnResponse_EntetFamilyWar(MemoryStream stream, int uid = 0)
        //{
        //    PKS_RZ_ENTER_FAMILY_WAR pks = MessagePacker.ProtobufHelper.Deserialize<PKS_RZ_ENTER_FAMILY_WAR>(stream);
        //    int pcUid = pks.PcUid;
        //    int dungeonId = pks.DungeonId;
        //    PlayerChar player = server.PCManager.FindPc(pcUid);
        //    if (player != null)
        //    {
        //        switch (pks.Result)
        //        {
        //            case PKS_RZ_ENTER_FAMILY_WAR.RESULT.IsFamily1:
        //                player.AskManagerFamilyWar(dungeonId, 1);
        //                break;
        //            case PKS_RZ_ENTER_FAMILY_WAR.RESULT.IsFamily2:
        //                player.AskManagerFamilyWar(dungeonId, 2);
        //                break;
        //            case PKS_RZ_ENTER_FAMILY_WAR.RESULT.IsObserber:
        //                player.AskManagerFamilyWar(dungeonId, 3);
        //                break;
        //            default:
        //                PKS_ZC_ENTER_FAMILY_WAR response = new PKS_ZC_ENTER_FAMILY_WAR();
        //                response.Result = PKS_ZC_ENTER_FAMILY_WAR.RESULT.DUNGEON_ERROR;
        //                player.Write(response);
        //                return;
        //        }
        //    }
        //}

        //public void OnResponse_NoticeFamilyWarMember(MemoryStream stream, int uid = 0)
        //{
        //    PKS_RZ_NOTICE_FAMILY_WAR_MEMBER pks = MessagePacker.ProtobufHelper.Deserialize<PKS_RZ_NOTICE_FAMILY_WAR_MEMBER>(stream);
        //    Log.Write("NoticeFamilyWarMember uid coiunt {0}", pks.Uids.Count);
        //    foreach (var pcUid in pks.Uids)
        //    {
        //        PlayerChar player = server.PCManager.FindPc(pcUid);
        //        if (player != null)
        //        {
        //            PKS_ZC_NOTICE_FAMILY_WAR_MEMBER msg = new PKS_ZC_NOTICE_FAMILY_WAR_MEMBER();
        //            player.Write(msg);
        //        }  
        //    }
        //}

        //public void OnResponse_FamilyWarAnnouncement(MemoryStream stream, int uid = 0)
        //{
        //    PKS_RZ_FAMILY_WAR_ANNOUNCEMENT pks = MessagePacker.ProtobufHelper.Deserialize<PKS_RZ_FAMILY_WAR_ANNOUNCEMENT>(stream);
        //    Log.Write("FamilyWarAnnouncement type {0} name {1}", pks.type, pks.name);
        //    ANNOUNCEMENT_TYPE type = (ANNOUNCEMENT_TYPE)pks.type;
        //    List<string> list = new List<string>();
        //    if (!string.IsNullOrEmpty(pks.name))
        //    {
        //        list.Add(pks.name);
        //    }
        //    server.BroadcastAnnouncement(type, list);
        //}

        //public void OnResponse_GetFamilyWarWinId(MemoryStream stream, int uid = 0)
        //{
        //    PKS_RZ_FAMILY_WAR_WIN_ID pks = MessagePacker.ProtobufHelper.Deserialize<PKS_RZ_FAMILY_WAR_WIN_ID>(stream);
        //    Log.Write("GetFamilyWarWinId id {0}", pks.fid);
        //    server.WinFid = pks.fid;

        //    if (pks.Uids.Count > 0)
        //    {
        //        foreach (var pcUid in pks.Uids)
        //        {
        //            PlayerChar player = server.PCManager.FindPc(pcUid);
        //            if (player != null)
        //            {
        //                player.CheckAddFamilyWarTitle();
        //            }
        //        }
        //    }
        //}

        //public void OnResponse_CheckFamilyWarTitle(MemoryStream stream, int uid = 0)
        //{
        //    PKS_RZ_CHECK_FAMILY_WAR_TITLE pks = MessagePacker.ProtobufHelper.Deserialize<PKS_RZ_CHECK_FAMILY_WAR_TITLE>(stream);
        //    //Log.Write("GetFamilyWarWinId id {0}", pks.fid);
        //    if (pks.Uids.Count > 0)
        //    {
        //        foreach (var pcUid in pks.Uids)
        //        {
        //            PlayerChar player = server.PCManager.FindPc(pcUid);
        //            if (player != null)
        //            {
        //                foreach (var title in player.TitleList)
        //                {
        //                    if (title.Value.ConditionType == TitleType.FamilyWar && title.Value.EquipIndex == 0)
        //                    {
        //                        if (server.WinFid != player.Fid || player.Fid == 0 || (int)player.FamilyTitle != title.Value.ConditionNum)
        //                        {
        //                            title.Value.EquipIndex = -1;
        //                            //保存DB
        //                            server.DB.Call(new QueryUpdateTitile(player.Uid, title.Value.Id, title.Value.EquipIndex), player.DBIndex);

        //                            PKS_ZC_CHANGE_TITLE changeMsg = new PKS_ZC_CHANGE_TITLE();
        //                            changeMsg.InstanceId = player.Instance_id;
        //                            changeMsg.TItleId = 0;
        //                            player.BroadCastNearby(changeMsg);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        //public void OnResponse_UpdateFamilyWarReward(MemoryStream stream, int uid = 0)
        //{
        //    //PKS_RZ_UPDATE_FAMILY_WAR_REWARD pks = MessagePacker.ProtobufHelper.Deserialize<PKS_RZ_UPDATE_FAMILY_WAR_REWARD>(stream);
        //    Log.Write("UpdateFamilyWarReward id 0");

        //    foreach (var player in server.PCManager.PcList)
        //    {
        //        player.Value.Currencies[CurrenciesType.FamilyWarReward] = 0;
        //    }

        //    foreach (var player in server.PCManager.PcOfflineList)
        //    {
        //        player.Value.Currencies[CurrenciesType.FamilyWarReward] = 0;
        //    }
        //}
    }
}
