using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpawnDummyPopup : PopupBase
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image selectedSprite;

    [Header("Animator")]
    [SerializeField] private Animator selectedSpriteAnim;
    [SerializeField] private string spriteAnimState = "Warning_Guard";
    private int spriteAnimHash;

    private DummySpawn spawner;

    [Header("Exit Button")]
    [SerializeField] private Button exitBtn;

    private enum DummyButtonType
    {
        Normal,
        Guard,
        Parry,
        Dodge
    }

    [Serializable]
    private struct ButtonBind
    {
        public DummyButtonType type;
        public Button button;
        public DummyEnemyDataHolder holder;
        public GameObject dummyPrefab;
    }

    [SerializeField] private List<ButtonBind> buttonList;

    private readonly Dictionary<DummyButtonType, Button> buttonDic = new();

    // 콜백 함수 매칭용
    private readonly Dictionary<Button, UnityEngine.Events.UnityAction> actionDic = new();

    private void Awake()
    {
        spriteAnimHash = Animator.StringToHash(spriteAnimState);

        foreach (ButtonBind bind in buttonList)
        {
            if (bind.button == null)
                continue;

            if (!buttonDic.TryAdd(bind.type, bind.button))
                continue;

            // 람다를 새로 만들기 때문에 그대로 AddListener에 넣으면 똑같은 콜백을 해제할 수 없음.
            UnityEngine.Events.UnityAction action = () => OnClickButton(bind.type);

            actionDic.Add(bind.button, action);
            bind.button.onClick.AddListener(action);

            if (bind.holder != null)
            {
                bind.holder.OnEnter -= HandlePointerEnter;
                bind.holder.OnExit -= HandlePointerExit;
                bind.holder.OnEnter += HandlePointerEnter;
                bind.holder.OnExit += HandlePointerExit;
            }
        }

        exitBtn.onClick.AddListener(CancelPopup);
        ShowDescription(null);
    }

    private void OnDestroy()
    {
        // 딕셔너리에 함께 등록한 콜백을 직접 해제
        foreach (var pair in actionDic)
            pair.Key.onClick.RemoveListener(pair.Value);

        foreach (ButtonBind bind in buttonList)
        {
            if (bind.holder != null)
            {
                bind.holder.OnEnter -= HandlePointerEnter;
                bind.holder.OnExit -= HandlePointerExit;
            }
        }

        exitBtn.onClick.RemoveListener(CancelPopup);
    }

    public void SetSpawner(DummySpawn spawner)
    {
        this.spawner = spawner;
    }

    private void OnClickButton(DummyButtonType type)
    {
        if (spawner == null)
            return;

        foreach (ButtonBind bind in buttonList)
        {
            if (bind.type != type || bind.dummyPrefab == null)
                continue;

            spawner.ReplaceDummy(bind.dummyPrefab);
            return;
        }
    }

    public void HandlePointerEnter(DummyAttackData dummyAttackData)
    {
        ShowDescription(dummyAttackData);

    }

    public void HandlePointerExit()
    {
        ShowDescription(null);
    }

    private void ShowDescription(DummyAttackData data)
    {
        if(descriptionText != null)
            descriptionText.text = data != null ? data.Description : string.Empty;

        if (data == null)
        {
            selectedSprite.enabled = false;
            return;
        }

        selectedSprite.sprite = data != null ? data.AttackTypeSprite : null;
        selectedSprite.color = data != null ? data.SpriteColor : Color.white;
        selectedSprite.enabled = true;

        if (selectedSpriteAnim != null)
            selectedSpriteAnim.Play(spriteAnimHash, 0, 0f);
    }

    public override void ClosePopup()
    {
        base.ClosePopup();

        spawner.HandlePopupClosed();
    }
}
