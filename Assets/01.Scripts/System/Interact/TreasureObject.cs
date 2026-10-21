using UnityEngine;

public class TreasureObject : InteractableBase
{
    [SerializeField] private SpriteRenderer chestSprite;
    [SerializeField] private Animator chestAnimator;

    private int openAnimHash;

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

    public void SetupChest(Sprite closedSprite, string animName)
    {
        chestAnimator.enabled = false;
        chestSprite.sprite = closedSprite;
        openAnimHash = Animator.StringToHash(animName);

        Debug.Log($"렌더러 적용 결과: {chestSprite.sprite.texture.name} / {chestSprite.sprite.name}");

    }

    public void Open()
    {
        if (chestAnimator == null || openAnimHash == 0)
            return;

        chestAnimator.enabled = true;
        chestAnimator.Play(openAnimHash, 0, 0f);
    }
}
