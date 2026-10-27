using System;
using UnityEngine;

/// <summary>
/// 몬스터의 데이터와 체력을 관리한다. 체력이 0이 되면 쓰러지고 잠시 뒤 사라진다.
/// </summary>
public class Enemy : MonoBehaviour, IDamageable
{
    [SerializeField] private EnemyData data;
    [Tooltip("쓰러진 뒤 사라지기까지의 시간(초)")]
    [SerializeField, Min(0f)] private float destroyDelay = 0.5f;

    public EnemyData Data => data;
    public float CurrentHealth { get; private set; }
    public bool IsDead { get; private set; }

    public event Action<float, Vector3> Damaged;
    public event Action Died;   // [추가] 쓰러졌을 때 방송

    private void Awake()
    {
        if (data == null)
        {
            Debug.LogError($"{nameof(Enemy)}: EnemyData가 연결되지 않았습니다.", this);
            enabled = false;
            return;
        }

        CurrentHealth = data.maxHealth;
    }

    public void TakeDamage(float amount, Vector3 hitPoint)
    {
        if (data == null || IsDead) return;   // [수정] 이미 쓰러졌으면 무시

        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
        Debug.Log($"[{data.enemyName}] 피격! 데미지 {amount}, 남은 체력 {CurrentHealth}");
        Damaged?.Invoke(amount, hitPoint);

        if (CurrentHealth <= 0f) Die();
    }

    private void Die()
    {
        IsDead = true;
        Debug.Log($"[{data.enemyName}] 처치!");

        // 쓰러진 뒤엔 공격에 맞지 않도록 충돌 끄기 (시체에 데미지 숫자가 뜨는 것 방지)
        foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
            col.enabled = false;

        Died?.Invoke();
        Destroy(gameObject, destroyDelay);
    }
}