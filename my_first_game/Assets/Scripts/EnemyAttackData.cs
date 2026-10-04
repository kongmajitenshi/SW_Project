using UnityEngine;

/// <summary>
/// 적 공격 한 종류의 데이터. 공격마다 에셋을 하나씩 만들고, 여러 몬스터가 같은 공격을 공유할 수 있다.
/// </summary>
[CreateAssetMenu(fileName = "NewEnemyAttack", menuName = "Combat/EnemyAttackData")]
public class EnemyAttackData : ScriptableObject
{
    public string attackName = "기본 공격";
    public float damage = 10f;

    [Tooltip("체크: 패리 가능 / 해제: 패리 불가 (회피해야 함)")]
    public bool parryable = true;

    [Tooltip("공격 예고(선딜) 시간. 이 시간이 끝나는 순간 공격이 적중한다.")]
    [Min(0.05f)] public float windupTime = 0.6f;

    [Tooltip("적중 후 다음 행동까지 쉬는 시간(후딜)")]
    [Min(0f)] public float recoveryTime = 0.5f;
}