using DBUtility;
using EnumerateUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using Message.IdGenerator;
using Message.Relation.Protocol.RZ;
using Message.Zone.Protocol.ZR;
using ServerFrame;
using ServerModels;
using ServerShared;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ZoneServerLib
{
    public partial class RelationServer : BackendServer
    {
        private ZoneServerApi Api
        { get { return (ZoneServerApi)api; } }

        public RelationServer(BaseApi api)
            : base(api)
        {
        }

        public override void Update(double dt)
        {
            base.Update(dt);

            UpdateChat();
        }

        protected override void BindResponser()
        {
            base.BindResponser();
            AddResponser(Id<MSG_RZ_RANKING_ALL_LIST>.Value, OnResponse_RankingAllList);
            // Friend
            AddResponser(Id<MSG_RZ_CHALLENGE_PLAYER_REQUEST>.Value, OnResponse_ChallengePlayerRequst);
            AddResponser(Id<MSG_RZ_FRIEND_HEART_GIVE>.Value, OnResponse_FriendHeartGive);

            AddResponser(Id<MSG_RZ_SEND_EMAILS>.Value, OnResponse_AddNewEmailInfo);
            AddResponser(Id<MSG_RZ_DELETE_ALL_EMAI>.Value, OnResponse_DeleteAllEmail);
            AddResponser(Id<MSG_RZ_SAVE_EMAI>.Value, OnResponse_SaveEmail);

            // Team
            AddResponser(Id<MSG_RZ_TEAM_TYPE_LIST>.Value, OnResponse_TeamTypeList);
            AddResponser(Id<MSG_RZ_CREATE_TEAM>.Value, OnResponse_CreateTeam);
            AddResponser(Id<MSG_RZ_JOIN_TEAM>.Value, OnResponse_JoinTeam);
            AddResponser(Id<MSG_RZ_NEW_TEAM_MEMBER_JOIN>.Value, OnResponse_NewTeamMemberJoin);
            AddResponser(Id<MSG_RZ_TEAM_MEMBER_LEAVE>.Value, OnResponse_TeamMemberLeave);
            AddResponser(Id<MSG_RZ_QUIT_TEAM>.Value, OnResponse_QuitTeam);
            AddResponser(Id<MSG_RZ_KICK_TEAM_MEMBER>.Value, OnResponse_KickTeamMember);
            AddResponser(Id<MSG_RZ_TRANDSFER_CAPTAIN>.Value, OnResponse_TransferCaptain);
            AddResponser(Id<MSG_RZ_CAPTAIN_CHANGE>.Value, OnResponse_CaptainChange);
            AddResponser(Id<MSG_RZ_TEAM_MEMBER_OFFLINE>.Value, OnResponse_TeamMemberOffline);
            AddResponser(Id<MSG_RZ_TEAM_MEMBER_ONLINE>.Value, OnResponse_TeamMemberOnline);
            AddResponser(Id<MSG_RZ_ASK_JOIN_TEAM>.Value, OnResponse_AskJoinTeam);
            AddResponser(Id<MSG_RZ_INVITE_JOIN_TEAM>.Value, OnResponse_InviteJoinTeam);
            AddResponser(Id<MSG_RZ_ASK_INVITE_JOIN_TEAM>.Value, OnResponse_AskInviteJoinTeam);
            AddResponser(Id<MSG_RZ_ANSWER_INVITE_JOIN_TEAM>.Value, OnResponse_AnswerInviteJoinTeam);

            AddResponser(Id<MSG_RZ_ASK_FOLLOW_CAPTAIN>.Value, OnResponse_AskFollowCaptain);
            AddResponser(Id<MSG_RZ_TRY_ASK_FOLLOW_CAPTAIN>.Value, OnResponse_TryAskFollowCaptain);
            AddResponser(Id<MSG_RZ_TRY_ANSWER_FOLLOW_CAPTAIN>.Value, OnResponse_TryAnswerFollowCaptain);
            AddResponser(Id<MSG_RZ_ANSWER_FOLLOW_CAPTAIN>.Value, OnResponse_AnswerFollowCaptain);
            AddResponser(Id<MSG_RZ_CHANGE_TEAM_TYPE>.Value, OnResponse_ChangeTeamType);
            AddResponser(Id<MSG_RZ_TEAM_MEMEBR_CHANGE_ZONE>.Value, OnResponse_TeamMemberChangeZone);
            AddResponser(Id<MSG_RZ_NEW_TEAM_DUNGEON>.Value, OnResponse_NewTeamDungeon);
            AddResponser(Id<MSG_RZ_TRY_CREATE_ROBOT_MEMBER>.Value, OnResponse_NewTeamDungeon4Robot);
            AddResponser(Id<MSG_RZ_NEED_TEAM_HELP>.Value, OnResponse_NeedTeamHelp);
            AddResponser(Id<MSG_RZ_REQUEST_TEAM_HELP>.Value, OnResponse_RequestTeamHelp);
            AddResponser(Id<MSG_RZ_RESPONSE_TEAM_HELP>.Value, OnResponse_ResponseTeamHelp);

            AddResponser(Id<MSG_RZ_TEAM_MEMBER_LEVELUP>.Value, OnResponse_TeamMemberLevelUp);
            AddResponser(Id<MSG_RZ_ASK_PVP_CHALLENGE>.Value, OnResponse_AskPVPChallenge);

            AddResponser(Id<MSG_RZ_NOTIFY_TEAM_CONT_HUNTING>.Value, OnResponse_NotifyTeamContinueHunting);
            AddResponser(Id<MSG_RZ_HUNTING_HELP>.Value, OnResponse_HuntingHelp);
            AddResponser(Id<MSG_RZ_HUNTING_HELP_ASK>.Value, OnResponse_HuntingHelpAsk);
            AddResponser(Id<MSG_RZ_HUNTING_HELP_ANSWER_JOIN>.Value, OnResponse_HuntingHelpAnswerJoin);


            //Family
            AddResponser(Id<MSG_RZ_JOIN_FAMILY>.Value, OnResponse_JoinFamily);
            AddResponser(Id<MSG_RZ_JOIN_FAMILIES>.Value, OnResponse_JoinFamilies);
            AddResponser(Id<MSG_RZ_NEW_FAMILY_APPLICANT>.Value, OnResponse_NewFamilyApplicant);
            AddResponser(Id<MSG_RZ_CREATE_FAMILY>.Value, OnResponse_CreateFamily);
            AddResponser(Id<MSG_RZ_UPDATE_FAMILY_INFO>.Value, OnResponse_UpdateFamilyInfo);
            AddResponser(Id<MSG_RZ_ASSIGN_FAMILY_TITLE>.Value, OnResponse_AssignFamilyTitle);
            AddResponser(Id<MSG_RZ_QUIT_FAMILY>.Value, OnResponse_QuitFamily);
            AddResponser(Id<MSG_RZ_KICK_FAMILY_MEMBER>.Value, OnResponse_KickFamilyMember);
            AddResponser(Id<MSG_RZ_FAMILY_LEVELUP>.Value, OnResponse_FamilyLevelUp);
            AddResponser(Id<MSG_RZ_FAMILY_CONTENT_EDIT>.Value, OnResponse_FamilyContentEdit);
            AddResponser(Id<MSG_RZ_NOTIFY_PEEPED>.Value, OnResponse_NotifyPeeped);


            //Rank
            AddResponser(Id<MSG_RZ_UPDATE_CAMP_RANK_PERIOD>.Value, OnResponse_UpdateCampRankPeriod);
            //AddResponser(Id<MSG_RZ_UPDATE_RANK_PERIOD>.Value, OnResponse_UpdateRankPeriod);
            AddResponser(Id<MSG_RZ_UPDATE_CAMP_ELECTION_PERIOD>.Value, OnResponse_UpdateElectionPeriod);
            AddResponser(Id<MSG_RZ_WEAK_CAMP>.Value, OnResponse_ChooseCamp);
            AddResponser(Id<MSG_RZ_CHANGE_CAMP>.Value, OnResponse_ChangeCamp);
            AddResponser(Id<MSG_RZ_CAMP_RANK_LIST>.Value, OnResponse_CampRankInfo);
            AddResponser(Id<MSG_RZ_CAMP_PANEL_LIST>.Value, OnResponse_CampPanelInfo);
            AddResponser(Id<MSG_RZ_CAMP_ELECTION_LIST>.Value, OnResponse_CampElectionInfo);
            //AddResponser(Id<MSG_RZ_POP_RANK_REFRESH>.Value, OnResponse_PopRankRefresh);
            //AddResponser(Id<MSG_RZ_POP_RANK_CLEAR>.Value, OnResponse_PopRankClear);
            AddResponser(Id<MSG_RZ_GET_RANK_LIST>.Value, OnResponse_NewGetRankList);


            //Vedio
            //CHAT
            AddResponser(Id<MSG_RZ_CHAT_LIST>.Value, OnResponse_ChatList);
            AddResponser(Id<MSG_RZ_CHAT_TRUMPET>.Value, OnResponse_ChatTrumpet);

            //Guild
            AddResponser(Id<MSG_RZ_MAX_GUILDID>.Value, OnResponse_MaxGuildId);

            //SHow
            AddResponser(Id<Message.Gate.Protocol.GateC.MSG_ZRZ_RETURN_PLAYER_SHOW>.Value, OnResponse_ReturnShowPlayer);
            AddResponser(Id<MSG_RZ_GET_SHOW_PLAYER>.Value, OnResponse_GetShowPlayer);
            AddResponser(Id<MSG_RZ_NOT_FIND_SHOW_PLAYER>.Value, OnResponse_NotFindShowPlayer);
            AddResponser(Id<MSG_RZ_ONE_SERVER_FIND_SHOW_PLAYER>.Value, OnResponse_OneServerFindShowPlayer);

            //arena  
            AddResponser(Id<MSG_RZ_GET_ARENA_CHALLENGERS>.Value, OnResponse_GeArenaChallenger);
            AddResponser(Id<MSG_RZ_SHOW_ARENA_RANK_INFO>.Value, OnResponse_ShowArenaRankInfo);
            AddResponser(Id<MSG_RZ_GET_ARENA_CHALLENGER>.Value, OnResponse_GetArenaChallenger);
            AddResponser(Id<Message.Gate.Protocol.GateC.MSG_ZRZ_RETURN_ARENA_CHALLENGER>.Value, OnResponse_ReturnArenaChallenger);
            AddResponser(Id<MSG_RZ_NOT_FIND_ARENA_CHALLENGER>.Value, OnResponse_NotFindArenaChallenger);
            AddResponser(Id<MSG_RZ_CHALLENGE_WIN_CHANGE_RANK>.Value, OnResponse_ChallengeWinChangeRank);
            AddResponser(Id<MSG_RZ_CHALLENGER_RANK_CHANGE>.Value, OnResponse_ChallengerRankChange);
            AddResponser(Id<Message.Gate.Protocol.GateC.MSG_ZGC_ARENA_CHALLENGER_HERO_INFO>.Value, OnResponse_GetChallengerInfo);

            //rank
            AddResponser(Id<MSG_RZ_SECRET_AREA_RANK_LIST>.Value, OnResponse_SecretAreaRankInfo);
            AddResponser(Id<MSG_RZ_NEW_RANK_REWARD>.Value, OnResponse_CheckNewRankReward);
            AddResponser(Id<MSG_RZ_RANK_REWARD_LIST>.Value, OnResponse_GetRankRewardList);
            AddResponser(Id<MSG_RZ_GET_RANK_REWARD>.Value, OnResponse_GetRankReward);
            AddResponser(Id<MSG_RZ_RANK_REWARD_PAGE>.Value, OnResponse_GetRankRewardPage);

            //
            AddResponser(Id<MSG_RZ_GET_BATTLE_PLAYER>.Value, OnResponse_GetCrossBattlePlayerInfo);
            AddResponser(Id<MSG_RZ_SHOW_CROSS_RANK_INFO>.Value, OnResponse_ShowCrossRankInfo);
            AddResponser(Id<MSG_RZ_SHOW_CROSS_LEADER_INFO>.Value, OnResponse_ShowCrossLeaderInfo);
            AddResponser(Id<MSG_RZ_UPDATE_CROSS_RANK>.Value, OnResponse_UpdateCrossRank);
            AddResponser(Id<MSG_RZ_GET_CROSS_VIDEO>.Value, OnResponse_GetCrossBattleVedio);
            AddResponser(Id<MSG_ZRZ_RETURN_BATTLE_PLAYER_INFO>.Value, OnResponse_ReturnCrossBattlePlayerInfo);
            AddResponser(Id<MSG_ZRZ_GET_BATTLE_CHALLENGER_INFO>.Value, OnResponse_GetCrossBattleChallengerInfo);
            AddResponser(Id<MSG_RZ_SHOW_CROSS_BATTLE_FINALS_INFO>.Value, OnResponse_ShowCrossBattleFinals);
            AddResponser(Id<MSG_RZ_CROSS_BATTLE_CHALLENGER>.Value, OnResponse_ShowCrossBattleChallenger);
            AddResponser(Id<MSG_RZ_GET_BATTLE_HEROS>.Value, OnResponse_GetCrossBattleHeros);
            AddResponser(Id<MSG_RZ_CLEAR_PLAYER_FINAL>.Value, OnResponse_ClearCrossFinalsPlayerRank);
            AddResponser(Id<MSG_RZ_UPDATE_PLAYER_FINAL>.Value, OnResponse_UpdateCrossFinalsPlayerRank);
            AddResponser(Id<MSG_RZ_CLEAR_BATTLE_RANK>.Value, OnResponse_ClearCrossBattleRanks);
            AddResponser(Id<MSG_RZ_BATTLE_START>.Value, OnResponse_CrossBattleStart);
            AddResponser(Id<MSG_RZ_GET_BATTLE_START>.Value, OnResponse_GetCrossBattleStart);
            AddResponser(Id<MSG_RZ_CROSS_BATTLE_SERVER_REWARD>.Value, OnResponse_CrossBattleServerReward);
            AddResponser(Id<MSG_RZ_GET_GET_GUESSING_INFO>.Value, OnResponse_GetGuessingPlayersInfo);
            AddResponser(Id<MSG_RZ_CROSS_GUESSING_CHOOSE>.Value, OnResponse_CrossGuessingChoose);
            AddResponser(Id<MSG_RZ_OPEN_GUESSING_TEAM>.Value, OnResponse_CrossGuessingTeam);
            AddResponser(Id<MSG_RZ_NOTICE_PLAYER_TEAM_ID>.Value, OnResponse_UpdateCrossBattleTeamId);

            //阵营建设
            AddResponser(Id<MSG_RZ_CAMPBUILD_INFO>.Value, OnResponse_CampBuildInfo);
            AddResponser(Id<MSG_RZ_CAMPBUILD_RANK_LIST>.Value, OnResponse_CampBuildRankList);
            AddResponser(Id<MSG_RZ_CAMPBUILD_RESET>.Value, OnResponse_CampBuildRest);
            AddResponser(Id<MSG_RZ_RESET_CAMP_BUILD_COUNTER>.Value, OnResponse_RestCampBuildCounter);

            //阵营战
            AddResponser(Id<MSG_RZ_SYNC_CAMPBATTLE_DATA>.Value, OnResponse_SyncCampBattleInfo);
            AddResponser(Id<MSG_RZ_GET_FORT_DATA>.Value, OnResponse_GetFortInfo);
            AddResponser(Id<MSG_RZ_CLEAR_CAMP_BATTLE_SCORE>.Value, OnResponse_ClearCampBattleScore);
            AddResponser(Id<MSG_RZ_CAMP_GRAIN>.Value, OnResponse_CampCoin);
            AddResponser(Id<MSG_RZ_CAMPBATTLE_RANK_LIST>.Value, OnResponse_GetRankList);
            AddResponser(Id<MSG_RZ_CAMP_CREATE_DUNGEON>.Value, OnResponse_CampCreateDungeon);
            AddResponser(Id<MSG_RZ_CAMP_DUNGEON_END>.Value, OnResponse_CampDungeonEnd);
            AddResponser(Id<MSG_RZ_CAMPBATTLE_END>.Value, OnResponse_CampBattleEnd);
            AddResponser(Id<MSG_RZ_CHECK_USE_NATURE_ITEM>.Value, OnResponse_CheckUseNatureItem);
            AddResponser(Id<MSG_RZ_USE_NATURE_ITEM>.Value, OnResponse_UseNatureItem);
            AddResponser(Id<MSG_RZ_CAMP_BOX_COUNT>.Value, OnResponse_CampBoxCount);
            AddResponser(Id<MSG_RZ_CAMP_BATTLE_SCORE_ADD>.Value, OnResponse_SyncCampBattleScoreAdd);
            AddResponser(Id<MSG_RZ_GIVEUP_FORT>.Value, OnResponse_GiveUpFort);
            AddResponser(Id<MSG_RZ_HOLD_FORT>.Value, OnResponse_HoldFort);

            //Announce
            AddResponser(Id<MSG_RZ_INTEGRALBOSS_START>.Value, OnResponse_IntegralBossStart);
            AddResponser(Id<MSG_RZ_INTEGRALBOSS_END>.Value, OnResponse_IntegralBossEnd);
            AddResponser(Id<MSG_RZ_BROADCAST_ANNOUNCEMENT>.Value, OnResponse_BroadcastAnnouncement);
            AddResponser(Id<MSG_ZGC_CROSS_NOTES_LIST>.Value, OnResponse_ReturnNotesList);

            //Gift
            AddResponser(Id<MSG_RZ_GIFT_CODE_REWARD>.Value, OnResponse_GiftCodeExchangeReward);
            AddResponser(Id<MSG_RZ_CHECK_GIFT_CODE_REWARD>.Value, OnResponse_CheckGiftCodeExchangeReward);
            AddResponser(Id<MSG_RZ_CHECK_CODE_UNIQUE>.Value, OnResponse_CheckCodeUnique);

            //金兰
            AddResponser(Id<MSG_RZ_BROTHERS_INVITE>.Value, OnResponse_BrotherInvite);
            AddResponser(Id<MSG_RZ_BROTHERS_RESPONSE>.Value, OnResponse_BrotherResponse);
            AddResponser(Id<MSG_RZ_BROTHERS_REMOVE>.Value, OnResponse_BrotherRemove);

            AddResponser(Id<MSG_RZ_FRIEND_INVITE>.Value, OnResponse_FriendInvite);
            AddResponser(Id<MSG_RZ_FRIEND_RESPONSE>.Value, OnResponse_FriendResponse);
            AddResponser(Id<MSG_RZ_FRIEND_REMOVE>.Value, OnResponse_FriendRemove);

            //贡献
            AddResponser(Id<MSG_RZ_CONTRIBUTION_INFO>.Value, OnResponse_ContributionInfo);

            //主题Boss
            AddResponser(Id<MSG_RZ_NEW_THEMEBOSS>.Value, OnResponse_OpenNewThemeBoss);
            //称号          
            AddResponser(Id<MSG_RZ_LOSE_ARENA_FIRST>.Value, OnResponse_LoseArenaFirst);


            AddResponser(Id<MSG_RZ_GET_HIDDER_WEAPON_VALUE>.Value, OnResponse_GetHidderWeaponInfo);
            AddResponser(Id<MSG_RZ_GET_SEA_TREASURE_VALUE>.Value, OnResponse_GetSeaTreasureInfo);
            AddResponser(Id<MSG_RZ_GET_DIVINE_LOVE_VALUE>.Value, OnResponse_GetDivineLoveInfo);
            AddResponser(Id<MSG_RZ_GET_STONE_WALL_VALUE>.Value, OnResponse_GetStoneWallInfo);
            AddResponser(Id<MSG_RZ_CLEAR_VALUE>.Value, OnResponse_ClearValue);


            AddResponser(Id<MSG_RZ_GET_CROSS_BOSS_INFO>.Value, OnResponse_GetCrossBossInfo);
            AddResponser(Id<MSG_ZRZ_GET_BOSS_PLAYER_INFO>.Value, OnResponse_GetCrossBossPlayerInfo);
            AddResponser(Id<MSG_ZRZ_RETURN_BOSS_PLAYER_INFO>.Value, OnResponse_ReturnCrossBossPlayerInfoFromCross);
            AddResponser(Id<MSG_RZ_STOP_CROSS_BOSS_DUNGEON>.Value, OnResponse_StopCrossBossDungeon);
            AddResponser(Id<MSG_RZ_CROSS_BOSS_PASS_REWARD>.Value, OnResponse_SendCrossBossPassReward);
            AddResponser(Id<MSG_RZ_CROSS_BOSS_RANK_REWARD>.Value, OnResponse_SendCrossBossRankReward);

            AddResponser(Id<MSG_RZ_GET_RANK_FIRST_INFO>.Value, OnResponse_GetRankFirstInfo);
            AddResponser(Id<MSG_RZ_GET_CROSS_RANK_REWARD>.Value, OnResponse_GetCrossRankReward);
            AddResponser(Id<MSG_RZ_RECORD_RANK_ACTIVE_INFO>.Value, OnResponse_RecordRankActiveInfo);

            //海岛登高
            AddResponser(Id<MSG_RZ_GET_ISLAND_HIGH_INFO>.Value, OnResponse_GetIslandHighInfo);
            AddResponser(Id<MSG_RZ_UPDATE_RECHARGE_ACTIVITY_VALUE>.Value, OnResponse_UpdateRechargeActivityValue);

            //Cross challenge
            AddResponser(Id<MSG_RZ_GET_CROSS_CHALLENGE_PLAYER>.Value, OnResponse_GetCrossChallengePlayerInfo);
            AddResponser(Id<MSG_RZ_SHOW_CROSS_CHALLENGE_RANK_INFO>.Value, OnResponse_ShowCrossChallengeRankInfo);
            AddResponser(Id<MSG_RZ_SHOW_CROSS_CHALLENGE_LEADER_INFO>.Value, OnResponse_ShowCrossChallengeLeaderInfo);
            AddResponser(Id<MSG_RZ_UPDATE_CROSS_CHALLENGE_RANK>.Value, OnResponse_UpdateCrossChallengeRank);
            AddResponser(Id<MSG_RZ_GET_CROSS_CHALLENGE_VIDEO>.Value, OnResponse_GetCrossChallengeVedio);
            AddResponser(Id<MSG_ZRZ_RETURN_CROSS_CHALLENGE_BATTLE_PLAYER_INFO>.Value, OnResponse_ReturnCrossChallengePlayerInfo);
            AddResponser(Id<MSG_ZRZ_GET_CROSS_CHALLENGE_CHALLENGER_INFO>.Value, OnResponse_GetCrossChallengeChallengerInfo);
            AddResponser(Id<MSG_RZ_SHOW_CROSS_CHALLENGE_FINALS_INFO>.Value, OnResponse_ShowCrossChallengeFinals);
            AddResponser(Id<MSG_RZ_CROSS_CHALLENGE_CHALLENGER>.Value, OnResponse_ShowCrossChallengeChallenger);
            AddResponser(Id<MSG_RZ_GET_CROSS_CHALLENGE_HEROS>.Value, OnResponse_GetCrossChallengeHeros);
            AddResponser(Id<MSG_RZ_CROSS_CHALLENGE_CLEAR_PLAYER_FINAL>.Value, OnResponse_ClearCrossChallengeFinalsPlayerRank);
            AddResponser(Id<MSG_RZ_CROSS_CHALLENGE_UPDATE_PLAYER_FINAL>.Value, OnResponse_UpdateCrossChallengeFinalsPlayerRank);
            AddResponser(Id<MSG_RZ_CROSS_CHALLENGE_CLEAR_BATTLE_RANK>.Value, OnResponse_ClearCrossChallengeRanks);
            AddResponser(Id<MSG_RZ_CROSS_CHALLENGE_BATTLE_START>.Value, OnResponse_CrossChallengeStart);
            AddResponser(Id<MSG_RZ_GET_CROSS_CHALLENGE_BATTLE_START>.Value, OnResponse_GetCrossChallengeStart);
            AddResponser(Id<MSG_RZ_CROSS_CHALLENGE_SERVER_REWARD>.Value, OnResponse_CrossChallengeServerReward);
            AddResponser(Id<MSG_RZ_GET_CROSS_CHALLENGE_GUESSING_INFO>.Value, OnResponse_GetCrossChallengeGuessingPlayersInfo);
            AddResponser(Id<MSG_RZ_CROSS_CHALLENGE_GUESSING_CHOOSE>.Value, OnResponse_CrossChallengeGuessingChoose);
            AddResponser(Id<MSG_RZ_CROSS_CHALLENGE_OPEN_GUESSING_TEAM>.Value, OnResponse_CrossChallengeGuessingTeam);
            AddResponser(Id<MSG_RZ_CROSS_CHALLENGE_NOTICE_PLAYER_TEAM_ID>.Value, OnResponse_UpdateCrossChallengeTeamId);

            //仓库
            AddResponser(Id<MSG_RZ_ADD_WAREHOUSE_ITEMINFO>.Value, OnResponse_AddWarehouseItemInfo);
            AddResponser(Id<MSG_RZ_SPACETIME_MONSTER_INFO>.Value, OnResponse_SpacetimeMonsterInfo);
            //ResponserEnd
        }

        protected override void SendRegistSpecInfo()
        {
            AskForRankPeriodInfos();
            //AskForCampBuildInfo();
            AskForCampGrianInfo();
            //AskForCampBattleInfo();
            //Api.InitCampCoin();
        }


        public void OnResponse_AddNewEmailInfo(MemoryStream stream, int uid = 0)
        {
            // TODO 新协议
            MSG_RZ_SEND_EMAILS msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_SEND_EMAILS>(stream);
            Log.Write("add new email count:{0}", msg.Emails.Count);
            foreach (var email in msg.Emails)
            {
                switch ((EmailType)email.Type)
                {
                    case EmailType.System:
                        {
                            EmailInfo info = EmailLibrary.GetEmailInfo(email.Id);
                            if (info != null)
                            {
                                if (email.PcUids.Count > 0)
                                {
                                    foreach (int pcUid in email.PcUids)
                                    {
                                        PlayerChar player = Api.PCManager.FindPc(pcUid);
                                        if (player != null)
                                        {
                                            player.AddNewSystemEmail(info, email.SendTime, email.DeleteTime, email.IsGet);
                                            player.SyncNewEmail();
                                        }
                                        else
                                        {
                                            player = Api.PCManager.FindOfflinePc(pcUid);
                                            if (player != null)
                                            {
                                                player.AddNewSystemEmail(info, email.SendTime, email.DeleteTime, email.IsGet);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    //给所有人发
                                    foreach (var player in Api.PCManager.PcList)
                                    {
                                        player.Value.AddNewSystemEmail(info, email.SendTime, email.DeleteTime, email.IsGet);
                                        player.Value.SyncNewEmail();
                                    }
                                    foreach (var player in Api.PCManager.PcOfflineList)
                                    {
                                        player.Value.AddNewSystemEmail(info, email.SendTime, email.DeleteTime, email.IsGet);
                                    }
                                }
                            }
                            else
                            {
                                //error 未找到邮件
                                Log.Error("add new email type {0} not find email {1} info", email.Type, email.Id);
                            }
                        }
                        break;
                    case EmailType.Custom:
                        {
                            if (email.PcUids.Count > 0)
                            {
                                foreach (int pcUid in email.PcUids)
                                {
                                    PlayerChar player = Api.PCManager.FindPc(pcUid);
                                    if (player != null)
                                    {
                                        player.AddNewCustomEmail(email.Uid, email.Title, email.Body, email.FromName, email.Rewards, email.SendTime, email.DeleteTime, email.IsGet, email.Param);

                                        player.SyncNewEmail();
                                    }
                                    else
                                    {
                                        player = Api.PCManager.FindOfflinePc(pcUid);
                                        if (player != null)
                                        {
                                            player.AddNewCustomEmail(email.Uid, email.Title, email.Body, email.FromName, email.Rewards, email.SendTime, email.DeleteTime, email.IsGet, email.Param);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                //没有要发送的人
                                Log.Error("add new email type {0} not find email {1} pc list ", email.Type, email.Id);
                            }
                        }
                        break;
                    default:
                        //给指定人发
                        {
                            EmailInfo info = EmailLibrary.GetEmailInfo(email.Id);
                            if (info != null)
                            {
                                if (email.PcUids.Count > 0)
                                {
                                    foreach (int pcUid in email.PcUids)
                                    {
                                        PlayerChar player = Api.PCManager.FindPc(pcUid);
                                        if (player != null)
                                        {
                                            player.AddNewPersonEmail(info, email.Uid, email.Body, email.Rewards, email.SendTime, email.DeleteTime, email.IsGet, email.Param);

                                            player.SyncNewEmail();
                                        }
                                        else
                                        {
                                            player = Api.PCManager.FindOfflinePc(pcUid);
                                            if (player != null)
                                            {
                                                player.AddNewPersonEmail(info, email.Uid, email.Body, email.Rewards, email.SendTime, email.DeleteTime, email.IsGet, email.Param);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    //给所有人发
                                    foreach (var player in Api.PCManager.PcList)
                                    {
                                        player.Value.AddNewSystemEmail(info, email.SendTime, email.DeleteTime, email.IsGet);
                                        player.Value.SyncNewEmail();
                                    }
                                    foreach (var player in Api.PCManager.PcOfflineList)
                                    {
                                        player.Value.AddNewSystemEmail(info, email.SendTime, email.DeleteTime, email.IsGet);
                                    }
                                    ////没有要发送的人
                                    //Log.Error("add new email type {0} not find email {1} pc list ", email.Type, email.Id);
                                }
                            }
                            else
                            {
                                //error 未找到邮件
                                Log.Error("add new email type {0} not find email {1} info", email.Type, email.Id);
                            }
                        }
                        break;
                }
            }
        }


        public void OnResponse_DeleteAllEmail(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_DELETE_ALL_EMAI pks = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_DELETE_ALL_EMAI>(stream);
            Log.Write("delete email id:{0} send {1}  delete {2}", pks.EmailId, pks.SendTime, pks.DeleteTime);
            Api.PCManager.DeleteAllEmail(pks.EmailId, pks.SendTime, pks.DeleteTime);
        }

        public void OnResponse_SaveEmail(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_SAVE_EMAI msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_SAVE_EMAI>(stream);

            EmailInfo info = EmailLibrary.GetEmailInfo(msg.EmailId);
            if (info != null)
            {
                PlayerChar player = Api.PCManager.FindPc(msg.PcUid);
                if (player != null)
                {
                    player.SendPersonEmail(msg.EmailId, "", msg.Reward);

                    //player.ClearTempEmailId(msg.EmailType);
                }
                else
                {
                    PlayerChar playerOffline = Api.PCManager.FindOfflinePc(msg.PcUid);
                    if (playerOffline != null)
                    {
                        player.SendPersonEmail(msg.EmailId, "", msg.Reward);

                        //player.ClearTempEmailId(msg.EmailType);
                    }
                }
            }
        }


        public void OnResponse_NotifyPeeped(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_NOTIFY_PEEPED msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_NOTIFY_PEEPED>(stream);
            PlayerChar player = Api.PCManager.FindPc(msg.Uid);
            //if (player != null)
            //{
            //    PKS_ZC_NOTIFY_PEEPED notify = new PKS_ZC_NOTIFY_PEEPED();
            //    notify.name = msg.peeperName;
            //    player.Write(notify);
            //}
        }


        public void LoadBattlePlayerInfoWithQuerys(int getTypeId, int findUid, object msg, int uid, List<int> heroIds = null, PlayerChar fromPlayerChar = null)
        {
            ChallengeIntoType getType = (ChallengeIntoType)getTypeId;
            //通过DB获取player
            List<AbstractDBQuery> querys = new List<AbstractDBQuery>();

            //string baseTableName = "character";1
            QueryLoadBattlePlayerBasic queryBasic = new QueryLoadBattlePlayerBasic(findUid);
            querys.Add(queryBasic);

            string queryHeroSqsl = string.Empty; ;
            if (heroIds != null)
            {
                if (heroIds.Count > 0)
                {
                    queryHeroSqsl = GetHeroListLoadHeroSqlString(heroIds);
                }
            }
            else
            {
                queryHeroSqsl = GetLoadHeroSqlString(getType);
            }
            //string heroTableName = "hero";
            QueryLoadHero queryHero = new QueryLoadHero(findUid, queryHeroSqsl);
            querys.Add(queryHero);

            QueryLoadHeroPos queryHeroPos = new QueryLoadHeroPos(findUid);
            querys.Add(queryHeroPos);

            //string itemTableName = "items";7
            QueryLoadItem itemInfo = new QueryLoadItem(findUid, $" AND typeId in ({EquipLibrary.AllXuanyuString}) ");
            querys.Add(itemInfo);

            QueryLoadSoulBone querySoulBone = new QueryLoadSoulBone(findUid, " AND (equipHeroId>0)");
            querys.Add(querySoulBone);

            QueryLoadSoulRing querySoulRing = new QueryLoadSoulRing(findUid, " AND (equipHeroId>0)");
            querys.Add(querySoulRing);

            QueryLoadEquipment queryEquip = new QueryLoadEquipment(findUid, " AND (equipHeroId>0)");
            querys.Add(queryEquip);

            QueryLoadHiddenWeapon queryHiddenWeapon = new QueryLoadHiddenWeapon(findUid, "AND (equipHeroId>0)");
            querys.Add(queryHiddenWeapon);

            QueryLoadTravelHero queryTravelHero = new QueryLoadTravelHero(findUid);
            querys.Add(queryTravelHero);

            QueryLoadDraw queryDraw = new QueryLoadDraw(findUid);
            querys.Add(queryDraw);

            QueryLoadEquipmentSlot queryEquipSlot = new QueryLoadEquipmentSlot(findUid);
            querys.Add(queryEquipSlot);

            QueryLoadHeroGod queryLoadHeroGod = new QueryLoadHeroGod(findUid);
            querys.Add(queryLoadHeroGod);

            QueryLoadCampStars queryCampStars = new QueryLoadCampStars(findUid);
            querys.Add(queryCampStars);

            QueryLoadCrossBattle queryCrossBattle = new QueryLoadCrossBattle(findUid);
            querys.Add(queryCrossBattle);

            QueryLoadHunting queryHunting = new QueryLoadHunting(findUid);
            querys.Add(queryHunting);

            QueryLoadCrossChallenge queryCrossChallenge = new QueryLoadCrossChallenge(findUid);
            querys.Add(queryCrossChallenge);

            //QueryLoadArena queryArena = new QueryLoadArena(findUid);
            //querys.Add(queryArena);

            //string titleTableName = "title";
            QeuryGetTitles queryTitle = new QeuryGetTitles(findUid);
            querys.Add(queryTitle);

            QueryLoadPet queryPet = new QueryLoadPet(findUid);
            querys.Add(queryPet);

            QueryLoadMainBattleQueue queryMainBattleQueue = new QueryLoadMainBattleQueue(findUid);
            querys.Add(queryMainBattleQueue);

            QueryLoadPetDungeonQueue queryPetQueues = new QueryLoadPetDungeonQueue(findUid);
            querys.Add(queryPetQueues);

            DBQueryTransaction dBQuerys = new DBQueryTransaction(querys, true);
            api.GameDBPool.Call(dBQuerys, (DBCallback)(ret =>
            {
                if ((int)ret == 0)
                {
                    PlayerChar findPlayer = new PlayerChar(Api, findUid);
                    if (queryBasic.FindPlayer)
                    {
                        findPlayer.Name = queryBasic.CharName;
                        findPlayer.Sex = queryBasic.Sex;
                        findPlayer.Level = queryBasic.Level;
                        findPlayer.Icon = queryBasic.FaceIcon;
                        findPlayer.HeroId = queryBasic.HeroId;
                        findPlayer.GodType = queryBasic.GodType;
                        findPlayer.Camp = (CampType)queryBasic.Camp;
                        findPlayer.MainLineId = queryBasic.MainLineId;
                        findPlayer.MainTaskId = queryBasic.MainTaskId;
                        findPlayer.BranchTaskIds = queryBasic.BranchTaskIds;
                        findPlayer.ResonanceLevel = queryBasic.ResonanceLevel;
                        findPlayer.BagSpace = queryBasic.BagSpace;
                        //伙伴列表
                        findPlayer.InitHero(queryHero.HeroList);
                        //养成
                        findPlayer.BagManager.NormalBag.LoadItems(itemInfo.List);
                        findPlayer.BagManager.SoulRingBag.LoadItems(querySoulRing.List);
                        findPlayer.BagManager.EquipBag.LoadItems(queryEquip.List);
                        findPlayer.BagManager.SoulBoneBag.LoadItems(querySoulBone.List);
                        findPlayer.BagManager.HiddenWeaponBag.LoadItems(queryHiddenWeapon.WeaponList);
                        //装备升级
                        findPlayer.EquipmentManager.LoadSlot(queryEquipSlot.hero_part_slot);
                        //阵营养成信息
                        findPlayer.DragonLevel = queryCampStars.DragonLevel;
                        findPlayer.TigerLevel = queryCampStars.TigerLevel;
                        findPlayer.PhoenixLevel = queryCampStars.PhoenixLevel;
                        findPlayer.TortoiseLevel = queryCampStars.TortoiseLevel;
                        //漫游记
                        findPlayer.InitTravelManager(queryTravelHero.heroList);
                        //羁绊
                        findPlayer.HeroMng.InitCombo(queryDraw.HeroCombo);
                        //成神
                        findPlayer.InitHeroGodInfo(queryLoadHeroGod.HeroGodList);
                        //上阵信息
                        findPlayer.HeroMng.InitHeroPos(queryHeroPos.List);

                        //findPlayer.ArenaMng.Init(queryArena.Info);
                        //跨服战
                        findPlayer.CrossInfoMng.Init(queryCrossBattle.Info);
                        // 跨服挑战
                        findPlayer.CrossChallengeInfoMng.Init(queryCrossChallenge.Info);
                        //狩猎
                        findPlayer.HuntingManager.BindHuntingInfo(queryHunting.Research, queryHunting.HuntingInfo, queryHunting.ActivityUnlock, queryHunting.ActivityPassed);
                        //称号
                        findPlayer.InitTitleInfo(queryTitle.TitleList);
                        //宠物
                        findPlayer.InitPets(queryPet.PetList);
                        //主战阵容
                        findPlayer.HeroMng.InitMainBattleQueue(queryMainBattleQueue.InfoList);
                        //宠物副本阵容
                        findPlayer.InitPetDungeonQueues(queryPetQueues.DungeonQueues);

                        //一开始初始化FSM会报错
                        findPlayer.InitFSMAfterHero();
                        //初始化伙伴属性
                        findPlayer.BindHerosNature();
                        //初始化宠物属性
                        findPlayer.BindPetsNature();
                    }
                    else
                    {
                        // 未找到该角色
                        Log.Warn("player {0} LoadBattlePlayerInfoWithQuerys load  failed: not find {1}", uid, findUid);
                        return;
                    }

                    switch (getType)
                    {
                        case ChallengeIntoType.CrossFinalsPlayer1:
                            {
                                MSG_RZ_GET_BATTLE_PLAYER msgInfo = msg as MSG_RZ_GET_BATTLE_PLAYER;

                                //获取到玩家1 信息，返回到Relation
                                MSG_ZRZ_RETURN_BATTLE_PLAYER_INFO addMsg = new MSG_ZRZ_RETURN_BATTLE_PLAYER_INFO();
                                addMsg.GetType = (int)ChallengeIntoType.CrossFinalsPlayer2;
                                addMsg.Player1 = findPlayer.GetBattlePlayerInfoMsg();
                                addMsg.Player1.Index = msgInfo.Player1.Index;

                                addMsg.Player2 = new ZR_BattlePlayerMsg();
                                addMsg.Player2.Uid = msgInfo.Player2.Uid;
                                addMsg.Player2.MainId = msgInfo.Player2.MainId;
                                addMsg.Player2.Index = msgInfo.Player2.Index;

                                addMsg.TimingId = msgInfo.TimingId;
                                addMsg.GroupId = msgInfo.GroupId;
                                addMsg.TeamId = msgInfo.TeamId;
                                addMsg.FightId = msgInfo.FightId;
                                Write(addMsg, msgInfo.Player2.Uid);
                            }
                            break;
                        case ChallengeIntoType.CrossFinalsPlayer2:
                            {
                                MSG_ZRZ_RETURN_BATTLE_PLAYER_INFO msgInfo = msg as MSG_ZRZ_RETURN_BATTLE_PLAYER_INFO;
                                PlayerCrossFightInfo fightInfo = GetCrossRobotInfo(msgInfo.Player1);
                                if (fightInfo != null)
                                {
                                    fightInfo.Type = ChallengeIntoType.CrossFinals;
                                    fightInfo.TimingId = msgInfo.TimingId;
                                    fightInfo.GroupId = msgInfo.GroupId;
                                    fightInfo.TeamId = msgInfo.TeamId;
                                    fightInfo.FightId = msgInfo.FightId;
                                    fightInfo.HeroIndex[msgInfo.Player1.Uid] = msgInfo.Player1.Index;
                                    fightInfo.HeroIndex[msgInfo.Player2.Uid] = msgInfo.Player2.Index;
                                    findPlayer.EnterCrossBattleMap(fightInfo);
                                }
                            }
                            break;
                        case ChallengeIntoType.CrossHeroInfo:
                            {
                                MSG_RZ_GET_BATTLE_HEROS msgInfo = msg as MSG_RZ_GET_BATTLE_HEROS;
                                findPlayer.SyncCrossHeroQueueMsg(msgInfo.SeeUid, msgInfo.SeeMainId);
                            }
                            break;
                        case ChallengeIntoType.CrossPreliminary:
                            {
                                MSG_ZRZ_GET_BATTLE_CHALLENGER_INFO msgInfo = msg as MSG_ZRZ_GET_BATTLE_CHALLENGER_INFO;
                                PlayerCrossFightInfo fightInfo = findPlayer.GetCrossRobotInfo();
                                if (fightInfo != null)
                                {
                                    PlayerChar player = Api.PCManager.FindPc(uid);
                                    if (player == null)
                                    {
                                        player = Api.PCManager.FindOfflinePc(uid);
                                        if (player == null)
                                        {
                                            Log.Warn("player {0} not find return cross challenger from relation find show player {1} failed: not find ", uid, msgInfo.ChallengerUid);
                                            return;
                                        }
                                    }
                                    fightInfo.Type = ChallengeIntoType.CrossPreliminary;
                                    player.EnterCrossBattleMap(fightInfo);


                                    //发送回Relation
                                    MSG_ZR_ADD_BATTLE_CHALLENGER_INFO addMsg = new MSG_ZR_ADD_BATTLE_CHALLENGER_INFO();
                                    addMsg.Info1 = findPlayer.GetBattlePlayerInfoMsg();
                                    addMsg.Info2 = player.GetBattlePlayerInfoMsg();
                                    Api.RelationServer.Write(addMsg);
                                }
                            }
                            break;
                        case ChallengeIntoType.CampDefender:
                            {
                                MSG_RZ_CAMP_CREATE_DUNGEON msgInfo = msg as MSG_RZ_CAMP_CREATE_DUNGEON;

                                PlayerCampFightInfo fightInfo = findPlayer.GetCampFightInfo();
                                if (fightInfo != null)
                                {
                                    PlayerChar player = Api.PCManager.FindPc(uid);
                                    if (player == null)
                                    {
                                        player = Api.PCManager.FindOfflinePc(uid);
                                        if (player == null)
                                        {
                                            //Log.Warn("player {0} not find return cross challenger from relation find show player {1} failed: not find ", uid, msgInfo.ChallengerUid);
                                            return;
                                        }
                                    }
                                    fightInfo.Camp = msgInfo.Camp;
                                    fightInfo.FortId = msgInfo.FortId;
                                    fightInfo.DungeonIndex = msgInfo.DungeonIndex;
                                    fightInfo.DungeonId = msgInfo.DungeonId;
                                    fightInfo.InspireCamp = msgInfo.InspireCamp;
                                    fightInfo.InspireDValue = msgInfo.InspireDValue;
                                    fightInfo.FortCamp = msgInfo.FortCamp;
                                    fightInfo.DefenderUid = msgInfo.DefenderUid;
                                    foreach (var item in msgInfo.AddNature)
                                    {
                                        fightInfo.AddNature[item.Key] = item.Value;
                                    }
                         
                                    player.EnterCampMap(fightInfo);
                                }
                            }
                            break;
                        case ChallengeIntoType.TeamOffline:
                            {
                                CallOfflineBrother2Map(fromPlayerChar, findPlayer, msg as TeamDungeonMap, queryHunting.Research);
                            }
                            break;
                        case ChallengeIntoType.ShowFind:
                            {
                                List<int> heroIdList = findPlayer.HeroMng.GetAllHeroPosHeroId();
                                //使用player 获取查看信息
                                MSG_ZGC_SHOW_PLAYER response = findPlayer.GetShowPlayerMsg(heroIdList);

                                MSG_RZ_ONE_SERVER_FIND_SHOW_PLAYER msgInfo = msg as MSG_RZ_ONE_SERVER_FIND_SHOW_PLAYER;

                                MSG_ZRZ_RETURN_PLAYER_SHOW returnMsg = new MSG_ZRZ_RETURN_PLAYER_SHOW();
                                returnMsg.PcUid = uid;
                                returnMsg.ShowPcUid = findUid;
                                returnMsg.Result = (int)ErrorCode.Success;
                                returnMsg.ShowInfo = response;
                                returnMsg.SeeMainId = msgInfo.SeeMainId;
                                Write(returnMsg, uid);
                            }
                            break;
                        case ChallengeIntoType.ShowNotFind:
                            {
                                List<int> heroIdList = findPlayer.HeroMng.GetAllHeroPosHeroId();
                                //使用player 获取查看信息
                                MSG_ZGC_SHOW_PLAYER response = findPlayer.GetShowPlayerMsg(heroIdList);

                                PlayerChar player = Api.PCManager.FindPc(uid);
                                if (player == null)
                                {
                                    Log.Warn("player {0} not find show player from relation find show player {1} failed: not find ", uid, findUid);
                                    return;
                                }
                                player.SendPlayerInfoMsg(response);

                                //发送给Relation 缓存信息
                                MSG_ZR_ADD_PLAYER_SHOW addMsg = new MSG_ZR_ADD_PLAYER_SHOW();
                                addMsg.Info = new MSG_ZRZ_RETURN_PLAYER_SHOW();
                                addMsg.Info.Result = (int)ErrorCode.Success;
                                addMsg.Info.PcUid = uid;
                                addMsg.Info.ShowPcUid = findUid;
                                addMsg.Info.ShowInfo = response;
                                Write(addMsg, uid);
                            }
                            break;
                        case ChallengeIntoType.Arena:
                            {
                                PlayerChar player = Api.PCManager.FindPc(uid);
                                if (player == null)
                                {
                                    Log.Warn("player {0} not find show arena challenger from relation find show player {1} failed: not find ", uid, findUid);
                                    return;
                                }

                                PlayerRankBaseInfo rankInfo = player.ArenaMng.GetArenaRankInfo(findUid);
                                if (rankInfo == null)
                                {
                                    Log.WarnLine("player {0} not find  show arena challenger info failed: not find rank info index {1}", uid, findUid);
                                    return;
                                }

                                MSG_RZ_NOT_FIND_ARENA_CHALLENGER msgInfo = msg as MSG_RZ_NOT_FIND_ARENA_CHALLENGER;
                                for (int i = 0; i < msgInfo.ChallengerDefensive.Count; i++)
                                {
                                    if (msgInfo.ChallengerDefensive[i] > 0)
                                    {
                                        if (msgInfo.CDefPoses.Count > 1)
                                        {
                                            findPlayer.ArenaMng.AddDefensiveHero(msgInfo.ChallengerDefensive[i], msgInfo.CDefPoses[i]);
                                        }
                                        else
                                        {
                                            findPlayer.ArenaMng.AddDefensiveHero(msgInfo.ChallengerDefensive[i], i + 1);
                                        }
                                    }
                                }

                                MSG_ZGC_ARENA_CHALLENGER_HERO_INFO response = findPlayer.GetChallengerMsg();
                                response.Info = findPlayer.GetArenaRankBaseInfo(rankInfo);
                                //缓存信息
                                foreach (var item in response.HeroList)
                                {
                                    RobotHeroInfo robotInfo = PlayerChar.GetRobotHeroInfo(item);
                                    rankInfo.HeroInfos.Add(robotInfo);
                                }
                                rankInfo.PetInfo = PlayerChar.GetRobotPetInfo(response.Pet);

                                rankInfo.NatureValues = new Dictionary<int, int>(response.NatureValues);
                                rankInfo.NatureRatios = new Dictionary<int, int>(response.NatureRatios);

                                //使用player 获取查看信息
                                player.Write(response);

                                //发送给Relation 缓存信息
                                MSG_ZR_ADD_ARENA_CHALLENGER_HERO_INFO addMsg = new MSG_ZR_ADD_ARENA_CHALLENGER_HERO_INFO();
                                addMsg.PcUid = uid;
                                addMsg.ChallengerUid = findUid;
                                addMsg.Info = response;
                                Write(addMsg, uid);
                            }
                            break;
                        case ChallengeIntoType.Versus:
                            {
                                PlayerChar player = Api.PCManager.FindPc(uid);
                                if (player == null)
                                {
                                    Log.Warn("player {0} not find show arena challenger from relation find show player {1} failed: not find ", uid, findUid);
                                    return;
                                }
                                PlayerRankBaseInfo rankInfo = findPlayer.GetChallengerRankBaseInfo();
                                List<HeroInfo> heroInfos = findPlayer.HeroMng.GetEquipHeros().Values.ToList();
                                findPlayer.SetHeroInfoRobotSoulRings(heroInfos);
                                PetInfo petInfo = findPlayer.PetManager.GetPetInfo(findPlayer.PetManager.OnFightPet);
                                player.EnterVersusMap(rankInfo, heroInfos, petInfo);
                            }
                            break;
                        case ChallengeIntoType.CrossBoss:
                            {
                                MSG_ZRZ_GET_BOSS_PLAYER_INFO msgInfo = msg as MSG_ZRZ_GET_BOSS_PLAYER_INFO;
                                SyncCrosHeroQueuMsg(msgInfo, findPlayer, (int)ChallengeIntoType.CrossBossReturn);
                            }
                            break;
                        case ChallengeIntoType.CrossBossSite:
                            {
                                MSG_ZRZ_GET_BOSS_PLAYER_INFO msgInfo = msg as MSG_ZRZ_GET_BOSS_PLAYER_INFO;
                                SyncCrosHeroQueuMsg(msgInfo, findPlayer, (int)ChallengeIntoType.CrossBossSiteReturn);
                            }
                            break;
                        case ChallengeIntoType.CrossBossSiteFight:
                            {
                                MSG_ZRZ_GET_BOSS_PLAYER_INFO msgInfo = msg as MSG_ZRZ_GET_BOSS_PLAYER_INFO;
                                SyncCrosHeroQueuMsg(msgInfo, findPlayer, (int)ChallengeIntoType.CrossBossSiteFightReturn);
                            }
                            break;
                        case ChallengeIntoType.CrossChallengeFinalsPlayer1:
                            {
                                MSG_RZ_GET_CROSS_CHALLENGE_PLAYER msgInfo = msg as MSG_RZ_GET_CROSS_CHALLENGE_PLAYER;

                                //获取到玩家1 信息，返回到Relation
                                MSG_ZRZ_RETURN_CROSS_CHALLENGE_BATTLE_PLAYER_INFO addMsg = new MSG_ZRZ_RETURN_CROSS_CHALLENGE_BATTLE_PLAYER_INFO();
                                addMsg.GetType = (int)ChallengeIntoType.CrossChallengeFinalsPlayer2;
                                addMsg.Player1 = findPlayer.GetBattleChallengePlayerInfoMsg();
                                addMsg.Player1.Index = msgInfo.Player1.Index;

                                addMsg.Player2 = new ZR_BattlePlayerMsg();
                                addMsg.Player2.Uid = msgInfo.Player2.Uid;
                                addMsg.Player2.MainId = msgInfo.Player2.MainId;
                                addMsg.Player2.Index = msgInfo.Player2.Index;

                                addMsg.TimingId = msgInfo.TimingId;
                                addMsg.GroupId = msgInfo.GroupId;
                                addMsg.TeamId = msgInfo.TeamId;
                                addMsg.FightId = msgInfo.FightId;
                                Write(addMsg, msgInfo.Player2.Uid);
                            }
                            break;
                        case ChallengeIntoType.CrossChallengeFinalsPlayer2:
                            {
                                MSG_ZRZ_RETURN_CROSS_CHALLENGE_BATTLE_PLAYER_INFO msgInfo = msg as MSG_ZRZ_RETURN_CROSS_CHALLENGE_BATTLE_PLAYER_INFO;
                                PlayerCrossFightInfo fightInfo = GetCrossRobotInfo(msgInfo.Player1);
                                if (fightInfo != null)
                                {
                                    fightInfo.Type = ChallengeIntoType.CrossChallengeFinals;
                                    fightInfo.TimingId = msgInfo.TimingId;
                                    fightInfo.GroupId = msgInfo.GroupId;
                                    fightInfo.TeamId = msgInfo.TeamId;
                                    fightInfo.FightId = msgInfo.FightId;
                                    fightInfo.HeroIndex[msgInfo.Player1.Uid] = msgInfo.Player1.Index;
                                    fightInfo.HeroIndex[msgInfo.Player2.Uid] = msgInfo.Player2.Index;
                                    findPlayer.EnterCrossChallengeMap(fightInfo);
                                }
                            }
                            break;
                        case ChallengeIntoType.CrossChallengeHeroInfo:
                            {
                                MSG_RZ_GET_CROSS_CHALLENGE_HEROS msgInfo = msg as MSG_RZ_GET_CROSS_CHALLENGE_HEROS;
                                findPlayer.SyncCrossChallengeHeroQueueMsg(msgInfo.SeeUid, msgInfo.SeeMainId);
                            }
                            break;
                        case ChallengeIntoType.CrossChallengePreliminary:
                            {
                                MSG_ZRZ_GET_CROSS_CHALLENGE_CHALLENGER_INFO msgInfo = msg as MSG_ZRZ_GET_CROSS_CHALLENGE_CHALLENGER_INFO;
                                PlayerCrossFightInfo fightInfo = findPlayer.GetCrossChallengeRobotInfo();
                                if (fightInfo != null)
                                {
                                    PlayerChar player = Api.PCManager.FindPc(uid);
                                    if (player == null)
                                    {
                                        player = Api.PCManager.FindOfflinePc(uid);
                                        if (player == null)
                                        {
                                            Log.Warn("player {0} not find return cross challenger from relation find show player {1} failed: not find ", uid, msgInfo.ChallengerUid);
                                            return;
                                        }
                                    }

                                    fightInfo.Type = ChallengeIntoType.CrossChallengePreliminary;
                                    player.EnterCrossChallengeMap(fightInfo);

                                    //发送回Relation
                                    MSG_ZR_ADD_CROSS_CHALLENGE_CHALLENGER_INFO addMsg = new MSG_ZR_ADD_CROSS_CHALLENGE_CHALLENGER_INFO();
                                    addMsg.Info1 = findPlayer.GetBattleChallengePlayerInfoMsg();
                                    addMsg.Info2 = player.GetBattleChallengePlayerInfoMsg();
                                    Api.RelationServer.Write(addMsg);
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    // 未找到该角色
                    Log.Warn("player {0} type {1} challenger info load  failed", findUid, getType);
                    return;
                }
            }));
        }

        private static string GetHeroListLoadHeroSqlString(List<int> heroIds)
        {
            string queryHeroSqsl;
            string ids = string.Empty;
            foreach (var item in heroIds)
            {
                ids += "," + item;
            }
            ids = ids.Substring(1);
            queryHeroSqsl = " AND  hero_id in (" + ids + ")";
            return queryHeroSqsl;
        }

        private static string GetLoadHeroSqlString(ChallengeIntoType getType)
        {
            string queryHeroSqsl = "";
            switch (getType)
            {
                case ChallengeIntoType.CrossPreliminary:
                case ChallengeIntoType.CrossFinals:
                case ChallengeIntoType.CrossFinalsTeturn:
                case ChallengeIntoType.CrossFinalsPlayer1:
                case ChallengeIntoType.CrossFinalsPlayer2:
                case ChallengeIntoType.CrossHeroInfo:
                    queryHeroSqsl = " AND (cross_queue_num>0 or cross_position_num>0)";
                    break;
                case ChallengeIntoType.CampDefender:
                    break;
                case ChallengeIntoType.CrossChallengePreliminary:
                case ChallengeIntoType.CrossChallengeFinals:
                case ChallengeIntoType.CrossChallengeFinalsTeturn:
                case ChallengeIntoType.CrossChallengeFinalsPlayer1:
                case ChallengeIntoType.CrossChallengeFinalsPlayer2:
                case ChallengeIntoType.CrossChallengeHeroInfo:
                    queryHeroSqsl = " AND (cross_challenge_queue_num>0 or cross_challenge_position_num>0)";
                    break;
                default:
                    break;
            }

            return queryHeroSqsl;
        }

        //private void LoadChallengerWithQuerys(int challengerUid)
        //{
        //    //通过DB获取player
        //    List<AbstractDBQuery> querys = new List<AbstractDBQuery>();

        //    //string baseTableName = "character";1
        //    QueryLoadPlayerBasic queryBasic = new QueryLoadPlayerBasic(challengerUid);
        //    querys.Add(queryBasic);

        //    //string heroTableName = "hero";
        //    QueryLoadHero queryHero = new QueryLoadHero(challengerUid);
        //    querys.Add(queryHero);

        //    QueryLoadHeroPos queryHeroPos = new QueryLoadHeroPos(challengerUid);
        //    querys.Add(queryHeroPos);

        //    QueryLoadDraw queryDraw = new QueryLoadDraw(challengerUid);
        //    querys.Add(queryDraw);

        //    //string itemTableName = "items";7
        //    QueryLoadItem itemInfo = new QueryLoadItem(challengerUid, $" AND typeId in ({EquipLibrary.AllXuanyuString}) ");
        //    querys.Add(itemInfo);

        //    QueryLoadSoulBone querySoulBone = new QueryLoadSoulBone(challengerUid, " AND (equipHeroId>0)");
        //    querys.Add(querySoulBone);

        //    QueryLoadSoulRing querySoulRing = new QueryLoadSoulRing(challengerUid, " AND (equipHeroId>0)");
        //    querys.Add(querySoulRing);

        //    QueryLoadEquipment queryEquip = new QueryLoadEquipment(challengerUid, " AND (equipHeroId>0)");
        //    querys.Add(queryEquip);

        //    QueryLoadEquipmentSlot queryEquipSlot = new QueryLoadEquipmentSlot(challengerUid);
        //    querys.Add(queryEquipSlot);

        //    QueryLoadArena queryArena = new QueryLoadArena(challengerUid);
        //    querys.Add(queryArena);

        //    QueryLoadHeroGod queryLoadHeroGod = new QueryLoadHeroGod(challengerUid);
        //    querys.Add(queryLoadHeroGod);

        //    DBQueryTransaction dBQuerysWithoutTransaction = new DBQueryTransaction(querys, true);
        //    server.GameDBPool.Call(dBQuerysWithoutTransaction, (DBCallback)(ret =>
        //    {
        //        if ((int)ret == 0)
        //        {
        //            PlayerChar challenger = new PlayerChar(server, challengerUid);

        //            challenger.Name = queryBasic.CharName;
        //            challenger.Sex = queryBasic.Sex;
        //            challenger.Level = queryBasic.Level;
        //            challenger.FollowerId = queryBasic.Follower;
        //            challenger.HeroId = queryBasic.HeroId;
        //            challenger.GodType = queryBasic.GodType;
        //            challenger.TimeCreated = queryBasic.TimeCreated;
        //            challenger.GuideId = queryBasic.GuideId;
        //            challenger.MainTaskId = queryBasic.MainTaskId;
        //            challenger.BranchTaskIds.AddRange(queryBasic.BranchTaskIds);
        //            challenger.AccountName = queryBasic.AccountName;
        //            challenger.MainId = queryBasic.MainId;
        //            //challenger.Icon = queryBasic.FaceIcon;
        //            challenger.Icon = challenger.HeroId;
        //            challenger.ShowDIYIcon = queryBasic.ShowFaceJpg;
        //            challenger.MainLineId = queryBasic.MainLineId;
        //            challenger.Job = (JobType)queryBasic.Job;
        //            challenger.BagSpace = queryBasic.BagSpace;
        //            challenger.Camp = (CampType)queryBasic.Camp;

        //            // step 12
        //            challenger.InitHero(queryHero.HeroList);

        //            //step6 各种背包的数据需要风别加载
        //            challenger.BagManager.NormalBag.LoadItems(itemInfo.List);
        //            //showPlayer.BagManager.FashionBag.LoadItems(fashionsInfo.List);
        //            //showPlayer.BagManager.FaceFrameBag.LoadItems(faceFrameInfo.List);
        //            //showPlayer.BagManager.ChatFrameBag.LoadItems(chatFrameInfo.List);
        //            challenger.BagManager.SoulRingBag.LoadItems(querySoulRing.List);
        //            challenger.BagManager.EquipBag.LoadItems(queryEquip.List);
        //            challenger.BagManager.SoulBoneBag.LoadItems(querySoulBone.List);
        //            challenger.EquipmentManager.LoadSlot(queryEquipSlot.hero_part_slot);
        //            //羁绊
        //            challenger.HeroMng.InitCombo(queryDraw.HeroCombo);

        //            challenger.ArenaMng.Init(queryArena.Info);

        //            //上阵信息
        //            challenger.HeroMng.InitHeroPos(queryHeroPos.List);
        //            //成神
        //            challenger.InitHeroGodInfo(queryLoadHeroGod.HeroGodList);

        //            PlayerRankBaseInfo rankInfo = challenger.GetChallengerRankBaseInfo();
        //            List<HeroInfo> heroInfos = challenger.HeroMng.GetEquipHeros().Values.ToList();
        //            challenger.SetHeroInfoRobotSoulRings(heroInfos);

        //            //初始化伙伴属性
        //            challenger.BindHerosNature();

        //            EnterVersusMap(rankInfo, heroInfos);
        //        }
        //        else
        //        {
        //            // 未找到该角色
        //            Log.Warn("player {0} arena challenger info load  failed", challengerUid);
        //            return;
        //        }
        //    }));
        //}


        //private void LoadChallengerWithQuerys(int playerUid, int challengerUid, RepeatedField<int> challengerDefensive, int getType, RepeatedField<int> cDefPoses)
        //{
        //    //通过DB获取player
        //    List<AbstractDBQuery> querys = new List<AbstractDBQuery>();

        //    //string baseTableName = "character";1
        //    QueryLoadPlayerBasic queryBasic = new QueryLoadPlayerBasic(challengerUid);
        //    querys.Add(queryBasic);

        //    //string heroTableName = "hero";
        //    QueryLoadHero queryHero = new QueryLoadHero(challengerUid);
        //    querys.Add(queryHero);

        //    QueryLoadDraw queryDraw = new QueryLoadDraw(challengerUid);
        //    querys.Add(queryDraw);

        //    //string itemTableName = "items";7
        //    QueryLoadItem itemInfo = new QueryLoadItem(challengerUid, $" AND typeId in ({EquipLibrary.AllXuanyuString}) ");
        //    querys.Add(itemInfo);

        //    ////string itemTableName = "fashions";12
        //    //QueryLoadFashion fashionsInfo = new QueryLoadFashion(player.Uid);
        //    //querys.Add(fashionsInfo);

        //    //////string itemTableName = "faceframes";13
        //    //QueryLoadFaceFrame faceFrameInfo = new QueryLoadFaceFrame(player.Uid);
        //    //querys.Add(faceFrameInfo);

        //    //////string itemTableName = "chatframes";14
        //    //QueryLoadChatFrame chatFrameInfo = new QueryLoadChatFrame(player.Uid);
        //    //querys.Add(chatFrameInfo);

        //    QueryLoadSoulBone querySoulBone = new QueryLoadSoulBone(challengerUid, " AND (equipHeroId>0)");
        //    querys.Add(querySoulBone);

        //    QueryLoadSoulRing querySoulRing = new QueryLoadSoulRing(challengerUid, " AND (equipHeroId>0)");
        //    querys.Add(querySoulRing);

        //    QueryLoadEquipment queryEquip = new QueryLoadEquipment(challengerUid, " AND (equipHeroId>0)");
        //    querys.Add(queryEquip);

        //    QueryLoadEquipmentSlot queryEquipSlot = new QueryLoadEquipmentSlot(challengerUid);
        //    querys.Add(queryEquipSlot);

        //    QueryLoadHeroGod queryLoadHeroGod = new QueryLoadHeroGod(challengerUid);
        //    querys.Add(queryLoadHeroGod);

        //    //string titleTableName = "title";
        //    //QeuryGetTitles queryTitle = new QeuryGetTitles(player.Uid, titleTableName);
        //    //querys.Add(queryTitle);


        //    DBQueryTransaction dBQuerysWithoutTransaction = new DBQueryTransaction(querys, true);
        //    api.GameDBPool.Call(dBQuerysWithoutTransaction, (DBCallback)(ret =>
        //    {
        //        if ((int)ret == 0)
        //        {
        //            PlayerChar challenger = new PlayerChar(Api, challengerUid);

        //            challenger.Name = queryBasic.CharName;
        //            challenger.Sex = queryBasic.Sex;
        //            challenger.Level = queryBasic.Level;
        //            challenger.FollowerId = queryBasic.Follower;
        //            challenger.HeroId = queryBasic.HeroId;
        //            challenger.GodType = queryBasic.GodType;
        //            challenger.TimeCreated = queryBasic.TimeCreated;
        //            challenger.GuideId = queryBasic.GuideId;
        //            challenger.MainTaskId = queryBasic.MainTaskId;
        //            challenger.BranchTaskIds.AddRange(queryBasic.BranchTaskIds);
        //            challenger.AccountName = queryBasic.AccountName;
        //            challenger.MainId = queryBasic.MainId;
        //            //challenger.Icon = queryBasic.FaceIcon;
        //            challenger.Icon = challenger.HeroId;
        //            challenger.ShowDIYIcon = queryBasic.ShowFaceJpg;
        //            challenger.MainLineId = queryBasic.MainLineId;
        //            challenger.Job = (JobType)queryBasic.Job;
        //            challenger.BagSpace = queryBasic.BagSpace;
        //            challenger.Camp = (CampType)queryBasic.Camp;


        //            // step 12
        //            challenger.InitHero(queryHero.HeroList);

        //            //step6 各种背包的数据需要风别加载
        //            challenger.BagManager.NormalBag.LoadItems(itemInfo.List);
        //            //showPlayer.BagManager.FashionBag.LoadItems(fashionsInfo.List);
        //            //showPlayer.BagManager.FaceFrameBag.LoadItems(faceFrameInfo.List);
        //            //showPlayer.BagManager.ChatFrameBag.LoadItems(chatFrameInfo.List);
        //            challenger.BagManager.SoulRingBag.LoadItems(querySoulRing.List);
        //            challenger.BagManager.EquipBag.LoadItems(queryEquip.List);
        //            challenger.BagManager.SoulBoneBag.LoadItems(querySoulBone.List);

        //            challenger.EquipmentManager.LoadSlot(queryEquipSlot.hero_part_slot);

        //            //羁绊
        //            challenger.HeroMng.InitCombo(queryDraw.HeroCombo);


        //            challenger.InitHeroGodInfo(queryLoadHeroGod.HeroGodList);

        //            //初始化伙伴属性
        //            challenger.BindHerosNature();

        //            for (int i = 0; i < challengerDefensive.Count; i++)
        //            {
        //                if (challengerDefensive[i] > 0)
        //                {
        //                    if (cDefPoses.Count > 1)
        //                    {
        //                        challenger.ArenaMng.AddDefensiveHero(challengerDefensive[i], cDefPoses[i]);
        //                    }
        //                    else
        //                    {
        //                        challenger.ArenaMng.AddDefensiveHero(challengerDefensive[i], i + 1);
        //                    }
        //                }
        //            }

        //            switch ((ChallengeIntoType)getType)
        //            {
        //                case ChallengeIntoType.Arena:
        //                    {
        //                        PlayerChar player = Api.PCManager.FindPc(playerUid);
        //                        if (player == null)
        //                        {
        //                            Log.Warn("player {0} not find show arena challenger from relation find show player {1} failed: not find ", playerUid, challengerUid);
        //                            return;
        //                        }

        //                        PlayerRankBaseInfo rankInfo = player.ArenaMng.GetArenaRankInfo(challengerUid);
        //                        if (rankInfo == null)
        //                        {
        //                            Log.WarnLine("player {0} not find  show arena challenger info failed: not find rank info index {1}", playerUid, challengerUid);
        //                            return;
        //                        }

        //                        MSG_ZGC_ARENA_CHALLENGER_HERO_INFO response = challenger.GetChallengerMsg();
        //                        response.Info = challenger.GetArenaRankBaseInfo(rankInfo);
        //                        //缓存信息
        //                        foreach (var item in response.HeroList)
        //                        {
        //                            RobotHeroInfo robotInfo = PlayerChar.GetRobotHeroInfo(item);
        //                            rankInfo.HeroInfos.Add(robotInfo);
        //                        }

        //                        //使用player 获取查看信息
        //                        player.Write(response);

        //                        //发送给Relation 缓存信息
        //                        MSG_ZR_ADD_ARENA_CHALLENGER_HERO_INFO addMsg = new MSG_ZR_ADD_ARENA_CHALLENGER_HERO_INFO();
        //                        addMsg.PcUid = playerUid;
        //                        addMsg.ChallengerUid = challengerUid;
        //                        addMsg.Info = response;
        //                        Write(addMsg, playerUid);
        //                    }
        //                    break;
        //                default:
        //                    break;
        //            }


        //        }
        //        else
        //        {
        //            // 未找到该角色
        //            Log.Warn("player {0} arena challenger info load  failed", challengerUid);
        //            return;
        //        }
        //    }));
        //}

        //private void LoadPlayerWithQuerys(int playerUid, int showPlayerUid, bool sendInfo)
        //{
        //    //通过DB获取player
        //    List<AbstractDBQuery> querys = new List<AbstractDBQuery>();

        //    //string baseTableName = "character";1
        //    QueryLoadPlayerBasic queryBasic = new QueryLoadPlayerBasic(showPlayerUid);
        //    querys.Add(queryBasic);

        //    //string heroTableName = "hero";
        //    QueryLoadHero queryHero = new QueryLoadHero(showPlayerUid);
        //    querys.Add(queryHero);

        //    //string itemTableName = "items";7
        //    QueryLoadItem itemInfo = new QueryLoadItem(showPlayerUid, $" AND typeId in ({EquipLibrary.AllXuanyuString}) ");
        //    querys.Add(itemInfo);

        //    QueryLoadSoulBone querySoulBone = new QueryLoadSoulBone(showPlayerUid, " AND (equipHeroId>0)");
        //    querys.Add(querySoulBone);

        //    QueryLoadSoulRing querySoulRing = new QueryLoadSoulRing(showPlayerUid, " AND (equipHeroId>0)");
        //    querys.Add(querySoulRing);

        //    QueryLoadEquipment queryEquip = new QueryLoadEquipment(showPlayerUid, " AND (equipHeroId>0)");
        //    querys.Add(queryEquip);

        //    QueryLoadEquipmentSlot queryEquipSlot = new QueryLoadEquipmentSlot(showPlayerUid);
        //    querys.Add(queryEquipSlot);

        //    QueryLoadHeroPos queryHeroPos = new QueryLoadHeroPos(showPlayerUid);
        //    querys.Add(queryHeroPos);

        //    QueryLoadHeroGod queryLoadHeroGod = new QueryLoadHeroGod(showPlayerUid);
        //    querys.Add(queryLoadHeroGod);

        //    QueryLoadDraw queryDraw = new QueryLoadDraw(showPlayerUid);
        //    querys.Add(queryDraw);


        //    //string titleTableName = "title";
        //    //QeuryGetTitles queryTitle = new QeuryGetTitles(player.Uid, titleTableName);
        //    //querys.Add(queryTitle);


        //    DBQueryTransaction dBQuerysWithoutTransaction = new DBQueryTransaction(querys, true);
        //    api.GameDBPool.Call(dBQuerysWithoutTransaction, ret =>
        //    {
        //        if ((int)ret == 0)
        //        {

        //            PlayerChar showPlayer = new PlayerChar(Api, showPlayerUid);

        //            showPlayer.Name = queryBasic.CharName;
        //            showPlayer.Sex = queryBasic.Sex;
        //            showPlayer.Level = queryBasic.Level;
        //            showPlayer.FollowerId = queryBasic.Follower;
        //            showPlayer.HeroId = queryBasic.HeroId;
        //            showPlayer.GodType = queryBasic.GodType;
        //            showPlayer.TimeCreated = queryBasic.TimeCreated;
        //            showPlayer.GuideId = queryBasic.GuideId;
        //            showPlayer.MainTaskId = queryBasic.MainTaskId;
        //            showPlayer.BranchTaskIds.AddRange(queryBasic.BranchTaskIds);
        //            showPlayer.AccountName = queryBasic.AccountName;
        //            showPlayer.MainId = queryBasic.MainId;
        //            //showPlayer.Icon = queryBasic.FaceIcon;
        //            showPlayer.Icon = showPlayer.HeroId;

        //            showPlayer.ShowDIYIcon = queryBasic.ShowFaceJpg;
        //            showPlayer.MainLineId = queryBasic.MainLineId;
        //            showPlayer.Job = (JobType)queryBasic.Job;
        //            showPlayer.BagSpace = queryBasic.BagSpace;
        //            showPlayer.Camp = (CampType)queryBasic.Camp;
        //            showPlayer.ResonanceLevel = queryBasic.ResonanceLevel;

        //            // step 12
        //            showPlayer.InitHero(queryHero.HeroList);

        //            //step6 各种背包的数据需要风别加载
        //            showPlayer.BagManager.NormalBag.LoadItems(itemInfo.List);
        //            showPlayer.BagManager.SoulRingBag.LoadItems(querySoulRing.List);
        //            showPlayer.BagManager.EquipBag.LoadItems(queryEquip.List);
        //            showPlayer.BagManager.SoulBoneBag.LoadItems(querySoulBone.List);

        //            showPlayer.EquipmentManager.LoadSlot(queryEquipSlot.hero_part_slot);



        //            showPlayer.HeroMng.InitHeroPos(queryHeroPos.List);
        //            List<int> heroIdList = showPlayer.HeroMng.GetAllHeroPosHeroId();

        //            //羁绊
        //            showPlayer.HeroMng.InitCombo(queryDraw.HeroCombo);

        //            showPlayer.InitHeroGodInfo(queryLoadHeroGod.HeroGodList);

        //            //初始化伙伴属性
        //            showPlayer.BindHerosNature();

        //            //使用player 获取查看信息
        //            MSG_ZGC_SHOW_PLAYER response = showPlayer.GetShowPlayerMsg(heroIdList);

        //            if (sendInfo)
        //            {
        //                PlayerChar player = Api.PCManager.FindPc(playerUid);
        //                if (player == null)
        //                {
        //                    Log.Warn("player {0} not find show player from relation find show player {1} failed: not find ", playerUid, showPlayerUid);
        //                    return;
        //                }
        //                player.Write(response);


        //                //发送给Relation 缓存信息
        //                MSG_ZR_ADD_PLAYER_SHOW addMsg = new MSG_ZR_ADD_PLAYER_SHOW();
        //                addMsg.Info = new MSG_ZRZ_RETURN_PLAYER_SHOW();
        //                addMsg.Info.Result = (int)ErrorCode.Success;
        //                addMsg.Info.PcUid = playerUid;
        //                addMsg.Info.ShowPcUid = showPlayerUid;
        //                addMsg.Info.ShowInfo = response;
        //                Write(addMsg, playerUid);
        //            }
        //            else
        //            {
        //                MSG_ZRZ_RETURN_PLAYER_SHOW returnMsg = new MSG_ZRZ_RETURN_PLAYER_SHOW();
        //                returnMsg.PcUid = playerUid;
        //                returnMsg.ShowPcUid = showPlayerUid;
        //                returnMsg.Result = (int)ErrorCode.Success;
        //                returnMsg.ShowInfo = response;
        //                Write(returnMsg, playerUid);
        //            }
        //        }
        //        else
        //        {
        //            // 未找到该角色
        //            Log.Warn("player {0} show info load  failed", showPlayerUid);
        //            return;
        //        }
        //    });
        //}

        //private void LoadOfflineBrotherAndAddForTeam(FieldMap dungeon, int brotherUid)
        //{
        //    //通过DB获取player
        //    List<AbstractDBQuery> querys = new List<AbstractDBQuery>();

        //    //string baseTableName = "character";1
        //    QueryLoadPlayerBasic queryBasic = new QueryLoadPlayerBasic(brotherUid);
        //    querys.Add(queryBasic);

        //    //string heroTableName = "hero";
        //    QueryLoadHero queryHero = new QueryLoadHero(brotherUid);
        //    querys.Add(queryHero);

        //    QueryLoadHeroPos queryHeroPos = new QueryLoadHeroPos(brotherUid);
        //    querys.Add(queryHeroPos);

        //    QueryLoadDraw queryDraw = new QueryLoadDraw(brotherUid);
        //    querys.Add(queryDraw);

        //    //string itemTableName = "items";7
        //    QueryLoadItem itemInfo = new QueryLoadItem(brotherUid, $" AND typeId in ({EquipLibrary.AllXuanyuString}) ");
        //    querys.Add(itemInfo);

        //    ////string itemTableName = "fashions";12
        //    //QueryLoadFashion fashionsInfo = new QueryLoadFashion(player.Uid);
        //    //querys.Add(fashionsInfo);

        //    //////string itemTableName = "faceframes";13
        //    //QueryLoadFaceFrame faceFrameInfo = new QueryLoadFaceFrame(player.Uid);
        //    //querys.Add(faceFrameInfo);

        //    //////string itemTableName = "chatframes";14
        //    //QueryLoadChatFrame chatFrameInfo = new QueryLoadChatFrame(player.Uid);
        //    //querys.Add(chatFrameInfo);

        //    QueryLoadSoulBone querySoulBone = new QueryLoadSoulBone(brotherUid, " AND (equipHeroId>0)");
        //    querys.Add(querySoulBone);

        //    QueryLoadSoulRing querySoulRing = new QueryLoadSoulRing(brotherUid, " AND (equipHeroId>0)");
        //    querys.Add(querySoulRing);

        //    QueryLoadEquipment queryEquip = new QueryLoadEquipment(brotherUid, " AND (equipHeroId>0)");
        //    querys.Add(queryEquip);

        //    QueryLoadEquipmentSlot queryEquipSlot = new QueryLoadEquipmentSlot(brotherUid);
        //    querys.Add(queryEquipSlot);

        //    //string titleTableName = "title";
        //    //QeuryGetTitles queryTitle = new QeuryGetTitles(player.Uid, titleTableName);
        //    //querys.Add(queryTitle);

        //    QueryLoadHunting queryHunting = new QueryLoadHunting(brotherUid);
        //    querys.Add(queryHunting);

        //    QueryLoadHeroGod queryLoadHeroGod = new QueryLoadHeroGod(brotherUid);
        //    querys.Add(queryLoadHeroGod);


        //    DBQueryTransaction dBQuerysWithoutTransaction = new DBQueryTransaction(querys, true);
        //    api.GameDBPool.Call(dBQuerysWithoutTransaction, (DBCallback)(ret =>
        //    {
        //        if ((int)ret == 0)
        //        {
        //            PlayerChar brother = new PlayerChar(Api, brotherUid);

        //            brother.Name = queryBasic.CharName;
        //            brother.Sex = queryBasic.Sex;
        //            brother.Level = queryBasic.Level;
        //            brother.FollowerId = queryBasic.Follower;
        //            brother.HeroId = queryBasic.HeroId;
        //            brother.GodType = queryBasic.GodType;
        //            brother.TimeCreated = queryBasic.TimeCreated;
        //            brother.GuideId = queryBasic.GuideId;
        //            brother.MainTaskId = queryBasic.MainTaskId;
        //            brother.BranchTaskIds.AddRange(queryBasic.BranchTaskIds);
        //            brother.AccountName = queryBasic.AccountName;
        //            brother.MainId = queryBasic.MainId;
        //            //challenger.Icon = queryBasic.FaceIcon;
        //            brother.Icon = brother.HeroId;
        //            brother.ShowDIYIcon = queryBasic.ShowFaceJpg;
        //            brother.MainLineId = queryBasic.MainLineId;
        //            brother.Job = (JobType)queryBasic.Job;
        //            brother.BagSpace = queryBasic.BagSpace;
        //            brother.Camp = (CampType)queryBasic.Camp;

        //            // step 12
        //            brother.InitHero(queryHero.HeroList);
        //            //step6 各种背包的数据需要风别加载
        //            brother.BagManager.NormalBag.LoadItems(itemInfo.List);
        //            //showPlayer.BagManager.FashionBag.LoadItems(fashionsInfo.List);
        //            //showPlayer.BagManager.FaceFrameBag.LoadItems(faceFrameInfo.List);
        //            //showPlayer.BagManager.ChatFrameBag.LoadItems(chatFrameInfo.List);
        //            brother.BagManager.SoulRingBag.LoadItems(querySoulRing.List);
        //            brother.BagManager.EquipBag.LoadItems(queryEquip.List);
        //            brother.BagManager.SoulBoneBag.LoadItems(querySoulBone.List);

        //            brother.EquipmentManager.LoadSlot(queryEquipSlot.hero_part_slot);

        //            //羁绊
        //            brother.HeroMng.InitCombo(queryDraw.HeroCombo);
        //            //初始化伙伴属性
        //            brother.BindHerosNature();
        //            //上阵信息
        //            brother.HeroMng.InitHeroPos(queryHeroPos.List);

        //            brother.HuntingManager.BindHuntingInfo(queryHunting.Research, queryHunting.HuntingInfo, queryHunting.ActivityUnlock, queryHunting.ActivityPassed);

        //            brother.InitHeroGodInfo(queryLoadHeroGod.HeroGodList);


        //            List<HeroInfo> heroInfos = brother.HeroMng.GetEquipHeros().Values.ToList();
        //            brother.SetHeroInfoRobotSoulRings(heroInfos);
        //            Dictionary<int, int> heroPos = new Dictionary<int, int>();
        //            foreach (var item in brother.HeroMng.GetHeroPos())
        //            {
        //                heroPos.Add(item.Key, item.Value);
        //            }

        //            TeamDungeonMap teamDungeon = dungeon as TeamDungeonMap;
        //            if (teamDungeon != null)
        //            {
        //                brother.SetCurrentMap(teamDungeon);
        //                teamDungeon.AddAttackerMirror(brother, heroPos);
        //                //teamDungeon.AddAttackerTeamRobot(heroInfos, brotherUid, heroPos);
        //                (teamDungeon as HuntingTeamDungeonMap)?.SetOfflineBrother(brother, queryHunting.Research);
        //            }
        //        }
        //    }));
        //}
    }
}
