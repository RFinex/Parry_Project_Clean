using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public enum EnemyAnimationType
{
    Idle,
    Move,
    Attack,
    Hurt,
    Jump,
    Death,
    Stagger
}

[Serializable]
public struct EnemyAnimationInfo
{
    public EnemyAnimationType type;
    public string stateName;
    public float duration;
}

public class EnemyAnimator
{
    private readonly Animator animator;
    private readonly Dictionary<EnemyAnimationType, int> animHashDic = new();

    public EnemyAnimator(Animator animator, EnemyAnimationInfo[] infos)
    {
        this.animator = animator;

        foreach (EnemyAnimationInfo info in infos)
        {
            if (!string.IsNullOrWhiteSpace(info.stateName))
            {
                animHashDic[info.type] = Animator.StringToHash(info.stateName);
            }
        }
    }

    // 나중에 최적화 필요
    public bool Play(EnemyAnimationType type)
    {
        if (animator == null || !animHashDic.TryGetValue(type, out int hash) || !animator.HasState(0, hash))
            return false;

        AnimatorStateInfo currentAnim = animator.GetCurrentAnimatorStateInfo(0);

        // 이미 재생중인 애니메이션이면 재생X
        if (currentAnim.fullPathHash == hash)
        {
            if (!animator.IsInTransition(0))
                return true;

            AnimatorStateInfo nextAnim = animator.GetNextAnimatorStateInfo(0);

            if (nextAnim.fullPathHash == hash)
                return true;
        }

        animator.Play(hash, 0, 0f);
        return true;
    }

    public bool PlayFromStart(EnemyAnimationType type)
    {
        if (animator == null || !animHashDic.TryGetValue(type, out int hash) || !animator.HasState(0, hash))
            return false;

        animator.Play(hash, 0, 0f);
        return true;
    }

    // 나중에 최적화 필요
    public bool Finish(EnemyAnimationType type)
    {
        if (animator == null || !animHashDic.TryGetValue(type, out int hash))
            return true;

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);

        if (state.fullPathHash != hash)
            return true;

        if (animator.IsInTransition(0) && animator.GetNextAnimatorStateInfo(0).fullPathHash != hash)
        {
            return true;
        }

        return state.normalizedTime >= 1f;
    }
}
