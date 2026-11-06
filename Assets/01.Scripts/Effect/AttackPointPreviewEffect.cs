using DG.Tweening;
using UnityEngine;

public class AttackPointPreviewEffect : EffectBase
{
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private float fadeInDuration = 0.08f;
    [SerializeField] private float holdDuration = 0.04f;
    [SerializeField] private float fadeOutDuration = 0.12f;

    private Vector3 originScale;

    private Sequence fadeSequence;

    private void Awake()
    {
        originScale = transform.localScale;
    }

    public override void InitPool()
    {
        base.InitPool();

        if (sr == null)
        {
            ReturnPool();
            return;
        }

        fadeSequence?.Kill();

        Color color = sr.color;
        color.a = 0f;
        sr.color = color;

        fadeSequence = DOTween.Sequence()
            .Append(sr.DOFade(1f, fadeInDuration))
            .AppendInterval(holdDuration)
            .Append(sr.DOFade(0f, fadeOutDuration))
            .SetLink(gameObject)
            .OnComplete(ReturnPool);
    }

    public override void InitPoolReturn()
    {
        fadeSequence?.Kill();
        fadeSequence = null;

        transform.localScale = originScale;

        base.InitPoolReturn();
    }
}
