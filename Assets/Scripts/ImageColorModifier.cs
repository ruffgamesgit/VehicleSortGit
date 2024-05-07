using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ImageColorModifier : MonoBehaviour
{
    public Image image;
    public float tweenDuration = .5f;
    public bool stopLoop = false;
    private Tween alphaTween;

    public void SetHighlight(bool activate)
    {
        stopLoop = !activate;
        ModifyAlphaLoop();
    }

    void ModifyAlphaLoop()
    {
        if (stopLoop)
        {
            alphaTween.Kill();
            alphaTween = null;
            image.DOFade(0, 0.25f);
            return;
        }

        alphaTween = image.DOFade(0, tweenDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                image.DOFade(.45f, tweenDuration)
                    .SetEase(Ease.Linear)
                    .OnComplete(() => ModifyAlphaLoop());
            });
    }

    bool flag = false;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            flag = !flag;
            SetHighlight(flag);
        }
    }
}
