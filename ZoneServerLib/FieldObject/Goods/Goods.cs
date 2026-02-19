using CommonUtility;
using DataProperty;
using EnumerateUtility;
using Message.Gate.Protocol.GateC;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public partial class Goods : FieldObject
    {

        private int zoneGoodsId;
        /// <summary>
        /// goods data name
        /// </summary>
        public int ZoneGoodsId
        {
            get { return zoneGoodsId; }
        }

        override public TYPE FieldObjectType
        {
            get { return TYPE.GOODS; }
        }

        // 动作ID
        private int animId { get; set; }

        internal Goods(ZoneServerApi server) : base(server) { }

        internal void Init(FieldMap currentMap, Data zoneGoodsData)
        {
            this.zoneGoodsId = zoneGoodsData.ID;

            InitFSM();

            SetCurrentMap(currentMap);

            Vec2 position = new Vec2();
            position.x = zoneGoodsData.GetFloat("PosX");
            position.y = zoneGoodsData.GetFloat("PosZ");
            SetPosition(position);

            GenAngle = zoneGoodsData.GetInt("Angle");
            animId = zoneGoodsData.GetInt("anim");

            int goodsId = zoneGoodsData.GetInt("GoodsId");
            Data goodsData = DataListManager.inst.GetData("Goods", goodsId);
            if (goodsData != null)
            {
                radius = goodsData.GetFloat("Radius");
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
            pc.Interact(this, ZoneGoodsId, CommonConst.GOODS_COLLECT, "", animId, 0);
        }


        public MSG_ZGC_GOODS_INFO GetGoodsPacketInfo()
        {
            MSG_ZGC_GOODS_INFO info = new MSG_ZGC_GOODS_INFO();
            info.InstanceId = InstanceId;
            info.ZoneGoodsId = ZoneGoodsId;
            info.X = Position.x;
            info.Y = Position.y;
            info.Angle = GenAngle;
            return info;
        }

    }
}