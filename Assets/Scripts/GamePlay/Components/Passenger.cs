using System;
using GamePlay.Data;
using UnityEngine;

namespace GamePlay.Components
{
    public class Passenger : MonoBehaviour
    {
        [SerializeField] private MeshRenderer meshRenderer;
        private ColorEnum color;
        public ColorEnum GetColor()
        {
            return color;
        }
        
        public void SetColor(ColorEnum passengerColor)
        {
            color = passengerColor;
            meshRenderer.material.color = passengerColor.GetColorCode();
        }
    }
}