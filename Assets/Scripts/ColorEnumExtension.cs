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
                ColorUtility.TryParseHtmlString("#E93B32", out var color0);
                return color0;
            case ColorEnum.BLUE:
                ColorUtility.TryParseHtmlString("#06B8FF", out var color1);
                return color1;
            case ColorEnum.GREEN:  
                ColorUtility.TryParseHtmlString("#4EC030", out var color2);
                return color2;
            case ColorEnum.YELLOW:
                ColorUtility.TryParseHtmlString("#F3DE3F", out var color3);
                return color3;
            case ColorEnum.PURPLE:
                ColorUtility.TryParseHtmlString("#A945FF", out var color4);
                return color4;
            case ColorEnum.ORANGE:
                ColorUtility.TryParseHtmlString("#F3943F", out var color5);
                return color5;
            case ColorEnum.PINK:
                ColorUtility.TryParseHtmlString("#F36B7B", out var color6);
                return color6;
            case ColorEnum.WHITE:
                ColorUtility.TryParseHtmlString("#F8D9DD", out var color7);
                return color7;
            case ColorEnum.BLACK:
                ColorUtility.TryParseHtmlString("#555555", out var color8);
                return color8;
            case ColorEnum.MILITARY_GREEN:
                ColorUtility.TryParseHtmlString("#617150", out var color9);
                return color9;
            case ColorEnum.DARK_BLUE:
                ColorUtility.TryParseHtmlString("#3F74F3", out var color10);
                return color10;
            case ColorEnum.GREY:
                ColorUtility.TryParseHtmlString("#898989", out var color11);
                return color11;
            case ColorEnum.TURQUOISE:
                ColorUtility.TryParseHtmlString("#81FABF", out var color12);
                return color12;
            case ColorEnum.PEACH:
                ColorUtility.TryParseHtmlString("#FFB37A", out var color13);
                return color13; 
            case ColorEnum.NAVY_BLUE:
                ColorUtility.TryParseHtmlString("#433FF3", out var color14);
                return color14;
            case ColorEnum.MAGENTA:
                ColorUtility.TryParseHtmlString("#DF3FF3", out var color15);
                return color15;
            case ColorEnum.CLARET_RED:
                ColorUtility.TryParseHtmlString("#A62D1D", out var color16);
                return color16;
            case ColorEnum.DARK_GREEN:
                ColorUtility.TryParseHtmlString("#0E6339", out var color17);
                return color17;
            case ColorEnum.SHADOWY_PURPLE:
                ColorUtility.TryParseHtmlString("#330476", out var color18);
                return color18;
            case ColorEnum.BROWNISH_RED:
                ColorUtility.TryParseHtmlString("#6F150A", out var color19);
                return color19;
            default:
                throw new ArgumentOutOfRangeException(nameof(colorEnum), colorEnum, null);
        }
    }
}
