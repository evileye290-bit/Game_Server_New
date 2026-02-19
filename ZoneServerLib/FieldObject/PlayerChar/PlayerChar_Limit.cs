using CommonUtility;
using EnumerateUtility;
using RedisUtility;
using ServerModels;
using ServerShared;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public partial class PlayerChar : FieldObject
    {
        //开启条件

        public void CheckLevelLimitOpen()
        {
            List<LimitType> LimitList = LimitLibrary.GetLevelLimitList(Level);
            if (LimitList != null)
            {
                foreach (var type in LimitList)
                {
                    LimitData data = LimitLibrary.GetLimitData(type);
                    if (data != null)
                    {
                        if (!CheckTaskId(data))
                        {
                            continue;
                        }

                        if (!CheckBranchTaskId(data))
                        {
                            continue;
                        }

                        LimitOpen(type);
                    }
                }
            }
        }

        public void CheckTaskIdLimitOpen(int taskId)
        {
            List<LimitType> LimitList = LimitLibrary.GetTaskIdLimitList(taskId);
            if (LimitList != null)
            {
                foreach (var type in LimitList)
                {
                    LimitData data = LimitLibrary.GetLimitData(type);
                    if (data != null)
                    {
                        if (!CheckLevelId(data))
                        {
                            continue;
                        }

                        if (!CheckBranchTaskId(data))
                        {
                            continue;
                        }

                        LimitOpen(type);
                    }
                }
            }
        }

        public void CheckBranchTaskIdLimitOpen(int taskId)
        {
            List<LimitType> LimitList = LimitLibrary.GetTaskIdLimitList(taskId);
            if (LimitList != null)
            {
                foreach (var type in LimitList)
                {
                    LimitData data = LimitLibrary.GetLimitData(type);
                    if (data != null)
                    {
                        if (!CheckLevelId(data))
                        {
                            continue;
                        }

                        if (!CheckTaskId(data))
                        {
                            continue;
                        }

                        LimitOpen(type);
                    }
                }
            }
        }

     

        public bool CheckLimitOpen(LimitType type)
        {
            LimitData data = LimitLibrary.GetLimitData(type);
            if (data == null)
            {
                return true;
            }
            return CheckLimitOpen(data);
        }

        private bool CheckLimitOpen(LimitData data)
        {
            if (!CheckLevelId(data))
            {
                return false;
            }
            if (!CheckTaskId(data))
            {
                return false;
            }
            if (!CheckBranchTaskId(data))
            {
                return false;
            }
            return true;
        }

        public bool CheckLimitOpen(MapType mapType)
        {
            switch (mapType)
            {
                case MapType.Gold:return CheckLimitOpen(LimitType.BenefitsGold);
                case MapType.Exp:return CheckLimitOpen(LimitType.BenefitsExp);
                case MapType.SoulPower:return CheckLimitOpen(LimitType.BenefitsSoulPower);
                case MapType.SoulBreath:return CheckLimitOpen(LimitType.BenefitsSoulBreath);
            }
            return false;
        }

    

        private bool CheckLevelId(LimitData data)
        {
            if (data.Level > 0)
            {
                if (Level>= data.Level)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        private bool CheckTaskId(LimitData data)
        {
            if (data.TaskIds.Count > 0)
            {
                foreach (var taskId in data.TaskIds)
                {
                    if (MainTaskId >= taskId)
                    {
                        return true;
                    }
                }
                return false;
            }
            else
            {
                return true;
            }
        }

        private bool CheckBranchTaskId(LimitData data)
        {
            if (data.BranchTaskIds.Count > 0)
            {
                foreach (var taskId in data.BranchTaskIds)
                {
                    if (BranchTaskIds.Contains(taskId))
                    {
                        return true;
                    }
                }
                return false;
            }
            else
            {
                return true;
            }
        }


        private void LimitOpen(LimitType type)
        {
            switch (type)
            {
                case LimitType.FriendHeart:
                    //FriendHeartLimitOpen();
                    break;
                case LimitType.CampStars:
                    CampStarsLimitOpen();
                    break;
                case LimitType.Onhook:
                    ChecnkAndOnhookOpen();
                    break;
                case LimitType.Activity:
                    ActivityOpen();
                    break;
                case LimitType.Tower:
                    TowerLimitOpen();
                    break;
                case LimitType.IslandChallenge:
                    IslandChallengeLimitOpen();
                    break;
                case LimitType.SpecialActivity:
                    SpecialActivityOpen();
                    SendSpecialActivityListMessage();
                    break;
                case LimitType.SpaceTimeTower:
                    SpaceTimeTowerLimitOpen();
                    break;
                case LimitType.DriftExplore:
                    DriftExploreOpen();
                    break;
                default:
                    break;
            }
        }



        //public void FriendHeartLimitOpen()
        //{
        //    //爱心信息添加
        //    FriendlyHeartLimitOpen();
        //}

        public void CampStarsLimitOpen()
        {
            //增加阵营养成属性
            AddCampStarsNatures();
        }
        //public void ShopCardLimitOpen()
        //{
        //    //更新商店
        //    RefreshCardList();

        //    SyncDbUpdateAllShopItem();
        //    Write(GetAllShopList());
        //}

        //public void ShopChestLimitOpen()
        //{
        //    //更新金币，宝箱商店
        //    RefreshChestList();

        //    SyncDbUpdateAllShopItem();
        //    Write(GetAllShopList());
        //}

        //public void ShopGoldLimitOpen()
        //{
        //    //更新金币，宝箱商店
        //    RefreshGoldList();

        //    SyncDbUpdateAllShopItem();
        //    Write(GetAllShopList());
        //}

        //public void ShopFishLimitOpen()
        //{
        //    //更新金币，宝箱商店
        //    RefreshFishList1();
        //    RefreshFishList2();

        //    SyncDbUpdateAllShopItem();
        //    Write(GetAllShopList());
        //}

        //public void CheckHeroIdLimitOpen(int heroId)
        //{
        //    List<LimitType> LimitList = LimitLibrary.GetHeroIdLimitList(heroId);
        //    if (LimitList != null)
        //    {
        //        foreach (var type in LimitList)
        //        {
        //            LimitData data = LimitLibrary.GetLimitData(type);
        //            if (data != null)
        //            {
        //                if (CheckLevelId(data))
        //                {
        //                    if (CheckTaskId(data))
        //                    {
        //                        if (CheckLadderLevel(data))
        //                        {
        //                            //if (CheckStory(data))
        //                            {
        //                                if (CheckBranchTaskId(data))
        //                                {
        //                                    LimitOpen(type);
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}



        //public void CheckLadderLevelLimitOpen()
        //{
        //    List<LimitType> LimitList = LimitLibrary.GetLadderLevelLimitList(ladderHistoryMaxLevel);
        //    if (LimitList != null)
        //    {
        //        foreach (var type in LimitList)
        //        {
        //            LimitData data = LimitLibrary.GetLimitData(type);
        //            if (data != null)
        //            {
        //                if (CheckLevelId(data))
        //                {

        //                    if (CheckTaskId(data))
        //                    {
        //                        //if (CheckStory(data))
        //                        {
        //                            if (CheckBranchTaskId(data))
        //                            {
        //                                LimitOpen(type);
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        //public void CheckStoryLimitOpen(int curStory)
        //{
        //    List<LimitType> LimitList = LimitLibrary.GetStoryLimitList(curStory);
        //    if (LimitList != null)
        //    {
        //        foreach (var type in LimitList)
        //        {
        //            LimitData data = LimitLibrary.GetLimitData(type);
        //            if (data == null) continue;
        //            if (!CheckLevelId(data)) continue;
        //            if (!CheckTaskId(data)) continue;
        //            if (!CheckLadderLevel(data)) continue;
        //            if (!CheckBranchTaskId(data)) continue;

        //            LimitOpen(type);
        //        }
        //    }
        //}
        //private bool CheckHeroId(LimitData data)
        //{
        //    if (data.HeroIds.Count > 0)
        //    {
        //        foreach (var heroId in data.HeroIds)
        //        {
        //            Hero hero = HeroMng.GetHero(heroId);
        //            if (hero != null)
        //            {
        //                if (hero.State == HeroState.Unlocked)
        //                {
        //                    //Log.Warn("player {0} check task {1} limit unlock error {2} hero {3}", Uid, taskId, hero.State, heroId);
        //                    return true;
        //                }
        //            }
        //            //else
        //            //{
        //            //    //Log.Warn("player {0} check task {1} limit unlock error not find hero {2}", Uid, taskId, heroId);
        //            //    return false;
        //            //}
        //        }
        //        return false;
        //    }
        //    else
        //    {
        //        return true;
        //    }
        //}


        //private bool CheckLadderLevel(LimitData data)
        //{
        //    if (data.LadderLevel > 0)
        //    {
        //        if (ladderHistoryMaxLevel >= data.LadderLevel)
        //        {
        //            return true;
        //        }
        //        else
        //        {
        //            return false;
        //        }
        //    }
        //    else
        //    {
        //        return true;
        //    }
        //}

        //private bool CheckStory(LimitData data)
        //{
        //    if (data.Story > 0)
        //    {
        //        //int curStory = GameLevelMng.FinishNormal;
        //        //if (curStory >= data.Story)
        //        //{
        //        //    return true;
        //        //}
        //        //else
        //        {
        //            return false;
        //        }
        //    }
        //    else
        //    {
        //        return true;
        //    }
        //}
    }
}
