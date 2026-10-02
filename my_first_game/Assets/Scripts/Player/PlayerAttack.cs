using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attack 액션으로 공격 (2D 프로젝트용). 입력은 PlayerInputHandler에서 받는다.
/// 데미지 = 플레이어 기본 공격력 + 장착 무기 공격력.
/// 공격은 Windup(선딜) -> Active(판정) -> Recovery(후딜) 3단계 상태머신으로 진행된다.
/// </summary>
public class PlayerAttack : MonoBehaviour
{
    public enum AttackState { Idle, Windup, Active, Recovery }

    [Header("References")]
    [SerializeField] private PlayerInputHandler input;
    [Tooltip("플레이어 기본 공격력을 읽어오기 위해 사용")]
    [SerializeField] private PlayerHealthStamina status;          // [추가]
    [Tooltip("판정 범위로 쓸 BoxCollider2D")]
    [SerializeField] private BoxCollider2D hitbox;
    [Tooltip("휘두르기 애니메이션이 들어있는 Animator (없어도 판정은 동작)")]
    [SerializeField] private Animator animator;

    [Header("Weapon")]
    [Tooltip("현재 장착한 무기. 비워두면 맨손(기본 공격력만)")]
    [SerializeField] private WeaponData weapon;                   // [추가]

    [Header("Timing (초) - 애니메이션 길이와 맞춰주세요")]
    [SerializeField, Min(0f)] private float windupTime = 0.10f;
    [SerializeField, Min(0.01f)] private float activeTime = 0.15f;
    [SerializeField, Min(0f)] private float recoveryTime = 0.25f;
    [Tooltip("후딜 끝나기 직전에 눌러둔 입력을 이 시간 동안 기억한다.")]
    [SerializeField, Min(0f)] private float inputBufferTime = 0.15f;

    [Header("Combat")]
    // [삭제] private float damage = 10f;  → 이제 CurrentDamage로 계산
    [SerializeField] private LayerMask targetLayers = ~0;

    public AttackState State { get; private set; } = AttackState.Idle;
    public bool IsAttacking => State != AttackState.Idle;

    /// <summary>true 이면 새 공격을 시작할 수 없다 (대쉬 중 등에서 외부가 설정).</summary>
    public bool InputLocked { get; set; }

    /// <summary>[추가] 지금 공격하면 들어갈 데미지 (기본 공격력 + 무기 공격력)</summary>
    public float CurrentDamage => status.BaseAttack + (weapon != null ? weapon.attackPower : 0f);

    /// <summary>[수정] 타격 성공 시 호출 (맞은 대상, 맞은 위치, 들어간 데미지)</summary>
    public event Action<IDamageable, Vector3, float> HitLanded;

    private static readonly int AttackTrigger = Animator.StringToHash("Attack");
    private static readonly int IdleState = Animator.StringToHash("Idle");

    private readonly HashSet<IDamageable> alreadyHit = new HashSet<IDamageable>();
    private readonly Collider2D[] overlapResults = new Collider2D[16];
    private ContactFilter2D contactFilter;
    private float stateTimer;
    private float bufferTimer;

    private void Awake()
    {
        // [수정] status 연결 여부도 검사
        if (input == null || status == null || hitbox == null)
        {
            Debug.LogError($"{nameof(PlayerAttack)}: input, status, hitbox 중 연결되지 않은 것이 있습니다.", this);
            enabled = false;
            return;
        }

        hitbox.isTrigger = true;

        contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(targetLayers);
        contactFilter.useTriggers = true;
    }

    private void OnDisable() => CancelAttack();

    private void Update()
    {
        if (input.AttackPressed && !InputLocked)
        {
            if (!IsAttacking) StartAttack();
            else bufferTimer = inputBufferTime;
        }

        if (bufferTimer > 0f) bufferTimer -= Time.deltaTime;

        if (IsAttacking) TickState(Time.deltaTime);
    }

    private void LateUpdate()
    {
        if (State == AttackState.Active) DetectHits();
    }

    /// <summary>[추가] 무기 교체. 나중에 인벤토리/장비창에서 호출하면 된다.</summary>
    public void EquipWeapon(WeaponData newWeapon)
    {
        weapon = newWeapon;
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
            animator.Play(IdleState, 0, 0f);
        }
    }

    private void TickState(float dt)
    {
        stateTimer += dt;

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
                if (bufferTimer > 0f) StartAttack();
                break;
        }
    }

    private void GetHitboxWorldBox(out Vector2 center, out Vector2 size, out float angle)
    {
        Transform t = hitbox.transform;
        Vector2 s = t.lossyScale;

        center = t.TransformPoint(hitbox.offset);
        size = Vector2.Scale(hitbox.size, new Vector2(Mathf.Abs(s.x), Mathf.Abs(s.y)));
        angle = t.eulerAngles.z;
    }

    private void DetectHits()
    {
        GetHitboxWorldBox(out Vector2 center, out Vector2 size, out float angle);

        int count = Physics2D.OverlapBox(center, size, angle, contactFilter, overlapResults);

        for (int i = 0; i < count; i++)
        {
            Collider2D col = overlapResults[i];
            if (col.transform.IsChildOf(transform)) continue;

            IDamageable target = col.GetComponentInParent<IDamageable>();
            if (target == null) continue;
            if (!alreadyHit.Add(target)) continue;

            Vector2 hitPoint = col.ClosestPoint(center);
            float damage = CurrentDamage;                   // [수정] 맞히는 순간의 공격력으로 계산
            target.TakeDamage(damage, hitPoint);
            HitLanded?.Invoke(target, hitPoint, damage);    // [수정] 데미지 값도 함께 알림
        }
    }

    private void OnDrawGizmos()
    {
        if (hitbox == null) return;

        GetHitboxWorldBox(out Vector2 center, out Vector2 size, out float angle);

        Gizmos.color = State == AttackState.Active ? Color.red : new Color(1f, 1f, 0f, 0.4f);
        Gizmos.matrix = Matrix4x4.TRS(center, Quaternion.Euler(0f, 0f, angle), Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, size);
    }
}