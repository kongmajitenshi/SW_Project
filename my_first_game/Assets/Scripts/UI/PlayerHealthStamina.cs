using System; // Action 이벤트를 사용하기 위해 필수로 필요합니다.
using UnityEngine;

public class PlayerHealthStamina : MonoBehaviour, IDamageable
{
    [SerializeField] private PlayerStatsData stats;

    // UI에서 읽어갈 수 있도록 public 프로퍼티 선언
    public float CurrentHealth { get; private set; }
    public float CurrentStamina { get; private set; }

    // UI가 구독할 이벤트 선언 (에러가 났던 원인)
    public event Action<float, float> OnHealthChanged;   // (현재 체력, 최대 체력)
    public event Action<float, float> OnStaminaChanged;  // (현재 스태미너, 최대 스태미너)
    public event Action OnDeath;
    public event Action OnStaminaExhausted;

    private float regenTimer;

    private void Awake()
    {
        if (stats == null)
        {
            Debug.LogError($"{nameof(PlayerHealthStamina)}: PlayerStatsData 에셋이 연결되지 않았습니다.", this);
            return;
        }

        CurrentHealth = stats.maxHealth;
        CurrentStamina = stats.maxStamina;
    }

    private void Update()
    {
        HandleStaminaRegen();
    }

    private void HandleStaminaRegen()
    {
        if (regenTimer > 0f)
        {
            regenTimer -= Time.deltaTime;
            return;
        }

        if (CurrentStamina < stats.maxStamina)
        {
            CurrentStamina = Mathf.Min(CurrentStamina + stats.staminaRegenRate * Time.deltaTime, stats.maxStamina);
            OnStaminaChanged?.Invoke(CurrentStamina, stats.maxStamina);
        }
    }

    // 스태미너 소모 시도 (대시, 패링에서 호출)
    public bool TryConsumeStamina(float amount)
    {
        if (CurrentStamina >= amount)
        {
            CurrentStamina -= amount;
            regenTimer = stats.regenDelay;
            OnStaminaChanged?.Invoke(CurrentStamina, stats.maxStamina);
            return true;
        }

        OnStaminaExhausted?.Invoke();
        return false;
    }

    // 패링 성공 시 스태미너 페이백
    public void PaybackStamina(float amount)
    {
        CurrentStamina = Mathf.Min(CurrentStamina + amount, stats.maxStamina);
        OnStaminaChanged?.Invoke(CurrentStamina, stats.maxStamina);
    }

    // IDamageable 구현 (피격 시 체력 감소 및 UI 알림)
    public void TakeDamage(float amount, Vector3 hitPoint)
    {
        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
        OnHealthChanged?.Invoke(CurrentHealth, stats.maxHealth);

        Debug.Log($"[플레이어 피격] 데미지: {amount}, 남은 체력: {CurrentHealth}");

        if (CurrentHealth <= 0f)
        {
            OnDeath?.Invoke();
            Debug.Log("[플레이어 사망]");
        }
    }

    // 구역 클리어 시 체력 완전 회복
    public void ResetHealth()
    {
        if (stats == null) return;
        CurrentHealth = stats.maxHealth;
        OnHealthChanged?.Invoke(CurrentHealth, stats.maxHealth);
    }
}
