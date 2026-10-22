using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatusBar : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private Image healthFill;
    [SerializeField] private Image healthDelayFill;
    [SerializeField] private float healthDelayDuration = 0.5f;

    [Header("Stamina")]
    [SerializeField] private Image staminaFill;

    private PlayerStats stats;
    private Tween healthDelayTween;

    private GameManager gameManager;

    //private void Bind(PlayerStats stats)
    //{
    //    this.stats = stats;
    //    stats.OnHealthChanged += SetHealth;
    //    stats.OnStaminaChanged += SetStamina;

    //    SetHealth(stats.NowHp, stats.MaxHp);
    //    SetStamina(stats.NowStamina, stats.MaxStamina);
    //}

    private void UnBind()
    {
        if (stats != null)
        {
            stats.OnHealthChanged -= SetHealth;
            stats.OnStaminaChanged -= SetStamina;
            stats = null;
        }

        healthDelayTween?.Kill();
        healthDelayTween = null;
    }

    private void SetHealth(float currentHealth, float maxHealth)
    {
        float ratio = maxHealth > 0f ? Mathf.Clamp01(currentHealth / maxHealth) : 0f;

        healthFill.fillAmount = ratio;

        healthDelayTween?.Kill();
        healthDelayTween = healthDelayFill.DOFillAmount(ratio, healthDelayDuration)
            .SetEase(Ease.OutQuad)
            .SetLink(gameObject);
    }

    private void SetStamina(float currentStamina, float maxStamina)
    {
        staminaFill.fillAmount = maxStamina > 0f ? Mathf.Clamp01(currentStamina / maxStamina) : 0f;
    }

    private void HandlePlayerChanged(PlayerController player)
    {
        PlayerStats newStats = player != null ? player.Stats : null;

        if (stats == newStats)
            return;

        UnBind();

        if (newStats == null)
            return;

        stats = newStats;
        stats.OnHealthChanged += SetHealth;
        stats.OnStaminaChanged += SetStamina;

        SetHealth(stats.NowHp, stats.MaxHp);
        SetStamina(stats.NowStamina, stats.MaxStamina);
    }

    private void OnEnable()
    {
        gameManager = GameManager.Instance;

        if (gameManager == null)
            return;

        gameManager.OnPlayerChanged += HandlePlayerChanged;
        HandlePlayerChanged(gameManager.Player);
    }

    private void OnDisable()
    {
        if(gameManager != null )
            gameManager.OnPlayerChanged -= HandlePlayerChanged;

        UnBind();
    }
}
