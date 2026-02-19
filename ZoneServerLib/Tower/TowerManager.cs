using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Google.Protobuf.Collections;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using Message.Zone.Protocol.ZM;
using RedisUtility;
using ScriptFunctions;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using Logger;
namespace ZoneServerLib
{
    public class TowerManager
    {
        private DateTime nextUpdateTime = DateTime.Now;
        //每个节点的任务
        private Dictionary<int, List<TowerTaskInfo>> nodeTaskList;
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
        //public List<int> FightingHeroList { get; private set; }//助战hero
        public List<int> BuffInfoList { get; private set; }//buff
        public List<int> ShopList { get; private set; }//buff
        public Dictionary<int, int> HeroPos { get; private set; }//hero布阵
        public Dictionary<int, float> HeroHp { get; private set; }//heroHp
        public Dictionary<int, float> MonsterHP { get; private set; }//monsterHp
        public Dictionary<int, ItemBasicInfo> SoulBoneList { get;private set;}//魂骨信息

        public bool DBChanged { get; set; }

        public DateTime StartTime { get; private set; }
        public DateTime StopTime { get; private set; }
        public int ReviveCount { get; private set; }

        //当前正在进行的任务
        public TowerTask TowerTask { get; private set; }

        public TowerManager(PlayerChar player)
        {
            this.Owner = player;
        }

        public void Init(TowerDBInfo info)
        {
            Period = info.Period;
            BattlePower = info.BattlePower;
            HeroLevel = info.HeroLevel;
            StartTime = info.StartTime;
            StopTime = StartTime.AddDays(TowerLibrary.OpenDays);

            RewardList = SplitString(info.RewardList, '|');
            DeadHeroList = SplitString(info.DeadHeroList, '|');
            BuffInfoList = SplitString(info.BuffListList, '|');
            ShopList = SplitString(info.ShopList, '|');
            HeroPos = SplitString(info.HeroPos);
            HeroHp = SplitString2Float(info.HeroHp);
            MonsterHP = SplitString2Float(info.MonsterHP);

            ReviveCount = info.ReviveCount;
            SetTaskInfo(info.NodeTaskInfo, info.SoulBoneList);

            SetNodeId(info.NodeId);
            SetTaskId(info.TaskId);
            SetSkillEnergy(info.SkillEnergy);

            CheckTime();
        }

        public bool IsOpening()
        {
            DateTime now = Owner.server.Now();
            return now >= StartTime && now <= StopTime;
        }

        public TowerTaskInfo GetTaskInfo(int nodeId, int taskId)
        {
            List<TowerTaskInfo> taskInfos;
            if (nodeTaskList.TryGetValue(nodeId, out taskInfos))
            {
                return taskInfos.Where(x => x.Id == taskId).FirstOrDefault();
            }
            return null;
        }


        public void Update()
        {
            if (nextUpdateTime< Owner.server.Now())
            {
                CheckTime();
                nextUpdateTime = Owner.server.Now().AddSeconds(10);
            }
        }

        public bool CurrNodeHaveShop()
        {
            int node = NodeId + 1;
            if (nodeTaskList.ContainsKey(node))
            {
                return nodeTaskList[node].FirstOrDefault(x => x.Model?.Type == TowerTaskType.Shop) != null;
            }

            return false;
        }

        public void CheckTime()
        {
            if (!Owner.CheckLimitOpen(LimitType.Tower)) return;

            if (StopTime < Owner.server.Now())
            {
                RandomTask();
            }
        }

        public bool CheckJobLimited(int heroId)
        {
            HeroModel model = HeroLibrary.GetHeroModel(heroId);
            if (model == null) return true;

            return model.Job == GetLimitedJob();
        }

        private int GetLimitedJob()
        { 
            int index = Period % TowerLibrary.PeriodLimitHeroJob.Count; ;
            return TowerLibrary.PeriodLimitHeroJob[index]; 
        }

        private void InitHeroPos()
        {
            foreach (var item in Owner.HeroMng.GetAllHeroPos())
            {
                if (CheckJobLimited(item.Item1)) continue;

                HeroPos.Add(item.Item1, item.Item2);
            }

            //所有的都限制了只上阵默认位置
            if (HeroPos.Count == 0)
            {
                foreach (var kv in Owner.HeroMng.GetHeroInfoList())
                {
                    if (!CheckJobLimited(kv.Key))
                    {
                        HeroPos.Add(kv.Value.Id, 1);
                        return;
                    }
                }
            }
        }

        private void Reset()
        {
            TowerTask = null;

            NodeId = 0;
            TaskId = 0;
            ReviveCount = 0;
            RewardList?.Clear();
            DeadHeroList?.Clear();
            ShopList?.Clear();
            BuffInfoList?.Clear();
            HeroHp?.Clear();
            MonsterHP?.Clear();
            HeroPos?.Clear();
            heroSkilEnergy?.Clear();
            SoulBoneList?.Clear();

            SetBuffListChanged(true);
            buffQualityCount?.Clear();
        }

        public bool RandomShopItem(TowerTaskInfo info)
        {
            if (info == null) return false;

            if (info.NeedFreshShopItem())
            {
                Owner.Cache5MaxBattlePowerHeroInfo();
                info.ShopFreshed = true;
                BuildTaskParam(info.Model, info);

                SyncTowerToDB();
                return true;
            }
            return false;
        }

        private void RandomTask()
        {
            Reset();

            //当前周期加1
            Period++;

            InitHeroPos();

            GroupId = TowerLibrary.RandomGroup();

            LoadBattlePower();

            SetOpenTime();

            InitTaskList(GroupId);

            SyncTowerToDB();
            Owner.GetTowerInfo();
        }

        private void LoadBattlePower()
        {
            if (BattlePower == 0 || HeroLevel == 0)
            {
                //第一期开启，使用相同配置的战斗力
                BattlePower = TowerLibrary.FirstBattlePower;
                HeroLevel = TowerLibrary.FirstHeroLevel;
                SyncBattlePowerToDB();
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
                });
            }
        }

        private void SetOpenTime()
        {
            StartTime = GetStartTime();
            StopTime = StartTime.AddDays(TowerLibrary.OpenDays);
            Owner.SendTowerTime();
        }

        private DateTime GetStartTime()
        {
            DateTime nowDate = Owner.server.Now().Date;
            DateTime opendServerDate = Owner.server.OpenServerDate;

            DateTime openDate = nowDate;
            int days = (int)((nowDate - opendServerDate).TotalDays) % (TowerLibrary.OpenDays + TowerLibrary.CloseDays);
            if (days < TowerLibrary.OpenDays)
            {
                openDate = openDate.AddDays(-1 * days);
            }
            else
            {
                openDate = openDate.AddDays(TowerLibrary.CloseDays);
            }
            return openDate;
        }

        private void InitTaskList(int groupId)
        {
            Dictionary<int, List<TowerTaskModel>> nodeTask = TowerLibrary.GetGroupTasks(groupId);
            if (nodeTask == null)
            {
                Logger.Log.Error($"player {Owner.Uid} init tower task error, have not group {groupId} task !");
                return;
            }
            
            nodeTaskList.Clear();
            Owner.Cache5MaxBattlePowerHeroInfo();

            foreach (var kv in nodeTask)
            {
                nodeTaskList[kv.Key] = new List<TowerTaskInfo>();
                foreach (var task in kv.Value)
                {
                    TowerTaskInfo taskInfo = new TowerTaskInfo(this, task);
                    if (task.Type != TowerTaskType.Shop)
                    {
                        BuildTaskParam(task, taskInfo);
                    }
                    else
                    {
                        taskInfo.param = new List<int>();
                    }

                    nodeTaskList[kv.Key].Add(taskInfo);
                }
            }
        }

        public void SetTaskId(int taskId)
        {
            TaskId = taskId;

            TowerTaskModel model = TowerLibrary.GetTaskModel(taskId);
            if (model != null)
            {
                SetGroupId(model.GroupId);
                InitTask(model);
            }
        }

        private void SetNodeId(int nodeId)
        {
            if (nodeId > TowerLibrary.MaxNode) return;
            NodeId = nodeId;
        }

        private void SetGroupId(int groupId)
        {
            GroupId = groupId;
        }

        public void InitTask(TowerTaskModel model)
        {
            if (model == null) return;

            TowerTask = CreateTowerTask(model);
        }

        private void BuildTaskParam(TowerTaskModel model, TowerTaskInfo info)
        {
            info.param = new List<int>();
            switch (model.Type)
            {
                case TowerTaskType.Dungeon:
                    {
                        info.param.Add(RandomDungeonId(model));
                    }
                    break;
                case TowerTaskType.Shop:
                    {
                        info.param.AddRange(RandomShopItem(model, Owner.MainTaskId));
                    }
                    break;
            }
        }

        public void GotoNextNode()
        {
            CheckAndAddReward();
            SetTaskId(0);
            SetNodeId(TowerTask.NodeId);

            TowerTask = null;

            ShopList.Clear();
            MonsterHP.Clear();
            SoulBoneList.Clear();

            SyncTaskInfoToDB();
            SyncHeroAndMonsterToDB();

            Owner.SendTowerInfo();
            Owner.SendTowerDungeonGrowth();
        }

        private void CheckAndAddReward()
        {
            TowerRewardModel model = TowerLibrary.GetTowerModel(TowerTask.NodeId);
            if (model != null && !string.IsNullOrEmpty(model.Data.GetString("Reward")))
            {
                AddReward(TowerTask.NodeId);
            }
        }

        private int RandomDungeonId(TowerTaskModel model)
        {
            TowerDungeonModel dungeonModel = TowerLibrary.RandomDungeon(model.Difficulty);
            return dungeonModel.Id;
        }

        private List<int> RandomShopItem(TowerTaskModel model, int mainTaskId)
        {
            List<int> itemIds = new List<int>();
            List<int> soulBone = new List<int>();
            bool hadRandomSoulBone = false;

            for (int i = 0; i < TowerLibrary.ShopItemCount; i++)
            {
                TowerShopItemType itemType = TowerLibrary.RandomShopItemType(mainTaskId, hadRandomSoulBone);
                int quality = Owner.GetShopItemQuality(itemType);

                TowerShopItemModel itemModel = TowerLibrary.RandopShopItem(itemType, quality);

                if (itemModel == null)
                {
                    Logger.Log.Warn($"随机商品出错 itemtype {itemType} quality {quality}");
                }

                hadRandomSoulBone |= itemType == TowerShopItemType.SoulBone;

                itemIds.Add(itemModel?.Id ?? 1);
            }

            return itemIds;
        }

        private TowerTask CreateTowerTask(TowerTaskModel model)
        {
            TowerTask task;
            switch (model.Type)
            {
                case TowerTaskType.Dungeon: task = new DungeonTowerTask(this, model.Id); break;
                case TowerTaskType.Shop: task = new ShopTowerTask(this, model.Id); break;
                case TowerTaskType.Compsite: task =  new CompsiteTowerTask(this, model.Id); break;
                case TowerTaskType.Fighting: task = new FightingTowerTask(this, model.Id);break;
                case TowerTaskType.Altar: task = new AltarTowerTask(this, model.Id); break;
                default: return null;
            }

            TowerTaskInfo info = GetTowerTaskInfo(model.NodeId, model.Id);

            task.SetNodeId(model.NodeId);
            task.SetTowerTaskInfo(info);

            return task;
        }

        public TowerTaskInfo GetTowerTaskInfo(int nodeId, int taskId)
        {
            List<TowerTaskInfo> taskInfos;
            if (nodeTaskList.TryGetValue(nodeId, out taskInfos))
            {
                return taskInfos.Where(x => x.Id == taskId).FirstOrDefault();
            }
            return null;
        }

        public void AddBuff(int id)
        {
            BuffInfoList.Add(id);
            SetBuffListChanged(true);

            Owner.SendTowerDungeonGrowth();
        }

        private bool buffListChanged = false;
        Dictionary<int, int> buffQualityCount = new Dictionary<int, int>();

        private void SetBuffListChanged(bool changed)
        {
            buffListChanged = changed;
        }

        public float GetMosterGrowth(int chapter, int chapterNodeIndex, int difficuty)
        {
            if (buffListChanged || buffQualityCount.Count == 0)
            {
                buffQualityCount.Clear();
                SetBuffListChanged(false);

                BuffInfoList.ForEach(x =>
                {
                    TowerBuffModel buffModel = TowerLibrary.GetTowerBuffModel(x);
                    if (buffModel != null)
                    {
                        int quality = buffModel.Quality;

                        if (buffQualityCount.ContainsKey(quality))
                        {
                            buffQualityCount[quality] += 1;
                        }
                        else
                        {
                            buffQualityCount.Add(quality, 1);
                        }
                    }
                });
            }

            return ScriptManager.TowerManager.CaculateDungeonGrowth(BattlePower, chapter, chapterNodeIndex, buffQualityCount, difficuty);
        }

        public int GetMonsterHeroSoulRingCount()
        {
            int count = HeroLevel / 10 + 1;
            return count;
        }

        public void SetHeroPos(RepeatedField<MSG_GateZ_HERO_POS> heroPos)
        {
            HeroPos.Clear();
            heroPos.ForEach(x => HeroPos.Add(x.HeroId, x.PosId));
            SyncHeroAndMonsterToDB();
        }

        public int GetHeroPos(int heroId)
        {
            int pos = -1;
            if (HeroPos.TryGetValue(heroId, out pos)) return pos;
            return -1;
        }

        public bool CheckPosDeadHero()
        {
            foreach (var kv in HeroPos)
            {
                if (DeadHeroList.Contains(kv.Key)) return true;
            }

            return false;
        }

        public void SetTowerHeroAndMonsterHP(Dictionary<int, float> heroHp, Dictionary<int, float> monsterHp)
        {
            foreach (var kv in heroHp)
            {
                if (kv.Value == 1)
                {
                    HeroHp.Remove(kv.Key);
                    continue;
                }
                else if (kv.Value <= 0)
                {
                    HeroHp.Remove(kv.Key);
                    DeadHeroList.Add(kv.Key);
                    continue;
                }

                HeroHp[kv.Key] = kv.Value;
            }

            foreach (var kv in monsterHp)
            {
                if (kv.Value == 1) continue;
                MonsterHP[kv.Key] = kv.Value;
            }

            SyncHeroAndMonsterToDB();
        }

        public void AddHeroHpRatio(float ratio)
        {
            if (HeroHp.Count == 0) return;

            foreach (var kv in HeroHp.Keys.ToList())
            {
                float currHp = HeroHp[kv] + ratio;
                if (currHp > 1)
                {
                    currHp = 1.0f;
                }
                HeroHp[kv] = currHp;
            }

            Owner.SendTowerHeroInfo();
            SyncHeroAndMonsterToDB();
        }

        public void UpdateHeroSkillEnergy(Dictionary<int, Dictionary<int, int>> heroSkillEnergy)
        {
            foreach (var kv in heroSkillEnergy)
            {
                //清除旧数据
                RemoveSkillEnergy(kv.Key);

                foreach (var item in kv.Value)
                {
                    AddHeroSkillEnergy(kv.Key, item.Key, item.Value);
                }
            }
        }

        public Dictionary<int, Dictionary<int, int>> GetHeroSkillEnergy() => heroSkilEnergy;

        private void RemoveSkillEnergy(int heroId)
        {
            heroSkilEnergy.Remove(heroId);
        }

        public void AddHeroSkillEnergy(int hero, int skillId, int energy)
        {
            if (energy <= 0) return;

            Dictionary<int, int> skillEnergy;
            if (!heroSkilEnergy.TryGetValue(hero, out skillEnergy))
            {
                skillEnergy = new Dictionary<int, int>();
                heroSkilEnergy.Add(hero, skillEnergy);
            }
            skillEnergy[skillId] = energy;
        }

        public void ReviveAllHero()
        {
            ReviveCount += 1;
            DeadHeroList.Clear();
            SyncHeroAndMonsterToDB();
        }

        public int ReviveRandomHero()
        {
            if (DeadHeroList.Count == 0) return 0;
            int index = RAND.Range(0, DeadHeroList.Count - 1);

            int heroId = DeadHeroList[index];

            DeadHeroList.Remove(heroId);

            Owner.SendTowerHeroInfo();
            SyncHeroAndMonsterToDB();

            return heroId;
        }

        public void AddBuyedShop(int id)
        {
            if (!ShopList.Contains(id))
            {
                ShopList.Add(id);
            }

            SyncShopItemToDB();
        }

        #region reward 

        public void AddReward(int id)
        {
            if (!RewardList.Contains(id))
            {
                RewardList.Add(id);
                SyncRewardToDB();
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

        #region db

        public void SetTaskInfo(string nodeTaskStr, string soulBoneStr)
        {
            nodeTaskList = new Dictionary<int, List<TowerTaskInfo>>();
            SoulBoneList = new Dictionary<int, ItemBasicInfo>();

            if (string.IsNullOrEmpty(nodeTaskStr)) return;

            string[] taskList = nodeTaskStr.Split('|');
            for (int i = 0; i < taskList.Length; i++)
            {
                int nodeId = i + 1;
                nodeTaskList[nodeId] = new List<TowerTaskInfo>();

                string[] nodeTasks = taskList[i].Split(':');
                for (int task = 0; task < nodeTasks.Length; task++)
                {
                    List<int> paramList = SplitString(nodeTasks[task], '-');
                    if (paramList.Count > 0)
                    {
                        TowerTaskModel model = TowerLibrary.GetTaskModel(paramList[0]);
                        if (model == null)
                        {
                            Log.Warn($"tower task error task id {paramList[0]}");
                            continue;
                        }
                        SetGroupId(model.GroupId);
                        TowerTaskInfo taskInfo = new TowerTaskInfo(this, model, paramList);
                        nodeTaskList[nodeId].Add(taskInfo);
                    }
                }
            }

            if(string.IsNullOrEmpty(soulBoneStr)) return;

            string[] soulBoneItems = soulBoneStr.Split('|');
            for (int i = 0; i < soulBoneItems.Length; i++)
            {
                ItemBasicInfo basicInfo = ItemBasicInfo.Parse(soulBoneItems[i]);
                SoulBoneList[basicInfo.Id] = basicInfo;
            }
        }

        //heroid:skillid-energy:skillid1-energy|heroid:skillid-energy:skillid1-energy
        public void SetSkillEnergy(string skillEnergyStr)
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

        private string GetTaskStr()
        {
            if (nodeTaskList == null) return string.Empty;
            List<string> nodeTaskStr = nodeTaskList.Values.ToList().ConvertAll(x => string.Join(":", x.ConvertAll(item=>item.ToString())));
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

        public string ConnectListItem<T>(List<T> list, string connector)
        {
            return list == null || list.Count == 0 ? string.Empty : string.Join(connector, list);
        }

        public List<int> SplitString(string str, char separator)
        {
            List<int> list = new List<int>();
            if (string.IsNullOrEmpty(str))
            {
                return list;
            }
            int value;
            foreach (var kv in str.Split(separator))
            {
                if (int.TryParse(kv, out value))
                {
                    list.Add(value);
                }
            }
            return list;
        }

        public Dictionary<int, int> SplitString(string str)
        {
            Dictionary<int, int> dic = new Dictionary<int, int>();
            if (string.IsNullOrEmpty(str)) return dic;

            string[] first = str.Split('|');
            for (int i = 0; i < first.Length; i++)
            {
                string[] second = first[i].Split(':');
                if (second.Length == 2)
                {
                    int key, vallue;
                    if (!int.TryParse(second[0], out key) || !int.TryParse(second[1], out vallue)) continue;

                    dic[key] = vallue;
                }
            }
            return dic;
        }

        public Dictionary<int, float> SplitString2Float(string str)
        {
            Dictionary<int, float> dic = new Dictionary<int, float>();
            if (string.IsNullOrEmpty(str)) return dic;

            string[] first = str.Split('|');
            for (int i = 0; i < first.Length; i++)
            {
                string[] second = first[i].Split(':');
                if (second.Length == 2)
                {
                    int key; 
                    float vallue;
                    if (!int.TryParse(second[0], out key) || !float.TryParse(second[1], out vallue)) continue;

                    dic[key] = vallue;
                }
            }
            return dic;
        }

        public string ConnectDictionary<K,V>(Dictionary<K, V> dic)
        {
            List<string> items = new List<string>();
            dic.ForEach(x => items.Add(x.Key + ":" + x.Value));

            return string.Join("|", items);
        }

        public TowerDBInfo GenerateTaskDBInfo()
        {
            TowerDBInfo info = new TowerDBInfo()
            {
                Period = Period,
                NodeId = NodeId,
                TaskId = TaskId,
                BattlePower = BattlePower,
                HeroLevel = HeroLevel,
                StartTime = StartTime,
                RewardList = ConnectListItem(RewardList, "|"),
                DeadHeroList = ConnectListItem(DeadHeroList, "|"),
                BuffListList = ConnectListItem(BuffInfoList, "|"),
                ShopList = ConnectListItem(ShopList, "|"),
                NodeTaskInfo = GetTaskStr(),
                HeroPos = ConnectDictionary(HeroPos),
                HeroHp = ConnectDictionary(HeroHp),
                MonsterHP = ConnectDictionary(MonsterHP),
                ReviveCount = ReviveCount,
                SkillEnergy = GetHeroSkillEnergyStr(),
                SoulBoneList = GetSoulBoneStr()
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

        #endregion

        public void SyncTowerToDB()
        {
            QueryUpdateTower query = new QueryUpdateTower(Owner.Uid, GenerateTaskDBInfo()); ;
            Owner.server.GameDBPool.Call(query) ;
        }

        public void SyncBuffToDB()
        {
            QueryUpdateTowerBuff query = new QueryUpdateTowerBuff(Owner.Uid, ConnectListItem(BuffInfoList, "|"));
            Owner.server.GameDBPool.Call(query);
        }

        public void SyncHeroAndMonsterToDB()
        {
            QueryUpdateTowerHeroAndMonsterHP query = new QueryUpdateTowerHeroAndMonsterHP(Owner.Uid,
                ConnectDictionary(HeroPos), ConnectDictionary(HeroHp), ConnectDictionary(MonsterHP), ConnectListItem(DeadHeroList, "|"), ReviveCount, GetHeroSkillEnergyStr());
            Owner.server.GameDBPool.Call(query);
        }

        public void SyncShopItemToDB()
        {
            QueryUpdateTowerShopItem query = new QueryUpdateTowerShopItem(Owner.Uid, ConnectListItem(ShopList, "|")) ;
            Owner.server.GameDBPool.Call(query);
        }

        public void SyncTaskInfoToDB()
        {
            QueryUpdateTowerNodeTask query = new QueryUpdateTowerNodeTask(Owner.Uid, NodeId, TaskId, ConnectListItem(ShopList, "|"), GetSoulBoneStr());
            Owner.server.GameDBPool.Call(query);
        }

        public void SyncRewardToDB()
        {
            QueryUpdateTowerReward query = new QueryUpdateTowerReward(Owner.Uid, ConnectListItem(RewardList, "|"));
            Owner.server.GameDBPool.Call(query);
        }

        public void SyncBattlePowerToDB()
        {
            QueryTowerUpdateBattlePower query = new QueryTowerUpdateBattlePower(Owner.Uid, BattlePower, HeroLevel);
            Owner.server.GameDBPool.Call(query);
        }

        public void SyncSoulBoneToDB()
        {
            QueryUpdateTowerSoulBoneInfo query = new QueryUpdateTowerSoulBoneInfo(Owner.Uid, GetSoulBoneStr());
            Owner.server.GameDBPool.Call(query);
        }

        public MSG_ZGC_TOWER_INFO GenerateMsg()
        {
            MSG_ZGC_TOWER_INFO msg = new MSG_ZGC_TOWER_INFO();
            msg.GroupId = GroupId;
            msg.NodeId = NodeId;
            msg.TaskId = TaskId;

            msg.RewardList.AddRange(RewardList);
            msg.ShopList.AddRange(ShopList);
            msg.LimitJob = GetLimitedJob();

            foreach (var kv in nodeTaskList)
            {
                MSG_TOWER_NODE_INFO nodeMsg = new MSG_TOWER_NODE_INFO();
                nodeMsg.Id = kv.Key;
                kv.Value.ForEach(x => nodeMsg.TaskList.Add(x.GenerateMsg()));

                msg.NodeList.Add(nodeMsg);
            }

            CheckSoulBoneChanged();

            return msg;
        }

        public MSG_ZGC_INIT_TOWER_HERO_INFO GenerateHeroInfo()
        {
            MSG_ZGC_INIT_TOWER_HERO_INFO msg = new MSG_ZGC_INIT_TOWER_HERO_INFO();
            msg.ReviveCount = ReviveCount;
            msg.DeadHeroList.AddRange(DeadHeroList);

            HeroPos.ForEach(x => msg.HeroPos.Add(new MSG_ZGC_HERO_POS() { HeroId = x.Key, PosId = x.Value }));
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

        public MSG_ZGC_TOWER_DUNGOEN_GROWTH GenerateDungeonGrowth()
        {
            MSG_ZGC_TOWER_DUNGOEN_GROWTH msg = new MSG_ZGC_TOWER_DUNGOEN_GROWTH();

            int currNode = NodeId == TowerLibrary.MaxNode ? NodeId : NodeId + 1;
            if (currNode == 0) currNode = 1;

            List<TowerTaskInfo> currNodeTask;
            if (nodeTaskList.TryGetValue(currNode, out currNodeTask))
            {
                currNodeTask.ForEach(x =>
                {
                    if (x.Model.Type == TowerTaskType.Dungeon)
                    {

                        //Log.Error($"TaskId: {x.Model.Id}");

                        msg.DungeonGrowthList.Add(new MSG_ZGC_TOWER_DUNGOEN_TASK_GROWTH()
                        {
                            TaskId = x.Model.Id,
                            DungeonGrowth = GetMosterGrowth(x.Model.Chapter, x.Model.ChapterNodeIndex, x.Model.Quality)
                        });

                    }
                });
            }
            return msg;
        }

        public MSG_ZMZ_TOWER_INFO GenerateTransformMsg()
        {
            MSG_ZMZ_TOWER_INFO msg = new MSG_ZMZ_TOWER_INFO();

            msg.Period = Period;
            msg.GroupId = GroupId;
            msg.NodeId = NodeId;
            msg.TaskId = TaskId;
            msg.BattlePower = BattlePower;
            msg.HeroLevel = HeroLevel;
            msg.ReviveCount = ReviveCount;
            msg.StartTime = Timestamp.GetUnixTimeStamp(StartTime);
            msg.StopTime = Timestamp.GetUnixTimeStamp(StopTime);

            msg.RewardList.AddRange(RewardList);
            msg.DeadHeroList.AddRange(DeadHeroList);
            msg.BuffInfoList.AddRange(BuffInfoList);
            msg.ShopList.AddRange(ShopList);

            HeroPos.ForEach(x => msg.HeroPos.Add(x.Key, x.Value));
            HeroHp.ForEach(x => msg.HeroHp.Add(x.Key, x.Value));
            MonsterHP.ForEach(x => msg.MonsterHP.Add(x.Key, x.Value));
            heroSkilEnergy.ForEach(x => x.Value.ForEach(y => msg.HeroEnergyList.Add(new MSG_ZM_TOWER_HERO_ENERGY() { HeroId = x.Key, SkillId = y.Key, Energy = y.Value, })));

            msg.TaskInfo = GetTaskStr();
            msg.SoulBoneInfo = GetSoulBoneStr();

            return msg;
        }

        public void LoadTransform(MSG_ZMZ_TOWER_INFO info)
        {
            ShopList = new List<int>();
            RewardList = new List<int>();
            DeadHeroList = new List<int>();
            BuffInfoList = new List<int>();
            HeroPos = new Dictionary<int, int>();
            HeroHp = new Dictionary<int, float>();
            MonsterHP = new Dictionary<int, float>();
            heroSkilEnergy = new Dictionary<int, Dictionary<int, int>>();

            Period = info.Period;
            GroupId = info.GroupId;
            BattlePower = info.BattlePower;
            HeroLevel = info.HeroLevel;
            ReviveCount = info.ReviveCount;
            StartTime = Timestamp.TimeStampToDateTime(info.StartTime);
            StopTime = Timestamp.TimeStampToDateTime(info.StopTime);

            ShopList.AddRange(info.ShopList);
            RewardList.AddRange(info.RewardList);
            DeadHeroList.AddRange(info.DeadHeroList);
            BuffInfoList.AddRange(info.BuffInfoList);

            info.HeroPos.ForEach(x => HeroPos.Add(x.Key, x.Value));
            info.HeroHp.ForEach(x => HeroHp.Add(x.Key, x.Value));
            info.MonsterHP.ForEach(x => MonsterHP.Add(x.Key, x.Value));
            info.HeroEnergyList.ForEach(x => AddHeroSkillEnergy(x.HeroId, x.SkillId, x.Energy));

            SetTaskInfo(info.TaskInfo, info.SoulBoneInfo);

            SetNodeId(info.NodeId);
            SetTaskId(info.TaskId);
        }
    }
}
