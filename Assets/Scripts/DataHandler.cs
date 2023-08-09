using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

#region 自定义比较器
public class Vector3EqualityComparer : IEqualityComparer<Vector3>
{
    public bool Equals(Vector3 v1, Vector3 v2)
    {
        int x1 = Mathf.RoundToInt(v1.x), x2 = Mathf.RoundToInt(v2.x);
        int y1 = Mathf.RoundToInt(v1.y), y2 = Mathf.RoundToInt(v2.y);
        int z1 = Mathf.RoundToInt(v1.z), z2 = Mathf.RoundToInt(v1.z);
        return x1 == x2 && y1 == y2 && z1 == z2;
    }

    public int GetHashCode(Vector3 v)
    {
        int x = Mathf.RoundToInt(v.x);
        int y = Mathf.RoundToInt(v.y);
        int z = Mathf.RoundToInt(v.z);

        int hashX = x.GetHashCode();
        int hashY = y.GetHashCode();
        int hashZ = z.GetHashCode();

        return hashX ^ hashY ^ hashZ;
    }
}
#endregion
public class DataHandler
{
    public static int HandleStringWithInteger(String text)
    {
        int res;
        string pattern = @"\d+";
        Match match = Regex.Match(text, pattern);
        res = Convert.ToInt32(match.Value);
        return res;
    }
}