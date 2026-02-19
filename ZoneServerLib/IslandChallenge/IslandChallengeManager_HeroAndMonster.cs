using System.Collections.Generic;
using System.Linq;
using EnumerateUtility;
using Google.Protobuf.Collections;
using Logger;
using Message.Gate.Protocol.GateZ;
using ScriptFunctions;
using ServerModels;
using ServerShared;

namespace ZoneServerLib
{
    public partial class IslandChallengeManager
    {
        public float GetMonsterGrowth(int chapter, int node, int difficulty)
        {
            float growth = ScriptManager.TowerManager.CaculateIslandChallengeDungeonGrowth(BattlePower, chapter, node, difficulty);
            Log.Debug($"--------------------------island challenge growth {growth}");
            return growth;
        }

        public int GetMonsterHeroSoulRingCount()
        {
            int count = HeroLevel / 10 + 1;
            return count;
        }

        public bool SetHeroPos(RepeatedField<MSG_GateZ_HERO_POS> heroPos, int queue)
        {
            IEnumerable<MSG_GateZ_HERO_POS> list = heroPos.Where(x => x.Queue == queue);

            if (list.Count() > 5)
            {
                return false;
            }

            HeroPos.Remove(queue);
            list.ForEach(x => HeroPos.Add(x.Queue, x.HeroId, x.PosId));
         
            SyncHeroToDB();
            return true;
        }

        public void SwapQueue(int queue1, int queue2)
        {
            Dictionary<int, int> queue1Hero;
            Dictionary<int, int> queue2Hero;
            if (HeroPos.TryGetValue(queue1, out queue1Hero) &&
                HeroPos.TryGetValue(queue2, out queue2Hero))
            {
                HeroPos[queue1] = queue2Hero;
                HeroPos[queue2] = queue1Hero;
            }
            SyncHeroToDB();
        }

        public int GetHeroPos(int queue, int heroId)
        {
            int pos = -1;
            if (HeroPos.TryGetValue(queue,heroId, out pos)) return pos;
            return -1;
        }

        public int GetHeroPosByDungeonId(int dungeonId, int heroId)
        {
            if (Task == null || Task.Type != TowerTaskType.Dungeon) return 0;
            IslandChallengeDungeonModel model = IslandChallengeLibrary.GetIslandChallengeDungeonModel(Task.TaskInfo.param[0]);
            if (model == null) return 0;

            if (!model.Dungeon2Queue.ContainsKey(dungeonId)) return 0;

            int pos = 0;
            Dictionary<int, int> heroPos1;
            if (HeroPos.TryGetValue(model.Dungeon2Queue[dungeonId], out heroPos1) && heroPos1.TryGetValue(heroId, out pos))
            {
                return pos;
            }

            return 0;
        }

        public bool GetHeroPos(int dungeonId, Dictionary<int,int> heroPos)
        {
            if (Task == null || Task.Type != TowerTaskType.Dungeon) return false;

            IslandChallengeDungeonModel model = IslandChallengeLibrary.GetIslandChallengeDungeonModel(Task.TaskInfo.param[0]);
            if (model == null) return false;

            if (!model.Dungeon2Queue.ContainsKey(dungeonId)) return false;
            Dictionary<int, int> heroPos1;
            if (!HeroPos.TryGetValue(model.Dungeon2Queue[dungeonId], out heroPos1))
            {
                return false;
            }

            heroPos1.ForEach(x => heroPos.Add(x.Key, x.Value));
            return true;
        }

        public bool CheckDeadHero()
        {
            foreach (KeyValuePair<int,Dictionary<int,int>> queue in HeroPos)
            {
                if (CheckPosDeadHero(queue.Value)) return true;
            }

            return false;
        }

        public bool CheckPosDeadHero(Dictionary<int, int> heroInts)
        {
            foreach (var heroPos in heroInts)
            {
                if (DeadHeroList.Contains(heroPos.Key)) return true;
            }

            return false;
        }

        public void SetHeroAndMonsterHP(Dictionary<int, float> heroHp, Dictionary<int, float> monsterHp)
        {
            foreach (var kv in heroHp)
            {
                if (kv.Value == 1)
                {
                    HeroHp.Remove(kv.Key);
                    continue;
                }
                else if (kv.Value <= 0)
                {
                    HeroHp.Remove(kv.Key);
                    DeadHeroList.Add(kv.Key);
                    continue;
                }

                HeroHp[kv.Key] = kv.Value;
            }

            foreach (var kv in monsterHp)
            {
                if (kv.Value == 1) continue;
            }

            SyncHeroToDB();
        }

        public void UpdateHeroSkillEnergy(Dictionary<int, Dictionary<int, int>> heroSkillEnergy)
        {
            foreach (var kv in heroSkillEnergy)
            {
                //清除旧数据
                RemoveSkillEnergy(kv.Key);

                foreach (var item in kv.Value)
                {
                    AddHeroSkillEnergy(kv.Key, item.Key, item.Value);
                }
            }
        }

        public Dictionary<int, Dictionary<int, int>> GetHeroSkillEnergy() => heroSkilEnergy;

        private void RemoveSkillEnergy(int heroId)
        {
            heroSkilEnergy.Remove(heroId);
        }

        public void AddHeroSkillEnergy(int hero, int skillId, int energy)
        {
            if (energy <= 0) return;

            Dictionary<int, int> skillEnergy;
            if (!heroSkilEnergy.TryGetValue(hero, out skillEnergy))
            {
                skillEnergy = new Dictionary<int, int>();
                heroSkilEnergy.Add(hero, skillEnergy);
            }
            skillEnergy[skillId] = energy;
        }

        public void ReviveAllHero()
        {
            ReviveCount += 1;
            DeadHeroList.Clear();
            HeroHp.Clear();
            SyncHeroToDB();
        }

        public Dictionary<int, int> GetQueue(int queue)
        {
            Dictionary<int, int> queueHero;
            HeroPos.TryGetValue(queue, out queueHero);
            return queueHero;
        }
    }
}
