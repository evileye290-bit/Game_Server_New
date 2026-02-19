using EnumerateUtility;
using Logger;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnumerateUtility.SpaceTimeTower;
using ZoneServerLib;

namespace ServerModels
{
    public abstract class SpaceTimeBaseEvent
    {
        public SpaceTimeEventType EventType { get; private set; }
        public List<int> ParamList { get; private set; }
        public SpaceTimeTowerManager Manager { get; private set; }
        protected bool ongoing { get; set; }//事件是否进行中

        protected ZoneServerApi Server;
        protected SpaceTimeBaseEvent(int eventType, List<int> paramList, SpaceTimeTowerManager manager, ZoneServerApi server)
        {
            EventType = (SpaceTimeEventType)eventType;
            ParamList = new List<int>();
            if (paramList != null)
            {
                ParamList.AddRange(paramList);
            }
            Manager = manager;
            Server = server;
        }

        public void SetOngoingState(bool state)
        {
            ongoing = state;
        }

        public virtual bool CheckCanExecuteEvent(SpaceTimeEventType type, int param, List<int> lstParam)
        {
            if (type != EventType || !ParamList.Contains(param))
            {
                Log.Warn($"player {Manager.Owner.Uid} execute event {type} param {param} failed: type or param error");
                return false;
            }
            return true;
        }

        public abstract ErrorCode ExecuteEvent(SpaceTimeEventType type, int param, List<int> lstParam);
    }

    public class SpaceTimeDungeonEvent : SpaceTimeBaseEvent
    {
        public SpaceTimeDungeonEvent(int eventType, List<int> paramList, SpaceTimeTowerManager manager, ZoneServerApi server) : base(eventType, paramList, manager, server)
        {
        }

        public override ErrorCode ExecuteEvent(SpaceTimeEventType type, int param, List<int> lstParam)
        {
            ErrorCode errorCode = ErrorCode.Fail;
            if (Manager.HeroQueue.Count == 0)
            {
                Log.Warn($"player {Manager.Owner.Uid} execute event {type} param {param} failed: hero queue is empty");
                errorCode = ErrorCode.SpaceTimeTowerHeroQueueLimit;
                return errorCode;
            }
            errorCode = Manager.Owner.CanCreateDungeon(param);
            if (errorCode != ErrorCode.Success) return errorCode;

            SpaceTimeTowerDungeon dungeon = Manager.Owner.server.MapManager.CreateDungeon(param, 0, Manager.Owner.Uid) as SpaceTimeTowerDungeon; //等manager消息进行下一步
            if (dungeon == null)
            {
                Log.Write($"player {Manager.Owner.Uid} request to create dungeon {param} failed: create dungeon failed");
                return ErrorCode.CreateDungeonFailed;
            }
            //设置怪物成长属性
            dungeon.SetPlayer(Manager.Owner);
            dungeon.SetMonsterGrowth(SpaceTimeTowerLibrary.GetMonsterNatureGrowth(Manager.PersonalPeriod, Manager.TowerLevel));

            //添加队伍
            PetInfo queuePet = Manager.Owner.PetManager.GetDungeonQueuePet(DungeonQueueType.SpaceTimeTower, 1);
            dungeon.AddAttackerMirror(Manager.Owner, Manager.HeroQueue, queuePet);

            // 成功 进入副本
            Manager.Owner.RecordEnterMapInfo(dungeon.MapId, dungeon.Channel, dungeon.BeginPosition);
            Manager.Owner.RecordOriginMapInfo();
            Manager.Owner.OnMoveMap();

            SetOngoingState(true);
            
            return ErrorCode.Success;
        }

        public override bool CheckCanExecuteEvent(SpaceTimeEventType type, int param, List<int> lstParam)
        {
             if (ongoing)
             {
                 Log.Warn($"player {Manager.Owner.Uid} execute event {type} param {param} failed: event is ongoing");
                 return false;
             }
            return base.CheckCanExecuteEvent(type, param, lstParam);
        }
    }
    
    public class SpaceTimeShopEvent : SpaceTimeBaseEvent
    {
        public SpaceTimeShopEvent(int eventType, List<int> paramList, SpaceTimeTowerManager manager, ZoneServerApi server) : base(eventType, paramList, manager, server)
        {
        }

        public override ErrorCode ExecuteEvent(SpaceTimeEventType type, int param, List<int> lstParam)
        {
            object[] arrObj = new object[lstParam.Count];
            for (int i = 0; i < lstParam.Count; i++)
            {
                arrObj[i] = lstParam[i];
            }

            return Manager.Owner.OptProduct(param, arrObj);
        }
    }
    
    public class SpaceTimeHouseEvent : SpaceTimeBaseEvent
    {
        public SpaceTimeHouseEvent(int eventType, List<int> paramList, SpaceTimeTowerManager manager, ZoneServerApi server) : base(eventType, paramList, manager, server)
        {
        }

        public override ErrorCode ExecuteEvent(SpaceTimeEventType type, int param, List<int> lstParam)
        {
            object[] arrObj = new object[lstParam.Count];
            for (int i = 0; i < lstParam.Count; i++)
            {
                arrObj[i] = lstParam[i];
            }
            
            return Manager.Owner.OptProduct(param, arrObj);
        }
        
        public override bool CheckCanExecuteEvent(SpaceTimeEventType type, int param, List<int> lstParam)
        {
            return base.CheckCanExecuteEvent(type, param, lstParam);
        }
    }
}
