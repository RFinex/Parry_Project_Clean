using System;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyAnimationType
{
    Idle,
    Move,
    Attack,
    Hurt,
    Jump,
    Death
}

[Serializable]
public struct EnemyAnimationInfo
{
    public EnemyAnimationType type;
    public string stateName;
}

public class EnemyAnimator
{
    private readonly Animator animator;
    private readonly Dictionary<EnemyAnimationType, int> animHashDic = new Dictionary<EnemyAnimationType, int>();

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

    public bool Play(EnemyAnimationType type)
    {
        if (animator == null || !animHashDic.TryGetValue(type, out int hash) ||
            !animator.HasState(0, hash))
        {
            return false;
        }

        animator.Play(hash, 0, 0f);
        return true;
    }

    public bool Finish(EnemyAnimationType type)
    {
        if (animator == null || !animHashDic.TryGetValue(type, out int hash))
        {
            return true;
        }

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);

        return state.fullPathHash == hash && state.normalizedTime >= 1f;
    }
}
