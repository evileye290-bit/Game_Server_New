using EnumerateUtility;
using RedisUtility;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrossServerLib
{
    public class CrossBossRankManager
    {
        private CrossServerApi server { get; set; }

        public Dictionary<int, CrossBossSiteRank> SiteList = new Dictionary<int, CrossBossSiteRank>();

        public Dictionary<int, CrossBossChapterRank> ChapterList = new Dictionary<int, CrossBossChapterRank>();

        public CrossBossRankManager(CrossServerApi server)
        {
            this.server = server;
        }

        public void AddSiteRank(int group, int siteId)
        {
            CrossBossSiteRank siteRank = new CrossBossSiteRank(server);
            siteRank.Init(group, siteId);
            //siteRank.LoadInitRankFromRedis();
            SiteList.Add(siteId, siteRank);
        }

        public CrossBossSiteRank GetSiteRank(int siteId)
        {
            CrossBossSiteRank value;
            SiteList.TryGetValue(siteId, out value);
            return value;
        }

        public void AddChapterRank(int group, int chapterId)
        {
            CrossBossChapterRank chapterRank = new CrossBossChapterRank(server);
            chapterRank.Init(group, chapterId);
            //chapterRank.LoadInitRankFromRedis();
            ChapterList.Add(chapterId, chapterRank);
        }

        public CrossBossChapterRank GetChapterRank(int chapterId)
        {
            CrossBossChapterRank value;
            ChapterList.TryGetValue(chapterId, out value);
            return value;
        }

        public void CheckRankScore()
        {
            foreach (var chapterRank in ChapterList)
            {
                Dictionary<int, RankBaseModel> uidRankInfoDic = chapterRank.Value.GetUidRankInfos();
                foreach (var uidRank in uidRankInfoDic)
                {
                    int uid = uidRank.Value.Uid;
                    int score = uidRank.Value.Score;
                    int totalScore = GetSiteTotalScore(uid);
                    if (score != totalScore)
                    {
                        Logger.Log.Warn($"player {uid} CheckRankScore old is {score} new {totalScore}");
                        chapterRank.Value.SetScore(uid, totalScore);
                    }
                }
                chapterRank.Value.sort();
            }
        }

        private int GetSiteTotalScore(int uid)
        {
            int totalScore = 0;
            foreach (var siteRank in SiteList)
            {
                RankBaseModel info = siteRank.Value.GetRankBaseInfo(uid);
                if (info != null)
                {
                    totalScore += info.Score;
                }
            }
            return totalScore;
        }
    }
}
