using UnityEngine;

public class RandomColor : MonoBehaviour
{
    [SerializeField] private Renderer targetRenderer;

    private void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<Renderer>();

        ApplyRandomColor();
    }

    public void ApplyRandomColor()
    {
        if (targetRenderer == null) return;

        Color brightColor = Random.ColorHSV(
            0f, 1f,     // Hue
            0.6f, 1f,   // Saturation (شاد)
            0.8f, 1f    // Value (روشن)
        );

        targetRenderer.material.color = brightColor;
    }
}