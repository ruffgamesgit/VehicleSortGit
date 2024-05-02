using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Passenger : MonoBehaviour
{
    public ColorEnum passengerColor = ColorEnum.GREEN;
    [SerializeField] private List<Material> materials = new List<Material>();
    [SerializeField] private MeshRenderer meshRenderer;

    public void SetColorEnumAndMat(ColorEnum color)
    {
        passengerColor = color;
        SetMat();
    }


    void SetMat()
    {
        int colorIndex = (int)passengerColor ;
        if (colorIndex >= 0 && colorIndex < materials.Count)
        {
            meshRenderer.material = materials[colorIndex];
        }
    }

    private void OnValidate()
    {
        SetMat();
    }
}
