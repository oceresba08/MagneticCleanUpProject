
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GiantClam : MonoBehaviour
{
    [Header("Shell")]
    public Transform topShellPivot;

    [Tooltip("The angle when the clam is wide open.")]
    public float openAngle = 90f;


    [Header("Timing")]
    public float openDuration = 4f;

    // Wind from 90 degrees to 180 degrees.
    public float windupDuration = 1.2f;

    // Pause at the top of the windup.
    public float windupHoldDuration = 0.35f;

    // Fast snap from 180 degrees to 0 degrees.
    public float closeDuration = 0.35f;

    public float closedDuration = 2f;
    public float reopenDuration = 0.8f;


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

    private bool gameOver = false;
    private bool treasureTaken = false;

    void Start()
    {
        // Check important references.
        if (topShellPivot == null ||
            captureZone == null ||
            playerBody == null)
        {
            Debug.LogError(
                "GiantClam is missing a reference!"
            );

            enabled = false;
            return;
        }

        // Ensure the capture area does not block the ball.
        captureZone.isTrigger = true;

        // Reset the game's time.
        // Do not change Time.timeScale here because
        // another system may control pausing.
        if (clamGameOverPanel != null)
            clamGameOverPanel.SetActive(false);

        // Start with the clam wide open.
        SetShellAngle(openAngle);

        ShowTreasure(true);

        StartCoroutine(ClamCycle());
    }


    void Update()
    {
        if (treasure == null || treasureTaken)
            return;

        // Detect when the coin attaches to the ball.
        if (treasure.IsHeld)
        {
            treasureTaken = true;

            // The clam no longer owns this treasure.
            Debug.Log("Clam treasure collected permanently!");

            // Move the treasure out of the clam's hierarchy.
            treasure.transform.SetParent(null, true);

            // Remove the anchor so it cannot interfere.
            ClamTreasureAnchor anchor =
                treasure.GetComponent<ClamTreasureAnchor>();

            if (anchor != null)
                anchor.enabled = false;
        }
    }



    IEnumerator ClamCycle()
    {
        while (!gameOver)
        {
            // 1. OPEN: Start upright.
            SetShellAngle(openAngle);
            SetStatus("CLAM OPEN!");
            ShowTreasure(true);

            yield return new WaitForSeconds(
                Mathf.Max(0f, openDuration)
            );

            // 2. WINDUP: Move from 90 to 180 degrees.
            SetStatus("CLAM WINDING UP! ESCAPE!");

            float windupAngle = openAngle * 2f;

            yield return StartCoroutine(
                AnimateShell(
                    openAngle,
                    windupAngle,
                    windupDuration
                )
            );

            // Brief pause before the snap.
            yield return new WaitForSeconds(
                Mathf.Max(0f, windupHoldDuration)
            );

            // 3. SNAP: Move from 180 to 0 degrees.
            SetStatus("SNAP!");

            yield return StartCoroutine(
                AnimateShell(
                    windupAngle,
                    0f,
                    closeDuration
                )
            );

            SetShellAngle(0f);

            // Check if the ball got caught.
            if (IsPlayerCaught())
            {
                TriggerGameOver();
                yield break;
            }

            // 4. CLOSED: Wait before reopening.
            SetStatus("CLAM CLOSED");
            ShowTreasure(false);

            yield return new WaitForSeconds(
                Mathf.Max(0f, closedDuration)
            );

            // 5. REOPEN: Return to the original
            // upright position, not 180 degrees.
            SetStatus("CLAM OPENING");

            yield return StartCoroutine(
                AnimateShell(
                    0f,
                    openAngle,
                    reopenDuration
                )
            );

            SetShellAngle(openAngle);
        }
    }


    IEnumerator AnimateShell(
        float startAngle,
        float endAngle,
        float duration)
    {
        duration = Mathf.Max(0.01f, duration);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(
                elapsed / duration
            );

            // Smooth animation.
            t = t * t * (3f - 2f * t);

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
        // Direct X rotation: no stored rotation offsets.
        topShellPivot.localRotation =
            Quaternion.Euler(angle, 0f, 0f);
    }

    bool IsPlayerCaught()
    {
        if (playerBody == null || captureZone == null)
            return false;

        Vector3 center =
            captureZone.transform.TransformPoint(
                captureZone.center
            );

        Vector3 scale =
            captureZone.transform.lossyScale;

        Vector3 halfSize = new Vector3(
            captureZone.size.x * Mathf.Abs(scale.x),
            captureZone.size.y * Mathf.Abs(scale.y),
            captureZone.size.z * Mathf.Abs(scale.z)
        ) * 0.5f;

        Physics.SyncTransforms();

        Collider[] hits = Physics.OverlapBox(
            center,
            halfSize,
            captureZone.transform.rotation,
            ~0,
            QueryTriggerInteraction.Ignore
        );

        foreach (Collider hit in hits)
        {
            if (hit.attachedRigidbody == playerBody)
            {
                Debug.Log("CLAM: Player caught!");
                return true;
            }
        }

        Debug.Log("CLAM: Player escaped!");
        return false;
    }



    void ShowTreasure(bool visible)
    {
        if (treasure == null)
            return;

        // Already successfully collected.
        if (treasure.IsHeld)
        {
            treasureTaken = true;
            return;
        }

        if (treasureTaken)
            return;

        ClamTreasureAnchor anchor =
            treasure.GetComponent<ClamTreasureAnchor>();

        // The coin was released from its anchor.
        // It now belongs to the normal physics system,
        // even if the magnet hasn't attached it yet.
        if (anchor != null && !anchor.enabled)
            return;

        // Only hide the coin while still anchored.
        treasure.gameObject.SetActive(visible);
    }



    void TriggerGameOver()
    {
        if (gameOver)
            return;

        gameOver = true;

        SetStatus("CLAMMED SHUT!");

        // Drop carried metal.
        if (playerMagnet != null)
        {
            playerMagnet.DropEverything();
            playerMagnet.enabled = false;
        }

        // Stop movement.
        if (playerMovement != null)
            playerMovement.enabled = false;

        // Freeze the ball without writing velocity
        // to an already-kinematic Rigidbody.
        if (playerBody != null)
        {
            if (!playerBody.isKinematic)
            {
                playerBody.linearVelocity = Vector3.zero;
                playerBody.angularVelocity = Vector3.zero;
            }

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

    void OnDrawGizmosSelected()
    {
        if (captureZone == null)
            return;

        Gizmos.color = Color.red;

        Matrix4x4 previousMatrix = Gizmos.matrix;

        Gizmos.matrix =
            captureZone.transform.localToWorldMatrix;

        Gizmos.DrawWireCube(
            captureZone.center,
            captureZone.size
        );

        Gizmos.matrix = previousMatrix;
    }
}
