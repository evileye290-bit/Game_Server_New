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
        public void OnResponse_OpenMailbox(MemoryStream stream, int uid = 0)
        {
            //查找角色名
            MSG_GZ_EMAIL_OPENE_BOX pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GZ_EMAIL_OPENE_BOX>(stream);
            Log.Write("player {0} open mail box", pks.PcUid);
            PlayerChar player = Api.PCManager.FindPc(pks.PcUid);
            if (player == null)
            {
                Log.Warn("player {0} open email box not in gateid {1} pc list", pks.PcUid, SubId);
                return;
            }
            //处理任务
            player.SyncEmailList(pks.Language);
        }

        /// <summary>
        /// 查看单个邮件
        /// </summary>
        /// <param name="stream">流中主要有邮件的Uid</param>
        public void OnResponse_ReadMail(MemoryStream stream, int uid = 0)
        {
            //查看单个邮件内容
            MSG_GZ_EMAIL_READ pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GZ_EMAIL_READ>(stream);
            Log.Write("player {0} read email uid {1} id {2} sendTime {3}", pks.PcUid, pks.Uid, pks.Id, pks.SendTime);
            PlayerChar player = Api.PCManager.FindPc(pks.PcUid);
            if (player == null)
            {
                Log.Warn("player {0} read email not in gateid {1} pc list", pks.PcUid, SubId);
                return;
            }
            //处理任务
            player.ReadEmail(pks.Uid, pks.Id, pks.SendTime, pks.Language);
        }

        /// <summary>
        /// 提取邮件附件
        /// </summary>
        /// <param name="stream">流中主要包含附件的uid</param>
        public void OnResponse_GetAttachment(MemoryStream stream, int uid = 0)
        {
            MSG_GZ_PICKUP_ATTACHMENT pks = MessagePacker.ProtobufHelper.Deserialize<MSG_GZ_PICKUP_ATTACHMENT>(stream);
            Log.Write("player {0} GetAttachment Uid:{1}", pks.PcUid, pks.Uid);
            PlayerChar player = Api.PCManager.FindPc(pks.PcUid);
            if (player == null)
            {
                Log.Warn("player {0} get attachment not in gateid {1} pc list", pks.PcUid, SubId);
                return;
            }
            if (pks.IsAll)
            {
                player.GetAllEmailAttachment();
            }
            else
            {
                player.GetEmailAttachment(pks.Uid, pks.Id, pks.SendTime);
            }
        }
    }
}
