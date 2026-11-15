using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ClearPopup : PopupBase
{
    [Header("Button")]
    [SerializeField] private Button lobbyBtn;

    private void Awake()
    {
        lobbyBtn.onClick.AddListener(ReturnLobby);
    }

    private void OnDestroy()
    {
        lobbyBtn.onClick.RemoveListener(ReturnLobby);
    }

    protected override void Initialized()
    {
        SoundManager.Instance.StopBGM();
        SoundManager.Instance.PlaySFX(SFXType.Clear);
    }

    private async void ReturnLobby()
    {
        if (SceneLoadManager.Instance == null)
            return;

        await SceneLoadManager.Instance.LoadSceneAsync(SceneName.LobbyScene);

        if (SceneLoadManager.Instance.IsLobby)
        {
            GameManager.Instance.ClearInventory();
            GameManager.Instance.SetGameState(GameState.Play);
        }
    }
}
