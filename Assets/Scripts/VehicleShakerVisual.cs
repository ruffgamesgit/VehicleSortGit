using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

public class VehicleShakerVisual : MonoBehaviour
{
    private MaterialPropertyBlock _mbp;
    private MaterialPropertyBlock _mbp2;
    private MaterialPropertyBlock _mbp3;
    

    private float _maxLerpValue;
    public float tweenTime;

    private Vector3 _prevPos;

    private bool _moving = true;

    private Vector3 _moveDirection;

    private bool _justOnce;

    private Tween moveTween;
    private Tween moveTween2;
    private Tween moveTween3;

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        _maxLerpValue = 0.01f;
        tweenTime = 0.25f;
        _prevPos = transform.position;
    }

    private void LateUpdate()
    {
        Vector3 tmpLocal = transform.eulerAngles;
        transform.eulerAngles = tmpLocal;
        
        if ((transform.position - _prevPos).magnitude > 0.005f)
        {
            _moving = true;

            _justOnce = false;

            _moveDirection = (transform.position - _prevPos).normalized;

            _prevPos = transform.position;

            if (moveTween != null) moveTween.Kill();
            if (moveTween2 != null) moveTween2.Kill();
            if (moveTween3 != null) moveTween3.Kill();
        }
        else
        {
            _moving = false;
        }

        if (_moving)
        {
            Vector3 targetRot = Vector3.Lerp(Vector3.up, -_moveDirection, _maxLerpValue);

            moveTween3 = DOVirtual.Vector3(transform.up, targetRot, tweenTime - .2f, (v) => transform.up = v);
        }
        else if (!_justOnce)
        {
            _justOnce = true;
            _moveDirection += new Vector3(0, Random.Range(0f, 2f)
                , 0);
            Vector3 targetRot = Vector3.Lerp(Vector3.up, _moveDirection, .15f);
            ;
            if (moveTween != null) moveTween.Kill();
            if (moveTween2 != null) moveTween2.Kill();
            if (moveTween3 != null) moveTween3.Kill();

            moveTween = DOVirtual.Vector3(transform.up, targetRot, tweenTime - .1f, (v) => transform.up = v)
                .OnComplete(
                    () => moveTween2 = DOVirtual.Vector3(transform.up, Vector3.up, .15f, (v) => transform.up = v));
        }
    }
}