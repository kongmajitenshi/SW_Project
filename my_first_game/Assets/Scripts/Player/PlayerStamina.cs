using System;
using UnityEngine;

/// <summary>
/// [새 파일] 플레이어의 스태미나를 관리한다. (기존 PlayerHealthStamina의 스태미나 부분)
/// 소모 시도, 회복(시간 경과 + 페이백)을 담당한다.
/// 각 행동이 얼마나 쓰는지는 행동 쪽이 PlayerStats에서 읽어서 넘겨준다.
/// </summary>
[RequireComponent(typeof(PlayerStats))]
public class PlayerStamina : MonoBehaviour
{
    private PlayerStats stats;
    private float regenTimer;

    public float CurrentStamina { get; private set; }
    public float MaxStamina => stats.MaxStamina;

    public event Action<float, float> OnStaminaChanged;  // (현재, 최대)
    public event Action OnStaminaExhausted;              // 소모하려 했는데 부족할 때

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();

        if (stats == null || !stats.IsValid)
        {
            Debug.LogError($"{nameof(PlayerStamina)}: PlayerStats가 없거나 데이터가 연결되지 않았습니다.", this);
            enabled = false;
            return;
        }

        CurrentStamina = stats.MaxStamina;
    }

    private void Update()
    {
        if (regenTimer > 0f)
        {
            regenTimer -= Time.deltaTime;
            return;
        }

        if (CurrentStamina < MaxStamina)
        {
            CurrentStamina = Mathf.Min(CurrentStamina + stats.StaminaRegenRate * Time.deltaTime, MaxStamina);
            OnStaminaChanged?.Invoke(CurrentStamina, MaxStamina);
        }
    }

    /// <summary>스태미나가 충분하면 소모하고 true, 부족하면 false (예전 TryConsumeStamina)</summary>
    public bool TryConsume(float amount)
    {
        if (CurrentStamina >= amount)
        {
            CurrentStamina -= amount;
            regenTimer = stats.StaminaRegenDelay;
            OnStaminaChanged?.Invoke(CurrentStamina, MaxStamina);
            return true;
        }

        Debug.Log("스태미너 부족!");
        OnStaminaExhausted?.Invoke();
        return false;
    }

    /// <summary>스태미나를 즉시 회복 (패리 페이백 등) (예전 PaybackStamina)</summary>
    public void Recover(float amount)
    {
        CurrentStamina = Mathf.Min(CurrentStamina + amount, MaxStamina);
        OnStaminaChanged?.Invoke(CurrentStamina, MaxStamina);
    }
}