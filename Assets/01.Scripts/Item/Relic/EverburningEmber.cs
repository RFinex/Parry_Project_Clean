using UnityEngine;

public class EverburningEmber : Artifact
{
    private float effectHealthRange = 0.8f;
    private float maxDamageValue = 70f;

    // 기본값
    private StatPair damagePair = new()
    {
        id = StatId.AttackDamage,
        modifierType = StatModifierType.Percent,
        value = 0f
    };


    public EverburningEmber()
    {
        // HP가 낮을수록 일반 공격 피해 증가(20% 이하 최대 +70%)
    }

    public override void OnEquip(PlayerBlackBoard board)
    {
        if (IsEquipped)
            return;

        base.OnEquip(board);

        if (!IsEquipped)
            return;

        board.stats.OnHealthChanged -= UpdateDamageRatio;
        board.stats.OnHealthChanged += UpdateDamageRatio;

        UpdateDamageRatio(board.stats.NowHp, board.stats.MaxHp);
    }

    public override void OnUnequip(PlayerBlackBoard board)
    {
        if (!IsEquipped)
            return;

        board.stats.OnHealthChanged -= UpdateDamageRatio;

        base.OnUnequip(board);
    }

    private void UpdateDamageRatio(float nowHp, float maxHp)
    {
        if (maxHp <= 0f)
            return;

        float hpRatio = nowHp / maxHp;

        // 최대 데미지 증가 지점 기준 현재 비율 계산
        float effectRatio = Mathf.Clamp01((1f - hpRatio) / effectHealthRange);
        
        // 계산된 비율에 최대 증가율을 곱해 증가 데미지 계산
        float effectPercent = effectRatio * maxDamageValue;

        if (damagePair.value == effectPercent)
            return;

        RemovePair();

        damagePair.value = effectPercent;

        if (damagePair.value > 0f)
            blackBoard.stats.AddStats(damagePair);
    }

    private void RemovePair()
    {
        if (damagePair.value <= 0f)
            return;

        blackBoard.stats.RemoveStats(damagePair);
        damagePair.value = 0f;
    }
}
