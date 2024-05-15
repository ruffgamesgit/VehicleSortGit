using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ImageColorModifier : MonoBehaviour
{
    public Image image;
    private Tween _alphaTween;

    public void SetHighlight(bool activate)
    {
        ModifyAlpha(activate);
    }
    private void ModifyAlpha(bool activate)
    {
        _alphaTween?.Kill(true);
        _alphaTween = image.DOFade(activate ? 0.25f : 0f, activate ? 0.25f : 0.15f).SetEase(Ease.OutQuad);
    }
    
}
