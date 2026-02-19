using CommonUtility;
using DataProperty;
using DBUtility;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using RedisUtility;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        //称号
        public HashSet<int> Titles = new HashSet<int>();

        public int CurTitleId { get; set; }

        public int FishCount { get; set; }

        public int NewTitle { get; set; }

        public void ChangeTitle(int titleId)
        {
            MSG_ZGC_CHANGE_TITLE_ANSWER response = new MSG_ZGC_CHANGE_TITLE_ANSWER();

            TitleInfo titleModel = TitleLibrary.GetTitleById(titleId);
            if (titleModel == null && titleId != 0)
            {
                Log.Warn($"player {Uid} change title failed not find title id info");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }

            if (!CheckCanChangeTitle(titleId))
            {
                Log.Warn($"player {Uid} change title failed");
                response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
            int power = HeroMng.CalcBattlePower();
            if (TitleMng.CurTitleId > 0)
            {
                TitleInfo currentTitle = TitleLibrary.GetTitleById(TitleMng.CurTitleId);
                if (currentTitle != null)
                {
                    KomoeEventLoguPlayerTitle(3, currentTitle.ObtainCondition, TitleMng.CurTitleId, power, power);
                }
            }
            TitleMng.ChangeTitle(titleId);

            response.Result = (int)ErrorCode.Success;
            response.CurTitle = TitleMng.CurTitleId;
            Write(response);

            MSG_GC_CHAR_SIMPLE_INFO simpleInfo = new MSG_GC_CHAR_SIMPLE_INFO();
            GetSimpleInfo(simpleInfo);
            BroadCast(simpleInfo);

            KomoeEventLoguPlayerTitle(2, titleModel.ObtainCondition, titleId, power, power);
        }

        public void SyncTitles()
        {
            SyncTitleToRedis();
            SyncTitlesToDB();
            SyncTitlesToClient();
        }

        public void SyncWithoutClient()
        {
            SyncTitleToRedis();
            SyncTitlesToDB();
        }

        public void SyncTitleToRedis()
        {
            server.GameRedis.Call(new OperateSetCurTitle(Uid, CurTitleId));
        }

        public void SyncTitlesToClient()
        {
            MSG_ZGC_TITLE_INFO info = GetTitleInfo();
            Write(info);
        }

        public void SyncTitlesToDB()
        {
            //string tableName = "title";

            QueryUpdateTitles queryTitle = new QueryUpdateTitles(Uid, GetTitleString(), CurTitleId, FishCount.ToString());

            server.GameDBPool.Call(queryTitle);
        }

        public void SyncFishCount()
        {
            SyncTitlesToDB();
            SyncTitlesToClient();
        }

        //public void CheckFishTitle(Dictionary<int, int> rewards)
        //{
        //    int originCount = FishCount;
        //    foreach (var tempItem in rewards)
        //    {
        //        int itemId = tempItem.Key;
        //        ulong uid = server.UID.NewEuid(server.MainId, server.SubId);
        //        NormalItem item = new NormalItem();
        //        if (item.Init(Uid, uid, itemId, tempItem.Value))
        //        {
        //            if (item.Type == ItemType.ConsumeItem && item.Id== 6 && item.Quality>=5)
        //            {
        //                FishCount += 1;
        //                break;
        //            }
        //        }
        //    }
        //    if (FishCount > originCount)
        //    {
        //        CheckTitles();
        //        SyncFishCount();
        //    }
        //}

        public static bool CheckBroadCastTitleInfo(int pcUid, double highestScore, double historyScore)
        {
            if (NeedBroadCast2Zone(highestScore, historyScore))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void BroadCastTitleInfo(int pcUid, double highestScore)
        {
            MSG_ZMZ_CHECK_TITLE checkTitleInfo = new MSG_ZMZ_CHECK_TITLE();
            checkTitleInfo.PcUid = pcUid;
            checkTitleInfo.HighestScore = (int)highestScore;
            PlayerChar targetplayer = server.PCManager.FindPcAnyway(pcUid);
            if (targetplayer == null)
            {
                server.ManagerServer.Write(checkTitleInfo, pcUid);
            }
            else
            {
                targetplayer.CheckTitles((int)highestScore);
            }
        }

        public static bool NeedBroadCast2Zone(double highestScore, double historyScore)
        {
            return TitleLibrary.NeedBroadCast(highestScore, historyScore);
        }

        public void CheckTitles(List<TitleInfo> titles)
        {
            bool changed = false;

            foreach (var info in titles)
            {
                switch (info.ObtainCondition)
                {
                    case (int)TitleObtainType.PopScore:
                        if (HighestPopScore >= info.ConditionNumber.ToInt())
                        {
                            Titles.Add(info.Id);
                            NewTitle = info.Id;
                            Logger.Log.Write("player {0} got new Title {1}", Uid, NewTitle);
                            changed = true;
                        }
                        break;
                    case (int)TitleObtainType.PopRank:
                        break;
                    //case (int)TitleObtainType.LadderLevel:
                    //    int ladderLevel = this.LadderLevel;
                    //    if (ladderLevel >= info.ConditionNumber)
                    //    {
                    //        Titles.Add(info.Id);
                    //        NewTitle = info.Id;
                    //        Logger.Log.Write("player {0} got new Title {1}", Uid, NewTitle);
                    //        changed = true;
                    //    }
                    //    break;
                    case (int)TitleObtainType.LadderRank:
                        break;
                    case (int)TitleObtainType.FishCount:
                        if (FishCount >= info.ConditionNumber.ToInt())
                        {
                            Titles.Add(info.Id);
                            NewTitle = info.Id;
                            Logger.Log.Write("player {0} got new Title {1}", Uid, NewTitle);
                            changed = true;
                        }
                        break;
                    default:
                        break;
                }
            }

            if (changed)
            {
                SyncTitles();
                NewTitle = 0;
            }
        }

        public void CheckTitles()
        {
            List<TitleInfo> infos = TitleLibrary.GetLeftTitles(Titles);

            CheckTitles(infos);

        }

        public void CheckTitles(TitleObtainType titleType)
        {
            List<TitleInfo> infos = TitleLibrary.GetTitleInType(titleType);

            CheckTitles(infos);
        }

        //物品使用查询绑定的称号检查
        public void CheckTitles(TitleObtainType title, int itemId)
        {
            List<TitleInfo> infos = TitleLibrary.GetTitleInType(title);

            bool changed = false;
            foreach (var info in infos)
            {
                switch (info.ObtainCondition)
                {
                    case (int)TitleObtainType.ItemUse:
                        //if (itemId == info.BindItem)
                        //{
                        //    Titles.Add(info.Id);
                        //    NewTitle = info.Id;
                        //    Logger.Log.Write("player {0} got new Title {1}", Uid, NewTitle);
                        //    changed = true;
                        //}
                        break;
                    default:
                        break;
                }
            }

            if (changed)
            {
                SyncTitles();
                NewTitle = 0;
            }

        }

        //检查当前的人气称号
        public void CheckTitles(int highestScore)
        {
            List<TitleInfo> infos = TitleLibrary.GetLeftTitles(Titles);
            //int ladderScore = this.GetLadderScore();
            //int ladderLevel = this.LadderLevel;
            bool changed = false;

            foreach (var info in infos)
            {
                switch (info.ObtainCondition)
                {
                    case (int)TitleObtainType.PopScore:
                        if (highestScore >= info.ConditionNumber.ToInt())
                        {
                            Titles.Add(info.Id);
                            NewTitle = info.Id;
                            Logger.Log.Write("player {0} got new Title {1}", Uid, NewTitle);
                            changed = true;
                        }
                        break;
                    default:
                        break;
                }
            }

            if (changed)
            {
                SyncTitles();
                NewTitle = 0;
            }
        }

        public bool AddTitle(int titleId)
        {
            if (Titles.Contains(titleId))
            {
                return false;
            }
            else
            {
                Titles.Add(titleId);
                return true;
            }
        }

        public MSG_ZGC_TITLE_INFO GetTitleInfo()
        {
            MSG_ZGC_TITLE_INFO info = new MSG_ZGC_TITLE_INFO();
            info.CurTitle = TitleMng.CurTitleId;
            foreach (var kv in TitleMng.TitleList)
            {
                switch (kv.Value.State)
                {
                    case TitleState.New:
                        info.NewTitles.Add(kv.Key);
                        break;
                    case TitleState.Activated:
                    case TitleState.Equiped:
                        info.Titles.Add(kv.Key);
                        break;
                    case TitleState.OverTime:
                        info.Titles.Add(kv.Key);
                        info.OverTimeTitles.Add(kv.Key);
                        break;
                    default:
                        break;
                }
            }           
            return info;
        }

        public MSG_ZGC_TITLE_INFO GetTitleInfoWhenLoading()
        {
            MSG_ZGC_TITLE_INFO info = new MSG_ZGC_TITLE_INFO();
            info.CurTitle = TitleMng.CurTitleId;          
            foreach (var kv in TitleMng.TitleList)
            {
                switch (kv.Value.State)
                {
                    case TitleState.New:
                        info.NewTitles.Add(kv.Key);
                        break;
                    case TitleState.Activated:
                        info.Titles.Add(kv.Key);
                        if (kv.Value.NeedSyncDb)
                        {
                            kv.Value.NeedSyncDb = false;
                            TitleMng.SyncDbUpdateTitleState(kv.Value);
                        }
                        break;
                    case TitleState.Equiped:
                        info.Titles.Add(kv.Key);                       
                        if (kv.Value.Id != TitleMng.CurTitleId)
                        {
                            kv.Value.State = TitleState.Activated;
                            TitleMng.SyncDbUpdateTitleState(kv.Value);
                        }
                        break;
                    case TitleState.OverTime:
                        info.Titles.Add(kv.Key);
                        info.OverTimeTitles.Add(kv.Key);
                        if (kv.Value.NeedSyncDb)
                        {
                            kv.Value.NeedSyncDb = false;
                            TitleMng.SyncDbUpdateTitleState(kv.Value);
                        }
                        break;
                    default:
                        break;
                }
            }
            return info;
        }

        public void LoadTitles(string titlesString)
        {
            if (!string.IsNullOrEmpty(titlesString))
            {
                string[] titles = titlesString.Split(':');
                foreach (string item in titles)
                {
                    if (item != null && item != "")
                    {
                        int temp = int.Parse(item);
                        Titles.Add(temp);
                    }
                }
            }
        }

        public string GetTitleString()
        {
            StringBuilder titles = new StringBuilder("");
            List<int> titleList = Titles.ToList<int>();
            titleList.Sort();
            for (int i = 0; i < titleList.Count; i++)
            {
                titles.Append(titleList[i]);
                if (i != titleList.Count)
                {
                    titles.Append(":");
                }
            }

            return titles.ToString();
        }

        public void LoadFishCount(string count)
        {
            if (!string.IsNullOrEmpty(count))
            {
                FishCount = int.Parse(count);
            }
        }     
     
        //-----------------------------------------------

        public TitleManager TitleMng { get; set; }

        public void InitTitleManager()
        {
            TitleMng = new TitleManager(this);
        }

        public void InitTitleInfo(List<DbTitleItem> titleList)
        {
            TitleMng.InitTitleInfo(titleList);
        }

        public void SendTitleInfo()
        {
            MSG_ZGC_TITLE_INFO info = GetTitleInfo();
            Write(info);
        }

        public void SendTitleInfoWhenLoading()
        {
            MSG_ZGC_TITLE_INFO info = GetTitleInfoWhenLoading();
            Write(info);
        }

        /// <summary>
        /// 激活称号
        /// </summary>
        public void ActivateTitle(int itemId)
        {
            int titleId = TitleLibrary.GetTitleIdByItemId(itemId);
            //if (titleId == 0)
            //{
            //    Log.Warn($"player {Uid} activate title failed: not find title in xml, title card itemId {itemId}");
            //    return;
            //}

            if (TitleMng.ActivateTitle(titleId))
            {

                //红点通知
                NotifyGetNewTitle(titleId);
                //属性加成
                TitleInfo title = TitleLibrary.GetTitleById(titleId);
                if (title == null)
                {
                    return;
                }
                int power = HeroMng.CalcBattlePower();


                TitleMng.AddNature(title);
                Dictionary<int, HeroInfo> heroInfoList = HeroMng.GetHeroInfoList();
                if (heroInfoList.Count > 0)
                {
                    foreach (var hero in heroInfoList)
                    {
                        //称号加成
                        foreach (var nature in title.NatureList)
                        {
                            hero.Value.AddNatureAddedValue((NatureType)nature.Key, nature.Value);
                        }
                        HeroMng.UpdateBattlePower(hero.Value);
                    }
                    //战力变动通知
                    HeroMng.NotifyClientBattlePower();
                }


                TitleInfo titleModel = TitleLibrary.GetTitleById(titleId);
                if (titleModel != null)
                {
                    KomoeEventLoguPlayerTitle(1, titleModel.ObtainCondition, titleId, power, HeroMng.CalcBattlePower());
                }
            }
        }

        private void NotifyGetNewTitle(int titleId)
        {
            MSG_ZGC_NEW_TITLE notify = new MSG_ZGC_NEW_TITLE();
            notify.TitleId = titleId;
            Write(notify);
        }

        public bool CheckCanChangeTitle(int titleId)
        {
            if (titleId == 0 && TitleMng.CurTitleId == 0)
            {
                return false;
            }
            TitleInfoItem title;
            if (!TitleMng.TitleList.TryGetValue(titleId, out title) && titleId != 0)
            {
                return false;
            }
            if (title != null)
            {
                if (title.State == TitleState.Equiped || title.Id == TitleMng.CurTitleId)
                {
                    return false;
                }
                TitleInfo titleModel = TitleLibrary.GetTitleById(titleId);
                switch ((TitleObtainCondition)titleModel.ObtainCondition)
                {
                    case TitleObtainCondition.CampLeader:
                        if (!CheckHasCampLeaderTitle(titleModel.SubType))
                        {
                            return false;
                        }
                        break;
                    case TitleObtainCondition.ArenaRankFirst:
                        if (titleModel.SubType == 1 && ArenaMng.Rank != 1)
                        {
                            return false;
                        }
                        break;
                    case TitleObtainCondition.CrossBattleFirst:
                        if (titleModel.SubType == 1 && CrossInfoMng.Info.LastFinalsRank != 1)
                        {
                            return false;
                        }
                        break;
                    default:
                        break;
                }
            }
            return true;
        }
     
        public void GetTitleConditionCount(int titleId)
        {
            MSG_ZGC_TITLE_CONDITION_COUNT response = new MSG_ZGC_TITLE_CONDITION_COUNT();
            TitleInfoItem title;
            if (!TitleMng.TitleList.TryGetValue(titleId, out title))
            {
                //Log.Warn($"player {Uid} get title condition count failed: not have title {titleId}");
                //response.Result = (int)ErrorCode.Fail;
                Write(response);
                return;
            }
            response.TitleId = title.Id;
            response.Count = title.FinishCount;
            Write(response);
        }

        public void LookTitle(int titleId)
        {
            MSG_ZGC_LOOK_TITLE response = new MSG_ZGC_LOOK_TITLE();
            TitleInfo title = TitleLibrary.GetTitleById(titleId);
            if (title == null)
            {
                Log.Warn($"player {Uid} look titile failed: not find title {titleId} in xml");
                return;
            }

            TitleMng.ChangeTitleState(titleId);
            response.TitleId = titleId;
            Write(response);
        }
                     
        public void CheckGetTitleConditionEquipment(EquipmentItem equipment)
        {
            List<TitleInfo> titleList = TitleLibrary.GetTitleListByCondition(TitleObtainCondition.GetEquipmentSuit);
            if (titleList == null)
            {
                return;
            }
            foreach (var item in titleList)
            {
                if (!string.IsNullOrEmpty(item.ConditionNumber))
                {
                    string[] conditonParam = StringSplit.GetArray("|", item.ConditionNumber);
                    if (conditonParam.Length < 2)
                    {
                        continue;
                    }
                    List<int> paramList = new List<int>();
                    paramList.Add(equipment.Model.Grade);
                    TitleMng.UpdateTitleConditionCount(TitleObtainCondition.GetEquipmentSuit, 1, paramList);
                }
            }
        }

        public void CheckSendThemeBossTitle()
        {
            List<int> paramList = new List<int>() { ThemeBossManager.Level };
            TitleMng.UpdateTitleConditionCount(TitleObtainCondition.FirstActivityHunting, 1, paramList);
        }

        public bool CheckHasCampLeaderTitle(int titleSubType)
        {
            foreach (var kv in server.RelationServer.TianDouWorship)
            {
                if (kv.Value.Uid == Uid && kv.Value.Rank == titleSubType)
                {
                    return true;
                }
            }
            foreach (var kv in server.RelationServer.XingLuoWorship)
            {
                if (kv.Value.Uid == Uid && kv.Value.Rank == titleSubType)
                {
                    return true;
                }
            }
            return false;
        }

        public void CheckCurTitle()
        {
            TitleMng.CheckCurTitle();
        }

        public bool CheckHasThisTitleCard(int itemId, int subType)
        {
            if ((ConsumableType)subType != ConsumableType.TitleCard)
            {
                return false;
            }
            int titleId = TitleLibrary.GetTitleIdByItemId(itemId);
            TitleMng.CheckNeedUpdateTitleState(titleId);
            BaseItem item = BagManager.GetItem(MainType.Consumable, itemId);
            if (item != null)
            {
                return true;
            }
            if (TitleMng.CheckHasThisTitleCard(titleId))
            {
                return true;
            }
            return false;
        }       

        public MSG_ZMZ_TITLE_INFO GetTitlesTransform()
        {
            MSG_ZMZ_TITLE_INFO msg = TitleMng.GetTitlesTransform();
            return msg;
        }

        public void LoadTtilesTransform(MSG_ZMZ_TITLE_INFO info)
        {
            TitleMng.LoadTitlesTransform(info);
        }
    }
}
