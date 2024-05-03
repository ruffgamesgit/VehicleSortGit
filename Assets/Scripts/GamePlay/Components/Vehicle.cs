using System.Collections.Generic;
using GamePlay.Data;
using UnityEngine;

namespace GamePlay.Components
{
    public class Vehicle : MonoBehaviour
    {
        [SerializeField] private List<Seat> seats = new List<Seat>(4);

        public List<ColorEnum> GetExistingColors()
        {
            List<ColorEnum> colors = new List<ColorEnum>();
            foreach (var seat in seats)
            {
                if (!seat.IsEmpty())
                    colors.Add(seat.GetPassenger().GetColor());
            }
            return colors;
        }

        public List<Seat> GetSeats()
        {
            return seats;
        }
        
        public bool HasEmptySeat()
        {
            foreach (var seat in seats)
            {
                if (seat.IsEmpty())
                    return true;
            }
            return false;
        }

        public void SortByType()
        {
            
        }

        public void Destroy()
        {
            Destroy(this.gameObject);
        }
    }
}