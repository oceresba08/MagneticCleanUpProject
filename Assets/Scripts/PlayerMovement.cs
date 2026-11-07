
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Environment")]
    public float environmentMoveMultiplier = 1f;

    [Header("Movement")]
    public float moveForce = 18f;
    public float rollTorque = 30f;

    [Header("Weight")]
    public float weightSlowdown = 0.15f;
    public float minimumPower = 0.4f;

    [Header("Tumble Assist")]
    public float tumbleUp = 5f;
    public float tumbleForward = 2f;
    public float tumbleRotation = 7f;

    public float stuckSpeed = 0.6f;
    public float stuckDuration = 0.35f;
    public float tumbleCooldown = 1.5f;

    [Header("Camera")]
    public Transform cameraTransform;

    private Rigidbody rb;
    private MagnetController magnet;
    private Vector3 movement;

    private float stuckTimer;
    private float cooldownTimer;

    private float power = 1f;

    private PowerUpController powerUps;

    void Start()
    {
        powerUps = GetComponent<PowerUpController>();
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        magnet = GetComponent<MagnetController>();

        rb.maxAngularVelocity = 30f;
    }


    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        // If no camera is assigned, use world movement.
        if (cameraTransform == null)
        {
            movement = new Vector3(x, 0f, z).normalized;
            return;
        }

        // Get the camera's forward and right directions.
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        // Remove vertical components so the ball
        // doesn't try to fly into the air.
        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        // Convert keyboard input into camera-relative movement.
        movement = (forward * z + right * x).normalized;
    }


    void FixedUpdate()
    {
        cooldownTimer -= Time.fixedDeltaTime;

        if (movement.sqrMagnitude < 0.001f)
        {
            stuckTimer = 0f;
            return;
        }

        float weight = 0f;
        int carriedCount = 0;

        if (magnet != null)
        {
            weight = magnet.TotalWeight;
            carriedCount = magnet.CarriedCount;
        }




        float strengthBoost = powerUps != null
            ? powerUps.StrengthMultiplier
            : 1f;

        bool strengthActive = strengthBoost > 1f;

        if (strengthActive)
        {
            // Ignore carried-weight slowdown completely.
            power = 1f;
        }
        else
        {
            // Normal weight slowdown.
            power = 1f /
                (1f + weight * weightSlowdown);

            power = Mathf.Max(minimumPower, power);
        }


        float speedBoost = powerUps != null
    ? powerUps.SpeedMultiplier
    : 1f;

        float movementBoost =
            speedBoost * (strengthActive ? 1.25f : 1f);

        // Move forward.
        rb.AddForce(
            movement * moveForce * power
            * environmentMoveMultiplier * speedBoost,
            ForceMode.Acceleration
        );



        // Roll forward.
        Vector3 rollAxis =
            Vector3.Cross(Vector3.up, movement);

        rb.AddTorque(
            rollAxis * rollTorque * power
            * speedBoost,
            ForceMode.Acceleration
        );


        // Check horizontal movement speed.
        Vector3 horizontalVelocity =
            new Vector3(rb.linearVelocity.x, 0f,
                        rb.linearVelocity.z);

        bool isStuck =
            horizontalVelocity.magnitude < stuckSpeed;

        // Check if near the ground or other solid objects.
        bool nearGround = Physics.Raycast(
            transform.position,
            Vector3.down,
            1.5f,
            ~0,
            QueryTriggerInteraction.Ignore
        );

        if (carriedCount > 0 && isStuck && nearGround)
        {
            stuckTimer += Time.fixedDeltaTime;
        }
        else
        {
            stuckTimer = 0f;
        }

        if (stuckTimer >= stuckDuration &&
            cooldownTimer <= 0f)
        {
            Tumble(rollAxis);

            stuckTimer = 0f;
            cooldownTimer = tumbleCooldown;
        }
    }

    void Tumble(Vector3 rollAxis)
    {
        // Launch upward and slightly forward.
        Vector3 boost =
            Vector3.up * tumbleUp +
            movement * tumbleForward;

        rb.AddForce(
            boost,
            ForceMode.VelocityChange
        );

        // Give the ball a sudden burst of rotation.
        rb.AddTorque(
            rollAxis * rollTorque * power
            * environmentMoveMultiplier,
            ForceMode.Acceleration
        );

        Debug.Log("TUMBLE!");
    }
}
