namespace PayServerLib
{
    public class VMResponse
    {
        public int code { get; set; }
        public string message { get; set; }

        public object data { get; set; }

        protected VMResponse()
        {
        }


        public VMResponse(VMallErrorCode errorCode, object data, string errorMessage = "success")
        {
            this.code = (int)errorCode;
            this.data = data;
            this.message = errorMessage;
        }

        public static VMResponse GetSuccess()
        {
            VMResponse response = new VMResponse() { code = 0, message = "success" };
            return response;
        }


        public static VMResponse GetFail(VMallErrorCode errorCode, string message = "")
        {
            VMResponse response = new VMResponse() { code = (int)errorCode, message = message };
            return response;
        }
    }


}