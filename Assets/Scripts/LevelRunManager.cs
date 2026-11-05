
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelRunManager : MonoBehaviour
{
    [Header("Player")]
    public MagnetController magnet;

    [Header("Timer")]
    public float timeLimit = 120f;
    public TMP_Text timerText;
    public TMP_Text bankedText;
    public TMP_Text zonePromptText;

    [Header("Results Panel")]
    public GameObject resultsPanel;
    public TMP_Text resultTitleText;
    public TMP_Text resultValueText;
    public TMP_Text resultItemsText;
    public TMP_Text resultScoreText;
    public TMP_Text resultTimeText;
    public TMP_Text bestTimeText;

    private bool started = false;
    private bool finished = false;
    private bool inStartZone = true;
    private bool inCollectorZone = false;

    private float elapsedTime = 0f;
    private string bestTimeKey;

    void Start()
    {
        Time.timeScale = 1f;

        bestTimeKey = "BestTime_" +
            SceneManager.GetActiveScene().name;

        if (resultsPanel != null)
            resultsPanel.SetActive(false);

        UpdateHUD();
    }

    void Update()
    {
        if (finished)
            return;

        if (started)
        {
            elapsedTime += Time.deltaTime;

            if (elapsedTime >= timeLimit)
            {
                elapsedTime = timeLimit;
                FinishLevel(false);
                return;
            }
        }

        if (inCollectorZone && started)
        {
            // Deposit without ending the level.
            if (Input.GetKeyDown(KeyCode.G))
                DepositCargo();

            // End the level voluntarily.
            if (Input.GetKeyDown(KeyCode.F))
            {
                FinishLevel(true);
                return;
            }
        }

        UpdateHUD();
    }

    public void SetZoneState(
        LevelZone.ZoneType type,
        bool inside)
    {
        if (finished)
            return;

        if (type == LevelZone.ZoneType.Start)
        {
            inStartZone = inside;

            // Leaving START begins the countdown.
            if (!inside && !started)
            {
                started = true;
                elapsedTime = 0f;
                Debug.Log("Level started!");
            }
        }
        else if (type == LevelZone.ZoneType.Collector)
        {
            inCollectorZone = inside;
        }
    }

    void DepositCargo()
    {
        if (magnet == null)
            return;

        int count = magnet.BankCargo();

        if (count > 0)
            Debug.Log("Cargo deposited!");

        UpdateHUD();
    }

    void UpdateHUD()
    {
        if (timerText != null)
        {
            float remaining = started
                ? Mathf.Max(0f, timeLimit - elapsedTime)
                : timeLimit;

            timerText.text = FormatTime(remaining);
        }

        if (magnet != null && bankedText != null)
        {
            bankedText.text =
                "Banked: $" + magnet.BankedValue;
        }

        if (zonePromptText != null)
        {
            if (!started)
            {
                zonePromptText.text =
                    "Leave START to begin!";
            }
            else if (inCollectorZone)
            {
                zonePromptText.text =
                    "G: Deposit  |  F: Finish";
            }
            else
            {
                zonePromptText.text =
                    "Return to the collector to bank cargo";
            }
        }
    }

    void FinishLevel(bool manualFinish)
    {
        if (finished)
            return;

        finished = true;

        // If inside the collector zone, count
        // any metal still attached to the player.
        if (inCollectorZone && magnet != null)
            magnet.BankCargo();

        // Save best time only for manual completion.
        if (manualFinish)
        {
            float previousBest =
                PlayerPrefs.GetFloat(bestTimeKey, -1f);

            if (previousBest < 0f ||
                elapsedTime < previousBest)
            {
                PlayerPrefs.SetFloat(
                    bestTimeKey,
                    elapsedTime
                );

                PlayerPrefs.Save();
            }
        }

        ShowResults(manualFinish);

        // Freeze gameplay while keeping UI buttons usable.
        Time.timeScale = 0f;
    }

    void ShowResults(bool manualFinish)
    {
        if (resultsPanel != null)
            resultsPanel.SetActive(true);

        if (resultTitleText != null)
        {
            resultTitleText.text =
                manualFinish
                ? "SCRAP-TASTIC!"
                : "TIME'S UP!";
        }

        int value = magnet != null
            ? magnet.BankedValue : 0;

        int items = magnet != null
            ? magnet.BankedCount : 0;

        int score = magnet != null
            ? magnet.Score : 0;

        if (resultValueText != null)
            resultValueText.text = "$" + value;

        if (resultItemsText != null)
            resultItemsText.text =
                "Items banked: " + items;

        if (resultScoreText != null)
            resultScoreText.text =
                "Discovery score: " + score;

        if (resultTimeText != null)
            resultTimeText.text =
                "Your time: " + FormatTime(elapsedTime);

        if (bestTimeText != null)
        {
            float best =
                PlayerPrefs.GetFloat(bestTimeKey, -1f);

            bestTimeText.text = best < 0f
                ? "Best time: --:--"
                : "Best time: " + FormatTime(best);
        }

        UpdateHUD();
    }

    string FormatTime(float seconds)
    {
        int total = Mathf.CeilToInt(
            Mathf.Max(0f, seconds)
        );

        int minutes = total / 60;
        int secs = total % 60;

        return minutes.ToString("00") +
            ":" + secs.ToString("00");
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    public void NextLevel()
    {
        Time.timeScale = 1f;

        int next =
            SceneManager.GetActiveScene().buildIndex + 1;

        if (next < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(next);
        else
            Debug.Log("No next scene configured.");
    }
}
