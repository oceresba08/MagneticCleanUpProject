
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class CrabPatrol : MonoBehaviour
{
    [Header("Chase Timer")]
    public float maxChaseDuration = 4f;
    private float chaseTimer = 0f;

    [Header("After Attack")]
    public float recoveryDuration = 3f;
    private float recoveryTimer = 0f;

    [Header("Patrol")]
    public Transform pointA;
    public Transform pointB;
    public float moveSpeed = 2f;
    public float pauseDuration = 0.6f;

    [Header("Terrain")]
    public Terrain beachTerrain;
    public float heightOffset = 0.5f;

    [Header("Chasing")]
    public Rigidbody playerBody;
    public float detectionRadius = 7f;
    public float escapeRadius = 10f;
    public float chaseSpeed = 5f;
    public float attackWarning = 0.5f;

    [Header("Attack")]
    public float pushStrength = 12f;
    public float upwardPush = 3f;
    public float pushCooldown = 2f;

    private Rigidbody rb;
    private Transform patrolTarget;

    private float pauseTimer;
    private float warningTimer;
    private float lastPushTime = -999f;

    private bool chasing;
    private bool warning;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.isKinematic = true;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode =
            CollisionDetectionMode.ContinuousSpeculative;
    }

    void Start()
    {
        patrolTarget = pointB;
        SnapToTerrain();
    }

    void FixedUpdate()
    {
        // After attacking, return to patrolling.
        // Don't detect the player again during recovery.
        if (recoveryTimer > 0f)
        {
            recoveryTimer -= Time.fixedDeltaTime;

            chasing = false;
            warning = false;

            Patrol();
            return;
        }

        if (playerBody == null)
            return;

        Vector3 toPlayer =
            playerBody.position - rb.position;

        // Measure distance horizontally.
        toPlayer.y = 0f;
        float distance = toPlayer.magnitude;

        // Start chasing when player gets close.
        if (!chasing && distance <= detectionRadius)
        {
            chasing = true;
            warning = true;
            warningTimer = attackWarning;

            // Start the chase countdown.
            chaseTimer = maxChaseDuration;

            Debug.Log("Crab spotted the player!");
        }

        // Stop chasing when player gets far away.
        if (chasing && distance >= escapeRadius)
        {
            chasing = false;
            warning = false;
            pauseTimer = pauseDuration;
        }


        if (chasing)
        {
            // Count down the chase time.
            chaseTimer -= Time.fixedDeltaTime;

            // Give up when the timer runs out.
            if (chaseTimer <= 0f)
            {
                chasing = false;
                warning = false;

                // Prevent immediate re-detection.
                recoveryTimer = recoveryDuration;

                pauseTimer = 0f;

                Debug.Log("Crab gave up chasing!");
                return;
            }

            // Brief warning before chasing.
            if (warning)
            {
                warningTimer -= Time.fixedDeltaTime;

                if (warningTimer <= 0f)
                    warning = false;

                return;
            }

            MoveTo(playerBody.position, chaseSpeed);
        }
        else
        {
            Patrol();
        }

    }

    void Patrol()
    {
        if (pointA == null || pointB == null)
            return;

        if (pauseTimer > 0f)
        {
            pauseTimer -= Time.fixedDeltaTime;
            return;
        }

        MoveTo(patrolTarget.position, moveSpeed);

        Vector2 crabXZ = new Vector2(
            rb.position.x, rb.position.z
        );

        Vector2 targetXZ = new Vector2(
            patrolTarget.position.x,
            patrolTarget.position.z
        );

        if (Vector2.Distance(crabXZ, targetXZ) < 0.15f)
        {
            patrolTarget = patrolTarget == pointA
                ? pointB
                : pointA;

            pauseTimer = pauseDuration;
        }
    }

    void MoveTo(Vector3 destination, float speed)
    {
        Vector3 current = rb.position;

        // Move across the ground, not vertically
        // toward the player's elevated center.
        Vector3 flatDestination = new Vector3(
            destination.x,
            current.y,
            destination.z
        );

        Vector3 next = Vector3.MoveTowards(
            current,
            flatDestination,
            speed * Time.fixedDeltaTime
        );

        if (beachTerrain != null)
        {
            next.y =
                beachTerrain.SampleHeight(next) +
                beachTerrain.transform.position.y +
                heightOffset;
        }

        rb.MovePosition(next);
    }

    void SnapToTerrain()
    {
        if (beachTerrain == null)
            return;

        Vector3 position = rb.position;

        position.y =
            beachTerrain.SampleHeight(position) +
            beachTerrain.transform.position.y +
            heightOffset;

        rb.position = position;
    }

    void OnCollisionEnter(Collision collision)
    {
        TryAttack(collision);
    }

    void OnCollisionStay(Collision collision)
    {
        TryAttack(collision);
    }

    void TryAttack(Collision collision)
    {
        if (playerBody == null)
            return;

        if (collision.rigidbody != playerBody)
            return;

        if (Time.time - lastPushTime < pushCooldown)
            return;

        // Prevent attacking while winding up.
        if (warning)
            return;

        lastPushTime = Time.time;

        MagnetController magnet =
            playerBody.GetComponent<MagnetController>();

        if (magnet != null)
            magnet.GetHitByCrab();

        Vector3 direction =
            playerBody.position - rb.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            direction = Vector3.forward;

        direction.Normalize();

        // Strong horizontal and upward knockback.
        playerBody.AddForce(
            direction * pushStrength +
            Vector3.up * upwardPush,
            ForceMode.VelocityChange
        );

        Debug.Log("CRAB ATTACK!");

        // Successful attack: stop chasing.
        chasing = false;
        warning = false;

        // Ignore the player for a few seconds.
        recoveryTimer = recoveryDuration;

        // Return to the normal patrol route.
        pauseTimer = 0f;

        Debug.Log("Crab returning to patrol!");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            transform.position, detectionRadius
        );

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            transform.position, escapeRadius
        );
    }
}
