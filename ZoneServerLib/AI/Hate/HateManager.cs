using CommonUtility;
using Message.Gate.Protocol.GateC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class HateManager : AbstractHateManager
    {
        protected FieldObject owner;
        protected float elapsedTime;
        protected float hateRange;
        protected float hateRefreshTime;
        protected float forgiveTime;

        

        // key instance id
        protected Dictionary<int, HateInfo> hateList;
        // 按照仇恨值由高到低排序
        protected List<HateInfo> hateOrderList;
        // instance id
        protected List<int> removeList;
        protected FieldObject target;
        public FieldObject Target
        {
            get
            {
                if (target != null && !target.IsDead) { return target; }
                return null;
            }
        }

        public HateManager(FieldObject owner, float hateRange, float hateRefreshTime, float forgiveTime)
        {
            this.owner = owner;
            hateList = new Dictionary<int, HateInfo>();
            hateOrderList = new List<HateInfo>();
            removeList = new List<int>();
            this.hateRange = hateRange;
            this.hateRefreshTime = hateRefreshTime;
            this.forgiveTime = forgiveTime;
        }

        public void AddHate(FieldObject target, int hate)
        {
            return;
            if (target == null)
            {
                return;
            }
            HateInfo info = null;
            if (hateList.TryGetValue(target.InstanceId, out info))
            {
                info.AddHate(hate);
            }
            else
            {
                info = new HateInfo(target, hate);
                hateList.Add(target.InstanceId, info);
            }
            info.ResetIdleTime();
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
                if (hate.IdleTime >= forgiveTime || hate.Target.IsDead)
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
            hateOrderList.Sort((left, right) =>
            {
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

        public void ClearTargetHate(FieldObject target)
        {
            if (target == null)
            {
                return;
            }
            hateList.Remove(target.InstanceId);
        }

        public void ClearTargetHateImmediately(FieldObject target)
        {
            if (target == null)
            {
                return;
            }
            if (this.target == target)
            {
                this.target = null;
            }
            hateList.Remove(target.InstanceId);
        }

        public void SetTarget(FieldObject newTarget)
        {
            if (owner.IsMonster && target != newTarget)
            {
                BroadcastHateInfo(target, newTarget);
            }
            else if(owner.IsRobot && target != newTarget)
            {
                BroadcastHateInfo(target, newTarget);
            }
            else if(owner.IsHero && target != newTarget && (owner as Hero).OwnerIsRobot)
            {
                BroadcastHateInfo(target, newTarget);
            }
            target = newTarget;
        }

        public void EnsureHasHate(FieldObject target)
        {
            if (!hateList.ContainsKey(target.InstanceId))
            {
                AddHate(target, 1);
            }
        }

        public void ClearAllHates()
        {
            hateList.Clear();
            SetTarget(null);
        }

        protected void BroadcastHateInfo(FieldObject oldTarget, FieldObject newTarget)
        {
            MSG_ZGC_HATE_INFO msg = new MSG_ZGC_HATE_INFO();
            msg.OwnerInstanceId = owner.InstanceId;
            msg.OldTargetInstanceId = oldTarget == null ? 0 : oldTarget.InstanceId;
            msg.NewTargetInstanceId = newTarget == null ? 0 : newTarget.InstanceId;
            owner.BroadCast(msg);
        }
    }
}
