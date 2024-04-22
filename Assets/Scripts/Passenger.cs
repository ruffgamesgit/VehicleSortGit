using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Passenger : MonoBehaviour
{
    public ColorEnum passengerColor;
    [SerializeField] List<Material> materials = new List<Material>();

    public void SetColorEnumAndMat(ColorEnum color)
    {
        passengerColor = color;
        SetMat();
    }


    void SetMat()
    {
        int colorIndex = (int)passengerColor;
        if (colorIndex >= 0 && colorIndex < materials.Count)
        {
            GetComponent<Renderer>().material = materials[colorIndex];
        }
    }

}
