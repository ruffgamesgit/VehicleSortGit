using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ImageColorModifier : MonoBehaviour
{
    public Image image;
    public float alphaDuration = .5f;
    public float loopInterval = .5f;
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
            alphaTween = null;
            return;
        }

        alphaTween = image.DOFade(0, alphaDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                if (!stopLoop)
                {
                    image.DOFade(.45f, alphaDuration)
                        .SetEase(Ease.Linear)
                        .OnComplete(() => ModifyAlphaLoop());
                }
                else
                    alphaTween = null;
            });
    }
}
