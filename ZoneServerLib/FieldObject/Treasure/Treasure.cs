using CommonUtility;
using DataProperty;
using EnumerateUtility;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class Treasure : FieldObject
    {
        private int zoneTreasureId;
        ///// <summary>
        ///// goods data name
        ///// </summary>
        public int ZoneTreasureId
        {
            get { return zoneTreasureId; }
        }

        override public TYPE FieldObjectType
        {
            get { return TYPE.TREASURE; }
        }

        // 动作ID
        private int animId { get; set; }

        internal Treasure(ZoneServerApi server) : base(server) { }

        internal void Init(FieldMap currentMap, Data zoneTreasureData)
        {
            this.zoneTreasureId = zoneTreasureData.ID;

            InitFSM();

            SetCurrentMap(currentMap);

            Vec2 position = new Vec2();
            position.x = zoneTreasureData.GetFloat("PosX");
            position.y = zoneTreasureData.GetFloat("PosZ");
            SetPosition(position);

            GenAngle = zoneTreasureData.GetInt("Angle");
            animId = zoneTreasureData.GetInt("anim");

            //int treasureId = zoneTreasureData.GetInt("TreasureId");
            //Data goodsData = DataListManager.inst.GetData("Goods", treasureId);
            //if (goodsData != null)
            //{
            //    radius = goodsData.GetFloat("Radius");
            //}
            //else
            //{
            //    radius = 1f;
            //}
        }

        /// <summary>
        /// 挖宝
        /// </summary>
        /// <param name="pc"></param>
        internal void OnClick(PlayerChar pc)
        {
            BaseItem item = pc.BagManager.GetItem(pc.ShovelTreasureMng.TreasureMapUid);
            if (item == null)
            {
                return;
            }
            if (item.Id == ShovelTreasureLibrary.HighTrerasureMap)
            {
                pc.Interact(this, 0, CommonConst.HIGH_TREASURE_DIG, "", animId, 0);
            }
            else
            {
                pc.Interact(this, 0, CommonConst.TREASURE_DIG, "", animId, 0);
            }  
        }
    }
}
