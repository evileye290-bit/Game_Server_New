using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateZ;
using System.IO;

namespace GateServerLib
{
    partial class Client
    {
        private void OnResponse_CreateDungeon(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_CREATE_DUNGEON msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_CREATE_DUNGEON>(stream);
            MSG_GateZ_CREATE_DUNGEON request = new MSG_GateZ_CREATE_DUNGEON
            {
                HuntingHelp = msg.HuntingHelp,
                DungeonId = msg.DungeonId,
                Uid = Uid
            };
            WriteToZone(request);
        }

        private void OnResponse_LeaveDungeon(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_LEAVE_DUNGEON msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_LEAVE_DUNGEON>(stream);
            MSG_GateZ_LEAVE_DUNGEON request = new MSG_GateZ_LEAVE_DUNGEON();
            request.Uid = Uid;
            WriteToZone(request);
        }

        private void OnResponse_DungeonStopBattle(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_DUNGEON_STOP_BATTLE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_DUNGEON_STOP_BATTLE>(stream);
            MSG_GateZ_DUNGEON_STOP_BATTLE request = new MSG_GateZ_DUNGEON_STOP_BATTLE();
            request.Uid = Uid;
            WriteToZone(request);
        }

        private void OnResponse_DungeonResult(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_DUNGEON_RESULT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_DUNGEON_RESULT>(stream);
            MSG_GateZ_DUNGEON_RESULT request = new MSG_GateZ_DUNGEON_RESULT();
            request.Result = msg.Result;
            WriteToZone(request);
        }

        private void OnResponse_DungeonBattleData(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_DUNGEON_BATTLE_DATA msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_DUNGEON_BATTLE_DATA>(stream);
            MSG_GateZ_DUNGEON_BATTLE_DATA request = new MSG_GateZ_DUNGEON_BATTLE_DATA();
            WriteToZone(request);
        }

        private void OnResponse_VerifyQuitDungeon(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_VERIFY_QUIT_DUNGEON msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_VERIFY_QUIT_DUNGEON>(stream);
            WriteToZone(new MSG_GateZ_VERIFY_QUIT_DUNGEON() { RequestUid = msg.RequestUid, Verifyed = msg.Verify});
        }

        private void OnResponse_DungeonSpeedUp(MemoryStream stream)
        {
            if (curZone == null) return;
            WriteToZone(new MSG_GateZ_DUNGEON_SPEED_UP());
        }

        private void OnResponse_DungeonSkipDungeon(MemoryStream stream)
        {
            if (curZone == null) return;
            WriteToZone(new MSG_GateZ_DUNGEON_SKIP_BATTLE() );
        }
    }
}
