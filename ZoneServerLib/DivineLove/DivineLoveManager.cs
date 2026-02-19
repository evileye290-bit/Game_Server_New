using ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoneServerLib
{
    public class DivineLoveManager
    {
        private Dictionary<int, DivineLoveInfo> divineLoveList = new Dictionary<int, DivineLoveInfo>();
        public Dictionary<int, DivineLoveInfo> DivineLoveList { get { return divineLoveList; } }

        private PlayerChar owner { get; set; }

        public DivineLoveManager(PlayerChar owner)
        {
            this.owner = owner;
        }

        public void Init(List<DivineLoveInfo> infoList)
        {
            foreach (var info in infoList)
            {
                divineLoveList.Add(info.Type, info);
            }
        }

        public DivineLoveInfo GetDivineLoveInfo(int type)
        {
            DivineLoveInfo info;
            divineLoveList.TryGetValue(type, out info);
            return info;
        }

        public void AddInfo(DivineLoveInfo info)
        {         
            divineLoveList.Add(info.Type, info);
        }

        public void Clear()
        {
            foreach (var item in divineLoveList)
            {
                item.Value.ResetInfo();
            }
        }
    }
}
