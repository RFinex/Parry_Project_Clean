using UnityEngine;

public class RecoveryObject : InteractableBase
{
    [SerializeField] private float recoveryAmount;

    private bool isAvailable = true;

    public override bool CanInteract => isAvailable;

    public override void Interact(PlayerController interactor)
    {
        if (!CanInteract || interactor == null)
            return;

        interactor.Stats.RestoreHp(recoveryAmount);

        isAvailable = false;
        RefreshIcon();

        EventBus.Publish(new RecoveryObjectUsedEvent(this));
    }

    public void SetAvailable(bool isAvailable)
    {
        this.isAvailable = isAvailable;
        RefreshIcon();
    }
}
