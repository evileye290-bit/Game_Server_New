using Logger;
using Message.Gate.Protocol.GateC;
using ServerShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossServerLib
{
    public class ChallengerInfoMessage
    {
        public DateTime DeleteTime { get; set; }
        public MSG_ZGC_ARENA_CHALLENGER_HERO_INFO Message { get; set; }
    }
    public partial class ChallengerManager
    {
        private Dictionary<int, ChallengerInfoMessage> challengerInfoDic = new Dictionary<int, ChallengerInfoMessage>();

        double showDeleteTime = 0.0;
        //double showDeleteTickTime = 60000.0;
        List<int> showRemoveList = new List<int>();

        public void DeletePlayerShowInfo(double dt)
        {
            if (!CheckDeleteTickTime(dt))
            {
                return;
            }
            foreach (var item in challengerInfoDic)
            {
                if (CrossServerApi.now > item.Value.DeleteTime)
                {
                    showRemoveList.Add(item.Key);
                }
            }

            if (showRemoveList.Count > 0)
            {
                foreach (var uid in showRemoveList)
                {
                    RemoveArenaChallengerInfo(uid);
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


        public ChallengerInfoMessage GetArenaChallengerInfo(int uid)
        {
            ChallengerInfoMessage info = null;
            challengerInfoDic.TryGetValue(uid, out info);
            return info;
        }

        public void AddArenaChallengerInfo(MSG_ZGC_ARENA_CHALLENGER_HERO_INFO info, int uid)
        {
            if (info == null)
            {
                Log.Warn("add arena challenger info failed: show info is null");
                return;
            }
            ChallengerInfoMessage msg = new ChallengerInfoMessage();
            msg.DeleteTime = CrossServerApi.now.AddMinutes(10);
            msg.Message = info;
            challengerInfoDic[uid] = msg;
        }


        public bool RemoveArenaChallengerInfo(int Uid)
        {
            return challengerInfoDic.Remove(Uid);
        }
    }
}
