using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class MonsterMoveModel
    {
        private int instanceId = 0;

        public int InstanceId
        {
            get { return instanceId; }
            set { instanceId = value; }
        }

        private double writeTime = 0.0;

        public double WriteTime
        {
            get { return writeTime; }
            set { writeTime = value; }
        }

        private bool isArrive = false;

        public bool IsArrive
        {
            get { return isArrive; }
            set { isArrive = value; }
        }

        private double autoTime = 10000.0;

        public double AutoTime
        {
            get { return autoTime; }
            set { autoTime = value; }
        }

        private bool canChange = false;

        public bool CanChange
        {
            get { return canChange; }
            set { canChange = value; }
        }
    }
}
