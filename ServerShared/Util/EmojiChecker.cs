using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
namespace ServerShared
{
    public class EmojiChecker
    {
        Regex reg = null;
        public EmojiChecker()
        {
            reg = new Regex("(\uD83C[\uDDE8-\uDDFF]\uD83C[\uDDE7-\uDDFF])|[\uD800-\uDBFF][\uDC00-\uDFFF]|[\u2600-\u27ff][\uFE0F]|[\u2600-\u27ff]");
        }
        //是否包含emoji符号
        public bool HasEmoji(string text)
        {
            //Regex reg = new Regex("(\uD83C[\uDDE8-\uDDFF]\uD83C[\uDDE7-\uDDFF])|[\uD800-\uDBFF][\uDC00-\uDFFF]|[\u2600-\u27ff][\uFE0F]|[\u2600-\u27ff]");
            //Regex reg = new Regex("[\uD800-\uDBFF][\uDC00-\uDFFF]");
            return reg.IsMatch(text);
        }

        //过滤emoji符号 
        public string FilterEmoji(string text, string filterStr = "?")
        {
            //Regex reg = new Regex("(\uD83C[\uDDE8-\uDDFF]\uD83C[\uDDE7-\uDDFF])|[\uD800-\uDBFF][\uDC00-\uDFFF]|[\u2600-\u27ff][\uFE0F]|[\u2600-\u27ff]");
            //Regex reg = new Regex("[\uD800-\uDBFF][\uDC00-\uDFFF]");
            return reg.Replace(text, filterStr);
        }

    }
}
