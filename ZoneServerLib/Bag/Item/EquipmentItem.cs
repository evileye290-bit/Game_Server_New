using CommonUtility;
using EnumerateUtility;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using ServerModels;
using ServerShared;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public class EquipmentItem : BaseItem
    {
        private EquipmentModel model;
        private EquipmentInfo equipInfo;

        public EquipmentModel Model { get { return model; } }

        public EquipmentInfo EquipInfo
        {
            get { return equipInfo; }
        }

        public bool Deleted;

        public EquipmentItem(EquipmentInfo equipInfo):base(equipInfo)
        {
            this.equipInfo = equipInfo;
            this.MainType = MainType.Equip;
            this.BindData(equipInfo.TypeId);
        }

        public EquipmentItem(EquipmentInfo equipInfo, bool delete) : this(equipInfo)
        {
            this.equipInfo = equipInfo.Clone();
            Deleted = true;
            this.equipInfo.EquipHeroId = -1;
        }

        public override bool BindData(int id)
        {
            this.model = EquipLibrary.GetEquipModel(id);
            if (this.model != null)
            {
                //TODO 绑定差异数据
                return true;
            }
            else
            {
                Logger.Log.Warn($"have no this faceframe model id {id}");
                return false;
            }
        }

        //做装备功能的时候需要修改为对应的装备同步流
        public MSG_ZGC_ITEM_EQUIPMENT GenerateSyncMessage()
        {
            MSG_ZGC_ITEM_EQUIPMENT syncMsg = new MSG_ZGC_ITEM_EQUIPMENT()
            {
                UidHigh = this.Uid.GetHigh(),
                UidLow = this.Uid.GetLow(),
                Id = this.Id,
                PileNum = this.PileNum,
                ActivateState = 0,
                GenerateTime = this.GenerateTime,
                EquipedHeroId =this.equipInfo.EquipHeroId,
                PartType = model.Part,
                Deleted=this.Deleted,
            };
            return syncMsg;
        }

        public MSG_ZGC_ITEM_EQUIPMENT GenerateSyncShowMessage()
        {
            MSG_ZGC_ITEM_EQUIPMENT syncMsg = new MSG_ZGC_ITEM_EQUIPMENT()
            {
                Id = this.Id,
                EquipedHeroId = this.equipInfo.EquipHeroId,
                PartType = model.Part,
            };
            return syncMsg;
        }

        public ZMZ_EQUIPMENT GenerateTransformMessage()
        {
            ZMZ_EQUIPMENT syncMsg = new ZMZ_EQUIPMENT()
            {
                Uid = this.Uid,
                Id = this.Id,
                PileNum = this.PileNum,
                EquipHeroId = this.equipInfo.EquipHeroId,
            };
            return syncMsg;
        }

        public EquipmentItem GenerateDeleteInfo(EquipmentInfo equipInfo)
        {
            EquipmentItem item = new EquipmentItem(equipInfo, true);        
            return item;
        }
    }
}
