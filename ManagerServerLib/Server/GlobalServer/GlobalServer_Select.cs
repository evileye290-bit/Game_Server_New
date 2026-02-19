using DataProperty;
using Logger;
using Message.Global.Protocol.GM;
using Message.Manager.Protocol.MR;
using ServerShared;
using System.IO;
using DBUtility;
using Message.Manager.Protocol.MG;
using ServerFrame;
using System.Linq;
using Message.Manager.Protocol.MZ;
using EnumerateUtility;
using Message.Manager.Protocol.MGate;

namespace ManagerServerLib
{
    public partial class GlobalServer
    {
      
        private void OnResponse_queryRoleInfo(MemoryStream stream, int uid = 0)
        {
            MSG_GM_queryRoleInfo msg = MessagePacker.ProtobufHelper.Deserialize<MSG_GM_queryRoleInfo>(stream);
            MSG_MG_RoleInfo response = new MSG_MG_RoleInfo();
            response.SessionUid = msg.SessionUid;
            response.RoleId = msg.RoleId;
            response.ErrorCode = 0;

            var query =  new QueryRoleInfo(msg.RoleId);
            Api.GameDBPool.Call(query, ret =>
            {
                if ((int)ret == 1)
                {
                    response.VipLevel = query.roleInfo.vipLevel;

                    response.RoleName = query.roleInfo.roleName;
                    response.RoleLevel = query.roleInfo.roleLevel;
                    response.Account = query.roleInfo.account;
                    response.RoleId = query.roleInfo.roleId;
                    response.LastLoginTime = query.roleInfo.lastLoginTime;
                    response.RechargeSum = query.roleInfo.rechargeSum;
                    response.PartnerId = query.roleInfo.partnerId;
                    Write(response);
                }
                else
                {
                    response.ErrorCode = 1;
                    Write(response);
                }
            });
        }

    }
    
}
