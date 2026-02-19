using CommonUtility;
using EnumerateUtility;
using Logger;
using ServerModels;
using ServerModels.Monster;
using ServerShared;
using System;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public class MonsterGenDelay
    {
        public int Count;
        public float Delay;
        public MonsterGenDelay(int count, float delay)
        {
            Count = count;
            Delay = delay;
        }
    }

    // One MonsterGen takes care of one type of monster
    public class BaseMonsterGen : IDisposable
    {
        protected FieldMap curMap;
        protected DungeonMap curDungeon;

        public MonsterGenModel Model
        { get; private set; }

        public int Id { get { return Model.Id; } }

        public MonsterGenType GenType
        { get { return Model.GenType; } }

        // 用于种怪点类型为 CircleRandom
        List<Vec2> genPosList;
        // 用于种怪点类型为 Queue
        Queue<Vec2> genPosQueue;
        public Vec2 GenCenter
        { get; private set; }

        // 是否已经种过怪
        protected bool generated = false;
        public bool Generated
        { get { return generated; } }

        private List<MonsterGenDelay> genDelayList = new List<MonsterGenDelay>();

        internal BaseMonsterGen(FieldMap map, MonsterGenModel model)
        {
            curMap = map;
            curDungeon = map as DungeonMap;
            Model = model;
            GenCenter = new Vec2(model.GenPosX, model.GenPosY);
            InitGenPos();
        }

        private void InitGenPos()
        {
            switch(Model.GenPosType)
            {
                case MonsterGenPosType.CircleRandom:
                    InitGenPos_CircleRandom();
                    break;
                case MonsterGenPosType.Queue:
                    InitGenPos_Queue();
                    break;
            }
        }
        private void InitGenPos_CircleRandom()
        {
            genPosList = new List<Vec2>();
            float genX = Model.GenPosX;
            float genY = Model.GenPosY;

            float minX = genX - Model.GenRange;
            float maxX = genX + Model.GenRange;

            float minY = genY - Model.GenRange;
            float maxY = genY + Model.GenRange;

            for (double x = minX; x <= maxX; x = x + 0.5)
            {
                for (double y = minY; y <= maxY; y = y + 0.5)
                {
                    if (curMap.IsWalkableAt((int)Math.Round(x), (int)Math.Round(y)))
                    {
                        genPosList.Add(new Vec2((float)x, (float)y));
                    }
                }
            }

            if (genPosList.Count == 0)
            {
                Log.Write($"map {curMap.MapId} monster regen id {Id} gen pos count is 0, check it!");
            }
        }

        private void InitGenPos_Queue()
        {
            genPosQueue = new Queue<Vec2>();
            string[] posArr = Model.GenPosParam.Split('|');
            foreach(var pos in posArr)
            {
                if(!string.IsNullOrEmpty(pos))
                {
                    Vec2 genPos = new Vec2(pos);
                    genPosQueue.Enqueue(genPos);
                }
            }
            
        }

        public Vec2 CalcGenPos()
        {
            switch(Model.GenPosType)
            {
                case MonsterGenPosType.CircleRandom:
                    return CalcGenPos_CircleRandom();
                case MonsterGenPosType.Queue:
                    return CalcGenPos_Queue();
                case MonsterGenPosType.Target:
                    return CalcTargetPos();
            }
            return curMap.BeginPosition;
        }

        private Vec2 CalcGenPos_CircleRandom()
        {
            if (genPosList == null || genPosList.Count == 0)
            {

                return curMap.BeginPosition;
            }
            return genPosList[RAND.Range(0, genPosList.Count - 1)];
        }

        private Vec2 CalcGenPos_Queue()
        {
            if (genPosQueue == null || genPosQueue.Count == 0)
            {
                return curMap.BeginPosition;
            }
            Vec2 pos = genPosQueue.Dequeue();
            genPosQueue.Enqueue(pos);
            return pos;
        }

        private Vec2 CalcTargetPos()
        {
            return new Vec2();
        }

        public virtual void Update(float dt)
        {
            //副本结束后，不再种怪
            if (curDungeon.State == DungeonState.Started)
            {
                CheckRegen();
                CheckGenDelay(dt);
            }
        }

        public void GenerateMonstersDelay(int count, float delay)
        {
            MonsterGenDelay genDelay = new MonsterGenDelay(count, delay);
            genDelayList.Add(genDelay);
        }

        private void GenerateMonsters(int count)
        {
            generated = true;
            BroadCast2Hero();
            for (int i = 0; i < count; i++)
            {
                Vec2 genPos = CalcGenPos();
                switch (Model.ModelType)
                {
                    case MonsterModelType.Monster:
                        curMap.CreateMonster(Model.MonsterId, genPos, this, 0);
                        break;
                    case MonsterModelType.Hero:
                        curMap.CreateMonsterHero(Model.MonsterId, genPos, this, 0);
                        break;
                    default:
                        break;
                }
            }
        }

        public void GenerateMonstersWithPosition(int count, Vec2 pos)
        {
            generated = true;
            BroadCast2Hero();
            for (int i = 0; i < count; i++)
            {
                switch (Model.ModelType)
                {
                    case MonsterModelType.Monster:
                        curMap.CreateMonster(Model.MonsterId, pos, this, 0);
                        break;
                    case MonsterModelType.Hero:
                        curMap.CreateMonsterHero(Model.MonsterId, pos, this, 0);
                        break;
                    default:
                        break;
                }
            }
        }

        private void BroadCast2Hero()
        {
            Vec2 dest = Model.WalkStraightVec;
            if (dest.magnitudePower != 0)
            {
                //获取出生的时间
                int monsterId = Model.MonsterId;
                //
                float bornTime = 0f;
                MonsterModel temp = MonsterLibrary.GetMonsterModel(monsterId);
                if (temp != null)
                {
                    bornTime = temp.BornTime;
                }
                curDungeon.BroadCastMonsterGen2Hero(Id, dest, bornTime);
                curDungeon.NotifyMonsterGen2Pet(bornTime);
            }
        }

        public virtual void CheckGen()
        {

        }

        public void CheckRegen()
        {
            // 还没有种过怪，则无需检查重刷
            if(!generated)
            {
                return;
            }
            if(Model.RegenType == MonsterRegenType.None)
            {
                return;
            }

            int aliveCount = curMap.GetAliveMonsterCountByGenId(Id);
            switch(Model.RegenType)
            {
                case MonsterRegenType.GenToMax:
                    if(aliveCount < Model.GenCount)
                    {
                        GenerateMonsters(Model.GenCount - aliveCount);
                    }
                    break;
                case MonsterRegenType.GenToMin:
                    int minCount = 0;
                    if(!int.TryParse(Model.RegenParam, out minCount))
                    {
                        break;
                    }
                    if(aliveCount < minCount)
                    {
                        GenerateMonsters(Model.GenCount - aliveCount);
                    }
                    break;
                default:
                    break;
            }
        }

        private void CheckGenDelay(float dt)
        {
            List<MonsterGenDelay> removeList = null;
            foreach(var item in genDelayList)
            {
                item.Delay -= dt;
                if(item.Delay <= 0)
                {
                    GenerateMonsters(item.Count);
                    if (removeList == null)
                    {
                        removeList = new List<MonsterGenDelay>();
                    }
                    removeList.Add(item);
                }
            }

            if(removeList != null)
            {
                //波数
                curDungeon.SetBattleStage(Model.BattleStage);

                foreach (var item in removeList)
                {
                    genDelayList.Remove(item);
                }
            }
        }

        public void Dispose()
        {
            genDelayList.Clear();
            curMap = null;
            curDungeon = null;
        }
    }
}
