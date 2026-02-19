using System;
using System.Collections.Generic;
using Message.Gate.Protocol.GateC;
using ServerModels;

namespace ZoneServerLib
{
    public partial class PlayerChar
    {

        private Dictionary<int, InteractionMessage> InteractionList = new Dictionary<int, InteractionMessage>();

        public void Interact(FieldObject sender, int zoneNpcId, string message, string argString, int argInt, float argFloat)
        {
            InteractionMessage msg = new InteractionMessage();
            if (sender != null)
            {
                msg.Id = sender.InstanceId;
            }
            else
            {
                msg.Id = InstanceId;
            }
            msg.ZoneNpcId = zoneNpcId;
            msg.Message = message;
            msg.ParamString = argString;
            msg.ParamInt = argInt;
            msg.ParamFloat = argFloat;
            msg.SendTime = ZoneServerApi.now.AddMilliseconds(25);
            InteractionList[msg.Id] = msg;
        }

    

        public void SendDelayMessage()
        {
            List<int> removeList = new List<int>();
            foreach (var delayMsg in InteractionList)
            {
                if (delayMsg.Value.SendTime <= ZoneServerApi.now)
                {
                    MSG_ZGC_INTERACTION msg = new MSG_ZGC_INTERACTION();
                    msg.ZoneNpcId = delayMsg.Value.ZoneNpcId;
                    msg.Message = delayMsg.Value.Message;
                    msg.ArgString = delayMsg.Value.ParamString;
                    msg.ArgInt = delayMsg.Value.ParamInt;
                    msg.ArgFloat = delayMsg.Value.ParamFloat;
                    msg.InstanceId = delayMsg.Value.Id;
                    Write(msg);

                    removeList.Add(delayMsg.Key);
                }
            }
            foreach (var id in removeList)
            {
                InteractionList.Remove(id);
            }
            removeList.Clear();
        }
    }
}