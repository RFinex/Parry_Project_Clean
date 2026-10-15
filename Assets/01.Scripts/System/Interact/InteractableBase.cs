using UnityEngine;

public abstract class InteractableBase : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject interactIcon;

    private bool isTrigger;

    public abstract bool CanInteract { get; }

    protected virtual void Awake()
    {
        if (interactIcon != null)
            interactIcon.SetActive(false);
    }

    public abstract void Interact(PlayerController interactor);

    public void SetTrigger(bool isTrigger)
    {
        if (this.isTrigger == isTrigger)
            return;

        this.isTrigger = isTrigger;
        RefreshIcon();
    }

    protected void RefreshIcon()
    {
        if (interactIcon == null)
            return;

        bool isShow = isTrigger && CanInteract;

        if (interactIcon.activeSelf != isShow)
            interactIcon.SetActive(isShow);
    }
}
