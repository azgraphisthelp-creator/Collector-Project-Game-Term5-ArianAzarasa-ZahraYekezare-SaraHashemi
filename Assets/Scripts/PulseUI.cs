using UnityEngine;
using DG.Tweening;

public class PulseUI : MonoBehaviour
{
    [SerializeField] private float scale = 1.15f;
    [SerializeField] private float duration = 0.6f;

    private void Start()
    {
        transform.localScale = Vector3.one;

        transform.DOScale(scale, duration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }
}