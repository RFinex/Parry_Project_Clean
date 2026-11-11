using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.InputManagerEntry;

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

    private void Awake()
    {
        spriteAnimHash = Animator.StringToHash(spriteAnimState);

        foreach (ButtonBind bind in buttonList)
        {
            if (bind.button == null)
                continue;

            if (!buttonDic.TryAdd(bind.type, bind.button))
                continue;

            bind.button.onClick.AddListener(() => OnClickButton(bind.type));

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
        foreach (ButtonBind bind in buttonList)
        {
            bind.button.onClick.RemoveListener(() => OnClickButton(bind.type));

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

        spawner?.HandlePopupClosed();
    }
}
