using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using GamePlay.Components.SortController;
using GamePlay.Data;
using UnityEngine;

namespace GamePlay.Components
{
    public class Vehicle : MonoBehaviour
    {
        [SerializeField] private List<Seat> seats = new List<Seat>(4);
        [SerializeField] private Outline outline;
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
            outline.HandleOutline(active ? 2.7f : 0f);
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
        
        public async UniTask SortByType(bool instant)
        {
            if (IsAllEmpty()) return;
            HashSet<Seat> swappingAnimationList = new HashSet<Seat>();

            if (HasEmptySeat())
            {
                var emptySeats = seats.FindAll(seat => seat.IsEmpty());
                for (int i = seats.Count - 1; i > seats.Count - emptySeats.Count - 1; i--)
                {
                    if (!seats[i].IsEmpty())
                    {
                        var swappingTarget =  seats.First(s => s.IsEmpty());
                        seats[i].Swap(swappingTarget);
                        swappingAnimationList.Add(seats[i]);  
                        swappingAnimationList.Add(swappingTarget);
                    }
                }
            }
            var colorCount = GetExistingColors();
            if(colorCount.Count == 1) goto finalize;
            iterate:
            if (colorCount.AreAllValuesEqual())
            {
                if (colorCount.Values.ToArray()[0] == 1)
                {
                    goto finalize;
                }
                else
                {
                    if (seats[0].GetPassenger().GetColor() == seats[1].GetPassenger().GetColor())
                    {
                        goto finalize;
                    }
                }
               
            }
            var highestValueColor = colorCount.GetMaxValue();
            var highestValueCount = colorCount[highestValueColor];
            colorCount.Remove(highestValueColor);

            for (int i = 0; i < highestValueCount; i++)
            {
                if (seats[i].GetPassenger().GetColor() != highestValueColor)
                {
                    var swappingTarget = seats.Last(s => s.GetPassenger() != null && s.GetPassenger().GetColor() == highestValueColor);
                    if(swappingTarget == seats[i])continue;
                    swappingAnimationList.Add(seats[i]);
                    swappingAnimationList.Add(swappingTarget);
                    seats[i].Swap(swappingTarget);
                }
            }
            
            if(colorCount.Count > 0)
                goto iterate;
            
            finalize:
            
            if (swappingAnimationList.Count > 0)
            {
                await swappingAnimationList.ToList().AnimateSeatChanges(instant);
            }

        }

        public void Destroy()
        {
            Destroy(this.gameObject);
        }
    }
}