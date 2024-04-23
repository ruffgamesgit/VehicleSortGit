using System.Collections.Generic;
using UnityEngine;

public enum ColorEnum
{
    RED, BLUE, GREEN, YELLOW
}

public class PassengerStack : MonoBehaviour
{
    [Header("Debug")]
    public ColorEnum stackColor;
    [SerializeField] List<Passenger> passengers = new();

    public void Initialize(ColorEnum _color)
    {
        stackColor = _color;


        for (int i = 0; i < transform.childCount; i++)
        {
            Passenger pass = transform.GetChild(i).GetComponent<Passenger>();
            if (pass != null)
            {
                passengers.Add(pass);
                pass.SetColorEnumAndMat(stackColor);
            }
        }
    }
}


[System.Serializable]
public class PaassengerStackInfo
{
    public ColorEnum Color;
}
