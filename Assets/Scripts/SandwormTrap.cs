
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(SphereCollider))]
public class SandwormTrap : MonoBehaviour
{
    [Header("Worm")]
    public Transform wormVisual;
    public Transform mouthPoint;
    public Terrain beachTerrain;

    [Header("Player")]
    public Rigidbody playerBody;
    public PlayerMovement playerMovement;
    public MagnetController magnet;
    public BeachSand beachSand;
    public LevelRunManager levelManager;

    [Header("Animation Timing")]
    public float warningDuration = 2.5f;
    public float riseDuration = 0.8f;
    public float holdAtTopDuration = 0.4f;
    public float sinkDuration = 1.4f;

    [Header("Animation Position")]
    public float hiddenDepth = 3f;
    public float riseHeight = 2.5f;
    public float mouthOffset = 1f;
    public float finalSinkDepth = 3f;

    [Header("UI")]
    public GameObject warningIndicator;
    public GameObject sandwormGameOverPanel;


    [Header("Sand Effects")]
    public ParticleSystem warningDust;
    public ParticleSystem burstDust;

    [Header("Warning Shake")]
    public float shakeStrength = 0.08f;
    public float shakeSpeed = 25f;


    private bool playerInside;
    private bool attackStarted;
    private Coroutine warningRoutine;

    void Start()
    {
        if (warningDust != null)
        {
            warningDust.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );
        }

        if (burstDust != null)
        {
            burstDust.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );
        }
        if (wormVisual != null)
            wormVisual.gameObject.SetActive(false);

        if (warningIndicator != null)
            warningIndicator.SetActive(false);

        if (sandwormGameOverPanel != null)
            sandwormGameOverPanel.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (playerBody == null ||
            other.attachedRigidbody != playerBody)
            return;

        if (attackStarted)
            return;

        playerInside = true;

        if (warningRoutine == null)
            warningRoutine = StartCoroutine(AttackSequence());
    }

    void OnTriggerExit(Collider other)
    {
        if (playerBody == null ||
            other.attachedRigidbody != playerBody)
            return;

        playerInside = false;
    }



    IEnumerator AttackSequence()
    {
        // Remember the warning indicator's original position.
        Vector3 originalPosition = Vector3.zero;

        if (warningIndicator != null)
        {
            originalPosition =
                warningIndicator.transform.localPosition;

            warningIndicator.SetActive(true);
        }

        // Start emitting warning dust.
        if (warningDust != null)
        {
            warningDust.Play();
        }

        float warningTimer = 0f;

        while (warningTimer < warningDuration)
        {
            warningTimer += Time.deltaTime;

            float progress = Mathf.Clamp01(
                warningTimer / Mathf.Max(0.01f, warningDuration)
            );

            // Shake harder as the warning approaches its end.
            float currentShake = shakeStrength * progress;

            float shakeX = Mathf.Sin(
                warningTimer * shakeSpeed
            ) * currentShake;

            float shakeZ = Mathf.Cos(
                warningTimer * shakeSpeed * 1.3f
            ) * currentShake;

            if (warningIndicator != null)
            {
                warningIndicator.transform.localPosition =
                    originalPosition +
                    new Vector3(shakeX, 0f, shakeZ);
            }

            // Escape cancels the attack.
            if (!playerInside)
                break;

            yield return null;
        }

        // Reset and hide the warning circle.
        if (warningIndicator != null)
        {
            warningIndicator.transform.localPosition =
                originalPosition;

            warningIndicator.SetActive(false);
        }

        // Stop creating warning particles,
        // but allow existing particles to fade naturally.
        if (warningDust != null)
        {
            warningDust.Stop(
                true,
                ParticleSystemStopBehavior.StopEmitting
            );
        }

        if (!playerInside || playerBody == null ||
            wormVisual == null || mouthPoint == null)
        {
            warningRoutine = null;
            yield break;
        }

        // KEEP the remaining sandworm attack code here.

        attackStarted = true;

        Vector3 playerStart = playerBody.position;

        float surfaceY = transform.position.y;

        if (beachTerrain != null)
        {
            surfaceY =
                beachTerrain.SampleHeight(playerStart) +
                beachTerrain.transform.position.y;
        }

        // The worm starts underneath the player.
        Vector3 wormBottom = new Vector3(
            playerStart.x,
            surfaceY - hiddenDepth,
            playerStart.z
        );

        // The worm rises out of the sand.
        Vector3 wormTop = new Vector3(
            playerStart.x,
            surfaceY + riseHeight,
            playerStart.z
        );

        // 2. Drop all collected metal.
        if (magnet != null)
        {
            magnet.DropEverything();
            magnet.magnetIsOn = false;
            magnet.enabled = false;
        }

        // Disable normal movement and other systems.
        if (playerMovement != null)
            playerMovement.enabled = false;

        if (beachSand != null)
            beachSand.enabled = false;

        if (levelManager != null)
            levelManager.enabled = false;

        playerBody.linearVelocity = Vector3.zero;
        playerBody.angularVelocity = Vector3.zero;

        playerBody.isKinematic = true;
        playerBody.detectCollisions = false;

        // Make the worm appear underground.
        wormVisual.gameObject.SetActive(true);
        wormVisual.position = wormBottom;


        // Create an explosion at the sand surface.
        if (burstDust != null)
        {
            burstDust.transform.position = new Vector3(
                playerStart.x,
                surfaceY + 0.15f,
                playerStart.z
            );

            burstDust.Play();
        }


        // 3. Worm rises, lifting the player.
        float timer = 0f;

        while (timer < riseDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(
                timer / Mathf.Max(0.01f, riseDuration)
            );

            // Smooth rise animation.
            t = t * t * (3f - 2f * t);

            wormVisual.position =
                Vector3.Lerp(wormBottom, wormTop, t);

            // The ball moves upward toward the mouth.
            playerBody.position = Vector3.Lerp(
                playerStart,
                mouthPoint.position,
                t
            );

            yield return null;
        }

        wormVisual.position = wormTop;
        playerBody.position = mouthPoint.position;

        // 4. Brief dramatic pause at the top.
        yield return new WaitForSeconds(holdAtTopDuration);

        // 5. Worm descends, carrying the player down.
        Vector3 wormEnd = new Vector3(
            playerStart.x,
            surfaceY - finalSinkDepth,
            playerStart.z
        );

        timer = 0f;

        while (timer < sinkDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(
                timer / Mathf.Max(0.01f, sinkDuration)
            );

            t = t * t * (3f - 2f * t);

            wormVisual.position =
                Vector3.Lerp(wormTop, wormEnd, t);

            // Keep the ball attached to the mouth position.
            playerBody.position = mouthPoint.position;

            yield return null;
        }

        wormVisual.position = wormEnd;
        playerBody.position = mouthPoint.position;

        // 6. Game Over!
        if (sandwormGameOverPanel != null)
            sandwormGameOverPanel.SetActive(true);

        Time.timeScale = 0f;
    }


    public void RestartLevel()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }
}
