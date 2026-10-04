using UnityEngine;

/// <summary>
/// 몬스터 한 종류의 데이터. 몬스터 종류마다 에셋을 하나씩 만든다.
/// </summary>
[CreateAssetMenu(fileName = "NewEnemy", menuName = "Combat/EnemyData")]
public class EnemyData : ScriptableObject
{
    public string enemyName = "새 몬스터";
    public float maxHealth = 100f;

    [Tooltip("이 몬스터가 사용하는 공격들. 위에서부터 순서대로 반복 사용한다.")]
    public EnemyAttackData[] attacks;

    [Tooltip("한 공격이 끝나고 다음 공격 예고가 시작되기까지의 대기 시간")]
    [Min(0f)] public float attackInterval = 1.5f;
}