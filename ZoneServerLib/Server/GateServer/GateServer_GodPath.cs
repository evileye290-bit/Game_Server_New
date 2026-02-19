using Logger;
using Message.Gate.Protocol.GateZ;
using System.IO;

namespace ZoneServerLib
{
    public partial class GateServer
    {
        public void OnResponse_GodHeroInfo(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GOD_HERO_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GOD_HERO_INFO>(stream);
            Log.Write("player {0} request GodHeroInfo", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GodHeroInfo not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.GodPathGetHeroInfo();
        }

        public void OnResponse_BuyGodPathPower(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GOD_PATH_BUY_POWER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GOD_PATH_BUY_POWER>(stream);
            Log.Write("player {0} request buy god path power {1} to hero {2}", uid, msg.Count, msg.HeroId);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} BuyGodPathPower not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.BuyGodPathPower(msg.HeroId, msg.Count);
        }

        public void OnResponse_GodPathSevenFightStart(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GOD_PATH_SEVEN_FIGHT_START msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GOD_PATH_SEVEN_FIGHT_START>(stream);
            Log.Write("player {0} request god path seven fight start, heroId {1} cardType {2}", uid, msg.HeroId, msg.CardType);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GodPathSevenFightStart not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.GodPathSevenFightStart(msg.HeroId, msg.CardType);
        }

        public void OnResponse_GodPathSevenFightNextStage(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GOD_PATH_SEVEN_FIGHT_NEXT_STAGE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GOD_PATH_SEVEN_FIGHT_NEXT_STAGE>(stream);
            Log.Write("player {0} request god path seven fight next stage, heroId {1}", uid, msg.HeroId);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GodPathSevenFightNextStage not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.GodPathSevenFightNextStage(msg.HeroId);
        }

        public void OnResponse_GodPathUseItem(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GOD_PATH_USE_ITEM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GOD_PATH_USE_ITEM>(stream);
            Log.Write("player {0} request god path use item, heroId {1} itemId {2} count {3}", uid, msg.HeroId, msg.ItemId, msg.Count);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GodPathUseItem not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.GodPathUseItem(msg.HeroId, msg.ItemId, msg.Count);
        }

        public void OnResponse_GodPathTrainBodyBuyShield(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GOD_PATH_TRAIN_BODY_BUY msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GOD_PATH_TRAIN_BODY_BUY>(stream);
            Log.Write("player {0} request GodPathTrainBodyBuyShield, heroId {1}", uid, msg.HeroId);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GodPathTrainBodyBuyShield not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.GodPathBuyBodyTrainShield(msg.HeroId);
        }

        public void OnResponse_GodPathTrainBody(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GOD_PATH_TRAIN_BODY msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GOD_PATH_TRAIN_BODY>(stream);
            Log.Write("player {0} request GodPathTrainBody, heroId {1}", uid, msg.HeroId);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GodPathTrainBody not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.GodPathTrainBody(msg.HeroId);
        }

        public void OnResponse_GodPathFinishStageTask(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GOD_FINISH_STAGE_TASK msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GOD_FINISH_STAGE_TASK>(stream);
            Log.Write("player {0} request GodPathFinishStageTask, heroId {1}", uid, msg.HeroId);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GodPathFinishStageTask not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.GodPathFinishStageTasks(msg.HeroId);
        }

        //海洋之心：购买
        public void OnResponse_GodPathOceanHeartBuyCount(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GOD_PATH_BUY_OCEAN_HEART msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GOD_PATH_BUY_OCEAN_HEART>(stream);
            Log.Write("player {0} request GodPathOceanHeartBuyCount, heroId {1}", uid, msg.HeroId);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GodPathOceanHeartBuyCount not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.GodPathBuyOceanHeartCount(msg.HeroId);
        }

        //海洋之心：重塑
        public void OnResponse_GodPathOceanHeartRepaint(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GOD_PATH_REPAINT_OCEAN_HEART msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GOD_PATH_REPAINT_OCEAN_HEART>(stream);
            Log.Write("player {0} request GodPathOceanHeartRepaint, heroId {1}", uid, msg.HeroId);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GodPathOceanHeartRepaint not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.GodPathOceanHeartRepaint(msg.HeroId);
        }

        //海洋之心：翻牌
        public void OnResponse_GodPathOceanHeartDraw(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GOD_PATH_OCEAN_HEART_DRAW msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GOD_PATH_OCEAN_HEART_DRAW>(stream);
            Log.Write("player {0} request GodPathOceanHeartDraw, heroId {1} index {2}", uid, msg.HeroId, msg.Index);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GodPathOceanHeartDraw not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.GodPathOceanHeartDraw(msg.HeroId, msg.Index);
        }

        //三叉戟：购买
        public void OnResponse_GodPathTridentBuy(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GOD_PATH_BUY_TRIDENT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GOD_PATH_BUY_TRIDENT>(stream);
            Log.Write("player {0} request GodPathTridentBuy, heroId {1}", uid, msg.HeroId);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GodPathTridentBuy not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.GodPathBuyTridentCount(msg.HeroId);
        }

        //三叉戟：使用
        public void OnResponse_GodPathTridentUse(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GOD_PATH_USE_TRIDENT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GOD_PATH_USE_TRIDENT>(stream);
            Log.Write("player {0} request GodPathTridentUse, heroId {1} strategyBuy {2}", uid, msg.HeroId, msg.StrategyBuy);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GodPathTridentUse not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.GodPathBuyTridentUse(msg.HeroId, msg.StrategyBuy);
        }

        //三叉戟：结果
        public void OnResponse_GodPathTridentResult(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GOD_PATH_TRIDENT_RESULT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GOD_PATH_TRIDENT_RESULT>(stream);
            Log.Write("player {0} request GodPathTridentResult, heroId {1} randomBuy {2} success {3}", uid, msg.HeroId, msg.RandomBuy.ToString(), msg.IsSuccess.ToString());

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GodPathTridentResult not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.GodPathBuyTridentResult(msg.HeroId, msg.RandomBuy, msg.IsSuccess);
        }

        public void OnResponse_GodPathTridentPush(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GOD_PATH_PUSH_TRIDENT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GOD_PATH_PUSH_TRIDENT>(stream);
            Log.Write("player {0} request GodPathTridentPush, heroId {1}", uid, msg.HeroId);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GodPathTridentPush not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.GodPathBuyTridentPush(msg.HeroId);
        }

        public void OnResponse_GodPathAcrossOceanLightPuzzle(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GOD_PATH_LIGHT_PUZZLE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GOD_PATH_LIGHT_PUZZLE>(stream);
            Log.Write("player {0} request GodPathAcrossOceanLightPuzzle, heroId {1} puzzleIndex {2}", uid, msg.HeroId, msg.PuzzleIndex);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} GodPathAcrossOceanLightPuzzle not in gateid {1} pc list", uid, SubId);
                return;
            }

            player.GodPathAcrossOceanLightPuzzle(msg.HeroId, msg.PuzzleIndex);
        }

        public void OnResponse_GodPathAcrossOceanSweep(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GOD_PATH_ACROSS_OCEAN_SWEEP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GOD_PATH_ACROSS_OCEAN_SWEEP>(stream);
            Log.Write("player {0} request GodPathAcrossOceanSweep id {1}", uid, msg.Id);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.AcrossOceanSweep(msg.Id);
            }
            else
            {
                player = Api.PCManager.FindOfflinePc(uid);
                if (player != null)
                {
                    Log.WarnLine("GodPathAcrossOceanSweep fail, player {0} is offline.", uid);
                }
                else
                {
                    Log.WarnLine("GodPathAcrossOceanSweep fail, can not find player {0} .", uid);
                }
            }
        }
    }
}
