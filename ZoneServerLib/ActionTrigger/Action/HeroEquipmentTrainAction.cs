using ServerModels;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    /// <summary>
    /// 某个Hero所有装备强化达到某阶段
    /// model param1 阶段
    /// </summary>
    public class HeroEquipmentTrainAction : BaseAction
    {
        private List<int> heroIds = new List<int>();

        public HeroEquipmentTrainAction(ActionManager manager, ActionModel model, ActionInfo actionInfo)
            : base(manager, model, actionInfo)
        {
        }

        protected override void InitActionInfo()
        {
            heroIds = actionInfo.Infos.ToList('|');
        }

        public override string BuildActionInfo()
        {
            return heroIds.ToString("|");
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

            if (slot == null || slot.Count < 4 || heroIds.Contains(heroId)) return false;

            //找出不满足条件的槽位
            Slot tempSlot = slot.Values.Where(x => x.EquipLevel < model.Param1).FirstOrDefault();
            if (tempSlot == null)
            {
                heroIds.Add(heroId);
                //走sdk推荐礼包
                if (CheckActionBySdk(model.Param1))
                {
                    return false;
                }
                SetNum(model.Param1, true);
                return true;
            }

            return false;
        }

        public override void SetFinishedBySdk(int current)
        {
            SetNum(model.Param1, true);
        }
    }
}
