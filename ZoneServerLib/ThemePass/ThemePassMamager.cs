using DBUtility;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class ThemePassMamager
    {
        public PlayerChar Owner { get; set; }
        private Dictionary<int, ThemePassItem> themePassList = new Dictionary<int, ThemePassItem>();
        //public int CurThemeType { get; private set; }

        public ThemePassMamager(PlayerChar owner)
        {
            Owner = owner;
        }

        public void InitThemePassInfo(Dictionary<int, DbThemePassItem> list)
        {
            foreach (var kv in list)
            {
                ThemePassItem item = new ThemePassItem(kv.Value);
                themePassList.Add(item.ThemeType, item);
            }
        }

        public MSG_ZGC_THEME_PASS_LIST GenerateThemePassInfo()
        {
            MSG_ZGC_THEME_PASS_LIST msg = new MSG_ZGC_THEME_PASS_LIST();
            //筛选出当前开启的通行证
            Dictionary<int, RechargeGiftModel> themePassDic = RechargeLibrary.GetRechargeGiftModelByGiftType(RechargeGiftType.ThemePass);
            foreach (var item in themePassList)
            {
                RechargeGiftModel gift;
                if (themePassDic.TryGetValue(item.Key, out gift))
                {
                    if (CheckThemePassOpened(gift))
                    {
                        msg.List.Add(GenerateThemePassInfo(item.Value));
                    }
                }
            }
            return msg;
        }

        public ZGC_THEME_PASS_INFO GenerateThemePassInfo(ThemePassItem item)
        {
            ZGC_THEME_PASS_INFO msg = new ZGC_THEME_PASS_INFO();
            msg.ThemeType = item.ThemeType;
            msg.Level = item.PassLevel;
            msg.Exp = item.Exp;
            msg.ExpItemCount = item.Exp / ThemePassLibrary.ExpRatio;
            msg.Bought = item.Bought;
            msg.BasicRewarded.AddRange(item.BasicRewardLevelSet);
            msg.SuperRewarded.AddRange(item.SuperRewardLevelSet);
            //经验值和道具数量不匹配时校准
            int itemId = ThemePassLibrary.GetThemePassItemByType(item.ThemeType);
            BaseItem expItem = Owner.BagManager.NormalBag.GetItem(itemId);
            if (expItem != null && expItem.PileNum > msg.ExpItemCount && item.PassLevel < ThemePassLibrary.MaxPassLevel)
            {
                int addCount = expItem.PileNum - msg.ExpItemCount;
                SortedDictionary<int, ThemePassLevel> passLevelDic = ThemePassLibrary.GetThemePassLevelByThemeType(item.ThemeType);
                AddRealPassExp(item, addCount, passLevelDic);
                CheckPassLevelUp(item, passLevelDic);
                SyncDbUpdateThemePassLevelExp(item);

                msg.Level = item.PassLevel;
                msg.Exp = item.Exp;
                msg.ExpItemCount = item.Exp / ThemePassLibrary.ExpRatio;
            }
            return msg;
        }

        /// <summary>
        /// 一键领取奖励
        /// </summary>
        public void GetAllLevelReward(int themeType, bool getAll, bool isSuper, List<int> rewardLevels)
        {
            MSG_ZGC_GET_THEMEPASS_REWARD response = new MSG_ZGC_GET_THEMEPASS_REWARD();

            response.ThemeType = themeType;
            response.Result = (int)ErrorCode.Success;
            response.GetAll = getAll;
            response.IsSuper = isSuper;
            response.RewardLevels.AddRange(rewardLevels);          

            ThemePassItem passItem;
            if (!themePassList.TryGetValue(themeType, out passItem))
            {
                response.Result = (int)ErrorCode.Fail;
                Log.Warn("player {0} get theme pass all level reward failed: not find themeType {1}", Owner.Uid, themeType);
                Owner.Write(response);
                return;
            }

            Dictionary<int, RechargeGiftModel> themePassDic = RechargeLibrary.GetRechargeGiftModelByGiftType(RechargeGiftType.ThemePass);
            RechargeGiftModel gift;
            if (themePassDic.TryGetValue(themeType, out gift))
            {
                if (!CheckThemePassOpened(gift))
                {
                    response.Result = (int)ErrorCode.Fail;
                    Log.Warn("player {0} get theme pass all level reward failed: time {1} not on theme pass {2} right time", Owner.Uid, ZoneServerApi.nowString, themeType);
                    Owner.Write(response);
                    return;
                }
            }
            else
            {
                response.Result = (int)ErrorCode.Fail;
                Log.Warn("player {0} get theme pass all level reward failed: time {1} not find theme pass {2} right time", Owner.Uid, ZoneServerApi.nowString, themeType);
                Owner.Write(response);
                return;
            }

            if (isSuper && !passItem.Bought)
            {
                response.Result = (int)ErrorCode.Fail;
                Log.Warn("player {0} get theme pass all level reward failed: not bought themePass {1}", Owner.Uid, themeType);
                Owner.Write(response);
                return;
            }

            List<string> rewardList = ThemePassLibrary.GetAllLeftThemePassLevelReward(passItem.ThemeType, passItem.PassLevel, passItem.BasicRewardLevelSet, false);

            if (passItem.Bought)
            {
                List<string> superRewardList = ThemePassLibrary.GetAllLeftThemePassLevelReward(passItem.ThemeType, passItem.PassLevel, passItem.SuperRewardLevelSet, true);
                rewardList.AddRange(superRewardList);
            }

            if (rewardList.Count > 0)
            {              
                foreach (var item in rewardList)
                {
                    RewardManager manager = new RewardManager();
                    manager.InitSimpleReward(item);
                    Owner.AddRewards(manager, ObtainWay.ThemePassLevelReward);
                    manager.GenerateRewardItemInfo(response.Rewards);
                }

                UpdateRewardLevelSet(passItem, isSuper, getAll);
                SyncDbThemePassInfo(passItem);
            }
            else
            {
                response.Result = (int)ErrorCode.Already;
            }

            Owner.Write(response);
        }

        public void GetLevelReward(int themeType, bool getAll, bool isSuper, List<int> rewardLevels)
        {
            MSG_ZGC_GET_THEMEPASS_REWARD response = new MSG_ZGC_GET_THEMEPASS_REWARD();

            response.ThemeType = themeType;
            response.GetAll = getAll;
            response.IsSuper = isSuper;
            response.RewardLevels.AddRange(rewardLevels);

            if (!CheckCanGetReward(themeType, isSuper, rewardLevels, response))
            {
                Owner.Write(response);
                return;
            }

            int rewardLevel = rewardLevels.FirstOrDefault();
            string reward = ThemePassLibrary.GetLevelReward(themeType, rewardLevel, isSuper);
            if (!string.IsNullOrEmpty(reward))
            {
                ThemePassItem passItem;
                themePassList.TryGetValue(themeType, out passItem);             

                RewardManager manager = new RewardManager();
                manager.InitSimpleReward(reward);
                Owner.AddRewards(manager, ObtainWay.ThemePassLevelReward);
                manager.GenerateRewardItemInfo(response.Rewards);

                UpdateRewardLevelSet(passItem, isSuper, getAll, rewardLevel);
                SyncDbThemePassInfo(passItem);

                response.Result = (int)ErrorCode.Success;
            }
            else
            {
                Log.Warn($"player {Owner.Uid} get theme pass {themeType} level reward failed: not find level {rewardLevel} reward");
                response.Result = (int)ErrorCode.NoData;
            }

            Owner.Write(response);
        }

        private List<string> ScreenNotReceivedRewards(SortedDictionary<int, string> rewardDic, SortedSet<int> rewardedSet)
        {
            List<string> rewardList = new List<string>();
            foreach (var item in rewardDic)
            {
                if (!rewardedSet.Contains(item.Key))
                {
                    rewardList.Add(item.Value);
                }
            }
            return rewardList;
        }

        private void UpdateRewardLevelSet(ThemePassItem passItem, bool isSuper, bool getAll, int rewardLevel = 1)
        {
            if (getAll)
            {
                if (passItem.Bought)
                {
                    SortedSet<int> superLevelSet = ThemePassLibrary.GetCurrentAllThemePassLevels(passItem.ThemeType, passItem.PassLevel, true);
                    passItem.UpdateSuperRewardLevelSet(superLevelSet);
                }
                SortedSet<int> basicLevelSet = ThemePassLibrary.GetCurrentAllThemePassLevels(passItem.ThemeType, passItem.PassLevel, true);
                passItem.UpdateBasicRewardLevelSet(basicLevelSet);
            }
            else
            {
                if (isSuper)
                {
                    passItem.AddSuperRewardLevel(rewardLevel);
                }
                else
                {
                    passItem.AddBasicRewardLevel(rewardLevel);
                }
            }
        }

        private bool CheckCanGetReward(int themeType, bool isSuper, List<int> rewardLevels, MSG_ZGC_GET_THEMEPASS_REWARD response)
        {           
            ThemePassItem passItem;
            if (!themePassList.TryGetValue(themeType, out passItem))
            {
                response.Result = (int)ErrorCode.Fail;
                Log.Warn("player {0} get theme pass level reward failed: not find themeType {1}", Owner.Uid, themeType);
                return false;
            }

            Dictionary<int, RechargeGiftModel> themePassDic = RechargeLibrary.GetRechargeGiftModelByGiftType(RechargeGiftType.ThemePass);
            RechargeGiftModel gift;
            if (themePassDic.TryGetValue(themeType, out gift))
            {
                if (!CheckThemePassOpened(gift))
                {
                    response.Result = (int)ErrorCode.Fail;
                    Log.Warn("player {0} get theme pass level reward failed: time {1} not on theme pass {2} right time", Owner.Uid, ZoneServerApi.nowString, themeType);
                    return false;
                }
            }
            else
            {
                response.Result = (int)ErrorCode.Fail;
                Log.Warn("player {0} get theme pass level reward failed: time {1} not find theme pass {2} right time", Owner.Uid, ZoneServerApi.nowString, themeType);
                return false;
            }

            if (isSuper && (passItem == null || !passItem.Bought))
            {
                response.Result = (int)ErrorCode.NotBought;
                Log.Warn("player {0} get theme pass level reward failed: not bought themePass {1}", Owner.Uid, themeType);
                return false;
            }

            if (rewardLevels.Count != 1)
            {
                response.Result = (int)ErrorCode.Fail;
                Log.Warn("player {0} get theme pass level reward failed: rewardLevels count {1} error", Owner.Uid, rewardLevels.Count);
                return false;
            }
            int rewardLevel = rewardLevels.FirstOrDefault();

            int passLevel = 1;
            if (passItem != null)
            {
                passLevel = passItem.PassLevel;
                if ((isSuper && passItem.SuperRewardLevelSet.Contains(rewardLevel)) || (!isSuper && passItem.BasicRewardLevelSet.Contains(rewardLevel)))
                {
                    response.Result = (int)ErrorCode.AlreadyGot;
                    Log.Warn("player {0} get theme pass level reward failed: already have {1}", Owner.Uid, rewardLevel);
                    return false;
                }
            }
            if (passLevel < rewardLevels.Max())
            {
                response.Result = (int)ErrorCode.PassLevelNotEnough;
                Log.Warn("player {0} get theme pass level reward failed: pass level {1} lower than param {2}", Owner.Uid, passLevel, rewardLevels.Max());
                return false;
            }
            return true;
        }

        /// <summary>
        /// 购买主题通行证
        /// </summary>
        public MSG_ZGC_BUY_THEMEPASS_RESULT BuyThemePass(int themeType)
        {
            MSG_ZGC_BUY_THEMEPASS_RESULT response = new MSG_ZGC_BUY_THEMEPASS_RESULT();
            response.ThemeType = themeType;
           
            ThemePassItem passItem;
            if (!themePassList.TryGetValue(themeType, out passItem))
            {
                response.Bought = false;
                response.Result = (int)ErrorCode.Fail;
                Log.Warn("player {0} buy theme pass failed: not find themeType {1}", Owner.Uid, themeType);
                return response;
            }

            if (passItem.Bought)
            {
                response.Bought = false;
                response.Result = (int)ErrorCode.Already;
                Log.Warn("player {0} buy theme pass failed: already bought themeType {1}", Owner.Uid, themeType);
                return response;
            }
            else
            {
                passItem.ChangeBuyState(true);
                SyncDbThemePassBuyState(passItem);

                response.Bought = true;
                response.Result = (int)ErrorCode.Success;
                return response;
            }
        }
           
        public bool CheckCanBuyThemePass(int themeType)
        {
            List<int> activityList;
            if (!RechargeLibrary.CheckInRechargeGiftTime(RechargeGiftType.ThemePass, ZoneServerApi.now, out activityList))
            {
                return false;
            }
            if (!activityList.Contains(themeType))
            {
                return false;
            }
            ThemePassItem passItem;
            if (themePassList.TryGetValue(themeType, out passItem) && passItem.Bought)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 主题通行证开启
        /// </summary>     
        public void OpenThemePass(int themeType)
        {
            ThemePassItem passItem;
            if (!themePassList.TryGetValue(themeType, out passItem))
            {
                passItem = new ThemePassItem(themeType);
                themePassList.Add(themeType, passItem);
                SyncDbInsertThemePassInfo(passItem);
            }
        }

        public void CheckUpdateThemePass(int timeType, RechargeGiftModel gift)
        {           
            switch (timeType)
            {
                case 1:
                    if (ZoneServerApi.now >= gift.StartTime && ZoneServerApi.now < gift.EndTime)
                    {
                        OpenThemePass(gift.SubType);
                        //UpdateCurThemePassType(gift.SubType);
                    }
                    else if (ZoneServerApi.now >= gift.EndTime)
                    {
                        CheckHasLeftLevelReward(gift.SubType);
                    }
                    break;
                case 2:
                    int day = (Owner.server.Now().Date - Owner.server.OpenServerDate).Days;
                    if (day >= gift.ServerOpenDayStart && day < gift.ServerOpenDayEnd)
                    {
                        OpenThemePass(gift.SubType);
                        //UpdateCurThemePassType(gift.SubType);
                    }
                    else if (day >= gift.ServerOpenDayEnd)
                    {
                        CheckHasLeftLevelReward(gift.SubType);
                    }
                    break;
                default:
                    break;
            }
        }
         
        /// <summary>
        /// 增加主题通行证经验
        /// </summary>
        public void AddThemePassExp(int itemId, int addCount)
        {
            int themeType = ThemePassLibrary.GetThemePassTypeByItemId(itemId);
            ThemePassItem passItem;
            themePassList.TryGetValue(themeType, out passItem);
            if (passItem == null)
            {
                return;
            }
            //检查是否在活动期
            int day = (Owner.server.Now().Date - Owner.server.OpenServerDate).Days;
            Dictionary<int, RechargeGiftModel> themePassDic = RechargeLibrary.GetRechargeGiftModelByGiftType(RechargeGiftType.ThemePass);
            RechargeGiftModel themePass;
            themePassDic.TryGetValue(themeType, out themePass);
            if (themePass.StartTime != DateTime.MinValue && (Owner.server.Now() < themePass.StartTime || Owner.server.Now() >= themePass.EndTime))
            {
                return;
            }
            else if (themePass.ServerOpenDayEnd > 0 && (day < themePass.ServerOpenDayStart || day >= themePass.ServerOpenDayEnd))
            {
                return;
            }

            SortedDictionary<int, ThemePassLevel> passLevelDic = ThemePassLibrary.GetThemePassLevelByThemeType(themeType);
            AddRealPassExp(passItem, addCount, passLevelDic);//
            CheckPassLevelUp(passItem, passLevelDic);
            SyncDbUpdateThemePassLevelExp(passItem);
            NotifyThemePassExpChange(passItem);
        }

        private void AddRealPassExp(ThemePassItem passItem, int addCount, SortedDictionary<int, ThemePassLevel> passLevelDic)
        {
            int exp = addCount * ThemePassLibrary.ExpRatio;
            int realAddExp = 0;
            int origin = passItem.Exp;
            ThemePassLevel levelModel = passLevelDic.Values.LastOrDefault();
            if (origin + exp > levelModel.Exp)
            {
                realAddExp = levelModel.Exp - origin;
                passItem.AddExp(realAddExp);
            }
            else
            {
                realAddExp = exp;
                passItem.AddExp(realAddExp);
            }
        }

        private void CheckPassLevelUp(ThemePassItem passItem, SortedDictionary<int, ThemePassLevel> levelDic)
        {
            int curLevel = passItem.PassLevel;
            if (curLevel == ThemePassLibrary.MaxPassLevel || curLevel == levelDic.Keys.LastOrDefault())
            {
                return;
            }
            ThemePassLevel levelModel;
            while (true)
            {
                if (levelDic.TryGetValue(curLevel++, out levelModel) && levelModel.Exp <= passItem.Exp)
                {
                    if (passItem.PassLevel == ThemePassLibrary.MaxPassLevel)
                    {
                        return;
                    }
                    passItem.PassLevelUp();
                }
                else
                {
                    break;
                }
            }
        }

        /// <summary>
        /// 通知主题通行证经验变动
        /// </summary>      
        public void NotifyThemePassExpChange(ThemePassItem passItem)
        {       
            MSG_ZGC_THEMEPASS_EXP_CHANGE notify = new MSG_ZGC_THEMEPASS_EXP_CHANGE();

            ZGC_THEMEPASS_EXP_INFO info = new ZGC_THEMEPASS_EXP_INFO();
            info.ThemeType = passItem.ThemeType;
            info.PassLevel = passItem.PassLevel;
            info.Exp = passItem.Exp;
            info.ExpItemCount = passItem.Exp / ThemePassLibrary.ExpRatio;
            notify.List.Add(info);

            Owner.Write(notify);
        }

        private void CheckHasLeftLevelReward(int themeType)
        {           
            ThemePassItem passItem;
            themePassList.TryGetValue(themeType, out passItem);
            if (passItem == null)
            {
                return;
            }

            List<string> rewardList = ThemePassLibrary.GetAllLeftThemePassLevelReward(passItem.ThemeType, passItem.PassLevel, passItem.BasicRewardLevelSet, false);
            if (passItem.Bought)
            {
                List<string> superRewardList = ThemePassLibrary.GetAllLeftThemePassLevelReward(passItem.ThemeType, passItem.PassLevel, passItem.SuperRewardLevelSet, true);
                rewardList.AddRange(superRewardList);
            }
            if (rewardList.Count > 0)
            {
                string rewards = "";
                int rewardCount = 0;
                foreach (var item in rewardList)
                {
                    rewards += "|" + item;
                    rewardCount++;
                    if (rewardCount == ThemePassLibrary.PerEmailRewardCount)
                    {
                        ThemePassOverTimeSendlRewardEmail(rewards);
                        rewards = "";
                        rewardCount = 0;
                    }
                }
                if (!string.IsNullOrEmpty(rewards) && rewardCount > 0)
                {
                    ThemePassOverTimeSendlRewardEmail(rewards);
                }
                UpdateRewardLevelSet(passItem, passItem.Bought, true);
                SyncDbThemePassInfo(passItem);
            }
        }

        /// <summary>
        /// 通行证到期邮件补发奖励
        /// </summary>
        private void ThemePassOverTimeSendlRewardEmail(string rewards)
        {
            Owner.SendPersonEmail(ThemePassLibrary.OverTimeRewardEmail, "", rewards);
        }

        private bool CheckThemePassOpened(RechargeGiftModel gift)
        {
            int timeType = 0;
            if (gift.StartTime != DateTime.MinValue)
            {
                timeType = 1;
            }
            else if (gift.ServerOpenDayEnd > 0)
            {
                timeType = 2;
            }
            switch (timeType)
            {
                case 1:
                    if (ZoneServerApi.now >= gift.StartTime && ZoneServerApi.now < gift.EndTime)
                    {
                        return true;
                    }
                    break;              
                case 2:
                    int day = (Owner.server.Now().Date - Owner.server.OpenServerDate).Days;
                    if (day >= gift.ServerOpenDayStart && day < gift.ServerOpenDayEnd)
                    {
                        return true;
                    }
                    break;
                default:
                    break;
            }
            return false;
        }      

        //private void UpdateCurThemePassType(int themeType)
        //{
        //    CurThemeType = themeType;
        //}

        #region syncDb
        private void SyncDbThemePassInfo(ThemePassItem passItem)
        {
            QueryUpdateThemePassRewardedLevels updateQuery = new QueryUpdateThemePassRewardedLevels(Owner.Uid, passItem.ThemeType, passItem.GetBasicRewardedLevelsStr(), passItem.GetSuperRewardedLevelsStr());
            Owner.server.GameDBPool.Call(updateQuery);
        }

        private void SyncDbThemePassBuyState(ThemePassItem passItem)
        {
            QueryUpdateThemePassBuyState updateQuery = new QueryUpdateThemePassBuyState(Owner.Uid, passItem.ThemeType, passItem.GetBoughtState());
            Owner.server.GameDBPool.Call(updateQuery);
        }

        private void SyncDbInsertThemePassInfo(ThemePassItem passItem)
        {
            QueryInsertThemePassInfo query = new QueryInsertThemePassInfo(Owner.Uid, passItem.ThemeType);
            Owner.server.GameDBPool.Call(query);
        }

        private void SyncDbUpdateThemePassLevelExp(ThemePassItem passItem)
        {
            QueryUpdateThemePassLevelExp query = new QueryUpdateThemePassLevelExp(Owner.Uid, passItem.ThemeType, passItem.PassLevel, passItem.Exp);
            Owner.server.GameDBPool.Call(query);
        }
        #endregion

        public void GenerateThemePassTransformMsg(ZMZ_THEME_INFO info)
        {
            foreach (var item in themePassList)
            {
                info.List.Add(GenerateThemePassItemTransformMsg(item.Value));
            }
            //info.ThemePassType = CurThemeType;
        }

        private ZMZ_THEMEPASS_ITEM GenerateThemePassItemTransformMsg(ThemePassItem passItem)
        {
            ZMZ_THEMEPASS_ITEM info = new ZMZ_THEMEPASS_ITEM();
            info.ThemeType = passItem.ThemeType;
            info.PassLevel = passItem.PassLevel;
            info.Exp = passItem.Exp;
            info.Bought = passItem.Bought;
            foreach (var level in passItem.BasicRewardLevelSet)
            {
                info.BasicRewardedLevels += level + "|";
            }
            foreach (var level in passItem.SuperRewardLevelSet)
            {
                info.SuperRewardedLevels += level + "|";
            }
            return info;
        }

        public void LoadThemePassInfoTransform(ZMZ_THEME_INFO info)
        {
            foreach (var item in info.List)
            {
                ThemePassItem passItem = new ThemePassItem(item.ThemeType);
                passItem.SetPassLevel(item.PassLevel);
                passItem.SetExp(item.Exp);
                passItem.SetBoughtState(item.Bought);
                passItem.SetBasicRewardLevelSet(item.BasicRewardedLevels);
                passItem.SetSuperRewardLevelSet(item.SuperRewardedLevels);
                themePassList.Add(passItem.ThemeType, passItem);
            }
            //CurThemeType = info.ThemePassType;
        }
    }
}
