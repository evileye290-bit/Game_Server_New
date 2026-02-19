using BattleServerLib;
using BattleServerLib.Client;
using Logger;
using Message.BattleManager.Protocol.BMBattle;
using Message.BattleManager.Protocol.BMZ;
using ServerFrame;
using ServerShared;
using System;
using System.Collections.Generic;

namespace BattleManagerServerLib
{
    public partial class BattleServerManager : FrontendServerManager
    {
        public BattleServerManager(BattleManagerServerApi api, ServerType serverType):base(api, serverType)
        {
        }

        private Dictionary<int, int> FindBarrleClients = new Dictionary<int, int>();

        private BattleServer curBattleServer = null;

        private Dictionary<int, FightClients> waiteChallengeList = new Dictionary<int, FightClients>();
        /// <summary>
        /// 等待挑战
        /// </summary>
        public Dictionary<int, FightClients> WaiteChallengeList
        {
            get { return waiteChallengeList; }
        }

        private Dictionary<int, FightClients> waiteTeamPlayerList = new Dictionary<int, FightClients>();
        /// <summary>
        /// 等待挑战
        /// </summary>


        private Dictionary<int, int> waiteTeamRoomList = new Dictionary<int, int>();
        /// <summary>
        /// 等待挑战房间队列
        /// </summary>
        public Dictionary<int, int> WaiteTeamRoomList
        {
            get { return waiteTeamRoomList; }
        }

        private int RoomId = 0;

        public override void UpdateServers(double dt)
        {
            base.UpdateServers(dt);
            try
            {
                //检查等待时间，降低匹配【频率
                //if (CheckUpdateOneVsOneMatchingTip(dt))
                //{
                //    UpdateOneVsOneMatchingBattle();
                //}

                //if (CheckUpdateTwoVsTwoMatchingTip(dt))
                //{
                //    UpdateTwoVsTwoMatchingBattle();
                //}

                //if (CheckUpdateTwoVsBossMatchingTip(dt))
                //{
                //    UpdateTwoVsBossMatchingBattle();
                //}
            }
            catch (Exception e)
            {
                Log.Alert(e.ToString());
            }
        }


        public override void DestroyServer(FrontendServer server)
        {
            base.DestroyServer(server);
            CalcBattleServer();
        }


        //public void AddFightingClient(BattleClient client)
        //{
        //    Dictionary<int, BattleClient> FightingList = GetFightingList(client.Uid);
        //    if (FightingList != null)
        //    {
        //        BattleClient tempClient;
        //        //检查玩家是否在战斗中
        //        if (FightingList.TryGetValue(client.Uid, out tempClient))
        //        {
        //            //玩家正在战斗中
        //            Log.Warn("player {0} add fighting battle list error because he is fighting.", client.Uid);
        //        }
        //        else
        //        {
        //            //可以加入等待战斗序列
        //            FightingList.Add(client.Uid, tempClient);
        //        }
        //    }
        //}

        //public bool RemoveFightingClient(int clientUid)
        //{
        //    Dictionary<int, BattleClient> FightingList = GetFightingList(clientUid);
        //    if (FightingList != null)
        //    {
        //          return FightingList.Remove(clientUid);
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        //public Dictionary<int, BattleClient> GetFightingList(int clientUid)
        //{
        //    switch (clientUid % 20)
        //    {
        //        case 0:
        //            return FightingList0;
        //        case 1:
        //            return FightingList1;
        //        case 2:
        //            return FightingList2;
        //        case 3:
        //            return FightingList3;
        //        case 4:
        //            return FightingList4;
        //        case 5:
        //            return FightingList5;
        //        case 6:
        //            return FightingList6;
        //        case 7:
        //            return FightingList7;
        //        case 8:
        //            return FightingList8;
        //        case 9:
        //            return FightingList9;
        //        case 10:
        //            return FightingList10;
        //        case 11:
        //            return FightingList11;
        //        case 12:
        //            return FightingList12;
        //        case 13:
        //            return FightingList13;
        //        case 14:
        //            return FightingList14;
        //        case 15:
        //            return FightingList15;
        //        case 16:
        //            return FightingList16;
        //        case 17:
        //            return FightingList17;
        //        case 18:
        //            return FightingList18;
        //        case 19:
        //            return FightingList19;
        //        default:
        //            Log.Warn("player {0} get fighting list error not find list id {1}.", clientUid, clientUid % 20);
        //            return null;
        //    }
        //}
        public void SendLeaveTeamMsg(BattleClient client)
        {
            MSG_BMZ_LEAVE_TEAM msg = new MSG_BMZ_LEAVE_TEAM();
            msg.Uid = client.Uid;
            client.CurZone.Write(msg);
        }

        public void SendLeaveMatchingMsg(BattleClient client)
        {
            MSG_BMZ_LEAVE_MATCHING msg = new MSG_BMZ_LEAVE_MATCHING();
            msg.Uid = client.Uid;
            client.CurZone.Write(msg);
        }

        public void AddWaitingChallengeClient(BattleClient client, int challengeUid)
        {
            //可以加入等待战斗序列
            // TODO 添加超时删除
            FightClients fightClients = new FightClients();
            fightClients.Client1 = client;

            fightClients.Client2 = new BattleClient();
            fightClients.Client2.Uid = challengeUid;
            waiteChallengeList[client.Uid] = fightClients;
        }
        //public FightClients CheckChallengeClient(BattleClient client, int challengeUid)
        //{
        //    FightClients fightClients = GetChallengeClient(challengeUid);
        //    //检查挑战玩家
        //    if (fightClients != null)
        //    {
        //        if (fightClients.Client1.Uid == challengeUid)
        //        {
        //            //可以加入等待战斗序列
        //            fightClients.Client2 = client;
        //            return fightClients;
        //        }
        //        else
        //        {
        //            Log.Warn("player {0} check waite challenge list client 1 player id {1}.", client.Uid, fightClients.Client1.Uid);
        //        }
        //    }
        //    else
        //    {
        //        Log.Warn("player {0} check waite challenge list not find player {1}.", client.Uid, challengeUid);
        //    }
        //    return null;
        //}


        public FightClients GetChallengeClient(int challengeUid)
        {
            FightClients fightClients;
            //检查挑战玩家
            WaiteChallengeList.TryGetValue(challengeUid, out fightClients);
            return fightClients;
        }

        public void RemoveWaitingChallengeClient(int clientUid)
        {
            FightClients fightClients;
            if (WaiteChallengeList.TryGetValue(clientUid, out fightClients))
            {
               waiteChallengeList.Remove(clientUid);
            }
        }

        public void CalcBattleServer()
        {
            // 按在线人数排序，总是试图分配在承载最低的zone
            List<BattleServer> battleServers = new List<BattleServer>();
            foreach (var item in serverList)
            {
                battleServers.Add((BattleServer)item.Value);
            }
            if (battleServers.Count > 0)
            {
                battleServers.Sort((left, right) =>
                {
                    if (left.State != ServerState.Started)
                    {
                        return 1;
                    }
                    if (left.FrameCount > right.FrameCount)
                    {
                        return -1;
                    }
                    else if (left.FrameCount == right.FrameCount)
                    {
                        if (left.SleepTime > right.SleepTime)
                        {
                            return -1;
                        }
                        else if (left.SleepTime == right.SleepTime)
                        {
                            if (left.Memory < right.Memory)
                            {
                                return -1;
                            }
                            else if (left.Memory == right.Memory)
                            {
                                if (left.SubId < right.SubId)
                                {
                                    return -1;
                                }
                                else
                                {
                                    return 1;
                                }
                            }
                            else
                            {
                                return 1;
                            }
                        }
                        else
                        {
                            return 1;
                        }
                    }
                    else
                    {
                        return 1;
                    }
                });
                curBattleServer = battleServers[0];
            }
        }

        public BattleServer GetBattleServer()
        {
            if (curBattleServer != null && curBattleServer.State == ServerState.Started)
            {
                int count = serverList.Count;
                int half = count / 4;
                int index = BaseApi.Random.Next(0, half+1);
                if (index < count)
                {
                    int i = 0;
                    foreach (var item in serverList)
                    {
                        if (i == index)
                        {
                            return (BattleServer)item.Value;
                        }
                        i++;
                    }
                }
                else
                {
                    return curBattleServer;
                }
            }
            return null;
        }

        public bool CheckFindBattleClientUid(int uid)
        {
            int isFind;
            return FindBarrleClients.TryGetValue(uid, out isFind);
        }

        public void AddWaiteTeamPlayerClient(FightClients fight)
        {
            int key;
            if (waiteTeamRoomList.TryGetValue(fight.Client1.Uid, out key))
            {
                //玩家已经有对应的房间
                Log.Warn("player {0} AddWaiteTeamPlayerClient has add client room id {1}", fight.Client1.Uid, key);
                //删除旧房间
                FightClients oldFight = RemoveWaiteTeamPlayerClients(fight.Client1.Uid);
                if (oldFight != null)
                {
                    if (oldFight.Client1 != null && oldFight.Client1.Uid != fight.Client1.Uid)
                    {
                        //通知离开匹配退出房间
                        SendLeaveMatchingMsg(oldFight.Client1);
                    }

                    if (oldFight.Client2 != null && oldFight.Client2.Uid != fight.Client1.Uid)
                    {
                        //通知离开匹配退出房间
                        SendLeaveMatchingMsg(oldFight.Client2);
                    }
                }
            }
            //新房间号
            RoomId++;

            if (fight.Client1 != null)
            {
                //替换玩家的房间
                waiteTeamRoomList[fight.Client1.Uid] = RoomId;
            }

            if (fight.Client2 != null)
            {
                //替换玩家的房间
                waiteTeamRoomList[fight.Client2.Uid] = RoomId;
            }
       
            //添加房间对待队列
            waiteTeamPlayerList[RoomId] = fight;

            fight.RoomId = RoomId;

        }

        public FightClients GetWaiteTeamPlayerClient(int pcUid)
        {
            FightClients fight = null;
            int roomId = GetWaiteTeamRoomId(pcUid);
            //先找房间号
            if (roomId > 0)
            {
                //再找对应的信息
                waiteTeamPlayerList.TryGetValue(roomId, out fight);
            }
            return fight;
        }

        public int GetWaiteTeamRoomId(int pcUid)
        {
            int key;
            waiteTeamRoomList.TryGetValue(pcUid, out key);
            return key;
        }

        public void AddWaiteTeamRoomId(int pcUid, int roomId)
        {
            waiteTeamRoomList[pcUid] = roomId;
        }

        public FightClients RemoveWaiteTeamPlayerClients(int pcUid)
        {
            int roomId = GetWaiteTeamRoomId(pcUid);
            if (roomId > 0)
            {
                FightClients fight = null;
                if (waiteTeamPlayerList.TryGetValue(roomId, out fight))
                {
                    waiteTeamPlayerList.Remove(roomId);

                    if (fight.Client1 != null)
                    {
                        RemoveWaiteTeamRoomClient(fight.Client1.Uid);
                    }

                    if (fight.Client2 != null)
                    {
                        RemoveWaiteTeamRoomClient(fight.Client2.Uid);
                    }
                    return fight;
                }
            }
            else
            {
                Log.Warn("player {0} RemoveWaiteTeamPlayerClients not find room id", pcUid);
            }
            return null;
        }

        public bool RemoveWaiteTeamRoomClient(int pcUid)
        {
            return waiteTeamRoomList.Remove(pcUid);
        }

    }
}
