using CommonUtility;
using ScriptFighting;
using ServerModels;
using System;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public class BaseBuff
    {
        protected FieldObject owner;
        protected FieldObject caster;

        protected BuffModel buffModel;
        public BuffModel Model
        { get { return buffModel; } }

        protected float skillLevel;

        public FieldObject Caster
        { get { return caster; } }
        public FieldObject Owner
        { get { return owner; } }


        public int Id
        { get { return buffModel.Id; } }

        public string Name
        { get { return buffModel.Name; } }

        public BuffType BuffType
        { get { return buffModel.BuffType; } }

        public BuffRefuseReplace RefuseRepalceType
        { get { return buffModel.RefuseReplaceType; } }

        public BuffSpecConType SpecConType
        { get { return buffModel.SpecConType; } }

        public bool Debuff
        { get { return buffModel.Debuff; } }

        public bool CleanUp
        { get { return buffModel.CleanUp; } }

        public bool Dispel
        { get { return buffModel.Dispel; } }

        protected float elapsedTime;

        public float LeftTime
        { get { return s - keepTime; } }

        protected float keepTime;
        public float KeepTime
        { get { return keepTime; } }

        protected bool isEnd = false;
        public bool IsEnd
        { get { return isEnd; } }

        public bool ControlledBuff
        { get { return buffModel.ControlledBuff; } }

        public int MaxPileNum { get; private set; }

        protected bool happened = false;

        //参数说明
        //伤害	m
        protected int m;
        public int M
        { get { return m; } }

        //半径	r
        protected float r;
        public float R
        { get { return r; } }

        //时间	s
        protected float s;
        public float S
        { get { return s; } }

        //概率	x, 10000代表概率必然事件
        protected int x;
        public int X
        { get { return x; } }

        //直线距离	d
        protected float d;
        public float D
        { get { return d; } }

        //回复量	n
        protected int n;
        public int N
        { get { return n; } }

        // 间隔 如每个deltaTime 回复n
        protected float deltaTime;
        public float DeltaTime
        { get { return deltaTime; } }

        //其他用数字的地方	通用数值c
        protected float c;
        public float C
        { get { return c; } }

        //其他用数字的地方	通用数值c
        protected float c2;

        //受属性影响
        protected NatureType natureType;
        //属性加成半分比
        protected float natureRatio;

        protected int pileNum = 1;
        public int PileNum => pileNum;

        protected int addByCasterNatureRatioValue;

        public BaseBuff(FieldObject caster, FieldObject owner, int skillLevel, BuffModel buffModel)
        {
            this.caster = caster; // 注意，caster可以为null,如场景触发给角色加buff
            this.owner = owner;
            this.buffModel = buffModel;
            this.skillLevel = skillLevel;
            InitParams();

            //不能默认为0，否则第一次不会生效
            elapsedTime = deltaTime;

            if (buffModel.DispatchCureSKillMsg)
            {
                caster.DispatchCastCureSkillMsg(new List<FieldObject>() { owner });
                caster.DispatchSkillAddCureBuffMsg(owner);
            }
        }

        protected void InitParams()
        {
            addByCasterNatureRatioValue = (int)(caster.GetNatureValue(buffModel.NatureType) * (buffModel.NatureRatio * 0.0001f));

            // 根据不同buff， 有不同的参数养成规则
            // 默认与BuffModel相同
            m = BuffParamCalculator.CalcM(buffModel.Name, skillLevel, buffModel.M) + addByCasterNatureRatioValue;
            r = BuffParamCalculator.CalcR(buffModel.Name, skillLevel, buffModel.R);
            s = BuffParamCalculator.CalcS(buffModel.Name, skillLevel, buffModel.S);
            x = BuffParamCalculator.CalcX(buffModel.Name, skillLevel, buffModel.X);
            d = BuffParamCalculator.CalcD(buffModel.Name, skillLevel, buffModel.D);
            n = BuffParamCalculator.CalcN(buffModel.Name, skillLevel, buffModel.N) + addByCasterNatureRatioValue;
            deltaTime = buffModel.DeltaTime;
            c = BuffParamCalculator.CalcC(buffModel.Name, skillLevel, buffModel.C) + addByCasterNatureRatioValue;

            MaxPileNum = Model.MaxPileNum;
        }

        public void OnStart()
        {
            isEnd = false;
            happened = false;
            keepTime = 0.0f;

            DispatchBuffStartMessage();
            Start();
            // todo 同步客户端
        }

        private void DispatchBuffStartMessage()
        {
            if (buffModel.ControlledBuff)
            {
                if (caster != null && caster.SubcribedMessage(TriggerMessageType.CastControlledBuff))
                {
                    caster.DispatchMessage(TriggerMessageType.CastControlledBuff, this);
                }
                if (owner.SubcribedMessage(TriggerMessageType.ControlledBuffStart))
                {
                    owner.DispatchMessage(TriggerMessageType.ControlledBuffStart, this);
                }
            }

            if (buffModel.Debuff)
            {
                if (caster != null && caster.SubcribedMessage(TriggerMessageType.CastDebuff))
                {
                    caster.DispatchMessage(TriggerMessageType.CastDebuff, this);
                }
            }

            if (caster != null && caster.SubcribedMessage(TriggerMessageType.CastBuff))
            {
                caster.DispatchMessage(TriggerMessageType.CastBuff, this);
            }

            if (caster != null && caster.SubcribedMessage(TriggerMessageType.CastTypebuff))
            {
                caster.DispatchMessage(TriggerMessageType.CastTypebuff, this);
            }

            if (owner.SubcribedMessage(TriggerMessageType.BuffStartId))
            {
                owner.DispatchMessage(TriggerMessageType.BuffStartId, new BuffStartTriMsg(Model.Id, (int)Model.BuffType));
            }

            if (owner.SubcribedMessage(TriggerMessageType.BuffStartType))
            {
                owner.DispatchMessage(TriggerMessageType.BuffStartType, new BuffStartTriMsg(Model.Id, (int)Model.BuffType));
            }

            owner.CurDungeon?.DispatchBridgeTriggerMessage(caster, TriggerMessageType.AllyCastTypeBuff, this);
        }

        protected virtual void Start()
        {
        }

        public void OnUpdate(float dt)
        {
            keepTime += dt;
            if (!isEnd)
            {
                UpdateBySpecialCondition();
                Update(dt);
            }
            if (keepTime >= s)
            {
                isEnd = true;
            }
        }

        protected virtual void Update(float dt)
        {          
        }

        public void OnEnd()
        {
            SendBuffEndMsg();

            End();
            isEnd = true;
            if (buffModel.ControlledBuff && owner.SubcribedMessage(TriggerMessageType.ControlledBuffEnd))
            {
                owner.DispatchMessage(TriggerMessageType.ControlledBuffEnd, this);
            }
        }

        protected virtual void SendBuffEndMsg()
        {
            if (owner.SubcribedMessage(TriggerMessageType.BuffEnd))
            {
                BuffEndTriMsg msg = new BuffEndTriMsg(Id, BuffEndReason.Time);
                owner.DispatchMessage(TriggerMessageType.BuffEnd, msg);
            }
        }

        //叠加层数
        protected virtual void Pile(int addNum)
        {
        }

        protected virtual void End()
        { 
        }

        // buff的特殊逻辑，如吸血buff根据造成的伤害进行血量回复
        public virtual void SpecLogic(object param)
        {
        }

        public bool ProbabilityHappened()
        {
            bool ans = false;
            if (x <= 0)
            {
                ans = false;
            }
            else if (x >= 10000)
            {
                ans = true;
            }
            else
            {
                ans= RAND.Range(1, 10000) <= x;
            }
            

            if (ans && Owner.SubcribedMessage(TriggerMessageType.BuffHappend))
            {
                Owner.DispatchMessage(TriggerMessageType.BuffHappend, null);
            }
            return ans;
        }

        protected void AddListener(TriggerMessageType messageType, Action<object> callback)
        {
            if(messageType == TriggerMessageType.None)
            {
                return;
            }

            MessageDispatcher dispatcher = owner.GetDispatcher();
            if (dispatcher != null)
            {
                dispatcher.AddListener(messageType, callback);
            }
        }

        protected void RemoveListener(TriggerMessageType messageType, Action<object> callback)
        {
            if(messageType == TriggerMessageType.None)
            {
                return;
            }

            MessageDispatcher dispatcher = owner.GetDispatcher();
            if (dispatcher != null)
            {
                dispatcher.RemoveListener(messageType, callback);
            }
        }

        // 改变buff的作用时长
        public void SetBuffDuringTime(float time)
        {
            s = time;
        }

        // 改名buff的伤害值
        public void SetDamage(int damage)
        {
            m = damage;
        }

        public void SetParamN(int cure)
        {
            n = cure;
        }

        public void SetParamC(float value)
        {
            c = value;
        }

        public void AddParamC(float value)
        {
            c += value; ;
        }

        public void AddTime(float time)
        {
            s += time;
        }

        public void SetNatureRatio(int value)
        {
            addByCasterNatureRatioValue = (int)(caster.GetNatureValue(buffModel.NatureType) * (value * 0.0001f));

            int changeRatio = value - buffModel.NatureRatio;
            int changeByCasterNatureRatioValue = (int)(caster.GetNatureValue(buffModel.NatureType) * (changeRatio * 0.0001f));

            // 根据不同buff， 有不同的参数养成规则
            // 默认与BuffModel相同
            m += changeByCasterNatureRatioValue;
            n += changeByCasterNatureRatioValue;
            c += changeByCasterNatureRatioValue;
        }

        protected virtual void UpdateBySpecialCondition()
        {
            switch (SpecConType)
            {
                case BuffSpecConType.LessHpRate:
                    if (owner.GetHpRate() * 10000 > buffModel.SpecParam)
                    {
                        isEnd = true;
                    }
                    break;
                case BuffSpecConType.GreaterHpRate:
                    if (owner.GetHpRate() * 10000 < buffModel.SpecParam)
                    {
                        isEnd = true;
                    }
                    break;
                default:
                    break;
            }
        }

        public void SetPileNum(int value)
        {
            pileNum = value;
        }

        public void AddPileNum(int value)
        {
            if (value > 0)
            {
                if (pileNum + value >= MaxPileNum)
                {
                    value = MaxPileNum - pileNum;
                }
                pileNum += value;
                ResetTime();
                Pile(value);
            }
            else
            {
                if (pileNum + value <= 0)
                {
                    OnEnd();
                    pileNum = 0;
                }
                else
                {
                    pileNum += value;
                    Pile(value);
                }
            }
        }

        public void ResetTime()
        {
            keepTime = 0f;
            s = BuffParamCalculator.CalcS(buffModel.Name, skillLevel, buffModel.S);
        }

        public void AddMaxPileNum(int value)
        {
            MaxPileNum += value;
            if (MaxPileNum <= 0)
            {
                MaxPileNum = 0;
            }
        }
    }
}
