using UnityEngine;

/// <summary>
/// 플레이어가 맞았을 때의 반응을 담당한다.
/// 맞으면 잠깐 경직(대쉬·패리로 탈출 가능), 체력이 0이 되면 사망 상태로 전환.
/// </summary>
public class PlayerHitReaction : MonoBehaviour
{
    [SerializeField] private PlayerHealthStamina health;
    [SerializeField] private PlayerStateController playerState;

    [Tooltip("맞았을 때 행동할 수 없는 시간(초). 0이면 경직 없음")]
    [SerializeField, Min(0f)] private float hitStunDuration = 0.3f;

    private float stunTimer;

    private void Awake()
    {
        if (health == null || playerState == null)
        {
            Debug.LogError($"{nameof(PlayerHitReaction)}: health 또는 playerState가 연결되지 않았습니다.", this);
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
        if (health.IsDead) return;           // 막타였다면 경직 말고 사망으로 처리
        if (hitStunDuration <= 0f) return;

        if (playerState.TryEnter(PlayerState.HitStun))
            stunTimer = hitStunDuration;     // 경직 중 또 맞으면 시간이 다시 처음부터
    }

    private void OnDeath()
    {
        playerState.TryEnter(PlayerState.Dead);
        stunTimer = 0f;
        Debug.Log("[플레이어] 사망 - 모든 행동 정지");
    }
}