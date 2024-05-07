using System.Collections.Generic;
using GamePlay.Data;
using UnityEngine;

namespace GamePlay.Components
{
    public class Vehicle : MonoBehaviour
    {
        [SerializeField] private List<Seat> seats = new List<Seat>(4);
        [SerializeField] private GameObject highLight;
        public Dictionary<ColorEnum,int> GetExistingColors()
        {
            var colorCount = new Dictionary<ColorEnum, int>();
            foreach (var seat in seats)
            {
                if (seat.IsEmpty()) continue;
                var color = seat.GetPassenger().GetColor();
                if (!colorCount.TryAdd(color, 1))
                {
                    colorCount[color]++;
                }
            }

            return colorCount;
        }
        
        public void SetHighlight(bool active)
        {
            highLight.SetActive(active);
        }
        
        public List<Seat> GetSeats()
        {
            return seats;
        }

        public bool IsAllEmpty()
        {
            return seats.FindAll(s => s.IsEmpty()).Count == 4;
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
        
        public bool IsAnimationOn()
        {
            foreach (var seat in seats)
            {
                if (seat.IsAnimating())
                    return true;
            }
            return false;
        }
        
        public void SortByType()
        {
            
        }

        public void Destroy()
        {
            GameManager.instance.OnVehicleDisappears();
            Destroy(this.gameObject);
        }
    }
}