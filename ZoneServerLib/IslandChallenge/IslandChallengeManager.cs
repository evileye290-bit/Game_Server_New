using System;
using System.Collections.Generic;
using System.Linq;
using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Google.Protobuf.Collections;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using RedisUtility;
using ServerModels;
using ServerShared;

namespace ZoneServerLib
{
    public partial class IslandChallengeManager
    {
        private DateTime nextUpdateTime = DateTime.Now;
        //每个节点的任务
        private Dictionary<int, IslandChallengeTaskInfo> nodeTaskList;
        private Dictionary<int, Dictionary<int, int>> heroSkilEnergy;

        public PlayerChar Owner { get; private set; }
        public int BattlePower { get; private set; }
        public int HeroLevel { get; private set; }
        public int Period { get; private set; }//当前周期
        public int GroupId { get; private set; }//当前任务组
        public int NodeId { get; private set; }//当前节点进度
        public int TaskId { get; private set; }//当前任务Id
        public List<int> RewardList { get; private set; }
        public List<int> DeadHeroList { get; private set; }//死亡hero
        public List<int> ShopList { get; private set; }//buff
        public Dictionary<int, int> WinInfo { get; private set; }
        public DoubleDepthMap<int, int, int> HeroPos { get; private set; }//hero布阵
        public Dictionary<int, float> HeroHp { get; private set; }//heroHp
        public Dictionary<int, ItemBasicInfo> SoulBoneList { get; private set; }//魂骨信息
        public bool NodeRewarded { get; private set; }//节点奖励

        public bool DBChanged { get; set; }

        public DateTime StartTime { get; private set; }
        public DateTime StopTime { get; private set; }
        public int ReviveCount { get; private set; }

        //当前正在进行的任务
        public BaseIslandChallengeTask Task { get; private set; }

        public IslandChallengeManager(PlayerChar player)
        {
            this.Owner = player;
        }

        #region Init

        public void Init(IslandChallengeDBInfo info)
        {
            HeroPos = new DoubleDepthMap<int, int, int>();

            Period = info.Period;
            BattlePower = info.BattlePower;
            HeroLevel = info.HeroLevel;
            StartTime = info.StartTime;
            StopTime = StartTime.AddDays(IslandChallengeLibrary.OpenDays);

            RewardList =info.RewardList.ToList( '|');
            DeadHeroList = info.DeadHeroList.ToList('|');
            ShopList = info.ShopList.ToList('|');
            HeroHp = StringSplit.SplitString2Float(info.HeroHp);
            WinInfo = info.WinInfo.ToDictionary('|', ':');
            NodeRewarded = info.NodeRewarded;

            ReviveCount = info.ReviveCount;

            SetHeroPosInfo(info.HeroPos);
            SetTaskInfo(info.NodeTaskInfo, info.SoulBoneList);

            SetNodeId(info.NodeId);
            SetTaskId(info.TaskId);
            SetSkillEnergy(info.SkillEnergy);

            CheckTime();
        }

        public void SetTaskId(int taskId)
        {
            TaskId = taskId;

            IslandChallengeTaskModel model = IslandChallengeLibrary.GetIslandChallengeTaskModel(taskId);
            if (model != null)
            {
                SetGroupId(model.GroupId);
                InitTask(model);
            }
        }

        private void SetNodeId(int nodeId)
        {
            if (nodeId > IslandChallengeLibrary.MaxNode) return;
            NodeId = nodeId;
        }

        private void SetGroupId(int groupId)
        {
            GroupId = groupId;
        }

        private void InitTaskList(int groupId)
        {
            Dictionary<int, IslandChallengeTaskModel> nodeTask = IslandChallengeLibrary.GetGroupTasks(groupId);
            if (nodeTask == null)
            {
                Log.Error($"player {Owner.Uid} init IslandChallenge task error, have not group {groupId} task !");
                return;
            }

            nodeTaskList.Clear();
            Owner.Cache5MaxBattlePowerHeroInfo();

            foreach (var kv in nodeTask)
            {
                var task = kv.Value;
                IslandChallengeTaskInfo taskInfo = new IslandChallengeTaskInfo(this, task);
                if (task.Type != TowerTaskType.Shop)
                {
                    BuildTaskParam(task, taskInfo);
                }
                else
                {
                    taskInfo.param = new List<int>();
                }

                nodeTaskList[kv.Key] = taskInfo;
            }
        }

        public void InitTask(IslandChallengeTaskModel model)
        {
            if (model == null) return;

            Task = CreateTowerTask(model);
        }


        private void RandomTask()
        {
            Reset();

            //当前周期加1
            Period++;

            InitHeroPos();

            GroupId = IslandChallengeLibrary.RandomGroup();

            LoadBattlePower();

        }

        private void LoadBattlePower()
        {
            if (BattlePower == 0 || HeroLevel == 0)
            {
                //第一期开启，使用相同配置的战斗力
                BattlePower = IslandChallengeLibrary.FirstBattlePower;
                HeroLevel = IslandChallengeLibrary.FirstHeroLevel;

                SyncBattlePowerToDB();
                RandomTaskNext();
            }
            else
            {
                OperateLoadMaxBattlePower operateLoad = new OperateLoadMaxBattlePower(Owner.Uid);
                Owner.server.GameRedis.Call(operateLoad, (object msg) =>
                {
                    if ((int)msg == 1)
                    {
                        BattlePower = operateLoad.BattlePower;
                        HeroLevel = operateLoad.HeroLevel;
                        if (BattlePower == 0)
                        {
                            BattlePower = Owner.HeroMng.CalcBattlePower();
                        }
                        if (HeroLevel == 0)
                        {
                            HeroLevel = Owner.ResonanceLevel;
                        }
                        SyncBattlePowerToDB();
                    }

                    RandomTaskNext();
                });
            }
        }

        private void RandomTaskNext()
        {
            SetOpenTime();

            InitTaskList(GroupId);

            SyncIslandChallengeToDB();
            Owner.GetIslandChallengeInfo();
        }

        private void Reset()
        {
            Task = null;

            NodeId = 0;
            TaskId = 0;
            ReviveCount = 0;
            NodeRewarded = false;

            WinInfo?.Clear();
            RewardList?.Clear();
            DeadHeroList?.Clear();
            ShopList?.Clear();
            HeroHp?.Clear();
            //HeroPos?.Clear();
            heroSkilEnergy?.Clear();
            SoulBoneList?.Clear();
        }


        private void InitHeroPos()
        {
            if (HeroPos.Count == 0)
            {
                foreach (var item in Owner.HeroMng.GetAllHeroPos())
                {
                    HeroPos.Add(1, item.Item1, item.Item2);
                }
            }

            //所有的都限制了只上阵默认位置
            if (HeroPos.Count == 0)
            {
                foreach (var kv in Owner.HeroMng.GetHeroInfoList())
                {
                    HeroPos.Add(1, kv.Value.Id, 1);
                }
            }
        }

        #endregion

        #region Open Check

        public bool IsOpening()
        {
            DateTime now = Owner.server.Now();
            return now >= StartTime && now <= StopTime;
        }

        public void Update()
        {
            if (nextUpdateTime < Owner.server.Now())
            {
                CheckTime();
                nextUpdateTime = Owner.server.Now().AddSeconds(10);
            }
        }

        public void CheckTime()
        {
            if (!Owner.CheckLimitOpen(LimitType.IslandChallenge)) return;

            if (StopTime < Owner.server.Now())
            {
                RandomTask();
            }
        }

        private void SetOpenTime()
        {
            StartTime = GetStartTime();
            StopTime = StartTime.AddDays(IslandChallengeLibrary.OpenDays);
            Owner.SendIslandChallengeTime();
        }

        private DateTime GetStartTime()
        {
            DateTime nowDate = Owner.server.Now().Date;
            DateTime opendServerDate = Owner.server.OpenServerDate;

            DateTime openDate = nowDate;
            int days = (int)((nowDate - opendServerDate).TotalDays) % (IslandChallengeLibrary.OpenDays + IslandChallengeLibrary.CloseDays);
            if (days < IslandChallengeLibrary.OpenDays)
            {
                openDate = openDate.AddDays(-1 * days);
            }
            else
            {
                openDate = openDate.AddDays(IslandChallengeLibrary.CloseDays);
            }
            return openDate;
        }

        #endregion


        public IslandChallengeTaskInfo GetTaskInfo(int nodeId)
        {
            IslandChallengeTaskInfo taskInfos;
            nodeTaskList.TryGetValue(nodeId, out taskInfos);
            return taskInfos;
        }

        public void SetDungeonResult(int dungeonId, bool win, int period)
        {
            //战斗过程中周期变了
            if(period!= Period)
            {
                Log.Warn($"period changed on battle end last period {period} this period {Period}");
                return;
            }

            if(WinInfo.ContainsKey(dungeonId)) return;

            WinInfo.Add(dungeonId, win ? 1 : 0);

            if (IsPassedDungeonNode())
            {
                SetNodeRewarded(true);
            }

            SyncDungeonResultToDB();

            Owner.SendIslandChallengeWinInfo();
        }

        public void SetNodeRewarded(bool reward)
        {
            NodeRewarded = reward;
        }

        public bool IsPassedDungeonNode()
        {
            return GetWinCount() >= 2;
        }

        public int GetWinCount()
        {
            return WinInfo.Where(x => x.Value == 1).Count();
        }

        public void ResetHeroInfo()
        {
            HeroHp.Clear();
            heroSkilEnergy.Clear();
            SyncHeroToDB();
        }

        public void ResetWinInfo( bool sync = true)
        {
            ResetHeroInfo();

            WinInfo.Clear();
            SyncDungeonResultToDB();
            if (sync)
            {
                Owner.SendIslandChallengeWinInfo();
            }
        }

        public void GotoNextNode()
        {
            CheckAndAddReward();
            SetTaskId(0);
            SetNodeId(Task.NodeId);

            Task = null;
            NodeRewarded = false;

            ShopList.Clear();
            SoulBoneList.Clear();

            HeroHp.Clear();
            heroSkilEnergy.Clear();

            SyncTaskInfoToDB();
            SyncHeroToDB();

            Owner.SendIslandChallengeInfo();
            Owner.SendIslandChallengeDungeonGrowth();
        }

        private void CheckAndAddReward()
        {
            IslandChallengeRewardModel model = IslandChallengeLibrary.GetIslandChallengeRewardModel(Task.NodeId);
            if (model != null && !string.IsNullOrEmpty(model.Data.GetString("Reward")))
            {
                AddReward(Task.NodeId);
                SyncRewardToDB();
            }
        }

        private int RandomDungeonId(IslandChallengeTaskModel model)
        {
            IslandChallengeDungeonModel dungeonModel = IslandChallengeLibrary.RandomDungeon(model.Difficulty);
            return dungeonModel.Id;
        }


        private BaseIslandChallengeTask CreateTowerTask(IslandChallengeTaskModel model)
        {
            BaseIslandChallengeTask task;
            switch (model.Type)
            {
                case TowerTaskType.Dungeon: task = new IslandChallengeDungeonTask(this, model.Id); break;
                case TowerTaskType.Shop: task = new IslandChallengeShopTask(this, model.Id); break;
                default: return null;
            }

            IslandChallengeTaskInfo info = GetIslandChallengeTaskInfo(model.NodeId);

            task.SetNodeId(model.NodeId);
            task.SetTaskInfo(info);

            return task;
        }

        public IslandChallengeTaskInfo GetIslandChallengeTaskInfo(int nodeId)
        {
            IslandChallengeTaskInfo taskInfos;
            nodeTaskList.TryGetValue(nodeId, out taskInfos);
            return taskInfos;
        }

        #region reward 

        public void AddReward(int id)
        {
            if (!RewardList.Contains(id))
            {
                RewardList.Add(id);
            }
        }

        public void RemoveId(int id)
        {
            if (RewardList.Contains(id))
            {
                RewardList.Remove(id);
                SyncRewardToDB();
            }
        }

        #endregion

        #region serialize

        private void SetHeroPosInfo(string posInfo)
        {
            string[] heroPosList = posInfo.Split('|');
            for (int i = 0; i < heroPosList.Length; i++)
            {
                List<int> info = heroPosList[i].ToList('-');
                if(info.Count!= 3) continue;

                HeroPos.Add(info[0], info[1], info[2]);
            }
        }

        private void SetTaskInfo(string nodeTaskStr, string soulBoneStr)
        {
            nodeTaskList = new Dictionary<int, IslandChallengeTaskInfo>();
            SoulBoneList = new Dictionary<int, ItemBasicInfo>();

            if (string.IsNullOrEmpty(nodeTaskStr)) return;

            string[] taskList = nodeTaskStr.Split('|');
            for (int i = 0; i < taskList.Length; i++)
            {
                int nodeId = i + 1;
                List<int> paramList = StringSplit.ToList(taskList[i], '-');
                if (paramList.Count > 0)
                {
                    IslandChallengeTaskModel model = IslandChallengeLibrary.GetIslandChallengeTaskModel(paramList[0]);
                    if (model == null)
                    {
                        Log.Warn($"IslandChallenge task error task id {paramList[0]}");
                        continue;
                    }

                    SetGroupId(model.GroupId);
                    IslandChallengeTaskInfo taskInfo = new IslandChallengeTaskInfo(this, model, paramList);
                    nodeTaskList[nodeId] = taskInfo;
                }
            }

            if (string.IsNullOrEmpty(soulBoneStr)) return;

            string[] soulBoneItems = soulBoneStr.Split('|');
            for (int i = 0; i < soulBoneItems.Length; i++)
            {
                ItemBasicInfo basicInfo = ItemBasicInfo.Parse(soulBoneItems[i]);
                SoulBoneList[basicInfo.Id] = basicInfo;
            }
        }

        //heroid:skillid-energy:skillid1-energy|heroid:skillid-energy:skillid1-energy
        private void SetSkillEnergy(string skillEnergyStr)
        {
            heroSkilEnergy = new Dictionary<int, Dictionary<int, int>>();
            if (string.IsNullOrEmpty(skillEnergyStr)) return;

            string[] heroList = skillEnergyStr.Split('|');
            for (int i = 0; i < heroList.Length; i++)
            {
                string[] heroAndSkillStr = heroList[i].Split(':');
                if (heroAndSkillStr.Length < 2) continue;

                int heroId;
                if (!int.TryParse(heroAndSkillStr[0], out heroId)) continue;

                for (int j = 1; j < heroAndSkillStr.Length; j++)
                {
                    string[] skillEnergy = heroAndSkillStr[j].Split('-');

                    if (skillEnergy.Length != 2) continue;

                    int skill, energy;
                    if (!int.TryParse(skillEnergy[0], out skill) || !int.TryParse(skillEnergy[1], out energy)) continue;

                    AddHeroSkillEnergy(heroId, skill, energy);
                }
            }
        }

        private string GetHeroPosStr()
        {
            List<string> tempList = new List<string>();
            foreach (KeyValuePair<int, Dictionary<int, int>> queue in HeroPos)
            {
                queue.Value.ForEach(x=> tempList.Add($"{queue.Key}-{x.Key}-{x.Value}"));
            }
            return tempList.ToString("|");
        }

        public string GetTrackHeroPosStr()
        {
            List<string> tempList = new List<string>();
            foreach (KeyValuePair<int, Dictionary<int, int>> queue in HeroPos)
            {
                queue.Value.ForEach(x => tempList.Add($"{queue.Key}:{x.Key}:{x.Value}"));
            }
            return tempList.ToString("_");
        }

        private string GetTaskStr()
        {
            if (nodeTaskList == null) return string.Empty;
            List<string> nodeTaskStr = nodeTaskList.Values.ToList().ConvertAll(item => item.ToString());
            return string.Join("|", nodeTaskStr);
        }

        private string GetSoulBoneStr()
        {
            if (SoulBoneList == null || SoulBoneList.Count <= 0) return string.Empty;

            List<string> strList = new List<string>();
            SoulBoneList.ForEach(x => strList.Add(x.Value == null ? string.Empty : x.Value.ToString()));
            return string.Join("|", strList);
        }

        private string GetHeroSkillEnergyStr()
        {
            if (heroSkilEnergy == null) return string.Empty;

            List<string> heroSkillStr = new List<string>();
            List<string> skillStr = new List<string>();

            foreach (var kv in heroSkilEnergy)
            {
                if (kv.Value.Count == 0) continue;

                skillStr.Clear();

                skillStr.Add(kv.Key.ToString());
                kv.Value.ForEach(x => skillStr.Add(x.Key + "-" + x.Value));

                heroSkillStr.Add(string.Join(":", skillStr));
            }

            return string.Join("|", heroSkillStr);
        }

        private static string ConnectListItem<T>(List<T> list, string connector)
        {
            return list == null || list.Count == 0 ? string.Empty : string.Join(connector, list);
        }

        public string ConnectDictionary<K, V>(Dictionary<K, V> dic)
        {
            List<string> items = new List<string>();
            dic.ForEach(x => items.Add(x.Key + ":" + x.Value));

            return string.Join("|", items);
        }

        public IslandChallengeDBInfo GenerateTaskDBInfo()
        {
            IslandChallengeDBInfo info = new IslandChallengeDBInfo()
            {
                Period = Period,
                NodeId = NodeId,
                TaskId = TaskId,
                BattlePower = BattlePower,
                HeroLevel = HeroLevel,
                StartTime = StartTime,
                RewardList = ConnectListItem(RewardList, "|"),
                DeadHeroList = ConnectListItem(DeadHeroList, "|"),
                ShopList = ConnectListItem(ShopList, "|"),
                NodeTaskInfo = GetTaskStr(),
                HeroPos = GetHeroPosStr(),
                HeroHp = ConnectDictionary(HeroHp),
                ReviveCount = ReviveCount,
                SkillEnergy = GetHeroSkillEnergyStr(),
                SoulBoneList = GetSoulBoneStr(),
                WinInfo = WinInfo.ToString("|",":"),
                NodeRewarded = NodeRewarded
            };

            return info;
        }


        private void CheckSoulBoneChanged()
        {
            if (DBChanged)
            {
                DBChanged = false;
                SyncSoulBoneToDB();
            }
        }

        #region db sync

        public void SyncIslandChallengeToDB()
        {
            QueryUpdateIslandChallenge query = new QueryUpdateIslandChallenge(Owner.Uid, GenerateTaskDBInfo()); ;
            Owner.server.GameDBPool.Call(query);
        }

        public void SyncDungeonResultToDB()
        {
            QueryUpdateIslandChallengeWinInfo query = new QueryUpdateIslandChallengeWinInfo(Owner.Uid, WinInfo.ToString("|", ":"), NodeRewarded);
            Owner.server.GameDBPool.Call(query);
        }

        public void SyncHeroToDB()
        {
            QueryUpdateIslandChallengeHeroAndMonsterHP query = new QueryUpdateIslandChallengeHeroAndMonsterHP(Owner.Uid,
                GetHeroPosStr(), ConnectDictionary(HeroHp), ConnectListItem(DeadHeroList, "|"), ReviveCount, GetHeroSkillEnergyStr(), WinInfo.ToString( "|",":"));
            Owner.server.GameDBPool.Call(query);
        }

        public void SyncShopItemToDB()
        {
            QueryUpdateIslandChallengeShopItem query = new QueryUpdateIslandChallengeShopItem(Owner.Uid, ConnectListItem(ShopList, "|"));
            Owner.server.GameDBPool.Call(query);
        }

        public void SyncTaskInfoToDB()
        {
            QueryUpdateIslandChallengeNodeTask query = new QueryUpdateIslandChallengeNodeTask(Owner.Uid, NodeId, TaskId, ConnectListItem(ShopList, "|"), GetSoulBoneStr());
            Owner.server.GameDBPool.Call(query);
        }

        public void SyncRewardToDB()
        {
            QueryUpdateIslandChallengeReward query = new QueryUpdateIslandChallengeReward(Owner.Uid, ConnectListItem(RewardList, "|"), NodeRewarded);
            Owner.server.GameDBPool.Call(query);
        }

        public void SyncBattlePowerToDB()
        {
            QueryIslandChallengeUpdateBattlePower query = new QueryIslandChallengeUpdateBattlePower(Owner.Uid, BattlePower, HeroLevel);
            Owner.server.GameDBPool.Call(query);
        }

        public void SyncSoulBoneToDB()
        {
            QueryUpdateIslandChallengeSoulBoneInfo query = new QueryUpdateIslandChallengeSoulBoneInfo(Owner.Uid, GetSoulBoneStr());
            Owner.server.GameDBPool.Call(query);
        }

        #endregion

        #endregion

        #region msg

        public MSG_ZGC_ISLAND_CHALLENGE_INFO GenerateMsg()
        {
            MSG_ZGC_ISLAND_CHALLENGE_INFO msg = new MSG_ZGC_ISLAND_CHALLENGE_INFO();
            msg.GroupId = GroupId;
            msg.NodeId = NodeId;
            msg.TaskId = TaskId;
            msg.NodeRewarded = NodeRewarded;
            msg.RewardList.AddRange(RewardList);
            msg.ShopList.AddRange(ShopList);

            WinInfo.ForEach(x =>
            {
                MSG_ZGC_ISLAND_CHALLENGE_WIN_INFO info = new MSG_ZGC_ISLAND_CHALLENGE_WIN_INFO() { DungeonId = x.Key, Win = x.Value };
                msg.WinInfo.Add(info);
            });

            foreach (var kv in nodeTaskList)
            {
                msg.NodeList.Add(kv.Value.GenerateMsg());
            }

            CheckSoulBoneChanged();

            return msg;
        }

        public MSG_ZGC_ISLAND_CHALLENGE_HERO_INFO GenerateHeroInfo()
        {
            MSG_ZGC_ISLAND_CHALLENGE_HERO_INFO msg = new MSG_ZGC_ISLAND_CHALLENGE_HERO_INFO();
            msg.ReviveCount = ReviveCount;
            msg.DeadHeroList.AddRange(DeadHeroList);

            GenerateHeroPosInfo(msg.HeroPos);
            HeroHp.ForEach(x => msg.HeroHp.Add(new MSG_TOWER_HERO_HP() { HeroId = x.Key, HpRatio = x.Value }));

            heroSkilEnergy.ForEach(x => x.Value.ForEach(y =>
            {
                SkillModel model = SkillLibrary.GetSkillModel(y.Key);
                if (model?.Type == SkillType.Body)
                {
                    msg.HeroEnergyList.Add(new MSG_ZGC_TOWER_HERO_ENERGY() { HeroId = x.Key, SkillId = y.Key, Energy = y.Value });
                }
            }));

            return msg;
        }

        private void GenerateHeroPosInfo(RepeatedField<MSG_ZGC_HERO_POS> heroPoses)
        {
            foreach (KeyValuePair<int, Dictionary<int, int>> queue in HeroPos)
            {
                queue.Value.ForEach(x => heroPoses.Add(new MSG_ZGC_HERO_POS()
                {
                    Queue = queue.Key, 
                    HeroId = x.Key, 
                    PosId = x.Value
                }));
            }
        }

        public MSG_ZGC_ISLAND_CHALLENGE_DUNGOEN_GROWTH GenerateDungeonGrowth()
        {
            MSG_ZGC_ISLAND_CHALLENGE_DUNGOEN_GROWTH msg = new MSG_ZGC_ISLAND_CHALLENGE_DUNGOEN_GROWTH();

            int currNode = NodeId == IslandChallengeLibrary.MaxNode ? NodeId : NodeId + 1;
            if (currNode == 0) currNode = 1;

            IslandChallengeTaskInfo currNodeTask;
            if (nodeTaskList.TryGetValue(currNode, out currNodeTask))
            {
                if (currNodeTask.Model.Type == TowerTaskType.Dungeon)
                {
                    msg.DungeonGrowthList.Add(new MSG_ZGC_ISLAND_CHALLENGE_DUNGOEN_TASK_GROWTH()
                    {
                        TaskId = currNodeTask.Model.Id,
                        DungeonGrowth = GetMonsterGrowth(currNodeTask.Model.Chapter, currNodeTask.Model.NodeId, currNodeTask.Model.Difficulty)
                    });

                }
            }
            return msg;
        }

        public MSG_ZGC_ISLAND_CHALLENGE_UPDATE_WININFO GenerateWinInfo()
        {
            MSG_ZGC_ISLAND_CHALLENGE_UPDATE_WININFO msg = new MSG_ZGC_ISLAND_CHALLENGE_UPDATE_WININFO() {NodeRewarded = NodeRewarded};
            WinInfo.ForEach(x =>
            {
                MSG_ZGC_ISLAND_CHALLENGE_WIN_INFO info = new MSG_ZGC_ISLAND_CHALLENGE_WIN_INFO() { DungeonId = x.Key, Win = x.Value };
                msg.WinInfo.Add(info);
            });
            return msg;
        }

        public MSG_ZMZ_ISLAND_CHALLENGE_INFO GenerateTransformMsg()
        {
            MSG_ZMZ_ISLAND_CHALLENGE_INFO msg = new MSG_ZMZ_ISLAND_CHALLENGE_INFO();

            msg.Period = Period;
            msg.GroupId = GroupId;
            msg.NodeId = NodeId;
            msg.TaskId = TaskId;
            msg.BattlePower = BattlePower;
            msg.HeroLevel = HeroLevel;
            msg.ReviveCount = ReviveCount;
            msg.StartTime = Timestamp.GetUnixTimeStamp(StartTime);
            msg.StopTime = Timestamp.GetUnixTimeStamp(StopTime);
            msg.NodeRewarded = NodeRewarded;

            msg.RewardList.AddRange(RewardList);
            msg.DeadHeroList.AddRange(DeadHeroList);
            msg.ShopList.AddRange(ShopList);


            WinInfo.ForEach(x => msg.WinList.Add(x.Key, x.Value));
            HeroHp.ForEach(x => msg.HeroHp.Add(x.Key, x.Value));
            heroSkilEnergy.ForEach(x => x.Value.ForEach(y => msg.HeroEnergyList.Add(new MSG_ZM_TOWER_HERO_ENERGY() { HeroId = x.Key, SkillId = y.Key, Energy = y.Value, })));

            foreach (KeyValuePair<int, Dictionary<int, int>> queue in HeroPos)
            {
                queue.Value.ForEach(x => msg.HeroPos.Add(new MSG_ZMZ_HERO_POS()
                {
                    Queue = queue.Key,
                    HeroId = x.Key,
                    PosId = x.Value
                }));
            }

            msg.TaskInfo = GetTaskStr();
            msg.SoulBoneInfo = GetSoulBoneStr();

            return msg;
        }

        #endregion

        public void LoadTransform(MSG_ZMZ_ISLAND_CHALLENGE_INFO info)
        {
            ShopList = new List<int>();
            RewardList = new List<int>();
            DeadHeroList = new List<int>();
            heroSkilEnergy = new Dictionary<int, Dictionary<int, int>>();

            Period = info.Period;
            GroupId = info.GroupId;
            BattlePower = info.BattlePower;
            HeroLevel = info.HeroLevel;
            ReviveCount = info.ReviveCount;
            NodeRewarded = info.NodeRewarded;
            StartTime = Timestamp.TimeStampToDateTime(info.StartTime);
            StopTime = Timestamp.TimeStampToDateTime(info.StopTime);

            ShopList.AddRange(info.ShopList);
            RewardList.AddRange(info.RewardList);
            DeadHeroList.AddRange(info.DeadHeroList);

            WinInfo = new Dictionary<int, int>(info.WinList);
            HeroHp = new Dictionary<int, float>(info.HeroHp);
            HeroPos = new DoubleDepthMap<int, int, int>();

            info.HeroPos.ForEach(x => HeroPos.Add(x.Queue, x.HeroId, x.PosId));
            info.HeroEnergyList.ForEach(x => AddHeroSkillEnergy(x.HeroId, x.SkillId, x.Energy));

            SetTaskInfo(info.TaskInfo, info.SoulBoneInfo);

            SetNodeId(info.NodeId);
            SetTaskId(info.TaskId);
        }
    }
}
