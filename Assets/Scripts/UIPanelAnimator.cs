using UnityEngine;
using DG.Tweening;

public class UIPanelAnimator : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform panel;
    [SerializeField] private float duration = 0.25f;
    [SerializeField] private float showScale = 2.62f;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        panel.localScale = Vector3.zero;
        canvasGroup.alpha = 0f;

        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);

        panel.localScale = Vector3.zero;
        canvasGroup.alpha = 0f;

        DOTween.Kill(panel);
        DOTween.Kill(canvasGroup);

        Sequence seq = DOTween.Sequence();
        seq.SetUpdate(true);

        seq.Join(panel.DOScale(showScale, duration).SetEase(Ease.OutBack));
        seq.Join(canvasGroup.DOFade(1f, duration));
    }

    public void Hide()
    {
        DOTween.Kill(panel);
        DOTween.Kill(canvasGroup);

        Sequence seq = DOTween.Sequence();
        seq.SetUpdate(true);

        seq.Join(panel.DOScale(0f, duration).SetEase(Ease.InBack));
        seq.Join(canvasGroup.DOFade(0f, duration));

        seq.OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }
}