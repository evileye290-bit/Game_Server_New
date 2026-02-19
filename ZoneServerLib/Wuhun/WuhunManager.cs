using EnumerateUtility;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    class WuhunManager
    {

        private int level = 1;
        public int Level
        {
            get
            {
                return level;
            }
        }

        private int titalLevel = 0;
        public int TitalLevel
        {
            get
            {
                return titalLevel;
            }
        }

        private WuhunState state = 0;
        public WuhunState State
        {
            get
            {
                return state;
            }
        }


        public void Init(HeroInfo hero)
        {

            this.level = hero.Level;

            this.titalLevel = hero.TitleLevel;

            this.state = (WuhunState)hero.State;
        }
    }
}
