using DataProperty;
using EnumerateUtility;
using ServerModels;

namespace ZoneServerLib
{
    public abstract class BaseItem
    {
        public ulong Uid { get; set; }//道具唯一id，主要用于区分一种道具有多个的情况

        public MainType MainType { get; set; }//主道具类型

        public int Id { get; set; }//道具id

        public int OwnerUid { get; set; }//道具所属pcid

        public int PileNum { get; set; }//数量

        public int GenerateTime { get; set; }

        public string PastDateTime { get; set; }

        public abstract bool BindData(int id);

        public BaseItem()
        {
        }

        public BaseItem(DBItemInfo itemInfo)
        {
            this.OwnerUid = itemInfo.OwnerUid;
            this.Uid = itemInfo.Uid;
            this.Id = itemInfo.TypeId;
            this.GenerateTime = itemInfo.GenerateTime;
            this.PileNum = itemInfo.PileNum;
            this.PastDateTime = "";
        }
    }
}
