using DBUtility;
using EnumerateUtility;
using Logger;
using Message.Relation.Protocol.RZ;
using Message.Zone.Protocol.ZR;
using RedisUtility;
using ServerModels;
using ServerModels.Contribution;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class ContributionManager
    {
        public int PhaseNum { get; set; }
        public int CurrentValue { get; set; }
        private ZoneServerApi server { get; set; }
        public ContributionManager(ZoneServerApi server)
        {
            this.server = server;
            LoadContributionFromDB();
        }

        private void LoadContributionFromDB()
        {
            QueryLoadContribution query = new QueryLoadContribution();
            server.GameDBPool.Call(query, ret =>
            {
                if (query.phaseNum > 0)
                {
                    PhaseNum = query.phaseNum;
                    CurrentValue = query.currentValue;
                }
                else
                {
                    PhaseNum = 1;
                    CurrentValue = 0;
                }

            });
        }

        public void UpdateContributionInfo(int phaseNum, int currentValue)
        {
            PhaseNum = phaseNum;
            CurrentValue = currentValue;
        }
    }
}
