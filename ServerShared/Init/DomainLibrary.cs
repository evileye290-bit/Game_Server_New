using DataProperty;
using ServerModels;
using System.Collections.Generic;

namespace ServerShared
{
    public class DomainLibrary
    {
        private static Dictionary<int, DomainModel> domainList = new Dictionary<int, DomainModel>();

        public static void Init()
        {
            Dictionary<int, DomainModel> domainList = new Dictionary<int, DomainModel>();
            //domainList.Clear();
            DataList dataList = DataListManager.inst.GetDataList("Domain");
            foreach (var item in dataList)
            {
                Data data = item.Value;
                domainList.Add(item.Key, new DomainModel(data));
            }
            DomainLibrary.domainList = domainList;
        }

        public static DomainModel GetDomain(int id)
        {
            DomainModel domain = null;
            domainList.TryGetValue(id, out domain);
            return domain;
        }
    }
}
