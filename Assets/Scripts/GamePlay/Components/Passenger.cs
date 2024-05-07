using GamePlay.Data;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay.Components
{
    public class Passenger : MonoBehaviour
    {
        [SerializeField] private List<Transform> meshTransforms;
        private ColorEnum _color;
        private readonly Dictionary<int, Vector3> _offsetDictionary = new Dictionary<int, Vector3>();


        private void Start()
        {
            CalculateOffsets();
        }
        public ColorEnum GetColor()
        {
            return _color;
        }

        public void SetColor(ColorEnum passengerColor)
        {
            _color = passengerColor;
            foreach (var t in meshTransforms)
            {
                t.GetComponent<MeshRenderer>().material.color = passengerColor.GetColorCode();
            }
        }

        void CalculateOffsets()
        {
            for (int i = 0; i < meshTransforms.Count; i++)
            {
                Transform child = meshTransforms[i];
                _offsetDictionary[i] = child.localPosition;
            }
        }

        public List<Transform> GetMeshTransforms()
        {
            return meshTransforms;
        }

        public Vector3 GetOffsetByIndex(int index)
        {
            int meshCountPerLine = meshTransforms.Count / 2;

            return _offsetDictionary[index] / meshCountPerLine;
        }

        public void SetMeshesParent()
        {
            foreach (var meshTr in meshTransforms)
            {
                meshTr.SetParent(transform);
            }
        }
    }
}