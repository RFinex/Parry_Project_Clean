using System.Collections.Generic;
using UnityEngine;

public class DummySpawn : InteractableBase
{
    [SerializeField] private GameObject dummyPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform statusBarParent;

    [SerializeField] private bool allowMultiSpawn;

    private readonly Dictionary<EnemyController, EnemyStatusBar> spawnedEnemies = new();

    public override bool CanInteract => allowMultiSpawn || spawnedEnemies.Count == 0;

    public override void Interact(PlayerController interactor)
    {
        if (!CanInteract || dummyPrefab == null || spawnPoint == null)
            return;

        GameObject spawnedDummy = Instantiate(dummyPrefab, spawnPoint.position, spawnPoint.rotation);

        EnemyController spawnedEnemy = spawnedDummy.GetComponent<EnemyController>();

        if (spawnedEnemy == null)
        {
            Destroy(spawnedDummy);
            return;
        }

        EnemyStatusBar spawnedStatusBar = null;

        if (UIManager.Instance != null && statusBarParent != null)
        { 
            spawnedStatusBar = UIManager.Instance.GetUI<EnemyStatusBar>(UIName.Enemy_Status_Bar, spawnedEnemy.transform.position, statusBarParent);

            if (spawnedStatusBar != null)
            {
                spawnedEnemy.SetStatusBar(spawnedStatusBar);
                spawnedStatusBar.Bind(spawnedEnemy);
            }
        }

        spawnedEnemy.OnDeathFinished += HandleDummyDeath;
        spawnedEnemies.Add(spawnedEnemy, spawnedStatusBar);
        RefreshIcon();
    }

    private void HandleDummyDeath(EnemyController enemy)
    {

        enemy.OnDeathFinished -= HandleDummyDeath;

        if (spawnedEnemies.TryGetValue(enemy, out EnemyStatusBar statusBar))
        {
            if (statusBar != null)
                statusBar.ReturnPool();

            spawnedEnemies.Remove(enemy);
        }

        Destroy(enemy.gameObject);
        RefreshIcon();
    }
}
