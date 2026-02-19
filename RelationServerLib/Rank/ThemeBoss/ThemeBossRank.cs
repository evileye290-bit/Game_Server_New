using EnumerateUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelationServerLib
{
    public class ThemeBossRank : BaseRank
    {
        public ThemeBossRank(RelationServerApi server, RankType rankType = RankType.ThemeBoss) : base(server, rankType)
        {
        }
    }
}
