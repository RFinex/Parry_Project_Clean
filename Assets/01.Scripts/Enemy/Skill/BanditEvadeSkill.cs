using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class BanditEvadeSkill : EnemySkill
{
    private readonly BanditEvadeSkillData evadeData;

    public BanditEvadeSkill(BanditEvadeSkillData data) : base(data)
    {
        evadeData = data;
    }

    public override async UniTask ExecuteAsync(EnemyController owner, EnemyBlackBoard board, CancellationToken ctk)
    {
        PlayerController player = Target;

        if (owner == null || board == null || player == null)
            return;

        float dir = owner.transform.position.x - player.transform.position.x;

        if (dir == 0f)
            dir = -owner.FrontDir;
        else
            dir = dir >= 0f ? 1f : -1f;
               
        try
        {
            owner.SetInvincible(true);
            board.animator.Play(evadeData.AnimationType);

            board.rb.linearVelocity = new Vector2(dir * evadeData.HorizontalSpeed, evadeData.JumpForce);

            // 착지할 때 까지 대기
            await UniTask.WaitUntil(() => owner.IsGround() && board.rb.linearVelocity.y <= 0f, cancellationToken: ctk);
        }
        finally
        {
            owner.SetInvincible(false);
            owner.StopMove();
        }

        await WaitRecovery(ctk);
    }
}
