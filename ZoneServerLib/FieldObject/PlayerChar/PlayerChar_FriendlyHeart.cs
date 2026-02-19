using System;
using EnumerateUtility;
using Message.Gate.Protocol.GateC;
using RedisUtility;
using ServerShared;
using CommonUtility;
using System.Collections.Generic;
using Logger;
using Message.Zone.Protocol.ZR;
using EnumerateUtility.Timing;
using ServerModels;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        /// <summary>
        /// 送星标识
        /// </summary>
        public Dictionary<int, bool> giveHeartFlagDic = new Dictionary<int, bool>();
        public bool CheckHeartGiveFlag(int friendUid)
        {
            bool gived = false;
            giveHeartFlagDic.TryGetValue(friendUid, out gived);
            return gived;
        }

        /// <summary>
        /// 收星标识
        /// </summary>
        public Dictionary<int, bool> recvHeartFlagDic = new Dictionary<int, bool>();

        public bool CheckHeartRecvFlag(int friendUid)
        {
            bool recv = false;
            recvHeartFlagDic.TryGetValue(friendUid, out recv);
            return recv;
        }

        public void LoadHeartFlag()
        {
            giveHeartFlagDic.Clear();
            recvHeartFlagDic.Clear();

            OperateGetFriendHeartGiveFlagList giveFlagList = new OperateGetFriendHeartGiveFlagList(uid);
            server.GameRedis.Call(giveFlagList, r1 => 
            {
                foreach (var item in giveFlagList.friendHeartFlagDic)
                {
                    giveHeartFlagDic[item.Key] = true;
                }
            });

            OperateGetFriendHeartRecvFlagList recvFlagList = new OperateGetFriendHeartRecvFlagList(uid);
            server.GameRedis.Call(recvFlagList, r1 => 
            {
                foreach (var item in recvFlagList.friendHeartFlagDic)
                {
                    recvHeartFlagDic[item.Key] = true;
                }

                OperateGetRecvFriendHeartList recvListOpreate = new OperateGetRecvFriendHeartList(Uid, FriendLib.TakeHeartMaxCnt);
                server.GameRedis.Call(recvListOpreate, r =>
                {
                    if (recvListOpreate.RecvList != null)
                    {
                        foreach (var item in recvListOpreate.RecvList)
                        {
                            TakeHeart((int)item);
                        }
                    }
                });
            });
        }


        private void FriendlyHeartCountRefresh(TimingType refresh_type)
        {
            bool isRefresh = false;
            List<CounterType> list = CounterLibrary.GetRefreshCounter(refresh_type);
            if (list != null)
            {
                Counter counter = null;
                foreach (var type in list)
                {
                    counter = GetCounter(type);
                    if (counter != null)
                    {
                        int count = CounterLibrary.GetMaxCount(type);
                        if (counter.Count < count)
                        {
                            counter.Count = count;
                            //counter.Changed = true;
                            updateCounterList[type] = true;
                            isRefresh = true;
                        }
                    }
                }
            }

            OperateClearFriendHeartFlag clearOperate = new OperateClearFriendHeartFlag(Uid);
            server.GameRedis.Call(clearOperate,ret=>
            {
                giveHeartFlagDic.Clear();
                recvHeartFlagDic.Clear();

                OperateGetRecvFriendHeartList recvListOpreate = new OperateGetRecvFriendHeartList(Uid,FriendLib.TakeHeartMaxCnt);
                server.GameRedis.Call(recvListOpreate, r => 
                {
                    if (recvListOpreate.RecvList != null)
                    {
                        foreach (var item in recvListOpreate.RecvList)
                        {
                            TakeHeart((int)item);
                        }
                    }

                    if (isRefresh)
                    {
                        Write(GetCounterMsg());
                    }
                });
            });
        }

        /// <summary>
        /// 收星数足够
        /// </summary>
        /// <returns></returns>
        public bool TakeHeartCountEnough()
        {
            return GetCounterValue(CounterType.TakeHeartCount) > 0;
        }

        /// <summary>
        /// 送星数足够
        /// </summary>
        /// <returns></returns>
        public bool GiveHeartCountEnough()
        {
            return GetCounterValue(CounterType.GiveHeartCount)>0 ;
        }

        public void TakeHeartCountUpdate()
        {
            UpdateCounter(CounterType.TakeHeartCount, -1);
            SyncTakeHeartCountMsg();
        }

        public void GiveHeartCountUpdate()
        {
            UpdateCounter(CounterType.GiveHeartCount, -1);
            SyncGiveHeartCountMsg();
        }

        private void SyncTakeHeartCountMsg()
        {
            MSG_ZGC_FRIEND_HEART_TAKE_COUNT take = new MSG_ZGC_FRIEND_HEART_TAKE_COUNT();
            take.Count = GetCounterValue(CounterType.TakeHeartCount);
            //take.LastRefreshTime = Timestamp.GetUnixTimeStampSeconds(TakeHeartCountLastFreshTime);
            Write(take);
        }

        private void SyncGiveHeartCountMsg()
        {
            MSG_ZGC_FRIEND_HEART_GIVE_COUNT give = new MSG_ZGC_FRIEND_HEART_GIVE_COUNT();
            give.Count = GetCounterValue(CounterType.GiveHeartCount);
            //give.LastRefreshTime = Timestamp.GetUnixTimeStampSeconds(GiveHeartCountLastFreshTime);
            Write(give);
        }
 

        public void GiveHeartFlag(int friendUid)
        {
            giveHeartFlagDic[friendUid] = true;

            server.GameRedis.Call(new OperateAddFriendHeartGiveFlag(uid, friendUid));

            server.GameRedis.Call(new OperateAddFriendHeartRecvList(friendUid, Uid));
        }

        public void TakeHeartFlag(int friendUid)
        {
            recvHeartFlagDic[friendUid] = true;

            server.GameRedis.Call(new OperateAddFriendHeartRecvFlag(uid, friendUid));
        }

        public void AddFriendHeartOfflineTake(int count)
        {
            AddCoins(CurrenciesType.friendlyHeart, count, ObtainWay.FriendlyHeart, "");
        }

        //internal void FriendHeartCountBuy(bool isGiveCount)
        //{
        //    if (isGiveCount)
        //    {
        //        HeartGiveCountBuy();
        //    }
        //    else
        //    {
        //        HeartTakeCountBuy();
        //    }
        //}

        //private void HeartTakeCountBuy()
        //{
        //    //ErrorCode errocode = HeartCountPayment(
        //    //    FriendLib.TakeHeartCntBuyCost,
        //    //    FriendLib.TakeHeartCntBuyCnt,
        //    //    FriendlyHeartCountType.Take,
        //    //    ConsumeWay.BuyFriendHeartTakeCount,
        //    //    CounterType.TakeHeartCountBuy);

        //    //MSG_ZGC_FRIEND_HEART_COUNT_BUY msg = new MSG_ZGC_FRIEND_HEART_COUNT_BUY();
        //    //msg.Result = (int)errocode;
        //    //msg.IsGiveCount = false;
        //    //Write(msg);

        //    //if (ErrorCode.Success == errocode) //支付成功
        //    //{
        //    //    //给货
        //    //    UpdateCounter(CounterType.TakeHeartCountBuy, 1);
        //    //    TakeHeartCount += FriendLib.TakeHeartCntBuyCnt;
        //    //    server.Redis.Call(new OperateFriendlyHeartTakeCountIntervalUpdate(uid, TakeHeartCount));
        //    //    SyncTakeHeartCountMsg();
        //    //}
        //}

        //private void HeartGiveCountBuy()
        //{
        //    //ErrorCode errocode = HeartCountPayment(
        //    //    FriendLib.GiveHeartCntBuyCost,
        //    //    FriendLib.GiveHeartCntBuyCnt,
        //    //    FriendlyHeartCountType.Give,
        //    //    ConsumeWay.BuyFriendHeartGiveCount,
        //    //    CounterType.GiveHeartCountBuy);

        //    //MSG_ZGC_FRIEND_HEART_COUNT_BUY msg = new MSG_ZGC_FRIEND_HEART_COUNT_BUY();
        //    //msg.Result = (int)errocode;
        //    //msg.IsGiveCount = true;
        //    //Write(msg);

        //    //if (ErrorCode.Success == errocode) //支付成功
        //    //{
        //    //    //给货
        //    //    UpdateCounter(CounterType.GiveHeartCountBuy, 1);
        //    //    GiveHeartCount += FriendLib.GiveHeartCntBuyCnt;
        //    //    server.Redis.Call(new OperateFriendlyHeartGiveCountIntervalUpdate(uid, GiveHeartCount));
        //    //    SyncGiveHeartCountMsg();
        //    //}
        //}

        ///// <summary>
        ///// 支付
        ///// </summary>
        ///// <param name="cost"></param>
        ///// <param name="cnt"></param>
        ///// <param name="countType"></param>
        ///// <param name="way"></param>
        ///// <param name="counterType"></param>
        ///// <returns></returns>
        //private ErrorCode HeartCountPayment(int cost, int cnt, FriendlyHeartCountType countType, ConsumeWay way, CounterType counterType)
        //{
        //    if (CheckCounter(counterType))
        //    {
        //        return ErrorCode.MaxCount;
        //    }

        //    if (!CheckCoins(CurrenciesType.diamond, cost))
        //    {
        //        return ErrorCode.DiamondNotEnough;
        //    }

        //    if (!DelCoins(CurrenciesType.diamond, cost, way, countType.ToString()))
        //    {
        //        return ErrorCode.Fail;
        //    }
        //    return ErrorCode.Success;
        //}

        public void TakeHeart(int giveHeartFriendUid)
        {
            if(CheckHeartRecvFlag(giveHeartFriendUid))
            {
                return;
            }
            TakeHeartFlag(giveHeartFriendUid);
            //AddFriendScore(Uid);
            if (!TakeHeartCountEnough())
            {
                return;
            }
            TakeHeartCountUpdate();

            AddCoins(CurrenciesType.friendlyHeart, 1, ObtainWay.FriendlyHeart,giveHeartFriendUid.ToString());
        }

        public void GiveHeart(int friendUid)
        {
            MSG_ZGC_FRIEND_HEART_GIVE response = new MSG_ZGC_FRIEND_HEART_GIVE();
            response.FriendUid = friendUid;

            if (!CheckFriendExist(friendUid))
            {
                Log.Warn($"player {Uid} give heart failed: player {friendUid} not in friendList");
                return;
            }

            if (CheckHeartGiveFlag(friendUid))
            {
                Log.Warn($"player {Uid} give heart failed: already give heart to friend {friendUid}");
                return;
            }

            if (!GiveHeartCountEnough())
            {
                Log.Warn($"player {Uid} give heart failed: give heart count not enough");
                return;
            }

            GiveHeartFlag(friendUid);

            GiveHeartCountUpdate();
            AddFriendScore(friendUid);

            AddPassCardTaskNum(TaskType.FriendHeart);
            AddSchoolTaskNum(TaskType.FriendHeart);
            //送心到指定次数发称号卡
            TitleMng.UpdateTitleConditionCount(TitleObtainCondition.GiveHeartCount);

            response.FriendScore = GetFriendScore(friendUid);
            response.Result = (int)ErrorCode.Success;
            Write(response);

            PlayerChar friend = server.PCManager.FindPc(friendUid);
            if (friend != null)
            {
                friend.TakeHeart(Uid);
            }
            else
            {
                MSG_ZR_FRIEND_HEART_GIVE giveHeartMsg = new MSG_ZR_FRIEND_HEART_GIVE();
                giveHeartMsg.FriendUid = friendUid;
                server.SendToRelation(giveHeartMsg, uid);
            }
        }

        /// <summary>
        /// 用于一键送星
        /// </summary>
        /// <param name="response"></param>
        /// <param name="friendUid"></param>
        private void GiveHeartOneKey(MSG_ZGC_REPAY_FRIENDS_HEART response, int friendUid)
        {
            GiveHeartFlag(friendUid);
            AddFriendScore(friendUid);

            AddPassCardTaskNum(TaskType.FriendHeart);
            AddSchoolTaskNum(TaskType.FriendHeart);

            FRIEND_SCORE_INFO scoreInfo = new FRIEND_SCORE_INFO();
            scoreInfo.FriendUid = friendUid;
            scoreInfo.FriendScore = GetFriendScore(friendUid);
            response.ScoresInfo.Add(scoreInfo);

            PlayerChar friend = server.PCManager.FindPc(friendUid);
            if (friend != null)
            {
                friend.TakeHeart(Uid);
            }
            else
            {
                MSG_ZR_FRIEND_HEART_GIVE giveHeartMsg = new MSG_ZR_FRIEND_HEART_GIVE();
                giveHeartMsg.FriendUid = friendUid;
                server.SendToRelation(giveHeartMsg, uid);
            }
        }
        internal void OneKeyGiveHeart()
        {
            MSG_ZGC_REPAY_FRIENDS_HEART response = new MSG_ZGC_REPAY_FRIENDS_HEART();

            List<int> friends = new List<int>();

            foreach (var item in recvHeartFlagDic)
            {
                if (!CheckFriendExist(item.Key))
                {
                    continue;
                }

                if (CheckHeartGiveFlag(item.Key))
                {
                    continue;
                }

                if (!GiveHeartCountEnough())
                {
                    break;
                }
                UpdateCounter(CounterType.GiveHeartCount, -1);
                friends.Add(item.Key);
            }


            foreach (var item in friendList)
            {
                if (friends.Contains(item.Key))
                {
                    continue;
                }

                if (CheckHeartGiveFlag(item.Key))
                {
                    continue;
                }

                if (!GiveHeartCountEnough())
                {
                    break;
                }
                UpdateCounter(CounterType.GiveHeartCount, -1);
                friends.Add(item.Key);
                //送心到指定次数发称号卡
                TitleMng.UpdateTitleConditionCount(TitleObtainCondition.GiveHeartCount);
            }

            foreach (var item in friends)
            {
                GiveHeartOneKey(response, item);
            }

            SyncGiveHeartCountMsg();
            //server.Redis.Call(new OperateFriendlyHeartGiveCountIntervalUpdate(uid, GiveHeartCount, Timestamp.GetUnixTimeStampSeconds(ZoneServerApi.now)));
            response.Result = (int)ErrorCode.Success;
            Write(response);

        }

        //public void RepayFriendHeart(int pcUid, int friendUid)
        //{
        //    PlayerChar friend = server.PCManager.FindPc(friendUid);
        //    if (friend != null)
        //    {
        //        //在当前zone
        //        //if (friend.CheckBlackExist(Uid))
        //        //{
        //        //    response.Result = (int)ErrorCode.InTargetBlack;
        //        //    //player.Write(response);
        //        //    return;
        //        //}

        //        //if (!friend.CheckTakeHeartCount())
        //        //{
        //        //    response.Result = (int)ErrorCode.FriendTakeHeartFail;
        //        //    //player.Write(response);
        //        //    return;
        //        //}

        //        //GiveHeartCountSub();
        //        AddFriendScore(friendUid);

        //        OperateGetFriendScore operate = new OperateGetFriendScore(uid, friendUid);
        //        server.Redis.Call(operate, ret1 =>
        //        {
        //            //player.Write(response);
        //            AddPassCardTaskNum(TaskType.FriendHeart);

        //            if (friend.TakeHeartCount > 0)
        //            {
        //                friend.TakeHeartCountSub(pcUid);
        //            }

        //            GiveHeartFlag(friendUid);
        //            ////发一条系统消息给好友
        //            //SendPersonSystemOnlineChat(friendUid, FriendLib.TakeHeartPersonSystemMsgId);
        //        });
        //    }
        //    else
        //    {
        //        //不在当前zone
        //        //OperateCheckBlackList checker = new OperateCheckBlackList(friendUid, pcUid);
        //        //server.Redis.Call(checker, inBlack =>
        //        //{
        //            //if ((int)inBlack == 1)
        //            //{
        //            //    if (checker.Exist)
        //            //    {
        //            //        //response.Result = (int)ErrorCode.InTargetBlack;
        //            //        return;
        //            //    }
        //            //}

        //            OperateGetFriendlyHeartInfo heartInfo = new OperateGetFriendlyHeartInfo(friendUid);
        //            server.Redis.Call(heartInfo, ret =>
        //            {
        //                if ((int)ret != 1)
        //                {
        //                    Log.Error("player {0} repay friend heart fail: redis error");
        //                    return;
        //                }

        //                //if (heartInfo.HeartInfo.TakeCount < 1)
        //                //{
        //                //    response.Result = (int)ErrorCode.FriendTakeHeartFail;
        //                //    //player.Write(response);
        //                //    return;
        //                //}

        //                //GiveHeartCountSub();
        //                AddFriendScore(friendUid);
        //                GiveHeartFlag(friendUid);
        //                AddPassCardTaskNum(TaskType.FriendHeart);

        //                //好友逻辑
        //                OperateGetOnlineState onlineStat = new OperateGetOnlineState(friendUid);
        //                server.Redis.Call(onlineStat, getOnline =>
        //                {
        //                    if (heartInfo.HeartInfo.TakeCount < 1)
        //                    {
        //                        return;
        //                    }

        //                    if (onlineStat.IsOnline)
        //                    {
        //                        //在线
        //                        //发一条系统消息给好友
        //                        SendPersonSystemOnlineChat(friendUid, FriendLib.TakeHeartPersonSystemMsgId);

        //                        //转给Relation
        //                        MSG_ZR_FRIEND_HEART_GIVE giveHeartMsg = new MSG_ZR_FRIEND_HEART_GIVE();//
        //                        giveHeartMsg.FriendUid = friendUid;
        //                        server.SendToRelation(giveHeartMsg, uid);
        //                        return;
        //                    }
        //                    else
        //                    {
        //                        //不在线
        //                        SendPersonSystemOfflineChat(friendUid, FriendLib.TakeHeartPersonSystemMsgId);
        //                        server.Redis.Call(new OperateFriendlyHeartTakeCountIntervalUpdate(friendUid, heartInfo.HeartInfo.TakeCount - 1));
        //                        return;
        //                    }
        //                });
        //            });
        //        //});
        //    }
        //}
    }
}
