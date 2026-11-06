
using System.Collections;
using UnityEngine;

public class DoublePawnShopDoor : MonoBehaviour
{
    [Header("Doors")]
    public Transform leftDoorPivot;
    public Transform rightDoorPivot;

    [Header("Player")]
    public Rigidbody playerBody;

    [Header("Animation")]
    public float openAngle = 90f;
    public float openDuration = 0.7f;

    private bool opened;

    void OnTriggerEnter(Collider other)
    {
        if (opened || playerBody == null)
            return;

        if (other.attachedRigidbody != playerBody)
            return;

        opened = true;
        StartCoroutine(OpenBothDoors());
    }

    IEnumerator OpenBothDoors()
    {
        if (leftDoorPivot == null ||
            rightDoorPivot == null)
            yield break;

        Quaternion leftStart =
            leftDoorPivot.localRotation;

        Quaternion rightStart =
            rightDoorPivot.localRotation;

        // The doors rotate in opposite directions.
        Quaternion leftEnd =
            leftStart *
            Quaternion.Euler(0f, -openAngle, 0f);

        Quaternion rightEnd =
            rightStart *
            Quaternion.Euler(0f, openAngle, 0f);

        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, openDuration);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(
                elapsed / duration
            );

            t = t * t * (3f - 2f * t);

            leftDoorPivot.localRotation =
                Quaternion.Slerp(leftStart, leftEnd, t);

            rightDoorPivot.localRotation =
                Quaternion.Slerp(rightStart, rightEnd, t);

            yield return null;
        }

        leftDoorPivot.localRotation = leftEnd;
        rightDoorPivot.localRotation = rightEnd;
    }
}
