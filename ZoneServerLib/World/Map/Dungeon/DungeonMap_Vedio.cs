using Message.Gate.Protocol.GateC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public partial class DungeonMap
    {

        private VedioManager vedioMng = new VedioManager();

        public int frameNum = 1;
        public float timeSpan = 0;

        private MSG_GC_BROADCAST_LIST broadcastList = new MSG_GC_BROADCAST_LIST();
        public MSG_GC_BROADCAST_LIST BroadcastList
        {
            get { return broadcastList; }
        }

        public void VedioUpdate(float delta)
        {
            //记录帧数和到开始的时间计数
            
            timeSpan += delta;
            vedioMng.Write(BroadcastList);
            broadcastList.List.Clear();
            frameNum++;
        }

        public void VedioRecordBroadCast<T>(T msg) where T : Google.Protobuf.IMessage
        {
            MSG_GC_BROADCAST_INFO broadcastMsg;
            if (CacheBroadcastMessage(msg, out broadcastMsg))
            {
                BroadcastList.List.Add(broadcastMsg);
            }
            else
            {
                vedioMng.Write(msg);
            }
        }


        public bool CacheBroadcastMessage<T>(T msg, out MSG_GC_BROADCAST_INFO broadcastMsg) where T : Google.Protobuf.IMessage
        {
            string msgName = msg.GetType().FullName;
            broadcastMsg = null; ;
            switch (msgName)
            {
                case "Message.Gate.Protocol.GateC.MSG_GC_CHAR_SIMPLE_INFO":
                    broadcastMsg = new MSG_GC_BROADCAST_INFO();
                    broadcastMsg.CharSimpleInfo = msg as MSG_GC_CHAR_SIMPLE_INFO;
                    return true;
                case "Message.Gate.Protocol.GateC.MSG_ZGC_HERO_SIMPLE_INFO":
                    broadcastMsg = new MSG_GC_BROADCAST_INFO();
                    broadcastMsg.HeroSimpleInfo = msg as MSG_ZGC_HERO_SIMPLE_INFO;
                    return true;
                case "Message.Gate.Protocol.GateC.MSG_ZGC_NPC_SIMPLE_INFO":
                    broadcastMsg = new MSG_GC_BROADCAST_INFO();
                    broadcastMsg.NpcSimpleInfo = msg as MSG_ZGC_NPC_SIMPLE_INFO;
                    return true;
                case "Message.Gate.Protocol.GateC.MSG_ZGC_MONSTER_SIMPLE_INFO":
                    broadcastMsg = new MSG_GC_BROADCAST_INFO();
                    broadcastMsg.MonsterInfo = msg as MSG_ZGC_MONSTER_SIMPLE_INFO;
                    return true;
                case "Message.Gate.Protocol.GateC.MSG_ZGC_ADD_BUFF":
                    broadcastMsg = new MSG_GC_BROADCAST_INFO();
                    broadcastMsg.AddBuff = msg as MSG_ZGC_ADD_BUFF;
                    return true;
                case "Message.Gate.Protocol.GateC.MSG_ZGC_REMOVE_BUFF":
                    broadcastMsg = new MSG_GC_BROADCAST_INFO();
                    broadcastMsg.RemoveBuff = msg as MSG_ZGC_REMOVE_BUFF;
                    return true;
                case "Message.Gate.Protocol.GateC.MSG_ZGC_SKILL_START":
                    broadcastMsg = new MSG_GC_BROADCAST_INFO();
                    broadcastMsg.SkillStart = msg as MSG_ZGC_SKILL_START;
                    return true;
                case "Message.Gate.Protocol.GateC.MSG_ZGC_SKILL_EFF":
                    broadcastMsg = new MSG_GC_BROADCAST_INFO();
                    broadcastMsg.SkillEff = msg as MSG_ZGC_SKILL_EFF;
                    return true;
                case "Message.Gate.Protocol.GateC.MSG_GC_FieldObject_MOVE":
                    broadcastMsg = new MSG_GC_BROADCAST_INFO();
                    broadcastMsg.Move = msg as MSG_GC_FieldObject_MOVE;
                    return true;
                case "Message.Gate.Protocol.GateC.MSG_ZGC_NPC_MOVE":
                    broadcastMsg = new MSG_GC_BROADCAST_INFO();
                    broadcastMsg.NpcMove = msg as MSG_ZGC_NPC_MOVE;
                    return true;
                case "Message.Gate.Protocol.GateC.MSG_ZGC_CHARACTER_STOP":
                    broadcastMsg = new MSG_GC_BROADCAST_INFO();
                    broadcastMsg.Stop = msg as MSG_ZGC_CHARACTER_STOP;
                    return true;

                case "Message.Gate.Protocol.GateC.MSG_GC_CHARACTER_LEAVE":
                    broadcastMsg = new MSG_GC_BROADCAST_INFO();
                    broadcastMsg.CharLeave = msg as MSG_GC_CHARACTER_LEAVE;
                    return true;
                case "Message.Gate.Protocol.GateC.MSG_ZGC_MONSTER_LEAVE":
                    broadcastMsg = new MSG_GC_BROADCAST_INFO();
                    broadcastMsg.MonsterLeave = msg as MSG_ZGC_MONSTER_LEAVE;
                    return true;
                case "MSG_ZGC_PET_LEAVEMSG_ZGC_NPC_LEAVE":
                    broadcastMsg = new MSG_GC_BROADCAST_INFO();
                    broadcastMsg.NpcLeave = msg as MSG_ZGC_NPC_LEAVE;
                    return true;
                case "MSG_ZGC_PET_LEAVE.MSG_ZGC_PET_LEAVE":
                    broadcastMsg = new MSG_GC_BROADCAST_INFO();
                    broadcastMsg.PetLeave = msg as MSG_ZGC_PET_LEAVE;
                    return true;

                case "Message.Gate.Protocol.GateC.MSG_ZGC_CHARACTER_HP":
                    broadcastMsg = new MSG_GC_BROADCAST_INFO();
                    broadcastMsg.Hp = msg as MSG_ZGC_CHARACTER_HP;
                    return true;
                case "Message.Gate.Protocol.GateC.MSG_ZGC_UPDATE_BASIC_NATURE":
                    broadcastMsg = new MSG_GC_BROADCAST_INFO();
                    broadcastMsg.UpdateNature = msg as MSG_ZGC_UPDATE_BASIC_NATURE;
                    return true;
                case "Message.Gate.Protocol.GateC.MSG_ZGC_FIELDOBJECT_REVIVE":
                    broadcastMsg = new MSG_GC_BROADCAST_INFO();
                    broadcastMsg.ReviveInfo = msg as MSG_ZGC_FIELDOBJECT_REVIVE;
                    return true;
            }
            return false;
        }
    }
}
