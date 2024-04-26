using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleController : MonoBehaviour
{

    [Header("Debug")]
    public bool isPicked;
    public bool IsPerformingTransfer;
    public LotController CurrentLot;

    [Header("References")]
    [SerializeField] PassengerStack stackPrefab;
    [SerializeField] List<PlacementPoint> placementPoints;
    public List<PassengerStack> CurrentPassengerStacks = new();
    [SerializeField] List<VehicleController> TargetVehiclesToMove = new();
    [SerializeField] List<ColorEnum> existingColorList = new();
    bool disappearing = false;


    public void Initiliaze(int passengerStackCount)
    {
        CurrentLot = transform.parent.GetComponent<LotController>();

        for (int i = 0; i < passengerStackCount; i++)
        {
            PlacementPoint targetPoint = GetFirstAvailablePoint();

            if (targetPoint == null) break; // If there is no placeable point that
                                            // indicates the vehicle is full

            Vector3 spawnPos;
            spawnPos = targetPoint.transform.position;
            PassengerStack cloneStack = Instantiate(stackPrefab, spawnPos, Quaternion.identity, targetPoint.transform);
            CurrentPassengerStacks.Add(cloneStack);
            cloneStack.SetCurrentVehicleAndPlacementPoint(this, targetPoint);

            targetPoint.SetOccupied(true);
        }
    }

    public void GetPicked()
    {
        isPicked = true;
        transform.DOMoveY(transform.position.y + 1, 0.25f);
    }

    public void GetReleased()
    {
        isPicked = false;
        transform.DOMoveY(transform.position.y - 1, 0.25f);
    }

    public void GoOtherLot(LotController targetLot)
    {
        if (CurrentLot)
        {
            CurrentLot.SetOccupied(false);
            CurrentLot.SetVehicle(null);
            Debug.LogWarning("CHANGE THE LOGIC, SHOULD NOT MODIFY DIRECTLY ANOTHER CLASSES' ATTRIBUTES");
        }


        CurrentLot = targetLot;
        CurrentLot.SetVehicle(this);
        transform.DOMove(targetLot.GetCenter(), .5f).OnComplete(() =>
        {
            GetReleased();
            StartCoroutine(ControlTransfer(.1f));
        });
    }

    #region Passenger Transfer Region

    IEnumerator ControlTransfer(float startControlDelay)
    {
        Debug.LogWarning("CONTROL ROUTINE INVOKED");


        if (disappearing) yield break;


        if (CurrentPassengerStacks.Count > 0)
        {
            yield return new WaitForSeconds(startControlDelay);

            if (IsVehicleSortedFully())
            {
                disappearing = true;
                float tweenDuration = .5f;
                yield return new WaitForSeconds(startControlDelay);

                Debug.LogWarning("This vehicle fully sorted: " + gameObject.name);
                CurrentLot.SetOccupied(false);
                transform.DOScale(Vector3.zero, tweenDuration);
                CurrentPassengerStacks.Clear(); // bir daha routine başlamaması için 

                //Wait Tween Completetion
                yield return new WaitForSeconds(tweenDuration);

                CurrentLot.SetOccupied(false);
                CurrentLot.SetVehicle(null);
            }
            else
            {
                List<LotController> neighborLots = CurrentLot.GetLotNeighbors();
                List<VehicleController> targetVehiclesToMove = new List<VehicleController>();

                for (int i = 0; i < neighborLots.Count; i++)
                {
                    // Komşu lotları üzerilerinde vehicle var mı diye kontrol ediyorum, vehicle var ise passenger stack sayısı 0 dan fazla mı ???
                    if (neighborLots[i].GetVehicle() != null && neighborLots[i].GetVehicle().GetPassengerStacks().Count > 0)
                    {
                        VehicleController targetVehicle = neighborLots[i].GetVehicle();

                        // Uygun vehicle bulunduktan sonra kendi üzerimdeki renkleri target vehicle üzerindeki renkler ile karşılaştırıyorum
                        // Eğer aynı renk var ise komşu target vehicle da hareket edilebilecek vehicle listeme ekliyorum
                        for (int a = 0; a < GetExistingColors().Count; a++)
                        {
                            ColorEnum myColor = GetExistingColors()[a];
                            for (int h = 0; h < targetVehicle.GetExistingColors().Count; h++)
                            {
                                ColorEnum targetColor = targetVehicle.GetExistingColors()[h];
                                if (myColor == targetColor && !TargetVehiclesToMove.Contains(targetVehicle) && targetVehicle != this)
                                    TargetVehiclesToMove.Add(targetVehicle);
                            }
                        }

                    }
                    //     TargetVehiclesToMove.Shuffle();

                    // EĞER KOMŞU VEHICLE LARDA UYUŞAN RENK VAR İSE
                    if (TargetVehiclesToMove.Count > 0)
                    {
                        #region Perform Transfer
                        for (int n = 0; n < TargetVehiclesToMove.Count; n++)
                        {
                            //TargetVehiclesToMove.AddRange(targetVehiclesToMove);
                            VehicleController targetVehicleToTranfer = TargetVehiclesToMove[n];
                            List<PassengerStack> stacksToTakeList = new List<PassengerStack>();
                            List<PassengerStack> stacksToSendList = new List<PassengerStack>();
                            List<PlacementPoint> availablePlacementPoints = new List<PlacementPoint>();
                            bool hasMajority = false;

                            // Eğer passenger stacklerimin içerisinde renk olarak çoğunlukla bir renk var ise
                            // ilk önce komşularımdan o rengi talep etmeliyim
                            ColorEnum myColorToControl;
                            int majorityStackNumber; // Bu değer çoğunluğa sahip bir renk varsa o renge sahip stack sayısını temsil ediyor
                            int demandingStackCountFromNeighbor; // Bu değer ise vehicle in tamamen sort olması için komşudan istenecek stack sayısı
                                                                 // tabi ki majority renge göre stack sayısı 

                            if (HasMajorityOfOneColor(out myColorToControl, out majorityStackNumber))
                            {
                                // Bir rengim çoğunluğa sahip ve komşumda bu renkten var mı diye kontrol ediyorum
                                if (targetVehicleToTranfer.GetExistingColors().Contains(myColorToControl))
                                {
                                    demandingStackCountFromNeighbor = 4 - majorityStackNumber;
                                    Debug.LogWarning("Has majority of one color: " + myColorToControl + ("\n") +
                                        ", and the stacks of that color: " + majorityStackNumber + ("\n") +
                                         ", demanding stack count of that color from neighbor:  " + demandingStackCountFromNeighbor);

                                    // Komşu vehicle içerisinde verdiğim color ile uyuşan passenger stacklerin listesini alıyorum
                                    List<PassengerStack> stacksWithMatchedColors = targetVehicleToTranfer.GetPassengerStacksBySpecificColor(myColorToControl);

                                    // Tamamen sorted olmam için gereken sayıda passenger stack eğer komşumda var ise direkt alıyorum
                                    for (int s = 0; s < stacksWithMatchedColors.Count; s++)
                                    {
                                        if (demandingStackCountFromNeighbor == 0) break; // Yeteri kadar stack aldıktan sonra loop kırılmalı
                                        if (!stacksToTakeList.Contains(stacksWithMatchedColors[s]))
                                        {
                                            stacksToTakeList.Add(stacksWithMatchedColors[s]);
                                            demandingStackCountFromNeighbor--;
                                        }

                                    }

                                    // Transfer işlemi sonucunda aldığım ve verdiğim stack sayısı eşit olmalı 
                                    int stackToSendCount = stacksToTakeList.Count;

                                    // Burada gönderilecek stackleri belirlemeden önce koyabileceğim boş placement point var mı diye kontrol etmeliyim
                                    // Çünkü göndermeme gerek kalmayabilir öncelikle boş pointler doldurulmalı
                                    if (GetAllAvailablePoints().Count != 0)
                                    {
                                        // Ne kadar available point var ise listeye ekliyorum ilk önce boş pointleri doldurmalıyım

                                        availablePlacementPoints.AddRange(GetAllAvailablePoints());
                                    }

                                    if (stackToSendCount > availablePlacementPoints.Count)
                                    {
                                        // Yeteri kadar boş point yok, aldığım stack sayısı kadarını göndermem lazım
                                        // ve göndereceklerimin placement pointlerini listeye eklemeliyim  

                                        stackToSendCount -= availablePlacementPoints.Count; // istenilen kadar olmasa da boş point olabilir o sayıya
                                                                                            // göre göndereceklerimin sayısını bilmem gerekli


                                        // Göndereceğim passenger stacklerin sayısını ve hangi color dan olmaması gerektiğini biliyorum
                                        //  onları bir listeye alıyorum
                                        for (int k = 0; k < CurrentPassengerStacks.Count; k++)
                                        {
                                            if (stackToSendCount == 0) break;
                                            if (CurrentPassengerStacks[k].stackColor != myColorToControl && !stacksToSendList.Contains(CurrentPassengerStacks[k]))
                                            {
                                                stacksToSendList.Add(CurrentPassengerStacks[k]);
                                                availablePlacementPoints.Add(stacksToSendList[k].GetCurrentPoint());
                                                stackToSendCount--;
                                            }

                                        }
                                    }

                                }
                                else
                                {
                                    // Bendeki çoğunluğa sahip rengim komşumda yok, diğer azınlık renkleri kontrol ediyorum
                                    //


                                }

                            }
                            else
                            {
                                List<ColorEnum> matchedColorList = new List<ColorEnum>();
                                for (int c = 0; c < GetExistingColors().Count; c++)
                                {
                                    ColorEnum myColor = GetExistingColors()[c];
                                    // Herhangi bir renk çoğunluğa sahip değil renk listemden komşularımın rengiyle uyuşanları kontrol ediyorum 
                                    if (targetVehicleToTranfer.GetExistingColors().Contains(myColor))
                                    {
                                        matchedColorList.Add(myColor);
                                    }

                                }

                                // Komşumda uyuşan rengim var
                                if (matchedColorList.Count > 0)
                                {
                                    matchedColorList.Shuffle();
                                    myColorToControl = matchedColorList[0];
                                    // Komşu vehicle içerisinde verdiğim color ile uyuşan passenger stacklerin listesini alıyorum
                                    List<PassengerStack> stacksWithMatchedColors = targetVehicleToTranfer.GetPassengerStacksBySpecificColor(myColorToControl);

                                    int neighborStackCountWithSpecificColor = targetVehicleToTranfer.GetPassengerStacksBySpecificColor(myColorToControl).Count;
                                    int myStackCountWithSpecificColor = GetPassengerStacksBySpecificColor(myColorToControl).Count;


                                    // Seçilen renk komşumda daha fazla var ise gönderilecekler listeme ekliyorum
                                    if (neighborStackCountWithSpecificColor > myStackCountWithSpecificColor)
                                    {
                                        stacksToSendList.AddRange(GetPassengerStacksBySpecificColor(myColorToControl));
                                    }
                                    else
                                    {
                                        // Eğer bende istenilen renk sayısı komşumdakiyle eşit veya fazlaysa alınacaklar listeme ekliyorum 
                                        stacksToTakeList.AddRange(targetVehicleToTranfer.GetPassengerStacksBySpecificColor(myColorToControl));
                                    }

                                    ///////////////////////////////////////////////////////////////////////////////////////////////////////////

                                    // Transfer işlemi sonucunda aldığım ve verdiğim stack sayısı eşit olmalı 
                                    int stackToSendCount = stacksToTakeList.Count;
                                    int takeableStackCount = stacksToTakeList.Count;


                                    // Burada gönderilecek stackleri belirlemeden önce koyabileceğim boş placement point var mı diye kontrol etmeliyim
                                    // Çünkü göndermeme gerek kalmayabilir öncelikle boş pointler doldurulmalı
                                    if (GetAllAvailablePoints().Count != 0)
                                    {
                                        for (int p = 0; p < GetAllAvailablePoints().Count; p++)
                                        {
                                            // Ne kadar available point var ise listeye ekliyorum ilk önce bkoş pointleri doldurmalıyım
                                            availablePlacementPoints.Add(GetAllAvailablePoints()[p]);

                                        }

                                    }


                                    // Boş point im var gönderme işlemi yapılmadan öncelikle boşları dolduruyorum
                                    if (availablePlacementPoints.Count > 0)
                                    {
                                        // Boş point sayım ile alabileceğim stack sayısını eşitliyorum
                                        if (availablePlacementPoints.Count < stacksToTakeList.Count)
                                        {
                                            int stackToRemoveCount = stacksToTakeList.Count - availablePlacementPoints.Count;

                                            for (int z = 0; z < stackToRemoveCount; z++)
                                            {
                                                stacksToTakeList.RemoveAt(stacksToTakeList.Count - 1);
                                            }
                                        }

                                        Debug.Log("Have empty placement point, no need to send stack in first hand");
                                    }
                                    else
                                    {
                                        // Boş point yok göndermeden stack alamam

                                        //if (stackToSendCount > availablePlacementPoints.Count)
                                        //{
                                        //    // Yeteri kadar boş point yok, aldığım stack sayısı kadarını göndermem lazım
                                        //    // ve göndereceklerimin placement pointlerini listeye eklemeliyim  

                                        //    stackToSendCount -= availablePlacementPoints.Count; // istenilen kadar olmasa da boş point olabilir o sayıya
                                        //                                                        // göre göndereceklerimin sayısını bilmem gerekli


                                        //    // Göndereceğim passenger stacklerin sayısını ve hangi color dan olmaması gerektiğini biliyorum
                                        //    //  onları bir listeye alıyorum
                                        //    for (int k = 0; k < CurrentPassengerStacks.Count; k++)
                                        //    {
                                        //        if (stackToSendCount == 0) break;
                                        //        if (CurrentPassengerStacks[k].stackColor != myColorToControl &&
                                        //            !stacksToSendList.Contains(CurrentPassengerStacks[k]))
                                        //        {
                                        //            stacksToSendList.Add(CurrentPassengerStacks[k]);
                                        //            availablePlacementPoints.Add(stacksToSendList[k].GetCurrentPoint());
                                        //            stackToSendCount--;
                                        //        }

                                        //    }
                                        //}
                                    }


                                }
                                else
                                {

                                    Debug.LogWarning("Has NO matched colors");

                                }
                            }

                            //  PERFORM TRANSFERRING BY THE DATA COLLECTED 
                            if (stacksToTakeList.Count == 0)
                            {
                                Debug.LogWarning("No stack to TAKE is found");
                            }
                            if (stacksToSendList.Count == 0)
                            {
                                Debug.LogWarning("No stack to SEND is found");
                            }
                            //

                            // İlk önce alma işlemini gerçekleştiriyorum
                            for (int t = 0; t < stacksToTakeList.Count; t++)
                            {
                                PassengerStack stack = stacksToTakeList[t];
                                // Kendi üzerimden göndereceğim stacklerin placement pointlerini alacaklarıma atıyorum
                                PlacementPoint placementPoint = availablePlacementPoints[t];

                                availablePlacementPoints.Add(stack.GetCurrentPoint());
                                availablePlacementPoints[t].SetOccupied(true);
                                stack.GoOtherVehicle(this, placementPoint);
                            }

                            for (int ss = 0; ss < stacksToSendList.Count; ss++)
                            {
                                PassengerStack stackToSend = stacksToSendList[ss];
                                PlacementPoint placementPoint = targetVehicleToTranfer.GetAllAvailablePoints()[ss];

                                stackToSend.GoOtherVehicle(targetVehicleToTranfer, placementPoint);

                            }



                            if (IsVehicleSortedFully())
                            {
                                disappearing = true;
                                float tweenDuration = .25f;
                                yield return new WaitForSeconds(startControlDelay);

                                Debug.LogWarning("This vehicle fully sorted: " + gameObject.name);
                                CurrentLot.SetOccupied(false);
                                transform.DOScale(Vector3.zero, tweenDuration);
                                CurrentPassengerStacks.Clear(); // bir daha routine başlamaması için 

                                //Wait Tween Completetion
                                yield return new WaitForSeconds(tweenDuration);

                                CurrentLot.SetOccupied(false);
                                CurrentLot.SetVehicle(null);
                            }

                        }

                        #endregion

                        //TRANSFER OLAYI GERÇEKLEŞTİ TEKRARDAN KONTROL BAŞLATILMALI
                        //StartCoroutine(ControlTransfer(0.1f));
                        //for (int nn = 0; nn < TargetVehiclesToMove.Count; nn++)
                        //{
                        //    VehicleController controller = TargetVehiclesToMove[nn];
                        //    if (!controller.IsPerformingTransfer)
                        //        TargetVehiclesToMove[nn].StartCoroutine(ControlTransfer(.1f));
                        //}
                    }

                    else
                        IsPerformingTransfer = false;
                }
            }

        }

        TargetVehiclesToMove.Clear();
        yield return null;

    }

    public bool IsVehicleSortedFully()
    {
        int sameColoredStackCount = 0;
        if (CurrentPassengerStacks.Count == 4)
        {
            ColorEnum firstColor = CurrentPassengerStacks[0].stackColor;
            for (int i = 0; i < CurrentPassengerStacks.Count; i++)
            {
                if (firstColor == CurrentPassengerStacks[i].stackColor)
                {
                    sameColoredStackCount++;
                }
            }

        }
        return sameColoredStackCount == 4;
    }

    public bool HasMajorityOfOneColor(out ColorEnum colorEnum, out int countOfTheStack)
    {
        Dictionary<ColorEnum, int> colorCounts = new Dictionary<ColorEnum, int>();

        if (CurrentPassengerStacks.Count == 1)
        {
            colorEnum = CurrentPassengerStacks[0].stackColor;
            countOfTheStack = 1;
            return true;
        }
        else
        {
            // Count occurrences of each color
            foreach (PassengerStack stack in CurrentPassengerStacks)
            {
                ColorEnum color = stack.stackColor;
                if (colorCounts.ContainsKey(color))
                {
                    colorCounts[color]++;
                }
                else
                {
                    colorCounts[color] = 1;
                }
            }

            // Find the color with the highest count
            int maxCount = 0;
            colorEnum = ColorEnum.NONE;

            foreach (KeyValuePair<ColorEnum, int> pair in colorCounts)
            {
                if (pair.Value > maxCount)
                {
                    maxCount = pair.Value;
                    colorEnum = pair.Key;
                }
            }

            // Check if the majority is at least countOfTheStack
            if (maxCount >= 2)
            {
                countOfTheStack = maxCount;
                return true;
            }
            else
            {
                colorEnum = ColorEnum.NONE;
                countOfTheStack = 0;
                return false;
            }
        }
    }

    public List<PassengerStack> GetPassengerStacksBySpecificColor(ColorEnum _color)
    {
        List<PassengerStack> stacks = new List<PassengerStack>();


        for (int i = 0; i < CurrentPassengerStacks.Count; i++)
        {
            if (CurrentPassengerStacks[i].stackColor == _color && !stacks.Contains(CurrentPassengerStacks[i]))
                stacks.Add(CurrentPassengerStacks[i]);
        }

        return stacks;
    }
    #endregion

    public void AddStack(PassengerStack stack)
    {
        if (CurrentPassengerStacks.Contains(stack)) return;

        CurrentPassengerStacks.Add(stack);
    }

    public void RemoveStack(PassengerStack stack)
    {
        if (!CurrentPassengerStacks.Contains(stack)) { return; }

        CurrentPassengerStacks.Remove(stack);
    }

    public int GetAvailablePointCount()
    {
        int num = 0;
        for (int i = 0; i < placementPoints.Count; i++)
        {
            if (!placementPoints[i].IsOccupied)
                num++;
        }

        return num;
    }

    List<PlacementPoint> GetAllAvailablePoints()
    {
        List<PlacementPoint> points = new List<PlacementPoint>();

        for (int i = 0; i < placementPoints.Count; i++)
        {
            if (!placementPoints[i].IsOccupied)
                points.Add(placementPoints[i]);
        }

        return points;
    }
    PlacementPoint GetFirstAvailablePoint()
    {
        for (int i = 0; i < placementPoints.Count; i++)
        {
            PlacementPoint point = placementPoints[i];

            if (!point.IsOccupied)
                return point;
            else
            {
                if (i == placementPoints.Count - 1)
                    break;
            }
        }

        return null;
    }
    public List<ColorEnum> GetExistingColors()
    {

        return existingColorList;

    }
    public List<PassengerStack> GetPassengerStacks()
    {
        return CurrentPassengerStacks;
    }
    public void AddExistingStackColors(ColorEnum _color)
    {
        if (!existingColorList.Contains(_color))
            existingColorList.Add(_color);
    }

    public void UpdateExistingColorList()
    {
        existingColorList.Clear();
        for (int i = 0; i < CurrentPassengerStacks.Count; i++)
        {
            ColorEnum stackColor = CurrentPassengerStacks[i].stackColor;

            if (!existingColorList.Contains(stackColor))
                existingColorList.Add(stackColor);
        }
    }
}
