namespace ServerLogger.KomoeLog
{
    public enum KomoeLogEventType
    {
        game_load = 1, //  P0  游戏前资源加载表    
        player_login = 2,//   P0  玩家登录表   
        player_logout = 3,//   P0  玩家登出表   
        online_num = 4,// 	P0  实时在线    
        create_account = 5,// 	P0  玩家注册表   
        create_role = 6,// 	P0  玩家创角表   
        guide_flow = 7,// 	P0  新手引导步骤通过    
        player_exp = 8,// 	P0  人物等级流水表(人物等级变化触发)	
        recharge_flow = 9,// 	P0  充值流水表 (充值成功时触发)	
        gift_push = 10,// 	P1  礼包推送(限时礼包，等级礼包等)	
        item_flow = 11,// 	P0  道具产销表   
        gold_flow = 12,// 	P0  货币产销表   
        shop_purchase = 13,// 	P1  商店流水(现有7个商店)	
        mission_flow = 14,// 	P1  任务流水    
        mail_flow = 15,// 	P1  邮件流水(邮件变化时触发)	
        operational_activity = 16,// 	P1  运营活动流水(活动完成触发日志) 	
        function_open = 17,// 	P2  功能开启日志  
        user_info_change = 18,// 	P2  角色信息变动表 
        gift_code_exchange = 19,// 	P2  礼包码兑换   
        player_title = 20,// 	P2  角色称号变动表 
        chat_flow = 21,// 	P1  聊天流水    
        friend_flow = 22,// 	P2  好友操作    
        draw_card = 23,// 	P1  招募  
        rank_flow = 24,// 	P1  排行榜 
        tie_flow = 25,// 	P2  羁绊  
        camp_flow = 26,// 	P2  阵营变化流水  
        camp_constellation = 27,// 	P2  阵营星宿变化流水    
        camp_worship = 28,// 	P2  阵营膜拜    
        camp_battle = 29,// 	P2  阵营对决    
        camp_build = 30,// 	P2  阵营建设    
        pve_fight = 31,// 	P1  PVE/爬塔  
        pvp_fight = 32,// 	P1  PVP 
        teamform_flow = 33,// 	P2  组队操作流水  
        main_task = 34,// 	P1  主线/支线任务 
        delegate_tasks = 35,// 	P2  委派事件    
        hero_resource = 36,// 	P1  魂师获取    
        hero_levelup = 37,// 	P2  魂师武魂升级  
        hero_starup = 38,// 	P2  魂师升星    
        hero_skin_resource = 39,// 	P2  魂师神位    
        modifytp_flow = 40,// 	P2  魂师天赋变化  
        hero_skill_levelup = 41,// 	P2  魂师魂技升级  
        hero_reset = 42,// 	P2  魂师重置    
        soullink_resource = 43,// 	P2  魂环获取    
        soulbone_resource = 44,// 	P2  魂骨获取    
        soulbone_quenching = 45,// 	P2  魂骨淬炼    
        equipment_resource = 46,// 	P2  装备获取    
        equipment_strengthen = 47,// 	P2  装备强化    
        equit_flow = 48,// 	P2  穿戴变化    
        battleteam_flow = 49,// 	P2  阵容变化    
        hero_resonance = 50,// 	P2  武魂共鸣变化  
        treasure_map = 51,// 	P2  藏宝图 
        intervention_activity = 52,// 	P1	运营活动流水
        get_applist = 53,// 	获取applist
        gift_push_buy = 54,// 	P1  礼包推送(限时礼包，等级礼包等)	
    }


    public enum KomoeLogUserType
    {
        user_snapshot = 1,// 	玩家快照    
        user_hero_snapshot = 2,// 	魂师快照    
        user_item_snapshot = 3,// 	背包快照    
        user_task_snapshot = 4,// 	副本进度快照  
        rank_list_snapshot = 5,// 	排行榜快照   
        user_function_open_snapshot = 6,// 	功能快照    
        user_camp_snapshot = 7,// 	阵营玩法快照  
    }
}
