using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class BanditMeleeSkill : EnemySkill
{
    public BanditMeleeSkill(BanditMeleeSkillData data) : base(data)
    {
        
    }

    public override async UniTask ExecuteAsync(EnemyController owner, EnemyBlackBoard board, CancellationToken ctk)
    {
        owner.StopMove();
        owner.LookAtTarget();

        if (!board.animator.Play(EnemyAnimationType.Attack))
            return;

        await WaitAndApplyAttack(owner, board, ctk);

        await UniTask.WaitUntil(() => board.animator.Finish(EnemyAnimationType.Attack), cancellationToken: ctk);

        await WaitRecovery(ctk);
    }
}
