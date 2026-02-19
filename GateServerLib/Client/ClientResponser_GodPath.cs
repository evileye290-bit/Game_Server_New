using System;
using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateZ;
using System.IO;
using CommonUtility;

namespace GateServerLib
{
    public partial class Client
    {
        public void OnResponse_GodHeroInfo(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GOD_HERO_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GOD_HERO_INFO>(stream);
            MSG_GateZ_GOD_HERO_INFO request = new MSG_GateZ_GOD_HERO_INFO();

            WriteToZone(request);
        }

        public void OnResponse_GodPathBuyPower(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GOD_PATH_BUY_POWER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GOD_PATH_BUY_POWER>(stream);
            MSG_GateZ_GOD_PATH_BUY_POWER request = new MSG_GateZ_GOD_PATH_BUY_POWER();
            request.Count = msg.Count;
            request.HeroId = msg.HeroId;

            WriteToZone(request);
        }

        public void OnResponse_GodPathSevenFightStart(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GOD_PATH_SEVEN_FIGHT_START msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GOD_PATH_SEVEN_FIGHT_START>(stream);
            MSG_GateZ_GOD_PATH_SEVEN_FIGHT_START request = new MSG_GateZ_GOD_PATH_SEVEN_FIGHT_START();
            request.HeroId = msg.HeroId;
            request.CardType = msg.CardType;

            WriteToZone(request);
        }

        public void OnResponse_GodPathSevenFightNextStage(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GOD_PATH_SEVEN_FIGHT_NEXT_STAGE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GOD_PATH_SEVEN_FIGHT_NEXT_STAGE>(stream);
            MSG_GateZ_GOD_PATH_SEVEN_FIGHT_NEXT_STAGE request = new MSG_GateZ_GOD_PATH_SEVEN_FIGHT_NEXT_STAGE();
            request.HeroId = msg.HeroId;

            WriteToZone(request);
        }

        public void OnResponse_GodPathUseItem(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GOD_PATH_USE_ITEM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GOD_PATH_USE_ITEM>(stream);
            MSG_GateZ_GOD_PATH_USE_ITEM request = new MSG_GateZ_GOD_PATH_USE_ITEM();
            request.HeroId = msg.HeroId;
            request.ItemId = ExtendClass.GetUInt64(msg.UidHigh, msg.UidLow);

            request.Count = msg.Count;

            WriteToZone(request);
        }

        public void OnResponse_GodPathTrainBodyBuyShield(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GOD_PATH_TRAIN_BODY_BUY msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GOD_PATH_TRAIN_BODY_BUY>(stream);
            MSG_GateZ_GOD_PATH_TRAIN_BODY_BUY request = new MSG_GateZ_GOD_PATH_TRAIN_BODY_BUY();
            request.HeroId = msg.HeroId;

            WriteToZone(request);
        }

        public void OnResponse_GodPathTrainBody(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GOD_PATH_TRAIN_BODY msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GOD_PATH_TRAIN_BODY>(stream);
            MSG_GateZ_GOD_PATH_TRAIN_BODY request = new MSG_GateZ_GOD_PATH_TRAIN_BODY();
            request.HeroId = msg.HeroId;

            WriteToZone(request);
        }

        public void OnResponse_GodPathFinishStageTask(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GOD_FINISH_STAGE_TASK msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GOD_FINISH_STAGE_TASK>(stream);
            MSG_GateZ_GOD_FINISH_STAGE_TASK request = new MSG_GateZ_GOD_FINISH_STAGE_TASK();
            request.HeroId = msg.HeroId;

            WriteToZone(request);
        }

        public void OnResponse_GodPathOceanHeartBuyCount(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GOD_PATH_BUY_OCEAN_HEART msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GOD_PATH_BUY_OCEAN_HEART>(stream);
            MSG_GateZ_GOD_PATH_BUY_OCEAN_HEART request = new MSG_GateZ_GOD_PATH_BUY_OCEAN_HEART();
            request.HeroId = msg.HeroId;
            WriteToZone(request);
        }

        public void OnResponse_GodPathOceanHeartRepaint(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GOD_PATH_REPAINT_OCEAN_HEART msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GOD_PATH_REPAINT_OCEAN_HEART>(stream);
            MSG_GateZ_GOD_PATH_REPAINT_OCEAN_HEART request = new MSG_GateZ_GOD_PATH_REPAINT_OCEAN_HEART();
            request.HeroId = msg.HeroId;
            WriteToZone(request);
        }

        public void OnResponse_GodPathOceanHeartDraw(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GOD_PATH_OCEAN_HEART_DRAW msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GOD_PATH_OCEAN_HEART_DRAW>(stream);
            MSG_GateZ_GOD_PATH_OCEAN_HEART_DRAW request = new MSG_GateZ_GOD_PATH_OCEAN_HEART_DRAW();
            request.HeroId = msg.HeroId;
            request.Index = msg.Index;
            WriteToZone(request);
        }


        public void OnResponse_GodPathTridentBuy(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GOD_PATH_BUY_TRIDENT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GOD_PATH_BUY_TRIDENT>(stream);
            MSG_GateZ_GOD_PATH_BUY_TRIDENT request = new MSG_GateZ_GOD_PATH_BUY_TRIDENT();
            request.HeroId = msg.HeroId;
            WriteToZone(request);
        }
        public void OnResponse_GodPathTridentUse(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GOD_PATH_USE_TRIDENT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GOD_PATH_USE_TRIDENT>(stream);
            MSG_GateZ_GOD_PATH_USE_TRIDENT request = new MSG_GateZ_GOD_PATH_USE_TRIDENT();
            request.HeroId = msg.HeroId;
            request.StrategyBuy = msg.StrategyBuy;
            WriteToZone(request);
        }
        public void OnResponse_GodPathTridentResult(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GOD_PATH_TRIDENT_RESULT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GOD_PATH_TRIDENT_RESULT>(stream);
            MSG_GateZ_GOD_PATH_TRIDENT_RESULT request = new MSG_GateZ_GOD_PATH_TRIDENT_RESULT();
            request.HeroId = msg.HeroId;
            request.RandomBuy = msg.RandomBuy;
            request.IsSuccess = msg.IsSuccess;
            WriteToZone(request);
        }

        public void OnResponse_GodPathTridentPush(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GOD_PATH_PUSH_TRIDENT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GOD_PATH_PUSH_TRIDENT>(stream);
            MSG_GateZ_GOD_PATH_PUSH_TRIDENT request = new MSG_GateZ_GOD_PATH_PUSH_TRIDENT();
            request.HeroId = msg.HeroId;
            WriteToZone(request);
        }

        public void OnResponse_GodPathAcrossOceanLightPuzzle(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GOD_PATH_LIGHT_PUZZLE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GOD_PATH_LIGHT_PUZZLE>(stream);
            MSG_GateZ_GOD_PATH_LIGHT_PUZZLE request = new MSG_GateZ_GOD_PATH_LIGHT_PUZZLE();
            request.HeroId = msg.HeroId;
            request.PuzzleIndex = msg.PuzzleIndex;
            WriteToZone(request);
        }

        public void OnResponse_GodPathAcrossOceanSweep(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GOD_PATH_ACROSS_OCEAN_SWEEP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GOD_PATH_ACROSS_OCEAN_SWEEP>(stream);
            MSG_GateZ_GOD_PATH_ACROSS_OCEAN_SWEEP request = new MSG_GateZ_GOD_PATH_ACROSS_OCEAN_SWEEP();
            request.Id = msg.Id;
            WriteToZone(request);
        }

    }
}
