using CommonUtility;
using EnumerateUtility;
using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class ShrekInvitationManager
    {
        private PlayerChar owner;
        private Dictionary<int, ShrekInvitationInfo> infoDic = new Dictionary<int, ShrekInvitationInfo>();
        public Dictionary<int, ShrekInvitationInfo> InfoDic { get { return infoDic; } }

        public ShrekInvitationManager(PlayerChar owner)
        {
            this.owner = owner;
        }

        public void Init(List<ShrekInvitationInfo> list)
        {
            foreach (var item in list)
            {
                infoDic.Add(item.Id, item);
            }
        }

        public ShrekInvitationInfo AddShrekInvitationInfo(int id)
        {
            ShrekInvitationInfo info = new ShrekInvitationInfo()
            {
                Id = id,
                GetState = RewardGetState.GetOnce,
                GetTime = Timestamp.GetUnixTimeStampSeconds(owner.server.Now())
            };
            infoDic.Add(info.Id, info);
            return info;
        }

        public void AddShrekInvitationInfo(int id, int getState, int time)
        {
            ShrekInvitationInfo info = new ShrekInvitationInfo()
            {
                Id = id,
                GetState = (RewardGetState)getState,
                GetTime = time
            };
            infoDic.Add(info.Id, info);
        }

        public void Clear()
        {
            foreach (var item in infoDic)
            {
                item.Value.GetState = RewardGetState.None;
                item.Value.GetTime = 0;
            }
        }
    }
}
