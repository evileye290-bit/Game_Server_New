using System;
using System.Collections.Generic;
using ServerShared;
using Logger;
using System.IO;
using Message.IdGenerator;
using Message.Barrack.Protocol.BG;
using DBUtility;
using ServerFrame;
using Message.Corss.Protocol.CorssR;
using Message.Relation.Protocol.RC;
using Message.Relation.Protocol.RZ;

namespace RelationServerLib
{
    /// <summary>
    /// 服务器封装，保存了进程引用
    /// </summary>
    public partial class CrossServer : BackendServer
    {
        RelationServerApi Api
        { get { return (RelationServerApi)api; } }

        public CrossServer(BaseApi api)
            : base(api)
        {
        }

        protected override void BindResponser()
        {
            base.BindResponser();
            AddResponser(Id<MSG_CorssR_GET_BATTLE_RANK>.Value, OnResponse_GetCrossFinalsPlayerRank);
            AddResponser(Id<MSG_CorssR_GET_BATTLE_PLAYER>.Value, OnResponse_GetCrossBattlePlayerInfo);
            AddResponser(Id<MSG_CorssR_GET_SHOW_PLAYER>.Value, OnResponse_GetShowPlayer);
            AddResponser(Id<MSG_CorssR_GET_CHALLENGER>.Value, OnResponse_GetChallengerInfo);
            AddResponser(Id<MSG_CorssR_SHOW_CROSS_BATTLE_FINALS_INFO>.Value, OnResponse_ShowCrossBattleFinals);
            AddResponser(Id<MSG_RCR_CROSS_BATTLE_CHALLENGER>.Value, OnResponse_ShowCrossBattleChallenger);
            AddResponser(Id<MSG_CorssR_GET_BATTLE_HEROS>.Value, OnResponse_GetCrossBattleHeros);
            AddResponser(Id<MSG_CorssR_GET_CROSS_VIDEO>.Value, OnResponse_GetCrossBattleVedio);
            AddResponser(Id<MSG_CorssR_SEND_FINALS_REWARD>.Value, OnResponse_SendCrossBattleFinalsReward);        
            AddResponser(Id<MSG_CorssR_CLEAR_PLAYER_FINAL>.Value, OnResponse_ClearCrossFinalsPlayerRank);
            AddResponser(Id<MSG_CorssR_UPDATE_PLAYER_FINAL>.Value, OnResponse_UpdateCrossFinalsPlayerRank);
            AddResponser(Id<MSG_CorssR_CLEAR_BATTLE_RANK>.Value, OnResponse_ClearCrossBattleRanks);
            AddResponser(Id<MSG_CorssR_BATTLE_START>.Value, OnResponse_CrossBattleStart);
            AddResponser(Id<MSG_CorssR_NOTICE_PLAYER_BATTLE_INFO>.Value, OnResponse_NoticePlayerBattleInfo);
            AddResponser(Id<MSG_CorssR_CROSS_BATTLE_WIN_FINAL>.Value, OnResponse_NoticePlayerFirst);
            AddResponser(Id<MSG_CorssR_GET_BATTLE_START>.Value, OnResponse_GetCrossBattleStart);
            AddResponser(Id<MSG_CorssR_GET_GET_GUESSING_INFO>.Value, OnResponse_GetGuessingPlayersInfo);
            AddResponser(Id<MSG_CorssR_NOTICE_CROSS_GUESSING_INFO>.Value, OnResponse_CrossBattleGuessingStart);
            AddResponser(Id<MSG_CorssR_NOTICE_CROSS_GUESSING_RESULT>.Value, OnResponse_CrossBattleGuessingResult);
            AddResponser(Id<MSG_CorssR_NOTICE_PLAYER_TEAM_ID>.Value, OnResponse_NoticePlayerBattleTeamId);


            AddResponser(Id<MSG_RCR_RETURN_BATTLE_PLAYER_INFO>.Value, OnResponse_ReturnCrossBattlePlayerInfo);
            AddResponser(Id<Message.Zone.Protocol.ZR.MSG_ZRZ_RETURN_BATTLE_PLAYER_INFO>.Value, OnResponse_ReturnCrossBattlePlayerInfoNew);

            AddResponser(Id<Message.Gate.Protocol.GateC.MSG_ZRZ_RETURN_PLAYER_SHOW>.Value, OnResponse_ReturnShowPlayer);
            AddResponser(Id<Message.Gate.Protocol.GateC.MSG_ZRZ_RETURN_ARENA_CHALLENGER>.Value, OnResponse_ReturnChallengerInfo);

            AddResponser(Id<MSG_CrossR_CHAT_TRUMPET>.Value, OnResponse_ChatTrumpet);
            AddResponser(Id<MSG_CorssR_BROADCAST_ANNOUNCEMENT>.Value, OnResponse_CrossBroadcastAnnouncement);

            AddResponser(Id<MSG_CorssR_GET_HIDDER_WEAPON_VALUE>.Value, OnResponse_GetHidderWeaponInfo);
            AddResponser(Id<MSG_CorssR_GET_SEA_TREASURE_VALUE>.Value, OnResponse_GetSeaTreasureInfo);
            AddResponser(Id<MSG_CorssR_GET_DIVINE_LOVE_VALUE>.Value, OnResponse_GetDivineLoveInfo);
            AddResponser(Id<MSG_CorssR_GET_STONE_WALL_VALUE>.Value, OnResponse_GetStoneWallInfo);
            AddResponser(Id<MSG_CorssR_CLEAR_VALUE>.Value, OnResponse_ClearValue);

            AddResponser(Id<MSG_RZ_GET_RANK_LIST>.Value, OnResponse_NewGetRankList);
            AddResponser(Id<MSG_CorssR_GET_RANK_FIRST_INFO>.Value, OnResponse_GetRankFirstInfo);
            AddResponser(Id<MSG_CrossR_GET_ISLAND_HIGH_INFO>.Value, OnResponse_GetIslandHighInfo);
            AddResponser(Id<MSG_CorssR_GET_RANK_REWARD>.Value, OnResponse_GetRankReward);
            
            AddResponser(Id<MSG_CorssR_GET_CROSS_BOSS_INFO>.Value, OnResponse_GetCrossBossInfo);
            AddResponser(Id<Message.Zone.Protocol.ZR.MSG_ZRZ_GET_BOSS_PLAYER_INFO>.Value, OnResponse_GetCrossBossPlayerInfo);
            AddResponser(Id<Message.Zone.Protocol.ZR.MSG_ZRZ_RETURN_BOSS_PLAYER_INFO>.Value, OnResponse_ReturnCrossBossPlayerInfoFromCross);
            AddResponser(Id<MSG_CorssR_STOP_CROSS_BOSS_DUNGEON>.Value, OnResponse_StopCrossBossDungeon);
            AddResponser(Id<MSG_CorssR_CROSS_BOSS_PASS_REWARD>.Value, OnResponse_SendCrossBossPassReward);
            AddResponser(Id<MSG_CorssR_CROSS_BOSS_RANK_REWARD>.Value, OnResponse_SendCrossBossRankReward);
            AddResponser(Id<Message.Gate.Protocol.GateC.MSG_ZGC_CROSS_NOTES_LIST>.Value, OnResponse_ReturnNotesList);

            AddResponser(Id<MSG_CorssR_RECORD_RANK_ACTIVE_INFO>.Value, OnResponse_RecordRankActiveInfo);
            AddResponser(Id<MSG_CorssR_RECORD_RANK_INFO>.Value, OnResponse_RecordRankInfo);


            //Cross challenge
            AddResponser(Id<MSG_CorssR_CROSS_CHALLENGE_GET_BATTLE_RANK>.Value, OnResponse_GetCrossChallengeFinalsPlayerRank);
            AddResponser(Id<MSG_CorssR_CROSS_CHALLENGE_GET_BATTLE_PLAYER>.Value, OnResponse_GetCrossChallengePlayerInfo);
            //AddResponser(Id<MSG_CorssR_CROSS_CHALLENGE_GET_SHOW_PLAYER>.Value, OnResponse_GetShowPlayer);
            //AddResponser(Id<MSG_CorssR_GET_CHALLENGER>.Value, OnResponse_GetChallengerInfo);
            AddResponser(Id<MSG_CorssR_SHOW_CROSS_CHALLENGE_FINALS_INFO>.Value, OnResponse_ShowCrossChallengeFinals);
            AddResponser(Id<MSG_RCR_CROSS_CHALLENGE_CHALLENGER>.Value, OnResponse_ShowCrossChallengeChallenger);
            AddResponser(Id<MSG_CorssR_CROSS_CHALLENGE_GET_BATTLE_HEROS>.Value, OnResponse_GetCrossChallengeHeros);
            AddResponser(Id<MSG_CorssR_CROSS_CHALLENGE_GET_CROSS_VIDEO>.Value, OnResponse_GetCrossChallengeVedio);
            AddResponser(Id<MSG_CorssR_CROSS_CHALLENGE_SEND_FINALS_REWARD>.Value, OnResponse_SendCrossChallengeFinalsReward);
            AddResponser(Id<MSG_CorssR_CROSS_CHALLENGE_CLEAR_PLAYER_FINAL>.Value, OnResponse_ClearCrossChallengeFinalsPlayerRank);
            AddResponser(Id<MSG_CorssR_CROSS_CHALLENGE_UPDATE_PLAYER_FINAL>.Value, OnResponse_UpdateCrossChallengeFinalsPlayerRank);
            AddResponser(Id<MSG_CorssR_CROSS_CHALLENGE_CLEAR_BATTLE_RANK>.Value, OnResponse_ClearCrossChallengeRanks);
            AddResponser(Id<MSG_CorssR_CROSS_CHALLENGE_BATTLE_START>.Value, OnResponse_CrossChallengeStart);
            AddResponser(Id<MSG_CorssR_CROSS_CHALLENGE_NOTICE_PLAYER_BATTLE_INFO>.Value, OnResponse_CrossChallengeNoticePlayerBattleInfo);
            AddResponser(Id<MSG_CorssR_CROSS_CHALLENGE_WIN_FINAL>.Value, OnResponse_CrossChallengeNoticePlayerFirst);
            AddResponser(Id<MSG_CorssR_CROSS_CHALLENGE_GET_BATTLE_START>.Value, OnResponse_GetCrossChallengeStart);
            AddResponser(Id<MSG_CorssR_CROSS_CHALLENGE_GET_GUESSING_INFO>.Value, OnResponse_OnResponse_CrossChallengeGetGuessingPlayersInfo);
            AddResponser(Id<MSG_CorssR_NOTICE_CROSS_CHALLENGE_GUESSING_INFO>.Value, OnResponse_CrossChallengeGuessingStart);
            AddResponser(Id<MSG_CorssR_NOTICE_CROSS_CHALLENGE_GUESSING_RESULT>.Value, OnResponse_CrossChallengeGuessingResult);
            AddResponser(Id<MSG_CorssR_CROSS_CHALLENGE_NOTICE_PLAYER_TEAM_ID>.Value, OnResponse_CrossChallengeNoticePlayerBattleTeamId);
            AddResponser(Id<Message.Zone.Protocol.ZR.MSG_ZRZ_RETURN_CROSS_CHALLENGE_BATTLE_PLAYER_INFO>.Value, OnResponse_ReturnCrossChallengePlayerInfoNew);
            AddResponser(Id<MSG_RCR_RETURN_CROSS_CHALLENGE_PLAYER_INFO>.Value, OnResponse_ReturnCrossChallengePlayerInfo);


            //ResponseEnd
        }

        public void OnResponse_CrossBroadcastAnnouncement(MemoryStream stream, int uid = 0)
        {
            MSG_CorssR_BROADCAST_ANNOUNCEMENT pks = MessagePacker.ProtobufHelper.Deserialize<MSG_CorssR_BROADCAST_ANNOUNCEMENT>(stream);

            MSG_RZ_BROADCAST_ANNOUNCEMENT msg = new MSG_RZ_BROADCAST_ANNOUNCEMENT();
            msg.Type = pks.Type;
            msg.List.AddRange(pks.List);
            FrontendServer server = Api.ZoneManager.GetOneServer();
            if (server != null)
            {
                server.Write(msg);
            }
        }
    }
}