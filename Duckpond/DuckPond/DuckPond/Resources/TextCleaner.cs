using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DuckPond.Resources
{
    class TextCleaner
    {
        public static String CleanHexString (String str)
        {
            Regex rgx = new Regex("[^A-H0-9]"); //Maybe some characters need to be scaped. 
            return rgx.Replace(str.ToUpper(), "");
        }

        public static String Clean(String str, int limit)
        {
            str = str.Trim();
            str = str.Substring(0, limit);
            return str;
        }
    }
}
