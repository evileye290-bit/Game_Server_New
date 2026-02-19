using JumpPointSearch;
using ServerShared.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public partial class FieldMap : BaseMap
    {
        protected GridMap dynamicMap;
        protected GridMap rDynamicMap;
        protected virtual void InitDynamicGrid()
        {
            UseDynamicGrid = false;
        }

        public void EnableDynamicGrid(bool enable)
        {
            UseDynamicGrid = enable;
        }
        public void SetFieldObjectObstract(FieldObject fieldObject, bool obstract)
        {
            if(dynamicMap == null || rDynamicMap == null || fieldObject == null)
            {
                return;
            } 
            int geoX = (int)Math.Round(fieldObject.Position.x);
            int geoY = (int)Math.Round(fieldObject.Position.y);
            int r = (int)(fieldObject.Radius);

            int dynamicPaddedId = dynamicMap.GeoToPaddedId(geoX, geoY);
            dynamicMap.SetDynamicWalkable(dynamicPaddedId, r, !obstract);
            //dynamicMap.SetDynamicWalkable_Old(dynamicPaddedId, r, !obstract);

            int rDynamicPaddedId = dynamicMap.MapToRMapPaddedId(dynamicPaddedId, rDynamicMap);
            rDynamicMap.SetDynamicWalkable(rDynamicPaddedId, r, !obstract);
            //rDynamicMap.SetDynamicWalkable_Old(rDynamicPaddedId, r, !obstract);
        }

        public virtual void UpdateDynamicGrid()
        {
            return;
        }
    }
}
