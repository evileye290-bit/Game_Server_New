using Logger;
using Message.Gate.Protocol.GateZ;
using System.IO;

namespace ZoneServerLib
{
    public partial class GateServer
    {
        public void OnResponse_GetChapterInfo(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_CHAPTER_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_CHAPTER_INFO>(stream);
            Log.Write("player {0} request get chapter info {1}", uid, msg.ChapterId);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GetChapterInfo not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.GetChapterInfo(msg.ChapterId);
        }

        public void OnResponse_ChapterNextPage(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_CHAPTER_NEXT_PAGE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_CHAPTER_NEXT_PAGE>(stream);
            Log.Write("player {0} request chapter {1} next page", uid, msg.ChapterId);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} BuyTimeSpacePower not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.GetChapterNextPageInfo(msg.ChapterId);
        }

        public void OnResponse_ChapterReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_CHAPTER_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_CHAPTER_REWARD>(stream);
            Log.Write("player {0} request chapter {1} reward {2} id {3}", uid, msg.ChapterId, msg.RewardType, msg.Id);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} ChapterReward not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.GetChapterReward(msg.ChapterId, msg.RewardType, msg.Id);
        }

        public void OnResponse_ChapterSweep(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_CHAPTER_SWEEP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_CHAPTER_SWEEP>(stream);
            Log.Write("player {0} request chapter sweep dungeon {1} count {2}", uid, msg.DungeonId, msg.SweepCount);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} ChapterSweep not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.ChapterSweep(msg.DungeonId, msg.SweepCount);
        }

        public void OnResponse_BuyTimeSpacePower(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_CHAPTER_BUY_POWER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_CHAPTER_BUY_POWER>(stream);
            Log.Write("player {0} request buy time spacePower {1}", uid, msg.Count);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} BuyTimeSpacePower not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.BuyTimeSpacePower(msg.Count);
        }
    }
}
