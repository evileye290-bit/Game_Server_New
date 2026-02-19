using System;
using System.Collections.Generic;
using CommonUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using ServerFrame;
using ServerModels;
using ServerShared;

namespace ZoneServerLib
{
    // 基础副本类，维护基础的副本逻辑，其他副本玩法应继承此类
    public partial class DungeonMap : FieldMap
    {
        private bool alarmed = false;

        public DungeonState State { get; protected set; }
        public DungeonResult DungeonResult { get; protected set; }

        public DateTime CurrTime { get; protected set; }
        public DateTime OpenTime { get; protected set; }
        public DateTime StartTime { get; protected set; }
        public float StopTime { get; protected set; }
        public DateTime CloseTime { get; protected set; }
        public DateTime AlarmTime { get; protected set; }
        public DungeonModel DungeonModel { get; private set; }
        public int FpsNum { get; protected set; }

        protected Queue<Vec2> beginPosQueue = new Queue<Vec2>();

        MessageDispatcher messageDispatcher = new MessageDispatcher();
        public MessageDispatcher MessageDispatcher
        { get { return messageDispatcher; } }

        protected bool canRevive = true;
        public bool CanRevive => canRevive;

        public BattleFpsManager BattleFpsManager { get; protected set; }

        public DungeonMap(ZoneServerApi server, int mapId, int channel):base(server, mapId, channel)
        {
            CurrTime = BaseApi.now;
            OpenTime = BaseApi.now;

            State = DungeonState.Open;
            DungeonResult = DungeonResult.None;
            DungeonModel = DungeonLibrary.GetDungeon(mapId);
            if(DungeonModel == null)
            {
                Log.Warn($"create dungeon map {mapId} channel {channel} failed, no such dungeon model!");
                return;
            }

            SetStartTime(OpenTime.AddSeconds(DungeonModel.ReadyTime));
            foreach (var pos in DungeonModel.BeginPosList)
            {
                beginPosQueue.Enqueue(pos);
            }

            InitMixSkillManager();
            InitBattleDataManaget();
            InitBattleFpsManager();
        }

        public void SetStartTime(DateTime time)
        {
            StartTime = time;
            StopTime = DungeonModel.StopTime;
            CloseTime = time.AddSeconds(DungeonModel.CloseTime);
            AlarmTime = time.AddSeconds(DungeonModel.AlarmTime - 1);
        }

        // 不同类型玩法，可能根据player设置不同的出生位置
        public virtual Vec2 CalcBeginPos(int i, FieldObject field)
        {
            //int i = 1;
            i = PcList.Count + RobotList.Count + 1;
            if (RobotList.Count > 0)
            {
                i = 4;//站到对面
            }
            return DungeonModel.PlayerPos[i] ?? OldCalcBeginPos();
        }

        public Vec2 OldCalcBeginPos()
        {
            if (beginPosQueue.Count == 0)
            {
                return Model.BeginPos;
            }
            Vec2 beginPos = beginPosQueue.Dequeue();
            beginPosQueue.Enqueue(beginPos);
            return beginPos;
        }

        public override void Update(float dt)
        {
            int loopCount = 1;
            float fixDt = dt;
            bool startedSpeedUp = false;

            CheckAndSpeedUp(dt);

            if (IsNeedCacheMessage())
            {
                startedSpeedUp = true;
                if (speedUpFsmIndex == 0)
                {
                    speedUpFsmIndex = FpsNum;
                }

                fixDt = DungeonLibrary.SpeedUpPerFpsAddTime;
                loopCount = DungeonLibrary.SkipBattleSpeedUp;
            }

            while (loopCount > 0)
            {
                CurrTime = CurrTime.AddSeconds(fixDt);

                base.Update(fixDt);

                //UpdateDynamicGrid(dt);
                UpdateTriggers(fixDt);
                UpdateState(fixDt);
                UpdateMixSkill(fixDt);
                CheckKickPlayer(fixDt);
                CheckBroadCastMonsterGeneratedWalk(fixDt);


                //加速后的帧率索引
                if (startedSpeedUp)
                {
                    ++speedUpFsmIndex;
                }
                --loopCount;

                //发送加速战斗缓存的消息
                BroadcastPlayerCachedMessage(FpsNum);
            }

            ++FpsNum;

            BattleFpsManager.Update(dt);
        }

        protected virtual void UpdateState(float dt)
        {
            StopTime -= dt;

            switch(State)
            {
                case DungeonState.Open:
                    if(CurrTime  >= StartTime)
                    {
                        Start();
                    }
                    break;
                case DungeonState.Started:
                case DungeonState.Stopped:
                    CheckAndAlarmDungeonStop();
                    if (BaseApi.now > CloseTime)
                    {
                        Close();
                    }
                    break;
                // 其余状态无需处理
                case DungeonState.Closed:
                default:
                    break;
            }
        }

        protected virtual void InitBattleFpsManager()
        { 
            BattleFpsManager = new BattleFpsManager(this);
        }

        public override MessageDispatcher GetMessageDispatcher()
        {
            return messageDispatcher;
        }

        public bool CheckNeedAlarm()
        {
            return AlarmTime <= BaseApi.now;
        }

        //战斗结束预警
        private void CheckAndAlarmDungeonStop()
        {
            if (CheckNeedAlarm() && !alarmed)
            {
                alarmed = true;
                int stopTime = (int)(StopTime * 1000);

                PcList.ForEach(player => player.Value.Write(new MSG_ZGC_BATTLE_END_TIME() { Time = stopTime }));
            }
        }


        public int GetFinishTime()
        {
            if (BaseApi.now > StartTime)
            {
                return (int)(BaseApi.now - StartTime).TotalSeconds;
            }
            else
            {
                return (int)((BaseApi.now - StartTime).TotalSeconds + DungeonModel.ReadyTime);
            }
        }

        public void DispatchFieldObjectDeadMsg(FieldObject fieldObject)
        {
            MessageDispatcher?.Dispatch(TriggerMessageType.Dead, fieldObject);

            PcList.ForEach(x => x.Value.DispatchFieldObjectDeadMessage(fieldObject));
            HeroList.ForEach(x => x.Value.DispatchFieldObjectDeadMessage(fieldObject));
            MonsterList.ForEach(x => x.Value.DispatchFieldObjectDeadMessage(fieldObject));
        }

        public void DispatchFieldObjectMsg(FieldObject fieldObject, TriggerMessageType massageType,object msg)
        {
            HeroList.ForEach(x => x.Value.DispatchMessage(massageType,Tuple.Create(fieldObject,msg)));
        }

        public int GetPointState(DungeonResult result)
        {
            int pointState = 1;
            switch (result)
            {
                case DungeonResult.HelpCountUseUp:
                case DungeonResult.Success:
                    pointState = 1;
                    break;
                case DungeonResult.TimeOut:
                    pointState = 4;
                    break;
                case DungeonResult.None:
                case DungeonResult.Failed:
                case DungeonResult.Tie:
                default:
                    pointState = 2;
                    break;
            }

            return pointState;
        }

        /// <summary>
        /// 只有PVE副本，学院灵池才生效
        /// </summary>
        /// <returns></returns>
        public bool IsSchoolPoolBuffValid()
        {
            switch (Model.MapType)
            {
                case MapType.Tower:
                case MapType.SecretArea:
                case MapType.IslandChallenge:
                case MapType.Hunting:
                case MapType.HuntingDeficute:
                case MapType.HuntingTeamDevil:
                case MapType.HuntingTeamHelp:
                case MapType.HuntingActivitySingle:
                case MapType.HuntingActivityTeam:
                case MapType.HuntingActivityTeamHelp:
                case MapType.HuntingIntrude:
                case MapType.ThemeBoss:
                case MapType.CrossBoss:
                case MapType.IntegralBoss:
                case MapType.Chapter:
                case MapType.PushFigure:
                    return true;
                default:
                    return false;
            }
        }
    }
}