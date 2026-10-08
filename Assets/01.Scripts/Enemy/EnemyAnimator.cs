using UnityEngine;

public class EnemyAnimator
{
    private readonly Animator animator;

    #region Animation Hash

    // integer & float
    private static readonly int AnimState = Animator.StringToHash("AnimState");
    private static readonly int AirSpeedY = Animator.StringToHash("AirSpeedY");

    // bool
    private static readonly int Grounded = Animator.StringToHash("Grounded");
    private static readonly int IdleBlock = Animator.StringToHash("IdleBlock");

    // trigger
    private static readonly int[] Attack =
    {
        Animator.StringToHash("Attack1"),
        Animator.StringToHash("Attack2"),
        Animator.StringToHash("Attack3")
    };
    private static readonly int Block = Animator.StringToHash("Block");
    private static readonly int Hurt = Animator.StringToHash("Hurt");
    private static readonly int Death = Animator.StringToHash("Death");
    private static readonly int Jump = Animator.StringToHash("Jump");
    private static readonly int Roll = Animator.StringToHash("Roll");

    #endregion

    public EnemyAnimator(Animator animator)
    {
        this.animator = animator;
    }

    #region Move

    public void SetAnimState(int value)
    {
        animator.SetInteger(AnimState, value);
    }

    public void SetGrounded(bool isGround)
    {
        animator.SetBool(Grounded, isGround);
    }

    public void SetAirSpeed(float value)
    {
        animator.SetFloat(AirSpeedY, value);
    }

    #endregion

    #region Action

    public void PlayJump()
    {
        animator.SetTrigger(Jump);
    }

    public void PlayDash()
    {
        animator.SetTrigger(Roll);
    }

    public void PlayAttack(int index)
    {
        if (index <= 0 || index > Attack.Length)
            return;

        animator.SetTrigger(Attack[index - 1]);
    }

    public void PlayExecuteAttack()
    {
        animator.SetTrigger(Attack[1]);
    }

    public void PlayGuard()
    {
        animator.SetTrigger(Block);
        animator.SetBool(IdleBlock, true);
    }

    public void StopGuard()
    {
        animator.SetBool(IdleBlock, false);
    }

    public void PlayHurt()
    {
        animator.SetTrigger(Hurt);
    }

    public void PlayDeath()
    {
        animator.SetTrigger(Death);
    }

    #endregion
}
