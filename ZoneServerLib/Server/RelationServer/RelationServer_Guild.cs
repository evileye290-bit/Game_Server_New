using Message.Relation.Protocol.RZ;
using EnumerateUtility;
using System.IO;
using DBUtility;
using Logger;
using Message.Gate.Protocol.GateC;
using System;
using Message.Gate.Protocol.GateZ;

namespace ZoneServerLib
{
    public partial class RelationServer
    {
        private void OnResponse_MaxGuildId(MemoryStream stream, int uid = 0)
        {
            MSG_RZ_MAX_GUILDID msg = MessagePacker.ProtobufHelper.Deserialize<MSG_RZ_MAX_GUILDID>(stream);
            PlayerChar player = Api.PCManager.FindPc(msg.PcUid);
            if (player == null)
            {
                return;
            }
            if (msg.Result == (int)ErrorCode.Success)
            {
                // 尝试创建公会
                Api.GameDBPool.Call(new QueryInsertGuildName(player.createGuildMsg.GuildName, msg.MaxGuildId), ret =>
                {
                    // 尝试创建公会名
                    if ((int)ret != 1)
                    {
                        MSG_ZGC_CREATE_GUILD response = new MSG_ZGC_CREATE_GUILD();
                        //公会名存在
                        Log.Write("player {0} try to create guild name {1} failed: name exited", player.Uid, player.createGuildMsg.GuildName);
                        player.ClearCreateGuildMsg();
                        response.Result = (int)ErrorCode.GuildNameExist;
                        player.Write(response);
                        return;
                    }
                    else
                    {
                        //创建公会
                        player.CreateGuild(msg.MaxGuildId);
                    }
                });
            }
            else
            {
                //获取id失败，
                MSG_ZGC_CREATE_GUILD response = new MSG_ZGC_CREATE_GUILD();
                //公会名存在
                Log.ErrorLine("player {0} try to create guild name {1} failed: wrong id {2}", player.Uid, player.createGuildMsg.GuildName,msg.MaxGuildId);
                player.ClearCreateGuildMsg();
                response.Result = (int)ErrorCode.Fail;
                player.Write(response);
            }

        }



      
    }
}
