using Logger;
using Message.Gate.Protocol.GateZ;
using System.IO;

namespace ZoneServerLib
{
    public partial class GateServer
    {
        public void OnResponse_GetIslandChallengeInfo(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_ISLAND_CHALLENGE_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_ISLAND_CHALLENGE_INFO>(stream);
            Log.Write("player {0} request get tower info", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GetIslandChallengeInfo not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.GetIslandChallengeInfo();
        }

        public void OnResponse_IslandChallengeReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_ISLAND_CHALLENGE_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_ISLAND_CHALLENGE_REWARD>(stream);
            Log.Write("player {0} request get tower reward {1}", uid, msg.Id);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} IslandChallengeReward not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.GetIslandChallengeReward(msg.Id, msg.Type);
        }

        public void OnResponse_IslandChallengeShopItemList(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_ISLAND_CHALLENGE_SHOP_ITEM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_ISLAND_CHALLENGE_SHOP_ITEM>(stream);
            Log.Write("player {0} request tower shopItemList : task {1}", uid, msg.TaskId);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} IslandChallengeShopItemList not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.IslandChallengeShopItemList(msg.TaskId);
        }

        public void OnResponse_IslandChallengeExecuteTask(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_ISLAND_CHALLENGE_EXECUTE_TASK msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_ISLAND_CHALLENGE_EXECUTE_TASK>(stream);
            Log.Write("player {0} request IslandChallengeExecuteTask: task {1} param {2}", uid, msg.TaskId, msg.Param);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} IslandChallengeExecuteTask not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.ExecuteIslandChallengeTask(msg.TaskId, msg.Param);
        }

        public void OnResponse_IslandChallengeUpdateHeroPos(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_ISLAND_CHALLENGE_UPDATE_HERO_POS msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_ISLAND_CHALLENGE_UPDATE_HERO_POS>(stream);
            Log.Write("player {0} request IslandChallengeUpdateHeroPos", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} IslandChallengeUpdateHeroPos not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.IslandChallengeUpdateHeroPos(msg.HeroPos);
        }

        public void OnResponse_IslandChallengeReviveHero(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_ISLAND_CHALLENGE_HERO_REVIVE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_ISLAND_CHALLENGE_HERO_REVIVE>(stream);
            Log.Write("player {0} request IslandChallengeReviveHero", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} IslandChallengeReviveHero not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.IslandChallengeReviveHero();
        }

        public void OnResponse_IslandChallengeReset(MemoryStream stream, int uid = 0)
        {
            //MSG_GateZ_ISLAND_CHALLENGE_RESET msg= MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_ISLAND_CHALLENGE_RESET>(stream);
            Log.Write("player {0} request IslandChallengeReviveHero", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} IslandChallengeReviveHero not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.IslandChallengeReset();
        }

        private void OnResponse_IslandChallengeSwapQueue(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_ISLAND_CHALLENGE_SWAP_QUEUE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_ISLAND_CHALLENGE_SWAP_QUEUE>(stream);
            Log.Write("player {0} request IslandChallengeSwapQueue", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} IslandChallengeSwapQueue not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.IslandChallengeSwapQueue(msg.Queue1, msg.Queue2);
        }
    }
}
