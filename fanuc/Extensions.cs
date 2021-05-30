using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace l99.driver.fanuc
{
    public static class Extensions
    {
        public static string AsAscii(this string text)
        {
            return Regex.Replace(text, @"[^\u0000-\u007F]+", string.Empty);
        }

        public static string AsAscii(this char character)
        {
            if (char.IsControl(character))
                return "";

            return character.ToString().Trim();
        }
        
        public static bool IsDifferentHash(this IEnumerable<dynamic> one, IEnumerable<dynamic> two)
        { 
            var one_hc = one.Select(x => x.GetHashCode());
            var two_hc = two.Select(x => x.GetHashCode());

            if (one_hc.Except(two_hc).Count() + two_hc.Except(one_hc).Count() > 0)
                return true;

            return false;
        }
    }
}