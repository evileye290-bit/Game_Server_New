using ServerModels;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    public abstract class PayAction : BaseAction
    {
        protected List<int> prodectIds = new List<int>();

        public PayAction(ActionManager manager, ActionModel model, ActionInfo actionInfo)
            : base(manager, model, actionInfo)
        {
        }

        public override bool Check(object obj)
        {
            return false;
        }

        protected override void InitActionInfo()
        {
            prodectIds.AddRange(actionInfo.Infos.ToList('|'));
        }

        public override string BuildActionInfo()
        {
            return prodectIds.ToString("|");
        }

        protected void AddProdectId(int id)
        {
            if (!prodectIds.Contains(id))
            {
                prodectIds.Add(id);
            }
        }
    }
}
