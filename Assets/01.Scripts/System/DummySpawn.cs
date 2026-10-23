using UnityEngine;

public class DummySpawn : InteractableBase
{
    [SerializeField] private GameObject dummyPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform statusBarParent;

    private GameObject spawnedDummy;
    private EnemyController spawnedEnemy;
    private EnemyStatusBar spawnedStatusBar;

    public override bool CanInteract => spawnedEnemy == null;

    public override void Interact(PlayerController interactor)
    {
        if (!CanInteract || dummyPrefab == null || spawnPoint == null)
            return;

        spawnedDummy = Instantiate(
            dummyPrefab,
            spawnPoint.position,
            spawnPoint.rotation);

        spawnedEnemy = spawnedDummy.GetComponent<EnemyController>();

        if (spawnedEnemy == null)
        {
            Destroy(spawnedDummy);
            spawnedDummy = null;
            return;
        }

        spawnedEnemy.OnDeathFinished += HandleDummyDeath;

        if (UIManager.Instance != null && statusBarParent != null)
        { 
            spawnedStatusBar = UIManager.Instance.GetUI<EnemyStatusBar>(UIName.Enemy_Status_Bar, spawnedEnemy.transform.position, statusBarParent);

            if (spawnedStatusBar != null)
            {
                spawnedEnemy.SetStatusBar(spawnedStatusBar);
                spawnedStatusBar.Bind(spawnedEnemy);
            }
        }

        RefreshIcon();
    }

    private void HandleDummyDeath(EnemyController enemy)
    {

        enemy.OnDeathFinished -= HandleDummyDeath;

        if (spawnedStatusBar != null)
            spawnedStatusBar.ReturnPool();

        spawnedStatusBar = null;
        spawnedEnemy = null;
        spawnedEnemy = null;

        Destroy(enemy.gameObject);
        RefreshIcon();
    }
}
