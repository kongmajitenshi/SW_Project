using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDashKeyboard : MonoBehaviour
{
    [Header("기본 이동")]
    [SerializeField] private float maxSpeed = 6f;
    [SerializeField] private float acceleration = 30f; // 가속도 (클수록 반응 빠름)
    [SerializeField] private float deceleration = 25f; // 감속도 (작을수록 더 많이 미끄러짐)

    [Header("대시/회피 설정")]
    [SerializeField] private float dashSpeed = 18f;     // 순간 돌진 속도
    [SerializeField] private float dashDuration = 0.15f; // 돌진 유지 시간
    [SerializeField] private float dashCooldown = 0.8f;  // 재사용 대기시간

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 lastMoveDirection = Vector2.right; // 멈춰있을 때 대시 방향 (기본 우측)

    private bool isDashing = false;
    private bool canDash = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (isDashing) return; // 대시 중에는 일반 입력 무시

        ReadKeyboardInput();
        CheckDashInput();
    }

    void FixedUpdate()
    {
        if (isDashing) return; // 대시 중에는 관성 로직 대신 대시 속도 유지

        ApplyInertiaMovement();
    }

    private void ReadKeyboardInput()
    {
        moveInput = Vector2.zero;

        if (Keyboard.current != null) {
            float x = (Keyboard.current.dKey.isPressed ? 1f : 0f) - (Keyboard.current.aKey.isPressed ? 1f : 0f);
            float y = (Keyboard.current.wKey.isPressed ? 1f : 0f) - (Keyboard.current.sKey.isPressed ? 1f : 0f);
            moveInput = new Vector2(x, y).normalized;

            // 이동 중일 때 마지막 입력 방향을 기록 (멈춘 상태에서 대시할 때 사용)
            if (moveInput.sqrMagnitude > 0.01f) {
                lastMoveDirection = moveInput;
            }
        }
    }

    private void CheckDashInput()
    {
        // 키보드 스페이스바 눌림 감지
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && canDash) {
            StartCoroutine(PerformDash());
        }
    }

    private void ApplyInertiaMovement()
    {
        // 1. 목표 속도 계산 (입력이 없으면 Vector2.zero)
        Vector2 targetVelocity = moveInput * maxSpeed;

        // 2. 가속/감속 수치 결정 (누르고 있으면 가속도, 뗐으면 감속도 적용)
        float currentRate = (moveInput.sqrMagnitude > 0.01f) ? acceleration : deceleration;

        // 3. 현재 속도에서 목표 속도로 일정 비율만큼 부드럽게 전진 (관성 연산)
        rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, targetVelocity, currentRate * Time.fixedDeltaTime);
    }

    private IEnumerator PerformDash()
    {
        canDash = false;
        isDashing = true;

        // 이동 중이면 이동 방향으로, 제자리에 서 있으면 마지막 진행 방향으로 돌진
        Vector2 dashDirection = (moveInput.sqrMagnitude > 0.01f) ? moveInput : lastMoveDirection;
        rb.linearVelocity = dashDirection * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        isDashing = false;

        // 쿨타임 대기
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
}