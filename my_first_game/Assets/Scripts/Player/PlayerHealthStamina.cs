using System;
using UnityEngine;

public class PlayerHealthStamina : MonoBehaviour, IDamageable
{
    [SerializeField] private PlayerStatsData stats;

    public float CurrentHealth { get; private set; }
    public float CurrentStamina { get; private set; }

    public float MaxHealth  => stats.maxHealth;
    public float MaxStamina => stats.maxStamina;
    public float DashCost   => stats.dashCost;
    public float ParryCost  => stats.parryCost;
    public float BaseAttack => stats.baseAttack;

    /// <summary>[추가] 체력이 0이면 사망</summary>
    public bool IsDead => CurrentHealth <= 0f;

    /// <summary>지금 무적 상태인지</summary>
    public bool IsInvincible => invincibleCount > 0;

    public event Action<float, float> OnHealthChanged;   // (현재, 최대)
    public event Action<float, float> OnStaminaChanged;  // (현재, 최대)
    public event Action<float> OnDamaged;                // [추가] 실제로 피해를 입었을 때 (데미지)
    public event Action OnDeath;
    public event Action OnStaminaExhausted;

    private float regenTimer;
    private int invincibleCount;   // 무적을 요청한 곳의 개수

    private void Awake()
    {
        if (stats == null)
        {
            Debug.LogError($"{nameof(PlayerHealthStamina)}: PlayerStatsData 에셋이 연결되지 않았습니다.", this);
            enabled = false;
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

    public bool TryConsumeStamina(float amount)
    {
        if (CurrentStamina >= amount)
        {
            CurrentStamina -= amount;
            regenTimer = stats.regenDelay;
            OnStaminaChanged?.Invoke(CurrentStamina, stats.maxStamina);
            return true;
        }

        Debug.Log("스태미너 부족!");
        OnStaminaExhausted?.Invoke();
        return false;
    }

    public void PaybackStamina(float amount)
    {
        CurrentStamina = Mathf.Min(CurrentStamina + amount, stats.maxStamina);
        OnStaminaChanged?.Invoke(CurrentStamina, stats.maxStamina);
    }

    public void OnParrySuccess()
    {
        PaybackStamina(stats.parryPayback);
    }

    public void BeginInvincible()
    {
        invincibleCount++;
    }

    public void EndInvincible()
    {
        invincibleCount = Mathf.Max(0, invincibleCount - 1);
    }

    public void TakeDamage(float amount, Vector3 hitPoint)
    {
        if (IsDead) return;   // [추가] 이미 죽었으면 더 맞지 않음

        if (IsInvincible)
        {
            Debug.Log("[플레이어] 무적 상태라 피해 무시");
            return;
        }

        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
        OnHealthChanged?.Invoke(CurrentHealth, stats.maxHealth);
        Debug.Log($"[플레이어 피격] 데미지: {amount}, 남은 체력: {CurrentHealth}");

        OnDamaged?.Invoke(amount);   // [추가] 경직 등 피격 반응용

        if (IsDead)
        {
            OnDeath?.Invoke();
            Debug.Log("[플레이어 사망]");
        }
    }

    public void ResetHealth()
    {
        CurrentHealth = stats.maxHealth;
        OnHealthChanged?.Invoke(CurrentHealth, stats.maxHealth);
    }
}