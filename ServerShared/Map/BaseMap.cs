using CommonUtility;
using DataProperty;
using EnumerateUtility;
using EpPathFinding;
using Logger;
using ServerModels;
using System;
using System.Collections.Generic;
using System.IO;

namespace ServerShared.Map
{
    public class BaseMap
    {
        #region 属性
        public int TokenId = 1;

        private int mapUid = 0;
        /// <summary>
        /// 地图标识符
        /// </summary>
        public int MapUid
        {
            get { return mapUid; }
            set { mapUid = value; }
        }

        /// <summary>
        /// 地图ID
        /// </summary>
        public int MapId
        {
            get { return model.MapId; }
        }

        private int channel = 0;
        /// <summary>
        /// 线路
        /// </summary>
        public int Channel
        {
            get { return channel; }
        }

        private MapModel model;
        public MapModel Model
        { get { return model; } }

        /// <summary>
        /// 最大玩家数
        /// </summary>
        public int MaxNum { get { return model.MaxNum; } }

        //private Data data;
        ///// <summary>
        ///// 源数据
        ///// </summary>
        //public Data MapData
        //{
        //    get { return data; }
        //}

        public Vec2 BeginPosition { get { return model.BeginPos; } }

        //边界值
        public int MinX { get { return model.MinX; } }
        public int MaxX { get { return model.MaxX; } }
        public int MinY { get { return model.MinY; } }
        public int MaxY { get { return model.MaxY; } }

        //格子
        private BaseGrid grid;
        private BaseGrid gridBig;
        private JumpPointParam jpParam;
        private JumpPointParam jpParamBig;

        /// <summary>
        /// 是高精度（格子长为默认的0.5）
        /// </summary>
        public bool HighPrecision { get { return model.HighPrecision; } }
                 
        public PvpType PVPType
        { get { return model.PvpType; } }

        public AOIType AoiType
        { get { return model.AoiType; } }
        #endregion


        public BaseMap(int mapId, int channel)
        {
            model = MapLibrary.GetMap(mapId);
            if(model == null)
            {
                Log.Warn("create map {0} channel {1} failed: no such map model", mapId, channel);
                return;
            }
            this.channel = channel;
            grid = model.Grid;
            gridBig = model.GridBig;
            jpParam = model.JpParam;
            jpParamBig = model.JpParamBig;
        }

        public bool IsWalkableAt(int x, int y, bool useBig = false)
        {
            if (useBig)
            {
                if (gridBig != null)
                    return gridBig.IsWalkableAt(x, y);
                else return false;
            }
            else
            {
                if (grid != null)
                    return grid.IsWalkableAt(x, y);
                else return false;
            }
        }

        public bool CheckPath(Vec2 from, Vec2 to, bool useBig = false)
        {
            GridPos start = VectorToGridPos(from);
            GridPos end = VectorToGridPos(to);
            List<GridPos> resultPath = null;
            JumpPointParam tempPointParam;
            if (useBig) tempPointParam = jpParamBig;
            else tempPointParam = jpParam;
            if (tempPointParam != null)
            {
                tempPointParam.Reset(start, end);
                resultPath = JumpPointFinder.FindPath(tempPointParam);
                // not found path
                if (resultPath == null || resultPath.Count == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        public Vec2[] GetPath(Vec2 from, Vec2 to, bool useBig = false)
        {
            GridPos start = VectorToGridPos(from);
            GridPos end = VectorToGridPos(to);
            List<GridPos> resultPath = null;
            Vec2[] result = null;
            JumpPointParam tempPointParam;
            if (useBig)
            {
                tempPointParam = jpParamBig;
            }
            else
            {
                tempPointParam = jpParam;
            }
            if (tempPointParam != null)
            {
                tempPointParam.Reset(start, end);
                resultPath = JumpPointFinder.FindPath(tempPointParam);

                // not found path
                if (resultPath == null || resultPath.Count == 0)
                {
                    //Log.Warn("map {0} 当前寻路没有找到路径！！！ ------ 当前目标点： {1}" , mapID, from.ToString());
                    return new Vec2[] { from, from };
                }

                List<int> removeList = CheckDirect(0, resultPath);
                removeList.Reverse();
                removeList.ForEach(index => resultPath.RemoveAt(index));

                if (resultPath.Count == 1)
                {
                    result = new Vec2[] { from, to };
                }
                else
                {
                    result = resultPath.ConvertAll<Vec2>(gridPos => new Vec2(gridPos.x, gridPos.y)).ToArray();
                    result[0] = from;
                    result[result.Length - 1] = to;
                }
                return result;
            }
            return null;
        }

        private GridPos VectorToGridPos(Vec2 source)
        {
            return new GridPos((int)Math.Round(source.x), (int)Math.Round(source.y));
        }

        private List<int> CheckDirect(int startIndex, List<GridPos> resultPath)
        {
            List<int> removeIndexList = new List<int>();
            int nextStartIndex = startIndex;

            for (int i = startIndex + 2; i < resultPath.Count; i++)
            {
                nextStartIndex = i - 1;
                if (IsDirect(resultPath[startIndex], resultPath[i]))
                {
                    removeIndexList.Add(i - 1);
                }
                else
                {
                    break;
                }
            }

            if (nextStartIndex + 2 < resultPath.Count)
                removeIndexList.AddRange(CheckDirect(nextStartIndex, resultPath));

            return removeIndexList;
        }

        private bool IsDirect(GridPos startPos, GridPos endPos, bool useBig = false)
        {
            // find range x,z
            int min_x = 0, min_y = 0, max_x = 0, max_y = 0;

            if (startPos.x < endPos.x) { min_x = startPos.x; max_x = endPos.x; }
            else { min_x = endPos.x; max_x = startPos.x; }

            if (startPos.y < endPos.y) { min_y = startPos.y; max_y = endPos.y; }
            else { min_y = endPos.y; max_y = startPos.y; }

            // find intersectionGrid
            for (int x = min_x; x < max_x + 1; x++)
            {
                for (int y = min_y; y < max_y + 1; y++)
                {
                    GridPos line = new GridPos(endPos.x - startPos.x, endPos.y - startPos.y);
                    if (!useBig)
                    {
                        if (IntersectionGrid(line, x - startPos.x, y - startPos.y) && grid.IsWalkableAt(x, y) == false)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (IntersectionGrid(line, x - startPos.x, y - startPos.y) && gridBig.IsWalkableAt(x, y) == false)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private bool IntersectionGrid(GridPos line, int x, int y)
        {
            float a = line.y;
            float b = -line.x;

            float result = ((a * x + b * y) * (a * x + b * y)) / (a * a + b * b);

            if (result < 0.5f)
                return true;
            else return false;
        }
    }

}
