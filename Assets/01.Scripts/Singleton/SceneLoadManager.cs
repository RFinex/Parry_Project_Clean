using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SceneName
{
    LobbyScene,
    Stage1_Scene,
    Stage2_Scene
}

public class SceneLoadManager : Singleton<SceneLoadManager>
{
    [SerializeField] private SceneTransitionUI transitionUI;

    private bool isLoading;
    private bool isReady;
    private bool isWaitForSceneReady;
    public bool IsLobby { get; private set; }

    private CancellationTokenSource token;

    private void OnEnable()
    {
        token = token.ResetToken();

        HandleActiveSceneChanged(default, SceneManager.GetActiveScene());
        SceneManager.activeSceneChanged -= HandleActiveSceneChanged;
        SceneManager.activeSceneChanged += HandleActiveSceneChanged;
    }

    private void OnDisable()
    {
        token = token.Cancel_DisposeToken();
     
        SceneManager.activeSceneChanged += HandleActiveSceneChanged;
    }

    public void LoadScene(SceneName sceneName)
    {
        SceneManager.LoadScene(sceneName.ToString());
    }

    public void LoadLobbyScene()
    {
        LoadScene(SceneName.LobbyScene);
    }

    public void LoadStageScene()
    {
        LoadScene(SceneName.Stage1_Scene);
    }

    public async UniTask LoadSceneAsync(SceneName name)
    {
        if (isLoading)
            return;

        isLoading = true;

        try
        {
            isReady = false;
            isWaitForSceneReady = false;

            await transitionUI.FadeOutAsync(token.Token);
            transitionUI.ShowLoading();

            await UniTask.NextFrame(cancellationToken: token.Token);

            // 참조 해제

            await SceneManager.LoadSceneAsync(name.ToString())
                .ToUniTask(cancellationToken: token.Token);

            // DDOL 초기화

            if (isWaitForSceneReady)
            {
                await UniTask.WaitUntil(() => isReady, cancellationToken: token.Token);
            }

            transitionUI.HideLoading();

            await transitionUI.FadeInAsync(token.Token);
        }
        catch (OperationCanceledException)
        {

        }
        finally
        {
            isLoading = false;
        }
    }

    public void SceneReady()
    {
        isReady = true;
    }

    public void RequireSceneReady()
    {
        isWaitForSceneReady = true;
    }

    /// <summary>
    /// 현재 씬이 로비인지 체크
    /// </summary>
    /// <param name="previousScene"></param>
    /// <param name="currentScene"></param>
    private void HandleActiveSceneChanged(Scene previousScene, Scene currentScene)
    {
        IsLobby = currentScene.name == nameof(SceneName.LobbyScene);
    }
}
