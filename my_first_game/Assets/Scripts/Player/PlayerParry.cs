using System;
using UnityEngine;

/// <summary>
/// 스페이스바(Parry 액션)로 패링 자세를 잡는다. 판정은 '타이밍'만으로 한다.
/// 패링 시작은 PlayerStateController의 허락을 받고, 대쉬 등으로 상태가 바뀌면 스스로 정리한다.
/// </summary>
public class PlayerParry : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInputHandler input;
    [SerializeField] private PlayerStateController playerState;   // [추가]
    // [삭제] PlayerAttack attack → 공격 취소는 상태 관리가 대신 함
    [Tooltip("패링 애니메이션이 들어있는 Animator (Animator 창에 'Parry', 'Idle' 상태가 있어야 함)")]
    [SerializeField] private Animator animator;
    [Tooltip("스태미나 소모/페이백용")]
    [SerializeField] private PlayerHealthStamina stamina;

    [Header("Parry Timing (초)")]
    [Tooltip("패링 입력 후 이 시간 안에 적 공격이 적중하면 퍼펙트 패리")]
    [SerializeField, Min(0.01f)] private float perfectWindow = 0.1f;
    [Tooltip("패링 입력 후 이 시간 안에 적 공격이 적중하면 일반 패리 (퍼펙트 시간 포함)")]
    [SerializeField, Min(0.05f)] private float normalWindow = 0.3f;
    [Tooltip("아무것도 막지 못하고 패링이 끝났을 때, 다시 쓸 수 있기까지의 시간")]
    [SerializeField, Min(0f)] private float missCooldown = 0.2f;

    public bool IsParrying { get; private set; }

    public event Action<ParryResult, EnemyAttackData> ParrySucceeded;

    private static readonly int ParryStateHash = Animator.StringToHash("Parry");
    private static readonly int IdleStateHash = Animator.StringToHash("Idle");

    private float parryStartTime;
    private float nextParryTime;

    private void Awake()
    {
        if (input == null || playerState == null || stamina == null)
        {
            Debug.LogError($"{nameof(PlayerParry)}: input, playerState, stamina 중 연결되지 않은 것이 있습니다.", this);
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

        // [수정] 패리 가능 여부는 상태 관리에게 물어본다
        if (input.ParryPressed && Time.time >= nextParryTime && playerState.CanEnter(PlayerState.Parrying))
            StartParry();
    }

    private void StartParry()
    {
        if (!stamina.TryConsumeStamina(stamina.ParryCost)) return;

        playerState.TryEnter(PlayerState.Parrying);   // 공격·경직 중이었다면 여기서 자동으로 취소됨

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
        if (!IsParrying) return;

        IsParrying = false;
        nextParryTime = succeeded ? Time.time : Time.time + missCooldown;

        if (animator != null) animator.Play(IdleStateHash, 0, 0f);
        if (playerState != null) playerState.Exit(PlayerState.Parrying);
    }

    // [추가] 패리 도중 대쉬·경직·사망으로 상태가 바뀌면 패리를 정리한다 (패리 취소)
    private void OnStateChanged(PlayerState prev, PlayerState next)
    {
        if (IsParrying && next != PlayerState.Parrying) EndParry(false);
    }
}