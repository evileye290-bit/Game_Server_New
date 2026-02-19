using CommonUtility;
using DataProperty;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public partial class Monster : FieldObject, IDisposable
    {
        public override TYPE FieldObjectType { get { return TYPE.MONSTER; } }

        private MonsterModel monsterModel;
        public MonsterModel MonsterModel
        { get { return monsterModel; } }

        private Data dropData;
        public Data DropModel { get { return dropData; } }

        private BaseMonsterGen generator;
        public BaseMonsterGen Generator { get { return generator; } }

        public int Level
        { get { return monsterModel.Level; } }

        public float SearchRange
        { get { return monsterModel.SearchRange; } }

        public float FollowRange
        { get { return MonsterModel.FollowRange; } }
    
        public Vec2 GenCenter
        { get; private set; }

        // 出生动画中
        //public bool Borning { get; set; }

        #region methods
        public Monster(ZoneServerApi server)
            : base(server)
        {
            Borning = true;
            autoAI = true;
        }

        public void Init(int instance_id, FieldMap currentMap, MonsterModel model, BaseMonsterGen generator)
        {
            this.currentMap = currentMap;
            SetUid(instance_id);
            SetInstanceId(instance_id);
            monsterModel = model;
            radius = monsterModel.Radius;
            HateRatio = monsterModel.HateRatio;
            dropData = DataListManager.inst.GetData("Monster_Drop", generator.Model.Id);
            this.generator = generator;
            GenAngle = generator.Model.GenAngle;
            InitNature();
            InitBaseBattleInfo();
        }

        public void SetGenPos(Vec2 pos)
        {
            GenCenter = new Vec2(pos);
            SetPosition(pos);
        }

        public override void InitNature()
        {
            float growth = currentMap.MonsterGrowth;
            if (currentMap.Model.MapType == MapType.Tower)
            {
                foreach (var item in monsterModel.NatureList)
                {
                    switch (item.Key)
                    {
                        case NatureType.PRO_MAX_HP:
                        case NatureType.PRO_ATK:
                        case NatureType.PRO_DEF:
                        case NatureType.PRO_IMP:
                        case NatureType.PRO_ARM:
                            SetNatureBaseValue(item.Key, (long)(growth * item.Value));
                            break;
                        default:
                            SetNatureBaseValue(item.Key, item.Value);
                            break;
                    }
                }
            }
            else if (currentMap.Model.MapType == MapType.SpaceTimeTower)
            {
                List<int> natureTypes = SpaceTimeTowerLibrary.GetGrowthEffectMonsterNatures();
                foreach (var item in monsterModel.NatureList)
                {
                    if (natureTypes.Contains((int)item.Key))
                    {
                        SetNatureBaseValue(item.Key, (long)(growth * item.Value));
                    }
                    else
                    {
                        SetNatureBaseValue(item.Key, item.Value);
                    }
                }
            }
            else
            {
                foreach (var item in monsterModel.NatureList)
                {
                    if (NatureLibrary.Basic9Nature.ContainsKey(item.Key))
                    {
                        SetNatureBaseValue(item.Key, (long)(growth * item.Value));
                    }
                }
            }
            SetNatureBaseValue(NatureType.PRO_HP, GetMaxHp());
            long proSpd = monsterModel.GetNature(NatureType.PRO_SPD);
            
            SetNatureBaseValue(NatureType.PRO_SPD, proSpd);
            SetNatureBaseValue(NatureType.PRO_RUN_IN_BATTLE, proSpd);
            SetNatureBaseValue(NatureType.PRO_RUN_OUT_BATTLE, proSpd);
            SetNatureBaseValue(NatureType.PRO_MUL_CRI, monsterModel.GetNature(NatureType.PRO_MUL_CRI));
        }

        public override void BindSkills()
        {
            foreach(var skillInfo in monsterModel.SkillList)
            {
                string[] skillStr = skillInfo.Split(':');
                if(skillStr.Length != 2)
                {
                    continue;
                }
                int skillId = int.Parse(skillStr[0]);
                int skillLevel = int.Parse(skillStr[1]);
                skillManager.Add(skillId, skillLevel);
            }
        }

        private void BindTriggers()
        {
            foreach(var triggerId in monsterModel.TriggerList)
            {
                BaseTrigger trigger = new TriggerInMonster(this, triggerId);
                AddTrigger(trigger);
            }
        }

        protected override void OnUpdate(float deltaTime)
        {
            
        }

        public void GetSimpleInfo(MSG_ZGC_MONSTER_SIMPLE_INFO info)
        {
            if (info == null) return;
            
            MSG_ZGC_MONSTER_INFO monInfo=new MSG_ZGC_MONSTER_INFO();
            monInfo.Angle = GenAngle;
            monInfo.InstanceId = InstanceId;
            monInfo.MonsterId = monsterModel.Id;
            monInfo.Hp = GetHp();
            monInfo.MaxHp = GetMaxHp();
            monInfo.PosX = Position.x;
            monInfo.PosY = Position.y;
            monInfo.InRealBody = InRealBody;
            info.Borning = Borning;
            info.BornTime = GetBornTime();
            // 移动中 则同步移动信息
            info.MonsterInfo = monInfo;
            if (IsMoving)
            {
                info.DestX = MoveHandler.MoveToPosition.x;
                info.DestY = MoveHandler.MoveToPosition.y;
                info.Speed = MoveHandler.MoveSpeed;
            }
            else
            {
                info.DestX = 0f;
                info.DestY = 0f;
                info.Speed = 0f;
            }
        }

        public override void BroadcastSimpleInfo()
        {
            MSG_ZGC_MONSTER_SIMPLE_INFO info = new MSG_ZGC_MONSTER_SIMPLE_INFO();
            GetSimpleInfo(info);
            BroadCast(info);
        }

        public override void ResetRadius()
        {
            radius = monsterModel.Radius;
        }

        public override void StopFighting()
        {
            base.StopFighting();
            base.ClearBasicBattleState();
        }

        public void StopFighttingByReplease()
        {
            base.StopFighting();
            ClearBasicBattleState();
        }

        public override void ClearBasicBattleState()
        {
            if (TriggerMng != null)
            {
                TriggerMng.ClearSelfTriggers();
                TriggerMng.StopTriggersFromOther();
            }
            if (messageDispatcher != null)
            {
                messageDispatcher.Stop();
            }

            //需要先清除buff 然后再重置nature，防止某类buff结束广播血量，导致前端显示角色死亡
            if (buffManager != null)
            {
                buffManager.Stop();
            }
            skillManager = new SkillManager(this);
            //ResetNature();
            //DisableRealBody();
            hateManager = null;
            markManager = null;
        }

        //回收monster资源，只能再mapclose的时候调用该方法
        public void Dispose()
        {
            fsmManager = null;
            generator = null;
            currentMap = null;
        }


        public float GetBornTime()
        {
            if (Borning)
            {
                if (FsmManager != null && FsmManager.CurFsmState.FsmStateType == FsmStateType.MONSTER_BORN)
                {
                    FsmMonsterBorn bornFSM = FsmManager.CurFsmState as FsmMonsterBorn;
                    if (bornFSM != null)
                    {
                        return bornFSM.GetBornTime();
                    }
                    else
                    {
                        Log.Warn("monster {0} get born time error : fsm is null", MonsterModel.Id);
                        Borning = false;
                        return 0;
                    }
                }
                else
                {
                    //Log.Warn("monster {0} get born time error : fsm is {1}", MonsterModel.Id, FsmManager.CurFsmState.FsmStateType);
                    //Borning = false;
                    return MonsterModel.BornTime;
                }
            }
            else
            {
                return 0;
            }
        }
        #endregion


        public void SetMonsterGenerator(BaseMonsterGen generator)
        {
            this.generator = generator;
        }
    }
}
