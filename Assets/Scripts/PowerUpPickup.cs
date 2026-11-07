
using UnityEngine;

public class PowerUpPickup : MonoBehaviour
{
    public PowerUpType type = PowerUpType.Speed;
    public float multiplier = 1.5f;
    public float duration = 8f;

    void OnTriggerEnter(Collider other)
    {
        Rigidbody body = other.attachedRigidbody;

        if (body == null)
            return;

        PowerUpController controller =
            body.GetComponent<PowerUpController>();

        if (controller == null)
            return;

        controller.Activate(type, multiplier, duration);

        Destroy(gameObject);
    }
}
