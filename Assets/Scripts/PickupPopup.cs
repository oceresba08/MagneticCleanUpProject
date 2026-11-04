
using UnityEngine;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class PickupPopup : MonoBehaviour
{
    public float duration = 1.2f;
    public float riseSpeed = 75f;

    private TMP_Text popupText;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    private float elapsed;

    void Awake()
    {
        popupText = GetComponent<TMP_Text>();
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
    }

    public void Initialize(int value)
    {
        popupText.text = "+$" + value;

        elapsed = 0f;
        canvasGroup.alpha = 1f;
    }

    void Update()
    {
        elapsed += Time.deltaTime;

        // Slowly rise upward.
        rectTransform.anchoredPosition +=
            Vector2.up * riseSpeed * Time.deltaTime;

        // Gradually fade away.
        float progress = elapsed / duration;
        canvasGroup.alpha = 1f - Mathf.Clamp01(progress);

        // Destroy the popup after the animation.
        if (elapsed >= duration)
        {
            Destroy(gameObject);
        }
    }
}
