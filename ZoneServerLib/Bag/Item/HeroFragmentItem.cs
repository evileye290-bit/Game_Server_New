using CommonUtility;
using EnumerateUtility;
using Message.Gate.Protocol.GateC;
using Message.Zone.Protocol.ZM;
using ServerModels;
using ServerShared;

namespace ZoneServerLib
{
    public class HeroFragmentItem : BaseItem
    {
        private HeroFragmentInfo itemInfo;
        private HeroFragmentModel itemModel;
        public HeroFragmentModel ItemModel
        { get { return this.itemModel; } }

        public HeroFragmentItem() { }

        public HeroFragmentItem(HeroFragmentInfo normalItemInfo) : base(normalItemInfo)
        {
            this.itemInfo = normalItemInfo;
            this.BindData(normalItemInfo.TypeId);
        }

        public override bool BindData(int id)
        {
            this.itemModel = BagLibrary.GetHeroFragmentModel(id);
            if (this.itemModel != null)
            {
                var data = this.itemModel.Data;
                this.MainType = (MainType)data.GetInt("MainType");

                return true;
            }
            else
            {
                Logger.Log.Warn($"have no this hreo Fragment model id {id}");
                return false;
            }
        }

        public ITEM GenerateSyncMessage()
        {
            ITEM syncMsg = new ITEM()
            {
                UidHigh = this.Uid.GetHigh(),
                UidLow = this.Uid.GetLow(),
                Id = this.Id,
                PileNum = this.PileNum,
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
            };
            return syncMsg;
        }

    }
}
