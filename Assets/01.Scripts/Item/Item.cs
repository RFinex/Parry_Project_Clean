public abstract class Item
{
    public ItemData Data { get; private set; }


    public virtual void Initialized(ItemData data)
    {
        Data = data;
    }
}

public class Artifact : Item
{
    public ArtifactData ArtifactItemData { get; private set; }
    public bool IsEquipped { get; private set; }

    public override void Initialized(ItemData data)
    {
        if (data is not ArtifactData artifactData)
            return;

        base.Initialized(data);
        ArtifactItemData = artifactData;
    }

    public virtual void OnEquip(PlayerBlackBoard board)
    {
        if (IsEquipped || board.stats == null || ArtifactItemData.StatPairs == null)
            return;

        IsEquipped = true;

        foreach (StatPair pair in ArtifactItemData.StatPairs)
            board.stats.AddStats(pair);
    }

    public virtual void OnUnequip(PlayerBlackBoard board)
    {
        if (!IsEquipped)
            return;

        IsEquipped = false;

        if (board == null)
            return;

        if (board.stats == null || ArtifactItemData.StatPairs == null)
            return;

        foreach (StatPair pair in ArtifactItemData.StatPairs)
            board.stats.RemoveStats(pair);
    }
}

public class Consumable : Item
{

}
