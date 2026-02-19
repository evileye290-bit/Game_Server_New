using System;
using System.Web.Script.Serialization;

namespace ZoneServerLib
{

    public abstract class ABoilHttpQuery 
    {
        public JavaScriptSerializer serializer = new JavaScriptSerializer();
        private Func<string> m_callback;
        public string Result;

        //public string ErrorText;

        public virtual void SetResponse(string result)
        {
            Result = result;
        }

        //public virtual void SetErrorText(string text)
        //{
        //    ErrorText = text;
        //}

        public void SetCallBack(Func<string> callback)
        {
            m_callback = callback;
        }


        internal void PostUpdate()
        {
            if (m_callback != null)
            {
                m_callback();
            }
        }
    }

}
