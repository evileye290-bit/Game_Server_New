namespace PayServerLib
{
    public enum VMallErrorCode
    {
        Success = 0,
        /// <summary>
        /// ⽤户不存在
        /// </summary>
        NoAccount = -600,

        /// <summary>
        /// 区服维护中
        /// </summary>
        IsMaintaining = -601,

        /// <summary>
        /// 区服不存在
        /// </summary>
        NoServer = -602,

        /// <summary>
        /// ⻆⾊不存在
        /// </summary>
        NoPlayer = -603,


        OtherError = -604,
    }
}