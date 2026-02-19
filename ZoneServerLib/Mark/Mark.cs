using CommonUtility;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class Mark
    {
        private MarkModel model;
        public MarkModel Model
        { get { return model; } }

        protected int curCount;
        public int CurCount
        { get { return curCount; } }
        public int Id
        { get { return model.Id; } }

        private bool locked;
        public bool Locked
        { get { return locked; } }

        public MarkType Type
        { get { return model.Type; } }

        public float TimeToLive
        { get; protected set; }

        public int MaxCount { get; private set; }

        public Mark(MarkModel model)
        {
            this.model = model;
            curCount = 0;
            MaxCount = model.MaxCount;
            TimeToLive = model.TimeToLive;
            locked = false;
        }

        public static Mark CreateMark(MarkModel model)
        {
            if (model.DivMark)
            {
                return new DivMark(model);
            }
            return new Mark(model);
        }

        public virtual bool Add(int instanceId, int count)
        {
            if(locked)
            {
                return false;
            }
            if (curCount + count >= MaxCount)
            {
                curCount = MaxCount;
            }
            else
            { 
                curCount += count;
            }
            TimeToLive = model.TimeToLive;
            return true;
        }

        public virtual bool Reduce(int count)
        {
            locked = false;
            //if (locked)
            //{
            //    return false;
            //}
            curCount -= count;
            return true;
        }

        public void Reset()
        {
            locked = false;
            curCount = 0;
            TimeToLive = model.TimeToLive;
        }

        public void AddMaxMarkCount(int addValue)
        {
            MaxCount += addValue;

            if (addValue > 0)
            {
                locked = false;
            }
            else
            {
                if (MaxCount <= 0)
                {
                    MaxCount = 0;
                }
            }
        }

        public bool Enough()
        {
            return curCount >= MaxCount;
        }

        public void Lock()
        {
            locked = true;
        }
        
        public virtual void Update(float dt)
        {
            if(locked || curCount <= 0)
            {
                return;
            }
            TimeToLive -= dt;
            if(TimeToLive <= 0)
            {
                curCount--;
                TimeToLive = model.TimeToLive;
            }
        }
    }
}
