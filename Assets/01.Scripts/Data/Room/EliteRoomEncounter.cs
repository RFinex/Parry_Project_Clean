using Cysharp.Threading.Tasks;
using DG.Tweening.Core.Easing;
using System;
using System.Threading;
using UnityEngine;

public class EliteRoomEncounter : MonoBehaviour
{
    [SerializeField] private Collider2D introTrigger;
    [SerializeField] private LayerMask playerLayer;

    private EliteEnemyController elite;
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

    public void Initialized(EliteEnemyController elite, PlayerController player)
    {
        this.elite = elite;
        this.player = player;
        isStarted = false;

        if (introTrigger != null)
            introTrigger.enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isStarted || elite == null || player == null)
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
            gameManager.SetCameraTarget(elite.transform);
            await UniTask.Delay(TimeSpan.FromSeconds(elite.Data.IntroFocusDuration), cancellationToken: ctk);

            elite.FacePlayer(player);
            await UniTask.Delay(TimeSpan.FromSeconds(elite.Data.IntroFaceDuration), cancellationToken: ctk);

            gameManager.SetCameraTarget(player.transform);
            await UniTask.Delay(TimeSpan.FromSeconds(elite.Data.IntroReturnDuration), cancellationToken: ctk);

            elite.StartCombat(player);
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
