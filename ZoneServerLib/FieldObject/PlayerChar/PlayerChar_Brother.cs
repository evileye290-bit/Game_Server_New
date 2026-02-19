using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Google.Protobuf.Collections;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZR;
using RedisUtility;
using ServerModels;
using ServerShared;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        //金兰 
        List<int> brotherList = new List<int>();
        //申请列表
        List<int> brotherInviteList = new List<int>();

        public int BrotherCount { get { return brotherList.Count; } }
        public void SendBrotherInfo()
        {
            LoadBrotherInviteList();
        }

        //金兰邀请相关逻辑
        public void BrotherInvite(int friendUid)
        {
            if (brotherList.Count() == BrothersLib.BROTHER_LIST_MAX_COUNT)
            {
                Log.Warn($"player {uid} invite brother fail,count max!");
                return;
            }

            int friendScore = GetScoreByFriendId(friendUid);
            if (friendScore < BrothersLib.BROTHER_FRIEND_SCORE)
            {
                Log.Warn($"player {uid} invite friend {friendUid} fail,friend score limit （{friendScore}）!");
                return;
            }

            MSG_ZGC_BROTHERS_INVITE response = new MSG_ZGC_BROTHERS_INVITE();
            response.FriendUid = friendUid;

            OperateGetBrotherListCount operate = new OperateGetBrotherListCount(friendUid);
            server.GameRedis.Call(operate, ret =>
            {
                if ((int)ret == 1)
                {
                    if (operate.Count == BrothersLib.BROTHER_LIST_MAX_COUNT)
                    {
                        Log.Warn($"player {uid} invite friend {friendUid} fail, brother list full");
                        response.Result = (int)ErrorCode.FriendBrotherListFull;
                        Write(response);
                        return;
                    }

                    OperateBrotherInviteAdd op = new OperateBrotherInviteAdd(friendUid, Uid);
                    server.GameRedis.Call(op, ret1 =>
                     {
                         if ((int)ret1 == 1)
                         {
                             MSG_ZR_BROTHERS_INVITE msg = new MSG_ZR_BROTHERS_INVITE();
                             msg.FriendUid = friendUid;
                             server.RelationServer.Write(msg, Uid);

                             response.Result = (int)ErrorCode.Success;
                             Write(response);
                         }
                         else
                         {
                             Log.Error($"player {Uid} BrotherInvite  fail ,Redis(OperateBrotherInviteAdd) error!");
                         }
                     });
                }
                else
                {
                    Log.Error($"player {Uid} BrotherInvite  fail ,Redis(OperateGetBrotherListCount) error!");
                }
            });
        }


        public void RemoveFromBrotherList(int brotherUid)
        {
            if (!DelFromBrotherList(brotherUid))
            {
                return;
            }
            //LoadBrotherList();
            MSG_ZGC_BROTHERS_REMOVE response = new MSG_ZGC_BROTHERS_REMOVE();
            response.BrotherUid = brotherUid;
            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        public void RemoveBrother(int brotherUid)
        {
            MSG_ZGC_BROTHERS_REMOVE response = new MSG_ZGC_BROTHERS_REMOVE();
            response.BrotherUid = brotherUid;

            if (!DelFromBrotherList(brotherUid))
            {
                Log.Warn("player {0} delete friend {1} fail: not find in brotherList", Uid, brotherUid);
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            OperateBrotherDel op = new OperateBrotherDel(uid, brotherUid);
            server.GameRedis.Call(op, ret =>
            {
                if ((int)ret == 1)
                {
                    MSG_ZR_BROTHERS_REMOVE msg = new MSG_ZR_BROTHERS_REMOVE();
                    msg.BrotherUid = brotherUid;
                    server.RelationServer.Write(msg, Uid);

                    response.Result = (int)ErrorCode.Success;
                    Write(response);
                }
                else
                {
                    Log.Error($"player {Uid} BrotherInviteResponse  fail ,Redis(OperateBrotherAdd) error!");
                }
            });
        }


        //金兰答复邀请相关逻辑
        public void BrotherInviteResponse(int inviterUid, bool agree)
        {
            DelBrotherInviteList(inviterUid);
            MSG_ZGC_BROTHERS_RESPONSE response = new MSG_ZGC_BROTHERS_RESPONSE();
            response.InviterUid = inviterUid;
            response.ResponserUid = uid;
            response.Agree = agree;
            response.Result = (int)ErrorCode.Success;
           
            if (agree)
            {
                if (!friendList.ContainsKey(inviterUid))
                {
                    response.Result = (int)ErrorCode.NotHisFriend;
                    Write(response);
                    return;
                }

                KomoeEventLogFriendFlow(9, "义结金兰同意", inviterUid);

                OperateGetBrotherList brotherListOp = new OperateGetBrotherList(Uid);
                server.GameRedis.Call(brotherListOp, ListRet =>
                {

                    if (brotherListOp.BrotherList.Count() == BrothersLib.BROTHER_LIST_MAX_COUNT)
                    {
                        Log.Warn($"player {uid} invite resopnse brother fail,count max!");
                        return;
                    }

                    if ((int)ListRet == 1)
                    {
                        if (brotherListOp.BrotherList != null)
                        {
                            brotherList.Clear();
                            foreach (var item in brotherListOp.BrotherList)
                            {
                                int brotherUid = (int)item;
                                brotherList.Add(brotherUid);
                            }
                            AddTaskNumForType(TaskType.BrotherNum, BrotherCount, false);
                        }
                    }

                    if (brotherList.Contains(inviterUid))
                    {
                        response.Result = (int)ErrorCode.Success;
                        Write(response);
                        return;
                    }

                    OperateGetBrotherListCount operate = new OperateGetBrotherListCount(inviterUid);
                    server.GameRedis.Call(operate, ret =>
                    {
                        if ((int)ret == 1)
                        {
                            if (operate.Count == BrothersLib.BROTHER_LIST_MAX_COUNT)
                            {
                                Log.Warn($"player {uid} invite resopnse brother fail, brother list full");
                                response.Result = (int)ErrorCode.FriendBrotherListFull;
                                Write(response);
                                return;
                            }

                            Add2BrotherList(inviterUid);

                            AddTaskNumForType(TaskType.BrotherNum, BrotherCount, false);

                            //完成义结金兰发称号卡
                            TitleMng.UpdateTitleConditionCount(TitleObtainCondition.Sworn);

                            OperateBrotherAdd op = new OperateBrotherAdd(uid, inviterUid);
                            server.GameRedis.Call(op, ret1 =>
                            {
                                if ((int)ret1 == 1)
                                {
                                    MSG_ZR_BROTHERS_RESPONSE msg = new MSG_ZR_BROTHERS_RESPONSE();
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
                                    Log.Error($"player {Uid} BrotherInviteResponse  fail ,Redis(OperateBrotherAdd) error!");
                                }
                            });
                        }
                        else
                        {
                            Log.Error($"player {Uid} BrotherInviteResponse fail ,Redis(OperateGetBrotherListCount) error!");
                        }
                    });
                });
            }
            else
            {
                MSG_ZR_BROTHERS_RESPONSE msg = new MSG_ZR_BROTHERS_RESPONSE();
                msg.InviterUid = inviterUid;
                msg.ResponserUid = uid;
                msg.Agree = agree;
                msg.Result = (int)ErrorCode.Success;
                server.RelationServer.Write(msg, Uid);

                response.Result = (int)ErrorCode.Success;
                Write(response);
            }

        }

        public void NotiyClientAdd2BrotherList(int inviterUid, int responserUid, bool agree)
        {
            MSG_ZGC_BROTHERS_RESPONSE response = new MSG_ZGC_BROTHERS_RESPONSE();
            response.InviterUid = inviterUid;
            response.ResponserUid = responserUid;
            response.Agree = agree;
            response.Result = (int)ErrorCode.Success;
            Write(response);
        }


        public bool Add2BrotherList(int brotherUid)
        {
            if (brotherList.Contains(brotherUid))
            {
                return false;
            }
            else
            {
                brotherList.Add(brotherUid);
            }
            return true;
        }

        private bool DelFromBrotherList(int playerId)
        {
            if (brotherList.Contains(playerId))
            {
                brotherList.Remove(playerId);
            }
            else
            {
                return false;
            }
            return true;
        }

        private void ClearBrotherList()
        {
            brotherList.Clear();
        }


        public bool CheckBrotherExist(int playerId)
        {
            return brotherList.Contains(playerId);
        }

        private bool CheckBrotherListFull()
        {
            if (brotherList.Count >= BrothersLib.BROTHER_LIST_MAX_COUNT)
            {
                return true;
            }
            return false;
        }

        public void LoadBrotherList()
        {
            OperateGetBrotherList operate = new OperateGetBrotherList(Uid);
            server.GameRedis.Call(operate, ret =>
            {
                MSG_ZGC_SYNC_BROTHERS_LIST response = new MSG_ZGC_SYNC_BROTHERS_LIST();
                if ((int)ret == 1)
                {
                    if (operate.BrotherList != null)
                    {
                        brotherList.Clear();
                        List<int> removeBrotherIdList = new List<int>(); //重复id纠错。具体原因是redis结构用错了，改用set才好
                        List<int> addBrotherIdList = new List<int>(); //重复id纠错。具体原因是redis结构用错了，改用set才好
                        foreach (var item in operate.BrotherList)
                        {
                            int brotherUid = (int)item;
                            if (friendList.ContainsKey(brotherUid))
                            {
                                if (!brotherList.Contains(brotherUid))
                                {
                                    brotherList.Add(brotherUid);
                                    response.FriendList.Add(brotherUid);
                                }
                                else
                                {
                                    if (!removeBrotherIdList.Contains(brotherUid))
                                    {
                                        removeBrotherIdList.Add(brotherUid);
                                        addBrotherIdList.Add(brotherUid);
                                    }
                                }
                            }
                            else
                            {
                                if (!removeBrotherIdList.Contains(brotherUid))
                                {
                                    removeBrotherIdList.Add(brotherUid);
                                }
                            }
                        }

                        foreach (var item in removeBrotherIdList)
                        {
                            //需要做删除
                            OperateBrotherDel operateBrotherDel = new OperateBrotherDel(Uid, item, true);
                            server.GameRedis.Call(operateBrotherDel,op =>
                            {
                                if (addBrotherIdList.Contains(item))
                                {
                                    OperateBrotherAdd operateBrotherAdd = new OperateBrotherAdd(Uid, item, true);
                                    server.GameRedis.Call(operateBrotherAdd);
                                }
                            });
                        }
                       
                        AddTaskNumForType(TaskType.BrotherNum, BrotherCount, false);
                        Write(response);
                        return;
                    }
                }
                Write(response);
                return;
            });
        }

        public void LoadBrotherInviteList()
        {
            OperateGetBrotherInviteInfoList operate = new OperateGetBrotherInviteInfoList(Uid, BrothersLib.BROTHER_INVITER_LIST_MAX_COUNT);
            server.GameRedis.Call(operate, ret =>
            {
                MSG_ZGC_SYNC_BROTHERS_INVITER_LIST response = new MSG_ZGC_SYNC_BROTHERS_INVITER_LIST();
                if ((int)ret == 1)
                {
                    if (operate.Characters != null)
                    {
                        brotherInviteList.Clear();
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
                Write(response);
                return;
            });
        }

        public void AddBrotherInviteList(int friendUid)
        {
            if (!brotherInviteList.Contains(friendUid))
            {
                brotherInviteList.Add(friendUid);
                LoadBrotherInviteList();
            }
        }

        public void DelBrotherInviteList(int friendUid)
        {
            if (brotherInviteList.Contains(friendUid))
            {
                brotherInviteList.Remove(friendUid);
            }

            OperateBrotherInvietDel operate = new OperateBrotherInvietDel(Uid, friendUid);
            server.GameRedis.Call(operate);
        }
    }
}
