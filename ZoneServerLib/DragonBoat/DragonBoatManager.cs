using DBUtility;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class DragonBoatManager
    {
        public PlayerChar Owner { get; private set; }
        private DragonBoatInfo info = new DragonBoatInfo();
        public DragonBoatInfo Info { get { return info; } }

        public DragonBoatManager(PlayerChar owner)
        {
            Owner = owner;
        }

        public void Init(DragonBoatInfo info)
        {
            this.info = info;
        }

        public MSG_ZGC_DRAGON_BOAT_INFO GenerateDragonBoatInfo()
        {
            MSG_ZGC_DRAGON_BOAT_INFO msg = new MSG_ZGC_DRAGON_BOAT_INFO();
            msg.CurDistance = Info.CurDistance;
            msg.Bought = Info.Bought;
            msg.FreeTicketState = Info.FreeTicketState;
            return msg;
        }

        public void UpdateCurDistance(int addDistance)
        {
            if (info.CurDistance + addDistance > DragonBoatLibrary.MaxDistance)
            {
                info.CurDistance = DragonBoatLibrary.MaxDistance;
            }
            else
            {
                info.CurDistance += addDistance;
            }
            SyncDbUpdateDragonBoatCurDistance();
        }

        public void UpdateBuyInfo()
        {
            info.Bought = 1;
            SyncDbUpdateDragonBoatBuyInfo();
        }

        public void Clear()
        {
            info.CurDistance = 0;
            info.Bought = 0;
            info.FreeTicketState = 0;
            MSG_ZGC_DRAGON_BOAT_INFO msg = GenerateDragonBoatInfo();
            Owner.Write(msg);
        }

        public void UpdateFreeTicketState(int state)
        {
            info.FreeTicketState = state;
            SyncDbUpdateDragonBoatFreeTicketState();
        }

        private void SyncDbUpdateDragonBoatCurDistance()
        {
            Owner.server.GameDBPool.Call(new QueryUpdateDragonBoatCurDistance(Owner.Uid, Info.CurDistance));
        }

        private void SyncDbUpdateDragonBoatBuyInfo()
        {
            Owner.server.GameDBPool.Call(new QueryUpdateDragonBoatBuyInfo(Owner.Uid, Info.Bought));
        }

        private void SyncDbUpdateDragonBoatFreeTicketState()
        {
            Owner.server.GameDBPool.Call(new QueryUpdateDragonBoatFreeTicketState(Owner.Uid, Info.FreeTicketState));
        }

        public void LoadDragonBoatTransform(MSG_ZMZ_DRAGON_BOAT_INFO msg)
        {
            info.CurDistance = msg.CurDistance;
            info.Bought = msg.Bought;
            info.FreeTicketState = msg.FreeTicketState;
        }
    }
}
