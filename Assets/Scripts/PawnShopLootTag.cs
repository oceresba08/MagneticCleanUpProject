
using UnityEngine;

public enum LootKind
{
    Coin,
    TinCan,
    ClamTreasure,
    SpamCan,
    TallCan,
    SpamCanBox,
    SpamCanRound
}

public class PawnShopLootTag : MonoBehaviour
{
    public LootKind kind;
}
