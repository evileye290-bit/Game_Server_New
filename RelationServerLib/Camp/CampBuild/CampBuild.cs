using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnumerateUtility;
using Logger;
using Message.Relation.Protocol.RZ;
using RedisUtility;
using ServerShared;

namespace RelationServerLib
{
    public partial class CampBuild : AbstractCampWeekActivity
    {
        CampActivityManager manager;


        public CampBuild(CampActivityType type, RelationServerApi server, CampActivityManager campActivityManager) : base(type, server)
        {
            this.manager = campActivityManager;
        }

        protected override void DoBeginBusiness()
        {

        }

        protected override void DoEndBusiness()
        {
            //InitRankList();
            //foreach (var item in manager.CampCoinDic)
            //{
            //    item.Value.ClearCoin(CampCoin.BuildValue);
            //}
        }

        public void InitRankReward()
        {
            InitRankList();
            foreach (var item in manager.CampCoinDic)
            {
                item.Value.ClearCoin(CampCoin.BuildValue);
            }
        }

        public override void Update(double dt)
        {
            base.Update(dt);
            SendRewardUpdate();
            if (CheckWrongTime(RelationServerApi.now))
            {
                return;
            }
        }


    }
}
