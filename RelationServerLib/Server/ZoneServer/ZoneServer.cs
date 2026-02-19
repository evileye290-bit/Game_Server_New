using System;
using System.Collections.Generic;
using System.IO;
using Engine;
using Message.IdGenerator;
using SocketShared;
using Logger;
using ServerShared;
using Message.Relation.Protocol.RZ;
using EnumerateUtility;
using DBUtility;
using Message.Zone.Protocol.ZR;
using Message.Relation.Protocol.RR;
using ServerFrame;
using Message.Shared.Protocol.Shared;

namespace RelationServerLib
{
    public partial class ZoneServer : FrontendServer
    {

        private ZoneServerManager ZoneManager
        { get { return (ZoneServerManager)serverManager; } }

        private RelationServerApi Api
        { get { return (RelationServerApi)api; } }

        public ZoneServer(BaseApi api)
            : base(api)
        {
        }

        protected override void BindResponser()
        {
            base.BindResponser();
            AddResponser(Id<MSG_ZR_REGIST_CLIENT_INFO_LIST>.Value, OnResponse_RegistClientInfoList);

            AddResponser(Id<MSG_ZR_CLIENT_ENTER>.Value, OnResponse_ClientEnter);
            AddResponser(Id<MSG_ZR_CLIENT_LEAVE_ZONE>.Value, OnResponse_ClientLeaveZone);
            AddResponser(Id<MSG_ZR_LEVEL_UP>.Value, OnResponse_ClientLevelUp);
            AddResponser(Id<MSG_ZR_UPDATE_CHAPTERID>.Value, OnResponse_ChapterIdChange);

            // Friend
            AddResponser(Id<MSG_ZR_FRIEND_HEART_GIVE>.Value, OnResponse_FriendHeartGive);

            // Team
            AddResponser(Id<MSG_ZR_TEAM_TYPE_LIST>.Value, OnResponse_TeamTypeList);
            AddResponser(Id<MSG_ZR_CREATE_TEAM>.Value, OnResponse_CreateTeam);
            AddResponser(Id<MSG_ZR_JOIN_TEAM>.Value, OnResponse_JoinTeam);
            AddResponser(Id<MSG_ZR_QUIT_TEAM>.Value, OnResponse_QuitTeam);
            AddResponser(Id<MSG_ZR_KICK_TEAM_MEMBER>.Value, OnResponse_KickTeamMember);
            AddResponser(Id<MSG_ZR_TRANDSFER_CAPTAIN>.Value, OnResponse_TransferCaptain);
            AddResponser(Id<MSG_ZR_ASK_JOIN_TEAM>.Value, OnResponse_AskJoinTeam);
            AddResponser(Id<MSG_ZR_INVITE_JOIN_TEAM>.Value, OnResponse_InviteJoinTeam);
            AddResponser(Id<MSG_ZR_ANSWER_INVITE_JOIN_TEAM>.Value, OnResponse_AnswerInviterJoinTeam);
            AddResponser(Id<MSG_ZR_ASK_FOLLOW_CAPTAIN>.Value, OnResponse_AskFollowCaptain);
            AddResponser(Id<MSG_ZR_TRY_ASK_FOLLOW_CAPTAIN>.Value, OnResponse_TryAskFollowCaptain);
            AddResponser(Id<MSG_ZR_ANSWER_FOLLOW_CAPTAIN>.Value, OnResponse_AnswerFollowCaptain);
            AddResponser(Id<MSG_ZR_TRY_ANSWER_FOLLOW_CAPTAIN>.Value, OnResponse_TRYAnswerFollowCaptain);
            AddResponser(Id<MSG_ZR_CHANGE_TEAM_TYPE>.Value, OnResponse_ChangeTeamType);
            AddResponser(Id<MSG_ZR_NEW_TEAM_DUNGEON>.Value, OnResponse_NewTeamDunegon);
            AddResponser(Id<MSG_ZR_TEAM_QUIT_DUNGEON>.Value, OnResponse_TeamQuitDunegon);
            AddResponser(Id<MSG_ZR_NEED_TEAM_HELP>.Value, OnResponse_NeedTeamHelp); 
            AddResponser(Id<MSG_ZR_RESPONSE_TEAM_HELP>.Value, OnResponse_ResponseTeamHelp);

            AddResponser(Id<MSG_ZR_PLAYER_ENTER_DUNGEON>.Value, OnResponse_EnterDungeon);
            AddResponser(Id<MSG_ZR_PLAYER_LEAVE_DUNGEON>.Value, OnResponse_LeaveDungeon);
            AddResponser(Id<MSG_ZR_QUIT_TEAM_ROBOT>.Value, OnResponse_QuitTeamRobot);
            AddResponser(Id<MSG_ZR_PLAYER_DETAIL_INFO>.Value, OnResponse_PlayerDetailInfo);
            AddResponser(Id<MSG_ZR_INVITE_FRIEND_JOIN_TEAM>.Value, OnResponse_InviteFriendJoinTeam);
            AddResponser(Id<MSG_ZR_TRANSFORM_DONE>.Value, OnResponse_TransferDone); 
            AddResponser(Id<MSG_ZR_NOTIFY_TEAM_CONT_HUNTING>.Value, OnResponse_NotifyTeamContinueHunting);         
            AddResponser(Id<MSG_ZR_HUNTING_HELP>.Value, OnResponse_HuntingHelpAsk);
            AddResponser(Id<MSG_ZR_HUNTING_HELP_ANSWER>.Value, OnResponse_HuntingHelpAnswer);

            //Family
            AddResponser(Id<MSG_ZR_FAMILY_INFO>.Value, OnResponse_FamilyInfo);
            AddResponser(Id<MSG_ZR_SEARCH_FAMILY>.Value, OnResponse_SearchFamily);
            AddResponser(Id<MSG_ZR_JOIN_FAMILY>.Value, OnResponse_JoinFamily);
            AddResponser(Id<MSG_ZR_CREATE_FAMILY>.Value, OnResponse_CreateFamily);
            AddResponser(Id<MSG_ZR_FAMILY_APPLY_LIST>.Value, OnResponse_FamilyApplyList);
            AddResponser(Id<MSG_ZR_FAMILY_APPLICATION_AGREE>.Value, OnResponse_FamilyApplicationAgree);
            AddResponser(Id<MSG_ZR_ASSIGN_FAMILY_TITLE>.Value, OnResponse_AssignFamilyTitle);
            AddResponser(Id<MSG_ZR_QUIT_FAMILY>.Value, OnResponse_QuitFamily);
            AddResponser(Id<MSG_ZR_KICK_FAMILY_MEMBER>.Value, OnResponse_KickFamilyMember);
            AddResponser(Id<MSG_ZR_UPDATE_FAMILY_CONTRIBUTION>.Value, OnResponse_UpdateFamilyContribution);
            AddResponser(Id<MSG_ZR_FAMILY_CONTENT_EDIT>.Value, OnResponse_FamilyContentEdit);
            
            AddResponser(Id<MSG_ZR_UPDATE_EMAI>.Value, OnResponse_UpdateEmail);
            AddResponser(Id<MSG_ZR_DELETE_ALL_EMAI>.Value, OnResponse_DeleteAllEmail);
            AddResponser(Id<MSG_ZR_SEND_EMAIL>.Value, OnResponse_SendEmail);

            //CHAT
            AddResponser(Id<MSG_ZR_ERROR_CODE>.Value, OnResponse_ErrorCode);
            AddResponser(Id<MSG_ZR_CHAT_LIST>.Value, OnResponse_ChatList);
            AddResponser(Id<MSG_ZR_CHAT_TRUMPET>.Value, OnResponse_ChatTrumpet);

            //Camp
            AddResponser(Id<MSG_ZR_GET_WEAK_CAMP>.Value, OnResponse_GetWeakCamp);
            AddResponser(Id<MSG_ZR_CHANGE_CAMP>.Value, OnResponse_ChangeCamp);
            AddResponser(Id<MSG_ZR_GET_CAMP_RANK>.Value, OnResponse_GetCampRank);
            AddResponser(Id<MSG_ZR_GET_CAMP_PANEL_INFO>.Value, OnResponse_GetCampPanel);
            AddResponser(Id<MSG_ZR_GET_CAMP_ELECTION>.Value, OnResponse_GetElectionRank);
            AddResponser(Id<MSG_ZR_UPDATE_ELECTION_RANK>.Value, OnResponse_TryUpdateElectionRank);
            AddResponser(Id<MSG_ZR_ASK_RANK_PERIOD>.Value, OnResponse_AskForRankPeriod);

            //Guild
            AddResponser(Id<MSG_ZR_MAX_GUILDID>.Value, OnResponse_MaxGuildId);


            //Show
            AddResponser(Id<Message.Gate.Protocol.GateC.MSG_ZRZ_RETURN_PLAYER_SHOW>.Value, OnResponse_ReturnShowPlayer);
            AddResponser(Id<Message.Gate.Protocol.GateC.MSG_ZR_ADD_PLAYER_SHOW>.Value, OnResponse_AddShowPlayer);

            AddResponser(Id<MSG_ZR_GET_SHOW_PLAYER>.Value, OnResponse_GetShowPlayer);
            AddResponser(Id<MSG_ZR_GET_CROSS_SHOW_PLAYER>.Value, OnResponse_GetCrossShowPlayer);

            //Arena
            AddResponser(Id<Message.Gate.Protocol.GateC.MSG_ZR_ADD_ARENA_CHALLENGER_HERO_INFO>.Value, OnResponse_AddChallengerInfo);
            AddResponser(Id<Message.Gate.Protocol.GateC.MSG_ZRZ_RETURN_ARENA_CHALLENGER>.Value, OnResponse_ReturnChallengerInfo);
            //AddResponser(Id<Message.Gate.Protocol.GateC.MSG_ZRZ_RETURN_CROSS_CHALLENGER>.Value, OnResponse_ReturnCrossChallenger);

            AddResponser(Id<MSG_ZR_GET_ARENA_CHALLENGERS>.Value, OnResponse_GeArenaChallenger);
            AddResponser(Id<MSG_ZR_SHOW_ARENA_RANK_INFO>.Value, OnResponse_ShowArenaRankInfo);
            AddResponser(Id<MSG_ZR_GET_ARENA_CHALLENGER>.Value, OnResponse_GetChallengerInfo);
            AddResponser(Id<MSG_ZR_CHALLENGE_WIN_CHANGE_RANK>.Value, OnResponse_ChallengeWinChangeRank);
            AddResponser(Id<MSG_ZR_ARENA_DAILY_REWARD>.Value, OnResponse_GetArenaDailyReward);
            AddResponser(Id<MSG_ZR_UPDATE_AREMA_DEFEMDER>.Value, OnResponse_ChangeChallengerDefensive);
            AddResponser(Id<MSG_ZR_UPDATE_ARENA_DEFENSIVE_PET>.Value, OnResponse_UpdateAreanaDefensivePet);

            //secret area 
            //AddResponser(Id<MSG_ZR_SECRET_AREA_RANK_LIST>.Value, OnResponse_GetSecretAreaRank);

            //cross battle
            AddResponser(Id<MSG_ZRZ_RETURN_BATTLE_PLAYER_INFO>.Value, OnResponse_ReturnCrossBattlePlayerInfo);
            AddResponser(Id<MSG_ZR_SET_BATTLE_RESULT>.Value, OnResponse_SetCrossBattleResult);
            AddResponser(Id<MSG_ZR_SHOW_CROSS_BATTLE_FINALS>.Value, OnResponse_ShowCrossBattleFinals);

            AddResponser(Id<MSG_ZR_SHOW_CROSS_BATTLE_CHALLENGER>.Value, OnResponse_ShowCrossBattleChallenger);
            AddResponser(Id<MSG_ZR_CROSS_BATTLE_CHALLENGER_HERO_INFO>.Value, OnResponse_UpdateCrossBattleHeroInfo);

            AddResponser(Id<MSG_ZRZ_GET_BATTLE_CHALLENGER_INFO>.Value, OnResponse_GetCrossBattleChallenger);
            AddResponser(Id<MSG_ZR_ADD_BATTLE_CHALLENGER_INFO>.Value, OnResponse_AddCrossBattleChallenger);
            //AddResponser(Id<MSG_ZR_SHOW_CROSS_SEASON_LEADER_INFO>.Value, OnResponse_ShowCrossleaderInfo);
            AddResponser(Id<MSG_ZR_GET_CROSS_VIDEO>.Value, OnResponse_GetCrossBattleVedio);
            AddResponser(Id<MSG_ZR_GET_CROSS_BATTLE_START>.Value, OnResponse_GetCrossBattleStartTime);
            AddResponser(Id<MSG_ZR_GET_GUESSING_INFO>.Value, OnResponse_GetCrossGuessingInfo);
            AddResponser(Id<MSG_ZR_CROSS_GUESSING_CHOOSE>.Value, OnResponse_CrossGuessingChoose);
            AddResponser(Id<MSG_ZR_CROSS_GUESSING_REWARD>.Value, OnResponse_CrossGuessingReward);

            //阵营建设
            AddResponser(Id<MSG_ZR_CAMPBUILD_INFO>.Value, OnResponse_CampBulidInfo);
            AddResponser(Id<MSG_ZR_CAMPBUILD_RANK_LIST>.Value, OnResponse_CampBuildRankList);
            AddResponser(Id<MSG_ZR_CAMPBUILD_ADD_VALUE>.Value, OnResponse_CampBuildAddValue);
            AddResponser(Id<MSG_ZR_CAMP_CREATE_DUNGEON>.Value, OnResponse_CampCreateDungeon);
            AddResponser(Id<MSG_ZR_CAMP_DUNGEON_END>.Value, OnResponse_CampBattleResult);

            //阵营战
            AddResponser(Id<MSG_ZR_GET_CAMPBATTLE_INFO>.Value, OnResponse_GetCampBattleInfo);
            AddResponser(Id<MSG_ZR_GET_FORT_INFO>.Value, OnResponse_GetFortInfo);
            AddResponser(Id<MSG_ZR_GET_CAMPBATTLE_RANK_LIST>.Value, OnResponse_GetCampBattleRankList);

            AddResponser(Id<MSG_ZR_ADD_CAMP_GRAIN>.Value, OnResponse_AddCampGrain);
            AddResponser(Id<MSG_ZR_GET_CAMP_GRAIN>.Value, OnResponse_GetCampCoin);

            //AddResponser(Id<MSG_ZR_CHECK_USE_NATURE_ITEM>.Value, OnResponse_CheckUseNatureItem);
            //AddResponser(Id<MSG_ZR_USE_NATURE_ITEM>.Value, OnResponse_UseNatureItem);
            AddResponser(Id<MSG_ZR_UPDATE_NATURE_COUNT>.Value, OnResponse_UpdateNatureCount);
            AddResponser(Id<MSG_ZR_GIVEUP_FORT>.Value, OnResponse_GiveUpFort);
            AddResponser(Id<MSG_ZR_HOLD_FORT>.Value, OnResponse_HoldFort);
            AddResponser(Id<MSG_ZR_SYNC_HISTORICALMAXCAMPSCORE>.Value, OnResponse_SyncHistoricalMaxCampScore);
            AddResponser(Id<MSG_ZR_UPDATE_DEFENSIVEQUEUE>.Value, OnResponse_UpdateDefensiveQueue);

            //RANK 
            AddResponser(Id<MSG_ZR_ADD_RANK_SCORE>.Value, OnResponse_AddRankScore);
            AddResponser(Id<MSG_ZR_GET_RANK_LIST>.Value, OnResponse_GetRankList);
            AddResponser(Id<MSG_ZR_CHECK_NEW_RANK_REWARD>.Value, OnResponse_CheckNewRankReward);
            AddResponser(Id<MSG_ZR_RANK_REWARD_LIST>.Value, OnResponse_GetRankRewardList);
            AddResponser(Id<MSG_ZR_GET_RANK_REWARD>.Value, OnResponse_GetRankReward);
            AddResponser(Id<MSG_ZR_UPDATE_RANK_VALUE>.Value, OnResponse_UpdateRankValue);
            AddResponser(Id<MSG_ZR_RANK_REWARD_PAGE>.Value, OnResponse_GetRankRewardPage);
            AddResponser(Id<MSG_ZR_NOTIFY_RANK_REWARD>.Value, OnResponse_NotifyRankReward);
            AddResponser(Id<MSG_ZR_GET_RANK_FIRST_INFO>.Value, OnResponse_GetRankFirstInfo);
            AddResponser(Id<MSG_ZR_GET_CROSS_RANK_REWARD>.Value, OnResponse_GetCrossRankReward);

            //Announce
            AddResponser(Id<MSG_ZR_INTEGRALBOSS_START>.Value, OnResponse_IntegralBossStart);
            AddResponser(Id<MSG_ZR_INTEGRALBOSS_END>.Value, OnResponse_IntegralBossEnd);
            AddResponser(Id<MSG_ZR_BROADCAST_ANNOUNCEMENT>.Value, OnResponse_CrossBroadcastAnnouncement);
            AddResponser(Id<MSG_ZR_CROSS_NOTES_LIST>.Value, OnResponse_CrossNotesList);
            AddResponser(Id<MSG_ZR_NOTES_LIST_BY_TYPE>.Value, OnResponse_GetNotesListByType);

            //Gift
            AddResponser(Id<MSG_ZR_GIFT_CODE_REWARD>.Value, OnResponse_GiftCodeExchangeReward);
            AddResponser(Id<MSG_ZR_CHECK_GIFT_CODE_REWARD>.Value, OnResponse_CheckGiftCodeExchangeReward);
            AddResponser(Id<MSG_ZR_CHECK_CODE_UNIQUE>.Value, OnResponse_CheckCodeUnique);

            //金兰
            AddResponser(Id<MSG_ZR_BROTHERS_INVITE>.Value, OnResponse_BrotherInvite);
            AddResponser(Id<MSG_ZR_BROTHERS_RESPONSE>.Value, OnResponse_BrotherResponse);
            AddResponser(Id<MSG_ZR_BROTHERS_REMOVE>.Value, OnResponse_BrotherRemove);

            AddResponser(Id<MSG_ZR_FRIEND_INVITE>.Value, OnResponse_FriendInvite);
            AddResponser(Id<MSG_ZR_FRIEND_RESPONSE>.Value, OnResponse_FriendResponse);
            AddResponser(Id<MSG_ZR_FRIEND_REMOVE>.Value, OnResponse_FriendRemove);

            //贡献
            AddResponser(Id<MSG_ZR_CONTRIBUTION_INFO>.Value, OnResponse_ContributionInfo);


            //暗器
            AddResponser(Id<MSG_ZR_GET_HIDDER_WEAPON_VALUE>.Value, OnResponse_GetHidderWeaponInfo);
            AddResponser(Id<MSG_ZR_UPDATE_HIDDER_WEAPON_VALUE>.Value, OnResponse_UpdateHidderWeaponInfo);
            AddResponser(Id<MSG_ZR_GET_SEA_TREASURE_VALUE>.Value, OnResponse_GetSeaTreasureInfo);
            AddResponser(Id<MSG_ZR_UPDATE_SEA_TREASURE_VALUE>.Value, OnResponse_UpdateSeaTreasureInfo);
            AddResponser(Id<MSG_ZR_GET_DIVINE_LOVE_VALUE>.Value, OnResponse_GetDivineLoveInfo);
            AddResponser(Id<MSG_ZR_UPDATE_DIVINE_LOVE_VALUE>.Value, OnResponse_UpdateDivineLoveInfo);
            AddResponser(Id<MSG_ZR_GET_STONE_WALL_VALUE>.Value, OnResponse_GetStoneWallInfo);
            AddResponser(Id<MSG_ZR_UPDATE_STONE_WALL_VALUE>.Value, OnResponse_UpdateStoneWallInfo);

            //cross boss
            AddResponser(Id<MSG_ZR_GET_CROSS_BOSS_INFO>.Value, OnResponse_GetCrossBossInfo);
            AddResponser(Id<MSG_ZR_ENTER_CROSS_BOSS_MAP>.Value, OnResponse_StartChallengeCrossBoss);
            AddResponser(Id<MSG_ZRZ_RETURN_BOSS_PLAYER_INFO>.Value, OnResponse_ReturnCrossBossPlayerInfo);
            AddResponser(Id<MSG_ZR_CROSS_BOSS_CHALLENGER>.Value, OnResponse_GetCrossBossChallenger);
            AddResponser(Id<MSG_ZR_CHALLENGE_CROSS_BOSS_MAP>.Value, OnResponse_ChallengeCrossBoss);
            AddResponser(Id<MSG_ZR_CHANGE_CROSS_BOSS_SCORE>.Value, OnResponse_AddCrossBossScore);

            //海岛登高 
            AddResponser(Id<MSG_ZR_GET_ISLAND_HIGH_INFO>.Value, OnResponse_GetIslandHighInfo);

            //cross challenge
            AddResponser(Id<MSG_ZRZ_RETURN_CROSS_CHALLENGE_BATTLE_PLAYER_INFO>.Value, OnResponse_ReturnCrossChallengePlayerInfo);
            AddResponser(Id<MSG_ZR_SET_CROSS_CHALLENGE_BATTLE_RESULT>.Value, OnResponse_SetCrossChallengeResult);
            AddResponser(Id<MSG_ZR_SHOW_CROSS_CHALLENGE_BATTLE_FINALS>.Value, OnResponse_ShowCrossChallengeFinals);

            AddResponser(Id<MSG_ZR_SHOW_CROSS_CHALLENGE_CHALLENGER>.Value, OnResponse_ShowCrossChallengeChallenger);
            AddResponser(Id<MSG_ZR_CROSS_CHALLENGE_CHALLENGER_HERO_INFO>.Value, OnResponse_UpdateCrossChallengeHeroInfo);

            AddResponser(Id<MSG_ZRZ_GET_CROSS_CHALLENGE_CHALLENGER_INFO>.Value, OnResponse_GetCrossChallengeChallenger);
            AddResponser(Id<MSG_ZR_ADD_CROSS_CHALLENGE_CHALLENGER_INFO>.Value, OnResponse_AddCrossChallengeChallenger);
            //AddResponser(Id<MSG_ZR_SHOW_CROSS_SEASON_LEADER_INFO>.Value, OnResponse_ShowCrossleaderInfo);
            AddResponser(Id<MSG_ZR_GET_CROSS_CHALLENGE_VIDEO>.Value, OnResponse_GetCrossChallengeVedio);
            AddResponser(Id<MSG_ZR_GET_CROSS_CHALLENGE_START>.Value, OnResponse_GetCrossChallengeStartTime);
            AddResponser(Id<MSG_ZR_GET_CROSS_CHALLENGE_GUESSING_INFO>.Value, OnResponse_GetCrossChallengeGuessingInfo);
            AddResponser(Id<MSG_ZR_CROSS_CHALLENGE_GUESSING_CHOOSE>.Value, OnResponse_CrossChallengeGuessingChoose);
            AddResponser(Id<MSG_ZR_CROSS_CHALLENGE_GUESSING_REWARD>.Value, OnResponse_CrossChallengeGuessingReward);

            //仓库
            AddResponser(Id<MSG_ZR_ADD_WAREHOUSE_ITEMINFO>.Value, OnResponse_AddWarehouseItemInfo);
            AddResponser(Id<MSG_ZR_SPACETIME_MONSTER_INFO>.Value, OnResponse_SpaceTimeMonsterInfo);
            //ResponserEnd
        }

        public override void Update(double dt)
        {
            base.Update(dt);

            UpdateChat();
        }
   
        public bool IsSameZone(int main_id, int sub_id)
        {
            return (MainId == main_id && SubId == sub_id);
        }

        private void OnResponse_RegistClientInfoList(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_REGIST_CLIENT_INFO_LIST msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_REGIST_CLIENT_INFO_LIST>(stream);
            Log.Write("got zone main {0} sub {1} regist online client count {2}", MainId, SubId, msg.ClientList.Count);
            foreach (var info in msg.ClientList)
            {
                Client client = new Client(info.Uid, Api, MainId);
                client.Level = info.Level;
                client.Research = info.Research;
                client.Camp = (CampType)info.Camp;
                client.ChapterId = info.ChapterId;
                ZoneManager.AddClient(client);

                Api.RPlayerInfoMng.CheckUpdatePlayerInfo(info.Uid);
            }
        }

		public void OnResponse_ClientEnter(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_CLIENT_ENTER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_CLIENT_ENTER>(stream);
            Log.Write("player {0} enter zone main {1} sub {2}", msg.CharacterUid, MainId, SubId);

            Client client = ZoneManager.GetClient(msg.CharacterUid);
            if (client == null)
            {
                client = new Client(msg.CharacterUid, Api, MainId);
                client.Level = msg.Level;
                client.ChapterId = msg.ChapterId;
                client.Research = msg.Research;
                client.Camp = (CampType)msg.Camp;
                try
                {
                    ZoneManager.AddClient(client);
                    Api.RPlayerInfoMng.CheckUpdatePlayerInfo(msg.CharacterUid);
                }
                catch (Exception e)
                {
                    Log.Alert(e.ToString());
                }
            }
            else
            {
                if (msg.Online == false)
                {
                    if (client.CurZone == null)
                    {
                        Log.Warn("player {0} enter zone main {1} sub {2} failed: curZone is null", msg.CharacterUid, MainId, SubId);
                        return;
                    }
                    else
                    {
                        if (!IsSameZone(client.CurZone.MainId, client.CurZone.SubId))
                        {
                            Log.Write("player {0} change zone from main {1} sub {2} to sub {3} with client", client.Uid, client.CurZone.MainId, client.CurZone.SubId, SubId);
                            client.UpdateZone(this);
                        }
                    }
                }
                else
                {
                    Log.Write("player {0} online main {1} sub {2}", client.Uid, MainId, SubId);

                    client.Team = ZoneManager.TeamManager.GetPcJoinedTeam(client.Uid, true);
                    client.SetOnline(this, msg.LastLoginTime);
                }
                return;
            }

            client.IsLoaded = true;
            client.Team = ZoneManager.TeamManager.GetPcJoinedTeam(client.Uid, true);
            client.SetOnline(this, msg.LastLoginTime);
        }

        public void OnResponse_ClientLeaveZone(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_CLIENT_LEAVE_ZONE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_CLIENT_LEAVE_ZONE>(stream);
            Log.Write("player {0} leave zone offline main {1} sub {2}", msg.CharacterUid, MainId, SubId);

            Client client = ZoneManager.GetClient(msg.CharacterUid);
            if (client == null)
            {
                Log.Warn("player {0} leave zone failed: no such client", msg.CharacterUid);
                return;
            }

            if (!ZoneManager.RemoveClient(msg.CharacterUid))
            {
                Log.Warn("player {0} leave zone failed: no such client", msg.CharacterUid);
                return; 
            }

            client.SetOffline(this);
        }

        public void OnResponse_ClientLevelUp(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_LEVEL_UP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_LEVEL_UP>(stream);

            Client client = ZoneManager.GetClient(msg.Uid);
            if (client == null)
            {
                Team team = ZoneManager.TeamManager.GetPcJoinedTeam(msg.Uid);
                if (team == null) return;
                    
                TeamMember member = team.GetTeamMember(msg.Uid);
                if (member == null) return;

                member.Research = msg.Research;
                return;
            }
            client.Level = msg.Level;
            client.Research = msg.Research;
            client.ChapterId = msg.Chapter;
            client.Team?.NotifyMemberLevelUp(client);
        }

        public void OnResponse_ChapterIdChange(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_UPDATE_CHAPTERID msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_UPDATE_CHAPTERID>(stream);
            Client client = ZoneManager.GetClient(uid);
            if (client == null)
            {
                Log.Warn("player {0} ChapterIdChange: no such client", uid);
                return;
            }
            client.ChapterId = msg.ChapterId;
            client.Team?.NotifyMemberLevelUp(client);
        }


        public void OnResponse_PlayerDetailInfo(MemoryStream stream, int uid = 0)
        {
            //MSG_ZR_PLAYER_DETAIL_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_PLAYER_DETAIL_INFO>(stream);
            //PKS_ZC_PLAYER_DETAIL_INFO response = new PKS_ZC_PLAYER_DETAIL_INFO();
            //response.applyUid = msg.applyUid;
            //Client client = zoneManager.GetClient(msg.Uid);
            //Client apply = zoneManager.GetClient(msg.applyUid);
            //if (client != null)
            //{
            //    client.GetDetailInfo(response);
            //    Write(response);

            //    if (apply != null && client.CurZone != null)
            //    {
            //        // 通知被偷看
            //        MSG_RZ_NOTIFY_PEEPED notify = new MSG_RZ_NOTIFY_PEEPED();
            //        notify.Uid = client.Uid;
            //        notify.peeperName = apply.Name;
            //        client.CurZone.Write(notify);
            //    }
            //}
        }

        public void OnResponse_UpdateEmail(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_UPDATE_EMAI msg = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_UPDATE_EMAI>(stream);
            int pcUid = msg.PcUid;
            string ids = msg.EmailIdS;
            Api.GameDBPool.Call(new QueryUpdateSystemEmail(pcUid, ids));
        }

        public void OnResponse_DeleteAllEmail(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_DELETE_ALL_EMAI pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_DELETE_ALL_EMAI>(stream);

            if (pks.EmailId > 0)
            {
                //string emailTableName = "email";
                Api.GameDBPool.Call(new QueryDeleteAllSystemEmail(pks.EmailId, pks.SendTime, pks.DeleteTime));

                MSG_RZ_DELETE_ALL_EMAI msg = new MSG_RZ_DELETE_ALL_EMAI();
                msg.EmailId = pks.EmailId;
                msg.SendTime = pks.SendTime;
                msg.DeleteTime = pks.DeleteTime;
                Api.ZoneManager.Broadcast(msg);
            }
        }

        public void OnResponse_SendEmail(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_SEND_EMAIL pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_SEND_EMAIL>(stream);

            if (pks.Uid > 0)
            {
                Api.EmailMng.SendPersonEmail(pks.Uid, pks.EmailId, pks.Reward, pks.SaveTime, pks.Param);
            }
        }

        public void OnResponse_ErrorCode(MemoryStream stream, int uid = 0)
        {
            MSG_ZR_ERROR_CODE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZR_ERROR_CODE>(stream);
            Client client = ZoneManager.GetClient(pks.PcUid);
            if (client == null)
            {
                BaseServer relation = Api.GetRelationServer(pks.MainId);
                if (relation == null)
                {
                    Log.Warn("player {0} ZR ErrorCode not find", pks.PcUid);
                    return;
                }
                else
                {
                    MSG_RR_ERROR_CODE msg = new MSG_RR_ERROR_CODE();
                    msg.PcUid = pks.PcUid;
                    msg.MainId = pks.MainId;
                    msg.ErrorCode = pks.ErrorCode;
                    relation.Write(msg);
                }
            }
            else
            {
                MSG_RZ_ERROR_CODE msg = new MSG_RZ_ERROR_CODE();
                msg.PcUid = pks.PcUid;
                msg.MainId = pks.MainId;
                msg.ErrorCode = pks.ErrorCode;
                client.CurZone.Write(msg);
            }
        }

        //protected override void SendRegistSpecInfo()
        //{
        //    SyncRankInfos(this);
        //}

    }
}
