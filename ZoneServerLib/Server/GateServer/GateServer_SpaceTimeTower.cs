using Logger;
using Message.Gate.Protocol.GateZ;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Message.Gate.Protocol.GateC;

namespace ZoneServerLib
{
    public partial class GateServer
    {
        public void OnResponse_SpaceTimeJoinTeam(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_SPACE_TIME_JOIN_TEAM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_SPACE_TIME_JOIN_TEAM>(stream);
            Log.Write("player {0} request space time join team {1}", uid, msg.Index);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} space time join team not in gateid {1} pc list", uid, SubId);
                return;
            }
            player.SpaceTimeHeroJoinTeam(msg.Index);
        }

        public void OnResponse_SpaceTimeQuitTeam(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_SPACE_TIME_QUIT_TEAM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_SPACE_TIME_QUIT_TEAM>(stream);
            Log.Write("player {0} request space time quit team {1}", uid, msg.HeroId);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} space time quit team not in gateid {1} pc list", uid, SubId);
                return;
            }
            player.SpaceTimeHeroQuitTeam(msg.HeroId);
        }
        
        /// <summary>
        /// 处理客户端手动刷新卡池
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="uid"></param>
        public void OnResponse_SpaceTimeRefreshCardPool(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_SPACETIME_REFRESH_CARD_POOL msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_SPACETIME_REFRESH_CARD_POOL>(stream);
            Log.Write("player {0} request space time refresh card pool", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} space time quit team not in gateid {1} pc list", uid, SubId);
                return;
            }
            player.SpaceTimeRefreshCardPool();
        }

        public void OnResponse_SpaceTimeHeroStepUp(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_SPACETIME_HERO_STEPUP msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_SPACETIME_HERO_STEPUP>(stream);
            Log.Write("player {0} request space time hero {1} step up", uid, msg.IndexList.ToString());

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} space time hero step up not in gateid {1} pc list", uid, SubId);
                return;
            }
            player.SpaceTimeHeroStepUp(msg.IndexList);
        }

        public void OnResponse_UpdateSpaceTimeHeroQueue(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_UPDATE_SPACETIME_HERO_QUEUE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_UPDATE_SPACETIME_HERO_QUEUE>(stream);
            Log.Write("player {0} request update space time hero queue", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} update space time hero queue not in gateid {1} pc list", uid, SubId);
                return;
            }
            player.UpdateSpaceTimeHeroQueue(msg.HeroDefInfos);
        }

        public void OnResponse_SpaceTimeExecuteEvent(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_SPACETIME_EXECUTE_EVENT msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_SPACETIME_EXECUTE_EVENT>(stream);
            Log.Write("player {0} request space time execute event {1} param {2}", uid, msg.Type, msg.Param);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} space time execute event not in gateid {1} pc list", uid, SubId);
                return;
            }

            List<int> lstParam = new List<int>();
            foreach (var param in msg.ArrParam)
            {
                lstParam.Add(param);
            }

            player.SpaceTimeExecuteEvent(msg.Type, msg.Param, lstParam);
        }
        
        public void OnResponse_SpaceTimeGetStageAward(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_SPACETIME_GET_STAGE_AWARD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_SPACETIME_GET_STAGE_AWARD>(stream);
            Log.Write("player {0} request space time get stage award  page : [{1}]", uid, msg.Page);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} space time get stage award not in gateid {1} pc list", uid, SubId);
                return;
            }
            player.SpaceTimeGetStageAward(msg.Page);
        }

        public void OnResponse_SpaceTimeReset(MemoryStream stream, int uid = 0)
        {
            Log.Write("player {0} request space time reset", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} space time reset not in gateid {1} pc list", uid, SubId);
                return;
            }
            player.SpaceTimeReset();
        }
        
        public void OnResponse_SelectGuideSoulItem(MemoryStream stream, int uid = 0)
        {
            MSG_GateZ_SELECT_GUIDESOUL_ITEM msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GateZ_SELECT_GUIDESOUL_ITEM>(stream);
            Log.Write("player {0} request select guide soul item {1}", uid, msg.ItemId);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} select guide soul item not in gateid {1} pc list", uid, SubId);
                return;
            }
            player.SelectGuideSoulItem(msg.ItemId);
        }
        
        public void OnResponse_SpaceTimeEnterNextLevel(MemoryStream stream, int uid = 0)
        {
            Log.Write("player {0} request space time enter next level", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} space time enter next level not in gateid {1} pc list", uid, SubId);
                return;
            }
            player.SpaceTimeEnterNextLevel();
        }
        
        public void OnResponse_SpaceTimeBeastSettlement(MemoryStream stream, int uid = 0)
        {
            Log.Write("player {0} request space time beast settlement", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} space time beast settlement not in gateid {1} pc list", uid, SubId);
                return;
            }
            player.SpaceTimeBeastDungeonSettlement();
        }
        
        public void OnResponse_SpaceTimeHouseRandomParam(MemoryStream stream, int uid = 0)
        {
            Log.Write("player {0} request space time house random param", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} space time house random param not in gateid {1} pc list", uid, SubId);
                return;
            }
            player.SpaceTimeHouseRandomParam();
        }
        
        public void OnResponse_EnterSpaceTimeTower(MemoryStream stream, int uid = 0)
        {
            Log.Write("player {0} request enter space time tower", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} enter space time tower not in gateid {1} pc list", uid, SubId);
                return;
            }
            player.EnterSpaceTimeTower();
        }
        
        public void OnResponse_SpaceTimeGetPastRewards(MemoryStream stream, int uid = 0)
        {
            Log.Write("player {0} request space time get past rewards", uid);

            PlayerChar player = Api.PCManager.FindPc(uid);
            if (player == null)
            {
                Log.Warn("player {0} space time get past rewards not in gateid {1} pc list", uid, SubId);
                return;
            }
            player.SpaceTimeGetPastRewards();
        }
    }
}
