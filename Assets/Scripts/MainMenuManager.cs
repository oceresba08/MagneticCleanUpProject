
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public GameObject instructionsPanel;

    public void StartGame()
    {
        Time.timeScale = 1f;

        // Start with a fresh run.
        FinalRunStats.ClearRun();
        PawnShopLootLog.Clear();

        SceneManager.LoadScene("Ocean");
    }

    public void ShowInstructions()
    {
        if (instructionsPanel != null)
            instructionsPanel.SetActive(true);
    }

    public void HideInstructions()
    {
        if (instructionsPanel != null)
            instructionsPanel.SetActive(false);
    }
}
