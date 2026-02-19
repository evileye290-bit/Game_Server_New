using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Google.Protobuf.Collections;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using Message.Zone.Protocol.ZR;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        private int usedPcUid = 0;
        //key:code_id
        private Dictionary<int, GiftCodeItem> itemDic = new Dictionary<int, GiftCodeItem>();
        //key:mode
        private Dictionary<int, List<GiftCodeItem>> modeDic = new Dictionary<int, List<GiftCodeItem>>();
        //key:code_id, value: useCount
        private Dictionary<int, int> codeUseCountDic = new Dictionary<int, int>();
        
        private List<string> sameModeCodeList = new List<string>();

        public GiftManager GiftManager { get; private set; }

        public void InitGiftManager()
        {
            GiftManager = new GiftManager(this);
        }

        public void InitGiftInfo(Dictionary<RechargeGiftType, DbGiftInfo> giftInfo, DbGiftInfo limitTimeGifts, Dictionary<int, DbGift2Info> cultivateGifts, DbRegularGiftInfo pettyGift, 
            Dictionary<int, DailyRechargeInfo> dailyRechargeInfos, Dictionary<int, HeroDaysRewardsInfo> daysRewardsInfos, Dictionary<int, NewServerPromotionInfo> newServerPromotionInfos)
        {
            GiftManager.BindGiftInfo(giftInfo);
            GiftManager.BindLimitTimeGiftInfo(limitTimeGifts);
            GiftManager.BindCultivateGiftInfo(cultivateGifts);
            GiftManager.BindPettyGiftInfo(pettyGift);
            GiftManager.BindDailyRechargeInfo(dailyRechargeInfos);
            GiftManager.BindHeroDaysRewardsInfo(daysRewardsInfos);
            GiftManager.BindNewServerPromotionInfo(newServerPromotionInfos);
        }

        #region 礼包码
        public void InitGiftCodeUseInfo(List<GiftCodeItem> list)
        {
            List<GiftCodeItem> modeList;
            foreach (var item in list)
            {
                CommonGiftCode model = GiftLibrary.GetCommonGiftCodeById(item.Id);
                if (model != null)
                {
                    item.CommonCodeData = model;
                }
                else
                {
                    GiftCodeInfo gift = GiftLibrary.GetGiftCodeInfoByCodeId(item.Id);
                    item.CodeData = gift;
                }
                itemDic.Add(item.Id, item);
                if (!modeDic.TryGetValue(item.CodeMode, out modeList))
                {
                    modeList = new List<GiftCodeItem>() { item };
                    modeDic.Add(item.CodeMode, modeList);
                }
                else
                {
                    modeList.Add(item);
                }
            }
        }

        public void CheckGiftCodeExchangeReward(string giftCode)
        {
            MSG_ZGC_GIFT_CODE_REWARD response = new MSG_ZGC_GIFT_CODE_REWARD();

            if (string.IsNullOrEmpty(giftCode))
            {
                response.Result = (int)ErrorCode.Fail;
                Log.Warn("player {0} use gift code exchange reward fail : gift code is null or empty", Uid);
                Write(response);
                return;
            }

            string code = giftCode.Trim();
            //统一转换成大写
            code = code.ToUpper();
            if (code.Length < 4)
            {
                response.Result = (int)ErrorCode.Fail;
                Log.Warn("player {0} use gift code exchange reward fail : gift code param error", Uid);
                Write(response);
                return;
            }
            //先检查码在不在通用码表，在的话执行完直接返回
            CommonGiftCode model = GiftLibrary.GetCommonGiftCodeByCode(code);
            if (model != null)
            {
                CommonGiftCodeExchangeReward(code, model, response);
            }
            else
            {
                MSG_ZR_CHECK_GIFT_CODE_REWARD request = new MSG_ZR_CHECK_GIFT_CODE_REWARD()
                {
                    GiftCode = code,
                };
                server.SendToRelation(request, Uid);
            }
        }
        public void GiftCodeExchangeReward(string code, bool checkResult)
        {
            MSG_ZGC_GIFT_CODE_REWARD response = new MSG_ZGC_GIFT_CODE_REWARD();
            //检查传入的是否是真的礼包码
            if (!checkResult)
            {
                response.Result = (int)ErrorCode.CodeNotExist;
                Log.Warn("player {0} use gift code exchange reward fail : gift code {1} not exist in txt", Uid, code);
                Write(response);
                return;
            }
            //if (string.IsNullOrEmpty(giftCode))
            //{
            //    response.Result = (int)ErrorCode.Fail;
            //    Log.Warn("player {0} use gift code exchange reward fail : gift code is null or empty", Uid);
            //    Write(response);
            //    return;
            //}

            //string code = giftCode.Trim();
            ////统一转换成大写
            //code = code.ToUpper();
            //if (code.Length < 4)
            //{
            //    response.Result = (int)ErrorCode.Fail;
            //    Log.Warn("player {0} use gift code exchange reward fail : gift code param error", Uid);
            //    Write(response);
            //    return;
            //}
            string codeTop4 = code.Substring(0, 4);

            ////检查传入的是否是真的礼包码
            //if (!CheckInputIsRightCode(code, codeTop4))
            //{
            //    response.Result = (int)ErrorCode.CodeNotExist;
            //    Log.Warn("player {0} use gift code exchange reward fail : gift code {1} not exist in txt", Uid, code);
            //    Write(response);
            //    return;
            //}
            ////先检查码在不在通用码表，在的话执行完直接返回
            //CommonGiftCode model = GiftLibrary.GetCommonGiftCodeByCode(code);
            //if (model != null)
            //{
            //    CommonGiftCodeExchangeReward(code, model, response);
            //    return;
            //}

            GiftCodeInfo gift = GiftLibrary.GetGiftCodeInfoByCodeStr(codeTop4);
            if (gift == null)
            {
                response.Result = (int)ErrorCode.CodeNotExist;
                Log.Warn("player {0} use gift code exchange reward fail : gift code {1} not exist", Uid, codeTop4);
                Write(response);
                return;
            }

            //检查该类礼包码是否已使用
            if (CheckCodeIsUsed(gift.Id, code))
            {
                response.Result = (int)ErrorCode.TypeCodeAlreayUse;
                Log.Warn("player {0} use gift code exchange reward fail : this type gift code {1} already used", Uid, gift.CodeTop4);
                Write(response);
                return;
            }

            //检查礼包码是否可用
            if (!CheckCodeCanUse(gift))
            {
                response.Result = (int)ErrorCode.TypeCodeAlreayUse;
                Log.Warn("player {0} use gift code exchange reward fail : gift code {1} mode {2} type {3} subType {4} already used", Uid, gift.CodeTop4, gift.Mode, gift.Type, gift.SubType);
                Write(response);
                return;
            }

            //检查时间
            if (!CheckIsOnRightTime(gift.BeginTime, gift.EndTime))
            {
                response.Result = (int)ErrorCode.NotOnRightTime;
                Log.Warn("player {0} use gift code exchange reward fail : gift code {1} not in right time", Uid, gift.CodeTop4);
                Write(response);
                return;
            }

            //检测渠道
            if (!CheckChannelIsRight(gift.Channels))
            {
                response.Result = (int)ErrorCode.ChannelError;
                Log.Warn("player {0} use gift code exchange reward fail : gift code {1} channel {2} not right", Uid, gift.CodeTop4, ChannelName);
                Write(response);
                return;
            }

            if (gift.Mode == 3)
            {
                //mode3礼包码
                InsertModeGiftCode(code, gift.Mode);
            }
            else
            {
                GiftCodeItem item = CreateGiftCodeItem(gift);
                SaveGiftCodeItemInfo(item);
                //同步db
                SyncDbInsertGiftCodeUse(item);
                //礼包码为唯一码时
                if (item.CodeData.IsUniversal == 0)
                {
                    SyncDbInsertGiftCodeList(code);
                }
            }          

            //发奖励邮件
            MSG_ZR_GIFT_CODE_REWARD request = new MSG_ZR_GIFT_CODE_REWARD()
            {
                PcUid = Uid,
                EmailId = gift.EmailId,
                Rewards = gift.Rewards
            };
            server.SendToRelation(request, Uid);
            BIRecordPackageCodeLog(code, string.Join("|", gift.Channels), false, gift.Id);

            //BI：激活码
            KomoeEventLoguGiftCodeExchange(code, "成功", "", RewardManager.GetRewardDic(gift.Rewards));
        }

        private bool CheckInputIsRightCode(string code, string codeTop4)
        {
            CommonGiftCode model = GiftLibrary.GetCommonGiftCodeByCode(code);
            if (model != null)
            {
                return true;
            }

            //List<string> list;
            //ZoneServerApi.GiftCodeList.TryGetValue(codeTop4, out list);
            //if (list != null && list.Contains(code))
            //{
            //    return true;
            //}
            return false;
        }

        private bool CheckCodeIsUsed(int codeId, string code)
        {
            GiftCodeItem item;
            if (itemDic.TryGetValue(codeId, out item) || sameModeCodeList.Contains(code))
            {
                return true;
            }
            return false;
        }

        private bool CheckCommonCodeCanUse(int codeId, int maxUseCount)
        {
            GiftCodeItem item;
            int useCount;
            if (itemDic.TryGetValue(codeId, out item))
            {
                return false;
            }
            else if (!codeUseCountDic.TryGetValue(codeId, out useCount))
            {
                return true;
            }                     
            else if (useCount < maxUseCount || maxUseCount == 0)
            {
                return true;
            }            
            return false;
        }

        private bool CheckIsOnRightTime(string codeBeginTime, string codeEndTime)
        {
            DateTime startTime = DateTime.Parse(codeBeginTime);
            if (startTime > ZoneServerApi.now)
            {
                return false;
            }
            DateTime endTime = DateTime.Parse(codeEndTime);
            if (endTime < ZoneServerApi.now)
            {
                return false;
            }
            return true;
        }

        private bool CheckChannelIsRight(string[] channels)
        {
            if (channels.Length == 0)
            {
                return true;
            }
            if (!string.IsNullOrEmpty(ChannelName) && channels.Contains(ChannelName))
            {
                return true;
            }
            return false;
        }

        private bool CheckCodeCanUse(GiftCodeInfo gift)
        {
            if (gift.IsUniversal == 0 && usedPcUid > 0)
            {
                //usedPcUid = 0;
                return false;
            }
            List<GiftCodeItem> list;
            if (modeDic.TryGetValue(gift.Mode, out list))
            {
                switch (gift.Mode)
                {
                    case 1:
                        foreach (var item in list)
                        {
                            if (item.CodeData != null && item.CodeData.Type == gift.Type)
                            {
                                return false;
                            }
                            if (item.CommonCodeData != null && item.CommonCodeData.Type == gift.Type)
                            {
                                return false;
                            }
                        }
                        break;
                    case 2:
                        foreach (var item in list)
                        {
                            if (item.CodeData != null && item.CodeData.SubType == gift.SubType)
                            {
                                return false;
                            }
                            if (item.CommonCodeData != null && item.CommonCodeData.SubType == gift.SubType)
                            {
                                return false;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }          
            return true;
        }

        private bool CheckCodeCanUse(CommonGiftCode gift)
        {
            //if (gift.IsUniversal == 0 && usedPcUid > 0)
            //{
            //    usedPcUid = 0;
            //    return false;
            //}
            List<GiftCodeItem> list;
            if (modeDic.TryGetValue(gift.Mode, out list))
            {
                switch (gift.Mode)
                {
                    case 1:
                        foreach (var item in list)
                        {
                            if (item.CommonCodeData != null && item.CommonCodeData.Type == gift.Type)
                            {
                                return false;
                            }
                            if (item.CodeData != null && item.CodeData.Type == gift.Type)
                            {
                                return false;
                            }
                        }
                        break;
                    case 2:
                        foreach (var item in list)
                        {
                            if (item.CommonCodeData != null && item.CommonCodeData.SubType == gift.SubType)
                            {
                                return false;
                            }
                            if (item.CodeData != null && item.CodeData.SubType == gift.SubType)
                            {
                                return false;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            return true;
        }       

        private void LoadGiftCodeListWithQuerys(string code, MSG_ZGC_CHECK_CODE_UNIQUE response)
        {
            //DB获取数据
            //List<AbstractDBQuery> querys = new List<AbstractDBQuery>();

            QueryLoadGiftCodeList queryGiftCodeList = new QueryLoadGiftCodeList(code);
            //querys.Add(queryGiftCodeList);

            //DBQueryTransaction dBQuerysWithoutTransaction = new DBQueryTransaction(querys, true);
            server.GameDBPool.Call(queryGiftCodeList, (ret =>
            {
                if ((int)ret == 1)
                {
                    usedPcUid = queryGiftCodeList.PcUid;
                }
                response.Result = (int)ErrorCode.Success;
                Write(response);
            }));
        }      

        private void LoadDbGiftCodeInSameMode(string code, MSG_ZGC_CHECK_CODE_UNIQUE response)
        {
            if (sameModeCodeList.Contains(code))
            {
                response.Result = (int)ErrorCode.Success;
                Write(response);
                return;
            }
            ////DB获取数据
            //List<AbstractDBQuery> querys = new List<AbstractDBQuery>();

            QueryLoadGiftCodeInSameMode queryCodeInSameMode = new QueryLoadGiftCodeInSameMode(code);
            //querys.Add(queryCodeInSameMode);

            //DBQueryTransaction dBQuerysWithoutTransaction = new DBQueryTransaction(querys, true);
            server.GameDBPool.Call(queryCodeInSameMode, (ret =>
            {
                if ((int)ret == 1 && queryCodeInSameMode.PcUid > 0)
                {
                    sameModeCodeList.Add(code);
                }
                response.Result = (int)ErrorCode.Success;
                Write(response);
            }));
        }

        private GiftCodeItem CreateGiftCodeItem(GiftCodeInfo gift)
        {
            GiftCodeItem item = new GiftCodeItem();
            item.Id = gift.Id;
            item.CodeMode = gift.Mode;
            switch (item.CodeMode)
            {
                case 1:
                    item.SubCode = gift.Type;
                    break;
                case 2:
                    item.SubCode = gift.SubType;
                    break;
                default:
                    break;
            }
            item.CodeData = gift;
            return item;
        }

        private GiftCodeItem CreateGiftCodeItem(CommonGiftCode gift)
        {
            GiftCodeItem item = new GiftCodeItem();
            item.Id = gift.Id;
            item.CodeMode = gift.Mode;
            switch (item.CodeMode)
            {
                case 1:
                    item.SubCode = gift.Type;
                    break;
                case 2:
                    item.SubCode = gift.SubType;
                    break;
                default:
                    break;
            }
            item.CommonCodeData = gift;
            return item;
        }

        private void SaveGiftCodeItemInfo(GiftCodeItem item)
        {
            itemDic.Add(item.Id, item);
            List<GiftCodeItem> modeList;
            if (!modeDic.TryGetValue(item.CodeMode, out modeList))
            {
                modeList = new List<GiftCodeItem>() { item };
                modeDic.Add(item.CodeMode, modeList);
            }
            else
            {
                modeList.Add(item);
            }
        }

        private void SyncDbInsertGiftCodeUse(GiftCodeItem item)
        {
            server.GameDBPool.Call(new QueryInsertGiftCodeUse(Uid, item.Id, item.CodeMode, item.SubCode));
        }

        private void SyncDbInsertGiftCodeList(string code)
        {
            server.GameDBPool.Call(new QueryInsertGiftCodeList(Uid, code));
        }

        public void CheckUniqueCodeInfo(string giftCode)
        {
            MSG_ZGC_CHECK_CODE_UNIQUE response = new MSG_ZGC_CHECK_CODE_UNIQUE();

            if (string.IsNullOrEmpty(giftCode))
            {
                response.Result = (int)ErrorCode.Fail;
                Log.Warn("player {0} check gift code unique fail : gift code is null or empty", Uid);
                Write(response);
                return;
            }

            string code = giftCode.Trim();
            //统一转换成大写
            code = code.ToUpper();
            if (code.Length < 4)
            {
                response.Result = (int)ErrorCode.Fail;
                Log.Warn("player {0} check gift code unique fail : gift code param error", Uid);
                Write(response);
                return;
            }
            CommonGiftCode model = GiftLibrary.GetCommonGiftCodeByCode(code);
            if (model != null)
            {
                LoadDbCommonCodeUseCount(model.Id, response);
                //response.Result = (int)ErrorCode.Success;
                //Write(response);
            }
            else
            {
                MSG_ZR_CHECK_CODE_UNIQUE request = new MSG_ZR_CHECK_CODE_UNIQUE()
                {
                    GiftCode = code,
                };
                server.SendToRelation(request, Uid);
            }
        }

        public void CheckAndSaveUniqueCodeInfo(string code, bool checkResult)
        {
            MSG_ZGC_CHECK_CODE_UNIQUE response = new MSG_ZGC_CHECK_CODE_UNIQUE();
            if (!checkResult)
            {
                response.Result = (int)ErrorCode.CodeNotExist;
                Log.Warn("player {0} check gift code unique fail fail : gift code {1} not exist in txt", Uid, code);
                Write(response);
                return;
            }
            //if (string.IsNullOrEmpty(giftCode))
            //{
            //    response.Result = (int)ErrorCode.Fail;
            //    Log.Warn("player {0} check gift code unique fail : gift code is null or empty", Uid);
            //    Write(response);
            //    return;
            //}

            //string code = giftCode.Trim();
            ////统一转换成大写
            //code = code.ToUpper();
            //if (code.Length < 4)
            //{
            //    response.Result = (int)ErrorCode.Fail;
            //    Log.Warn("player {0} check gift code unique fail : gift code param error", Uid);
            //    Write(response);
            //    return;
            //}
            string codeTop4 = code.Substring(0, 4);

            ////检查传入的是否是真的礼包码
            //if (!CheckInputIsRightCode(code, codeTop4))
            //{
            //    response.Result = (int)ErrorCode.CodeNotExist;
            //    Log.Warn("player {0} check gift code unique fail fail : gift code {1} not exist in txt", Uid, code);
            //    Write(response);
            //    return;
            //}
            CommonGiftCode model = GiftLibrary.GetCommonGiftCodeByCode(code);
            if (model != null)
            {
                LoadDbCommonCodeUseCount(model.Id, response);
                //response.Result = (int)ErrorCode.Success;
                //Write(response);
            }
            else
            {
                GiftCodeInfo gift = GiftLibrary.GetGiftCodeInfoByCodeStr(codeTop4);
                if (gift == null)
                {
                    response.Result = (int)ErrorCode.CodeNotExist;
                    Log.Warn("player {0} check gift code unique fail fail : gift code {1} not exist", Uid, codeTop4);
                    Write(response);
                    return;
                }
                if (gift.Mode == 3)
                {
                    LoadDbGiftCodeInSameMode(code, response);
                }
                else
                {
                    SaveUniqueCodeUsePcUid(gift, code, response);
                }
            }
        }

        private void SaveUniqueCodeUsePcUid(GiftCodeInfo gift, string code, MSG_ZGC_CHECK_CODE_UNIQUE response)
        {
            if (gift.IsUniversal == 0)
            {
                LoadGiftCodeListWithQuerys(code, response);
            }
            else
            {
                response.Result = (int)ErrorCode.Success;
                Write(response);
            }
        }

        private void CommonGiftCodeExchangeReward(string giftCode, CommonGiftCode model, MSG_ZGC_GIFT_CODE_REWARD response)
        {
            //检查该类礼包码是否已使用
            if (!CheckCommonCodeCanUse(model.Id, model.MaxUseCount))
            {
                response.Result = (int)ErrorCode.CodeUseMaxCount;
                Log.Warn("player {0} use gift code exchange reward fail : this type gift code {1} reach max use limit", Uid, model.Code);
                Write(response);
                return;
            }

            //检查礼包码是否可用
            if (!CheckCodeCanUse(model))
            {
                response.Result = (int)ErrorCode.TypeCodeAlreayUse;
                Log.Warn("player {0} use gift code exchange reward fail : gift code {1} mode {2} type {3} subType {4} already used", Uid, model.Code, model.Mode, model.Type, model.SubType);
                Write(response);
                return;
            }

            //检查时间
            if (!CheckIsOnRightTime(model.BeginTime, model.EndTime))
            {
                response.Result = (int)ErrorCode.NotOnRightTime;
                Log.Warn("player {0} use gift code exchange reward fail : gift code {1} not in right time", Uid, model.Code);
                Write(response);
                return;
            }

            //检测渠道
            if (!CheckChannelIsRight(model.Channels))
            {
                response.Result = (int)ErrorCode.ChannelError;
                Log.Warn("player {0} use gift code exchange reward fail : gift code {1} channel {2} not right", Uid, model.Code, ChannelName);
                Write(response);
                return;
            }

            GiftCodeItem item = CreateGiftCodeItem(model);
            SaveGiftCodeItemInfo(item);
            SyncDbInsertGiftCodeUse(item);
            //通用码使用总人数限制
            UpdateGiftCodeUseCount(item.Id);

            //礼包码为唯一码时
            //if (item.CommonCodeData.IsUniversal == 0)
            //{
            //    SyncDbInsertGiftCodeList(code);
            //}

            //发奖励邮件
            MSG_ZR_GIFT_CODE_REWARD request = new MSG_ZR_GIFT_CODE_REWARD()
            {
                PcUid = Uid,
                EmailId = model.EmailId,
                Rewards = model.Rewards
            };
            server.SendToRelation(request, Uid);

            BIRecordPackageCodeLog(giftCode, string.Join("|", model.Channels), false, 0);
            //BI：激活码
            KomoeEventLoguGiftCodeExchange(giftCode, "成功", "", RewardManager.GetRewardDic(model.Rewards));
        }

        private void UpdateGiftCodeUseCount(int codeId)
        {
            if (!codeUseCountDic.ContainsKey(codeId))
            {
                codeUseCountDic.Add(codeId, 1);
                SyncDbInsertGiftCodeUseCount(codeId, 1);
            }
            else
            {
                codeUseCountDic[codeId]++;
                SyncDbUpdateGiftCodeUseCount(codeId, codeUseCountDic[codeId]);
            }
        }

        private void InsertModeGiftCode(string code, int codeMode)
        {
            sameModeCodeList.Add(code);
            server.GameDBPool.Call(new QueryInsertSameModeGiftCode(code, Uid, codeMode));
        }

        private void SyncDbInsertGiftCodeUseCount(int codeId, int useCount)
        {
            server.GameDBPool.Call(new QueryInsertGiftCodeUseCount(codeId, useCount));
        }

        private void SyncDbUpdateGiftCodeUseCount(int codeId, int useCount)
        {
            server.GameDBPool.Call(new QueryUpdateGiftCodeUseCount(codeId, useCount));
        }

        private void LoadDbCommonCodeUseCount(int codeId, MSG_ZGC_CHECK_CODE_UNIQUE response)
        {
            if (codeUseCountDic.ContainsKey(codeId))
            {
                response.Result = (int)ErrorCode.Success;
                Write(response);
                return;
            }
            //DB获取数据
            //List<AbstractDBQuery> querys = new List<AbstractDBQuery>();

            QueryLoadGiftCodeUseCount queryGiftCodeUseCount = new QueryLoadGiftCodeUseCount(codeId);
            //querys.Add(queryGiftCodeUseCount);

            //DBQueryTransaction dBQuerysWithoutTransaction = new DBQueryTransaction(querys, true);
            server.GameDBPool.Call(queryGiftCodeUseCount, (ret =>
            {
                if ((int)ret == 1 && queryGiftCodeUseCount.UseCount > 0)
                {
                    codeUseCountDic.Add(codeId, queryGiftCodeUseCount.UseCount);
                }
                response.Result = (int)ErrorCode.Success;
                Write(response);
            }));
        }

        #endregion

        #region 礼包
        public void SendGiftInfo()
        {
            MSG_ZGC_GIFT_INFO msg = GiftManager.GenerateGiftInfoMsg();
            Write(msg);

            MSG_ZGC_LIMIT_TIME_GIFTS notify = GiftManager.GenerateOpenedGiftListMsg();
            Write(notify);

            MSG_ZGC_CULTIVATE_GIFT_LIST culGiftMsg = GiftManager.GenerateCultivateGiftListMsg();
            Write(culGiftMsg);
          
            MSG_ZGC_PETTY_GIFT_LIST pettyGiftMsg = GiftManager.GeneratePettyGiftListMsg();
            Write(pettyGiftMsg);

            MSG_ZGC_DAILY_RECHARGE_INFO dailyRechargeMsg = GiftManager.GenerateDailyRechargeMsg();
            Write(dailyRechargeMsg);

            MSG_ZGC_HERO_DAYS_REWARDS_INFO daysRewardsMsg = GiftManager.GenerateHeroDaysRewardsMsg();
            Write(daysRewardsMsg);

            MSG_ZGC_NEWSERVER_PROMOTION_INFO newServerPromotionMsg = GiftManager.GenerateNewServerPromotionMsg();
            Write(newServerPromotionMsg);

            GiftManager.SendLuckyFlipCardMsg();

            GiftManager.SendIslandHighGiftInfo();

            GiftManager.SendTreasureFlipCardMsg();
        }

        public GiftItem UpdateGiftItem(RechargeItemModel recharge)
        {
             return GiftManager.UpdateGiftItem(recharge);
        }

        public void RefreshDailyRechargeGift()
        {
            GiftManager.RefreshDailyRechargeGift();
        }

        public void RefreshWeeklyRechargeGift()
        {
            GiftManager.RefreshWeeklyRechargeGift();
        }

        public void RefreshMonthlyRechargeGift()
        {
            GiftManager.RefreshMonthlyRechargeGift();
        }
      
        //public bool CheckCanBuyRechargeGift(int rechargeId, ulong rechargeUid, string orderId, float amount, RechargeItemModel rechargeItem, RechargePriceModel price, bool hasDiscount)
        //{
        //    if (rechargeItem == null)
        //    {
        //        Log.Error($"player {Uid} recharge  rechargeId {rechargeId} order {orderId} get error, not find rechargeItem");
        //        return false;
        //    }
        //    if (price.Money != (int)amount && (hasDiscount && price.DiscountMoney != (int)amount))
        //    {
        //        Log.Error($"player {Uid} recharge  rechargeId {rechargeId} order {orderId} get error with amount {price.Money} discountAmount {price.DiscountMoney} and realAmount {amount}");
        //        return false;
        //    }
        //    if (RechargeMng.AccumulateTotal < rechargeItem.AccumulateDiamond)
        //    {
        //        Log.Error($"player {Uid} recharge  rechargeId {rechargeId} order {orderId} get error with accumulateDiamond for {rechargeItem.AccumulateDiamond} and realaccumulateDiamond {RechargeMng.AccumulateTotal}");
        //        return false;
        //    }
        //    if (!GiftManager.CheckGiftItemHaveBuyCount(rechargeItem, rechargeUid))
        //    {
        //        Log.Error($"player {Uid} recharge  rechargeId {rechargeId} order {orderId} get error, buy count not enough");
        //        return false;
        //    }
        //    return true;
        //}
        
        public int GetRechargeItemRatio(RechargeItemModel rechargeItemModel)
        {
            if (rechargeItemModel.GiftType != RechargeGiftType.Common || rechargeItemModel.SubType != (int)CommonGiftType.Diamond)
            {
                return 1;
            }
            return GiftManager.GetRechargeItemRatio(rechargeItemModel.Id);
        }

        /// <summary>
        /// 充值折扣重置
        /// </summary>
        public void RefreshRechargeDiscount()
        {
            GiftManager.RefreshRechargeDiscount();
        }

        /// <summary>
        /// 魂师手札购买重置
        /// </summary>
        public void RefreshPassCardBuyState()
        {
            GiftManager.RefreshPassCardBuyState();
        }

        //记录行为触发礼包时间
        public ulong RecordActionTriggerGiftTime(int giftItemId, int actionId, bool isSdkGift, string dataBox)
        {
            return GiftManager.RecordActionTriggerGiftTime(giftItemId, actionId, isSdkGift, dataBox);
        }

        //该礼包当前购买次数
        public int GiftItemCurBuyCount(int id)
        {
            return GiftManager.GetGiftItemCurBuyCount(id);
        }

        //触发养成礼包(特卖场)
        public void CheckTriggerCultivateGift(TriggerGiftType triggerType, object param)
        {
            List<GiftItemModel> giftItems = GiftLibrary.GetGiftItemsByTriggerType((int)triggerType);
            if (giftItems == null)
            {
                return;
            }
            switch (triggerType)
            {
                case TriggerGiftType.MainTask:
                case TriggerGiftType.BranchTask:
                    int num = (int)param;         
                    GiftManager.TriggerCultivateGift(giftItems, num);
                    break;
                default:
                    break;
            }        
        }

        //购买养成礼包(特卖场)
        public void BuyCultivateGift(int giftId)
        {
            MSG_ZGC_BUY_CULTIVATE_GIFT response = new MSG_ZGC_BUY_CULTIVATE_GIFT();
            response.GiftId = giftId;

            GiftItemModel giftModel = GiftLibrary.GetGiftItemModel(giftId);
            if (giftModel == null)
            {
                response.Result = (int)ErrorCode.Fail;
                Log.Warn($"player {Uid} buy cultivate gift {giftId} failed: not find gift in xml");
                Write(response);
                return;
            }         

            int coniType = giftModel.Price[0].ToInt();
            int costCoin = giftModel.Price[2].ToInt();
            int coins = GetCoins((CurrenciesType)coniType);
            if (coins < costCoin)
            {
                response.Result = (int)ErrorCode.NoCoin;
                Log.Warn($"player {Uid} buy cultivate gift {giftId} failed: not have enough coins");
                Write(response);
                return;
            }

            //更新礼包信息
            GiftManager.UpdateCultivateGift(giftModel, response);

            if (response.Result != (int)ErrorCode.Success)
            {
                Write(response);
                return;
            }                     
            //扣钱
            DelCoins((CurrenciesType)coniType, costCoin, ConsumeWay.BuyCultivateGift, giftId.ToString());

            //发奖
            RewardManager manager = GetSimpleReward(giftModel.Rewards, ObtainWay.BuyCultivateGift, 1, giftId.ToString());
            manager.GenerateRewardItemInfo(response.Rewards);
            Write(response);

            if (giftModel.Type == 1)
            {
                List<GiftItemModel> giftModelList = GiftLibrary.GetGiftItemModelsByType(2);
                GiftManager.TriggerCultivateGift(giftModelList);
            }
        }

        #region 1元礼包
        public PettyGiftItem RefreshPettyGift(bool reachTime = false)
        {
            return GiftManager.RefreshPettyGift(reachTime);
        }

        //更新小额礼包
        public void UpdatePettyMoneyGift(RechargeItemModel recharge)
        {
            GiftManager.UpdatePettyMoneyGift(recharge);
        }

        //免费领取小额礼包
        public void ReceiveFreePettyGift(int giftId)
        {           
            GiftManager.ReceiveFreePettyGift(giftId);         
        }

        public void SendPettyGiftRefreshMsg()
        {
            PettyGiftItem refreshItem = RefreshPettyGift(true);
            if (refreshItem != null)
            {
                MSG_ZGC_PETTY_GIFT_REFRESH response = new MSG_ZGC_PETTY_GIFT_REFRESH();
                response.GiftId = refreshItem.Id;
                response.BuyState = refreshItem.BuyState;
                Write(response);
                GiftManager.SetRefreshPettyGift(refreshItem);
            }
        }
        #endregion

        #region 每日充值
        //更新每日充值信息
        public void UpdateDailyRecharge(RechargeItemModel recharge, string reward)
        {
            GiftManager.UpdateDailyRecharge(recharge);          
            SendBuyRechargeGiftInfo(recharge.Id, reward);
        }

        private void SendBuyRechargeGiftInfo(int id, string reward)
        {
            MSG_ZGC_RECHARGE_GIFT response = new MSG_ZGC_RECHARGE_GIFT();
            response.GiftItemId = id;
            response.BuyCount = 1;
            response.Result = (int)ErrorCode.Success;
            RewardManager rewards = new RewardManager();
            rewards.InitSimpleReward(reward);
            rewards.GenerateRewardItemInfo(response.Rewards);
            Write(response);
        }

        //领取每日充值累计天数奖励
        public void GetDailyRechargeReward(int rewardId)
        {
            MSG_ZGC_GET_DAILY_RECHARGE_REWARD response = new MSG_ZGC_GET_DAILY_RECHARGE_REWARD();
            response.Id = rewardId;

            DailyRechargeModel model = GiftLibrary.GetDailyRechargeModel(rewardId);
            if (model == null)
            {
                Log.Warn($"player {Uid} GetDailyRechargeReward {rewardId} failed: not find in xml");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
            RechargeGiftModel curDailyRecharge = GetCurRechargeGiftByType(RechargeGiftType.DailyRecharge);
            if (curDailyRecharge == null)
            {
                Log.Warn($"player {Uid} GetDailyRechargeReward {rewardId} failed: not find daily recharge in right time");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
            if (curDailyRecharge.SubType != model.Period)
            {
                Log.Warn($"player {Uid} GetDailyRechargeReward {rewardId} failed: not cur period reward");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
            DailyRechargeInfo info = GiftManager.GetDailyRechargeByPeriod(curDailyRecharge.SubType);          
            //没充过
            if (info == null)
            {
                Log.Warn($"player {Uid} GetDailyRechargeReward period {curDailyRecharge.SubType} reward {rewardId} failed: does not have daily recharge info");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
            //检查累充天数
            string[] ids = StringSplit.GetArray("|", info.Ids);
            if (ids.Length < model.Days)
            {
                Log.Warn($"player {Uid} GetDailyRechargeReward period {curDailyRecharge.SubType} reward {rewardId} failed: recharge accumulate days {ids.Length}");
                response.Result = (int)ErrorCode.NotReach;
                Write(response);
                return;
            }
            //是否已领取
            string[] getStates = StringSplit.GetArray("|", info.GetStates);
            if (getStates.Contains(model.Id.ToString()))
            {
                Log.Warn($"player {Uid} GetDailyRechargeReward period {curDailyRecharge.SubType} reward {rewardId} failed: reward already got");
                response.Result = (int)ErrorCode.AlreadyReceived;
                Write(response);
                return;
            }
            GiftManager.UpdateDailyRechargeGetState(info, model.Id);
            
            SyncDbUpdateDailyRechargeGetRewardInfo(info);

            getStates = StringSplit.GetArray("|", info.GetStates);
            getStates.ForEach(x => response.GetStateList.Add(x.ToInt()));

            //发奖
            string reward = string.Empty;
            if (!string.IsNullOrEmpty(model.Rewards))
            {
                reward = model.Rewards;
            }
            else if (!string.IsNullOrEmpty(model.FinalRewards))
            {
                reward = model.FinalRewards;
            }
            response.Result = (int)ErrorCode.Success;
            RewardManager manager = GetSimpleReward(reward, ObtainWay.GetDailyRechargeDaysReward);
            manager.GenerateRewardItemInfo(response.Rewards);
            Write(response);
        }

        private void SyncDbUpdateDailyRechargeGetRewardInfo(DailyRechargeInfo info)
        {
            server.GameDBPool.Call(new QueryUpdateDailyRechargeGetRewardInfo(Uid, info.Period, info.GetStates));
        }

        private RechargeGiftModel GetCurRechargeGiftByType(RechargeGiftType giftType)
        {
            RechargeGiftModel curRechargeGift = null;      
            RechargeLibrary.CheckInRechargeActivityTime(giftType, ZoneServerApi.now, out curRechargeGift);
            return curRechargeGift;
        }

        public void NotifyClientRefreshDailyRecharge()
        {
            MSG_ZGC_DAILY_RECHARGE_INFO msg = GiftManager.GenerateDailyRechargeMsg();
            Write(msg);
        }
        #endregion

        #region 角色七日奖
        //获得指定角色解锁七日奖励
        public void AddHeroDaysRewardsInfo(int heroId)
        {
            RechargeGiftModel curPeriodModel = GetCurRechargeGiftByType(RechargeGiftType.HeroDaysRewards);
            if (curPeriodModel == null)
            {
                return;
            }
            int periodHero = GiftLibrary.GetHeroIdByPeriod(curPeriodModel.SubType);
            if (heroId != periodHero)
            {
                return;
            }
            GiftManager.AddHeroDaysRewardsInfo(curPeriodModel.SubType);
        }

        //领取角色七日奖励
        public void GetHeroDaysReward(int rewardId)
        {
            MSG_ZGC_GET_HERO_DAYS_REWARD response = new MSG_ZGC_GET_HERO_DAYS_REWARD();
            response.Id = rewardId;

            HeroDaysRewardsModel model = GiftLibrary.GetHeroDaysReward(rewardId);
            if (model == null)
            {
                Log.Warn($"player {Uid} GetHeroDaysReward {rewardId} failed: not find in xml");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
            RechargeGiftModel curPeriodModel = GetCurRechargeGiftByType(RechargeGiftType.HeroDaysRewards);
            if (curPeriodModel == null)
            {
                Log.Warn($"player {Uid} GetHeroDaysReward {model.Id} failed: not find hero days rewards in right time");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
            if (model.Period != curPeriodModel.SubType)
            {
                Log.Warn($"player {Uid} GetHeroDaysReward {model.Id} failed: reward is not curPeriod {curPeriodModel.SubType}");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
            HeroDaysRewardsInfo info = GiftManager.GetHeroDaysRewardsInfoByPeriod(model.Period);
            if (info == null)
            {
                Log.Warn($"player {Uid} GetHeroDaysReward {model.Id} failed: not get hero yet");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
            int day = (ZoneServerApi.now.Date - info.HeroGetTime.Date).Days + 1;
            if (model.Day > day)
            {
                Log.Warn($"player {Uid} GetHeroDaysReward {model.Id} failed: not reach receive time, cur day {day}");
                response.Result = (int)ErrorCode.NotReachTime;
                Write(response);
                return;
            }
            //是否已领取
            string[] gotRewards = StringSplit.GetArray("|", info.Rewards);
            if (gotRewards.Contains(model.Id.ToString()))
            {
                Log.Warn($"player {Uid} GetHeroDaysReward {model.Id} failed: reward already got");
                response.Result = (int)ErrorCode.AlreadyReceived;
                Write(response);
                return;
            }
            GiftManager.UpdateHeroDaysRewardsGotRewardsInfo(info, model.Id);

            SyncDbUpdateHeroDaysRewardsGotRewardsInfo(info);

            gotRewards = StringSplit.GetArray("|", info.Rewards);
            gotRewards.ForEach(x => response.GetList.Add(x.ToInt()));

            //发奖              
            response.Result = (int)ErrorCode.Success;
            RewardManager manager = GetSimpleReward(model.Reward, ObtainWay.GetHeroDaysReward);
            manager.GenerateRewardItemInfo(response.Rewards);
            Write(response);
        }

        private void SyncDbUpdateHeroDaysRewardsGotRewardsInfo(HeroDaysRewardsInfo info)
        {
            server.GameDBPool.Call(new QueryUpdateHeroDaysRewardsGotRewardsInfo(Uid, info.Period, info.Rewards));
        }
        #endregion

        #region 新服促销
        //更新新服大促销信息
        public void UpdateNewServerPromotion(RechargeItemModel recharge, string reward)
        {
            GiftManager.UpdateNewServerPromotion(recharge);
            SendBuyRechargeGiftInfo(recharge.Id, reward);
        }

        public void NotifyClientRefreshNewServerPromotion()
        {
            MSG_ZGC_NEWSERVER_PROMOTION_INFO msg = GiftManager.GenerateNewServerPromotionMsg();
            Write(msg);
        }

        //领取新服大促销累计天数奖励
        public void GetNewServerPromotionReward(int rewardId)
        {
            MSG_ZGC_GET_NEWSERVER_PROMOTION_REWARD response = new MSG_ZGC_GET_NEWSERVER_PROMOTION_REWARD();
            response.Id = rewardId;

            NewServerPromotionModel model = GiftLibrary.GetNewServerPromotionModel(rewardId);
            if (model == null)
            {
                Log.Warn($"player {Uid} GetNewServerPromotionReward {rewardId} failed: not find in xml");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
            
            RechargeGiftModel curRechargeGift = GetCurRechargeGiftByOpenTimeType(RechargeGiftType.NewServerPromotion, RechargeOpenTimeType.OpenServerDay);
            if (curRechargeGift == null)
            {
                Log.Warn($"player {Uid} GetNewServerPromotionReward {rewardId} failed: not find new server promotion in right time");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            NewServerPromotionInfo info = GiftManager.GetNewServerPromotionByPeriod(curRechargeGift.SubType);
            //没充过
            if (info == null)
            {
                Log.Warn($"player {Uid} GetNewServerPromotionReward period {curRechargeGift.SubType} reward {rewardId} failed: does not have new server promotion info");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
            //检查累充天数
            string[] ids = StringSplit.GetArray("|", info.Ids);
            if (ids.Length < model.Days)
            {
                Log.Warn($"player {Uid} GetNewServerPromotionReward period {curRechargeGift.SubType} reward {rewardId} failed: recharge accumulate days {ids.Length}");
                response.Result = (int)ErrorCode.NotReach;
                Write(response);
                return;
            }
            //是否已领取
            string[] getStates = StringSplit.GetArray("|", info.GetStates);
            if (getStates.Contains(model.Id.ToString()))
            {
                Log.Warn($"player {Uid} GetNewServerPromotionReward period {curRechargeGift.SubType} reward {rewardId} failed: reward already got");
                response.Result = (int)ErrorCode.AlreadyReceived;
                Write(response);
                return;
            }
            GiftManager.UpdateNewServerPromotionGetState(info, model.Id);

            SyncDbUpdateNewServerPromotionGetRewardInfo(info);

            getStates = StringSplit.GetArray("|", info.GetStates);
            getStates.ForEach(x => response.GetStateList.Add(x.ToInt()));

            //发奖
            string reward = string.Empty;
            if (!string.IsNullOrEmpty(model.Rewards))
            {
                reward = model.Rewards;
            }
            else if (!string.IsNullOrEmpty(model.FinalRewards))
            {
                reward = model.FinalRewards;
            }
            response.Result = (int)ErrorCode.Success;
            RewardManager manager = GetSimpleReward(reward, ObtainWay.NewServerPromotionDaysReward);
            manager.GenerateRewardItemInfo(response.Rewards);
            Write(response);
        }

        private RechargeGiftModel GetCurRechargeGiftByOpenTimeType(RechargeGiftType giftType, RechargeOpenTimeType openTimeType)
        {
            RechargeGiftModel curRechargeGift = null;
            Dictionary<int, RechargeGiftModel> rechargeGiftDic = RechargeLibrary.GetRechargeGiftModelByGiftType(giftType);
            if (rechargeGiftDic == null)
            {
                return null;
            }
            foreach (var item in rechargeGiftDic)
            {
                if (CheckHasOpenedActivity(openTimeType, item.Value))
                {
                    return item.Value;
                }
               
            }
            return curRechargeGift;
        }

        private bool CheckHasOpenedActivity(RechargeOpenTimeType openTimeType, RechargeGiftModel gift)
        {
            switch (openTimeType)
            {
                case RechargeOpenTimeType.NormalTime:
                    if (gift.StartTime != DateTime.MinValue && ZoneServerApi.now >= gift.StartTime && gift.EndTime != DateTime.MinValue && ZoneServerApi.now <= gift.EndTime)
                    {
                        return true;
                    }
                    break;
                case RechargeOpenTimeType.OpenServerDay:
                    int serverDay = (int)(server.Now().Date - server.OpenServerDate).Days;
                    if (serverDay >= gift.ServerOpenDayStart && gift.ServerOpenDayEnd != 0 && serverDay < gift.ServerOpenDayEnd)
                    {
                        return true;
                    }
                    break;
                default:
                    break;
            }
            return false;
        }

        private void SyncDbUpdateNewServerPromotionGetRewardInfo(NewServerPromotionInfo info)
        {
            server.GameDBPool.Call(new QueryUpdateNewServerPromotionGetRewardInfo(Uid, info.Period, info.GetStates));
        }
        #endregion

        #region 幸运翻翻乐
        //购买幸运翻翻乐抽卡次数
        private void UpdateLuckyFlipCardInfo(RechargeItemModel recharge, string reward)
        {
            GiftManager.UpdateLuckyFlipCardInfo(recharge);
            SendBuyRechargeGiftInfo(recharge.Id, reward);
        }

        //幸运翻翻乐翻牌
        public void GetLuckyFlipCardReward(int rewardId, int index)
        {
            MSG_ZGC_GET_LUCKY_FLIP_CARD_REWARD response = new MSG_ZGC_GET_LUCKY_FLIP_CARD_REWARD();

            response.Id = rewardId;

            if (!RechargeLibrary.CheckInRechargeActivityTime(RechargeGiftType.LuckyFlipCard, ZoneServerApi.now))
            {
                Log.Warn($"player {Uid} GetLuckyFlipCardReward {rewardId} failed: time error");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            LuckyFlipCardRewardModel rewardModel = GiftLibrary.GetLuckyFlipCardRewardModel(rewardId);
            if (rewardModel == null)
            {
                Log.Warn($"player {Uid} GetLuckyFlipCardReward {rewardId} failed: not find in xml");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            LuckyFlipCardInfo info = GiftManager.GetLuckyFlipCardInfoByPeriod();
            if (info == null)
            {
                Log.Warn($"player {Uid} GetLuckyFlipCardReward {rewardId} failed: not find lucky flip card info");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
            if (info.Period != rewardModel.Period)
            {
                Log.Warn($"player {Uid} GetLuckyFlipCardReward {rewardId} failed: period error, real period {info.Period}");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
            //if (!rewardModel.RewardDic.ContainsKey(index))
            //{
            //    Log.Warn($"player {Uid} GetLuckyFlipCardReward {rewardId} failed: period error, index {index}");
            //    response.Result = (int)ErrorCode.Fail;
            //    Write(response);
            //    return;
            //}

            int curRchargeId = 0;
            RechargeItemModel rechargeItem = RechargeLibrary.GetSuitableGiftItem(RechargeGiftType.LuckyFlipCard, GiftManager.LuckFCPeriod, rewardModel.SubType);
            if (rechargeItem != null)
            {
                curRchargeId = rechargeItem.Id;
            }

            if (!info.RechargeIdList.Contains(curRchargeId))
            {
                Log.Warn($"player {Uid} GetLuckyFlipCardReward {rewardId} failed: not buy recharge {curRchargeId}");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            if (info.RandRewardList.Contains(rewardId) || info.RandRewardList.Count + 1 != rewardModel.SubType)
            {
                Log.Warn($"player {Uid} GetLuckyFlipCardReward {rewardId} failed: already got or count error {info.RandRewardList.Count}");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
           
            if (rewardModel.SubType == GiftLibrary.GetLuckFlipCardRewardMaxSubType(info.Period))
            {
                info.RechargeIdList.Clear();
                info.RandRewardList.Clear();
                info.Round++;
            }
            else
            {
                info.RandRewardList.Add(rewardId);
            }
            SyncDbUpdateLuckyFlipCardGetRewardInfo(info);
            GiftManager.SendLuckFlipCardInfo();


            //随机
            int rand = RAND.Range(1, rewardModel.TotalRatio);
            int cardId = rewardModel.GetCardByRatio(rand);
            string reward;
            rewardModel.RewardDic.TryGetValue(cardId, out reward);

            //固定
            //string reward;
            //rewardModel.RewardDic.TryGetValue(index, out reward);

            RewardManager manager = new RewardManager();
            List<ItemBasicInfo> rewardItems = new List<ItemBasicInfo>();

            if (!string.IsNullOrEmpty(reward))
            {
                RewardDropItemList rewardDrop = new RewardDropItemList(RewardDropType.Fixed, reward);
                List<ItemBasicInfo> items = RewardManagerEx.GetRewardBasicInfoList(rewardDrop, (int)Job);
                rewardItems.AddRange(items);
            }

            foreach (var rewardItem in rewardItems)
            {
                REWARD_ITEM_INFO rewardMsg = new REWARD_ITEM_INFO();
                rewardMsg.MainType = rewardItem.RewardType;
                rewardMsg.TypeId = rewardItem.Id;
                rewardMsg.Num = rewardItem.Num;
                if (rewardItem.Attrs != null)
                {
                    foreach (var attr in rewardItem.Attrs)
                    {
                        rewardMsg.Param.Add(attr);
                    }
                }
                response.Rewards.Add(rewardMsg);
            }

            manager.AddReward(rewardItems);
            manager.BreakupRewards(true);
            // 发放奖励
            AddRewards(manager, ObtainWay.LuckyFlipCardReward, rewardId.ToString());
            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        private void SyncDbUpdateLuckyFlipCardGetRewardInfo(LuckyFlipCardInfo info)
        {
            server.GameDBPool.Call(new QueryUpdatLuckyFlipCardGetRewardInfo(Uid, info.Period, info.RechargeIdList, info.RandRewardList, info.Round));
        }

        //领取幸运翻翻乐累计奖励
        public void GetLuckyFlipCardCumulateReward(int rewardId)
        {
            MSG_ZGC_GET_LUCKY_FLIP_CARD_CUMULATE_REWARD response = new MSG_ZGC_GET_LUCKY_FLIP_CARD_CUMULATE_REWARD();
            response.Id = rewardId;

            if (!RechargeLibrary.CheckInRechargeActivityTime(RechargeGiftType.LuckyFlipCard, ZoneServerApi.now))
            {
                Log.Warn($"player {Uid} GetLuckyFlipCardCumulateReward {rewardId} failed: time error");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            LuckyFlipCardCumulateRewardModel rewardModel = GiftLibrary.GetLuckyFlipCardCumulateRewardModel(rewardId);
            if (rewardModel == null)
            {
                Log.Warn($"player {Uid} GetLuckyFlipCardCumulateReward {rewardId} failed: not find in xml");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            LuckyFlipCardInfo info = GiftManager.GetLuckyFlipCardInfoByPeriod();
            if (info == null)
            {
                Log.Warn($"player {Uid} GetLuckyFlipCardCumulateReward {rewardId} failed: not find lucky flip card info");//
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
            if (info.Period != rewardModel.Period)
            {
                Log.Warn($"player {Uid} GetLuckyFlipCardCumulateReward {rewardId} failed: period error, real period {info.Period}");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            if (info.CumulateRewardList.Contains(rewardId))
            {
                Log.Warn($"player {Uid} GetLuckyFlipCardCumulateReward {rewardId} failed: already got");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            int flipCount = GiftLibrary.GetLuckFlipCardRewardMaxSubType(info.Period) * info.Round + info.RandRewardList.Count;
            if (flipCount < rewardModel.CumulateCount)
            {
                Log.Warn($"player {Uid} GetLuckyFlipCardCumulateReward {rewardId} failed: flipCount {flipCount} not enough");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            info.CumulateRewardList.Add(rewardId);
            SyncDbUpdateLuckyFlipCardCumulateRwardInfo(info);

            RewardManager manager = new RewardManager();
            List<ItemBasicInfo> rewardItems = new List<ItemBasicInfo>();

            if (!string.IsNullOrEmpty(rewardModel.Rewards))
            {
                RewardDropItemList rewardDrop = new RewardDropItemList(RewardDropType.Fixed, rewardModel.Rewards);
                List<ItemBasicInfo> items = RewardManagerEx.GetRewardBasicInfoList(rewardDrop, (int)Job);
                rewardItems.AddRange(items);
            }

            foreach (var rewardItem in rewardItems)
            {
                REWARD_ITEM_INFO rewardMsg = new REWARD_ITEM_INFO();
                rewardMsg.MainType = rewardItem.RewardType;
                rewardMsg.TypeId = rewardItem.Id;
                rewardMsg.Num = rewardItem.Num;
                if (rewardItem.Attrs != null)
                {
                    foreach (var attr in rewardItem.Attrs)
                    {
                        rewardMsg.Param.Add(attr);
                    }
                }
                response.Rewards.Add(rewardMsg);
            }

            manager.AddReward(rewardItems);
            manager.BreakupRewards(true);
            // 发放奖励
            AddRewards(manager, ObtainWay.LuckyFlipCardCumulateReward, rewardId.ToString());
            response.Result = (int)ErrorCode.Success;
            response.CumulateRewardList.AddRange(info.CumulateRewardList);
            Write(response);
        }

        private void SyncDbUpdateLuckyFlipCardCumulateRwardInfo(LuckyFlipCardInfo info)
        {
            server.GameDBPool.Call(new QueryUpdatLuckyFlipCardCumulateReward(Uid, info.Period, info.CumulateRewardList));
        }

        public void NotifyClientRefreshLuckyFlipCard()
        {
            GiftManager.SendLuckyFlipCardMsg();
        }

        //免费领取幸运翻翻乐奖励
        public void GetLuckyFlipCardRewardForFree(RechargeItemModel recharge)
        {
            if (!GiftManager.CheckLuckyFlipCardHaveBuyCount(recharge))
            {
                Log.Warn($"player {Uid} GetLuckyFlipCardRewardForFree productId {recharge.Id} error: can not get lucky flip card reward for free");
                return;
            }
            //获得奖励
            RewardManager rewards = new RewardManager();
            rewards.InitSimpleReward(recharge.Reward);
            AddRewards(rewards, ObtainWay.Recharge, recharge.Id.ToString());
            UpdateLuckyFlipCardInfo(recharge, recharge.Reward);
        }
        #endregion

        #region 夺宝翻翻乐
        //购买夺宝翻翻乐抽卡次数
        private void UpdateTreasureFlipCardInfo(RechargeItemModel recharge, string reward)
        {
            GiftManager.UpdateTreasureFlipCardInfo(recharge);
            SendBuyRechargeGiftInfo(recharge.Id, reward);
        }

        //夺宝翻翻乐翻牌
        public void GetTreasureFlipCardReward(int rewardId, int index)
        {
            MSG_ZGC_GET_TREASURE_FLIP_CARD_REWARD response = new MSG_ZGC_GET_TREASURE_FLIP_CARD_REWARD();

            response.Id = rewardId;

            if (!RechargeLibrary.CheckInRechargeActivityTime(RechargeGiftType.TreasureFlipCard, ZoneServerApi.now))
            {
                Log.Warn($"player {Uid} GetTreasureFlipCardReward {rewardId} failed: time error");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            TreasureFlipCardRewardModel rewardModel = GiftLibrary.GetTreasureFlipCardRewardModel(rewardId);
            if (rewardModel == null)
            {
                Log.Warn($"player {Uid} GetTreasureFlipCardReward {rewardId} failed: not find in xml");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            TreasureFlipCardInfo info = GiftManager.GetTreasureFlipCardInfoByPeriod();
            if (info == null)
            {
                Log.Warn($"player {Uid} GetTreasureFlipCardReward {rewardId} failed: not find treasure flip card info");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
            if (info.Period != rewardModel.Period)
            {
                Log.Warn($"player {Uid} GetTreasureFlipCardReward {rewardId} failed: period error, real period {info.Period}");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            if (!rewardModel.RewardDic.ContainsKey(index))
            {
                Log.Warn($"player {Uid} GetTreasureFlipCardReward {rewardId} failed: period error, index {index}");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            int curRchargeId = 0;
            RechargeItemModel rechargeItem = RechargeLibrary.GetSuitableGiftItem(RechargeGiftType.TreasureFlipCard, GiftManager.TreasureFCPeriod, rewardModel.SubType);
            if (rechargeItem != null)
            {
                curRchargeId = rechargeItem.Id;
            }

            if (!info.RechargeIdList.Contains(curRchargeId))
            {
                Log.Warn($"player {Uid} GetTreasureFlipCardReward {rewardId} failed: not buy recharge {curRchargeId}");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            if (info.RandRewardList.Contains(rewardId) || info.RandRewardList.Count + 1 != rewardModel.SubType)
            {
                Log.Warn($"player {Uid} GetTreasureFlipCardReward {rewardId} failed: already got or count error {info.RandRewardList.Count}");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
            //TODO
            if (info.FlipCardNum < 3 && info.FlipCardNum >= 0)
            {
                int round;
                int count = RAND.Range(0, 10000);
                if (info.Round < RechargeLibrary.TreasureFlipCardMaxRound)
                {
                    round = info.Round;
                }
                else
                {
                    round = RechargeLibrary.TreasureFlipCardMaxRound;
                }
                TreasureFlipCardFlipCardRatioModel flipCardRatio = GiftLibrary.GetTreasureFlipCardRatioModel(rewardModel.SubType, round);
                if (flipCardRatio.First != 0)
                {
                    if (count < flipCardRatio.First)
                    {
                        info.FlipCardNum += 1;
                        Log.Info($"player {Uid} GetFreeTreasureFlipCard CardNum {info.FlipCardNum}");
                    }
                }
                else
                {
                    if (count < flipCardRatio.Probability)
                    {
                        info.FlipCardNum += 1;
                        Log.Info($"player {Uid} GetFreeTreasureFlipCard CardNum {info.FlipCardNum}");
                    }
                }
            }

            if (rewardModel.SubType == GiftLibrary.GetTreasureFlipCardRewardMaxSubType(info.Period))
            {
                info.RechargeIdList.Clear();
                info.RandRewardList.Clear();
                info.Round++;
                
            }

            
            else
            {
                info.RandRewardList.Add(rewardId);
            }
           
            
            SyncDbUpdateTreasureFlipCardGetRewardInfo(info);
            GiftManager.SendTreasureFlipCardInfo();


            //随机
            //int rand = RAND.Range(1, rewardModel.TotalRatio);
            //int cardId = rewardModel.GetCardByRatio(rand);
            //string reward;
            //rewardModel.RewardDic.TryGetValue(cardId, out reward);

            //优化后固定获取
            string reward;
            rewardModel.RewardDic.TryGetValue(index, out reward);

            RewardManager manager = new RewardManager();
            List<ItemBasicInfo> rewardItems = new List<ItemBasicInfo>();

            if (!string.IsNullOrEmpty(reward))
            {
                RewardDropItemList rewardDrop = new RewardDropItemList(RewardDropType.Fixed, reward);
                List<ItemBasicInfo> items = RewardManagerEx.GetRewardBasicInfoList(rewardDrop, (int)Job);
                rewardItems.AddRange(items);
            }

            foreach (var rewardItem in rewardItems)
            {
                REWARD_ITEM_INFO rewardMsg = new REWARD_ITEM_INFO();
                rewardMsg.MainType = rewardItem.RewardType;
                rewardMsg.TypeId = rewardItem.Id;
                rewardMsg.Num = rewardItem.Num;
                if (rewardItem.Attrs != null)
                {
                    foreach (var attr in rewardItem.Attrs)
                    {
                        rewardMsg.Param.Add(attr);
                    }
                }
                response.Rewards.Add(rewardMsg);
            }

            manager.AddReward(rewardItems);
            manager.BreakupRewards(true);
            // 发放奖励
            AddRewards(manager, ObtainWay.TreasureFlipCardReward, rewardId.ToString());
            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        private void SyncDbUpdateTreasureFlipCardGetRewardInfo(TreasureFlipCardInfo info)
        {
            server.GameDBPool.Call(new QueryUpdateTreasureFlipCardGetRewardInfo(Uid, info.Period, info.RechargeIdList, info.RandRewardList, info.Round, info.FlipCardNum));
        }

        //领取夺宝翻翻乐累计奖励
        public void GetTreasureFlipCardCumulateReward(int rewardId)
        {
            MSG_ZGC_GET_TREASURE_FLIP_CARD_CUMULATE_REWARD response = new MSG_ZGC_GET_TREASURE_FLIP_CARD_CUMULATE_REWARD();
            response.Id = rewardId;

            if (!RechargeLibrary.CheckInRechargeActivityTime(RechargeGiftType.TreasureFlipCard, ZoneServerApi.now))
            {
                Log.Warn($"player {Uid} GetTreasureFlipCardCumulateReward {rewardId} failed: time error");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            TreasureFlipCardCumulateRewardModel rewardModel = GiftLibrary.GetTreasureFlipCardCumulateRewardModel(rewardId);
            if (rewardModel == null)
            {
                Log.Warn($"player {Uid} GetTreasureFlipCardCumulateReward {rewardId} failed: not find in xml");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            TreasureFlipCardInfo info = GiftManager.GetTreasureFlipCardInfoByPeriod();
            if (info == null)
            {
                Log.Warn($"player {Uid} GetTreasureFlipCardCumulateReward {rewardId} failed: not find treasure flip card info");//
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
            if (info.Period != rewardModel.Period)
            {
                Log.Warn($"player {Uid} GetTreasureFlipCardCumulateReward {rewardId} failed: period error, real period {info.Period}");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            if (info.CumulateRewardList.Contains(rewardId))
            {
                Log.Warn($"player {Uid} GetTreasureFlipCardCumulateReward {rewardId} failed: already got");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            int flipCount = GiftLibrary.GetTreasureFlipCardRewardMaxSubType(info.Period) * (info.Round - 1) + info.RandRewardList.Count;
            if (flipCount < rewardModel.CumulateCount)
            {
                Log.Warn($"player {Uid} GetTreasureFlipCardCumulateReward {rewardId} failed: flipCount {flipCount} not enough");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            info.CumulateRewardList.Add(rewardId);
            SyncDbUpdateTreasureFlipCardCumulateRwardInfo(info);

            RewardManager manager = new RewardManager();
            List<ItemBasicInfo> rewardItems = new List<ItemBasicInfo>();

            if (!string.IsNullOrEmpty(rewardModel.Rewards))
            {
                RewardDropItemList rewardDrop = new RewardDropItemList(RewardDropType.Fixed, rewardModel.Rewards);
                List<ItemBasicInfo> items = RewardManagerEx.GetRewardBasicInfoList(rewardDrop, (int)Job);
                rewardItems.AddRange(items);
            }

            foreach (var rewardItem in rewardItems)
            {
                REWARD_ITEM_INFO rewardMsg = new REWARD_ITEM_INFO();
                rewardMsg.MainType = rewardItem.RewardType;
                rewardMsg.TypeId = rewardItem.Id;
                rewardMsg.Num = rewardItem.Num;
                if (rewardItem.Attrs != null)
                {
                    foreach (var attr in rewardItem.Attrs)
                    {
                        rewardMsg.Param.Add(attr);
                    }
                }
                response.Rewards.Add(rewardMsg);
            }

            manager.AddReward(rewardItems);
            manager.BreakupRewards(true);
            // 发放奖励
            AddRewards(manager, ObtainWay.TreasureFlipCardCumulateReward, rewardId.ToString());
            response.Result = (int)ErrorCode.Success;
            response.CumulateRewardList.AddRange(info.CumulateRewardList);
            Write(response);
        }

        private void SyncDbUpdateTreasureFlipCardCumulateRwardInfo(TreasureFlipCardInfo info)
        {
            server.GameDBPool.Call(new QueryUpdateTreasureFlipCardCumulateReward(Uid, info.Period, info.CumulateRewardList));
        }

        public void NotifyClientRefreshTreasureFlipCard()
        {
            GiftManager.SendTreasureFlipCardMsg();
        }

        //免费领取夺宝翻翻乐奖励
        public void GetTreasureFlipCardRewardForFree(RechargeItemModel recharge)
        {
            if (!GiftManager.CheckTreasureFlipCardHaveBuyCount(recharge))
            {
                Log.Warn($"player {Uid} GetTreasureFlipCardRewardForFree productId {recharge.Id} error: can not get treasure flip card reward for free");
                return;
            }
            //获得奖励
            RewardManager rewards = new RewardManager();
            rewards.InitSimpleReward(recharge.Reward);
            AddRewards(rewards, ObtainWay.Recharge, recharge.Id.ToString());
            UpdateTreasureFlipCardInfo(recharge, recharge.Reward);
        }

        //使用翻翻卡领取夺宝翻翻乐奖励
        public void GetTreasureFlipCardRewardForFlipCard(RechargeItemModel recharge)
        {
            TreasureFlipCardInfo info = GiftManager.GetTreasureFlipCardInfoByPeriod();
            if (info == null)
            {
                Log.Warn($"player {Uid} GetTreasureFlipCardRewardFlipCard productId {recharge.Id} error: treasure flip card info is null");
                return;
            }
            if (info.FlipCardNum <= 0)
            {
                Log.Warn($"player {Uid} GetTreasureFlipCardRewardFlipCard productId {recharge.Id} error: flip card is null");
                return;
            }
            if (!GiftManager.CheckTreasureFlipCardHaveBuyCount(recharge))
            {
                Log.Warn($"player {Uid} GetTreasureFlipCardRewardFlipCard productId {recharge.Id} error: can not get treasure flip card reward for free");
                return;
            }
            info.FlipCardNum -= 1;
            Log.Info($"player {Uid} UseFreeTreasureFlipCard CardNum {info.FlipCardNum} , GetTreasureFlipCard use free card");
            SyncDbUpdateTreasureFlipCardGetRewardInfo(info);
            //获得奖励
            RewardManager rewards = new RewardManager();
            rewards.InitSimpleReward(recharge.Reward);
            AddRewards(rewards, ObtainWay.Recharge, recharge.Id.ToString());
            UpdateTreasureFlipCardInfo(recharge, recharge.Reward);
        }
        #endregion
        #region 登高礼包
        //海岛登高礼包刷新
        public void RefreshIslandHighGift()
        {
            List<int> subList;
            if (RechargeLibrary.CheckInSpecialRechargeActivityTime(RechargeGiftType.IslandHighGift, ZoneServerApi.now, out subList))
            {
                foreach (var subType in subList)
                {
                    GiftManager.ResetIslandHighGiftInfo(subType);
                }              
                GiftManager.SendIslandHighGiftInfo();
            }
        }

        //免费领取海岛登高礼包奖励
        public void GetIslandHighGiftRewardForFree(RechargeItemModel rechargeItem)
        {
            if (!GiftManager.CheckIslandHighGiftHaveBuyCount(rechargeItem))
            {
                Log.Warn($"player {Uid} GetIslandHighGiftRewardForFree productId {rechargeItem.Id} error: can not get island high gift reward for free");
                return;
            }
            //获得奖励
            RewardManager rewards = new RewardManager();
            rewards.InitSimpleReward(rechargeItem.Reward);
            AddRewards(rewards, ObtainWay.Recharge, rechargeItem.Id.ToString());
            UpdateIslandHighGiftInfo(rechargeItem, rechargeItem.Reward);
        }

        private void UpdateIslandHighGiftInfo(RechargeItemModel rechargeItem, string reward)
        {
            GiftManager.UpdateIslandHighGiftInfo(rechargeItem);
            SendBuyRechargeGiftInfo(rechargeItem.Id, reward);
        }
        #endregion

        //直购礼包
        public void UpdateDirectPurchaseInfo(RechargeItemModel recharge, string reward)
        {
            GiftManager.UpdateDirectPurchaseInfo(recharge);
            SendBuyRechargeGiftInfo(recharge.Id, reward);
        }

        #endregion

        public void UpdateRechargeActivityValue(RechargeGiftTimeType giftType)
        {
            switch (giftType)
            {               
                case RechargeGiftTimeType.IslandHighGiftStart:
                    GiftManager.ClearIslandHighGiftMemory(IslandHighGiftSubType.IslandHigh);
                    break;
                case RechargeGiftTimeType.Trident:
                case RechargeGiftTimeType.NewServerTridentEnd:
                    TridentManager.Clear();
                    break;;
                case RechargeGiftTimeType.DragonBoatStart:
                    DragonBoatManager.Clear();
                    break;
                case RechargeGiftTimeType.CarnivalRechargeStart:
                    ClearCarnivalRechargeInfo();
                    break;
                case RechargeGiftTimeType.CarnivalMallStart:
                    ClearCarnivalMallInfo();
                    break;
                case RechargeGiftTimeType.ShrekInvitaionStart:
                    ClearShrekInvitationInfo();
                    break;
                case RechargeGiftTimeType.CanoeGiftStart:
                    GiftManager.ClearIslandHighGiftMemory(IslandHighGiftSubType.Canoe);
                    break;
                case RechargeGiftTimeType.IslandGiftThreeStart:
                    GiftManager.ClearIslandHighGiftMemory(IslandHighGiftSubType.Three);
                    break;
                case RechargeGiftTimeType.MidAutumnStart:
                    ClearMidAutumnInfo();
                    break;
                case RechargeGiftTimeType.ThemeFireworkStart:
                    ClearThemeFireworkInfo();
                    break;
                case RechargeGiftTimeType.ActivityShopEnd:
                    ActivityShopResetRefreshFlag(CommonShopLibrary.EndActivityShop);
                    break;
                case RechargeGiftTimeType.NineTestStart:
                    ClearNineTestInfo();
                    break;
                case RechargeGiftTimeType.DiamondRebateStart:
                    ClearDiamondRebateInfo();
                    break;
                case RechargeGiftTimeType.XuanBoxStart:
                    XuanBoxManager.Clear();
                    break;
                case RechargeGiftTimeType.WishLanternStart:
                    WishLanternManager.Clear();
                    break;
                case RechargeGiftTimeType.NewRechargeGiftStart:
                    RechargeMng.NewRechargeGiftScore = 0;
                    RechargeMng.NewRechargeGiftRewards = string.Empty;
                    SendRechargeManger();
                    GiftManager.ClearTypeGift(RechargeGiftType.NewRechargeGift);

                    //MSG_ZGC_GIFT_INFO msg = GiftManager.GenerateGiftInfoMsg();
                    //Write(msg);
                    break;
                case RechargeGiftTimeType.DaysRechargeStart:
                    DaysRechargeManager.Clear();
                    break;
                case RechargeGiftTimeType.ShreklandStart:
                    ClearShreklandInfo();
                    break;
                case RechargeGiftTimeType.DevilTrainingStart:
                    SendDevilTrainingInfo();
                    break;
                case RechargeGiftTimeType.DomainBenedictionStart:
                    ClearDomainBenedictionInfo();
                    break;
                default:
                    break;
            }
        }

        private static int GIFT_TRANSFORM_LIMIT = 30;
        public List<MSG_ZMZ_GIFT_INFO_LIST> GenerateGiftInfoTransformMsg()
        {
            List<MSG_ZMZ_GIFT_INFO_LIST> msgList = new List<MSG_ZMZ_GIFT_INFO_LIST>();

            int num = 0;
            MSG_ZMZ_GIFT_INFO_LIST msg = new MSG_ZMZ_GIFT_INFO_LIST();
            foreach (var item in itemDic)
            {
                ++num;
                if (num >= GIFT_TRANSFORM_LIMIT)
                {
                    num = 0;
                    msg = new MSG_ZMZ_GIFT_INFO_LIST();
                    msgList.Add(msg);
                }

                msg.CodeList.Add(GenerateCodeItemMsg(item.Value));
            }
            msgList.Add(msg);

            //数据拆分
            GiftManager.GenerateGiftInfoListMsg(msgList);
            msg = new MSG_ZMZ_GIFT_INFO_LIST() { IsEnd = true };
            foreach (var item in codeUseCountDic)
            {
                msg.CodeUseList.Add(GenerateCodeUseMsg(item.Key, item.Value));
            }
            msgList.Add(msg);

            return msgList;
        }     

        private ZMZ_GIFT_CODE_ITEM GenerateCodeItemMsg(GiftCodeItem item)
        {
            ZMZ_GIFT_CODE_ITEM itemMsg = new ZMZ_GIFT_CODE_ITEM() { Id = item.Id, CodeMode = item.CodeMode, SubCode = item.SubCode};
            return itemMsg;
        }

        private ZMZ_GIFT_CODE_USE GenerateCodeUseMsg(int codeId, int useCount)
        {
            ZMZ_GIFT_CODE_USE useMsg = new ZMZ_GIFT_CODE_USE() { Id = codeId, UseCount = useCount};
            return useMsg;
        }

        public void LoadGiftInfoTransform(MSG_ZMZ_GIFT_INFO_LIST giftInfo)
        {
            List<GiftCodeItem> list = new List<GiftCodeItem>();
            foreach (var item in giftInfo.CodeList)
            {
                list.Add(CreateGiftCodeItem(item));
            }
            InitGiftCodeUseInfo(list);
            GiftManager.LoadGiftListInfoTransform(giftInfo.GiftList);
            foreach (var item in giftInfo.CodeUseList)
            {
                codeUseCountDic.Add(item.Id, item.UseCount);
            }
        }

        private GiftCodeItem CreateGiftCodeItem(ZMZ_GIFT_CODE_ITEM itemInfo)
        {
            GiftCodeItem item = new GiftCodeItem()
            {
                Id = itemInfo.Id,
                CodeMode = itemInfo.CodeMode,
                SubCode = itemInfo.SubCode
            };
            return item;
        }

        public MSG_ZMZ_CULTIVATE_GIFT_LIST GenerateCultivateGiftTransformMsg()
        {
            MSG_ZMZ_CULTIVATE_GIFT_LIST msg = GiftManager.GenerateCultivateGiftTransformMsg();
            return msg;
        }

        public void LoadCultivateGiftTransform(MSG_ZMZ_CULTIVATE_GIFT_LIST giftMsg)
        {
            GiftManager.LoadCultivateGiftTransform(giftMsg.GiftList);
        }

        public MSG_ZMZ_PETTY_GIFT GeneratePettyGiftTransformMsg()
        {
            MSG_ZMZ_PETTY_GIFT msg = GiftManager.GeneratePettyGiftTransformMsg();
            return msg;
        }

        public void LoadPettyGiftTransform(MSG_ZMZ_PETTY_GIFT giftMsg)
        {
            GiftManager.LoadPettyGiftTransform(giftMsg.ItemList);
        }

        public RepeatedField<ZMZ_DAILY_RECHARGE> GenerateDailyRechargeTransformMsg()
        {
            RepeatedField<ZMZ_DAILY_RECHARGE> msg = GiftManager.GenerateDailyRechargeTransformMsg();
            return msg;
        }

        public RepeatedField<ZMZ_HERO_DAYS_REWARDS> GenerateHeroDaysRewardsTransformMsg()
        {
            RepeatedField<ZMZ_HERO_DAYS_REWARDS> msg = GiftManager.GenerateHeroDaysRewardsTransformMsg();
            return msg;
        }

        public RepeatedField<ZMZ_DAILY_RECHARGE> GenerateNewServerPromotionTransformMsg()
        {
            RepeatedField<ZMZ_DAILY_RECHARGE> msg = GiftManager.GenerateNewServerPromotionTransformMsg();
            return msg;
        }

        public void LoadDaysRewardHeroTransform(MSG_ZMZ_DAYS_REWARD_HERO msg)
        {
            GiftManager.LoadDailyRechargeTransform(msg.DailyRecharge);
            GiftManager.LoadHeroDaysRewardsTransform(msg.HeroDaysRewards);
            GiftManager.LoadNewServerPromotionTransform(msg.NewServerPromotion);
        }

        public MSG_ZMZ_FLIP_CARD_INFO GenerateFlipCardTransformMsg()
        {
            MSG_ZMZ_FLIP_CARD_INFO msg = new MSG_ZMZ_FLIP_CARD_INFO();
            msg.LuckyInfo = GiftManager.GenerateLuckyFlipCardTransformMsg();
            return msg;
        }

        public void LoadFlipCardTransform(MSG_ZMZ_FLIP_CARD_INFO msg)
        {
            GiftManager.LoadLuckyFlipCardTransform(msg.LuckyInfo);
        }

        public MSG_ZMZ_ISLAND_HIGH_GIFT_INFO GenerateIslandHighGiftTransformMsg()
        {
            MSG_ZMZ_ISLAND_HIGH_GIFT_INFO msg = GiftManager.GenerateIslandHighGiftTransformMsg();                   
            return msg;
        }

        public void LoadIslandHighGiftTransform(MSG_ZMZ_ISLAND_HIGH_GIFT_INFO msg)
        {
            GiftManager.LoadIslandHighGiftTransform(msg);
        }

        public MSG_ZMZ_TREASURE_FLIP_CARD_INFO GenerateTreasureFlipCardTransformMsg()
        {
            MSG_ZMZ_TREASURE_FLIP_CARD_INFO msg = new MSG_ZMZ_TREASURE_FLIP_CARD_INFO();
            msg.TreasureInfo = GiftManager.GenerateTreasureFlipCardTransformMsg();
            return msg;
        }
        public void LoadTreasureFlipCardTransform(MSG_ZMZ_TREASURE_FLIP_CARD_INFO msg)
        {
            GiftManager.LoadTreasureFlipCardTransform(msg.TreasureInfo);
        }
    }
}
