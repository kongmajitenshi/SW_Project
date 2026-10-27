using UnityEngine;

/// <summary>
/// 플레이어가 맞았을 때의 반응을 담당한다.
/// 맞으면 잠깐 경직(대쉬·패리로 탈출 가능), 체력이 0이 되면 사망 상태로 전환.
/// </summary>
[RequireComponent(typeof(PlayerHealth), typeof(PlayerStateController))]   // [추가]
public class PlayerHitReaction : MonoBehaviour
{
    // [삭제] health, playerState 칸 → Awake에서 자동으로 찾음

    [Tooltip("맞았을 때 행동할 수 없는 시간(초). 0이면 경직 없음")]
    [SerializeField, Min(0f)] private float hitStunDuration = 0.3f;

    private PlayerHealth health;               // [수정] PlayerHealthStamina → PlayerHealth, 자동 연결
    private PlayerStateController playerState; // [수정] 자동 연결
    private float stunTimer;

    private void Awake()
    {
        // [수정] 같은 오브젝트의 컴포넌트를 스스로 찾는다
        health = GetComponent<PlayerHealth>();
        playerState = GetComponent<PlayerStateController>();

        if (health == null || playerState == null)
        {
            Debug.LogError($"{nameof(PlayerHitReaction)}: 같은 오브젝트에 PlayerHealth 또는 PlayerStateController가 없습니다.", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (health == null) return;
        health.OnDamaged += OnDamaged;
        health.OnDeath += OnDeath;
    }

    private void OnDisable()
    {
        if (health == null) return;
        health.OnDamaged -= OnDamaged;
        health.OnDeath -= OnDeath;
    }

    private void Update()
    {
        if (stunTimer <= 0f) return;

        stunTimer -= Time.deltaTime;
        if (stunTimer <= 0f) playerState.Exit(PlayerState.HitStun);
    }

    private void OnDamaged(float amount)
    {
        if (health.IsDead) return;
        if (hitStunDuration <= 0f) return;

        if (playerState.TryEnter(PlayerState.HitStun))
            stunTimer = hitStunDuration;
    }

    private void OnDeath()
    {
        playerState.TryEnter(PlayerState.Dead);
        stunTimer = 0f;
        Debug.Log("[플레이어] 사망 - 모든 행동 정지");
    }
}