using DataProperty;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using ServerShared;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace ZoneServerLib
{
    public partial class GateServer
    {
        private void OnResponse_Chat(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_CHAT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_CHAT>(stream);
            Log.WriteLine("player {0} chat channel {1} content {2} param {3} emojiId {4}", msg.PcUid, msg.ChatChannel, msg.Content, msg.Param, msg.EmojiId);
            PlayerChar player = Api.PCManager.FindPc(msg.PcUid);
            if (player == null)
            {
                Log.Warn("player {0} chat failed, no such player", msg.PcUid);
                return;
            }
#if DEBUG
            if (player.IsGm == 1 && player.CheckGMCommand(msg.Content))
            {
                return;
            }
#endif
            //内容为空直接返回
            if (string.IsNullOrEmpty(msg.Content) && msg.EmojiId <= 0)
            {
                return;
            }
            //TODO:判断活跃度是否足够
            if (!player.CheckActivation())
            {
                Log.WarnLine("player {0} chat activation is not enough", msg.PcUid);
                return;
            }

            //  判断是否禁言
            if (ZoneServerApi.now < player.SilenceTime)
            {
                player.SendSilenceInfo();
                return;
            }
            else
            {
                player.CheckOpenVoice();
            }
            //
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0}  chat not in map ", msg.PcUid);
                return;
            }

            if (msg.EmojiId > 0)
            {
                Data data = DataListManager.inst.GetData("WorldEmoticon", msg.EmojiId);
                if (data == null)
                {
                    Log.Warn("player {0} chat not find emoji {1} ", msg.PcUid, msg.EmojiId);
                    return;
                }
            }

            //检查是否允许在该频道发言
            if (!player.CheckChatLimitOpen(msg.ChatChannel))
            {
                Log.WarnLine("player {0} chat not open.", msg.PcUid);
                return;
            }

            //检查字长
            int wordsLen = Api.wordChecker.GetWordLen(msg.Content);
            if (wordsLen > ChatLibrary.ChatWordMaxCount)
            {
                Log.WarnLine("player {0} send words error, words len is {1} ", msg.PcUid, wordsLen);
                player.SendErrorCodeMsg(ErrorCode.LengthLimit);
                return;
            }

            //检查是否是聊天白名单
            bool isWhite = false;
            if (msg.EmojiId > 0)
            {
                isWhite = true;
            }
            else
            {
                isWhite = ChatLibrary.CheckIsWhiteContent(msg.Content);
            }

            if (!isWhite)
            {
                if (player.server.httpSensitiveHelper.CheckOpen == 1)
                {
                    HttpChecker(msg, player);
                }
                else
                {
                    msg.Content = RecordSensitiveWord(player, msg.Content);
                    SendChat(player, msg);
                }
            }
            else
            {
                SendChat(player, msg);
            }
        }

        private void HttpChecker(MSG_GateZ_CHAT msg, PlayerChar player)
        {
            CheckSensitiveQuery_Text checkSensitive = new CheckSensitiveQuery_Text();
            string toUid = null;
            ChatChannel chatType = (ChatChannel)msg.ChatChannel;
            if (chatType == ChatChannel.Person)
            {
                toUid = msg.Param.ToString();
            }

            var paramsArr1 = player.GetSensitiveTextCheckParameters(msg.Content, chatType.ToString(), msg.ChatChannel.ToString(), toUid);

            player.server.httpSensitiveHelper.PostTextAsync(paramsArr1, checkSensitive, () =>
            {
                if (checkSensitive.Result != null)
                {
                    if (checkSensitive.ErrorCode == 0)
                    {
                        if (checkSensitive.AdvertCheckedResult != 0) //,非0，依据confidence字段判断 
                        {
                            //广告行为
                            player.CheckSetSilence();
                            player.NotifyClientSensitiveWord();
                            return checkSensitive.Result;
                        }

                        if (checkSensitive.IsSensitive)
                        {
                            //敏感字
                            player.CheckSetSilence();
                            player.NotifyClientSensitiveWord();
                            return checkSensitive.Result;
                        }
                    }
                }

                if (player.server.http163Helper.CheckOpen == 1)
                {
                    Http163Checker(msg, player);
                }
                else
                {
                    msg.Content = RecordSensitiveWord(player, msg.Content);
                    SendChat(player, msg);
                }
                return checkSensitive.Result;
            });
        }

        private void Http163Checker(MSG_GateZ_CHAT msg, PlayerChar player)
        {
            Check163Query_Text check163 = new Check163Query_Text();
            var paramsArr = player.Get163TextCheckParameters(Context163Type.Chat, msg.Content);
            player.server.http163Helper.PostTextAsync(paramsArr, check163, () =>
            {
                if (check163.Result != null)
                {
                    if (check163.action != 0) //不通过
                    {
                        //TODO:BOIL 1：嫌疑，2：不通过
                        player.CheckSetSilence();
                        player.NotifyClientSensitiveWord();
                        return check163.Result;
                    }
                }

                msg.Content = RecordSensitiveWord(player, msg.Content);
                SendChat(player, msg);
                return check163.Result;
            });
        }

        private void SendChat(PlayerChar player, MSG_GateZ_CHAT msg)
        {
            switch ((ChatChannel)msg.ChatChannel)
            {
                //组队 家族 阵营频道转发给Relation
                case ChatChannel.Family:
                    if (player.CheckFamilySpeakTime())
                    {
                        Api.RelationServer.AddChat(player, msg);
                    }
                    else
                    {
                        Log.Warn("player {0} Chat {1} time error, last time is {2}", player.Uid, msg.ChatChannel, player.FamilySpeakTime);
                    }
                    break;
                case ChatChannel.Camp:
                    if (player.CheckCampSpeakTime())
                    {
                        Api.RelationServer.AddChat(player, msg);
                    }
                    else
                    {
                        Log.Warn("player {0} Chat {1} time error, last time is {2}", player.Uid, msg.ChatChannel, player.CampSpeakTime);
                    }
                    break;
                case ChatChannel.Team:
                    if (player.CheckTeamSpeakTime())
                    {
                        Api.RelationServer.AddChat(player, msg);
                    }
                    else
                    {
                        Log.Warn("player {0} Chat {1} time error, last time is {2}", player.Uid, msg.ChatChannel, player.TeamSpeakTime);
                    }
                    break;
                case ChatChannel.World:
                    if (player.CheckWorldSpeakTime())
                    {
                        //player.SetFristSpeak(msg.Content);
                        //player.SetTotalSpeakCount();
                        player.SendWorldChat(msg);
                    }
                    else
                    {
                        Log.Warn("player {0} Chat {1} time error, last time is {2}", player.Uid, msg.ChatChannel, player.WorldSpeakTime);
                    }
                    break;
                case ChatChannel.Recruit:
                    if (player.CheckRecruitSpeakTime())
                    {
                        //TODO:征召
                    }
                    else
                    {
                        Log.Warn("player {0} Chat {1} time error, last time is {2}", player.Uid, msg.ChatChannel, player.RecruitSpeakTime);
                    }
                    break;
                case ChatChannel.Person:
                    if (player.SpeakSensitiveWord)
                    {
                        Log.Warn("player {0} Chat {1} failed: speak sensitive word", player.Uid, msg.ChatChannel);
                        return;
                    }
                    player.SendPersonChat(msg);
                    break;
                default:
                    Log.Warn("player {0} not find ChatChannel {1}", player.Uid, msg.ChatChannel);
                    break;
            }

            if (msg.EmojiId > 0)
            {
                player.KomoeEventLogChatFlow(msg.ChatChannel, msg.EmojiId.ToString(), msg.PcUid);
            }
            else
            {
                player.KomoeEventLogChatFlow(msg.ChatChannel, msg.Content, msg.PcUid);
            }
        }

        private string RecordSensitiveWord(PlayerChar player, string content)
        {
            // 敏感字记录到日志 服务器ID|角色ID|名称|等级|vip等级|钻石|内容|时间
            if (Api.sensitiveWordChecker.HasBadWord(content))
            {
                if (GameConfig.TrackingLogSwitch)
                {
                    string log = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}",
                    Api.MainId, player.Uid, player.Name, player.Level, player.PassLevel,
                    player.GetCoins(CurrenciesType.diamond), content, ZoneServerApi.now.ToString("yyyy-MM-dd HH:mm:ss"));
                    Api.TrackingLoggerMng.Write(log, TrackingLogType.LISTENCHAT);
                }
                //说敏感词标记
                player.SpeakSensitiveWord = true;
            }
            //屏蔽字替换
            string finalContent = FilterBadWord(content);
            return finalContent;
        }

        private void OnResponse_ChatTrumpet(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_USE_CHAT_TRUMPET msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_USE_CHAT_TRUMPET>(stream);
            Log.Write("player {0} use chat trumpet id {1} words {2} useItem {3}", msg.PcUid, msg.Id, msg.Words, msg.UseItem.ToString());
            PlayerChar player = Api.PCManager.FindPc(msg.PcUid);
            if (player == null)
            {
                Log.Warn("player {0} chat trumpet not in gateid {1} pc list", msg.PcUid, SubId);
                return;
            }
            //内容为空直接返回
            if (string.IsNullOrEmpty(msg.Words))
            {
                return;
            }
            // 判断是否被禁言 
            if (ZoneServerApi.now < player.SilenceTime)
            {
                player.SendSilenceInfo();
                return;
            }
            else
            {
                player.CheckOpenVoice();
            }

            if (player.CurrentMap == null)
            {
                Log.Warn("player {0} chat trumpet not in map ", msg.PcUid);
                return;
            }
            //TODO 喇叭使用限制
            if (!player.CheckLimitOpen(LimitType.ChatTrumpet))
            {
                Log.WarnLine("player {0} chat trumpet not open.", msg.PcUid);
                return;
            }

            //检查字长       
            int wordsLen = Api.wordChecker.GetWordLen(msg.Words);
            if (wordsLen > ChatLibrary.ChatTrumpetMaxCount)
            {
                Log.WarnLine("player {0} chat trumpet error, words len is {1} ", msg.PcUid, wordsLen);
                player.SendErrorCodeMsg(ErrorCode.LengthLimit);
                return;
            }

            int itemId = msg.Id;

            int id = ChatLibrary.GetTrumpetId(itemId);
            Data data = DataListManager.inst.GetData("ChatTrumpet", id);
            if (data == null)
            {
                Log.Warn("player {0} chat trumpet not find trumpet {1} in ChatTrumpet", msg.PcUid, id);
                return;
            }

            //检查是否可发送符合
            if (msg.UseItem)
            {
                //扣除喇叭
                if (itemId > 0)
                {
                    BaseItem item = player.BagManager.GetItem(MainType.Consumable, itemId);
                    if (item == null || item.PileNum <= 0)
                    {
                        //没有这类物品
                        Log.Warn("player {0} chat trumpet not find item {1}", msg.PcUid, itemId);
                        return;
                    }
                    else
                    {
                        item = player.DelItem2Bag(item, RewardType.NormalItem, 1, ConsumeWay.UseTrumpet);
                        if (item != null)
                        {
                            player.SyncClientItemInfo(item);
                        }
                    }
                }
                else
                {
                    Log.Warn("player {0}  chat trumpet  not find itemId {1} ", msg.PcUid, itemId);
                    return;
                }

            }
            else
            {
                int price = 0;

                bool isDiscount = false;
                string DiscountStartTime = data.GetString("discountStartDate");
                string DiscountEndTime = data.GetString("discountEndDate");
                //有打折起始时间和结束时间
                if (!string.IsNullOrEmpty(DiscountStartTime) && !string.IsNullOrEmpty(DiscountEndTime))
                {
                    //在打折时间范围内
                    DateTime discountStart = DateTime.Parse(DiscountStartTime);
                    DateTime discountEnd = DateTime.Parse(DiscountEndTime);
                    if (discountStart <= ZoneServerApi.now && ZoneServerApi.now <= discountEnd)
                    {
                        isDiscount = true;
                    }
                    else
                    {
                        isDiscount = false;
                    }
                }
                //有打折起始时间，没有结束时间
                else if (!string.IsNullOrEmpty(DiscountStartTime) && string.IsNullOrEmpty(DiscountEndTime))
                {
                    DateTime discountStart = DateTime.Parse(DiscountStartTime);
                    if (discountStart <= ZoneServerApi.now)
                    {
                        isDiscount = true;
                    }
                    else
                    {
                        isDiscount = false;
                    }
                }
                //没有打折起始时间
                else
                {
                    isDiscount = false;
                }

                if (isDiscount)
                {
                    price = data.GetInt("disprice");
                }
                else
                {
                    price = data.GetInt("price");
                }
                int currencyType = data.GetInt("currency");
                if (price > 0)
                {
                    //扣除货币
                    int curNum = player.GetCoins(currencyType);
                    if (curNum < price)
                    {
                        //传入错误数量参数
                        player.SendErrorCodeMsg(ErrorCode.DiamondNotEnough);
                        Log.Warn("player {0} use chat trumpet failed: coin {1} not enough, price {2}", msg.PcUid, curNum, price);
                        return;
                    }
                    else
                    {
                        //扣货币
                        player.DelCoins((CurrenciesType)currencyType, price, ConsumeWay.UseTrumpet, id.ToString());
                    }
                }
                else
                {
                    Log.Warn("player {0} chat trumpet not find costNum {1} ", msg.PcUid, price);
                    return;
                }
            }

            if (player.server.httpSensitiveHelper.CheckOpen == 1)
            {
                CheckSensitiveQuery_Text checkSensitive = new CheckSensitiveQuery_Text();
                ChatChannel chatChannel = ChatChannel.Speaker;
                var paramsArr1 = player.GetSensitiveTextCheckParameters(msg.Words, chatChannel.ToString(), ((int)chatChannel).ToString(), null);

                player.server.httpSensitiveHelper.PostTextAsync(paramsArr1, checkSensitive, () =>
                {
                    if (checkSensitive.Result != null)
                    {
                        if (checkSensitive.ErrorCode == 0)
                        {
                            if (checkSensitive.AdvertCheckedResult != 0) //,非0，依据confidence字段判断 
                            {
                                //广告行为
                                player.CheckSetSilence();
                                player.NotifyClientSensitiveWord();
                                return checkSensitive.Result;
                            }

                            if (checkSensitive.IsSensitive)
                            {
                                //敏感字
                                player.CheckSetSilence();
                                player.NotifyClientSensitiveWord();
                                return checkSensitive.Result;
                            }
                        }
                    }

                    if (player.server.http163Helper.CheckOpen == 1)
                    {
                        //----------
                        Http163Checker(msg, player, itemId);
                    }
                    else
                    {
                        SendChatTrumpet(msg, player, itemId);
                    }
                    return checkSensitive.Result;
                });
            }
            else
            {
                if (player.server.http163Helper.CheckOpen == 1)
                {
                    //----------
                    Http163Checker(msg, player, itemId);
                }
                else
                {
                    SendChatTrumpet(msg, player, itemId);
                }
            }
        }

        private void Http163Checker(MSG_GateZ_USE_CHAT_TRUMPET msg, PlayerChar player, int itemId)
        {
            if (player.server.http163Helper.CheckOpen == 1)
            {
                Check163Query_Text check = new Check163Query_Text();
                var paramsArr = player.Get163TextCheckParameters(Context163Type.Chat, msg.Words);
                player.server.http163Helper.PostTextAsync(paramsArr, check, () =>
                {
                    if (check.action == 0) //通过
                    {
                        SendChatTrumpet(msg, player, itemId);
                    }
                    else
                    {
                        //TODO:BOIL 1：嫌疑，2：不通过
                        player.CheckSetSilence();
                        player.NotifyClientSensitiveWord();
                    }
                    return check.Result;
                });
            }
            else
            {
                SendChatTrumpet(msg, player, itemId);
            }
        }

        private void SendChatTrumpet(MSG_GateZ_USE_CHAT_TRUMPET msg, PlayerChar player, int itemId)
        {
            msg.Words = RecordSensitiveWord(player, msg.Words);

            if (player.SpeakSensitiveWord)
            {
                player.SendChatTrumpet(itemId, msg.Words);
            }
            else
            {
                player.SendChatTrumpetToRelation(itemId, msg.Words);
            }

            MSG_ZGC_CHAT_TRUMPET_RESULT result = new MSG_ZGC_CHAT_TRUMPET_RESULT();
            result.Result = MSG_ZGC_CHAT_TRUMPET_RESULT.Types.RESULT.Success;
            player.Write(result);

            player.KomoeEventLogChatFlow((int)ChatChannel.Speaker, msg.Words, 0);
        }

        private void OnResponse_NearbyEmoji(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_NEARBY_EMOJI pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_NEARBY_EMOJI>(stream);
            Log.Write("player {0} nearby emoji {1}", pks.PcUid, pks.EmojiId);
            PlayerChar player = Api.PCManager.FindPc(pks.PcUid);
            if (player == null)
            {
                Log.Warn("player {0} nearby emoji not in gateid {1} pc list", pks.PcUid, SubId);
                return;
            }

            // 判断是否被禁言 
            if (ZoneServerApi.now < player.SilenceTime)
            {
                player.SendSilenceInfo();
                return;
            }
            else
            {
                player.CheckOpenVoice();
            }

            if (player.CurrentMap == null)
            {
                Log.Warn("player {0}  nearby emoji not in map ", pks.PcUid);
                return;
            }

            //检查是否可以发送
            if (player.CheckPersonSpeakTime())
            {
                //检查是否有这个表情
                Data data = DataListManager.inst.GetData("WorldEmoticon", pks.EmojiId);
                if (data == null)
                {
                    Log.WarnLine("player {0} NearbyEmoji is not find emoji {1}.", pks.PcUid, pks.EmojiId);
                    return;
                }

                MSG_ZGC_NEARBY_EMOJI msg = new MSG_ZGC_NEARBY_EMOJI();
                msg.EmojiId = pks.EmojiId;
                msg.InstanceId = player.InstanceId;

                //广播
                player.BroadCast(msg);
            }
            else
            {
                Log.Warn("player {0} NearbyEmoji time error, last time is {1}", player.Uid, player.PersonSpeakTime);
            }
        }

        private void OnResponse_TipOff(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_TIP_OFF msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_TIP_OFF>(stream);
            Log.Write("player {0} tip off player {1} type {2}", msg.SourceUid, msg.DestUid, msg.Type);
            PlayerChar player = Api.PCManager.FindPc(msg.SourceUid);
            if (player == null)
            {
                Log.Warn("player {0} tip off not in gateid {1} pc list", msg.SourceUid, SubId);
                return;
            }
            player.TipOff(msg.DestUid, msg.DestName, msg.Type, msg.Content, msg.Detail);          
        }

        private void OnResponse_ActivityChatBubble(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_ACTIVITY_CHAT_BUBBLE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_ACTIVITY_CHAT_BUBBLE>(stream);
            Log.Write("player {0} buy chat bubble {1}", msg.PcUid, msg.BubbleId);
            PlayerChar player = Api.PCManager.FindPc(msg.PcUid);
            if (player == null)
            {
                Log.Warn("player {0} ActivityChatBubble not find  pc", msg.PcUid);
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0}  ActivityChatBubble not in map ", msg.PcUid);
                return;
            }

            player.ActivityChatFrame(msg.BubbleId);
        }

        
        private void OnResponse_CheckChatLimit(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_CHECK_CHATLIMIT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_CHECK_CHATLIMIT>(stream);
            //Log.Write("player {0} check chat channel {1} limit", msg.PcUid, msg.ChatChannel);
            PlayerChar player = Api.PCManager.FindPc(msg.PcUid);
            if (player == null)
            {
                Log.Warn("player {0} CheckChatLimit not find  pc", msg.PcUid);
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0}  CheckChatLimit not in map ", msg.PcUid);
                return;
            }
            //player.CheckChatLimitOpen(msg.ChatChannel);
            if (player.CheckChatLimitOpen(msg.ChatChannel))
            {
                MSG_ZGC_CHECK_CHATLIMIT checkMsg = new MSG_ZGC_CHECK_CHATLIMIT();
                checkMsg.Result = MSG_ZGC_CHECK_CHATLIMIT.Types.RESULT.Success;
                checkMsg.ChatChannel = msg.ChatChannel;
                player.Write(checkMsg);
            }
        }       

        private void OnResponse_BuyChatTrumpet(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_BUY_TRUMPET msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_BUY_TRUMPET>(stream);
            Log.Write("player {0} buy chat trumpet {1} num {2}", msg.PcUid, msg.ItemId, msg.Num);
            PlayerChar player = Api.PCManager.FindPc(msg.PcUid);
            if (player != null)
            {
                player.BuyChatTrumpet(msg.ItemId, msg.Num);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(msg.PcUid);
                if (player != null)
                {
                    Log.WarnLine("buy item fail, player {0} is offline.", msg.PcUid);
                }
                else
                {
                    Log.WarnLine("buy item fail, can not find player {0} .", msg.PcUid);
                }
            }
        }

        private void OnResponse_ClearBubbleRedPoint(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_CLEAR_BUBBLE_REDPOINT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_CLEAR_BUBBLE_REDPOINT>(stream);
            Log.Write("player {0} clear bubble {1} red point", uid, msg.ItemId);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} ClearBubbleRedPoint not find  pc", uid);
                return;
            }
            if (player.CurrentMap == null)
            {
                Log.Warn("player {0}  ClearBubbleRedPoint not in map ", uid);
                return;
            }

            player.ClearBubbleRedPoint(msg.ItemId);
        }

        private string FilterBadWord(string content)
        {
            Regex reg = new Regex(@"\[[0-9a-fA-F]{3,6}\]", RegexOptions.Compiled);
            content = reg.Replace(content, "");
            content = Api.wordChecker.FilterBadWord(content);
            return content;
        }
    }
}
