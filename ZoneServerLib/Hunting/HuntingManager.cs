using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Google.Protobuf.Collections;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using Message.Zone.Protocol.ZM;
using Message.Zone.Protocol.ZR;
using RedisUtility;
using ServerFrame;
using ServerModels;
using ServerShared;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    public class HuntingManager
    {
        private static readonly string infoLinkStr = "|";

        private bool continueHunting = false;
        private List<int> huntingList = new List<int>();

        public PlayerChar Owner { get; private set; }
        public int Research { get; set; }
        public List<int> HuntingList { get { return huntingList; } }
        public bool ContinueHunting { get { return continueHunting; } }

        #region 凶兽森林-猎杀魂兽

        private List<int> huntingActivityPassList = new List<int>();
        private List<int> huntingActivityUnlockList = new List<int>();

        public List<int> HuntingActivityList => huntingActivityPassList;
        public List<int> HuntingActivityUnlockList => huntingActivityUnlockList;

        #endregion

        public HuntingManager(PlayerChar owner)
        {
            this.Owner = owner;
        }

        public bool CheckPassed(int id)
        {
            return HuntingList.Contains(id);
        }

        public bool CheckPassedByDungeonId(int dungeonId)
        {
            HuntingModel model =HuntingLibrary.GetByMapId(dungeonId);
            if (model == null) return false;

            return CheckPassed(model.Id);
        }

        public void AddResearch(int value, bool sync = true)
        {
            if (Research >= HuntingLibrary.ResearchMax) return;

            Research += value;

            if (Research >= HuntingLibrary.ResearchMax)
            {
                Research = HuntingLibrary.ResearchMax;
            }

            if (sync)
            {
                SyncHuntingResearch();
            }
        }

        public void AddPassedId(int id)
        {
            if (!HuntingList.Contains(id))
            {
                HuntingList.Add(id);
                SyncUpdateHuntingInfoToDB();
            }
        }

        public void BindHuntingInfo(int research, string info, string unlock, string passed)
        {
            Research = research;
            huntingList = info.ToList('|');
            huntingActivityPassList = passed.ToList('|');
            huntingActivityUnlockList = unlock.ToList('|');
        }

        public bool ChangeHuntingState(bool isContinue)
        {
            if (continueHunting && isContinue)
            {
                return false;
            }
            if (!continueHunting && !isContinue)
            {
                return false;
            }
            continueHunting = isContinue;
            return true;
        }

        public void ReSetContinueState()
        {
            continueHunting = false;
        }

        #region 凶兽森林-猎杀魂兽

        public void AddActivityUnlocked(int id)
        {
            if (!huntingActivityUnlockList.Contains(id))
            {
                huntingActivityUnlockList.Add(id);
                SyncUpdateHuntingActivityUnlockInfoToDB();
            }
        }

        public void AddActivityPassed(int id)
        {
            if (!huntingActivityPassList.Contains(id))
            {
                huntingActivityPassList.Add(id);
                SyncUpdateHuntingActivityPassedInfoToDB();
            }
        }

        public bool IsActivityUnlocked(int id)
        {
            return huntingActivityUnlockList.Contains(id);
        }

        public bool IsActivityPassed(int id)
        {
            return huntingActivityPassList.Contains(id);
        }

        public bool CheckActivityPassedByDungeonId(int dungeonId)
        {
            HuntingActivityModel model = HuntingLibrary.GetHuntingActivityModelByMapId(dungeonId);
            if (model == null) return false;

            return IsActivityPassed(model.Id);
        }

        public bool CheckHuntingActivityUnlocked(int dungeonId)
        {
            //普通本不需要检测
            HuntingModel huntingModel = HuntingLibrary.GetByMapId(dungeonId);
            if (huntingModel != null) return false;

            HuntingActivityModel model = HuntingLibrary.GetHuntingActivityModelByMapId(dungeonId);
            if (model == null) return true;
                
            return !huntingActivityUnlockList.Contains(model.Id);
        }

        #endregion

        private void SyncUpdateHuntingInfoToDB()
        {
            Owner.server.GameDBPool.Call(new QueryUpdateHuntingInfo(Owner.Uid, Research, huntingList.ToString(infoLinkStr)));
        }

        private void SyncUpdateHuntingActivityUnlockInfoToDB()
        {
            Owner.server.GameDBPool.Call(new QueryUpdateHuntingActivityUnlockInfo(Owner.Uid, huntingActivityUnlockList.ToString(infoLinkStr)));
        }

        private void SyncUpdateHuntingActivityPassedInfoToDB()
        {
            Owner.server.GameDBPool.Call(new QueryUpdateHuntingActivityPassedInfo(Owner.Uid, Research, huntingActivityPassList.ToString(infoLinkStr)));
        }

        public MSG_ZGC_HUNTING_INFO GenerateHuntingMsg()
        {
            int week = HuntingLibrary.GetWeekIndex(BaseApi.now);
            MSG_ZGC_HUNTING_INFO msg = new MSG_ZGC_HUNTING_INFO
            {
                Week = week,
                Researsh = Research
            };
            msg.PassedList.AddRange(huntingList);
            msg.ActivityPassedList.AddRange(huntingActivityPassList);
            msg.ActivityUnlockList.AddRange(huntingActivityUnlockList);
            msg.RevertNextTime = Timestamp.GetUnixTimeStampSeconds(Owner.GetRecoryTime(CounterType.HuntingCount));

            return msg;
        }

        public MSG_ZMZ_HUNTING_INFO GenerateTransformMsg()
        {
            MSG_ZMZ_HUNTING_INFO msg = new MSG_ZMZ_HUNTING_INFO();
            msg.Researsh = Research;
            msg.PassedList.AddRange(huntingList);
            msg.ActivityPassedList.AddRange(huntingActivityPassList);
            msg.ActivityUnlockList.AddRange(huntingActivityUnlockList);
            msg.ContinueHunting = ContinueHunting;
            msg.HuntingIntrude = GenerateHuntingIntrudeTransformMsg();
            return msg;
        }

        public void LoadFromTransform(MSG_ZMZ_HUNTING_INFO info)
        {
            Research = info.Researsh;
            huntingList.AddRange(info.PassedList);
            huntingActivityPassList.AddRange(info.ActivityPassedList);
            huntingActivityUnlockList.AddRange(info.ActivityUnlockList);
            continueHunting = info.ContinueHunting;

            LoadHuntingIntrudeFromTransform(info.HuntingIntrude);
        }

        private void SyncHuntingResearch()
        {
            MSG_ZR_LEVEL_UP notifyRelation = new MSG_ZR_LEVEL_UP();
            notifyRelation.Uid = Owner.Uid;
            notifyRelation.Level = Owner.Level;
            notifyRelation.Research = Research;
            notifyRelation.Chapter = Owner.ChapterId;
            Owner.server.SendToRelation(notifyRelation);

            Owner.server.GameDBPool.Call(new QueryUpdateHuntingResearch(Owner.Uid, Research));
            Owner.server.GameRedis.Call(new OperateUpdateRankScore(RankType.Hunting, Owner.server.MainId, Owner.Uid, Research, Owner.server.Now()));

            Owner.SerndUpdateRankValue(RankType.Hunting, Research);

            Owner.server.GameRedis.Call(new OperateHuntingResearch(Owner.Uid, Research));
        }



        #region 凶兽入侵

        private List<HuntingIntrudeInfo> huntingIntrudeInfos;
        private Dictionary<int, int> huntingIntrudeHeroPos;
        public Dictionary<int, int> HuntingIntrudeHeroPos => huntingIntrudeHeroPos;

        public void InitHuntingIntrudeInfo(string heroPos, List<HuntingIntrudeInfo> infos)
        {
            huntingIntrudeInfos = infos;
            huntingIntrudeHeroPos = heroPos.ToDictionary('|',':');
            CheckHuntingIntrudeOutOfTime(false);
        }

        public void CheckHuntingIntrudeOutOfTime(bool sync = true)
        {
            if (huntingIntrudeInfos.Count == 0) return;

            var list = huntingIntrudeInfos.Where(x => x.EndTime <= BaseApi.now).ToList();
            if (list.Count == 0) return;

            foreach (var info in list)
            {
                huntingIntrudeInfos.Remove(info);
                SyncDeleteHuntingIntrudeInfoToDB(info.Id);
            }

            if (sync)
            {
                Owner.SendHuntingIntrudeInfo();
            }
        }

        public bool CheckCountLimit()
        {
            return huntingIntrudeInfos.Count >= HuntingLibrary.HuntingIntrudeNum;
        }

        public HuntingIntrudeInfo GetHuntingIntrudeInfo(ulong id)
        {
            return huntingIntrudeInfos.FirstOrDefault(x => x.Id == id);
        }


        public void RemoveHuntingIntrudeInfo(HuntingIntrudeInfo info)
        {
            huntingIntrudeInfos.Remove(info);
            SyncDeleteHuntingIntrudeInfoToDB(info.Id);
            Owner.SendHuntingIntrudeInfo();
        }

        public void AddHuntingIntrudeInfo(HuntingIntrudeInfo info)
        {
            huntingIntrudeInfos.Add(info);
            SyncInsertHuntingIntrudeInfoToDB(info);
        }

        public void UpdateHeroPosInfo(RepeatedField<MSG_GateZ_HERO_POS> heroPos)
        {
            huntingIntrudeHeroPos.Clear();
            heroPos.ForEach(h => huntingIntrudeHeroPos[h.HeroId] = h.PosId);
            SyncUpdateHeroPosToDB();
        }

        public MSG_ZGC_HUNTING_INTRUDE_INFO GenerateHuntingIntrudeMsg()
        {
            MSG_ZGC_HUNTING_INTRUDE_INFO msg = new MSG_ZGC_HUNTING_INTRUDE_INFO();
            foreach (var kv in huntingIntrudeInfos)
            {
                msg.InfoList.Add(new MSG_HUNTING_INTRUDE_INFO()
                {
                    Id = new MSG_ZGC_ITEM_ID()
                    {
                        UidLow = kv.Id.GetLow(),
                        UidHigh = kv.Id.GetHigh(),
                    },
                    IntrudeId = kv.IntrudeId,
                    LimitJob = kv.JobLimit,
                    BuffSuitId = kv.BuffSuitId,
                    EndTime = Timestamp.GetUnixTimeStampSeconds(kv.EndTime),
                });
            }

            GenerateHuntignIntrudeHeroPosMsg(msg.HeroPos);

            return msg;
        }

        public void GenerateHuntignIntrudeHeroPosMsg(RepeatedField<MSG_ZGC_HERO_POS> heroPos)
        {
            huntingIntrudeHeroPos.ForEach(h => heroPos.Add(new MSG_ZGC_HERO_POS() { HeroId = h.Key, PosId = h.Value }));
        }

        public MSG_ZMZ_HUNTING_INTRUDE_INFO GenerateHuntingIntrudeTransformMsg()
        {
            MSG_ZMZ_HUNTING_INTRUDE_INFO msg = new MSG_ZMZ_HUNTING_INTRUDE_INFO();
            foreach (var kv in huntingIntrudeInfos)
            {
                msg.InfoList.Add(new MSG_HUNTING_INTRUDE_INFO1()
                {
                    Id = kv.Id,
                    IntrudeId = kv.IntrudeId,
                    LimitJob = kv.JobLimit,
                    BuffSuitId= kv.BuffSuitId,
                    EndTime = Timestamp.GetUnixTimeStampSeconds(kv.EndTime),
                });
            }

            huntingIntrudeHeroPos.ForEach(h => msg.HeroPos.Add(new MSG_ZMZ_HERO_POS() { HeroId = h.Key, PosId = h.Value }));
            return msg;
        }

        public void LoadHuntingIntrudeFromTransform(MSG_ZMZ_HUNTING_INTRUDE_INFO info)
        {
            huntingIntrudeHeroPos = new Dictionary<int, int>();
            huntingIntrudeInfos = new List<HuntingIntrudeInfo>();
            info.InfoList.ForEach(x => huntingIntrudeInfos.Add(new HuntingIntrudeInfo()
            {
                Id = x.Id,
                IntrudeId = x.IntrudeId,
                Uid = Owner.Uid,
                JobLimit = x.LimitJob,
                BuffSuitId= x.BuffSuitId,
                EndTime = Timestamp.TimeStampToDateTime(x.EndTime),
            }));

            info.HeroPos.ForEach(x => huntingIntrudeHeroPos.Add(x.HeroId, x.PosId));
        }


        public void SyncInsertHuntingIntrudeInfoToDB(HuntingIntrudeInfo info)
        {
            Owner.server.GameDBPool.Call(new QueryInsertHuntingIntrude(info));
        }

        public void SyncDeleteHuntingIntrudeInfoToDB(ulong id)
        {
            Owner.server.GameDBPool.Call(new QueryDeleteHuntingIntrude(id));
        }

        public void SyncUpdateHeroPosToDB()
        {
            string str = huntingIntrudeHeroPos.Count == 0 ? string.Empty : string.Join("|",huntingIntrudeHeroPos.Select(x => $"{x.Key}:{x.Value}"));
            Owner.server.GameDBPool.Call(new QueryUpdateHuntingIntrudeHeroPos(Owner.Uid, str));
        }

        #endregion

    }
}
