using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInputHandler input;
    [SerializeField] private PlayerAttack attack;

    [Header("Move")]
    [SerializeField] private float moveSpeed = 6f;
    [Tooltip("공격 중 이동 속도 배율 (0 = 멈춤, 1 = 그대로)")]
    [SerializeField, Range(0f, 1f)] private float moveWhileAttacking = 0.3f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 16f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 0.4f;

    private Rigidbody2D rb;
    private Vector2 moveDir;
    private Vector2 lastDir = Vector2.right;
    private Vector2 dashDir;
    private bool isDashing;
    private float dashEndTime;
    private float nextDashTime;

    public bool IsDashing => isDashing;

    private void Awake() => rb = GetComponent<Rigidbody2D>();

    private void OnDisable()
    {
        if (isDashing) EndDash();
    }

    private void Update()
    {
        moveDir = input.Move;
        if (moveDir.sqrMagnitude > 0.01f) lastDir = moveDir.normalized;

        if (!isDashing && input.DashPressed && Time.time >= nextDashTime)
            StartDash();

        if (isDashing && Time.time >= dashEndTime)
            EndDash();
    }

    private void FixedUpdate()
    {
        if (isDashing) {
            rb.linearVelocity = dashDir * dashSpeed;
            return;
        }

        float speed = moveSpeed;
        if (attack != null && attack.IsAttacking) speed *= moveWhileAttacking;

        rb.linearVelocity = moveDir * speed;
    }

    private void StartDash()
    {
        isDashing = true;
        dashDir = moveDir.sqrMagnitude > 0.01f ? moveDir.normalized : lastDir; // 입력 없으면 마지막 방향
        dashEndTime = Time.time + dashDuration;

        if (attack != null) {
            attack.CancelAttack();      // 대쉬로 공격 캔슬
            attack.InputLocked = true;  // 대쉬 중엔 공격 불가
        }
    }

    private void EndDash()
    {
        isDashing = false;
        nextDashTime = Time.time + dashCooldown;
        if (attack != null) attack.InputLocked = false;
    }
}