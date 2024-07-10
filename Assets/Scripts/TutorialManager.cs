using System.Collections.Generic;
using DG.Tweening;
using GamePlay.Components;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [Header("First Step Properties")] [SerializeField]
    private ParkingLot firstParkingLot;

    [SerializeField] private Transform firstHand;
    private Tween _firstTween;

    [Header("Second Step Properties")] [SerializeField]
    private ParkingLot secondParkingLot;
    private ParkingLot _lotToDeactivate;

    [SerializeField] private Transform secondHand;
    private Sequence _secondTweenSequence;
    [SerializeField] private List<ParkingLot> targetLots;

    private void Start()
    {
        secondParkingLot = targetLots[0].GetCurrentVehicle() == null ? targetLots[0] : targetLots[1];
        _lotToDeactivate = targetLots[0].GetCurrentVehicle() == null ? targetLots[1] : targetLots[0];
        _lotToDeactivate.GetComponent<Collider>().enabled = false;
        
        firstParkingLot.OnParkingLotClicked += OnFirstParkingLotClicked;
        secondParkingLot.OnParkingLotClicked += OnSecondParkingLotClicked;

        _firstTween = firstHand.DOScale(Vector3.one * .5f, 1).SetLoops(-1, LoopType.Yoyo);
        _firstTween.Play();
    }


    private void OnFirstParkingLotClicked(object sender, Vehicle e)
    {
        if (_firstTween.IsPlaying())
            _firstTween.Pause();

        firstHand?.gameObject.SetActive(false);
        PerformSecondStep();
    }

    private void PerformSecondStep()
    {
        secondHand.gameObject.SetActive(true);
        _secondTweenSequence = DOTween.Sequence();
        _secondTweenSequence.Join(secondHand
            .DOMove(
                new Vector3(secondParkingLot.transform.position.x, secondHand.transform.position.y,
                    secondParkingLot.transform.position.z), 1));

        _secondTweenSequence.Append(secondHand.DOScale(Vector3.one * .5f, 1).SetLoops(-1, LoopType.Yoyo));


        _secondTweenSequence.Play();
    }

    private void OnSecondParkingLotClicked(object sender, Vehicle e)
    {
        if (_secondTweenSequence.IsPlaying())
            _secondTweenSequence.Pause();

        secondHand.gameObject.SetActive(false);

        Destroy(gameObject);
    }
}