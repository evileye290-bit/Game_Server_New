using CommonUtility;
using EnumerateUtility;
using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class FsmMonsterSearchState : FsmBaseState
    {
        private Vec2 dest = null;
        private FieldObject target = null;
        private Vec2 targetPos = null;
        private List<FieldObject> targetList = new List<FieldObject>();

        public FsmMonsterSearchState(FieldObject owner)
            : base(owner)
        {
            fsmStateType = FsmStateType.MONSTER_SEARCH;
        }

        private bool CheckUpdate()
        {
            if(((owner.CurrentMap as DungeonMap)?.OnePlayerDone ?? true) && ((owner.CurrentMap as DungeonMap)?.OnePlayerPetDone ?? true))
            {
                return true;
            }
            return false;
        }
        protected override void Start(FsmStateType prevState)
        {
            target = null;
            targetPos = null;
            targetList.Clear();

            if (!CheckUpdate())
            {
                return;
            }
            if (FindTarget())
            {
                GoToNextState(FsmStateType.MONSTER_ATTACK);
                return;
            }
            if(monster.HateManager.Target != null)
            {
                GoToNextState(FsmStateType.MONSTER_FOLLOW);
                return;
            }
            //StartSearching();
        }

        protected override void Update(float deltaTime)
        {
            elapsedTime += deltaTime;
            if (elapsedTime >= 0.2)
            {
                elapsedTime = 0;
                if (!CheckUpdate())
                {
                    return;
                }
                // 执行找目标逻辑
                if (FindTarget())
                {
                    // 找到，则退出当前状态，由FsmAIUpdate进行跳转
                    isEnd = true;
                    monster.OnMoveStop();
                    monster.BroadCastStop();
                    GoToNextState(FsmStateType.MONSTER_ATTACK);
                    return;
                }
            }
            //bool arrived = monster.OnMove(deltaTime);
            //if (arrived)
            //{
            //    StartSearching();
            //}
        }

        protected override void End(FsmStateType nextState)
        {
            base.End(nextState);
            dest = null;
            target = null;
            targetPos = null;
            targetList.Clear();
        }

        private Vec2 NextSearchPosition()
        {
            Vec2 position;
            for (int i = 0; i < 3; i++)
            {
                position = Vec2.GetRandomPos(monster.GenCenter, monster.SearchRange);
                if (monster.CurrentMap.IsWalkableAt((int)Math.Round(position.x), (int)Math.Round(position.y)))
                {
                    return position;
                }
            }
            position = new Vec2(monster.GenCenter);
            return position;
        }

        private void StartSearching()
        {
            dest = NextSearchPosition();
            monster.SetDestination(dest);
            monster.OnMoveStart();
            monster.BroadCastMove();
        }

        private bool FindTarget()
        {
            targetList.Clear();
            target = null;
            targetPos = null;

            // 确保HateManager有Target
            if (monster.HateManager.Target == null)
            {
                monster.GetEnemyInSplash(owner, SplashType.Circle, monster.Position, owner.Position, monster.SearchRange, 0f, 0f, targetList, 10, -1, true);
                if (targetList.Count > 0)
                {
                    FieldObject temp = null;
                    //float disPower = float.MaxValue;
                    //foreach(var item in targetList)
                    //{
                    //    float tempDisPower = (item.Position - owner.Position).magnitudePower;
                    //    if (tempDisPower <= disPower)
                    //    {
                    //        temp = item;
                    //        disPower = tempDisPower;
                    //    }
                    //}

                    float curMaxHateRatio = 0f;
                    float curMinLengthPow = float.MaxValue;
                    foreach (var item in targetList)
                    {
                        float tempHateRatio = 0f;
                        float tempLengthPow = 0f;
                        if (temp == null)
                        {
                            temp = item;
                            curMaxHateRatio = item.HateRatio;
                            curMinLengthPow = (owner.Position-item.Position).magnitudePower;
                            continue;
                        }

                        tempHateRatio = item.HateRatio;
                        tempLengthPow = (owner.Position - item.Position).magnitudePower;
                        if (tempLengthPow < curMinLengthPow || (tempLengthPow == curMinLengthPow && tempHateRatio > curMaxHateRatio))
                        {
                            temp = item;
                            curMaxHateRatio = tempHateRatio;
                            curMinLengthPow = tempLengthPow;
                        }
                    }
                    monster.HateManager.SetTarget(temp);

                    Skill skill;
                    if (monster.SkillEngine.TryFetchOneSkill(out skill, out target, out targetPos))
                    {
                        // 有准备好的技能，且该技能满足释放条件
                        return true;
                    }
                }
            }

     
            return false;
        }

    }
}
