using Message.Gate.Protocol.GateC;
using Logger;
using Message.Relation.Protocol.RZ;
using EnumerateUtility;
using System.IO;
using RedisUtility;

namespace ZoneServerLib
{
    public partial class RelationServer
    {
        private void OnResponse_FriendHeartGive(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_FRIEND_HEART_GIVE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_FRIEND_HEART_GIVE>(stream);
            Log.Write("player {0} mainId {1} give friend heart to player {2}", msg.GiveHeartUid, MainId, uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.TakeHeart(msg.GiveHeartUid); 
            }
            else
            {
                Log.Error("player {0} mainId {1} give friend heart to player {2} fail,can not find player {2}",msg.GiveHeartUid, MainId,uid);
            }
        }

        private void OnResponse_FriendInvite(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_FRIEND_INVITE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_FRIEND_INVITE>(stream);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.AddFriendInviteList(msg.InviterUid);
            }
            else
            {
                Log.Error("player {0} mainId {1} friend invite to player {2} fail,can not find player {3}", msg.InviterUid, MainId, uid, uid);
            }
        }

        private void OnResponse_FriendResponse(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_FRIEND_RESPONSE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_FRIEND_RESPONSE>(stream);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.NotiyClientAdd2FriendList(uid, msg.FriendUid, msg.Agree);
                if (msg.Agree)
                {
                    player.Add2FriendList(msg.FriendUid);
                }
            }
            else
            {
                Log.Error("player {0} mainId {1} OnResponse_FriendResponse player {2} fail,can not find player", msg.FriendUid, MainId, uid);
            }
        }

        private void OnResponse_FriendRemove(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_FRIEND_REMOVE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_FRIEND_REMOVE>(stream);
            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player != null)
            {
                player.RemoveFromFriendList(msg.FriendUid);
            }
            else
            {
                Log.Error("player {0} mainId {1} remove friend {2} fail,can not find player {3}", uid, MainId, msg.FriendUid, uid);
            }

        }

        private void OnResponse_ChallengePlayerRequst(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_CHALLENGE_PLAYER_REQUEST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_CHALLENGE_PLAYER_REQUEST>(stream);
            Log.Debug("OnResponse_ChallengePlayerRequst");
            //int attackerUid = msg.Attacker.Uid;
            //int defenderUid = msg.DefenderUid;

            ////找到被邀请者
            //PlayerChar defender = Api.PCManager.FindPc(defenderUid);
            //if (defender != null)
            //{

            //    if (defender.CheckBlackExist(attackerUid))
            //    {
            //        //在黑名单中
            //        Log.WarnLine("player {0} challenge request player {1} failed: in target black.", attackerUid, defenderUid);
            //        defender.SendChallengeRequestError(attackerUid, defenderUid, ErrorCode.InTargetBlack, false);
            //        return;
            //    }

            //    if (!defender.CheckFriendExist(attackerUid))
            //    {
            //        //不在好友列表中
            //        Log.WarnLine("player {0} challenge request player {1} failed: not int player {2} friend list.", attackerUid, defenderUid, defenderUid);
            //        defender.SendChallengeRequestError(attackerUid, defenderUid, ErrorCode.NotHisFriend, false);
            //        return;
            //    }

            //    if (defender.OutState != (int)OutsideState.Normal)
            //    {
            //        //状态错误
            //        Log.WarnLine("player {0} challenge request player {1} failed: outside state is {2}.", attackerUid, defenderUid, defender.OutState);
            //        defender.SendChallengeRequestError(attackerUid, defenderUid, ErrorCode.TargetIsFighting, false);
            //        return;
            //    }

            //    if (!defender.CheckLimitOpen(LimitType.Challenge))
            //    {
            //        Log.WarnLine("player {0} challenge request player {1} failed: not open {2}.", attackerUid, defenderUid, defender.OutState);
            //        defender.SendChallengeRequestError(attackerUid, defenderUid, ErrorCode.NotOpen, false);
            //        return;
            //    }

            //    //通知被邀请者
            //    MSG_ZGC_CHALLENGE_PLAYER_REQUEST response = new MSG_ZGC_CHALLENGE_PLAYER_REQUEST();
            //    response.Attacker = PlayerInfo.GetPlayerBaseInfo(msg.Attacker);
            //    response.Result = (int)ErrorCode.Success;
            //    defender.Write(response);

            //    //私聊信息，已经不需要了
            //    //SendChallengeChatWords(player, defender);

            }
        }



        //private void OnResponse_ChallengePlayerRequst_Return(MemoryStream stream, int uid = 0)
        //{
        //    MSG_RZ_CHALLENGE_PLAYER_REQUEST_RETURN msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_CHALLENGE_PLAYER_REQUEST_RETURN>(stream);

        //    MSG_ZGC_CHALLENGE_PLAYER_REQUEST response = new MSG_ZGC_CHALLENGE_PLAYER_REQUEST();
        //    response.AttackerUid = msg.AttackerUid;
        //    PlayerChar player = server.PCManager.FindPc(msg.AttackerUid);
        //    if (player != null)
        //    {
        //        response.Result = msg.Result;
        //        //if (response.Result == (int)ErrorCode.Success)
        //        //{

        //        //}
        //        //else
        //        //{
        //        //    player.DelChallengeWaiting();
        //        //}
        //        player.Write(response);
        //    }
        //}

        //private void OnResponse_ChallengePlayerCancel(MemoryStream stream, int uid = 0)
        //{
        //    MSG_RZ_CHALLENGE_PLAYER_CANCEL msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_CHALLENGE_PLAYER_CANCEL>(stream);

        //    MSG_ZGC_CHALLENGE_PLAYER_CANCEL response = new MSG_ZGC_CHALLENGE_PLAYER_CANCEL();
        //    response.AttackerUid = msg.AttackerUid;
        //    PlayerChar player = server.PCManager.FindPc(msg.DefenderUid);
        //    if (player != null)
        //    {
        //        //if (player.GetChallengeWaiting().AttackerUid == msg.AttackerUid)
        //        //{
        //        //    player.DelChallengeWaiting();
        //            player.Write(response);
        //        //}
        //    }
        //}

        //private void OnResponse_ChallengePlayerResponse(MemoryStream stream, int uid = 0)
        //{
        //    MSG_RZ_CHALLENGE_PLAYER_RESPONSE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_CHALLENGE_PLAYER_RESPONSE>(stream);

        //    MSG_ZGC_CHALLENGE_PLAYER_RESPONSE response = new MSG_ZGC_CHALLENGE_PLAYER_RESPONSE();
        //    response.Agree = msg.Agree;
        //    response.DefenderUid = msg.DefenderUid;
        //    response.DefenderName = msg.DefenderName;


        //    MSG_ZBM_FIND_CHALLENGE_PLAYER errorMsg = new MSG_ZBM_FIND_CHALLENGE_PLAYER();
        //    errorMsg.Defender = new MSG_ZBM_BATTLE_PLAYER_INFO();
        //    errorMsg.Defender.Uid = msg.DefenderUid;
        //    errorMsg.AttackerUid = msg.AttackerUid;
        //    errorMsg.IsAgree = msg.Agree;

        //    PlayerChar player = server.PCManager.FindPc(msg.AttackerUid);
        //    if (player != null)
        //    {
        //        //if (!msg.Agree)
        //        //{
        //        //    player.DelChallengeWaiting();
        //        //}
        //        if (player.IsFighting)
        //        {
        //            Log.WarnLine("player {0}  response player {1} challenge failed: is fighting.", msg.DefenderUid, msg.AttackerUid);
        //            //response.Result = (int)ErrorCode.TargetIsFighting;
        //            //player.Write(response);
        //            //player.SendErrorCodeMsg(ErrorCode.TargetIsFighting);
        //            return;
        //        }

        //        if (player.IsMatching)
        //        {
        //            Log.WarnLine("player {0}  response  player {1} challenge failed: is mactching.", msg.DefenderUid, msg.AttackerUid);
        //            //response.Result = (int)ErrorCode.TargetIsMatching;
        //            //player.Write(response);
        //            //player.SendErrorCodeMsg(ErrorCode.TargetIsMatching);
        //            return;
        //        }
        //        response.Result =(int) ErrorCode.Success;
        //        player.Write(response);

        //        if (msg.Agree)
        //        {
        //            player.ChangeBattleMatchingState(true, BattleType.Challenge);
        //            //player.DelChallengeWaiting();
        //            //开始匹配
        //            //MSG_ZBM_CHALLENGE_PLAYER msg2bm = new MSG_ZBM_CHALLENGE_PLAYER();
        //            //msg2bm.DefenderUid = msg.DefenderUid;
        //            //msg2bm.Attacker = player.GetBattlePlayerInfo(BattleType.Challenge);
        //            //if (msg2bm.Attacker != null)
        //            //{
        //            //    server.BMServer.Write(msg2bm);
        //            //}
        //        }
        //    }
        //    else
        //    {
        //        errorMsg.ErrorCode = (int)ErrorCode.TargetNotFind;
        //        server.BMServer.Write(errorMsg);
        //    }
        //}
    
}
