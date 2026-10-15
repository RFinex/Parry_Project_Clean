using UnityEngine;

public class RoomExit : InteractableBase
{
    public bool isOpen;
    public bool hasInteracted;

    public override bool CanInteract => isOpen && !hasInteracted;

    private void OnEnable()
    {
        EventBus.Subscribe<RoomClearedEvent>(HandelRoomCleard);
    }

    private void OnDisable()
    {
        EventBus.UnSubscribe<RoomClearedEvent>(HandelRoomCleard);
    }

    private void HandelRoomCleard(RoomClearedEvent eventData)
    {
        Open();
    }

    private void Open()
    {
        if (isOpen)
            return;

        isOpen = true;

        RefreshIcon();
    }

    public override void Interact()
    {
        if (!CanInteract)
            return;

        hasInteracted = true;
        RefreshIcon();

        EventBus.Publish(new RoomExitInteractedEvent());
    }
}
