
using UnityEngine;

public static class FinalRunStats
{
    public static void SaveLevel(
        string level,
        int money,
        int items,
        int score,
        float seconds)
    {
        PlayerPrefs.SetInt(level + "_Money", money);
        PlayerPrefs.SetInt(level + "_Items", items);
        PlayerPrefs.SetInt(level + "_Score", score);
        PlayerPrefs.SetFloat(level + "_Time", seconds);

        PlayerPrefs.Save();
    }

    public static int GetMoney(string level)
    {
        return PlayerPrefs.GetInt(level + "_Money", 0);
    }

    public static int GetItems(string level)
    {
        return PlayerPrefs.GetInt(level + "_Items", 0);
    }

    public static int GetScore(string level)
    {
        return PlayerPrefs.GetInt(level + "_Score", 0);
    }

    public static float GetTime(string level)
    {
        return PlayerPrefs.GetFloat(level + "_Time", 0f);
    }

    public static void ClearRun()
    {
        foreach (string level in new[] { "Ocean", "Beach" })
        {
            PlayerPrefs.DeleteKey(level + "_Money");
            PlayerPrefs.DeleteKey(level + "_Items");
            PlayerPrefs.DeleteKey(level + "_Score");
            PlayerPrefs.DeleteKey(level + "_Time");
        }

        PlayerPrefs.Save();
    }
}
