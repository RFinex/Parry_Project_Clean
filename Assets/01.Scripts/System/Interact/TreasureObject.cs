using UnityEngine;

public class TreasureObject : InteractableBase
{
    [Header("Reward")]
    [SerializeField] private TreasureChestData chestData;

    [Header("Animation")]
    [SerializeField] private Animator chestAnimator;
    [SerializeField] private string openAnimName = "Open";

    [Header("Item Drop Position")]
    [SerializeField] private Transform dropTransform;
    public Vector3 DropPos => dropTransform != null ? dropTransform.position : transform.position;

    private int openAnimHash;

    private bool isAvailable = true;

    public TreasureChestData ChestData => chestData;


    public override bool CanInteract => isAvailable && chestData != null;

    protected override void Awake()
    {
        base.Awake();

        openAnimHash = string.IsNullOrEmpty(openAnimName) ? 0 : Animator.StringToHash(openAnimName);

        if (chestAnimator != null)
            chestAnimator.enabled = false;
    }

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

    public void Open()
    {
        if (chestAnimator == null || openAnimHash == 0)
            return;

        chestAnimator.enabled = true;
        chestAnimator.Play(openAnimHash, 0, 0f);
    }
}
