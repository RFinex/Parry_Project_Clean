using UnityEngine;

public abstract class Item
{
    public ItemData Data { get; }

    protected Item(ItemData data)
    {
        Data = data;
    }
}

public abstract class Relic : Item
{
    protected Relic(RelicData data) : base(data)
    {

    }
}

public abstract class Consumable : Item
{
    protected Consumable(ConsumableData data) : base(data)
    {

    }
}