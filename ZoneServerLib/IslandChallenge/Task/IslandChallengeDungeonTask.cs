using System.Collections.Generic;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using ServerModels;
using ServerShared;

namespace ZoneServerLib
{
    public class IslandChallengeDungeonTask : BaseIslandChallengeTask
    {
        public IslandChallengeDungeonTask(IslandChallengeManager manager, int id) : base(manager, id, TowerTaskType.Dungeon)
        {
        }

        public override ErrorCode Execute(int param, MSG_ZGC_ISLAND_CHALLENGE_EXECUTE_TASK msg)
        {
            ErrorCode code = Manager.Owner.CheckCanCreateIslandChallengeDungeon(param);
            if (code != ErrorCode.Success) return code;

            code = Manager.Owner.CanCreateDungeon(param);
            if (code != ErrorCode.Success) return code;

            IslandChallengeTaskModel taskModel = IslandChallengeLibrary.GetIslandChallengeTaskModel(Manager.TaskId);
            IslandChallengeDungeon dungeon = Manager.Owner.server.MapManager.CreateDungeon(param, Manager.Owner.HeroMng.GetGoldHeroCount(), Manager.Owner.Uid) as IslandChallengeDungeon; //等manager消息进行下一步
            if (dungeon == null || taskModel == null)
            {
                Log.Write($"player {Manager.Owner.Uid} request to create dungeon {param} failed: create dungeon failed");
                return ErrorCode.CreateDungeonFailed;
            }

            Dictionary<int, int> heroPos = new Dictionary<int, int>();
            if (!Manager.GetHeroPos(param, heroPos))
            {
                Log.Write($"player {Manager.Owner.Uid} request to create dungeon {param} failed: have no heroPos Info");
                return ErrorCode.CreateDungeonFailed;
            }

            //设置怪物成长属性
            dungeon.SetMonsterGrowth(Manager.GetMonsterGrowth(taskModel.Chapter, taskModel.NodeId, taskModel.Difficulty));
            dungeon.SetHeroHp(Manager.HeroHp);
            dungeon.SetSkillEnergy(Manager.GetHeroSkillEnergy());
            dungeon.MonsterHeroSoulRingCount = Manager.GetMonsterHeroSoulRingCount();
            dungeon.SetPlayer(Manager.Owner);
            dungeon.SetPeriod(Manager.Period);

            //添加队伍
            Dictionary<int, PetInfo> queuePet = Manager.Owner.PetManager.GetIslandDungeonPet(param);
            dungeon.AddAttackerMirror(Manager.Owner, heroPos, queuePet);

            // 成功 进入副本
            Manager.Owner.RecordEnterMapInfo(dungeon.MapId, dungeon.Channel, dungeon.BeginPosition);
            Manager.Owner.RecordOriginMapInfo();
            Manager.Owner.OnMoveMap();

            return ErrorCode.Success;
        }
    }
}
