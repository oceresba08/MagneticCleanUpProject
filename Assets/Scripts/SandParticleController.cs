
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SandParticleController : MonoBehaviour
{
    [Header("References")]
    public ParticleSystem sandParticles;

    [Header("Dust Settings")]
    public float minimumSpeed = 0.35f;
    public float maximumSpeed = 8f;
    public float maximumEmission = 45f;

    [Header("Ground Detection")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 2.5f;
    public float particleHeight = 0.08f;

    private Rigidbody rb;
    private ParticleSystem.EmissionModule emission;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (sandParticles != null)
        {
            emission = sandParticles.emission;
            emission.rateOverTime = 0f;
        }
    }

    void FixedUpdate()
    {
        if (sandParticles == null)
            return;

        // Measure horizontal ball speed.
        Vector3 horizontalVelocity = new Vector3(
            rb.linearVelocity.x,
            0f,
            rb.linearVelocity.z
        );

        float speed = horizontalVelocity.magnitude;

        // Search for the sand beneath the ball.

        Vector3 rayStart =
            rb.position + Vector3.up * 1.5f;

        bool onGround = Physics.Raycast(
            rayStart,
            Vector3.down,
            out RaycastHit hit,
            groundCheckDistance,
            groundLayer,
            QueryTriggerInteraction.Ignore
        );

        // Debug the raycast result.
        if (Time.frameCount % 120 == 0)
        {
            Debug.Log(
                "Ground: " + onGround +
                " | Speed: " + speed +
                " | Hit: " +
                (onGround ? hit.collider.name : "Nothing")
            );
        }


        if (onGround)
        {
            // Position the dust at the sand surface.
            sandParticles.transform.position =
                hit.point + hit.normal * particleHeight;
        }

        // No dust while stationary or airborne.
        if (!onGround || speed < minimumSpeed)
        {
            emission.rateOverTime = 0f;
            return;
        }

        // Faster movement produces more dust.
        float speedPercent = Mathf.InverseLerp(
            minimumSpeed,
            maximumSpeed,
            speed
        );

        float dustAmount =
            Mathf.Lerp(5f, maximumEmission, speedPercent);

        emission.rateOverTime = dustAmount;
    }
}
