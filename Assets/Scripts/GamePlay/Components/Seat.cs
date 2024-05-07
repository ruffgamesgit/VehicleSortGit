using Cysharp.Threading.Tasks;
using DG.Tweening;
using GamePlay.Data;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay.Components
{
    public class Seat : MonoBehaviour
    {
        private Passenger _passenger;
        private ColorEnum _preColor;
        private bool _isAnimationOn = false;
        public void Occupy(Passenger passenger)
        {
            _passenger = passenger;
            _passenger.transform.parent = transform;
            // LATER : Add delay
        }

        public void SetEmpty()
        {
            _passenger = null;
        }

        public bool IsEmpty()
        {
            return _passenger == null;
        }

        public Passenger GetPassenger()
        {
            return _passenger;
        }

        public ColorEnum GetPreColor()
        {
            return _preColor;
        }

        public void SetPreColor(ColorEnum color)
        {
            _preColor = color;
        }

        public void ResetPreColor()
        {
            _preColor = ColorEnum.NONE;
        }

        public void InstantiatePreColor(Passenger passengerPrefab)
        {
            if (_preColor == ColorEnum.NONE) return;
            var passenger = Instantiate(passengerPrefab, this.transform.position, Quaternion.identity);
            passenger.SetColor(_preColor);
            Occupy(passenger);
        }

        public bool IsAnimating()
        {
            return _isAnimationOn;
        }
        
        public void TakePassengerWithAnimation(UniTaskCompletionSource ucs)
        {
            if (_passenger == null)
            {
                ucs.TrySetResult();
                return;
            }
            _isAnimationOn = true;
            List<Transform> passengerMeshes = _passenger.GetMeshTransforms();
            
            for (int i = 0; i < passengerMeshes.Count; i++)
            {
                Transform meshTr = passengerMeshes[i];

                meshTr.SetParent(transform);
                Vector3 targetPos = _passenger.GetOffsetByIndex(i);
              
                int index = i;

                meshTr.transform.DOLocalJump(targetPos, 1, 1, .5f).OnComplete(() =>
                {
                    if (index == passengerMeshes.Count - 1)
                    {
                        _passenger.transform.position = transform.position;
                        _passenger.SetMeshesParent();
                        _isAnimationOn = false;
                        ucs.TrySetResult();
                    }

                }).SetEase(Ease.OutQuad).SetDelay(i * .1f);
            }
        }
    }
}