using System.Collections.Generic;
using UnityEngine;

public class DummySpawn : InteractableBase
{
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform statusBarParent;

    private bool isPopupOpen;
    private EnemyController spawnedEnemy;
    private EnemyStatusBar spawnedStatusBar;

    public override bool CanInteract => !isPopupOpen;

    public override void Interact(PlayerController interactor)
    {
        if (!CanInteract || UIManager.Instance == null)
            return;

        isPopupOpen = UIManager.Instance.OpenSpawnDummyPopup(this);
        RefreshIcon();
    }

    public void HandlePopupClosed()
    {
        isPopupOpen = false;
        RefreshIcon();
    }

    /// <summary>
    /// 더미 소환 시, 기존 더미 제거 후 재소화
    /// </summary>
    /// <param name="prefab"> 교체할 프리팹 </param>
    /// <returns></returns>
    public bool ReplaceDummy(GameObject prefab)
    {
        if (prefab == null || spawnPoint == null)
            return false;

        RemoveCurrentDummy();

        GameObject instance = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
        EnemyController enemy = instance.GetComponent<EnemyController>();

        if (enemy == null)
        {
            Destroy(instance);
            return false;
        }

        spawnedEnemy = enemy;

        if (UIManager.Instance != null && statusBarParent != null)
        {
            spawnedStatusBar = UIManager.Instance.GetUI<EnemyStatusBar>(UIName.Enemy_Status_Bar, enemy.transform.position, statusBarParent);

            if (spawnedStatusBar != null)
            {
                enemy.SetStatusBar(spawnedStatusBar);
                spawnedStatusBar.Bind(enemy);
            }
        }

        enemy.OnDeathFinished -= HandleDummyDeath;
        enemy.OnDeathFinished += HandleDummyDeath;
        return true;
    }

    /// <summary>
    /// 소환중인 더미 제거
    /// </summary>
    private void RemoveCurrentDummy()
    {
        if (spawnedEnemy != null)
        {
            spawnedEnemy.OnDeathFinished -= HandleDummyDeath;
            Destroy(spawnedEnemy.gameObject);
            spawnedEnemy = null;
        }

        if (spawnedStatusBar != null)
        {
            spawnedStatusBar.ReturnPool();
            spawnedStatusBar = null;
        }
    }

    private void HandleDummyDeath(EnemyController enemy)
    {
        if (enemy != spawnedEnemy)
            return;

        enemy.OnDeathFinished -= HandleDummyDeath;

        if (spawnedStatusBar != null)
            spawnedStatusBar.ReturnPool();

        spawnedEnemy = null;
        spawnedStatusBar = null;

        Destroy(enemy.gameObject);
    }
}
