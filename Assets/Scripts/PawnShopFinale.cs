
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PawnShopFinale : MonoBehaviour
{
    [Header("Player")]
    public Rigidbody playerBody;

    [Header("Treasure Rain")]
    public GameObject[] rainPrefabs;
    public int rainCount = 25;
    public float rainDuration = 3f;
    public float rainWidth = 3f;
    public float rainLength = 3f;
    public float spawnHeight = 5f;

    [Header("Final Board")]
    public GameObject finalBoard;
    public TMP_Text totalMoneyText;
    public TMP_Text oceanText;
    public TMP_Text beachText;
    public TMP_Text totalItemsText;
    public TMP_Text totalTimeText;

    private bool finaleStarted;

    void Start()
    {
        if (finalBoard != null)
            finalBoard.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (finaleStarted || playerBody == null)
            return;

        if (other.attachedRigidbody != playerBody)
            return;

        finaleStarted = true;
        StartCoroutine(PlayFinale());
    }

    IEnumerator PlayFinale()
    {
        // IMPORTANT:
        // Do not disable PlayerMovement.
        // Do not freeze the Rigidbody.
        // The player can keep rolling!

        if (rainPrefabs != null &&
            rainPrefabs.Length > 0)
        {
            float delay = rainDuration /
                Mathf.Max(1, rainCount);

            for (int i = 0; i < rainCount; i++)
            {
                GameObject prefab =
                    rainPrefabs[
                        Random.Range(0, rainPrefabs.Length)
                    ];

                if (prefab != null)
                {
                    // Read the player's CURRENT position
                    // for every falling object.
                    Vector3 center = playerBody.position;

                    Vector3 spawnPosition =
                        center + new Vector3(
                            Random.Range(
                                -rainWidth / 2f,
                                rainWidth / 2f
                            ),
                            spawnHeight,
                            Random.Range(
                                -rainLength / 2f,
                                rainLength / 2f
                            )
                        );

                    Instantiate(
                        prefab,
                        spawnPosition,
                        Random.rotation
                    );
                }

                yield return new WaitForSeconds(delay);
            }
        }

        // Give the last objects time to fall.
        yield return new WaitForSeconds(1f);

        UpdateFinalBoard();

        if (finalBoard != null)
            finalBoard.SetActive(true);

        // No Time.timeScale = 0!
        // No Rigidbody freezing!
    }

    void UpdateFinalBoard()
    {
        int oceanMoney =
            FinalRunStats.GetMoney("Ocean");

        int beachMoney =
            FinalRunStats.GetMoney("Beach");

        int oceanItems =
            FinalRunStats.GetItems("Ocean");

        int beachItems =
            FinalRunStats.GetItems("Beach");

        float oceanTime =
            FinalRunStats.GetTime("Ocean");

        float beachTime =
            FinalRunStats.GetTime("Beach");

        int totalMoney = oceanMoney + beachMoney;
        int totalItems = oceanItems + beachItems;

        if (totalMoneyText != null)
            totalMoneyText.text = "$" + totalMoney;

        if (oceanText != null)
        {
            oceanText.text =
                "OCEAN\n" +
                "Money: $" + oceanMoney + "\n" +
                "Items banked: " + oceanItems + "\n" +
                "Time: " + FormatTime(oceanTime);
        }

        if (beachText != null)
        {
            beachText.text =
                "BEACH\n" +
                "Money: $" + beachMoney + "\n" +
                "Items banked: " + beachItems + "\n" +
                "Time: " + FormatTime(beachTime);
        }

        if (totalItemsText != null)
            totalItemsText.text =
                "Total items banked: " + totalItems;

        if (totalTimeText != null)
            totalTimeText.text =
                "Combined time: " +
                FormatTime(oceanTime + beachTime);
    }

    string FormatTime(float seconds)
    {
        int total = Mathf.FloorToInt(
            Mathf.Max(0f, seconds)
        );

        return string.Format(
            "{0:00}:{1:00}",
            total / 60,
            total % 60
        );
    }


    public void PlayAgain()
    {
        // Restore normal game speed.
        Time.timeScale = 1f;

        // Clear the previous Ocean and Beach results.
        FinalRunStats.ClearRun();

        // Return to the first level.
        SceneManager.LoadScene("Ocean");
    }

}
