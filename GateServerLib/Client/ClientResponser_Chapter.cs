using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateZ;
using System.IO;

namespace GateServerLib
{
    public partial class Client
    {
        public void OnResponse_ChapterInfo(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_CHAPTER_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_CHAPTER_INFO>(stream);
            MSG_GateZ_CHAPTER_INFO request = new MSG_GateZ_CHAPTER_INFO();
            request.ChapterId = msg.ChapterId;

            WriteToZone(request);
        }

        public void OnResponse_ChapterReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_CHAPTER_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_CHAPTER_REWARD>(stream);
            MSG_GateZ_CHAPTER_REWARD request = new MSG_GateZ_CHAPTER_REWARD()
            {
                RewardType = msg.RewardType,
                Id = msg.Id,
                ChapterId = msg.ChapterId
            };

            WriteToZone(request);
        }

        public void OnResponse_ChapterSweep(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_CHAPTER_SWEEP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_CHAPTER_SWEEP>(stream);
            MSG_GateZ_CHAPTER_SWEEP request = new MSG_GateZ_CHAPTER_SWEEP()
            {
                DungeonId = msg.DungeonId,
                SweepCount = msg.SweepCount
            };

            WriteToZone(request);
        }

        public void OnResponse_BuyTimeSpacePower(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_CHAPTER_BUY_POWER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_CHAPTER_BUY_POWER>(stream);
            MSG_GateZ_CHAPTER_BUY_POWER request = new MSG_GateZ_CHAPTER_BUY_POWER()
            {
                Count = msg.Count,
            };

            WriteToZone(request);
        }

        public void OnResponse_ChapterNextPage(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_CHAPTER_NEXT_PAGE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_CHAPTER_NEXT_PAGE>(stream);
            MSG_GateZ_CHAPTER_NEXT_PAGE request = new MSG_GateZ_CHAPTER_NEXT_PAGE();
            request.ChapterId = msg.ChapterId;

            WriteToZone(request);
        }
    }
}
