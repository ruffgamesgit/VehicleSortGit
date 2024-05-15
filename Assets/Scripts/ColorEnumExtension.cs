using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GamePlay.Data;
using UnityEngine;

public static class ColorEnumExtension 
{
    private static System.Random Rng = new System.Random();

    public static ColorEnum GetRandomObjectType()
    {
        var valueList = GetAsList();
        valueList.Remove(0);
        ColorEnum[] values = valueList.ToArray();
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
        objectTypesList.Remove(0);
        return objectTypesList;
    }

    public static Color GetColorCode(this ColorEnum colorEnum)
    {
        switch (colorEnum)
        {
            case ColorEnum.NONE:
                return Color.clear;
            case ColorEnum.RED:
                return Color.red;
            case ColorEnum.BLUE:
                return Color.blue;
            case ColorEnum.GREEN:
                return Color.green;
            case ColorEnum.YELLOW:
                return Color.yellow;
            case ColorEnum.PURPLE:
                return new Color(0.5f, 0, 0.5f);
            case ColorEnum.ORANGE:
                return new Color(1, 0.5f, 0);
            case ColorEnum.PINK:
                return new Color(1, 0.5f, 0.5f);
            case ColorEnum.WHITE:
                return Color.white;
            case ColorEnum.BLACK:
                return Color.black;
            case ColorEnum.MAGENTA:
                return Color.magenta;
            case ColorEnum.CYAN:
                return Color.cyan;
            case ColorEnum.GREY:
                return Color.grey;
            case ColorEnum.TURQUOISE:
                return new Color(0, 0.6f, 1f);
            case ColorEnum.LIME:
                return new Color(0.6f, 0.7f, 0.5f);
            case ColorEnum.MAROON:
                return new Color(0, 1f, 0.7f);
            case ColorEnum.OLIVE:
                return new Color(0.3f, 0.7f, 0);
            default:
                throw new ArgumentOutOfRangeException(nameof(colorEnum), colorEnum, null);
        }
    }
}
