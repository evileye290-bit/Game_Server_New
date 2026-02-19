using Logger;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class TriggerInHero:BaseTrigger
    {
        public override FieldMap CurMap
        { get { return Owner.CurrentMap; } }

        public TriggerInHero(FieldObject owner, int triggerId) : base(owner, owner)
        {
            TriggerModel model = TriggerInHeroLibrary.GetModel(triggerId);
            if (model == null)
            {
                Log.Warn("create trigger in hero {0} failed: no such trigger", triggerId);
                return;
            }

            Init(model);
        }

        public override MessageDispatcher GetMessageDispatcher()
        {
            return Owner.GetDispatcher();
        }
    }
}
