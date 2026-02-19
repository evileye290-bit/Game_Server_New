using System;
using System.Collections.Generic;
using ServerShared;
using Logger;
using CommonUtility;
using ServerFrame;

namespace CrossServerLib
{
    public partial class CrossServerApi : BaseApi
    {
        // args [path]
        public override void Init(string[] args)
        {
            base.Init(args);

            InitManagers();
            // init完毕，完成起服
            InitDone();
        }

        public override void InitDone()
        {
            base.InitDone();
        }

        private static void InitLibrarys()
        {
            //跨服 
            CrossBattleLibrary.Init();
            RobotLibrary.LoadRobotHeroInfo();
            RobotLibrary.InitCrossFinalsRobotInfos();
            RankLibrary.Init();
            HidderWeaponLibrary.Init();
            CrossBossLibrary.Init();
            RechargeLibrary.Init();
            DivineLoveLibrary.Init();
            IslandHighLibrary.Init();
            StoneWallLibrary.Init();
            RouletteLibrary.Init();
            CanoeLibrary.Init();
            ThemeFireworkLibrary.Init();
            MidAutumnLibrary.Init();
            NineTestLibrary.Init();
            CrossChallengeLibrary.Init();
            RobotLibrary.InitCrossChallengeFinalsRobotInfos();
            GameConfig.InitGameCongfig();
        }

        public override void StopServer(int min = 0)
        {
            if (State != ServerState.Stopped && State != ServerState.Stopping)
            {
                //MSG_MR_SHUTDOWN msgRelation = new MSG_MR_SHUTDOWN();
                //if (RelationServerManager != null)
                //{
                //    foreach (var item in RelationServerManager.ServerList)
                //    {
                //        item.Value.Write(msgRelation);
                //    }
                //}
                // 关闭所有客户端连接 并且禁止新客户端连接
                base.StopServer(min);
            }
        }

    }

}
