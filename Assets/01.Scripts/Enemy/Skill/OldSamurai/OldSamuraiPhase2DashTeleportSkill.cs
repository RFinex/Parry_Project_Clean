using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class OldSamuraiPhase2DashTeleportSkill : EnemySkill
{
    private readonly OldSamuraiPhase2DashTeleportData teleportData;

    public OldSamuraiPhase2DashTeleportSkill(OldSamuraiPhase2DashTeleportData data) : base(data)
    {
        teleportData = data;
    }

    public override async UniTask ExecuteAsync(EnemyController owner, EnemyBlackBoard board, CancellationToken ctk)
    {
        if (owner == null || board == null || teleportData == null || Target == null)
            return;

        owner.StopMove();

        try
        {
            owner.ShowAttackWarning(data.CanGuard, data.CanParry);
            await WaitStartup(ctk);

            if (!board.animator.PlayFromStart(EnemyAnimationType.Dash))
                return;

            // Dash 애니메이션에서 실제 이동이 시작되는 시점까지 대기
            if (teleportData.DashStartDelay > 0f)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(teleportData.DashStartDelay), cancellationToken: ctk);
            }
            else
            {
                await UniTask.NextFrame(PlayerLoopTiming.Update, ctk);
            }

            if (Target == null)
                return;

            Vector2 playerPosition = Target.transform.position;
            Vector2 playerForward = Vector2.right * Target.FrontDir;

            // 플레이어 후방
            Vector2 rearPosition = playerPosition - playerForward * teleportData.RearDistance;

            // 플레이어 정면
            Vector2 frontPosition = playerPosition + playerForward * teleportData.RearDistance;

            Vector2 teleportPosition;

            if (!IsPositionBlocked(board, rearPosition))
                teleportPosition = rearPosition;
            else if (!IsPositionBlocked(board, frontPosition))
                teleportPosition = frontPosition;
            else
                return;

            board.rb.position = teleportPosition;
            owner.LookAtTarget();

            if (teleportData.AnimEndDelay > 0f)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(teleportData.AnimEndDelay), cancellationToken: ctk);
            }

            board.animator.Play(EnemyAnimationType.Idle);
            await WaitRecovery(ctk);
        }
        catch (OperationCanceledException)
        {

        }
        finally
        {
            owner.StopMove();
        }
    }

    /// <summary>
    /// 플레이어 후방에 벽이 있는지 검사
    /// </summary>
    /// <param name="board"> 스킬 주인의 BlackBoard </param>
    /// <param name="position"> 플레이어 후방 위치 </param>
    /// <returns></returns>
    private bool IsPositionBlocked(EnemyBlackBoard board, Vector2 position)
    {
        if (board.col == null)
            return true;

        float bottomIgnore = 0.2f;

        Vector2 checkSize = board.col.bounds.size;
        checkSize.y -= bottomIgnore;

        Vector2 centerOffset =
            board.col.bounds.center - board.col.transform.position;

        centerOffset.y += bottomIgnore * 0.5f;

        Collider2D hit = Physics2D.OverlapBox(position + centerOffset, checkSize, 0f, board.obstacleLayer);

        return hit != null;
    }
}
