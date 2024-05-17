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
        private bool _isAnimationOn;
        private Sequence _sequence;
        private const float TWEEN_DURATION = .3f;

        public void Occupy(Passenger passenger)
        {
            _passenger = passenger;
            _passenger.transform.parent = transform;
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

        public void TakePassengerWithAnimation(UniTaskCompletionSource ucs, bool instant)
        {
            if (_passenger == null)
            {
                ucs.TrySetResult();
                return;
            }

            _sequence?.Kill(true);
            _isAnimationOn = true;
            List<Transform> passengerMeshes = _passenger.GetMeshTransforms();
            
            _sequence = DOTween.Sequence();
            for (int i = 0; i < passengerMeshes.Count; i++)
            {
                Transform meshTr = passengerMeshes[i];
                meshTr.SetParent(transform);
                Vector3 targetPos = _passenger.GetOffsetByIndex(i);
                _sequence.Insert(i * 0.1f,
                    meshTr.transform.DOLocalJump(targetPos, 1, 1, TWEEN_DURATION).SetEase(Ease.OutQuad));
            }
            
            _sequence.OnComplete(() =>
            {
                if (_passenger != null)
                {
                    _passenger.transform.position = transform.position;
                    _passenger.SetMeshesParent();
                }
                _isAnimationOn = false;
                ucs.TrySetResult();
            });

            if (instant)
                _sequence.Complete();
        }

        public void RotatePassengers()
        {
            if(_passenger == null) return;
            List<Transform> passengerMeshes = _passenger.GetMeshTransforms();
            foreach (var meshTr in passengerMeshes)
            {
                meshTr.transform.DORotate(new Vector3(0, 180, 0), 0, RotateMode.LocalAxisAdd)
                    .SetEase(Ease.OutQuad).SetDelay(passengerMeshes.Count * 0.05f);
            }
        }
    }
}