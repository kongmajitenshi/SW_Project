using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovementSmooth : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInputHandler input;
    [SerializeField] private PlayerStateController playerState;   // [추가]
    [SerializeField] private PlayerHealthStamina stamina;
    // [삭제] PlayerAttack attack → 공격 잠금·취소는 상태 관리가 대신 함

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
    private Vector2 inputDir;   // [추가] 플레이어가 누른 방향 (경직 중에도 기억 → 대쉬 방향에 사용)
    private Vector2 moveDir;    // 실제로 걸어갈 방향 (경직·사망 중엔 0)
    private Vector2 lastDir = Vector2.right;
    private Vector2 dashDir;
    private bool isDashing;
    private float dashTimer;
    private float nextDashTime;

    public bool IsDashing => isDashing;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (input == null || playerState == null || stamina == null)
        {
            Debug.LogError($"{nameof(PlayerMovementSmooth)}: input, playerState, stamina 중 연결되지 않은 것이 있습니다.", this);
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

        // [추가] 경직·사망 중엔 걸어서 이동 불가 (대쉬로 탈출은 가능)
        PlayerState current = playerState.Current;
        bool canWalk = current != PlayerState.HitStun && current != PlayerState.Dead;
        moveDir = canWalk ? inputDir : Vector2.zero;

        // [수정] 대쉬 가능 여부는 상태 관리에게 물어본다
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
        if (playerState.Current == PlayerState.Attacking) maxSpeed *= moveWhileAttacking;   // [수정]

        Vector2 targetVelocity = moveDir * maxSpeed;
        bool hasInput = moveDir.sqrMagnitude > 0.01f;
        float rate = moveSpeed / (hasInput ? accelTime : decelTime);

        rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, targetVelocity, rate * Time.fixedDeltaTime);
    }

    private void StartDash()
    {
        // 순서 중요: 상태 허락(Update에서 확인) → 스태미나 소모 → 상태 전환 → 무적
        if (!stamina.TryConsumeStamina(stamina.DashCost)) return;

        playerState.TryEnter(PlayerState.Dashing);   // 공격·패리·경직 중이었다면 여기서 자동으로 취소됨
        stamina.BeginInvincible();

        isDashing = true;
        dashTimer = 0f;
        dashDir = inputDir.sqrMagnitude > 0.01f ? inputDir.normalized : lastDir;
    }

    private void EndDash()
    {
        if (!isDashing) return;   // 두 번 불려도 무적이 두 번 꺼지지 않게

        isDashing = false;
        stamina.EndInvincible();
        nextDashTime = Time.time + dashCooldown;
        playerState.Exit(PlayerState.Dashing);
    }

    // [추가] 대쉬 도중 다른 상태(사망 등)로 바뀌면 대쉬를 정리한다
    private void OnStateChanged(PlayerState prev, PlayerState next)
    {
        if (isDashing && next != PlayerState.Dashing) EndDash();
    }
}