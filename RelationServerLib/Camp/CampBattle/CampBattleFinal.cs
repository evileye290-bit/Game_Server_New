//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using EnumerateUtility;
//using Logger;
//using ServerShared;

//namespace RelationServerLib
//{
//    public partial class CampBattleFinal : AbstractCampWeekActivity
//    {
//        CampActivityManager manager;

//        public string SpecialEnd = "";

//        public CampBattleFinal(CampActivityType type, RelationServerApi server, CampActivityManager campActivityManager) : base(type, server)
//        {
//            manager = campActivityManager;
//        }

//        public override void Init(int nowShowPhaseNum)
//        {
//            base.Init(nowShowPhaseNum);
//            if (nowShowPhaseNum > 0)
//            {
//                if (nowShowBegin <= RelationServerApi.now)
//                {
//                    manager.CurCampBattleStep = CampBattleStep.Final;
//                }
//                if (manager.CurCampBattleStep == CampBattleStep.Final)
//                {
//                    foreach (var item in manager.AllForts)
//                    {
//                        item.Value.SubDungeonDic.Clear();
//                    }
//                }
//            }
//        }

//        protected override void DoBeginBusiness()
//        {
//            manager.CurCampBattleStep = CampBattleStep.Final;

//            foreach (var item in manager.AllForts)
//            {
//                item.Value.MainDungeon.ResetMonster();
//                item.Value.SubDungeonDic.Clear();
//                item.Value.ResetFinalProgress();
//                manager.BattleProgressIncrement(item.Value.CampType);
//            }

//            //ClearRewardList();
//            manager.needUpdate = true;
//        }

//        protected override void DoEndBusiness()
//        {
//            BroadcastAnnouncementEnd();
//            //InitRankList();
//            manager.CurCampBattleStep = CampBattleStep.Rest;
//            SpecialEnd = "";
//        }
//        private void BroadcastAnnouncementEnd()
//        {
//            foreach (var item in server.ZoneManager.ServerList)
//            {
//                ((ZoneServer)item.Value).NotifyCampBattleEnd((int)manager.GetWinCamp());
//            }
//        }



//        public override void Update(double dt)
//        {
//            base.Update(dt);
//            //SendRewardUpdate();
//            if (CheckWrongTime(RelationServerApi.now))
//            {
//                return;
//            }

//            bool needEnd = false;
//            foreach (var item in manager.GetBattleProgressDic())
//            {
//                if (item.Value == 0)
//                {
//                    needEnd = true;
//                    break;
//                }
//            }

//            if (needEnd)
//            {
//                DateTime specialEndTime;
//                if (SpecialEnd == "")
//                {
//                    int endSec = 3600;
//                    var config = CampBattleLibrary.GetCampBattleExpend((int)manager.CurCampBattleStep);
//                    if (config != null)
//                    {
//                        endSec = config.BattleSpecialEndSpenTime;
//                    }

//                    specialEndTime = server.Now().AddSeconds(endSec);
//                    SpecialEnd = specialEndTime.ToString();
//                }
//                else
//                {
//                    specialEndTime = DateTime.Parse(SpecialEnd);
//                }

//                if (server.Now() > specialEndTime)
//                {
//                    DoEndBusiness();
//                }
//            }
//            else
//            {
//                SpecialEnd = "";
//            }

//        }

//    }
//}
