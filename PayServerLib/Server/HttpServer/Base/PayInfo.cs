using Message.Pay.Protocol.PM;

namespace PayServerLib
{
    public class PayInfo
    {
        private PaySDKBase sdk;
        private MSG_PM_RECHARGE_RESULT message;

        public PayInfo(MSG_PM_RECHARGE_RESULT pks, PaySDKBase sdk)
        {
            this.message = pks;
            this.sdk = sdk;
        }

        public void DoPay()
        {
            sdk.DoPay(message);
        }
    }
}
