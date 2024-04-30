using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SortManager : MonoSingleton<SortManager>
{
    [SerializeField] List<LotController> affectedLots = new List<LotController>();
    public void Sort(LotController lot, float startControlDelay)
    {
        StartCoroutine(SortingRoutine());

        IEnumerator SortingRoutine()
        {
            VehicleController currentVehicle = lot.GetVehicle();

            if (currentVehicle != null && currentVehicle.GetPassengerStacks().Count > 0)
            {
                yield return new WaitForSeconds(startControlDelay);


                if (currentVehicle.IsVehicleSortedFully())
                {
                    float tweenDuration = .5f;
                    yield return new WaitForSeconds(startControlDelay);

                    lot.SetOccupied(false);
                    currentVehicle.Disappear(tweenDuration);

                    yield return new WaitForSeconds(tweenDuration);

                    lot.SetOccupied(false);
                    lot.SetVehicle(null);
                }
                else
                {
                    List<LotController> neighborLots = lot.GetLotNeighbors();
                    List<VehicleController> targetVehiclesToMove = new List<VehicleController>();


                    for (int i = 0; i < neighborLots.Count; i++)
                    {
                        LotController neighborLot = neighborLots[i];
                        // Komşu lotları üzerilerinde vehicle var mı diye kontrol ediyorum, vehicle var ise passenger stack sayısı 0 dan fazla mı ???
                        if (neighborLot.GetVehicle() != null && neighborLot.GetVehicle().GetPassengerStacks().Count > 0)
                        {
                            VehicleController targetVehicle = neighborLots[i].GetVehicle();

                            // Uygun vehicle bulunduktan sonra kendi üzerimdeki renkleri target vehicle üzerindeki renkler ile karşılaştırıyorum
                            // Eğer aynı renk var ise komşu target vehicle da hareket edilebilecek vehicle listeme ekliyorum
                            for (int a = 0; a < currentVehicle.GetExistingColors().Count; a++)
                            {
                                ColorEnum myColor = currentVehicle.GetExistingColors()[a];
                                for (int h = 0; h < targetVehicle.GetExistingColors().Count; h++)
                                {
                                    ColorEnum targetColor = targetVehicle.GetExistingColors()[h];
                                    if (myColor == targetColor && !targetVehiclesToMove.Contains(targetVehicle) && targetVehicle != this)
                                        targetVehiclesToMove.Add(targetVehicle);
                                }
                            }

                        }
                        //     TargetVehiclesToMove.Shuffle();
                    }

                    if (targetVehiclesToMove.Count > 0)
                    {
                        #region Perform Transfer
                        for (int n = 0; n < targetVehiclesToMove.Count; n++)
                        {
                            //TargetVehiclesToMove.AddRange(targetVehiclesToMove);
                            VehicleController targetVehicleToTranfer = targetVehiclesToMove[n];
                            List<PassengerStack> stacksToTakeList = new List<PassengerStack>();
                            List<PassengerStack> stacksToSendList = new List<PassengerStack>();
                            List<PlacementPoint> availablePlacementPoints = new List<PlacementPoint>();

                            // Eğer passenger stacklerimin içerisinde renk olarak çoğunlukla bir renk var ise
                            // ilk önce komşularımdan o rengi talep etmeliyim
                            ColorEnum myColorToControl;
                            int majorityStackNumber; // Bu değer çoğunluğa sahip bir renk varsa o renge sahip stack sayısını temsil ediyor
                            int demandingStackCountFromNeighbor; // Bu değer ise vehicle in tamamen sort olması için komşudan istenecek stack sayısı
                                                                 // tabi ki majority renge göre stack sayısı 

                            if (currentVehicle.HasMajorityOfOneColor(out myColorToControl, out majorityStackNumber))
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
                                    if (currentVehicle.GetAllAvailablePoints().Count != 0)
                                    {
                                        // Ne kadar available point var ise listeye ekliyorum ilk önce boş pointleri doldurmalıyım

                                        availablePlacementPoints.AddRange(currentVehicle.GetAllAvailablePoints());
                                    }

                                    if (stackToSendCount > availablePlacementPoints.Count)
                                    {
                                        // Yeteri kadar boş point yok, aldığım stack sayısı kadarını göndermem lazım
                                        // ve göndereceklerimin placement pointlerini listeye eklemeliyim  

                                        stackToSendCount -= availablePlacementPoints.Count; // istenilen kadar olmasa da boş point olabilir o sayıya
                                                                                            // göre göndereceklerimin sayısını bilmem gerekli


                                    }
                                    // Göndereceğim passenger stacklerin sayısını ve hangi color dan olmaması gerektiğini biliyorum
                                    //  onları bir listeye alıyorum
                                    for (int k = 0; k < currentVehicle.GetPassengerStacks().Count; k++)
                                    {
                                        if (stackToSendCount == 0) break;
                                        if (currentVehicle.GetPassengerStacks()[k].stackColor != myColorToControl
                                            && !stacksToSendList.Contains(currentVehicle.GetPassengerStacks()[k]))
                                        {
                                            stacksToSendList.Add(currentVehicle.GetPassengerStacks()[k]);
                                            stackToSendCount--;
                                        }

                                    }

                                    // Göndereceklerimin placement pointleri bana lazım listeye alıyorum
                                    for (int gg = 0; gg < stacksToSendList.Count; gg++)
                                    {
                                        availablePlacementPoints.Add(stacksToSendList[gg].GetCurrentPoint());
                                    }

                                }
                                else
                                {
                                    // Bendeki çoğunluğa sahip rengim komşumda yok, diğer azınlık renkleri kontrol ediyorum
                                    // Buradakiler stackToSend listesine eklenecek büyük ihitmalle

                                    List<ColorEnum> minorityColorStacks = new List<ColorEnum>();

                                    // Bende azınlık olan ve aynı zamanda komşumda da bulunan renkleri listeye ekliyorum göndermek için
                                    for (int m = 0; m < currentVehicle.GetPassengerStacks().Count; m++)
                                    {
                                        PassengerStack passengerStack = currentVehicle.GetPassengerStacks()[m];
                                        if (myColorToControl != passengerStack.stackColor && !targetVehicleToTranfer.GetExistingColors().Contains(myColorToControl))
                                            minorityColorStacks.Add(passengerStack.stackColor);
                                    }
                                    if (minorityColorStacks.Count == 0)
                                    {
                                        Debug.LogWarning("Neighbour list is set incorrectly.");
                                        break;
                                    }

                                    myColorToControl = minorityColorStacks[0];

                                    for (int t = 0; t < currentVehicle.GetPassengerStacks().Count; t++)
                                    {
                                        PassengerStack passengerStack = currentVehicle.GetPassengerStacks()[t];
                                        if (passengerStack.stackColor == myColorToControl && !stacksToSendList.Contains(currentVehicle.GetPassengerStacks()[t]))
                                            stacksToSendList.Add(passengerStack);
                                    }

                                }

                            }
                            else
                            {
                                List<ColorEnum> matchedColorList = new List<ColorEnum>();
                                for (int c = 0; c < currentVehicle.GetExistingColors().Count; c++)
                                {
                                    ColorEnum myColor = currentVehicle.GetExistingColors()[c];
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
                                    int myStackCountWithSpecificColor = currentVehicle.GetPassengerStacksBySpecificColor(myColorToControl).Count;


                                    // Seçilen renk komşumda daha fazla var ise gönderilecekler listeme ekliyorum
                                    if (neighborStackCountWithSpecificColor > myStackCountWithSpecificColor)
                                    {
                                        stacksToSendList.AddRange(currentVehicle.GetPassengerStacksBySpecificColor(myColorToControl));
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
                                    if (currentVehicle.GetAllAvailablePoints().Count != 0)
                                    {
                                        for (int p = 0; p < currentVehicle.GetAllAvailablePoints().Count; p++)
                                        {
                                            // Ne kadar available point var ise listeye ekliyorum ilk önce bkoş pointleri doldurmalıyım
                                            availablePlacementPoints.Add(currentVehicle.GetAllAvailablePoints()[p]);

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

                                //  Debug.LogWarning("No stack to TAKE is found");
                            }
                            if (stacksToSendList.Count == 0)
                            {
                                Debug.LogWarning("No stack to SEND is found");
                            }
                            //

                            // İlk önce alma işlemini gerçekleştiriyorum
                            for (int t = 0; t < stacksToTakeList.Count; t++)
                            {
                                LotController targetLot = stacksToTakeList[t].GetCurrentLot();
                                if (!affectedLots.Contains(targetLot) && targetLot != lot)
                                    affectedLots.Add(targetLot);

                                PassengerStack stack = stacksToTakeList[t];
                                // Kendi üzerimden göndereceğim stacklerin placement pointlerini alacaklarıma atıyorum
                                PlacementPoint placementPoint = availablePlacementPoints[t];

                                availablePlacementPoints.Add(stack.GetCurrentPoint());
                                availablePlacementPoints[t].SetOccupied(true);
                                stack.GoOtherVehicle(currentVehicle, placementPoint);
                            }

                            List<PlacementPoint> neighbourPoints = new List<PlacementPoint>(targetVehicleToTranfer.GetAllAvailablePoints());
                            int iterateCount = Mathf.Min(stacksToSendList.Count, neighbourPoints.Count);

                            for (int ss = 0; ss < iterateCount; ss++)
                            {
                                PassengerStack stackToSend = stacksToSendList[ss];
                                PlacementPoint placementPoint = neighbourPoints[ss];
                                stackToSend.GoOtherVehicle(targetVehicleToTranfer, placementPoint);
                                LotController targetLot = stacksToSendList[ss].GetCurrentLot();
                                if (!affectedLots.Contains(targetLot) && targetLot != lot)
                                    affectedLots.Add(targetLot);

                            }

                            if (currentVehicle.IsVehicleSortedFully())
                            {
                                currentVehicle.Disappearing = true;
                                float tweenDuration = .25f;
                                yield return new WaitForSeconds(startControlDelay);

                                Debug.LogWarning("This vehicle fully sorted: " + gameObject.name);
                                lot.SetOccupied(false);
                                currentVehicle.transform.DOScale(Vector3.zero, tweenDuration);
                                currentVehicle.GetPassengerStacks().Clear(); // bir daha routine başlamaması için 

                                //Wait Tween Completetion
                                yield return new WaitForSeconds(tweenDuration);

                                lot.SetOccupied(false);
                                lot.SetVehicle(null);
                            }

                            // Perform transfer süreci son komşuyla da etkileşime geçtikten sonra tekrar kontrol edilmeli
                        }
                        #endregion

                    }
                    // TRANSFER OLAYI GERÇEKLEŞTİ TEKRARDAN KONTROL BAŞLATILMALI
                    int counter = 0;
                    while (affectedLots.Count > 0)
                    {
                        var affectedSlot = affectedLots[0];
                        affectedLots.RemoveAt(0);
                        Debug.LogWarning($"{affectedSlot.name}");
                        counter++;
                        yield return new WaitForSeconds(counter * .1f);
                        Sort(affectedSlot, .1f);

                    }

                }

            }
        }
    }

}
