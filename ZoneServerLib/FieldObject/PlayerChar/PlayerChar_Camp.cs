using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using Message.Relation.Protocol.RZ;
using Message.Zone.Protocol.ZR;
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
        //阵营
        public CampType Camp { get; set; }

        public int PrestigePeriodCount;//用来比对从而在访问时更新prestige而不是period更新时,分散压力

        public int ElectionPeriodCount;//


        private int electionScore;
        public int ElectionScore
        {
            get
            {
                if (ElectionPeriodCount != server.RelationServer.ElectionPeriod)
                {
                    ElectionPeriodCount = server.RelationServer.ElectionPeriod;
                    LoadElectionInfo();
                }
                return electionScore;
            }

            set
            {
                electionScore = value;
            }
        }

        private int prestige;
        public int Prestige//访问时比对版本，不对需要更新
        {
            get
            {
                if (PrestigePeriodCount != server.RelationServer.CampRankPeriod)
                {
                    PrestigePeriodCount = server.RelationServer.CampRankPeriod;
                    LoadPrestigeInfo();
                }
                return prestige;
            }
            set { prestige = value; }
        }
        public int HisPrestige { get; set; }

        public int LastCampRewardPeriod { get; set; } //为CampReward维护的字段

        public Dictionary<int, bool> Worshiped = new Dictionary<int, bool>();
        public DateTime LastUpdateWorshipTime = ZoneServerApi.now - new TimeSpan(1, 0, 0, 0);

        public void UpdateCamp()
        {
            if (LastUpdateWorshipTime.Date != ZoneServerApi.now.Date)
            {
                UpdateWorshipInfo();
                SendBaseCampInfo();
            }
            //CampBuild.CheckRefresh();

            //if (PrestigePeriodCount != server.RelationServer.CampRankPeriod)
            //{
            //    PrestigePeriodCount = server.RelationServer.CampRankPeriod;
            //    LoadPrestigeInfo();
            //}
            //if (ElectionPeriodCount != server.RelationServer.ElectionPeriod)
            //{
            //    ElectionPeriodCount = server.RelationServer.ElectionPeriod;
            //    LoadElectionInfo();
            //}
        }

        /// <summary>
        /// 领取声望排名的奖励
        /// </summary>
        public void GetCampReward()
        {

            MSG_ZGC_GET_CAMP_REWARD ans = new MSG_ZGC_GET_CAMP_REWARD();
            ans.ErrorCode = (int)ErrorCode.Success;

            OperateGetLastCampRankAndPeriod4Pc op = new OperateGetLastCampRankAndPeriod4Pc(MainId, (int)Camp, Uid);
            server.GameRedis.Call(op, ret =>
            {
                int curPeriod = op.CurPeriod;
                int lastRank = op.Rank;
                if (LastCampRewardPeriod < curPeriod - 1)
                {
                    // 发奖
                    PrestigeReward reward = CampLibrary.GetReward(lastRank);
                    if (reward != null)
                    {
                        RewardManager rewardMng = GetSimpleReward(reward.Reward, ObtainWay.CampRankReward);

                        rewardMng.GenerateRewardItemInfo(ans.Rewards);

                        //修改LastCampRewardPeriod为上周期
                        LastCampRewardPeriod = curPeriod - 1;
                        OperateSetLastCampRewardPeriod setRePeriod = new OperateSetLastCampRewardPeriod(curPeriod - 1, Uid);
                        server.GameRedis.Call(setRePeriod);
                    }
                }
                else
                {
                    Logger.Log.Warn($"player {Uid} get camp reward failed: lastCampRewardPeriod {LastCampRewardPeriod}");
                    ans.ErrorCode = (int)ErrorCode.Already;
                }
                Write(ans);
            });
        }

        /// <summary>
        /// 选择阵营
        /// </summary>
        /// <param name="msg"></param>
        public void ChooseCamp(MSG_GateZ_CHOOSE_CAMP msg)
        {
            MSG_ZGC_CHOOSE_CAMP_RESULT result = new MSG_ZGC_CHOOSE_CAMP_RESULT();
            result.ErrorCode = (int)ErrorCode.Success;

            int old = (int)Camp;
            if (Camp == CampType.None)
            {
                switch (msg.Camp)
                {
                    case 1:
                    case 2:
                        Camp = (CampType)msg.Camp;
                        //komoelog
                        KomoeEventLogCampFlow(((int)Camp).ToString(), Camp.ToString(), 1, 0);
                        break;
                    case 3:
                        //Camp = (CampType)RAND.Range(1, 2);
                        RandomChooseCamp();
                        return;
                    default:
                        break;
                }
            }
            else
            {
                switch (msg.Camp)
                {
                    case 1:
                    case 2:
                        if (old != msg.Camp)
                        {
                            if (CheckChangeCampMoney())
                            {
                                ChooseCampWithRankCheck(msg.Camp);
                                return;
                            }
                            else
                            {
                                Logger.Log.Warn($"player {Uid} choose camp failed: coin not enough");
                                result.ErrorCode = (int)ErrorCode.NoCoin;
                            }
                        }
                        else
                        {
                            Logger.Log.Warn($"player {Uid} choose camp failed: param camp error");
                            result.ErrorCode = (int)ErrorCode.Unknown;
                        }
                        break;
                    case 3:
                        Logger.Log.Warn($"player {Uid} already choosed Camp can not random choose");
                        break;
                    default:
                        break;
                }             
            }
            if (old != (int)Camp)
            {
                UpdateCamp2DB();
                UpdateCamp2Redis(old > 0);

            }
            
            if (old == (int)CampType.None)
            {
                //加入阵营属性加成
                HeroMng.AddCampStarsNatures(Camp);
                GetCampBuildInfo();

                //首次加入阵营发称号卡
                TitleMng.UpdateTitleConditionCount(TitleObtainCondition.JoinCamp);               
            }

            result.Camp = (int)Camp;

            //worship
            GetWorship4CampChoose(result);


            Write(result);
        }

        /// <summary>
        /// 参与竞选
        /// </summary>
        public void RunInElection()
        {
            MSG_ZGC_RUN_IN_ELECTION_RESULT res = new MSG_ZGC_RUN_IN_ELECTION_RESULT();
            DateTime now = ZoneServerApi.now;
            if (Level < CampLibrary.RunInElectionLevelLimit)
            {
                Logger.Log.Warn($"player {Uid} run in election failed: level {Level} limit");
                res.ErrorCode = (int)ErrorCode.LevelLimit;
                Write(res);
                return;
            }
            if (now > server.RelationServer.ElectionRankBegin && now < server.RelationServer.ElectionRankEnd)
            {
                //OperateRunInCampElection op = new OperateRunInCampElection(MainId, (int)Camp, Uid);
                OperateRunInCampElection op = new OperateRunInCampElection(MainId, (int)Camp, RankType.CampElection, Uid, server.Now());
                server.GameRedis.Call(op, ret =>
                 {
                     int result = (int)ret;
                     if (result == 1)
                     {
                         res.ErrorCode = (int)ErrorCode.Success;
                         MSG_ZR_UPDATE_ELECTION_RANK msg = new MSG_ZR_UPDATE_ELECTION_RANK();
                         msg.Camp = (int)Camp;
                         server.RelationServer.Write(msg);
                     }
                     else if (result == 2)
                     {
                         res.ErrorCode = (int)ErrorCode.Already;
                         Logger.Log.Warn($"player {Uid} run in election failed: already run in election");
                     }
                     else
                     {
                         res.ErrorCode = (int)ErrorCode.Fail;
                         Logger.Log.Warn($"player {Uid} run in election failed: not find in redis");
                     }
                     Write(res);
                 });
            }
            else
            {
                Logger.Log.Warn($"player {Uid} run in election failed: not in election rank time");
                res.ErrorCode = (int)ErrorCode.NotOpen;
                Write(res);
            }
        }

       public void SendWorshipShow()
        {
            MSG_ZGC_CAMP_WORSHIP_SHOW msg = new MSG_ZGC_CAMP_WORSHIP_SHOW();
            msg.Worships.AddRange(GetWorshipsByCamp(CampType.TianDou));
            msg.Worships.AddRange(GetWorshipsByCamp(CampType.XingLuo));
            Write(msg);
        }


        public void  SyncWorshipShowMsg(CampType campType)
        {
            MSG_ZGC_CAMP_WORSHIP_SHOW_UPDATE msg = new MSG_ZGC_CAMP_WORSHIP_SHOW_UPDATE();
            msg.Camp = (int)campType;
            msg.Worships.AddRange(GetWorshipsByCamp(campType));
            Write(msg);
        }


        private List<WORSHIP_SHOW_MODEL> GetWorshipsByCamp(CampType camp)
        {
            List<WORSHIP_SHOW_MODEL> worshipList = new List<WORSHIP_SHOW_MODEL>();
            Dictionary<int, WorshipRedisInfo> worships = null;
            if (camp == CampType.TianDou)
            {
                worships = server.RelationServer.TianDouWorship;
            }
            else if (camp == CampType.XingLuo)
            {
                worships = server.RelationServer.XingLuoWorship;
            }
            if (worships != null)
            {
                for (int i = 1; i <= 3; i++)
                {
                    WORSHIP_SHOW_MODEL worship = new WORSHIP_SHOW_MODEL();
                    WorshipRedisInfo temp = null;
                    if (worships.TryGetValue(i, out temp) && Worshiped.ContainsKey(i))
                    {
                        worship.PcUid = temp.Uid;
                        worship.Rank = temp.Rank;
                        worship.Name = temp.Name;
                        worship.ModelId = temp.ModelId;
                        worship.HeroId = temp.HeroId;
                        worship.Worshiped = Worshiped[i];
                        worship.Camp = (int)camp;
                        worship.Icon = temp.Icon;
                        worship.Level = temp.Level;
                        worship.BattlePower = temp.BattlePower;
                        worship.GodType = temp.GodType;
                    }
                    worshipList.Add(worship);
                }
            }
            return worshipList;
        }


        /// <summary>
        /// 阵营膜拜
        /// </summary>
        /// <param name="rank"></param>
        public void WorshipRank(int rank)
        {
            if (LastUpdateWorshipTime.Date != ZoneServerApi.now.Date)
            {
                OperateUpdateCharWorshipRecord update = new OperateUpdateCharWorshipRecord(Uid);
                server.GameRedis.Call(update);
                Worshiped[1] = false;
                Worshiped[2] = false;
                Worshiped[3] = false;
                LastUpdateWorshipTime = ZoneServerApi.now.Date;
            }


            MSG_ZGC_CAMP_WORSHIP_RESULT result = new MSG_ZGC_CAMP_WORSHIP_RESULT();
            result.ErrorCode = (int)ErrorCode.Success;
            result.Rank = rank;

            if (rank > 0 && rank <= 3)
            {
                //检查扣费
                WorshipBase temp = CampLibrary.GetWorshipBase(rank);
                if (!DelCoins(temp.Type, temp.CurrencyCount, ConsumeWay.CampWorship, rank.ToString()))
                {
                    Logger.Log.Warn($"player {Uid} WorshipRank failed: coins not enough");
                    result.ErrorCode = (int)ErrorCode.NoCoin;
                    Write(result);
                    return;
                }

                if (!Worshiped[rank])
                {
                    RewardManager rewardMng = GetSimpleReward(temp.Reward, ObtainWay.CampWorship);
                    Worshiped[rank] = true;
                    OperateSetCharWorshipRecord op = new OperateSetCharWorshipRecord(Uid, rank);
                    server.GameRedis.Call(op);

                    //阵营膜拜
                    AddTaskNumForType(TaskType.CampWorshipRank);
                    AddPassCardTaskNum(TaskType.CampWorshipRank);
                    //累积膜拜次数发称号卡
                    //TitleMng.UpdateTitleConditionCount(TitleObtainCondition.CampWorshipCount);

                    //komoelog
                    List<Dictionary<string, object>> consume = ParseConsumeInfoToList(null, (int)temp.Type, temp.CurrencyCount);
                    KomoeEventLogCampWorship(((int)Camp).ToString(), Camp.ToString(), 0, rank, consume);
                }
                else
                {
                    result.ErrorCode = (int)ErrorCode.Already;
                    Logger.Log.Warn($"player {Uid} worship rank {rank} already worshiped");
                }
            }
            else
            {
                result.ErrorCode = (int)ErrorCode.NotExist;
                Logger.Log.Warn($"player {Uid} worship rank {rank} not exist");
            }

            Write(result);
        }

        /// <summary>
        /// 阵营投票
        /// </summary>
        /// <param name="msg"></param>
        public void CampElect(MSG_GateZ_VOTE msg)
        {
            MSG_ZGC_CAMP_VOTE_RESULT result = new MSG_ZGC_CAMP_VOTE_RESULT();
            result.ErrorCode = (int)ErrorCode.Success;
            //查看在投票周期内
            DateTime now = ZoneServerApi.now;
            if (now > server.RelationServer.ElectionRankBegin && now < server.RelationServer.ElectionRankEnd)
            {
                //投票并返回
                VoteItem item = CampLibrary.GetVoteItem(msg.ItemId);
                if (item != null)
                {
                    //result.ErrorCode=(int)UseItem(item.Id, msg.Num);

                    ErrorCode errorCode = ErrorCode.Fail;
                    BaseItem bitem = BagManager.GetItem(MainType.Material, item.Id);
                    if (!CheckItemInfo(bitem, msg.Num, ref errorCode))
                    {
                        result.ErrorCode = (int)errorCode;
                    }
                    BaseItem baseItem = DelItem2Bag(bitem, RewardType.NormalItem, msg.Num, ConsumeWay.ItemUse);

                    if (baseItem != null)
                    {
                        SyncClientItemInfo(bitem);
                    }
                    if (result.ErrorCode == (int)ErrorCode.Success)
                    {
                        CampVoteFor(msg.ToPcUid, item.Ticket * msg.Num);
                    }
                }
            }
            else
            {
                Logger.Log.Warn($"player {Uid} camp elect toPcUid {msg.ToPcUid} failed: not in election rank time");
                result.ErrorCode = (int)ErrorCode.NotOpen;
            }

            Write(result);

        }

        public void CampVoteFor(int toUid, int tickets)
        {
            //
            OperateVoteForCampElectionRank op = new OperateVoteForCampElectionRank(MainId, (int)Camp, RankType.CampElection, toUid, tickets, server.Now());
            server.GameRedis.Call(op,ret=>
            {
                MSG_ZR_UPDATE_ELECTION_RANK msg = new MSG_ZR_UPDATE_ELECTION_RANK();
                msg.Camp = (int)Camp;
                server.RelationServer.Write(msg);
            });
        }

        public bool CheckChangeCampMoney()
        {

            int coinCost = CampLibrary.ChangeCost;
            int curDiamond = GetCoins(CurrenciesType.diamond);
            if (coinCost <= curDiamond)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void RandomChooseCamp()
        {
            MSG_ZR_GET_WEAK_CAMP msg = new MSG_ZR_GET_WEAK_CAMP();
            msg.PcUid = Uid;
            server.SendToRelation(msg, Uid);
        }

        public void ChooseCampWithRankCheck(int camp)
        {
            MSG_ZR_CHANGE_CAMP msg = new MSG_ZR_CHANGE_CAMP();
            msg.PcUid = Uid;
            msg.CampId = camp;
            server.SendToRelation(msg, Uid);
        }

        public void ChooseCampWithRankCheck(bool allowed, int camp)
        {
            MSG_ZGC_CHOOSE_CAMP_RESULT result = new MSG_ZGC_CHOOSE_CAMP_RESULT();
            result.ErrorCode = (int)ErrorCode.Success;

            if (!allowed)
            {
                result.ErrorCode = (int)ErrorCode.NotAllowed;
                Write(result);
                return;
            }
            if (CheckChangeCampMoney())
            {

                //扣费
                int coinCost = CampLibrary.ChangeCost;
                //int curDiamond = GetCoins(CurrenciesType.diamond);
                DelCoins(CurrenciesType.diamond, coinCost, ConsumeWay.ChangeCamp, camp.ToString());

                server.GameRedis.Call(new OperateClearCampRankScore(RankType.CampBattlePower, Camp, MainId, uid));
                CampBattleScoreClear(uid, Camp);
                CampBattleBoxClear(uid);
                CampBuildValueClear(uid, Camp);
                Camp = (CampType)camp;


                UpdateCamp2DB();
                UpdateCamp2Redis(true);
                //重新发送base
                SendBaseCampInfo();

                //komoelog
                KomoeEventLogCampFlow(((int)Camp).ToString(), Camp.ToString(), 2, 0);
            }
            else
            {
                result.ErrorCode = (int)ErrorCode.NoCoin;
            }
            result.Camp = (int)Camp;

            //worship
            GetWorship4CampChoose(result);
            Write(result);
        }

        private void CampBattleBoxClear(int uid)
        {
            //UpdateCounter(CounterType.CampBoxCount, 9999);
            SetCounter(CounterType.CampBoxCount, 9999);
            CampBoxLeftCount = 0;
        }

        private void CampBattleScoreClear(int uid, CampType camp)
        {
            CampBattleMng.CampScore = 0;
            server.GameRedis.Call(new OperateDelCampScore(MainId, (int)camp, RankType.CampBattleScore, uid));
            QueryUpdateCampScore query = new QueryUpdateCampScore(uid, 0);
            server.GameDBPool.Call(query);
        }

        private void CampBuildValueClear(int uid, CampType camp)
        {
            CampBattleMng.CampBuildValue = 0;
            CampBuildPhaseInfo phaseInfo = GetCampBuild();
            if (phaseInfo != null && phaseInfo.EndTime <= server.Now() && server.Now() <= phaseInfo.NextBeginTime)
            {
                //结算期间不清理数据
            }
            else
            {
                server.GameRedis.Call(new OperateDelCampScore(MainId, (int)camp, RankType.CampBuild, uid));
            }
            QueryUpdateCampBuildValue query = new QueryUpdateCampBuildValue(Uid, CampBattleMng.CampBuildValue);
            server.GameDBPool.Call(query);
        }


        public void SendRandomChooseCampResult(MSG_RZ_WEAK_CAMP msg)
        {
            MSG_ZGC_CHOOSE_CAMP_RESULT result = new MSG_ZGC_CHOOSE_CAMP_RESULT();
            Camp = (CampType)msg.Camp;
            UpdateCamp2DB();
            UpdateCamp2Redis(false);
            result.Camp = msg.Camp;
            result.ErrorCode = (int)ErrorCode.Success;
            // 给奖励
            result.RewardDiamond = CampLibrary.GainDiamond;
            AddCoins(CurrenciesType.diamond, result.RewardDiamond, ObtainWay.RandomChooseCamp, "");
            //加入阵营属性加成
            HeroMng.AddCampStarsNatures(Camp);
            GetCampBuildInfo();

            //worship
            GetWorship4CampChoose(result);

            //首次加入阵营发称号卡
            TitleMng.UpdateTitleConditionCount(TitleObtainCondition.JoinCamp);

            //komoelog
            KomoeEventLogCampFlow(((int)Camp).ToString(), Camp.ToString(), 1, 0);

            Write(result);
        }

        public void ShowCampInfos(int camp, int page)
        {
            MSG_ZR_GET_CAMP_RANK req = new MSG_ZR_GET_CAMP_RANK();
            req.PcUid = Uid;
            if (camp > 0 && camp < 3)
            {
                req.Camp = camp;

            }
            else
            {
                req.Camp = (int)Camp;
            }
            req.Page = page;
            server.SendToRelation(req, Uid);
        }

        public void ShowElectionInfos(int page)
        {
            MSG_ZR_GET_CAMP_ELECTION req = new MSG_ZR_GET_CAMP_ELECTION();
            req.PcUid = Uid;
            req.Page = page;
            req.Camp = (int)Camp;
            server.SendToRelation(req, Uid);
        }

        public void ShowCampPanelInfos()
        {
            if (Camp > 0)
            {
                MSG_ZR_GET_CAMP_PANEL_INFO req = new MSG_ZR_GET_CAMP_PANEL_INFO();
                req.PcUid = Uid;
                req.Camp = (int)Camp;
                server.SendToRelation(req, Uid);
            }
        }

        public void SendCampRankInfo(MSG_RZ_CAMP_RANK_LIST msg)
        {
            MSG_ZGC_CAMP_INFO infos = new MSG_ZGC_CAMP_INFO();
            infos.Camp = msg.CampId;
            infos.Page = msg.Page;
            infos.TotalCount = msg.TotalCount;
            infos.Begin = server.RelationServer.CampRankBegin.ToString();
            infos.End = server.RelationServer.CampRankEnd.ToString();
            foreach (var item in msg.RankInfos)
            {
                MSG_ZGC_CAMP_RANK_INFO info = new MSG_ZGC_CAMP_RANK_INFO();
                info.Uid = item.Uid;
                info.Name = item.Name;
                info.ShowDIYIcon = item.ShowDIYIcon;
                info.Icon = item.Icon;
                info.IconFrame = item.IconFrame;
                info.Level = item.Level;
                info.HisPrestige = item.HisPrestige;
                info.Family = item.Family;
                info.Prestige = item.Prestige;
                info.Rank = item.Rank;
                info.GodType = item.GodType;
                infos.RankInfos.Add(info);
            }

            if ((int)Camp == msg.CampId)
            {

                MSG_ZGC_CAMP_RANK_INFO self = new MSG_ZGC_CAMP_RANK_INFO();
                self.Uid = Uid;
                self.Name = Name;
                self.Icon = Icon;
                self.Level = Level;
                self.HisPrestige = HisPrestige;
                self.Family = FamilyId;
                self.Prestige = Prestige;
                self.GodType = GodType;

                infos.OwnerInfo = self;

                //LoadRankAndSend(infos);
                infos.OwnerInfo.Rank = msg.OwnerRank;
            }
            Write(infos);
        }

        public int CampBoxLeftCount = 0;

        public void SendCampPanelInfo(MSG_RZ_CAMP_PANEL_LIST msg)
        {
            MSG_ZGC_CAMP_PANEL_INFO infos = new MSG_ZGC_CAMP_PANEL_INFO();
            infos.Camp = msg.CampId;
            CampBoxLeftCount = msg.BoxCount - GetCounter(CounterType.CampBoxCount).Count;
            if (CampBoxLeftCount < 0)
            {
                CampBoxLeftCount = 0;
            }

            infos.CampBoxCount = CampBoxLeftCount;

            //infos.HasReward = msg.HasReward;  
            foreach (var item in msg.RankInfos)
            {
                MSG_ZGC_CAMP_RANK_INFO info = new MSG_ZGC_CAMP_RANK_INFO();
                info.Uid = item.Uid;
                info.Name = item.Name;
                info.ShowDIYIcon = item.ShowDIYIcon;
                info.Icon = item.Icon;
                info.IconFrame = item.IconFrame;
                info.Level = item.Level;
                info.HisPrestige = item.HisPrestige;
                info.Family = item.Family;
                info.Prestige = item.Prestige;
                info.Rank = item.Rank;
                info.GodType = item.GodType;
                infos.RankInfos.Add(info);
            }

            MSG_ZGC_CAMP_RANK_INFO self = new MSG_ZGC_CAMP_RANK_INFO();
            self.Uid = Uid;
            self.Name = Name;
            self.Icon = Icon;
            self.Level = Level;
            self.HisPrestige = HisPrestige;
            self.Family = FamilyId;
            self.Prestige = Prestige;
            self.GodType = Prestige;
            infos.OwnerInfo = self;
            infos.OwnerInfo.Rank = GodType;


            LoadPanelAndSend(infos,msg.Period);
        }

        private void LoadPanelAndSend(MSG_ZGC_CAMP_PANEL_INFO msg,int curPeriod)
        {
            // todo  需要放到relation去要
            //OperateGetCampRankAndPeriod4Pc op = new OperateGetCampRankAndPeriod4Pc(MainId, (int)Camp, Uid);
            //server.Redis.Call(op, ret =>
            //{
            //    if ((int)ret > 0)
            //    {
            //        msg.OwnerInfo.Rank = op.Rank;
            //        if (op.LastPeriodElectionRank > 0 && op.LastPeriodElectionRank < 4)
            //        {
            //            msg.LastPeriodElectionRank = op.LastPeriodElectionRank;
            //        }
                    if (LastCampRewardPeriod >= curPeriod - 1)
                    {
                        Write(msg);
                    }
                    else
                    {
                        //获取上次的排名，设置可领奖
                        OperateGetLastCampRank opR = new OperateGetLastCampRank(MainId, (int)Camp, curPeriod - 1, Uid);
                        server.GameRedis.Call(opR, ret2 =>
                        {
                            if (opR.Rank > 0)
                            {
                                msg.HasReward = true;
                                msg.LastPeriodRank = opR.Rank;
                            }
                            else
                            {
                                msg.HasReward = false;
                                msg.LastPeriodRank = -1;
                            }
                            Write(msg);
                        });
                    }
            //    }
            //});
        }

        //private void LoadRankAndSend(MSG_ZGC_CAMP_INFO msg)
        //{
        //    OperateGetCampRank4Character op = new OperateGetCampRank4Character(MainId, (int)Camp, Uid);
        //    server.Redis.Call(op, ret =>
        //     {
        //         msg.OwnerInfo.Rank = op.Rank;
        //         Write(msg);
        //     });
        //}

        public void SendCampElectionInfo(MSG_RZ_CAMP_ELECTION_LIST msg)
        {
            MSG_ZGC_CAMP_ELECTION_INFO infos = new MSG_ZGC_CAMP_ELECTION_INFO();
            infos.Camp = msg.CampId;
            infos.Page = msg.Page;
            infos.TotalCount = msg.TotalCount;
            infos.Begin = Timestamp.GetUnixTimeStampSeconds(server.RelationServer.ElectionRankBegin);
            infos.End = Timestamp.GetUnixTimeStampSeconds(server.RelationServer.ElectionRankEnd);
            foreach (var item in msg.ElectionInfos)
            {
                MSG_ZGC_ELECTION_INFO info = new MSG_ZGC_ELECTION_INFO();
                info.Uid = item.Uid;
                info.Name = item.Name;
                info.ShowDIYIcon = item.ShowDIYIcon;
                info.Icon = item.Icon;
                info.IconFrame = item.IconFrame;
                info.Level = item.Level;
                info.HisPrestige = item.HisPrestige;
                info.Family = item.Family;
                info.Rank = item.Rank;
                info.TicketCount = item.TicketScore;
                infos.RankInfos.Add(info);
            }

            Write(infos);
        }

        public void AddPrestige(int delta)
        {
            Prestige = Prestige + delta;
            HisPrestige = HisPrestige + delta;
            AddPrestige2Redis(delta);
            UpdateHisPrestige2DB();
        }

        private void AddPrestige2Redis(int delta)
        {
            //OperateAddPrestige op = new OperateAddPrestige(MainId, (int)Camp, Uid, delta);
            OperateAddPrestige op = new OperateAddPrestige(MainId, (int)Camp, RankType.CampPrestige, Uid, delta, server.Now());
            server.GameRedis.Call(op);
        }

        private void UpdateHisPrestige2DB()
        {
            QueryUpdateHisPrestige updateHisPrestige = new QueryUpdateHisPrestige(Uid, HisPrestige);
            server.GameDBPool.Call(updateHisPrestige);
        }

        private void UpdateCamp2DB()
        {
            QueryUpdateCamp op = new QueryUpdateCamp(Uid, (int)Camp);
            server.GameDBPool.Call(op);
        }

        private void UpdateCamp2Redis(bool changeRank)
        {
            OperateSetCamp op = new OperateSetCamp(Uid, MainId, (int)Camp);
            server.GameRedis.Call(op);

            //判断是否更改榜单
            if (changeRank)
            {
                //更新膜拜信息
                //UpdateWorshipInfo();
                //
                OperateChangeCampRank cop = new OperateChangeCampRank(Uid, MainId, (int)Camp);
                server.GameRedis.Call(cop, ret =>
                 {
                     LoadPrestigeInfo();
                     UpdateCampBattlePower(uid);
                     //LoadElectionInfo();
                 });

            }
        }

        private void UpdateCampBattlePower(int uid)
        {
            int power =HeroMng.CalcBattlePower();// info.GetBattlePower();
            server.GameRedis.Call(new OperateUpdateCampRankScore(RankType.CampBattlePower, Camp,server.MainId, Uid, power, server.Now()));
        }

        public void LoadCampInfos()
        {
            //加载redis维护的数据
            LoadPrestigeInfo();
            LoadCampRewardInfos();
            //LoadElectionInfo();
            LoadWorShipInfo(true);
        }

        //同步加载
        private void LoadWorShipInfo(bool sync = false)
        {
            OperateGetCharWorshipRecord op = new OperateGetCharWorshipRecord(Uid);
            if (sync)
            {
                op.DoExccute();
                {
                    if (op.got)
                    {
                        LastUpdateWorshipTime = op.LastUpdate;
                        if (LastUpdateWorshipTime.Date != ZoneServerApi.now.Date)
                        {
                            OperateUpdateCharWorshipRecord update = new OperateUpdateCharWorshipRecord(Uid);
                            server.GameRedis.Call(update);
                            Worshiped[1] = false;
                            Worshiped[2] = false;
                            Worshiped[3] = false;
                            LastUpdateWorshipTime = ZoneServerApi.now.Date;
                        }
                        else
                        {
                            Worshiped[1] = op.rank1;
                            Worshiped[2] = op.rank2;
                            Worshiped[3] = op.rank3;
                        }
                    }
                    else
                    {
                        OperateUpdateCharWorshipRecord update = new OperateUpdateCharWorshipRecord(Uid);
                        server.GameRedis.Call(update);
                        LastUpdateWorshipTime = ZoneServerApi.now.Date;
                        Worshiped[1] = false;
                        Worshiped[2] = false;
                        Worshiped[3] = false;
                    }
                }
            }
            else
            {
                server.GameRedis.Call(op, ret =>
                {
                    if (op.got)
                    {
                        LastUpdateWorshipTime = op.LastUpdate;
                        if (LastUpdateWorshipTime.Date != ZoneServerApi.now.Date)
                        {
                            OperateUpdateCharWorshipRecord update = new OperateUpdateCharWorshipRecord(Uid);
                            server.GameRedis.Call(update);
                            Worshiped[1] = false;
                            Worshiped[2] = false;
                            Worshiped[3] = false;
                            LastUpdateWorshipTime = ZoneServerApi.now.Date;
                        }
                        else
                        {
                            Worshiped[1] = op.rank1;
                            Worshiped[2] = op.rank2;
                            Worshiped[3] = op.rank3;
                        }
                    }
                    else
                    {
                        OperateUpdateCharWorshipRecord update = new OperateUpdateCharWorshipRecord(Uid);
                        server.GameRedis.Call(update);
                        LastUpdateWorshipTime = ZoneServerApi.now.Date;
                        Worshiped[1] = false;
                        Worshiped[2] = false;
                        Worshiped[3] = false;
                    }
                });
            }
        }

        private void LoadElectionInfo()
        {
            OperateGetElectionScore op = new OperateGetElectionScore(MainId, (int)Camp, Uid);
            server.GameRedis.Call(op, ret =>
            {
                electionScore = op.Score;
            });
        }

        private void LoadPrestigeInfo()
        {
            OperateGetPrestige op = new OperateGetPrestige(MainId, (int)Camp, Uid);
            server.GameRedis.Call(op, ret =>
            {
                prestige = op.Prestige;
                //HisPrestige = op.HisPrestige;
            });
        }

        private void LoadCampRewardInfos()
        {
            OperateGetLastCampRewardPeriod op = new OperateGetLastCampRewardPeriod(Uid);
            server.GameRedis.Call(op, ret =>
             {
                 LastCampRewardPeriod = op.LastCampRewardPeriod;
             });
        }

        public MSG_ZGC_CAMP_BASE GetCampMsg()
        {
            MSG_ZGC_CAMP_BASE info = new MSG_ZGC_CAMP_BASE();
            info.Camp = (int)Camp;
            info.HisPrestige = HisPrestige;
            info.DragonLevel = DragonLevel;
            info.TigerLevel = TigerLevel;
            info.PhoenixLevel = PhoenixLevel;
            info.TortoiseLevel = TortoiseLevel;

            int titleLevel = CampStarsLibrary.GetCampTitleLevel(HisPrestige);
            info.TitleLevel = titleLevel;

            info.Blessing = GetCounterValue(CounterType.CampBlessingCount);
            Dictionary<int, WorshipRedisInfo> worships = null;
            if (Camp == CampType.TianDou)
            {
                worships = server.RelationServer.TianDouWorship;
            }
            else if (Camp == CampType.XingLuo)
            {
                worships = server.RelationServer.XingLuoWorship;
            }
            if (worships != null)
            {
                for (int i = 1; i <= 3; i++)
                {
                    MSG_ZGC_CAMP_WORSHIP_INFO worship = new MSG_ZGC_CAMP_WORSHIP_INFO();
                    WorshipRedisInfo temp = null;
                    if (worships.TryGetValue(i, out temp) && Worshiped.ContainsKey(i))
                    {
                        worship.PcUid = temp.Uid;
                        worship.Rank = temp.Rank;
                        worship.Name = temp.Name;
                        worship.ModelId = temp.ModelId;
                        worship.HeroId = temp.HeroId;
                        worship.GodType = temp.GodType;
                        worship.Worshiped = Worshiped[i];
                        info.Worships.Add(worship);
                    }
                }
            }
            return info;
        }

        private MSG_ZGC_CHOOSE_CAMP_RESULT GetWorship4CampChoose(MSG_ZGC_CHOOSE_CAMP_RESULT result)
        {
            Dictionary<int, WorshipRedisInfo> worships = null;
            if (Camp == CampType.TianDou)
            {
                worships = server.RelationServer.TianDouWorship;
            }
            else if (Camp == CampType.XingLuo)
            {
                worships = server.RelationServer.XingLuoWorship;
            }
            if (worships != null)
            {
                for (int i = 1; i <= 3; i++)
                {
                    MSG_ZGC_CAMP_WORSHIP_INFO worship = new MSG_ZGC_CAMP_WORSHIP_INFO();
                    WorshipRedisInfo temp = null;
                    if (worships.TryGetValue(i, out temp))
                    {
                        worship.PcUid = temp.Uid;
                        worship.Rank = temp.Rank;
                        worship.Name = temp.Name;
                        worship.ModelId = temp.ModelId;
                        worship.HeroId = temp.HeroId;
                        worship.GodType = temp.GodType;
                        worship.Worshiped = Worshiped[i];
                        result.Worships.Add(worship);
                    }
                }
            }

            return result;
        }

        public void SendBaseCampInfo()
        {
            MSG_ZGC_CAMP_BASE msg = GetCampMsg();
            Write(msg);
        }

        public void UpdateWorshipInfo()
        {
            OperateUpdateCharWorshipRecord update = new OperateUpdateCharWorshipRecord(Uid);
            server.GameRedis.Call(update);
            Worshiped[1] = false;
            Worshiped[2] = false;
            Worshiped[3] = false;
            LastUpdateWorshipTime = ZoneServerApi.now.Date;
        }
    }
}
