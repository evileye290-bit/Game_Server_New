using EnumerateUtility;
using Message.Corss.Protocol.CorssR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossServerLib
{
    public class CarnivalBossManager
    {
        private CrossServerApi server { get; set; }

        public CarnivalBossManager(CrossServerApi server)
        {
            this.server = server;           
        }

        public void Clear()
        {
            server.RankMng.ClearCarnivalBossRank();

            MSG_CorssR_CLEAR_VALUE msg = new MSG_CorssR_CLEAR_VALUE();
            msg.GiftType = (int)RechargeGiftType.CarnivalBoss;
            server.RelationManager.Broadcast(msg);
        }
    }
}
