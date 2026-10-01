using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    [SerializeField] private float attackDamage = 5f;

    public void Attack(Transform target)
    {
        if (target == null)
            return;

        Utils.Log<EnemyCombat>($"Рћ АјАн : {target.name}");
    }
}
