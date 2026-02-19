using System;
using EnumerateUtility;
using CommonUtility;
using Message.Gate.Protocol.GateC;
using ServerModels;
using ServerShared;
using Logger;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public partial class Pet : FieldObject
    {
        //private PlayerChar owner;
        //public PlayerChar Owner
        //{ get { return owner; } }

        private FieldObject owner;
        public FieldObject Owner
        {
            get
            {
                return owner;
            }
        }

        private PetInfo petInfo;
        public PetInfo PetInfo
        { get { return petInfo; } }
      
        public ulong PetUid
        { get { return petInfo.PetUid; } }

        public int Level
        { get { return petInfo.Level; } }

        public float Shape
        { get { return petInfo.Shape; } }

        public int BreakLevel
        { get { return petInfo.BreakLevel; } }

        public int PetId
        { get { return petModel.Id; } }

        private PetModel petModel;
        public PetModel PetModel
        { get { return petModel; } }

        override public TYPE FieldObjectType
        { get { return TYPE.PET; } }

        public float WalkRange
        { get { return petModel.WalkRange; } }
        public float TransmitDistance
        { get { return petModel.TransmitDistance; } }
        public float FollowDistance
        { get { return petModel.FollowDistance; } }

        public int CollisionPriority { get; private set; }

        private List<int> skills = new List<int>();
        public List<int> Skills
        { get { return skills; } }

        public bool OwnerIsRobot { get; private set; }
        public int QueueNum { get; private set; }
        /// <summary>
        /// 玩家最后坐标
        /// </summary>
        private Vec2 m_ownerLastPos = new Vec2();

        public int SkinId = 0;

        internal Pet(ZoneServerApi server, FieldObject owner, PetInfo info, PetModel petModel, int queueNum)
            : base(server)
        {
            this.owner = owner;
            currentMap = owner.CurrentMap;
            Vec2 pos = GetWalkablePoint();
            SetPosition(pos);

            petInfo = info;

            InitPetModelData(petModel, info.PetId);

            IsAttacker = owner.IsAttacker;
            CollisionPriority = -1;//优先级比所有英雄小

            if (owner is Robot)
            {
                OwnerIsRobot = true;
            }
            QueueNum = queueNum;
        }

        private void InitPetModelData(PetModel petModel, int petId)
        {
            if (petModel == null)
            {
                Log.Warn("player {0} create pet {1} failed: no such pet model", owner.Uid, petId);
            }
            else
            {
                this.petModel = petModel;

                radius = petModel.Radius;
                HateRatio = petModel.HateRatio;
                hateRange = petModel.HateRange;
                hateRefreshTime = petModel.HateRefreshTime;
                //ForgiveTime = petModel.ForgiveTime;
                skills.AddRange(petModel.NormalAttacks);
                skills.AddRange(petModel.InbornSkills);
            }
        }

        public void Init()
        {
            InitFSM();
            InitNature();
            FsmManager.SetNextFsmStateType(FsmStateType.IDLE);
        }

        protected override void OnUpdate(float deltaTime)
        {
            //不在统一地图或者同一线
            if (currentMap != null && Owner.CurrentMap != null && (owner.CurrentMap.MapId != CurrentMap.MapId || owner.CurrentMap.Channel != CurrentMap.Channel))
            {
                CurrentMap.RemovePet(InstanceId);
                return;
            }
        }
        
        private Vec2 GetWalkablePoint()
        {
            Vec2 pos = owner.Position;
            Vec2 position = new Vec2();
            bool find = false;
            int maxFindCnt = 3;
            int index = 0;
            while (index < maxFindCnt)
            {
                Vec2 vec = Vec2.GetRandomPos(pos, 2);
                float minDisPower = 1.2f;
                bool allSuit = true;

                Vec2 dis = pos - vec;
                float disPower = dis.x * dis.x + dis.y * dis.y;
                if (disPower < minDisPower)
                {
                    allSuit = false;
                }

                if (allSuit)
                {
                    find = true;
                    position.x = vec.x;
                    position.y = vec.y;
                    break;
                }
                index++;
            }
            if (!find)
            {
                position.x = owner.Position.x + RAND.Range(-1, 1);
                position.y = owner.Position.y + RAND.Range(-1, 1);
            }
            return position;
        }

        bool syncBornAnim = true;
        public void GetSimpleInfo(MSG_ZGC_PET_SIMPLE_INFO info)
        {
            if (info == null) return;
            info.InstanceId = instanceId;
            info.TypeId = PetId;
            info.Level = Level;
            info.PosX = Position.X;
            info.PosY = Position.Y;
            info.OwnerUid = Owner.Uid;
            info.Shape = Shape;
            info.BreakLevel = BreakLevel;
            //if (syncBornAnim)
            //{
            //    info.BornAnim = true;
            //    syncBornAnim = false;
            //}
            //else
            //{
            //    info.BornAnim = false;
            //}
            // 移动中 则同步移动信息
            if (IsMoving)
            {
                info.DestX = MoveHandler.MoveToPosition.x;
                info.DestY = MoveHandler.MoveToPosition.y;
                //info.Speed = GetMoveSpeed();
                info.Speed = GetNatureValue(NatureType.PRO_SPD) * 0.0001f;
            }
        }

        public bool InTransmitDis()
        {
            return Vec2.InRange(Position, Owner.Position, TransmitDistance);
        }

        public bool InFollowDis()
        {
            return Vec2.InRange(Position, Owner.Position, FollowDistance);
        }

        public bool InWalkDis()
        {
            return Vec2.InRange(Position, Owner.Position, WalkRange);
        }

        public override void BroadcastSimpleInfo()
        {
            MSG_ZGC_PET_SIMPLE_INFO info = new MSG_ZGC_PET_SIMPLE_INFO();
            GetSimpleInfo(info);
            BroadCast(info);
        }

        public override FieldObject GetOwner()
        {
            return owner;
        }

        public void InitSpeedInBattle()
        {
            SetNatureBaseValue(NatureType.PRO_RUN_IN_BATTLE, PetModel.PRO_RUN_IN_BATTLE);
            SetNatureBaseValue(NatureType.PRO_SPD, GetNatureValue(NatureType.PRO_RUN_IN_BATTLE));
        }
    }
}
