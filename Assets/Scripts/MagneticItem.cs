
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class MagneticItem : MonoBehaviour
{
    public float weight = 1f;
    public int value = 1;

    public bool IsHeld { get; private set; }
    public Rigidbody Body { get; private set; }

    private Collider itemCollider;

    void Awake()
    {
        Body = GetComponent<Rigidbody>();
        itemCollider = GetComponent<Collider>();
    }

    public void AttachTo(
        Transform magnet,
        Vector3 direction,
        float distance)
    {
        // Stop the item's physics movement.
        Body.linearVelocity = Vector3.zero;
        Body.angularVelocity = Vector3.zero;

        // The magnet now controls the item.
        Body.isKinematic = true;
        Body.useGravity = false;
        itemCollider.enabled = false;

        // Attach it to the ball.
        transform.SetParent(magnet, true);
        transform.position =
            magnet.position + direction.normalized * distance;

        IsHeld = true;
    }

    public void Release(Vector3 ballPosition, float dropForce)
    {
        // Find a direction away from the ball.
        Vector3 direction = transform.position - ballPosition;
        direction.y = Mathf.Max(direction.y, 0.3f);
        direction.Normalize();

        // Detach from the ball.
        transform.SetParent(null, true);

        // Move outward to reduce overlap with the ball.
        transform.position =
            ballPosition + direction * 1.15f;

        // Restore physics.
        Body.isKinematic = false;
        Body.useGravity = true;
        itemCollider.enabled = true;

        // Push the item away a little.
        Body.AddForce(
            direction * dropForce,
            ForceMode.Impulse
        );

        IsHeld = false;
    }
}
