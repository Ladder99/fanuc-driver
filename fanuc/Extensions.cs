// ReSharper disable once CheckNamespace

using System.Dynamic;

namespace l99.driver.fanuc;

public static class Extensions
{
    public static string AsAscii(this string text)
    {
        return Regex
            //.Replace(text, @"[^\u0000-\u007F]+", string.Empty)
            .Replace(text, @"\p{C}+", string.Empty)
            .Replace("\0", string.Empty);
    }

    public static string AsAscii(this char character)
    {
        if (char.IsControl(character))
            return "";

        return character.ToString().Trim();
    }

    /*
    public static bool IsDifferentHash(this IEnumerable<dynamic> one, IEnumerable<dynamic> two)
    { 
        var oneHc = one.Select(x => x.GetHashCode());
        var twoHc = two.Select(x => x.GetHashCode());

        if (oneHc.Except(twoHc).Count() + twoHc.Except(oneHc).Count() > 0)
            return true;

        return false;
    }
    */

    public static bool IsDifferentString(this object one, object? two)
    {
        if (two == null) return true;

        return !JObject.FromObject(one).ToString()
            .Equals(JObject.FromObject(two).ToString());
    }

    public static bool IsDifferentExpando(ExpandoObject one, ExpandoObject? two)
    {
        if (two == null) return true;

        var obj1AsColl = (ICollection<KeyValuePair<string,object>>)one;
        var obj2AsDict = (IDictionary<string,object>)two;

        // Make sure they have the same number of properties
        if (obj1AsColl.Count != obj2AsDict.Count)
            return true;

        foreach (var pair in obj1AsColl)
        {
            // Try to get the same-named property from obj2
            object o;
            if (!obj2AsDict.TryGetValue(pair.Key, out o))
                return true;

            // Property names match, what about the values they store?
            if (!object.Equals(o, pair.Value))
                return true;
        }

        // Everything matches
        return false;
    }
}