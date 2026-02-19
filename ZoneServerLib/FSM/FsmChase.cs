using CommonUtility;
using EnumerateUtility;
using System;

namespace ZoneServerLib
{
    public class FsmChaseState : FsmBaseState
    {

        // 为了定时切换小网格寻路
        bool m_useSmallPath = false;
        float m_switchSmallPathTime = 1.0f;
        float m_totalDuringTime = 0.0f;

        //float m_range = 1.0f;
        //上面的赋值原来用于“和目标要保持的距”计算里增加距离，现不需要，去掉了，有BUG可以砍我——陈驰
        float ownerAndChaseTargetRange = 0.0f;
        FieldObject target = null;

        public FsmChaseState(FieldObject owner)
            : base(owner)
        {
            fsmStateType = FsmStateType.CHASE;
        }

        protected override void Start(FsmStateType prevState)
        {
            target = startParam as FieldObject;
            if (target == null || owner.CurrentMap == null)
            {
                // 没有找到chase目标 放弃
                owner.FsmManager.SetNextFsmStateType(FsmStateType.IDLE);
                owner.BroadCastStop();
                return;
            }
            //获取寻路目标

            //和目标需要保持的距离
            ownerAndChaseTargetRange = (float)Math.Pow(owner.Radius + target.Radius, 2);

            if (!IsInRange())
            {
                //与目标距离不在指定范围内
                if (!player.InFly() && !player.InTreasureFly())
                {
                    //不是飞行NPC且不是飞行藏宝图，更新目标坐标，开始移动
                    owner.SetDestination(target.Position);
                }
                else if(player.InFly())
                {
                    //飞行NPC飞行到目标点
                    owner.SetDestination(player.tempTaskFlyInfo.end);
                }
                else if (player.InTreasureFly())
                {
                    //飞行藏宝图飞行到目标点
                    owner.SetDestination(player.treasureFlyInfo.end);
                }
                owner.OnMoveStart();
                owner.BroadCastMove();
            }

            //是否使用高精度格子
            m_useSmallPath = owner.IsInSmallGrid();
            m_totalDuringTime = 0.0f;
        }

        protected override void Update(float deltaTime)
        {
            if (target == null)
            {
                //目标找不到停止寻路
                isEnd = true;
                return;
            }

            bool isArrived = IsInRange();
            if (!isArrived)
            {
                //与目标距离不在指定范围内
                if (owner.CheckDestination())
                {
                    //需要改变目标路径
                    owner.OnMoveStart();
                    owner.BroadCastMove();
                }

                m_totalDuringTime += deltaTime;
                if (!m_useSmallPath && m_totalDuringTime >= m_switchSmallPathTime)
                {
                    if (owner.IsInSmallGrid())
                    {
                        if (!player.InFly() && !player.InTreasureFly())
                        {
                            owner.SetDestination(target.Position);
                        }
                        m_useSmallPath = true;
                        m_totalDuringTime = 0.0f;
                    }
                }
                // 移动
                isArrived = owner.OnMove(deltaTime);
            }

            if (isArrived)
            {
                //到达目的地
                isEnd = true;
                switch (target.FieldObjectType)
                {
                    case TYPE.NPC:
                        {
                            if (player != null)
                            {
                                player.EndFly();
                                player.BroadCastStop();

                                NPC trigger = (NPC)target;
                                trigger.OnClick(player);
                            }
                        }
                        break;
                    case TYPE.PROPBOOK:
                        {
                            if (player != null)
                            {
                                player.BroadCastStop();
                                PropBook trigger = (PropBook)target;
                                trigger.OnClick(player);
                            }
                        }
                        break;
                    case TYPE.GOODS:
                        {
                            if (player != null)
                            {
                                player.BroadCastStop();

                                Goods trigger = (Goods)target;
                                trigger.OnClick(player);
                            }
                        }
                        break;
                    case TYPE.TREASURE:
                        {
                            if (player != null)
                            {
                                player.EndTreausureFly();
                                player.BroadCastStop();

                                Treasure trigger = (Treasure)target;
                                trigger.OnClick(player);
                                player.SendShovelRewards();
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }


        protected override void End(FsmStateType nextState)
        {
            owner.OnMoveStop();
            base.End(nextState);
            target = null;
        }


        public bool IsInRange()
        {
            if (target != null)
            {
                Vec2 vec = owner.TmpVec;
                Vec2.OperatorMinus(target.Position, owner.Position, ref vec);
                return owner.TmpVec.magnitudePower <= ownerAndChaseTargetRange;
            }
            return false;
        }
    }
}