using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class HateInfo
    {
        private FieldObject target;
        public FieldObject Target
        { get { return target; } }

        private int hate;
        public int Hate
        { get { return hate; } }

        private float idleTime;
        public float IdleTime
        { get { return idleTime; } }

        public HateInfo(FieldObject target, int hate)
        {
            this.target = target;
            this.hate = hate;
        }

        public void AddHate(int value)
        {
            this.hate += value;
        }

        public void ResetHate()
        {
            this.hate = 0;
        }

        public void AddIdleTime(float deltaTime)
        {
            idleTime += deltaTime; 
        }

        public void ResetIdleTime()
        {
            idleTime = 0;
        }
    }
}
