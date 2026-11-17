using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuButton : MonoBehaviour
{
    private enum MenuButtonType
    {
        Start,
        Load,
        Setting,
        Exit
    }

    [Serializable]
    private struct ButtonBind
    {
        public MenuButtonType type;
        public Button button;
    }

    [SerializeField] private List<ButtonBind> buttonList;

    private readonly Dictionary<MenuButtonType, Button> buttonDic = new();
    private readonly Dictionary<Button, UnityEngine.Events.UnityAction> actionDic = new();

    private void Awake()
    {
        foreach (ButtonBind bind in buttonList)
        {
            if (bind.button == null)
                continue;

            if (!buttonDic.TryAdd(bind.type, bind.button))
                continue;

            UnityEngine.Events.UnityAction action = () => OnClickButton(bind.type);

            actionDic.Add(bind.button, action);
            bind.button.onClick.AddListener(action);
        }
    }

    private void OnDestroy()
    {
        foreach (var pair in actionDic)
            pair.Key.onClick.RemoveListener(pair.Value);
    }

    private void OnClickButton(MenuButtonType type)
    {
        switch (type)
        {
            case MenuButtonType.Start:
                HandleStart();
                break;
            case MenuButtonType.Load:
                HandleLoad();
                break;
            case MenuButtonType.Setting:
                HandleSetting();
                break;
            case MenuButtonType.Exit:
                HandleExit();
                break;
        }
    }

    public void HandleStart()
    {        
        SceneLoadManager.Instance.LoadSceneAsync(SceneName.LobbyScene).Forget();
    }

    public void HandleLoad()
    {

    }

    public void HandleSetting()
    {
        UIManager.Instance.OpenPopup(UIName.Popup_Setting);
    }

    public void HandleExit()
    {
        UIManager.Instance.OpenWarningPopup(ConstString.ExitMessage, Application.Quit);
    }
}
