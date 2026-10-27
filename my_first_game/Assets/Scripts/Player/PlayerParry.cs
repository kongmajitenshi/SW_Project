using System;
using UnityEngine;

/// <summary>
/// 스페이스바(Parry 액션)로 패리 자세를 잡는다. 판정은 '타이밍'만으로 한다.
/// 패리 시작은 PlayerStateController의 허락을 받고, 대쉬 등으로 상태가 바뀌면 스스로 정리한다.
/// 성공 시 스태미나 페이백도 여기서 처리한다.
/// </summary>
[RequireComponent(typeof(PlayerInputHandler), typeof(PlayerStateController))]   // [추가]
[RequireComponent(typeof(PlayerStats), typeof(PlayerStamina))]                  // [추가]
public class PlayerParry : MonoBehaviour
{
    [Header("References")]
    // [삭제] input, playerState, stamina 칸 → Awake에서 자동으로 찾음
    [Tooltip("패리 애니메이션이 들어있는 Animator (Animator 창에 'Parry', 'Idle' 상태가 있어야 함)")]
    [SerializeField] private Animator animator;

    [Header("Parry Timing (초)")]
    [Tooltip("패리 입력 후 이 시간 안에 적 공격이 적중하면 퍼펙트 패리")]
    [SerializeField, Min(0.01f)] private float perfectWindow = 0.1f;
    [Tooltip("패리 입력 후 이 시간 안에 적 공격이 적중하면 일반 패리 (퍼펙트 시간 포함)")]
    [SerializeField, Min(0.05f)] private float normalWindow = 0.3f;
    [Tooltip("아무것도 막지 못하고 패리가 끝났을 때, 다시 쓸 수 있기까지의 시간")]
    [SerializeField, Min(0f)] private float missCooldown = 0.2f;

    public bool IsParrying { get; private set; }

    public event Action<ParryResult, EnemyAttackData> ParrySucceeded;

    private static readonly int ParryStateHash = Animator.StringToHash("Parry");
    private static readonly int IdleStateHash = Animator.StringToHash("Idle");

    private PlayerInputHandler input;          // [수정] 자동 연결
    private PlayerStateController playerState; // [수정] 자동 연결
    private PlayerStats stats;                 // [추가] 소모량·페이백 읽기용
    private PlayerStamina stamina;             // [수정] PlayerHealthStamina → PlayerStamina, 자동 연결

    private float parryStartTime;
    private float nextParryTime;

    private void Awake()
    {
        // [수정] 같은 오브젝트의 컴포넌트를 스스로 찾는다
        input = GetComponent<PlayerInputHandler>();
        playerState = GetComponent<PlayerStateController>();
        stats = GetComponent<PlayerStats>();
        stamina = GetComponent<PlayerStamina>();

        if (input == null || playerState == null || stats == null || stamina == null)
        {
            Debug.LogError($"{nameof(PlayerParry)}: 같은 오브젝트에 필요한 컴포넌트가 없습니다. " +
                           "(PlayerInputHandler, PlayerStateController, PlayerStats, PlayerStamina)", this);
            enabled = false;
        }
    }

    private void OnValidate()
    {
        if (perfectWindow > normalWindow) perfectWindow = normalWindow;
    }

    private void OnEnable()
    {
        if (playerState != null) playerState.StateChanged += OnStateChanged;
    }

    private void OnDisable()
    {
        if (playerState != null) playerState.StateChanged -= OnStateChanged;
        if (IsParrying) EndParry(false);
    }

    private void Update()
    {
        if (IsParrying)
        {
            if (Time.time - parryStartTime > normalWindow) EndParry(false);
            return;
        }

        if (input.ParryPressed && Time.time >= nextParryTime && playerState.CanEnter(PlayerState.Parrying))
            StartParry();
    }

    private void StartParry()
    {
        if (!stamina.TryConsume(stats.ParryCost)) return;   // [수정] 새 스태미나 스크립트, 소모량은 PlayerStats에서

        playerState.TryEnter(PlayerState.Parrying);

        IsParrying = true;
        parryStartTime = Time.time;

        if (animator != null) animator.Play(ParryStateHash, 0, 0f);
    }

    public ParryResult Judge(EnemyAttackData incoming)
    {
        if (incoming == null || !incoming.parryable) return ParryResult.None;
        if (!IsParrying) return ParryResult.None;

        float elapsed = Time.time - parryStartTime;
        if (elapsed > normalWindow) return ParryResult.None;

        ParryResult result = elapsed <= perfectWindow ? ParryResult.Perfect : ParryResult.Normal;

        stamina.Recover(stats.ParryPayback);   // [수정] 예전 stamina.OnParrySuccess() → 패리가 보상을 직접 정함
        Debug.Log(result == ParryResult.Perfect
            ? $"퍼펙트 패리! (입력 후 {elapsed:F3}초)"
            : $"패리 성공 (입력 후 {elapsed:F3}초)");

        ParrySucceeded?.Invoke(result, incoming);
        EndParry(true);
        return result;
    }

    private void EndParry(bool succeeded)
    {
        if (!IsParrying) return;

        IsParrying = false;
        nextParryTime = succeeded ? Time.time : Time.time + missCooldown;

        if (animator != null) animator.Play(IdleStateHash, 0, 0f);
        if (playerState != null) playerState.Exit(PlayerState.Parrying);
    }

    private void OnStateChanged(PlayerState prev, PlayerState next)
    {
        if (IsParrying && next != PlayerState.Parrying) EndParry(false);
    }
}