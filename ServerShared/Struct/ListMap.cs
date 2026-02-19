using System.Collections;
using System.Collections.Generic;
using System.Web.UI.WebControls;

namespace ServerShared
{
    public class ListMap<A, B> : IEnumerable
    {
        private readonly Dictionary<A, List<B>> dic = new Dictionary<A, List<B>>();

        public List<B> this[A a]
        {
            get
            {
                List<B> first;
                TryGetValue(a, out first);
                return first;
            }
        }

        public Dictionary<A,List<B>>.KeyCollection Keys => dic.Keys;

        public void Add(A a, B b)
        {
            List<B> first;
            if (!dic.TryGetValue(a, out first))
            {
                first = new List<B>();

                dic.Add(a, first);
            }
            first.Add(b);
        }

        public void Add(A a, List<B> b)
        {
            List<B> first;
            if (!dic.TryGetValue(a, out first))
            {
                first = new List<B>();

                dic.Add(a, first);
            }
            first.AddRange(b);
        }

        public bool ContainsKey(A a)
        {
            return dic.ContainsKey(a);
        }

        public bool TryGetValue(A a, out List<B> bs)
        {
            if (dic.TryGetValue(a, out bs))
            {
                return true;
            }
            bs = null;
            return false;
        }

        public void Remove(A a)
        {
            dic.Remove(a);
        }

        public void Remove(A a, B b)
        {
            List<B> first;
            if (dic.TryGetValue(a, out first))
            {
                first.Remove(b);
            }
        }

        public void Clear()
        {
            dic.Clear();
        }

        public IEnumerator GetEnumerator()
        {
            return dic.GetEnumerator();
        }
    }
}
