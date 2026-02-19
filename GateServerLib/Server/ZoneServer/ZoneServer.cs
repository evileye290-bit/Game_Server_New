using System.IO;
using Logger;
using Message.IdGenerator;
using Message.Zone.Protocol.ZGate;
using Message.Gate.Protocol.GateC;
using ServerFrame;
using Message.Client.Protocol.CGate;

namespace GateServerLib
{
    public partial class ZoneServer : BackendServer
    {
        private GateServerApi Api
        { get { return (GateServerApi)api; } }
        
        public ZoneServer(BaseApi api):base(api)
        {
        }

        protected override bool NeedConnect()
        {
            //不主动重新连接到被禁的zone
            return !ZoneTransformManager.Instance.IsForbided(this.SubId);
        }

        protected override void BindResponser()
        {
            base.BindResponser();
            AddResponser(Id<MSG_ZGC_ERROR_CODE>.Value, OnResponse_ErrorCode);
            AddResponser(Id<MSG_ZGC_LOGIN_FREEZE>.Value, OnResponse_LoginFreeze);
            //AddResponser(Id<MSG_MM_GM_SEND_EMAIL>.Value, OnResponse_GmSendEmail);
            AddResponser(Id<MSG_ZGate_ENTER_OTHER_ZONE>.Value, OnResponse_EnterOtherZone);
            AddResponser(Id<MSG_ZGate_LeaveWorld>.Value, OnResponse_LeaveWorld);
            AddResponser(Id<MSG_GC_ENTER_ZONE>.Value, OnResponse_EnterZone);
            AddResponser(Id<MSG_GC_ENTER_WORLD>.Value, OnResponse_EnterWorld);
            AddResponser(Id<MSG_GC_TIME_SYNC>.Value, OnResponse_TimeSync);
            AddResponser(Id<MSG_ZGC_SUGGEST>.Value, OnResponse_Suggest);
            AddResponser(Id<MSG_ZGC_FIGHTING_COUNT>.Value, OnResponse_FighingCount);
            AddResponser(Id<MSG_ZGC_CROSS_NOTES_LIST>.Value, OnResponse_ReturnNotesList);
            //geography
            AddResponser(Id<MSG_ZGC_GEOGRAPHY>.Value, OnResponse_Geography);
            //
            AddResponser(Id<MSG_GC_FieldObject_MOVE>.Value, OnResponse_FieldObjectMove);
            AddResponser(Id<MSG_GC_CHARACTER_ENTER_LIST>.Value, OnResponse_CharacterEnterList);
            AddResponser(Id<MSG_GC_HERO_ENTER_LIST>.Value, OnResponse_HeroEnterList);
            AddResponser(Id<MSG_GC_MONSTER_ENTER_LIST>.Value, OnResponse_MonsterList);
            AddResponser(Id<MSG_GC_BROADCAST_LIST>.Value, OnResponse_BroadcastList);
            AddResponser(Id<MSG_GC_INSTANCES_REMOVE>.Value, OnResponse_InstancesRemove);
            AddResponser(Id<MSG_ZGC_INTERACTION>.Value, OnResponse_Interaction);
            AddResponser(Id<MSG_ZGC_CHARACTER_STOP>.Value, OnResponse_CharacterStop);
            AddResponser(Id<MSG_ZGC_NPC_MOVE>.Value, OnResponse_NpcMove);
            AddResponser(Id<MSG_GC_NPC_ENTER_LIST>.Value, OnResponse_NPCEnterList);
            AddResponser(Id<MSG_ZGC_HIDDEN_WEAPON_INFO>.Value, OnResponse_WeaponInfo); 
            AddResponser(Id<MSG_ZGC_HERO_HIDDEN_WEAPON_INFO>.Value, OnResponse_HeroWeaponInfo);
            AddResponser(Id<MSG_GC_PET_ENTER_LIST>.Value, OnResponse_PetEnterList);

            // Chat
            AddResponser(Id<MSG_ZGC_CHAT_LIST>.Value, OnResponse_ChatList);
            //AddResponser(Id<MSG_ZGC_CHAT_TRUMPET>.Value, OnResponse_ChatTrumpet);
            AddResponser(Id<MSG_ZGC_CHAT_TRUMPET_RESULT>.Value, OnResponse_ChatTrumpetResult);
            AddResponser(Id<MSG_ZGate_BROADCAST_ANNOUNCEMENT>.Value, OnResponse_BroadcastAnnouncement);
            AddResponser(Id<MSG_ZGC_NEARBY_EMOJI>.Value, OnResponse_NearbyEmoji);
            AddResponser(Id<MSG_ZGC_TIP_OFF>.Value, OnResponse_TipOff);
            AddResponser(Id<MSG_ZGC_CHAT_BROADCAST>.Value, OnResponse_BroadcastChat);
            AddResponser(Id<MSG_ZGC_SILENCE>.Value, OnResponse_ChatSilence);
            AddResponser(Id<MSG_ZGC_ACTIVITY_CHAT_BUBBLE>.Value, OnResponse_ActivityChatBubble);
            AddResponser(Id<MSG_ZGate_GM>.Value, OnResponse_SetGm);
            AddResponser(Id<MSG_ZGC_CHECK_CHATLIMIT>.Value, OnResponse_CheckChatLimit);
            AddResponser(Id<MSG_ZGC_BUY_TRUMPET>.Value, OnResponse_BuyChatTrumpet);
            AddResponser(Id<MSG_ZGC_CHAT>.Value, OnResponse_Chat);
            AddResponser(Id<MSG_ZGC_NEW_BUBBLE_LIST>.Value, OnResponse_NewBubbleList);
            AddResponser(Id<MSG_ZGC_SENSITIVE_WORD>.Value, OnResponse_SensitiveWord);

            //Bag
            AddResponser(Id<MSG_ZGC_BAG_SYNC>.Value, OnResponse_SyncBag);
            AddResponser(Id<MSG_ZGC_BAG_UPDATE>.Value, OnResponse_UpdateBag);
            AddResponser(Id<MSG_ZGC_ITEM_USE>.Value, OnResponse_ItemUse);
            AddResponser(Id<MSG_ZGC_ITEM_USE_BATCH>.Value, OnResponse_ItemUseBatch);
            AddResponser(Id<MSG_ZGC_ITEM_SELL>.Value, OnResponse_ItemSell); 
            AddResponser(Id<MSG_ZGC_ITEM_BUY>.Value, OnResponse_ItemBuy);
            AddResponser(Id<MSG_ZGC_ITEM_COMPOSE>.Value, OnResponse_ItemCompose);
            AddResponser(Id<MSG_ZGC_ITEM_FORGE>.Value, OnResponse_ItemForge);
            AddResponser(Id<MSG_ZGC_ITEM_RESOLVE>.Value, OnResponse_ItemResolve);
            AddResponser(Id<MSG_ZGC_BAGSPACE_INC>.Value, OnResponse_BagSpaceInc);
            AddResponser(Id<MSG_ZGC_USE_FIREWORKS>.Value, OnResponse_UseFireworks);
            AddResponser(Id<MSG_ZGC_FIREWORK_REWARD>.Value, OnResponse_GetFireworkReward);
            AddResponser(Id<MSG_ZGC_ITEM_BATCH_RESOLVE>.Value, OnResponse_ItemBatchResolve);
            AddResponser(Id<MSG_ZGC_RECEIVE_ITEM>.Value, OnResponse_ReceiveItem);
            AddResponser(Id<MSG_ZGC_ITEM_EXCHANGE_REWARD>.Value, OnResponse_ItemExchangeReward);
            AddResponser(Id<MSG_ZGC_OPEN_CHOOSE_BOX>.Value, OnResponse_OpenChooseBox);

            //Soulbone
            AddResponser(Id<MSG_ZGC_SOULBONE_SMELT_RESULT>.Value, OnResponse_SmeltResult);
            AddResponser(Id<MSG_ZGC_EQUIP_SOULBONE_RESULT>.Value, OnResponse_EquipSoulBoneResult);

            AddResponser(Id<MSG_ZGC_EQUIP_FACEFRAME>.Value, OnResponse_EquipFaceFrame);
            AddResponser(Id<MSG_ZGC_SOULBONE_QUENCHING>.Value, OnResponse_SoulBoneQuenching);

            //Email
            AddResponser(Id<MSG_ZGC_EMAIL_OPENE_BOX>.Value, OnResponse_SyncTaskChangeMessage);
            AddResponser(Id<MSG_ZGC_EMAIL_REMIND>.Value, OnResponse_EmailRemaind);
            AddResponser(Id<MSG_ZGC_EMAIL_READ>.Value, OnResponse_EmailRead);
            AddResponser(Id<MSG_ZGC_PICKUP_ATTACHMENT>.Value, OnResponse_GetAttachment);
            AddResponser(Id<MSG_ZGC_PICKUP_ATTACHMENT_BATCH>.Value, OnResponse_GetAttachmentBatch);

            //TASK
            //AddResponser(Id<MSG_ZGC_TASK_LIST>.Value, OnResponse_SyncTaskListMessage);
            AddResponser(Id<MSG_ZGC_TASK_CHANGE>.Value, OnResponse_SyncTaskChange);
            AddResponser(Id<MSG_ZGC_GET_TASK_RESULT>.Value, OnResponse_SyncGetTaskResult);
            AddResponser(Id<MSG_ZGC_TASK_COLLECT>.Value, OnResponse_TaskCollectResult);
            AddResponser(Id<MSG_ZGC_TASK_COMPLETE>.Value, OnResponse_TaskCompleteResult);
            AddResponser(Id<MSG_ZGC_TASK_FINISH_STATE>.Value, OnResponse_TaskFinishState);
            AddResponser(Id<MSG_ZGC_TASK_FINISH_STATE_REWARD>.Value, OnResponse_TaskFinishStateReward);

            AddResponser(Id<MSG_ZGC_USE_TASKFLY_ANSWER>.Value, OnResponse_TaskFlyAnswer);
            AddResponser(Id<MSG_ZGC_TASKFLY_POSITION_SETDONE>.Value, OnResponse_TaskFlySetDone);
            //currencies
            AddResponser(Id<MSG_ZGC_SYNC_CURRENCIES>.Value, OnResponse_SyncCurrencies);
            AddResponser(Id<MSG_ZGC_FIRST_ADD_EXP>.Value, OnResponse_FirstAddExp);
            AddResponser(Id<MSG_ZGC_COUNTER_INFO>.Value, OnResponse_SyncCounter);
            AddResponser(Id<MSG_ZGC_COUNTER_BUY_COUNT>.Value, OnResponse_BuyCounter);
            AddResponser(Id<MSG_ZGC_GET_SPECIAL_COUNT>.Value, OnResponse_GetSpecialCount);          

            //Friend
            AddResponser(Id<MSG_ZGC_FRIEND_SEARCH>.Value, OnResponse_SearchFriend);
            AddResponser(Id<MSG_ZGC_FRIEND_RECOMMEND>.Value, OnResponse_RecommendFriend);
            AddResponser(Id<MSG_ZGC_FRIEND_ADD>.Value, OnResponse_FriendAdd);
            AddResponser(Id<MSG_ZGC_FRIEND_DELETE>.Value, OnResponse_FriendDelete);
            AddResponser(Id<MSG_ZGC_FRIEND_LIST>.Value, OnResponse_FriendList);
            AddResponser(Id<MSG_ZGC_FRIEND_BLACK_ADD>.Value, OnResponse_BlackAdd);
            AddResponser(Id<MSG_ZGC_FRIEND_BLACK_DEL>.Value, OnResponse_BlackDel);
            AddResponser(Id<MSG_ZGC_FRIEND_BLACK_LIST>.Value, OnResponse_FriendBlackList);
            AddResponser(Id<MSG_ZGC_FRIEND_RECENT_LIST>.Value, OnResponse_FriendRecentList);

            AddResponser(Id<MSG_ZGC_FRIEND_HEART_GIVE>.Value, OnResponse_FriendHeartGive);
            AddResponser(Id<MSG_ZGC_FRIEND_HEART_GIVE_COUNT>.Value, OnResponse_FriendHeartGiveCount);
            AddResponser(Id<MSG_ZGC_FRIEND_HEART_TAKE_COUNT>.Value, OnResponse_FriendHeartTakeCount);
            AddResponser(Id<MSG_ZGC_FRIEND_HEART_COUNT_BUY>.Value, OnResponse_FriendHeartCountBuy);
            AddResponser(Id<MSG_ZGC_REPAY_FRIENDS_HEART>.Value, OnResponse_RepayFriendsHeart);
            
            //Photo
            AddResponser(Id<MSG_ZGC_UPLOAD_PHOTO>.Value, OnResponse_UploadPhoto);
            AddResponser(Id<MSG_ZGC_REMOVE_PHOTO>.Value, OnResponse_RemovePhoto);
            AddResponser(Id<MSG_ZGC_PHOTO_LIST>.Value, OnResponse_PhotoList);
            AddResponser(Id<MSG_ZGC_POP_RANK>.Value, OnResponse_PopRank);

            //Show
            AddResponser(Id<SHOW_SPACEINFO>.Value, OnResponse_SpaceInfo);
            AddResponser(Id<MSG_ZGC_SHOW_PLAYER>.Value, OnResponse_ShowPlayer);
            AddResponser(Id<MSG_ZGC_NOTIFY_PLAYER_SHOW>.Value, OnResponse_NotifyPlayerShow);
            AddResponser(Id<MSG_ZGC_SHOW_FACEICON>.Value, OnResponse_ShowFaceIcon);
            AddResponser(Id<MSG_ZGC_SHOW_FACEJPG>.Value, OnResponse_ShowFaceJpg);
            AddResponser(Id<MSG_ZGC_CHANGE_NAME>.Value, OnResponse_ChangeName);
            AddResponser(Id<MSG_ZGC_SET_SEX>.Value, OnResponse_SetSex);
            AddResponser(Id<MSG_ZGC_SET_BIRTHDAY>.Value, OnResponse_SetBirthday);

            AddResponser(Id<MSG_ZGC_SET_SIGNATURE>.Value, OnResponse_SetSignature);
            AddResponser(Id<MSG_ZGC_SET_SOCIAL_NUM>.Value, OnResponse_SetWQ);
            AddResponser(Id<MSG_ZGC_GET_SOCIAL_NUM>.Value, OnResponse_GetWQ);
            AddResponser(Id<MSG_ZGC_SHOW_VOICE>.Value, OnResponse_ShowVoice);
            AddResponser(Id<MSG_ZGC_PRESENT_GIFT>.Value, OnResponse_PresentGift);
            AddResponser(Id<MSG_ZGC_GET_GIFTRECORD>.Value, OnResponse_GetGiftRecord);
            AddResponser(Id<MSG_ZGC_SHOW_CAREER>.Value, OnResponse_ShowCareer);
            AddResponser(Id<MSG_ZGC_RANKING_FRIEND_LIST>.Value, OnResponse_GetRankingFriendList);
            AddResponser(Id<MSG_ZGC_RANKING_ALL_LIST>.Value, OnResponse_GetRankingAllList);
            AddResponser(Id<MSG_ZGC_RANKING_NEARBY_LIST>.Value, OnResponse_GetRankingNearbyList);

            AddResponser(Id<MSG_ZGC_UPDATE_SOME_SHOW>.Value, OnResponse_UpdateSomeShow);
            //称号
            AddResponser(Id<MSG_ZGC_CHANGE_TITLE_ANSWER>.Value, OnResponse_ChangeTitleAnswer);
            AddResponser(Id<MSG_ZGC_TITLE_INFO>.Value, OnResponse_TitleChanged);
            AddResponser(Id<MSG_ZGC_NEW_TITLE>.Value, OnResponse_GetNewTitle);
            AddResponser(Id<MSG_ZGC_TITLE_CONDITION_COUNT>.Value, OnResponse_GetTitleConditionCount);
            AddResponser(Id<MSG_ZGC_LOOK_TITLE>.Value, OnResponse_LookTitle);

            AddResponser(Id<MSG_ZGC_WELFARE_TRIGGER_CHANGE>.Value, OnResponse_WelfareTriggerChange);

            // 切线
            AddResponser(Id<MSG_ZGC_CHANGE_CHANNEL>.Value, OnResponse_ChangeChannel);
            AddResponser(Id<MSG_ZGate_LOGIN_OTHER_MANAGER>.Value, OnResponse_LoginOtherManager);
            AddResponser(Id<MSG_GC_RECONNECT_LOGIN>.Value, OnResponse_ReconnectLogin);
            AddResponser(Id<MSG_ZGC_KICK>.Value, OnResponse_Kick);

            //Guild
            AddResponser(Id<MSG_ZGC_CREATE_GUILD>.Value, OnResponse_CreateGuild);

            //recharge
            AddResponser(Id<MSG_ZGC_RECHARGE_HISTORY>.Value, OnResponse_GetRechargeHistory);
            AddResponser(Id<MSG_ZGC_RECHARGE_MANAGER>.Value, OnResponse_RechargeManager);
            AddResponser(Id<MSG_ZGC_GET_ORDER_ID>.Value, OnResponse_RechargeHistoryId);
            AddResponser(Id<MSG_ZGC_GET_ACCUMULATE_RECHARGE_REWARD>.Value, OnResponse_GetAccumulateRechargeReward);
            AddResponser(Id<MSG_ZGC_GET_NEW_RECHARGE_GIFT_REWARD>.Value, OnResponse_GetNewRechargeGiftAccumulateReward);

            //Activity
            AddResponser(Id<MSG_ZGC_ACTIVITY_LIST>.Value, OnResponse_SyncActivityListMessage);
            AddResponser(Id<MSG_ZGC_ACTIVITY_CHANGE>.Value, OnResponse_SyncActivityChange);
            AddResponser(Id<MSG_ZGC_ACTIVITY_COMPLETE>.Value, OnResponse_ActivityCompleteResult);
            AddResponser(Id<MSG_ZGC_QUESTIONNAIRE_COMPLETE>.Value, OnResponse_QuestionnaireCompleteResult);
            AddResponser(Id<MSG_ZGC_ACTIVITY_TYPE_COMPLETE>.Value, OnResponse_ActivityTypeCompleteResult);
            AddResponser(Id<MSG_ZGC_ACTIVITY_RELATED_COMPLETE>.Value, OnResponse_ActivityRelatedCompleteResult);
            AddResponser(Id<MSG_ZGC_RECHARGE_REBATE_INFO>.Value, OnResponse_RechargeRebateInfo);
            AddResponser(Id<MSG_ZGC_RECHARGE_REBATE_GET_REWARD>.Value, OnResponse_RechargeRebateReward);
            AddResponser(Id<MSG_ZGC_SPECIAL_ACTIVITY_MANAGER>.Value, OnResponse_SyncSpceilActivityListMessage);
            AddResponser(Id<MSG_ZGC_SPECIAL_ACTIVITY_CHANGE>.Value, OnResponse_SyncSpceilActivityChange);
            AddResponser(Id<MSG_ZGC_SPECIAL_ACTIVITY_COMPLETE>.Value, OnResponse_SpecialActivityComplete);
            AddResponser(Id<MSG_ZGC_RUNAWAY_ACTIVITY_MANAGER>.Value, OnResponse_SyncRunawayActivityListMessage);
            AddResponser(Id<MSG_ZGC_RUNAWAY_ACTIVITY_CHANGE>.Value, OnResponse_SyncRunawayActivityChange);
            AddResponser(Id<MSG_ZGC_RUNAWAY_ACTIVITY_COMPLETE>.Value, OnResponse_RunawayActivityComplete);
            AddResponser(Id<MSG_ZGC_WEBPAY_RECHARGE_REBATE>.Value, OnResponse_WebPayRecbargeRebateInfo);
            AddResponser(Id<MSG_ZGC_GET_WEBPAY_REBATE_REWARD>.Value, OnResponse_GetWebPayRebateReward);

            //DailyQuestion            
            AddResponser(Id<MSG_ZGC_DAILY_QUESTION_COUNTER>.Value, OnResponse_DailyQuestionCounter);
            AddResponser(Id<MSG_ZGC_DAILY_QUESTION_REWARD>.Value, OnResponse_DailyQuestionReward);
           
            //Radio
            AddResponser(Id<MSG_ZGC_RADIO_ALL_ANCHOR_RANK>.Value, OnResponse_RadioAllAnchorRank);
            AddResponser(Id<MSG_ZGC_RADIO_ANCHOR_CONTRIBUTION_RANK>.Value, OnResponse_RadioAnchorContributionRank);
            AddResponser(Id<MSG_ZGC_RADIO_ALL_CONTRIBUTION_RANK>.Value, OnResponse_RadioAllContributionRank);
            AddResponser(Id<MSG_ZGC_RADIO_CONTRIBUTION_REWARD>.Value, OnResponse_RadioContributionReward);
            AddResponser(Id<MSG_ZGC_RADIO_GIFT>.Value, OnResponse_RadioGift);

            //Pet
            AddResponser(Id<MSG_ZGC_CALL_PET>.Value, OnResponse_CallPet);
            AddResponser(Id<MSG_ZGC_RECALL_PET>.Value, OnResponse_RecallPet);
            AddResponser(Id<MSG_ZGC_PET_LIST>.Value, OnResponse_PetInfoList);
            AddResponser(Id<MSG_ZGC_PET_EGG_LIST>.Value, OnResponse_PetEggList);
            AddResponser(Id<MSG_ZGC_UPDATE_PET_EGG>.Value, OnResponse_UpdatePetEgg);
            AddResponser(Id<MSG_ZGC_HATCH_PET_EGG>.Value, OnResponse_HatchPetEgg);
            AddResponser(Id<MSG_ZGC_FINISH_HATCH_PET_EGG>.Value, OnResponse_FinishHatchPetEgg);
            AddResponser(Id<MSG_ZGC_RELEASE_PET>.Value, OnResponse_ReleasePet);
            AddResponser(Id<MSG_ZGC_SHOW_PET_NATURE>.Value, OnResponse_ShowPetNature);
            AddResponser(Id<MSG_ZGC_PETS_CHANGE>.Value, OnResponse_PetsChange);
            AddResponser(Id<MSG_ZGC_PET_LEVEL_UP>.Value, OnResponse_PetLevelUp);
            AddResponser(Id<MSG_ZGC_UPDATE_MAINQUEUE_PET>.Value, OnResponse_UpdateMainQueuePet);
            AddResponser(Id<MSG_ZGC_PET_INHERIT>.Value, OnResponse_PetInherit);
            AddResponser(Id<MSG_ZGC_PET_SKILL_BAPTIZE>.Value, OnResponse_PetSkillBaptize);
            AddResponser(Id<MSG_ZGC_PET_BREAK>.Value, OnResponse_PetBreak);
            AddResponser(Id<MSG_ZGC_ONE_KEY_PET_BREAK>.Value, OnResponse_OneKeyPetBreak);
            AddResponser(Id<MSG_ZGC_PET_BLEND>.Value, OnResponse_PetBlend);
            AddResponser(Id<MSG_ZGC_PET_FEED>.Value, OnResponse_PetFeed);
            AddResponser(Id<MSG_ZGC_UPDATE_PET_DUNGEON_QUEUE>.Value, OnResponse_UpdatePetDungeonQueue);
            AddResponser(Id<MSG_ZGC_PET_DUNGEON_QUEUE_LIST>.Value, OnResponse_PetDungeonQueueList);

            //Hero
            AddResponser(Id<MSG_ZGC_HERO_LIST>.Value, OnResponse_HeroList);
            AddResponser(Id<MSG_ZGC_HERO_CHANGE>.Value, OnResponse_HeroChange);
            AddResponser(Id<MSG_ZGC_HERO_LEVEL_UP>.Value, OnResponse_HeroLevelUp);
            AddResponser(Id<MSG_ZGC_HERO_AWAKEN>.Value, OnResponse_HeroAwaken);
            AddResponser(Id<MSG_ZGC_CALL_HERO>.Value, OnResponse_CallHero);
            AddResponser(Id<MSG_ZGC_RECALL_HERO>.Value, OnResponse_RecallHero);
            AddResponser(Id<MSG_ZGC_HERO_CHANGE_FOLLOWER>.Value, OnResponse_ChangeFollower);
            AddResponser(Id<MSG_ZGC_EQUIP_HERO_RESULT>.Value, OnResponse_EquipHeroResult);
            AddResponser(Id<MSG_ZGC_HERO_STEPS_UP>.Value, OnResponse_HeroStepsUp);
            AddResponser(Id<MSG_ZGC_HERO_GOD_STEPS_UP>.Value, OnResponse_HeroGodStepsUp);
            AddResponser(Id<MSG_ZGC_ONEKEY_HERO_STEPS_UP>.Value, OnResponse_OnekeyHeroStepsUp);
            AddResponser(Id<MSG_ZGC_HERO_TITLE_UP>.Value, OnResponse_HeroTitleUp);
            AddResponser(Id<MSG_ZGC_HERO_REVERT>.Value, OnResponse_HeroRevert);
            AddResponser(Id<MSG_ZGC_MAIN_HERO_CHANGE>.Value, OnResponse_ChangeMainHero);
            AddResponser(Id<MSG_ZGC_UPDATE_HERO_POS_RESULT>.Value, OnResponse_UpdateHeroPos);
            AddResponser(Id<MSG_ZGC_INIT_HERO_POS>.Value, OnResponse_InitHeroPos);
            AddResponser(Id<MSG_ZGC_MAINQUEUE_INFO>.Value, OnResponse_MainBattleQueueInfo);
            AddResponser(Id<MSG_ZGC_UPDATE_MAINQUEUE_HEROPOS>.Value, OnResponse_UpdateMainQueueHeroPos);
            AddResponser(Id<MSG_ZGC_UNLOCK_MAINQUEUE>.Value, OnResponse_UnlockMainQueue);
            AddResponser(Id<MSG_ZGC_CHANGE_MAINQUEUE_NAME>.Value, OnResponse_ChangeMainQueueName);
            AddResponser(Id<MSG_ZGC_MAINQUEUE_DISPATCH_BATTLE>.Value, OnResponse_MainQueueDispatchBattle);
            AddResponser(Id<MSG_ZGC_HERO_INHERIT>.Value, OnResponse_HeroInherit);

            AddResponser(Id<MSG_ZGC_HERO_GOD_INFO_LIST>.Value, OnResponse_HeroGodList);
            AddResponser(Id<MSG_ZGC_HERO_GOD_INFO>.Value, OnResponse_HeroGodInfo);
            AddResponser(Id<MSG_ZGC_HERO_GOD_UNLOCK>.Value, OnResponse_HeroGodUnlock);
            AddResponser(Id<MSG_ZGC_HERO_GOD_EQUIP>.Value, OnResponse_HeroGodEquip);

            //Skill
            AddResponser(Id<MSG_ZGC_SKILL_ALARM>.Value, OnResponse_SkillAlarm);
            AddResponser(Id<MSG_ZGC_SKILL_START>.Value, OnResponse_SkillStart);
            AddResponser(Id<MSG_ZGC_SKILL_EFF>.Value, OnResponse_SkillEff);
            AddResponser(Id<MSG_ZGC_SKILL_ENERGY_LIST>.Value, OnResponse_SkillEnergyList);
            AddResponser(Id<MSG_ZGC_SKILL_ENERGY>.Value, OnResponse_SkillEnergy);
            AddResponser(Id<MSG_ZGC_HERO_SKILL_READY>.Value, OnResponse_HeroSkillReady);
            AddResponser(Id<MSG_ZGC_DAMAGE>.Value, OnResponse_Damage);
            AddResponser(Id<MSG_ZGC_CAST_HERO_SKILL>.Value, OnResponse_CastHeroSkill);
            AddResponser(Id<MSG_ZGC_HATE_INFO>.Value, OnResponse_HateInfo);
            AddResponser(Id<MSG_ZGC_REALBODY_TIME>.Value, OnResponse_RealBodyTime);
            AddResponser(Id<MSG_ZGC_ADD_NORMAL_SKILL_ENERGY>.Value, OnResponse_AddNormalSkillEnergy);
            AddResponser(Id<MSG_ZGC_MARK>.Value, OnResponse_Mark);
            AddResponser(Id<MSG_ZGC_MIX_SKILL>.Value, OnResponse_MixSkill);
            AddResponser(Id<MSG_ZGC_MIX_SKILL_EFFECT>.Value, OnResponse_MixSkillEffect);
            AddResponser(Id<MSG_ZGC_BUFF_SPEC_END>.Value, OnResponse_BuffSpecEnd);
            AddResponser(Id<MSG_ZGC_CAST_SKILL>.Value, OnResponse_CastSkill);
            AddResponser(Id<MSG_ZGC_STARTFIGHTING>.Value, OnResponse_BattleStart);
            AddResponser(Id<MSG_ZGC_SKILL_ENERGY_CHANGE>.Value, OnResponse_SkillEnergyChange);
            

            //Camp
            AddResponser(Id<MSG_ZGC_CHOOSE_CAMP_RESULT>.Value, OnResponse_ChooseCampResult);
            AddResponser(Id<MSG_ZGC_GET_CAMP_REWARD>.Value, OnResponse_CampRewardResult);
            AddResponser(Id<MSG_ZGC_CAMP_WORSHIP_RESULT>.Value, OnResponse_WorshipResult);
            AddResponser(Id<MSG_ZGC_CAMP_VOTE_RESULT>.Value, OnResponse_CampVoteResult);
            AddResponser(Id<MSG_ZGC_RUN_IN_ELECTION_RESULT>.Value, OnResponse_CampRunInElection);
            AddResponser(Id<MSG_ZGC_CAMP_INFO>.Value, OnResponse_ShowCampInfos);
            AddResponser(Id<MSG_ZGC_CAMP_BASE>.Value, OnResponse_SendCampBaseInfo);
            AddResponser(Id<MSG_ZGC_CAMP_PANEL_INFO>.Value, OnResponse_ShowCampPanelInfos);
            AddResponser(Id<MSG_ZGC_CAMP_ELECTION_INFO>.Value, OnResponse_ShowElectionInfos);

            AddResponser(Id<MSG_ZGC_GET_STARLEVEL>.Value, OnResponse_GetStarLevel);
            AddResponser(Id<MSG_ZGC_STAR_LEVELUP>.Value, OnResponse_CampStarLevelUp);
            AddResponser(Id<MSG_ZGC_CAMP_GATHER>.Value, OnResponse_CampGather);
            AddResponser(Id<MSG_ZGC_GATHER_DIALOGUE_COMPLETE>.Value, OnResponse_GatherDialogueComplete);
            AddResponser(Id<MSG_ZGC_CAMP_WORSHIP_SHOW>.Value, OnResponse_CampWorshipShow);
            AddResponser(Id<MSG_ZGC_CAMP_WORSHIP_SHOW_UPDATE>.Value, OnResponse_CampWorshipShowUpdate);
            

            //Team
            AddResponser(Id<MSG_ZGC_TEAM_TYPE_LIST>.Value, OnResponse_TeamTypeList);
            AddResponser(Id<MSG_ZGC_CREATE_TEAM>.Value, OnResponse_CreateTeam); 
            AddResponser(Id<MSG_ZGC_JOIN_TEAM>.Value, OnResponse_JoinTeam);
            AddResponser(Id<MSG_ZGC_NEW_TEAM_MEMBER_JOIN>.Value, OnResponse_NewMemberJoinTeam);
            AddResponser(Id<MSG_ZGC_TEAM_MEMBER_LEAVE>.Value, OnResponse_LeaveTeam);
            AddResponser(Id<MSG_ZGC_QUIT_TEAM>.Value, OnResponse_QuitTeam);
            AddResponser(Id<MSG_ZGC_KICK_TEAM_MEMBER>.Value, OnResponse_KickTeamMember); 
            AddResponser(Id<MSG_ZGC_TRANSFER_CAPTAIN>.Value, OnResponse_TransferCaptain); 
            AddResponser(Id<MSG_ZGC_CAPTAIN_CHANGE>.Value, OnResponse_CaptainChange);
            AddResponser(Id<MSG_ZGC_TEAM_MEMBER_OFFLINE>.Value, OnResponse_TeamMemberOffline);
            AddResponser(Id<MSG_ZGC_TEAM_MEMBER_ONLINE>.Value, OnResponse_TeamMemberOnline);
            AddResponser(Id<MSG_ZGC_ASK_JOIN_TEAM>.Value, OnResponse_AskJoinTeam);
            AddResponser(Id<MSG_ZGC_INVITE_JOIN_TEAM>.Value, OnResponse_InviteJoinTeam);
            AddResponser(Id<MSG_ZGC_ASK_INVITE_JOIN_TEAM>.Value, OnResponse_AskInviteJoinTeam);
            AddResponser(Id<MSG_ZGC_ANSWER_INVITE_JOIN_TEAM>.Value, OnResponse_AnswerInviteJoinTeam);
            AddResponser(Id<MSG_ZGC_CHANGE_TEAM_TYPE>.Value, OnResponse_ChangeTeamType);
            AddResponser(Id<MSG_ZGC_TEAM_RELIVE_TEAMMEMBER>.Value, OnResponse_ReliveTeamMember);
            AddResponser(Id<MSG_ZGC_NEED_TEAM_HELP>.Value, OnResponse_NeedTeamHelp);
            AddResponser(Id<MSG_ZGC_REQUEST_TEAM_HELP>.Value, OnResponse_RequestTeamHelp);
            AddResponser(Id<MSG_ZGC_RESPONSE_TEAM_HELP>.Value, OnResponse_ResponseTeamHelp);
            AddResponser(Id<MSG_ZGC_FOLLOW_CAPTAIN>.Value, OnResponse_ResponseFlowCaptain);
            AddResponser(Id<MSG_ZGC_TRY_FOLLOW_CAPTAIN>.Value, OnResponse_ResponseTryFlowCaptain);
            AddResponser(Id<MSG_ZGC_RELIVE_HERO>.Value, OnResponse_ReliveHero);
            AddResponser(Id<MSG_ZGC_INVITE_FRIEND_JOIN_TEAM>.Value, OnResponse_InviteFriendJoinTeam);
            AddResponser(Id<MSG_ZGC_QUIT_TEAM_INDUNGEON>.Value, OnResponse_QuitTeamInDungeon);

            //Equipment
            AddResponser(Id<MSG_ZGC_EQUIP_EQUIPMENT_RESULT>.Value, OnResponse_EquipEquipmentResult);
            AddResponser(Id<MSG_ZGC_UPGRADE_EQUIPMENT_RESULT>.Value, OnResponse_UpgradeEquipmentResult);
            AddResponser(Id<MSG_ZGC_UPGRADE_EQUIPMENT_DIRECTLY>.Value, OnResponse_UpgradeSlotDirectly);
            AddResponser(Id<MSG_ZGC_EQUIPMENT_INJECTION_RESULT>.Value, OnResponse_InjectEquipment);
            AddResponser(Id<MSG_ZGC_EQUIP_BETTER_EQUIPMENT>.Value, OnResponse_EquipBetterEquipment);
            AddResponser(Id<MSG_ZGC_RETURN_UPGRADE_EQUIPMENT_COST>.Value, OnResponse_ReturnUpgradeEquipmentCost);
            AddResponser(Id<MSG_ZGC_EQUIPMENT_ADVANCE>.Value, OnResponse_EquipmentAdvance);
            AddResponser(Id<MSG_ZGC_EQUIPMENT_ADVANCE_ONE_KEY>.Value, OnResponse_EquipmentAdvanceOneKey);
            AddResponser(Id<MSG_ZGC_JEWEL_ADVANCE>.Value, OnResponse_JewelAdvance);
            AddResponser(Id<MSG_ZGC_EQUIPMENT_ENCHANT>.Value, OnResponse_EquipEnchant);

            //soulring
            AddResponser(Id<MSG_ZGC_ABSORB_SOULRING>.Value, OnResponse_AbsorbSoulRing);
            AddResponser(Id<MSG_ZGC_HELP_ABSORB_SOULRING>.Value, OnResponse_HelpAbsorbSoulRing);
            AddResponser(Id<MSG_ZGC_GET_ABSORBINFO>.Value, OnResponse_AbsorbInfo);
            AddResponser(Id<MSG_ZGC_CANCEL_ABSORB>.Value, OnResponse_CancelAbsorb);
            AddResponser(Id<MSG_ZGC_ABSORB_FINISH>.Value, OnResponse_FinishAbsorb);

            AddResponser(Id<MSG_ZGC_GET_HELP_THANKS_LIST>.Value, OnResponse_GetHelpThanksList);
            AddResponser(Id<MSG_ZGC_THANK_FRIEND>.Value, OnResponse_ThankFriend);
            AddResponser(Id<MSG_ZGC_ENHANCE_SOULRING>.Value, OnResponse_EnhanceSoulRing);
            AddResponser(Id<MSG_ZGC_ONEKEY_ENHANCE_SOULRING>.Value, OnResponse_OneKeyEnhanceSoulRing);

            AddResponser(Id<MSG_ZGC_GET_All_ABSORBINFO>.Value, OnResponse_GetAllAbsorbInfo);
            AddResponser(Id<MSG_ZGC_GET_FRIEND_INFO>.Value, OnResponse_GetAbsorbFriendInfo);
            AddResponser(Id<MSG_ZGC_SHOW_HERO_SOULRING>.Value, OnResponse_ShowHeroSoulRing);
            AddResponser(Id<MSG_ZGC_REPLACE_BETTER_SOULRING>.Value, OnResponse_ReplaceBetterSoulRing);
            AddResponser(Id<MSG_ZGC_SELECT_SOULRING_ELEMENT>.Value, OnResponse_SelectSoulRingElement);

            //Dungeon 
            AddResponser(Id<MSG_ZGC_CREATE_DUGEON>.Value, OnResponse_CreateDungeon); 
            AddResponser(Id<MSG_ZGC_DUNGEON_REWARD>.Value, OnResponse_DungeonReward);
            AddResponser(Id<MSG_ZGC_DUNGEON_STOPTIME>.Value, OnResponse_DungeonStopTime);
            AddResponser(Id<MSG_ZGC_TEAM_DUNGEON_ENERGY_INFO>.Value, OnResponse_EnergyInfo);
            AddResponser(Id<MSG_ZGC_NPC_APPEAR>.Value, OnResponse_NPCAppear);
            AddResponser(Id<MSG_ZGC_NPC_DISAPPEAR>.Value, OnResponse_NPCDisappear);
            AddResponser(Id<MSG_ZGC_REENTER_DUNGEON>.Value, OnResponse_ReEnterDungeon);
            AddResponser(Id<MSG_ZGC_REVIVE>.Value, OnResponse_Revive);
            AddResponser(Id<MSG_ZGC_BATTLE_END_TIME>.Value, OnResponse_BattleEndTime);
            AddResponser(Id<MSG_ZGC_DUNGEON_START>.Value, OnResponse_DungeonStartTime);
            AddResponser(Id<MSG_ZGC_DUNGEON_LOADINGDONE>.Value, OnResponse_DungeonLoadingDone);
            AddResponser(Id<MSG_ZGC_TEAMMEMBER_LOADINGDONE>.Value, OnResponse_TeamMemberLoadingDone);
            AddResponser(Id<MSG_ZGC_DUNGEON_BATTLE_STAGE>.Value, OnResponse_BattleStageChange);
            AddResponser(Id<MSG_ZGC_DUNGEON_EQUIPED_HERO>.Value, OnResponse_DungeonHeroInfo);
            AddResponser(Id<MSG_ZGC_MONSTER_GENERATED_WALK>.Value, OnResponse_MonsterGeneratedWalk);
            AddResponser(Id<MSG_ZGC_DUNGEON_BATTLE_DATA>.Value, OnResponse_DungeonBattleData);
            AddResponser(Id<MSG_ZGC_REQUEST_QUIT_DUNGEON>.Value, OnResponse_RequestQuitDungeon);
            AddResponser(Id<MSG_ZGC_RSPONSE_VERIFY_QUIT_DUNGEON>.Value, OnResponse_ResponseVerifyQuitDungeon);
            AddResponser(Id<MSG_ZGC_DUNGEON_SPEED_UP>.Value, OnResponse_DungeonSpeedUp);
            AddResponser(Id<MSG_ZGC_DUNGEON_SPEEDUP_END>.Value, OnResponse_DungeonSpeedUpEnd);
            AddResponser(Id<MSG_ZGC_DUNGEON_SKIP_BATTLE>.Value, OnResponse_DungeonSkipBattle);

            //Nature
            AddResponser(Id<MSG_ZGC_HERO_NATURE>.Value, OnResponse_GetHeroNature);
            AddResponser(Id<MSG_ZGC_HERO_BATTLEPOWER>.Value, OnResponse_SendBattlePower);
            AddResponser(Id<MSG_ZGC_GET_HERO_POWER>.Value, OnResponse_GetHeroPower);

            //Delegation
            AddResponser(Id<MSG_ZGC_DELEGATION_LIST>.Value, OnResponse_DelegationList);
            AddResponser(Id<MSG_ZGC_DELEGATE_HEROS>.Value, OnResponse_DelegateHeros);
            AddResponser(Id<MSG_ZGC_COMPLETE_DELEGATION>.Value, OnResponse_CompleteDelegation);
            AddResponser(Id<MSG_ZGC_DELEGATION_REWARDS>.Value, OnResponse_GetDelegationRewards);
            AddResponser(Id<MSG_ZGC_DELEGATION_DAILY_REFRESH>.Value, OnResponse_DelegationDailyRefresh);
            AddResponser(Id<MSG_ZGC_REFRESH_DELEGATION>.Value, OnResponse_RefreshDelegation);
            AddResponser(Id<MSG_ZGC_BUY_DELEGATION_COUNT>.Value, OnResponse_BuyDelegationCount);

            //Hunting
            AddResponser(Id<MSG_ZGC_HUNTING_INFO>.Value, OnResponse_HuntingInfo);
            AddResponser(Id<MSG_ZGC_BATTLE_START_TIME>.Value, OnResponse_BattleStartTime);
            AddResponser(Id<MSG_ZGC_HUNTING_DROP_SOULRING>.Value, OnResponse_HuntingDropSoulRing);
            AddResponser(Id<MSG_ZGC_HUNTING_CHALLENBGE_COUNT>.Value, OnResponse_HuntingChallangeCount);
            AddResponser(Id<MSG_ZGC_HUNTING_SWEEP>.Value, OnResponse_HuntingSweep);
            AddResponser(Id<MSG_ZGC_CONTINUE_HUNTING>.Value, OnResponse_ContinueHunting);
            AddResponser(Id<MSG_ZGC_MEMBER_LEAVE_MAP>.Value, OnResponse_MemberLeaveMap);
            AddResponser(Id<MSG_ZGC_NOTIFY_CAPTAIN_MEMBERLEAVE>.Value, OnResponse_NotifyCaptainMemberLeave);
            AddResponser(Id<MSG_ZGC_HUNTING_ACTICITY_UNLOCK>.Value, OnResponse_HuntingActivityUnlock);
            AddResponser(Id<MSG_ZGC_HUNTING_ACTICITY_SWEEP>.Value, OnResponse_HuntingActivitySweep);
            AddResponser(Id<MSG_ZGC_HUNTING_HELP>.Value, OnResponse_HuntingHelp);
            AddResponser(Id<MSG_ZGC_HUNTING_HELP_ASK>.Value, OnResponse_HuntingHelpAsk);
            AddResponser(Id<MSG_ZGC_HUNTING_HELP_ANSWER_JOIN>.Value, OnResponse_HuntingHelpAnswerJoin);
            AddResponser(Id<MSG_ZGC_HUNTING_INTRUDE_INFO>.Value, OnResponse_HuntingintrudeInfo);
            AddResponser(Id<MSG_ZGC_HUNTING_INTRUDE_CHALLENGE>.Value, OnResponse_HuntingintrudeChallenge);
            AddResponser(Id<MSG_ZGC_HUNTING_INTRUDE_HERO_POS>.Value, OnResponse_HuntingIntrudeUpdateHeroPos);

            //Integral 
            AddResponser(Id<MSG_ZGC_INTERGRAL_BOSS_INFO>.Value, OnResponse_IntegralBossInfo);
            AddResponser(Id<MSG_ZGC_INTERGRAL_BOSS_STATE>.Value, OnResponse_IntegralBossState);
            AddResponser(Id<MSG_ZGC_INTERGRAL_BOSS_KILLINFO>.Value, OnResponse_IntegralBossKillInfo);

            //Level
            AddResponser(Id<MSG_ZGC_PLAYER_LEVEL>.Value, OnResponse_PlayerLevel);

            //竞技场
            AddResponser(Id<MSG_ZGC_ARENA_MANAGER>.Value, OnResponse_ArenaManager);
            AddResponser(Id<MSG_ZGC_SAVE_DEFEMSIVE>.Value, OnResponse_SaveArenaDefensive);
            AddResponser(Id<MSG_ZGC_RESET_ARENA_FIGHT_TIME>.Value, OnResponse_ResetArenaFightTime);
            AddResponser(Id<MSG_ZGC_GET_RANK_LEVEL_REWARD>.Value, OnResponse_GetRankLevelReward);
            AddResponser(Id<MSG_ZGC_GET_ARENA_CHALLENGERS>.Value, OnResponse_GetArenaChallenger);
            AddResponser(Id<MSG_ZGC_ARENA_RANK_INFO_LIST>.Value, OnResponse_ArenaRankInfoList);
            AddResponser(Id<MSG_ZGC_ARENA_CHALLENGER_HERO_INFO>.Value, OnResponse_ShowArenaChallengerInfo);
            AddResponser(Id<MSG_ZGC_CHALLENGER_RANK_CHANGE>.Value, OnResponse_ChallengerRankChange);

            //秘境  
            AddResponser(Id<MSG_ZGC_SECRET_AREA_INFO>.Value, OnResponse_SecretAreaInfo);
            AddResponser(Id<MSG_ZGC_SECRET_AREA_SWEEP>.Value, OnResponse_SecretAreaSweep);
            AddResponser(Id<MSG_ZGC_SECRET_AREA_RANK_LIST>.Value, OnResponse_SecretAreaRankInfo);
            AddResponser(Id<MSG_ZGC_SECRET_AREA_CONT_FIGHT>.Value, OnResponse_SecretAreaContinueFight);

            //Shop
            AddResponser(Id<MSG_ZGC_SHOP_INFO>.Value, OnResponse_ShopInfo);
            AddResponser(Id<MSG_ZGC_SHOP_BUY>.Value, OnResponse_ShopBuyItem);
            AddResponser(Id<MSG_ZGC_SHOP_REFRESH>.Value, OnResponse_ShopRefresh);
            AddResponser(Id<MSG_ZGC_SHOP_SOULBONE_BONUS>.Value, OnResponse_ShopSoulBoneBonus);
            AddResponser(Id<MSG_ZGC_SHOP_SOULBONE_REWARD>.Value, OnResponse_ShopSoulBoneReward);
            AddResponser(Id<MSG_ZGC_SHOP_DAILY_REFRESH>.Value, OnResponse_ShopDailyRefresh);
            AddResponser(Id<MSG_ZGC_BUY_SHOP_ITEM>.Value, OnResponse_BuyShopItem);

            //通行证
            AddResponser(Id<MSG_ZGC_PASSCARD_RECHARGE_RESULT>.Value,OnResponse_PasscardRecharged);
            AddResponser(Id<MSG_ZGC_PASSCARD_PANEL_INFO>.Value, OnResponse_PasscardPanelInfo);
            AddResponser(Id<MSG_ZGC_PASSCARD_LEVEL_REWARD_RESULT>.Value, OnResponse_PasscardLevelReward);
            AddResponser(Id<MSG_ZGC_PASSCARD_DAILY_REWARD_RESULT>.Value, OnResponse_PasscardDailyReward);
            AddResponser(Id<MSG_ZGC_PASSCARD_RECHARGE_LEVEL_RESULT>.Value, OnResponse_PasscardRechargeLevel);
            AddResponser(Id<MSG_ZGC_UPDATE_PASSCARD_TASK>.Value, OnResponse_PasscardUpdateTask);

            //抽卡
            AddResponser(Id<MSG_ZGC_DRAW_HERO>.Value, OnResponse_DrawHero);
            AddResponser(Id<MSG_ZGC_ACTIVATE_HERO_COMBO>.Value, OnResponse_ActivateHeroCombo);
            AddResponser(Id<MSG_ZGC_DRAW_MANAGER>.Value, OnResponse_DrawManager);

            //章节 
            AddResponser(Id<MSG_ZGC_CHAPTER_INFO>.Value, OnResponse_ChapterInfo);
            AddResponser(Id<MSG_ZGC_CHAPTER_NEXT_PAGE>.Value, OnResponse_ChapterNextPage);
            AddResponser(Id<MSG_ZGC_CHAPTER_REWARD>.Value, OnResponse_ChapterReward);
            AddResponser(Id<MSG_ZGC_CHAPTER_SWEEP>.Value, OnResponse_ChapterSweep);
            AddResponser(Id<MSG_ZGC_CHAPTER_BUY_POWER>.Value, OnResponse_BuyTimeSpacePower);
            AddResponser(Id<MSG_ZGC_CHAPTER_REWATRD_REDDOT>.Value, OnResponse_ChapterRewardReddot);

            //魂师试炼 
            AddResponser(Id<MSG_ZGC_BENEFIT_INFO>.Value, OnResponse_BenefitInfo);
            AddResponser(Id<MSG_ZGC_BENEFIT_SWEEP>.Value, OnResponse_BenefitSweep);

            //成神之路 
            AddResponser(Id<MSG_ZGC_GOD_HERO_INFO>.Value, OnResponse_GodHeroInfo);
            AddResponser(Id<MSG_ZGC_GOD_PATH_BUY_POWER>.Value, OnResponse_GodPathBuyPower);
            AddResponser(Id<MSG_ZGC_GOD_PATH_SEVEN_FIGHT_START>.Value, OnResponse_GodPathSevenFightStart);
            AddResponser(Id<MSG_ZGC_GOD_PATH_SEVEN_FIGHT_NEXT_STAGE>.Value, OnResponse_GodPathSevenFightNextStage);
            AddResponser(Id<MSG_ZGC_GOD_PATH_USE_ITEM>.Value, OnResponse_GodPathUseItem);
            AddResponser(Id<MSG_ZGC_GOD_PATH_TRAIN_BODY_BUY>.Value, OnResponse_GodPathTrainBodyBuyShield);
            AddResponser(Id<MSG_ZGC_GOD_PATH_TRAIN_BODY>.Value, OnResponse_GodPathTrainBody);
            AddResponser(Id<MSG_ZGC_GOD_FINISH_STAGE_TASK>.Value, OnResponse_GodPathFinishStageTask);

            AddResponser(Id<MSG_ZGC_GOD_PATH_BUY_OCEAN_HEART>.Value, OnResponse_GodPathOceanHeartBuyCount);
            AddResponser(Id<MSG_ZGC_GOD_PATH_REPAINT_OCEAN_HEART>.Value, OnResponse_GodPathOceanHeartRepaint);
            AddResponser(Id<MSG_ZGC_GOD_PATH_OCEAN_HEART_DRAW>.Value, OnResponse_GodPathOceanHeartDraw);

            AddResponser(Id<MSG_ZGC_GOD_PATH_BUY_TRIDENT>.Value, OnResponse_GodPathTridentBuy);
            AddResponser(Id<MSG_ZGC_GOD_PATH_USE_TRIDENT>.Value, OnResponse_GodPathTridentUse);
            AddResponser(Id<MSG_ZGC_GOD_PATH_TRIDENT_RESULT>.Value, OnResponse_GodPathTridentResult);
            AddResponser(Id<MSG_ZGC_GOD_PATH_PUSH_TRIDENT>.Value, OnResponse_GodPathTridentPush);

            AddResponser(Id<MSG_ZGC_GOD_PATH_LIGHT_PUZZLE>.Value, OnResponse_GodPathAcrossOceanLightPuzzle);
            AddResponser(Id<MSG_ZGC_GOD_PATH_ACROSS_OCEAN_SWEEP>.Value, OnResponse_GodPathAcrossOceanSweep);
            AddResponser(Id<MSG_ZGC_GOD_PATH_ACROSS_OCEAN_NEW_DUNGEON>.Value, OnResponse_GodPathAcrossPassNewDungeon);

            //福利
            AddResponser(Id<MSG_ZGC_WELFARE_TRIGGER_STATE>.Value, OnResponse_WelfareTriggerState);

            AddResponser(Id<MSG_ZGC_WISHPOOL_INFO>.Value, OnResponse_SendWishPoolInfo);
            AddResponser(Id<MSG_ZGC_USINIG_WISHPOOL>.Value, OnResponse_UsingWishPoolResult);
            //cross battle
            AddResponser(Id<MSG_ZGC_CROSS_BATTLE_MANAGER>.Value, OnResponse_CrossManager);
            AddResponser(Id<MSG_ZGC_SAVE_CROSS_BATTLE_DEFEMSIVE>.Value, OnResponse_SaveCrossDefensive);
            AddResponser(Id<MSG_ZGC_GET_CROSS_BATTLE_ACTIVE_REWARD>.Value, OnResponse_GetCrossBattleActiveReward);
            AddResponser(Id<MSG_ZGC_GET_CROSS_BATTLE_PRELIMINARY_REWARD>.Value, OnResponse_GetCrossBattlePreliminaryReward);
            AddResponser(Id<MSG_ZGC_CROSS_BATTLE_RANK_INFO_LIST>.Value, OnResponse_GetCrossRankInfoList);
            AddResponser(Id<MSG_ZGC_CROSS_BATTLE_CHALLENGER_HERO_INFO>.Value, OnResponse_GetCrossChallengerHeroInfo);
            AddResponser(Id<MSG_ZGC_SHOW_CROSS_LEADER_INFO>.Value, OnResponse_ShowCrossLeaderInfo);
            AddResponser(Id<MSG_ZGC_SHOW_CROSS_BATTLE_FINALS_INFO>.Value, OnResponse_ShowCrossBattleFinals);
            AddResponser(Id<MSG_ZGC_CROSS_BATTLE_CHALLENGER_INFO>.Value, OnResponse_ShowCrossBattleChallenger);
            AddResponser(Id<MSG_ZGC_UPDATE_DEFENSIVE_QUEUE>.Value, OnResponse_UpdateDefensiveQueue);
            AddResponser(Id<MSG_ZGC_UPDATE_CROSS_QUEUE>.Value, OnResponse_UpdateCrossQueue);
            AddResponser(Id<MSG_ZGC_GET_CROSS_VIDEO>.Value, OnResponse_GetCrossBattleVedio);
            AddResponser(Id<MSG_ZGC_GET_CROSS_BATTLE_SERVER_REWARD>.Value, OnResponse_GetCrossBattleServerReward);
            AddResponser(Id<MSG_ZGC_NEW_CROSS_BATTLE_SERVER_REWARD>.Value, OnResponse_CrossBattleServerReward);
            AddResponser(Id<MSG_ZGC_GET_GUESSING_INFO>.Value, OnResponse_GetGuessingPlayersInfo);
            AddResponser(Id<MSG_ZGC_CROSS_GUESSING_CHOOSE>.Value, OnResponse_CrossGuessingChoose);

            //阵营建设
            AddResponser(Id<MSG_ZGC_CAMPBUILD_INFO>.Value, OnResponse_CampBuildInfo);
            AddResponser(Id<MSG_ZGC_SYNC_CAMPBUILD_INFO>.Value, OnResponse_SyncCampBuildInfo);
            AddResponser(Id<MSG_ZGC_CAMPBUILD_GO>.Value, OnResponse_CampBuildGo);
            AddResponser(Id<MSG_ZGC_BUY_CAMPBUILD_GO_COUNT>.Value, OnResponse_BuyCampBuildGoCount);
            AddResponser(Id<MSG_ZGC_OPEN_CAMPBUILD_BOX>.Value, OnResponse_OpenCampBuildBox);
            AddResponser(Id<MSG_ZGC_CAMPBUILD_RANK_LIST>.Value, OnResponse_CampBuildRankList);

            //阵营战
            AddResponser(Id<MSG_ZGC_SYNC_CAMPBATTLE>.Value, OnResponse_SyncCampBattleInfo);
            AddResponser(Id<MSG_ZGC_FORT_INFO>.Value, OnResponse_CampBattleFortInfo);
            AddResponser(Id<MSG_ZGC_CAMP_CREATE_DUNGEON>.Value, OnResponse_CampCreateDungeon);
            AddResponser(Id<MSG_ZGC_CAMP_RANK_LIST_BY_TYPE>.Value, OnResponse_CampRankListByType);
            AddResponser(Id<MSG_ZGC_OPEN_CAMP_BOX>.Value, OnResponse_OpenCampBox); 
            AddResponser(Id<MSG_ZGC_CHECK_IN_BATTLE_RANK>.Value, OnResponse_CheckInBattleRank);
            AddResponser(Id<MSG_ZGC_USE_NATURE_ITEM>.Value, OnResponse_UseNatureItem);
            AddResponser(Id<MSG_ZGC_CAMP_BOX_COUNT>.Value, OnResponse_BattleBoxCount);
            AddResponser(Id<MSG_ZGC_GIVEUP_FORT>.Value, OnResponse_GiveUpFort);
            AddResponser(Id<MSG_ZGC_HOLD_FORT>.Value, OnResponse_HoldFort);
            AddResponser(Id<MSG_ZGC_CAMPBATTLE_ANNOUNCE_LIST>.Value, OnResponse_CampbattleAnnouncementList);
            

            //爬塔
            AddResponser(Id<MSG_ZGC_TOWER_INFO>.Value, OnResponse_TowerInfo);
            AddResponser(Id<MSG_ZGC_TOWER_REWARD>.Value, OnResponse_TowerReward);
            AddResponser(Id<MSG_ZGC_TOWER_SHOP_ITEM>.Value, OnResponse_TowerShopItemList);
            AddResponser(Id<MSG_ZGC_TOWER_TIME>.Value, OnResponse_TowerTime);
            AddResponser(Id<MSG_ZGC_TOWER_EXECUTE_TASK>.Value, OnResponse_TowerExecuteTask);
            AddResponser(Id<MSG_ZGC_TOWER_SELECT_BUFF>.Value, OnResponse_TowerSelectBuff) ;
            AddResponser(Id<MSG_ZGC_TOWER_BUFF>.Value, OnResponse_TowerBuff);
            AddResponser(Id<MSG_ZGC_TOWER_RANDOM_BUFF>.Value, OnResponse_TowerRandomBuff);
            AddResponser(Id<MSG_ZGC_UPDATE_TOWER_HERO_POS>.Value, OnResponse_TowerHeroPos);
            AddResponser(Id<MSG_ZGC_INIT_TOWER_HERO_INFO>.Value, OnResponse_TowerHeroInfo);
            AddResponser(Id<MSG_ZGC_TOWER_HERO_REVIVE>.Value, OnResponse_TowerReviveHero);
            AddResponser(Id<MSG_ZGC_TOWER_DUNGOEN_GROWTH>.Value, OnResponse_TowerDungeonGrowth);
            

            AddResponser(Id<MSG_ZGC_RANK_LIST_BY_TYPE>.Value, OnResponse_RankList);
            AddResponser(Id<MSG_ZGC_NEW_RANK_REWARD>.Value, OnResponse_NewRankReward);
            AddResponser(Id<MSG_ZGC_RANK_REWARD_LIST>.Value, OnResponse_GetRankRewardList);
            AddResponser(Id<MSG_ZGC_GET_RANK_REWARD>.Value, OnResponse_GetRankReward);
            AddResponser(Id<MSG_ZGC_RANK_REWARD_PAGE>.Value, OnResponse_GetRankRewardPage);
            AddResponser(Id<MSG_ZGC_GET_CROSS_RANK_REWARD>.Value, OnResponse_GetCrossRankReward);

            //传送进图
            AddResponser(Id<MSG_ZGC_TRANSFER_ENTER_MAP>.Value, OnResponse_TransferEnterMap);
            AddResponser(Id<MSG_ZGC_AUTOPATHFINDING>.Value, OnResponse_AutoPathFinding);
            //挂机
            AddResponser(Id<MSG_ZGC_ONHOOK_INFO>.Value, OnResponse_OnhookInfo);
            AddResponser(Id<MSG_ZGC_ONHOOK_GET_REWARD>.Value, OnResponse_OnhookGetReward);

            //推图 
            AddResponser(Id<MSG_ZGC_PUSHFIGURE_INFO>.Value, OnResponse_PushFigureInfo);
            AddResponser(Id<MSG_ZGC_PUSHFIGURE_FINISHTASK>.Value, OnResponse_PushFigureFinishTask);

            //武魂共鳴
            AddResponser(Id<MSG_ZGC_OPEN_RESONANCE_GRID>.Value, OnResponse_OpenResonanceGrid);
            AddResponser(Id<MSG_ZGC_ADD_RESONANCE>.Value, OnResponse_AddResonance);
            AddResponser(Id<MSG_ZGC_SUB_RESONANCE>.Value, OnResponse_SubResonance);
            AddResponser(Id<MSG_ZGC_RESONANCE_GRID_INFO>.Value, OnResponse_ResonanceGridInfo);
            AddResponser(Id<MSG_ZGC_RESONANCE_LEVEL>.Value, OnResponse_ResonanceLevelUp);

            //礼包
            AddResponser(Id<MSG_ZGC_GIFT_CODE_REWARD>.Value, OnResponse_GiftCodeExchangeReward);
            AddResponser(Id<MSG_ZGC_CHECK_CODE_UNIQUE>.Value, OnResponse_CheckCodeUnique);
            AddResponser(Id<MSG_ZGC_GIFT_INFO>.Value, OnResponse_SendGiftInfo);
            AddResponser(Id<MSG_ZGC_RECHARGE_GIFT>.Value, OnResponse_RechargeGift);
            AddResponser(Id<MSG_ZGC_REFRESH_GIFT>.Value, OnResponse_RefreshGift);
            AddResponser(Id<MSG_ZGC_RECEIVE_RECHARGE_REWARD>.Value, OnResponse_ReceiveRechargeReward);
            AddResponser(Id<MSG_ZGC_TEST_RECHARGE>.Value, OnResponse_TestRecharge);
            AddResponser(Id<MSG_ZGC_GIFT_OPEN>.Value, OnResponse_GiftOpen);
            AddResponser(Id<MSG_ZGC_LIMIT_TIME_GIFTS>.Value, OnResponse_LimitTimeGifts);
            AddResponser(Id<MSG_ZGC_USE_RECHARGE_TOKEN>.Value, OnResponse_UseRechargeToken);
            AddResponser(Id<MSG_ZGC_RESET_DOUBLE_FLAG>.Value, OnResponse_ResetDoubleFlag);
            AddResponser(Id<MSG_ZGC_RESET_RECHARGE_DISCOUNT>.Value, OnResponse_ResetRechargeDiscount);
            AddResponser(Id<MSG_ZGC_CULTIVATE_GIFT_OPEN>.Value, OnResponse_CultivateGiftOpen);
            AddResponser(Id<MSG_ZGC_BUY_CULTIVATE_GIFT>.Value, OnResponse_BuyCultivateGift);
            AddResponser(Id<MSG_ZGC_CULTIVATE_GIFT_LIST>.Value, OnResponse_CultivateGiftList);
            AddResponser(Id<MSG_ZGC_PETTY_GIFT_LIST>.Value, OnResponse_PettyGiftList);
            AddResponser(Id<MSG_ZGC_PETTY_GIFT_REFRESH>.Value, OnResponse_PettyGiftRefresh);
            AddResponser(Id<MSG_ZGC_BUY_PETTY_GIFT>.Value, OnResponse_BuyPettyGift);
            AddResponser(Id<MSG_ZGC_FREE_PETTY_GIFT>.Value, OnResponse_ReceiveFreePettyGift);
            AddResponser(Id<MSG_ZGC_DAILY_RECHARGE_INFO>.Value, OnResponse_GetDailyRechargeInfo);
            AddResponser(Id<MSG_ZGC_GET_DAILY_RECHARGE_REWARD>.Value, OnResponse_GetDailyRechargeReward);
            AddResponser(Id<MSG_ZGC_HERO_DAYS_REWARDS_INFO>.Value, OnResponse_HeroDaysRewardsInfo);
            AddResponser(Id<MSG_ZGC_GET_HERO_DAYS_REWARD>.Value, OnResponse_GetHeroDaysReward);
            AddResponser(Id<MSG_ZGC_NEWSERVER_PROMOTION_INFO>.Value, OnResponse_GetNewServerPromotionInfo);
            AddResponser(Id<MSG_ZGC_GET_NEWSERVER_PROMOTION_REWARD>.Value, OnResponse_GetNewServerPromotionReward);
            AddResponser(Id<MSG_ZGC_LUCKY_FLIP_CARD_INFO>.Value, OnResponse_GetLuckyFlipCardInfo);
            AddResponser(Id<MSG_ZGC_GET_LUCKY_FLIP_CARD_REWARD>.Value, OnResponse_GetLuckyFlipCardReward);
            AddResponser(Id<MSG_ZGC_GET_LUCKY_FLIP_CARD_CUMULATE_REWARD>.Value, OnResponse_GetLuckyCardCumulateReward);
            AddResponser(Id<MSG_ZGC_ISLAND_HIGH_GIFT_INFO>.Value, OnResponse_IslandHighGiftInfo);
            AddResponser(Id<MSG_ZGC_TREASURE_FLIP_CARD_INFO>.Value, OnResponse_GetTreasureFlipCardInfo);
            AddResponser(Id<MSG_ZGC_GET_TREASURE_FLIP_CARD_REWARD>.Value, OnResponse_GetTreasureFlipCardReward);
            AddResponser(Id<MSG_ZGC_GET_TREASURE_FLIP_CARD_CUMULATE_REWARD>.Value, OnResponse_GetTreasureCardCumulateReward);

            //金兰
            AddResponser(Id<MSG_ZGC_BROTHERS_INVITE>.Value, OnResponse_BrotherInvite);
            AddResponser(Id<MSG_ZGC_BROTHERS_RESPONSE>.Value, OnResponse_BrotherResponse);
            AddResponser(Id<MSG_ZGC_BROTHERS_REMOVE>.Value, OnResponse_BrothersRemove);
            AddResponser(Id<MSG_ZGC_SYNC_BROTHERS_LIST>.Value, OnResponse_SyncBrothersList);
            AddResponser(Id<MSG_ZGC_SYNC_BROTHERS_INVITER_LIST>.Value, OnResponse_SyncBrothersInviterList);

            AddResponser(Id<MSG_ZGC_FRIEND_RESPONSE>.Value, OnResponse_FriendResponse);
            AddResponser(Id<MSG_ZGC_SYNC_FRIEND_INVITER_LIST>.Value, OnResponse_SyncFriendInviterList);
            AddResponser(Id<MSG_ZGC_ONEKEY_IGNORE_INVITER>.Value, OnResponse_OnekeyIgnoreInviter);


            //挖宝
            AddResponser(Id<MSG_ZGC_SHOVEL_GAME_REWARDS>.Value, OnResponse_ShovelGameRewards);
            AddResponser(Id<MSG_ZGC_LIGHT_TREASURE_PUZZLE>.Value, OnResponse_LightTreasurePuzzle);
            AddResponser(Id<MSG_ZGC_RANDOM_PUZZLE>.Value, OnResponse_RandomPuzzle);
            AddResponser(Id<MSG_ZGC_TREASURE_PUZZLE_REWARD>.Value, OnResponse_TreasurePuzzleReward);
            AddResponser(Id<MSG_ZGC_SHOVEL_TREASURE_FLY>.Value, OnResponse_ShovelTreasureFly);
            AddResponser(Id<MSG_ZGC_SHOVEL_GAME_START>.Value, OnResponse_ShovelGameStart);
            AddResponser(Id<MSG_ZGC_SHOVEL_GAME_REVIVE>.Value, OnResponse_ShovelGameRevive);

            //贡献
            AddResponser(Id<MSG_ZGC_CONTRIBUTION_INFO>.Value, OnResponse_ContributionInfo);
            AddResponser(Id<MSG_ZGC_GET_CONTRIBUTION_REWARD>.Value, OnResponse_GetContributionReward);

            //主题通行证
            AddResponser(Id<MSG_ZGC_THEME_PASS_LIST>.Value, OnResponse_GetThemePassList);
            AddResponser(Id<MSG_ZGC_GET_THEMEPASS_REWARD>.Value, OnResponse_GetThemePassReward);
            AddResponser(Id<MSG_ZGC_BUY_THEMEPASS_RESULT>.Value, OnResponse_BuyThemePassResult);
            AddResponser(Id<MSG_ZGC_THEMEPASS_EXP_CHANGE>.Value, OnResponse_ThemePassExpChange);

            //主题Boss
            AddResponser(Id<MSG_ZGC_THEME_BOSS_INFO>.Value, OnResponse_ThemeBossInfo);
            AddResponser(Id<MSG_ZGC_THEMEBOSS_DUNGEON>.Value, OnResponse_ThemeBossDungeon);
            AddResponser(Id<MSG_ZGC_GET_THEMEBOSS_REWARD>.Value, OnResponse_GetThemeBossReward);
            AddResponser(Id<MSG_ZGC_THEMEBOSS_UPDATE_DEFENSIVE_QUEUE>.Value, OnResponse_UpdateThemeBossQueue);
         
            AddResponser(Id<MSG_ZGC_HIDDER_WEAPON_VALUE>.Value, OnResponse_GetHidderWeaponInfo);
            AddResponser(Id<MSG_ZGC_GET_HIDDER_WEAPON_REWARD>.Value, OnResponse_GetHidderWeaponReward);
            AddResponser(Id<MSG_ZGC_GET_HIDDER_WEAPON_NUM_REWARD>.Value, OnResponse_GetHidderWeaponNumReward);
            AddResponser(Id<MSG_ZGC_BUY_HIDDER_WEAPON_ITEM>.Value, OnResponse_BuyHidderWeaponItem);
            AddResponser(Id<MSG_ZGC_SEA_TREASURE_VALUE>.Value, OnResponse_GetSeaTreasureInfo);
            AddResponser(Id<MSG_ZGC_GET_SEA_TREASURE_REWARD>.Value, OnResponse_GetSeaTreasureReward);
            AddResponser(Id<MSG_ZGC_BUY_SEA_TREASURE_ITEM>.Value, OnResponse_BuySeaTreasureItem);
            AddResponser(Id<MSG_ZGC_GET_SEA_TREASURE_BLESSING>.Value, OnResponse_GetSeaTreasureBlessing);
            AddResponser(Id<MSG_ZGC_CLOSE_SEA_TREASURE_BLESSING>.Value, OnResponse_CloseSeaTreasureBlessing);
            AddResponser(Id<MSG_ZGC_GET_SEA_TREASURE_NUM_REWARD>.Value, OnResponse_GetSeaTreasureNumReward);        

            //跨服Boss
            AddResponser(Id<MSG_ZGC_GET_CROSS_BOSS_INFO>.Value, OnResponse_GetCrossBossInfo);
            AddResponser(Id<MSG_ZGC_UPDATE_CROSS_BOSS_QUEUE>.Value, OnResponse_UpdateCrossBossQueue);
            AddResponser(Id<MSG_ZGC_GET_CROSS_BOSS_PASS_REWARD>.Value, OnResponse_GetCrossBossPassReward);
            AddResponser(Id<MSG_ZGC_GET_CROSS_BOSS_RANK_REWARD>.Value, OnResponse_GetCrossBossRankReward);
            AddResponser(Id<MSG_ZGC_CROSS_BOSS_CHALLENGER_INFO>.Value, OnResponse_GetCrossBossDefenserInfo);

            //百草园
            AddResponser(Id<MSG_ZGC_GARDEN_INFO>.Value, OnResponse_GetGardenInfo);
            AddResponser(Id<MSG_ZGC_GARDEN_PLANTED_SEED>.Value, OnResponse_PlantedSeed);
            AddResponser(Id<MSG_ZGC_GARDEN_REAWARD>.Value, OnResponse_GetGardenReward);
            AddResponser(Id<MSG_ZGC_GARDEN_BUY_SEED>.Value, OnResponse_BuySeed);
            AddResponser(Id<MSG_ZGC_GARDEN_SHOP_EXCHANGE>.Value, OnResponse_GardenShopExchange);

            //乾坤问情
            AddResponser(Id<MSG_ZGC_DIVINE_LOVE_VALUE>.Value, OnResponse_GetDivineLoveInfo);
            AddResponser(Id<MSG_ZGC_GET_DIVINE_LOVE_REWARD>.Value, OnResponse_GetDivineLoveReward);
            AddResponser(Id<MSG_ZGC_GET_DIVINE_LOVE_CUMULATE_REWARD>.Value, OnResponse_GetDivineLoveCumulateReward);
            AddResponser(Id<MSG_ZGC_BUY_DIVINE_LOVE_ITEM>.Value, OnResponse_BuyDivineLoveItem);
            AddResponser(Id<MSG_ZGC_DIVINE_LOVE_INFO_LIST>.Value, OnResponse_GetDivineLoveInfoList);
            AddResponser(Id<MSG_ZGC_CLOSE_DIVINE_LOVE_ROUND>.Value, OnResponse_CloseDivineLoveRound);
            AddResponser(Id<MSG_ZGC_OPEN_DIVINE_LOVE_ROUND>.Value, OnResponse_OpenDivineLoveRound);

            //海岛登高
            AddResponser(Id<MSG_ZGC_ISLAND_HIGH_INFO>.Value, OnResponse_IslandHighInfo);
            AddResponser(Id<MSG_ZGC_ISLAND_HIGH_ROCK>.Value, OnResponse_IslandHighRock);
            AddResponser(Id<MSG_ZGC_ISLAND_HIGH_REWARD>.Value, OnResponse_IslandHighReward);
            AddResponser(Id<MSG_ZGC_ISLAND_HIGH_BUY_ITEM>.Value, OnResponse_IslandHighBuyItem);

            //三叉戟
            AddResponser(Id<MSG_ZGC_TRIDENT_INFO>.Value, OnResponse_TridentInfo);
            AddResponser(Id<MSG_ZGC_TRIDENT_REWARD>.Value, OnResponse_TridentReward);
            AddResponser(Id<MSG_ZGC_TRIDENT_USE_SHOVEL>.Value, OnResponse_TridentUseShovel);

            //端午活动
            AddResponser(Id<MSG_ZGC_DRAGON_BOAT_INFO>.Value, OnResponse_GetDragonBoatInfo);
            AddResponser(Id<MSG_ZGC_DRAGON_BOAT_GAME_START>.Value, OnResponse_DragonBoatGameStart);
            AddResponser(Id<MSG_ZGC_DRAGON_BOAT_GAME_END>.Value, OnResponse_DragonBoatGameEnd);
            AddResponser(Id<MSG_ZGC_DRAGON_BOAT_BUY_TICKET>.Value, OnResponse_DragonBoatBuyTicket);
            AddResponser(Id<MSG_ZGC_DRAGON_BOAT_FREE_TICKET>.Value, OnResponse_DragonBoatGetFreeTicket);

            //昊天石壁
            AddResponser(Id<MSG_ZGC_STONE_WALL_VALUE>.Value, OnResponse_GetStoneWallInfo);
            AddResponser(Id<MSG_ZGC_STONE_WALL_INFO_LIST>.Value, OnResponse_GetStoneWallInfoList);
            AddResponser(Id<MSG_ZGC_GET_STONE_WALL_REWARD>.Value, OnResponse_GetStoneWallReward);
            AddResponser(Id<MSG_ZGC_BUY_STONE_WALL_ITEM>.Value, OnResponse_BuyStoneWallItem);
            AddResponser(Id<MSG_ZGC_RESET_STONE_WALL>.Value, OnResponse_ResetStoneWall);


            //海岛挑战
            AddResponser(Id<MSG_ZGC_ISLAND_CHALLENGE_INFO>.Value, OnResponse_IslandChallengeInfo);
            AddResponser(Id<MSG_ZGC_ISLAND_CHALLENGE_REWARD>.Value, OnResponse_IslandChallengeReward);
            AddResponser(Id<MSG_ZGC_ISLAND_CHALLENGE_SHOP_ITEM>.Value, OnResponse_IslandChallengeShopItemList);
            AddResponser(Id<MSG_ZGC_ISLAND_CHALLENGE_TIME>.Value, OnResponse_IslandChallengeTime);
            AddResponser(Id<MSG_ZGC_ISLAND_CHALLENGE_EXECUTE_TASK>.Value, OnResponse_IslandChallengeExecuteTask);
            AddResponser(Id<MSG_ZGC_ISLAND_CHALLENGE_HERO_POS>.Value, OnResponse_IslandChallengeHeroPos);
            AddResponser(Id<MSG_ZGC_ISLAND_CHALLENGE_HERO_INFO>.Value, OnResponse_IslandChallengeHeroInfo);
            AddResponser(Id<MSG_ZGC_ISLAND_CHALLENGE_HERO_REVIVE>.Value, OnResponse_IslandChallengeReviveHero);
            AddResponser(Id<MSG_ZGC_ISLAND_CHALLENGE_DUNGOEN_GROWTH>.Value, OnResponse_IslandChallengeDungeonGrowth);
            AddResponser(Id<MSG_ZGC_ISLAND_CHALLENGE_RESET>.Value, OnResponse_IslandChallengeReset);
            AddResponser(Id<MSG_ZGC_ISLAND_CHALLENGE_UPDATE_WININFO>.Value, OnResponse_IslandChallengeUpdateWinInfo);
            AddResponser(Id<MSG_ZGC_ISLAND_CHALLENGE_SWAP_QUEUE>.Value, OnResponse_IslandChallengeSwapQueue);

            //嘉年华boss
            AddResponser(Id<MSG_ZGC_CARNIVAL_BOSS_INFO>.Value, OnResponse_GetCarnivalBossInfo);
            AddResponser(Id<MSG_ZGC_ENTER_CARNIVAL_BOSS_DUNGEON>.Value, OnResponse_EnterCarnivalBossDungeon);
            AddResponser(Id<MSG_ZGC_GET_CARNIVAL_BOSS_REWARD>.Value, OnResponse_GetCarnivalBossReward);
            AddResponser(Id<MSG_ZGC_UPDATE_CARNIVAL_BOSS_QUEUE>.Value, OnResponse_UpdateCarnivalBossQueue);
            AddResponser(Id<MSG_ZGC_CARNIVAL_RECHARGE_INFO>.Value, OnResponse_GetCarnivalRechargeInfo);
            AddResponser(Id<MSG_ZGC_GET_CARNIVAL_RECHARGE_REWARD>.Value, OnResponse_GetCarnivalRechargeReward);
            AddResponser(Id<MSG_ZGC_CARNIVAL_MALL_INFO>.Value, OnResponse_GetCarnivalMallInfo);
            AddResponser(Id<MSG_ZGC_BUY_CARNIVAL_MALL_GIFT_ITEM>.Value, OnResponse_BuyCarnivalMallGiftItem);

            //漫游
            AddResponser(Id<MSG_ZGC_TRAVEL_MANAGER>.Value, OnResponse_SyncTravelManagerMessage);
            AddResponser(Id<MSG_ZGC_ACTIVATE_HERO_TRAVEL>.Value, OnResponse_ActivateHeroTravel);
            AddResponser(Id<MSG_ZGC_ADD_HERO_TRAVEL_AFFINITY>.Value, OnResponse_AddHeroTravelAffinity);
            AddResponser(Id<MSG_ZGC_START_HERO_TRAVEL_EVENT>.Value, OnResponse_StartHeroTravelEvevt);
            AddResponser(Id<MSG_ZGC_GET_HERO_TRAVEL_EVENT>.Value, OnResponse_GetHeroTravelEvevt);
            AddResponser(Id<MSG_ZGC_BUY_HERO_TRAVEL_SHOP_ITEM>.Value, OnResponse_BuyHeroTravelHsopItem);
            AddResponser(Id<MSG_ZGC_UPDATE_HERO_TRAVEL_CARD_INFO>.Value, OnResponse_UpdateHeroTravelCardItem);

            //暗器
            AddResponser(Id<MSG_ZGC_HIDENWEAPON_EQUIP>.Value, OnResponse_HiddenWeaponEquip);
            AddResponser(Id<MSG_ZGC_HIDENWEAPON_UPGRADE>.Value, OnResponse_HiddenWeaponUpgrade);
            AddResponser(Id<MSG_ZGC_HIDENWEAPON_STAR>.Value, OnResponse_HiddenWeaponStar);
            AddResponser(Id<MSG_ZGC_HIDENWEAPON_WASH>.Value, OnResponse_HiddenWeaponWash);
            AddResponser(Id<MSG_ZGC_HIDENWEAPON_WASH_CONFIRM>.Value, OnResponse_HiddenWeaponWashConfirm);
            AddResponser(Id<MSG_ZGC_HIDENWEAPON_SMASH>.Value, OnResponse_HiddenWeaponSmash);

            //史莱克邀约
            AddResponser(Id<MSG_ZGC_SHREK_INVITATION_INFO>.Value, OnResponse_GetShrekInvitationInfo);
            AddResponser(Id<MSG_ZGC_GET_SHREK_INVITAION_REWARD>.Value, OnResponse_GetShrekInvitationReward);

            //轮盘
            AddResponser(Id<MSG_ZGC_ROULETTE_GET_INFO>.Value, OnResponse_GetRouletteInfo);
            AddResponser(Id<MSG_ZGC_ROULETTE_RANDOM>.Value, OnResponse_RouletteRandom);
            AddResponser(Id<MSG_ZGC_ROULETTE_REWARD>.Value, OnResponse_RouletteReward);
            AddResponser(Id<MSG_ZGC_ROULETTE_REFRESH>.Value, OnResponse_RouletteRefresh);
            AddResponser(Id<MSG_ZGC_ROULETTE_BUY_ITEM>.Value, OnResponse_RouletteBuyItem);

            //皮划艇
            AddResponser(Id<MSG_ZGC_CANOE_INFO>.Value, OnResponse_GetCanoeInfo);
            AddResponser(Id<MSG_ZGC_CANOE_GAME_START>.Value, OnResponse_CanoeGameStart);
            AddResponser(Id<MSG_ZGC_CANOE_GAME_END>.Value, OnResponse_CanoeGameEnd);
            AddResponser(Id<MSG_ZGC_CANOE_GET_REWARD>.Value, OnResponse_CanoeGetReward);

            //中秋
            AddResponser(Id<MSG_ZGC_MIDAUTUMN_INFO>.Value, OnResponse_GetMidAutumnInfo);
            AddResponser(Id<MSG_ZGC_DRAW_MIDAUTUMN_REWARD>.Value, OnResponse_DrawMidAutumnReward);
            AddResponser(Id<MSG_ZGC_GET_MIDAUTUMN_SCORE_REWARD>.Value, OnResponse_GetMidAutumnScoreReward);

            //主题烟花
            AddResponser(Id<MSG_ZGC_THEME_FIREWORK_INFO>.Value, OnResponse_GetThemeFireworkInfo);
            AddResponser(Id<MSG_ZGC_THEME_FIREWORK_REWARD>.Value, OnResponse_ThemeFireworkReward);
            AddResponser(Id<MSG_ZGC_USE_THEME_FIREWORK>.Value, OnResponse_UseThemeFirework);
            AddResponser(Id<MSG_ZGC_THEME_FIREWORK_SCORE_REWARD>.Value, OnResponse_GetThemeFireworkScoreReward);
            AddResponser(Id<MSG_ZGC_THEME_FIREWORK_USECOUNT_REWARD>.Value, OnResponse_GetThemeFireworkUseCountReward);

            //ResponserEnd

            //cross battle
            AddResponser(Id<MSG_ZGC_CROSS_CHALLENGE_MANAGER>.Value, OnResponse_CrossChallengeManager);
            AddResponser(Id<MSG_ZGC_SAVE_CROSS_CHALLENGE_DEFEMSIVE>.Value, OnResponse_SaveCrossChallengeDefensive);
            AddResponser(Id<MSG_ZGC_GET_CROSS_CHALLENGE_ACTIVE_REWARD>.Value, OnResponse_GetCrossChallengeActiveReward);
            AddResponser(Id<MSG_ZGC_GET_CROSS_CHALLENGE_PRELIMINARY_REWARD>.Value, OnResponse_GetCrossChallengePreliminaryReward);
            AddResponser(Id<MSG_ZGC_CROSS_CHALLENGE_RANK_INFO_LIST>.Value, OnResponse_GetCrossChallengeRankInfoList);
            AddResponser(Id<MSG_ZGC_CROSS_CHALLENGE_CHALLENGER_HERO_INFO>.Value, OnResponse_GetCrossChallengeChallengerHeroInfo);
            AddResponser(Id<MSG_ZGC_SHOW_CROSS_CHALLENGE_LEADER_INFO>.Value, OnResponse_ShowCrossChallengeLeaderInfo);
            AddResponser(Id<MSG_ZGC_SHOW_CROSS_CHALLENGE_FINALS_INFO>.Value, OnResponse_ShowCrossChallengeFinals);
            AddResponser(Id<MSG_ZGC_CROSS_CHALLENGE_CHALLENGER_INFO>.Value, OnResponse_ShowCrossChallengeChallenger);
            //AddResponser(Id<MSG_ZGC_UPDATE_CROSS_CHALLENGE_DEFENSIVE_QUEUE>.Value, OnResponse_UpdateCrossChallengeDefensiveQueue);
            AddResponser(Id<MSG_ZGC_UPDATE_CROSS_CHALLENGE_QUEUE>.Value, OnResponse_UpdateCrossChallengeQueue);
            AddResponser(Id<MSG_ZGC_GET_CROSS_CHALLENGE_VIDEO>.Value, OnResponse_GetCrossChallengeVedio);
            AddResponser(Id<MSG_ZGC_GET_CROSS_CHALLENGE_SERVER_REWARD>.Value, OnResponse_GetCrossChallengeServerReward);
            AddResponser(Id<MSG_ZGC_NEW_CROSS_CHALLENGE_SERVER_REWARD>.Value, OnResponse_CrossChallengeServerReward);
            AddResponser(Id<MSG_ZGC_GET_CROSS_CHALLENGE_GUESSING_INFO>.Value, OnResponse_GetCrossChallengeGuessingPlayersInfo);
            AddResponser(Id<MSG_ZGC_CROSS_CHALLENGE_GUESSING_CHOOSE>.Value, OnResponse_CrossChallengeGuessingChoose);
            AddResponser(Id<MSG_ZGC_CROSS_CHALLENGE_SWAP_QUEUE>.Value, OnResponse_CrossChallengeSwapQueue);
            AddResponser(Id<MSG_ZGC_CROSS_CHALLENGE_SWAP_HERO>.Value, OnResponse_CrossChallengeSwapHero);

            //九考试炼
            AddResponser(Id<MSG_ZGC_GET_NINETEST_INFO>.Value, OnResponse_GetNineTestInfo);
            AddResponser(Id<MSG_ZGC_NINETEST_CLICK_GRID>.Value, OnResponse_NineTestClickGrid);
            AddResponser(Id<MSG_ZGC_NINETEST_SCORE_REWARD>.Value, OnResponse_NineTestScoreReward);
            AddResponser(Id<MSG_ZGC_NINETEST_RESET>.Value, OnResponse_NineTestReset);

            //仓库
            AddResponser(Id<MSG_ZGC_SYNC_WAREHOUSE_CURRENCIES>.Value, OnResponse_SyncWarehouseCurrencies);
            AddResponser(Id<MSG_ZGC_GET_WAREHOUSE_CURRENCIES>.Value, OnResponse_GetWarehouseCurrencies);
            AddResponser(Id<MSG_ZGC_NEW_WAREHOUSE_SOULRING>.Value, OnResponse_NewWarehouseSoulRing);
            AddResponser(Id<MSG_ZGC_SYNC_WAREHOUSE_ITEMS>.Value, OnResponse_SyncWarehouseItems);
            AddResponser(Id<MSG_ZGC_SHOW_WAREHOUSE_ITEMS>.Value, OnResponse_ShowWarehouseItems);
            AddResponser(Id<MSG_ZGC_BATCH_GET_WAREHOUSE_ITEMS>.Value, OnResponse_BathchGetWarehouseItems);

            //学院 
            AddResponser(Id<MSG_ZGC_SCHOOL_INFO>.Value, OnResponse_SchoolInfo);
            AddResponser(Id<MSG_ZGC_ENTER_SCHOOL>.Value, OnResponse_EnterSchool);
            AddResponser(Id<MSG_ZGC_LEAVE_SCHOOL>.Value, OnResponse_LeaveSchool);
            AddResponser(Id<MSG_ZGC_SCHOOL_POOL_USE_ITEM>.Value, OnResponse_SchoolPoolUseItem);
            AddResponser(Id<MSG_ZGC_SCHOOL_POOL_LEVEL_UP>.Value, OnResponse_SchoolPoolLevelUp);
            AddResponser(Id<MSG_ZGC_SCHOOLTASK_FINISH_INFO>.Value, OnResponse_SchoolTaskFinishInfo);
            AddResponser(Id<MSG_ZGC_INIT_SCHOOLTASKS_INFO>.Value, OnResponse_InitSchoolTasksInfo);
            AddResponser(Id<MSG_ZGC_UPDATE_SCHOOLTASKS_INFO>.Value, OnResponse_UpdateSchoolTasksInfo);
            AddResponser(Id<MSG_ZGC_GET_SCHOOLTASK_FINISH_REWARD>.Value, OnResponse_GetSchoolTaskFinishReward);
            AddResponser(Id<MSG_ZGC_GET_SCHOOLTASK_BOX_REWARD>.Value, OnResponse_GetSchoolTaskBoxReward);
            AddResponser(Id<MSG_ZGC_ANSWER_QUESTION_INFO>.Value, OnResponse_AnswerQuestionInfo);
            AddResponser(Id<MSG_ZGC_ANSWER_QUESTION_SUBMIT>.Value, OnResponse_AnswerQuestionSubmit);

            //钻石返利
            AddResponser(Id<MSG_ZGC_DIAMOND_REBATE_INFO>.Value, OnResponse_DiamondRebateInfo);
            AddResponser(Id<MSG_ZGC_GET_DIAMOND_REBATE_REWARDS>.Value, OnResponse_GetDiamondRebateRewards);

            //轮盘
            AddResponser(Id<MSG_ZGC_XUANBOX_GET_INFO>.Value, OnResponse_GetXuanBoxInfo);
            AddResponser(Id<MSG_ZGC_XUANBOX_RANDOM>.Value, OnResponse_XuanBoxRandom);
            AddResponser(Id<MSG_ZGC_XUANBOX_REWARD>.Value, OnResponse_XuanBoxReward);

            //九笼祈愿
            AddResponser(Id<MSG_ZGC_WISH_LANTERN_INFO>.Value, OnResponse_WishLanternInfo);
            AddResponser(Id<MSG_ZGC_WISH_LANTERN_SELECT_REWARD>.Value, OnResponse_WishLanternSelect);
            AddResponser(Id<MSG_ZGC_WISH_LANTERN_LIGHT>.Value, OnResponse_WishLanternLight);
            AddResponser(Id<MSG_ZGC_WISH_LANTERN_RESET>.Value, OnResponse_WishLanternReset);

            //n日充值
            AddResponser(Id<MSG_ZGC_DAYS_RECHARGE_INFO>.Value, OnResponse_DaysRechargeInfo);

            //史莱克乐园
            AddResponser(Id<MSG_ZGC_SHREKLAND_INFO>.Value, OnResponse_ShreklandInfo);
            AddResponser(Id<MSG_ZGC_SHREKLAND_USE_ROULETTE>.Value, OnResponse_ShreklandUseRoulette);
            AddResponser(Id<MSG_ZGC_SHREKLAND_REFRESH_REWARDS>.Value, OnResponse_ShreklandRefreshRewards);
            AddResponser(Id<MSG_ZGC_SHREKLAND_GET_SCORE_REWARD>.Value, OnResponse_ShreklandGetScoreReward);

            //魔鬼训练
            AddResponser(Id<MSG_ZGC_DEVIL_TRAINING_INFO>.Value, OnResponse_DevilTrainingInfo);
            AddResponser(Id<MSG_ZGC_GET_DEVIL_TRAINING_REWARD>.Value, OnResponse_DevilTrainingReward);
            AddResponser(Id<MSG_ZGC_BUY_DEVIL_TRAINING_ITEM>.Value, OnResponse_DevilTrainingBuyItem);
            AddResponser(Id<MSG_ZGC_GET_DEVIL_TRAINING_POINT_REWARD>.Value, OnResponse_DevilTrainingPointReward);
            AddResponser(Id<MSG_ZGC_CHANGE_DEVIL_TRAINING_BUFF>.Value, OnResponse_DevilTrainingChangeBuff);

            //时空塔
            AddResponser(Id<MSG_ZGC_SPACE_TIME_TOWER_INFO>.Value, OnResponse_SpaceTimeTowerInfo);
            AddResponser(Id<MSG_ZGC_SPACE_TIME_JOIN_TEAM>.Value, OnResponse_SpaceTimeJoinTeam);
            AddResponser(Id<MSG_ZGC_SPACE_TIME_QUIT_TEAM>.Value, OnResponse_SpaceTimeQuitTeam);
            AddResponser(Id<MSG_ZGC_SPACE_TIME_HERO_CHANGE>.Value, OnResponse_SpaceTimeHeroChange);
            AddResponser(Id<MSG_ZGC_SPACETIME_HERO_STEPUP>.Value, OnResponse_SpaceTimeHeroStepUp);
            AddResponser(Id<MSG_ZGC_SPACETIME_REFRESH_CARD_POOL>.Value, OnResponse_SpaceTimeRefreshCardPool);
            AddResponser(Id<MSG_ZGC_UPDATE_SPACETIME_HERO_QUEUE>.Value, OnResponse_UpdateSpaceTimeHeroQueue);
            AddResponser(Id<MSG_ZGC_SPACETIME_EXECUTE_EVENT>.Value, OnResponse_SpaceTimeExecuteEvent);
            AddResponser(Id<MSG_ZGC_SPACETIME_TIME>.Value, OnResponse_NotifySpaceTimeOpenTime);
            AddResponser(Id<MSG_ZGC_SPACETIME_GET_STAGE_AWARD>.Value, OnResponse_SpaceTimeGetStageAward);
            AddResponser(Id<MSG_ZGC_SPACETIME_RESET>.Value, OnResponse_SpaceTimeReset);
            AddResponser(Id<MSG_ZGC_SPACETIME_DUNGEON_SETTLEMENT>.Value, OnResponse_SpaceTimeDungeonSettlement);
            AddResponser(Id<MSG_ZGC_SPACETIME_SHOP_INFO>.Value, OnResponse_SpaceTimeShopInfo);
            AddResponser(Id<MSG_ZGC_SPACETIME_GUIDESOUL_RESTCOUNTS>.Value, OnResponse_SpaceTimeGuideSoulRestCounts);
            AddResponser(Id<MSG_ZGC_SPACETIME_BEAST_SETTLEMENT>.Value, OnResponse_SpaceTimeBeastSettlement);
            AddResponser(Id<MSG_ZGC_SELECT_GUIDESOUL_ITEM>.Value, OnResponse_SelectGuideSoulItem);
            AddResponser(Id<MSG_ZGC_SPACETIME_ENTER_NEXTLEVEL>.Value, OnResponse_SpaceTimeEnterNextLevel);
            AddResponser(Id<MSG_ZGC_SPACETIME_HOUSE_RANDOM_PARAM>.Value, OnResponse_SpaceTimeHouseRandomParam);
            AddResponser(Id<MSG_ZGC_ENTER_SPACETIME_TOWER>.Value, OnResponse_EnterSpaceTimeTower);
            AddResponser(Id<MSG_ZGC_SPACETIME_GET_PAST_REWARDS>.Value, OnResponse_SpaceTimeGetPastRewards);
            
            //神域赐福
            AddResponser(Id<MSG_ZGC_DOMAIN_BENEDICTION_GET_STAGE_AWARD>.Value, OnResponse_HandleGetStageAward);
            AddResponser(Id<MSG_ZGC_DOMAIN_BENEDICTION_GET_BASE_CURRENCY_AWARD>.Value, OnResponse_HandleGetBaseCurrencyAward);
            AddResponser(Id<MSG_ZGC_DOMAIN_BENEDICTION_PRAY_OPERATION>.Value, OnResponse_HandlePrayOperation);
            AddResponser(Id<MSG_ZGC_DOMAIN_BENEDICTION_DRAW_OPERATION>.Value, OnResponse_HandleDrawOperation);
            AddResponser(Id<MSG_ZGC_DOMAIN_BENEDICTION_LOAD_AND_UPDATE>.Value, OnResponse_LoadOrUpdate);
            
            //漂流探宝
            AddResponser(Id<MSG_ZGC_DRIFT_EXPLORE_TASK_REWARD>.Value, OnResponse_DriftExploreTaskReward);
            AddResponser(Id<MSG_ZGC_DRIFT_EXPLORE_REWARD>.Value, OnResponse_DriftExploreReward);
            AddResponser(Id<MSG_ZGC_INIT_DRIFT_EXPLORE_INFO>.Value, OnResponse_InitDriftExploreInfo);
            AddResponser(Id<MSG_ZGC_UPDATE_DRIFT_EXPLORE_TASK_INFO>.Value, OnResponse_UpdateDriftExploreTaskInfo);
            AddResponser(Id<MSG_ZGC_UPDATE_DRIFT_EXPLORE_INFO>.Value, OnResponse_UpdateDriftExploreInfo);
            

            //ResponserEnd 
        }


        public void OnResponse_Default(MemoryStream stream, int uid, uint msg_id)
        {
            Client client = Api.ClientMng.FindClientByUid(uid);
            if (client != null)
            {
                client.Write(msg_id, stream);
            }
        }

        /// <summary>
        /// 错误码统一接口
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="pcUid"></param>
        public void OnResponse_ErrorCode(MemoryStream stream, int pcUid)
        {
            //MSG_ZGC_ERROR_CODE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZGC_ERROR_CODE>(stream);
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_ERROR_CODE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} error code not find client", pcUid);
            }
        }

        public void OnResponse_LoginFreeze(MemoryStream stream, int pcUid)
        {
            //MSG_ZGC_ERROR_CODE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZGC_ERROR_CODE>(stream);
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_LOGIN_FREEZE>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} login freeze not find client", pcUid);
            }
        }

        public void OnResponse_FighingCount(MemoryStream stream, int pcUid)
        {
            //MSG_ZGC_ERROR_CODE pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZGC_ERROR_CODE>(stream);
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_FIGHTING_COUNT>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} fighting count not find client", pcUid);
            }
        }

        public void OnResponse_ReturnNotesList(MemoryStream stream, int pcUid)
        {
            //MSG_ZGC_CROSS_NOTES_LIST pks = MessagePacker.ProtobufHelper.Deserialize<MSG_ZGC_CROSS_NOTES_LIST>(stream);
            Client client = Api.ClientMng.FindClientByUid(pcUid);
            if (client != null)
            {
                client.Write(Id<MSG_ZGC_CROSS_NOTES_LIST>.Value, stream);
            }
            else
            {
                Log.WarnLine("player {0} ReturnNotesList not find client", pcUid);
            }
        }
    }
}