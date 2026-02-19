using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using RedisUtility;
using System.Collections.Generic;
using System;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        //黑名单
        List<int> blackList = new List<int>();

        private bool Add2BlackList(int playerId)
        {
            if (blackList.Contains(playerId))
            {
                return false;
            }
            else
            {
                blackList.Add(playerId);
            }
            return true;
        }

        private bool DelFromBlackList(int playerId)
        {
            if (blackList.Contains(playerId))
            {
                blackList.Remove(playerId);
            }
            else
            {
                return false;
            }
            return true;
        }

        private void ClearBlackList()
        {
            blackList.Clear();
        }

        public bool CheckBlackExist(int playerId)
        {
            return blackList.Contains(playerId);
        }

        private bool CheckBlackFull()
        {
            if (blackList.Count >= FriendLib.BLACK_LIST_MAX_COUNT)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public void LoadBlackList()
        {
            OperateGetBlackList operate = new OperateGetBlackList(Uid);
            server.GameRedis.Call(operate, ret =>
            {
                if ((int)ret == 1)
                {
                    ClearBlackList();
                    foreach (var item in operate.ids)
                    {
                        if (item.IsNullOrEmpty)
                        {
                        }
                        else
                        {
                            Add2BlackList((int)item);
                        }
                    }
                }
            });
        }

    
        public void AddBlack(int friendId)
        {
            MSG_ZGC_FRIEND_BLACK_ADD response = new MSG_ZGC_FRIEND_BLACK_ADD();
            response.Uid = friendId;

            if (CheckBlackExist(friendId))
            {
                response.Result = (int)ErrorCode.InBlack;
                response.Uid = friendId;
                Log.Warn("player {0} add player {1}  to black list errorcode:{2} ", Uid, friendId, response.Result);
                Write(response);
                return;
            }

            if (CheckBlackFull())
            {
                response.Result = (int)ErrorCode.BlackListFull;
                response.Uid = friendId;
                Log.Warn("player {0} add player {1}  to black list errorcode:{2} ", Uid, friendId, response.Result);
                Write(response);
                return;
            }

            if (Add2BlackList(friendId))
            {
                //清除一切关系 ,是否需要检查
                //从好友列表删除好友
                DelFriend(friendId);
                //添加到黑名单
                server.GameRedis.Call(new OperateBlackListAdd(Uid, friendId));
                response.Result = (int)ErrorCode.Success;
                Write(response);

                Log.Write("player {0} add player {1} to black list success ", Uid, friendId);
                return;
            }
            else
            {
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                Log.Error("player {0} add black list fail ", Uid, friendId);
                return;
            }
         
        }

        public void DelBlack(int friendId)
        {
            MSG_ZGC_FRIEND_BLACK_DEL response = new MSG_ZGC_FRIEND_BLACK_DEL();
            if (DelFromBlackList(friendId))
            {
                server.GameRedis.Call(new OperateBlackListDel(Uid, friendId));
                response.Result = (int)ErrorCode.Success;
                response.Uid = friendId;
                Write(response);
                Log.Write("player {0} delete player {1} from black list success ", Uid, friendId);
            }
            else
            {
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                Log.Error("player {0} delete player {1} from black list fail ", Uid, friendId);
                return;
            }
        }

        

        public void GetBlackList()
        {
            OperateGetBlackInfoList operate = new OperateGetBlackInfoList(Uid);

            server.GameRedis.Call(operate, ret =>
            {
                MSG_ZGC_FRIEND_BLACK_LIST response = new MSG_ZGC_FRIEND_BLACK_LIST();

                if ((int)ret == 1)
                {
                    if (operate.Characters != null)
                    {
                        foreach (var item in operate.Characters)
                        {
                            PLAYER_BASE_INFO info = PlayerInfo.GetPlayerBaseInfo(item.Value);
                            response.List.Add(info);
                        }
                    }
                    else
                    {
                        Log.Error("player {0} execute OperateGetAllBlackListInfo fail:operate.Characters is null !", Uid);
                    }
                }
                else
                {
                    Log.Error("player {0} execute OperateGetAllBlackListInfo fail: redis data error!", Uid);
                }
                Write(response);
            });
        }

    }
}
