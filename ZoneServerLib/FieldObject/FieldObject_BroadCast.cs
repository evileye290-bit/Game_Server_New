using Message.Gate.Protocol.GateC;
using System;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public partial class FieldObject
    {
        // NOTE : Sync
        public void BroadCastAll<T>(T msg) where T : Google.Protobuf.IMessage
        {
            if (CurrentMap == null) return;
            MSG_GC_BROADCAST_INFO broadcastMsg;
            if (CacheBroadcastMessage(msg, out broadcastMsg))
            {
                foreach (var player in currentMap.PcList)
                {
                    player.Value.BroadcastList.List.Add(broadcastMsg);
                }
                CurDungeon?.BattleFpsManager.WriteBroadcastMsg(broadcastMsg);
            }
            else
            {
                CurrentMap.BroadCast(msg);
            }
        }

        public virtual void BroadCast<T>(T msg) where T : Google.Protobuf.IMessage
        {
            if (CurrentMap == null) return;
            switch (CurrentMap.AoiType)
            {
                case EnumerateUtility.AOIType.Nearby:
                    BroadCastNearby(msg);
                    break;
                case EnumerateUtility.AOIType.All:
                    BroadCastAll(msg);
                    break;
                default:
                    break;
            }
        }

        private void BroadCastNearby<T>(T msg) where T : Google.Protobuf.IMessage
        {
            MSG_GC_BROADCAST_INFO broadcastMsg;
            if (CacheBroadcastMessage(msg, out broadcastMsg))
            {
                foreach (var player in CurRegion.PlayerList)
                {
                    player.Value.BroadcastList.List.Add(broadcastMsg);
                }
                for (int i = 0; i < 8; i++)
                {
                    if (CurRegion.NeighborList[i] != null)
                    {
                        foreach (var player in CurRegion.NeighborList[i].PlayerList)
                        {
                            player.Value.BroadcastList.List.Add(broadcastMsg);
                        }
                    }
                }
                return;
            }

            ArraySegment<byte> body;
            ushort bodyLen = 0;
            PlayerChar.BroadCastMsgBodyMaker(msg, out body, out bodyLen);
            if (curRegion != null)
            {
                foreach (var player in curRegion.PlayerList)
                {
                    ArraySegment<byte> header;
                    player.Value.BroadCastMsgHeaderMaker(msg, bodyLen, out header);
                    player.Value.Write(header, body);
                }
                for (int i = 0; i < 8; i++)
                {
                    if (curRegion.NeighborList[i] != null)
                    {
                        foreach (var player in curRegion.NeighborList[i].PlayerList)
                        {
                            ArraySegment<byte> header;
                            player.Value.BroadCastMsgHeaderMaker(msg, bodyLen, out header);
                            player.Value.Write(header, body);
                        }
                    }
                }
            }
        }

        // 广播targetList的九宫格交集，减少广播量
        Dictionary<int, Region> targetCurRegionList = new Dictionary<int, Region>();
        Dictionary<int, Region> broadcastRegionList = new Dictionary<int, Region>();
        public void BroadCastTargetsArea<T>(T msg, List<FieldObject> targetList) where T : Google.Protobuf.IMessage
        {
            if (currentMap.AoiType == EnumerateUtility.AOIType.All)
            {
                BroadCastAll(msg);
                return;
            }
            targetCurRegionList.Clear();
            broadcastRegionList.Clear();
            targetCurRegionList.Add(CurRegion.index, CurRegion);
            foreach (var target in targetList)
            {
                if (target.CurRegion != null && targetCurRegionList.ContainsKey(target.CurRegion.index) == false)
                {
                    targetCurRegionList.Add(target.CurRegion.index, target.CurRegion);
                }
            }
            foreach (var curRegionItem in targetCurRegionList)
            {
                if (broadcastRegionList.ContainsKey(curRegionItem.Key) == false)
                {
                    broadcastRegionList.Add(curRegionItem.Key, curRegionItem.Value);
                }
                for (int i = 0; i < 8; i++)
                {
                    Region neibor = curRegionItem.Value.NeighborList[i];
                    if (neibor != null && broadcastRegionList.ContainsKey(neibor.index) == false)
                    {
                        broadcastRegionList.Add(neibor.index, neibor);
                    }
                }
            }
            BroadCastRegionList(msg, broadcastRegionList);
        }

        public virtual void BroadCastRegionList<T>(T msg, Dictionary<int, Region> regionList) where T : Google.Protobuf.IMessage
        {
            MSG_GC_BROADCAST_INFO broadcastMsg;
            if (CacheBroadcastMessage(msg, out broadcastMsg))
            {
                foreach (var item in regionList)
                {
                    Region region = item.Value;
                    if (region != null)
                    {
                        foreach (var player in region.PlayerList)
                        {
                            player.Value.BroadcastList.List.Add(broadcastMsg);
                        }
                    }
                }
                return;
            }

            ArraySegment<byte> body;
            ushort bodyLen = 0;
            PlayerChar.BroadCastMsgBodyMaker(msg, out body, out bodyLen);

            foreach (var item in regionList)
            {
                Region region = item.Value;
                if (region != null)
                {
                    foreach (var player in region.PlayerList)
                    {
                        ArraySegment<byte> header;
                        player.Value.BroadCastMsgHeaderMaker(msg, bodyLen, out header);
                        player.Value.Write(header, body);
                    }
                }
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
                    broadcastMsg.AddBuff= msg as MSG_ZGC_ADD_BUFF;
                    return true;
                case "Message.Gate.Protocol.GateC.MSG_ZGC_REMOVE_BUFF":
                    broadcastMsg = new MSG_GC_BROADCAST_INFO();
                    broadcastMsg.RemoveBuff = msg as MSG_ZGC_REMOVE_BUFF;
                    return true;
                case "Message.Gate.Protocol.GateC.MSG_ZGC_SKILL_START":
                    broadcastMsg = new MSG_GC_BROADCAST_INFO();
                    broadcastMsg.SkillStart= msg as MSG_ZGC_SKILL_START;
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
                case "Message.Gate.Protocol.GateC.MSG_ZGC_PET_SIMPLE_INFO":
                    broadcastMsg = new MSG_GC_BROADCAST_INFO();
                    broadcastMsg.PetSimpleInfo = msg as MSG_ZGC_PET_SIMPLE_INFO;
                    return true;
            }
            return false;
        }

        public void BroadCastNearbyMsg<T>(T msg) where T : Google.Protobuf.IMessage
        {
            if (CurrentMap == null) return;
            BroadCastNearby(msg);
        }
    }
}