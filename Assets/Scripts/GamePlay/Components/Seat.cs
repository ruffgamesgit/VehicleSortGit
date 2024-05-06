using Cysharp.Threading.Tasks;
using DG.Tweening;
using GamePlay.Data;
using UnityEngine;

namespace GamePlay.Components
{
    public class Seat : MonoBehaviour
    {
        private Passenger _passenger;
        private ColorEnum _preColor;
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
            if(_preColor == ColorEnum.NONE) return;
            var passenger = Instantiate(passengerPrefab, this.transform.position, Quaternion.identity);
            passenger.SetColor(_preColor); 
            Occupy(passenger);
        }

        public void TakePassengerWithAnimation(UniTaskCompletionSource ucs)
        {
            if (_passenger == null)
            {
                ucs.TrySetResult();
                return;
            }
            _passenger.transform.DOMove(transform.position, 0.75f).OnComplete(() =>
            {
                ucs.TrySetResult();
            }).SetEase(Ease.OutQuad);
        }
        
        
    }
}