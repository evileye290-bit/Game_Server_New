using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Google.Protobuf.Collections;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZR;
using RedisUtility;
using ServerShared;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        //好友 uid 友好度
        Dictionary<int,int> friendList = new Dictionary<int,int>();

        internal int GetFriendScore(int item)
        {
            int value = 0;
            friendList.TryGetValue(item, out value);
            return value;
        }

        //好友 uid 战力
        Dictionary<int, int> friendBattlePowerList = new Dictionary<int, int>();

        //申请列表
        List<int> friendInviteList = new List<int>();

        public void SendFriendInfo()
        {
            LoadAndSendFriendInviteList();
            SyncGiveHeartCountMsg();
            SyncTakeHeartCountMsg();
        }

        public void LoadAndSendFriendInviteList()
        {
            OperateGetFriendInviteInfoList operate = new OperateGetFriendInviteInfoList(Uid,FriendLib.FRIEND_INVITER_LIST_MAXCNT);
            server.GameRedis.Call(operate, ret =>
            {
                if ((int)ret == 1)
                {
                    if (operate.Characters != null)
                    {
                        brotherInviteList.Clear();

                        MSG_ZGC_SYNC_FRIEND_INVITER_LIST response = new MSG_ZGC_SYNC_FRIEND_INVITER_LIST();
                        foreach (var item in operate.Characters)
                        {
                            FRIEND_INFO friendInfo = GetFriendInfo(item.Value);
                            response.InviterList.Add(friendInfo);

                            brotherInviteList.Add(friendInfo.BaseInfo.Uid);
                        }

                        Write(response);
                        return;
                    }
                }
            });


        }

        public void AddFriendInviteList(int friendUid)
        {
            if (!friendInviteList.Contains(friendUid))
            {
                friendInviteList.Add(friendUid);
                LoadAndSendFriendInviteList();
            }
        }

        public void DelFriendInviteList(int friendUid)
        {
            if (friendInviteList.Contains(friendUid))
            {
                friendInviteList.Remove(friendUid);
            }

            OperateFriendInvietDel operate = new OperateFriendInvietDel(Uid, friendUid);
            server.GameRedis.Call(operate);
        }

        public void OneKeyIgnoreFriendInvite()
        {
            foreach (var inviterUid in friendInviteList)
            {
                KomoeEventLogFriendFlow(3, "忽略申请", inviterUid);
            }
            friendInviteList.Clear();
            server.GameRedis.Call(new OperateClearFriendInviteList(uid));

            MSG_ZGC_ONEKEY_IGNORE_INVITER res = new MSG_ZGC_ONEKEY_IGNORE_INVITER();
            res.Result = (int)ErrorCode.Success;
            Write(res);
        }

        //答复添加请求相关逻辑
        public void FriendInviteResponse(int inviterUid, bool agree)
        {
            DelFriendInviteList(inviterUid);

            MSG_ZGC_FRIEND_RESPONSE response = new MSG_ZGC_FRIEND_RESPONSE();
            response.InviterUid = inviterUid;
            response.ResponserUid = uid;
            response.Agree = agree;
            response.Result = (int)ErrorCode.Success;

            if (agree)
            {
                if (CheckFriendListFull())
                {
                    Log.Warn($"player {uid} invite resopnse friend fail,count max!");
                    response.Result = (int)ErrorCode.FriendListFull;
                    Write(response);
                    return;
                }

                OperateGetFriendList operate = new OperateGetFriendList(inviterUid);
                server.GameRedis.Call(operate, ret =>
                {
                    if ((int)ret == 1)
                    {
                        if (CheckFriendListFull(operate.friendList.Count))
                        {
                            Log.Warn($"player {uid} invite resopnse friend fail, friend list full");
                            response.Result = (int)ErrorCode.HisFriendListFull;
                            Write(response);
                            return;
                        }

                        OperateFriendAdd op1 = new OperateFriendAdd(uid, inviterUid);
                        server.GameRedis.Call(op1);

                        Add2FriendList(inviterUid);

                        UpdateFriendList(inviterUid, 0);

                        OperateGetBaseInfo op = new OperateGetBaseInfo(inviterUid);
                        server.GameRedis.Call(op, re =>
                        {
                            UpdateFriendBattlePowerList(inviterUid, op.Player.BattlePower);
                        });

                        OperateFriendAdd op2 = new OperateFriendAdd(inviterUid, uid);
                        server.GameRedis.Call(op2, ret1 =>
                        {
                            if ((int)ret1 == 1)
                            {
                                MSG_ZR_FRIEND_RESPONSE msg = new MSG_ZR_FRIEND_RESPONSE();
                                msg.InviterUid = inviterUid;
                                msg.ResponserUid = uid;
                                msg.Agree = agree;
                                msg.Result = (int)ErrorCode.Success;
                                server.RelationServer.Write(msg, Uid);

                                response.Result = (int)ErrorCode.Success;
                                Write(response);

                               
                            }
                            else
                            {
                                Log.Error($"player {Uid} FriendInviteResponse  fail ,Redis(OperateFriendAdd) error!");
                            }
                        });
                    }
                    else
                    {
                        Log.Error($"player {Uid} FriendInviteResponse fail ,Redis(OperateGetFriendListCount) error!");
                    }
                });
            }
            else
            {
                response.Result = (int)ErrorCode.Success;
                Write(response);
            }

        }

        public void NotiyClientAdd2FriendList(int inviterUid, int responserUid, bool agree)
        {
            MSG_ZGC_FRIEND_RESPONSE response = new MSG_ZGC_FRIEND_RESPONSE();
            response.InviterUid = inviterUid;
            response.ResponserUid = responserUid;
            response.Agree = agree;
            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        public bool Add2FriendList(int friendId,int friendScore=0)
        {
            if (friendList.ContainsKey(friendId))
            {
                return false;
            }
            else
            {
                friendList.Add(friendId, friendScore);
            }
            return true;
        }

        public bool AddFriendScore(int friendUid,int addScore =1)
        {
            if (friendList.ContainsKey(friendUid))
            {
                friendList[friendUid] += addScore;
                OperatIncrementFriendScore operate = new OperatIncrementFriendScore(uid, friendUid, addScore);
                server.GameRedis.Call(operate, ret1 =>
                {
                    friendList[friendUid] = operate.FriendScore;
                });
            }
            return true;
        }

        public void UpdateFriendList(int friendUid, int score)
        {
            //if (friendList.ContainsKey(friendUid))
            {
                friendList[friendUid] = score;
            }
        }

        public void UpdateFriendBattlePowerList(int friendUid, int battlePower)
        {
             friendBattlePowerList[friendUid] = battlePower;
        }

        public int GetScoreByFriendId(int friendId)
        {
            int friendScore = 0;
            if (friendList.TryGetValue(friendId,out friendScore))
            {
                return friendScore;
            }
            return friendScore;
        }

        private bool DelFromFriendList(int playerId)
        {
            if (friendList.ContainsKey(playerId))
            {
                friendList.Remove(playerId);
            }
            else
            {
                return false;
            }
            return true;
        }

        private void ClearFriendList()
        {
            friendList.Clear();
        }


        public bool CheckFriendExist(int playerId)
        {
            return friendList.ContainsKey(playerId);
        }

        private bool CheckFriendListFull()
        {
            if (friendList.Count >= FriendLib.FRIEND_LIST_MAX_COUNT)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool CheckFriendListFull(int count)
        {
            if (count >= FriendLib.FRIEND_LIST_MAX_COUNT)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void LoadFriendList()
        {
            OperateGetFriendList operate = new OperateGetFriendList(Uid);
            server.GameRedis.Call(operate, ret =>
            {
                if ((int)ret == 1)
                {
                    ClearFriendList();

                    OperateGetFriendInfoList ope = new OperateGetFriendInfoList(Uid);
                    server.GameRedis.Call(ope, r =>
                    {

                        if ((int)r == 1)
                        {
                            if (ope.FriendInfoList != null)
                            {
                                foreach (var item in ope.FriendInfoList)
                                {
                                    FRIEND_INFO friendInfo = GetFriendInfo(item.Value);
                                    UpdateFriendList(friendInfo.BaseInfo.Uid, friendInfo.FriendScore);
                                    UpdateFriendBattlePowerList(friendInfo.BaseInfo.Uid, friendInfo.BaseInfo.BattlePower);
                                }
                                LoadBrotherList();
                                return;
                            }
                            else
                            {
                                return;
                            }
                        }
                        else
                        {
                            return;
                        }
                    });
                }
            });
        }

        private void SearchPlayerById(int playerId)
        {
            List<RedisValue> ids = new List<RedisValue>();
            ids.Add(playerId);

            OperateGetFriendInfoListByIds operate = new OperateGetFriendInfoListByIds(uid, ids);
            server.GameRedis.Call(operate, ret1 =>
            {
                MSG_ZGC_FRIEND_SEARCH response = new MSG_ZGC_FRIEND_SEARCH();
                if ((int)ret1 == 1)
                {
                    if (operate.Characters == null)
                    {
                        response.Result = (int)ErrorCode.CharNotExist;
                        Write(response);
                        return;
                    }
                    if (operate.Characters.Count > 0)
                    {
                        foreach (var item in operate.Characters)
                        {
                            FRIEND_INFO friendInfo = GetFriendInfo(item.Value);
                            response.Info = friendInfo;
                        }
                        response.Result = (int)ErrorCode.Success;
                        Write(response);
                    }
                    else
                    {
                        response.Result = (int)ErrorCode.CharNotExist;
                        Write(response);
                        return;
                    }
                    return;
                }
                else
                {
                    //没找到对应id的信息
                    //Log.Error("player {0} search an not exist playerId {1} :redis date error", Uid, playerId);
                    response.Result = (int)ErrorCode.CharNotExist;
                    Write(response);
                    return;
                }
            });
        }

        private bool CheckIdSearch(string keyWord)
        {
            return keyWord.Length > 1 && keyWord.Substring(0, 1) == CONST.SEARCH_ID_PREFIX;
        }

        public void SearchFriend(string keyWord)
        {
            int Id = 0;
            Log.Debug("player {0} search keyWord {1}", Uid, keyWord);
            MSG_ZGC_FRIEND_SEARCH response = new MSG_ZGC_FRIEND_SEARCH();
            if (CheckIdSearch(keyWord))  //ID:查找
            {
                string strUid = keyWord.Substring(1);
                if (strUid == null)
                {
                    Log.Warn("player {0} search an not exist name {1} strUid is null errocode is {2}", Uid, keyWord, response.Result);
                    response.Result = (int)ErrorCode.CharNotExist;
                    Write(response);
                    return;
                }
                else
                {
                    if (Regex.IsMatch(strUid, @"^\d*$")) //判断是否为数字
                    {
                        if(int.TryParse(strUid,out Id))
                        {
                            SearchPlayerById(Id);
                        }
                        else
                        {
                            Log.Warn("player {0} search an not exist Id {1} is not an id num", Uid, Id);
                            response.Result = (int)ErrorCode.Fail;
                            Write(response);
                        }
                    }
                    else
                    {
                        //没找到对应id的信息
                        Log.Warn("player {0} search an not exist Id {1} is not an id num", Uid, Id);
                        response.Result = (int)ErrorCode.Fail;
                        Write(response);
                        return;
                    }
                }
            }
            else //昵称查找
            {
                QueryGetCharacterIdByName queryId = new QueryGetCharacterIdByName(keyWord);

                server.GameDBPool.Call(queryId, ret =>
                {
                    if ((int)ret == 1)
                    {
                        if (queryId.PlayUid == 0)
                        {
                            //没找到
                            response.Result = (int)ErrorCode.CharNotExist;
                            Log.Warn("player {0} search an not exist name {1} got wrong id {2} ", Uid, keyWord, queryId.PlayUid);
                            Write(response);
                            return;
                        }
                        else
                        {
                            Id = queryId.PlayUid;
                            SearchPlayerById(Id);
                        }
                    }
                    else
                    {
                        response.Result = (int)ErrorCode.CharNotExist;
                        Log.Warn("player {0} search an not exist name {1} db data error ", Uid, keyWord);
                        Write(response);
                        return;
                    }
                });
            }
        }

        public void FriendInvite(int friendUid,int flag=0)
        {
            MSG_ZGC_FRIEND_ADD response = new MSG_ZGC_FRIEND_ADD();
            response.FriendUid = friendUid;
            response.Flag = flag;
            
            if (Uid == friendUid)// 不能加自己为好友 
            {
                Log.Warn("player {0} try to add himself (friend id :{1}) is an wrong operate!", Uid, friendUid);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            if (CheckBlackExist(friendUid))//不能加黑名单中的人为好友
            {
                Log.Warn("player {0} add request friend {1} in black", Uid, friendUid);
                response.Result = (int)ErrorCode.InBlack;
                Write(response);
                return;
            }
            
            if(CheckFriendExist(friendUid))//已经是好友了
            {
                response.Result = (int)ErrorCode.FriendExist;
                Log.Warn("player {0} add request friend {1} exist at friend list", Uid, friendUid);
                Write(response);
                return;
            }

            if (CheckFriendListFull()) //好友列表满不能添加
            {
                Log.Warn("player {0} add request friend list reach max cout {1}", Uid, friendList.Count);
                response.Result = (int)ErrorCode.FriendListFull;
                Write(response);
                return;
            }

            OperateGetFriendList operate = new OperateGetFriendList(friendUid);
            server.GameRedis.Call(operate, ret => 
            {
                if ((int) ret >0)
                {
                    if (CheckFriendListFull(friendList.Count))
                    {
                        Log.Warn("player {0} add request friend {1} list  reach max count ", Uid,friendUid);
                        response.Result = (int)ErrorCode.FriendListFull;
                        Write(response);
                    }

                    OperateFriendInviteAdd op = new OperateFriendInviteAdd(friendUid, Uid, Timestamp.GetUnixTimeStampSeconds(server.Now()));
                    server.GameRedis.Call(op, ret1 =>
                    {
                        if ((int)ret1 == 1)
                        {
                            MSG_ZR_FRIEND_INVITE msg = new MSG_ZR_FRIEND_INVITE();
                            msg.FriendUid = friendUid;
                            server.RelationServer.Write(msg, Uid);

                            response.Result = (int)ErrorCode.Success;
                            Write(response);

                            //添加一次好友
                            AddTaskNumForType(TaskType.FriendAdd);
                        }
                        else
                        {
                            Log.Error($"player {Uid} friendInvite  fail ,Redis(OperateFriendInviteAdd) error!");
                        }
                    });
                }
            } );
        }

        public void DelFriend(int friendUid,int flag=0)
        {

            MSG_ZGC_FRIEND_DELETE response = new MSG_ZGC_FRIEND_DELETE();
            response.FriendUid = friendUid;
            response.Flag = flag;

            if (DelFromFriendList(friendUid))
            {
                server.GameRedis.Call(new OperateFriendDel(Uid, friendUid));
                server.GameRedis.Call(new OperateClearFriendScore(Uid, friendUid));
                if (CheckBrotherExist(friendUid))
                {
                    RemoveBrother(friendUid);
                }

                response.Result = (int)ErrorCode.Success;
                Write(response);

                MSG_ZR_FRIEND_REMOVE msg = new MSG_ZR_FRIEND_REMOVE();
                msg.FriendUid = friendUid;
                server.RelationServer.Write(msg, Uid);

                Log.Write("player {0} delete friend {1} from friend list success ", Uid, friendUid);
            }
            else
            {
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                Log.Error("player {0} delete friend {1} fail", Uid, friendUid);
            }
        }

        public void RemoveFromFriendList(int friendUid)
        {
            if (!DelFromFriendList(friendUid))
            {
                return;
            }
            LoadFriendList();

            //MSG_ZGC_SYNC_BROTHERS_LIST response = new MSG_ZGC_SYNC_BROTHERS_LIST();
            //Write(response);
        }


        public FRIEND_INFO GetFriendInfo(FriendInfo info)
        {
            FRIEND_INFO friendInfo = new FRIEND_INFO();
            friendInfo.BaseInfo = PlayerInfo.GetPlayerBaseInfo(info.BaseInfo);
            friendInfo.FriendScore = info.FriendScore;
            friendInfo.SendedHeart = info.GivedHeart;
            friendInfo.RecvedHeart = info.RecvedHeart;
            return friendInfo;
        }

        public void GetFriendList()
        {
            OperateGetFriendInfoList operate = new OperateGetFriendInfoList(Uid);
            server.GameRedis.Call(operate, ret =>
            {
                MSG_ZGC_FRIEND_LIST response = new MSG_ZGC_FRIEND_LIST();

                if ((int)ret == 1)
                {
                    if (operate.FriendInfoList != null)
                    {
                        foreach (var item in operate.FriendInfoList)
                        {
                            FRIEND_INFO friendInfo = GetFriendInfo(item.Value);
                            response.List.Add(friendInfo);
                            UpdateFriendList(friendInfo.BaseInfo.Uid, friendInfo.FriendScore);
                            UpdateFriendBattlePowerList(friendInfo.BaseInfo.Uid, friendInfo.BaseInfo.BattlePower);
                        }
                        Write(response);
                        //金兰标记
                        LoadBrotherList();
                        return;
                    }
                    else
                    {
                        Write(response);
                        return;
                    }
                }
                else
                {
                    Write(response);
                    return;
                }
            });
        }
        public void GetRecentList(RepeatedField<int> Ids)
        {
            var fields = Ids.Select(x => (RedisValue)x);
            MSG_ZGC_FRIEND_RECENT_LIST response = new MSG_ZGC_FRIEND_RECENT_LIST();

            OperateGetFriendInfoListByIds operate = new OperateGetFriendInfoListByIds(Uid,fields);
            server.GameRedis.Call(operate, ret =>
            {
                if ((int)ret == 1)
                {
                    if (operate.Characters != null)
                    {
                        foreach (var item in operate.Characters)
                        {
                            FRIEND_INFO friendInfo = GetFriendInfo(item.Value);
                            response.List.Add(friendInfo);
                        }
                        Write(response);
                        return;
                    }
                    else
                    {
                        Write(response);
                        return;
                    }
                }
                else
                {
                    //Log.Error("player {0} execute GetRecentList fail: redis data error!", Uid);
                    Write(response);
                    return;
                }
            });
        }

        public void RecommendPlayers()
        {
            MSG_ZGC_FRIEND_RECOMMEND response = new MSG_ZGC_FRIEND_RECOMMEND();

            OperateRecommendPlayersNew opr = new OperateRecommendPlayersNew(Uid, MainId, FriendLib.RECOMMEND_COUNT, blackList, friendList.Keys.ToList(),Level,FriendLib.RECOMMEND_LEVEL_MAX, FriendLib.RECOMMEND_LEVEL_MIN,10);
            server.GameRedis.Call(opr, ret =>
            {
                if ((int)ret == 1)
                {
                    if (opr.Ids.Count() > 0)
                    {
                        GetRecommendPlayers(response, opr.Ids.ToList());
                        return;
                    }
                    else
                    {
                        Log.Warn("player {0} RecommendPlayers fail there is no players", Uid);
                        response.Result = (int)ErrorCode.Fail;
                        Write(response);
                        return;
                    }
                }
                else
                {
                    Log.ErrorLine("player {0} RecommendPlayers can not get Info from redis", Uid);
                    response.Result = (int)ErrorCode.Fail;
                    Write(response);
                    return;
                }
            });

        }

        private void GetRecommendPlayers(MSG_ZGC_FRIEND_RECOMMEND response, List<RedisValue> ids)
        {
            OperateGetFriendInfoListByIds oper = new OperateGetFriendInfoListByIds(uid,ids);
            server.GameRedis.Call(oper, re =>
            {
                if ((int)re == 1)
                {
                    if (oper.Characters != null)
                    {
                        foreach (var item in oper.Characters)
                        {
                            FRIEND_INFO friendInfo = GetFriendInfo(item.Value);
                            response.List.Add(friendInfo);
                        }
                        response.Result = (int)ErrorCode.Success;
                        Write(response);
                        return;
                    }
                    else
                    {
                        Log.Error("player {0} get recommend players fail oper.Characters is null", Uid);
                        response.Result = (int)ErrorCode.Fail;
                        Write(response);
                        return;
                    }
                }
                else
                {
                    Log.Error("player {0} get recommend players fail , can not get info from redis", Uid);
                    response.Result = (int)ErrorCode.Fail;
                    Write(response);
                    return;
                }
            });
        }


   
    }
}
