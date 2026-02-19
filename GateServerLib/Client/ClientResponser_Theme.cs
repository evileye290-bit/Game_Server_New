using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateZ;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GateServerLib
{
    public partial class Client
    {
        public void OnResponse_GetThemePassReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_THEMEPASS_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_THEMEPASS_REWARD>(stream);
            MSG_GateZ_GET_THEMEPASS_REWARD request = new MSG_GateZ_GET_THEMEPASS_REWARD();
            request.ThemeType = msg.ThemeType;
            request.GetAll = msg.GetAll;
            request.IsSuper = msg.IsSuper;
            request.RewardLevels.AddRange(msg.RewardLevels);
            WriteToZone(request);
        }

        public void OnResponse_ThemeBossDungeon(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_THEMEBOSS_DUNGEON msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_THEMEBOSS_DUNGEON>(stream);
            MSG_GateZ_THEMEBOSS_DUNGEON request = new MSG_GateZ_THEMEBOSS_DUNGEON();
            request.DungeonId = msg.DungeonId;
            WriteToZone(request);
        }

        public void OnResponse_GetThemeBossReward(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_GET_THEMEBOSS_REWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_GET_THEMEBOSS_REWARD>(stream);
            MSG_GateZ_GET_THEMEBOSS_REWARD request = new MSG_GateZ_GET_THEMEBOSS_REWARD();
            request.RewardId = msg.RewardId;
            WriteToZone(request);
        }

        public void OnResponse_UpdateThemeBossQueue(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_THEMEBOSS_UPDATE_DEFENSIVE_QUEUE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_THEMEBOSS_UPDATE_DEFENSIVE_QUEUE>(stream);
            MSG_GateZ_THEMEBOSS_UPDATE_DEFENSIVE_QUEUE request = new MSG_GateZ_THEMEBOSS_UPDATE_DEFENSIVE_QUEUE();
            msg.HeroDefInfos.ForEach(x =>
            {
                request.HeroDefInfos.Add(new HERO_DEFENSIVE_DATA() { HeroId = x.HeroId, QueueNum = x.QueueNum, PositionNum = x.PositionNum });
            });
            WriteToZone(request);
        }
    }
}
