using UnityEngine;

public class RecoveryObject : InteractableBase
{
    [SerializeField] private ParticleSystem particle;
    [SerializeField] private float recoveryAmount;

    private bool isAvailable = true;

    public override bool CanInteract => isAvailable;

    public override void Interact(PlayerController interactor)
    {
        if (!CanInteract || interactor == null)
            return;

        interactor.Stats.RestoreHp(recoveryAmount);

        if (particle != null)
            particle.Stop();

        SoundManager.Instance.PlaySFX(SFXType.Heal);


        isAvailable = false;
        RefreshIcon();

        EventBus.Publish(new RecoveryObjectUsedEvent(this));
    }

    public void SetAvailable(bool isAvailable)
    {
        this.isAvailable = isAvailable;

        if (particle != null)
        {
            if (isAvailable)
                particle.Play();
            else
                particle.Stop();
        }

        RefreshIcon();
    }
}
