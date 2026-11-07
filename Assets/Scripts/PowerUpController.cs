
using System.Collections;
using UnityEngine;

public enum PowerUpType
{
    Speed,
    Strength
}

public class PowerUpController : MonoBehaviour
{
    public float SpeedMultiplier { get; private set; } = 1f;
    public float StrengthMultiplier { get; private set; } = 1f;

    private Coroutine speedRoutine;
    private Coroutine strengthRoutine;

    public void Activate(
        PowerUpType type,
        float multiplier,
        float duration)
    {
        if (type == PowerUpType.Speed)
        {
            if (speedRoutine != null)
                StopCoroutine(speedRoutine);

            speedRoutine = StartCoroutine(
                SpeedBoost(multiplier, duration)
            );
        }
        else if (type == PowerUpType.Strength)
        {
            if (strengthRoutine != null)
                StopCoroutine(strengthRoutine);

            strengthRoutine = StartCoroutine(
                StrengthBoost(multiplier, duration)
            );
        }
    }

    IEnumerator SpeedBoost(float multiplier, float duration)
    {
        SpeedMultiplier = Mathf.Max(1f, multiplier);

        yield return new WaitForSeconds(duration);

        SpeedMultiplier = 1f;
        speedRoutine = null;
    }

    IEnumerator StrengthBoost(float multiplier, float duration)
    {
        StrengthMultiplier = Mathf.Max(1f, multiplier);

        yield return new WaitForSeconds(duration);

        StrengthMultiplier = 1f;
        strengthRoutine = null;
    }

    void OnDisable()
    {
        SpeedMultiplier = 1f;
        StrengthMultiplier = 1f;
    }
}
