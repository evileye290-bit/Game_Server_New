using EnumerateUtility;
using Google.Protobuf.Collections;
using Message.Relation.Protocol.RC;
using Message.Relation.Protocol.RZ;
using RedisUtility;
using ServerModels;
using ServerShared;
using System.Collections.Generic;
using System.Linq;

namespace RelationServerLib
{
    public class CrossGuessingManager
    {
        private RelationServerApi server { get; set; }

        public int TeamId { get; set; }

        private Dictionary<CrossBattleTiming, CrossGuessingManagerModel> timingGuessingList = new Dictionary<CrossBattleTiming, CrossGuessingManagerModel>();

        public CrossGuessingManager(RelationServerApi server)
        {
            this.server = server;

            LoadGuessingPlayers();
        }

        /// <summary>
        /// 获取决赛队员
        /// </summary>
        public void LoadGuessingPlayers()
        {
            InitGuessingList();

            LoadGuessingInfo();
        }

        private void InitGuessingList()
        {
            timingGuessingList.Clear();
            GetGuessingItemOrNew(CrossBattleTiming.BattleTime1);
            GetGuessingItemOrNew(CrossBattleTiming.BattleTime2);
            GetGuessingItemOrNew(CrossBattleTiming.BattleTime3);
            GetGuessingItemOrNew(CrossBattleTiming.BattleTime4);
            GetGuessingItemOrNew(CrossBattleTiming.BattleTime5);
            GetGuessingItemOrNew(CrossBattleTiming.BattleTime6);
        }

        private void LoadGuessingInfo()
        {
            //加载前8
            OperateGetCrossGuessingInfo op = new OperateGetCrossGuessingInfo(server.MainId, timingGuessingList.Keys.ToList());
            server.GameRedis.Call(op, ret =>
            {
                List<CrossGuessingRedisInfo> infoLsit = op.InfoLsit;
                if (infoLsit.Count > 0)
                {
                    foreach (var info in infoLsit)
                    {
                        int timingId = info.GetIntValue(HFCrossGuessingPlayer.Timing);
                        int uid1 = info.GetIntValue(HFCrossGuessingPlayer.Uid1);
                        int uid2 = info.GetIntValue(HFCrossGuessingPlayer.Uid2);
                        int teamId = info.GetIntValue(HFCrossGuessingPlayer.Team);
                        CrossGuessingManagerModel manager = GetGuessingItemOrNew((CrossBattleTiming)timingId);
                        manager.Player1 = uid1;
                        manager.Player2 = uid2;
                        manager.TeamId = teamId;
                        TeamId = teamId;
                    }
                }

                LoadGuessingChoose();
            });
        }

        public void LoadGuessingChoose()
        {
            //加载前8
            OperateGetCrossGuessingChooseInfo op = new OperateGetCrossGuessingChooseInfo(server.MainId, timingGuessingList);
            server.GameRedis.Call(op, ret =>
            {
                timingGuessingList = op.timingGuessingList;

                LoadGuessingReward();
            });
        }

        public void LoadGuessingReward()
        {
            //加载前8
            OperateGetCrossGuessingRewardInfo op = new OperateGetCrossGuessingRewardInfo(server.MainId, timingGuessingList);
            server.GameRedis.Call(op, ret =>
            {
                timingGuessingList = op.timingGuessingList;
            });
        }


        public void ClearGuessingInfo()
        {
            TeamId = 0;
            InitGuessingList();
            server.GameRedis.Call(new OperateClearGuessingInfo(server.MainId, timingGuessingList.Keys.ToList()));
        }

        public CrossGuessingManagerModel GetGuessingItemOrNew(CrossBattleTiming timing)
        {
            CrossGuessingManagerModel fight;
            if (!timingGuessingList.TryGetValue(timing, out fight))
            {
                fight = new CrossGuessingManagerModel();
                timingGuessingList.Add(timing, fight);
            }
            return fight;
        }

        public CrossGuessingManagerModel GetGuessingItem(CrossBattleTiming timing)
        {
            CrossGuessingManagerModel fight;
            timingGuessingList.TryGetValue(timing, out fight);
            return fight;
        }

        public void GetGuessingPlayersInfo(int uid)
        {
            Dictionary<int, int> uids = new Dictionary<int, int>();
            foreach (var timingGuessing in timingGuessingList)
            {
                if (timingGuessing.Value.Player1 > 0)
                {
                    uids[timingGuessing.Value.Player1] = 0;
                }
                if (timingGuessing.Value.Player2 > 0)
                {
                    uids[timingGuessing.Value.Player2] = 0;
                }
            }
            MSG_RC_GET_GET_GUESSING_INFO msg = new MSG_RC_GET_GET_GUESSING_INFO();
            if (uids.Count > 0)
            {
                msg.Uids.AddRange(uids.Keys.ToList());
            }
            server.CrossServer.Write(msg, uid);

        }

        public List<MSG_RZ_GUESSING_ITEM_INFO> GetGuessingPlayersInfoMsg(int uid)
        {
            List<MSG_RZ_GUESSING_ITEM_INFO> list = new List<MSG_RZ_GUESSING_ITEM_INFO>();

            foreach (var timingGuessing in timingGuessingList)
            {
                MSG_RZ_GUESSING_ITEM_INFO info = new MSG_RZ_GUESSING_ITEM_INFO();
                info.Player1 = timingGuessing.Value.Player1;
                info.Player2 = timingGuessing.Value.Player2;
                info.TimingId = (int)timingGuessing.Key;
                info.Player1Choose = timingGuessing.Value.Choose1.Count;
                info.Player2Choose = timingGuessing.Value.Choose2.Count;
                info.Choose = timingGuessing.Value.GetChoose(uid);
                info.Winner = timingGuessing.Value.Winner;
                list.Add(info);
            }

            return list;
        }

        public void SetGuessingPlayers(int timingId, int uid1, int uid2, int teamId)
        {
            CrossGuessingManagerModel manager = GetGuessingItemOrNew((CrossBattleTiming)timingId);
            manager.Player1 = uid1;
            manager.Player2 = uid2;
            manager.TeamId = teamId;
            server.GameRedis.Call(new OperateSetCrossGuessingInfo(server.MainId, timingId, uid1, uid2, teamId));

            TeamId = teamId;
        }

        public void SetPlayerChoose(int timingId, int uid, int choose)
        {
            CrossGuessingManagerModel manager = GetGuessingItemOrNew((CrossBattleTiming)timingId);
            manager.AddChoose(uid, choose);
            server.GameRedis.Call(new OperateSetCrossGuessingChoose(server.MainId, timingId, uid, choose));
        }

        public void SetPlayerReward(int timingId, int uid, string reward)
        {
            CrossGuessingManagerModel manager = GetGuessingItemOrNew((CrossBattleTiming)timingId);
            manager.AddReward(uid, reward);
            server.GameRedis.Call(new OperateSetCrossGuessingReward(server.MainId, timingId, uid, reward));
        }

        /// <summary>
        /// 检查是否有下注
        /// </summary>
        /// <param name="timingId"></param>
        /// <returns></returns>
        public int CheckGuessingModel(int timingId)
        {
            int errorCode = (int)ErrorCode.Success;
            CrossGuessingManagerModel manager = GetGuessingItem((CrossBattleTiming)timingId);
            if (manager == null)
            {
                errorCode = (int)ErrorCode.NotOpen;
            }
            else
            {
                if (manager.Player1 == 0 || manager.Player2 == 0)
                {
                    errorCode = (int)ErrorCode.NotOpen;
                }
            }
            return errorCode;
        }

        public string GetGuessingChooseReward(int timingId, int uid)
        {
            string reward = string.Empty;
            CrossGuessingManagerModel manager = GetGuessingItem((CrossBattleTiming)timingId);
            if (manager != null)
            {
                reward = manager.GetReward(uid);
            }
            return reward;
        }

        public void SendGuessingReward(int timingId, RepeatedField<int> uids)
        {
            CrossGuessingManagerModel manager = GetGuessingItem((CrossBattleTiming)timingId);
            if (manager != null)
            {
                int emailId = CrossBattleLibrary.GuessingEmail;
                if (uids.Contains(manager.Player1))
                {
                    manager.Winner = 1;
                    foreach (var uid in manager.Choose1)
                    {
                        string reward = manager.GetReward(uid);
                        if (!string.IsNullOrEmpty(reward))
                        {
                            //通知玩家发送信息回来
                            server.EmailMng.SendPersonEmail(uid, emailId, reward);
                        }
                    }
                }
                if (uids.Contains(manager.Player2))
                {
                    manager.Winner = 2;
                    foreach (var uid in manager.Choose2)
                    {
                        string reward = manager.GetReward(uid);
                        if (!string.IsNullOrEmpty(reward))
                        {
                            //通知玩家发送信息回来
                            server.EmailMng.SendPersonEmail(uid, emailId, reward);
                        }
                    }
                }
            }
        }
    }
}
