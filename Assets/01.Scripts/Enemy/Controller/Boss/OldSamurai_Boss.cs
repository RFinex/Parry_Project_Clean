using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class OldSamurai_Boss : BossController
{
    [SerializeField] private Transform roomCenter;

    [Header("Skill Data(Phase1")]
    [SerializeField] private ParryComboSkillData parryComboData;
    [SerializeField] private OldSamuraiThrowSkillData throwData;
    [SerializeField] private OldSamuraiUpperSkillData upperData;
    [SerializeField] private OldSamuraiLargeComboAttackData largeComboData;

    [Header("Skill Data(Phase2)")]
    [SerializeField] private ParryComboSkillData phase2ParryComboData;
    [SerializeField] private OldSamuraiThrowSkillData phase2ThrowData;
    [SerializeField] private OldSamuraiUpperSkillData phase2UpperData;
    [SerializeField] private OldSamuraiLargeComboAttackData phase2LargeComboData;
    [SerializeField] private OldSamuraiPhase2SweepData sweepData;
    [SerializeField] private OldSamuraiPhase2CrossSlashData crossData;
    [SerializeField] private OldSamuraiPhase2LineBarrageData lineData;
    [SerializeField] private OldSamuraiPhase2DashTeleportData dashData;


    private OldSamuraiParryComboSkill parryComboSkill;
    private OldSamuraiThrowSkill throwSkill;
    private OldSamuraiUpperSkill upperSkill;
    private OldSamuraiLargeComboAttackSkill largeComboSkill;

    private OldSamuraiParryComboSkill phase2ParryComboSkill;
    private OldSamuraiThrowSkill phase2ThrowSkill;
    private OldSamuraiUpperSkill phase2UpperSkill;
    private OldSamuraiLargeComboAttackSkill phase2LargeComboSkill;
    private OldSamuraiPhase2SweepSkill sweepSkill;
    private OldSamuraiPhase2CrossSlashSkill crossSkill;
    private OldSamuraiPhase2LineBarrageSkill lineSkill;
    private OldSamuraiPhase2DashTeleportSkill dashSkill;

    private Transform jumpPoint_1;
    private Transform jumpPoint_2;

    protected override bool HasPhase2Intro => true;

    protected override void InitBlackBoard()
    {
        base.InitBlackBoard();

        parryComboSkill = new(parryComboData);
        throwSkill = new(throwData);
        upperSkill = new(upperData);
        largeComboSkill = new(largeComboData);

        phase2ParryComboSkill = new(phase2ParryComboData);
        phase2ThrowSkill = new(phase2ThrowData);
        phase2UpperSkill = new(phase2UpperData);
        phase2LargeComboSkill = new(phase2LargeComboData);
        sweepSkill = new(sweepData);
        crossSkill = new(crossData);
        lineSkill = new(lineData, roomCenter);
        dashSkill = new(dashData);

        blackBoard.skills.Clear();
        blackBoard.skills.Add(parryComboSkill);
        blackBoard.skills.Add(throwSkill);
        blackBoard.skills.Add(upperSkill);
        blackBoard.skills.Add(largeComboSkill);

        blackBoard.skills.Add(phase2ParryComboSkill);
        blackBoard.skills.Add(phase2ThrowSkill);
        blackBoard.skills.Add(phase2UpperSkill);
        blackBoard.skills.Add(phase2LargeComboSkill);
        blackBoard.skills.Add(sweepSkill);
        blackBoard.skills.Add(crossSkill);
        blackBoard.skills.Add(lineSkill);
        blackBoard.skills.Add(dashSkill);
    }



    protected override INode BuildPhase1BT()
    {
        SequenceNode largeComboSequence = new();
        largeComboSequence.Add(new ActionNode(CheckLargeComboRange));
        largeComboSequence.Add(new ActionNode(CheckLargeComboCooldown));
        largeComboSequence.Add(new ActionNode(UseLargeComboAttack));

        SequenceNode throwSequence = new();
        throwSequence.Add(new ActionNode(CheckThrowRange));
        throwSequence.Add(new ActionNode(CheckThrowCooldown));
        throwSequence.Add(new ActionNode(UseThrowAttack));

        SequenceNode upperSequence = new();
        upperSequence.Add(new ActionNode(CheckUpperRange));
        upperSequence.Add(new ActionNode(CheckUpperCooldown));
        upperSequence.Add(new ActionNode(UseUpperAttack));

        SequenceNode parryComboSequence = new();
        parryComboSequence.Add(new ActionNode(CheckParryComboRange));
        parryComboSequence.Add(new ActionNode(CheckParryComboCooldown));
        parryComboSequence.Add(new ActionNode(UseParryComboAttack));

        SequenceNode traceSequence = new();
        traceSequence.Add(new ActionNode(CheckPlayerAlive));
        traceSequence.Add(new ActionNode(TracePlayer));

        SelectorNode phase1Selector = new();
        phase1Selector.Add(largeComboSequence);
        phase1Selector.Add(upperSequence);
        phase1Selector.Add(throwSequence);
        phase1Selector.Add(parryComboSequence);
        phase1Selector.Add(traceSequence);
        phase1Selector.Add(new ActionNode(Idle));

        return phase1Selector;
    }

    protected override INode BuildPhase2BT()
    {
        SequenceNode lineSequence = new();
        lineSequence.Add(new ActionNode(CheckLineRange));
        lineSequence.Add(new ActionNode(CheckLineCooldown));
        lineSequence.Add(new ActionNode(UseLineAttack));

        SequenceNode upperSequence = new();
        upperSequence.Add(new ActionNode(CheckPhase2UpperRange));
        upperSequence.Add(new ActionNode(CheckPhase2UpperCooldown));
        upperSequence.Add(new ActionNode(UsePhase2UpperAttack));

        SequenceNode largeComboSequence = new();
        largeComboSequence.Add(new ActionNode(CheckPhase2LargeComboRange));
        largeComboSequence.Add(new ActionNode(CheckPhase2LargeComboCooldown));
        largeComboSequence.Add(new ActionNode(UsePhase2LargeComboAttack));

        SequenceNode dashSequence = new();
        dashSequence.Add(new ActionNode(CheckDashRange));
        dashSequence.Add(new ActionNode(CheckDashCooldown));
        dashSequence.Add(new ActionNode(UseDash));

        SequenceNode sweepSequence = new();
        sweepSequence.Add(new ActionNode(CheckSweepRange));
        sweepSequence.Add(new ActionNode(CheckSweepCooldown));
        sweepSequence.Add(new ActionNode(UseSweepAttack));

        SequenceNode crossSequence = new();
        crossSequence.Add(new ActionNode(CheckCrossRange));
        crossSequence.Add(new ActionNode(CheckCrossCooldown));
        crossSequence.Add(new ActionNode(UseCrossAttack));

        SequenceNode throwSequence = new();
        throwSequence.Add(new ActionNode(CheckPhase2ThrowRange));
        throwSequence.Add(new ActionNode(CheckPhase2ThrowCooldown));
        throwSequence.Add(new ActionNode(UsePhase2ThrowAttack));

        SequenceNode parryComboSequence = new();
        parryComboSequence.Add(new ActionNode(CheckPhase2ParryComboRange));
        parryComboSequence.Add(new ActionNode(CheckPhase2ParryComboCooldown));
        parryComboSequence.Add(new ActionNode(UsePhase2ParryComboAttack));

        SequenceNode traceSequence = new();
        traceSequence.Add(new ActionNode(CheckPlayerAlive));
        traceSequence.Add(new ActionNode(TracePlayer));

        SelectorNode phase2Selector = new();
        phase2Selector.Add(lineSequence);
        phase2Selector.Add(upperSequence);
        phase2Selector.Add(largeComboSequence);
        phase2Selector.Add(dashSequence);
        phase2Selector.Add(parryComboSequence);
        phase2Selector.Add(sweepSequence);
        phase2Selector.Add(crossSequence);
        phase2Selector.Add(throwSequence);
        phase2Selector.Add(traceSequence);
        phase2Selector.Add(new ActionNode(Idle));

        return phase2Selector;
    }

    #region BT Method

    private UniTask<INode.State> CheckLargeComboRange(CancellationToken ctk)
    {
        return CheckSkillRange(largeComboSkill);
    }
    private UniTask<INode.State> CheckThrowRange(CancellationToken ctk)
    {
        return CheckSkillRange(throwSkill);
    }

    private UniTask<INode.State> CheckUpperRange(CancellationToken ctk)
    {
        return CheckSkillRange(upperSkill);
    }

    private UniTask<INode.State> CheckParryComboRange(CancellationToken ctk)
    {
        return CheckSkillRange(parryComboSkill);
    }


    private UniTask<INode.State> CheckPhase2LargeComboRange(CancellationToken ctk)
    {
        return CheckSkillRange(phase2LargeComboSkill);
    }

    private UniTask<INode.State> CheckPhase2ThrowRange(CancellationToken ctk)
    {
        return CheckSkillRange(phase2ThrowSkill);
    }

    private UniTask<INode.State> CheckPhase2UpperRange(CancellationToken ctk)
    {
        return CheckSkillRange(phase2UpperSkill);
    }

    private UniTask<INode.State> CheckPhase2ParryComboRange(CancellationToken ctk)
    {
        return CheckSkillRange(phase2ParryComboSkill);
    }

    private UniTask<INode.State> CheckSweepRange(CancellationToken ctk)
    {
        return CheckSkillRange(sweepSkill);
    }

    private UniTask<INode.State> CheckCrossRange(CancellationToken ctk)
    {
        return CheckSkillRange(crossSkill);
    }

    private UniTask<INode.State> CheckLineRange(CancellationToken ctk)
    {
        return CheckSkillRange(lineSkill);
    }

    private UniTask<INode.State> CheckDashRange(CancellationToken ctk)
    {
        return CheckSkillRange(dashSkill);
    }

    private UniTask<INode.State> CheckSkillRange(EnemySkill skill)
    {
        bool inRange = GetPlayer() != null && skill != null && skill.IsInRange(this);
        return UniTask.FromResult(inRange ? INode.State.Success : INode.State.Failure);
    }

    private UniTask<INode.State> CheckLargeComboCooldown(CancellationToken ctk)
    {
        return CheckSkillReady(largeComboSkill);
    }

    private UniTask<INode.State> CheckThrowCooldown(CancellationToken ctk)
    {
        return CheckSkillReady(throwSkill);
    }

    private UniTask<INode.State> CheckUpperCooldown(CancellationToken ctk)
    {
        return CheckSkillReady(upperSkill);
    }

    private UniTask<INode.State> CheckParryComboCooldown(CancellationToken ctk)
    {
        return CheckSkillReady(parryComboSkill);
    }


    private UniTask<INode.State> CheckPhase2LargeComboCooldown(CancellationToken ctk)
    {
        return CheckSkillReady(phase2LargeComboSkill);
    }

    private UniTask<INode.State> CheckPhase2ThrowCooldown(CancellationToken ctk)
    {
        return CheckSkillReady(phase2ThrowSkill);
    }

    private UniTask<INode.State> CheckPhase2UpperCooldown(CancellationToken ctk)
    {
        return CheckSkillReady(phase2UpperSkill);
    }

    private UniTask<INode.State> CheckPhase2ParryComboCooldown(CancellationToken ctk)
    {
        return CheckSkillReady(phase2ParryComboSkill);
    }

    private UniTask<INode.State> CheckSweepCooldown(CancellationToken ctk)
    {
        return CheckSkillReady(sweepSkill);
    }

    private UniTask<INode.State> CheckCrossCooldown(CancellationToken ctk)
    {
        return CheckSkillReady(crossSkill);
    }

    private UniTask<INode.State> CheckLineCooldown(CancellationToken ctk)
    {
        return CheckSkillReady(lineSkill);
    }

    private UniTask<INode.State> CheckDashCooldown(CancellationToken ctk)
    {
        return CheckSkillReady(dashSkill);
    }

    private UniTask<INode.State> CheckSkillReady(EnemySkill skill)
    {
        bool isReady = skill != null && skill.IsReady;
        return UniTask.FromResult(isReady ? INode.State.Success : INode.State.Failure);
    }

    private UniTask<INode.State> UseLargeComboAttack(CancellationToken ctk)
    {
        return largeComboSkill.TryUse(this, blackBoard, ctk);
    }

    private UniTask<INode.State> UseThrowAttack(CancellationToken ctk)
    {
        return throwSkill.TryUse(this, blackBoard, ctk);
    }

    private UniTask<INode.State> UseUpperAttack(CancellationToken ctk)
    {
        return upperSkill.TryUse(this, blackBoard, ctk);
    }

    private UniTask<INode.State> UseParryComboAttack(CancellationToken ctk)
    {
        return parryComboSkill.TryUse(this, blackBoard, ctk);
    }


    private UniTask<INode.State> UsePhase2LargeComboAttack(CancellationToken ctk)
    {
        return phase2LargeComboSkill.TryUse(this, blackBoard, ctk);
    }

    private UniTask<INode.State> UsePhase2ThrowAttack(CancellationToken ctk)
    {
        return phase2ThrowSkill.TryUse(this, blackBoard, ctk);
    }

    private UniTask<INode.State> UsePhase2UpperAttack(CancellationToken ctk)
    {
        return phase2UpperSkill.TryUse(this, blackBoard, ctk);
    }

    private UniTask<INode.State> UsePhase2ParryComboAttack(CancellationToken ctk)
    {
        return phase2ParryComboSkill.TryUse(this, blackBoard, ctk);
    }

    private UniTask<INode.State> UseSweepAttack(CancellationToken ctk)
    {
        return sweepSkill.TryUse(this, blackBoard, ctk);
    }

    private UniTask<INode.State> UseCrossAttack(CancellationToken ctk)
    {
        return crossSkill.TryUse(this, blackBoard, ctk);
    }

    private UniTask<INode.State> UseLineAttack(CancellationToken ctk)
    {
        return lineSkill.TryUse(this, blackBoard, ctk);
    }

    private UniTask<INode.State> UseDash(CancellationToken ctk)
    {
        return dashSkill.TryUse(this, blackBoard, ctk);
    }
    #endregion

    private const float JumpLandingTimeout = 5f;

    public override async UniTask PlayIntroAsync(PlayerController player, Transform jumpPoint, Transform jumpPoint2, CancellationToken ctk)
    {
        if (player == null || jumpPoint == null || jumpPoint2 == null)
            return;

        jumpPoint_1 = jumpPoint;
        jumpPoint_2 = jumpPoint2;
        isIntroRunning = true;

        try
        {
            FacePlayer(player);
            blackBoard.animator.Play(EnemyAnimationType.Idle);

            if (Data.IntroFaceDuration > 0f)
                await UniTask.Delay(TimeSpan.FromSeconds(Data.IntroFaceDuration), cancellationToken: ctk);

            await JumpToPointAsync(jumpPoint_2, ctk);

        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            StopMove();
            blackBoard.animator.Play(EnemyAnimationType.Idle);
            isIntroRunning = false;
        }
    }

    protected override async UniTask PlayPhase2IntroAsync(CancellationToken ctk)
    {
        GameManager gameManager = GameManager.Instance;

        if (jumpPoint_1 == null || jumpPoint_2 == null || blackBoard == null || blackBoard.rb == null)
        {
            CompletePhase2Transition();
            return;
        }

        isIntroRunning = true;

        if (gameManager != null)
            gameManager.SetGameState(GameState.Cutscene);

        try
        {
            if (gameManager != null)
                await gameManager.MoveCameraTargetAsync(transform, Data.IntroFocusDuration, ctk);

            if (Data.IntroFaceDuration > 0f)
                await UniTask.Delay(TimeSpan.FromSeconds(Data.IntroFaceDuration), cancellationToken: ctk);
            if (!await JumpToPointAsync(jumpPoint_1, ctk))
                return;
            blackBoard.animator.Play(EnemyAnimationType.Idle);
            if (Data.IntroFaceDuration > 0f)
                await UniTask.Delay(TimeSpan.FromSeconds(Data.IntroFaceDuration), cancellationToken: ctk);

            FacePlayer(GetPlayer());

            if (blackBoard.animator.PlayFromStart(EnemyAnimationType.Heal))
                await UniTask.WaitUntil(
                    () => blackBoard.animator.Finish(EnemyAnimationType.Heal),
                    cancellationToken: ctk);
            await JumpToPointAsync(jumpPoint_2, ctk);

            if (gameManager != null && gameManager.Player != null)
                await gameManager.MoveCameraTargetAsync(gameManager.Player.transform, Data.IntroReturnDuration, ctk);
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            StopMove();
            blackBoard.animator.Play(EnemyAnimationType.Idle);
            isIntroRunning = false;

            if (gameManager != null)
            {
                if (gameManager.Player != null)
                    gameManager.SetCameraTarget(gameManager.Player.transform);

                if (gameManager.CurrentState == GameState.Cutscene)
                    gameManager.SetGameState(GameState.Play);
            }
            CompletePhase2Transition();
        }
    }

    private async UniTask<bool> JumpToPointAsync(Transform point, CancellationToken ctk)
    {
        if (point == null || blackBoard == null || blackBoard.rb == null || BossData == null)
            return false;

        SetFrontDir(point.position.x - transform.position.x);
        blackBoard.animator.PlayFromStart(EnemyAnimationType.Jump);
        blackBoard.rb.linearVelocity = new Vector2(
            FrontDir * BossData.IntroJumpSpeed,
            BossData.IntroJumpForce);

        float elapsed = 0f;

        while (elapsed < JumpLandingTimeout)
        {
            ctk.ThrowIfCancellationRequested();

            if (IsGround() && blackBoard.rb.linearVelocity.y <= 0f)
            {
                StopMove();
                return true;
            }

            await UniTask.NextFrame(PlayerLoopTiming.FixedUpdate, ctk);
            elapsed += Time.fixedDeltaTime;
        }

        StopMove();
        Utils.LogWarning<OldSamurai_Boss>($"Jump landing timeout at {point.name}.");
        return false;
    }

    public override void SetRoomCenter(Transform center)
    {
        roomCenter = center;
        lineSkill?.SetRoomCenter(center);
    }
}
