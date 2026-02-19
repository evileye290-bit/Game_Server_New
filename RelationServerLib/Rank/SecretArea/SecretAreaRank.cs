using CommonUtility;
using EnumerateUtility;
using Logger;
using Message.Relation.Protocol.RZ;
using RedisUtility;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RelationServerLib
{
    public class SecretAreaRank : BaseRank
    {
        public SecretAreaRank(RelationServerApi server,RankType rankType = RankType.SecretArea) : base(server,rankType)
        {
        }
    }

}
