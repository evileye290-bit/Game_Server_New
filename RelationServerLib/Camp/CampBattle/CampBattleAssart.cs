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


//    public class CampBattleAssart:AbstractCampWeekActivity
//    {
//        CampActivityManager manager;


//        public CampBattleAssart(CampActivityType type,RelationServerApi server, CampActivityManager campActivityManager) :base(type,server)
//        {
//            this.manager = campActivityManager;
//        }

//        public override void Init(int nowShowPhaseNum)
//        {
//            base.Init(nowShowPhaseNum);

//            if (nowShowPhaseNum > 0)
//            {
//                if (nowShowBegin <= RelationServerApi.now)
//                {
//                    manager.CurCampBattleStep = CampBattleStep.Assart;
//                }
//            }
//        }

//        protected override void DoBeginBusiness()
//        {
//            manager.Clear();
//            //
//            manager.CurCampBattleStep = CampBattleStep.Assart;
//            manager.FixNew();
//        }
//        protected override void DoEndBusiness()
//        {
//            manager.CurCampBattleStep = CampBattleStep.Rest;
//        }

//        public void UpdateCampBuildInfo()
//        {

//        }
//    }


//}
