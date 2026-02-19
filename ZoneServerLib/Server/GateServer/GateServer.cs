using Logger;
using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using Message.IdGenerator;
using Message.Shared.Protocol.Shared;
using ServerFrame;
using System;
using System.Collections.Generic;
using System.IO;
using MSG_GateZ_DOMAIN_BENEDICTION_DRAW_OPERATION = Message.Gate.Protocol.GateZ.MSG_GateZ_DOMAIN_BENEDICTION_DRAW_OPERATION;

namespace ZoneServerLib
{
    public partial class GateServer : FrontendServer
    {
        private ZoneServerApi Api
        { get { return (ZoneServerApi)api; } }

        public GateServer(BaseApi api)
            : base(api)
        {
        }

        protected override void BindResponser()
        {
            base.BindResponser();
            //AddResponser(Id<MSG_RM_MARRIAGE_DUNGEON>.Value, OnResponse_MarriageDungeon);
            AddResponser(Id<MSG_GateZ_EnterWorld>.Value, OnResponse_EnterWorld);
            AddResponser(Id<MSG_GateZ_LeaveWorld>.Value, OnResponse_LeaveWorld);
            AddResponser(Id<MSG_GateZ_MAP_LOADING_DONE>.Value, OnResponse_MapLoadingDone);
            AddResponser(Id<MSG_GateZ_MOVE_ZONE>.Value, OnResponse_MoveZone);
            AddResponser(Id<MSG_GateZ_CHANGE_CHANNEL>.Value, OnResponse_ChangeChannel);
            AddResponser(Id<MSG_GateZ_RECONNECT_LOGIN>.Value, OnResponse_ReconnectLogin);
            AddResponser(Id<MSG_GateZ_CHARACTER_MOVE>.Value, OnResponse_CharacterMove);
            AddResponser(Id<MSG_GateZ_AUTOPATHFINDING>.Value, OnResponse_AutoPathFinding);
            AddResponser(Id<MSG_GateZ_CROSS_PORTAL>.Value, OnResponse_CrossPortal);
            AddResponser(Id<MSG_GateZ_SUGGEST>.Value, OnResponse_Suggest);
            AddResponser(Id<MSG_GateZ_LOGOUT>.Value, OnResponse_LogOut);
            AddResponser(Id<MSG_GateZ_LOGIN_GET_SOFTWARES>.Value, OnResponse_LoginGetSoftwares);
            AddResponser(Id<MSG_GateZ_SAVE_GUIDE_ID>.Value, OnResponse_SaveGuideId);
            AddResponser(Id<MSG_GateZ_SAVE_MAIN_LINE_ID>.Value, OnResponse_SaveMainLineId);

            ////geography
            //AddResponser(Id<MSG_GateZ_GEOGRAPHY>.Value, OnResponse_Geography);

            //Chat
            AddResponser(Id<MSG_GateZ_CHAT>.Value, OnResponse_Chat);
            AddResponser(Id<MSG_GateZ_USE_CHAT_TRUMPET>.Value, OnResponse_ChatTrumpet);
            AddResponser(Id<MSG_GateZ_NEARBY_EMOJI>.Value, OnResponse_NearbyEmoji);
            AddResponser(Id<MSG_GateZ_TIP_OFF>.Value, OnResponse_TipOff);
            AddResponser(Id<MSG_GateZ_ACTIVITY_CHAT_BUBBLE>.Value, OnResponse_ActivityChatBubble);
            AddResponser(Id<MSG_GateZ_CHECK_CHATLIMIT>.Value, OnResponse_CheckChatLimit);
            AddResponser(Id<MSG_GateZ_BUY_TRUMPET>.Value, OnResponse_BuyChatTrumpet);
            AddResponser(Id<MSG_GateZ_CLEAR_BUBBLE_REDPOINT>.Value, OnResponse_ClearBubbleRedPoint);

            //Bag
            AddResponser(Id<MSG_GateZ_ITEM_USE>.Value, OnResponse_ItemUse);
            AddResponser(Id<MSG_GateZ_ITEM_USE_BATCH>.Value, OnResponse_ItemUseBatch);
            AddResponser(Id<MSG_GateZ_ITEM_SELL>.Value, OnResponse_ItemSell);
            AddResponser(Id<MSG_GateZ_ITEM_BUY>.Value, OnResponse_ItemBuy);
            AddResponser(Id<MSG_GateZ_USE_FIREWORKS>.Value, OnResponse_UseFireworks);
            AddResponser(Id<MSG_GateZ_ITEM_COMPOSE>.Value, OnResponse_ItemCompose);
            AddResponser(Id<MSG_GateZ_ITEM_FORGE>.Value, OnResponse_ItemForge);
            AddResponser(Id<MSG_GateZ_ITEM_RESOLVE>.Value, OnResponse_ItemResolve);
            AddResponser(Id<MSG_GateZ_BAGSPACEINC>.Value, OnResponse_BagSpaceInc);
            AddResponser(Id<MSG_GateZ_ITEM_BATCH_RESOLVE>.Value, OnResponse_ItemBatchResolve);
            AddResponser(Id<MSG_GateZ_RECEIVE_ITEM>.Value, OnResponse_ReceiveItem);
            AddResponser(Id<MSG_GateZ_ITEM_EXCHANGE_REWARD>.Value, OnResponse_ItemExchangeReward);
            AddResponser(Id<MSG_GateZ_OPEN_CHOOSE_BOX>.Value, OnResponse_OpenChooseBox);

            //soulbone
            AddResponser(Id<MSG_GateZ_SMELT_SOULBONE>.Value, OnResponse_SmeltSoulBones);
            AddResponser(Id<MSG_GateZ_EQUIP_SOULBONE>.Value, OnResponse_EquipSoulBone);
            AddResponser(Id<MSG_GateZ_SOULBONE_QUENCHING>.Value, OnResponse_SoulBoneQuenching);

            //equipment
            AddResponser(Id<MSG_GateZ_EQUIP_EQUIPMENT>.Value, OnResponse_EquipEquipment);
            AddResponser(Id<MSG_GateZ_UPGRADE_EQUIPMENT>.Value, OnResponse_UpgradeSlot);
            AddResponser(Id<MSG_GateZ_UPGRADE_EQUIPMENT_DIRECTLY>.Value, OnResponse_UpgradeSlotDirectly);
            AddResponser(Id<MSG_GateZ_INJECTION_EQUIPMENT>.Value, OnResponse_InjectEquipment);
            AddResponser(Id<MSG_GateZ_EQUIP_BETTER_EQUIPMENT>.Value, OnResponse_EquipBetterEquipment);
            AddResponser(Id<MSG_GateZ_RETURN_UPGRADE_EQUIPMENT_COST>.Value, OnResponse_ReturnUpgradeCost);
            AddResponser(Id<MSG_GateZ_EQUIPMENT_ADVANCE>.Value, OnResponse_EquipmentAdvance);
            AddResponser(Id<MSG_GateZ_EQUIPMENT_ADVANCE_ONE_KEY>.Value, OnResponse_EquipmentAdvanceOneKey);
            AddResponser(Id<MSG_GateZ_JEWEL_ADVANCE>.Value, OnResponse_JewelAdvance);
            AddResponser(Id<MSG_GateZ_EQUIPMENT_ENCHANT>.Value, OnResponse_EquipmentEnchant);

            //soulring 
            AddResponser(Id<MSG_GateZ_ABSORB_SOULRING>.Value, OnResponse_AbsorbSoulRing);
            AddResponser(Id<MSG_GateZ_HELP_ABSORB_SOULRING>.Value, OnResponse_HelpAbsorbSoulRing);
            AddResponser(Id<MSG_GateZ_GET_ABSORBINFO>.Value, OnResponse_GetAbsorbInfo);
            AddResponser(Id<MSG_GateZ_CANCEL_ABSORB>.Value, OnResponse_CancelAbsorb);
            AddResponser(Id<MSG_GateZ_ABSORB_FINISH>.Value, OnResponse_FinishAbsorb);
            AddResponser(Id<MSG_GateZ_GET_HELP_THANKS_LIST>.Value, OnResponse_GetHelpThanksList);
            AddResponser(Id<MSG_GateZ_THANK_FRIEND>.Value, OnResponse_ThankFriend);
            AddResponser(Id<MSG_GateZ_ENHANCE_SOULRING>.Value, OnResponse_EnhanceSoulRing);
            AddResponser(Id<MSG_GateZ_ONEKEY_ENHANCE_SOULRING>.Value, OnResponse_OneKeyEnhanceSoulRing);
            AddResponser(Id<MSG_GateZ_GET_All_ABSORBINFO>.Value, OnResponse_GetAllAbsorbInfo);
            AddResponser(Id<MSG_GateZ_GET_FRIEND_INFO>.Value, OnResponse_GetAbsorbFriendInfo);
            AddResponser(Id<MSG_GateZ_SHOW_HERO_SOULRING>.Value, OnResponse_ShowHeroSoulRing);
            AddResponser(Id<MSG_GateZ_REPLACE_BETTER_SOULRING>.Value, OnResponse_ReplaceBetterSoulRing);
            AddResponser(Id<MSG_GateZ_SELECT_SOULRING_ELEMENT>.Value, OnResponse_SelectSoulRingElement);


            //Camp
            AddResponser(Id<MSG_GateZ_CHOOSE_CAMP>.Value, OnResponse_ChooseCamp);
            AddResponser(Id<MSG_GateZ_WORSHIP>.Value, OnResponse_Worship);
            AddResponser(Id<MSG_GateZ_VOTE>.Value, OnResponse_CampElect);
            AddResponser(Id<MSG_GateZ_RUN_IN_ELECTION>.Value, OnResponse_RunInElection);
            AddResponser(Id<MSG_GateZ_SHOW_CAMP_PANEL_INFO>.Value, OnResponse_ShowCampPanelInfo);
            AddResponser(Id<MSG_GateZ_GET_CAMP_REWARD>.Value, OnResponse_GetCampRankReward);
            AddResponser(Id<MSG_GateZ_SHOW_CAMP_INFO>.Value, OnResponse_ShowCampInfos);
            AddResponser(Id<MSG_GateZ_SHOW_CAMP_ELECTION_INFO>.Value, OnResponse_ShowCampElectionInfos);
            AddResponser(Id<MSG_GateZ_GET_STARLEVEL>.Value, OnResponse_GetStarLevel);
            AddResponser(Id<MSG_GateZ_STAR_LEVELUP>.Value, OnResponse_CampStarLevelUp);
            AddResponser(Id<MSG_GateZ_CAMP_GATHER>.Value, OnResponse_CampGather);
            AddResponser(Id<MSG_GateZ_GATHER_DIALOGUE_COMPLETE>.Value, OnResponse_GatherDialogueComplete);

            //IslandHighManager
            AddResponser(Id<MSG_GateZ_TASK_COMPLETE>.Value, OnResponse_TaskComplete);
            AddResponser(Id<MSG_GateZ_OPENE_EMAIL_TASK>.Value, OnResponse_OpenEmailTask);
            AddResponser(Id<MSG_GateZ_TASK_COLLECT>.Value, OnResponse_TaskCollect);
            AddResponser(Id<MSG_GateZ_TASK_SELECT>.Value, OnResponse_TaskSelect);
            AddResponser(Id<MSG_GateZ_TASK_MAKE>.Value, OnResponse_TaskMake);
            AddResponser(Id<MSG_GateZ_TASKFLY_FLY_DONE>.Value, OnResponse_TaskFlyDone);
            AddResponser(Id<MSG_GateZ_TASKFLY_STARTPATHFINDING>.Value, OnResponse_TaskFlyPathFinding);
            AddResponser(Id<MSG_GateZ_TASK_FINISH_STATE_REWARD>.Value, OnResponse_GetTaskFinishReward);

            //Email
            AddResponser(Id<MSG_GZ_EMAIL_OPENE_BOX>.Value, OnResponse_OpenMailbox);
            AddResponser(Id<MSG_GZ_EMAIL_READ>.Value, OnResponse_ReadMail);
            AddResponser(Id<MSG_GZ_PICKUP_ATTACHMENT>.Value, OnResponse_GetAttachment);

            //Friend
            AddResponser(Id<MSG_GateZ_FRIEND_RECOMMEND>.Value, OnResponse_RecommendFriend);
            AddResponser(Id<MSG_GateZ_FRIEND_SEARCH>.Value, OnResponse_FriendSearch);

            AddResponser(Id<MSG_GateZ_FRIEND_ADD>.Value, OnResponse_FriendAdd);
            AddResponser(Id<MSG_GateZ_FRIEND_DELETE>.Value, OnResponse_FriendDelete);
            AddResponser(Id<MSG_GateZ_FRIEND_LIST>.Value, OnResponse_FriendList);

            AddResponser(Id<MSG_GateZ_FRIEND_BLACK_ADD>.Value, OnResponse_BlackAdd);
            AddResponser(Id<MSG_GateZ_FRIEND_BLACK_DEL>.Value, OnResponse_BlackDel);
            AddResponser(Id<MSG_GateZ_FRIEND_BLACK_LIST>.Value, OnResponse_FriendBlackList);

            AddResponser(Id<MSG_GateZ_FRIEND_HEART_GIVE>.Value, OnResponse_FriendHeartGive);
            AddResponser(Id<MSG_GateZ_FRIEND_HEART_COUNT_BUY>.Value, OnResponse_FriendHeartCountBuy);
            AddResponser(Id<MSG_GateZ_REPAY_FRIENDS_HEART>.Value, OnResponse_RepayFriendsHeart);

            AddResponser(Id<MSG_GateZ_FRIEND_RECENT_LIST>.Value, OnResponse_FriendRecentList);

            ////Photo
            //AddResponser(Id<MSG_GateZ_UPLOAD_PHOTO>.Value, OnResponse_UploadPhoto);
            //AddResponser(Id<MSG_GateZ_REMOVE_PHOTO>.Value, OnResponse_RemovePhoto);
            //AddResponser(Id<MSG_GateZ_PHOTO_LIST>.Value, OnResponse_PhotoList);
            //AddResponser(Id<MSG_GateZ_POP_RANK>.Value, OnResponse_PopRank);

            //Show
            AddResponser(Id<MSG_GateZ_SHOW_PLAYER>.Value, OnResponse_ShowPlayer);
            //AddResponser(Id<MSG_GateZ_SHOW_FACEICON>.Value, OnResponse_ShowFaceIcon);
            //AddResponser(Id<MSG_GateZ_SHOW_FACEJPG>.Value, OnResponse_ShowFaceJpg);
            AddResponser(Id<MSG_GateZ_CHANGE_NAME>.Value, OnResponse_ChangeName);
            //AddResponser(Id<MSG_GateZ_SET_SEX>.Value, OnResponse_SetSex);
            //AddResponser(Id<MSG_GateZ_SET_BIRTHDAY>.Value, OnResponse_SetBirthday);
            //AddResponser(Id<MSG_GateZ_SET_SIGNATURE>.Value, OnResponse_SetSignature);
            //AddResponser(Id<MSG_GateZ_SET_SOCIAL_NUM>.Value, OnResponse_SetWQ);
            //AddResponser(Id<MSG_GateZ_GET_SOCIAL_NUM>.Value, OnResponse_GetWQ);
            //AddResponser(Id<MSG_GateZ_SHOW_VOICE>.Value, OnResponse_ShowVoice);
            //AddResponser(Id<MSG_GateZ_PRESENT_GIFT>.Value, OnResponse_PresentGift);
            //AddResponser(Id<MSG_GateZ_GET_GIFTRECORD>.Value, OnResponse_GetGiftRecord);

            //AddResponser(Id<MSG_GateZ_SHOW_CAREER>.Value, OnResponse_ShowCareer);
            //AddResponser(Id<MSG_GateZ_GET_RANKING_FRIEND_LIST>.Value, OnResponse_GetRankingFriendList);
            //AddResponser(Id<MSG_GateZ_GET_RANKING_ALL_LIST>.Value, OnResponse_GetRankingAllList);
            AddResponser(Id<MSG_GateZ_CHANGE_TITLE>.Value, OnResponse_ChangeTitle);
            AddResponser(Id<MSG_GateZ_TITLE_CONDITION_COUNT>.Value, OnResponse_GetTitleConditionCount);
            AddResponser(Id<MSG_GateZ_LOOK_TITLE>.Value, OnResponse_LookTitle);

            //Gamecomment
            AddResponser(Id<MSG_GateZ_SAVE_GAME_COMMENT>.Value, OnResponse_CommentGame);

            //Recharge
            //AddResponser(Id<MSG_GateZ_SAVE_ORDER>.Value, OnResponse_SaveOrderId);
            AddResponser(Id<MSG_GateZ_GET_RECHARGE_HISTORY>.Value, OnResponse_ReadRechargeHistory);
            //AddResponser(Id<MSG_GateZ_DELETE_RECHARGE_HISTORY>.Value, OnResponse_DeleteOrderId);
            AddResponser(Id<MSG_GateZ_DEBUG_RECHARGE>.Value, OnResponse_DebugRecharge);
            AddResponser(Id<MSG_GateZ_GET_ORDER_ID>.Value, OnResponse_GetOrderId);
            AddResponser(Id<MSG_GateZ_BUY_RECHARGE_GIFT>.Value, OnResponse_BuyRechargeGift);
            AddResponser(Id<MSG_GateZ_RECEIVE_RECHARGE_REWARD>.Value, OnResponse_ReceiveRechargeReward);
            AddResponser(Id<MSG_GateZ_USE_RECHARGE_TOKEN>.Value, OnResponse_UseRechargeToken);
            AddResponser(Id<MSG_GateZ_OPEN_RECHARGE_GIFT>.Value, OnResponse_OpenRechargeGift);
            AddResponser(Id<MSG_GateZ_BUY_CULTIVATE_GIFT>.Value, OnResponse_BuyCultivateGift);
            AddResponser(Id<MSG_GateZ_FREE_PETTY_GIFT>.Value, OnResponse_ReceiveFreePettyGift);
            AddResponser(Id<MSG_GateZ_GET_DAILY_RECHARGE_REWARD>.Value, OnResponse_GetDailyRechargeReward);
            AddResponser(Id<MSG_GateZ_GET_HERO_DAYS_REWARD>.Value, OnResponse_GetHeroDaysReward);
            AddResponser(Id<MSG_GateZ_GET_ACCUMULATE_RECHARGE_REWARD>.Value, OnResponse_GetAccumulateRechargeReward);
            AddResponser(Id<MSG_GateZ_GET_NEWSERVER_PROMOTION_REWARD>.Value, OnResponse_GetNewServerPromotionReward);
            AddResponser(Id<MSG_GateZ_GET_LUCKY_FLIP_CARD_REWARD>.Value, OnResponse_GetLuckyFlipCardReward);
            AddResponser(Id<MSG_GateZ_GET_LUCKY_FLIP_CARD_CUMULATE_REWARD>.Value, OnResponse_GetLuckyFlipCardCumulateReward);
            AddResponser(Id<MSG_GateZ_GET_NEW_RECHARGE_GIFT_REWARD>.Value, OnResponse_GetNewRechargeGiftAccumulateReward);
            AddResponser(Id<MSG_GateZ_GET_TREASURE_FLIP_CARD_REWARD>.Value, OnResponse_GetTreasureFlipCardReward);
            AddResponser(Id<MSG_GateZ_GET_TREASURE_FLIP_CARD_CUMULATE_REWARD>.Value, OnResponse_GetTreasureFlipCardCumulateReward);
            AddResponser(Id<MSG_GateZ_FLIP_CARD_RECHARGE_GIFT>.Value, OnResponse_FlipCardRechargeGift);

            //Guild
            AddResponser(Id<MSG_GateZ_CREATE_GUILD>.Value, OnResponse_CreateGuild);

            //Activity
            AddResponser(Id<MSG_GateZ_ACTIVITY_COMPLETE>.Value, OnResponse_ActivityComplete);
            AddResponser(Id<MSG_GateZ_ACTIVITY_TYPE_COMPLETE>.Value, OnResponse_ActivityTypeComplete);
            AddResponser(Id<MSG_GateZ_ACTIVITY_RELATED_COMPLETE>.Value, OnResponse_ActivityRelatedComplete);
            AddResponser(Id<MSG_GateZ_RECHARGE_REBATE_GET_REWARD>.Value, OnResponse_RechargeRebateReward);
            AddResponser(Id<MSG_GateZ_SPECIAL_ACTIVITY_COMPLETE>.Value, OnResponse_SpceilActivityComplete);
            AddResponser(Id<MSG_GateZ_RUNAWAY_ACTIVITY_COMPLETE>.Value, OnResponse_RunawayActivityComplete);
            AddResponser(Id<MSG_GateZ_GET_WEBPAY_REBATE_REWARD>.Value, OnResponse_GetWebPayRebateReward);

            //Questionnaire
            AddResponser(Id<MSG_GateZ_QUESTIONNAIRE_COMPLETE>.Value, OnResponse_QuestionnaireComplete);
            //DailyQuestion
            AddResponser(Id<MSG_GateZ_DAILY_QUESTION_COUNTER>.Value, OnResponse_DailyQuestionCounter);
            AddResponser(Id<MSG_GateZ_DAILY_QUESTION_REWARD>.Value, OnResponse_DailyQuestionReward);
            //Radio
            AddResponser(Id<MSG_GateZ_RADIO_GET_ALL_ANCHOR_RANK>.Value, OnResponse_GetRedioAllAnchorRank);
            AddResponser(Id<MSG_GateZ_RADIO_GET_ANCHOR_CONTRIBUTION_RANK>.Value, OnResponse_GetRedioAnchorContributionRank);
            AddResponser(Id<MSG_GateZ_RADIO_GET_ALL_CONTRIBUTION_RANK>.Value, OnResponse_GetRedioAllContributionRank);
            AddResponser(Id<MSG_GateZ_RADIO_GET_CONTRIBUTION_REWARD>.Value, OnResponse_GetRedioContributionReward);
            AddResponser(Id<MSG_GateZ_RADIO_GIVE_GIFT>.Value, OnResponse_RedioGiveGift);

            //Pet
            AddResponser(Id<MSG_GateZ_CALL_PET>.Value, OnResponse_CallPet);
            AddResponser(Id<MSG_GateZ_RECALL_PET>.Value, OnResponse_RecallPet);
            AddResponser(Id<MSG_GateZ_HATCH_PET_EGG>.Value, OnResponse_HatchPetEgg);
            AddResponser(Id<MSG_GateZ_FINISH_HATCH_PET_EGG>.Value, OnResponse_FinishHatchPetEgg);
            AddResponser(Id<MSG_GateZ_RELEASE_PET>.Value, OnResponse_ReleasePet);
            AddResponser(Id<MSG_GateZ_SHOW_PET_NATURE>.Value, OnResponse_ShowPetNature);
            AddResponser(Id<MSG_GateZ_PET_LEVEL_UP>.Value, OnResponse_PetLevelUp);
            AddResponser(Id<MSG_GateZ_UPDATE_MAINQUEUE_PET>.Value, OnResponse_UpdateMainQueuePet);
            AddResponser(Id<MSG_GateZ_PET_INHERIT>.Value, OnResponse_PetInherit);
            AddResponser(Id<MSG_GateZ_PET_SKILL_BAPTIZE>.Value, OnResponse_PetSkillBaptize);
            AddResponser(Id<MSG_GateZ_PET_BREAK>.Value, OnResponse_PetBreak);
            AddResponser(Id<MSG_GateZ_ONE_KEY_PET_BREAK>.Value, OnResponse_OneKeyPetBreak);
            AddResponser(Id<MSG_GateZ_PET_BLEND>.Value, OnResponse_PetBlend);
            AddResponser(Id<MSG_GateZ_PET_FEED>.Value, OnResponse_PetFeed);
            AddResponser(Id<MSG_GateZ_UPDATE_PET_DUNGEON_QUEUE>.Value, OnResponse_UpdatePetDungeonQueue);

            //Hero
            AddResponser(Id<MSG_GateZ_HERO_LEVEL_UP>.Value, OnResponse_HeroLevelUp);
            AddResponser(Id<MSG_GateZ_HERO_AWAKEN>.Value, OnResponse_HeroAwaken);
            AddResponser(Id<MSG_GateZ_HERO_TITLE_UP>.Value, OnResponse_HeroTitleUp);
            AddResponser(Id<MSG_GateZ_HERO_CLICK_TALENT>.Value, OnResponse_HeroClickTalent);
            AddResponser(Id<MSG_GateZ_HERO_RESET_TALENT>.Value, OnResponse_HeroResetTalent);
            AddResponser(Id<MSG_GateZ_CALL_HERO>.Value, OnResponse_CallHero);
            AddResponser(Id<MSG_GateZ_RECALL_HERO>.Value, OnResponse_RecallHero);
            AddResponser(Id<MSG_GateZ_HERO_CHANGE_FOLLOWER>.Value, OnResponse_ChangeFollower);
            AddResponser(Id<MSG_GateZ_EQUIP_HERO>.Value, OnResponse_EquipHero);
            AddResponser(Id<MSG_GateZ_HERO_STEPS_UP>.Value, OnResponse_HeroStepsUp);
            AddResponser(Id<MSG_GateZ_ONEKEY_HERO_STEPS_UP>.Value, OnResponse_OnekeyHeroStepsUp);
            AddResponser(Id<MSG_GateZ_HERO_REVERT>.Value, OnResponse_HeroRevert);
            AddResponser(Id<MSG_GateZ_MAIN_HERO_CHANGE>.Value, OnResponse_ChangeMainHero);
            AddResponser(Id<MSG_GateZ_UPDATE_HERO_POS>.Value, OnResponse_UpdateHeroPos);
            AddResponser(Id<MSG_GateZ_HERO_GOD_UNLOCK>.Value, OnResponse_HeroGodUnlock);
            AddResponser(Id<MSG_GateZ_HERO_GOD_EQUIP>.Value, OnResponse_HeroGodEquip);
            AddResponser(Id<MSG_GateZ_UPDATE_MAINQUEUE_HEROPOS>.Value, OnResponse_UpdateMainQueueHeroPos);
            AddResponser(Id<MSG_GateZ_UNLOCK_MAINQUEUE>.Value, OnResponse_UnlockMainBattleQueue);
            AddResponser(Id<MSG_GateZ_CHANGE_MAINQUEUE_NAME>.Value, OnResponse_ChangeMainQueueName);
            AddResponser(Id<MSG_GateZ_MAINQUEUE_DISPATCH_BATTLE>.Value, OnResponse_MainQueueDispatchBattle);
            AddResponser(Id<MSG_GateZ_HERO_INHERIT>.Value, OnResponse_HeroInherit);
            AddResponser(Id<MSG_GateZ_HERO_GOD_STEPS_UP>.Value, OnResponse_HeroGodStepsUp);

            //Skill
            AddResponser(Id<MSG_GateZ_CAST_SKILL>.Value, OnResponse_CastSkill);
            AddResponser(Id<MSG_GateZ_CAST_HERO_SKILL>.Value, OnResponse_CastHeroSkill);

            //Nature
            AddResponser(Id<MSG_GateZ_HERO_NATURE>.Value, OnResponse_GetHeroNature);
            AddResponser(Id<MSG_GateZ_GET_HERO_POWER>.Value, OnResponse_GetHeroPower);

            // 组队 Team
            AddResponser(Id<MSG_GateZ_TEAM_TYPE_LIST>.Value, OnResponse_TeamTypeList);
            AddResponser(Id<MSG_GateZ_CREATE_TEAM>.Value, OnResponse_CreateTeam);
            AddResponser(Id<MSG_GateZ_JOIN_TEAM>.Value, OnResponse_JoinTeam);
            AddResponser(Id<MSG_GateZ_QUIT_TEAM>.Value, OnResponse_QuitTeam);
            AddResponser(Id<MSG_GateZ_KICK_TEAM_MEMBER>.Value, OnResponse_KickTeamMember);
            AddResponser(Id<MSG_GateZ_TRANSFER_CAPTAIN>.Value, OnResponse_TramsforCaptain);
            AddResponser(Id<MSG_GateZ_ASK_JOIN_TEAM>.Value, OnResponse_AskJoinTeam);
            AddResponser(Id<MSG_GateZ_INVITE_JOIN_TEAM>.Value, OnResponse_InviteJoinTeam);
            AddResponser(Id<MSG_GateZ_ANSWER_INVITE_JOIN_TEAM>.Value, OnResponse_AnswerInviteJoinTeam);
            AddResponser(Id<MSG_GateZ_TRY_ASK_FOLLOW_CAPTAIN>.Value, OnResponse_TryAskFollowCaptain);
            AddResponser(Id<MSG_GateZ_ASK_FOLLOW_CAPTAIN>.Value, OnResponse_AskFollowCaptain);
            AddResponser(Id<MSG_GateZ_CHANGE_TEAM_TYPE>.Value, OnResponse_ChangeTeamType);
            AddResponser(Id<MSG_GateZ_TEAM_RELIVE_TEAMMEMBER>.Value, OnResponse_ReliveTeamMember);
            AddResponser(Id<MSG_GateZ_NEED_TEAM_HELP>.Value, OnResponse_NeedTeamHelp);
            AddResponser(Id<MSG_GateZ_RESPONSE_TEAM_HELP>.Value, OnResponse_ResponseTeamHelp);
            AddResponser(Id<MSG_GateZ_RELIVE_HERO>.Value, OnResponse_ReliveHero);
            AddResponser(Id<MSG_GateZ_INVITE_FRIEND_JOIN_TEAM>.Value, OnResponse_InviteFriendJoinTeam);
            AddResponser(Id<MSG_GateZ_QUIT_TEAM_INDUNGEON>.Value, OnResponse_QuitTeamInDungeon);

            // 副本 
            AddResponser(Id<MSG_GateZ_CREATE_DUNGEON>.Value, OnResponse_CreateDungeon);
            AddResponser(Id<MSG_GateZ_LEAVE_DUNGEON>.Value, OnResponse_LeaveDungeon);
            AddResponser(Id<MSG_GateZ_DUNGEON_STOP_BATTLE>.Value, OnResponse_DungeonStopBattle);
            AddResponser(Id<MSG_GateZ_DUNGEON_RESULT>.Value, OnResponse_DungeonResult);
            AddResponser(Id<MSG_GateZ_DUNGEON_BATTLE_DATA>.Value, OnResponse_DungeonBattleData);
            AddResponser(Id<MSG_GateZ_VERIFY_QUIT_DUNGEON>.Value, OnResponse_VerifyQuitDungeon);
            AddResponser(Id<MSG_GateZ_DUNGEON_SPEED_UP>.Value, OnResponse_DungeonSpeedUp);
            AddResponser(Id<MSG_GateZ_DUNGEON_SKIP_BATTLE>.Value, OnResponse_DungeonSkipBattle);

            //Counter
            AddResponser(Id<MSG_GateZ_COUNTER_BUY_COUNT>.Value, OnResponse_CounterBuyCount);
            AddResponser(Id<MSG_GateZ_GET_SPECIAL_COUNT>.Value, OnResponse_GetSpecialCount);

            //Delegation
            AddResponser(Id<MSG_GateZ_DELEGATION_LIST>.Value, OnResponse_DelegationList);
            AddResponser(Id<MSG_GateZ_DELEGATE_HEROS>.Value, OnResponse_DelegateHeros);
            AddResponser(Id<MSG_GateZ_COMPLETE_DELEGATION>.Value, OnResponse_CompleteDelegation);
            AddResponser(Id<MSG_GateZ_DELEGATION_REWARDS>.Value, OnResponse_GetDelegationRewards);
            AddResponser(Id<MSG_GateZ_REFRESH_DELEGATION>.Value, OnResponse_RefreshDelegation);
            AddResponser(Id<MSG_GateZ_BUY_DELEGATION_COUNT>.Value, OnResponse_BuyDelegationCount);

            //Hunting 
            AddResponser(Id<MSG_GateZ_HUNTING_INFO>.Value, OnResponse_HuntingInfo);
            AddResponser(Id<MSG_GateZ_HUNTING_SWEEP>.Value, OnResponse_HuntingSweep);
            AddResponser(Id<MSG_GateZ_CONTINUE_HUNTING>.Value, OnResponse_ContinueHunting);
            AddResponser(Id<MSG_GateZ_HUNTING_ACTICITY_UNLOCK>.Value, OnResponse_HuntingActivityUnlock);
            AddResponser(Id<MSG_GateZ_HUNTING_ACTICITY_SWEEP>.Value, OnResponse_HuntingActivitySweep);
            AddResponser(Id<MSG_GateZ_HUNTING_HELP>.Value, OnResponse_HuntingHelp);
            AddResponser(Id<MSG_GateZ_HUNTING_HELP_ANSWER>.Value, OnResponse_HuntingHelpAnswer);
            AddResponser(Id<MSG_GateZ_HUNTING_INTRUDE_HERO_POS>.Value, OnResponse_HuntingIntrudeUpdateHeroPos);
            AddResponser(Id<MSG_GateZ_HUNTING_INTRUDE_CHALLENGE>.Value, OnResponse_HuntingIntrudeChallenge); 

            // integral boss  
            AddResponser(Id<MSG_GateZ_INTERGRAL_BOSS_INFO>.Value, OnResponse_IntegralInfo);
            AddResponser(Id<MSG_GateZ_INTERGRAL_BOSS_KILLINFO>.Value, OnResponse_IntegralKillInfo);


            //Arena
            AddResponser(Id<MSG_GateZ_SAVE_DEFEMSIVE>.Value, OnResponse_SaveDefensive);
            AddResponser(Id<MSG_GateZ_RESET_ARENA_FIGHT_TIME>.Value, OnResponse_ResetArenaFightTime);
            AddResponser(Id<MSG_GateZ_GET_RANK_LEVEL_REWARD>.Value, OnResponse_GetRankLevelReward);
            AddResponser(Id<MSG_GateZ_GET_ARENA_CHALLENGERS>.Value, OnResponse_GetArenaChallenger);
            AddResponser(Id<MSG_GateZ_SHOW_ARENA_RANK_INFO>.Value, OnResponse_ShowArenaRankInfo);
            AddResponser(Id<MSG_GateZ_SHOW_ARENA_CHALLENGER>.Value, OnResponse_ShowArenaChallengerInfo);
            AddResponser(Id<MSG_GateZ_ENTER_ARENA_MAP>.Value, OnResponse_EnterArenaMap);
            AddResponser(Id<MSG_GateZ_VERSUS_PLAYER>.Value, OnResponse_EnterVersusMap);

            //秘境 
            AddResponser(Id<MSG_GateZ_SECRET_AREA_INFO>.Value, OnResponse_SecretAreaInfo);
            AddResponser(Id<MSG_GateZ_SECRET_AREA_SWEEP>.Value, OnResponse_SecretAreaSweep);
            AddResponser(Id<MSG_GateZ_SECRET_AREA_RANK_INFO>.Value, OnResponse_SecretAreaRankInfo);
            AddResponser(Id<MSG_GateZ_SECRET_AREA_CONT_FIGHT>.Value, OnResponse_SecretAreaContinueFight);

            //Shop
            AddResponser(Id<MSG_GateZ_SHOP_INFO>.Value, OnResponse_GetShopInfo);
            AddResponser(Id<MSG_GateZ_SHOP_BUY_ITEM>.Value, OnResponse_ShopBuyItem);
            AddResponser(Id<MSG_GateZ_SHOP_REFRESH>.Value, OnResponse_ShopRefresh);
            AddResponser(Id<MSG_GateZ_SHOP_SOULBONE_BONUS>.Value, OnResponse_ShopSoulBoneBonus);
            AddResponser(Id<MSG_GateZ_SHOP_SOULBONE_REWARD>.Value, OnResponse_ShopSoulBoneReward);
            AddResponser(Id<MSG_GateZ_BUY_SHOP_ITEM>.Value, OnResponse_BuyShopItem);

            //通行证
            AddResponser(Id<MSG_GateZ_GET_PASSCARD_PANEL_INFO>.Value, OnResponse_GetPasscardPanelInfo);
            AddResponser(Id<MSG_GateZ_GET_PASSCARD_LEVEL_REWARD>.Value, OnResponse_GetPasscardLevelReward);
            AddResponser(Id<MSG_GateZ_GET_PASSCARD_DAILY_REWARD>.Value, OnResponse_GetPasscardDailyReward);
            AddResponser(Id<MSG_GateZ_GET_PASSCARD_RECHARGED_LEVEL>.Value, OnResponse_GetPasscardRechargeLevel);
            AddResponser(Id<MSG_GateZ_GET_PASSCARD_RECHARGED>.Value, OnResponse_GetPasscardRecharged);
            AddResponser(Id<MSG_GateZ_GET_PASSCARD_TASK_EXP>.Value, OnResponse_GetPasscardTask);

            //抽卡
            AddResponser(Id<MSG_GateZ_DRAW_HERO>.Value, OnResponse_DrawHero);
            AddResponser(Id<MSG_GateZ_ACTIVATE_HERO_COMBO>.Value, OnResponse_ActivateHeroCombo);

            //章节 
            AddResponser(Id<MSG_GateZ_CHAPTER_INFO>.Value, OnResponse_GetChapterInfo);
            AddResponser(Id<MSG_GateZ_CHAPTER_NEXT_PAGE>.Value, OnResponse_ChapterNextPage);
            AddResponser(Id<MSG_GateZ_CHAPTER_REWARD>.Value, OnResponse_ChapterReward);
            AddResponser(Id<MSG_GateZ_CHAPTER_SWEEP>.Value, OnResponse_ChapterSweep);
            AddResponser(Id<MSG_GateZ_CHAPTER_BUY_POWER>.Value, OnResponse_BuyTimeSpacePower);

            //魂师试炼  
            AddResponser(Id<MSG_GateZ_BENEFIT_INFO>.Value, OnResponse_BenefitInfo);
            AddResponser(Id<MSG_GateZ_BENEFIT_SWEEP>.Value, OnResponse_BenefitSweep);

            //成神之路 
            AddResponser(Id<MSG_GateZ_GOD_HERO_INFO>.Value, OnResponse_GodHeroInfo);
            AddResponser(Id<MSG_GateZ_GOD_PATH_BUY_POWER>.Value, OnResponse_BuyGodPathPower);
            AddResponser(Id<MSG_GateZ_GOD_PATH_SEVEN_FIGHT_START>.Value, OnResponse_GodPathSevenFightStart);
            AddResponser(Id<MSG_GateZ_GOD_PATH_SEVEN_FIGHT_NEXT_STAGE>.Value, OnResponse_GodPathSevenFightNextStage);
            AddResponser(Id<MSG_GateZ_GOD_PATH_USE_ITEM>.Value, OnResponse_GodPathUseItem);
            AddResponser(Id<MSG_GateZ_GOD_PATH_TRAIN_BODY_BUY>.Value, OnResponse_GodPathTrainBodyBuyShield);
            AddResponser(Id<MSG_GateZ_GOD_PATH_TRAIN_BODY>.Value, OnResponse_GodPathTrainBody);
            AddResponser(Id<MSG_GateZ_GOD_FINISH_STAGE_TASK>.Value, OnResponse_GodPathFinishStageTask);
            AddResponser(Id<MSG_GateZ_GOD_PATH_BUY_OCEAN_HEART>.Value, OnResponse_GodPathOceanHeartBuyCount);
            AddResponser(Id<MSG_GateZ_GOD_PATH_REPAINT_OCEAN_HEART>.Value, OnResponse_GodPathOceanHeartRepaint);
            AddResponser(Id<MSG_GateZ_GOD_PATH_OCEAN_HEART_DRAW>.Value, OnResponse_GodPathOceanHeartDraw);

            AddResponser(Id<MSG_GateZ_GOD_PATH_BUY_TRIDENT>.Value, OnResponse_GodPathTridentBuy);
            AddResponser(Id<MSG_GateZ_GOD_PATH_USE_TRIDENT>.Value, OnResponse_GodPathTridentUse);
            AddResponser(Id<MSG_GateZ_GOD_PATH_TRIDENT_RESULT>.Value, OnResponse_GodPathTridentResult);
            AddResponser(Id<MSG_GateZ_GOD_PATH_PUSH_TRIDENT>.Value, OnResponse_GodPathTridentPush);

            AddResponser(Id<MSG_GateZ_GOD_PATH_LIGHT_PUZZLE>.Value, OnResponse_GodPathAcrossOceanLightPuzzle);
            AddResponser(Id<MSG_GateZ_GOD_PATH_ACROSS_OCEAN_SWEEP>.Value, OnResponse_GodPathAcrossOceanSweep);

            //福利
            AddResponser(Id<MSG_GateZ_WELFARE_TRIGGER_STATE>.Value, OnResponse_WelfareTriggerState);

            //许愿池
            AddResponser(Id<MSG_GateZ_USINIG_WISHPOOL>.Value, OnResponse_UseWishPool);
            AddResponser(Id<MSG_GateZ_GET_WISHPOOL_UPDATE>.Value, OnResponse_GetWishPoolInfo);


            //Cross battle
            AddResponser(Id<MSG_GateZ_UPDATE_CROSS_QUEUE>.Value, OnResponse_SaveCrossBattleDefensive);
            AddResponser(Id<MSG_GateZ_SHOW_CROSS_BATTLE_CHALLENGER>.Value, OnResponse_ShowCrossBattleChallenger);
            AddResponser(Id<MSG_GateZ_SHOW_CROSS_LEADER_INFO>.Value, OnResponse_ShowCrossLeaderInfo);
            AddResponser(Id<MSG_GateZ_GET_CROSS_BATTLE_ACTIVE_REWARD>.Value, OnResponse_GetCrossActiveReward);
            AddResponser(Id<MSG_GateZ_GET_CROSS_BATTLE_PRELIMINARY_REWARD>.Value, OnResponse_GetCrossPreliminaryReward);
            AddResponser(Id<MSG_GateZ_ENTER_CROSS_BATTLE_MAP>.Value, OnResponse_EnterCrossMap);
            AddResponser(Id<MSG_GateZ_SHOW_CROSS_BATTLE_FINALS>.Value, OnResponse_GetCrossFinalsInfo);
            AddResponser(Id<MSG_GateZ_GET_CROSS_VIDEO>.Value, OnResponse_GetCrossBattleVedio);
            AddResponser(Id<MSG_GateZ_GET_CROSS_BATTLE_SERVER_REWARD>.Value, OnResponse_GetCrossServerReward);
            AddResponser(Id<MSG_GateZ_GET_GUESSING_INFO>.Value, OnResponse_GetCrossGuessingInfo);
            AddResponser(Id<MSG_GateZ_CROSS_GUESSING_CHOOSE>.Value, OnResponse_CrossGuessingChoose);

            //阵营建设
            AddResponser(Id<MSG_GateZ_GET_CAMPBUILD_INFO>.Value, OnResponse_GetCampBuildInfo);
            AddResponser(Id<MSG_GateZ_CAMPBUILD_GO>.Value, OnResponse_CampBuildGo);
            //AddResponser(Id<MSG_GateZ_BUY_CAMPBUILD_GO_COUNT>.Value, OnResponse_BuyCampBuildGoCount);
            AddResponser(Id<MSG_GateZ_CAMPBUILD_RANK_LIST>.Value, OnResponse_CampBuildRankList);
            AddResponser(Id<MSG_GateZ_OPEN_CAMPBUILD_BOX>.Value, OnResponse_OpenCampBuildBox);
            AddResponser(Id<MSG_GateZ_CAMP_CREATE_DUNGEON>.Value, OnResponse_CampCreateDungeon);

            //爬塔
            AddResponser(Id<MSG_GateZ_TOWER_INFO>.Value, OnResponse_GetTowerInfo);
            AddResponser(Id<MSG_GateZ_TOWER_REWARD>.Value, OnResponse_TowerReward);
            AddResponser(Id<MSG_GateZ_TOWER_SHOP_ITEM>.Value, OnResponse_TowerShopItemList);
            AddResponser(Id<MSG_GateZ_TOWER_EXECUTE_TASK>.Value, OnResponse_TowerExecuteTask);
            AddResponser(Id<MSG_GateZ_TOWER_SELECT_BUFF>.Value, OnResponse_TowerSelectBuff);
            AddResponser(Id<MSG_GateZ_TOWER_BUFF>.Value, OnResponse_TowerBuff);
            AddResponser(Id<MSG_GateZ_UPDATE_TOWER_HERO_POS>.Value, OnResponse_TowerUpdateHeroPos);
            AddResponser(Id<MSG_GateZ_TOWER_HERO_REVIVE>.Value, OnResponse_TowerReviveHero);


            // 阵营战
            AddResponser(Id<MSG_GateZ_GET_CAMPBATTLE_INFO>.Value, OnResponse_GetCampBattleInfo);
            AddResponser(Id<MSG_GateZ_FORT_INFO>.Value, OnResponse_GetFortInfo);
            AddResponser(Id<MSG_GateZ_GET_CAMPBATTLE_RANK_LIST>.Value, OnResponse_GetCampBattleRankList);
            AddResponser(Id<MSG_GateZ_OPEN_CAMP_BOX>.Value, OnResponse_OpenCampBox);
            AddResponser(Id<MSG_GateZ_CHECK_IN_BATTLE_RANK>.Value, OnResponse_CheckInBattleRank);
            AddResponser(Id<MSG_Gate_USE_NATURE_ITEM>.Value, OnResponse_UseNatureItem);
            AddResponser(Id<MSG_GateZ_UPDATE_DEFENSIVE_QUEUE>.Value, OnResponse_UpdateDefensiveQueue);
            AddResponser(Id<MSG_GateZ_GIVEUP_FORT>.Value, OnResponse_GiveUpFort);
            AddResponser(Id<MSG_GateZ_HOLD_FORT>.Value, OnResponse_HoldFort);
            AddResponser(Id<MSG_GateZ_GET_CAMPBATTLE_ANNOUNCE>.Value, OnResponse_GetCampBattleAnnouce);


            //Rank
            AddResponser(Id<MSG_GateZ_GET_RANK_LIST_BY_TYPE>.Value, OnResponse_GetRankListByType);
            AddResponser(Id<MSG_GateZ_RANK_REWARD_LIST>.Value, OnResponse_GetRankRewardInfos);
            AddResponser(Id<MSG_GateZ_GET_RANK_REWARD>.Value, OnResponse_GetRankReward);
            AddResponser(Id<MSG_GateZ_GET_CROSS_RANK_REWARD>.Value, OnResponse_GetCrossRankReward);

            //传送进图
            AddResponser(Id<MSG_GateZ_TRANSFER_ENTER_MAP>.Value, OnResponse_TransferEnterMap);

            //挂机
            AddResponser(Id<MSG_GateZ_ONHOOK_INFO>.Value, OnResponse_GetOnhookInfo);
            AddResponser(Id<MSG_GateZ_ONHOOK_GET_REWARD>.Value, OnResponse_GetOnhookReward);

            //推图 
            AddResponser(Id<MSG_GateZ_PUSHFIGURE_FINISHTASK>.Value, OnResponse_PushFigureFinishTask);

            //武魂共鳴
            AddResponser(Id<MSG_GateZ_OPEN_RESONANCE_GRID>.Value, OnResponse_OpenResonanceGrid);
            AddResponser(Id<MSG_GateZ_ADD_RESONANCE>.Value, OnResponse_AddResonance);
            AddResponser(Id<MSG_GateZ_SUB_RESONANCE>.Value, OnResponse_SubResonance);
            AddResponser(Id<MSG_GateZ_RESONANCE_LEVEL_UP>.Value, OnResponse_ResonanceLevelUp);

            //礼包码
            AddResponser(Id<MSG_GateZ_GIFT_CODE_REWARD>.Value, OnResponse_GiftCodeExchangeReward);
            AddResponser(Id<MSG_GateZ_CHECK_CODE_UNIQUE>.Value, OnResponse_CheckCodeUnique);

            //金兰
            AddResponser(Id<MSG_GateZ_BROTHERS_INVITE>.Value, OnResponse_BrotherInvite);
            AddResponser(Id<MSG_GateZ_BROTHERS_RESPONSE>.Value, OnResponse_BrotherResponse);
            AddResponser(Id<MSG_GateZ_BROTHERS_REMOVE>.Value, OnResponse_BrotherRemove);

            AddResponser(Id<MSG_GateZ_FRIEND_RESPONSE>.Value, OnResponse_FriendResponse);
            AddResponser(Id<MSG_GateZ_ONEKEY_IGNORE_INVITER>.Value, OnResponse_OnekeyIgnoreInviter);

            //挖宝
            AddResponser(Id<MSG_GateZ_SHOVEL_GAME_REWARDS>.Value, OnResponse_ShovelGameRewards);
            AddResponser(Id<MSG_GateZ_SHOVEL_GAME_START>.Value, OnResponse_ShovelGameStart);
            AddResponser(Id<MSG_GateZ_LIGHT_TREASURE_PUZZLE>.Value, OnResponse_LightTreasurePuzzle);
            AddResponser(Id<MSG_GateZ_TREASURE_FLY_START>.Value, OnResponse_TreasureFlyStart);
            AddResponser(Id<MSG_GateZ_TREASURE_FLY_DONE>.Value, OnResponse_TreasureFlyDone);
            AddResponser(Id<MSG_GateZ_LOOK_PUZZLE_INFO>.Value, OnResponse_LookPuzzleInfo);
            AddResponser(Id<MSG_GateZ_SHOVEL_GAME_REVIVE>.Value, OnResponse_ShovelGameRevive);

            //贡献
            AddResponser(Id<MSG_GateZ_CONTRIBUTION_INFO>.Value, OnResponse_ContributionInfo);
            AddResponser(Id<MSG_GateZ_GET_CONTRIBUTION_REWARD>.Value, OnResponse_GetContributionReward);

            //主题通行证
            AddResponser(Id<MSG_GateZ_GET_THEMEPASS_REWARD>.Value, OnResponse_GetThemePassReward);
            //主题Boss
            AddResponser(Id<MSG_GateZ_THEMEBOSS_DUNGEON>.Value, OnResponse_ThemeBossDungeon);
            AddResponser(Id<MSG_GateZ_GET_THEMEBOSS_REWARD>.Value, OnResponse_GetThemeBossReward);
            AddResponser(Id<MSG_GateZ_THEMEBOSS_UPDATE_DEFENSIVE_QUEUE>.Value, OnResponse_UpdateThemeBossQueue);


            AddResponser(Id<MSG_GateZ_GET_HIDDER_WEAPON_VALUE>.Value, OnResponse_GetHidderWeaponInfo);
            AddResponser(Id<MSG_GateZ_GET_HIDDER_WEAPON_REWARD>.Value, OnResponse_GetHidderWeaponReward);
            AddResponser(Id<MSG_GateZ_GET_HIDDER_WEAPON_NUM_REWARD>.Value, OnResponse_GetHidderWeaponNumReward);
            AddResponser(Id<MSG_GateZ_BUY_HIDDER_WEAPON_ITEM>.Value, OnResponse_BuyHidderWeaponItem);
            AddResponser(Id<MSG_GateZ_GET_SEA_TREASURE_VALUE>.Value, OnResponse_GetSeaTreasureInfo);
            AddResponser(Id<MSG_GateZ_GET_SEA_TREASURE_REWARD>.Value, OnResponse_GetSeaTreasureReward);
            AddResponser(Id<MSG_GateZ_BUY_SEA_TREASURE_ITEM>.Value, OnResponse_BuySeaTreasureItem);
            AddResponser(Id<MSG_GateZ_NOTES_LIST_BY_TYPE>.Value, OnResponse_GetNotesListByType);
            AddResponser(Id<MSG_GateZ_GET_SEA_TREASURE_BLESSING>.Value, OnResponse_GetSeaTreasureBlessing);
            AddResponser(Id<MSG_GateZ_CLOSE_SEA_TREASURE_BLESSING>.Value, OnResponse_CloseSeaTreasureBlessing);
            AddResponser(Id<MSG_GateZ_GET_SEA_TREASURE_NUM_REWARD>.Value, OnResponse_GetSeaTreasureNumReward);

            //cross boss
            AddResponser(Id<MSG_GateZ_GET_CROSS_BOSS_INFO>.Value, OnResponse_GetCrossBossInfo);
            AddResponser(Id<MSG_GateZ_UPDATE_CROSS_BOSS_QUEUE>.Value, OnResponse_UpdateCrossBossQueue);
            AddResponser(Id<MSG_GateZ_GET_CROSS_BOSS_PASS_REWARD>.Value, OnResponse_GetCrossBossPassReward);
            AddResponser(Id<MSG_GateZ_GET_CROSS_BOSS_RANK_REWARD>.Value, OnResponse_GetCrossBossRankReward);
            AddResponser(Id<MSG_GateZ_ENTER_CROSS_BOSS_MAP>.Value, OnResponse_EnterCrossBossMap);
            AddResponser(Id<MSG_GateZ_CROSS_BOSS_CHALLENGER>.Value, OnResponse_CrossBossChallenger);
            AddResponser(Id<MSG_GateZ_CHALLENGE_CROSS_BOSS_MAP>.Value, OnResponse_ChallengeCrossBoss);

            //百草园
            AddResponser(Id<MSG_GateZ_GARDEN_INFO>.Value, OnResponse_GetGardenInfo);
            AddResponser(Id<MSG_GateZ_GARDEN_PLANTED_SEED>.Value, OnResponse_PlantedSeed);
            AddResponser(Id<MSG_GateZ_GARDEN_REAWARD>.Value, OnResponse_GetGardenReward);
            AddResponser(Id<MSG_GateZ_GARDEN_BUY_SEED>.Value, OnResponse_BuySeed);
            AddResponser(Id<MSG_GateZ_GARDEN_SHOP_EXCHANGE>.Value, OnResponse_GardenShopExchange);

            //乾坤问情
            AddResponser(Id<MSG_GateZ_GET_DIVINE_LOVE_VALUE>.Value, OnResponse_GetDivineLoveInfo);
            AddResponser(Id<MSG_GateZ_GET_DIVINE_LOVE_REWARD>.Value, OnResponse_GetDivineLoveReward);
            AddResponser(Id<MSG_GateZ_GET_DIVINE_LOVE_CUMULATE_REWARD>.Value, OnResponse_GetDivineLoveCumulateReward);
            AddResponser(Id<MSG_GateZ_BUY_DIVINE_LOVE_ITEM>.Value, OnResponse_BuyDivineLoveItem);
            AddResponser(Id<MSG_GateZ_CLOSE_DIVINE_LOVE_ROUND>.Value, OnResponse_CloseDivineLoveRound);
            AddResponser(Id<MSG_GateZ_OPEN_DIVINE_LOVE_ROUND>.Value, OnResponse_OpenDivineLoveRound);

            //海岛登高
            AddResponser(Id<MSG_GateZ_ISLAND_HIGH_INFO>.Value, OnResponse_IslandHighInfo);
            AddResponser(Id<MSG_GateZ_ISLAND_HIGH_ROCK>.Value, OnResponse_IslandHighRock);
            AddResponser(Id<MSG_GateZ_ISLAND_HIGH_REWARD>.Value, OnResponse_IslandHighReward);
            //AddResponser(Id<MSG_GateZ_ISLAND_HIGH_BUY_ITEM>.Value, OnResponse_IslandHighBuyItem); 

            //三叉戟
            AddResponser(Id<MSG_GateZ_TRIDENT_REWARD>.Value, OnResponse_TridentReward);
            AddResponser(Id<MSG_GateZ_TRIDENT_USE_SHOVEL>.Value, OnResponse_TridentUseShovel);

            //端午活动
            AddResponser(Id<MSG_GateZ_DRAGON_BOAT_GAME_START>.Value, OnResponse_DragonBoatGameStart);
            AddResponser(Id<MSG_GateZ_DRAGON_BOAT_GAME_END>.Value, OnResponse_DragonBoatGameEnd);
            AddResponser(Id<MSG_GateZ_DRAGON_BOAT_BUY_TICKET>.Value, OnResponse_DragonBoatBuyTicket);
            AddResponser(Id<MSG_GateZ_DRAGON_BOAT_FREE_TICKET>.Value, OnResponse_DragonBoatGetFreeTicket);

            //昊天石壁
            AddResponser(Id<MSG_GateZ_GET_STONE_WALL_VALUE>.Value, OnResponse_GetStoneWallInfo);
            AddResponser(Id<MSG_GateZ_GET_STONE_WALL_REWARD>.Value, OnResponse_GetStoneWallReward);
            AddResponser(Id<MSG_GateZ_BUY_STONE_WALL_ITEM>.Value, OnResponse_BuyStoneWallItem);
            AddResponser(Id<MSG_GateZ_RESET_STONE_WALL>.Value, OnResponse_RestStoneWall);

            //海岛挑战
            AddResponser(Id<MSG_GateZ_ISLAND_CHALLENGE_INFO>.Value, OnResponse_GetIslandChallengeInfo);
            AddResponser(Id<MSG_GateZ_ISLAND_CHALLENGE_REWARD>.Value, OnResponse_IslandChallengeReward);
            AddResponser(Id<MSG_GateZ_ISLAND_CHALLENGE_SHOP_ITEM>.Value, OnResponse_IslandChallengeShopItemList);
            AddResponser(Id<MSG_GateZ_ISLAND_CHALLENGE_EXECUTE_TASK>.Value, OnResponse_IslandChallengeExecuteTask);
            AddResponser(Id<MSG_GateZ_ISLAND_CHALLENGE_UPDATE_HERO_POS>.Value, OnResponse_IslandChallengeUpdateHeroPos);
            AddResponser(Id<MSG_GateZ_ISLAND_CHALLENGE_HERO_REVIVE>.Value, OnResponse_IslandChallengeReviveHero);
            AddResponser(Id<MSG_GateZ_ISLAND_CHALLENGE_RESET>.Value, OnResponse_IslandChallengeReset);
            AddResponser(Id<MSG_GateZ_ISLAND_CHALLENGE_SWAP_QUEUE>.Value, OnResponse_IslandChallengeSwapQueue);
            
            //嘉年华
            AddResponser(Id<MSG_GateZ_ENTER_CARNIVAL_BOSS_DUNGEON>.Value, OnResponse_EnterCarnivalBossDungeon);
            AddResponser(Id<MSG_GateZ_GET_CARNIVAL_BOSS_REWARD>.Value, OnResponse_GetCarnivalBossReward);
            AddResponser(Id<MSG_GateZ_UPDATE_CARNIVAL_BOSS_QUEUE>.Value, OnResponse_UpdateCarnivalBossQueue);
            AddResponser(Id<MSG_GateZ_GET_CARNIVAL_RECHARGE_REWARD>.Value, OnResponse_GetCarnivalRechargeReward);
            AddResponser(Id<MSG_GateZ_BUY_CARNIVAL_MALL_GIFT_ITEM>.Value, OnResponse_BuyCarnivalMallGiftItem);

            //漫游

            AddResponser(Id<MSG_GateZ_ACTIVATE_HERO_TRAVEL>.Value, OnResponse_ActivateHeroTravel);
            AddResponser(Id<MSG_GateZ_ADD_HERO_TRAVEL_AFFINITY>.Value, OnResponse_AddHeroTravelAffinity);
            AddResponser(Id<MSG_GateZ_START_HERO_TRAVEL_EVENT>.Value, OnResponse_StartHeroTravelEvevt);
            AddResponser(Id<MSG_GateZ_GET_HERO_TRAVEL_EVENT>.Value, OnResponse_GetHeroTravelEvevt);
            AddResponser(Id<MSG_GateZ_BUY_HERO_TRAVEL_SHOP_ITEM>.Value, OnResponse_ButTravelShopItem);

            //暗器
            AddResponser(Id<MSG_GateZ_HIDENWEAPON_EQUIP>.Value, OnResponse_HiddenWeaponEquip);
            AddResponser(Id<MSG_GateZ_HIDENWEAPON_UPGRADE>.Value, OnResponse_HiddenWeaponUpgrade);
            AddResponser(Id<MSG_GateZ_HIDENWEAPON_STAR>.Value, OnResponse_HiddenWeaponStar);
            AddResponser(Id<MSG_GateZ_HIDENWEAPON_WASH>.Value, OnResponse_HiddenWeaponWash);
            AddResponser(Id<MSG_GateZ_HIDENWEAPON_WASH_CONFIRM>.Value, OnResponse_HiddenWeaponWashConfirm);
            AddResponser(Id<MSG_GateZ_HIDENWEAPON_SMASH>.Value, OnResponse_HiddenWeaponSmash);

            //史莱克邀约
            AddResponser(Id<MSG_GateZ_GET_SHREK_INVITAION_REWARD>.Value, OnResponse_GetShrekInvitationReward);

            //轮盘
            AddResponser(Id<MSG_GateZ_ROULETTE_GET_INFO>.Value, OnResponse_GetRouletteInfo);
            AddResponser(Id<MSG_GateZ_ROULETTE_RANDOM>.Value, OnResponse_RouletteRandom);
            AddResponser(Id<MSG_GateZ_ROULETTE_REWARD>.Value, OnResponse_RouletteReward);
            AddResponser(Id<MSG_GateZ_ROULETTE_REFRESH>.Value, OnResponse_RouletteRefresh);
            AddResponser(Id<MSG_GateZ_ROULETTE_BUY_ITEM>.Value, OnResponse_RouletteBuyItem);
      
            //皮划艇
            AddResponser(Id<MSG_GateZ_GET_CANOE_INFO>.Value, OnResponse_GetCanoeInfo);
            AddResponser(Id<MSG_GateZ_CANOE_GAME_START>.Value, OnResponse_CanoeGameStart);
            AddResponser(Id<MSG_GateZ_CANOE_GAME_END>.Value, OnResponse_CanoeGameEnd);
            AddResponser(Id<MSG_GateZ_CANOE_GET_REWARD>.Value, OnResponse_CanoeGetReward);

            //中秋
            AddResponser(Id<MSG_GateZ_GET_MIDAUTUMN_INFO>.Value, OnResponse_GetMidAutumnInfo);
            AddResponser(Id<MSG_GateZ_DRAW_MIDAUTUMN_REWARD>.Value, OnResponse_DrawMidAutumnReward);
            AddResponser(Id<MSG_GateZ_GET_MIDAUTUMN_SCORE_REWARD>.Value, OnResponse_GetMidAutumnScoreReward);

            //主题烟花
            AddResponser(Id<MSG_GateZ_THEME_FIREWORK_INFO>.Value, OnResponse_GetThemeFireworkInfo);
            AddResponser(Id<MSG_GateZ_THEME_FIREWORK_SCORE_REWARD>.Value, OnResponse_GetThemeFireworkScoreReward);
            AddResponser(Id<MSG_GateZ_THEME_FIREWORK_USECOUNT_REWARD>.Value, OnResponse_GetThemeFireworkUseCountReward);

            //Cross challenge
            AddResponser(Id<MSG_GateZ_UPDATE_CROSS_CHALLENGE_QUEUE>.Value, OnResponse_SaveCrossChallengeDefensive);
            AddResponser(Id<MSG_GateZ_SHOW_CROSS_CHALLENGE_CHALLENGER>.Value, OnResponse_ShowCrossChallengeChallenger);
            AddResponser(Id<MSG_GateZ_SHOW_CROSS_CHALLENGE_LEADER_INFO>.Value, OnResponse_ShowCrossChallengeLeaderInfo);
            AddResponser(Id<MSG_GateZ_GET_CROSS_CHALLENGE_ACTIVE_REWARD>.Value, OnResponse_GetCrossChallengeActiveReward);
            AddResponser(Id<MSG_GateZ_GET_CROSS_CHALLENGE_PRELIMINARY_REWARD>.Value, OnResponse_GetCrossChallengePreliminaryReward);
            AddResponser(Id<MSG_GateZ_ENTER_CROSS_CHALLENGE_MAP>.Value, OnResponse_EnterCrossChallengeMap);
            AddResponser(Id<MSG_GateZ_SHOW_CROSS_CHALLENGE_FINALS>.Value, OnResponse_GetCrossChallengeFinalsInfo);
            AddResponser(Id<MSG_GateZ_GET_CROSS_CHALLENGE_VIDEO>.Value, OnResponse_GetCrossChallengeVideo);
            AddResponser(Id<MSG_GateZ_GET_CROSS_CHALLENGE_SERVER_REWARD>.Value, OnResponse_GetCrossChallengeServerReward);
            AddResponser(Id<MSG_GateZ_GET_CROSS_CHALLENGE_GUESSING_INFO>.Value, OnResponse_GetCrossChallengeGuessingInfo);
            AddResponser(Id<MSG_GateZ_CROSS_CHALLENGE_GUESSING_CHOOSE>.Value, OnResponse_CrossChallengeGuessingChoose);
            AddResponser(Id<MSG_GateZ_CROSS_CHALLENGE_SWAP_QUEUE>.Value, OnResponse_CrossChallengeSwapQueue);
            AddResponser(Id<MSG_GateZ_CROSS_CHALLENGE_SWAP_HERO>.Value, OnResponse_CrossChallengeSwapHero);
            

            //九考试炼
            AddResponser(Id<MSG_GateZ_GET_NINETEST_INFO>.Value, OnResponse_GetNineTestInfo);
            AddResponser(Id<MSG_GateZ_NINETEST_CLICK_GRID>.Value, OnResponse_NineTestClickGrid);
            AddResponser(Id<MSG_GateZ_NINETEST_SCORE_REWARD>.Value, OnResponse_NineTestScoreReward);
            AddResponser(Id<MSG_GateZ_NINETEST_RESET>.Value, OnResponse_NineTestReset);

            //仓库
            AddResponser(Id<MSG_GateZ_GET_WAREHOUSE_CURRENCIES>.Value, OnResponse_GetWareHouseCurrencies);
            AddResponser(Id<MSG_GateZ_SHOW_WAREHOUSE_ITEMS>.Value, OnResponse_ShowWareHouseItems);
            AddResponser(Id<MSG_GateZ_BATCH_GET_WAREHOUSE_ITEMS>.Value, OnResponse_BatchGetWareHouseItems);

            //学院
            AddResponser(Id<MSG_GateZ_ENTER_SCHOOL>.Value, OnResponse_EnterSchool);
            AddResponser(Id<MSG_GateZ_LEAVE_SCHOOL>.Value, OnResponse_LeaveSchool);
            AddResponser(Id<MSG_GateZ_SCHOOL_POOL_USE_ITEM>.Value, OnResponse_SchoolPoolUseItem);
            AddResponser(Id<MSG_GateZ_SCHOOL_POOL_LEVEL_UP>.Value, OnResponse_SchoolPoolLevelUp);
            AddResponser(Id<MSG_GateZ_GET_SCHOOLTASK_FINISH_REWARD>.Value, OnResponse_GetSchoolTaskFinishReward);
            AddResponser(Id<MSG_GateZ_GET_SCHOOLTASK_BOX_REWARD>.Value, OnResponse_GetSchoolTaskBoxReward);
            AddResponser(Id<MSG_GateZ_ANSWER_QUESTION_START>.Value, OnResponse_AnswerQuestionStart);
            AddResponser(Id<MSG_GateZ_ANSWER_QUESTION_SUBMIT>.Value, OnResponse_AnswerQuestionSubmit);
            AddResponser(Id<MSG_GateZ_GET_DIAMOND_REBATE_REWARDS>.Value, OnResponse_GetDiamondRebateRewards);

            //玄天宝箱
            AddResponser(Id<MSG_GateZ_XUANBOX_GET_INFO>.Value, OnResponse_GetXuanBoxInfo);
            AddResponser(Id<MSG_GateZ_XUANBOX_RANDOM>.Value, OnResponse_XuanBoxRandom);
            AddResponser(Id<MSG_GateZ_XUANBOX_REWARD>.Value, OnResponse_XuanBoxReward);

            //九笼祈愿
            AddResponser(Id<MSG_GateZ_WISH_LANTERN_SELECT_REWARD>.Value, OnResponse_WishLanternSelect);
            AddResponser(Id<MSG_GateZ_WISH_LANTERN_LIGHT>.Value, OnResponse_WishLanternLight);
            AddResponser(Id<MSG_GateZ_WISH_LANTERN_RESET>.Value, OnResponse_WishLanternReset);

            //史莱克乐园
            AddResponser(Id<MSG_GateZ_SHREKLAND_USE_ROULETTE>.Value, OnResponse_ShreklandUseRoulette);
            AddResponser(Id<MSG_GateZ_SHREKLAND_REFRESH_REWARDS>.Value, OnResponse_ShreklandRefreshRewards);
            AddResponser(Id<MSG_GateZ_SHREKLAND_GET_SCORE_REWARD>.Value, OnResponse_ShreklandGetScoreReward);

            //魔鬼训练
            AddResponser(Id<MSG_GateZ_GET_DEVIL_TRAINING_INFO>.Value, OnResponse_DevilTrainingInfo);
            AddResponser(Id<MSG_GateZ_GET_DEVIL_TRAINING_REWARD>.Value, OnResponse_DevilTrainingReward);
            AddResponser(Id<MSG_GateZ_BUY_DEVIL_TRAINING_ITEM>.Value, OnResponse_DevilTrainingBuyItem);
            AddResponser(Id<MSG_GateZ_GET_DEVIL_TRAINING_POINT_REWARD>.Value, OnResponse_DevilTrainingPointReward);
            AddResponser(Id<MSG_GateZ_CHANGE_DEVIL_TRAINING_BUFF>.Value, OnResponse_DevilTrainingChangeBuff);

            //时空塔
            AddResponser(Id<MSG_GateZ_SPACE_TIME_JOIN_TEAM>.Value, OnResponse_SpaceTimeJoinTeam);
            AddResponser(Id<MSG_GateZ_SPACE_TIME_QUIT_TEAM>.Value, OnResponse_SpaceTimeQuitTeam);
            AddResponser(Id<MSG_GateZ_SPACETIME_REFRESH_CARD_POOL>.Value, OnResponse_SpaceTimeRefreshCardPool);
            AddResponser(Id<MSG_GateZ_SPACETIME_HERO_STEPUP>.Value, OnResponse_SpaceTimeHeroStepUp);
            AddResponser(Id<MSG_GateZ_UPDATE_SPACETIME_HERO_QUEUE>.Value, OnResponse_UpdateSpaceTimeHeroQueue);
            AddResponser(Id<MSG_GateZ_SPACETIME_EXECUTE_EVENT>.Value, OnResponse_SpaceTimeExecuteEvent);
            AddResponser(Id<MSG_GateZ_SPACETIME_GET_STAGE_AWARD>.Value, OnResponse_SpaceTimeGetStageAward);
            AddResponser(Id<MSG_GateZ_SPACETIME_RESET>.Value, OnResponse_SpaceTimeReset);
            AddResponser(Id<MSG_GateZ_SELECT_GUIDESOUL_ITEM>.Value, OnResponse_SelectGuideSoulItem);
            AddResponser(Id<MSG_GateZ_SPACETIME_ENTER_NEXTLEVEL>.Value, OnResponse_SpaceTimeEnterNextLevel);
            AddResponser(Id<MSG_GateZ_SPACETIME_BEAST_SETTLEMENT>.Value, OnResponse_SpaceTimeBeastSettlement);
            AddResponser(Id<MSG_GateZ_SPACETIME_HOUSE_RANDOM_PARAM>.Value, OnResponse_SpaceTimeHouseRandomParam);
            AddResponser(Id<MSG_GateZ_ENTER_SPACETIME_TOWER>.Value, OnResponse_EnterSpaceTimeTower);
            AddResponser(Id<MSG_GateZ_SPACETIME_GET_PAST_REWARDS>.Value, OnResponse_SpaceTimeGetPastRewards);
            
            //神域赐福
            AddResponser(Id<MSG_GateZ_DOMAIN_BENEDICTION_GET_STAGE_AWARD>.Value, OnResponse_HandleGetStageAward);
            AddResponser(Id<MSG_GateZ_DOMAIN_BENEDICTION_GET_BASE_CURRENCY_AWARD>.Value, OnResponse_HandleGetBaseCurrencyAward);
            AddResponser(Id<MSG_GateZ_DOMAIN_BENEDICTION_PRAY_OPERATION>.Value, OnResponse_HandlePrayOperation);
            AddResponser(Id<MSG_GateZ_DOMAIN_BENEDICTION_DRAW_OPERATION>.Value, OnResponse_HandleDrawOperation);
            
            //漂流探宝
            AddResponser(Id<MSG_GateZ_DRIFT_EXPLORE_TASK_REWARD>.Value, OnResponse_DriftExploreTaskReward);
            AddResponser(Id<MSG_GateZ_DRIFT_EXPLORE_REWARD>.Value, OnResponse_DriftExploreReward);
            
            //ResponserEnd
        }

        protected override void OnDisconnect()
        {
            //链接断开后马上存盘
            List<PlayerChar> playerList = Api.PCManager.GetGatePlayerList(this.SubId);
            foreach (var player in playerList)
            {
                try
                {
                    player.LeaveWorld();
                }
                catch (Exception e)
                {
                    Log.Alert(e.ToString());
                }
            }
            base.OnDisconnect();
        }

        public override void OnResponse_RegistServer(MemoryStream stream, int uid = 0)
        {
            base.OnResponse_RegistServer(stream, uid);
            stream.Seek(0, SeekOrigin.Begin);
            MSG_REGIST_SERVER msg = MessagePacker.ProtobufHelper.Deserialize<MSG_REGIST_SERVER>(stream);
            // gate<->zone断线重连后，需要重置旧gate维护的player
            foreach (var player in Api.PCManager.PcList)
            {
                if (player.Value.Gate.SubId == msg.ServerInfo.SubId && player.Value.Gate.MainId == msg.ServerInfo.MainId)
                {
                    player.Value.BindGate(this);
                }
            }
            foreach (var player in Api.PCManager.LoadingList)
            {
                if (player.Value.Gate.SubId == msg.ServerInfo.SubId && player.Value.Gate.MainId == msg.ServerInfo.MainId)
                {
                    player.Value.BindGate(this);
                }
            }
            foreach (var player in Api.PCManager.LoadingDoneList)
            {
                if (player.Value.Gate.SubId == msg.ServerInfo.SubId && player.Value.Gate.MainId == msg.ServerInfo.MainId)
                {
                    player.Value.BindGate(this);
                }
            }
        }

    }
}
