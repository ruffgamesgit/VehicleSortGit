using GamePlay.Data;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay.Components
{
    public class Passenger : MonoBehaviour
    {
        [SerializeField] private List<Transform> meshTransforms;
        private ColorEnum color;
        private Dictionary<int, Vector3> offsetDictionary = new Dictionary<int, Vector3>();


        private void Start()
        {
            CalculateOffsets();
        }
        public ColorEnum GetColor()
        {
            return color;
        }

        public void SetColor(ColorEnum passengerColor)
        {
            color = passengerColor;
            for (int i = 0; i < meshTransforms.Count; i++)
            {
                meshTransforms[i].GetComponent<MeshRenderer>().material.color = passengerColor.GetColorCode();
            }
        }

        void CalculateOffsets()
        {
            for (int i = 0; i < meshTransforms.Count; i++)
            {
                Transform child = meshTransforms[i];
                offsetDictionary[i] = child.localPosition;
            }
        }

        public List<Transform> GetMeshTransforms()
        {
            return meshTransforms;
        }

        public Vector3 GetOffsetByIndex(int index)
        {
            int meshCountPerLine = meshTransforms.Count / 2;

            return offsetDictionary[index] / meshCountPerLine;
        }

        public void SetMeshesParent()
        {
            for (int i = 0; i < meshTransforms.Count; i++)
            {
                Transform meshTr = meshTransforms[i];
                meshTr.SetParent(transform);
            }
        }
    }
}