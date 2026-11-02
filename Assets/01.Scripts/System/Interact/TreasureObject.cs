using UnityEngine;

public class TreasureObject : InteractableBase
{
    [Header("Reward")]
    [SerializeField] private TreasureChestData chestData;

    [Header("Animation")]
    [SerializeField] private Animator chestAnimator;
    [SerializeField] private string openAnimName = "Open";

    [Header("Item Drop Position")]
    [SerializeField] private Transform dropTransform;

    [Header("Pickup Pool Id")]
    [SerializeField] private int pickupPoolId;

    public Vector3 DropPos => dropTransform != null ? dropTransform.position : transform.position;

    private int openAnimHash;

    private bool isAvailable = true;

    public TreasureChestData ChestData => chestData;

    private ArtifactSystem artifactSystem;

    public override bool CanInteract => isAvailable && chestData != null;

    protected override void Awake()
    {
        base.Awake();

        openAnimHash = string.IsNullOrEmpty(openAnimName) ? 0 : Animator.StringToHash(openAnimName);

        if (chestAnimator != null)
            chestAnimator.enabled = false;
    }

    public override void Interact(PlayerController interactor)
    {
        if (!CanInteract)
            return;

        ServiceLocator.Inject(out artifactSystem);

        TryDropArtifact();

        SetAvailable(false);
        Open();

        EventBus.Publish(new TreasureSelectedEvent(this));
    }

    /// <summary>
    /// 상자 상호작용 설정 메소드
    /// </summary>
    /// <param name="isAvailable"></param>
    public void SetAvailable(bool isAvailable)
    {
        this.isAvailable = isAvailable;
        RefreshIcon();
    }

    /// <summary>
    /// 상자 Open 애니메이션 실행
    /// </summary>
    public void Open()
    {
        if (chestAnimator == null || openAnimHash == 0)
            return;

        chestAnimator.enabled = true;
        chestAnimator.Play(openAnimHash, 0, 0f);
    }

    /// <summary>
    /// 현재 상자 기준 등급을 뽑고 상자 위치에 아티팩트 드롭
    /// </summary>
    /// <returns></returns>
    private bool TryDropArtifact()
    {
        if (chestData == null || artifactSystem == null)
        {
            Utils.LogWarning<TreasureObject>("상자 프리팹에 data 연결이 안됨 혹은 artifactSystem이 비어있음");
            return false;
        }

        // SO에 설정된 가중치로 등급 뽑기
        if (!artifactSystem.TryRollRarity(chestData, out ItemRarity rarity))
        {
            Utils.LogWarning<TreasureObject>("상자별 등급 가중치 재확인 필요");
            return false;
        }

        ObjectPoolManager poolManager = ObjectPoolManager.Instance;

        if (poolManager == null)
            return false;

        // 뽑힌 등급에 해당하는 무작위 아티팩트 하나를 data로 반환
        if (!artifactSystem.TryGetRandomArtifactData(rarity, out ArtifactData data))
        {
            Utils.LogWarning<TreasureObject>($"{rarity} 등급 아티팩트가 존재하지 않음");
            return false;
        }

        // 선택된 data로 아이템 객체 생성
        Item item = artifactSystem.CreateArtifact(data.Id);

        // Artifact 타입 검사
        if (item is not Artifact)
        {
            Utils.LogWarning<TreasureObject>($"아티팩트 생성 실패 : {data.Id}");
            return false;
        }

        // 공용 픽업 프리팹 가져오기
        ItemPickupPool pickup = poolManager.Get<ItemPickupPool>(pickupPoolId, DropPos, null);

        if (pickup == null)
        {
            Utils.LogWarning<TreasureObject>($"픽업 풀 발견 실패 : {pickupPoolId}");
            return false;
        }

        // 픽업 프리팹에 아이템을 넣고 초기화
        pickup.SetItem(item);
        // 드롭풀에서 해당 아티팩트 제외
        artifactSystem.MarkArtifactDropped(data.Id);
        return true;
    }
}
