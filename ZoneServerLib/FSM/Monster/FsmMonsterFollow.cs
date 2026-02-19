using CommonUtility;
using EnumerateUtility;
using System;

namespace ZoneServerLib
{
    public class FsmMonsterFollow : FsmBaseState
    {
        bool following;
        Vec2 targetPos;
        Vec2 followPos;

        private FieldObject target;
        private Skill skill;

        public FsmMonsterFollow(FieldObject owner)
            : base(owner)
        {
            fsmStateType = FsmStateType.MONSTER_FOLLOW;
        }
        protected override void Start(FsmStateType prevState)
        {
            if(monster.HateManager.Target == null)
            {
                GoToNextState(FsmStateType.MONSTER_SEARCH);
                return;
            }
            if (!InFollowRange())
            {
                StartFollowing();
            }
        }

        protected override void Update(float deltaTime)
        {
            if (following)
            {
                bool arrived = monster.OnMove(deltaTime);
                if(arrived)
                {
                    StopFollowing();
                }
            }

            elapsedTime += deltaTime;
            if (elapsedTime >= 0.5f)
            {
                elapsedTime = 0;
                if (monster.SkillEngine.TryFetchOneSkill(out skill, out target, out targetPos))
                {
                    GoToNextState(FsmStateType.MONSTER_ATTACK);
                    return;
                }
                if (CheckReturn())
                {
                    GoToNextState(FsmStateType.MONSTER_RETURN);
                    return;
                }
                if(monster.HateManager.Target == null)
                {
                    GoToNextState(FsmStateType.MONSTER_SEARCH);
                    return;
                }
                CheckFollowing();
            }
        }


        private bool CheckReturn()
        {
            return !Vec2.InRange(monster.GenCenter, monster.Position, monster.SearchRange);
        }

        private bool InFollowRange()
        {
            return Vec2.InRange(monster.Position, monster.HateManager.Target.Position, monster.FollowRange);
        }

        private void StartFollowing()
        {
            following = true;
            followPos = new Vec2(monster.HateManager.Target.Position);
            monster.SetDestination(followPos);
            monster.OnMoveStart();
            monster.BroadCastMove();
        }

        private void StopFollowing()
        {
            following = false;
            monster.OnMoveStop();
            monster.BroadCastStop();
        }

        private void RestartFollowing()
        {
            StopFollowing();
            StartFollowing();
        }

        private void CheckFollowing()
        {
            // 如果未在跟随中 则尝试跟随
            if(!following)
            {
                if(!InFollowRange())
                {
                    StartFollowing();
                }
                return;
            }

            // 跟随中 如果目标位置变化 则尝试重新跟随
            if (!Vec2.VeryClose(followPos, monster.HateManager.Target.Position))
            {
                RestartFollowing();
                return;
            }

            // 跟随中 且目标未动 如果在范围内 则停止跟随 
            if (InFollowRange())
            {
                StopFollowing();
                return;
            }

        }


        protected override void GoToNextState(FsmStateType state)
        {
            base.GoToNextState(state);
            if (following)
            {
                StopFollowing();
            }
        }

        protected override void End(FsmStateType nextState)
        {
            base.End(nextState);
            target = null;
            skill = null;
        }
    }
}
