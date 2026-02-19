using DBUtility.Sql;
using ServerShared;
using System;

namespace ZoneServerLib
{
    public class WorldLevelManager
    {
        private ZoneServerApi server;
        private int serverLevel = 1;//
        private int currLevelDays = 0;//当前level的第几天
        private int chapterOpenId = 1;
        private DateTime lastDay = ZoneServerApi.now.Date;

        /// <summary>
        /// 当前服务器章节
        /// </summary>
        public int ChapterOpenId => chapterOpenId;

        /// <summary>
        /// 当前服务器等级
        /// </summary>
        public int ServerLevel => serverLevel;

        public int CurrLevelDays => currLevelDays;


        public void Init(ZoneServerApi api)
        { 
            server = api;
            CaculateServerLevel();
            //LoadServerLevel();
        }

        public bool CheckChapterOpend(int chapter)
        {
            return ChapterOpenId >= chapter;
        }

        public void Update()
        {
            DateTime today = ZoneServerApi.now.Date;
            if (lastDay < today)
            {
                lastDay = today;
                CaculateServerLevel();
            }
        }

        //private void LoadServerLevel()
        //{
        //    QueryLoadServerLevel query = new QueryLoadServerLevel();
        //    server.GameDBPool.Call(query, result =>
        //     {
        //         if ((int)result == 1)
        //         {
        //             serverLevel = query.serverLevel;
        //             currLevelDays = query.days;

        //             CaculateServerLevel();
        //         }
        //     });
        //}

        private void CaculateServerLevel()
        {
            DateTime now = ZoneServerApi.now.Date;
            int openServerDays = (int)(now - server.OpenServerTime.Date).TotalDays;

            int level = 1, day = 0;
            WorldLevelLibrary.GetServerLevel(openServerDays, ref level, ref day);
            if (serverLevel != level || currLevelDays != day)
            {
                serverLevel = level;
                currLevelDays = day;
            }
            chapterOpenId = WorldLevelLibrary.GetChapterId(serverLevel);
        }

        //private void SyncDBServerLevel()
        //{
        //    QueryUpdateServerLevel query = new QueryUpdateServerLevel(serverLevel, currLevelDays);
        //    server.GameDBPool.Call(query);
        //}
    }
}
