using CommonUtility;
using DBUtility;
using EnumerateUtility;
using Google.Protobuf.Collections;
using Logger;
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

    public partial class CampActivityManager : AbstractCampWeekActivity
    {
        private int mainId;

        bool DbLoaded = false;
        bool RedisLoaded = false;
        bool Inited = false;

        public Dictionary<CampType, CampCoinManager> CampCoinDic = new Dictionary<CampType, CampCoinManager>();


        public CampActivityManager(RelationServerApi server, int mainId) : base(CampActivityType.Default, server)
        {
            this.server = server;
            this.mainId = mainId;

            battleBuildPhase = new CampBuild(CampActivityType.Build, server, this);

            battleAssartPhase = new CampBattle(CampActivityType.BattleAssart, server, this);
            battleFinalPhase = new CampBattle(CampActivityType.BattleFinal, server, this);

            CampCoinDic.Add(CampType.TianDou, new CampCoinManager(server, CampType.TianDou));
            CampCoinDic.Add(CampType.XingLuo, new CampCoinManager(server, CampType.XingLuo));

            campHoldCount.Add(CampType.TianDou, 0);
            campHoldCount.Add(CampType.XingLuo, 0);
            //campHoldCount.Add(CampType.None, 0);

            DbLoaded = false;
            RedisLoaded = false;
            LoadActivityDataFromXML();
            LoadAcitvityDataFromDB();

            InitRankList();

        }

        public bool CheckLoadedData()
        {
            return DbLoaded && RedisLoaded;
        }

        private void LoadAcitvityDataFromDB()
        {
            List<AbstractDBQuery> querys = new List<AbstractDBQuery>();

            QueryLoadCampActivityInfo queryActivityInfo = new QueryLoadCampActivityInfo();
            querys.Add(queryActivityInfo);
            QueryLoadCampCoins queryCampCount = new QueryLoadCampCoins();
            querys.Add(queryCampCount);

            DBQueryTransaction dBQuerysWithoutTransaction = new DBQueryTransaction(querys, true);

            server.GameDBPool.Call(dBQuerysWithoutTransaction, ret =>
            {
                LoadCampActivityInfo(queryActivityInfo);
                LoadCampCountData(queryCampCount);
                LoadCampBattleDataFromRedis();
                DbLoaded = true;
            });
        }

        private void LoadCampActivityInfo(QueryLoadCampActivityInfo queryActivityInfo)
        {
            nowShowPhaseNum = queryActivityInfo.activityInfo.PhaseNum;

            if (nowShowPhaseNum == -1)
            {
                nowShowPhaseNum = 1;
                QueryInsertCampActivityInfo query = new QueryInsertCampActivityInfo(nowShowPhaseNum);
                server.GameDBPool.Call(query);
            }
            else if (nowShowPhaseNum == 0)
            {
                nowShowPhaseNum = 1;
                QueryUpdateCampActivityPhase query = new QueryUpdateCampActivityPhase(nowShowPhaseNum);
                server.GameDBPool.Call(query);
            }
        }

        public void LoadCampCountData(QueryLoadCampCoins queryCampCount)
        {
            foreach (var item in queryCampCount.CampCoins)
            {
                CampType campType = (CampType)item.Key;
                foreach (var it in item.Value)
                {
                    CampCoinDic[campType].LoadCoin(it.Key, it.Value);
                }
            }
        }

        private void LoadCampBattleDataFromRedis()
        {
            OperateGetAllHistoricalMaxCampScore opMCS = new OperateGetAllHistoricalMaxCampScore(server.MainId);
            server.GameRedis.Call(opMCS, ret =>
            {

                if ((int)ret > 0)
                {
                    if (opMCS.uidScoreDic != null)
                    {
                        foreach (var item in opMCS.uidScoreDic)
                        {
                            if (!playerMaxScoreDic.ContainsKey(item.Key))
                            {
                                playerMaxScoreDic.Add(item.Key, item.Value);
                            }
                        }

                        //布局信息获取并填充
                        OperateGetCampFortInfo opFort = new OperateGetCampFortInfo(server.MainId, nowShowPhaseNum);
                        server.GameRedis.Call(opFort, r =>
                        {
                            if (opFort.FortArr.Length == 0)
                            {
                                FixNew();
                            }
                            else
                            {
                                foreach (var item in opFort.FortArr)
                                {
                                    DeserializeFort((int)item.Name, item.Value);
                                }
                            }

                            RedisLoaded = true;
                        });
                    }
                }
            });

        }

        private void UpdateActivityPhase2DB()
        {
            QueryUpdateCampActivityPhase query = new QueryUpdateCampActivityPhase(nowShowPhaseNum);
            server.GameDBPool.Call(query);
        }


        public void LoadActivityDataFromXML()
        {

            foreach (var item in CampActivityLibrary.campActivtyTimers)
            {
                switch (item.Key)
                {
                    case CampActivityType.Default:
                        LoadXMLData(item.Value);
                        break;
                    case CampActivityType.Build:
                        battleBuildPhase.LoadXMLData(item.Value);
                        break;
                    case CampActivityType.BattleAssart:
                        battleAssartPhase.LoadXMLData(item.Value);
                        break;
                    case CampActivityType.BattleFinal:
                        battleFinalPhase.LoadXMLData(item.Value);
                        break;
                    default:
                        break;
                }

            }
        }

        public void AddGrain(CampType camp, int addValue)
        {
            CampCoinManager campCoinMng;
            if (!CampCoinDic.TryGetValue(camp, out campCoinMng))
            {
                if (camp != CampType.None)
                {
                    campCoinMng = new CampCoinManager(server, camp);
                }
                return;
            }
            campCoinMng.AddCoin(CampCoin.Grain, addValue);
        }
        private void DelGrain(CampType camp, int subValue)
        {
            CampCoinManager campCoinMng;
            if (!CampCoinDic.TryGetValue(camp, out campCoinMng))
            {
                if (camp != CampType.None)
                {
                    campCoinMng = new CampCoinManager(server, camp);
                }
                return;
            }
            campCoinMng.DelCoin(CampCoin.Grain, subValue);
        }

        public void NotifyCampBoxCount(CampType camp)
        {
            int count = GetBoxCount(camp);

            foreach (var item in server.ZoneManager.ServerList)
            {
                ((ZoneServer)item.Value).SyncCampBoxCount(camp, count);
            }
        }
        /// <summary>
        /// 更新宝箱数目
        /// </summary>
        /// <param name="camp"></param>
        /// <param name="addValue"></param>
        public void AddBox(CampType camp, int addValue = 1)
        {
            CampCoinDic[camp].AddCoin(CampCoin.BoxCount, addValue);
        }

        public int GetBoxCount(CampType camp)
        {
            return CampCoinDic[camp].GetCoins(CampCoin.BoxCount);
        }

        public void AddCampBuildValue(CampType camp, int addValue = 1)
        {
            CampCoinDic[camp].AddCoin(CampCoin.BuildValue, addValue);
        }
        public int GetCampBuildValue(CampType camp)
        {
            return CampCoinDic[camp].GetCoins(CampCoin.BuildValue);
        }


        public void AddCampBattleScore(CampType camp, int addValue = 1)
        {
            CampCoinDic[camp].AddCoin(CampCoin.BattleScore, addValue);
        }
        public int GetCampBattleScore(CampType camp)
        {
            return CampCoinDic[camp].GetCoins(CampCoin.BattleScore);
        }


        internal MSG_RZ_CAMP_GRAIN GetGrain(CampType camp)
        {
            MSG_RZ_CAMP_GRAIN msg = new MSG_RZ_CAMP_GRAIN();
            int grain = 0;
            switch (camp)
            {
                case CampType.None:
                    foreach (var item in CampCoinDic)
                    {
                        grain = item.Value.GetCoins(CampCoin.Grain);
                        msg.GrainMap.Add((int)item.Key, grain);
                    }
                    break;
                case CampType.TianDou:
                case CampType.XingLuo:
                    grain = CampCoinDic[camp].GetCoins(CampCoin.Grain);
                    msg.GrainMap.Add((int)camp, grain);
                    break;
                default:
                    break;
            }

            return msg;
        }

        public void InitPhase(bool isUpdatexml = false)
        {
            if (isUpdatexml)
            {
                nowShowPhaseNum = 0;
            }
            base.Init(nowShowPhaseNum);

            battleBuildPhase.Init(nowShowPhaseNum);

            battleAssartPhase.Init(nowShowPhaseNum);
            battleFinalPhase.Init(nowShowPhaseNum);


            if (nowShowPhaseNum > 0)
            {
                if (isUpdatexml)
                {
                    UpdateActivityPhase2DB();
                }

                SyncCampActivityPhaseInfo2Zone();
            }
            Inited = true;
        }

        public override void SyncCampActivityPhaseInfo2Zone()
        {
            switch (CurCampBattleStep)
            {
                case CampBattleStep.Rest:
                    break;
                case CampBattleStep.Assart:
                    battleAssartPhase.SyncCampActivityPhaseInfo2Zone();
                    break;
                case CampBattleStep.Final:
                    battleFinalPhase.SyncCampActivityPhaseInfo2Zone();
                    break;
                default:
                    break;
            }
        }

        double totalsec = 0;
        double passSec = 5;
        public bool needUpdate = false;
        /// <summary>
        /// 此处update周期为1秒1帧
        /// </summary>
        /// <param name="dt"></param>
        public override void Update(double dt)
        {
            if (CheckLoadedData())
            {
                if (!Inited)
                {
                    InitPhase();
                }
                base.Update(dt);
                if (CheckWrongTime(RelationServerApi.now))
                {
                    return;
                }

                //foreach (var item in CampCoinDic)
                //{
                //    item.Value.Update();
                //}

                battleBuildPhase.Update(dt);

                battleAssartPhase.Update(dt);
                battleFinalPhase.Update(dt);

                foreach (var item in CampCoinDic)
                {
                    item.Value.Update();
                }

                if (totalsec < passSec)
                {
                    totalsec += 1;
                    return;
                }
                UpdateCampBattleData2Redis();
                totalsec = 0;
            }
        }


        private void UpdateCampBattleData2Redis()
        {
            ///FIXME:BOIL 这里是持久化到redis的操作。我在这里用needUpdate记录是否需要upload。本函数隔passSec秒运行一次。
            if (needUpdate)
            {
                Log.Write("update camp battle data to redis ");
                var fortsStrDic = SerializeAllForts();
                //布局信息持久化
                OperateRecordCampFortInfo operate = new OperateRecordCampFortInfo(server.MainId, nowShowPhaseNum, fortsStrDic);
                server.GameRedis.Call(operate);

                needUpdate = false;
            }
        }

        public void ClearCampCurStepInfo()
        {
            server.GameRedis.Call(new OperateClearCampBattleInfo(server.MainId, nowShowPhaseNum));
            foreach (var item in CampCoinDic)
            {
                item.Value.ClearCoin(CampCoin.BattleScore);
                item.Value.ClearCoin(CampCoin.Grain);
                //item.Value.ClearCoin(CampCoin.BuildValue);
            }
        }

        protected override void DoBeginBusiness()
        {
            UpdateActivityPhase2DB();
        }

        protected override void DoEndBusiness()
        {
        }


        public CampType GetWinCamp()
        {
            CampType winCamp = CampType.None;
            int tiandouFortCount = campHoldCount[CampType.TianDou];
            int xinluoFortCount = campHoldCount[CampType.XingLuo];

            if (tiandouFortCount < xinluoFortCount)
            {
                winCamp = CampType.XingLuo;
            }
            else if (tiandouFortCount > xinluoFortCount)
            {
                winCamp = CampType.TianDou;
            }
            return winCamp;
        }

        public void InitRankReward()
        {
            battleBuildPhase.InitRankReward();
        }

    }



}
