using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using DG.Tweening;

public class ChargeWarning : EffectBase
{
    [SerializeField] private Transform fill;

    private Sequence seq;

    public override void InitPool()
    {
        base.InitPool();
        ResetFill();
    }

    public override void InitPoolReturn()
    {
        seq?.Kill();

        seq = null;
        ResetFill();
    }

    public async UniTask FillAsync(float duration, float dir, CancellationToken ctk)
    {
        if (fill == null)
        {
            if (duration > 0f)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: ctk);
            }

            return;
        }

        Vector3 scale = fill.localScale;
        scale.x = 0f;
        fill.localScale = scale;

        Vector3 pos = fill.localPosition;
        pos.x = dir >= 0f ? -0.5f : 0.5f;
        fill.localPosition = pos;

        if (duration <= 0f)
        {
            scale.x = 1f;
            fill.localScale = scale;
            fill.localPosition = Vector3.zero;
            return;
        }

        seq = DOTween.Sequence();

        seq = seq.Join(
            fill.DOScaleX(1f, duration)
            .SetEase(Ease.Linear));

        seq = seq.Join(
            fill.DOLocalMoveX(0f, duration)
            .SetEase(Ease.Linear));

        await seq.ToUniTask(TweenCancelBehaviour.Kill, ctk);
    }

    private void ResetFill()
    {
        if (fill == null)
            return;

        fill.localScale = Vector3.one;
        fill.localPosition = Vector3.zero;
    }
}
