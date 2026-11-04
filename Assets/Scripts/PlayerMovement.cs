
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    public float moveForce = 12f;

    [Header("Weight Effects")]
    public float weightSlowdown = 0.25f;

    private Rigidbody rb;
    private MagnetController magnet;
    private Vector3 movement;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        magnet = GetComponent<MagnetController>();
    }

    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        movement = new Vector3(
            moveX, 0f, moveZ
        ).normalized;
    }

    void FixedUpdate()
    {
        float weight = 0f;

        if (magnet != null)
        {
            weight = magnet.TotalWeight;
        }

        float multiplier =
            1f / (1f + weight * weightSlowdown);

        multiplier = Mathf.Max(0.2f, multiplier);

        rb.AddForce(
            movement * moveForce * multiplier,
            ForceMode.Acceleration
        );
    }
}
