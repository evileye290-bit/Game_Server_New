using Logger;
using Message.Gate.Protocol.GateZ;
using System.IO;

namespace ZoneServerLib
{
    public partial class GateServer
    {
        public void OnResponse_GetTowerInfo(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_TOWER_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_TOWER_INFO>(stream);
            Log.Write("player {0} request get tower info", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GetTowerInfo not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.GetTowerInfo();
        }

        public void OnResponse_TowerReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_TOWER_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_TOWER_REWARD>(stream);
            Log.Write("player {0} request get tower reward {1}", uid, msg.Id);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} TowerReward not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.GetTowerReward(msg.Id);
        }

        public void OnResponse_TowerShopItemList(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_TOWER_SHOP_ITEM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_TOWER_SHOP_ITEM>(stream);
            Log.Write("player {0} request tower shopItemList : task {1}", uid, msg.TaskId);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} TowerShopItemList not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.TowerShopItemList(msg.TaskId);
        }

        public void OnResponse_TowerExecuteTask(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_TOWER_EXECUTE_TASK msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_TOWER_EXECUTE_TASK>(stream);
            Log.Write("player {0} request TowerExecuteTask: task {1} param {2}", uid, msg.TaskId, msg.Param);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} TowerExecuteTask not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.ExecuteTowerTask(msg.TaskId, msg.Param);
        }

        public void OnResponse_TowerSelectBuff(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_TOWER_SELECT_BUFF msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_TOWER_SELECT_BUFF>(stream);
            Log.Write("player {0} request TowerSelectBuff: index {1}", uid, msg.Index);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} TowerSelectBuff not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.TowerSelectBuff(msg.Index);
        }

        public void OnResponse_TowerBuff(MemoryStream stream, int uid = 0)
        {
            PlayerChar player = Api.PCManager.FindPc(uid);
            Log.Write("player {0} request tower buff", uid);

            if (player == null)
            {
                Log.Warn("player {0} TowerBuff not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.TowerBuffList();
        }

        public void OnResponse_TowerUpdateHeroPos(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_UPDATE_TOWER_HERO_POS msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_UPDATE_TOWER_HERO_POS>(stream);
            Log.Write("player {0} request TowerUpdateHeroPos", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} TowerUpdateHeroPos not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.TowerUpdateHeroPos(msg.HeroPos);
        }

        public void OnResponse_TowerReviveHero(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_TOWER_HERO_REVIVE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_TOWER_HERO_REVIVE>(stream);
            Log.Write("player {0} request TowerReviveHero", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} TowerReviveHero not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.TowerReviveHero();
        }
    }
}
