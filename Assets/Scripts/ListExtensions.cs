using System;
using System.Collections.Generic;

public static class ListExtensions
{
    private static readonly Random Rng = new();

    public static List<T> Shuffle<T>(this List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Rng.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
        return list;
    }
}