namespace PayServerLib
{
    internal class SDKFactory
    {
        public static PayServerBase BuildBasePayServer(SDKType type, int port)
        {
            switch (type)
            {
                case SDKType.SEA: 
                    return new SEAPayServer(port.ToString());
                default: return null;
            }
        }
    }
}
