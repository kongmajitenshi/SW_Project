/*제미나이 시켜서 만든 wasd 조작, 8방향 이동*/
using UnityEngine;
using UnityEngine.InputSystem; // Unity 6 최신 입력 시스템

public class PlayerMovementKeyboard : MonoBehaviour
{
    [Header("이동 속도")]
    [SerializeField] private float moveSpeed = 6f;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    void Awake()
    {
        // 1. 내 몸(Player_Test)에 달린 물리 엔진(Rigidbody2D) 부품을 가져와 변수에 보관함
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 2. 매 프레임마다 키보드 입력을 읽어서 방향(Vector2)을 만듦
        ReadKeyboardInput();
    }

    void FixedUpdate()
    {
        // 3. 물리 엔진이 돌아가는 고정 주기에 맞춰 속도(linearVelocity)를 꽂아줌
        // 방향(moveInput) * 속도 수치(moveSpeed)
        rb.linearVelocity = moveInput * moveSpeed;
    }

    private void ReadKeyboardInput()
    {
        // 초기화: 아무것도 안 누르면 (0, 0)
        moveInput = Vector2.zero;

        // 키보드가 연결되어 있는지 체크
        if (Keyboard.current != null) {
            // D키를 누르면 +1, A키를 누르면 -1, 안 누르면 0
            float x = (Keyboard.current.dKey.isPressed ? 1f : 0f) - (Keyboard.current.aKey.isPressed ? 1f : 0f);

            // W키를 누르면 +1, S키를 누르면 -1, 안 누르면 0
            float y = (Keyboard.current.wKey.isPressed ? 1f : 0f) - (Keyboard.current.sKey.isPressed ? 1f : 0f);

            // 대각선 이동 시 루트2(1.414...)배 빨라지는 걸 막기 위해 길이를 1로 정규화
            moveInput = new Vector2(x, y).normalized;
        }
    }
}