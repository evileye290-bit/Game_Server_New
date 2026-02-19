using System.Collections;
using System.Collections.Generic;

namespace ServerShared
{
    public class DoubleDepthMap<A, B, C> : IEnumerable
    {
        private readonly Dictionary<A, Dictionary<B, C>> dic = new Dictionary<A, Dictionary<B, C>>();

        public Dictionary<B, C> this[A a]
        {
            get
            {
                Dictionary<B, C> first;
                dic.TryGetValue(a, out first);
                return first;
            }
            set
            {
                dic[a] = value;
            }
        }

        public void Add(A a, B b, C c)
        {
            Dictionary<B, C> first;
            if (!dic.TryGetValue(a, out first))
            {
                first = new Dictionary<B, C>();
                first.Add(b, c);

                dic.Add(a, first);
            }
            else
            {
                first.Add(b, c);
            }
        }

        public void Update(A a, B b, C c)
        {
            Dictionary<B, C> first;
            if (!dic.TryGetValue(a, out first))
            {
                first = new Dictionary<B, C>();
                first.Add(b, c);

                dic.Add(a, first);
            }
            else
            {
                first[b] = c;
            }
        }

        public bool ContainsKey(A a)
        {
            return dic.ContainsKey(a);
        }

        public bool TryGetValue(A a, out Dictionary<B, C> first)
        {
            if (dic.TryGetValue(a, out first))
            {
                return true;
            }
            first = null;
            return false;
        }

        public bool TryGetValue(A a, B b, out C cs)
        {
            Dictionary<B, C> first;
            if (dic.TryGetValue(a, out first))
            {
                if (first.TryGetValue(b, out cs))
                {
                    return true;
                }
            }

            cs = default(C);

            return false;
        }

        public void Remove(A a)
        {
            dic.Remove(a);
        }

        public void Remove(A a, B b)
        {
            Dictionary<B, C> first;
            if (dic.TryGetValue(a, out first))
            {
                first.Remove(b);
                if (first.Count == 0)
                {
                    dic.Remove(a);
                }
            }
        }

        public int Count => dic.Count;

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
