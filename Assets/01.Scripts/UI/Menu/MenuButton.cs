using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
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

    private void Awake()
    {
        foreach (ButtonBind bind in buttonList)
        {
            if (bind.button == null)
                continue;

            if (!buttonDic.TryAdd(bind.type, bind.button))
                continue;

            bind.button.onClick.AddListener(() => OnClickButton(bind.type));
        }
    }

    private void OnDisable()
    {
        foreach (ButtonBind bind in buttonList)
            bind.button.onClick.RemoveAllListeners();
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
