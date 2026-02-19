using System;
using System.Collections.Generic;
using System.Text;
using CommonUtility;
using DataProperty;
using DBUtility;
using EnumerateUtility;
using EnumerateUtility.SpaceTimeTower;
using Google.Protobuf.Collections;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using ServerModels;
using ServerShared;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        public SpaceTimeTowerManager SpaceTimeTowerMng { get; private set; }

        public void InitSpaceTimeTower()
        {
            SpaceTimeTowerMng = new SpaceTimeTowerManager(this);
        }

        public void SendSpaceTimeTowerMsg()
        {
            SendSpaceTimeTowerInfo();
            SendSpaceTimeTowerOpenTime();
            SendSpaceTimeShopInfo();
            SendGuideSoulRestCountsInfo();
            SendSpaceTimeDungeonSettlement();
        }
        
        public void SendSpaceTimeTowerInfo()
        {
            MSG_ZGC_SPACE_TIME_TOWER_INFO msg = new MSG_ZGC_SPACE_TIME_TOWER_INFO();
            msg.TowerLevel = SpaceTimeTowerMng.TowerLevel;
            foreach (var kv in SpaceTimeTowerMng.HeroPool)
            {
                msg.HeroPool.Add(kv.Key, kv.Value);
            }
            foreach (var kv in SpaceTimeTowerMng.HeroTeam)
            {
                msg.HeroTeam.Add(kv.Key, GenerateSpaceTimeHeroInfo(kv.Value));
            }
            if (SpaceTimeTowerMng.TowerEvent != null)
            {
                msg.EventType = (int)SpaceTimeTowerMng.TowerEvent.EventType;
                msg.ParamList.AddRange(SpaceTimeTowerMng.TowerEvent.ParamList);
            }
            msg.GetAwardId.AddRange(SpaceTimeTowerMng.LstGetAwardId);
            msg.FailCount = SpaceTimeTowerMng.FailCount;
            msg.PassedTopLevel = SpaceTimeTowerMng.PassedTopLevel;
            msg.CurrRefreshNum = SpaceTimeTowerMng.ICurrRefreshNum;
            msg.Period = server.SpaceTimeTowerManager.Period;
            msg.Started = SpaceTimeTowerMng.Started;
            msg.PersonalPeriod = SpaceTimeTowerMng.PersonalPeriod;
            msg.PeriodPassedBefore = SpaceTimeTowerMng.PeriodPassedBefore;
            msg.PastRewardsState = SpaceTimeTowerMng.PastRewardsState;
            msg.StageRewardPeriod = SpaceTimeTowerMng.StageRewardPeriod;
            msg.Week = SpaceTimeTowerMng.Week;
            msg.HeroPoolWeek = SpaceTimeTowerMng.Week <= SpaceTimeTowerLibrary.MaxWeek ? SpaceTimeTowerMng.Week : SpaceTimeTowerLibrary.MaxWeek;
            Write(msg);
        }

        private ZGC_SPACE_TIME_HERO GenerateSpaceTimeHeroInfo(SpaceTimeHeroInfo heroInfo)
        {
            ZGC_SPACE_TIME_HERO msg = new ZGC_SPACE_TIME_HERO()
            {
                HeroId = heroInfo.Id,
                StepLevel = heroInfo.StepLevel,
                GodType = heroInfo.GodType,
                PositionNum = heroInfo.PositionNum,
                Nature = GenerateNineNatureMsg(heroInfo.Nature)
            };
            return msg;
        }

        /// <summary>
        /// 选择英雄加入队伍
        /// </summary>
        public void SpaceTimeHeroJoinTeam(int index)
        {
            MSG_ZGC_SPACE_TIME_JOIN_TEAM response = new MSG_ZGC_SPACE_TIME_JOIN_TEAM();
            response.Index = index;

            if (!SpaceTimeTowerMng.IsOpening())
            {
                Log.Warn($"player {Uid} space time hero join team failed: SpaceTimeTower not open");
                response.Result = (int)ErrorCode.NotOpen;
                Write(response);
                return;
            }

            if (SpaceTimeTowerMng.HeroTeam.Count >= SpaceTimeTowerLibrary.HeroMaxNum)
            {
                Log.Warn($"player {Uid} space time hero join team failed: hero team member max");
                response.Result = (int)ErrorCode.SpaceTimeTowerHeroTeamLimit;
                Write(response);
                return;
            }

            int heroId;
            if (!SpaceTimeTowerMng.HeroPool.TryGetValue(index, out heroId))
            {
                response.Result = (int)ErrorCode.Fail;
                Log.Warn($"player {Uid} SpaceTimeHeroJoinTeam failed: not find index {index} in hero pool");
                Write(response);
                return;
            }
            SpaceTimeHeroInfo heroInfo;
            if (SpaceTimeTowerMng.HeroTeam.TryGetValue(heroId, out heroInfo))
            {
                response.Result = (int)ErrorCode.Fail;
                Log.Warn($"player {Uid} SpaceTimeHeroJoinTeam failed: hero {heroId} already in team");
                Write(response);
                return;
            }
            SpaceTimeTowerMng.HeroJoinTeam(index, heroId);

            if (SpaceTimeTowerMng.HeroTeam.TryGetValue(heroId, out heroInfo))
            {
                response.Hero = GenerateSpaceTimeHeroInfo(heroInfo);
            }
            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        /// <summary>
        /// 英雄请离队伍
        /// </summary>
        public void SpaceTimeHeroQuitTeam(int heroId)
        {
            MSG_ZGC_SPACE_TIME_QUIT_TEAM response = new MSG_ZGC_SPACE_TIME_QUIT_TEAM();
            response.HeroId = heroId;

            SpaceTimeHeroInfo heroInfo;
            if (!SpaceTimeTowerMng.HeroTeam.TryGetValue(heroId, out heroInfo))
            {
                response.Result = (int)ErrorCode.Fail;
                Log.Warn($"player {Uid} SpaceTimeHeroQuitTeam failed: hero {heroId} not in hero team");
                Write(response);
                return;
            }
            SpaceTimeTowerMng.RemoveHeroFromTeam(heroId);
            SpaceTimeTowerMng.RemoveHeroFromQueue(heroInfo.Id);

            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        #region [私有函数]

        //TODO:Jinzi 切换阶段的时候调用该函数即可
        /// <summary>
        /// 重置领取过得阶段奖励id
        /// </summary>
        private void resetStageAwardId()
        {
            SpaceTimeTowerMng.LstGetAwardId.Clear();
        }
        
        /// <summary>
        /// 重置卡池刷新次数
        /// </summary>
        public void ResetCardPoolRefreshNum()
        {
            SpaceTimeTowerMng.ICurrRefreshNum = 0;
            /*\ 重新刷新卡池 /*/
            SpaceTimeRefreshCardPool(true);
        }
        
        /// <summary>
        /// 计算当前刷新卡池消耗
        /// </summary>
        /// <returns>消耗的钻石数量</returns>
        private int calcCurrRefreshCardPoolExpend()
        {
            int iExpendNum = -1;
            Data oData = DataListManager.inst.GetData("SpaceTimeTowerConfig", 1);
            if (oData == null)
            {
                return iExpendNum;
            }

            string strExpend = oData.GetString("RefreshCardPoolExpend");
            string[] strNum = strExpend.Split('|');
            foreach (var str in strNum)
            {
                string[] strGoldInfo = str.Split(':');
                if (strGoldInfo.Length >= 2)
                {
                    int iNum;
                    int iExpend;
                    int.TryParse(strGoldInfo[0], out iNum);
                    int.TryParse(strGoldInfo[1], out iExpend);
                    /*\ +1是因为默认是从0开始 表里配的是默认从1开始 /*/
                    if (SpaceTimeTowerMng.ICurrRefreshNum + 1 <= iNum)
                    {
                        iExpendNum = iExpend;
                        return iExpendNum;
                    }
                    else if (SpaceTimeTowerMng.ICurrRefreshNum >= iNum)
                    {
                        iExpendNum = iExpend;
                    }
                }
            }

            return iExpendNum;
        }
        
        /// <summary>
        /// 刷新卡池英雄
        /// </summary>
        /// <returns>返回heroId</returns>
        private int randomCardPoolHero()
        {
            int iHeroId = 0;
            /*\ 获取随机表 /*/
            var oXmlData = SpaceTimeTowerLibrary.GetTowerLevelModel(SpaceTimeTowerMng.TowerLevel);
            
            /*\ 随机品质 /*/
            int iGroupId = oXmlData.RandomHeroQuality();

            var lstRandomHero = SpaceTimeTowerLibrary.GetRandomHeroInfoWithGroupId(SpaceTimeTowerMng.Week, iGroupId);
            if (lstRandomHero == null)
            {
                return iHeroId;
            }

            /*\ 随机 /*/
            int iIndex = NewRAND.Next(0, lstRandomHero.Count - 1);

            if (lstRandomHero[iIndex] != null)
            {
                iHeroId = lstRandomHero[iIndex].iHeroId;
            }

            return iHeroId;
        }

        /// <summary>
        /// 刷新并添加卡池
        /// </summary>
        private void addRefreshCardPoolHero()
        {
            SpaceTimeTowerMng.HeroPool.Clear();
            int iMaxNum = SpaceTimeTowerLibrary.iRefreshHeroMaxNum;
            for (int iPos = 0; iPos < iMaxNum; iPos++)
            {
                int iHeroId = randomCardPoolHero();
                if (iHeroId == 0)
                {
                    continue;
                }
                
                if (!SpaceTimeTowerMng.HeroPool.ContainsKey(iPos))
                {
                    SpaceTimeTowerMng.HeroPool.Add(iPos, iHeroId);
                }
            }
        }

        #endregion

        #region [商城相关逻辑]

        /// <summary>
        /// 构建协议类型
        /// </summary>
        /// <returns></returns>
        public MSG_ZGC_SPACETIME_SHOP_INFO BuildInfo()
        {
            if (SpaceTimeTowerMng.OShopInfo == null)
            {
                return null;
            }
            
            MSG_ZGC_SPACETIME_SHOP_INFO oInfo = new MSG_ZGC_SPACETIME_SHOP_INFO
            {
                Type = (int) SpaceTimeTowerMng.OShopInfo.EShopType
            };
            
            foreach (var shopInfo in SpaceTimeTowerMng.OShopInfo.DicBuyNum)
            {
                if (!oInfo.ProductBuyNum.ContainsKey(shopInfo.Key))
                {
                    oInfo.ProductBuyNum.Add(shopInfo.Key, shopInfo.Value);
                }
            }

            oInfo.ProductId.AddRange(SpaceTimeTowerMng.OShopInfo.LstProductId);

            return oInfo;
        }
        
        /// <summary>
        /// 保存并同步客户端
        /// </summary>
        public void SaveAndUpdateShopInfo()
        {
            /*\ 更新db /*/
            SpaceTimeTowerMng.SyncDbUpdateSpaceTimeTower();

            SendSpaceTimeShopInfo();
        }

        /// <summary>
        /// 玩家登录同步
        /// </summary>
        public void SendSpaceTimeShopInfo()
        {
            var oSend = BuildInfo();
            if (oSend != null)
            {
                Write(oSend);
            }
        }

        /// <summary>
        /// 校验商城商品是否可以购买
        /// </summary>
        /// <param name="oProductInfo">商品信息</param>
        /// <returns></returns>
        public bool CheckShopBuyCondition(SpaceTimeTowerProduct oProductInfo)
        {
            bool bIsCan = false;
            switch (oProductInfo.eProductType)
            {
                case EnumSpaceTimeProductType.SpaceTimeUplevelStar:
                {//校验上阵阵容中是否全部达到最大
                    foreach (var hero in SpaceTimeTowerMng.HeroTeam)
                    {
                        if (hero.Value.StepLevel < SpaceTimeTowerLibrary.StepMaxLevel)
                        {
                            bIsCan = true;
                            break;
                        }
                    }
                    break;
                }
                case EnumSpaceTimeProductType.SpaceTimeUplevelGod:
                {//校验是否都达到了最高神
                    foreach (var hero in SpaceTimeTowerMng.HeroTeam)
                    {
                        var oTbHeroGod = GodHeroLibrary.GetHeroGodModel(hero.Key);
                        if (oTbHeroGod == null)
                        {
                            continue;
                        }

                        int iNextGod = 0;
                        if (!oTbHeroGod.CheckGodTypeIsMax(hero.Value.GodType, out iNextGod))
                        {
                            bIsCan = true;
                        }
                    }
                    break;
                }
                case EnumSpaceTimeProductType.SpaceTimeInherit:
                {//上阵部队小于两个不可以购买
                    if (SpaceTimeTowerMng.HeroTeam.Count >= 2)
                    {
                        bIsCan = true;
                    }
                    break;
                }
                case EnumSpaceTimeProductType.SpaceTimeRecycleHero:
                {
                    if (SpaceTimeTowerMng.HeroTeam.Count > 0)
                    {
                        bIsCan = true;
                    }
                    break;
                }
                case EnumSpaceTimeProductType.SpaceTimeDiscardHunDaoQi:
                {
                    if (SpaceTimeTowerMng.GuideSoulRestCounts.Count > 0)
                    {
                        bIsCan = true;
                    }
                    break;
                }
                default:
                {
                    bIsCan = true;
                    break;
                }
            }

            return bIsCan;
        }
        
        /// <summary>
        /// 效果-英雄升星
        /// </summary>
        /// <param name="oProductInfo">商品信息</param>
        /// <param name="arrReqParam">参数</param>
        private ErrorCode handleHeroUplevelStar(SpaceTimeTowerProduct oProductInfo, object[] arrReqParam)
        {
            ErrorCode eErrorCode = ErrorCode.Fail;
            do
            {
                if (oProductInfo.lstFuncParam.Count < 2)
                {
                    Log.ErrorLine($"SpaceTimeTowerProduct table data fail id [{oProductInfo.iId}] field FuncParam");
                    break;
                }

                /*\ 要升级的英雄id 大于该效果可以升级的最大英雄数量 /*/
                int iMaxHeroNum = 0;
                int iUplevelStar = 0;
                int.TryParse(oProductInfo.lstFuncParam[0], out iMaxHeroNum);
                int.TryParse(oProductInfo.lstFuncParam[1], out iUplevelStar);
                if (arrReqParam.Length > iMaxHeroNum)
                {
                    Log.WarnLine("client req handleHeroUplevelStar hero num > table hero num");
                    eErrorCode = ErrorCode.SpaceTimeTowerReqHeroExceedMax;
                    break;
                }

                /*\ 校验要升级的是否是部队当中 并且都未升级到最大等级 /*/
                bool bIsRet = false;
                List<SpaceTimeHeroInfo> lstHeroInfo = new List<SpaceTimeHeroInfo>();
                foreach (var heroId in arrReqParam)
                {
                    int iHeroId = Convert.ToInt32(heroId);
                    var oHeroInfo = SpaceTimeTowerMng.GetHeroInfo(iHeroId);
                    if (oHeroInfo == null)
                    {
                        bIsRet = true;
                        eErrorCode = ErrorCode.SpaceTimeTowerHeroTroopNotExist;
                        break;
                    }

                    if (oHeroInfo.StepLevel >= SpaceTimeTowerLibrary.StepMaxLevel)
                    {
                        bIsRet = true;
                        eErrorCode = ErrorCode.SpaceTimeTowerHeroStarIsMax;
                        break;
                    }
                    
                    lstHeroInfo.Add(oHeroInfo);
                }
                
                if (bIsRet)
                {
                    break;
                }
                
                /*\ 进行升级操作 /*/
                foreach (var oHero in lstHeroInfo)
                {
                    oHero.SetUplevelUp(iUplevelStar, SpaceTimeTowerLibrary.StepMaxLevel);
                    SpaceTimeTowerMng.BindHeroNature(oHero);
                }

                /*\ 同步英雄信息 /*/
                SyncClientSpaceTimeHeroChange(lstHeroInfo);

                /*\ 更新数据库 /*/
                SyncDbUpdateSpaceTimeHeros(lstHeroInfo);
                eErrorCode = ErrorCode.Success;
            } while (false);

            return eErrorCode;
        }

        /// <summary>
        /// 效果-神袛传承 星级和神袛交换
        /// </summary>
        /// <param name="oProductInfo"></param>
        /// <param name="arrReqParam"></param>
        /// <returns></returns>
        private ErrorCode handleGodInherit(SpaceTimeTowerProduct oProductInfo, object[] arrReqParam)
        {
            ErrorCode eErrorCode = ErrorCode.Fail;
            do
            {
                if (arrReqParam.Length != 2)
                {
                    Log.ErrorLine("client req handleGodInherit param error");
                    break;
                }

                /*\ 校验第一个英雄是否是神袛 /*/
                int iHeroId1 = (int) arrReqParam[0];
                int iHeroId2 = (int) arrReqParam[1];

                var oHeroInfo1 = SpaceTimeTowerMng.GetHeroInfo(iHeroId1);
                var oHeroInfo2 = SpaceTimeTowerMng.GetHeroInfo(iHeroId2);
                if (oHeroInfo1 == null || oHeroInfo2 == null)
                {
                    Log.WarnLine($"heroid [{iHeroId1}] or [{iHeroId2}] not in troop");
                    eErrorCode = ErrorCode.SpaceTimeTowerHeroTroopNotExist;
                    break;
                }

                /*\ 被传承者如果是神袛的话 /*/
                var oTbHeroInfo1 = HeroLibrary.GetHeroModel(iHeroId1);
                var oTbHeroInfo2 = HeroLibrary.GetHeroModel(iHeroId2);
                if (oTbHeroInfo1 == null || oTbHeroInfo2 == null)
                {
                    Log.WarnLine($"heroid [{iHeroId1}] or [{iHeroId2}] not in troop");
                    eErrorCode = ErrorCode.SpaceTimeTowerHeroTroopNotExist;
                    break;
                }

                /*\ 校验品质是否相同 /*/
                if (oTbHeroInfo1.Quality != oTbHeroInfo2.Quality)
                {
                    Log.WarnLine($"heroid [{iHeroId1}] and heroid [{iHeroId2}] quality different");
                    eErrorCode = ErrorCode.SpaceTimeTowerHeroQualityDifferent;
                    break;
                }

                /*\ 校验是否有神袛 /*/
                // var oTbHeroGod1 = GodHeroLibrary.GetHeroGodModel(iHeroId1);
                // var oTbHeroGod2 = GodHeroLibrary.GetHeroGodModel(iHeroId2);
                // if (oTbHeroGod1 != null && oTbHeroGod2 != null)
                // {
                //     /*\ 进行传承操作 /*/
                //     int iHeroIndex1 = oTbHeroGod1.IndexGodType(oHeroInfo1.GodType); 
                //     int iHeroIndex2 = oTbHeroGod1.IndexGodType(oHeroInfo2.GodType);
                //     // if (iHeroIndex1 <= iHeroIndex2 && oHeroInfo1.StepLevel <= oHeroInfo2.StepLevel)
                //     // {
                //     //     Log.WarnLine($"roleId [{Uid}] heroid1 [{iHeroId1}] godType < heroid2 [{iHeroId2}] godType or star");
                //     //     eErrorCode = ErrorCode.SpaceTimeTowerHeroOneLessHeroTwo;
                //     //     break;
                //     // }
                //
                //     /*\ 交换神袛 交换星级 /*/
                //     int iNowGod1 = oTbHeroGod2.GetGodType(iHeroIndex2);
                //     int iNowGod2 = oTbHeroGod1.GetGodType(iHeroIndex1);
                //
                //     oHeroInfo1.SetGodType(iNowGod1);
                //     oHeroInfo2.SetGodType(iNowGod2);
                // }

                /*\ 交换星级 /*/
                int iTempStar = oHeroInfo1.StepLevel;
                oHeroInfo1.SetStarLv(oHeroInfo2.StepLevel);
                oHeroInfo2.SetStarLv(iTempStar);

                List<SpaceTimeHeroInfo> lstHeroInfo = new List<SpaceTimeHeroInfo>()
                {
                    oHeroInfo1,
                    oHeroInfo2
                };

                SpaceTimeTowerMng.BindHeroNature(oHeroInfo1);
                SpaceTimeTowerMng.BindHeroNature(oHeroInfo2);

                /*\ 同步英雄信息 /*/
                SyncClientSpaceTimeHeroChange(lstHeroInfo);

                /*\ 更新数据库 /*/
                SyncDbUpdateSpaceTimeHeros(lstHeroInfo);
                
                eErrorCode = ErrorCode.Success;
            } while (false);

            return eErrorCode;
        }

        /// <summary>
        /// 效果-随机魂导器奖励
        /// </summary>
        /// <param name="oProductInfo"></param>
        /// <param name="arrReqParam"></param>
        /// <returns></returns>
        private ErrorCode handleHunDaoqi(SpaceTimeTowerProduct oProductInfo, object[] arrReqParam)
        {
            ErrorCode eErrorCode = ErrorCode.Fail;

            do
            {
                List<string> lstRandom = new List<string>(oProductInfo.lstFuncParam);
                int iHunDaoqiId = 0;
                while (lstRandom.Count > 0)
                {
                    int iIndex = RAND.Range(0, lstRandom.Count - 2);
                    if (iIndex >= lstRandom.Count)
                    {
                        return eErrorCode;
                    }

                    int iId = 0;
                    int.TryParse(lstRandom[iIndex], out iId);
                    if (SpaceTimeTowerMng.GuideSoulRestCounts.ContainsKey(iId))
                    {
                        lstRandom.Remove(lstRandom[iIndex]);
                    }
                    else
                    {
                        iHunDaoqiId = iId;
                        break;
                    }
                }

                if (iHunDaoqiId == 0)
                {
                    eErrorCode = ErrorCode.SpaceTimeTowerHundaoqiGetMax;
                    return eErrorCode;
                }
                
                /*\ 构建通用奖励信息 /*/
                string strAward = iHunDaoqiId + ":" + (int)RewardType.GuideSoulItem + ":" + 1;
                
                /*\ 下发奖励 /*/
                RewardManager oRewardInfo = new RewardManager();
                oRewardInfo.AddSimpleReward(strAward);
                oRewardInfo.BreakupRewards();
                AddRewards(oRewardInfo, ObtainWay.SpaceTimeShopBuy);
                
                oRewardInfo.GenerateRewardItemInfo(SpaceTimeTowerMng.lstAwardInfo);
                
                eErrorCode = ErrorCode.Success;
            } while (false);

            return eErrorCode;
        }

        /// <summary>
        /// 效果-丢弃魂导器
        /// </summary>
        /// <param name="oProductInfo"></param>
        /// <param name="arrReqParam"></param>
        /// <returns></returns>
        private ErrorCode handleDiscardHunDaoQi(SpaceTimeTowerProduct oProductInfo, object[] arrReqParam)
        {
            ErrorCode eErrorCode = ErrorCode.Fail;
            
            do
            {
                if (oProductInfo.lstFuncParam.Count < 1)
                {
                    Log.ErrorLine($"SpaceTimeTowerProduct table id [{oProductInfo.iId}] fail");
                    break;
                }

                if (arrReqParam.Length < 1)
                {
                    Log.WarnLine("client req handleDiscardHunDaoQi param fail");
                    eErrorCode = ErrorCode.SpaceTimeTowerHeroTroopNotExist;
                    break;
                }

                int iMaxNum = 0;
                int.TryParse(oProductInfo.lstFuncParam[0], out iMaxNum);
                if (arrReqParam.Length > iMaxNum)
                {
                    Log.WarnLine("client req num > server handle num");
                    eErrorCode = ErrorCode.SpaceTimeTowerClientReqParamFail;
                    break;
                }

                /*\ 处理删除魂导器 /*/
                /*\ 同步魂导器 /*/
                SpaceTimeTowerMng.DelGuideSoulItem((int)arrReqParam[0]);

                eErrorCode = ErrorCode.Success;
            } while (false);

            return eErrorCode;
        }

        /// <summary>
        /// 效果-神袛升阶
        /// </summary>
        /// <param name="oProductInfo"></param>
        /// <param name="arrReqParam"></param>
        /// <returns></returns>
        private ErrorCode handleUplevelGod(SpaceTimeTowerProduct oProductInfo, object[] arrReqParam)
        {
            ErrorCode eErrorCode = ErrorCode.Fail;

            do
            {
                if (oProductInfo.lstFuncParam.Count < 1)
                {
                    Log.ErrorLine($"SpaceTimeTowerProduct table id [{oProductInfo.iId}] fail");
                    break;
                }

                if (arrReqParam.Length < 1)
                {
                    Log.WarnLine("client req handleUplevelGod param fail");
                    eErrorCode = ErrorCode.SpaceTimeTowerHeroTroopNotExist;
                    break;
                }

                int iMaxNum = 0;
                int.TryParse(oProductInfo.lstFuncParam[0], out iMaxNum);
                if (arrReqParam.Length > iMaxNum)
                {
                    Log.WarnLine("client req num > server handle num");
                    eErrorCode = ErrorCode.SpaceTimeTowerClientReqParamFail;
                    break;
                }

                Dictionary<SpaceTimeHeroInfo, int> dicInfo = new Dictionary<SpaceTimeHeroInfo, int>();

                /*\ 判断英雄是否达到了最大神阶 /*/
                bool bIsOk = true;
                foreach (var heroId in arrReqParam)
                {
                    int iHeroId = (int) heroId;
                    var oHeroTroop = SpaceTimeTowerMng.GetHeroInfo(iHeroId);
                    if (oHeroTroop == null)
                    {
                        bIsOk = false;
                        eErrorCode = ErrorCode.SpaceTimeTowerHeroTroopNotExist;
                        break;
                    }
                    
                    var oHeroGod = GodHeroLibrary.GetHeroGodModel(iHeroId);
                    if (oHeroGod == null)
                    {
                        bIsOk = false;
                        eErrorCode = ErrorCode.SpaceTimeTowerHeroTroopNotExist;
                        break;
                    }

                    /*\ 校验神袛是否达到最大 /*/
                    int iNextGodType = 0;
                    if (oHeroGod.CheckGodTypeIsMax(oHeroTroop.GodType, out iNextGodType) ||
                        iNextGodType == 0)
                    {
                        eErrorCode = ErrorCode.SpaceTimeTowerHeroGodMax;
                        bIsOk = false;
                        break;
                    }

                    if (!dicInfo.ContainsKey(oHeroTroop))
                    {
                        dicInfo.Add(oHeroTroop, iNextGodType);
                    }
                }

                if (!bIsOk)
                {
                    break;
                }
                
                /*\ 处理神袛升级 /*/
                foreach (var hero in dicInfo)
                {
                    hero.Key.SetGodType(hero.Value);
                    SpaceTimeTowerMng.BindHeroNature(hero.Key);
                }

                List<SpaceTimeHeroInfo> lstUpdateHero = new List<SpaceTimeHeroInfo>(dicInfo.Keys);
                /*\ 同步英雄信息 /*/
                SyncClientSpaceTimeHeroChange(lstUpdateHero);

                /*\ 更新数据库 /*/
                SyncDbUpdateSpaceTimeHeros(lstUpdateHero);

                eErrorCode = ErrorCode.Success;

            } while (false);
            
            return eErrorCode;
        }

        /// <summary>
        /// 效果-直接下发奖励
        /// </summary>
        /// <param name="oProductInfo"></param>
        /// <param name="arrReqParam"></param>
        /// <returns></returns>
        private ErrorCode handleDirectSendAward(SpaceTimeTowerProduct oProductInfo, object[] arrReqParam)
        {
            ErrorCode eErrorCode = ErrorCode.Fail;

            do
            {
                /*\ 下发奖励 /*/
                RewardManager oRewardInfo = new RewardManager();
                oRewardInfo.AddSimpleReward(oProductInfo.strAward);

                AddRewards(oRewardInfo, ObtainWay.SpaceTimeShopBuy);

                oRewardInfo.GenerateRewardItemInfo(SpaceTimeTowerMng.lstAwardInfo);
                
                eErrorCode = ErrorCode.Success;
            } while (false);

            return eErrorCode;
        }

        /// <summary>
        /// 回收魂师
        /// </summary>
        /// <param name="oProductInfo"></param>
        /// <param name="arrReqParam"></param>
        /// <returns></returns>
        private ErrorCode handleRecycleHero(SpaceTimeTowerProduct oProductInfo, object[] arrReqParam)
        { 
            ErrorCode eErrorCode = ErrorCode.Fail;
            int countLimit = 0;
            if (oProductInfo.lstFuncParam.Count == 1)
            {
                int.TryParse(oProductInfo.lstFuncParam[0], out countLimit);
            }
            
            if ((countLimit > 0 && arrReqParam.Length > countLimit) || arrReqParam.Length > SpaceTimeTowerMng.HeroTeam.Count)
            {
                Log.Warn($"player {Uid} handleRecycleHero failed: arrReqParam length error");
                return eErrorCode;
            }
            
            RewardManager manager = new RewardManager();
            StringBuilder rewards = new StringBuilder();
            foreach (var param in arrReqParam)
            {
                int heroId = (int) param;
                SpaceTimeHeroInfo heroInfo;
                if (!SpaceTimeTowerMng.HeroTeam.TryGetValue(heroId, out heroInfo))
                {
                    Log.Warn($"player {Uid} handleRecycleHero failed: hero {heroId} not in hero team");
                    continue;
                }
                SpaceTimeRecycleHeroRewards rewardModel  = SpaceTimeTowerLibrary.GetRecycleHeroRewards(heroInfo.StepLevel);
                if (rewardModel == null)
                {
                    Log.Warn($"player {Uid} handleRecycleHero failed: not find stepLevel {heroInfo.StepLevel} rewards");
                    continue;
                }
                SpaceTimeTowerMng.RemoveHeroFromTeam(heroId);
                SpaceTimeTowerMng.RemoveHeroFromQueue(heroId);
                
                int quality = heroInfo.GetData().GetInt("Quality");
                string reward = rewardModel.GetRecycleHeroRewards(quality);
                if (!string.IsNullOrEmpty(reward))
                {
                    rewards.Append(reward);
                    eErrorCode = ErrorCode.Success;
                }
                else
                {
                    Log.Warn($"player {Uid} handleRecycleHero failed: not find hero {heroId} stepLevel {heroInfo.StepLevel} rewards");
                }
            }
            manager.InitSimpleReward(rewards.ToString(), true);
            AddRewards(manager, ObtainWay.SpaceTimeShopBuy);
            manager.GenerateRewardItemInfo(SpaceTimeTowerMng.lstAwardInfo);

            return eErrorCode;
        }
        
        /// <summary>
        /// 商品操作
        /// </summary>
        /// <param name="iProductId"></param>
        public ErrorCode OptProduct(int iProductId, object[] arrReqParam)
        {
            ErrorCode eErrorCode = ErrorCode.Fail;
            if (arrReqParam == null)
            {
                Log.WarnLine("Function OptProduct arrReqParam is null");
                return eErrorCode;
            }

            if (SpaceTimeTowerMng.OShopInfo == null)
            {
                Log.WarnLine("Space Time Tower Shop Is Null");
                return eErrorCode;
            }
            
            var oProductInfo = SpaceTimeTowerLibrary.GetProductInfo(iProductId);
            if (oProductInfo == null)
            {
                return eErrorCode;
            }

            /*\ 商城的时候才去校验货币 /*/
            if (SpaceTimeTowerMng.OShopInfo.EShopType == SpaceTimeEventType.Shop)
            {
                /*\ 校验是否可以购买 /*/
                if (!CheckShopBuyCondition(oProductInfo))
                {
                    eErrorCode = ErrorCode.SpaceTimeTowerBuyConditionNot;
                    return eErrorCode;
                }
                
                /*\ 校验消耗是否足够 /*/
                if (!CheckCoins(CurrenciesType.spaceTimeCoin, oProductInfo.iExpend))
                {
                    eErrorCode = ErrorCode.NotEnough;
                    return eErrorCode;
                }
            }
            
            int iMaxUseNum = 0;
            /*\ 校验是否有购买次数 /*/
            if (SpaceTimeTowerMng.OShopInfo.EShopType == SpaceTimeEventType.Shop)
            {
                iMaxUseNum = oProductInfo.iShopBuyNum;
            }
            else if (SpaceTimeTowerMng.OShopInfo.EShopType == SpaceTimeEventType.House)
            {
                iMaxUseNum = oProductInfo.iHouseMaxNum;
            }
            
            int iCurrBuyNum = 0;
            if (SpaceTimeTowerMng.OShopInfo.DicBuyNum.TryGetValue(iProductId, out iCurrBuyNum))
            {
                if (iCurrBuyNum >= iMaxUseNum)
                {
                    eErrorCode = ErrorCode.SpaceTimeTowerShopNotBuyNum;
                    return eErrorCode;
                }
            }

            //TODO:Jinzi 感觉设计上不太合理 我没用使用物品 但是扣了货币
            if (arrReqParam.Length > 0 ||
                oProductInfo.eProductType == EnumSpaceTimeProductType.SpaceTimeHunDaoQi ||
                oProductInfo.eProductType == EnumSpaceTimeProductType.SpaceTimeMoreCurrencyAndHunDaoQi)
            {
                switch (oProductInfo.eProductType)
                {
                    case EnumSpaceTimeProductType.SpaceTimeUplevelStar: 
                    {//英雄升星
                        eErrorCode = handleHeroUplevelStar(oProductInfo, arrReqParam);
                        break;
                    }
                    case EnumSpaceTimeProductType.SpaceTimeInherit:
                    {//神袛传承
                        eErrorCode = handleGodInherit(oProductInfo, arrReqParam);
                        break;
                    }
                    case EnumSpaceTimeProductType.SpaceTimeHunDaoQi:
                    {//魂导器
                        eErrorCode = handleHunDaoqi(oProductInfo, arrReqParam);
                        break;
                    }
                    case EnumSpaceTimeProductType.SpaceTimeMoreCurrencyAndHunDaoQi:
                    {//直接下发奖励
                        eErrorCode = handleDirectSendAward(oProductInfo, arrReqParam);
                        break;
                    }
                    case EnumSpaceTimeProductType.SpaceTimeDiscardHunDaoQi:
                    {//丢弃一件魂导器
                        eErrorCode = handleDiscardHunDaoQi(oProductInfo, arrReqParam);
                        break;
                    }
                    case EnumSpaceTimeProductType.SpaceTimeUplevelGod:
                    {//提升神阶
                        eErrorCode = handleUplevelGod(oProductInfo, arrReqParam);
                        break;
                    }
                    case EnumSpaceTimeProductType.SpaceTimeRecycleHero:
                    {//回收魂师
                        eErrorCode = handleRecycleHero(oProductInfo, arrReqParam);
                        break;
                    }
                }
            }
            else
            {
                eErrorCode = ErrorCode.Success;
            }

            if (eErrorCode == ErrorCode.Success)
            {
                /*\ 增加次数 /*/
                if (!SpaceTimeTowerMng.OShopInfo.DicBuyNum.ContainsKey(iProductId))
                {
                    SpaceTimeTowerMng.OShopInfo.DicBuyNum.Add(iProductId, 1);
                }
                else
                {
                    SpaceTimeTowerMng.OShopInfo.DicBuyNum[iProductId]++;
                }
                
                if (SpaceTimeTowerMng.OShopInfo.EShopType == SpaceTimeEventType.Shop)
                {
                    DelCoins(CurrenciesType.spaceTimeCoin, oProductInfo.iExpend, ConsumeWay.SpaceTimeShopBuy,
                        iProductId.ToString());
                    SpaceTimeTowerMng.UpdateConsumeCoins(oProductInfo.iExpend);
                }

                if (SpaceTimeTowerMng.TowerEvent != null && SpaceTimeTowerMng.TowerEvent.EventType == SpaceTimeEventType.House)
                {
                    SpaceTimeTowerMng.TowerLevelUp();
                    SendSpaceTimeTowerInfo();
                }
            }

            /*\ 同步商城信息 /*/
            SaveAndUpdateShopInfo();

            return eErrorCode;
        }
        
        #endregion
        
        #region [协议处理函数]

        /// <summary>
        /// 处理刷新卡池
        /// </summary>
        /// <param name="bIsAuto">是否是自动刷新 自动刷新不扣除次数</param>
        public void SpaceTimeRefreshCardPool(bool bIsAuto = false)
        {
            ErrorCode eErrCode = ErrorCode.Fail;
            do
            {
                if (!SpaceTimeTowerMng.IsOpening())
                {
                    Log.Warn($"player {Uid} SpaceTimeTower not open");
                    eErrCode = ErrorCode.NotOpen;
                    break;
                }
                
                /*\ 校验是否是免费刷新 /*/
                if (bIsAuto)
                {
                    addRefreshCardPoolHero();
                    break;
                }
                else
                {
                    /*\ 校验是否有次数 /*/
                    if (SpaceTimeTowerMng.ICurrRefreshNum >= SpaceTimeTowerLibrary.iRefreshCardPoolMaxNum)
                    {
                        Log.WarnLine("百兽玩法刷新卡池 没有刷新次数");
                        eErrCode = ErrorCode.SpaceTimeTowerRefreshMaxNum;
                        break;
                    }

                    if (!SpaceTimeTowerMng.Started)
                    {
                        Log.WarnLine("非正常请求，没有扣活动次数");
                        eErrCode = ErrorCode.Fail;
                        break;
                    }
                    
                    /*\ 校验消耗 /*/
                    int iExpendNum = calcCurrRefreshCardPoolExpend();
                    if (iExpendNum == -1)
                    {
                        break;
                    }

                    int iExpendType = SpaceTimeTowerLibrary.iBuyExpendType;
                    
                    /*\ 校验消耗 /*/
                    if (!CheckCoins((CurrenciesType) iExpendType , iExpendNum))
                    {
                        Log.WarnLine("百兽玩法 消耗不足");
                        eErrCode = ErrorCode.NoCoin;
                        break;
                    }

                    /*\ 刷新英雄并添加 /*/
                    addRefreshCardPoolHero();

                    SpaceTimeTowerMng.ICurrRefreshNum++;
                    
                    DelCoins((CurrenciesType) iExpendType, iExpendNum, ConsumeWay.SpaceTimeRefreshCardPool,
                        SpaceTimeTowerMng.TowerLevel.ToString());

                    SpaceTimeTowerMng.UpdateConsumeCoins(iExpendNum);

                    SpaceTimeTowerMng.SyncDbUpdateSpaceTimeTower();

                }
                
                /*\ 同步卡池信息 /*/
                SendSpaceTimeTowerInfo();
                eErrCode = ErrorCode.Success;
            } while (false);

            if (bIsAuto)
            {
                return;
            }
            /*\ 返回协议结果 /*/
            MSG_ZGC_SPACETIME_REFRESH_CARD_POOL oRes = new MSG_ZGC_SPACETIME_REFRESH_CARD_POOL
            {
                Result = (int) eErrCode,
                CurrRefreshNum = SpaceTimeTowerMng.ICurrRefreshNum
            };
            
            Write(oRes);
        }

        /// <summary>
        /// 领取阶段奖励
        /// </summary>
        /// <param name="iPage">页签</param>
        public void SpaceTimeGetStageAward(int iPage)
        {
            MSG_ZGC_SPACETIME_GET_STAGE_AWARD oMsg = new MSG_ZGC_SPACETIME_GET_STAGE_AWARD();
            oMsg.Result = (int) ErrorCode.Fail;

            do
            {
                // if (!SpaceTimeTowerMng.IsOpening())
                // {
                //     Log.Warn($"player {Uid} SpaceTimeTower not open");
                //     oMsg.Result = (int) ErrorCode.NotOpen;
                //     break;
                // }

                 if (SpaceTimeTowerMng.StageRewardPeriod != SpaceTimeTowerMng.PersonalPeriod)
                 {
                     Log.Warn($"player {Uid} SpaceTimeTower SpaceTimeGetStageAward failed: illegal request stage {SpaceTimeTowerMng.StageRewardPeriod} personal {SpaceTimeTowerMng.PersonalPeriod}");
                     oMsg.Result = (int) ErrorCode.Fail;
                     break;
                 }
                //此情况说明是上一阶段刚通关领完阶段奖励，还没有进行重置
                // else if (SpaceTimeTowerMng.StageRewardPeriod == SpaceTimeTowerMng.PersonalPeriod && !SpaceTimeTowerMng.PersonalPassed && SpaceTimeTowerMng.TowerLevel > SpaceTimeTowerLibrary.TowerMaxLevel)
                // {
                //     Log.Warn($"player {Uid} SpaceTimeTower SpaceTimeGetStageAward failed: personal period {SpaceTimeTowerMng.PersonalPeriod} not start yet");
                //     oMsg.Result = (int) ErrorCode.Fail;
                //     break;
                // }

                int iCurrStage = 1;
                if (SpaceTimeTowerMng.StageRewardPeriod > SpaceTimeTowerLibrary.iMaxStage)
                {
                    iCurrStage = SpaceTimeTowerLibrary.iMaxStage;
                }
                else
                {
                    iCurrStage = SpaceTimeTowerMng.StageRewardPeriod;
                }
                
                /*\ 判断是否领取过 /*/
                var lstAwardInfo = SpaceTimeTowerLibrary.GetStageAwardInfo(iCurrStage, iPage);
                if (lstAwardInfo == null)
                {
                    oMsg.Result = (int) ErrorCode.SpaceTimeTowerGetAwardAlready;
                    break;
                }

                List<int> lstGetId = new List<int>();
                List<string> lstAward = new List<string>();
                foreach (var oStageAward in lstAwardInfo)
                {
                    /*\ 判断当前层数是否可领取 /*/
                    if (!CheckCanGetStageRewards(oStageAward.iTier))continue;
                    
                    if (!SpaceTimeTowerMng.LstGetAwardId.Contains(oStageAward.iId))
                    {
                        lstAward.Add(oStageAward.strAward);
                        lstGetId.Add(oStageAward.iId);
                    }
                }

                if (lstAward.Count == 0)
                {
                    oMsg.Result = (int) ErrorCode.SpaceTimeTowerGetAwardAlready;
                    break;
                }

                /*\ 下发奖励 /*/
                RewardManager oAwardMgr = new RewardManager();
                foreach (var oAward in lstAward)
                {
                    oAwardMgr.AddSimpleReward(oAward);
                }
                oAwardMgr.BreakupRewards();
                AddRewards(oAwardMgr, ObtainWay.SpaceTimeStageAward, iPage.ToString());
            
                oAwardMgr.GenerateRewardItemInfo(oMsg.AwardInfo);

                /*\ 添加领取id /*/
                foreach (var id in lstGetId)
                {
                    if (!SpaceTimeTowerMng.LstGetAwardId.Contains(id))
                    {
                        SpaceTimeTowerMng.LstGetAwardId.Add(id);
                    }
                }

                /*\ 更新db /*/
                SpaceTimeTowerMng.SyncDbUpdateSpaceTimeTower();

                oMsg.Result = (int) ErrorCode.Success;
            } while (false);

            /*\ 更新奖励领取id /*/
            SendSpaceTimeTowerInfo();
            
            /*\ 同步客户端 /*/
            Write(oMsg);
        }

        private bool CheckCanGetStageRewards(int towerLevelLimit)
        {
            //个人难度小于本服难度
            if (SpaceTimeTowerMng.PersonalPeriod < server.SpaceTimeTowerManager.Period)
            {
                if (SpaceTimeTowerMng.PassedTopLevel < towerLevelLimit)
                {
                    return false;
                }
            }
            else
            {
                //个人难度等于本服难度
                if (!SpaceTimeTowerMng.PersonalPassed && SpaceTimeTowerMng.PassedTopLevel < towerLevelLimit)
                {
                    return false;
                }
            }
            return true;
        }
        #endregion

        /// <summary>
        /// 魂师升星/一键升星
        /// </summary>
        /// <param name="indexList">英雄池升星的英雄下标</param>
        public void SpaceTimeHeroStepUp(RepeatedField<int> indexList)
        {
            MSG_ZGC_SPACETIME_HERO_STEPUP response = new MSG_ZGC_SPACETIME_HERO_STEPUP();
            //response.IndexList.AddRange(indexList);

            if (!SpaceTimeTowerMng.IsOpening())
            {
                Log.Warn($"player {Uid} space time hero step up failed: SpaceTimeTower not open");
                response.Result = (int)ErrorCode.NotOpen;
                Write(response);
                return;
            }
            response.Result = (int)ErrorCode.Fail;

            List<SpaceTimeHeroInfo> updateList = new List<SpaceTimeHeroInfo>();
            foreach (int index in indexList)
            {
                int heroId;
                if (!SpaceTimeTowerMng.HeroPool.TryGetValue(index, out heroId))
                {
                    response.Result = (int)ErrorCode.Fail;
                    Log.Warn($"player {Uid} SpaceTimeHeroStepUp failed: index {index} not in hero pool");
                    continue;
                }

                ErrorCode errorCode = SpaceTimeTowerMng.HeroStepLevelUp(heroId, index, updateList);
                if (errorCode != ErrorCode.Success)
                {
                    response.Result = (int)errorCode;
                    Log.Warn($"player {Uid} SpaceTimeHeroStepUp failed: index {index} hero {heroId} not in team");
                    continue;
                }
                response.IndexList.Add(index);
            }
            if (updateList.Count > 0)
            {
                response.Result = (int)ErrorCode.Success;
                //同步db
                SyncDbUpdateSpaceTimeHeros(updateList);
                //同步变化给客户端
                SyncClientSpaceTimeHeroChange(updateList);
            }
            Write(response);
        }

        public void SyncClientSpaceTimeHeroChange(List<SpaceTimeHeroInfo> updateList)
        {
            MSG_ZGC_SPACE_TIME_HERO_CHANGE msg = new MSG_ZGC_SPACE_TIME_HERO_CHANGE();
            foreach (var hero in updateList)
            {
                msg.UpdateList.Add(GenerateSpaceTimeHeroInfo(hero));
            }
            Write(msg);
        }

        /// <summary>
        /// 更新阵容
        /// </summary>
        public void UpdateSpaceTimeHeroQueue(RepeatedField<HERO_DEFENSIVE_DATA> heroDefInfos)
        {
            MSG_ZGC_UPDATE_SPACETIME_HERO_QUEUE response = new MSG_ZGC_UPDATE_SPACETIME_HERO_QUEUE();
            if (heroDefInfos.Count > HeroLibrary.HeroPosCount)
            {
                response.Result = (int)ErrorCode.Fail;
                Log.Warn($"player {Uid} UpdateSpaceTimeHeroQueue failed: param count error");
                Write(response);
                return;
            }
            Dictionary<int, SpaceTimeHeroInfo> oldQueue = new Dictionary<int, SpaceTimeHeroInfo>();
            SpaceTimeTowerMng.HeroQueue.ForEach(x=>oldQueue.Add(x.Key, x.Value));
            SpaceTimeTowerMng.CLearHeroQueue();
            
            List<SpaceTimeHeroInfo> updateList = new List<SpaceTimeHeroInfo>();
            foreach (var item in heroDefInfos)
            {
                var queueInfo = item;
                int heroId = queueInfo.HeroId;
                SpaceTimeHeroInfo heroInfo = SpaceTimeTowerMng.GetHeroInfo(heroId);
                if (heroInfo == null)
                {
                    Log.Error($"player {uid} update space time hero queue fail,hero {heroId} not exist");
                    continue;
                }
                // if (heroInfo.PositionNum == item.PositionNum)
                // {
                //     Log.Error($"player {uid} update space time hero queue fail, hero {heroId} exist in position {item.PositionNum}");
                //     continue;
                // }
                SpaceTimeTowerMng.UpdateHeroQueue(heroInfo, item.PositionNum, updateList, oldQueue);
            }

            if (oldQueue.Count > 0)
            {
                foreach (var removeHero in oldQueue)
                {
                    removeHero.Value.SetPostionNum(-1);
                    updateList.Add(removeHero.Value);
                }
            }
            
            if (updateList.Count > 0)
            {
                SyncDbUpdateSpaceTimeHeros(updateList);
                SyncClientSpaceTimeHeroChange(updateList);
            }

            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        /// <summary>
        /// 执行事件内容
        /// </summary>
        /// <param name="eventType">事件类型</param>
        /// <param name="param">参数</param>
        public void SpaceTimeExecuteEvent(int eventType, int param, List<int> lstParam)
        {
            MSG_ZGC_SPACETIME_EXECUTE_EVENT response = new MSG_ZGC_SPACETIME_EXECUTE_EVENT();
            response.Type = eventType;
            response.Param = param;
            response.ArrParam.AddRange(lstParam);

            if (!SpaceTimeTowerMng.IsOpening())
            {
                Log.Warn($"player {Uid} execute space time event {eventType} param {param}  failed: SpaceTimeTower not open");
                response.Result = (int)ErrorCode.NotOpen;
                Write(response);
                return;
            }

            if (!SpaceTimeTowerMng.Started)
            {
                Log.Warn($"player {Uid} execute space time event {eventType} param {param}  failed: not click start yet");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            SpaceTimeTowerLevel towerLevelModel = SpaceTimeTowerLibrary.GetTowerLevelModel(SpaceTimeTowerMng.TowerLevel);
            if (towerLevelModel == null)
            {
                response.Result = (int)ErrorCode.Fail;
                Log.Warn($"player {Uid} SpaceTimeExecuteEvent {eventType} failed: not find tower level {SpaceTimeTowerMng.TowerLevel} model");
                Write(response);
                return;
            }

            ErrorCode result = SpaceTimeTowerMng.ExecuteEvent((SpaceTimeEventType)eventType, param, lstParam);
            if (result != ErrorCode.Success)
            {
                response.Result = (int)result;
                Write(response);
                return;
            }

            response.AwardInfo.AddRange(SpaceTimeTowerMng.lstAwardInfo);
            SpaceTimeTowerMng.lstAwardInfo.Clear();
            
            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        /// <summary>
        /// 重置
        /// </summary>
        public void SpaceTimeReset()
        {
            MSG_ZGC_SPACETIME_RESET response = new MSG_ZGC_SPACETIME_RESET();

            // if (!SpaceTimeTowerMng.IsOpening())
            // {
            //     Log.Warn($"player {Uid} execute space time reset failed: SpaceTimeTower not open");
            //     response.Result = (int)ErrorCode.NotOpen;
            //     Write(response);
            //     return;
            // }

            //策划要求前一阶段奖励领取完后要给玩家显示领取状态，阶段奖励变更和下个难度变更要放在重置里操作
            //阶段奖励领取完成进入下一阶段
            if (SpaceTimeTowerMng.PersonalPassed && SpaceTimeTowerMng.PersonalPeriod < server.SpaceTimeTowerManager.Period)
            {
                if (SpaceTimeTowerMng.LstGetAwardId.Count >= SpaceTimeTowerLibrary.GetStageAwardListCount(SpaceTimeTowerMng.StageRewardPeriod))
                {
                    SpaceTimeTowerMng.CheckChangeToNextDifficulty();
                }
                else
                {
                    Log.Warn($"player {Uid} space time reset failed: some stage rewards not get yet");
                    response.Result = (int)ErrorCode.SpaceTimeTowerNotGetStageAward;
                    Write(response);
                    return;
                }
            }
            
            SpaceTimeTowerMng.Reset(true);
            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        public void SpaceTimeTowerSuccess(int isBeastDungeon)
        {
            //魂导器扣减次数
            SpaceTimeTowerMng.ConsumeGuideSoulItems();
            
            //凶兽关卡
            if (isBeastDungeon == 1)
            {
                SpaceTimeTowerMng.RandomOptionalGuideSoulItems();
            }
            
            SpaceTimeTowerMng.TowerLevelUp();
            SendSpaceTimeTowerInfo();
            SendSpaceTimeDungeonSettlement();
        }

        public void SpaceTimeTowerFail(int pointState)
        {
            //魂导器扣减次数
            SpaceTimeTowerMng.ConsumeGuideSoulItems();
            
            SpaceTimeTowerMng.ChallengeFail();
            SendSpaceTimeTowerInfo();
            SendSpaceTimeDungeonSettlement();
            
            RecordPetBITowerLog(SpaceTimeTowerMng.TowerLevel, pointState);
        }

        /// <summary>
        /// 通关结算通知
        /// </summary>
        private void SendSpaceTimeDungeonSettlement()
        {
            if (SpaceTimeTowerMng.TowerLevel > SpaceTimeTowerLibrary.TowerMaxLevel || SpaceTimeTowerMng.FailCount >= SpaceTimeTowerLibrary.ChallengeMaxCount)
            {
                MSG_ZGC_SPACETIME_DUNGEON_SETTLEMENT notify = new MSG_ZGC_SPACETIME_DUNGEON_SETTLEMENT();
                notify.TowerLevel = SpaceTimeTowerMng.TowerLevel;
                notify.FailCount = SpaceTimeTowerMng.FailCount;
                notify.GetCoins = GetCoins(CurrenciesType.spaceTimeCoin) - SpaceTimeTowerLibrary.InitSpaceTimeCoins + SpaceTimeTowerMng.ConsumeCoins;
                notify.RemainCoins = GetCoins(CurrenciesType.spaceTimeCoin);
                notify.Success = SpaceTimeTowerMng.TowerLevel > SpaceTimeTowerLibrary.TowerMaxLevel ? true : false;
                Write(notify);
            }
        }

        /// <summary>
        /// 凶兽关卡结算通知
        /// </summary>
        public void SpaceTimeBeastDungeonSettlement()
        {
            MSG_ZGC_SPACETIME_BEAST_SETTLEMENT response = new MSG_ZGC_SPACETIME_BEAST_SETTLEMENT();
            foreach (var kv in SpaceTimeTowerMng.OptionalGuideSoulItems)
            {
                response.Rewards.Add(new ZGC_GUIDESOUL_SELECTION_REWARD(){ItemId = kv.Key, ExtraRewardNum = kv.Value});
            }
            Write(response);
        }

        /// <summary>
        /// 凶兽结算选择魂导器
        /// </summary>
        /// <param name="itemId"></param>
        public void SelectGuideSoulItem(int itemId)
        {
            MSG_ZGC_SELECT_GUIDESOUL_ITEM response = new MSG_ZGC_SELECT_GUIDESOUL_ITEM();
            response.ItemId = itemId;
            if (!SpaceTimeTowerMng.IsOpening())
            {
                Log.Warn($"player {Uid} select guide soul item failed: SpaceTimeTower not open");
                response.Result = (int)ErrorCode.NotOpen;
                Write(response);
                return;
            }

            int coinNum;
            if (!SpaceTimeTowerMng.OptionalGuideSoulItems.TryGetValue(itemId, out coinNum))
            {
                Log.Warn($"player {Uid} select guide soul item failed: ");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            SpaceTimeTowerMng.ClearOptionalGuideSoulItems();
            
            GuideSoulItemModel itemModel = SpaceTimeTowerLibrary.GetGuideSoulItemModel(itemId);
            string rewards = string.Format($"{itemId}:{(int)RewardType.GuideSoulItem}:1|{(int)CurrenciesType.spaceTimeCoin}:{(int)RewardType.Currencies}:{coinNum}");
            RewardManager manager = new RewardManager();
            manager.InitSimpleReward(rewards.ToString());
            AddRewards(manager, ObtainWay.SpaceTimeTower);
            manager.GenerateRewardItemInfo(response.Rewards);
            
            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        /// <summary>
        /// 商店进入下一层
        /// </summary>
        public void SpaceTimeEnterNextLevel()
        {
            MSG_ZGC_SPACETIME_ENTER_NEXTLEVEL response = new MSG_ZGC_SPACETIME_ENTER_NEXTLEVEL();

            if (!SpaceTimeTowerMng.IsOpening())
            {
                Log.Warn($"player {Uid} space time enter next level failed: SpaceTimeTower not open");
                response.Result = (int)ErrorCode.NotOpen;
                Write(response);
                return;
            }
            
            if (SpaceTimeTowerMng.TowerEvent == null || SpaceTimeTowerMng.TowerEvent.EventType != SpaceTimeEventType.Shop)
            {
                Log.Warn($"player {Uid} space time enter next level failed: cur event is not shop");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
            
            SpaceTimeTowerMng.TowerLevelUp();
            SendSpaceTimeTowerInfo();
            
            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        /// <summary>
        /// 时空屋随机参数
        /// </summary>
        public void SpaceTimeHouseRandomParam()
        {
            MSG_ZGC_SPACETIME_HOUSE_RANDOM_PARAM response = new MSG_ZGC_SPACETIME_HOUSE_RANDOM_PARAM();
            if (!SpaceTimeTowerMng.IsOpening())
            {
                Log.Warn($"player {Uid} space time house random param failed: SpaceTimeTower not open");
                response.Result = (int)ErrorCode.NotOpen;
                Write(response);
                return;
            }
            
            if (SpaceTimeTowerMng.TowerEvent == null || SpaceTimeTowerMng.TowerEvent.EventType != SpaceTimeEventType.House)
            {
                Log.Warn($"player {Uid} space time house random param failed: cur event is not shop");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            if (!SpaceTimeTowerMng.RandomHouseEventParam())
            {
                Log.Warn($"player {Uid} space time house random param failed: random param error");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            response.Info = BuildInfo();
            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        /// <summary>
        /// 进入时空塔(开始闯关)
        /// </summary>
        public void EnterSpaceTimeTower()
        {
            MSG_ZGC_ENTER_SPACETIME_TOWER response = new MSG_ZGC_ENTER_SPACETIME_TOWER();
            if (!SpaceTimeTowerMng.IsOpening())
            {
                Log.Warn($"player {Uid} enter space time tower failed: SpaceTimeTower not open");
                response.Result = (int)ErrorCode.NotOpen;
                Write(response);
                return;
            }

            if (SpaceTimeTowerMng.TowerLevel != 1 || GetCounterValue(CounterType.SpaceTimeTowerCount) <= 0)
            {
                Log.Warn($"player {Uid} enter space time tower failed: count not enough");
                response.Result = (int)ErrorCode.SpaceTimeTowerCountNotEnough;
                Write(response);
                return;
            }

            if (SpaceTimeTowerMng.Started)
            {
                Log.Warn($"player {Uid} enter space time tower failed: already start");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
            //挑战下一难度前需先领取前一难度的奖励
            int rewardsCount = SpaceTimeTowerLibrary.GetStageAwardListCount(SpaceTimeTowerMng.StageRewardPeriod);
            if (SpaceTimeTowerMng.PersonalPassed && SpaceTimeTowerMng.PersonalPeriod < server.SpaceTimeTowerManager.Period && SpaceTimeTowerMng.LstGetAwardId.Count < rewardsCount)
            {
                Log.Warn($"player {Uid} enter space time tower failed: need get stage rewards first");
                response.Result = (int)ErrorCode.SpaceTimeTowerNotGetStageAward;
                Write(response);
                return;
            }
            
            UpdateCounter(CounterType.SpaceTimeTowerCount, -1);

            SpaceTimeTowerMng.UpdateStartState(true);
            SpaceTimeTowerMng.SyncDbUpdateSpaceTimeTower();
            
            response.Result = (int)ErrorCode.Success;
            Write(response);
        }

        /// <summary>
        /// 领取往期奖励
        /// </summary>
        public void SpaceTimeGetPastRewards()
        {
            MSG_ZGC_SPACETIME_GET_PAST_REWARDS response = new MSG_ZGC_SPACETIME_GET_PAST_REWARDS();

            if (!CheckLimitOpen(LimitType.SpaceTimeTower))
            {
                Log.Warn($"player {Uid} space time get past rewards failed: open limit");
                response.Result = (int)ErrorCode.NotOpen;
                Write(response);
                return;
            }

            if (SpaceTimeTowerMng.PastRewardsState)
            {
                Log.Warn($"player {Uid} space time get past rewards failed: already got past rewards");
                response.Result = (int)ErrorCode.AlreadyGot;
                Write(response);
                return;
            }

            List<string> rewards = SpaceTimeTowerLibrary.GetPastRewardsByStage(SpaceTimeTowerMng.PeriodPassedBefore);
            if (rewards.Count == 0)
            {
                Log.Warn($"player {Uid} space time get past rewards failed: not find past rewards");
                response.Result = (int)ErrorCode.SpaceTimeTowerNotFindPastRewards;
                Write(response);
                return;
            }

            SpaceTimeTowerMng.UpdatePastRewardsState(true);
            SpaceTimeTowerMng.SyncDbUpdateSpaceTimeTower();
            
            RewardManager manager = new RewardManager();
            foreach (string reward in rewards)
            {
                if (!string.IsNullOrEmpty(reward))
                {
                    manager.AddSimpleReward(reward);
                }
            }
            manager.BreakupRewards();
            AddRewards(manager, ObtainWay.SpaceTimeStageAward);
            manager.GenerateRewardMsg(response.Rewards);

            response.Result = (int)ErrorCode.Success;
            Write(response);
        }
        
        public void SendSpaceTimeTowerOpenTime()
        {
            MSG_ZGC_SPACETIME_TIME msg = new MSG_ZGC_SPACETIME_TIME();
            msg.Status = SpaceTimeTowerMng.IsOpening();
            msg.Time = msg.Status ? Timestamp.GetUnixTimeStampSeconds(SpaceTimeTowerMng.EndTime) : Timestamp.GetUnixTimeStampSeconds(SpaceTimeTowerMng.StartTime);
            Write(msg);
        }

        public void SpaceTimeTowerLimitOpen()
        {
            SpaceTimeTowerMng.CheckTime();
        }

        public void UpdateSpaceTimeTower()
        {
            SpaceTimeTowerMng.Update();
        }
        
        private ZGC_NINE_NATURE GenerateNineNatureMsg(Natures natures)
        {
            ZGC_NINE_NATURE nature = new ZGC_NINE_NATURE();
            nature.MaxHp = natures.GetNatureValue(NatureType.PRO_MAX_HP).ToInt64TypeMsg();
            nature.Def = natures.GetNatureValue(NatureType.PRO_DEF).ToInt64TypeMsg();
            nature.Atk = natures.GetNatureValue(NatureType.PRO_ATK).ToInt64TypeMsg();
            nature.Cri = natures.GetNatureValue(NatureType.PRO_CRI).ToInt64TypeMsg();
            nature.Hit = natures.GetNatureValue(NatureType.PRO_HIT).ToInt64TypeMsg();
            nature.Flee = natures.GetNatureValue(NatureType.PRO_FLEE).ToInt64TypeMsg();
            nature.Imp = natures.GetNatureValue(NatureType.PRO_IMP).ToInt64TypeMsg();
            nature.Arm = natures.GetNatureValue(NatureType.PRO_ARM).ToInt64TypeMsg();
            nature.Res = natures.GetNatureValue(NatureType.PRO_RES).ToInt64TypeMsg();
            return nature;
        }

        public void SyncDbUpdateSpaceTimeHeros(List<SpaceTimeHeroInfo> updateList)
        {
            server.GameDBPool.Call(new QueryUpdateSpaceTimeHeros(Uid, updateList));
        }

        public void SendGuideSoulRestCountsInfo(List<int> deleteList = null, int deleteType=0)
        {
            MSG_ZGC_SPACETIME_GUIDESOUL_RESTCOUNTS msg = new MSG_ZGC_SPACETIME_GUIDESOUL_RESTCOUNTS();
            foreach (var kv in SpaceTimeTowerMng.GuideSoulRestCounts)
            {
                msg.GuideSoulRestCounts.Add(kv.Key, kv.Value);
            }
            if (deleteList != null)
            {
                 msg.DeleteList.AddRange(deleteList);
                 msg.DeleteType = deleteType;
            }
            Write(msg);
        }
        
        public void AddGuideSoulItems(RewardManager manager, ObtainWay way, string extraParam = "")
        {
            Dictionary<int, int> addList = new Dictionary<int, int>();
            var guideSoulItems = manager.GetRewardList(RewardType.GuideSoulItem);
            if (guideSoulItems == null)
            {
                return;
            }
            foreach (var item in guideSoulItems)
            {
                GuideSoulItemModel itemModel = SpaceTimeTowerLibrary.GetGuideSoulItemModel(item.Key);
                if (itemModel != null)
                {
                    addList.Add(item.Key, itemModel.EffectCount);
                    //获取埋点
                    RecordObtainLog(way, RewardType.GuideSoulItem, item.Key, 1, 1, extraParam);
                    BIRecordObtainItem(RewardType.GuideSoulItem, way, item.Key, 1, 1);
                }
            }

            if (addList.Count > 0)
            {
                SpaceTimeTowerMng.AddGuideSoulItems(addList);
                if (way != ObtainWay.SpaceTimeTowerDungeonReward)
                {
                    SendGuideSoulRestCountsInfo();
                }
            }
        }
        
        public void RecordPetBITowerLog(int towerLevel,  int pointState)
        {
            PetInfo petInfo = PetManager.GetDungeonQueuePet(DungeonQueueType.SpaceTimeTower, 1);
            if (petInfo != null)
            {
                BIRecordPetTowerLog(towerLevel, pointState, petInfo.PetId, petInfo.Level, petInfo.Aptitude, petInfo.BreakLevel);
            }
            else
            {
                BIRecordPetTowerLog(towerLevel, pointState, 0, 0, 0, 0);
            }
        }
    }
}
