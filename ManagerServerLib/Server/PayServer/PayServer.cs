using System;
using System.IO;
using Message.IdGenerator;
using Logger;
using Message.Barrack.Protocol.BM;
using ServerFrame;
using EnumerateUtility;
using Message.Pay.Protocol.PM;

namespace ManagerServerLib
{
    public partial class PayServer : BackendServer
    {
        private ManagerServerApi Api
        { get { return (ManagerServerApi)api; } }

        public PayServer(BaseApi api) : base(api)
        {
        }

        protected override void BindResponser()
        {
            base.BindResponser();
            AddResponser(Id<MSG_PM_RECHARGE_RESULT>.Value, OnResponse_PcRecharge);
            AddResponser(Id<MSG_PM_WEB_RECHAEGE>.Value, OnResponse_GetWebRecharge);
        }

        public void OnResponse_PcRecharge(MemoryStream stream, int uid = 0)
        {
            MSG_PM_RECHARGE_RESULT pks = MessagePacker.ProtobufHelper.Deserialize<MSG_PM_RECHARGE_RESULT>(stream);
            if (pks.Status == 1)
            {
                DateTime time;
                if (!DateTime.TryParse(pks.PayTime, out time))
                {
                    time = ManagerServerApi.now;
                    Log.Warn($"player recharge id {pks.OrderId} info {pks.OrderInfo} time {pks.PayTime} amount {pks.Money} PayCurrency  {pks.PayCurrency} IsSandbox {pks.IsSandbox} PayMode {pks.PayMode} error: not find pay time");
                }
                Api.RechargeMng.UpdateRechargeManager(pks.OrderId, pks.OrderInfo, time, pks.Money, pks.PayCurrency, RechargeWay.SDK, pks.IsSandbox, pks.PayMode);
            }
            else
            {
                Log.Warn($"player recharge id {pks.OrderId} info {pks.OrderInfo} time {pks.PayTime} amount {pks.Money} PayCurrency  {pks.PayCurrency} error: status {pks.Status}");
            }
        }
    }
}