
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GiantClam : MonoBehaviour
{
    [Header("Shell")]
    public Transform topShellPivot;

    // Use 1 or -1 to reverse rotation direction.
    public float rotationDirection = 1f;

    [Header("Timing")]
    public float openDuration = 4f;
    public float windupDuration = 0.8f;
    public float snapDuration = 0.15f;
    public float closedDuration = 2f;
    public float reopenDuration = 0.6f;

    [Header("Treasure")]
    public MagneticItem treasure;

    [Header("Player")]
    public Rigidbody playerBody;
    public PlayerMovement playerMovement;
    public MagnetController playerMagnet;

    [Header("Capture Detection")]
    public BoxCollider captureZone;

    [Header("UI")]
    public TMP_Text statusText;
    public GameObject clamGameOverPanel;

    private Quaternion closedRotation;
    private bool treasureTaken;
    private bool gameOver;

    void Start()
    {
        if (topShellPivot == null || captureZone == null)
        {
            Debug.LogError(
                "Assign TopShellPivot and CaptureZone!"
            );

            enabled = false;
            return;
        }

        // The pivot's Inspector rotation represents 0°.
        closedRotation = topShellPivot.localRotation;

        if (clamGameOverPanel != null)
            clamGameOverPanel.SetActive(false);

        // Start OPEN at 90 degrees.
        SetShellAngle(90f);
        ShowTreasure(true);

        StartCoroutine(ClamCycle());
    }

    void Update()
    {
        // Remember if the treasure was collected.
        if (treasure != null && treasure.IsHeld)
            treasureTaken = true;
    }

    IEnumerator ClamCycle()
    {
        while (!gameOver)
        {
            // PHASE 1: Open at 90 degrees.
            SetShellAngle(90f);
            SetStatus("CLAM OPEN!");

            ShowTreasure(true);

            yield return new WaitForSeconds(
                Mathf.Max(0f, openDuration)
            );

            // PHASE 2: Wind forward to 180.
            SetStatus("CLAM WINDING UP! ESCAPE!");

            yield return StartCoroutine(
                RotateShell(
                    90f,
                    180f,
                    windupDuration,
                    false
                )
            );

            // PHASE 3: Snap from 180 to 0.
            SetStatus("SNAP!");

            yield return StartCoroutine(
                RotateShell(
                    180f,
                    0f,
                    snapDuration,
                    true
                )
            );

            SetShellAngle(0f);

            // Check actual collider overlap NOW.
            if (IsPlayerInsideClam())
            {
                TriggerClamGameOver();
                yield break;
            }

            // Player escaped.
            SetStatus("CLAM CLOSED");
            ShowTreasure(false);

            yield return new WaitForSeconds(
                Mathf.Max(0f, closedDuration)
            );

            // PHASE 4: Open from 0 to 90.
            SetStatus("CLAM OPENING");

            yield return StartCoroutine(
                RotateShell(
                    0f,
                    90f,
                    reopenDuration,
                    false
                )
            );
        }
    }

    IEnumerator RotateShell(
        float startAngle,
        float endAngle,
        float duration,
        bool snap)
    {
        duration = Mathf.Max(0.01f, duration);

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(
                timer / duration
            );

            if (snap)
            {
                // Fast acceleration.
                t = t * t;
            }
            else
            {
                // Smooth windup / opening.
                t = t * t * (3f - 2f * t);
            }

            // Mathf.Lerp preserves the intended 180-to-0
            // angle path, rather than choosing a shortcut.
            float angle = Mathf.Lerp(
                startAngle,
                endAngle,
                t
            );

            SetShellAngle(angle);

            yield return null;
        }

        SetShellAngle(endAngle);
    }

    void SetShellAngle(float angle)
    {
        topShellPivot.localRotation =
            closedRotation *
            Quaternion.Euler(
                angle * rotationDirection,
                0f,
                0f
            );
    }

    bool IsPlayerInsideClam()
    {
        if (playerBody == null || captureZone == null)
            return false;

        // Use the actual BoxCollider geometry
        // to check for overlapping objects.
        Vector3 center = captureZone.transform.TransformPoint(
            captureZone.center
        );

        Vector3 size = captureZone.size;

        Vector3 scale =
            captureZone.transform.lossyScale;

        Vector3 halfExtents = new Vector3(
            size.x * Mathf.Abs(scale.x) * 0.5f,
            size.y * Mathf.Abs(scale.y) * 0.5f,
            size.z * Mathf.Abs(scale.z) * 0.5f
        );

        // Ensure current physics transforms are up to date.
        Physics.SyncTransforms();

        Collider[] hits = Physics.OverlapBox(
            center,
            halfExtents,
            captureZone.transform.rotation,
            ~0,
            QueryTriggerInteraction.Ignore
        );

        foreach (Collider hit in hits)
        {
            if (hit.attachedRigidbody == playerBody)
            {
                Debug.Log("Player caught by clam!");
                return true;
            }
        }

        Debug.Log("Player escaped the clam!");
        return false;
    }

    void ShowTreasure(bool visible)
    {
        if (treasure == null || treasureTaken)
            return;

        treasure.gameObject.SetActive(visible);
    }

    void TriggerClamGameOver()
    {
        if (gameOver)
            return;

        gameOver = true;

        SetStatus("CLAM GAME OVER!");

        if (playerMagnet != null)
        {
            playerMagnet.DropEverything();
            playerMagnet.enabled = false;
        }

        if (playerMovement != null)
            playerMovement.enabled = false;

        if (playerBody != null)
        {
            playerBody.linearVelocity = Vector3.zero;
            playerBody.angularVelocity = Vector3.zero;
            playerBody.isKinematic = true;
        }

        if (clamGameOverPanel != null)
            clamGameOverPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    // Show the capture area when the clam is selected.
    void OnDrawGizmosSelected()
    {
        if (captureZone == null)
            return;

        Gizmos.color = Color.red;

        Matrix4x4 oldMatrix = Gizmos.matrix;

        Gizmos.matrix =
            captureZone.transform.localToWorldMatrix;

        Gizmos.DrawWireCube(
            captureZone.center,
            captureZone.size
        );

        Gizmos.matrix = oldMatrix;
    }
}
