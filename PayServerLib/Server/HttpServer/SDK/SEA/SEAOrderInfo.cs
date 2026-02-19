namespace PayServerLib
{
    public class SEAOrderInfo
    {
        public string userId;
        public string orderId;
        public string serverId;
        public string productId;

        public string payMoney;
        public string payCurrency;
        public string isSandbox;

        public string payMode;
    }

    public class SEAVerifyPayResult
    {
        public int code;
        public string message;
        public SEAPayOrderVerifyInfo data;
        public int timestemp;
        public string request_id;
        public string trace_id;
    }


    public class SEAPayOrderVerifyInfo
    {
        public string extension_info;
        public int game_id;
        public int is_sandbox;
        public int merchant_id;
        public string order_no;
        public int order_status;
        public string out_trade_no;
        public int pay_mode;
        public int pay_money;
        public string pay_currency;
        public int pay_time;
        public string product_id;
        public string server_id;
        public string uid;
    }
}
