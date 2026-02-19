using System;
using DataProperty;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using ServerModels;

namespace ServerShared
{
    public static class BattlePowerLibrary
    {
        private static  Dictionary<int, BattlePowerSuppressModel> battlePowerSuppress = new Dictionary<int, BattlePowerSuppressModel>();

        public static int SoulRingBattlePowerBasic { get; private set; }
        public static int SoulBoneBattlePowerBasic { get; private set; }
        public static int SoulRingElementBattlePowerBasic { get; private set; }
        public static int StepBattlePowerBasic { get; private set; }
        
        public static void Init()
        {
            InitConfig();
            InitBattlePowerSuppress();
        }

        private static void InitConfig()
        {
            Data data = DataListManager.inst.GetData("BattlePowerConfig", 1);
            SoulRingBattlePowerBasic = data.GetInt("SoulRingBattlePowerBasic");
            SoulBoneBattlePowerBasic = data.GetInt("SoulBoneBattlePowerBasic");
            SoulRingElementBattlePowerBasic = data.GetInt("SoulRingElementBattlePowerBasic");
            StepBattlePowerBasic = data.GetInt("StepBattlePowerBasic");
        }

        private static void InitBattlePowerSuppress()
        {
            Dictionary<int, BattlePowerSuppressModel> battlePowerSuppress = new Dictionary<int, BattlePowerSuppressModel>();
            DataList dataList = DataListManager.inst.GetDataList("BattlePowerSuppress");
            foreach (var data in dataList)
            {
                BattlePowerSuppressModel model = new BattlePowerSuppressModel(data.Value);
                battlePowerSuppress.Add(model.DisparityRatio, model);
            }

            BattlePowerLibrary.battlePowerSuppress = battlePowerSuppress.OrderByDescending(x => x.Key)
                .ToDictionary(k => k.Key, v => v.Value);
        }

        public static BattlePowerSuppressModel GertBattlePowerSuppressModel(long attackerBP, long defenderBP)
        {
            long min = Math.Min(attackerBP, defenderBP);
            long max = Math.Max(attackerBP, defenderBP);

            int ratio = (int) ((max - min) / (max * 0.0001f));

            foreach (var kv in battlePowerSuppress)
            {
                if (ratio >= kv.Key) return kv.Value;
            }

            return null;
        }
    }
}
