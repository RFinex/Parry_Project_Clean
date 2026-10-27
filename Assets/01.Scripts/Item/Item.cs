using UnityEngine;

public abstract class Item
{
    public ItemData Data { get; }

    public Item(ItemData data)
    {
        Data = data;
    }
}

public class Artifact : Item
{
    public ArtifactData ArtifactData { get; }

    public Artifact(ArtifactData data) : base(data)
    {
        ArtifactData = data;
    }
}

public class Consumable : Item
{
    public Consumable(ConsumableData data) : base(data)
    {

    }
}