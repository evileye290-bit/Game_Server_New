using ServerModels;
using ServerShared;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace ZoneServerLib
{
    /// <summary>
    /// Hero装备镶嵌一套某品质玄玉
    /// model param1 阶段
    /// </summary>
    public class HeroEquipmentInjectAction : BaseAction
    {
        public HeroEquipmentInjectAction(ActionManager manager, ActionModel model, ActionInfo actionInfo)
            : base(manager, model, actionInfo)
        {
        }

        public override bool Check(object obj)
        {
            int heroId;
            int.TryParse(obj.ToString(), out heroId);

            Dictionary<int, Slot> slot;
            if (!Owner.EquipmentManager.GetSlotDic().TryGetValue(heroId, out slot))
            {
                return false;
            }

            if (slot == null || slot.Count < 4) return false;

            //找出不满足条件的槽位
            foreach (var kv in slot)
            {
                BaseItem item = Owner.BagManager.GetItem(kv.Value.JewelUid);
                if (item == null) return false;

                ItemXuanyuModel curModel = Owner.EquipmentManager.GetXuanyuModel(kv.Value.JewelUid);
                if (curModel == null) return false;

                if (curModel.Level < model.Param1) return false;
            }

            //走sdk推荐礼包
            if (CheckActionBySdk(model.Param1))
            {
                return false;
            }

            AddNum(1, true);
            return true;
        }

        public override void SetFinishedBySdk(int current)
        {
            AddNum(1, true);
        }
    }
}
