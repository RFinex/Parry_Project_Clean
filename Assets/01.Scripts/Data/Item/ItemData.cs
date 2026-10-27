using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum ItemRarity
{
    Common,
    Rare,
    Unique,
    Epic,
    Legendary
}

public class ItemData : ScriptableObject
{
    [SerializeField] private int id;
    [SerializeField] private string itemName;

    [TextArea]
    [SerializeField] private string description;

    [SerializeField] private ItemRarity rarity;
    [SerializeField] private string iconId;
    [SerializeField] private Sprite icon;

    [SerializeField] private List<StatPair> statPairs = new();

    public int Id => id;
    public string ItemName => itemName;

    public string Description => description;

    public ItemRarity Rarity => rarity;
    public string IconId => iconId;
    public Sprite Icon => icon;

    public IReadOnlyList<StatPair> StatPairs => statPairs;
}

[Serializable]
public struct StatPair
{
    public StatId id;

    public StatModifierType modifierType;

    public float value;
}

public enum StatModifierType
{
    Flat = 0,
    Percent = 1
}

public enum StatId
{
    AttackDamage,
    MaxHp,
    MaxStamina,
    MoveSpeed,
    ParryTime,
    ParryDamage,
    GuardStaminaCost,
    DashSpeed,
    DashCooldown,
    JumpForce,
    DashTime,
    AttackRange,
    InvincibleTime
}