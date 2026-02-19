using CommonUtility;
using Message.Gate.Protocol.GateC;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    partial class FieldObject
    {
        public HolaManager HolaManager { get; private set; }

        public void InitHolaManager()
        {
            HolaManager = new HolaManager(this);
        }

        protected void InitHolaEffect()
        {
            currentMap.HolaEffect(this);
        }
    }
}
