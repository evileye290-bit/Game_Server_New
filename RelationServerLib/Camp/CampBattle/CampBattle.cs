using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBUtility;
using EnumerateUtility;
using Logger;
using Message.Relation.Protocol.RZ;
using RedisUtility;
using ServerShared;

namespace RelationServerLib
{
    public class InspireTimeHelp
    {
        /// <summary>
        /// 鼓舞开始时间  
        /// </summary>
        DateTime InspireHourStartTime;
        /// <summary>
        /// 整点鼓舞结束时间
        /// </summary>
        DateTime InspireHourEndTime;

        private int continuedMinutes;  //持续多久 注：这个时间必须小于半小时也就是 30分钟

        bool isHalf = false;

        public InspireTimeHelp(bool isHalf)
        {
            this.isHalf = isHalf;
            continuedMinutes =  CampBattleLibrary.GetAttributeIntensifyTimeSec()/60;
        }

        public void CalcInspireTime(DateTime now)
        {
            if (isHalf)
            {
                if (now.Minute - 30 >= continuedMinutes)
                {
                    InspireHourStartTime = now.Date.AddHours(now.Hour + 1).AddMinutes(30);
                }
                else
                {
                    InspireHourStartTime = now.Date.AddHours(now.Hour).AddMinutes(30); ;
                }
            }
            else
            {
                if (now.Minute >= continuedMinutes)
                {
                    InspireHourStartTime = now.Date.AddHours(now.Hour+1);
                }
                else
                {
                    InspireHourStartTime = now.Date.AddHours(now.Hour); ;
                }
            }

            InspireHourEndTime = InspireHourStartTime.AddMinutes(continuedMinutes);
        }

        public bool CheckInspireStart(DateTime now)
        {
            if (InspireHourStartTime <= now && now <= InspireHourEndTime)
            {
                return true;
            }
            return false;
        }


        public bool CheckInspireEnd(DateTime now)
        {
            if (InspireHourEndTime <= now)
            {
                return true;
            }
            return false;
        }

        public void UpdateStartTime()
        {
            InspireHourStartTime = InspireHourStartTime.AddHours(1);
        }

        public void UpdateEndTime()
        {
            InspireHourEndTime = InspireHourEndTime.AddHours(1);
        }
    }

    public class CampBattle : AbstractCampWeekActivity
    {
        CampActivityManager manager;
        InspireTimeHelp timeHelp1;

        public double PassTime { get; private set; }

        //InspireTimeHelp timeHelp2;


        public CampBattle(CampActivityType type, RelationServerApi server, CampActivityManager campActivityManager) : base(type, server)
        {
            this.manager = campActivityManager;
            timeHelp1 = new InspireTimeHelp(true);
            //timeHelp2 = new InspireTimeHelp(false);
        }


        public CampBattleStep GetBattleStep()
        {
            switch (type)
            {
                case CampActivityType.BattleAssart:
                    return CampBattleStep.Assart;
                case CampActivityType.BattleFinal:
                    return CampBattleStep.Final;
                default:
                    break;
            }
            return CampBattleStep.Rest;
        }


        public override void Init(int nowShowPhaseNum)
        {
            base.Init(nowShowPhaseNum);
            if (nowShowPhaseNum > 0)
            {
                if (nowShowBegin <= RelationServerApi.now)
                {
                    manager.CurCampBattleStep = GetBattleStep();
                }
                if (nowShowEnd <= RelationServerApi.now)
                {
                    manager.CurCampBattleStep = CampBattleStep.Rest;
                }
            }
            timeHelp1.CalcInspireTime(server.Now());
            //timeHelp2.CalcInspireTime(server.Now());
        }


        protected override void DoBeginBusiness()
        {
            manager.ClearCampCurStepInfo();
            manager.CurCampBattleStep = GetBattleStep();
            manager.needUpdate = true;

            foreach (var item in manager.CampCoinDic)
            {
                item.Value.SetCoin(CampCoin.Grain, CampLibrary.GrainInitValue);
            }

            foreach (var item in manager.AllForts)
            {
                item.Value.ResetFortNatureInfo();
            }
        }
        protected override void DoEndBusiness()
        {
            manager.CurCampBattleStep = CampBattleStep.Rest;
            manager.RecordCampBoxCount();
        }

        public override void Update(double dt)
        {
            base.Update(dt);
            if (CheckWrongTime(RelationServerApi.now))
            {
                return;
            }
            manager.CurCampBattleStep = GetBattleStep();
            if (timeHelp1.CheckInspireStart(RelationServerApi.now))
            {
                timeHelp1.UpdateStartTime();
                DoInspireStart();
            }

            //if (timeHelp2.CheckInspireStart(RelationServerApi.now))
            //{
            //    timeHelp2.UpdateStartTime();
            //    DoInspireStart();
            //}

            if (timeHelp1.CheckInspireEnd(RelationServerApi.now))
            {
                timeHelp1.UpdateEndTime();
                DoInspireEnd();
            }

            //if (timeHelp2.CheckInspireEnd(RelationServerApi.now))
            //{
            //    timeHelp2.UpdateEndTime();
            //    DoInspireEnd();
            //}

            WindUpHoldScore(dt);
        }

        /// <summary>
        /// 鼓舞开始
        /// </summary>
        private void DoInspireStart()
        {
            //属性加成添加
            int tiandouFortCount = manager.campHoldCount[CampType.TianDou];
            int xinluoFortCount = manager.campHoldCount[CampType.XingLuo];
            int count = xinluoFortCount - tiandouFortCount;
            if (count == 0)
            {
                manager.InspireCamp = CampType.None;
                manager.InspireDValue = 0;
                return;
            }
            Log.Debug("```````` 鼓舞开始```````````````");
            if (count > 0)
            {
                manager.InspireCamp = CampType.TianDou;
                manager.InspireDValue = count;
            }
            else
            {
                manager.InspireCamp = CampType.XingLuo;
                manager.InspireDValue = -count;
            }

            manager.RecordCampBattleInspireAnnouncement();
        }

        /// <summary>
        /// 鼓舞结束
        /// </summary>
        private void DoInspireEnd()
        {
            //属性加成减少
            Log.Debug("```````` 鼓舞结束```````````````");
            manager.InspireCamp = CampType.None;
            manager.InspireDValue = 0;
        }

        public void WindUpHoldScore(double dt)
        {
            int tiandouScore = 0;
            int xinluoScore = 0;
            int baseScore = CampBattleLibrary.GetCampBattleBaseCampScore();
            foreach (var item in manager.AllForts)
            {
                CampFort campFort = item.Value;
                int addScore = campFort.WindUpBattleScore(dt);

                CampType campType = campFort.CampType;
                if (addScore > 0)
                {
                    switch (campFort.CampType)
                    {
                        case CampType.TianDou:
                            tiandouScore = tiandouScore + addScore;
                            break;
                        case CampType.XingLuo:
                            xinluoScore = xinluoScore + addScore;
                            break;
                    }

                    int uid = campFort.GetDefenderUid();
                    if (uid > 0)
                    {
                        server.GameRedis.Call(new OperateIncrementCampScore(server.MainId, (int)campType, RankType.CampBattleScore, uid, addScore, server.Now()), ret =>
                        {
                            if ((int)ret == 1)
                            {
                                Client client = server.ZoneManager.GetClient(uid);
                                if (client == null)
                                {
                                    //不在线
                                    OperateGetCampScore operateGetCampScore = new OperateGetCampScore(server.MainId, (int)campType, RankType.CampBattleScore, uid);
                                    server.GameRedis.Call(operateGetCampScore, ret1 =>
                                    {
                                        if ((int)ret1 == 1)
                                        {
                                            int score1 = operateGetCampScore.Score;

                                            QueryUpdateCampScore query = new QueryUpdateCampScore(uid, score1);
                                            server.GameDBPool.Call(query);
                                        }
                                    });
                                    return;
                                }

                                MSG_RZ_CAMP_BATTLE_SCORE_ADD msg = new MSG_RZ_CAMP_BATTLE_SCORE_ADD();
                                msg.Uid = uid;
                                msg.AddScore = addScore;
                                client.Write(msg);
                            }
                        });
                    }
                }
            }
            if (dt < 1000)
            {
                PassTime = PassTime + 1000;
            }
            else
            {
                PassTime = PassTime + dt;
            }

            if (PassTime > 60000)
            {
                PassTime = 0;
                tiandouScore = tiandouScore + baseScore;
                xinluoScore = xinluoScore + baseScore;
            }

            manager.AddCampBattleScore(CampType.TianDou, tiandouScore );
            manager.AddCampBattleScore(CampType.XingLuo, xinluoScore );
        }
    }


}
