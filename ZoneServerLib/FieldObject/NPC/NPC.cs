using CommonUtility;
using DataProperty;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public class NpcFactory
    { 
        public static NPC CreateNpc(ZoneServerApi server, int id)
        {
            if (LiftConfig.NpcId == id)
            {
                return new NpcLift(server);
            }
            else
            {
                return new NPC(server);
            }
        }
    }
	public partial class  NPC : FieldObject
    {

        public int CanRearchMapId { get; set; }

        public ZoneNPCModel Model { get; private set; }
        public bool IsVisable { get; set; }

        public int ZoneNpcId
        { get { return Model.Id; } }

        public override TYPE FieldObjectType
        { get { return TYPE.NPC; } }

        public NPCType NPCType
        { get { return Model.NPCType; } }

        public bool IsIntegralBossNPC
        { get { return Model.NPCType == NPCType.IntegralBoss; } }

        public bool IsThemeBossNPC
        { get { return Model.NPCType == NPCType.ThemeBoss; } }

        public bool IsCarnivalBossNPC
        { get { return Model.NPCType == NPCType.CarnivalBoss; } }

        public NPC(ZoneServerApi server) : base(server) { }

        public virtual void Init(FieldMap currentMap, ZoneNPCModel model)
        {
            this.Model = model;

            //InitFSM();

            SetCurrentMap(currentMap);

            Vec2 position = new Vec2(model.PosX, model.PosY);
            SetPosition(position);

            GenAngle = model.Angle;

            IsVisable = true;
            if (IsIntegralBossNPC || IsThemeBossNPC || IsCarnivalBossNPC)
            {
                IsVisable = false;
            }

            if (CheckParamKey(NpcParamType.FLY_MAP_ID))
            {
                int mapId = GetParamIntValue(NpcParamType.FLY_MAP_ID);
                if (mapId > 0)
                {
                    //传送
                    CanRearchMapId = mapId;
                }
                else
                {
                    Log.ErrorLine("NPC {0} init got an error mapid {1} : Please Check xml or Npc name", ZoneNpcId, mapId);
                }
            }
            NPCModel npcModel = NPCLibrary.GetNPCModel(Model.NpcId);
            if (npcModel != null)
            {
                radius = npcModel.Radius;
            }
            else
            {
                radius = 1f;
            }
            ////巡逻
            //BindPatrolPoints(npcData, zoneNpcData);

            //IsWarp = NpcData.GetString("type") == "ZoneMove";
        }

        public bool NeedSync()
        {
            return NPCType == NPCType.IntegralBoss || NPCType == NPCType.ThemeBoss || NPCType == NPCType.CarnivalBoss;
        }

        internal void OnClick(PlayerChar pc)
		{
            //if (IsPatrol && InRunningState)
            //{
            //    this.BroadCastStop();
            //    this.FsmManager.SetNextFsmStateType(FsmStateType.IDLE);
            //}

            if (CheckParamKey(NpcParamType.FLY_MAP_ID))
            {
                //if (!IsStateIdle)
                //{
                //    this.BroadCastStop();
                //    this.FsmManager.SetNextFsmStateType(FsmStateType.IDLE);
                //}
                MoveMap(pc);
            }
            else
            {
                if (NPCType == NPCType.IntegralBoss)
                {
                    pc.Interact(this, ZoneNpcId, CommonConst.NPC_INTEGRALBOSSPANEL, "", ZoneNpcId, 0);
                }
                else if (NPCType == NPCType.ThemeBoss)
                {
                    pc.Interact(this, ZoneNpcId, CommonConst.NPC_THEMEBOSS, "", ZoneNpcId, 0);
                }
                else if (NPCType == NPCType.CarnivalBoss)
                {
                    pc.Interact(this, ZoneNpcId, CommonConst.NPC_CARNIVALBOSS, "", ZoneNpcId, 0);
                }
                else
                {
                    pc.Interact(this, ZoneNpcId, CommonConst.NPC_TRIGGER, "", ZoneNpcId, 0);
                }
            }
        }

		protected override void OnUpdate(float dt)
        {
            ////判断是否是巡逻NPC
            //if (IsPatrol)
            //{
            //    PatrolUpdate();
            //}
        }



        public virtual void GetSimpleInfo(MSG_ZGC_NPC_SIMPLE_INFO info)
        {
            if (info == null) return;
            info.InstanceId = instanceId;
            info.ZoneNpcId = ZoneNpcId;
            info.X = Position.X;
            info.Y = Position.Y;
            // 移动中 则同步移动信息
            //if (IsMoving)
            //{
            //    info.MoveInfo = new MSG_ZGC_NPC_SIMPLE_INFO.Types.MOVE_INFO();
            //    info.MoveInfo.DestX = MoveHandler.MoveToPosition.x;
            //    info.MoveInfo.DestY = MoveHandler.MoveToPosition.y;
            //    info.MoveInfo.Duration = MoveHandler.GetDuration(MoveHandler.MoveToPosition, Position);
            //}
        }

        public void MoveMap(PlayerChar pc)
        {
            int mapId = GetParamIntValue(NpcParamType.FLY_MAP_ID);
            if (mapId == 0)
            {
                Log.ErrorLine("player {0} cross Npc {1} move zone got an error mapid {2} : Please Check xml or Npc name",pc.Uid, ZoneNpcId, mapId);
                return;
            }
            float x = GetParamFloatValue(NpcParamType.POS_X);
            float y = GetParamFloatValue(NpcParamType.POS_Y);
            pc.NextAngle = GetParamIntValue(NpcParamType.ANGLE);

            //增加检验
            Vec2 targetPos = new Vec2(x, y);
            MapModel model = MapLibrary.GetMap(mapId);
            if (!pc.CheckCanEnterMap(model))
            {
                pc.SendAutoPathFindingMsg();
                return;
            }
            if(!model.CheckStrictPosInMap(new Vec2(x, y)))
            {
                Log.Warn($"npc {ZoneNpcId} pos {targetPos} is not strict in map {mapId}");
            }
            if(!model.CheckNoneStrictPosInMap(new Vec2(x, y)))
            {
                Log.Warn($"npc {ZoneNpcId} pos {targetPos} is not in map {mapId}");
                targetPos.X = model.BeginPosX;
                targetPos.Y = model.BeginPosY;
            }
            pc.AskForEnterMap(mapId, pc.CurrentMap.Channel, targetPos);
        }

        public string GetParamValue(string key)
        {
            string value;
            Model.Params.TryGetValue(key, out value);
            return value;
        }

        public int GetParamIntValue(string key)
        {
            string valueString = GetParamValue(key);
            int value;
            int.TryParse(valueString, out value);
            return value;
        }

        public float GetParamFloatValue(string key)
        {
            string valueString = GetParamValue(key);
            float value;
            float.TryParse(valueString, out value);
            return value;
        }

        public bool CheckParamKey(string key)
        {
            return Model.Params.ContainsKey(key);
        }

        public MSG_ZGC_NPC_INFO GetNpcPacketInfo()
        {
            MSG_ZGC_NPC_INFO info = new MSG_ZGC_NPC_INFO();
            info.InstanceId = InstanceId;
            info.ZoneNpcId = ZoneNpcId;
            return info;
        }

        //private List<Vec2> PatrolPoints = new List<Vec2>();
        //private bool IsPatrol { get; set; }
        //private int PatrolTime { get; set; }
        //private int WaitTime { get; set; }
        //private int PatrolIndex { get; set; }
        //private Vec2 DestinationPoint { get; set; }
        //private DateTime LastPatrolTime { get; set; }


        /// <summary>
        /// 巡逻
        /// </summary>
        /// <param name="npcData"></param>
        /// <param name="zoneData"></param>
        //private void BindPatrolPoints(Data npcData, Data zoneData)
        //{
        //    PatrolTime = zoneData.GetInt("patrolTime");
        //    if (PatrolTime > 0)
        //    {
        //        WaitTime = zoneData.GetInt("waitTime");
        //        IsPatrol = true;
        //        PatrolIndex = 0;
        //        LastPatrolTime = ZoneServerApi.now;
        //        DestinationPoint = null;
        //        this.FsmManager.SetNextFsmStateType(FsmStateType.IDLE);

        //        float moveSpeed = npcData.GetInt("speed");
        //        //MoveHandler.SetMoveSpeed(moveSpeed);

        //        Vec2 tempPoint;
        //        string pointString = zoneData.GetString("patrolPoint");
        //        string[] points = StringSplit.GetArray("|", pointString);
        //        foreach (var pintTtring in points)
        //        {
        //            string[] point = StringSplit.GetArray(":", pintTtring);
        //            float x = float.Parse(point[0]);
        //            float y = float.Parse(point[1]);
        //            tempPoint = new Vec2(x, y);
        //            PatrolPoints.Add(tempPoint);
        //        }
        //        if (PatrolPoints.Count == 0)
        //        {
        //            IsPatrol = false;
        //        }
        //    }

        //    if (IsPatrol)
        //    {
        //        AddToAoi();
        //    }
        //}

        //private void PatrolUpdate()
        //{
        //    //判断状态是否是IDE
        //    if (IsStateIdle)
        //    {
        //        //检查IDE时间
        //        double passTime = (ZoneServerApi.now - LastPatrolTime).TotalSeconds;
        //        //判断是否是被打断的
        //        if (DestinationPoint != null)
        //        {
        //            double length = Vec2.GetRangePower(Position, DestinationPoint);
        //            if (length > 1)
        //            {
        //                if (passTime > WaitTime)
        //                {
        //                    LastPatrolTime = ZoneServerApi.now;
        //                    NpcBroadCastNearbyMove();
        //                    //继续之前的点
        //                    this.SetDestination(DestinationPoint);
        //                    //if (this.isNotMove == false)
        //                    //{
        //                    this.FsmManager.SetNextFsmStateType(FsmStateType.RUN);
        //                    //}
        //                }
        //            }
        //            else
        //            {
        //                if (passTime > PatrolTime)
        //                {
        //                    LastPatrolTime = ZoneServerApi.now;
        //                    NpcBroadCastNearbyMove();
        //                    //获取下一个点
        //                    Vec2 newPoint = PatrolPoints[PatrolIndex];
        //                    PatrolIndex++;
        //                    if (PatrolIndex >= PatrolPoints.Count)
        //                    {
        //                        PatrolIndex = 0;
        //                    }

        //                    //巡逻到下一个点
        //                    this.SetDestination(newPoint);
        //                    DestinationPoint = newPoint;
        //                    //if (this.isNotMove == false)
        //                    //{
        //                    this.FsmManager.SetNextFsmStateType(FsmStateType.RUN);
        //                    //}
        //                }
        //            }
        //        }
        //        else
        //        {
        //            if (passTime > PatrolTime)
        //            {
        //                LastPatrolTime = ZoneServerApi.now;
        //                NpcBroadCastNearbyMove();
        //                //获取下一个点
        //                Vec2 newPoint = PatrolPoints[PatrolIndex];
        //                PatrolIndex++;
        //                if (PatrolIndex >= PatrolPoints.Count)
        //                {
        //                    PatrolIndex = 0;
        //                }

        //                //巡逻到下一个点
        //                this.SetDestination(newPoint);
        //                DestinationPoint = newPoint;

        //                //if (this.isNotMove == false)
        //                //{
        //                this.FsmManager.SetNextFsmStateType(FsmStateType.RUN);
        //                //}
        //            }
        //        }
        //    }
        //}

        //private void NpcBroadCastNearbyMove()
        //{
        //    MSG_ZGC_NPC_MOVE msg = new MSG_ZGC_NPC_MOVE();
        //    msg.InstanceId = instanceId;
        //    BroadCast(msg);
        //}
    }
}