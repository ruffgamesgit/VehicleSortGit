using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ColorEnumExtension 
{
    private static System.Random Rng = new System.Random();

    public static ColorEnum GetRandomObjectType()
    {
        ColorEnum[] values = GetAsList().ToArray();
        var randomValue = Rng.Next(values.Length - 1);
        var randomObjectType = values[randomValue];
        return randomObjectType;
    }

    public static ColorEnum GetRandomObjectType(this List<ColorEnum> objectTypes)
    {
        if (objectTypes.Count == 0)
        {
            objectTypes =
                Enum.GetValues(typeof(ColorEnum)).Cast<ColorEnum>().ToList();
        }
        var randomValue = Rng.Next(objectTypes.Count);
        var randomObjectType = objectTypes[randomValue];
        return randomObjectType;
    }

    public static List<ColorEnum> GetAsList()
    {
        List<ColorEnum> objectTypesList =
            Enum.GetValues(typeof(ColorEnum)).Cast<ColorEnum>().ToList();
        return objectTypesList;
    }
}
