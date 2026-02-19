using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Relation.Protocol.RZ;
using RedisUtility;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelationServerLib
{
    public class HuntingRank : BaseRank
    {
        public HuntingRank(RelationServerApi server, RankType rankType = RankType.Hunting) : base(server, rankType)
        {
        }
    }
}
