using ServerShared;
using System;
using System.Collections.Generic;
using CommonUtility;
using ServerModels;
using EnumerateUtility;
using Logger;
using RedisUtility;
using DataProperty;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZGate;
using Message.Gate.Protocol.GateZ;
using System.IO;
using Message.Zone.Protocol.ZR;
using DBUtility;
using Google.Protobuf.Collections;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        //聊天
        public int GetChatFrame()
        {
            return BagManager.ChatFrameBag.CurChatFrameId;
        }
        /// <summary>
        /// 禁言时间
        /// </summary>
        public DateTime SilenceTime = DateTime.MinValue;
        /// <summary>
        /// 禁言原因
        /// </summary>
        public string SilenceReason = string.Empty;

        //public DateTime NearbySpeakTime = ZoneServerApi.now;
        public DateTime FamilySpeakTime = ZoneServerApi.now;
        public DateTime WorldSpeakTime = ZoneServerApi.now;
        public DateTime PersonSpeakTime = ZoneServerApi.now;
        public DateTime CampSpeakTime = ZoneServerApi.now;
        public DateTime TeamSpeakTime = ZoneServerApi.now;
        public DateTime RecruitSpeakTime = ZoneServerApi.now;     

        public MSG_ZGC_CHAT_LIST ChatList = new MSG_ZGC_CHAT_LIST();
        private DateTime LastSendChatTime = ZoneServerApi.now;

        public bool SpeakSensitiveWord = false;

        private DateTime lastSaySensWordTime = DateTime.MinValue;
        private int saySensWordCount = 0;

        public void SendWorldChat(MSG_GateZ_CHAT chat)
        {
            MSG_ZGC_CHAT_INFO msg = new MSG_ZGC_CHAT_INFO();

            msg.PcInfo = PlayerInfo.GetChatPlayerInfo(this);

            msg.ChatChannel = chat.ChatChannel;
            msg.Content = chat.Content;
            msg.EmojiId = chat.EmojiId;
            msg.Time= Timestamp.GetUnixTimeStampSeconds(ZoneServerApi.now);
            msg.ChatFrameId = GetChatFrame();
            msg.SensitiveWord = SpeakSensitiveWord;
            //GateBroadcast(msg);
            server.ChatMng.AddBroadcastChat(msg);
        }

        public void SendPersonSystemOnlineChat(int toUid,int systemChatId)
        {
            MSG_ZGC_CHAT_INFO chatInfo = GetPersonSystemChatMsg(systemChatId);
            // 查找给person是否在当前zone 在的话直接发
            PlayerChar person = server.PCManager.FindPc(toUid);
            if (person != null)
            {
                if (person.CheckBlackExist(Uid))
                {
                    MSG_ZGC_CHAT response = new MSG_ZGC_CHAT()
                    {
                        Result = (int)ErrorCode.InTargetBlack,
                        ToUid = toUid
                    };
                    Write(response);
                    return;
                    //SendErrorCodeMsg(ErrorCode.InTargetBlack);
                }
                person.AddChat(chatInfo);
            }
            else
            {
                MSG_ZR_CHAT chat = new MSG_ZR_CHAT();
                chat.SpeakerInfo = PlayerInfo.GetChatSpeakerInfo(this);
                chat.ChatChannel = chatInfo.ChatChannel;
                chat.Content = chatInfo.Content;
                chat.Param = toUid;
                chat.Time = Timestamp.GetUnixTimeStampSeconds(ZoneServerApi.now);
                chat.ChatFrameId = GetChatFrame();
                      
                SendChatToRelation(chat);
            }
        }

        public void SendPersonSystemOfflineChat(int toUid, int systemChatId)
        {
            MSG_ZGC_CHAT_INFO chatInfo = GetPersonSystemChatMsg(systemChatId);
            //不在线发离线消息
            SendPersonOfflineMsg(toUid, chatInfo);
        }

        public void SendPersonChat(int toUid, MSG_ZGC_CHAT_INFO chatInfo)
        {
            MSG_ZGC_CHAT response = new MSG_ZGC_CHAT();
            response.ToUid = toUid;

            // 查找给person是否在当前zone 在的话直接发
            PlayerChar person = server.PCManager.FindPc(toUid);
            if (person != null)
            {
                if (person.CheckBlackExist(Uid))
                {
                    response.Result = (int)ErrorCode.InTargetBlack;
                    Write(response);
                    return;
                    //SendErrorCodeMsg(ErrorCode.InTargetBlack);
                }
                response.Result = (int)ErrorCode.Success;
                Write(response);

                person.AddChat(chatInfo);
            }
            else
            {
                //不在的当前zone
                OperateCheckBlackList checker = new OperateCheckBlackList(toUid, Uid);
                server.GameRedis.Call(checker, inBlack =>
                {
                    if ((int)inBlack == 1)
                    {
                        MSG_ZGC_CHAT res1 = new MSG_ZGC_CHAT();
                        res1.ToUid = toUid;
                        if (checker.Exist)
                        {
                            res1.Result = (int)ErrorCode.InTargetBlack;
                            Write(res1);
                            return;
                        }

                        //看这个人在不在线
                        OperateGetOnlineState onlineStat = new OperateGetOnlineState(toUid);
                        server.GameRedis.Call(onlineStat, getOnline =>
                        {
                            if ((int)getOnline == 1)
                            {
                                MSG_ZGC_CHAT res2 = new MSG_ZGC_CHAT();
                                res1.ToUid = toUid;
                                if (onlineStat.IsOnline)// 在线就去relation找其他zone
                                {
                                    //在线转给Relation
                                    MSG_ZR_CHAT chat = new MSG_ZR_CHAT();
                                    chat.SpeakerInfo = PlayerInfo.GetChatSpeakerInfo(this);
                                    chat.ChatChannel = chatInfo.ChatChannel;
                                    chat.Content = chatInfo.Content;
                                    chat.Param = toUid;
                                    chat.Time = Timestamp.GetUnixTimeStampSeconds(ZoneServerApi.now);
                                    chat.ChatFrameId = GetChatFrame();

                                    SendChatToRelation(chat);

                                    res2.Result = (int)ErrorCode.Success;
                                    Write(res2);
                                    return;
                                }
                                else
                                {
                                    //不在线发离线消息
                                    SendPersonOfflineMsg(toUid, chatInfo);

                                    res2.Result = (int)ErrorCode.ChatTargetOffline;
                                    Write(res2);
                                    return;
                                }
                            }
                        });

                        return;
                    }
                });
              
            }
        }

        public void SendPersonChat(MSG_GateZ_CHAT msg)
        {
            int toUid = msg.Param;
            if (toUid == Uid || toUid <= 0)
            {
                Log.Error("player {0} send person chat fail,can not send to self", Uid);
                return;
            }

            if (CheckBlackExist(toUid))
            {
                MSG_ZGC_CHAT response = new MSG_ZGC_CHAT()
                {
                    Result = (int)ErrorCode.InBlack,
                    ToUid = toUid
                };
                Write(msg);
                //SendErrorCodeMsg(ErrorCode.InBlack);
                Log.Warn("player {0} send person chat fail, in {1} black list", Uid, toUid);
                return;
            }

            if (CheckPersonSpeakTime())
            {
                MSG_ZGC_CHAT_INFO chatInfo = GetChatMsg(msg);
                SendPersonChat(toUid, chatInfo);
            }
            else
            {
                Log.Warn("player {0} chat {1} time error, last time is {2}", Uid, msg.ChatChannel, PersonSpeakTime);
            }
        }

        private void SendChatToRelation(MSG_ZR_CHAT chat)
        {
            server.RelationServer.AddChat(Uid, chat);
        }

        public void SendPersonOnlineMsg(int toUid, MSG_ZGC_CHAT_INFO chatinfo)
        {

        }

        public void SendPersonOfflineMsg(int toUid ,MSG_ZGC_CHAT_INFO chatinfo)
        {
            //SendErrorCodeMsg(ErrorCode.ChatTargetOffline);


            MemoryStream stream = new MemoryStream();
            MessagePacker.ProtobufHelper.Serialize(stream, chatinfo);
            //离线消息
            server.GameRedis.Call(new OperateSetSaveChatInfo(toUid, chatinfo.Time, stream));
        }

        //public void SendPersonChatWords(int type, int toUid, string words, int emojiId, string param, bool needSave)
        //{

        //    words = FilterBadWord(words);

        //    int time = Timestamp.GetUnixTimeStampSeconds(ZoneServerApi.now);

        //    MSG_ZGC_PERSON_CHAT_WORDS returnMsg = new MSG_ZGC_PERSON_CHAT_WORDS();
        //    returnMsg.EmojiId = emojiId;
        //    returnMsg.Words = words;
        //    returnMsg.Type = type;
        //    returnMsg.ToUid = toUid;
        //    returnMsg.PcInfo = PlayerInfo.GetChatPlayerInfo(this, false);
        //    returnMsg.Time = time;
        //    returnMsg.Param = param;
        //    PlayerChar toPc = server.PCManager.FindPc(toUid);
        //    if (toPc == null)
        //    {
        //        //获取UID Main ID
        //        OperateGetOnlineState onlineStat = new OperateGetOnlineState(toUid);
        //        server.Redis.Call(onlineStat, getOnline =>
        //        {
        //            if ((int)getOnline == 1)
        //            {
        //                if (onlineStat.IsOnline)// 在线就去relation找其他zone
        //                {
        //                    MSG_ZR_PERSON_CHAT_WORDS msg = new MSG_ZR_PERSON_CHAT_WORDS();
        //                    msg.PcInfo = PlayerInfo.GetZRPersonChatPlayerInfo(this);
        //                    msg.Type = type;
        //                    msg.ToUid = toUid;
        //                    msg.Words = words;
        //                    msg.EmojiId = emojiId;
        //                    msg.Param = param;
        //                    msg.MainId = onlineStat.MainId;
        //                    server.SendToRelation(msg);
        //                    return;
        //                }
        //                else
        //                {
        //                    if (needSave)
        //                    {
        //                        SendErrorCodeMsg(ErrorCode.TargetOffline);

        //                        MemoryStream stream = new MemoryStream();
        //                        MessagePacker.ProtobufHelper.Serialize(stream, returnMsg);
        //                        //离线消息
        //                        server.Redis.Call(new OperateSetSaveChatInfo(toUid, time, stream));
        //                        return;
        //                    }
        //                }
        //            }
        //        });
        //    }
        //    else
        //    {
        //        if (toPc.CheckBlackExist(Uid))
        //        {
        //            SendErrorCodeMsg(ErrorCode.InTargetBlack);
        //        }
        //        else
        //        {
        //            toPc.Write(returnMsg);
        //        }
        //    }
        //    Write(returnMsg);
        //}

        //private string FilterBadWord(string words)
        //{
        //    if (IsGm == 0)
        //    {
        //        Regex reg = new Regex(@"\[[0-9a-fA-F]{3,6}\]", RegexOptions.Compiled);
        //        words = reg.Replace(words, "");
        //        words = server.wordChecker.FilterBadWord(words);
        //    }
        //    return words;
        //}

        public bool CheckWorldSpeakTime()
        {
            double time = (ZoneServerApi.now - WorldSpeakTime).TotalSeconds;
            int intervalTime = ChatLibrary.GetChatIntervalTimeByLevel(Level);
            if (time >= intervalTime)
            {
                WorldSpeakTime = ZoneServerApi.now;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CheckFamilySpeakTime()
        {
            double time = (ZoneServerApi.now - FamilySpeakTime).TotalSeconds;
            if (time >= ChatLibrary.FamilyIntervalTime)
            {
                FamilySpeakTime = ZoneServerApi.now;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CheckPersonSpeakTime()
        {
            double time = (ZoneServerApi.now - PersonSpeakTime).TotalSeconds;
            if (time >= ChatLibrary.PersonIntervalTime)
            {
                PersonSpeakTime = ZoneServerApi.now;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CheckCampSpeakTime()
        {
            double time = (ZoneServerApi.now - CampSpeakTime).TotalSeconds;
            if (time >= ChatLibrary.CampIntervalTime)
            {
                CampSpeakTime = ZoneServerApi.now;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CheckTeamSpeakTime()
        {
            double time = (ZoneServerApi.now - TeamSpeakTime).TotalSeconds;
            if (time >= ChatLibrary.TeamIntervalTime)
            {
                TeamSpeakTime = ZoneServerApi.now;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CheckRecruitSpeakTime()
        {
            double time = (ZoneServerApi.now - RecruitSpeakTime).TotalSeconds;
            if (time >= ChatLibrary.RecruitIntervalTime)
            {
                RecruitSpeakTime = ZoneServerApi.now;
                return true;
            }
            else
            {
                return false;
            }
        }

        private void GetSaveChatInfo()
        {
            OperateGetSaveChatInfo operate = new OperateGetSaveChatInfo(Uid);

            server.GameRedis.Call(operate, (ret) =>
            {
                if (operate.ChatInfos.Count > 0)
                {
                    int takeHeartMsgCount = 0;

                    foreach (var info in operate.ChatInfos)
                    {
                        Byte[] ms = new MemoryStream(Convert.FromBase64String(info)).ToArray();
                        MSG_ZGC_CHAT_INFO returnMsg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZGC_CHAT_INFO>(ms);

                        AddChat(returnMsg);
                        //离线消息里把收获爱心的单独摘出来特殊处理。
                        if (returnMsg.ChatChannel == (int)ChatChannel.PersonSystem)
                        {
                            if (returnMsg.Content == (FriendLib.TakeHeartPersonSystemMsgId).ToString())
                            {
                                takeHeartMsgCount++;
                            }
                        }
                    }
                    if (takeHeartMsgCount>0)
                    {
                        //离线消息里收获爱心,这里更新爱心货币
                        AddFriendHeartOfflineTake(takeHeartMsgCount);
                    }
                    server.GameRedis.Call(new OperateDeleteSaveChatInfo(Uid));
                }
            });
        }

        public void SendChatTrumpet(int itemId, string words)
        {
            MSG_ZGC_CHAT_TRUMPET msg = new MSG_ZGC_CHAT_TRUMPET();
            msg.PcInfo = PlayerInfo.GetChatPlayerInfo(this);
            msg.ItemId = itemId;
            msg.Words = words;
            msg.Time = Timestamp.GetUnixTimeStampSeconds(ZoneServerApi.now);
            msg.ChatFrameId = GetChatFrame();
            msg.MainId = this.MainId;
            msg.SensitiveWord = true;
            server.ChatMng.AddBroadcastTrumpet(msg);
        }

        public void SendChatTrumpetToRelation(int itemId, string words)
        {
            MSG_ZR_CHAT_TRUMPET msg = new MSG_ZR_CHAT_TRUMPET();
            msg.MainId = this.MainId;
            msg.ItemId = itemId;
            msg.Words = words;
            msg.PcInfo = PlayerInfo.GetChatSpeakerInfo(this);
            msg.ChatFrameId = GetChatFrame();
            server.RelationServer.Write(msg, Uid);
        }

        // 公告

        public void UseChatFrame(ChatFrameItem item)
        {
            //把当气泡 激活状态设为0 表示非使用
            if (item.Id == BagManager.ChatFrameBag.CurChatFrameId)
            {
                //重复装备同一件物品,不做更改
                return;
            }
            //超过有效期不允许使用
            if (!CheckCanUseChatFrame(item))
            {
                return;
            }
            //更新到客户端
            List<BaseItem> updateList = new List<BaseItem>();

            ChatFrameItem curChatFrame = BagManager.ChatFrameBag.GetItem(BagManager.ChatFrameBag.CurChatFrameId) as ChatFrameItem;
            if (curChatFrame != null)
            {
                curChatFrame.ActivateState = 0;
                BagManager.ChatFrameBag.UpdateItem(curChatFrame);
                updateList.Add(curChatFrame);
            }
            //设置气泡框  激活状态设为1 表示启用
            item.ActivateState = 1;
            BagManager.ChatFrameBag.UpdateItem(item);
            //最后
            BagManager.ChatFrameBag.CurChatFrameId = item.Id;

            updateList.Add(item);
            SyncClientItemsInfo(updateList);

            //更新到redis
            //server.Redis.Call(new OperateSetChatFrame(uid, BagManager.ChatFrameBag.CurChatFrameId));
        }

        //购买气泡框
        public void ActivityChatFrame(int itemId)
        {
            MSG_ZGC_ACTIVITY_CHAT_BUBBLE msg = new MSG_ZGC_ACTIVITY_CHAT_BUBBLE();
            msg.Result = (int)ErrorCode.Success;

            //检查是否有这个气泡
            int bubbleId = ChatLibrary.GetBubbleId(itemId);
            Data bubbleData = DataListManager.inst.GetData("ChatBubble", bubbleId);
            if (bubbleData == null)
            {
                Log.WarnLine("player {0} buy ChatBubble fail ,can not find bubble {1} in xml.", Uid, itemId);
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }
            //if (bubbleData.Type != ChatBubbleType.Buy)
            //{
            //    Log.WarnLine("player {0} ActivityChatBubble type is {1}.", Uid, bubbleData.Type);
            //    msg.Result = MSG_ZGC_ACTIVITY_CHAT_BUBBLE.Types.RESULT.Error;
            //    Write(msg);
            //    return;
            //}              

            Data data = BagLibrary.GetItemModelData(itemId);
            if (data == null)
            {
                Log.WarnLine("player {0} ActivityChatBubble fail ,can not find item {1} in xml", Uid, itemId);
                msg.Result = (int)ErrorCode.NotFoundItem;
                Write(msg);
                return;
            }
          
            //检查是否有售卖开始日期和截止日期
            string startDateStr = bubbleData.GetString("startDate");
            string endDateStr = bubbleData.GetString("endDate");
           
            if (startDateStr != null && endDateStr != null && startDateStr != "" && endDateStr != "")
            {
                DateTime startDate = DateTime.Parse(startDateStr);
                DateTime endDate = DateTime.Parse(endDateStr);
                if (startDate > ZoneServerApi.now || endDate < ZoneServerApi.now)
                {
                    Log.Warn("player {0} buy chatBubble fail , startDate is {1}, endDate is {2}, now is {3}", Uid, startDate, endDate, ZoneServerApi.now);
                    msg.Result = (int)ErrorCode.NotOnSale;
                    Write(msg);
                    return;
                }
            }

            //检查通行证等级
            if (PassLevel < bubbleData.GetInt("passLevel"))
            {
                Log.WarnLine("player {0} ActivityChatBubble fail , curPassLevel is {1}", Uid, PassLevel);
                msg.Result = (int)ErrorCode.PassLevelNotEnough;
                Write(msg);
                return;
            }

            bool hasItem = false;
            ChatFrameItem curChatFrame = BagManager.ChatFrameBag.GetItem(itemId) as ChatFrameItem;
            if (curChatFrame != null)
            {
                hasItem = true;
            }

            int price = 0;

            bool isDiscount = false;
            int discount = bubbleData.GetInt("discount");
            if (discount > 0)
            {
                isDiscount = true;
            }
            //是否打折
            if (isDiscount)
            {
                price = bubbleData.GetInt("disprice");
            }
            else
            {
                price = bubbleData.GetInt("price");
            }            

            int coinType = bubbleData.GetInt("currency");
            if (coinType > 0 && price > 0)
            {
                int curCoin = GetCoins(coinType);
                if (curCoin < price)
                {
                    //传入错误数量参数
                    Log.WarnLine("player {0} ActivityChatBubble fail , curCoin is {1}", Uid, curCoin);
                    msg.Result = (int)ErrorCode.DiamondNotEnough;
                    Write(msg);
                    return;
                }
                else
                {
                    //扣货币
                    DelCoins((CurrenciesType)coinType, price, ConsumeWay.ItemBuy, itemId.ToString());
                    List<BaseItem> item = new List<BaseItem>();
                    if (hasItem)
                    {
                        //更新气泡框获取时间和红点状态
                        BagManager.ChatFrameBag.UpdateItemObtainInfo(curChatFrame);
                        item.Add(curChatFrame);
                    }
                    else
                    {
                        //添加物品
                        item = AddItem2Bag(MainType.ChatFrame, RewardType.ChatFrame, itemId, 1, ObtainWay.ItemBuy);
                    }
                   
                    ////购买统计
                    //AddItemStat(item);
                    SyncClientItemInfo(item[0]);
                    Write(msg);
                }
            }
            else if (coinType > 0 && price == 0)
            {
                List<BaseItem> item = new List<BaseItem>();
                if (hasItem)
                {
                    //更新气泡框获取时间
                    BagManager.ChatFrameBag.UpdateItemObtainInfo(curChatFrame);
                    item.Add(curChatFrame);
                }
                else
                {
                    //添加物品
                    item = AddItem2Bag(MainType.ChatFrame, RewardType.ChatFrame, itemId, 1, ObtainWay.Activity);
                }
                SyncClientItemInfo(item[0]);
                Write(msg);
            }
            else
            {
                Log.WarnLine("player {0} ActivityChatBubble fail , coinType is {1} , sellingPrice is {2}", Uid, coinType, price);
                msg.Result = (int)ErrorCode.Fail;
                Write(msg);
                return;
            }
        }

        /// <summary>
        /// 禁言
        /// </summary>
        public void SendSilenceInfo()
        {
            MSG_ZGC_SILENCE notify = new MSG_ZGC_SILENCE();
            notify.Time = SilenceTime.ToString(CONST.DATETIME_TO_STRING_1);
            if (string.IsNullOrEmpty(SilenceReason))
            {
                notify.Reason = ChatLibrary.DefaultSilenceReason;
            }
            else
            {
                notify.Reason = SilenceReason;
            }
            Write(notify);
        }

        public MSG_ZGC_CHAT_INFO GetChatMsg(MSG_GateZ_CHAT msg)
        {
            MSG_ZGC_CHAT_INFO chat = new MSG_ZGC_CHAT_INFO();
            if (msg == null)
            {
                return chat;
            }
            chat.PcInfo = PlayerInfo.GetChatPlayerInfo(this);

            chat.ChatChannel = msg.ChatChannel;
            chat.EmojiId = msg.EmojiId;
            chat.Content = msg.Content;
            chat.Time = Timestamp.GetUnixTimeStampSeconds(ZoneServerApi.now);
            chat.ChatFrameId = GetChatFrame();
            return chat;
        }

        public MSG_ZGC_CHAT_INFO GetPersonSystemChatMsg(int systemChatMsgId)
        {
            MSG_ZGC_CHAT_INFO chat = new MSG_ZGC_CHAT_INFO();
            chat.PcInfo = PlayerInfo.GetChatPlayerInfo(this);
            chat.Time = Timestamp.GetUnixTimeStampSeconds(ZoneServerApi.now);
            chat.ChatFrameId = GetChatFrame();
            chat.ChatChannel = (int)ChatChannel.PersonSystem;
            chat.EmojiId = 0;
            chat.Content = systemChatMsgId.ToString();
            return chat;
        }

        public void AddChat(MSG_ZGC_CHAT_INFO chat)
        {
            ChatList.List.Add(chat);
            
            if (ChatList.List.Count > CONST.CHATINFO_PER_PKG_COUNT)
            {
                Write(ChatList);
                ChatList.List.Clear();
                LastSendChatTime = ZoneServerApi.now;
            }
        }

        public void UpdateChat()
        {
            if (ChatList.List.Count > 0)
            {
                if ((ZoneServerApi.now - LastSendChatTime).TotalMilliseconds > 500 || ChatList.List.Count > CONST.CHATINFO_PER_PKG_COUNT)
                {
                    Write(ChatList);
                    ChatList.List.Clear();
                    LastSendChatTime = ZoneServerApi.now;
                }
            }
        }

        public bool CheckChatLimitOpen(int chatChannel)
        {
            MSG_ZGC_CHECK_CHATLIMIT msg = new MSG_ZGC_CHECK_CHATLIMIT();
            msg.Result = MSG_ZGC_CHECK_CHATLIMIT.Types.RESULT.Success;
            msg.ChatChannel = chatChannel;
            switch ((ChatChannel)chatChannel)
            {
                case ChatChannel.System:
                case ChatChannel.Recruit:
                    Log.WarnLine("player {0} chatChannel {1} not support chat.", Uid, chatChannel);
                    msg.Result = MSG_ZGC_CHECK_CHATLIMIT.Types.RESULT.Error;
                    msg.ReasonId = (int)ErrorCode.NoChat;
                    msg.ChatChannel = chatChannel;
                    Write(msg);
                    return false;
                case ChatChannel.Team:
                    if (Team == null)
                    {
                        Log.WarnLine("player {0} chatChannel {1} not have team.", Uid, chatChannel);
                        msg.Result = MSG_ZGC_CHECK_CHATLIMIT.Types.RESULT.Error;
                        msg.ReasonId = (int)ErrorCode.NoTeam;
                        msg.ChatChannel = chatChannel;
                        Write(msg);
                        return false;
                    }
                    break;
                case ChatChannel.Camp:
                    if (Camp == CampType.None)
                    {
                        Log.WarnLine("player {0} chatChannel {1} not have camp.", Uid, chatChannel);
                        msg.Result = MSG_ZGC_CHECK_CHATLIMIT.Types.RESULT.Error;
                        msg.ReasonId = (int)ErrorCode.NoCamp;
                        msg.ChatChannel = chatChannel;
                        Write(msg);
                        return false;
                    }
                    break;
                case ChatChannel.Family:
                    if (FamilyId == 0)
                    {
                        Log.WarnLine("player {0} chatChannel {1} not have family.", Uid, chatChannel);
                        msg.Result = MSG_ZGC_CHECK_CHATLIMIT.Types.RESULT.Error;
                        msg.ReasonId = (int)ErrorCode.NoFamily;
                        msg.ChatChannel = chatChannel;
                        Write(msg);
                        return false;
                    }
                    break;
                default:
                    break;
            }
            if (chatChannel == (int)ChatChannel.Person)
            {
                if (!CheckLimitOpen(LimitType.PersonChat))
                {
                    Log.WarnLine("player {0} chatChannel {1} level is not enough, curLevel is {2}.", Uid, chatChannel, Level);
                    msg.Result = MSG_ZGC_CHECK_CHATLIMIT.Types.RESULT.Error;
                    msg.ReasonId = (int)ErrorCode.ChatLimit;
                    msg.ChatChannel = chatChannel;
                    Write(msg);
                    return false;
                }
            }
            else
            {
                if (!CheckLimitOpen(LimitType.WorldChat))
                {
                    Log.WarnLine("player {0} chatChannel {1} level is not enough, curLevel is {2}.", Uid, chatChannel, Level);
                    msg.Result = MSG_ZGC_CHECK_CHATLIMIT.Types.RESULT.Error;
                    msg.ReasonId = (int)ErrorCode.ChatLimit;
                    msg.ChatChannel = chatChannel;
                    Write(msg);
                    return false;
                }
            }
           
            //Write(msg);
            return true;
        }

        public bool CheckActivation()
        {
            return true;
        }

        public void BuyChatTrumpet(int itemId, int num)
        {
            MSG_ZGC_BUY_TRUMPET response = new MSG_ZGC_BUY_TRUMPET();
            response.ItemId = itemId;
            if (num <= 0)
            {
                //传入错误数量参数
                Log.Warn("player {0} buy trumpet {1} got a wrong num {1}", Uid, itemId, num);
                response.Result = (int)ErrorCode.NumIsZero;
                Write(response);
                return;
            }

            int trumpetId = ChatLibrary.GetTrumpetId(itemId);
            Data data = DataListManager.inst.GetData("ChatTrumpet", trumpetId);
            if (data == null)
            {
                Log.Warn("player {0} buy trumpet fail ,can not find trumpet {1} in xml", Uid, trumpetId);
                response.Result = (int)ErrorCode.NotFoundItem;
                Write(response);
                return;
            }
            else
            {
                //检查是否有售卖开始日期和截止日期                
               
                if (data.GetString("startDate") != null && data.GetString("endDate") != null && data.GetString("startDate") !="" && data.GetString("endDate") != "")
                {
                    DateTime startDate = DateTime.Parse(data.GetString("startDate"));
                    DateTime endDate = DateTime.Parse(data.GetString("endDate"));
                    if (startDate > ZoneServerApi.now || endDate < ZoneServerApi.now)
                    {
                        Log.Warn("player {0} buy trumpet fail , startDate is {1}, endDate is {2}, now is {3}", Uid, startDate, endDate, ZoneServerApi.now);
                        response.Result = (int)ErrorCode.NotOnSale;
                        Write(response);
                        return;
                    }
                }

                //检查通行证等级
                if (PassLevel < data.GetInt("passLevel"))
                {
                    Log.Warn("player {0} buy trumpet fail , curPassLevel is {1}", Uid, PassLevel);
                    response.Result = (int)ErrorCode.PassLevelNotEnough;
                    Write(response);
                    return;
                }

                //单次最大购买量99
                if (num > data.GetInt("maxNum"))
                {
                    Log.Warn("player {0} buy trumpet fail , num is {1}, maxNum is {2}", Uid, num, data.GetInt("maxNum"));
                    response.Result = (int)ErrorCode.MaxCount;
                    Write(response);
                    return;
                }

                //背包已满,且背包中没有这种喇叭
                Bag_Normal bag = (Bag_Normal)BagManager.GetBag(MainType.Consumable);
                BaseItem item = bagManager.NormalBag.GetItem(itemId);
                if (bag.Manager.GetBagRestSpace() <= 0 && item == null)
                {
                    Log.Warn("BadPacket: player {0} maxbagspace curr space {1}", Uid, BagSpace);
                    response.Result = (int)ErrorCode.MaxBagSpace;
                    Write(response);
                    return;
                }

                int price = 0;

                bool isDiscount = false;
                int discount = data.GetInt("discount");
                if (discount > 0)
                {
                    isDiscount = true;
                }
                //是否打折
                if (isDiscount)
                {
                    price = data.GetInt("disprice");
                }
                else
                {
                    price = data.GetInt("price");
                }
                int currencyType = data.GetInt("currency");
                int sellingPrice = price * num;
                if (price > 0)
                {
                    //扣除货币
                    int curNum = GetCoins(currencyType);
                    if (curNum < price)
                    {
                        //传入错误数量参数
                        Log.Warn("player {0} buy trumpet {1} not have enough diamond, curNum is {2}, price is {3}", Uid, itemId, curNum, price);
                        response.Result = (int)ErrorCode.DiamondNotEnough;
                        Write(response);
                        return;
                    }
                    else
                    {
                        //扣货币
                        DelCoins((CurrenciesType)currencyType, sellingPrice, ConsumeWay.ItemBuy, trumpetId.ToString());
                        //添加物品
                        List<BaseItem> items = AddItem2Bag(MainType.Consumable, RewardType.NormalItem, itemId, num, ObtainWay.ItemBuy);
                        response.Result = (int)ErrorCode.Success;

                        SyncClientItemsInfo(items);
                        Write(response);
                    }
                }
                //else if (price == 0)
                //{
                //    //添加物品
                //    List<BaseItem> items = AddItem2Bag(MainType.Consumable, itemId, num, ObtainWay.Activity);
                //    response.Result = (int)ErrorCode.Success;

                //    SyncClientItemsInfo(items);
                //    Write(response);
                //}
                else
                {
                    Log.Warn("player {0} chat trumpet not find costNum {1} ", Uid, price);
                    return;
                }
            }

        }

        public void CheckCurChatFrame(bool needSync = true)
        {
            ChatFrameItem curChatFrame = BagManager.ChatFrameBag.GetItem(BagManager.ChatFrameBag.CurChatFrameId) as ChatFrameItem;
            if (curChatFrame != null)
            {
                if (!CheckCanUseChatFrame(curChatFrame))
                {
                    //更新到客户端
                    List<BaseItem> updateList = new List<BaseItem>();
                    //重设当前气泡框
                    curChatFrame.ActivateState = 0;
                    BagManager.ChatFrameBag.UpdateItem(curChatFrame);
                    updateList.Add(curChatFrame);

                    ChatFrameItem chatFrame = BagManager.ChatFrameBag.GetItem(CharacterInitLibrary.ChatFrame) as ChatFrameItem;
                    chatFrame.ActivateState = 1;
                    BagManager.ChatFrameBag.UpdateItem(chatFrame);
                    BagManager.ChatFrameBag.CurChatFrameId = chatFrame.Id;
                    updateList.Add(chatFrame);

                    SyncClientItemsInfo(updateList);
                }     
            }
            //通知红点
            if (needSync)
            {
                BagManager.ChatFrameBag.NotifyNewBubbleList();
            }
        }

        private bool CheckCanUseChatFrame(ChatFrameItem chatFrame)
        {
            int expiryDate = chatFrame.Model.Data.GetInt("expiryDate");
            if (expiryDate > 0)
            {
                DateTime generateTime = Timestamp.TimeStampToDateTime(chatFrame.GenerateTime);
                int days = (int)(server.Now() - generateTime).TotalDays;
                if (days >= expiryDate)
                {
                    return false;
                }
            }
            return true;
        }

        public void ClearBubbleRedPoint(int itemId)
        {
            BagManager.ChatFrameBag.UpdateItemNewObtainState(itemId);
        }

        public void NotifyClientSensitiveWord()
        {
            MSG_ZGC_SENSITIVE_WORD notify = new MSG_ZGC_SENSITIVE_WORD();
            notify.Result = (int)ErrorCode.BadWord;
            Write(notify);
        }

        public void CheckSetSilence()
        {
            if (lastSaySensWordTime == DateTime.MinValue)
            {
                lastSaySensWordTime = ZoneServerApi.now;
            }
            if ((ZoneServerApi.now - lastSaySensWordTime).TotalSeconds <= ChatLibrary.SensitiveWordDuration)
            {
                saySensWordCount++;
            }
            if (saySensWordCount >= ChatLibrary.SensitiveWordCountLimit)
            {
                SilenceTime = ZoneServerApi.now.AddMinutes(ChatLibrary.SensitiveWordSilenceTime);
                SilenceReason = ChatLibrary.SensitiveWordSilenceReason;
                server.GameDBPool.Call(new QueryUpdateSilenceTime(Uid, SilenceTime.ToString(), SilenceReason));
            }
        }

        public void CheckOpenVoice()
        {
            if (string.IsNullOrEmpty(SilenceReason))
            {
                return;
            }
            SilenceTime = DateTime.MinValue;
            SilenceReason = "";
            server.GameDBPool.Call(new QueryUpdateSilenceTime(Uid, SilenceTime.ToString(), SilenceReason));
        }

        //举报
        public void TipOff(int destUid, string destName, int type, RepeatedField<string> content, string description)
        {
            MSG_ZGC_TIP_OFF response = new MSG_ZGC_TIP_OFF();
            response.DestUid = destUid;

            //List<int> typeList = ChatLibrary.GetTipOffTypeList();
            //if (!typeList.Contains(type))
            //{
            //    Log.WarnLine("player {0} tip off error, type not exists", Uid, type);
            //    response.Result = (int)ErrorCode.Fail;
            //    Write(response);
            //    return;
            //}
            //名字长度检查
            int nameLen = server.NameChecker.GetWordLen(destName);
            if (nameLen > WordLengthLimit.CharNameLenLimit)
            {
                Log.WarnLine("player {0} tip off error, chat record entry over limit", Uid, content.Count);
                response.Result = (int)ErrorCode.NameLength;
                Write(response);
                return;
            }
            //描述长度检查
            int wordsLen = server.wordChecker.GetWordLen(description);
            if (wordsLen > ChatLibrary.TipOffDescriptionLimit)
            {
                Log.WarnLine("player {0} tip off error, description len is {1} ", Uid, wordsLen);
                SendErrorCodeMsg(ErrorCode.LengthLimit);
                return;
            }
            //举报次数      
            if (CheckCounter(CounterType.TipOffCount))
            {
                Log.WarnLine("player {0} tip off error, already max count", Uid);
                response.Result = (int)ErrorCode.MaxCount;
                Write(response);
                return;
            }
            //聊天记录条目检查
            if (content.Count > ChatLibrary.TipOffChatRecordEntry)
            {
                Log.WarnLine("player {0} tip off error, chat record entry over limit", Uid, content.Count);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
            //每条聊天记录长度检查
            string contentStr = "";
            foreach (var item in content)
            {
                wordsLen = server.wordChecker.GetWordLen(item);
                if (wordsLen > Math.Max(ChatLibrary.ChatWordMaxCount, ChatLibrary.ChatTrumpetMaxCount))
                {
                    Log.WarnLine("player {0} tip off error, content len is {1} ", Uid, wordsLen);
                    SendErrorCodeMsg(ErrorCode.LengthLimit);
                    return;
                }
                contentStr += item + ";";
            }
            response.Result = (int)ErrorCode.Success;
           
            //// 验证通过 记录举报埋点
            UpdateCounter(CounterType.TipOffCount, 1);

            server.AccountDBPool.Call(new QueryInsertTipOffInfo(MainId, Uid, Name, destUid, destName, type, contentStr, description, ZoneServerApi.now.ToString()));

            string log = string.Format("{0}|{1}|{2}|{3}|{4}|{5}", Uid, ZoneServerApi.now.ToString(), destUid, type, contentStr, description);
            server.TrackingLoggerMng.Write(log, TrackingLogType.TIPOFF);

            KomoeEventLogFriendFlow(7, "举报", destUid);

            Write(response);
        }
    }
}
