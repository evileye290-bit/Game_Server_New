using CommonUtility;
using Google.Protobuf.Collections;
using Logger;
using Message.Relation.Protocol.RZ;
using Message.Zone.Protocol.ZR;
using ServerModels;
using ServerModels.Monster;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelationServerLib
{
    public class CampDungeon
    {
        public int Id;
        public int DungeonId;
        CampFort ownerFort;
        private DungeonModel dungeonData;
        public Defender DefenderInfo;

        public bool IsBeenHold;

        public CampDungeon(DungeonModel dungeonData, CampFort fort)
        {
            DungeonId = dungeonData.Id;
            this.dungeonData = dungeonData;
            DefenderInfo = null;
            ownerFort = fort;
            IsBeenHold = false;
        }

        internal void InitDefender(Defender defenderInfo)
        {
            DefenderInfo = defenderInfo;
            Id = defenderInfo.DefQueueId;
        }


        internal int GetPower()
        {
            return DefenderInfo == null ? 0 : DefenderInfo.TotalBattlePower;
        }

        internal CAMPDUNGEON Serialize()
        {
            CAMPDUNGEON info = new CAMPDUNGEON();
            info.DungeonId = DungeonId;
            info.QueueId = Id;
            info.DefenderInfo = DefenderInfo?.GetDefenderStruct();
            info.IsBeenHold = IsBeenHold;
            return info;
        }

        internal void Deserialize(CAMPDUNGEON dungeon)
        {
            DungeonId = dungeon.DungeonId;
            Id = dungeon.QueueId;
            IsBeenHold = dungeon.IsBeenHold;
            if (dungeon.DefenderInfo != null)
            {
                DefenderInfo = new Defender(dungeon.QueueId, dungeon.DefenderInfo.HeroList);
            }
        }


        internal void GiveUp()
        {
            DefenderInfo = null;
            IsBeenHold = false;
        }

        internal List<CAMP_CHALLENGER_HERO_INFO> GetDefenderHeroList()
        {
            if (DefenderInfo == null)
            {
                return new List<CAMP_CHALLENGER_HERO_INFO>();
            }
            return DefenderInfo.GetHeroList();
        }
    }
}
