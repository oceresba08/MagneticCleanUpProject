
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Camera Position")]
    public float height = 7f;
    public float distance = 6f;
    public float lookAhead = 1.5f;

    [Header("Smooth Following")]
    public float followSpeed = 8f;

    void LateUpdate()
    {
        if (target == null)
            return;

        // Fixed camera offset, above and behind the ball.
        Vector3 offset = new Vector3(
            0f,
            height,
            -distance
        );

        Vector3 desiredPosition =
            target.position + offset;

        // Move smoothly toward the ball.
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            followSpeed * Time.deltaTime
        );

        // Look toward a point slightly ahead of the ball.
        Vector3 lookTarget =
            target.position + Vector3.forward * lookAhead;

        transform.LookAt(lookTarget);
    }
}
