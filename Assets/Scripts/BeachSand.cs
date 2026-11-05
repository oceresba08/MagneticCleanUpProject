
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

[RequireComponent(typeof(Rigidbody))]
public class BeachSand : MonoBehaviour
{
    [Header("Sand Movement")]
    public float sandMoveMultiplier = 0.65f;
    public float sandResistance = 2.5f;

    [Header("Sinking")]
    public float idleDelay = 1f;
    public float sinkingSpeed = 0.18f;
    public float weightSinkBonus = 0.012f;
    public float recoverySpeed = 0.3f;
    public float stillSpeedThreshold = 0.4f;

    [Header("Sinking Visual")]
    public Transform ballVisual;
    public float visualSinkDepth = 0.75f;

    [Header("UI")]
    public Image sinkingBarFill;
    public TMP_Text sinkingText;
    public GameObject gameOverPanel;

    private Rigidbody rb;
    private PlayerMovement movement;
    private MagnetController magnet;

    private float idleTimer;
    private float sinkAmount;
    private bool gameOver;

    public float SinkAmount => sinkAmount;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        movement = GetComponent<PlayerMovement>();
        magnet = GetComponent<MagnetController>();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (movement != null)
            movement.environmentMoveMultiplier =
                sandMoveMultiplier;
    }

    void FixedUpdate()
    {
        if (gameOver)
            return;

        // Measure horizontal movement.
        Vector3 horizontalVelocity = new Vector3(
            rb.linearVelocity.x,
            0f,
            rb.linearVelocity.z
        );

        float speed = horizontalVelocity.magnitude;

        // Sand resists movement.
        rb.AddForce(
            -horizontalVelocity * sandResistance,
            ForceMode.Acceleration
        );

        // Detect whether movement keys are pressed.
        float horizontalInput =
            Input.GetAxisRaw("Horizontal");

        float verticalInput =
            Input.GetAxisRaw("Vertical");

        bool pressingMovement =
            Mathf.Abs(horizontalInput) > 0.1f ||
            Mathf.Abs(verticalInput) > 0.1f;

        bool standingStill =
            !pressingMovement &&
            speed < stillSpeedThreshold;

        if (standingStill)
        {
            idleTimer += Time.fixedDeltaTime;

            if (idleTimer >= idleDelay)
            {
                float weight = 0f;

                if (magnet != null)
                    weight = magnet.TotalWeight;

                float actualSinkSpeed =
                    sinkingSpeed +
                    weight * weightSinkBonus;

                sinkAmount +=
                    actualSinkSpeed * Time.fixedDeltaTime;
            }
        }
        else
        {
            idleTimer = 0f;

            // Moving helps the player escape.
            sinkAmount -=
                recoverySpeed * Time.fixedDeltaTime;
        }

        sinkAmount = Mathf.Clamp01(sinkAmount);

        UpdateSinkingVisual();
        UpdateSinkingUI();

        if (sinkAmount >= 1f)
        {
            TriggerGameOver();
        }
    }

    void UpdateSinkingVisual()
    {
        if (ballVisual == null)
            return;

        // Move the visible sphere down in WORLD
        // space, even when the ball is rotated.
        Vector3 worldOffset =
            Vector3.down * visualSinkDepth * sinkAmount;

        ballVisual.localPosition =
            transform.InverseTransformVector(worldOffset);
    }

    void UpdateSinkingUI()
    {
        if (sinkingBarFill != null)
            sinkingBarFill.fillAmount = sinkAmount;

        if (sinkingText != null)
        {
            int percent =
                Mathf.RoundToInt(sinkAmount * 100f);

            sinkingText.text =
                "Sinking: " + percent + "%";
        }
    }

    void TriggerGameOver()
    {
        gameOver = true;

        if (movement != null)
            movement.enabled = false;

        if (magnet != null)
            magnet.enabled = false;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.constraints = RigidbodyConstraints.FreezeAll;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Debug.Log("GAME OVER: The ball sank!");
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }
}
