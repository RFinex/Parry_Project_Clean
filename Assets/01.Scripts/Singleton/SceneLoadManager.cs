using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SceneName
{
    LobbyScene,
    Stage1_Scene
}

public class SceneLoadManager : Singleton<SceneLoadManager>
{
    [SerializeField] private SceneTransitionUI transitionUI;

    private bool isLoading;
    private bool isReady;

    private CancellationTokenSource token;

    private void OnEnable()
    {
        Utils.EnterToken(ref token);
    }

    private void OnDisable()
    {
        Utils.ExitToken(ref token);
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
        try
        {
            if (isLoading)
                return;

            isReady = false;
            isLoading = true;

            await transitionUI.FadeOutAsync(token.Token);
            transitionUI.ShowLoading();

            await UniTask.NextFrame(cancellationToken: token.Token);

            // 참조 해제

            await SceneManager.LoadSceneAsync(name.ToString())
                .ToUniTask(cancellationToken: token.Token);

            // DDOL 초기화

            if (name == SceneName.Stage1_Scene)
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
}
