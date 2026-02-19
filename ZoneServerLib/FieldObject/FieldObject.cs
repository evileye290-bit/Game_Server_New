using CommonUtility;
using DataProperty;
using System;
using System.Collections.Generic;
using System.IO;
using EnumerateUtility;
using ScriptFighting;
using ServerShared;

namespace ZoneServerLib
{
    public partial class FieldObject : IFightingObject
    {

        public ZoneServerApi server;

        protected int uid;
        public int Uid
        {
            get { return uid; }
        }

        protected int instanceId;
        public int InstanceId
        {
            get { return instanceId; }
        }


        public virtual TYPE FieldObjectType
        {
            get { return TYPE.NONE; }
        }

        protected FieldMap currentMap;
        /// <summary>
        /// 当前所在地图
        /// </summary>
        public FieldMap CurrentMap
        {
            get { return currentMap; }
        }

        public DungeonMap CurDungeon
        { get { return currentMap as DungeonMap; } }

        public TeamDungeonMap CurTeamDungeon
        { get { return currentMap as TeamDungeonMap; } }

        public bool IsPlayer
        { get { return FieldObjectType == TYPE.PC; } }

        public bool IsRobot
        { get { return FieldObjectType == TYPE.ROBOT; } }

        public bool IsHero
        { get { return FieldObjectType == TYPE.HERO; } }

        public bool IsMonster
        { get { return FieldObjectType == TYPE.MONSTER; } }

        public bool IsPet
        { get { return FieldObjectType == TYPE.PET; } }

        public DateTime LoadingDoneCreateDungeonWaiting = ZoneServerApi.now;

        /// <summary>
        /// 当前地图ID
        /// </summary>
        public int CurrentMapId
        {
            get
            {
                if (currentMap != null)
                {
                    return currentMap.MapId;
                }
                return CONST.MAIN_MAP_ID;
            }
        }

        public int CurrentChannel
        {
            get
            {
                if (currentMap != null)
                {
                    return currentMap.Channel;
                }
                return CONST.MAIN_MAP_CHANNEL;
            }
        }

        /// <summary>
        /// 当前坐标
        /// </summary>
        public Vec2 Position
        {
            get { return MoveHandler.CurPosition; }
        }

        public int GenAngle
        {
            get { return MoveHandler.GenAngle; }
            set { MoveHandler.GenAngle = value; }
        }


        public Vec2 TmpVec
        {
            get { return MoveHandler.TmpVec; }
            set { MoveHandler.TmpVec = value; }
        }

        /// <summary>
        /// 半径
        /// </summary>
        protected float radius = 0.5f;
        public float Radius
        { get { return radius; } }


        // NOTE : Func
        internal FieldObject(ZoneServerApi server)
        {
            this.server = server;
            //移动
            InitMoveHandler();
        }


        public void SetUid(int uid)
        {
            this.uid = uid;
        }

        public void SetInstanceId(int id)
        {
            this.instanceId = id;
            CurrentMap.TokenId++;
        }

        /// <summary>
        /// 自运行
        /// </summary>
        /// <param name="dt">单位是秒</param>
        public void Update(float dt)
        {
            UpdateInBattle(dt);
            //状态机
            FsmUpdate(dt);

            OnUpdate(dt);
        }

        private void UpdateInBattle(float dt)
        {
            if(!InBattle)
            {
                return;
            }
            if (TriggerMng != null)
            {
                TriggerMng.Update(dt);
            }
            if (BuffManager != null)
            {
                BuffManager.Update(dt);
            }
            if(HateManager != null)
            {
                HateManager.Update(dt);
            }
            if(markManager != null)
            {
                markManager.Update(dt);
            }
        }


        protected virtual void OnUpdate(float dt) { }

        public void SetCurrentMap(FieldMap map)
        {
            currentMap = map;
        }

        public virtual void BroadcastSimpleInfo()
        {
        }

        public virtual FieldObject GetOwner()
        {
            return null; 
        }

        public void SetRadius(float r)
        {
            radius = r;
        }

        public virtual void ResetRadius()
        {
            radius = 0.5f;
        }

        public void SetCurrMap(FieldMap map)
        {
            this.currentMap = map;
        }

        public int GetHeroId()
        {
            if(IsHero)
            {
                Hero hero = this as Hero;
                return hero.HeroId;
            }
            if (IsPlayer)
            {
                PlayerChar player = this as PlayerChar;
                return player.HeroId;
            }
            if (IsRobot)
            {
                return (this as Robot)?.HeroId ?? 0;
            }
            return 0;
        }

        public int GetPetId()
        {
            if (IsPet)
            {
                Pet pet = this as Pet;
                return pet.PetId;
            }
            return 0;
        }
    }
}