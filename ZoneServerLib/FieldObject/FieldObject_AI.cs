using CommonUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    partial class FieldObject
    {
        // 是否执行AI，对于客户端 autoAI = false
        // 对于 monster，hero autoAI = true
        protected bool autoAI = false;
        public bool AutoAI
        { get { return autoAI; } }

        protected SkillEngine skillEngine;
        public SkillEngine SkillEngine
        { get { return skillEngine; } }

        protected HateManager hateManager;
        public HateManager HateManager
        { get { return hateManager; } }

        public float HateRatio;//攻击别人时使用

        public virtual void InitAI()
        {
        }

        public bool Borning = false;

        /// <summary>
        /// 需要在战斗之前初始化完毕，否则战斗会有trigger不生效问题
        /// </summary>
        public void InitBaseBattleInfo()
        {
            InitSkillManager();
            InitBuffManager();
            InitMarkManager();
            InitHolaManager();

            skillEngine = new SkillEngine(this);

            InitTrigger();
        }
    }
}
