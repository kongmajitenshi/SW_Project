using System;
using UnityEngine;

/// <summary>
/// [새 파일] 플레이어의 체력을 관리한다. (기존 PlayerHealthStamina의 체력 부분)
/// 데미지를 받는 유일한 입구(TakeDamage)이고, 무적 검사와 사망 판정도 여기서 한다.
/// 언제 무적인지는 여기서 정하지 않는다 (PlayerInvincibility 등이 요청).
/// </summary>
[RequireComponent(typeof(PlayerStats))]
public class PlayerHealth : MonoBehaviour, IDamageable
{
    private PlayerStats stats;
    private int invincibleCount;   // 무적을 요청한 곳의 개수 (0보다 크면 무적)

    public float CurrentHealth { get; private set; }
    public float MaxHealth => stats.MaxHealth;
    public bool IsDead => CurrentHealth <= 0f;
    public bool IsInvincible => invincibleCount > 0;

    public event Action<float, float> OnHealthChanged;   // (현재, 최대)
    public event Action<float> OnDamaged;                // 실제로 피해를 입었을 때 (데미지)
    public event Action OnDeath;

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();

        if (stats == null || !stats.IsValid)
        {
            Debug.LogError($"{nameof(PlayerHealth)}: PlayerStats가 없거나 데이터가 연결되지 않았습니다.", this);
            enabled = false;
            return;
        }

        CurrentHealth = stats.MaxHealth;
    }

    public void TakeDamage(float amount, Vector3 hitPoint)
    {
        if (IsDead) return;

        if (IsInvincible)
        {
            Debug.Log("[플레이어] 무적 상태라 피해 무시");
            return;
        }

        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        Debug.Log($"[플레이어 피격] 데미지: {amount}, 남은 체력: {CurrentHealth}");

        OnDamaged?.Invoke(amount);

        if (IsDead)
        {
            OnDeath?.Invoke();
            Debug.Log("[플레이어 사망]");
        }
    }

    /// <summary>무적 시작 요청. 반드시 EndInvincible()과 짝을 맞춰 호출할 것.</summary>
    public void BeginInvincible()
    {
        invincibleCount++;
    }

    /// <summary>무적 종료 요청.</summary>
    public void EndInvincible()
    {
        invincibleCount = Mathf.Max(0, invincibleCount - 1);
    }

    /// <summary>체력을 최대로 회복 (구역 클리어, 불러오기 등)</summary>
    public void ResetHealth()
    {
        CurrentHealth = MaxHealth;
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }
}