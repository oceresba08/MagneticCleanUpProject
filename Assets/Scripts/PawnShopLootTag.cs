
using UnityEngine;

public enum LootKind
{
    Coin,
    TinCan,
    Gear
}

public class PawnShopLootTag : MonoBehaviour
{
    public LootKind kind;
}
