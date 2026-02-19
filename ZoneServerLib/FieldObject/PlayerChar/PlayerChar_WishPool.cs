using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
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
    public partial class PlayerChar//TODO 跨zone
    {
        public int WishPoolDiamond;//计时器自动刷新
        public DateTime WishPoolUpdateTime;//上次领取时间
        public int WishPoolStage;
        public int WishPoolLevel;

        public void LoadWishPool(QueryLoadWishPoolInfo query)
        {
            WishPoolDiamond = query.diamond;
            WishPoolUpdateTime = Timestamp.TimeStampToDateTime(query.updateTime);
            WishPoolStage = query.stage;
            WishPoolLevel = query.level;
        }

        public void UpdateWishPoolInfo()
        {
            SendWishPoolInfo();
            UpdateWishPool2DB();
        }

        public void SendWishPoolInfo()
        {
            MSG_ZGC_WISHPOOL_INFO info = new MSG_ZGC_WISHPOOL_INFO();
            info.Stage = WishPoolStage;
            info.Level = WishPoolLevel+1;
            info.EndTime = Timestamp.GetUnixTimeStampSeconds(WishPoolLibrary.GetEndTime(server.OpenServerTime));

            DateTime refreshTime = WishPoolLibrary.GetTodayRefreshTime(server.Now());

            bool canUse = false;
            if(WishPoolUpdateTime< refreshTime)
            {
                if (server.Now() < refreshTime)
                {
                    if (WishPoolUpdateTime < refreshTime.AddDays(-1))
                    {
                        canUse = true;
                    }
                }
                else
                {
                    canUse = true;
                }
                
            }
            if (refreshTime <= server.Now())
            {
                refreshTime = refreshTime.AddDays(1);
            }
            info.RefreshTime = Timestamp.GetUnixTimeStampSeconds(refreshTime);

            if (WishPoolStage == 1)
            {
                WishPoolItem1 item = WishPoolLibrary.GetWishPoolItem1(WishPoolLevel + 1);
                info.Consume = item.ConsumeNum;
                info.ConsumeType = item.ConsumeItemId;
                info.ObtainType = item.ObtainItemId;
                info.ObtainHigh = item.ObtainHigh;
                info.ObtainLow = item.ObtainLow;
            }
            else
            {
                WishPoolItem2 item = WishPoolLibrary.GetWishPoolItem2();
                info.ConsumeType = (int)CurrenciesType.diamond;
                info.ObtainType = info.ConsumeType;
                if (WishPoolDiamond >= item.LimitDiamond)
                {
                    info.Consume = item.LimitDiamond;
                    info.ObtainHigh = info.Consume + item.OverLimitObtainHigh;
                    info.ObtainLow = info.Consume + item.OverLimitObtainLow;
                }
                else
                {
                    if (WishPoolDiamond < item.ObtainBase)
                    {
                        WishPoolDiamond = item.ObtainBase;
                        UpdateWishPool2DB();
                    }
                    info.Consume = WishPoolDiamond;
                    info.ObtainHigh = info.Consume + item.ObtainHigh;
                    info.ObtainLow = info.Consume + item.ObtainLow;
                }
                info.AlreadyUsed = !canUse;
                
            }

            Write(info);
        }

        public void UsingWishPool()
        {
            MSG_ZGC_USINIG_WISHPOOL ans = new MSG_ZGC_USINIG_WISHPOOL();
            ans.Result = (int)ErrorCode.Success;
            //先判断一遍更新
            if (CheckWishPoolStageUpdate())
            {
                UpdateWishPoolInfo();
            }

            //
            if (WishPoolStage == 1)
            {
                //判断钱
                WishPoolItem1 item=WishPoolLibrary.GetWishPoolItem1(WishPoolLevel + 1);

                int count = GetCoins((CurrenciesType)item.ConsumeItemId);
                if (count < item.ConsumeNum)
                {
                    Log.Warn($"player {Uid} use wish pool failed: coin not enough, curCoin {count} consume {item.ConsumeNum}");
                    ans.Result = (int)ErrorCode.NoCoin;
                    Write(ans);
                    return;
                }

                //第一阶段的领取
                DelCoins((CurrenciesType)item.ConsumeItemId, item.ConsumeNum, ConsumeWay.WishPool, WishPoolStage.ToString());

                int obtain = RAND.Range(item.ObtainLow, item.ObtainHigh);
                AddCoins((CurrenciesType)item.ObtainItemId, obtain, ObtainWay.WishPool);
                ans.Obtain = obtain;
                ans.ObtainType = item.ObtainItemId;

                //BI
                BIRecordWishingWellLog(WishPoolLevel, (CurrenciesType)item.ConsumeItemId, (CurrenciesType)item.ObtainItemId, item.ConsumeNum, obtain);
                //更新内容
                WishPoolLevel++;//档位增加
                //判断是否到顶
                CheckWishPoolStageUpdate();
                UpdateWishPoolInfo();
                //第一次许愿发称号卡
                TitleMng.UpdateTitleConditionCount(TitleObtainCondition.Wish);
            }
            else 
            {
                //第二阶段判断时间 和 钱
                //if (WishPoolUpdateTime >= WishPoolLibrary.GetTodayRefreshTime(server.Now()))
                //{
                //    ans.Result = (int)ErrorCode.WishPoolAlreadyUsed;
                //    Write(ans);
                //    return;
                //}

                DateTime refreshTime = WishPoolLibrary.GetTodayRefreshTime(server.Now());

                bool canUse = false;
                if (WishPoolUpdateTime < refreshTime)
                {
                    if (server.Now() < refreshTime)
                    {
                        if (WishPoolUpdateTime < refreshTime.AddDays(-1))
                        {
                            canUse = true;
                        }
                    }
                    else
                    {
                        canUse = true;
                    }

                }
                if (!canUse)
                {
                    Log.Warn($"player {Uid} use wish pool failed: wish pool already used");
                    ans.Result = (int)ErrorCode.WishPoolAlreadyUsed;
                    Write(ans);
                    return;
                }

                int count = GetCoins(CurrenciesType.diamond);
                WishPoolItem2 item = WishPoolLibrary.GetWishPoolItem2();
                if (count < WishPoolDiamond)
                {
                    Log.Warn($"player {Uid} use wish pool failed: curCoin {count} cost {WishPoolDiamond}");
                    ans.Result = (int)ErrorCode.NoCoin;
                    Write(ans);
                    return;
                }

                //第二阶段的操作
                if (WishPoolDiamond < item.LimitDiamond)
                {
                    int obtain = RAND.Range(item.ObtainLow, item.ObtainHigh);
                    AddCoins(CurrenciesType.diamond, obtain, ObtainWay.WishPool);
                    ans.Obtain = obtain + WishPoolDiamond;
                    ans.ObtainType = (int)CurrenciesType.diamond;
                }
                else
                {
                    int obtain = RAND.Range(item.OverLimitObtainLow, item.OverLimitObtainHigh);
                    AddCoins(CurrenciesType.diamond, obtain, ObtainWay.WishPool);
                    ans.Obtain = obtain + WishPoolDiamond;
                    ans.ObtainType = (int)CurrenciesType.diamond;
                }
                WishPoolUpdateTime = server.Now();
                //更新内容
                UpdateWishPoolInfo();
            }

            Write(ans);
        }

        public bool CheckWishPoolStageUpdate()
        {
            //判断更新stage
            if (WishPoolStage <= 1 && (WishPoolLevel >= WishPoolLibrary.GetMaxLevel4Stage1() || ZoneServerApi.now >= WishPoolLibrary.GetEndTime(server.OpenServerTime)))
            {
                WishPoolStage = 2;
                WishPoolDiamond = GetCoins(CurrenciesType.diamond);
                WishPoolItem2 item = WishPoolLibrary.GetWishPoolItem2();
                if (WishPoolDiamond >= item.LimitDiamond)
                {
                    WishPoolDiamond = item.LimitDiamond;
                }
                else
                {
                    WishPoolDiamond += item.ObtainBase;
                }
                return true;
            }
            return false;
        }

        public void UpdateWishPool2DB()
        {
            QueryUpdateWishPoolInfo update = new QueryUpdateWishPoolInfo(uid, WishPoolDiamond, Timestamp.GetUnixTimeStampSeconds(WishPoolUpdateTime), WishPoolStage, WishPoolLevel);
            server.GameDBPool.Call(update);
        }

        public void RefreshDailyWishPool()
        {
            if (!CheckWishPoolStageUpdate())
            {
                WishPoolDiamond = GetCoins(CurrenciesType.diamond);
                WishPoolItem2 item = WishPoolLibrary.GetWishPoolItem2();
                if (WishPoolDiamond >= item.LimitDiamond)
                {
                    WishPoolDiamond = item.LimitDiamond;
                }
                else
                {
                    WishPoolDiamond += item.ObtainBase;
                }
            }

            UpdateWishPoolInfo();
        }

        public bool CheckNextStage()
        {
            int maxLevel = WishPoolLibrary.GetMaxLevel4Stage1();
            if (WishPoolLevel >= maxLevel)
            {
                return true;
            }
            return false;
        }

        public ZMZ_WISH_POOL_INFO GetWishPoolTransform()
        {
            ZMZ_WISH_POOL_INFO msg = new ZMZ_WISH_POOL_INFO();
            msg.WishPoolDiamond = WishPoolDiamond;
            msg.WishPoolUpdateTime = Timestamp.GetUnixTimeStampSeconds(WishPoolUpdateTime);
            msg.WishPoolStage = WishPoolStage;
            msg.WishPoolLevel = WishPoolLevel;
            return msg;
        }

        public void LoadWishPoolTransform(ZMZ_WISH_POOL_INFO info)
        {
            WishPoolDiamond = info.WishPoolDiamond;
            WishPoolUpdateTime = Timestamp.TimeStampToDateTime(info.WishPoolUpdateTime);
            WishPoolStage = info.WishPoolStage;
            WishPoolLevel = info.WishPoolLevel;
        }
    }
}
