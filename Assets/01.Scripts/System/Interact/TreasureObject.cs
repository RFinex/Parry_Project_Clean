using UnityEngine;

public class TreasureObject : InteractableBase
{
    private bool isAvailable = true;
    public override bool CanInteract => isAvailable;

    public override void Interact(PlayerController interactor)
    {
        if (!CanInteract)
            return;

        EventBus.Publish(new TreasureSelectedEvent(this));
    }

    public void SetAvailable(bool isAvailable)
    {
        this.isAvailable = isAvailable;
        RefreshIcon();
    }
}
