using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Message.Zone.Protocol.ZM;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public class OnhookManager
    {
        public PlayerChar Owner { get; private set; }
        public int TierId { get; private set; }
        public DateTime LastRewardTime { get; private set; }
        public int LastLookTime { get; private set; }//已经计算了奖励的普通奖励时间
        public int LastRandomTime { get; private set; }//已经计算随机奖励的时间
        public string Reward { get; private set; }

        public OnhookManager(PlayerChar player)
        {
            this.Owner = player;
        }

        public void Init(int towerId, DateTime lastRewardTime, int lastLookTime, int lastRandomTime, string reward)
        {
            this.TierId = towerId;
            LastRewardTime = lastRewardTime;
            LastLookTime = lastLookTime;
            LastRandomTime = lastRandomTime;
            Reward = reward;

            //纠错逻辑
            int fixId = OnhookLibrary.CheckNewId(Owner.MainTaskId);
            if (fixId > TierId)
            {
                TierId = fixId;
                SyncOnhookToDB();
            }
        }

        public int GetOnhookTime()
        {
            return (int)Math.Min((Owner.server.Now() - LastRewardTime).TotalSeconds, OnhookLibrary.MaxRewardTime);
        }

        public void CheckAndOpenNew(int taskId)
        {
            int id;
            if (OnhookLibrary.OpenNew(taskId, out id))
            {
                SetNewId(id);
            }
        }

        public void SetNewId(int id)
        {
            if (id <= TierId) return;

            TierId = id;
            SyncOnhookToDB();
        }

        public void FirstOpen()
        {
            TierId = 1;
            LastRewardTime = Owner.server.Now();
            LastLookTime = 0;
            LastRandomTime = 0;
            Reward = OnhookLibrary.OpenReward;
            SyncOnhookToDB();
        }

        public RewardManager LookReward()
        {
            RewardManager manager = new RewardManager();
            manager.AddSimpleReward(Reward);

            bool changed = AddReward(manager);
            if (changed)
            {
                manager.MergeRewards();
                Reward = manager.ToString();

                SyncOnhookToDB();
            }

            return manager;
        }

        public RewardManager ResetReward(bool needReward = true)
        {
            RewardManager manager = new RewardManager();
            manager.AddSimpleReward(Reward);

            if (needReward)
            {
                //添加本次随机奖励
                AddReward(manager);
            }

            LastRewardTime = Owner.server.Now();
            LastLookTime = 0;
            LastRandomTime = 0;
            Reward = String.Empty;
            SyncOnhookToDB();

            return manager;
        }

        public RewardManager GetFastReward()
        {
            RewardManager manager = new RewardManager();
            OnhookModel model = OnhookLibrary.GetOnhookModel(TierId);
            if (model != null)
            {
                //添加本次固定奖励
                manager.AddSimpleReward(model.Data.GetString("ExpCardReward"), OnhookLibrary.FastRewardTime / OnhookLibrary.RewardTime);
                manager.AddSimpleReward(model.Data.GetString("GoldCardReward"), OnhookLibrary.FastRewardTime / OnhookLibrary.RewardTime);
                //添加本次随机奖励
                int randomRewardCount = OnhookLibrary.FastRewardTime / OnhookLibrary.RandomRewardTime;
                if (randomRewardCount > 0)
                {
                    for (int i = 0; i < randomRewardCount; i++)
                    {
                        manager.AddCalculateReward(1, model.Data.GetString("RandomReward"));

                        List<ItemBasicInfo> getList = Owner.AddRewardDrop(model.Data.GetIntList("RewardDropId", "|"));
                        manager.AddReward(getList);
                    }

                    //附加奖励
                    TimeLimitHookModel timeLimitHookModel = OnhookLibrary.GeTimeLimitHookModel(Owner.server.Now());
                    if (timeLimitHookModel != null)
                    {
                        RewardDropItemList itemList = new RewardDropItemList(timeLimitHookModel.Data);
                        for (int i = 0; i < randomRewardCount; i++)
                        {
                            List<ItemBasicInfo> getList = RewardManagerEx.GetRewardBasicInfoList(itemList, (int)Owner.Job);
                            manager.AddReward(getList);
                        }
                    }
                }
            }

            Owner.MonthCardUpOnhookRewards(manager);

            return manager;
        }
       

        private bool AddReward(RewardManager manager)
        {
            bool changed = false;
            DateTime now = Owner.server.Now();
            OnhookModel model = OnhookLibrary.GetOnhookModel(TierId);
            if (model == null)
            {
                return false;
            }

            int rewardTime;
            RewardManager thisRewardManager = new RewardManager();

            //常规奖励
            if (LastLookTime < OnhookLibrary.MaxRewardTime)
            {
                //计算未计算的部分奖励
                rewardTime = GetOnhookTime() - LastLookTime;

                int rewardCount = (int)(rewardTime / OnhookLibrary.RewardTime);
                if (rewardCount > 0)
                {
                    changed = true;
                    thisRewardManager.AddSimpleReward(model.Data.GetString("ExpCardReward"), rewardCount);
                    thisRewardManager.AddSimpleReward(model.Data.GetString("GoldCardReward"), rewardCount);
                    SetLastLookTime(LastLookTime + rewardCount * OnhookLibrary.RewardTime);
                }
            }

            //添加本次随机奖励
            {
                if(LastRandomTime < OnhookLibrary.MaxRewardTime)
                {
                    //计算未计算的部分奖励
                    rewardTime = GetOnhookTime() - LastRandomTime;

                    int randomRewardCount = (int)(rewardTime / OnhookLibrary.RandomRewardTime);
                    if (randomRewardCount > 0)
                    {
                        for (int i = 0; i < randomRewardCount; i++)
                        {
                            thisRewardManager.AddCalculateReward(1, model.Data.GetString("RandomReward"));

                            List<ItemBasicInfo> getList = Owner.AddRewardDrop(model.Data.GetIntList("RewardDropId", "|"));
                            thisRewardManager.AddReward(getList);
                        }

                        changed = true;
                        SetLastRandomTime(LastRandomTime + randomRewardCount * OnhookLibrary.RandomRewardTime);
                    }

                    //附加奖励
                    TimeLimitHookModel timeLimitHookModel = OnhookLibrary.GeTimeLimitHookModel(Owner.server.Now());
                    if (timeLimitHookModel != null)
                    {
                        rewardTime = (int)Math.Min(rewardTime, (now - timeLimitHookModel.StartTime).TotalSeconds);
                        int rewardCount = (int)(rewardTime / OnhookLibrary.RandomRewardTime);
                        if (rewardCount > 0)
                        {
                            RewardDropItemList itemList = new RewardDropItemList(timeLimitHookModel.Data);
                            for (int i = 0; i < randomRewardCount; i++)
                            {
                                List<ItemBasicInfo> getList = RewardManagerEx.GetRewardBasicInfoList(itemList, (int)Owner.Job);
                                thisRewardManager.AddReward(getList);
                            }

                            changed = true;
                            SetLastRandomTime(LastRandomTime + randomRewardCount * OnhookLibrary.RandomRewardTime);
                        }
                    }
                }
            }

            Owner.MonthCardUpOnhookRewards(thisRewardManager);

            manager.AllRewards.AddRange(thisRewardManager.AllRewards);

            return changed;
        }


        private void SetLastLookTime(int time)
        { 
             LastLookTime = time;
        }

        private void SetLastRandomTime(int time)
        {
            LastRandomTime = time;
        }

        public void SyncOnhookToDB()
        {
            QueryUpdateOnhook query = new QueryUpdateOnhook(Owner.Uid, TierId, LastRewardTime, LastLookTime, LastRandomTime, Reward);
            Owner.server.GameDBPool.Call(query);
        }

        public MSG_ZMZ_ONHOOK_INFO GenerateTransformMsg()
        {
            MSG_ZMZ_ONHOOK_INFO msg = new MSG_ZMZ_ONHOOK_INFO()
            {
                TierId = TierId,
                LastRewardTime = Timestamp.GetUnixTimeStamp(LastRewardTime),
                LastLookTime = LastLookTime,
                LastRandomTime = LastRandomTime,
                Reward = Reward
            };
            return msg;
        }

        public void LoadTransform(MSG_ZMZ_ONHOOK_INFO info)
        {
            TierId = info.TierId;
            LastRewardTime = Timestamp.TimeStampToDateTime(info.LastRewardTime);
            LastLookTime = info.LastLookTime;
            LastRandomTime = info.LastRandomTime;
            Reward = info.Reward;
        }

        public RewardManager GetOnhookReward(NormalItem item)
        {
            RewardManager manager = new RewardManager();
            int hour = OnhookLibrary.GetOnhookCardHourType(item.Id);
            if (hour == 0)
            {
                return manager;
            }
            OnhookModel model = OnhookLibrary.GetOnhookModel(TierId);
            switch ((ConsumableType)item.SubType)
            {           
                case ConsumableType.OnhookExpCard:
                    if (model != null)
                    {
                        manager.AddSimpleReward(model.Data.GetString("ExpCardReward"), hour * 3600 / OnhookLibrary.RewardTime);
                    }
                    else
                    {
                        model = OnhookLibrary.GetFirstOnhookModel();
                        if (model != null)
                        {
                            manager.AddSimpleReward(model.Data.GetString("ExpCardReward"), hour * 3600 / OnhookLibrary.RewardTime);
                        }
                    }
                    break;
                case ConsumableType.OnhookGoldCard:
                    if (model != null)
                    {
                        manager.AddSimpleReward(model.Data.GetString("GoldCardReward"), hour * 3600 / OnhookLibrary.RewardTime);
                    }
                    else
                    {
                        model = OnhookLibrary.GetFirstOnhookModel();
                        if (model != null)
                        {
                            manager.AddSimpleReward(model.Data.GetString("GoldCardReward"), hour * 3600 / OnhookLibrary.RewardTime);
                        }
                    }
                    break;
                default:
                    break;
            }
            return manager;
        }
    }
}
