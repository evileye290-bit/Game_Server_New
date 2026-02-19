using ServerFrame;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZoneServerLib
{
    public class DivMark : Mark
    {
        private Dictionary<int, DateTime> markValidTime = new Dictionary<int, DateTime>();

        public DivMark(MarkModel model) : base(model)
        {
        }

        public override bool Add(int instanceId, int count)
        {
            if (Locked)
            {
                return false;
            }
            DateTime avlidTime;
            if (!markValidTime.TryGetValue(instanceId, out avlidTime))
            {
                base.Add(instanceId, count);
                markValidTime.Add(instanceId, BaseApi.now.AddSeconds(base.Model.TimeToLive));
                return true;
            }
            else
            {
                markValidTime[instanceId] = BaseApi.now;
                return false;
            }
        }

        public override void Update(float dt)
        {
            if (Locked || curCount <= 0)
            {
                return;
            }

            TimeToLive -= dt;
            if (TimeToLive > 0)
            {
                return;
            }

            curCount--;
            TimeToLive = Model.TimeToLive;

            //移除一个过期的标记
            int removeId = 0;
            foreach (var kv in markValidTime)
            {
                if (kv.Value <= BaseApi.now)
                {
                    removeId = kv.Key;
                    break;
                }
            }

            //没有过期的标记默认移除第一个
            if (removeId == 0)
            {
                removeId = markValidTime.Keys.FirstOrDefault();
            }
            markValidTime.Remove(removeId);
        }
    }
}
