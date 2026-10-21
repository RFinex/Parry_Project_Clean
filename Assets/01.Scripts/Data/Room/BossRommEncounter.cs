using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class BossRommEncounter : MonoBehaviour
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
            gameManager.SetCameraTarget(boss.transform);
            await UniTask.Delay(TimeSpan.FromSeconds(boss.Data.IntroFocusDuration), cancellationToken: ctk);

            boss.FacePlayer(player);
            await UniTask.Delay(TimeSpan.FromSeconds(boss.Data.IntroFaceDuration), cancellationToken: ctk);

            gameManager.SetCameraTarget(player.transform);
            await UniTask.Delay(TimeSpan.FromSeconds(boss.Data.IntroReturnDuration), cancellationToken: ctk);

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
