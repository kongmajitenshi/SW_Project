using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 마우스 좌클릭 / 게임패드 X(West) 버튼으로 공격 (2D 프로젝트용).
/// 공격은 Windup(선딜) -> Active(판정) -> Recovery(후딜) 3단계 상태머신으로 진행된다.
/// </summary>
public class PlayerAttack : MonoBehaviour
{
    public enum AttackState { Idle, Windup, Active, Recovery }

    [Header("References")]
    [Tooltip("무기(직사각형)에 붙은 BoxCollider2D. 판정 범위로만 사용된다.")]
    [SerializeField] private BoxCollider2D hitbox;
    [Tooltip("휘두르기 애니메이션이 들어있는 Animator (없어도 판정은 동작)")]
    [SerializeField] private Animator animator;

    [Header("Timing (초) - 애니메이션 길이와 맞춰주세요")]
    [SerializeField, Min(0f)] private float windupTime = 0.10f;
    [SerializeField, Min(0.01f)] private float activeTime = 0.15f;
    [SerializeField, Min(0f)] private float recoveryTime = 0.25f;
    [Tooltip("후딜 끝나기 직전에 눌러둔 입력을 이 시간 동안 기억한다.")]
    [SerializeField, Min(0f)] private float inputBufferTime = 0.15f;

    [Header("Combat")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private LayerMask targetLayers = ~0;

    public AttackState State { get; private set; } = AttackState.Idle;
    public bool IsAttacking => State != AttackState.Idle;

    /// <summary>true 이면 새 공격을 시작할 수 없다 (대쉬 중 등에서 외부가 설정).</summary>
    public bool InputLocked { get; set; }

    /// <summary>타격 성공 시 호출. 역경직/카메라 연출 등을 여기에 연결하면 된다.</summary>
    public event Action<IDamageable, Vector3> HitLanded;

    private static readonly int AttackTrigger = Animator.StringToHash("Attack");
    private static readonly int IdleState = Animator.StringToHash("Idle");

    private readonly HashSet<IDamageable> alreadyHit = new HashSet<IDamageable>();
    private readonly Collider2D[] overlapResults = new Collider2D[16]; // GC 없이 재사용할 결과 버퍼
    private ContactFilter2D contactFilter;
    private InputAction attackAction;
    private float stateTimer;
    private float bufferTimer;

    private void Awake()
    {
        if (hitbox == null)
        {
            Debug.LogError($"{nameof(PlayerAttack)}: hitbox(BoxCollider2D)가 연결되지 않았습니다.", this);
            enabled = false;
            return;
        }

        // 판정 전용 콜라이더: 물리 충돌로 캐릭터/적을 밀어내지 않도록 트리거로 고정
        hitbox.isTrigger = true;

        contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(targetLayers);
        contactFilter.useTriggers = true; // 적의 히트박스 등 트리거 콜라이더도 감지 대상에 포함

        attackAction = new InputAction("Attack", InputActionType.Button);
        attackAction.AddBinding("<Mouse>/leftButton");
        attackAction.AddBinding("<Gamepad>/buttonWest"); // Xbox 컨트롤러의 X 버튼
    }

    private void OnEnable() => attackAction?.Enable();

    private void OnDisable()
    {
        attackAction?.Disable();
        CancelAttack();
    }

    private void OnDestroy() => attackAction?.Dispose();

    private void Update()
    {
        if (attackAction.WasPressedThisFrame() && !InputLocked)
        {
            if (!IsAttacking) StartAttack();
            else bufferTimer = inputBufferTime; // 공격 중 입력은 잠깐 기억해뒀다가 후딜 뒤에 실행
        }

        if (bufferTimer > 0f) bufferTimer -= Time.deltaTime;

        if (IsAttacking) TickState(Time.deltaTime);
    }

    // 애니메이터는 Update 뒤 LateUpdate 전에 포즈를 갱신하므로,
    // 이번 프레임에 실제로 보이는 무기 위치로 판정하려면 LateUpdate에서 검사한다.
    private void LateUpdate()
    {
        if (State == AttackState.Active) DetectHits();
    }

    private void StartAttack()
    {
        State = AttackState.Windup;
        stateTimer = 0f;
        bufferTimer = 0f;
        alreadyHit.Clear();

        if (animator != null) animator.SetTrigger(AttackTrigger);
    }

    /// <summary>공격을 즉시 중단한다 (대쉬 캔슬, 피격 등에서 호출).</summary>
    public void CancelAttack()
    {
        State = AttackState.Idle;
        stateTimer = 0f;
        bufferTimer = 0f;

        if (animator != null)
        {
            animator.ResetTrigger(AttackTrigger);
            animator.Play(IdleState, 0, 0f); // 컨트롤러에 "Idle" 이름의 상태가 있어야 함
        }
    }

    private void TickState(float dt)
    {
        stateTimer += dt;

        // 프레임이 길어져도 단계를 건너뛰지 않도록 while 사용 (남은 시간은 다음 단계로 이월)
        while (IsAttacking && stateTimer >= CurrentStateDuration())
        {
            stateTimer -= CurrentStateDuration();
            AdvanceState();
        }
    }

    private float CurrentStateDuration()
    {
        switch (State)
        {
            case AttackState.Windup: return windupTime;
            case AttackState.Active: return activeTime;
            case AttackState.Recovery: return recoveryTime;
            default: return 0f;
        }
    }

    private void AdvanceState()
    {
        switch (State)
        {
            case AttackState.Windup:
                State = AttackState.Active;
                break;
            case AttackState.Active:
                State = AttackState.Recovery;
                break;
            case AttackState.Recovery:
                State = AttackState.Idle;
                if (bufferTimer > 0f) StartAttack(); // 선입력이 남아있으면 바로 다음 공격
                break;
        }
    }

    private void GetHitboxWorldBox(out Vector2 center, out Vector2 size, out float angle)
    {
        Transform t = hitbox.transform;
        Vector2 s = t.lossyScale;

        center = t.TransformPoint(hitbox.offset);
        size = Vector2.Scale(hitbox.size, new Vector2(Mathf.Abs(s.x), Mathf.Abs(s.y)));
        angle = t.eulerAngles.z; // 2D는 Z축 회전만 의미가 있음
    }

    private void DetectHits()
    {
        GetHitboxWorldBox(out Vector2 center, out Vector2 size, out float angle);

        int count = Physics2D.OverlapBox(center, size, angle, contactFilter, overlapResults);

        for (int i = 0; i < count; i++)
        {
            Collider2D col = overlapResults[i];
            if (col.transform.IsChildOf(transform)) continue; // 자기 자신(플레이어) 제외

            IDamageable target = col.GetComponentInParent<IDamageable>();
            if (target == null) continue;
            if (!alreadyHit.Add(target)) continue; // 한 번 휘두를 때 같은 대상은 1회만

            Vector2 hitPoint = col.ClosestPoint(center);
            target.TakeDamage(damage, hitPoint); // Vector2 -> Vector3 암시적 변환 (z=0)
            HitLanded?.Invoke(target, hitPoint);
        }
    }

    // Scene/Game 뷰에서 판정 범위 확인용 (노란색: 대기, 빨간색: 판정 활성)
    private void OnDrawGizmos()
    {
        if (hitbox == null) return;

        GetHitboxWorldBox(out Vector2 center, out Vector2 size, out float angle);

        Gizmos.color = State == AttackState.Active ? Color.red : new Color(1f, 1f, 0f, 0.4f);
        Gizmos.matrix = Matrix4x4.TRS(center, Quaternion.Euler(0f, 0f, angle), Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, size);
    }
}