
using UnityEngine;
using TMPro;

public class HUDController : MonoBehaviour
{
    [Header("References")]
    public MagnetController magnet;

    [Header("UI Text")]
    public TMP_Text scoreText;
    public TMP_Text valueText;
    public TMP_Text itemText;

    void Update()
    {
        if (magnet == null)
            return;

        scoreText.text =
            "Score: " + magnet.Score;

        valueText.text =
            "Value: $" + magnet.CarriedValue;

        itemText.text =
            "Items: " + magnet.CarriedCount;
    }
}
