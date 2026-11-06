
using UnityEngine;

public static class PawnShopLootLog
{
    public static void RecordItem(LootKind kind)
    {
        string key = "PawnLoot_" + kind;

        int current = PlayerPrefs.GetInt(key, 0);

        PlayerPrefs.SetInt(key, current + 1);
        PlayerPrefs.Save();
    }

    public static int GetCount(LootKind kind)
    {
        return PlayerPrefs.GetInt(
            "PawnLoot_" + kind, 0
        );
    }

    public static void Clear()
    {
        foreach (LootKind kind in
                 System.Enum.GetValues(typeof(LootKind)))
        {
            PlayerPrefs.DeleteKey("PawnLoot_" + kind);
        }

        PlayerPrefs.Save();
    }
}
