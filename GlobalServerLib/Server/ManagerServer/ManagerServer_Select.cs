using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;
using DBUtility;
using EnumerateUtility;
using Message.Manager.Protocol.MG;
using ServerFrame;
using ServerModels;

namespace GlobalServerLib
{
    public partial class ManagerServer : FrontendServer
    {
    
        public void OnResponse_RoleInfo(MemoryStream stream, int uid = 0)
        {
            MSG_MG_RoleInfo msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MG_RoleInfo>(stream);
            int sessionUid = msg.SessionUid;

            AHttpSession session = GlobalServerApi.GetCacheHttpSession(sessionUid);
            if (session!=null)
            {
                Dictionary<string, object> dic = new Dictionary<string, object>();
                dic.Add("errorCode", msg.ErrorCode);
                dic.Add("errorMessage", "success");

                if (msg.ErrorCode == 0)
                {
                    RoleInfo roleInfo = new RoleInfo();
                    roleInfo.roleName = msg.RoleName;
                    roleInfo.roleLevel = msg.RoleLevel;
                    roleInfo.account = msg.Account;
                    roleInfo.roleId = msg.RoleId;
                    roleInfo.lastLoginTime = msg.LastLoginTime;
                    roleInfo.vipLevel = msg.VipLevel;
                    roleInfo.rechargeSum = msg.RechargeSum;
                    roleInfo.partnerId = msg.PartnerId;

                    dic.Add("data", roleInfo);
                }

                string jsonStr = SelectSession.ObjectToJson(dic);
                session.AnswerHttpCmd(jsonStr);

                GlobalServerApi.SelectCallBackDic.Remove(sessionUid);
            }

        }


    }
}
