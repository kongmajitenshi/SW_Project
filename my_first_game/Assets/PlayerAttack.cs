using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("공격 설정")]
    public GameObject attackHitbox; 
    public float attackDuration = 0.2f; 
    public float attackCooldown = 0.4f; 
    
    [Header("애니메이션 설정")]
    [Tooltip("플레이어의 Animator 컴포넌트를 여기에 연결하세요.")]
    public Animator animator; 

    private bool isAttacking = false;
    private InputAction attackAction;

    private void Awake()
    {
        attackAction = new InputAction("Attack", InputActionType.Button);
        attackAction.AddBinding("<Mouse>/leftButton");
        attackAction.AddBinding("<Gamepad>/buttonWest");
    }

    private void OnEnable() => attackAction.Enable();
    private void OnDisable() => attackAction.Disable();

    void Start()
    {
        if (attackHitbox != null) attackHitbox.SetActive(false);
    }

    void Update()
    {
        if (attackAction.WasPressedThisFrame() && !isAttacking)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;

        // 1. 애니메이션 실행: Animator에 설정해둔 "Attack" 트리거를 작동시킵니다.
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // 2. 공격 시작: 무기(히트박스) 표시 및 판정 활성화
        if (attackHitbox != null) attackHitbox.SetActive(true);

        // 3. 무기가 휘둘러지는 시간(공격 유지 시간)만큼 대기
        yield return new WaitForSeconds(attackDuration);

        // 4. 공격 종료: 무기 숨김
        if (attackHitbox != null) attackHitbox.SetActive(false);

        // 5. 남은 쿨타임만큼 대기 후 다음 공격 허용
        float remainingCooldown = Mathf.Max(0, attackCooldown - attackDuration);
        yield return new WaitForSeconds(remainingCooldown);
        
        isAttacking = false;
    }
}