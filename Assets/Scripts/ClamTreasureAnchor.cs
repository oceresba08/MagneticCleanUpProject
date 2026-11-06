
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ClamTreasureAnchor : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public MagneticItem magneticItem;

    [Header("Settings")]
    public float releaseDistance = 3f;

    private Rigidbody rb;
    private bool released;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (magneticItem == null)
            magneticItem = GetComponent<MagneticItem>();
    }

    void OnEnable()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        // Never re-anchor a collected item.
        if (magneticItem != null && magneticItem.IsHeld)
            return;

        released = false;

        // Hold the treasure inside the open clam.
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    void Update()
    {
        if (released || player == null)
            return;

        if (magneticItem != null && magneticItem.IsHeld)
        {
            released = true;
            return;
        }

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        if (distance <= releaseDistance)
        {
            ReleaseTreasure();
        }
    }

    void ReleaseTreasure()
    {
        if (released)
            return;

        released = true;

        // Enable normal physics so the player's
        // magnet can pull and attach the coin.
        rb.isKinematic = false;
        rb.useGravity = true;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Debug.Log("Clam treasure released!");

        // The anchor no longer controls this coin.
        enabled = false;
    }
}
