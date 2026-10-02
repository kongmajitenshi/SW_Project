using System;
using UnityEngine;

public class PlayerHealthStamina : MonoBehaviour, IDamageable
{
    [SerializeField] private PlayerStatsData stats;

    public float CurrentHealth { get; private set; }
    public float CurrentStamina { get; private set; }

    // 다른 스크립트가 SO를 직접 들고 있지 않아도 되도록 여기서 꺼내준다
    public float MaxHealth  => stats.maxHealth;
    public float MaxStamina => stats.maxStamina;
    public float DashCost   => stats.dashCost;
    public float ParryCost  => stats.parryCost;
    public float BaseAttack => stats.baseAttack;
    
    public event Action<float, float> OnHealthChanged;   // (현재, 최대)
    public event Action<float, float> OnStaminaChanged;  // (현재, 최대)
    public event Action OnDeath;
    public event Action OnStaminaExhausted;

    private float regenTimer;

    private void Awake()
    {
        if (stats == null)
        {
            Debug.LogError($"{nameof(PlayerHealthStamina)}: PlayerStatsData 에셋이 연결되지 않았습니다.", this);
            enabled = false; // Update가 돌면서 에러를 매 프레임 뿜지 않도록 꺼둔다
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

    /// <summary>스태미나가 충분하면 소모하고 true, 부족하면 false</summary>
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

    /// <summary>패링 성공 시 PlayerParry가 호출</summary>
    public void OnParrySuccess()
    {
        PaybackStamina(stats.parryPayback);
    }

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

    public void ResetHealth()
    {
        CurrentHealth = stats.maxHealth;
        OnHealthChanged?.Invoke(CurrentHealth, stats.maxHealth);
    }
}