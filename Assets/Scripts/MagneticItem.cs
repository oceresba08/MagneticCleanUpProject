
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class MagneticItem : MonoBehaviour
{
    public float weight = 1f;
    public int value = 1;

    public bool IsHeld { get; private set; }
    public Rigidbody Body { get; private set; }

    private FixedJoint attachment;

    void Awake()
    {
        Body = GetComponent<Rigidbody>();
    }

    public void AttachTo(Rigidbody ballBody)
    {
        if (IsHeld) return;

        // Keep the item physically active.
        Body.isKinematic = false;
        Body.useGravity = true;

        // Prevent sudden movement when attaching.
        Body.linearVelocity = ballBody.linearVelocity;
        Body.angularVelocity = ballBody.angularVelocity;

        // Physically connect the item to the ball.
        attachment = gameObject.AddComponent<FixedJoint>();

        attachment.connectedBody = ballBody;
        attachment.enableCollision = false;
        attachment.breakForce = Mathf.Infinity;
        attachment.breakTorque = Mathf.Infinity;

        IsHeld = true;
    }

    public void Release(Vector3 ballPosition, float dropForce)
    {
        if (!IsHeld) return;

        if (attachment != null)
        {
            Destroy(attachment);
            attachment = null;
        }

        IsHeld = false;

        // Small outward push when released.
        Vector3 direction =
            transform.position - ballPosition;

        if (direction.sqrMagnitude < 0.001f)
            direction = Vector3.right;

        direction.y += 0.3f;

        Body.AddForce(
            direction.normalized * dropForce,
            ForceMode.Impulse
        );
    }
}
