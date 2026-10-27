using UnityEngine;

/// <summary>
/// 걷기와 대쉬(회피)를 담당한다. 대쉬 시작은 PlayerStateController의 허락을 받는다.
/// 대쉬 중 무적은 이 스크립트가 아니라 PlayerInvincibility가 상태를 보고 처리한다.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(PlayerInputHandler), typeof(PlayerStateController))]   // [수정]
[RequireComponent(typeof(PlayerStats), typeof(PlayerStamina))]                                        // [추가]
public class PlayerMovementSmooth : MonoBehaviour
{
    // [삭제] [Header("References")] input, playerState, stamina 칸 → Awake에서 자동으로 찾음

    [Header("Move")]
    [SerializeField] private float moveSpeed = 6f;
    [Tooltip("0에서 최고속도까지 걸리는 시간. 짧을수록 반응이 빠르고, 길수록 묵직하다.")]
    [SerializeField, Min(0.01f)] private float accelTime = 0.08f;
    [Tooltip("최고속도에서 멈추기까지 걸리는 시간. 길수록 많이 미끄러진다.")]
    [SerializeField, Min(0.01f)] private float decelTime = 0.12f;
    [Tooltip("공격 중 이동 속도 배율 (0 = 멈춤, 1 = 그대로)")]
    [SerializeField, Range(0f, 1f)] private float moveWhileAttacking = 0.3f;

    [Header("Dash")]
    [Tooltip("대쉬 시작 순간의 속도 (곡선 값 1일 때)")]
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField, Min(0.05f)] private float dashDuration = 0.22f;
    [SerializeField] private float dashCooldown = 0.4f;
    [Tooltip("가로: 대쉬 진행도(0~1), 세로: 속도 배율. 처음 빠르고 끝날수록 느려지게 설정됨.")]
    [SerializeField] private AnimationCurve dashSpeedCurve = AnimationCurve.Linear(0f, 1f, 1f, 0.2f);

    private Rigidbody2D rb;
    private PlayerInputHandler input;          // [수정] 자동 연결
    private PlayerStateController playerState; // [수정] 자동 연결
    private PlayerStats stats;                 // [추가] 대쉬 소모량 읽기용
    private PlayerStamina stamina;             // [수정] PlayerHealthStamina → PlayerStamina, 자동 연결

    private Vector2 inputDir;
    private Vector2 moveDir;
    private Vector2 lastDir = Vector2.right;
    private Vector2 dashDir;
    private bool isDashing;
    private float dashTimer;
    private float nextDashTime;

    public bool IsDashing => isDashing;

    private void Awake()
    {
        // [수정] 같은 오브젝트의 컴포넌트를 스스로 찾는다
        rb = GetComponent<Rigidbody2D>();
        input = GetComponent<PlayerInputHandler>();
        playerState = GetComponent<PlayerStateController>();
        stats = GetComponent<PlayerStats>();
        stamina = GetComponent<PlayerStamina>();

        if (input == null || playerState == null || stats == null || stamina == null)
        {
            Debug.LogError($"{nameof(PlayerMovementSmooth)}: 같은 오브젝트에 필요한 컴포넌트가 없습니다. " +
                           "(PlayerInputHandler, PlayerStateController, PlayerStats, PlayerStamina)", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (playerState != null) playerState.StateChanged += OnStateChanged;
    }

    private void OnDisable()
    {
        if (playerState != null) playerState.StateChanged -= OnStateChanged;
        if (isDashing) EndDash();
    }

    private void Update()
    {
        inputDir = Vector2.ClampMagnitude(input.Move, 1f);
        if (inputDir.sqrMagnitude > 0.01f) lastDir = inputDir.normalized;

        PlayerState current = playerState.Current;
        bool canWalk = current != PlayerState.HitStun && current != PlayerState.Dead;
        moveDir = canWalk ? inputDir : Vector2.zero;

        if (!isDashing && input.DashPressed && Time.time >= nextDashTime
            && playerState.CanEnter(PlayerState.Dashing))
            StartDash();
    }

    private void FixedUpdate()
    {
        if (isDashing)
        {
            dashTimer += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(dashTimer / dashDuration);
            rb.linearVelocity = dashDir * (dashSpeed * dashSpeedCurve.Evaluate(t));

            if (t >= 1f) EndDash();
            return;
        }

        float maxSpeed = moveSpeed;
        if (playerState.Current == PlayerState.Attacking) maxSpeed *= moveWhileAttacking;

        Vector2 targetVelocity = moveDir * maxSpeed;
        bool hasInput = moveDir.sqrMagnitude > 0.01f;
        float rate = moveSpeed / (hasInput ? accelTime : decelTime);

        rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, targetVelocity, rate * Time.fixedDeltaTime);
    }

    private void StartDash()
    {
        if (!stamina.TryConsume(stats.DashCost)) return;   // [수정] 새 스태미나 스크립트, 소모량은 PlayerStats에서

        playerState.TryEnter(PlayerState.Dashing);
        // [삭제] stamina.BeginInvincible(); → PlayerInvincibility가 Dashing 상태를 보고 자동으로 켬

        isDashing = true;
        dashTimer = 0f;
        dashDir = inputDir.sqrMagnitude > 0.01f ? inputDir.normalized : lastDir;
    }

    private void EndDash()
    {
        if (!isDashing) return;

        isDashing = false;
        // [삭제] stamina.EndInvincible(); → 상태가 Dashing에서 바뀌면 PlayerInvincibility가 자동으로 끔
        nextDashTime = Time.time + dashCooldown;
        playerState.Exit(PlayerState.Dashing);
    }

    private void OnStateChanged(PlayerState prev, PlayerState next)
    {
        if (isDashing && next != PlayerState.Dashing) EndDash();
    }
}