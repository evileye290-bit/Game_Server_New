using System.IO;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using Logger;
using EnumerateUtility;
using ServerModels;

namespace ZoneServerLib
{
    public partial class GateServer
    {
        public void OnResponse_CallPet(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_CALL_PET msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_CALL_PET>(stream);
            Log.Write("player {0} request to call pet {1}", uid, msg.Uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} call pet {1} failed: no such player", uid, msg.Uid);
                return;
            }
            player.CallPet(msg.Uid);
        }

        public void OnResponse_RecallPet(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_RECALL_PET msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_RECALL_PET>(stream);
            Log.Write("player {0} request to recall pet {1}", uid, msg.Uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} request to recall pet {1} failed: no such player", uid, msg.Uid);
                return;
            }
            player.RecallPet(msg.Uid);
        }

        public void OnResponse_HatchPetEgg(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_HATCH_PET_EGG msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_HATCH_PET_EGG>(stream);
            Log.Write("player {0} request to hatch pet egg {1}", uid, msg.Uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} hatch pet egg {1} failed: no such player", uid, msg.Uid);
                return;
            }
            player.HatchPetEgg(msg.Uid);
        }

        public void OnResponse_FinishHatchPetEgg(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_FINISH_HATCH_PET_EGG msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_FINISH_HATCH_PET_EGG>(stream);
            Log.Write("player {0} request to finish hatch pet egg {1}", uid, msg.Uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} finish hatch pet egg {1} failed: no such player", uid, msg.Uid);
                return;
            }
            player.FinishHatchPetEgg(msg.Uid);
        }

        public void OnResponse_ReleasePet(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_RELEASE_PET msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_RELEASE_PET>(stream);
            Log.Write("player {0} request to release pet", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} request to release pet failed: no such player", uid);
                return;
            }
            player.ReleasePet(msg.Uids);
        }

        public void OnResponse_ShowPetNature(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_SHOW_PET_NATURE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_SHOW_PET_NATURE>(stream);
            Log.Write("player {0} request to show pet {1} nature", uid, msg.Uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} request to show pet {1} nature failed: no such player", uid, msg.Uid);
                return;
            }
            player.ShowPetNature(msg.Uid);
        }

        public void OnResponse_PetLevelUp(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_PET_LEVEL_UP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_PET_LEVEL_UP>(stream);
            Log.Write("player {0} request to pet {1} level up isMulti {2}", uid, msg.Uid, msg.Multi);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} request to pet {1} level up failed: no such player", uid, msg.Uid);
                return;
            }
            player.PetLevelUp(msg.Uid, msg.Multi);
        }

        public void OnResponse_UpdateMainQueuePet(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_UPDATE_MAINQUEUE_PET msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_UPDATE_MAINQUEUE_PET>(stream);
            Log.Write("player {0} request to update main queue {1} pet {2} removeState {3}", uid, msg.QueueNum, msg.Uid, msg.Remove);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} request to update main queue {1} pet {2} removeState {3} failed: no such player", uid, msg.QueueNum, msg.Uid, msg.Remove);
                return;
            }
            player.UpdateMainQueuePet(msg.QueueNum, msg.Uid, msg.Remove);
        }

        public void OnResponse_PetInherit(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_PET_INHERIT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_PET_INHERIT>(stream);
            Log.Write("player {0} request to pet inherit fromPet {1} toPet {2}", uid, msg.FromUid, msg.ToUid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} request to pet inherit from {1} to {2} failed: no such player", uid, msg.FromUid, msg.ToUid);
                return;
            }
            player.PetInherit(msg.FromUid, msg.ToUid);
        }

        public void OnResponse_PetSkillBaptize(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_PET_SKILL_BAPTIZE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_PET_SKILL_BAPTIZE>(stream);
            Log.Write("player {0} request to pet {1} skill baptize, slot {2} useItem {3}", uid, msg.Uid, msg.Slot, msg.UseItem);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} request to pet {1} skill baptize, slot {2} useItem {3} failed: no such player", uid, msg.Uid, msg.Slot, msg.UseItem);
                return;
            }
            player.PetSkillBaptize(msg.Uid, msg.Slot, msg.UseItem);
        }

        public void OnResponse_PetBreak(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_PET_BREAK msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_PET_BREAK>(stream);
            Log.Write("player {0} request to pet break {1} consume {2}", uid, msg.Uid, msg.ConsumeUids);//----Check
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} request to pet break {1} consume {2} failed: no such player", uid, msg.Uid, msg.ConsumeUids);
                return;
            }
            player.PetBreak(msg.Uid, msg.ConsumeUids);
        }

        public void OnResponse_OneKeyPetBreak(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_ONE_KEY_PET_BREAK msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_ONE_KEY_PET_BREAK>(stream);
            Log.Write("player {0} request to one key pet break", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} request to one key pet break failed: no such player", uid);
                return;
            }

            player.OneKeyPetBreak(msg.List);
        }

        public void OnResponse_PetBlend(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_PET_BLEND msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_PET_BLEND>(stream);
            Log.Write("player {0} request to pet blend, main {1} blend {2}", uid, msg.MainUid, msg.BlendUid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} request to pet blend, main {1} blend {2} failed: no such player", uid, msg.MainUid, msg.BlendUid);
                return;
            }
            player.PetBlend(msg.MainUid, msg.BlendUid);
        }

        public void OnResponse_PetFeed(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_PET_FEED msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_PET_FEED>(stream);
            Log.Write("player {0} request to pet feed, pet {1} food {2}", uid, msg.Uid, msg.ItemId);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} request to pet feed, pet {1} food {2} failed: no such player", uid, msg.Uid, msg.ItemId);
                return;
            }
            player.PetFeed(msg.Uid, msg.ItemId);
        }

        public void OnResponse_UpdatePetDungeonQueue(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_UPDATE_PET_DUNGEON_QUEUE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_UPDATE_PET_DUNGEON_QUEUE>(stream);
            Log.Write("player {0} request to update pet dungeon queue: queueType {1} queueNum {2} pet {3}", uid, msg.QueueType, msg.QueueNum, msg.Uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} request to update pet dungeon queue: queueType {1} queueNum {2} pet {3} failed: no such player", uid, msg.QueueType, msg.QueueNum, msg.Uid);
                return;
            }
            player.UpdatePetDungeonQueue((DungeonQueueType)msg.QueueType, msg.QueueNum, msg.Uid, msg.Remove);
        }
    }
}
