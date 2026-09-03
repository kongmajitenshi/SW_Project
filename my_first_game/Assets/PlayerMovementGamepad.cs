using UnityEngine;
using UnityEngine.InputSystem; // Unity 6 최신 입력 시스템

public class PlayerMovement_Gamepad : MonoBehaviour
{
    [Header("이동 속도")]
    [SerializeField] private float moveSpeed = 6f;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    void Awake()
    {
        // 내 몸에 달린 물리 엔진(Rigidbody2D) 부품 가져오기
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 매 프레임마다 게임패드 왼쪽 스틱의 기울기 값을 읽어옴
        ReadGamepadInput();
    }

    void FixedUpdate()
    {
        // 물리 주기에 맞춰 속도 적용 (Unity 6 최신 문법)
        rb.linearVelocity = moveInput * moveSpeed;
    }

    private void ReadGamepadInput()
    {
        moveInput = Vector2.zero;

        // PC에 게임패드가 꽂혀(연결되어) 있는지 체크
        if (Gamepad.current != null) {
            // 왼쪽 아날로그 스틱의 2D 기울기 값 (-1.0 ~ 1.0)을 그대로 읽어옴
            Vector2 stickInput = Gamepad.current.leftStick.ReadValue();

            // 패드 스틱은 키보드와 달리 미세하게 살살 밀면 천천히 걷고, 끝까지 밀면 뛰어야 함
            // 따라서 스틱을 끝까지 밀었을 때(길이가 1을 넘길 때)만 정규화해서 최대 속도를 제한함
            if (stickInput.magnitude > 1f) {
                moveInput = stickInput.normalized;
            } else {
                moveInput = stickInput; // 살살 기울인 아날로그 감도 유지
            }
        }
    }
}