using EnumerateUtility;
using ServerModels;
using ServerShared;

namespace ZoneServerLib
{
    public class FashionItem : BaseItem
    {
        public int ActivateState { get; set; }//激活状态

        public int Announce { get; set; }//播报

        private int durationDay = 0;
        public int DurationDay
        {
            get { return durationDay; }
            set { durationDay = value; }
        }

        public int SonType { get; private set; }

        private FashionInfo fashionInfo;
        private FashionModel model;
        public FashionModel FashionModel
        {
            get { return this.model; } 
        }

        public FashionItem(FashionInfo fashionInfo) : base(fashionInfo)
        {
            this.fashionInfo = fashionInfo;
            this.MainType = MainType.Fashion;
            this.BindData(fashionInfo.TypeId);
        }

        public FashionItem(FashionModel model, FashionInfo fashionInfo) : base(fashionInfo)
        {
            this.model = model;
            this.MainType = MainType.Fashion;
            this.fashionInfo = fashionInfo;
        }

        public override bool BindData(int id)
        {
            this.model = BagLibrary.GetFashionModel(id);
            if (this.model != null)
            {
                var data = this.model.Data;

                //TODO 绑定差异数据
                return true;
            }
            else
            {
                Logger.Log.Warn($"have no this fashion model id {id}");
                return false;
            }
        }

    }
}
