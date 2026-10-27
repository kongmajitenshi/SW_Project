using UnityEngine;

/// <summary>
/// [새 파일] 플레이어의 "최종 수치"를 알려주는 곳.
/// 지금은 PlayerStatsData 값을 그대로 돌려주지만, 장비 시스템이 생기면
/// "기본값 + 장비 보너스"를 여기서 계산한다.
/// 다른 스크립트는 수치가 필요하면 SO를 직접 들고 있지 말고 여기서만 읽는다.
/// </summary>
public class PlayerStats : MonoBehaviour
{
    [SerializeField] private PlayerStatsData data;

    /// <summary>데이터가 연결돼 있는지 (다른 스크립트가 시작할 때 확인용)</summary>
    public bool IsValid => data != null;

    // 체력
    public float MaxHealth => data.maxHealth;

    // 공격
    public float BaseAttack => data.baseAttack;

    // 스태미나
    public float MaxStamina => data.maxStamina;
    public float StaminaRegenRate => data.staminaRegenRate;
    public float StaminaRegenDelay => data.regenDelay;

    // 행동별 스태미나
    public float DashCost => data.dashCost;
    public float ParryCost => data.parryCost;
    public float ParryPayback => data.parryPayback;

    private void Awake()
    {
        if (data == null)
        {
            Debug.LogError($"{nameof(PlayerStats)}: PlayerStatsData 에셋이 연결되지 않았습니다.", this);
            enabled = false;
        }
    }
}