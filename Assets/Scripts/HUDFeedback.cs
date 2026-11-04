
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDFeedback : MonoBehaviour
{
    [Header("Game References")]
    public MagnetController magnet;
    public Camera gameCamera;
    public Canvas canvas;

    [Header("Magnet Indicator")]
    public TMP_Text magnetStatusText;
    public Color magnetOnColor = Color.green;
    public Color magnetOffColor = Color.red;

    [Header("Weight Bar")]
    public Image weightBarFill;
    public TMP_Text weightText;
    public float maxDisplayWeight = 20f;

    [Header("Pickup Popup")]
    public PickupPopup popupPrefab;

    void OnEnable()
    {
        if (magnet != null)
        {
            magnet.OnItemCollected += ShowPickupPopup;
        }
    }

    void OnDisable()
    {
        if (magnet != null)
        {
            magnet.OnItemCollected -= ShowPickupPopup;
        }
    }

    void Update()
    {
        if (magnet == null)
            return;

        UpdateMagnetIndicator();
        UpdateWeightBar();
    }

    void UpdateMagnetIndicator()
    {
        if (magnet.magnetIsOn)
        {
            magnetStatusText.text = "MAGNET ON";
            magnetStatusText.color = magnetOnColor;
        }
        else
        {
            magnetStatusText.text = "MAGNET OFF";
            magnetStatusText.color = magnetOffColor;
        }
    }

    void UpdateWeightBar()
    {
        float weight = magnet.TotalWeight;

        weightBarFill.fillAmount =
            Mathf.Clamp01(weight / maxDisplayWeight);

        weightText.text =
            "Weight: " + weight.ToString("0.##") +
            " / " + maxDisplayWeight.ToString("0.##");

        // Change bar color as cargo gets heavier.
        float percentage = weightBarFill.fillAmount;

        if (percentage >= 0.8f)
            weightBarFill.color = new Color(1f, 0.45f, 0.3f);
        else if (percentage >= 0.5f)
            weightBarFill.color = Color.yellow;
        else
            weightBarFill.color = Color.green;
    }

    void ShowPickupPopup(int value, Vector3 worldPosition)
    {
        if (popupPrefab == null || canvas == null ||
            gameCamera == null)
            return;

        // Convert the 3D item's position to screen pixels.
        Vector3 screenPosition =
            gameCamera.WorldToScreenPoint(
                worldPosition + Vector3.up * 0.7f
            );

        // Don't show objects behind the camera.
        if (screenPosition.z <= 0f)
            return;

        RectTransform canvasRect =
            canvas.GetComponent<RectTransform>();

        Camera uiCamera =
            canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : canvas.worldCamera;

        Vector2 localPosition;

        bool converted =
            RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPosition,
                uiCamera,
                out localPosition
            );

        if (!converted)
            return;

        // Create a new popup under the Canvas.
        PickupPopup popup =
            Instantiate(popupPrefab, canvasRect);

        RectTransform popupRect =
            popup.GetComponent<RectTransform>();

        popupRect.anchorMin = new Vector2(0.5f, 0.5f);
        popupRect.anchorMax = new Vector2(0.5f, 0.5f);
        popupRect.pivot = new Vector2(0.5f, 0.5f);

        popupRect.anchoredPosition = localPosition;

        // Show the correct dollar amount.
        popup.Initialize(value);
    }
}
