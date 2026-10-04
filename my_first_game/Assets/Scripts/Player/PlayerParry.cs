using System;
using UnityEngine;

/// <summary>
/// 스페이스바(Parry 액션)로 패링 자세를 잡는다. 판정은 '타이밍'만으로 한다 (좌표 범위 없음).
/// 적이 공격을 적중시키는 순간 Judge()를 호출하면,
/// 패링 입력 후 지난 시간으로 퍼펙트 / 일반 / 실패를 판정해서 돌려준다.
/// </summary>
public class PlayerParry : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInputHandler input;
    [Tooltip("패링 중 공격을 막고, 발동 시 진행 중인 공격을 취소하기 위해 사용한다.")]
    [SerializeField] private PlayerAttack attack;
    [Tooltip("패링 애니메이션이 들어있는 Animator (Animator 창에 'Parry', 'Idle' 상태가 있어야 함)")]
    [SerializeField] private Animator animator;
    [Tooltip("스태미나 소모/페이백용")]
    [SerializeField] private PlayerHealthStamina stamina;

    [Header("Parry Timing (초)")]
    [Tooltip("패링 입력 후 이 시간 안에 적 공격이 적중하면 퍼펙트 패리")]
    [SerializeField, Min(0.01f)] private float perfectWindow = 0.1f;
    [Tooltip("패링 입력 후 이 시간 안에 적 공격이 적중하면 일반 패리 (퍼펙트 시간 포함)")]
    [SerializeField, Min(0.05f)] private float normalWindow = 0.3f;
    [Tooltip("아무것도 막지 못하고 패링이 끝났을 때, 다시 쓸 수 있기까지의 시간 (헛패링 연타 방지)")]
    [SerializeField, Min(0f)] private float missCooldown = 0.2f;

    /// <summary>패링 자세 중이면 true (입력 후 normalWindow 동안)</summary>
    public bool IsParrying { get; private set; }

    /// <summary>패링 성공 시 호출 (결과, 막아낸 공격). 역경직/카메라 연출 등을 여기에 연결하면 된다.</summary>
    public event Action<ParryResult, EnemyAttackData> ParrySucceeded;

    private static readonly int ParryStateHash = Animator.StringToHash("Parry");
    private static readonly int IdleStateHash = Animator.StringToHash("Idle");

    private float parryStartTime;
    private float nextParryTime;

    private void Awake()
    {
        if (input == null || stamina == null)
        {
            Debug.LogError($"{nameof(PlayerParry)}: input 또는 stamina가 연결되지 않았습니다.", this);
            enabled = false;
        }
    }

    // 인스펙터에서 값을 바꿀 때마다 호출된다: 퍼펙트 시간이 일반 시간보다 길어지지 않게 막는다
    private void OnValidate()
    {
        if (perfectWindow > normalWindow) perfectWindow = normalWindow;
    }

    private void OnDisable()
    {
        if (IsParrying) EndParry(false);
    }

    private void Update()
    {
        if (IsParrying)
        {
            // 일반 패리 시간이 다 지나도록 아무 공격도 안 왔다 = 헛패링
            if (Time.time - parryStartTime > normalWindow) EndParry(false);
            return;
        }

        if (input.ParryPressed && Time.time >= nextParryTime && CanStartParry())
            StartParry();
    }

    private bool CanStartParry() => attack == null || !attack.InputLocked;

    private void StartParry()
    {
        if (!stamina.TryConsumeStamina(stamina.ParryCost)) return;

        IsParrying = true;
        parryStartTime = Time.time; // 판정의 기준이 되는 "입력 시각"

        if (attack != null)
        {
            attack.CancelAttack();
            attack.InputLocked = true;
        }

        if (animator != null) animator.Play(ParryStateHash, 0, 0f);
    }

    /// <summary>
    /// 적이 공격을 적중시키는 순간 호출한다. 패링 결과를 돌려준다.
    /// 성공(Normal/Perfect)이면 스태미나 페이백까지 여기서 처리한다.
    /// </summary>
    public ParryResult Judge(EnemyAttackData incoming)
    {
        if (incoming == null || !incoming.parryable) return ParryResult.None; // 패리 불가 공격
        if (!IsParrying) return ParryResult.None;                              // 패링 안 누름

        float elapsed = Time.time - parryStartTime;          // 입력 후 지난 시간
        if (elapsed > normalWindow) return ParryResult.None; // 너무 일찍 누름

        ParryResult result = elapsed <= perfectWindow ? ParryResult.Perfect : ParryResult.Normal;

        stamina.OnParrySuccess();
        Debug.Log(result == ParryResult.Perfect
            ? $"퍼펙트 패리! (입력 후 {elapsed:F3}초)"
            : $"패리 성공 (입력 후 {elapsed:F3}초)");

        ParrySucceeded?.Invoke(result, incoming);
        EndParry(true);
        return result;
    }

    private void EndParry(bool succeeded)
    {
        IsParrying = false;

        // 성공하면 바로 다음 패링 가능 (연속 공격 대응), 헛패링이면 짧은 쿨타임
        nextParryTime = succeeded ? Time.time : Time.time + missCooldown;

        if (attack != null) attack.InputLocked = false;
        if (animator != null) animator.Play(IdleStateHash, 0, 0f);
    }
}