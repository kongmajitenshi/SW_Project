using UnityEngine;

/// <summary>
/// [새 파일] "어떤 상태일 때 무적인가" 규칙을 한 곳에서 관리한다.
/// 상태가 바뀔 때마다 확인해서, 무적 상태에 들어가면 무적을 켜고 나가면 끈다.
/// 대쉬 스크립트는 무적이라는 개념을 몰라도 된다.
/// </summary>
[RequireComponent(typeof(PlayerStateController), typeof(PlayerHealth))]
public class PlayerInvincibility : MonoBehaviour
{
    [Tooltip("이 상태에 있는 동안 무적. 목록에 상태를 추가하면 그 상태도 무적이 된다.")]
    [SerializeField] private PlayerState[] invincibleStates = { PlayerState.Dashing };

    private PlayerStateController playerState;
    private PlayerHealth health;
    private bool isGranting;   // 내가 지금 무적을 켜둔 상태인지 (켠 만큼만 정확히 끄기 위해)

    private void Awake()
    {
        playerState = GetComponent<PlayerStateController>();
        health = GetComponent<PlayerHealth>();
    }

    private void OnEnable()
    {
        if (playerState != null) playerState.StateChanged += OnStateChanged;
    }

    private void OnDisable()
    {
        if (playerState != null) playerState.StateChanged -= OnStateChanged;
        if (isGranting) Release();   // 꺼질 때 무적이 켜진 채로 남지 않게
    }

    private void OnStateChanged(PlayerState prev, PlayerState next)
    {
        bool shouldBeInvincible = IsInvincibleState(next);

        if (shouldBeInvincible && !isGranting) Grant();
        else if (!shouldBeInvincible && isGranting) Release();
    }

    private bool IsInvincibleState(PlayerState state)
    {
        foreach (PlayerState s in invincibleStates)
            if (s == state) return true;
        return false;
    }

    private void Grant()
    {
        health.BeginInvincible();
        isGranting = true;
    }

    private void Release()
    {
        health.EndInvincible();
        isGranting = false;
    }
}