using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonEffector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private Button button;

    private Vector3 originScale;
    [SerializeField] private Vector2 tweenSize = new(1.1f, 1.1f);
    [SerializeField] private float tweenTime = 0.1f;

    private void Awake()
    {
        button = GetComponent<Button>();
        originScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!button.interactable)
            return;

        SoundManager.Instance.PlaySFX(SFXType.MouseOnButton);

        transform.DOScale(tweenSize, tweenTime)
            .SetEase(Ease.Linear)
            .SetUpdate(true)
            .SetLink(gameObject, LinkBehaviour.KillOnDisable);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!button.interactable)
            return;

        transform.DOScale(originScale, tweenTime)
            .SetEase(Ease.Linear)
            .SetUpdate(true)
            .SetLink(gameObject, LinkBehaviour.KillOnDisable);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!button.interactable)
            return;

        SoundManager.Instance.PlaySFX(SFXType.ClickButton);
    }
}
