using Logger;
using Message.Gate.Protocol.GateC;
using ServerShared;
using System;
using System.Collections.Generic;

namespace RelationServerLib
{
    public class ShowInfoMessage
    {
        public DateTime DeleteTime { get; set; }
        public MSG_ZRZ_RETURN_PLAYER_SHOW Message { get; set; }
    }
    public partial class ShowManager
    {
        private Dictionary<int, ShowInfoMessage> playerShowInfoDic = new Dictionary<int, ShowInfoMessage>();

        double showDeleteTime = 0.0;
        //double showDeleteTickTime = 60000.0;
        List<int> showRemoveList = new List<int>();

        public void DeletePlayerShowInfo(double dt)
        {
            if (!CheckDeleteTickTime(dt))
            {
                return;
            }
            foreach (var item in playerShowInfoDic)
            {
                if (RelationServerApi.now > item.Value.DeleteTime)
                {
                    showRemoveList.Add(item.Key);
                }
            }

            if (showRemoveList.Count > 0)
            {
                foreach (var uid in showRemoveList)
                {
                    RemoveShowInfo(uid);
                }
                showRemoveList.Clear();
            }
        }

        private bool CheckDeleteTickTime(double deltaTime)
        {
            showDeleteTime += (float)deltaTime;
            if (showDeleteTime < CrossBattleLibrary.ShowRefreshTime)
            {
                return false;
            }
            else
            {
                showDeleteTime = 0;
                return true;
            }
        }


        public ShowInfoMessage GetShowInfo(int uid)
        {
            ShowInfoMessage info = null;
            playerShowInfoDic.TryGetValue(uid, out info);    
            return info;
        }

        public void AddShowInfo(MSG_ZRZ_RETURN_PLAYER_SHOW info)
        {
            if (info == null|| info.ShowInfo == null || info.ShowInfo.PlayerInfo == null)
            {
                Log.Warn("add show info failed: show info is null");
                return;
            }
            ShowInfoMessage msg = new ShowInfoMessage();
            msg.DeleteTime = RelationServerApi.now.AddMinutes(10);
            msg.Message = info;
            playerShowInfoDic[info.ShowPcUid] = msg;
        }


        public bool RemoveShowInfo(int Uid)
        {
            return playerShowInfoDic.Remove(Uid);
        }
    }
}
