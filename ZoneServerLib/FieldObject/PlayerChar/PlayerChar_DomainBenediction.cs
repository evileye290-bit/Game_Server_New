/*************************************************
    文 件 : PlayerChar_DomainBenediction.cs
    日 期 : 2022年4月6日 17:49:46
    作 者 : jinzi
    策 划 : 
    说 明 : 神域赐福逻辑处理
*************************************************/

using System.Collections.Generic;
using CommonUtility;
using DBUtility.Sql;
using EnumerateUtility;
using EnumerateUtility.DomainBenediction;
using Google.Protobuf.Collections;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using ServerModels;
using ServerModels.DomainBenediction;
using ServerShared;
using ZoneServerLib.DomainBenediction;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        private DomainBenedictionManager oDomainBenedictionMng;

        public DomainBenedictionManager ODomainBenedictionMng
        {
            get => oDomainBenedictionMng;
            set => oDomainBenedictionMng = value;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void InitDomainBenediction()
        {
            oDomainBenedictionMng = new DomainBenedictionManager(this);
        }

        #region [db]

        /// <summary>
        /// db 插入
        /// </summary>
        /// <param name="oInfo"></param>
        private void dbSyncInsertDomainBenediction(DbDomainBenedictionInfo oInfo)
        {
            server.GameDBPool.Call(new QueryInsertDomainBenedictionInfo(Uid, oInfo.ICurrSuccNum, oInfo.ICurrFreeNum,
                oInfo.ICurrHalfNum, oInfo.ICurrIntegralNum, oInfo.LstGetAwardId.ToString("|")));
        }

        /// <summary>
        /// db 更新 使用类中的数据
        /// </summary>
        /// <param name="oInfo"></param>
        private void dbSyncUpdateDomainBenediction(DbDomainBenedictionInfo oInfo)
        {
            server.GameDBPool.Call(new QueryUpdateDomainBenedictionInfo(Uid, oInfo.ICurrSuccNum, oInfo.ICurrFreeNum,
                oInfo.ICurrHalfNum, oInfo.ICurrIntegralNum, oInfo.LstGetAwardId.ToString("|"), oInfo.StrBaseAward, oInfo.CurDrawType));
        }

        /// <summary>
        /// db 更新使用Mng中的数据
        /// </summary>
        private void dbSyncUpdateDomainBenediction()
        {
            server.GameDBPool.Call(new QueryUpdateDomainBenedictionInfo(Uid, oDomainBenedictionMng.ICurrSuccNum,
                oDomainBenedictionMng.ICurrFreeNum, oDomainBenedictionMng.ICurrHalfNum,
                oDomainBenedictionMng.ICurrIntegralNum,
                oDomainBenedictionMng.LstGetAwardId.ToString("|"), oDomainBenedictionMng.StrBaseAward, oDomainBenedictionMng.CurDrawType));
        }

        #endregion

        #region [客户端同步]

        /// <summary>
        /// 加载或更新客户端同步数据信息
        /// </summary>
        public void LoadOrUpdateInfo()
        {
            MSG_ZGC_DOMAIN_BENEDICTION_LOAD_AND_UPDATE oRes = new MSG_ZGC_DOMAIN_BENEDICTION_LOAD_AND_UPDATE
            {
                CurrSuccNum = ODomainBenedictionMng.ICurrSuccNum,
                CurrFreeNum = oDomainBenedictionMng.ICurrFreeNum,
                CurrHalfNum = oDomainBenedictionMng.ICurrHalfNum,
                CurrIntegralNum = oDomainBenedictionMng.ICurrIntegralNum,
                CurDrawType = ODomainBenedictionMng.CurDrawType,
                //true 可以祈愿 false 不可以祈愿，直接领取奖励
                IsCanDomain = !oDomainBenedictionMng.LastPrayFailed
            };
            oRes.GetAwardId.AddRange(oDomainBenedictionMng.LstGetAwardId);
            Write(oRes);
        }

        #endregion

        #region [私有函数]

        /// <summary>
        /// 单抽
        /// </summary>
        /// <param name="oTbInfo"></param>
        /// <returns></returns>
        private ErrorCode drawOnly(DomainBenedictionTypeModel oTbInfo, out RepeatedField<REWARD_ITEM_INFO> lstAwardInfo)
        {
            lstAwardInfo = new RepeatedField<REWARD_ITEM_INFO>();
            ErrorCode eErrorCode = ErrorCode.Fail;
            
            /*\ 随机奖励 /*/
            List<string> lstRandomAward =
                DomainBenedictionLibrary.RandomAwardInfo((EnumDomainBenedictionType) oTbInfo.iId);
            if (lstRandomAward == null)
            {
                return eErrorCode;
            }

            int iExpendNum = 0;
            bool bIsFree = oDomainBenedictionMng.ICurrFreeNum < DomainBenedictionLibrary.iFreeNumDay;
            bool bIsHalf = false;
            if (bIsFree)
            {
                oDomainBenedictionMng.ICurrFreeNum++;
            }
            else
            {
                if (oDomainBenedictionMng.ICurrHalfNum < DomainBenedictionLibrary.iHalfFareNum)
                {
                    bIsHalf = true;
                    int iDiscount = DomainBenedictionLibrary.RandomDiscount() / 1000;
                    iExpendNum = (int)(oTbInfo.iExpendNum * (iDiscount / 10f));
                }
                else
                {
                    iExpendNum = oTbInfo.iExpendNum;
                }

                /*\ 校验消耗 /*/
                if (!CheckCoins(CurrenciesType.diamond, iExpendNum))
                {
                    eErrorCode = ErrorCode.DiamondNotEnough;
                    return eErrorCode;
                }
            }

            /*\ 下发奖励 /*/
            RewardManager oAwardMng = new RewardManager();
            foreach (var strAward in lstRandomAward)
            {
                oAwardMng.AddSimpleReward(strAward);
            }

            oAwardMng.BreakupRewards();
            AddRewards(oAwardMng, ObtainWay.DomainBenedictionDrawAwardOnly, oTbInfo.iId.ToString());
            oAwardMng.GenerateRewardMsg(lstAwardInfo);

            /*\ 扣消耗 /*/
            DelCoins(CurrenciesType.diamond, iExpendNum, ConsumeWay.DomainBenedictionDrawExpend,
                oTbInfo.iId.ToString());

            if (bIsHalf)
                oDomainBenedictionMng.ICurrHalfNum++;

            /*\ 增加积分 /*/
            oDomainBenedictionMng.ICurrIntegralNum += oTbInfo.iIntegralNum;
            oDomainBenedictionMng.StrBaseAward = oTbInfo.strBaseAward;
            oDomainBenedictionMng.CurDrawType = (int)EnumDomainBenedictionType.domain_benediction_only;
            
            /*\ 入库 同步客户端 /*/
            dbSyncUpdateDomainBenediction();

            LoadOrUpdateInfo();

            eErrorCode = ErrorCode.Success;

            return eErrorCode;
        }

        /// <summary>
        /// 多抽
        /// </summary>
        /// <param name="oTbInfo"></param>
        /// <param name="lstAwardInfo"></param>
        /// <returns></returns>
        private ErrorCode drawMore(DomainBenedictionTypeModel oTbInfo, out RepeatedField<REWARD_ITEM_INFO> lstAwardInfo)
        {
            ErrorCode eErrorCode = ErrorCode.Fail;
            lstAwardInfo = new RepeatedField<REWARD_ITEM_INFO>();
            
            do
            {
                List<string> lstRandomAward = new List<string>();
                /*\ 校验消耗是否足够 /*/
                if (!CheckCoins(CurrenciesType.diamond, oTbInfo.iExpendNum))
                {
                    eErrorCode = ErrorCode.NotEnough;
                    return eErrorCode;
                }

                /*\ 随机奖励 /*/
                for (int i = 0; i < oTbInfo.iDrawNum; i++)
                {
                    var lstAward = DomainBenedictionLibrary.RandomAwardInfo((EnumDomainBenedictionType) oTbInfo.iId);
                    if (lstAward == null)
                    {
                        continue;
                    }

                    foreach (var strAward in lstAward)
                    {
                        lstRandomAward.Add(strAward);
                    }
                }

                /*\ 下发奖励 /*/
                RewardManager oAwardMng = new RewardManager();
                foreach (var strAward in lstRandomAward)
                {
                    oAwardMng.AddSimpleReward(strAward);
                }
                
                oAwardMng.BreakupRewards();
                AddRewards(oAwardMng, ObtainWay.DomainBenedictionDrawAwardMore, oTbInfo.iId.ToString());
                oAwardMng.GenerateRewardMsg(lstAwardInfo);

                /*\ 扣消耗 /*/
                DelCoins(CurrenciesType.diamond, oTbInfo.iExpendNum, ConsumeWay.DomainBenedictionDrawExpend,
                    oTbInfo.iId.ToString());
                
                /*\ 增加积分 /*/
                oDomainBenedictionMng.ICurrIntegralNum += oTbInfo.iIntegralNum;
                oDomainBenedictionMng.StrBaseAward = oTbInfo.strBaseAward;
                oDomainBenedictionMng.CurDrawType = (int)EnumDomainBenedictionType.domain_benediction_more;
                
                /*\ 存储并同步 /*/
                dbSyncUpdateDomainBenediction();
                
                LoadOrUpdateInfo();
                
                eErrorCode = ErrorCode.Success;

            } while (false);

            return eErrorCode;
        }

        /// <summary>
        /// 随机祈福是否成功
        /// </summary>
        /// <returns></returns>
        private bool randomParyIsSucc()
        {
            if (oDomainBenedictionMng.ICurrSuccNum >= DomainBenedictionLibrary.iMaxNum || oDomainBenedictionMng.LastPrayFailed)
            {
                return false;
            }
            
            int iRandom = DomainBenedictionLibrary.GetProbabilityWithNum(oDomainBenedictionMng.ICurrSuccNum);
            //祈福概率加成
            int probBonusA = DomainBenedictionLibrary.GetDomainBenedictionPrayProbBonus(1, RechargeManager.AccumulateTotal);
            int probBonusB = DomainBenedictionLibrary.GetDomainBenedictionPrayProbBonus(2, RechargeManager.AccumulateDaily);
            iRandom += probBonusA + probBonusB;
            /*\ 随机 /*/
            int iIndex = RAND.Range(0, 10000);
            if (iIndex <= iRandom)
            {
                return true;
            }

            oDomainBenedictionMng.ICurrSuccNum = 0;
            // oDomainBenedictionMng.CurDrawType = 0;
            oDomainBenedictionMng.LastPrayFailed = true;
 
            return false;
        }

        /// <summary>
        /// 公告
        /// </summary>
        private void checknNoticeHandle()
        {
            if (oDomainBenedictionMng.ICurrSuccNum >= DomainBenedictionLibrary.iNotictNum)
            {
                BroadCastDomainBenediction(oDomainBenedictionMng.ICurrSuccNum);
            }
        }

        private void checkSendBenedictionPrayTitle()
        {
            //第一次连续成功发称号卡
            if (oDomainBenedictionMng.ICurrSuccNum == DomainBenedictionLibrary.iTitleSuccNum)
            {
                TitleMng.UpdateTitleConditionCount(TitleObtainCondition.PrayContSuccess);
            }
        }
        #endregion

        #region [其他模块调用函数]

        /// <summary>
        /// 重置每日次数
        /// </summary>
        public void RefreshDomainBenedictionNum()
        {
            oDomainBenedictionMng.ClearEveryDay();
            LoadOrUpdateInfo();
        }

        #endregion
        
        #region [协议处理]

        /// <summary>
        /// 处理领取阶段奖励
        /// </summary>
        /// <param name="iTbId">表id</param>
        public void HandleGetStageAward(int iTbId)
        {
            MSG_ZGC_DOMAIN_BENEDICTION_GET_STAGE_AWARD oRes = new MSG_ZGC_DOMAIN_BENEDICTION_GET_STAGE_AWARD();
            ErrorCode eErrorCode = ErrorCode.Fail;
            do
            {
                /*\ 判断功能开启 /*/ 
                RechargeGiftModel activityModel;
                if (!RechargeLibrary.CheckInRechargeActivityShowTime(RechargeGiftType.DomainBenediction, ZoneServerApi.now, out activityModel))
                {
                    oRes.Result = (int)ErrorCode.RouletteNotOpen;
                    Log.Warn($"player {Uid} HandleGetStageAward failed: not open");
                    Write(oRes);
                    return;
                }
                
                /*\ 是否领取过奖励 /*/
                if (oDomainBenedictionMng.LstGetAwardId.Contains(iTbId))
                {
                    eErrorCode = ErrorCode.DomainBenedictionGetAwardAlready;
                    break;
                }

                /*\ 获取判断积分是否足够 /*/
                var oTbDomain = DomainBenedictionLibrary.GetStageAward(iTbId);
                if (oTbDomain == null)
                {
                    Log.WarnLine($"uid [{Uid}] get DomainBenedictionStageAward table id [{iTbId}] not found");
                    eErrorCode = ErrorCode.DomainBenedictionTableDataNotFound;
                    break;
                }

                if (oTbDomain.iIntegralNum > oDomainBenedictionMng.ICurrIntegralNum)
                {
                    Log.DebugLine(
                        $"uid [{Uid}] integral not enough, curr integral [{oDomainBenedictionMng.ICurrIntegralNum}]");
                    eErrorCode = ErrorCode.DomainBenedictionIntegralNotEnough;
                    break;
                }

                /*\ 领取奖励 /*/
                RewardManager oAwardMng = new RewardManager();
                oAwardMng.AddSimpleReward(oTbDomain.strAward);
                oAwardMng.BreakupRewards();
                AddRewards(oAwardMng, ObtainWay.DomainBenedictionStageAward, iTbId.ToString());
                oAwardMng.GenerateRewardMsg(oRes.Reward);

                /*\ 增加领取id /*/
                oDomainBenedictionMng.LstGetAwardId.Add(iTbId);

                /*\ 更新数据库 /*/
                dbSyncUpdateDomainBenediction();

                /*\ 同步客户端 /*/
                LoadOrUpdateInfo();

                eErrorCode = ErrorCode.Success;
            } while (false);

            /*\ 同步 /*/
            oRes.Result = (int) eErrorCode;
            Write(oRes);
        }

        /// <summary>
        /// 处理领取祈愿币奖励
        /// </summary>
        public void HandleGetBaseCurrencyAward()
        {
            MSG_ZGC_DOMAIN_BENEDICTION_GET_BASE_CURRENCY_AWARD oRes =
                new MSG_ZGC_DOMAIN_BENEDICTION_GET_BASE_CURRENCY_AWARD();
            
            oRes.Result = (int) ErrorCode.Fail;
            do
            {
                //TODO:Jinzi 功能开启
                RechargeGiftModel activityModel;
                if (!RechargeLibrary.CheckInRechargeActivityShowTime(RechargeGiftType.DomainBenediction, ZoneServerApi.now, out activityModel))
                {
                    oRes.Result = (int)ErrorCode.RouletteNotOpen;
                    Log.Warn($"player {Uid} HandleGetStageAward failed: not open");
                    Write(oRes);
                    return;
                }
                
                /*\ 判断是否有奖励可以领取 /*/
                if (string.IsNullOrEmpty(oDomainBenedictionMng.StrBaseAward))
                {
                    oRes.Result = (int) ErrorCode.DomainBenedictionIntegralNotGetBaseAward;
                    break;
                }

                string strAward = oDomainBenedictionMng.ClacBaseAward();
                if (string.IsNullOrEmpty(strAward))
                {
                    oRes.Result = (int) ErrorCode.DomainBenedictionIntegralNotGetBaseAward;
                    break;
                }

                RewardManager oAwardMng = new RewardManager();
                oAwardMng.AddSimpleReward(strAward);
                oAwardMng.BreakupRewards();
                AddRewards(oAwardMng, ObtainWay.DomainBenedictionBaseAward);

                oAwardMng.GenerateRewardMsg(oRes.Reward);

                /*\ 修改数据 同步 /*/
                ODomainBenedictionMng.StrBaseAward = string.Empty;
                oDomainBenedictionMng.ICurrSuccNum = 0;
                oDomainBenedictionMng.CurDrawType = 0;
                oDomainBenedictionMng.LastPrayFailed = false;

                dbSyncUpdateDomainBenediction();
                LoadOrUpdateInfo();
                
                oRes.Result = (int) ErrorCode.Success;
            } while (false);

            Write(oRes);
        }

        /// <summary>
        /// 进行祈福操作
        /// </summary>
        public void HandlePrayOperation()
        {
            MSG_ZGC_DOMAIN_BENEDICTION_PRAY_OPERATION oRes = new MSG_ZGC_DOMAIN_BENEDICTION_PRAY_OPERATION();
            oRes.Result = (int) ErrorCode.Fail;
            oRes.IsSucc = false;

            do
            {
                RechargeGiftModel activityModel;
                if (!RechargeLibrary.CheckInRechargeActivityShowTime(RechargeGiftType.DomainBenediction, ZoneServerApi.now, out activityModel))
                {
                    oRes.Result = (int)ErrorCode.RouletteNotOpen;
                    Log.Warn($"player {Uid} HandlePrayOperation failed: not open");
                    Write(oRes);
                    return;
                }
                /*\ 进行祈福操作 根据次数随机是否成功 /*/
                if (randomParyIsSucc())
                {
                    oRes.IsSucc = true;
                    oDomainBenedictionMng.ICurrSuccNum++;
                    checknNoticeHandle();
                    checkSendBenedictionPrayTitle();
                }

                dbSyncUpdateDomainBenediction();
                LoadOrUpdateInfo();
                
                oRes.Result = (int) ErrorCode.Success;
            } while (false);

            Write(oRes);
        }

        /// <summary>
        /// 进行抽取操作
        /// </summary>
        public void HandleDrawOperation(int iTbId)
        {
            MSG_ZGC_DOMAIN_BENEDICTION_DRAW_OPERATION oRes = new MSG_ZGC_DOMAIN_BENEDICTION_DRAW_OPERATION();
            oRes.Id = iTbId;
            oRes.Result = (int) ErrorCode.Fail;
            do
            {
                //校验功能开启
                if (!RechargeLibrary.CheckInRechargeActivityTime(RechargeGiftType.DomainBenediction, ZoneServerApi.now))
                {
                    oRes.Result = (int)ErrorCode.RouletteNotOpen;
                    Log.Warn($"player {Uid} HandleDrawOperation failed: activity not open");
                    Write(oRes);
                    return;
                }

                EnumDomainBenedictionType eType = (EnumDomainBenedictionType) iTbId;
                var oTbTypeInfo =
                    DomainBenedictionLibrary.GetDomainBenedictionTypeInfo(eType);
                if (oTbTypeInfo == null)
                {
                    Log.WarnLine($"client req HandleDrawOperation table id error, id [{iTbId}]");
                    oRes.Result = (int) ErrorCode.DomainBenedictionTableDataNotFound;
                    break;
                }

                RepeatedField<REWARD_ITEM_INFO> lstAawrdInfo = null;
                /*\ 校验消耗 /*/
                switch (eType)
                {
                    case EnumDomainBenedictionType.domain_benediction_only:
                    {
                        oRes.Result = (int) drawOnly(oTbTypeInfo, out lstAawrdInfo);
                        break;
                    }
                    case EnumDomainBenedictionType.domain_benediction_more:
                    {
                        oRes.Result = (int) drawMore(oTbTypeInfo, out lstAawrdInfo);
                        break;
                    }
                    default:
                    {
                        break;
                    }
                }

                if (lstAawrdInfo == null)
                {
                    break;
                }

                oRes.Reward.AddRange(lstAawrdInfo);
                
                if (oDomainBenedictionMng.LastPrayFailed)
                {
                    oDomainBenedictionMng.LastPrayFailed = false;
                }
                
                Write(oRes);
            } while (false);
        }

        public void ClearDomainBenedictionInfo()
        {
            ODomainBenedictionMng.ResetAll();
            LoadOrUpdateInfo();
        }
        
        /// <summary>
        /// 跨Zone
        /// </summary>
        /// <returns></returns>
        public MSG_ZMZ_DOMAIN_BENEDICTION_INFO GenerateDomainBenedictionTransformMsg()
        {
            MSG_ZMZ_DOMAIN_BENEDICTION_INFO msg = new MSG_ZMZ_DOMAIN_BENEDICTION_INFO();
            msg.ICurrSuccNum = ODomainBenedictionMng.ICurrSuccNum;
            msg.ICurrFreeNum = ODomainBenedictionMng.ICurrFreeNum;
            msg.ICurrHalfNum = ODomainBenedictionMng.ICurrHalfNum;
            msg.ICurrIntegralNum = ODomainBenedictionMng.ICurrIntegralNum;
            msg.LstGetAwardId.AddRange(ODomainBenedictionMng.LstGetAwardId);
            msg.StrBaseAward = ODomainBenedictionMng.StrBaseAward;
            msg.CurDrawType = ODomainBenedictionMng.CurDrawType;
            return msg;
        }

        public void LoadDomainBenedictionTransformMsg(MSG_ZMZ_DOMAIN_BENEDICTION_INFO msg)
        {
            ODomainBenedictionMng.ICurrSuccNum = msg.ICurrSuccNum;
            ODomainBenedictionMng.ICurrFreeNum = msg.ICurrFreeNum;
            ODomainBenedictionMng.ICurrHalfNum = msg.ICurrHalfNum;
            ODomainBenedictionMng.ICurrIntegralNum = msg.ICurrIntegralNum;
            ODomainBenedictionMng.LstGetAwardId = new List<int>();
            ODomainBenedictionMng.LstGetAwardId.AddRange(msg.LstGetAwardId);
            ODomainBenedictionMng.StrBaseAward = msg.StrBaseAward;
            ODomainBenedictionMng.CurDrawType = msg.CurDrawType;
        }
        #endregion
    }
}