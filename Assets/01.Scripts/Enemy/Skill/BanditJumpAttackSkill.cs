using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class BanditJumpAttackSkill : EnemySkill
{
    private readonly BanditJumpAttackSkillData jumpAttackData;

    public BanditJumpAttackSkill(BanditJumpAttackSkillData data) : base(data)
    {
        jumpAttackData = data;
    }

    public override async UniTask ExecuteAsync(EnemyController owner, EnemyBlackBoard board, CancellationToken ctk)
    {
        owner.StopMove();
        owner.LookAtTarget();

        board.animator.Play(EnemyAnimationType.Idle);
        await WaitStartup(ctk);

        if (!board.animator.Play(data.AnimationType))
            return;

        board.rb.linearVelocity = new Vector2(owner.FrontDir * jumpAttackData.HorizontalSpeed, jumpAttackData.JumpForce);

        await UniTask.WaitUntil(() => owner.IsGround() && board.rb.linearVelocity.y <= 0f, cancellationToken: ctk);

        ApplyAttack(owner, board);

        owner.StopMove();

        await WaitRecovery(ctk);
    }
}
