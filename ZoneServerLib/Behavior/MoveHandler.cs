using CommonUtility;
using EnumerateUtility;
using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    class MoveHandler
    {
        FieldObject owner;

        FieldMap currentMap
        { get { return owner.CurrentMap; } } 

        Region curRegion
        {
            get { return owner.CurRegion; }
        }

        public MoveHandler(FieldObject owner)
        {
            this.owner = owner;
        }

        public void InitMoveSpeed()
        {
            // 通知新格子的角色我的移动信息
            switch (owner.FieldObjectType)
            {
                case TYPE.NPC:
                    //MoveSpeed = GetInt("SPD");
                    break;
                default:
                    //speed = Nature.Speed;
                    break;
            }
        }

        /// <summary>
        /// 当前位置点
        /// </summary>
        //Vec2.zero;
        Vec2 curPosition = new Vec2(Vec2.zero.Y, Vec2.zero.Y);
        Vec2 curPositionCopy = new Vec2(Vec2.zero.Y, Vec2.zero.Y); //此副本为防止curPosition在外部被更改，赋值操作统一走SetPosition函数
        public Vec2 CurPosition{ get { return GetCurPosition(); }}

        private Vec2 GetCurPosition()
        {
            curPositionCopy.x = curPosition.x;
            curPositionCopy.y = curPosition.y;
            return curPositionCopy;
        }

        Vec2 tmpVec = new Vec2();
        public Vec2 TmpVec
        {
            get { return tmpVec; }
            set { tmpVec = value; }
        }

        /// <summary>
        /// 位移起始点
        /// </summary>
        Vec2 moveFromPosition = new Vec2();
        /// <summary>
        /// 位移目标点
        /// </summary>
        Vec2 moveToPosition = new Vec2();

        /// <summary>
        /// 需要寻路
        /// </summary>
        public bool NeedFindPath = true;

        public bool UseNewJps = true;
        /// <summary>
        /// 改变寻路终点
        /// </summary>
        bool NeedChangePathDestination = false;
        /// <summary>
        /// 位移路径点的集合
        /// </summary>
        Vec2[] movePath;
        /// <summary>
        /// 位移路径点下标
        /// </summary>
        int movePathIndex;

        /// <summary>
        /// 寻路终点
        /// </summary>
        public Vec2 pathDestination = new Vec2();

        /// <summary>
        /// 位移距离
        /// </summary>
        float moveDistance = 0.0f;
        public float MoveDistance
        { get { return moveDistance; } }

        float moveFactor;
        float moveFactorSpeed;

        /// <summary>
        /// 当前单位是不是在移动； 
        /// 2015年5月4日10:55:01 用于定时广播
        /// </summary>
        private bool isMoving = false;
        public bool IsMoving
        { get { return isMoving; } }

        //public float MoveSpeed = 3.125f;
        public float MoveSpeed
        //{ get { return owner.GetMoveSpeed(); } }
        { get { return owner.GetNatureValue(NatureType.PRO_SPD) * 0.0001f; } }

        MoveType moveType = MoveType.COMMON;
        public MoveType Movetype { get { return moveType; } set { moveType = value; } }
        public bool IsMoveCharge { get { return moveType == MoveType.CHARGE; } }
        public bool IsMoveCommon { get { return moveType == MoveType.COMMON; } }

        public float timeNotMove { get; set; }

        private int genAngle = 0;

        private float duration;
        public float Duration
        { 
            get { return duration; }
            set { duration = value; }
        } 

        public int GenAngle
        {
            get { return genAngle; }
            set { genAngle = value; }
        }

        internal void SetPosition(float x, float y)
        {
            curPosition.x = x;
            curPosition.y = y;
        }

        public Vec2 MoveFromPosition
        { get { return moveFromPosition; } } 

        public Vec2 MoveToPosition
        { get { return moveToPosition; } }

        public void SetDestination(Vec2 dest, MoveType move_type = MoveType.COMMON)
        {
            if (!owner.CanMove())
            {
                return;
            }
            if (!Vec2.VeryClose(pathDestination, dest))
            {
                moveType = move_type;
                pathDestination.Init(dest);
                NeedChangePathDestination = true;
            }
            //Console.WriteLine(dest.x.ToString()+"/"+dest.y.ToString()+"/"+"common");
        }

        public void MoveStart(float speedScale = 1.0f)
        {
            isMoving = true;
            bool useDynamicGrid = owner.UseDynamicGrid();
            if (useDynamicGrid)
            {
                // 生成动态阻挡信息
                currentMap.UpdateDynamicGrid();
                // 将自己设置为非阻挡后，再寻路
                currentMap.SetFieldObjectObstract(owner, false);
            }
            InitPath(CurPosition, pathDestination, useDynamicGrid);
            InitValue();
        }

        public void MoveStop()
        {
            isMoving = false;
        }

        private void UpdatePositionByClampFactor(Vec2 from, Vec2 to, float clampFactor)
        {
            float x = from.X * (1 - clampFactor) + to.X * clampFactor;
            float y = from.Y * (1 - clampFactor) + to.Y * clampFactor;
            SetPosition(x, y);
        }

        /// <summary>
        /// 移动
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <returns>返回duration</returns>
        public void Move(float deltaTime)
        {
            if (currentMap != null)
            {
                isMoving = true;
                moveFactor += moveFactorSpeed * deltaTime;
                float clampFactor = MATH.Clamp01(moveFactor);
                //curPosition = MoveFromPosition * (1f- clampFactor) + MoveToPosition * clampFactor;
                UpdatePositionByClampFactor(MoveFromPosition, MoveToPosition, clampFactor);

                // 全图同步，不需要对格子进行维护
                if (currentMap.AoiType == AOIType.All)
                {
                    return;
                }
                Region destRegion = currentMap.RegionMgr.GetRegion(curPosition);
                if (destRegion != null && curRegion != null)
                {
                    if (destRegion.index == curRegion.index)
                    {
                        return;
                    }
                    else
                    {
                        //Log.Write("field object instance id {0} move from region {1} to region {2}", instance_id, curRegion.index, destRegion.index);
                        //curRegion.PrintNeigbor();
                        //destRegion.PrintNeigbor(); 
                        for (int i = 0; i < 8; i++)
                        {
                            // 对于旧格子，清除相关数据
                            if (curRegion.NeighborList[i] != null && !destRegion.InMyRegions(curRegion.NeighborList[i]))
                            {
                                curRegion.NeighborList[i].NotifyCurRegionFieldObjectOut(owner);
                            }
                        }
                        curRegion.LeaveRegion(owner);

                        for (int i = 0; i < 8; i++)
                        {
                            // 对于新格子，添加相关数据 
                            if (destRegion.NeighborList[i] != null && !curRegion.InMyRegions(destRegion.NeighborList[i]))
                            {
                                destRegion.NeighborList[i].NotifyCurRegionFieldObjectIn(owner);
                            }
                        }
                        destRegion.EnterRegion(owner);

                        owner.SetCurRegion(destRegion);
                    }
                }
            }
            return;
        }



        /// <summary>
        /// 传送
        /// </summary>
        /// <param name="dest">目的地点</param>
        /// <returns>duration</returns>
        public void Transmit(Vec2 dest)
        {
            if (currentMap != null)
            {
                SetPosition(dest.x,dest.y);
                if (currentMap.AoiType == AOIType.All)
                {
                    // 对于全图 同步，广播当前位置信息即可
                    owner.BroadcastSimpleInfo();
                    return;
                }
                Region destRegion = currentMap.RegionMgr.GetRegion(dest);
                if (destRegion != null && curRegion != null)
                {
                    if (!destRegion.InMyRegions(curRegion))
                    {
                        curRegion.NotifyCurRegionFieldObjectOut(owner);
                    }
                    for (int i = 0; i < 8; i++)
                    {
                        // 对于旧格子，清除相关数据
                        if (curRegion.NeighborList[i] != null && !destRegion.InMyRegions(curRegion.NeighborList[i]))
                        {
                            curRegion.NeighborList[i].NotifyCurRegionFieldObjectOut(owner);
                        }
                    }
                    curRegion.LeaveRegion(owner);

                    if (!curRegion.InMyRegions(destRegion))
                    {
                        destRegion.NotifyCurRegionFieldObjectIn(owner);
                    }
                    for (int i = 0; i < 8; i++)
                    {
                        // 对于新格子，添加相关数据 
                        if (destRegion.NeighborList[i] != null && !curRegion.InMyRegions(destRegion.NeighborList[i]))
                        {
                            destRegion.NeighborList[i].NotifyCurRegionFieldObjectIn(owner);
                        }
                    }
                    destRegion.EnterRegion(owner);

                    owner.SetCurRegion(destRegion);
                }
            }
        }

        public float GetDuration(Vec2 dest, Vec2 sour)
        {
           // Vec2.OperatorMinus(dest, sour, TmpVec);
            //float duration = TmpVec.magnitude / MoveSpeed;
            float distance = Vec2.GetDistance(sour, dest);
            float duration = distance / MoveSpeed;
            return duration;
        }


        /// <summary>
        /// 检查寻路终点
        /// </summary>
        /// <returns></returns>
        public bool CheckDestination()
        {
            return NeedChangePathDestination;
        }

        public bool CheckPathEnd()
        {
            if (CurPosition == MoveToPosition)
            {
                // Move has ended
                if (movePathIndex == movePath.Length - 1)
                {
                    pathDestination.Init(CurPosition);
                    isMoving = false;
                    return true;
                }
                else
                {
                    ++movePathIndex;
                    moveFromPosition.Init(MoveToPosition);
                    moveToPosition.Init(movePath[movePathIndex]);

                    InitValue();
                    owner.BroadCastMove();
                    return false;
                }
            }
            return false;
        }


        private bool NeedUseBigPath(Vec2[] path)
        {

            if ((owner.CurFsmStateType == FsmStateType.MONSTER_SEARCH) || owner.FieldObjectType == TYPE.PC)
            {
                //TODO:这里是否必要。
                //FieldObject tmpTarget = (FieldObject)GetTarget();
                //if (tmpTarget != null)
                //{
                if (path.Length == 2 && path[0].x == path[1].x && path[0].y == path[1].y)
                {
                    return true;
                }
                //}
            }
            return false;
        }

        public void InitPath(Vec2 from, Vec2 to, bool useDynamic)
        {
            //if (currentMap.IsHighPrecision)
            //{
            //    //高精度地图，约定精度为0.5，乘以2取整，结果路径需要除以2还原。
            //    from = from * 2.0f;
            //    to = to * 2.0f;
            //}

            if (NeedFindPath && owner.FieldObjectType == TYPE.PC)
            {
                if(UseNewJps)
                {
                    movePath = currentMap.GetPath_New(from, to, useDynamic);
                    if (movePath == null || movePath.Length == 0)
                    {
                        movePath = new Vec2[2];
                        movePath[0] = from;
                        movePath[1] = from;
                    }
                }
                else
                {
                    movePath = currentMap.GetPath(from, to);
                    if (NeedUseBigPath(movePath))
                    {
                        Log.Debug("---------------UseBig----------------");
                        movePath = currentMap.GetPath(from, to, true);
                    }
                }
            }
            else
            {
                movePath = new Vec2[2];
                movePath[0]=from;
                movePath[1]=to;
            }

            //if (currentMap.IsHighPrecision)
            //{
            //    foreach (var item in movePath)
            //    {
            //        //路径除以2还原
            //        item.X = item.X / 2.0f;
            //        item.Y = item.Y / 2.0f;
            //    }
            //}
            moveFromPosition.Init(movePath[0]);
            moveToPosition.Init(movePath[1]);
            movePathIndex = 1;
            pathDestination.Init(movePath[movePath.Length - 1]);
            NeedChangePathDestination = false;
            NeedFindPath = true;
        }

        public float InitValue()
        {
            moveFactor = 0;

            Duration = GetDuration(MoveToPosition, MoveFromPosition);
            if (Duration > 0)
            {
                moveFactorSpeed = 1 / Duration;
            }
            else
            {
                moveFactor = 1;
            }

            return Duration;
        }


    }
}
