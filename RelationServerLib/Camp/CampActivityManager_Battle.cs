using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Google.Protobuf.Collections;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Relation.Protocol.RZ;
using Message.Zone.Protocol.ZR;
using RedisUtility;
using ServerModels;
using ServerModels.Monster;
using ServerShared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelationServerLib
{

    public partial class CampActivityManager
    {
        /// <summary>
        /// 当前阶段（开荒为1，决战为2，休息为0）
        /// </summary>
        public CampBattleStep CurCampBattleStep = CampBattleStep.Rest;

        /// <summary>
        /// 所有要塞
        /// </summary>
        public Dictionary<int, CampFort> AllForts = new Dictionary<int, CampFort>();

        /// <summary>
        /// key player uid 占领数据
        /// </summary>
        public Dictionary<int, Dictionary<int,CampFort>> playerHoldForts = new Dictionary<int, Dictionary<int, CampFort>>();

        /// <summary>
        /// 占领情况
        /// </summary>
        public Dictionary<CampType, int> campHoldCount = new Dictionary<CampType, int>();

        /// <summary>
        /// 当前鼓舞阵营
        /// </summary>
        public CampType InspireCamp = CampType.None;
        public int InspireDValue = 0;
        CampBattle battleAssartPhase;
        CampBattle battleFinalPhase;

        public Dictionary<int, string> SerializeAllForts()
        {
            Dictionary<int, string> fortsStrDic = new Dictionary<int, string>();
            foreach (var item in AllForts)
            {
                string str = item.Value.GetSerialize();
                fortsStrDic.Add(item.Key, str);
            }
            return fortsStrDic;
        }

        public void DeserializeFort(int fortId, string strFort)
        {
            var fortData = CampActivityLibrary.GetCampFortData(fortId);
            if (fortData == null)
            {
                Log.Error($"camp battle error: cannot find fort {fortId},please check xml");
            }

            CampFort fort = new CampFort(fortData);
            fort.Deserialize(strFort);

            int cout;
            if (!campHoldCount.TryGetValue(fort.CampType, out cout))
            {
                campHoldCount.Add(fort.CampType, 1);
            }
            campHoldCount[fort.CampType] = cout + 1;

            AddPlayerHoldFort(fort);

            AllForts.Add(fort.Id, fort);

            //BattleProgressIncrement(fort.CampType);
        }


        public Dictionary<int, int> playerMaxScoreDic = new Dictionary<int, int>();

        internal void SyncHistoricalMaxCampScore(int uid, int maxScore)
        {
            int score;
            if (!playerMaxScoreDic.TryGetValue(uid,out score))
            {
                playerMaxScoreDic.Add(uid, maxScore);
            }

            if (score < maxScore)
            {
                playerMaxScoreDic[uid] = maxScore;
            }
        }

        private void AddPlayerHoldFort(CampFort fort)
        {
            int uid = fort.GetDefenderUid();
            if (uid > 0)
            {
                Dictionary<int, CampFort> forts;
                if (!playerHoldForts.TryGetValue(uid, out forts))
                {
                    forts = new Dictionary<int, CampFort>();
                    playerHoldForts.Add(uid, forts);
                }
                forts.Add(fort.Id, fort);
            }
        }

        private void AddPlayerHoldFortAndCheckGiveUp(CampFort fort)
        {
            int uid = fort.GetDefenderUid();
            if (uid > 0)
            {
                Dictionary<int, CampFort> forts;
                if (!playerHoldForts.TryGetValue(uid, out forts))
                {
                    forts = new Dictionary<int, CampFort>();
                    playerHoldForts.Add(uid, forts);
                }
                forts.Add(fort.Id, fort);

                if (CheckPlayerMaxFortCount(uid, forts.Count))
                {
                    var giveUpTime = CampBattleLibrary.GetCampBattleFortGiveUpTime();

                    CampBattleFortGiveUpTimerQuery tquery = new CampBattleFortGiveUpTimerQuery(giveUpTime, uid, forts);
                    server.TaskTimerMng.Call(tquery, (ret) =>
                    {
                        AutoGiveUp(uid, tquery.Forts);

                        server.TrackingLoggerMng.TrackTimerLog(server.MainId, "relation", "CampBattleFortGiveUp", server.Now());
                    });
                }
            }
        }

        private void AutoGiveUp(int uid, Dictionary<int,CampFort> forts)
        {
            int nowCount = 0;
            int fortId = 0;
            int tempStar = 100;
            foreach (var item in forts)
            {
                if (item.Value.GetDefenderUid() == uid)
                {
                    if (tempStar > item.Value.XmlData.Star)
                    {
                        tempStar = item.Value.XmlData.Star;
                        fortId = item.Value.Id;
                    }
                    nowCount++;
                }
            }

            if (CheckPlayerMaxFortCount(uid, nowCount))
            {
                GiveUpFort(uid, fortId);
            }
        }

        /// <summary>
        /// 超最大据点占领数量
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="nowFortCount"></param>
        /// <returns></returns>
        private bool CheckPlayerMaxFortCount(int uid,int nowFortCount)
        {
            int score = CampBattleLibrary.GetCampBattleStrongPointControlLimitScore(nowFortCount);
            int maxScore = 0;
            playerMaxScoreDic.TryGetValue(uid, out maxScore);
            return score > maxScore;
        }

        private void DelPlayerHoldFort(CampFort fort)
        {
            int uid = fort.GetDefenderUid();
            if (uid > 0)
            {
                Dictionary<int, CampFort> forts;
                if (playerHoldForts.TryGetValue(uid, out forts))
                {
                    forts.Remove(fort.Id);
                }
            }
        }

        internal void UpdateDefensiveQueue(int uid, RepeatedField<HERO_INFO> heroList,bool setForts)
        {
            Dictionary<int, CampFort> forts;

            Dictionary<int, RepeatedField<HERO_INFO>> queues = null;
            if (playerHoldForts.TryGetValue(uid, out forts))
            {
                foreach (var item in forts)
                {
                    CampFort fort = item.Value;
                    if (!fort.IsInBattle)
                    {
                        if (queues == null)
                        {
                            queues = new Dictionary<int, RepeatedField<HERO_INFO>>();
                            foreach (var hero in heroList)
                            {
                                RepeatedField<HERO_INFO> list;
                                int queueId = hero.DefensiveQueueNum;
                                if (!queues.TryGetValue(queueId, out list))
                                {
                                    list = new RepeatedField<HERO_INFO>();
                                    queues.Add(queueId, list);
                                }
                                list.Add(hero);
                            }
                        }

                        if (setForts)
                        {
                            fort.SetDefensiveQueue(queues);
                        }
                        else
                        {
                            fort.UpdateDefensiveHeroInfo(queues);
                        }
                        needUpdate = true;
                    }
                }
            }
        }

        public void DungeonWindUp(MSG_ZR_CAMP_DUNGEON_END msg)
        {
            needUpdate = true;

            int fortId = msg.FortId;
            int dungeonId = msg.DungeonId;

            CampFort fort = GetCampBattleFort(fortId);
            if (fort == null)
            {
                Log.Error($" camp {CurCampBattleStep} step wind up got an fail fort id {fortId} dungeon id {dungeonId}");
                return;
            }

            CampDungeon dungeon = fort.GetDungeon(dungeonId);
            if (dungeon == null)
            {
                if (dungeonId == fort.XmlData.BossDungeonId)
                {
                    NotifyAlreayHold(fort, dungeonId, msg.Result, msg.AttackerInfo);
                    return;
                }
                else if (fort.XmlData.FollowerDungoenIdList.Contains(dungeonId))
                {
                    NotifyAlreayHold(fort, dungeonId, msg.Result, msg.AttackerInfo);
                    return;
                }
                else
                {
                    Log.Error($" camp assert step wind up got an fail dungeon id {dungeonId}(fort in state {fort.State})");
                    return;
                }
            }

            if (msg.Result == (int)DungeonResult.Success)
            {
                HoldAndNotify(fort, dungeon, msg.Result, msg.AttackerInfo, msg.AttackerHeroList);
            }

        }

        /// <summary>
        /// 通知已经被别人攻克
        /// </summary>
        /// <param name="attackterUid"></param>
        /// <param name="fortId"></param>
        /// <param name="dungeonId"></param>
        private void NotifyAlreayHold(CampFort fort, int dungeonId, int result, PLAY_BASE_INFO attackerInfo)
        {
            DungeonResult dungeonResult = (DungeonResult)result;
            //TODO:BOIL 通知
            Log.Info($"fort {fort.Id} dungeon {dungeonId} already be hold!");

        }

        /// 攻克副本并通知
        private void HoldAndNotify(CampFort fort, CampDungeon dungeon, int result, PLAY_BASE_INFO attackerInfo, RepeatedField<HERO_INFO> attackerHeroList)
        {
            DungeonResult dungeonResult = (DungeonResult)result;

            if (dungeon.Id == fort.MainDungeon.Id) //如果是主副本
            {
                if (attackerInfo.Uid == fort.GetDefenderUid())
                {
                    return;
                }

                DelHoldCount(fort.CampType);
                DelPlayerHoldFort(fort);

                //跑马灯通知
                NotifyResult(attackerInfo.Uid, fort, dungeon);
                RecordCampBattleHoldAnnouncement(fort,attackerInfo);

                fort.SetFortHold(dungeon, attackerInfo, attackerHeroList);

                AddHoldCount((CampType)attackerInfo.Camp);
                AddPlayerHoldFortAndCheckGiveUp(fort);
            }
            else
            {
                dungeon.IsBeenHold = true;
                fort.IsInBattle = true;
                fort.UpdateProgress();
            }
            //
            needUpdate = true;
            Log.Info($"player {attackerInfo.Uid} holded fort {fort.Id} dungeon {dungeon.Id}!");
        }

        private void AddHoldCount(CampType camp)
        {
            int count ;
            if (campHoldCount.TryGetValue(camp,out count))
            {
                count += 1;
                campHoldCount[camp] = count;
            }
        }

        private void DelHoldCount(CampType camp)
        {
            int count;
            if (campHoldCount.TryGetValue(camp, out count))
            {
                count -= 1;
                if (count<0)
                {
                    count = 0;
                }
                campHoldCount[camp] = count;
            }
        }

        public void NotifyResult(int uid, CampFort fort, CampDungeon dungeon)
        {
            string defenderName = fort.GetDefenderName();

            MSG_RZ_CAMP_DUNGEON_END notify = new MSG_RZ_CAMP_DUNGEON_END();
            notify.FortId = fort.Id;
            notify.DefenderName = defenderName;
            notify.DungeonId = dungeon.DungeonId;

            Client client = server.ZoneManager.GetClient(uid);//FIXME:是否存在玩家副本跨zone的情况？没仔细看先这样
            if (client != null)
            {
                client.Write(notify);
            }
            else
            {
                Logger.Log.Error("player {0} camp battle NotifyResult fail! cannot find client", uid);
            }
        }


        /// <summary>
        /// 检查粮草
        /// </summary>
        /// <param name="camp"></param>
        /// <returns></returns>
        public bool CheckGrain(CampType camp, MapType type)
        {
            return CampCoinDic[camp].GetCoins(CampCoin.Grain) >= CampBattleLibrary.GetBattleSpendGrain(CurCampBattleStep, type);
        }


        internal MSG_RZ_CAMP_CREATE_DUNGEON CreateDungeon(int uid, int camp, int fortId, int dungeonId)
        {
            MSG_RZ_CAMP_CREATE_DUNGEON msg = new MSG_RZ_CAMP_CREATE_DUNGEON();
            msg.Camp = camp;
            msg.FortId = fortId;
            msg.Result = (int)ErrorCode.Success;
            msg.DungeonIndex = dungeonId;

            msg.InspireCamp = (int)InspireCamp;
            msg.InspireDValue = InspireDValue;

            CampType campType = (CampType)msg.Camp;
            CampFort fort = GetCampBattleFort(fortId);

            int realDungeonId = 0;
            if (fortId == 0)
            {
                realDungeonId = dungeonId;
            }
            else
            {
                if (fort == null)
                {
                    return msg;
                }
                if (dungeonId == 0)
                {
                    realDungeonId = fort.XmlData.BossDungeonId;
                }
                else
                {
                    realDungeonId = fort.XmlData.DefenderDungeonId;
                }
            }
            msg.DungeonId = realDungeonId;
            msg.FortCamp =(int)fort.CampType;

            DungeonModel model = DungeonLibrary.GetDungeon(realDungeonId);
            //尝试消耗阵营粮草
            if (!CheckGrain(campType, (MapType)model.Type))
            {
                msg.Result = (int)ErrorCode.GrianNotEnough;
                return msg;
            }

            if (fort == null)
            {
                return msg;
            }
            if (!CheckRelationFort(fort, campType))
            {
                msg.Result = (int)ErrorCode.CanNotBeAttack;
                return msg;
            }
            //据点是否冷却
            if (fort.CheckInCDTime())
            {
                msg.Result = (int)ErrorCode.CampFortInCD;
                return msg;
            }

            CampDungeon dungeon = fort.GetDungeon(dungeonId);
            if (dungeon == null)
            {
                msg.Result = (int)ErrorCode.DungeonAlreadyHold;
                return msg;
            }
            if (dungeon.IsBeenHold)
            {
                msg.Result = (int)ErrorCode.DungeonAlreadyHold;
                return msg;
            }
            if (fort.State == FortState.Defender)
            {
                msg.DefenderUid = fort.DefenderPlayerInfo.Uid;
                if (fort.MainDungeon.Id == dungeonId)
                {
                    if ((int)fort.CampType != camp)
                    {
                        if (!fort.CheckCanAttackMain())
                        {
                            msg.Result = (int)ErrorCode.PleaseAttackFollowerFrist;
                            return msg;
                        }
                    }
                }
            
                //玩家防守 hero信息
                msg.HeroList.AddRange(dungeon.GetDefenderHeroList());
                var addNatures = fort.GetAddNatures();

                foreach (var item in addNatures)
                {
                    msg.AddNature.Add(item.Key, item.Value);
                }
            }
            if (msg.Result == (int)ErrorCode.Success)
            {
                DelGrain((CampType)camp, CampBattleLibrary.GetBattleSpendGrain(CurCampBattleStep, (MapType)model.Type));
            }

            return msg;
        }

        internal void RecordCampBoxCount()
        {
            foreach (var item in CampCoinDic)
            {
                CampType campType =   item.Key;
                int score =  item.Value.GetCoins(CampCoin.BattleScore);
                int count = CalcBoxCountByBattleScore(score);
                item.Value.SetCoin(CampCoin.BoxCount,count);
            }
        }

        private int CalcBoxCountByBattleScore(int score)
        {
            //计算 分数和数量转换
            var config = CampBattleLibrary.GetCampBattleExpend();
            int count = score / config.BattleScoreToBox;
            return count;
        }

        private bool CheckRelationFort(CampFort fort, CampType camp)
        {
            if (fort.XmlData.IsStartOpen/* && fort.XmlData.CampType == camp*/)
            {
                return true;
            }
            List<int> relationList = fort.GetRelationFort();
            foreach (var item in relationList)
            {
                if (AllForts[item].CampType == camp)
                {
                    return true;
                }
            }
            return false;
        }

        public MSG_RZ_SYNC_CAMPBATTLE_DATA GetCampBattleInfo(int uid =0)
        {
            MSG_RZ_SYNC_CAMPBATTLE_DATA req = GetNowCampBattleInfo();
            if (uid > 0)
            {
                Dictionary<int, CampFort> forts;
                if (playerHoldForts.TryGetValue(uid,out forts))
                {
                    foreach (var item in forts)
                    {
                        req.ScoreUp += item.Value.CalcBattleScoreUp();
                    }
                }
            }
            foreach (var item in AllForts)
            {
                CampFort fort = item.Value;

                FORT_DATA data = new FORT_DATA();
                data.Id = fort.Id;
                data.State = (int)fort.State;
                data.Icon = fort.GetIcon();
                data.Camp = (int)fort.CampType;
                data.Progress = fort.Progress;
                data.MaxProgress = fort.MaxProgress;
                data.CDTime = Timestamp.GetUnixTimeStampSeconds(fort.CDTime);
                data.Uid = fort.GetDefenderUid();
                req.FortList.Add(data);
            }

            
            var TianDoudata = new CAMPBATTLE_DATA();
            TianDoudata.CampId = (int)CampType.TianDou;
            TianDoudata.Grain = CampCoinDic[CampType.TianDou].GetCoins(CampCoin.Grain);
            TianDoudata.Progress = campHoldCount[CampType.TianDou];
            TianDoudata.TotalCampScore = GetCampBattleScore(CampType.TianDou);
            req.BattleInfoList.Add(TianDoudata);

            var XinLuodata = new CAMPBATTLE_DATA();
            XinLuodata.CampId = (int)CampType.XingLuo;
            XinLuodata.Grain = CampCoinDic[CampType.XingLuo].GetCoins(CampCoin.Grain);
            XinLuodata.Progress = campHoldCount[CampType.XingLuo];
            XinLuodata.TotalCampScore = GetCampBattleScore(CampType.XingLuo);
            req.BattleInfoList.Add(XinLuodata);

            return req;
        }

        private MSG_RZ_SYNC_CAMPBATTLE_DATA GetNowCampBattleInfo()
        {
            MSG_RZ_SYNC_CAMPBATTLE_DATA req = new MSG_RZ_SYNC_CAMPBATTLE_DATA();
            req.PhaseNum = nowShowPhaseNum;
            req.Step = (int)CurCampBattleStep;
            req.InspireCamp = (int)InspireCamp;
            req.InspireDValue = InspireDValue;
            switch (CurCampBattleStep)
            {
                case CampBattleStep.Assart:
                    req.BeginTime = battleAssartPhase.NowShowBeginTime.ToString();
                    req.EndTime = battleAssartPhase.NowShowEndTime.ToString();
                    break;
                case CampBattleStep.Final:
                    req.BeginTime = battleFinalPhase.NowShowBeginTime.ToString();
                    req.EndTime = battleFinalPhase.NowShowEndTime.ToString();
                    break;
                default:
                    req.BeginTime = NowShowBeginTime.ToString();
                    req.EndTime = NowShowEndTime.ToString();
                    break;
            }
            return req;
        }

        internal MSG_RZ_GET_FORT_DATA GetCampBattleFortData(int fortId)
        {
            CampFort fort = AllForts[fortId];

            MSG_RZ_GET_FORT_DATA data = new MSG_RZ_GET_FORT_DATA();
            data.FortId = fortId;
            data.FortState = (int)fort.State;
            data.Uid = fort.GetDefenderUid(); 
            data.CDTime = Timestamp.GetUnixTimeStampSeconds(fort.CDTime);
            data.ScoreUp = fort.CalcBattleScoreUp();
            data.Dungeons.Add(GetFortDungenDataMsg(fort.MainDungeon));

            var subDugeons = fort.SubDungeonDic;

            if (subDugeons != null)
            {
                foreach (var item in subDugeons)
                {
                    FORT_DUNGEON_DATA itemData = GetFortDungenDataMsg(item.Value);
                    data.Dungeons.Add(itemData);
                }
            }

            var addNatures = fort.GetAddNatures();
            if (addNatures != null)
            {
                foreach (var item in addNatures)
                {
                    ADD_NATURE_DATA itemData = new ADD_NATURE_DATA();
                    itemData.Id = item.Key;
                    itemData.Value = item.Value;
                    data.AddNatures.Add(itemData);
                }
            }

            return data;
        }

        private static FORT_DUNGEON_DATA GetFortDungenDataMsg(CampDungeon dungeon)
        {
            FORT_DUNGEON_DATA itemData = new FORT_DUNGEON_DATA();
            itemData.DungeonId = dungeon.Id;
            itemData.Power = dungeon.GetPower();
            itemData.IsBeenHold = dungeon.IsBeenHold;

            List<CAMP_CHALLENGER_HERO_INFO> heroList = dungeon.GetDefenderHeroList();
            if (heroList != null)
            {
                itemData.HeroList.AddRange(heroList);
            }
            return itemData;
        }

        internal CampFort GetCampBattleFort(int fortId)
        {
            CampFort fort;
            AllForts.TryGetValue(fortId, out fort);
            return fort;
        }

        internal void FixNew()
        {
            AllForts.Clear();
            foreach (var item in CampActivityLibrary.CampFortLayout)
            {
                CampFort fort = new CampFort(item.Value);
                fort.InitMonster();
                AllForts.Add(fort.Id, fort);
            }
        }

        internal ErrorCode GiveUpFort(int uid, int fortId = 0)
        {
            if (fortId == 0)
            {
                needUpdate = true;
                Dictionary<int, CampFort> forts;
                if (playerHoldForts.TryGetValue(uid,out forts))
                {
                    foreach (var item in forts)
                    {
                        CampFort fort = item.Value;
                        RecordCampBattleGiveUpAnnouncement(fort);

                        //DelHoldCount(fort.CampType);
                        //AddHoldCount(CampType.None);
                        fort.GiveUp();
                    }
                    forts.Clear();
                    playerHoldForts.Remove(uid);
                }
                return ErrorCode.Success;
            }

            CampFort campFort = GetCampBattleFort(fortId);
            if (campFort == null)
            {
                Log.Warn($"player {uid} give up fort {fortId},got an error! fort not find");
                return ErrorCode.Fail;
            }
            if (campFort.GetDefenderUid()!= uid)
            {
                Log.Warn($"player {uid} give up fort {fortId},got an error! the fort owener is wrong");
                return ErrorCode.Fail;
            }
            needUpdate = true;

            RecordCampBattleGiveUpAnnouncement(campFort);

            //DelHoldCount(campFort.CampType);
            DelPlayerHoldFort(campFort);
            //AddHoldCount(CampType.None);
            campFort.GiveUp();
            return ErrorCode.Success;
        }

        internal ErrorCode HoldFort(MSG_ZR_HOLD_FORT msg)
        {
            CampFort campFort = GetCampBattleFort(msg.FortId);
            if (campFort == null)
            {
                Log.Error($"player {msg.AttackerInfo.Uid} hold fort {msg.FortId},got an error! fort not find");
                return ErrorCode.Fail;
            }

            if ((int)campFort.CampType != msg.AttackerInfo.Camp)
            {
                //据点是否冷却
                if (campFort.CheckInCDTime())
                {
                    return ErrorCode.CampFortInCD;
                }
            }

            //尝试消耗阵营粮草
            if (!CheckGrain((CampType)msg.AttackerInfo.Camp, MapType.CampDefense))
            {
                return ErrorCode.GrianNotEnough;
            }

            DelGrain((CampType)msg.AttackerInfo.Camp, CampBattleLibrary.GetBattleSpendGrain(CurCampBattleStep, MapType.CampDefense));


            HoldAndNotify(campFort, campFort.MainDungeon, (int)ErrorCode.Success, msg.AttackerInfo, msg.AttackerHeroList);
            return ErrorCode.Success;
        }

        internal void UpdateAddNature(int uid, int newCount)
        {
            Dictionary<int, CampFort> holdForts = new Dictionary<int, CampFort>();
            if (playerHoldForts.TryGetValue(uid,out holdForts))
            {
                foreach (var item in holdForts)
                {
                    item.Value.UpdateNatureCount(newCount);
                }
            }
        }

        //public MSG_RZ_USE_NATURE_ITEM UseNatureItem(int fortId, int itemId)
        //{
        //    MSG_RZ_USE_NATURE_ITEM response = new MSG_RZ_USE_NATURE_ITEM();
        //    response.FortId = fortId;
        //    response.ItemId = itemId;
        //    response.Result = (int)ErrorCode.Success;
        //    //TODO：BOIL 屬性添加
        //    var itemNatureModel = CampBattleLibrary.GetNatureItems(itemId);
        //    if (itemNatureModel == null)
        //    {
        //        Log.Error($"use nature item {itemId} fail,please check xml!~");
        //        response.Result = (int)ErrorCode.Fail;
        //        return response;
        //    }
        //    UpdateAddNature(fortId, (int)NatureType.PRO_MAX_HP, itemNatureModel.NatureAddRatio);

        //    return response;
        //}

        //internal MSG_RZ_CHECK_USE_NATURE_ITEM CheckUseNatureItem(int fortId, int itemId, int camp)
        //{
        //    MSG_RZ_CHECK_USE_NATURE_ITEM response = new MSG_RZ_CHECK_USE_NATURE_ITEM();
        //    response.FortId = fortId;
        //    response.ItemId = itemId;
        //    switch (CurCampBattleStep)
        //    {
        //        case CampBattleStep.Rest:
        //            Log.Warn("rest step cannot add nature item");
        //            break;
        //        case CampBattleStep.Assart:
        //            response.Result = (int)ErrorCode.Success;
        //            break;
        //        case CampBattleStep.Final:
        //            CampFort fort = GetCampBattleFort(fortId);
        //            if (fort == null)
        //            {
        //                response.Result = (int)ErrorCode.Fail;
        //                return response;
        //            }

        //            if ((int)fort.CampType != camp)
        //            {
        //                response.Result = (int)ErrorCode.FortAlreadyHold;
        //                return response;
        //            }

        //            //据点冷却 FIXME:這裏 -2 意思是cd剩餘2秒及認爲不能操作
        //            if (fort.CDTime > DateTime.MinValue && (RelationServerApi.now - fort.CDTime).TotalSeconds - 2 < CampBattleLibrary.GetCampBattleFortGuardTime())
        //            {
        //                response.Result = (int)ErrorCode.Success;
        //            }
        //            else
        //            {
        //                response.Result = (int)ErrorCode.CampFortNotInCD;
        //                return response;
        //            }
        //            break;
        //        default:
        //            break;
        //    }
        //    return response;
        //}






    }



}
