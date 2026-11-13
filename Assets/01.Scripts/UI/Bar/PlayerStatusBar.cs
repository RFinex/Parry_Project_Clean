using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatusBar : MonoBehaviour
{
    [Header("Rect Transform")]
    [SerializeField] private RectTransform staminaRect;

    [Header("Health")]
    [SerializeField] private Image healthFill;
    [SerializeField] private Image healthDelayFill;
    [SerializeField] private float healthDelayDuration = 0.5f;
    [SerializeField] private TextMeshProUGUI healthText;

    [Header("Stamina")]
    [SerializeField] private Image staminaFill;

    [Header("Stamina Broken")]
    [SerializeField] private float staminaShakeDuration = 0.25f;
    [SerializeField] private Vector2 staminaShakePower = new(8f, 0f);
    [SerializeField] private int shakeVibrato = 20;
    [SerializeField] private float shakeRandomness = 90f;
    [SerializeField] private bool shakeFadeOut = true;
    [SerializeField] private Color staminaBrokenColor = Color.red;
    [SerializeField] private float staminaBrokenColorDuration = 0.2f;

    private Color staminaOriginColor;

    private PlayerStats stats;
    private Tween healthDelayTween;

    private Sequence staminaBrokenSeq;

    private void Awake()
    {
        staminaOriginColor = staminaFill.color;
    }

    private void UnBind()
    {
        if (stats != null)
        {
            stats.OnHealthChanged -= SetHealth;
            stats.OnStaminaChanged -= SetStamina;
            stats.OnStaminaBroken -= HandleStaminaBroken;
            stats.OnStaminaRestore -= HandleStaminaRestore;
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
        stats.OnHealthChanged -= SetHealth;
        stats.OnStaminaChanged -= SetStamina;
        stats.OnHealthChanged += SetHealth;
        stats.OnStaminaChanged += SetStamina;

        stats.OnStaminaBroken -= HandleStaminaBroken;
        stats.OnStaminaRestore -= HandleStaminaRestore;
        stats.OnStaminaBroken += HandleStaminaBroken;
        stats.OnStaminaRestore += HandleStaminaRestore;

        SetHealth(stats.NowHp, stats.MaxHp);
        SetStamina(stats.NowStamina, stats.MaxStamina);
    }

    /// <summary>
    /// 스테미너 이미지 흔들림 및 색 변경
    /// </summary>
    private void HandleStaminaBroken()
    {
        staminaBrokenSeq?.Kill();

        staminaRect.DOShakeAnchorPos(staminaShakeDuration, staminaShakePower, shakeVibrato, shakeRandomness, false, shakeFadeOut)
            .SetLink(gameObject);

        staminaBrokenSeq = DOTween.Sequence();

        staminaBrokenSeq.Append(staminaFill.DOColor(staminaBrokenColor, staminaBrokenColorDuration))
            .Append(staminaFill.DOColor(staminaOriginColor, staminaBrokenColorDuration))
            .SetLoops(-1)
            .SetLink(gameObject);
    }

    /// <summary>
    /// 스테미너 이미지 원복
    /// </summary>
    private void HandleStaminaRestore()
    {
        staminaBrokenSeq?.Kill();
        staminaBrokenSeq = null;
        staminaFill.color = staminaOriginColor;
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

        if(staminaOriginColor != null)
            staminaFill.color = staminaOriginColor;

        UnBind();
    }
}
