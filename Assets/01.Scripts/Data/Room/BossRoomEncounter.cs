using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class BossRoomEncounter : MonoBehaviour
{
    [SerializeField] private Collider2D introTrigger;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private Transform bossJumpPoint_1;
    [SerializeField] private Transform bossJumpPoint_2;

    private BossController boss;
    private PlayerController player;

    private bool isStarted;

    private CancellationTokenSource token;

    private void OnEnable()
    {
        token = token.ResetToken();
    }

    private void OnDisable()
    {
        token = token.Cancel_DisposeToken();
    }

    public void Initialized(BossController boss, PlayerController player)
    {
        this.boss = boss;
        this.player = player;

        isStarted = false;

        if (introTrigger != null)
            introTrigger.enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isStarted || boss == null || player == null)
            return;

        if ((playerLayer.value & (1 << collision.gameObject.layer)) == 0)
            return;

        isStarted = true;

        if(introTrigger != null)
            introTrigger.enabled = false;

        PlayIntroAsync(token.Token).Forget();
    }

    private async UniTaskVoid PlayIntroAsync(CancellationToken ctk)
    {
        GameManager gameManager = GameManager.Instance;

        if (gameManager == null)
            return;

        gameManager.SetGameState(GameState.Cutscene);

        try
        {
            await gameManager.MoveCameraTargetAsync(boss.transform, boss.Data.IntroFocusDuration, ctk);

            await boss.PlayIntroAsync(player, bossJumpPoint_1, bossJumpPoint_2, ctk);

            await gameManager.MoveCameraTargetAsync(player.transform, boss.Data.IntroReturnDuration, ctk);

            EventBus.Publish(new BossFightStartedEvent(boss));
            boss.StartCombat(player);
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            if (gameManager.Player != null)
                gameManager.SetCameraTarget(gameManager.Player.transform);

            if (gameManager.CurrentState == GameState.Cutscene)
                gameManager.SetGameState(GameState.Play);
        }
    }
}
