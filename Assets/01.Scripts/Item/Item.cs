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

    protected PlayerBlackBoard blackBoard;

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

        blackBoard = board;
        IsEquipped = true;

        foreach (StatPair pair in ArtifactItemData.StatPairs)
            blackBoard.stats.AddStats(pair);
    }

    public virtual void OnUnequip(PlayerBlackBoard board)
    {
        if (!IsEquipped)
            return;

        PlayerBlackBoard targetBoard = board ?? blackBoard;

        IsEquipped = false;

        if (targetBoard == null || targetBoard.stats == null)
        {
            blackBoard = null;
            return;
        }

        if (ArtifactItemData.StatPairs == null)
        {
            blackBoard = null;
            return;
        }

        foreach (StatPair pair in ArtifactItemData.StatPairs)
            targetBoard.stats.RemoveStats(pair);

        blackBoard = null;
    }
}

public class Consumable : Item
{

}
