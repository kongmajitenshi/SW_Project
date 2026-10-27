using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attack 액션으로 공격 (2D 프로젝트용). 입력은 PlayerInputHandler에서 받는다.
/// 데미지 = 플레이어 기본 공격력(PlayerStats) + 장착 무기 공격력.
/// 공격 시작은 PlayerStateController의 허락을 받고, 상태가 바뀌어 끊기면 스스로 정리한다.
/// </summary>
[RequireComponent(typeof(PlayerInputHandler), typeof(PlayerStateController), typeof(PlayerStats))]   // [추가]
public class PlayerAttack : MonoBehaviour
{
    public enum AttackState { Idle, Windup, Active, Recovery }

    [Header("References")]
    // [삭제] input, playerState, status 칸 → Awake에서 자동으로 찾음
    [Tooltip("판정 범위로 쓸 BoxCollider2D (무기 자식 오브젝트)")]
    [SerializeField] private BoxCollider2D hitbox;
    [Tooltip("휘두르기 애니메이션이 들어있는 Animator (없어도 판정은 동작)")]
    [SerializeField] private Animator animator;

    [Header("Weapon")]
    [Tooltip("현재 장착한 무기. 비워두면 맨손(기본 공격력만)")]
    [SerializeField] private WeaponData weapon;

    [Header("Timing (초) - 애니메이션 길이와 맞춰주세요")]
    [SerializeField, Min(0f)] private float windupTime = 0.10f;
    [SerializeField, Min(0.01f)] private float activeTime = 0.15f;
    [SerializeField, Min(0f)] private float recoveryTime = 0.25f;
    [Tooltip("후딜 끝나기 직전에 눌러둔 입력을 이 시간 동안 기억한다.")]
    [SerializeField, Min(0f)] private float inputBufferTime = 0.15f;

    [Header("Combat")]
    [SerializeField] private LayerMask targetLayers = ~0;

    /// <summary>공격 내부 단계 (선딜/판정/후딜). 플레이어 전체 상태(PlayerState)와는 다르다.</summary>
    public AttackState State { get; private set; } = AttackState.Idle;
    public bool IsAttacking => State != AttackState.Idle;

    /// <summary>지금 공격하면 들어갈 데미지 (기본 공격력 + 무기 공격력)</summary>
    public float CurrentDamage => stats.BaseAttack + (weapon != null ? weapon.attackPower : 0f);   // [수정] status → stats

    public event Action<IDamageable, Vector3, float> HitLanded;

    private static readonly int AttackTrigger = Animator.StringToHash("Attack");
    private static readonly int IdleState = Animator.StringToHash("Idle");

    private PlayerInputHandler input;          // [수정] 자동 연결
    private PlayerStateController playerState; // [수정] 자동 연결
    private PlayerStats stats;                 // [수정] PlayerHealthStamina status → PlayerStats, 자동 연결

    private readonly HashSet<IDamageable> alreadyHit = new HashSet<IDamageable>();
    private readonly Collider2D[] overlapResults = new Collider2D[16];
    private ContactFilter2D contactFilter;
    private float stateTimer;
    private float bufferTimer;

    private void Awake()
    {
        // [수정] 같은 오브젝트의 컴포넌트를 스스로 찾는다
        input = GetComponent<PlayerInputHandler>();
        playerState = GetComponent<PlayerStateController>();
        stats = GetComponent<PlayerStats>();

        if (input == null || playerState == null || stats == null || hitbox == null)
        {
            Debug.LogError($"{nameof(PlayerAttack)}: 필요한 컴포넌트가 없거나 hitbox가 연결되지 않았습니다.", this);
            enabled = false;
            return;
        }

        hitbox.isTrigger = true;

        contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(targetLayers);
        contactFilter.useTriggers = true;
    }

    private void OnEnable()
    {
        if (playerState != null) playerState.StateChanged += OnStateChanged;
    }

    private void OnDisable()
    {
        if (playerState != null) playerState.StateChanged -= OnStateChanged;
        if (IsAttacking) StopAttack();
    }

    private void Update()
    {
        if (input.AttackPressed)
        {
            if (IsAttacking) bufferTimer = inputBufferTime;
            else if (playerState.TryEnter(PlayerState.Attacking)) StartAttack();
        }

        if (bufferTimer > 0f) bufferTimer -= Time.deltaTime;

        if (IsAttacking) TickState(Time.deltaTime);
    }

    private void LateUpdate()
    {
        if (State == AttackState.Active) DetectHits();
    }

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

    private void StopAttack()
    {
        State = AttackState.Idle;
        stateTimer = 0f;
        bufferTimer = 0f;

        if (animator != null)
        {
            animator.ResetTrigger(AttackTrigger);
            animator.Play(IdleState, 0, 0f);
        }

        if (playerState != null) playerState.Exit(PlayerState.Attacking);
    }

    private void OnStateChanged(PlayerState prev, PlayerState next)
    {
        if (IsAttacking && next != PlayerState.Attacking) StopAttack();
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
                playerState.Exit(PlayerState.Attacking);
                if (bufferTimer > 0f && playerState.TryEnter(PlayerState.Attacking))
                    StartAttack();
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
            float damage = CurrentDamage;
            target.TakeDamage(damage, hitPoint);
            HitLanded?.Invoke(target, hitPoint, damage);
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