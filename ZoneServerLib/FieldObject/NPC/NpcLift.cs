using CommonUtility;
using DataProperty;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using ServerModels;
using ServerShared;
using System;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public class NpcLift : NPC
    {
        private Data liftData = null;
        private LiftState liftState = LiftState.Down;
        private float period = 0;
        public NpcLift(ZoneServerApi server):base(server)
        {
        }
        public override void Init(FieldMap currentMap, ZoneNPCModel model)
        {
            base.Init(currentMap, model);

            AddToAoi();
            liftData = DataListManager.inst.GetData("LiftConfig", 1);
            if (liftData == null)
            {
                Log.Warn("Init Npc Lift failed: lift config not exist");
            }
        }

        public override void GetSimpleInfo(MSG_ZGC_NPC_SIMPLE_INFO info)
        {
            if (info == null) return;
            base.GetSimpleInfo(info);
            info.Param = (int)liftState;
            info.Param2 = period;
        }

        protected override void OnUpdate(float dt)
        {
            period -= dt;
            if(period <= 0)
            {
                GetNextState();
                MSG_ZGC_NPC_SIMPLE_INFO npcSimpleInfo = new MSG_ZGC_NPC_SIMPLE_INFO();
                GetSimpleInfo(npcSimpleInfo);
                BroadCast(npcSimpleInfo);
            }
        }

        private void GetNextState()
        {
            switch (liftState)
            { 
                case LiftState.Down:
                    period = liftData.GetFloat("GoingUpPeriod");
                    liftState = LiftState.GoingUp;
                    break;
                case LiftState.GoingUp:
                    period = liftData.GetFloat("UpPeriod");
                    liftState = LiftState.Up;
                    break;
                case LiftState.Up:
                    period = liftData.GetFloat("GoingDownPeriod");
                    liftState = LiftState.GoingDown;
                    break;
                case LiftState.GoingDown:
                    period = liftData.GetFloat("DownPeriod");
                    liftState = LiftState.Down;
                    break;
                default:
                    Log.Warn("got invalid lift state {0}", liftState.ToString());
                    break;
            }
        }
    }
}
