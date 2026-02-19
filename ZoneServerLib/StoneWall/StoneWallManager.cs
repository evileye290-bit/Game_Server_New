using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class StoneWallManager
    {
        private Dictionary<int, StoneWallInfo> infoList = new Dictionary<int, StoneWallInfo>();
        public Dictionary<int, StoneWallInfo> InfoList { get { return infoList; } }
        private PlayerChar owner { get; set; }

        public StoneWallManager(PlayerChar owner)
        {
            this.owner = owner;
        }

        public void Init(List<StoneWallInfo> infoList)
        {
            foreach (var info in infoList)
            {
                this.infoList.Add(info.Type, info);
            }
        }

        public StoneWallInfo GetStoneWallInfo(int type)
        {
            StoneWallInfo info;
            infoList.TryGetValue(type, out info);
            return info;
        }

        public void AddStoneWallInfo(StoneWallInfo info)
        {
            infoList.Add(info.Type, info);
        }

        public void Clear()
        {
            foreach (var info in infoList)
            {
                info.Value.Clear();
            }
        }
    }
}
