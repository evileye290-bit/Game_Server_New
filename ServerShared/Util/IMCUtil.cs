using System.Collections.Generic;
using System.Linq;
using System;
using System.Reflection;

public static partial class IMCUtil
{
    #region Linq ext
    public class Stat : IDictionary<string, int>
    {
        private Dictionary<string, int> internalDict = new Dictionary<string, int>();

        //Sorry forgot this :P
        public String Name { get; set; }

        public void Add(string key, int value)
        {
            internalDict.Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return internalDict.ContainsKey(key);
        }

        public ICollection<string> Keys
        {
            get { return internalDict.Keys; }
        }

        public bool Remove(string key)
        {
            return internalDict.Remove(key);
        }

        public bool TryGetValue(string key, out int value)
        {
            return internalDict.TryGetValue(key, out value);
        }

        public ICollection<int> Values
        {
            get { return internalDict.Values; }
        }

        public int this[string key]
        {
            get
            {
                int _value = 0;
                if (internalDict.TryGetValue(key, out _value))
                {
                    return _value;
                }
                else return 0;
            }
            set
            {
                internalDict[key] = value;
            }
        }

        public void Add(KeyValuePair<string, int> item)
        {
            internalDict.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            internalDict.Clear();
        }

        public bool Contains(KeyValuePair<string, int> item)
        {
            return (internalDict.ContainsKey(item.Key) && internalDict.ContainsValue(item.Value));
        }

        public void CopyTo(KeyValuePair<string, int>[] array, int arrayIndex)
        {
            //Could be done but you prolly could figure this out yourself;
            throw new Exception("do not use");
        }

        public int Count
        {
            get { return internalDict.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(KeyValuePair<string, int> item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, int>> GetEnumerator()
        {
            return internalDict.GetEnumerator();
        }

        global::System.Collections.IEnumerator global::System.Collections.IEnumerable.GetEnumerator()
        {
            return internalDict.GetEnumerator();
        }
    }
    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> handler)
    {
        foreach (T item in enumerable)
            handler(item);
    }

    public static void ForEachWithIndex<T>(this IEnumerable<T> enumerable, Action<T, int> handler)
    {
        int idx = 0;
        foreach (T item in enumerable)
            handler(item, idx++);
    }

    public static void ForEachArrayPair<T>(this IEnumerable<T> enumerable, int pairCnt, Action<T[]> handler)
    {
        if (pairCnt <= 0) throw new ArgumentOutOfRangeException("pairCnt");
        var enumerator = enumerable.GetEnumerator();
        T[] arr = new T[pairCnt];
        int idx = 0;
        while (enumerator.MoveNext())
        {
            arr[idx++] = enumerator.Current;
            if (idx >= pairCnt)
            {
                idx = 0;
                handler(arr);
            }
        }
    }

    public static IEnumerable<IEnumerable<T>> ArrayPair<T>(this IEnumerable<T> enumerable, int pairCnt)
    {
        if (pairCnt <= 0) throw new ArgumentOutOfRangeException("pairCnt");

        var enumerator = enumerable.GetEnumerator();
        T[] arr = new T[pairCnt];
        int idx = 0;
        while (enumerator.MoveNext())
        {
            arr[idx++] = enumerator.Current;
            if (idx >= pairCnt)
            {
                idx = 0;
                yield return arr;
            }
        }
    }

    public static Stat ToStat<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
    {
        Stat stat = new Stat();
        foreach (TSource element in source)
        {
            stat.Add(keySelector(element).ToString(), elementSelector(element).ToString().ToInt());
        }
        return stat;
    }

    #endregion

    public static KeyValuePair<T1, T2> Pair<T1, T2>(this T1 key, T2 value)
    {
        return new KeyValuePair<T1, T2>(key, value);
    }
    /// <summary>
    /// 返回序列的最后一个元素的Key.
    /// 异常:
    ///   System.ArgumentNullException:
    ///     source 为 null。
    ///
    ///   System.InvalidOperationException:
    ///     源序列为空。
    /// </summary>
    public static T FindLast<T,K> (this Dictionary<T,K> dic)
    {
        try
        {
            T num = dic.Keys.Last();
            return num;
        }
        catch (ArgumentNullException )
        {
            throw;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
    }

    public static bool AddValue<K>(this Dictionary<K, int> dic, K key, int value)
    {
        if (key == null)
        {
            return false;
        }

        if (dic.ContainsKey(key))
        {
            dic[key] += value;
        }
        else
        {
            dic.Add(key,value);
        }
        return true;
    }

    public static bool AddValue<K>(this Dictionary<K, long> dic, K key, long value)
    {
        if (key == null)
        {
            return false;
        }

        if (dic.ContainsKey(key))
        {
            dic[key] += value;
        }
        else
        {
            dic.Add(key, value);
        }
        return true;
    }

    public static bool AddValue<K>(this Dictionary<K, int> dic, Dictionary<K, int> dic1)
    {
        foreach (var kv in dic1)
        {
            dic.AddValue(kv.Key, kv.Value);
        }
        return true;
    }

    public static bool AddValue<K>(this Dictionary<K, long> dic, Dictionary<K, long> dic1)
    {
        foreach (var kv in dic1)
        {
            dic.AddValue(kv.Key, kv.Value);
        }
        return true;
    }

    public static int ToInt(this char value)
    {
        try
        {
            return int.Parse(value.ToString());
        }
        catch (System.Exception)
        {
            return 0;
        }
    }
    public static int ToInt(this string value)
    {
        try
        {
            return int.Parse(value);
        }
        catch (System.Exception)
        {
            return 0;
        }
    }

    public static float ToFloat(this string value)
    {
        try
        {
            return float.Parse(value);
        }
        catch (System.Exception)
        {
            return 0;
        }
    }

    public static bool ToBool(this string value)
    {
        try
        {
            return bool.Parse(value);
        }
        catch (System.Exception)
        {
            return false;
        }
    }


    public static E ToEnum<E>(this string value) where E : struct, IConvertible
    {
        return (E)System.Enum.Parse(typeof(E), value);
    }

    //http://stackoverflow.com/questions/2587814/copy-an-object-to-another-object-but-only-with-the-same-fields
    public static void CopyFieldsTo<T, U>(this T source, U dest)
    {
        var plistsource = typeof(T).GetFields();//from prop1 in typeof(T).GetProperties() select prop1;        
        var plistdest = typeof(U).GetFields();//from prop2 in  select prop2;

        foreach (FieldInfo destprop in plistdest)
        {
            var sourceprops = plistsource.Where((p) => p.Name == destprop.Name &&
              destprop.FieldType.Equals(p.FieldType));

            foreach (FieldInfo sourceprop in sourceprops)
            { // should only be one                
                destprop.SetValue(dest, sourceprop.GetValue(source));
            }
        }
    }

    public static FieldInfo GetFieldByName<T>(this T source, string name)
    {
        return typeof(T).GetFields().FirstOrDefault(f => f.Name == name);
    }

    static public bool IsStringParam(string[] strings, string chkString)
    {
        for (int i = 0; i < strings.Length; ++i)
        {
            if (strings[i] == chkString) return true;
        }
        return false;
    }

    static public string GetStringParam(string[] strings, string chkString)
    {
        for (int i = 0; i < strings.Length; ++i)
        {
            if (strings[i].Contains(chkString)) return strings[i];
        }
        return null;
    }

    public static List<int> ToList(this string strings, char first)
    {
        if (string.IsNullOrEmpty(strings)) return new List<int>();
        try
        {
            return strings.Split(first).Where(x=>!string.IsNullOrEmpty(x)).ToList().ConvertAll(x => int.Parse(x));
        }
        catch (Exception e)
        {
        }

        return new List<int>();
    }

    public static Dictionary<int, int> ToDictionary(this string strings, char first, char second)
    {
        Dictionary<int, int> dic = new Dictionary<int, int>();
        if (string.IsNullOrEmpty(strings)) return dic;

        string[] kvs = strings.Split(first);
        if (kvs.Length > 0)
        {
            for (int i = 0; i < kvs.Length; i++)
            {
                string[] kv = kvs[i].Split(second);
                if (kv.Length != 2) continue;

                int k = 0, v = 0;
                if (int.TryParse(kv[0], out k) && int.TryParse(kv[1], out v))
                {
                    dic[k] = v;
                }
            }
        }

        return dic;
    }

    public static string ToString(this List<int> dic, string first)
    {
        return string.Join(first, dic);
    }

    public static string ToString<T>(this List<T> dic, string first)
    {
        return string.Join(first, dic);
    }

    public static string ToString(this Dictionary<int, int> dic, string first, string second)
    {
        List<string> strs = new List<string>();
        dic.ForEach(x => strs.Add($"{x.Key}{second}{x.Value}"));
        return string.Join(first, strs);
    }

    public static string ToString<K,V>(this Dictionary<K, V> dic, string first, string second)
    {
        List<string> strs = new List<string>();
        dic.ForEach(x => strs.Add($"{x.Key}{second}{x.Value}"));
        return string.Join(first, strs);
    }

    public static string FirstCharTower(this string stringInfo)
    {
        return stringInfo.Substring(0, 1).ToLower() + stringInfo.Substring(1);
    }
}
