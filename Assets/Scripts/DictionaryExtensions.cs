using System;
using System.Collections.Generic;
using System.Linq;
using GamePlay.Data; 

public static class DictionaryExtensions
{
    public static bool AreAllValuesEqual<T>(this Dictionary<ColorEnum, T> dict)
    {
        // If the dictionary is empty, all values are considered equal.
        if (dict.Count == 0)
            return true;

        // Get the first value as a reference value.
        T referenceValue = dict.First().Value;

        // Check if all other values are equal to the reference value.
        foreach (T value in dict.Values)
        {
            if (!EqualityComparer<T>.Default.Equals(value, referenceValue))
                return false;
        }

        return true;
    }
    
    public static ColorEnum GetMaxValue(this Dictionary<ColorEnum, int> dict)
    {
        // If the dictionary is empty, return the default value of T.
        if (dict.Count == 0)
            return default;

        // Initialize max value with the first value in the dictionary.
        var maxValueItem = dict.First();

        // Iterate through the dictionary to find the maximum value.
        foreach (var item in dict)
        {
            if (item.Value.CompareTo(maxValueItem.Value) > 0)
            {
                maxValueItem = item;
            }
        }
        return maxValueItem.Key;
    }

    public static ColorEnum GetMinValue(this Dictionary<ColorEnum, int> dict)
    {
        if (dict.Count == 0)
            return default;

        // Initialize max value with the first value in the dictionary.
        var maxValueItem = dict.First();

        // Iterate through the dictionary to find the maximum value.
        foreach (var item in dict)
        {
            if (item.Value.CompareTo(maxValueItem.Value) < 0)
            {
                maxValueItem = item;
            }
        }
        return maxValueItem.Key;
    }
}