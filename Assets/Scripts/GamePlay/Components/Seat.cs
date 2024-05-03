using Cysharp.Threading.Tasks;
using GamePlay.Data;
using UnityEngine;

namespace GamePlay.Components
{
    public class Seat : MonoBehaviour
    {
        private Passenger _passenger;
        private ColorEnum _preColor;
        public void Occupy(Passenger passenger, bool withAnimation ,UniTaskCompletionSource ucs)
        {
            _passenger = passenger;
            _passenger.transform.parent = this.transform;
            if (!withAnimation)
            {
                passenger.transform.position = this.transform.position;
                ucs?.TrySetResult();
            }
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
            Occupy(passenger,false,null);
        }
        
        
    }
}