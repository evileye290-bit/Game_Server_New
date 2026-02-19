using System.IO;
using System.Collections.Generic;
using System.Threading;
using DataProperty;
using CommonUtility;
using Engine;
using ScriptFunctions;
using ServerShared;
using ServerFrame;

namespace BattleServerLib
{
    public partial class BattleServerApi 
    {
        public BackendServerManager ZoneManager
        { get { return serverManagerProxy.GetBackendServerManager(ServerType.ZoneServer); } }

        public BackendServer BattleManagerServer
        { get { return serverManagerProxy.GetSinglePointBackendServer(ServerType.BattleManagerServer, ClusterId); } }

        public override void InitData()
        {
            base.InitData();

            string[] aiFiles = Directory.GetFiles(PathExt.FullPathFromServerData("XML\\RobotAI"), "*.xml", SearchOption.AllDirectories);
            List<string> aiFileList = new List<string>();
            foreach (var item in aiFiles)
            {
                string[] filePath = item.Split('\\');
                string baseFileName = filePath[filePath.Length - 1];
                string fileId = baseFileName.Split('.')[0];
                aiFileList.Add(fileId);
            }
            DataListManager.inst.GroupFiles.Add("RobotAI", aiFileList);

            ScriptManager.Init(PathExt.FullPathFromServer("Script"));
        }

        public void InitFieldMapManager()
        {
            //初始化地图
        }

        public override void InitProtocol()
        {
            base.InitProtocol();
            Message.Battle.Protocol.BattleBM.BattleBMIdGenerator.GenerateId();
            Message.Battle.Protocol.BattleG.BattleGIdGenerator.GenerateId();
            Message.Battle.Protocol.BattleZ.BattleZIdGenerator.GenerateId();
            Message.Gate.Protocol.GateC.GateCIdGenerator.GenerateId();
            Message.BattleManager.Protocol.BMBattle.BMBattleIdGenerator.GenerateId();
            Message.Global.Protocol.GBattle.GBattleIdGenerator.GenerateId();
            Message.Zone.Protocol.ZBattle.ZBattleIdGenerator.GenerateId();
        }

        public void InitVedioUploaer()
        {
        }

    }
}
