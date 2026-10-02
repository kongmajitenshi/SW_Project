using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovementSmooth : MonoBehaviour
{
    // 상단 변수 선언부에 추가
    [Header("Stamina Reference")]
    [SerializeField] private PlayerHealthStamina stamina;
    
    [Header("References")]
    [SerializeField] private PlayerInputHandler input;
    [SerializeField] private PlayerAttack attack;

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
    private Vector2 moveDir;
    private Vector2 lastDir = Vector2.right;
    private Vector2 dashDir;
    private bool isDashing;
    private float dashTimer;
    private float nextDashTime;

    public bool IsDashing => isDashing;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        if(stamina == null)
            Debug.LogError($"{nameof(PlayerMovementSmooth)}: stamina(PlayerHealthStamina)가 연결되지 않았습니다.", this);

    }

    private void OnDisable()
    {
        if (isDashing) EndDash();
    }

    private void Update()
    {
        moveDir = Vector2.ClampMagnitude(input.Move, 1f);
        if (moveDir.sqrMagnitude > 0.01f) lastDir = moveDir.normalized;

        if (!isDashing && input.DashPressed && Time.time >= nextDashTime)
            StartDash();
    }

    private void FixedUpdate()
    {
        // 대쉬: 시간이 지날수록 곡선을 따라 속도가 줄어든다
        if (isDashing) {
            dashTimer += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(dashTimer / dashDuration);
            rb.linearVelocity = dashDir * (dashSpeed * dashSpeedCurve.Evaluate(t));

            if (t >= 1f) EndDash();
            return;
        }

        // 일반 이동: 현재 속도를 목표 속도 쪽으로 조금씩 이동시킨다
        float maxSpeed = moveSpeed;
        if (attack != null && attack.IsAttacking) maxSpeed *= moveWhileAttacking;

        Vector2 targetVelocity = moveDir * maxSpeed;
        bool hasInput = moveDir.sqrMagnitude > 0.01f;

        // 입력이 있으면 가속, 없으면 감속(미끄러짐)
        float rate = moveSpeed / (hasInput ? accelTime : decelTime);

        rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, targetVelocity, rate * Time.fixedDeltaTime);
    }

    private void StartDash()
    {
        // 스태미나가 연결되어 있다면 먼저 소모 시도 (부족하면 대시 취소)
        // 스태미나가 부족하면 대쉬 취소 (부족 메시지는 PlayerHealthStamina가 출력)
        if (stamina == null || !stamina.TryConsumeStamina(stamina.DashCost)) return;

        isDashing = true;
        dashTimer = 0f;
        dashDir = moveDir.sqrMagnitude > 0.01f ? moveDir.normalized : lastDir;

        if (attack != null)
        {
            attack.CancelAttack();
            attack.InputLocked = true;
        }
    }
    private void EndDash()
    {
        isDashing = false;
        nextDashTime = Time.time + dashCooldown;
        if (attack != null) attack.InputLocked = false;
    }
}