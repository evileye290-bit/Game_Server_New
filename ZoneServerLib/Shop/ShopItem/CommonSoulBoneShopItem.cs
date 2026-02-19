using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class CommonSoulBoneShopItem : BaseShopItem
    {
        private SoulBone soulBone;
        public SoulBone SoulBone
        {
            get { return soulBone; }
            private set { soulBone = value; }
        }

        public CommonSoulBoneShopItem(int id, int buyCount, string info) : base(id, buyCount, info)
        {
        }


    }
}
