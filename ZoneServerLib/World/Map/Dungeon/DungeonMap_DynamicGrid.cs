using JumpPointSearch;
using Logger;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public partial class DungeonMap : FieldMap
    {
        protected override void InitDynamicGrid()
        {
            //UseDynamicGrid = true;

            //GridMap gridMap = Model.JpsPathFinder.GetGridMap();
            //dynamicMap = new GridMap(gridMap);
            //dynamicMap.SetAllWalkable();

            //rDynamicMap = dynamicMap.CreateRMap(dynamicMap);
            //rDynamicMap.SetAllWalkable();
        }

        public override void UpdateDynamicGrid()
        {
            dynamicMap.SetAllWalkable();
            rDynamicMap.SetAllWalkable();

            //foreach(var kv in PcList)
            //{
            //    SetFieldObjectObstract(kv.Value, true);
            //}
            foreach(var kv in HeroList)
            {
                SetFieldObjectObstract(kv.Value, true);
            }
            foreach(var kv in MonsterList)
            {
                SetFieldObjectObstract(kv.Value, true);
            }
        }

    }
}
