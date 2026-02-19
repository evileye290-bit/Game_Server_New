using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using ServerModels;
using ServerShared;

namespace ZoneServerLib
{
    public class DungeonTowerTask : TowerTask
    {
        public DungeonTowerTask(TowerManager manager, int id) : base(manager, id, TowerTaskType.Dungeon)
        {
        }

        public override ErrorCode Execute(int param, MSG_ZGC_TOWER_EXECUTE_TASK msg)
        {
            TowerDungeonModel dungeonModel = TowerLibrary.GetTowerDungeonModel(param);
            if (dungeonModel == null) return ErrorCode.Fail;

            if (TaskId != Manager.TaskId) return ErrorCode.Fail;

            TowerTaskModel taskModel = TowerLibrary.GetTaskModel(TaskId);
            if (dungeonModel == null) return ErrorCode.Fail;

            if (Manager.CheckPosDeadHero()) return ErrorCode.TowerEquipDeadHero;

            if (Manager.HeroPos.Count == 0) return ErrorCode.TowerFormationNoHero;

            ErrorCode code = Manager.Owner.CanCreateDungeon(dungeonModel.Id);
            if (code != ErrorCode.Success) return code;

            TowerDungeon dungeon = Manager.Owner.server.MapManager.CreateDungeon(dungeonModel.Id, Manager.Owner.HeroMng.GetGoldHeroCount(), Manager.Owner.Uid) as TowerDungeon; //等manager消息进行下一步
            if (dungeon == null)
            {
                Log.Write($"player {Manager.Owner.Uid} request to create dungeon {dungeonModel.Id} failed: create dungeon failed");
                return ErrorCode.CreateDungeonFailed;
            }

            //设置怪物成长属性
            dungeon.SetMonsterGrowth(Manager.GetMosterGrowth(taskModel.Chapter, taskModel.ChapterNodeIndex, taskModel.Quality));
            dungeon.SetTowerBuff(Manager.BuffInfoList);
            dungeon.SetPeriod(Manager.Period);
            dungeon.SetHeroHp(Manager.HeroHp);
            dungeon.SetMonsterHp(Manager.MonsterHP);
            dungeon.SetSkillEnergy(Manager.GetHeroSkillEnergy());
            dungeon.MonsterHeroSoulRingCount = Manager.GetMonsterHeroSoulRingCount();

            // 成功 进入副本
            Manager.Owner.RecordEnterMapInfo(dungeon.MapId, dungeon.Channel, dungeon.BeginPosition);
            Manager.Owner.RecordOriginMapInfo();
            Manager.Owner.OnMoveMap();

            return ErrorCode.Success;
        }


        public override bool CheckFinished()
        {
            if (Manager.MonsterHP.Count == 0) return false;

            foreach (var kv in Manager.MonsterHP)
            {
                if (kv.Value > 0) return false;
            }

            return true;
        }
    }
}
