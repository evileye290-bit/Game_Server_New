using CommonUtility;
using EnumerateUtility;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using ServerModels;
using ServerShared;

namespace ZoneServerLib
{
    public class NormalItem : BaseItem
    {
        private int subType;
        public int SubType => subType;

        public EquipIndexType EquipIndexType { get; set; }//装备类型


        private ItemInfo itemInfo;
        private ItemModel itemModel;
        public ItemModel ItemModel
        { get { return this.itemModel; } }

        public NormalItem(ItemInfo normalItemInfo) : base(normalItemInfo)
        {
            this.itemInfo = normalItemInfo;
            this.BindData(normalItemInfo.TypeId);
        }

        public override bool BindData(int id)
        {
            this.itemModel = BagLibrary.GetItemModel(id);
            if (this.itemModel != null)
            {
                this.MainType = itemModel.MainType;
                this.subType = itemModel.SubType;
                return true;
            }

            Logger.Log.Warn($"have no this normal item model id {id}");
            return false;
        }

        public ITEM GenerateSyncMessage()
        {
            ITEM syncMsg = new ITEM()
            {
                UidHigh = this.Uid.GetHigh(),
                UidLow = this.Uid.GetLow(),
                Id = this.Id,
                PileNum = this.PileNum,
                ActivateState = 0,
                GenerateTime = this.GenerateTime,
            };
            return syncMsg;
        }

        public ZMZ_ITEM GenerateTransformMessage()
        {
            ZMZ_ITEM syncMsg = new ZMZ_ITEM()
            {
                Uid = this.Uid,
                Id = this.Id,
                PileNum = this.PileNum,
                ActivateState = 0,
                GenerateTime = this.GenerateTime,
            };
            return syncMsg;
        }

    }
}
