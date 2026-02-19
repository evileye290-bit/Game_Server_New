using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    public class ChapterManager
    {
        private Dictionary<int, Chapter> chapterList = new Dictionary<int, Chapter>();
        private DateTime timeSpacePowerRecoryLastTime;
        private DateTime timeSpacePowerRecoryNextTime;
        public DateTime TimeSpacePowerRecoryNextTime => timeSpacePowerRecoryNextTime;

        public PlayerChar Owner { get; private set; }
        public int TimeSpacePowerUpperLimit { get; private set; }

        public ChapterManager(PlayerChar player)
        {
            Owner = player;
        }

        public void Init(Dictionary<int, ChapterDBInfo> chapterList, DateTime powerRecoryLastTime, int powerLimit)
        {
            TimeSpacePowerUpperLimit = powerLimit;

            foreach (var kv in chapterList)
            {
                Chapter chapter = new Chapter(this, kv.Key);
                chapter.BoxRewardList.AddRange(kv.Value.BoxRewardList);
                chapter.PassedDungeonIds.AddRange(kv.Value.PassedDungeonIds);
                chapter.StoryRewardList.AddRange(kv.Value.StoryRewardList);
                AddChapter(chapter);
            }

            CaculateOfflineNeedAddPower(powerRecoryLastTime);
        }

        public Chapter GetChapter(int chapterId)
        {
            Chapter chapter;
            chapterList.TryGetValue(chapterId, out chapter);
            return chapter;
        }

        public Chapter AddChapter(int chapterId)
        {
            Chapter chapter = new Chapter(this, chapterId);
            AddChapter(chapter);
            SyncDBInsertChapter(chapterId);
            return chapter;
        }

        public void AddChapter(Chapter chapter)
        {
            chapterList[chapter.ChapterId] = chapter;
        }

        /// <summary>
        /// 检查是否有可以领取的奖励
        /// </summary>
        public MSG_ZGC_CHAPTER_REWATRD_REDDOT GetReward()
        {
            MSG_ZGC_CHAPTER_REWATRD_REDDOT msg = new MSG_ZGC_CHAPTER_REWATRD_REDDOT();
            foreach (var kv in chapterList)
            {
                //是否有没有领取的奖励
                List<StoryLineModel> storylineReward = ChapterLibrary.GetStoryLineRewardIds(kv.Key);
                if (storylineReward != null)
                {
                    StoryLineModel model = storylineReward.Where(x => !kv.Value.StoryRewardList.Contains(x.Id) && Owner.CheckTaskFinished(x.TaskId)).FirstOrDefault();
                    if (model != null)
                    { 
                        msg.Chapter.Add(kv.Key);
                        continue;
                    }
                }

                //是否有没有领取的箱子
                var boxReward = ChapterLibrary.GetBoxRewardIds(kv.Key);
                if (boxReward != null)
                {
                    StoryRewardModel model = boxReward.Where(x => !kv.Value.BoxRewardList.Contains(x.Id) && Owner.CheckTaskFinished(x.TaskId)).FirstOrDefault();
                    if (model != null)
                    {
                        msg.Chapter.Add(kv.Key);
                        continue;
                    }
                }
            }
            return msg;
        }

        public bool HadPassedDungeon(int dungeonId)
        {
            StoryLineModel storyLine = ChapterLibrary.GetStoryLineModelByDungeonId(dungeonId);
            if (storyLine == null)
            {
                return false;
            }
            Chapter chapter = GetChapter(storyLine.Chapter);
            return chapter != null && chapter.PassedDungeonIds.Contains(dungeonId);
        }

        public void Update()
        {
            if (timeSpacePowerRecoryNextTime <= ZoneServerApi.now)
            {
                CheckAndAddPowerByTimeRecory(1);
                SetPowerRecoryTime(ZoneServerApi.now);
            }
        }

        public void CheckPowerLimit(int taskId)
        {
            int power = ChapterLibrary.GetPowerLimit(taskId);
            if (power > TimeSpacePowerUpperLimit)
            {
                TimeSpacePowerUpperLimit = power;
                SyncDBPowerLimit(power);
            }
        }

        private void CaculateOfflineNeedAddPower(DateTime powerRecoryLastTime)
        {
            DateTime now = ZoneServerApi.now;

            //超过30天没有上过线
            if ((now - powerRecoryLastTime).TotalDays > 30)
            {
                SetPowerRecoryTime(now);
                CheckAndAddPowerByTimeRecory(TimeSpacePowerUpperLimit);
                return;
            }

            double elapseTime = (now - powerRecoryLastTime).TotalSeconds;
            if (elapseTime > ChapterLibrary.PowerRecover)
            {
                int recoryCount = (int)elapseTime / ChapterLibrary.PowerRecover;//可以回复的次数
                double restTime = elapseTime % ChapterLibrary.PowerRecover;

                CheckAndAddPowerByTimeRecory(recoryCount);
                SetPowerRecoryTime(now.AddSeconds(restTime * -1));
            }
            else
            {
                //离线时间不到回复一次的
                SetPowerRecoryTime(powerRecoryLastTime);
            }
        }

        private void SetPowerRecoryTime(DateTime time)
        {
            timeSpacePowerRecoryLastTime = time;
            timeSpacePowerRecoryNextTime = time.AddSeconds(ChapterLibrary.PowerRecover);
        }

        private void CheckAndAddPowerByTimeRecory(int num)
        {
            int currNum = Owner.GetCoins(CurrenciesType.spaceTimePower);
            if (currNum >= TimeSpacePowerUpperLimit)
            {
                return;
            }
            else if (currNum + num > TimeSpacePowerUpperLimit)
            {
                num = TimeSpacePowerUpperLimit - currNum;
            }

            Owner.AddCoins(CurrenciesType.spaceTimePower, num, ObtainWay.TimeSpacePowerRecory);
        }

        public ErrorCode CheckCreateDungeon(DungeonModel model)
        {
            if (Owner.Team != null)
            {
                return ErrorCode.InTeam;
            }

            if (Owner.GetCoins(CurrenciesType.spaceTimePower) < model.Power)
            {
                return ErrorCode.SpaceTimePowerNotEnough;
            }

            StoryLineModel storyLine =  ChapterLibrary.GetStoryLineModelByDungeonId(model.Id);
            if (model == null)
            {
                return ErrorCode.CreateDungeonFailed;
            }

            if (!Owner.CheckTaskFinished(storyLine.TaskId))
            {
                return ErrorCode.EarlierTaskNotFinished;
            }

            if (!chapterList.ContainsKey(storyLine.Chapter))
            {
                return ErrorCode.ChapterNotOpen; 
            }

            return ErrorCode.Success;
        }

        public void SyncDBPowerRecoryTime()
        {
            QueryUpdatePowerRecoryTime query = new QueryUpdatePowerRecoryTime(Owner.Uid, timeSpacePowerRecoryLastTime);
            Owner.server.GameDBPool.Call(query);
        }

        private void SyncDBPowerLimit(int limit)
        {
            QueryUpdatePowerLimit query = new QueryUpdatePowerLimit(Owner.Uid, limit);
            Owner.server.GameDBPool.Call(query);
        }

        private void SyncDBInsertChapter(int chapterId)
        {
            QueryInsertChapter query = new QueryInsertChapter(Owner.Uid, chapterId);
            Owner.server.GameDBPool.Call(query);
        }

        public MSG_ZMZ_CHAPTER_INFO GenerateTransformMsg()
        {
            MSG_ZMZ_CHAPTER_INFO msg = new MSG_ZMZ_CHAPTER_INFO();
            msg.TimeSpacePowerRecoryLastTime = Timestamp.GetUnixTimeStampSeconds(timeSpacePowerRecoryLastTime);
            msg.TimeSpacePowerUpperLimit = TimeSpacePowerUpperLimit;
            chapterList.Values.ForEach(x => msg.ChapterList.Add(GenerateChapterMsg(x)));
            return msg;
        }

        private ZMZ_CHAPTER GenerateChapterMsg(Chapter chapter)
        {
            ZMZ_CHAPTER msg = new ZMZ_CHAPTER();
            msg.ChapterId = chapter.ChapterId;
            msg.BoxRewardList.AddRange(chapter.BoxRewardList);
            msg.PassedDungeonIds.AddRange(chapter.PassedDungeonIds);
            msg.StoryRewardList.AddRange(chapter.StoryRewardList);
            return msg;
        }

        public void LoadTransform(MSG_ZMZ_CHAPTER_INFO info)
        {
            DateTime powerRecoryLastTime = Timestamp.TimeStampToDateTime(info.TimeSpacePowerRecoryLastTime);
            TimeSpacePowerUpperLimit = info.TimeSpacePowerUpperLimit;
            foreach (var item in info.ChapterList)
            {
                Chapter chapter = new Chapter(this, item.ChapterId);
                chapter.BoxRewardList.AddRange(item.BoxRewardList);
                chapter.PassedDungeonIds.AddRange(item.PassedDungeonIds);
                chapter.StoryRewardList.AddRange(item.StoryRewardList);
                AddChapter(chapter);

                CaculateOfflineNeedAddPower(powerRecoryLastTime);
            }
        }
    }
}
