using System;
using UnityEngine;

/// <summary>플레이어가 지금 무엇을 하는 중인지</summary>
public enum PlayerState { Idle, Attacking, Dashing, Parrying, HitStun, Dead }

/// <summary>
/// 플레이어의 현재 상태와 "어떤 상태에서 어떤 행동을 시작할 수 있는지" 규칙을 한 곳에서 관리한다.
/// 각 행동 스크립트는 TryEnter()로 허락을 받고, 끝나면 Exit()로 알린다.
/// 상태가 바뀌면 StateChanged를 방송하고, 중간에 끊긴 행동은 그걸 듣고 스스로 정리한다.
/// </summary>
public class PlayerStateController : MonoBehaviour
{
    [Tooltip("상태가 바뀔 때마다 콘솔에 출력 (테스트용)")]
    [SerializeField] private bool logStateChanges = true;

    public PlayerState Current { get; private set; } = PlayerState.Idle;

    /// <summary>상태가 바뀔 때 방송 (이전 상태, 새 상태)</summary>
    public event Action<PlayerState, PlayerState> StateChanged;

    /// <summary>
    /// 행동 규칙표. 지금 상태(Current)에서 next 상태로 넘어갈 수 있는지 판단한다.
    /// 규칙을 바꾸고 싶으면 이 함수만 고치면 된다.
    /// </summary>
    public bool CanEnter(PlayerState next)
    {
        if (Current == PlayerState.Dead) return false;   // 사망 중: 전부 불가
        if (next == PlayerState.Dead) return true;       // 사망은 언제든 들어감
        if (next == PlayerState.HitStun) return true;    // 맞으면 언제든 경직

        switch (Current)
        {
            case PlayerState.Idle:
                return true;

            case PlayerState.Attacking:   // 공격 중: 공격(선입력)·대쉬·패리 가능
                return next == PlayerState.Attacking
                    || next == PlayerState.Dashing
                    || next == PlayerState.Parrying;

            case PlayerState.Dashing:     // 대쉬 중: 전부 불가
                return false;

            case PlayerState.Parrying:    // 패리 중: 대쉬만 가능 (패리 취소)
                return next == PlayerState.Dashing;

            case PlayerState.HitStun:     // 경직 중: 대쉬·패리 가능 (경직 탈출)
                return next == PlayerState.Dashing
                    || next == PlayerState.Parrying;

            default:
                return false;
        }
    }

    /// <summary>허락되면 상태를 바꾸고 true. 거절되면 아무것도 안 하고 false.</summary>
    public bool TryEnter(PlayerState next)
    {
        if (!CanEnter(next)) return false;
        if (next == Current) return true; // 이미 그 상태면 바꿀 것 없음 (경직 중 또 맞은 경우 등)

        ChangeTo(next);
        return true;
    }

    /// <summary>
    /// 행동이 끝났을 때 호출. 지금 상태가 그 행동일 때만 대기로 돌아간다.
    /// (이미 다른 행동으로 넘어갔다면 아무 일도 안 함)
    /// </summary>
    public void Exit(PlayerState finished)
    {
        if (Current == finished) ChangeTo(PlayerState.Idle);
    }

    private void ChangeTo(PlayerState next)
    {
        PlayerState prev = Current;
        Current = next;

        if (logStateChanges) Debug.Log($"[플레이어 상태] {prev} → {next}");
        StateChanged?.Invoke(prev, next);
    }
}