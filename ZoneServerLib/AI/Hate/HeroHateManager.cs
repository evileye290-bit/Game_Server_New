using CommonUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class HeroHateManager : HateManager
    {
        public HeroHateManager(FieldObject owner, float hateRange, float hateRefreshTime, float forgiveTime=float.MaxValue)
            : base(owner, hateRange, hateRefreshTime, forgiveTime)
        {
        }

        public override void Update(float deltaTime)
        {
            elapsedTime += deltaTime;
            if (elapsedTime <= hateRefreshTime)
            {
                return;
            }
            elapsedTime = 0;
            removeList.Clear();
            hateOrderList.Clear();
            foreach (var item in hateList)
            {
                HateInfo hate = item.Value;
                hate.AddIdleTime(deltaTime);
                if (hate.IdleTime > forgiveTime || hate.Target.IsDead)
                {
                    removeList.Add(item.Key);
                }
                else if (!Vec2.InRange(owner.Position, hate.Target.Position, hateRange))
                {
                    removeList.Add(item.Key);
                }
                else
                {
                    hateOrderList.Add(hate);
                }
            }
            foreach (var item in removeList)
            {
                hateList.Remove(item);
            }
            hateOrderList.Sort((left, right) => {
                if (left.Hate > right.Hate)
                {
                    return -1;
                }
                if (left.Hate < right.Hate)
                {
                    return 1;
                }
                // 仇恨值相同
                if (Vec2.GetRangePower(left.Target.Position, owner.Position) < Vec2.GetRangePower(right.Target.Position, owner.Position))
                {
                    return -1;
                }
                return 1;
            });
            if (hateOrderList.Count > 0)
            {
                SetTarget(hateOrderList[0].Target);
                Logger.Log.Debug($"{owner.FieldObjectType} instance {owner.InstanceId} hate target {target.FieldObjectType} instance {target.InstanceId}");
            }
        }


    }
}
