using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PausedPopup : PopupBase
{
    private enum PausedButtonType
    {
        Continue,
        Restart,
        Retire,
        Setting,
        Exit,
        Credit
    }

    [Serializable]
    private struct ButtonBind
    {
        public PausedButtonType action;
        public Button button;
        public UnityEvent callback;
    }

    [SerializeField] private List<ButtonBind> buttonList;

    private readonly Dictionary<PausedButtonType, Button> buttonDic = new();

    private void Awake()
    {
        foreach (ButtonBind bind in buttonList)
        {
            if (bind.button == null)
                continue;

            if (!buttonDic.TryAdd(bind.action, bind.button))
                continue;

            if (bind.callback != null)
                ButtonBinding(bind.action, bind.callback.Invoke);
        }

        bool isLobby = SceneLoadManager.Instance.IsLobby;

        if (buttonDic.TryGetValue(PausedButtonType.Restart, out Button restartButton))
            restartButton.gameObject.SetActive(!isLobby);

        if (buttonDic.TryGetValue(PausedButtonType.Retire, out Button retireButton))
            retireButton.gameObject.SetActive(!isLobby);
    }

    private void ButtonBinding(PausedButtonType type, UnityEngine.Events.UnityAction callback)
    {
        if (buttonDic.TryGetValue(type, out Button button))
            button.onClick.AddListener(callback);
    }
    private void ButtonUnBinding(PausedButtonType type, UnityEngine.Events.UnityAction callback)
    {
        if (buttonDic.TryGetValue(type, out Button button))
            button.onClick.RemoveListener(callback);
    }

    private void OnDestroy()
    {
        foreach (ButtonBind bind in buttonList)
        {
            if (bind.callback != null)
                ButtonUnBinding(bind.action, bind.callback.Invoke);
        }        
    }

    public void HandleContinue()
    {
        UIManager.Instance.ClosePopup(uiName);
    }

    public void HandleRestart()
    {
        UIManager.Instance.OpenWarningPopup(ConstString.RestartMessage, () => ExitRun(SceneName.Stage1_Scene).Forget());
    }

    public void HandleRetire()
    {
        UIManager.Instance.OpenWarningPopup(ConstString.RetireMessage, () => ExitRun(SceneName.LobbyScene).Forget());
    }

    public void HandleSetting()
    {
        UIManager.Instance.OpenPopup(UIName.Popup_Setting);
    }

    public void HandleCredit()
    {
        UIManager.Instance.OpenPopup(UIName.Popup_Credit);
    }

    public void HandleExit()
    {
        UIManager.Instance.OpenWarningPopup(ConstString.ExitMessage, Application.Quit);
    }

    private async UniTask ExitRun(SceneName scene)
    {
        GameManager gameManager = GameManager.Instance;

        if (gameManager == null)
            return;

        UIManager.Instance.ClosePopup(uiName);
        EventBus.Publish(new GameStateChangedEvent(GameState.SceneTransition));

        try
        {
            gameManager.Inventory.Clear();
            await SceneLoadManager.Instance.LoadSceneAsync(scene);
        }
        finally
        {
            if (gameManager.CurrentState == GameState.SceneTransition)
                EventBus.Publish(new GameStateChangedEvent(GameState.Play));
        }
    }
}
