using CommonUtility;
using EnumerateUtility;
using Google.Protobuf.Collections;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using Message.Zone.Protocol.ZR;
using ServerModels;
using ServerShared;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        //烟火
        public void UseFireworks(int itemId)
        {
            MSG_ZGC_USE_FIREWORKS broadCastMsg = new MSG_ZGC_USE_FIREWORKS();
        
            broadCastMsg.Id = itemId;
            broadCastMsg.PosX = Position.X;
            broadCastMsg.PosY = Position.Y;
            //广播
            BroadCast(broadCastMsg);

            SendFireworkRewards(itemId);
        }

        ///// <summary>
        ///// 幸运烟花发送邮件
        ///// </summary>
        //public void SendFireworkLuckyEmail()
        //{
        //    EmailInfo info = EmailLibrary.GetEmailInfo(EmailLibrary.FireworkLuckyEmail);
        //    if (info == null)
        //    {
        //        Log.Warn("player {0} SendFireworkEmail not find email id:{1}", Uid, EmailLibrary.FireworkLuckyEmail);
        //    }
        //    SendPersonEmail(info, info.Body);
        //}  
        
        private void SendFireworkRewards(int itemId)
        {
            MSG_ZGC_FIREWORK_REWARD response = new MSG_ZGC_FIREWORK_REWARD();
            BaseItem item = BagManager.GetItem(MainType.Consumable, itemId);
            if (item == null)
            {
                Log.Warn("player {0} use firework {1} fail: not find item in bag", uid, itemId);
                return;
            }
            response.Result = (int)UseFireWork(item, 1, response.Rewards);
            Write(response);
        }

        private ErrorCode UseFireWork(BaseItem item, int num, RepeatedField<REWARD_ITEM_INFO> rewards)
        {
            ErrorCode errorCode = ErrorCode.Fail;

            if (!CheckItemInfo(item, num, ref errorCode))
            {
                return errorCode;
            }
            else
            {
                errorCode = CheckCanUseFireWork(item);
                if (errorCode != ErrorCode.Success)
                {
                    return errorCode;
                }
                ConsumeFireworkGetReward(item as NormalItem, num, rewards);
            }
            return ErrorCode.Success;
        }

        private ErrorCode CheckCanUseFireWork(BaseItem item)
        {
            if (item.MainType != MainType.Consumable)
            {
                return ErrorCode.Fail;
            }
            ItemModel model = BagLibrary.GetItemModel(item.Id);
            if (model == null)
            {
                Log.Warn($"player {Uid} ItemUse have not model ItemModel item id {item.Id}");
                return ErrorCode.Fail;
            }
            if (!model.IsUsable)
            {
                Log.Warn($"player {Uid} ItemUse item id {item.Id} IsUsable ");
                return ErrorCode.CanNotUse;
            }
            if (Level < model.LevelLimit)
            {
                Log.Warn($"player {Uid} ItemUse item id {item.Id} levellimit ");
                return ErrorCode.UseItemLevelLimt;
            }
            return ErrorCode.Success;
        }

        private void ConsumeFireworkGetReward(NormalItem item, int num, RepeatedField<REWARD_ITEM_INFO> rewards)
        {
            ItemUsingModel usingModel = BagLibrary.GetItemUsingModel(item.Id);
            if (usingModel != null)
            {
                if (!string.IsNullOrEmpty(usingModel.Rewards))
                {
                    RewardManager manager = new RewardManager();
                    manager.InitBatchReward(usingModel.Type, usingModel.Rewards, num);

                    AddSoulRing2BagFromChest(item, manager, ObtainWay.ItemUse);
                    AddRewards(manager, ObtainWay.ItemUse);

                    manager.GenerateRewardItemInfo(rewards);
                }
                if (!string.IsNullOrEmpty(usingModel.SoulBoneReward))
                {
                    //产出魂骨
                    List<ItemBasicInfo> itemsList = SoulBoneManager.GenerateSoulboneReward(usingModel.SoulBoneReward, null, (int)HeroMng.GetFirstHeroJob());
                    if (itemsList != null)
                    {
                        RewardManager manager = new RewardManager();
                        manager.AddReward(itemsList);
                        manager.BreakupRewards();

                        AddRewards(manager, ObtainWay.ItemUse);

                        manager.GenerateRewardItemInfo(rewards);
                    }
                }
            }
            BaseItem baseItem = DelItem2Bag(item, RewardType.NormalItem, num, ConsumeWay.ItemUse);

            if (baseItem != null)
            {
                SyncClientItemInfo(item);
                //使用消耗品
                AddTaskNumForType(TaskType.UseConsumable, 1, true, item.SubType);
            }
        }

        #region 主题烟花
        public ThemeFireworkManager ThemeFireworkMng { get; private set; }

        public void InitThemeFireworkManager()
        {
            ThemeFireworkMng = new ThemeFireworkManager(this);
        }       

        public void GetThemeFireworkByLoading()
        {
            RechargeGiftModel model;
            if (!RechargeLibrary.CheckInSpecialRechargeGiftShowTime(RechargeGiftType.ThemeFirework, ZoneServerApi.now, out model))
            {
                return;
            }
            GetThemeFireworkInfo();
        }

        public void GetThemeFireworkInfo()
        {
            //获取第一名信息
            MSG_ZR_GET_RANK_FIRST_INFO msg = new MSG_ZR_GET_RANK_FIRST_INFO() { RankType = (int)RankType.ThemeFirework};
            server.SendToRelation(msg, Uid);
        }

        public MSG_ZGC_THEME_FIREWORK_INFO GenerateThemeFireworkInfo()
        {
            MSG_ZGC_THEME_FIREWORK_INFO msg = new MSG_ZGC_THEME_FIREWORK_INFO();

            msg.Score = ThemeFireworkMng.Info.Score;
            msg.HighestUseCount = ThemeFireworkMng.Info.HighestUseCount;
            msg.ScoreRewards.AddRange(ThemeFireworkMng.Info.ScoreRewards);
            msg.HighestUseCountRewards.AddRange(ThemeFireworkMng.Info.HighestUseCountRewards);

            return msg;
        }

        /// <summary>
        /// 使用烟花
        /// </summary>
        /// <param name="item"></param>
        /// <param name="num"></param>
        private void UseThemeFirework(NormalItem item, int num)
        {
            MSG_ZGC_THEME_FIREWORK_REWARD response = new MSG_ZGC_THEME_FIREWORK_REWARD();

            int period = ThemeFireworkLibrary.GetThemeFireworkPeriod(item.Id);
            if (period == 0)
            {
                Log.Warn($"player {Uid} UseThemeFirework failed: time is error");
                response.Result = (int)ErrorCode.NotOnTime;
                Write(response);
                return;
            }
            //判定是否在活动时间内
            RechargeGiftModel model;
            RechargeLibrary.CheckInSpecialRechargeGiftShowTime(RechargeGiftType.ThemeFirework, ZoneServerApi.now, out model);

            bool inActivity = false;
            if (model != null && ZoneServerApi.now <= model.EndTime)
            {
                inActivity = true;
            }

            ThemeFireworkConfig config = ThemeFireworkLibrary.GetConfig(period);
            if (config == null)
            {
                Log.Warn($"player {Uid} UseThemeFirework failed: not find period {period} config");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            BaseItem baseItem = DelItem2Bag(item, RewardType.NormalItem, num, ConsumeWay.ItemUse);

            if (baseItem != null)
            {
                SyncClientItemInfo(item);
                //使用消耗品
                AddTaskNumForType(TaskType.UseConsumable, 1, true, item.SubType);
            }

            int fireworkType = config.GetFireworkType(item.Id);

            //随机奖励
            List<RandomRewardModel> randomRewards = ThemeFireworkLibrary.GetRandomRewardList(period, fireworkType);
            List<string> rewards = GenerateRandomReward(period, randomRewards, num);
            //随机折扣券
            List<RandomRewardModel> randomCoupons = ThemeFireworkLibrary.GetRandomCouponList(period, fireworkType);
            List<string> coupons = GenerateRandomReward(period, randomCoupons, num);
            rewards.AddRange(coupons);
            //随机烟花
            List<RandomRewardModel> randomFireworks = ThemeFireworkLibrary.GetRandomFireworkList(period, fireworkType);
            List<string> fireworks = GenerateRandomReward(period, randomFireworks, num);
            if (fireworks.Count > 0)
            {
                rewards.AddRange(fireworks);
            }

            if (rewards.Count > 0)
            {
                RewardManager manager = new RewardManager();
                List<ItemBasicInfo> rewardItems = new List<ItemBasicInfo>();
                //按有装备和魂骨生成奖励
                foreach (var reward in rewards)
                {
                    if (!string.IsNullOrEmpty(reward))
                    {
                        RewardDropItemList rewardDrop = new RewardDropItemList(RewardDropType.Fixed, reward);
                        List<ItemBasicInfo> items = RewardManagerEx.GetRewardBasicInfoList(rewardDrop, (int)Job);
                        rewardItems.AddRange(items);
                    }
                }
                manager.AddReward(rewardItems);
                manager.BreakupRewards(true);
                AddRewards(manager, ObtainWay.ThemeFirework);
                manager.GenerateRewardMsg(response.Rewards);
            }

            if (inActivity)
            {
                int score = config.GetScore(fireworkType);
                bool highest = false;
                if (fireworkType == config.HighestFirework)
                {
                    highest = true;
                }
                ThemeFireworkMng.UpdateThemeFireworkUseInfo(score, highest, num);

                SerndUpdateRankValue(RankType.ThemeFirework, ThemeFireworkMng.Info.Score);
                
                BIRecordPointGameLog(score * num, ThemeFireworkMng.Info.Score, "theme_forework", model.SubType);
            }

            response.ItemId = item.Id;
            response.Score = ThemeFireworkMng.Info.Score;
            response.HighestUseCount = ThemeFireworkMng.Info.HighestUseCount;
            response.Result = (int)ErrorCode.Success;
            Write(response);

            //广播烟花
            MSG_ZGC_USE_THEME_FIREWORK broadCastMsg = new MSG_ZGC_USE_THEME_FIREWORK();

            broadCastMsg.ItemId = item.Id;
            broadCastMsg.PosX = Position.X;
            broadCastMsg.PosY = Position.Y;

            BroadCast(broadCastMsg);
        }

        /// <summary>
        /// 领积分奖励
        /// </summary>
        public void GetThemeFireworkScoreReward(int rewardId)
        {
            MSG_ZGC_THEME_FIREWORK_SCORE_REWARD response = new MSG_ZGC_THEME_FIREWORK_SCORE_REWARD();
            response.RewardId = rewardId;

            RechargeGiftModel model;
            if (!RechargeLibrary.CheckInSpecialRechargeGiftShowTime(RechargeGiftType.ThemeFirework, ZoneServerApi.now, out model))
            {
                Log.Warn($"player {Uid} GetThemeFireworkScoreReward failed: time is error");
                response.Result = (int)ErrorCode.NotOnTime;
                Write(response);
                return;
            }
            int period = model.SubType;

            ThemeFireworkAccumulateReward rewardModel = ThemeFireworkLibrary.GetAccumulateReward(rewardId);
            if (rewardModel == null)
            {
                Log.Warn($"player {Uid} GetThemeFireworkScoreReward failed: not find rewardId {rewardId}");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            if (rewardModel.Period != period)
            {
                Log.Warn($"player {Uid} GetThemeFireworkScoreReward failed: rewardId {rewardId} not cur period {period}");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            if (rewardModel.RewardType != 2)
            {
                Log.Warn($"player {Uid} GetThemeFireworkScoreReward failed: reward {rewardId} not score reward");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            if (ThemeFireworkMng.Info.ScoreRewards.Contains(rewardModel.Id))
            {
                Log.Warn($"player {Uid} GetThemeFireworkScoreReward failed: rewardId {rewardId} alrady got");
                response.Result = (int)ErrorCode.AlreadyGot;
                Write(response);
                return;
            }

            if (ThemeFireworkMng.Info.Score < rewardModel.Count)
            {
                Log.Warn($"player {Uid} GetThemeFireworkScoreReward {rewardId} failed: score {ThemeFireworkMng.Info.Score} not enough");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            ThemeFireworkMng.UpdateScoreRewards(rewardModel.Id);

            if (!string.IsNullOrEmpty(rewardModel.Reward))
            {
                //按有装备和魂骨生成奖励
                RewardManager manager = new RewardManager();
                RewardDropItemList rewardDrop = new RewardDropItemList(RewardDropType.Fixed, rewardModel.Reward);
                List<ItemBasicInfo> rewardItems = RewardManagerEx.GetRewardBasicInfoList(rewardDrop, (int)Job);

                manager.AddReward(rewardItems);
                manager.BreakupRewards(true);
                AddRewards(manager, ObtainWay.ThemeFirework);
                manager.GenerateRewardMsg(response.Rewards);
            }

            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        /// <summary>
        /// 领4级烟花累积使用奖励
        /// </summary>
        /// <param name="rewardId"></param>
        public void GetThemeFireworkUseCountReward(int rewardId)
        {
            MSG_ZGC_THEME_FIREWORK_USECOUNT_REWARD response = new MSG_ZGC_THEME_FIREWORK_USECOUNT_REWARD();
            response.RewardId = rewardId;

            RechargeGiftModel model;
            if (!RechargeLibrary.CheckInSpecialRechargeGiftShowTime(RechargeGiftType.ThemeFirework, ZoneServerApi.now, out model))
            {
                Log.Warn($"player {Uid} GetThemeFireworkUseCountReward failed: time is error");
                response.Result = (int)ErrorCode.NotOnTime;
                Write(response);
                return;
            }
            int period = model.SubType;

            ThemeFireworkAccumulateReward rewardModel = ThemeFireworkLibrary.GetAccumulateReward(rewardId);
            if (rewardModel == null)
            {
                Log.Warn($"player {Uid} GetThemeFireworkUseCountReward failed: not find rewardId {rewardId}");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            if (rewardModel.Period != period)
            {
                Log.Warn($"player {Uid} GetThemeFireworkUseCountReward failed: rewardId {rewardId} not cur period {period}");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            if (rewardModel.RewardType != 1)
            {
                Log.Warn($"player {Uid} GetThemeFireworkUseCountReward failed: reward {rewardId} not useCount reward");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            if (ThemeFireworkMng.Info.HighestUseCountRewards.Contains(rewardModel.Id))
            {
                Log.Warn($"player {Uid} GetThemeFireworkUseCountReward failed: rewardId {rewardId} alrady got");
                response.Result = (int)ErrorCode.AlreadyGot;
                Write(response);
                return;
            }

            if (ThemeFireworkMng.Info.HighestUseCount < rewardModel.Count)
            {
                Log.Warn($"player {Uid} GetThemeFireworkUseCountReward {rewardId} failed: use count {ThemeFireworkMng.Info.HighestUseCount} not enough");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            ThemeFireworkMng.UpdateHighestUseCountRewards(rewardModel.Id);

            if (!string.IsNullOrEmpty(rewardModel.Reward))
            {
                //按有装备和魂骨生成奖励
                RewardManager manager = new RewardManager();
                RewardDropItemList rewardDrop = new RewardDropItemList(RewardDropType.Fixed, rewardModel.Reward);
                List<ItemBasicInfo> rewardItems = RewardManagerEx.GetRewardBasicInfoList(rewardDrop, (int)Job);

                manager.AddReward(rewardItems);
                manager.BreakupRewards(true);
                AddRewards(manager, ObtainWay.ThemeFirework);
                manager.GenerateRewardMsg(response.Rewards);
            }

            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        private void ClearThemeFireworkInfo()
        {
            ThemeFireworkMng.Clear();
            GetThemeFireworkInfo();
        }
        #endregion
    }
}
