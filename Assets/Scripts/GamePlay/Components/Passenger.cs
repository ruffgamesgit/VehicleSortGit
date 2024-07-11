using System;
using GamePlay.Data;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay.Components
{
    public class Passenger : MonoBehaviour
    {
        [SerializeField] private List<Transform> meshTransforms;
        private ColorEnum _color;
        private Dictionary<int, Vector3> _offsetDictionary = new Dictionary<int, Vector3>();
        const float PLACEMENT_OFFSET_MULTIPLIER = 1.6f; // Some magic mysterious golden ratio stuff
        private MaterialPropertyBlock _propertyBlock;
        private static readonly int Color1 = Shader.PropertyToID("_Color");

        private void Awake()
        {
            _propertyBlock = new MaterialPropertyBlock();
            CalculateOffsets();
        }

        public ColorEnum GetColor()
        {
            return _color;
        }

        public void SetColor(ColorEnum passengerColor)
        {
            // _color = passengerColor;
            // foreach (var mesh in meshTransforms)
            // {
            //     mesh.GetComponent<MeshRenderer>().material.color = passengerColor.GetColorCode();
            // }

            _color = passengerColor;
            Color color = passengerColor.GetColorCode();

            foreach (Transform mesh in meshTransforms)
            {
                MeshRenderer meshRenderer = mesh.GetComponent<MeshRenderer>();
                meshRenderer.GetPropertyBlock(_propertyBlock,0);
                _propertyBlock.SetColor(Color1, color);
                meshRenderer.SetPropertyBlock(_propertyBlock,0);
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
            return _offsetDictionary[index] * PLACEMENT_OFFSET_MULTIPLIER;
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