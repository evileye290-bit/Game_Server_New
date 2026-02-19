using CommonUtility;
using DBUtility;
using Message.Gate.Protocol.GateC;
using ServerShared;
using System;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public class Chapter
    {
        private PlayerChar owner;
        private ChapterManager chapterManager;
        public int ChapterId { get; private set; }
        public List<int> BoxRewardList { get; private set; }
        public List<int> PassedDungeonIds { get; private set; }
        public List<int> StoryRewardList { get; private set; }

        public Chapter(ChapterManager manager, int chapterId)
        {
            owner = manager.Owner;
            chapterManager = manager;

            ChapterId = chapterId;
            BoxRewardList = new List<int>();
            PassedDungeonIds = new List<int>();
            StoryRewardList = new List<int>();
        }

        public void AddPassedDungeon(int dungeonId)
        {
            if (!PassedDungeonIds.Contains(dungeonId))
            {
                PassedDungeonIds.Add(dungeonId);
                SyncDBDungeon();
            }
        }

        public void AddBoxReward(int index)
        {
            if (!BoxRewardList.Contains(index))
            {
                BoxRewardList.Add(index);
                SyncDBBoxReward();
            }
        }

        public void AddStoryReward(int taskId)
        {
            if (!StoryRewardList.Contains(taskId))
            {
                StoryRewardList.Add(taskId);
                SyncDBStoryReward();
            }
        }

        public void GenerateChapterMsg(MSG_ZGC_CHAPTER_INFO msg)
        {
            msg.ChapterId = ChapterId;
            msg.BoxReward.AddRange(BoxRewardList);
            msg.DungeonIds.AddRange(PassedDungeonIds);
            msg.StoryReward.AddRange(StoryRewardList);

            int ms = (int)(chapterManager.TimeSpacePowerRecoryNextTime - ZoneServerApi.now).TotalMilliseconds;
            msg.PowerNextTime = ms;
        }


        #region DB

        public void SyncDBDungeon()
        {
            QueryUpdateChapterDungeonList query = new QueryUpdateChapterDungeonList(owner.Uid, ChapterId, PassedDungeonIds);
            owner.server.GameDBPool.Call(query);
        }

        public void SyncDBStoryReward()
        {
            QueryUpdateChapterStoryReward query = new QueryUpdateChapterStoryReward(owner.Uid, ChapterId, StoryRewardList);
            owner.server.GameDBPool.Call(query);
        }

        public void SyncDBBoxReward()
        {
            QueryUpdateChapterBoxReward query = new QueryUpdateChapterBoxReward(owner.Uid, ChapterId, BoxRewardList);
            owner.server.GameDBPool.Call(query);
        }

        #endregion

    }
}
