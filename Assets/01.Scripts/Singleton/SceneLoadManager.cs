using UnityEngine;
using UnityEngine.SceneManagement;

public enum SceneId
{
    LobbyScene,
    RoomTestScene
}

public class SceneLoadManager : Singleton<SceneLoadManager>
{
    public void LoadScene(SceneId sceneId)
    {
        SceneManager.LoadScene(sceneId.ToString(), LoadSceneMode.Single);
    }

    public void LoadLobbyScene()
    {
        LoadScene(SceneId.LobbyScene);
    }

    public void LoadStageScene()
    {
        LoadScene(SceneId.RoomTestScene);
    }
}
