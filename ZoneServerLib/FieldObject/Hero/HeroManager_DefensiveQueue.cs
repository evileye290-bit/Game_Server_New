using EnumerateUtility;
using ServerModels;
using ServerShared;
using System.Linq;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public partial class HeroManager
    {
        public Dictionary<int, Dictionary<int, HeroInfo>> DefensiveQueue = new Dictionary<int, Dictionary<int, HeroInfo>>();
        public Dictionary<int, Dictionary<int, HeroInfo>> CrossQueue = new Dictionary<int, Dictionary<int, HeroInfo>>();
        public Dictionary<int, Dictionary<int, HeroInfo>> ThemeBossQueue = new Dictionary<int, Dictionary<int, HeroInfo>>();
        public Dictionary<int, Dictionary<int, HeroInfo>> CrossBossQueue = new Dictionary<int, Dictionary<int, HeroInfo>>();
        public Dictionary<int, Dictionary<int, HeroInfo>> CarnivalBossQueue = new Dictionary<int, Dictionary<int, HeroInfo>>();
        public Dictionary<int, Dictionary<int, HeroInfo>> CrossChallengeQueue = new Dictionary<int, Dictionary<int, HeroInfo>>();

        public void BindHeroQueueList(HeroInfo info)
        {
            if (info.DefensiveQueueNum > 0)
            {
                Dictionary<int, HeroInfo> heroPos = GetDefensivePosOrNew(HeroQueueType.CampDefensive, info.DefensiveQueueNum);
                if (!heroPos.ContainsKey(info.DefensivePositionNum) && heroPos.Count < HeroLibrary.HeroPosCount)
                {
                    heroPos.Add(info.DefensivePositionNum, info);
                }
            }

            if (info.CrossQueueNum > 0)
            {
                Dictionary<int, HeroInfo> heroPos = GetDefensivePosOrNew(HeroQueueType.CrossBattle, info.CrossQueueNum);
                if (!heroPos.ContainsKey(info.CrossPositionNum) && heroPos.Count < HeroLibrary.HeroPosCount)
                {
                    heroPos.Add(info.CrossPositionNum, info);
                }
            }

            //主题Boss
            if (info.ThemeBossQueueNum > 0)
            {
                Dictionary<int, HeroInfo> heroPos = GetDefensivePosOrNew(HeroQueueType.ThemeBoss, info.ThemeBossQueueNum);
                if (!heroPos.ContainsKey(info.ThemeBossPositionNum) && heroPos.Count < HeroLibrary.HeroPosCount)
                {
                    heroPos.Add(info.ThemeBossPositionNum, info);
                }
            }

            //跨服boss
            if (info.CrossBossQueueNum > 0)
            {
                Dictionary<int, HeroInfo> heroPos = GetDefensivePosOrNew(HeroQueueType.CrossBoss, info.CrossBossQueueNum);
                if (!heroPos.ContainsKey(info.CrossBossPositionNum) && heroPos.Count < HeroLibrary.HeroPosCount)
                {
                    heroPos.Add(info.CrossBossPositionNum, info);
                }
            }

            //嘉年华Boss
            if (info.CarnivalBossQueueNum > 0)
            {
                Dictionary<int, HeroInfo> heroPos = GetDefensivePosOrNew(HeroQueueType.CarnivalBoss, info.CarnivalBossQueueNum);
                if (!heroPos.ContainsKey(info.CarnivalBossPositionNum) && heroPos.Count < HeroLibrary.HeroPosCount)
                {
                    heroPos.Add(info.CarnivalBossPositionNum, info);
                }
            }

            //魂师挑战
            if (info.CrossChallengeQueueNum > 0)
            {
                Dictionary<int, HeroInfo> heroPos = GetDefensivePosOrNew(HeroQueueType.CrossChallenge, info.CrossChallengeQueueNum);
                if (!heroPos.ContainsKey(info.CrossChallengePositionNum) && heroPos.Count < HeroLibrary.HeroPosCount)
                {
                    heroPos.Add(info.CrossChallengePositionNum, info);
                }
            }
        }

        public void CheckAndFixCrossPos()
        {
            var temp = heroInfoList.Values.Where(x => x.CrossQueueNum > 0);
            List<HeroInfo> heroInfos = new List<HeroInfo>();
            foreach (var info in temp)
            {
                Dictionary<int, HeroInfo> heroPos = GetDefensivePosOrNew(HeroQueueType.CrossBattle, info.CrossQueueNum);
                if (heroPos.ContainsKey(info.CrossPositionNum))
                {
                    //相同位置上了重复英雄
                    if (heroPos[info.CrossPositionNum].Id != info.Id)
                    {
                        heroInfos.Add(info);
                    }
                }
                else
                { 
                    //上阵多了
                    heroInfos.Add(info);
                }
            }

            if (heroInfos.Count > 0)
            {
                foreach (var kv in heroInfos)
                {
                    kv.CrossQueueNum = 0;
                    kv.CrossPositionNum = 0;
                    owner.SyncDbUpdateHeroItem(kv);
                }
            }
        }

        public void CheckAndFixCrossChallengePos()
        {
            var temp = heroInfoList.Values.Where(x => x.CrossChallengeQueueNum > 0);
            List<HeroInfo> heroInfos = new List<HeroInfo>();
            foreach (var info in temp)
            {
                Dictionary<int, HeroInfo> heroPos = GetDefensivePosOrNew(HeroQueueType.CrossChallenge, info.CrossChallengeQueueNum);
                if (heroPos.ContainsKey(info.CrossChallengePositionNum))
                {
                    //相同位置上了重复英雄
                    if (heroPos[info.CrossChallengePositionNum].Id != info.Id)
                    {
                        heroInfos.Add(info);
                    }
                }
                else
                {
                    //上阵多了
                    heroInfos.Add(info);
                }
            }

            if (heroInfos.Count > 0)
            {
                foreach (var kv in heroInfos)
                {
                    kv.CrossChallengeQueueNum = 0;
                    kv.CrossChallengePositionNum = 0;
                    owner.SyncDbUpdateHeroItem(kv);
                }
            }
        }

        /// <summary>
        /// 更新位置
        /// </summary>
        /// <param name="heroInfo"></param>
        /// <param name="queueNum"></param>
        /// <param name="posNum"></param>
        /// <returns></returns>
        public void UpdateDefQueue(HeroQueueType type, HeroInfo heroInfo, int queueNum, int posNum, Dictionary<int, HeroInfo> updateList)
        {
            switch (type)
            {
                case HeroQueueType.CampDefensive:
                    {
                        //卸下原来布阵
                        HeroInfo curInfo = RemoveDefensivePos(type, heroInfo.DefensiveQueueNum, heroInfo.DefensivePositionNum);
                        if (curInfo != null)
                        {
                            updateList[heroInfo.Id] = heroInfo;
                        }

                        //修改当前位置
                        heroInfo.DefensiveQueueNum = queueNum;
                        heroInfo.DefensivePositionNum = posNum;
                    }

                    break;
                case HeroQueueType.CrossBattle:
                    {
                        //卸下原来布阵
                        HeroInfo curInfo = RemoveDefensivePos(type, heroInfo.CrossQueueNum, heroInfo.CrossPositionNum);
                        if (curInfo != null)
                        {
                            updateList[heroInfo.Id] = heroInfo;
                        }

                        //修改当前位置
                        heroInfo.CrossQueueNum = queueNum;
                        heroInfo.CrossPositionNum = posNum;
                    }
                    break;
                case HeroQueueType.ThemeBoss:
                    {
                        //卸下原来布阵
                        HeroInfo curInfo = RemoveDefensivePos(type, heroInfo.ThemeBossQueueNum, heroInfo.ThemeBossPositionNum);
                        if (curInfo != null)
                        {
                            updateList[heroInfo.Id] = heroInfo;
                        }

                        //修改当前位置
                        heroInfo.ThemeBossQueueNum = queueNum;
                        heroInfo.ThemeBossPositionNum = posNum;
                    }
                    break;
                case HeroQueueType.CrossBoss:
                    {
                        //卸下原来布阵
                        HeroInfo curInfo = RemoveDefensivePos(type, heroInfo.CrossBossQueueNum, heroInfo.CrossBossPositionNum);
                        if (curInfo != null)
                        {
                            updateList[heroInfo.Id] = heroInfo;
                        }

                        //修改当前位置
                        heroInfo.CrossBossQueueNum = queueNum;
                        heroInfo.CrossBossPositionNum = posNum;
                    }
                    break;
                case HeroQueueType.CarnivalBoss:
                    {
                        //卸下原来布阵
                        HeroInfo curInfo = RemoveDefensivePos(type, heroInfo.CarnivalBossQueueNum, heroInfo.CarnivalBossPositionNum);
                        if (curInfo != null)
                        {
                            updateList[heroInfo.Id] = heroInfo;
                        }

                        //修改当前位置
                        heroInfo.CarnivalBossQueueNum = queueNum;
                        heroInfo.CarnivalBossPositionNum = posNum;
                    }
                    break;
                case HeroQueueType.CrossChallenge:
                {
                    //卸下原来布阵
                    HeroInfo curInfo = RemoveDefensivePos(type, heroInfo.CrossChallengeQueueNum, heroInfo.CrossChallengePositionNum);
                    if (curInfo != null)
                    {
                        updateList[heroInfo.Id] = heroInfo;
                    }

                    //修改当前位置
                    heroInfo.CrossChallengeQueueNum = queueNum;
                    heroInfo.CrossChallengePositionNum = posNum;
                }
                    break;
                default:
                    break;
            }

            //移除摆放位置
            HeroInfo removeInfo = RemoveDefensivePos(type, queueNum, posNum);
            if (removeInfo != null)
            {
                updateList[removeInfo.Id] = removeInfo;
            }

            BindHeroQueueList(heroInfo);
            updateList[heroInfo.Id] = heroInfo;
        }

        private Dictionary<int, HeroInfo> GetDefensivePosOrNew(HeroQueueType type, int queueNum)
        {
            Dictionary<int, HeroInfo> heroPos = GetDefensivePos(type, queueNum);
            if (heroPos == null)
            {
                heroPos = new Dictionary<int, HeroInfo>();

                switch (type)
                {
                    case HeroQueueType.CampDefensive:
                        DefensiveQueue.Add(queueNum, heroPos);
                        break;
                    case HeroQueueType.CrossBattle:
                        CrossQueue.Add(queueNum, heroPos);
                        break;
                    case HeroQueueType.ThemeBoss:
                        ThemeBossQueue.Add(queueNum, heroPos);
                        break;
                    case HeroQueueType.CrossBoss:
                        CrossBossQueue.Add(queueNum, heroPos);
                        break;
                    case HeroQueueType.CarnivalBoss:
                        CarnivalBossQueue.Add(queueNum, heroPos);
                        break;
                    case HeroQueueType.CrossChallenge:
                        CrossChallengeQueue.Add(queueNum, heroPos);
                        break;
                    default:
                        break;
                }
            }
            return heroPos;
        }

        public Dictionary<int, HeroInfo> GetDefensivePos(HeroQueueType type, int queueNum)
        {
            Dictionary<int, HeroInfo> heroPos = null;
            switch (type)
            {
                case HeroQueueType.CampDefensive:
                    DefensiveQueue.TryGetValue(queueNum, out heroPos);
                    break;
                case HeroQueueType.CrossBattle:
                    CrossQueue.TryGetValue(queueNum, out heroPos);
                    break;
                case HeroQueueType.ThemeBoss:
                    ThemeBossQueue.TryGetValue(queueNum, out heroPos);
                    break;
                case HeroQueueType.CrossBoss:
                    CrossBossQueue.TryGetValue(queueNum, out heroPos);
                    break;
                case HeroQueueType.CarnivalBoss:
                    CarnivalBossQueue.TryGetValue(queueNum, out heroPos);
                    break;
                case HeroQueueType.CrossChallenge:
                    CrossChallengeQueue.TryGetValue(queueNum, out heroPos);
                    break;
                default:
                    break;
            }
            return heroPos;
        }

        private HeroInfo RemoveDefensivePos(HeroQueueType type, int srcQueueNum, int srcPosNum)
        {
            HeroInfo heroInfo = null;
            if (srcQueueNum > 0)
            {
                Dictionary<int, HeroInfo> heroPos = GetDefensivePos(type, srcQueueNum);
                if (heroPos != null)
                {
                    if (heroPos.TryGetValue(srcPosNum, out heroInfo))
                    {
                        switch (type)
                        {
                            case HeroQueueType.CampDefensive:
                                heroInfo.DefensiveQueueNum = 0;
                                heroInfo.DefensivePositionNum = 0;
                                break;
                            case HeroQueueType.CrossBattle:
                                heroInfo.CrossQueueNum = 0;
                                heroInfo.CrossPositionNum = 0;
                                break;
                            case HeroQueueType.ThemeBoss:
                                heroInfo.ThemeBossQueueNum = 0;
                                heroInfo.ThemeBossPositionNum = 0;
                                break;
                            case HeroQueueType.CrossBoss:
                                heroInfo.CrossBossQueueNum = 0;
                                heroInfo.CrossBossPositionNum = 0;
                                break;
                            case HeroQueueType.CarnivalBoss:
                                heroInfo.CarnivalBossQueueNum = 0;
                                heroInfo.CarnivalBossPositionNum = 0;
                                break;
                            case HeroQueueType.CrossChallenge:
                                heroInfo.CrossChallengeQueueNum = 0;
                                heroInfo.CrossChallengePositionNum = 0;
                                break;
                            default:
                                break;
                        }
                        heroPos.Remove(srcPosNum);
                    }
                }
            }
            return heroInfo;
        }

        public bool CheckDefQueueNum(int defQueueNum)
        {
            if (defQueueNum > 0 && DefensiveQueue.ContainsKey(defQueueNum))
            {
                return true;
            }
            return false;
        }

        public bool CheckDefPosNum(int defPosNum)
        {
            if (defPosNum > 0 && defPosNum < 10)
            {
                return true;
            }
            return false;
        }

        public HeroInfo GetHeroInfoByDefQueue(int defQueueNum, int defPosNum)
        {
            Dictionary<int, HeroInfo> heroPos;
            if (!DefensiveQueue.TryGetValue(defQueueNum, out heroPos))
            {
                return null;
            }
            HeroInfo heroInfo;
            if (!heroPos.TryGetValue(defPosNum, out heroInfo))
            {
                return null;
            }
            return heroInfo;
        }

        public int GetBattlePower(SortedDictionary<int, int> heroPos)
        {
            int battlePower = 0;
            foreach (var heroPo in heroPos)
            {
                HeroInfo heroInfo = GetHeroInfo(heroPo.Key);
                if(heroInfo == null) continue;
                battlePower += heroInfo.GetBattlePower();
            }

            return battlePower;
        }

        public int GetBattlePower()
        {
            int battlePower = 0;
            foreach (var heroPo in heroPos)
            {
                HeroInfo heroInfo = GetHeroInfo(heroPo.Key);
                if (heroInfo == null) continue;
                battlePower += heroInfo.GetBattlePower();
            }

            return battlePower;
        }


        public int GetBattlePower(HeroQueueType queueType)
        {
            int battlePower = 0;

            switch (queueType)
            {
                case HeroQueueType.CampDefensive:
                    battlePower = GetBattlePower(DefensiveQueue);
                    break;
                case HeroQueueType.CrossBattle:
                    battlePower = GetBattlePower(CrossQueue);
                    break;
                case HeroQueueType.CrossBoss:
                    battlePower = GetBattlePower(CrossBossQueue);
                    break;
                case HeroQueueType.ThemeBoss:
                    battlePower = GetBattlePower(ThemeBossQueue);
                    break;
                case HeroQueueType.CarnivalBoss:
                    battlePower = GetBattlePower(CarnivalBossQueue);
                    break;
                case HeroQueueType.CrossChallenge:
                    battlePower = GetBattlePower(CrossChallengeQueue);
                    break;
                default:
                    battlePower = CalcBattlePower();
                    break;
            }
            
            return battlePower;
        }

        public long GetBattlePower64(HeroQueueType queueType)
        {
            long battlePower = 0;

            switch (queueType)
            {
                case HeroQueueType.CrossBattle:
                    battlePower = GetBattlePower64(CrossQueue);
                    break;
                case HeroQueueType.CrossBoss:
                    battlePower = GetBattlePower(CrossBossQueue);
                    break;
                case HeroQueueType.CrossChallenge:
                    battlePower = GetBattlePower64(CrossChallengeQueue);
                    break;
                default:
                    battlePower = CalcBattlePower();
                    break;
            }

            return battlePower;
        }

        private int GetBattlePower(Dictionary<int, Dictionary<int, HeroInfo>> queue)
        {
            return queue.Select(x => x.Value.Values.Sum(item => item.GetBattlePower())).ToList().Sum();
        }

        private long GetBattlePower64(Dictionary<int, Dictionary<int, HeroInfo>> queue)
        {
            return queue.Select(x => x.Value.Values.Sum(item => (long)item.GetBattlePower())).ToList().Sum();
        }
    }
}
