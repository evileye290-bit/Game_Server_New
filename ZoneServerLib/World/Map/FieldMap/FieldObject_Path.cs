using CommonUtility;
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
        public Vec2[] GetPath_New(Vec2 from, Vec2 to, bool useDynamic)
        {
            Vec2[] result = null;
            int startX = (int)Math.Round(from.x);
            int startY = (int)Math.Round(from.y);
            int endX = (int)Math.Round(to.x);
            int endY = (int)Math.Round(to.y);
            Stack<KeyValuePair<int, int>> path = null;
            if(useDynamic)
            {
                Model.JpsPathFinder.SetDynamicGrid(dynamicMap, rDynamicMap);
                //dynamicMap.PrintUnwalkable();
            }
            else
            {
                Model.JpsPathFinder.SetDynamicGrid(null, null);
            }
            path = Model.JpsPathFinder.FindPath(new KeyValuePair<int, int>(startX, startY), new KeyValuePair<int, int>(endX, endY), true);
            // 重置动态格挡信息
            Model.JpsPathFinder.SetDynamicGrid(null, null);

            if (path == null)
            {
                // 小网格未找到，使用大网格不考虑动态阻挡
                Model.JpsPathFinderBig.SetDynamicGrid(null, null);
                path = Model.JpsPathFinderBig.FindPath(new KeyValuePair<int, int>(startX, startY), new KeyValuePair<int, int>(endX, endY), true);
                if (path == null)
                {
                    return null;
                }
            }

            KeyValuePair<int, int>[] arr = path.ToArray();
            result = new Vec2[arr.Length];
            for (int i = 0; i < arr.Length; i++)
            {
                result[i] = new Vec2(arr[i].Key, arr[i].Value);
            }

            //string pathStr = string.Empty;
            //for (int i = 0; i < result.Length; i++)
            //{
            //    pathStr += " " + result[i].ToString() + "";
            //}
            //Logger.Log.Warn($"find path {pathStr}");
            return result;
        }
    }
}
