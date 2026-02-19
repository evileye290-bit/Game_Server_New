using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ServerShared
{
    public class DoubleDeapthListMap<A, B, C> : IEnumerable
    {
        private readonly Dictionary<A, Dictionary<B, List<C>>> dic = new Dictionary<A, Dictionary<B, List<C>>>();

        public Dictionary<B, List<C>> this[A a]
        {
            get
            {
                Dictionary<B, List<C>> first;
                dic.TryGetValue(a, out first);
                return first;
            }
        }

        public List<C> this[A a, B b]
        {
            get
            {
                List<C> second;
                TryGetValue(a, b, out second);
                return second;
            }
        }


        public void Add(A a, B b, C c)
        {
            List<C> second;
            Dictionary<B, List<C>> first;
            if (!dic.TryGetValue(a, out first))
            {
                first = new Dictionary<B, List<C>>();
                second = new List<C>();
                second.Add(c);

                first.Add(b, second);
                dic.Add(a, first);
            }
            else
            {
                if (!first.TryGetValue(b, out second))
                {
                    second = new List<C>();
                    second.Add(c);
                    first.Add(b, second);
                }
                else
                { 
                    second.Add(c);
                }
            }
        }

        public void Add(A a, B b, List<C> cs)
        {
            List<C> second;
            Dictionary<B, List<C>> first;
            if (!dic.TryGetValue(a, out first))
            {
                first = new Dictionary<B, List<C>>();
                second = new List<C>();
                second.AddRange(cs);

                first.Add(b, second);
                dic.Add(a, first);
            }
            else
            {
                if (!first.TryGetValue(b, out second))
                {
                    second = new List<C>(cs);
                    second.AddRange(cs);

                    first.Add(b, second);
                }
                else
                { 
                    second.AddRange(cs);
                }
            }
        }

        public bool TryGetValue(A a, out Dictionary<B, List<C>> first)
        {
            if (dic.TryGetValue(a, out first))
            {
                return true;
            }
            first = null;
            return false;
        }

        public bool TryGetValue(A a, B b, out List<C> cs)
        {
            Dictionary<B, List<C>> first;
            if (dic.TryGetValue(a, out first))
            {
                if (first.TryGetValue(b, out cs))
                {
                    return true;
                }
            }
            cs = null;
            return false;
        }

        public void Remove(A a, B b)
        {
            Dictionary<B, List<C>> first;
            if (dic.TryGetValue(a, out first))
            {
                first.Remove(b);
                if (first.Count == 0)
                {
                    dic.Remove(a);
                }
            }
        }

        public void Remove(A a, B b, C c)
        {
            Dictionary<B, List<C>> first;
            if (dic.TryGetValue(a, out first))
            {
                List<C> second;
                if (first.TryGetValue(b, out second))
                {
                    second.Remove(c);
                    if (second.Count == 0)
                    {
                        first.Remove(b);
                    }
                }
                if (first.Count == 0)
                {
                    dic.Remove(a);
                }
            }
        }

        public int Count()
        {
            return dic.Count;
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
