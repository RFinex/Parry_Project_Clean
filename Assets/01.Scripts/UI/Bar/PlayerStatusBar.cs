using DG.Tweening;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatusBar : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private Image healthFill;
    [SerializeField] private Image healthDelayFill;
    [SerializeField] private float healthDelayDuration = 0.5f;
    [SerializeField] private TextMeshProUGUI healthText;

    [Header("Stamina")]
    [SerializeField] private Image staminaFill;

    private PlayerStats stats;
    private Tween healthDelayTween;

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

        healthText.SetText("{0:0}/{1:0}", currentHealth, maxHealth);

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
        if (GameManager.Instance == null)
            return;

        GameManager.Instance.OnPlayerChanged += HandlePlayerChanged;
        HandlePlayerChanged(GameManager.Instance.Player);
    }

    private void OnDisable()
    {
        if(GameManager.Instance != null )
            GameManager.Instance.OnPlayerChanged -= HandlePlayerChanged;

        UnBind();
    }
}
