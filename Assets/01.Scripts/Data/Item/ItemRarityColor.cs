using UnityEngine;

public static class ItemRarityColor
{
    public static string ToHex(ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Common => "#FFFFFF",
            ItemRarity.Rare => "#00FF00",
            ItemRarity.Unique => "#A020F0",
            ItemRarity.Epic => "#FFD700",
            ItemRarity.Legendary => "#FF0000",
            _ => "#FFFFFF"
        };
    }

    public static string Apply(ItemRarity rarity, string text)
    {
        return $"<color={ToHex(rarity)}>{text}</color>";
    }
}
