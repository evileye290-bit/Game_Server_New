using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateZ;
using System.IO;

namespace GateServerLib
{
    public partial class Client
    {
        private void OnResponse_IslandChallengeInfo(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_ISLAND_CHALLENGE_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_ISLAND_CHALLENGE_INFO>(stream);
            MSG_GateZ_ISLAND_CHALLENGE_INFO request = new MSG_GateZ_ISLAND_CHALLENGE_INFO();

            WriteToZone(request);
        }

        private void OnResponse_IslandChallengeReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_ISLAND_CHALLENGE_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_ISLAND_CHALLENGE_REWARD>(stream);
            MSG_GateZ_ISLAND_CHALLENGE_REWARD request = new MSG_GateZ_ISLAND_CHALLENGE_REWARD() {Id = msg.Id, Type = msg.Type};

            WriteToZone(request);
        }

        private void OnResponse_IslandChallengeShopItemList(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_ISLAND_CHALLENGE_SHOP_ITEM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_ISLAND_CHALLENGE_SHOP_ITEM>(stream);
            MSG_GateZ_ISLAND_CHALLENGE_SHOP_ITEM request = new MSG_GateZ_ISLAND_CHALLENGE_SHOP_ITEM()
            {
                TaskId = msg.TaskId,
            };

            WriteToZone(request);
        }

        private void OnResponse_IslandChallengeExecuteTask(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_ISLAND_CHALLENGE_EXECUTE_TASK msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_ISLAND_CHALLENGE_EXECUTE_TASK>(stream);
            MSG_GateZ_ISLAND_CHALLENGE_EXECUTE_TASK request = new MSG_GateZ_ISLAND_CHALLENGE_EXECUTE_TASK()
            {
                TaskId = msg.TaskId,
                Param = msg.Param
            };

            WriteToZone(request);
        }

        private void OnResponse_IslandChallengeUpdateHeroPos(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_ISLAND_CHALLENGE_HERO_POS msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_ISLAND_CHALLENGE_HERO_POS>(stream);
            MSG_GateZ_ISLAND_CHALLENGE_UPDATE_HERO_POS request = new MSG_GateZ_ISLAND_CHALLENGE_UPDATE_HERO_POS();

            msg.HeroPos.ForEach(x => request.HeroPos.Add(new MSG_GateZ_HERO_POS() { Delete = x.Delete, HeroId = x.HeroId, PosId = x.PosId, Queue = x.Queue}));

            WriteToZone(request);
        }

        private void OnResponse_IslandChallengeReviveHero(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_GateZ_ISLAND_CHALLENGE_HERO_REVIVE request = new MSG_GateZ_ISLAND_CHALLENGE_HERO_REVIVE();

            WriteToZone(request);
        }

        private void OnResponse_IslandChallengeReset(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_GateZ_ISLAND_CHALLENGE_RESET request = new MSG_GateZ_ISLAND_CHALLENGE_RESET();

            WriteToZone(request);
        }

        private void OnResponse_IslandChallengeSwapQueue(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_ISLAND_CHALLENGE_SWAP_QUEUE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_ISLAND_CHALLENGE_SWAP_QUEUE>(stream);
            MSG_GateZ_ISLAND_CHALLENGE_SWAP_QUEUE request = new MSG_GateZ_ISLAND_CHALLENGE_SWAP_QUEUE()
            {
                Queue1 = msg.Queue1, Queue2 = msg.Queue2,
            };

            WriteToZone(request);
        }
    }
}
