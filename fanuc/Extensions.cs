// ReSharper disable once CheckNamespace

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
}