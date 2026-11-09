using UnityEngine;

public class RoomExitObject : InteractableBase
{
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Sprite openDoorSprite;
    [SerializeField] private Sprite closedDoorSprite;


    public bool isOpen;

    public override bool CanInteract => isOpen;

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

        sr.sprite = openDoorSprite;
        RefreshIcon();
    }

    public override void Interact(PlayerController interactor)
    {
        if (!CanInteract)
            return;

        EventBus.Publish(new RoomExitInteractedEvent());
    }
}
