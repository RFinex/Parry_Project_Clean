using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Attack_Data_Dummy", menuName = "Enemy/Dummy Attack")]
public class DummyAttackData : EnemyAttackData
{
    [SerializeField, TextArea] private string description;
    [SerializeField] private Sprite attackTypeSprite;
    [SerializeField] private Color spriteColor;

    public string Description => description;
    public Sprite AttackTypeSprite => attackTypeSprite;
    public Color SpriteColor => spriteColor;
}
