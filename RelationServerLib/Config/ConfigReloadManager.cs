using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelationServerLib
{
    public class ConfigReloadManager
    {
        private List<ILoadConfig> loadConfigList = new List<ILoadConfig>();

        public void Add(ILoadConfig loader)
        {
            loadConfigList.Add(loader);
        }

        public void LoadConfig()
        {
            loadConfigList.ForEach(x => x.LoadConfig());
        }
    }
}
