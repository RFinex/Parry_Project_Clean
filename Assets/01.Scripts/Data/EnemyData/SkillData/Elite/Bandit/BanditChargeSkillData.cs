using UnityEngine;

[CreateAssetMenu(fileName = "BanditChargeSkillData", menuName = "Enemy/Skills/Bandit Charge")]
public class BanditChargeSkillData : EnemySkillData
{
    [SerializeField] private float chargeSpeed = 8f;
    [SerializeField] private float chargeDuration = 0.35f;

    public float ChargeSpeed => chargeSpeed;
    public float ChargeDuration => chargeDuration;
}
