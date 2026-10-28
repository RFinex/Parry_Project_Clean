using UnityEngine;

public class TestItemDrop : MonoBehaviour
{
    [Header("아이템 프리팹 풀 ID")]
    [SerializeField] private int pickupPoolId;

    [Header("생성할 아이템 데이터 ID")]
    [SerializeField] private int itemId = 1000001;

    [ContextMenu("테스트 아이템 드롭")]
    private void DropItem()
    {
        if (!Application.isPlaying)
        {
            Utils.LogWarning<TestItemDrop>("플레이 모드에서 실행 가능");
            return;
        }

        GameManager gameManager = GameManager.Instance;
        ObjectPoolManager poolManager = ObjectPoolManager.Instance;

        if (gameManager == null || gameManager.ItemFactory == null || poolManager == null)
        {
            Utils.LogWarning<TestItemDrop>("Factory 또는 PoolManager 초기화되지 않음");
            return;
        }

        Item item = gameManager.CreateItem(itemId);

        if (item == null)
            return;

        if (item is not Artifact)
        {
            Utils.LogWarning<TestItemDrop>("Artifact만 획득 가능");
            return;
        }

        ItemPickupPool pickup = poolManager.Get<ItemPickupPool>(pickupPoolId, transform.position, null);

        if (pickup == null)
        {
            Utils.LogWarning<TestItemDrop>($"풀에 등록되지 않은 아이템 : {pickupPoolId}");
            return;
        }

        pickup.SetItem(item);
    }
}
