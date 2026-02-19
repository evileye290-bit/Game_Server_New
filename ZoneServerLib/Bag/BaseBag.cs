using EnumerateUtility;
using System.Collections.Generic;
using ServerModels;
using System;

namespace ZoneServerLib
{
    public abstract class BaseBag
    {
        public BagType Type { get; }

        public BagManager Manager { get; private set; }

        public BaseBag(BagManager manager, BagType type)
        {
            this.Type = type;
            this.Manager = manager;
        }
        
        public abstract BaseItem GetItem(ulong uid);
        public virtual BaseItem GetItem(int id)
        {
            return null;
        }

        public abstract List<BaseItem> AddItem(int typeId, int num);

        public abstract BaseItem DelItem(ulong uid, int num);

        public abstract bool RemoveItem(BaseItem baseItem);

        public abstract void UpdateItem(BaseItem baseItem);

        public abstract void Clear();

        public abstract Dictionary<ulong, BaseItem> GetAllItems();

        public abstract void InsertItemToDb(BaseItem baseItem);
        public abstract void DeleteItemFromDb(BaseItem baseItem);
        public abstract void SyncDbItemInfo(BaseItem baseItem);

        //不占用背包空间的不需要继承该方法
        public virtual int ItemsCount()
        {
            return 0;
        }

        public virtual void DisposePastData()
        {
            //不同背包做不同处理
            //如果不过期则不需要重载该方法
        }

        public virtual bool CheckPastData(BaseItem baseItem)
        {
            //不同背包做不同处理
            //如果不过期则不需要重载该方法

            return false;
        }

        /// <summary>
        /// 将背包放不下的物品放入到邮件
        /// </summary>
        protected int Add2MailAndReturnAddBagNum(RewardType rewardType, int id, int num, int attr1 = 0, int attr2 = 0, int attr3 = 0, int attr4 = 0)
        {
            int add2MailNum = 0;
            int restBagSpace = this.Manager.GetBagRestSpace();
            if (restBagSpace <= 0)
            {
                //没空间了所有都放入邮件
                add2MailNum = num;
            }
            else
            {
                if (restBagSpace < num)
                {
                    add2MailNum = num - restBagSpace;
                }
            }

            if (add2MailNum > 0)
            {
                this.Manager.SendItem2Mail((int)rewardType, id, add2MailNum,attr1, attr2, attr3, attr4);//放入邮件中的
            }

            return Math.Max(0, num - add2MailNum);//需要放入背包中的
        }
    }
}
