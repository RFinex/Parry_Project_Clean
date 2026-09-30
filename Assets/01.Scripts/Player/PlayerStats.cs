using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private float nowHp;
    [SerializeField] private float maxHp;

    [SerializeField] private float nowStamina;
    [SerializeField] private float maxStamina;

    public float NowHp => nowHp;
    public float NowStamina => nowStamina;
    
    public void Initialized(PlayerBaseData data)
    {
        maxHp = data.MaxHp;
        nowHp = maxHp;

        maxStamina = data.MaxStamina;
        nowStamina = maxStamina;
    }

    public void TakeDamage(float damage)
    {
        nowHp -= damage;

        if (nowHp <= 0)
            nowHp = 0f;
    }

    public bool UseStamina(float stamina)
    {
        if (nowStamina < stamina)
        {
            Debug.Log("스테미너 부족");
            return false;
        }

        nowStamina -= stamina;

        return true;
    }
}
