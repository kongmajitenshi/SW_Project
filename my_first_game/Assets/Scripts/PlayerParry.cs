using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 스페이스바(Parry 액션)를 누르면 parryDuration(기본 0.3초) 동안 패링이 발동된다.
/// 패링 중에는 무기가 90도 돌아간 자세로 멈춰 있고(Parry 애니메이션),
/// 그 무기 콜라이더(hitbox)에 닿은 IParryable 대상에게 OnParried를 호출한다.
/// 입력은 PlayerInputHandler에서 받는다 (키를 직접 확인하지 않는다).
/// </summary>
public class PlayerParry : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInputHandler input;
    [Tooltip("패링 중 공격을 막고, 발동 시 진행 중인 공격을 취소하기 위해 사용한다.")]
    [SerializeField] private PlayerAttack attack;
    [Tooltip("패링 판정 범위로 쓸 BoxCollider2D. PlayerAttack에 연결한 무기 콜라이더를 그대로 연결하면, 90도 돌아간 무기 모양이 판정 범위가 된다.")]
    [SerializeField] private BoxCollider2D hitbox;
    [Tooltip("패링 애니메이션이 들어있는 Animator (Animator 창에 'Parry', 'Idle' 상태가 있어야 함)")]
    [SerializeField] private Animator animator;

    [Header("Parry")]
    [SerializeField, Min(0.05f)] private float parryDuration = 0.3f;
    [Tooltip("패링이 끝난 뒤 다시 발동할 수 있을 때까지의 시간 (스페이스바 연타 방지)")]
    [SerializeField, Min(0f)] private float parryCooldown = 0.2f;
    [SerializeField] private LayerMask targetLayers = ~0;

    /// <summary>패링이 발동 중인 동안(기본 0.3초)만 true.</summary>
    public bool IsParrying { get; private set; }

    /// <summary>패링 판정에 대상이 걸렸을 때 호출. 역경직/카메라 연출 등을 여기에 연결하면 된다.</summary>
    public event Action<IParryable, Vector3> ParrySucceeded;

    private static readonly int ParryStateHash = Animator.StringToHash("Parry"); // Animator 창의 상태 이름
    private static readonly int IdleStateHash = Animator.StringToHash("Idle");

    private readonly HashSet<IParryable> alreadyParried = new HashSet<IParryable>();
    private readonly Collider2D[] overlapResults = new Collider2D[16];
    private ContactFilter2D contactFilter;
    private float parryEndTime;
    private float nextParryTime;

    private void Awake()
    {
        if (input == null || hitbox == null) {
            Debug.LogError($"{nameof(PlayerParry)}: input 또는 hitbox가 연결되지 않았습니다.", this);
            enabled = false;
            return;
        }

        hitbox.isTrigger = true;

        contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(targetLayers);
        contactFilter.useTriggers = true;
    }

    private void OnDisable()
    {
        if (IsParrying) EndParry();
    }

    private void Update()
    {
        if (IsParrying) {
            if (Time.time >= parryEndTime) EndParry();
            return;
        }

        if (input.ParryPressed && Time.time >= nextParryTime && CanStartParry())
            StartParry();
    }

    // 애니메이터가 이번 프레임의 무기 자세를 정한 뒤에 판정해야 하므로 LateUpdate에서 검사한다.
    private void LateUpdate()
    {
        if (IsParrying) DetectParry();
    }

    // 대쉬 중처럼 공격 입력이 잠긴 상태에서는 패링을 시작할 수 없다.
    private bool CanStartParry() => attack == null || !attack.InputLocked;

    private void StartParry()
    {
        IsParrying = true;
        parryEndTime = Time.time + parryDuration;
        alreadyParried.Clear();

        if (attack != null) {
            attack.CancelAttack();     // 휘두르던 공격이 있으면 취소
            attack.InputLocked = true; // 패링 중엔 공격 불가
        }

        if (animator != null) animator.Play(ParryStateHash, 0, 0f);
    }

    private void EndParry()
    {
        IsParrying = false;
        nextParryTime = Time.time + parryCooldown;

        if (attack != null) attack.InputLocked = false;
        if (animator != null) animator.Play(IdleStateHash, 0, 0f); // 무기를 원래 자세로
    }

    private void GetHitboxWorldBox(out Vector2 center, out Vector2 size, out float angle)
    {
        Transform t = hitbox.transform;
        Vector2 s = t.lossyScale;

        center = t.TransformPoint(hitbox.offset);
        size = Vector2.Scale(hitbox.size, new Vector2(Mathf.Abs(s.x), Mathf.Abs(s.y)));
        angle = t.eulerAngles.z;
    }

    private void DetectParry()
    {
        GetHitboxWorldBox(out Vector2 center, out Vector2 size, out float angle);

        int count = Physics2D.OverlapBox(center, size, angle, contactFilter, overlapResults);

        for (int i = 0; i < count; i++) {
            Collider2D col = overlapResults[i];
            if (col.transform.IsChildOf(transform)) continue; // 자기 자신(플레이어) 제외

            IParryable target = col.GetComponentInParent<IParryable>();
            if (target == null) continue;
            if (!alreadyParried.Add(target)) continue; // 한 번 패링에 같은 대상은 1회만

            Vector2 hitPoint = col.ClosestPoint(center);
            target.OnParried(hitPoint);
            ParrySucceeded?.Invoke(target, hitPoint);
        }
    }

    // Scene 뷰에서 패링 판정 범위 확인용 (진한 초록: 패링 발동 중)
    private void OnDrawGizmos()
    {
        if (hitbox == null) return;

        GetHitboxWorldBox(out Vector2 center, out Vector2 size, out float angle);

        Gizmos.color = IsParrying ? Color.green : new Color(0f, 1f, 0f, 0.15f);
        Gizmos.matrix = Matrix4x4.TRS(center, Quaternion.Euler(0f, 0f, angle), Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, size);
    }
}