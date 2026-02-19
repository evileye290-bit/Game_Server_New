using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateZ;
using System.IO;

namespace GateServerLib
{
    public partial class Client
    {
        public void OnResponse_TowerInfo(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_TOWER_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_TOWER_INFO>(stream);
            MSG_GateZ_TOWER_INFO request = new MSG_GateZ_TOWER_INFO();

            WriteToZone(request);
        }

        public void OnResponse_TowerReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_TOWER_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_TOWER_REWARD>(stream);
            MSG_GateZ_TOWER_REWARD request = new MSG_GateZ_TOWER_REWARD()
            {
                Id = msg.Id,
            };

            WriteToZone(request);
        }

        public void OnResponse_TowerShopItemList(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_TOWER_SHOP_ITEM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_TOWER_SHOP_ITEM>(stream);
            MSG_GateZ_TOWER_SHOP_ITEM request = new MSG_GateZ_TOWER_SHOP_ITEM()
            {
                TaskId = msg.TaskId,
            };

            WriteToZone(request);
        }

        public void OnResponse_TowerExecuteTask(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_TOWER_EXECUTE_TASK msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_TOWER_EXECUTE_TASK>(stream);
            MSG_GateZ_TOWER_EXECUTE_TASK request = new MSG_GateZ_TOWER_EXECUTE_TASK()
            {
                TaskId = msg.TaskId,
                Param = msg.Param
            };

            WriteToZone(request);
        }

        public void OnResponse_TowerSelectBuff(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_TOWER_SELECT_BUFF msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_TOWER_SELECT_BUFF>(stream);
            MSG_GateZ_TOWER_SELECT_BUFF request = new MSG_GateZ_TOWER_SELECT_BUFF()
            {
                Index = msg.Index
            };

            WriteToZone(request);
        }

        public void OnResponse_TowerBuff(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_TOWER_BUFF msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_TOWER_BUFF>(stream);
            MSG_GateZ_TOWER_BUFF request = new MSG_GateZ_TOWER_BUFF();

            WriteToZone(request);
        }

        public void OnResponse_TowerUpdateHeroPos(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_UPDATE_TOWER_HERO_POS msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_UPDATE_TOWER_HERO_POS>(stream);
            MSG_GateZ_UPDATE_TOWER_HERO_POS request = new MSG_GateZ_UPDATE_TOWER_HERO_POS();

            msg.HeroPos.ForEach(x => request.HeroPos.Add(new MSG_GateZ_HERO_POS() { Delete = x.Delete, HeroId = x.HeroId, PosId = x.PosId }));

            WriteToZone(request);
        }

        public void OnResponse_TowerReviveHero(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_GateZ_TOWER_HERO_REVIVE request = new MSG_GateZ_TOWER_HERO_REVIVE();

            WriteToZone(request);
        }
    }
}
