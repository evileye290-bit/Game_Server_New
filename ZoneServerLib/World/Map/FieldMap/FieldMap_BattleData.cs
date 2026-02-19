namespace ZoneServerLib
{
    public partial class FieldMap
    {
        private BattleDataManager battleDataManager;

        public BattleDataManager BattleDataManager { get { return battleDataManager; } }

        public void InitBattleDataManaget()
        {
            battleDataManager = new BattleDataManager(this);
        }


        public void RecordBattleDataHurt(FieldObject caster, FieldObject target, BattleDataType dataType, long num)
        {
            //屏蔽自己对自己造成的伤害
            if (caster.InstanceId == target.InstanceId) return;

            battleDataManager.AddBattleDataHurt(caster, target, dataType, num);
        }

        public void RecordBattleDataCure(FieldObject caster, FieldObject target, BattleDataType dataType, long num)
        { 
            battleDataManager.AddBattleDataCure(caster, target, dataType, num);
        }

        public BattleData GetBattleDataByInstanceIdAndId(int instanceId, int id)
        {
             return battleDataManager.GetBattleDataByInstanceIdAndId(instanceId, id);
        }
    }
}
