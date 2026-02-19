using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    partial class Hero
    {
        private WuhunManager WuhunMng { get; set; }

        public void InitWuhunManager(HeroInfo hero)
        {
            WuhunMng = new WuhunManager();
            WuhunMng.Init(hero);
        }
    }
}
