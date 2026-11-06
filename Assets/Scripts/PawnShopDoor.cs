
using System.Collections;
using UnityEngine;

public class PawnShopDoor : MonoBehaviour
{
    public Transform doorPivot;
    public Rigidbody playerBody;

    public float openAngle = 90f;
    public float openDuration = 0.6f;

    private bool opened;

    void OnTriggerEnter(Collider other)
    {
        if (opened || playerBody == null)
            return;

        if (other.attachedRigidbody != playerBody)
            return;

        opened = true;
        StartCoroutine(OpenDoor());
    }

    IEnumerator OpenDoor()
    {
        if (doorPivot == null)
            yield break;

        Quaternion start = doorPivot.localRotation;

        Quaternion end = start *
            Quaternion.Euler(0f, openAngle, 0f);

        float elapsed = 0f;

        while (elapsed < openDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(
                elapsed / Mathf.Max(0.01f, openDuration)
            );

            doorPivot.localRotation =
                Quaternion.Slerp(start, end, t);

            yield return null;
        }

        doorPivot.localRotation = end;
    }
}
