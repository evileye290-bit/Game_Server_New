using CommonUtility;
using System;
using EnumerateUtility;
using Message.Gate.Protocol.GateC;

namespace ZoneServerLib
{
    public partial class FieldObject
    {
        MoveHandler moveHandler;
        internal MoveHandler MoveHandler
        { get { return moveHandler; } }

        public bool CanMove()
        {
            if (InSkillState || InBuffState(BuffType.Dizzy) || InBuffState(BuffType.Fixed))
            {
                return false;
            }
            return true;
        }

        public bool IsMoving
        { get { return moveHandler.IsMoving; } }


        public void InitMoveHandler() {
            moveHandler = new MoveHandler(this);
        }

        public float GetMoveDistance()
        {
            return MoveHandler.MoveDistance;
        }

        public bool IsInSmallGrid()
        {
            return CurrentMap.IsWalkableAt((int)Math.Round(Position.x), (int)Math.Round(Position.y));
        }

        public void Move(float deltaTime)        
        {
            MoveHandler.Move(deltaTime);
        }

        public void Transmit(Vec2 dest)
        {
            MoveHandler.Transmit(dest);
            BroadCastMove();
        }

        public Vec2 SetObjectAngle(float inputX, float inputY)
        {
            return Position.GetAngle(inputX, inputY); 
        }

        public Vec2 RandomDes()
        {
            Vec2 des = new Vec2();
            des.X = this.Position.X + RAND.Range(-10, 10);
            des.Y = this.Position.Y + RAND.Range(-10, 10);
            if (this.CurrentMap.CheckPath(Position,des))
            {
                return des;
            }
            else
            {
                return RandomDes(des);
            }
        }

        public Vec2 RandomPos(int r)
        {
            Vec2 des = new Vec2();
            des.X = this.Position.X + RAND.Range(-r, r);
            des.Y = this.Position.Y + RAND.Range(-r, r);
            return des;
        }

        public Vec2 RandomDes(Vec2 des)
        {
            if (des.X > Position.X)
            {
                des.X -= 1;
            }
            else
            {
                des.X += 1;
            }

            if (des.Y > Position.Y)
            {
                des.Y -= 1;
            }
            else
            {
                des.Y += 1;
            }

            if (this.CurrentMap.CheckPath(Position, des))
            {
                return des;
            }
            else
            {
                return RandomDes(des);
            }
        }

        public bool CheckDestination()
        {
           return MoveHandler.CheckDestination(); ;
        }

        // NOTE : Check End
        public bool CheckPathEnd()
        {
            return MoveHandler.CheckPathEnd();
        }

        // NOTE : OnStart
        public void OnMoveStart(float speedScale = 1.0f)
        {
            try
            {
                // 防止寻路失败 导致状态机内寻路卡死
                MoveHandler.MoveStart(speedScale);
            }
            catch (Exception e)
            {
                Logger.Log.Alert(e.ToString());
            }
        }

        public void OnMoveStop()
        {
            moveHandler.MoveStop();
        }

        //public void OnChaseStart(float speed_scale = 1.0f)
        //{
        //    //InitPath(Position, Target.Position);
        //    InitMoveValue(speed_scale);
        //}

        // NOTE : OnUpdate      
        public virtual bool OnMove(float deltaTime)
        {
            if (CanMove())
            {
                Move(deltaTime);
                return CheckPathEnd();
            }
            else
            {
                return true; 
            }
        }

        public void SetDestination(Vec2 dest, MoveType moveType = MoveType.COMMON)
        {
            MoveHandler.SetDestination(dest, moveType);
        }

        public void SetPosition(Vec2 pos)
        {
            moveHandler.SetPosition(pos.x, pos.y);
        }

        public void BroadCastMove()
        {
            // TODO 新协议 
            if (!CanMove())
            {
                return;
            }
            MSG_GC_FieldObject_MOVE move = new MSG_GC_FieldObject_MOVE();

            move.InstanceId = InstanceId;
            move.DestPosX = MoveHandler.MoveToPosition.x;
            move.DestPosY = MoveHandler.MoveToPosition.y;
            move.PoxX = Position.x;
            move.PoxY = Position.y;
            //move.SpeedScale = m_speed_scale;
            //move.target_id = m_target_id;
            move.Speed = MoveHandler.MoveSpeed;
            BroadCast(move);
        }

        public void BroadCastStop()
        {
            MSG_ZGC_CHARACTER_STOP stopMsg = new MSG_ZGC_CHARACTER_STOP();
            stopMsg.InstanceId = InstanceId;
            stopMsg.PoxX = Position.x;
            stopMsg.PoxY = Position.y;
            BroadCast(stopMsg);
        }


        public bool UseDynamicGrid()
        {
            //if(FieldObjectType == TYPE.HERO || FieldObjectType == TYPE.MONSTER)
            if(FieldObjectType == TYPE.HERO || FieldObjectType == TYPE.MONSTER || FieldObjectType == TYPE.PC||FieldObjectType==TYPE.ROBOT)
            {
                return (currentMap != null && currentMap.UseDynamicGrid);
            }
            return false;
        }

    }
}