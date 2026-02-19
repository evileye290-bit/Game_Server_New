using System;
using System.Reflection;
using System.Windows.Forms;

namespace ServerFrame
{
    public class ApiFactory
    {
        public static BaseApi CreateApi()
        {
            string productName = Application.ProductName;
            string assemblyName = productName + "Lib";
            string className = assemblyName + "." + productName + "Api";
            Assembly assembly = null;
            Assembly[] asses = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var ass in asses)
            {
                if (ass.ToString().Contains(assemblyName))
                {
                    assembly = ass;
                    break;
                }
            }
            if (assembly == null)
            {
                assembly = Assembly.Load(assemblyName);
            }
            if (assembly != null)
            {
                return (BaseApi)assembly.CreateInstance(className);
            }
            return null;
        }

    }
}
