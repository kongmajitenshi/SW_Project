using UnityEngine;

// 이 속성을 적어주면 유니티 에디터 우클릭 메뉴에 생성 버튼이 추가됩니다.
[CreateAssetMenu(fileName = "NewPlayerStats", menuName = "Combat/PlayerStatsData")]
public class PlayerStatsData : ScriptableObject
{
    [Header("Attack")]
    public float baseAttack = 10f;         // 플레이어 기본 공격력 (무기 공격력이 여기에 더해짐)

    
    [Header("Health")]
    public float maxHealth = 100f;

    [Header("Stamina")]
    public float maxStamina = 100f;
    public float dashCost = 15f;           // 대시(회피) 소모량
    public float parryCost = 20f;          // 패링 시도 소모량
    public float parryPayback = 30f;       // 패링 성공 시 돌려받는 양
    public float staminaRegenRate = 40f;   // 초당 스태미너 회복량
    public float regenDelay = 1.0f;        // 소모 후 회복 시작까지 대기 시간
}