using DBUtility;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZR;
using RedisUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {
        public void SyncDbPlayerBattlePower()
        {
            int battlePower = HeroMng.CalcBattlePower();
            QuerySetBattlePower query = new QuerySetBattlePower(Uid, battlePower);
            server.GameDBPool.Call(query);
        }


        /// <summary>
        /// 同步战斗力到relation
        /// </summary>
        public void SyncBattlePowerToRelation()
        {
            MSG_ZGC_BATTLEPOWER msg = new MSG_ZGC_BATTLEPOWER();
            msg.BattePower = 2000;
            Write(msg);
        }


        public void GetBattlePowerRank(int page, int type)
        {
            //server.BattlePowerRank.PushRankListInfo(this, page);
            MSG_ZR_GET_RANK_LIST request = new MSG_ZR_GET_RANK_LIST();
            request.RankType = type;
            request.Page = page;
            server.SendToRelation(request, uid);
        }

    }
}
