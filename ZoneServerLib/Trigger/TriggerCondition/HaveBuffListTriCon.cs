using CommonUtility;
using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class HaveBuffListTriCon : BaseTriCon
    {
        private readonly int buffId;
        private List<int> buffList;
        public HaveBuffListTriCon(BaseTrigger trigger, TriggerCondition conditionType, string conditionParam)
            : base(trigger, conditionType, conditionParam)
        {
            string[] buffs = conditionParam.Split('|');
            if (buffs.Length < 2)
            {
                Log.Warn($"init HaveBuffListTriCon failed: invalid buffType {conditionParam}");
                return;
            }
            buffList = new List<int>();
            foreach (var buff in buffs)
            {
                if (int.TryParse(buff, out buffId))
                {
                    buffList.Add(buffId);
                }
            }
        }

        public override bool Check()
        {
            foreach (var buff in buffList)
            {
                if (trigger.Owner?.BuffManager.GetBuff(buff) == null)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
