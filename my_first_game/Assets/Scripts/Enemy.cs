using UnityEngine;

/// <summary>
/// 몬스터의 데이터와 체력을 관리한다. IDamageable이라 플레이어 공격에 맞을 수 있다.
/// </summary>
public class Enemy : MonoBehaviour, IDamageable
{
    [SerializeField] private EnemyData data;

    public EnemyData Data => data;
    public float CurrentHealth { get; private set; }

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
        if (data == null) return;

        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
        Debug.Log($"[{data.enemyName}] 피격! 데미지 {amount}, 남은 체력 {CurrentHealth}");

        if (CurrentHealth <= 0f)
        {
            Debug.Log($"[{data.enemyName}] 처치! (테스트용: 체력 회복)");
            CurrentHealth = data.maxHealth; // 테스트 중엔 죽지 않고 다시 채움
        }
    }
}