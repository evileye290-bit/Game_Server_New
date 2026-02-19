using EnumerateUtility;
using Message.Client.Protocol.CGate;
using Message.Gate.Protocol.GateC;
using Message.Gate.Protocol.GateZ;
using ServerModels;
using System.IO;

namespace GateServerLib
{
    public partial class Client
    {
        public void OnResponse_CreateGuild(MemoryStream stream)
        {
            if (curZone == null) return;
            MSG_CG_CREATE_GUILD msg = MessagePacker.ProtobufHelper.Deserialize<MSG_CG_CREATE_GUILD>(stream);

            if (IsGm == 0)
            {
                if (server.NameChecker.GetWordLen(msg.GuildName) > WordLengthLimit.CharNameLenLimit)
                {
                    MSG_ZGC_CREATE_GUILD response = new MSG_ZGC_CREATE_GUILD();
                    response.Result = (int)ErrorCode.LengthLimit;
                    Write(response);
                    return;
                }
                if (server.NameChecker.HasBadWord(msg.GuildName) || server.NameChecker.HasSpecialSymbol(msg.GuildName) || server.NameChecker.CheckNameWord(msg.GuildName))
                {
                    MSG_ZGC_CREATE_GUILD response = new MSG_ZGC_CREATE_GUILD();
                    response.Result = (int)ErrorCode.BadWord;
                    Write(response);
                    return;
                }

                if (server.NameChecker.GetWordLen(msg.GuildSignature) > WordLengthLimit.SignatureSize)
                {
                    MSG_ZGC_CREATE_GUILD response = new MSG_ZGC_CREATE_GUILD();
                    response.Result = (int)ErrorCode.LengthLimit;
                    Write(response);
                    return;
                }
                if (server.NameChecker.HasBadWord(msg.GuildSignature) || server.NameChecker.HasSpecialSymbol(msg.GuildName))
                {
                    MSG_ZGC_CREATE_GUILD response = new MSG_ZGC_CREATE_GUILD();
                    response.Result = (int)ErrorCode.BadWord;
                    Write(response);
                    return;
                }
            }
           

            MSG_GateZ_CREATE_GUILD msg2z = new MSG_GateZ_CREATE_GUILD();
            msg2z.PcUid = Uid;
            msg2z.GuildName = msg.GuildName;
            msg2z.GuildIcon = msg.GuildIcon;
            msg2z.GuildSignature = msg.GuildSignature;
            WriteToZone(msg2z);
        }



    }
}
