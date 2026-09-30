using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private int nowHp;
    [SerializeField] private int maxHp;
    
    private void Initialized(PlayerBaseData data)
    {
        maxHp = data.MaxHp;
    }
}
