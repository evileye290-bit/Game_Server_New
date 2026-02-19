using System.Collections.Generic;
using System.IO;
using Message.IdGenerator;
using Message.Manager.Protocol.MP;
using ServerFrame;

namespace PayServerLib
{
    public class ManagerServer : FrontendServer
    {
        PayServerApi server;

        public ManagerServer(BaseApi api)
            : base(api)
        {
            server = (PayServerApi)api;
        }

        protected override void BindResponser()
        {
            base.BindResponser();

            AddResponser(Id<MSG_MP_GET_ROLE_INFO>.Value, OnResponse_GetRoleInfo);
            AddResponser(Id<MSG_MP_RECHAEGE>.Value, OnResponse_GetRecharge);
            AddResponser(Id<MSG_MP_WEB_RECHAEGE>.Value, OnResponse_GetWebRecharge);
        }

        private void OnResponse_GetWebRecharge(MemoryStream stream, int uid = 0)
        {
            MSG_MP_WEB_RECHAEGE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MP_WEB_RECHAEGE>(stream);
            int sessionUid = msg.SessionUid;

            VMallSession session = server.GetCacheHttpSession(sessionUid);
            if (session != null)
            {
                if (msg.ErrorCode == 0)
                {
                    Dictionary<string, object> dataDic = new Dictionary<string, object>();
                    dataDic.Add("out_trade_no", msg.OrderId);
                    dataDic.Add("notify_ur", server.PayUrl);
                    dataDic.Add("extension_info", $"{msg.OrderId}_{msg.GameId}_{msg.ServerId}");

                    //string jsonStr = VMallHelper.JsonSerialize(dataDic);

                    VMResponse response = VMResponse.GetSuccess();
                    response.data = dataDic;
                    session.WriteResponse(response);
                }
                else
                {
                    VMResponse response = VMResponse.GetFail(VMallErrorCode.OtherError, "web recharge not find product id");
                    session.WriteResponse(response);
                }
            }
        }

        private void OnResponse_GetRoleInfo(MemoryStream stream, int uid = 0)
{
            MSG_MP_GET_ROLE_INFO msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MP_GET_ROLE_INFO>(stream);
            int sessionUid = msg.SessionUid;

            VMallSession session = server.GetCacheHttpSession(sessionUid);
            if (session != null)
            {
                if (msg.ErrorCode == 0)
                {
                    Dictionary<string, object> dataDic = new Dictionary<string, object>();
                    dataDic.Add("roleName", msg.RoleName);
                    dataDic.Add("roleLevel", msg.RoleLevel);
                    dataDic.Add("account", msg.Account);
                    dataDic.Add("roleId", msg.RoleId);
                    dataDic.Add("lastLoginTime", msg.LastLoginTime);
                    dataDic.Add("vipLevel", 0);
                    dataDic.Add("rechargeSum", msg.RechargeSum);
                    dataDic.Add("registerTime", msg.RegisterTime);
                    dataDic.Add("channelId", msg.PartnerId);

                    //string jsonStr = VMallHelper.JsonSerialize(dataDic);

                    VMResponse response = VMResponse.GetSuccess();
                    response.data = dataDic;
                    session.WriteResponse(response);
                }
                else
                {
                    //VMResponse response = VMResponse.GetFail(VMallErrorCode.OtherError,((ErrorCode)msg.ErrorCode).ToString());
                    //session.WriteResponse(response);
                }
            }

        }

        private void OnResponse_GetRecharge(MemoryStream stream, int uid = 0)
{
            MSG_MP_RECHAEGE msg = MessagePacker.ProtobufHelper.Deserialize<MSG_MP_RECHAEGE>(stream);
            int sessionUid = msg.SessionUid;

            VMallSession session = server.GetCacheHttpSession(sessionUid);
            if (session != null)
            {
                if (msg.ErrorCode == 0)
                {
                    VMResponse response = VMResponse.GetSuccess();
                    //response.data = session.Data;
                    session.WriteResponse(response);
                }
                else
                {
                    //VMResponse response = VMResponse.GetFail(VMallErrorCode.OtherError, ((ErrorCode)msg.ErrorCode).ToString());
                    //session.WriteResponse(response);
                }
            }
        }
    }
}
