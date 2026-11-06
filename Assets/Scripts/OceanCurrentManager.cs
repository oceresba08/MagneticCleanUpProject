
using UnityEngine;
using TMPro;

public class OceanCurrentManager : MonoBehaviour
{
    public enum CurrentDirection
    {
        North,
        South,
        East,
        West
    }

    [Header("Player")]
    public Rigidbody playerBody;

    [Header("Current Settings")]
    public float currentStrength = 5f;
    public float changeInterval = 8f;
    public float warningTime = 2f;

    [Header("UI")]
    public TMP_Text directionText;
    public TMP_Text timerText;
    public TMP_Text warningText;

    private CurrentDirection currentDirection;
    private CurrentDirection nextDirection;

    private Vector3 currentForce;
    private float changeTimer;

    void Start()
    {
        changeInterval = Mathf.Max(0.1f, changeInterval);
        warningTime = Mathf.Clamp(
            warningTime, 0f, changeInterval
        );

        changeTimer = changeInterval;

        currentDirection = (CurrentDirection)
            Random.Range(0, 4);

        ChooseNextDirection();
        UpdateCurrentForce();
        UpdateUI();
    }

    void Update()
    {
        changeTimer -= Time.deltaTime;

        if (changeTimer <= 0f)
        {
            ChangeCurrent();
        }

        UpdateUI();
    }

    void FixedUpdate()
    {
        if (playerBody == null || playerBody.isKinematic)
            return;

        // Apply current continuously across the level.
        playerBody.AddForce(
            currentForce * currentStrength,
            ForceMode.Acceleration
        );
    }

    void ChangeCurrent()
    {
        currentDirection = nextDirection;

        ChooseNextDirection();
        UpdateCurrentForce();

        changeTimer = changeInterval;

        Debug.Log(
            "Ocean current changed: " +
            currentDirection
        );
    }

    void ChooseNextDirection()
    {
        // Don't repeat the same direction.
        do
        {
            nextDirection = (CurrentDirection)
                Random.Range(0, 4);
        }
        while (nextDirection == currentDirection);
    }

    void UpdateCurrentForce()
    {
        switch (currentDirection)
        {
            case CurrentDirection.North:
                currentForce = Vector3.forward;
                break;

            case CurrentDirection.South:
                currentForce = Vector3.back;
                break;

            case CurrentDirection.East:
                currentForce = Vector3.right;
                break;

            case CurrentDirection.West:
                currentForce = Vector3.left;
                break;
        }
    }

    void UpdateUI()
    {
        if (directionText != null)
        {
            string arrow = "";

            switch (currentDirection)
            {
                case CurrentDirection.North:
                    arrow = "↑";
                    break;
                case CurrentDirection.South:
                    arrow = "↓";
                    break;
                case CurrentDirection.East:
                    arrow = "→";
                    break;
                case CurrentDirection.West:
                    arrow = "←";
                    break;
            }

            directionText.text =
                "CURRENT: " + arrow + " " +
                currentDirection.ToString().ToUpper();
        }

        if (timerText != null)
        {
            timerText.text =
                "Next shift: " +
                Mathf.CeilToInt(
                    Mathf.Max(0f, changeTimer)
                ) + "s";
        }

        if (warningText != null)
        {
            if (changeTimer <= warningTime)
            {
                warningText.gameObject.SetActive(true);

                warningText.text =
                    "CURRENT SHIFTING TO " +
                    nextDirection.ToString().ToUpper() +
                    "!";
            }
            else
            {
                warningText.gameObject.SetActive(false);
            }
        }
    }
}
