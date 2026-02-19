using CommonUtility;
using DataProperty;
using EnumerateUtility;
using Message.Gate.Protocol.GateC;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public class PropBook : FieldObject
    {
        private int transferMapID;
        private int zonePropId;
        /// <summary>
        /// goods data name
        /// </summary>
        public int ZonePropId
        {
            get { return zonePropId; }
        }

        override public TYPE FieldObjectType
        {
            get { return TYPE.PROPBOOK; }
        }

        internal PropBook(ZoneServerApi server) : base(server) { }

        internal void Init(FieldMap currentMap, Data zonePropData)
        {
            this.zonePropId = zonePropData.ID;

            //InitFSM();

            SetCurrentMap(currentMap);

            Vec2 position = new Vec2();
            position.x = zonePropData.GetFloat("PosX");
            position.y = zonePropData.GetFloat("PosZ");
            SetPosition(position);

            //GenAngle = zoneGoodsData.GetInt("Angle");

            transferMapID = zonePropData.GetInt("TransId");
            int propId = zonePropData.GetInt("PropID");
            Data propData = DataListManager.inst.GetData("Prop", propId);
            if (propData != null)
            {
                radius = propData.GetFloat("Radius");
            }
            else
            {
                radius = 1f;
            }
        }

        /// <summary>
        /// 点击Goods
        /// </summary>
        /// <param name="pc"></param>
        internal void OnClick(PlayerChar pc)
        {
            pc.Interact(this, ZonePropId, CommonConst.PROP_BOOK, "", transferMapID, 0);
        }
    }
}
