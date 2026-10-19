using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public abstract class EnemySkill
{
    protected readonly EnemySkillData data;

    private bool isCooldown;

    private CancellationTokenSource coolToken;

    private PlayerController target;

    protected PlayerController Target
    {
        get
        {
            if (target == null && GameManager.Instance != null)
                target = GameManager.Instance.Player;

            if (target == null || target.Stats.IsDead)
                return null;

            return target;
        }
    }

    public bool IsReady => !isCooldown;

    public Vector2 HitboxSize => data != null ? data.HitboxSize : Vector2.zero;
    public float HitboxXOffset => data != null ? data.HitboxXOffset : 0f;

    protected EnemySkill(EnemySkillData data)
    {
        this.data = data;
    }

    public bool IsInRange(EnemyController owner)
    {
        if (owner == null || data == null)
            return false;

        if (Target == null)
            return false;

        float distance = Mathf.Abs(Target.transform.position.x - owner.transform.position.x);
        return distance >= data.MinRange && distance <= data.MaxRange;
    }
    
    /// <summary>
    /// 현재 스킬이 사용 가능한지 판단
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="board"></param>
    /// <param name="ctk"></param>
    /// <returns></returns>
    public async UniTask<INode.State> TryUse(EnemyController owner, EnemyBlackBoard board, CancellationToken ctk)
    {
        if (owner == null || board == null)
            return INode.State.Failure;

        if (!IsReady || !IsInRange(owner))
            return INode.State.Failure;

        board.currentSkill = this;
        StartCooldown(ctk);

        try
        {
            await ExecuteAsync(owner, board, ctk);
            return INode.State.Success;
        }
        catch (OperationCanceledException)
        {
            return INode.State.Running;
        }
        finally
        {
            if (board.currentSkill == this)
                board.currentSkill = null;
        }
    }

    public void StartCooldown(CancellationToken patternToken)
    {
        CancelCooldown();

        if (data.Cooldown <= 0f)
            return;

        isCooldown = true;
        coolToken = CancellationTokenSource.CreateLinkedTokenSource(patternToken);

        CooldownAsync(coolToken).Forget();
    }

    private async UniTask CooldownAsync(CancellationTokenSource token)
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(data.Cooldown), cancellationToken: token.Token);
        }
        catch (OperationCanceledException)
        {

        }
        finally
        {
            if (coolToken == token)
            {
                coolToken = null;
                isCooldown = false;
                token?.Dispose();
            }
        }
    }

    private void CancelCooldown()
    {
        coolToken = coolToken.Cancel_DisposeToken();
        isCooldown = false;
    }

    public abstract UniTask ExecuteAsync(EnemyController owner, EnemyBlackBoard board, CancellationToken ctk);

    public bool ApplyAttack(EnemyController owner, EnemyBlackBoard board)
    {
        if (owner == null || board == null)
            return false;

        if (Target == null)
            return false;

        Vector2 center = (Vector2)owner.transform.position + Vector2.right * owner.FrontDir * data.HitboxXOffset;

        Collider2D hit = Physics2D.OverlapBox(center, data.HitboxSize, 0f, board.playerLayer);

        if (hit == null)
            return false;

        board.combat.Attack(hit.transform, owner, data.Damage);

        return true;
    }

    protected async UniTask WaitAndApplyAttack(EnemyController owner, EnemyBlackBoard board, CancellationToken ctk)
    {
        if (data.AttackDelay > 0f)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(data.AttackDelay), cancellationToken: ctk);
        }

        ApplyAttack(owner, board);
    }

    public virtual void Reset()
    {
        CancelCooldown();
        target = null;
    }
}
