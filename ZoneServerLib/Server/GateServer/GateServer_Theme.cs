using Logger;
using Message.Gate.Protocol.GateZ;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public partial class GateServer
    {
        public void OnResponse_GetThemePassReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_THEMEPASS_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_THEMEPASS_REWARD>(stream);
            Log.Write("player {0} request get theme pass {1} reward", uid, msg.ThemeType);

            PlayerChar player = Api.PCManager.FindPc(uid);           
            if (player == null)
            {
                Log.Warn("player {0} get theme pass reward not in gateid {1} pc list", uid, SubId);
                return;
            }
            List<int> rewardLevels = new List<int>();
            rewardLevels.AddRange(msg.RewardLevels);
            player.GetThemePassReward(msg.ThemeType, msg.GetAll, msg.IsSuper, rewardLevels);
        }

        public void OnResponse_ThemeBossDungeon(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_THEMEBOSS_DUNGEON msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_THEMEBOSS_DUNGEON>(stream);
            Log.Write("player {0} request  theme boss dungeon {1}", uid, msg.DungeonId);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} theme boss dungeon not in gateid {1} pc list", uid, SubId);
                return;
            }
            player.EnterThemeBossDungeon(msg.DungeonId);
        }

        public void OnResponse_GetThemeBossReward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_GET_THEMEBOSS_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_GET_THEMEBOSS_REWARD>(stream);
            Log.Write("player {0} request get theme boss reward {1}", uid, msg.RewardId);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} get theme boss reward not in gateid {1} pc list", uid, SubId);
                return;
            }
            player.GetThemeBossReward(msg.RewardId);//实际传的是进度不是奖励id
        }

        public void OnResponse_UpdateThemeBossQueue(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_THEMEBOSS_UPDATE_DEFENSIVE_QUEUE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_THEMEBOSS_UPDATE_DEFENSIVE_QUEUE>(stream);
            Log.Write("player {0} request update theme boss queue{1}", uid, msg.HeroDefInfos);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} update theme boss queue not in gateid {1} pc list", uid, SubId);
                return;
            }
            player.UpdateThemeQueue(msg.HeroDefInfos);//实际传的是进度不是奖励id
        }
    }
}
