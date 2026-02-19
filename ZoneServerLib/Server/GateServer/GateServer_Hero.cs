using System.IO;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using ServerShared;

namespace ZoneServerLib
{
    public partial class GateServer
    {
        //武魂升级
        public void OnResponse_HeroLevelUp(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_HERO_LEVEL_UP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_HERO_LEVEL_UP>(stream);
            Log.Write("player {0} request to hero {1} level up", uid, msg.HeroId);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} hero {1} level up failed: no such player", uid, msg.HeroId);
                return;
            }
            player.HeroLevelUp(msg.HeroId);
        }

        // 武魂觉醒 
        public void OnResponse_HeroAwaken(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_HERO_AWAKEN msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_HERO_AWAKEN>(stream);
            Log.Write("player {0} request to hero {1} awaken", uid, msg.HeroId);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} hero {1} awaken failed: no such player", uid, msg.HeroId);
                return;
            }
            player.HeroAwaken(msg.HeroId);
        }

        //称号认证
        public void OnResponse_HeroTitleUp(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_HERO_TITLE_UP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_HERO_TITLE_UP>(stream);
            Log.Write("player {0} request to hero {1} title up", uid, msg.HeroId);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} hero {1} title up failed: no such player", uid, msg.HeroId);
                return;
            }
            player.HeroTitleUp(msg.HeroId);
        }

        // 天赋加点 
        public void OnResponse_HeroClickTalent(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_HERO_CLICK_TALENT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_HERO_CLICK_TALENT>(stream);
            Log.Write("player {0} request to hero {1} click talent Strength {2} Physical {3} Agility {4} Outburst {5}",
                uid, msg.HeroId, msg.Strength, msg.Physical, msg.Agility, msg.Outburst);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} hero {1} click talent failed: no such player", uid, msg.HeroId);
                return;
            }
            player.HeroClickTalent(msg.HeroId, msg.Strength, msg.Physical, msg.Agility, msg.Outburst);
        }

        // 重置天赋
        public void OnResponse_HeroResetTalent(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_HERO_RESET_TALENT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_HERO_RESET_TALENT>(stream);
            Log.Write("player {0} request to hero {1} reset talent ", uid, msg.HeroId);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} hero {1} reset talent failed: no such player", uid, msg.HeroId);
                return;
            }
            player.HeroResetTalent(msg.HeroId);
        }

        //伙伴出阵
        public void OnResponse_EquipHero(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_EQUIP_HERO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_EQUIP_HERO>(stream);
            Log.Write("player {0} request to equip hero {1}", uid, msg.HeroId);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} request to equip hero {1} failed: no such player", uid, msg.HeroId);
                return;
            }
            //player.EquipHero(msg.HeroId, msg.Equip);

        }

        //更改跟随的英雄，0为无
        public void OnResponse_ChangeFollower(MemoryStream stream,int uid = 0)
        {
            MSG_GateZ_HERO_CHANGE_FOLLOWER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_HERO_CHANGE_FOLLOWER>(stream);
            Log.Write("player {0} request to change follower hero {1}", uid, msg.HeroId);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} request to change follower hero {1} failed: no such player", uid, msg.HeroId);
                return;
            }

            MSG_ZGC_HERO_CHANGE_FOLLOWER response = new MSG_ZGC_HERO_CHANGE_FOLLOWER();
            response.Result = (int)ErrorCode.Success;
            response.HeroId = msg.HeroId;
            if (msg.HeroId >= 0)
            {
                player.ChangeFollower(msg.HeroId);
            }
            else
            {
                Log.Warn("player {0} request to change follower hero {1} failed: hero param error", uid, msg.HeroId);
                response.Result = (int)ErrorCode.NotAllowed;
            }

            player.Write(response);
        }

        public void OnResponse_ChangeMainHero(MemoryStream stream,int uid = 0)
        {
            MSG_GateZ_MAIN_HERO_CHANGE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_MAIN_HERO_CHANGE>(stream);
            Log.Write("player {0} request to change follower hero {1}", uid, msg.HeroId);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} request to change main hero {1} failed: no such player", uid, msg.HeroId);
                return;
            }

            player.ChangeMainHero(msg.HeroId);

            //MSG_ZGC_HERO_CHANGE_FOLLOWER response = new MSG_ZGC_HERO_CHANGE_FOLLOWER();
            //response.Result = (int)ErrorCode.Success;
            //response.HeroId = msg.HeroId;
            //if (msg.HeroId >= 0)
            //{
            //    player.ChangeFollower(msg.HeroId);
            //}
            //else
            //{
            //    response.Result = (int)ErrorCode.NotAllowed;
            //}
        }

        //召唤武魂
        public void OnResponse_CallHero(MemoryStream stream, int uid = 0)
        {
            //MSG_GateZ_CALL_HERO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_CALL_HERO>(stream);
            //Log.Write("player {0} request to call pet {1}", uid, msg.HeroId);
            //PlayerChar player = Api.PCManager.FindPc(uid);
            //if (player == null)
            //{
            //    Log.Warn("player {0} call hero {1} failed: no such player", uid, msg.HeroId);
            //    return;
            //}

            //MSG_ZGC_CALL_HERO response = new MSG_ZGC_CALL_HERO();
            //response.HeroId = msg.HeroId;

            //HeroInfo heroInfo = player.HeroMng.GetHeroInfo(msg.HeroId);
            //if (heroInfo == null)
            //{
            //    response.Result = (int)ErrorCode.NotExist;
            //    player.Write(response);
            //    return;
            //}

            //Hero hero = player.HeroMng.GetSummonedHero(msg.HeroId);
            //if (hero != null)
            //{
            //    response.Result = (int)ErrorCode.AlreadySummoned;
            //    player.Write(response);
            //    return;
            //}
            //response.Result = (int)ErrorCode.Success;
            //player.Write(response);
            //player.HeroMng.CallHero(msg.HeroId);
        }

        //收回武魂
        public void OnResponse_RecallHero(MemoryStream stream, int uid = 0)
        {
            //MSG_GateZ_RECALL_HERO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_RECALL_HERO>(stream);
            //Log.Write("player {0} request to recall hero {1}", uid, msg.HeroId);
            //PlayerChar player = Api.PCManager.FindPc(uid);
            //if (player == null)
            //{
            //    Log.Warn("player {0} request to recall hero {1} failed: no such player", uid, msg.HeroId);
            //    return;
            //}

            //MSG_ZGC_RECALL_HERO response = new MSG_ZGC_RECALL_HERO();
            //response.HeroId = msg.HeroId;

            //HeroInfo heroInfo = player.HeroMng.GetHeroInfo(msg.HeroId);
            //if (heroInfo == null)
            //{
            //    response.Result = (int)ErrorCode.NotExist;
            //    player.Write(response);
            //    return;
            //}
            //Hero hero = player.HeroMng.GetSummonedHero(msg.HeroId);
            //if (hero == null)
            //{
            //    response.Result = (int)ErrorCode.NotSummoned;
            //    player.Write(response);
            //    return;
            //}
            //response.Result = (int)ErrorCode.Success;
            //player.Write(response);
            //player.HeroMng.RecallHero(msg.HeroId, true);
        }

        //进阶
        public void OnResponse_HeroStepsUp(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_HERO_STEPS_UP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_HERO_STEPS_UP>(stream);
            Log.Write("player {0} request to steps up hero {1}", uid, msg.HeroId);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} request to steps up hero {1} failed: no such player", uid, msg.HeroId);
                return;
            }
            player.HeroStepsUp(msg.HeroId);
        }
        public void OnResponse_OnekeyHeroStepsUp(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_ONEKEY_HERO_STEPS_UP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_ONEKEY_HERO_STEPS_UP>(stream);
            Log.Write("player {0} request to steps up hero {1}", uid, msg.HeroIds.ToString());
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} request to steps up hero {1} failed: no such player", uid, msg.HeroIds.ToString());
                return;
            }
            player.OnekeyHeroStepsUp(msg.HeroIds);
        }
        //武魂返还
        public void OnResponse_HeroRevert(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_HERO_REVERT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_HERO_REVERT>(stream);
            Log.Write("player {0} request to hero {1} revet", uid, msg.HeroId);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} hero {1} revet failed: no such player", uid, msg.HeroId);
                return;
            }
            player.HeroRevert(msg.HeroId);
        }

        public void OnResponse_UpdateHeroPos(MemoryStream stream,int uid = 0)
        {
            MSG_GateZ_UPDATE_HERO_POS msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_UPDATE_HERO_POS>(stream);
            Log.Write("player {0} request to set hero Pos", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} set hero Pos failed: no such player", uid);
                return;
            }
            player.UpdateHeroPos(msg);
        }

        public void OnResponse_UpdateMainQueueHeroPos(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_UPDATE_MAINQUEUE_HEROPOS msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_UPDATE_MAINQUEUE_HEROPOS>(stream);
            Log.Write("player {0} request to update main queue hero Pos", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} update main queue hero Pos failed: no such player", uid);
                return;
            }
            player.UpdateMainBattleQueueHeroPos(msg);
        }

        public void OnResponse_UnlockMainBattleQueue(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_UNLOCK_MAINQUEUE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_UNLOCK_MAINQUEUE>(stream);
            Log.Write("player {0} request to unlock main battle queue", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} unlock main battle queue failed: no such player", uid);
                return;
            }
            player.UnlockMainBattleQueue(msg.QueueNum);
        }

        public void OnResponse_ChangeMainQueueName(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_CHANGE_MAINQUEUE_NAME msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_CHANGE_MAINQUEUE_NAME>(stream);
            Log.Write("player {0} request to change main queue name", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} change main queue name failed: no such player", uid);
                return;
            }
            if (string.IsNullOrEmpty(msg.Name))
            {
                MSG_ZGC_CHANGE_MAINQUEUE_NAME notify = new MSG_ZGC_CHANGE_MAINQUEUE_NAME();
                notify.Result = (int)ErrorCode.Fail;
                Write(notify, uid);
                return;
            }
            //检查字长
            int nameLen = Api.NameChecker.GetWordLen(msg.Name);
            if (nameLen > HeroLibrary.BattleQueueNameLength)
            {
                MSG_ZGC_CHANGE_MAINQUEUE_NAME notify = new MSG_ZGC_CHANGE_MAINQUEUE_NAME();
                notify.Result = (int)ErrorCode.LengthLimit;
                Write(notify, uid);
                return;
            }
            //检查屏蔽字
            if (Api.NameChecker.HasSpecialSymbol(msg.Name) || Api.NameChecker.HasBadWord(msg.Name))
            {
                MSG_ZGC_CHANGE_MAINQUEUE_NAME notify = new MSG_ZGC_CHANGE_MAINQUEUE_NAME();
                notify.Result = (int)ErrorCode.BadWord;
                Write(notify, uid);
                return;
            }
            player.ChangeMainBattleQueueName(msg.QueueNum, msg.Name);
        }

        public void OnResponse_MainQueueDispatchBattle(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_MAINQUEUE_DISPATCH_BATTLE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_MAINQUEUE_DISPATCH_BATTLE>(stream);
            Log.Write("player {0} request to main queue dispatch battle", uid);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} main queue dispatch battle failed: no such player", uid);
                return;
            }
            player.MainBattleQueueDispatchBattle(msg.QueueNum);
        }

        private void OnResponse_HeroInherit(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_HERO_INHERIT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_HERO_INHERIT>(stream);
            Log.Write("player {0} request hero inherit", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.HeroInherit(msg.FromHeroId, msg.ToHeroId);
            }
            else
            {
                Log.Warn($"OnResponse_HeroInherit find no player {uid}");
            }
        }

        public void OnResponse_HeroGodStepsUp(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_HERO_GOD_STEPS_UP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_HERO_GOD_STEPS_UP>(stream);
            Log.Write("player {0} request to steps up hero god {1}", uid, msg.HeroId);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} request to steps up hero {1} god failed: no such player", uid, msg.HeroId);
                return;
            }
            player.HeroGodStepsUp(msg.HeroId);
        }

    }
}
