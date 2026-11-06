
using UnityEngine;

public enum LootKind
{
    Coin,
    TinCan,
    ClamTreasure
}

public class PawnShopLootTag : MonoBehaviour
{
    public LootKind kind;
}
