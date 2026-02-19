using EnumerateUtility;
using Logger;
using Message.Corss.Protocol.CorssR;
using Message.IdGenerator;
using Message.Relation.Protocol.RC;
using ServerFrame;
using ServerModels;
using ServerShared;
using System.IO;

namespace CrossServerLib
{
    /// <summary>
    /// 服务器封装，保存了进程引用
    /// </summary>
    public partial class RelationServer : FrontendServer
    {
        private RelationServerManager RelationManager
        { get { return (RelationServerManager)serverManager; } }

        public CrossServerApi Api
        { get { return (CrossServerApi)api; } }

        public RelationServer(BaseApi api)
            : base(api)
        {
        }

        protected override void BindResponser()
        {
            base.BindResponser();
            AddResponser(Id<MSG_RC_GET_FINALS_PLAYER_LIST>.Value, OnResponse_GetFinalsPlayerInfo);
            AddResponser(Id<Message.Zone.Protocol.ZR.MSG_ZRZ_RETURN_BATTLE_PLAYER_INFO>.Value, OnResponse_ReturnCrossBattlePlayerInfo);
            AddResponser(Id<MSG_RC_SET_BATTLE_RESULT>.Value, OnResponse_SetCrossBattleResult);
            AddResponser(Id<MSG_RC_SHOW_CROSS_BATTLE_FINALS>.Value, OnResponse_ShowCrossBattleFinals);
            AddResponser(Id<MSG_RC_SHOW_CROSS_BATTLE_CHALLENGER>.Value, OnResponse_ShowCrossBattleChallenger);
            AddResponser(Id<MSG_RC_CROSS_BATTLE_CHALLENGER_HERO_INFO>.Value, OnResponse_UpdateCrossBattleHeroInfo);
            AddResponser(Id<MSG_RC_GET_CROSS_VIDEO>.Value, OnResponse_GetCrossBattleVedio);
            AddResponser(Id<MSG_RC_GET_CROSS_BATTLE_START>.Value, OnResponse_GetCrossBattleStartTime);
            AddResponser(Id<MSG_RC_GET_GET_GUESSING_INFO>.Value, OnResponse_GetGuessingPlayersInfo);

            AddResponser(Id<MSG_RC_GET_SHOW_PLAYER>.Value, OnResponse_GetShowPlayer);
            AddResponser(Id<Message.Gate.Protocol.GateC.MSG_ZRZ_RETURN_PLAYER_SHOW>.Value, OnResponse_ReturnShowPlayer);
            AddResponser(Id<Message.Gate.Protocol.GateC.MSG_ZR_ADD_PLAYER_SHOW>.Value, OnResponse_AddShowPlayer);

            AddResponser(Id<MSG_RC_GET_CHALLENGER>.Value, OnResponse_GetChallengerInfo);
            AddResponser(Id<Message.Gate.Protocol.GateC.MSG_ZRZ_RETURN_ARENA_CHALLENGER>.Value, OnResponse_ReturnChallenger);
            AddResponser(Id<Message.Gate.Protocol.GateC.MSG_ZR_ADD_ARENA_CHALLENGER_HERO_INFO>.Value, OnResponse_AddChallengerInfo);

            AddResponser(Id<MSG_RC_CHAT_TRUMPET>.Value, OnResponse_ChatTrumpet);
            AddResponser(Id<MSG_RC_BROADCAST_ANNOUNCEMENT>.Value, OnResponse_CrossBroadcastAnnouncement);
            AddResponser(Id<MSG_RC_CROSS_NOTES_LIST>.Value, OnResponse_CrossNotesList);
            AddResponser(Id<MSG_RC_NOTES_LIST_BY_TYPE>.Value, OnResponse_GetNotesListByType);

            AddResponser(Id<MSG_RC_GET_HIDDER_WEAPON_VALUE>.Value, OnResponse_GetHidderWeaponInfo);
            AddResponser(Id<MSG_RC_UPDATE_HIDDER_WEAPON_VALUE>.Value, OnResponse_UpdateHidderWeaponInfo);
            AddResponser(Id<MSG_RC_GET_SEA_TREASURE_VALUE>.Value, OnResponse_GetSeaTreasureInfo);
            AddResponser(Id<MSG_RC_UPDATE_SEA_TREASURE_VALUE>.Value, OnResponse_UpdateSeaTreasureInfo);
            AddResponser(Id<MSG_RC_GET_DIVINE_LOVE_VALUE>.Value, OnResponse_GetDivineLoveInfo);
            AddResponser(Id<MSG_RC_UPDATE_DIVINE_LOVE_VALUE>.Value, OnResponse_UpdateDivineLoveInfo);
            AddResponser(Id<MSG_RC_GET_STONE_WALL_VALUE>.Value, OnResponse_GetStoneWallInfo);
            AddResponser(Id<MSG_RC_UPDATE_STONE_WALL_VALUE>.Value, OnResponse_UpdateStoneWallInfo);

            //rank
            AddResponser(Id<MSG_RC_GET_RANK_LIST>.Value, OnResponse_GetRankList);
            AddResponser(Id<MSG_RC_GET_RANK_FIRST_INFO>.Value, OnResponse_GetRankFirstInfo);
            AddResponser(Id<MSG_RC_UPDATE_RANK_VALUE>.Value, OnResponse_UpdateRankValue);
            AddResponser(Id<MSG_RC_GET_RANK_REWARD>.Value, OnResponse_GetCrossRankReward);

            //cross boss 
            AddResponser(Id<MSG_RC_GET_CROSS_BOSS_INFO>.Value, OnResponse_GetCrossBossInfo);
            AddResponser(Id<MSG_RC_ENTER_CROSS_BOSS_MAP>.Value, OnResponse_StartChallengeCrossBoss);
            AddResponser(Id<Message.Zone.Protocol.ZR.MSG_ZRZ_RETURN_BOSS_PLAYER_INFO>.Value, OnResponse_ReturnCrossBossPlayerInfo);
            AddResponser(Id<MSG_RC_CROSS_BOSS_CHALLENGER>.Value, OnResponse_GetCrossBossChallenger);
            AddResponser(Id<MSG_RC_CHALLENGE_CROSS_BOSS_MAP>.Value, OnResponse_ChallengeCrossBoss);
            AddResponser(Id<MSG_RC_CHANGE_CROSS_BOSS_SCORE>.Value, OnResponse_AddCrossBossScore);

            //海岛登高 
            AddResponser(Id<MSG_RC_GET_ISLAND_HIGH_INFO>.Value, OnResponse_GetIslandHighInfo);

            //Cross Challenge
            AddResponser(Id<MSG_RC_CROSS_CHALLENGE_GET_FINALS_PLAYER_LIST>.Value, OnResponse_GetCrossChallengeFinalsPlayerInfo);
            AddResponser(Id<Message.Zone.Protocol.ZR.MSG_ZRZ_RETURN_CROSS_CHALLENGE_BATTLE_PLAYER_INFO>.Value, OnResponse_ReturnCrossChallengePlayerInfo);
            AddResponser(Id<MSG_RC_SET_CROSS_CHALLENGE_RESULT>.Value, OnResponse_SetCrossChallengeResult);
            AddResponser(Id<MSG_RC_SHOW_CROSS_CHALLENGE_FINALS>.Value, OnResponse_ShowCrossChallengeFinals);
            AddResponser(Id<MSG_RC_SHOW_CROSS_CHALLENGE_CHALLENGER>.Value, OnResponse_ShowCrossChallengeChallenger);
            AddResponser(Id<MSG_RC_CROSS_CHALLENGE_CHALLENGER_HERO_INFO>.Value, OnResponse_UpdateCrossChallengeHeroInfo);
            AddResponser(Id<MSG_RC_GET_CROSS_CHALLENGE_VIDEO>.Value, OnResponse_GetCrossChallengeVideo);
            AddResponser(Id<MSG_RC_GET_CROSS_CHALLENGE_START>.Value, OnResponse_GetCrossChallengeStartTime);
            AddResponser(Id<MSG_RC_GET_CROSS_CHALLENGE_GUESSING_INFO>.Value, OnResponse_GetCrossChallengeGuessingPlayersInfo);

            //ResponserEnd
        }

        public void OnResponse_CrossBroadcastAnnouncement(MemoryStream stream, int uid = 0)
        {
            MSG_RC_BROADCAST_ANNOUNCEMENT pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RC_BROADCAST_ANNOUNCEMENT>(stream);
            Log.Write($"player {uid} get CrossBroadcastAnnouncement from main {MainId} info ");
            //MSG_CorssR_BROADCAST_ANNOUNCEMENT msg = new MSG_CorssR_BROADCAST_ANNOUNCEMENT();
            //msg.Type = pks.Type;
            //msg.List.AddRange(pks.List);
            Api.RelationManager.BroadcastAnnouncement(pks.Type, MainId, pks.List);
        }


        public void OnResponse_CrossNotesList(MemoryStream stream, int uid = 0)
        {
            MSG_RC_CROSS_NOTES_LIST pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RC_CROSS_NOTES_LIST>(stream);
            Log.Write($"player {uid} get CrossNotesList from main {MainId} info " );
            int groupId = CrossBattleLibrary.GetGroupId(MainId);
            if (groupId == 0)
            {
                Log.Warn($"player {uid} CrossNotesList from main {MainId} not find group id ");
                return;
            }
            NotesType type = (NotesType)pks.Type;

            foreach (var item in pks.List)
            {
                NotesModel model = new NotesModel();
                model.Time = item.Time;
                model.List.AddRange(item.List);
                Api.NotesMng.AddNotesInfo(groupId, type, model);
            }
        }

        public void OnResponse_GetNotesListByType(MemoryStream stream, int uid = 0)
        {
            MSG_RC_NOTES_LIST_BY_TYPE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RC_NOTES_LIST_BY_TYPE>(stream);
            Log.Write($"player {uid} GetNotesListByType from main {MainId} info " + pks.Type);
            int groupId = CrossBattleLibrary.GetGroupId(MainId);
            if (groupId == 0)
            {
                Log.Warn($"player {uid} GetNotesListByType from main {MainId} not find group id ");
                return;
            }
            NotesType type = (NotesType)pks.Type;
            Api.NotesMng.PushRankListMsg(groupId, type, uid, MainId);
        }
    }
}